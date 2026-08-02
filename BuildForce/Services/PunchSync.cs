#pragma warning disable CA1416
using System;
using System.Threading.Tasks;

namespace BuildForce.Services;

// [OFF3a] Drains the durable punch queue once signal comes back.
//
// PunchQueue writes punches to disk; nothing ever transmitted them, so an
// offline punch was simply lost. This is the missing half.
//
// Scope: CLOCK-OUT only. An offline clock-IN has no server timesheet id,
// so site watch, breaks and job switching would all have nothing to work
// with - that needs provisional ids and reconciliation, which is a much
// bigger change and is deliberately not attempted here.
//
// Safety: every punch carries a ClientPunchId the server treats as an
// idempotency key, so replaying one can never double-punch a worker.
public static class PunchSync
{
    private static bool _started;
    private static bool _draining;
    private static System.Timers.Timer? _timer;

    // Raised after the queue shrinks, so a page can refresh its indicator.
    public static event Action? QueueChanged;

    public static bool IsOnline
    {
        get
        {
            try { return Connectivity.NetworkAccess == NetworkAccess.Internet; }
            catch { return true; }
        }
    }

    // Idempotent - safe to call from every page constructor.
    public static void Start()
    {
        if (_started) return;
        _started = true;
        try
        {
            Connectivity.ConnectivityChanged += OnConnectivityChanged;

            // Belt and braces: ConnectivityChanged is unreliable on some OEM
            // builds, so also sweep on a timer.
            _timer = new System.Timers.Timer(60000);
            _timer.AutoReset = true;
            _timer.Elapsed += (s, e) => { _ = DrainAsync(); };
            _timer.Start();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("[OFF3a] Start error: " + ex.Message);
        }
        _ = DrainAsync();
    }

    private static void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        if (e.NetworkAccess == NetworkAccess.Internet) _ = DrainAsync();
    }

    // [OFF3c] Decide "offline" from what actually happened to the request,
    // not from what Android claims about connectivity.
    //
    // Airplane mode fails fast with "Connection failure". But WiFi that is
    // ASSOCIATED WITH NO ROUTE - a jobsite trailer, a truck yard, a dead
    // hotspot - leaves NetworkAccess reporting Internet while the call hangs
    // until the HttpClient timeout. Both are offline to the worker, and only
    // the second one was slipping through.
    //
    // Deliberately narrow: a real server rejection ("Server returned 400",
    // "not found") must NOT be swallowed into the queue.
    public static bool LooksOffline(string? err)
    {
        if (string.IsNullOrWhiteSpace(err)) return false;
        var e = err.ToLowerInvariant();
        if (e.Contains("connection failure")) return true;
        if (e.Contains("connection refused")) return true;
        if (e.Contains("timeout")) return true;
        if (e.Contains("timed out")) return true;
        if (e.Contains("was canceled")) return true;
        if (e.Contains("no such host")) return true;
        if (e.Contains("unreachable")) return true;
        if (e.Contains("no route")) return true;
        if (e.Contains("network is")) return true;
        if (e.Contains("ssl")) return true;
        return false;
    }

    public static async Task DrainAsync()
    {
        if (_draining) return;
        if (PunchQueue.Count == 0) return;
        if (!IsOnline) return;

        _draining = true;
        try
        {
            var api = new ApiService();

            while (true)
            {
                var punch = PunchQueue.First();
                if (punch == null) break;

                // Offline clock-ins are out of scope; leave them queued rather
                // than spinning on them forever.
                if (punch.Kind != PunchKind.ClockOut) break;

                System.Diagnostics.Debug.WriteLine("[OFF3a] sending " + punch.ClientPunchId +
                    " ts=" + punch.TimesheetId + " attempt=" + punch.AttemptCount);

                var photo = PunchQueue.LoadPhoto(punch.PhotoPath);
                var result = await api.ClockOutAsync(
                    punch.TimesheetId,
                    punch.Latitude,
                    punch.Longitude,
                    punch.InjuryReported,
                    punch.InjuryDetails,
                    photo,
                    punch.AutoClockOut,
                    null,
                    punch.ClientPunchId,
                    punch.ClockInClientPunchId,
                    punch.OccurredAtUtc);

                var settled = result != null;

                if (!settled)
                {
                    // NOT FOUND means the server already closed this segment -
                    // site watch beat us to it. Retrying forever would be wrong.
                    var err = (api.LastError ?? "").ToLowerInvariant();
                    if (err.Contains("not found")) settled = true;
                }

                if (!settled)
                {
                    PunchQueue.MarkAttempt(punch.ClientPunchId, api.LastError);
                    System.Diagnostics.Debug.WriteLine("[OFF3a] deferred: " + api.LastError);
                    break;   // still no good - stop and try again next sweep
                }

                PunchQueue.Remove(punch.ClientPunchId);
                System.Diagnostics.Debug.WriteLine("[OFF3a] synced, remaining " + PunchQueue.Count);
                try { QueueChanged?.Invoke(); } catch { }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("[OFF3a] drain error: " + ex.Message);
        }
        finally
        {
            _draining = false;
        }
    }
}
