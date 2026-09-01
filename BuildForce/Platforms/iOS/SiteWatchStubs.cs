using CoreLocation;
using UserNotifications;

namespace BuildForce;

// [SW-iOS1] iOS port of the Android foreground-service site watch
// (Platforms/Android/SiteWatchService.cs). A CLLocationManager with
// AllowsBackgroundLocationUpdates keeps the app running after backgrounding
// (blue indicator visible - deliberate, honest UX), and the SAME 60s tick
// logic as Android runs: 200m exit check, 15-min grace via sw_exit_utc,
// material-run mute via sw_mute_until, backdated auto punch.
// Works with While-Using permission; no "Always" prompt, no extra review risk.
// Countdown difference vs Android: no ongoing tray notification on iOS, so we
// notify at exit, at 5 minutes left, and at the punch instead of every minute.
public static class SiteWatchService
{
    private const int GraceMinutes = 15;
    private const double ExitMeters = 200;

    private static CLLocationManager? _mgr;
    private static System.Timers.Timer? _tick;
    private static bool _warned5;

    public static void Start()
    {
        try
        {
            _warned5 = false;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    SiteWatchNotifier.RequestPermission();
                    if (_mgr == null)
                    {
                        _mgr = new CLLocationManager();
                        _mgr.DesiredAccuracy = CLLocation.AccuracyHundredMeters;
                        _mgr.DistanceFilter = 50;
                        _mgr.PausesLocationUpdatesAutomatically = false;
                        // [254] background location removed for App Store
                        
                    }
                    _mgr.StartUpdatingLocation();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("SiteWatch iOS mgr start: " + ex.Message);
                }
            });

            _tick?.Stop();
            _tick = new System.Timers.Timer(60000);
            _tick.AutoReset = true;
            _tick.Elapsed += async (s, e) => { await TickAsync(); };
            _tick.Start();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("SiteWatchService.Start error: " + ex.Message);
        }
    }

    public static void Stop()
    {
        try
        {
            _tick?.Stop(); _tick?.Dispose(); _tick = null;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try { _mgr?.StopUpdatingLocation(); } catch { }
            });
        }
        catch { }
    }

    private static bool TryReadUtc(string key, out DateTime value)
    {
        value = DateTime.MinValue;
        var raw = Preferences.Get(key, "");
        if (string.IsNullOrEmpty(raw)) return false;
        if (!DateTime.TryParse(raw, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.RoundtripKind, out value)) return false;
        value = value.ToUniversalTime();
        return true;
    }

    private static async Task TickAsync()
    {
        try
        {
            var tsId = Preferences.Get("sw_tsid", 0);
            if (tsId <= 0) return;   // not on the clock

            // Material run or snooze: hold off, and stand the grace clock down.
            if (TryReadUtc("sw_mute_until", out var muteUntil) && DateTime.UtcNow < muteUntil)
            {
                Preferences.Remove("sw_exit_utc");
                return;
            }

            double projLat = Preferences.Get("sw_lat", 0.0);
            double projLng = Preferences.Get("sw_lng", 0.0);
            if (projLat == 0 && projLng == 0) return;

            Location? loc = null;
            try
            {
                loc = await Geolocation.GetLocationAsync(
                    new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(20)));
            }
            catch { }
            if (loc == null) return;

            double dist = HaversineMeters(loc.Latitude, loc.Longitude, projLat, projLng);
            System.Diagnostics.Debug.WriteLine("[SW-iOS1] tick tsId=" + tsId +
                " dist=" + dist.ToString("F0") + "m");

            if (dist <= ExitMeters)
            {
                Preferences.Remove("sw_exit_utc");   // back on site
                _warned5 = false;
                return;
            }

            if (!TryReadUtc("sw_exit_utc", out var exitUtc))
            {
                exitUtc = DateTime.UtcNow;
                Preferences.Set("sw_exit_utc", exitUtc.ToString("o"));
                try
                {
                    SiteWatchNotifier.Notify("Leaving the job site",
                        "You are " + dist.ToString("F0") + "m from the site and still on the clock. " +
                        "Clocking out in " + GraceMinutes + " min unless you return.");
                }
                catch { }
                return;
            }

            var elapsedMin = (DateTime.UtcNow - exitUtc).TotalMinutes;
            if (elapsedMin < GraceMinutes)
            {
                var remain = (int)Math.Ceiling(GraceMinutes - elapsedMin);
                if (remain <= 5 && !_warned5)
                {
                    _warned5 = true;
                    try
                    {
                        SiteWatchNotifier.Notify("Still off site",
                            "Clocking out in " + remain + " min unless you return to the site.");
                    }
                    catch { }
                }
                return;
            }

            // Grace is up and they are still outside - punch, backdated to the exit.
            var api = new BuildForce.Services.ApiService();
            var result = await api.ClockOutAsync(
                tsId, loc.Latitude, loc.Longitude,
                false, null, null,
                autoClockOut: true, exitedAt: exitUtc);

            if (result != null)
            {
                Preferences.Remove("sw_tsid");
                Preferences.Remove("sw_exit_utc");
                Preferences.Remove("sw_mute_until");
                try
                {
                    SiteWatchNotifier.Notify("Clocked out automatically",
                        "You left the job site over " + GraceMinutes +
                        " minutes ago. Your hours were recorded up to when you left.");
                }
                catch { }
                Stop();
            }
            else
            {
                var err = (api.LastError ?? "").ToLowerInvariant();
                if (err.Contains("not found"))
                {
                    System.Diagnostics.Debug.WriteLine("[SW-iOS1] timesheet gone - standing down");
                    Preferences.Remove("sw_tsid");
                    Preferences.Remove("sw_exit_utc");
                    Preferences.Remove("sw_mute_until");
                    Stop();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[SW-iOS1] punch failed, will retry - " + err);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("SiteWatch tick error: " + ex.Message);
        }
    }

    private static double HaversineMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}

