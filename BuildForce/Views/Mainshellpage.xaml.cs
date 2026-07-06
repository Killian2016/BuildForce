#pragma warning disable CA1416
using BuildForce.Services;

namespace BuildForce.Views;

public partial class MainShellPage : ContentPage
{
    private bool _isClocked = false;
    private readonly DashboardPage _dashboardPage;
    private readonly ProjectsPage _projectsPage;
    private readonly InvoicesPage _invoicesPage;
    private readonly TimeClockPage _timeClockPage;
    private readonly AuthService _auth;

    public MainShellPage(AuthService auth, ApiService api)
    {
        InitializeComponent();
        _auth = auth;
        _dashboardPage = new DashboardPage(api);
        _projectsPage = new ProjectsPage(api);
        _invoicesPage = new InvoicesPage(api);
        _timeClockPage = new TimeClockPage(api);
        ShowPage(_dashboardPage);
    }

    private void ShowPage(ContentPage page)
    {
        PageContent.Content = page.Content;
    }

    private void NavHome_Tapped(object sender, TappedEventArgs e)
    {
        ShowPage(_dashboardPage);
        SetActive(IcoHome, LblHome);
    }

    private void NavProjects_Tapped(object sender, TappedEventArgs e)
    {
        ShowPage(_projectsPage);
        SetActive(IcoProjects, LblProjects);
    }

    private void NavClock_Tapped(object sender, TappedEventArgs e)
    {
        ShowPage(_timeClockPage);
        SetActive(IcoMore, LblMore);

        if (_timeClockPage.IsClockedIn)
        {
            _isClocked = true;
            ClockBtn.BackgroundColor = Color.FromArgb("#b3261e");
            ClockLabel.Text = "OUT";
            ClockLabel.TextColor = Colors.White;
            LblClock.TextColor = Color.FromArgb("#b3261e");
        }
        else
        {
            _isClocked = false;
            ClockBtn.BackgroundColor = Color.FromArgb("#f0a500");
            ClockLabel.Text = "GPS";
            ClockLabel.TextColor = Color.FromArgb("#1a1a1a");
            LblClock.TextColor = Color.FromArgb("#b57c00");
        }
    }

    private void NavInvoices_Tapped(object sender, TappedEventArgs e)
    {
        ShowPage(_invoicesPage);
        SetActive(IcoInvoices, LblInvoices);
    }

    // Phone-essential menu only: everything here works natively today.
    private async void NavMore_Tapped(object sender, TappedEventArgs e)
    {
        var action = await DisplayActionSheet("More", "Cancel", null,
            "Estimates", "Expenses", "Time Clock", "Sign Out");

        switch (action)
        {
            case "Estimates":
                try { await Application.Current!.MainPage!.Navigation.PushModalAsync(new EstimatesPage()); }
                catch (Exception ex) { await DisplayAlert("Navigation Error", ex.Message, "OK"); }
                break;
            case "Expenses":
                try { await Application.Current!.MainPage!.Navigation.PushModalAsync(new ExpensesPage()); }
                catch (Exception ex) { await DisplayAlert("Navigation Error", ex.Message, "OK"); }
                break;
            case "Time Clock":
                ShowPage(_timeClockPage);
                SetActive(IcoMore, LblMore);
                break;
            case "Sign Out":
                Preferences.Clear();
                Application.Current!.MainPage = new LoginPage(_auth);
                break;
        }
    }

    private void SetActive(Label icon, Label label)
    {
        var muted = Color.FromArgb("#8a93a8");
        IcoHome.TextColor = muted;
        LblHome.TextColor = muted;
        IcoProjects.TextColor = muted;
        LblProjects.TextColor = muted;
        IcoInvoices.TextColor = muted;
        LblInvoices.TextColor = muted;
        IcoMore.TextColor = muted;
        LblMore.TextColor = muted;

        var active = Color.FromArgb("#f0a500");
        icon.TextColor = active;
        label.TextColor = active;
    }
}