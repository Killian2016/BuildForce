#pragma warning disable CA1416
using Android.App;
using Android.Content;
using Android.OS;

namespace BuildForce;

// [VISPING1] Foreground service that keeps sending crew position while one or
// more visits are OnTheWay. The old SchedulePage dispatcher timer froze the
// moment "On the way" auto-opened Google Maps, so the server only ever got the
// first ping and MilesTraveled stayed 0. Cloned from SiteWatchService [SW2b]:
// own silent Low-importance channel + notification id, type="location" so
// ACCESS_BACKGROUND_LOCATION is not required.
//
// Tracked visit ids live in Preferences "vp_ids" (comma list) with a hard
// expiry "vp_until" so a crash can never leave a zombie notification pinging
// forever - the loop stands down on empty list or expiry.
[Service(ForegroundServiceType = Android.Content.PM.ForegroundService.TypeLocation, Exported = false)]
public class VisitPingService : Service
{
    public const int NotifId = 3002;                  // SiteWatchService owns 3001
    public const string ChannelId = "visitping_status"; // sitewatch_status is taken
    private const int MaxTripHours = 3;               // safety expiry per trip

    public override IBinder? OnBind(Intent? intent) => null;

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        try
        {
            CreateChannel();
            var n = BuildNotification("On the way", "Sharing your trip so the office and customer can see your ETA");
            if (Build.VERSION.SdkInt >= BuildVersionCodes.UpsideDownCake)
                StartForeground(NotifId, n, Android.Content.PM.ForegroundService.TypeLocation);
            else
                StartForeground(NotifId, n);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("VisitPingService start error: " + ex.Message);
        }
        StartPingLoop();
        return StartCommandResult.Sticky;
    }

    private System.Timers.Timer? _tick;

    private void StartPingLoop()
    {
        try
        {
            _tick?.Stop();
            _tick = new System.Timers.Timer(60000);
            _tick.AutoReset = true;
            _tick.Elapsed += async (s, e) => { await TickAsync(); };
            _tick.Start();
            _ = TickAsync();   // first ping immediately, not in 60s
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("VisitPing loop error: " + ex.Message);
        }
    }

    public override void OnDestroy()
    {
        try { _tick?.Stop(); _tick?.Dispose(); _tick = null; } catch { }
        base.OnDestroy();
    }

    private async Task TickAsync()
    {
        try
        {
            var ids = ReadIds();
            if (ids.Count == 0) { Stop(); return; }

            // Trip expiry: if the newest Track() is older than MaxTripHours the
            // phone almost certainly missed an Arrived tap - stand down.
            var raw = Preferences.Get("vp_until", "");
            if (DateTime.TryParse(raw, System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.RoundtripKind, out var until)
                && DateTime.UtcNow > until.ToUniversalTime())
            {
                System.Diagnostics.Debug.WriteLine("[VISPING1] trip expired - standing down");
                ClearAll();
                Stop();
                return;
            }

            Location? loc = null;
            try
            {
                loc = await Geolocation.GetLocationAsync(
                    new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(20)));
            }
            catch { }
            if (loc == null) return;   // no fix this minute - try again next tick

            var api = new BuildForce.Services.ApiService();
            foreach (var id in ids)
            {
                var ok = await api.SendVisitLocationAsync(id, loc.Latitude, loc.Longitude);
                System.Diagnostics.Debug.WriteLine("[VISPING1] ping visit " + id + " ok=" + ok);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("VisitPing tick error: " + ex.Message);
        }
    }

    // ---- id list helpers ----
    private static List<int> ReadIds()
    {
        var list = new List<int>();
        foreach (var part in Preferences.Get("vp_ids", "").Split(',', StringSplitOptions.RemoveEmptyEntries))
            if (int.TryParse(part, out var n) && !list.Contains(n)) list.Add(n);
        return list;
    }

    private static void WriteIds(List<int> ids)
    {
        Preferences.Set("vp_ids", string.Join(",", ids));
    }

    private static void ClearAll()
    {
        Preferences.Remove("vp_ids");
        Preferences.Remove("vp_until");
    }

    // ---- notification plumbing (SiteWatchService pattern) ----
    private static void CreateChannel()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.O) return;
        var ctx = global::Android.App.Application.Context;
        var mgr = (NotificationManager?)ctx.GetSystemService(Context.NotificationService);
        if (mgr == null) return;
        var channel = new NotificationChannel(ChannelId, "Trip sharing", NotificationImportance.Low);
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
            pi = PendingIntent.GetActivity(ctx, 0, launch,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
        var b = new Notification.Builder(ctx, ChannelId)
            .SetContentTitle(title)
            .SetContentText(text)
            .SetSmallIcon(icon)
            .SetOngoing(true)
            .SetOnlyAlertOnce(true);
        if (pi != null) b.SetContentIntent(pi);
        return b.Build();
    }

    // ---- public API used by SchedulePage [VISPING1] ----
    public static void Track(int visitId)
    {
        try
        {
            var ids = ReadIds();
            if (!ids.Contains(visitId)) ids.Add(visitId);
            WriteIds(ids);
            Preferences.Set("vp_until",
                DateTime.UtcNow.AddHours(MaxTripHours).ToString("o"));
            var ctx = global::Android.App.Application.Context;
            var intent = new Intent(ctx, typeof(VisitPingService));
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                ctx.StartForegroundService(intent);
            else
                ctx.StartService(intent);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("VisitPingService.Track error: " + ex.Message);
        }
    }

    public static void Untrack(int visitId)
    {
        try
        {
            var ids = ReadIds();
            ids.Remove(visitId);
            WriteIds(ids);
            if (ids.Count == 0) { ClearAll(); Stop(); }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("VisitPingService.Untrack error: " + ex.Message);
        }
    }

    public static void Stop()
    {
        try
        {
            var ctx = global::Android.App.Application.Context;
            ctx.StopService(new Intent(ctx, typeof(VisitPingService)));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("VisitPingService.Stop error: " + ex.Message);
        }
    }
}
