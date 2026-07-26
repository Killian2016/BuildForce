#pragma warning disable CA1416
using Android.App;
using Android.Content;
using Android.OS;

[assembly: UsesPermission(global::Android.Manifest.Permission.PostNotifications)]

namespace BuildForce;

public static class SiteWatchNotifier
{
    public static void Notify(string title, string message)
    {
        try
        {
            var ctx = global::Android.App.Application.Context;
            var mgr = (NotificationManager?)ctx.GetSystemService(Context.NotificationService);
            if (mgr == null) return;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel("sitewatch", "Site watch",
                    NotificationImportance.High);
                mgr.CreateNotificationChannel(channel);
            }

            int icon = ctx.ApplicationInfo != null ? ctx.ApplicationInfo.Icon : 0;
            if (icon == 0) icon = global::Android.Resource.Drawable.IcDialogInfo;

            var builder = new Notification.Builder(ctx, "sitewatch")
                .SetContentTitle(title)
                .SetContentText(message)
                .SetSmallIcon(icon)
                .SetAutoCancel(true);

            mgr.Notify(2001, builder.Build());
        }
        catch { }
    }
}
