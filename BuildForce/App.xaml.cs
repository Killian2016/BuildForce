#pragma warning disable CA1416
using BuildForce.Services;
using BuildForce.Views;

namespace BuildForce;

public partial class App : Application
{
    private readonly AuthService _auth;

    public App(AuthService auth)
    {
        InitializeComponent();
        _auth = auth;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // [DISC1] Prominent disclosure before first login - Play requirement.
        bool acked = Preferences.Get(DisclosurePage.AckKey, false);
        return new Window(acked ? new LoginPage(_auth) : (Page)new DisclosurePage(_auth));
    }
}