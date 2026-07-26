#pragma warning disable CA1416
using BuildForce.Services;

namespace BuildForce.Views;

public partial class DashboardPage : ContentPage
{
    private readonly ApiService _api;
    private readonly AuthService _auth;

    public DashboardPage(ApiService api, AuthService auth)
    {
        InitializeComponent();
        _api = api;
        _auth = auth;
        LoadHeader();
        LoadLive();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadHeader();
        LoadLive();
    }

    private void LoadHeader()
    {
        var name = Preferences.Get("full_name", "");
        var email = Preferences.Get("email", "");
        var display = string.IsNullOrEmpty(name) ? email : name;
        if (string.IsNullOrEmpty(display)) display = "BuildForce";

        UserNameLabel.Text = display;
        RoleLabel.Text = DateTime.Now.ToString("dddd, MMMM d");

        var parts = display.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string initials;
        if (parts.Length >= 2)
            initials = parts[0].Substring(0, 1) + parts[1].Substring(0, 1);
        else if (display.Length >= 2)
            initials = display.Substring(0, 2);
        else
            initials = "BF";
        AvatarLabel.Text = initials.ToUpper();
    }

    private async void LoadLive()
    {
        try
        {
            var dash = await _api.GetDashboardAsync();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (dash != null)
                {
                    RoleLabel.Text = DateTime.Now.ToString("dddd, MMMM d") + "  |  " + dash.ActiveProjects + " active projects";
                    SyncLabel.Text = "Online";
                    SyncLabel.TextColor = Color.FromArgb("#10b981");
                }
                else
                {
                    SyncLabel.Text = "Offline";
                    SyncLabel.TextColor = Color.FromArgb("#f0a500");
                }
            });
        }
        catch
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                SyncLabel.Text = "Offline";
                SyncLabel.TextColor = Color.FromArgb("#f0a500");
            });
        }

        try
        {
            var active = await _api.GetActiveTimesheetAsync();
            var summary = await _api.GetTimesheetSummaryAsync();
            string hours = summary != null ? summary.TotalHours.ToString("F1") + "h this week" : "";
            bool onClock = active != null && active.Status == "Active";
            string text = onClock ? "You are on the clock" : "You are not clocked in";
            if (hours.Length > 0) text = text + "  |  " + hours;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                CrewSub.Text = text;
                CrewSub.TextColor = onClock ? Color.FromArgb("#10b981") : Color.FromArgb("#7d8590");
            });
        }
        catch { }

        try
        {
            var expenses = await _api.GetExpensesAsync();
            var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var recent = expenses.Where(e => e.ExpenseDate >= monthStart).OrderByDescending(e => e.ExpenseDate).ToList();
            string text;
            if (recent.Count == 0)
            {
                text = "Nothing logged this month";
            }
            else
            {
                var last = recent[0];
                var who = string.IsNullOrEmpty(last.Vendor) ? last.Description : last.Vendor;
                text = recent.Count + " this month  |  last " + who;
            }
            MainThread.BeginInvokeOnMainThread(() => MaterialsSub.Text = text);
        }
        catch { }
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

    private async void OnTakePicture(object sender, EventArgs e)
    {
        try
        {
            await Application.Current!.MainPage!.Navigation.PushModalAsync(new ProjectPhotosPage(_api));
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert("Navigation error", ex.Message, "OK");
        }
    }

    private async void OnViewProjects(object sender, EventArgs e)
    {
        try
        {
            var host = HostPage;
            if (host != null)
                await host.Navigation.PushModalAsync(new ProjectsPage(_api));
        }
        catch (Exception ex)
        {
            await AlertAsync("Navigation error", ex.Message);
        }
    }

    private async void OnDailyLog(object sender, EventArgs e)
    {
        await ComingSoonAsync("Daily site logs");
    }

    private async void OnDailyLogTap(object sender, TappedEventArgs e)
    {
        await ComingSoonAsync("Daily site logs");
    }

    private async void OnCrew(object sender, TappedEventArgs e)
    {
        await ComingSoonAsync("Crew");
    }

    private async void OnBlueprints(object sender, TappedEventArgs e)
    {
        await ComingSoonAsync("Blueprints");
    }

    private async void OnSearch(object sender, EventArgs e)
    {
        await ComingSoonAsync("Search");
    }

    private async void OnMaterialsReceived(object sender, TappedEventArgs e)
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

    private async void OnSafetyForms(object sender, TappedEventArgs e)
    {
        await ComingSoonAsync("Safety inspection forms");
    }

    private async void OnSubmittals(object sender, TappedEventArgs e)
    {
        await ComingSoonAsync("Submittals");
    }

    private async void OnAccountSettings(object sender, TappedEventArgs e)
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