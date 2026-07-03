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
        return new Window(new LoginPage(_auth));
    }
}