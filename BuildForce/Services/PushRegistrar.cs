using System.Net.Http.Json;
using Plugin.FirebasePushNotifications;

namespace BuildForce.Services
{
    // [PUSH1] Registers this device FCM token with the CMS.
    // Called after login and on remember-me auto-login; also re-sends
    // whenever Firebase rotates the token (TokenRefreshed).
    public static class PushRegistrar
    {
        private static bool _hooked;

        public static async Task RegisterAsync()
        {
            try
            {
                var fcm = IFirebasePushNotification.Current;
                if (!_hooked)
                {
                    _hooked = true;
                    fcm.TokenRefreshed += async (s, e) =>
                        await SendAsync(e.Token);
                }
                await fcm.RegisterForPushNotificationsAsync();
                var token = fcm.Token;
                if (!string.IsNullOrEmpty(token))
                    await SendAsync(token);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "[PUSH1] register failed: " + ex.Message);
            }
        }

        private static async Task SendAsync(string token)
        {
            try
            {
                var auth = Preferences.Get("auth_token", "");
                if (string.IsNullOrEmpty(auth)) return;
                if (string.IsNullOrEmpty(token)) return;
                using var client = new HttpClient
                { BaseAddress = new Uri("https://mezanocm.com") };
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Bearer", auth);
                var plat = DeviceInfo.Platform == DevicePlatform.iOS
                    ? "ios" : "android";
                await client.PostAsJsonAsync(
                    "/api/mobile/devices/register",
                    new { Token = token, Platform = plat });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "[PUSH1] send failed: " + ex.Message);
            }
        }
    }
}
