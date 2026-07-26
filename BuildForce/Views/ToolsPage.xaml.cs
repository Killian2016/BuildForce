#pragma warning disable CA1416
using BuildForce.Services;

namespace BuildForce.Views;

public partial class ToolsPage : ContentPage
{
    private readonly ApiService _api;
    private readonly AuthService _auth;

    public ToolsPage(ApiService api, AuthService auth)
    {
        InitializeComponent();
        _api = api;
        _auth = auth;
    }

    private static Page? HostPage => Application.Current?.MainPage;

    private static async Task AlertAsync(string title, string message)
    {
        var host = HostPage;
        if (host != null)
            await host.DisplayAlert(title, message, "OK");
    }

    private static async Task ComingSoonAsync(string feature)
    {
        await AlertAsync(feature, feature + " needs a Mezano CM endpoint before it can go live.");
    }

    private async void OnCrew(object sender, TappedEventArgs e)
    {
        try
        {
            var host = HostPage;
            if (host != null)
                await host.Navigation.PushModalAsync(new CrewPage(_api));
        }
        catch (Exception ex)
        {
            await AlertAsync("Navigation error", ex.Message);
        }
    }

    private async void OnPhotos(object sender, TappedEventArgs e)
    {
        try
        {
            var host = HostPage;
            if (host != null)
                await host.Navigation.PushModalAsync(new ProjectPhotosPage(_api));
        }
        catch (Exception ex)
        {
            await AlertAsync("Navigation error", ex.Message);
        }
    }

    private async void OnDailyLogs(object sender, TappedEventArgs e)
    {
        await ComingSoonAsync("Daily site logs");
    }

    private async void OnBlueprints(object sender, TappedEventArgs e)
    {
        await ComingSoonAsync("Blueprints");
    }

    private async void OnMaterials(object sender, TappedEventArgs e)
    {
        try
        {
            var host = HostPage;
            if (host != null)
                await host.Navigation.PushModalAsync(new ExpenseCreatePage(_api));
        }
        catch (Exception ex)
        {
            await AlertAsync("Navigation error", ex.Message);
        }
    }

    private async void OnSafety(object sender, TappedEventArgs e)
    {
        await ComingSoonAsync("Safety inspection forms");
    }

    private async void OnSubmittals(object sender, TappedEventArgs e)
    {
        await ComingSoonAsync("Submittals");
    }

    private async void OnSettings(object sender, TappedEventArgs e)
    {
        await ComingSoonAsync("Account settings");
    }

    private async void OnSignOut(object sender, TappedEventArgs e)
    {
        var host = HostPage;
        if (host == null) return;

        bool confirm = await host.DisplayAlert("Sign out", "Sign out of BuildForce?", "Sign out", "Cancel");
        if (!confirm) return;

        Preferences.Clear();
        Application.Current!.MainPage = new LoginPage(_auth);
    }
}
