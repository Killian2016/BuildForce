#pragma warning disable CA1416
using Android.App;
using Android.Content;
using Android.OS;

namespace BuildForce;

// [SW2b] Foreground service so site watch keeps ticking when the app is
// backgrounded or the screen is off. A page timer cannot do this - Android
// suspends it, which is why the 15-minute auto clock-out was firing an hour
// or more late.
//
// Uses its OWN channel and notification id: SiteWatchNotifier owns "sitewatch"
// at High importance for alerts, so reusing it would overwrite those and make
// a permanent notification buzz. This one is Low importance and silent.
//
// type="location" keeps the app classed as in-use, so ACCESS_BACKGROUND_LOCATION
// is NOT required and Play's background-location review is avoided.
[Service(ForegroundServiceType = Android.Content.PM.ForegroundService.TypeLocation, Exported = false)]
public class SiteWatchService : Service
{
    public const int NotifId = 3001;
    public const string ChannelId = "sitewatch_status";

    public override IBinder? OnBind(Intent? intent) => null;

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        try
        {
            CreateChannel();
            var notification = BuildNotification("Clocked in", "Site watch on - you will be alerted if you leave the site");

            if (Build.VERSION.SdkInt >= BuildVersionCodes.UpsideDownCake)
            {
                StartForeground(NotifId, notification,
                    Android.Content.PM.ForegroundService.TypeLocation);
            }
            else
            {
                StartForeground(NotifId, notification);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("SiteWatchService start error: " + ex.Message);
        }

        // Sticky: if Android kills the process, restart the service so a worker
        // who is still on the clock stays watched.
        StartWatchLoop();
        return StartCommandResult.Sticky;
    }

    // ---- [SW2b-2b] the watch loop, now owned by the service ----
    private const int GraceMinutes = 15;
    private const double ExitMeters = 200;
    private System.Timers.Timer? _tick;

    private void StartWatchLoop()
    {
        try
        {
            _tick?.Stop();
            _tick = new System.Timers.Timer(60000);
            _tick.AutoReset = true;
            _tick.Elapsed += async (s, e) => { await TickAsync(); };
            _tick.Start();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("StartWatchLoop error: " + ex.Message);
        }
    }

    public override void OnDestroy()
    {
        try { _tick?.Stop(); _tick?.Dispose(); _tick = null; } catch { }
        base.OnDestroy();
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

    private async Task TickAsync()
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
            var exitDbg = Preferences.Get("sw_exit_utc", "");
            System.Diagnostics.Debug.WriteLine("[SW2b-5] tick tsId=" + tsId +
                " dist=" + dist.ToString("F0") + "m exit=" +
                (string.IsNullOrEmpty(exitDbg) ? "none" : exitDbg));

            if (dist <= ExitMeters)
            {
                Preferences.Remove("sw_exit_utc");   // back on site
                UpdateStatus("Clocked in", "Site watch on - you will be alerted if you leave the site");
                return;
            }

            if (!TryReadUtc("sw_exit_utc", out var exitUtc))
            {
                // First detection. Record when they left and warn them.
                exitUtc = DateTime.UtcNow;
                UpdateStatus("Left the job site", "Clocking out in " + GraceMinutes + " min unless you return");
                Preferences.Set("sw_exit_utc", exitUtc.ToString("o"));
                try
                {
                    SiteWatchNotifier.Notify("Leaving the job site",
                        "You are " + dist.ToString("F0") + "m from the site and still on the clock.");
                }
                catch { }
                return;
            }

            // [SW3a] still inside the grace window - show the countdown
            var elapsedMin = (DateTime.UtcNow - exitUtc).TotalMinutes;
            if (elapsedMin < GraceMinutes)
            {
                var remain = (int)Math.Ceiling(GraceMinutes - elapsedMin);
                if (remain < 1) remain = 1;
                UpdateStatus("Left the job site", "Clocking out in " + remain + " min unless you return");
                return;
            }

            // Grace is up and they are still outside - punch them out, backdated
            // to when they actually crossed the fence.
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
                // [SW2b-5] Tell "this timesheet is gone" apart from "no signal".
                // NOT FOUND means the server already closed this segment, so clear
                // state and stop. ANY other failure (network, auth, 500) must keep
                // retrying, or site watch dies the moment a worker loses signal.
                var err = (api.LastError ?? "").ToLowerInvariant();
                if (err.Contains("not found"))
                {
                    System.Diagnostics.Debug.WriteLine("[SW2b-5] timesheet gone - standing down");
                    Preferences.Remove("sw_tsid");
                    Preferences.Remove("sw_exit_utc");
                    Preferences.Remove("sw_mute_until");
                    Stop();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[SW2b-5] punch failed, will retry - " + err);
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

    private static void CreateChannel()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.O) return;
        var ctx = global::Android.App.Application.Context;
        var mgr = (NotificationManager?)ctx.GetSystemService(Context.NotificationService);
        if (mgr == null) return;

        var channel = new NotificationChannel(ChannelId, "Site watch status",
            NotificationImportance.Low);
        channel.SetShowBadge(false);
        mgr.CreateNotificationChannel(channel);
    }

    private static Notification BuildNotification(string title, string text)
    {
        var ctx = global::Android.App.Application.Context;

        int icon = ctx.ApplicationInfo != null ? ctx.ApplicationInfo.Icon : 0;
        if (icon == 0) icon = global::Android.Resource.Drawable.IcDialogInfo;

        var launch = ctx.PackageManager?.GetLaunchIntentForPackage(ctx.PackageName ?? "");
        PendingIntent? pi = null;
        if (launch != null)
        {
            pi = PendingIntent.GetActivity(ctx, 0, launch,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
        }

        var builder = new Notification.Builder(ctx, ChannelId)
            .SetContentTitle(title)
            .SetContentText(text)
            .SetSmallIcon(icon)
            .SetOngoing(true)
            .SetOnlyAlertOnce(true);

        if (pi != null) builder.SetContentIntent(pi);

        return builder.Build();
    }

    // [SW3a] Re-post the ongoing notification in place. Same id and
    // SetOnlyAlertOnce(true), so the text can change every minute without
    // the phone ever buzzing.
    private static void UpdateStatus(string title, string text)
    {
        try
        {
            var ctx = global::Android.App.Application.Context;
            var mgr = (NotificationManager?)ctx.GetSystemService(Context.NotificationService);
            if (mgr == null) return;
            mgr.Notify(NotifId, BuildNotification(title, text));
        }
        catch { }
    }

    public static void Start()
    {
        try
        {
            var ctx = global::Android.App.Application.Context;
            var intent = new Intent(ctx, typeof(SiteWatchService));
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                ctx.StartForegroundService(intent);
            else
                ctx.StartService(intent);
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
            var ctx = global::Android.App.Application.Context;
            ctx.StopService(new Intent(ctx, typeof(SiteWatchService)));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("SiteWatchService.Stop error: " + ex.Message);
        }
    }
}