public static class SiteWatchNotifier
{
    private static SwNotifDelegate? _delegate;

    public static void RequestPermission()
    {
        try
        {
            if (_delegate == null)
            {
                _delegate = new SwNotifDelegate();
                UNUserNotificationCenter.Current.Delegate = _delegate;
            }
            UNUserNotificationCenter.Current.RequestAuthorization(
                UNAuthorizationOptions.Alert | UNAuthorizationOptions.Sound | UNAuthorizationOptions.Badge,
                (granted, err) => {
                    System.Diagnostics.Debug.WriteLine("[NOTIF1] auth granted=" + granted + " err=" + err?.LocalizedDescription);
                });
        }
        catch { }
    }

    public static void Notify(string title, string message)
    {
        try
        {
            if (UNUserNotificationCenter.Current.Delegate == null)   // [NOTIF1]
            {
                _delegate ??= new SwNotifDelegate();
                UNUserNotificationCenter.Current.Delegate = _delegate;
            }
            var content = new UNMutableNotificationContent
            {
                Title = title,
                Body = message,
                Sound = UNNotificationSound.Default
            };
            content.InterruptionLevel = UNNotificationInterruptionLevel.Active;   // [NOTIF1] TimeSensitive needs an entitlement we do not have
            var req = UNNotificationRequest.FromIdentifier(
                Guid.NewGuid().ToString(), content, null);
            UNUserNotificationCenter.Current.AddNotificationRequest(req, err => {
                if (err != null) System.Diagnostics.Debug.WriteLine("[NOTIF1] post failed: " + err.LocalizedDescription);
            });
        }
        catch { }
    }

    // Show banners even when the app is foreground (iOS hides them by default).
    private sealed class SwNotifDelegate : UNUserNotificationCenterDelegate
    {
        public override void WillPresentNotification(UNUserNotificationCenter center,
            UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
            => completionHandler(UNNotificationPresentationOptions.Banner |
                                 UNNotificationPresentationOptions.List |
                                 UNNotificationPresentationOptions.Sound);
    }
}
