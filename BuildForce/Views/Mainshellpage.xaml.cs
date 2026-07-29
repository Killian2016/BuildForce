#pragma warning disable CA1416
using BuildForce.Services;

namespace BuildForce.Views;

public partial class MainShellPage : ContentPage
{
    private bool _isClocked = false;
    private readonly DashboardPage _dashboardPage;
    private readonly ProjectsPage _projectsPage;
    private readonly TimeClockPage _timeClockPage;
    private readonly ToolsPage _toolsPage;
    private readonly SchedulePage _schedulePage;
    private readonly AuthService _auth;
    private readonly ApiService _api;

    public MainShellPage(AuthService auth, ApiService api)
    {
        InitializeComponent();
        _auth = auth;
        _api = api;
        _dashboardPage = new DashboardPage(api, auth);
        _projectsPage = new ProjectsPage(api);
        _timeClockPage = new TimeClockPage(api);
        _toolsPage = new ToolsPage(api, auth);
        _schedulePage = new SchedulePage(api);
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
        SetActive(IcoTools, LblTools);

        if (_timeClockPage.IsClockedIn)
        {
            _isClocked = true;
            ClockBtn.BackgroundColor = Color.FromArgb("#ef4444");
            ClockLabel.Text = "OUT";
            ClockLabel.TextColor = Colors.White;
            LblClock.TextColor = Color.FromArgb("#ef4444");
        }
        else
        {
            _isClocked = false;
            ClockBtn.BackgroundColor = Color.FromArgb("#f0a500");
            ClockLabel.Text = "GPS";
            ClockLabel.TextColor = Color.FromArgb("#1a1a1a");
            LblClock.TextColor = Color.FromArgb("#f0a500");
        }
    }

    private void NavSchedule_Tapped(object sender, TappedEventArgs e)
    {
        SetActive(IcoSchedule, LblSchedule);
        var host = Application.Current?.MainPage;
        if (host != null)
            ShowPage(_schedulePage); _schedulePage.LoadSchedule();
    }

    private void NavTools_Tapped(object sender, TappedEventArgs e)
    {
        ShowPage(_toolsPage);
        SetActive(IcoTools, LblTools);
    }

    private void SetActive(Label icon, Label label)
    {
        var muted = Color.FromArgb("#7d8590");
        IcoHome.TextColor = muted;
        LblHome.TextColor = muted;
        IcoProjects.TextColor = muted;
        LblProjects.TextColor = muted;
        IcoSchedule.TextColor = muted;
        LblSchedule.TextColor = muted;
        IcoTools.TextColor = muted;
        LblTools.TextColor = muted;

        var active = Color.FromArgb("#f0a500");
        icon.TextColor = active;
        label.TextColor = active;
    }
}

