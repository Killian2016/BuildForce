using BuildForce.Services;

namespace BuildForce.Views;

// [DISC1] Prominent disclosure shown once before first login. Google Play
// requires the app to explain location use in its OWN UI before the system
// permission dialog appears. Also the workers here did not sign themselves
// up - their employer created the account - so showing them plainly what is
// collected matters beyond compliance.
public partial class DisclosurePage : ContentPage
{
    public const string AckKey = "disclosure_ack";
    private readonly AuthService _auth;

    public DisclosurePage(AuthService auth)
    {
        InitializeComponent();
        _auth = auth;
    }

    private async void OnPrivacy(object sender, EventArgs e)
    {
        try { await Launcher.OpenAsync("https://mezanocm.com/privacy"); } catch { }
    }

    private async void OnDeletion(object sender, EventArgs e)
    {
        try { await Launcher.OpenAsync("https://mezanocm.com/account-deletion"); } catch { }
    }

    private void OnContinue(object sender, EventArgs e)
    {
        Preferences.Set(AckKey, true);
        if (Application.Current?.Windows.Count > 0)
            Application.Current.Windows[0].Page = new LoginPage(_auth);
    }
}
