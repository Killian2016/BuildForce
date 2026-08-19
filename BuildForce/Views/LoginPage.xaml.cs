#pragma warning disable CA1416
using BuildForce.Services;
namespace BuildForce.Views;
public partial class LoginPage : ContentPage
{
    private readonly AuthService _auth;
    private bool _passwordVisible = false;
    private bool _remember = true; // [BFRM1]
    public LoginPage(AuthService auth)
    {
        InitializeComponent();
        _auth = auth;
        _remember = Preferences.Get("remember_me", true); // [BFRM1]
        ApplyRemember();
        if (_remember) EmailEntry.Text = Preferences.Get("email", "");
        VersionLabel.Text = "v" + AppInfo.VersionString; // [BFLGN1]
    }
    private void OnTogglePassword(object sender, TappedEventArgs e)
    {
        _passwordVisible = !_passwordVisible;
        PasswordEntry.IsPassword = !_passwordVisible;
        ToggleEye.Text = _passwordVisible ? "HIDE" : "SHOW";
            ToggleEye.TextColor = _passwordVisible ? Color.FromArgb("#c8e63c") : Color.FromArgb("#9fb0c4");
    }
    private async void OnForgotPassword(object sender, TappedEventArgs e)
    {
        await Browser.OpenAsync("https://mezanocm.com/Account/ForgotPassword");
    }
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text;
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ErrorLabel.Text = "Please enter your email and password.";
            ErrorLabel.IsVisible = true;
            return;
        }
        ErrorLabel.IsVisible = false;
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        try
        {
            var result = await _auth.LoginAsync(email, password);
            if (result?.Success == true)
            {
                Preferences.Set("remember_me", _remember); // [BFRM1]
                Application.Current!.MainPage = new AppShell();
            }
            else
            {
                ErrorLabel.Text = result?.Message ?? "Invalid credentials. Please try again.";
                ErrorLabel.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
            ErrorLabel.Text = "Connection error. Please check your internet.";
            ErrorLabel.IsVisible = true;
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
    // [GSIM1] Continue with Google (server-side OAuth via WebAuthenticator)
    private async void OnGoogleTapped(object sender, TappedEventArgs e)
    {
        ErrorLabel.IsVisible = false;
        LoadingIndicator.IsVisible = true; LoadingIndicator.IsRunning = true;
        try
        {
            var result = await _auth.LoginWithGoogleAsync();
            if (result?.Success == true)
            {
                Preferences.Set("remember_me", _remember);
                Application.Current!.MainPage = new AppShell();
            }
            else if (!string.IsNullOrEmpty(result?.Message) && !result.Message.Contains("cancelled"))
            {
                ErrorLabel.Text = result!.Message; ErrorLabel.IsVisible = true;
            }
        }
        catch (Exception ex) { ErrorLabel.Text = "Google sign-in failed: " + ex.Message; ErrorLabel.IsVisible = true; }
        finally { LoadingIndicator.IsRunning = false; LoadingIndicator.IsVisible = false; }
    }
    private async void OnRegisterTapped(object sender, TappedEventArgs e)
    {
        await Browser.OpenAsync("https://mezanocm.com/Account/Register");
    }

    // [BFLGN1] legal footer links
    private async void OnPrivacyTapped(object sender, TappedEventArgs e)
    {
        await Browser.OpenAsync("https://mezanocm.com/privacy");
    }
    private async void OnTermsTapped(object sender, TappedEventArgs e)
    {
        await Browser.OpenAsync("https://mezanocm.com/terms");
    }
    private async void OnDeleteAccountTapped(object sender, TappedEventArgs e)
    {
        await Browser.OpenAsync("https://mezanocm.com/account-deletion");
    }

    // [BFRM1] Remember me tick-box
    private void OnRememberTapped(object sender, TappedEventArgs e) { _remember = !_remember; ApplyRemember(); }
    private void ApplyRemember()
    {
        RememberBox.BackgroundColor = _remember ? Color.FromArgb("#c8e63c") : Color.FromArgb("#111f38");
        RememberBox.Stroke = new SolidColorBrush(_remember ? Color.FromArgb("#c8e63c") : Color.FromArgb("#1e3a5f"));
        RememberTick.IsVisible = _remember;
    }
}
