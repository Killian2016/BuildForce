#pragma warning disable CA1416
using BuildForce.Services;
namespace BuildForce.Views;
public partial class LoginPage : ContentPage
{
    private readonly AuthService _auth;
    private bool _passwordVisible = false;
    public LoginPage(AuthService auth)
    {
        InitializeComponent();
        _auth = auth;
    }
    private void OnTogglePassword(object sender, TappedEventArgs e)
    {
        _passwordVisible = !_passwordVisible;
        PasswordEntry.IsPassword = !_passwordVisible;
        ToggleEye.Text = _passwordVisible ? "🙈" : "👁";
    }
    private async void OnForgotPassword(object sender, TappedEventArgs e)
    {
        await Browser.OpenAsync("https://mezanoconstructionmanagementplatform.com/Account/ForgotPassword");
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
    private async void OnRegisterTapped(object sender, TappedEventArgs e)
    {
        await Browser.OpenAsync("https://mezanoconstructionmanagementplatform.com/Account/Register");
    }
}