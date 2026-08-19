using Android.App;
using Android.Content;
using Android.Content.PM;

namespace BuildForce;

// [GSIM1] Receives the buildforce://auth deep link that ends the Google sign-in custom tab.
[Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
[IntentFilter(new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = "buildforce", DataHost = "auth")]
public class WebAuthCallbackActivity : Microsoft.Maui.Authentication.WebAuthenticatorCallbackActivity
{
}
