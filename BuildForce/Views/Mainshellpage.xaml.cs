#pragma warning disable CA1416
using BuildForce.Services;
using Microsoft.Maui.Devices.Sensors;

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
        _projectsPage = new ProjectsPage();
        _invoicesPage = new InvoicesPage();
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
        // Just navigate to the Time Clock page — let TimeClockPage handle all validation
        ShowPage(_timeClockPage);
        SetActive(IcoMore, LblMore);

        // Sync dock button state with actual clock state
        if (_timeClockPage.IsClockedIn)
        {
            _isClocked = true;
            ClockBtn.BackgroundColor = Color.FromArgb("#ef4444");
            ClockLabel.Text = "OUT";
            ClockLabel.TextColor = Color.FromArgb("#ffffff");
            LblClock.TextColor = Color.FromArgb("#ef4444");
        }
        else
        {
            _isClocked = false;
            ClockBtn.BackgroundColor = Color.FromArgb("#c8e63c");
            ClockLabel.Text = "GPS";
            ClockLabel.TextColor = Color.FromArgb("#000000");
            LblClock.TextColor = Color.FromArgb("#c8e63c");
        }
    }

    private void NavInvoices_Tapped(object sender, TappedEventArgs e)
    {
        ShowPage(_invoicesPage);
        SetActive(IcoInvoices, LblInvoices);
    }

    private async void NavMore_Tapped(object sender, TappedEventArgs e)
    {
        var action = await DisplayActionSheet("More", "Cancel", null,
            "Estimates", "Expenses", "Contracts",
            "Payroll", "Employees", "Customers",
            "Subcontractors", "Time Clock",
            "Company Settings", "Sign Out");

        switch (action)
        {
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
        IcoHome.TextColor = Color.FromArgb("#6b7280");
        LblHome.TextColor = Color.FromArgb("#6b7280");
        IcoProjects.TextColor = Color.FromArgb("#6b7280");
        LblProjects.TextColor = Color.FromArgb("#6b7280");
        IcoInvoices.TextColor = Color.FromArgb("#6b7280");
        LblInvoices.TextColor = Color.FromArgb("#6b7280");
        IcoMore.TextColor = Color.FromArgb("#6b7280");
        LblMore.TextColor = Color.FromArgb("#6b7280");

        icon.TextColor = Color.FromArgb("#c8e63c");
        label.TextColor = Color.FromArgb("#c8e63c");
    }
}