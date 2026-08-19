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
        // [BFRM1] Remember me: go straight in when a valid token was kept
        bool remember = Preferences.Get("remember_me", true);
        var savedToken = Preferences.Get("auth_token", "");
        if (!remember) Preferences.Remove("auth_token");
        if (acked && remember && !string.IsNullOrEmpty(savedToken) && !JwtExpired(savedToken))
            return new Window(new AppShell());
        return new Window(acked ? new LoginPage(_auth) : (Page)new DisclosurePage(_auth));
    }

    // [BFRM1] true when the JWT is expired or unreadable (60s slack)
    private static bool JwtExpired(string token)
    {
        try
        {
            var parts = token.Split(".");
            if (parts.Length < 2) return true;
            var p = parts[1].Replace("-", "+").Replace("_", "/");
            switch (p.Length % 4) { case 2: p += "=="; break; case 3: p += "="; break; }
            var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(p));
            var m = System.Text.RegularExpressions.Regex.Match(json, "\"exp\"\\s*:\\s*(\\d+)");
            if (!m.Success) return true;
            var exp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(m.Groups[1].Value));
            return exp <= DateTimeOffset.UtcNow.AddSeconds(60);
        }
        catch { return true; }
    }
}
