$path = "C:\Users\mezan\source\repos\BuildForce\BuildForce\Views\Dashboardpage.xaml.cs"

@'
#pragma warning disable CA1416
using BuildForce.Services;

namespace BuildForce.Views;

public partial class DashboardPage : ContentPage
{
    private readonly ApiService _api;
    private System.Timers.Timer? _clockTimer;
    private DateTime _sessionStart = DateTime.Now;

    public DashboardPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        StartClock();
        LoadData();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadData();
    }

    private void StartClock()
    {
        DateLabel.Text = $"— {DateTime.Now:dddd, MMMM d, yyyy}";
        var name = Preferences.Get("full_name", "");
        var email = Preferences.Get("email", "");
        WelcomeLabel.Text = $"Welcome back, {(string.IsNullOrEmpty(name) ? email : name)}";

        _clockTimer?.Stop();
        _clockTimer = new System.Timers.Timer(1000);
        _clockTimer.Elapsed += (s, e) =>
        {
            var elapsed = DateTime.Now - _sessionStart;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                TimerLabel.Text = elapsed.ToString(@"hh\:mm\:ss");
            });
        };
        _clockTimer.Start();
    }

    private async void LoadData()
    {
        try
        {
            var data = await _api.GetDashboardAsync();
            if (data != null)
            {
                RevenueLabel.Text = data.TotalRevenue.ToString("C0");
                RevBadge.Text = $"✅ {data.PaidInvoices} paid invoices · Collected";
                PendingLabel.Text = data.OutstandingBalance.ToString("C0");
                PendBadge.Text = $"🕐 {data.PendingInvoices} invoices out · Outstanding";
                ExpensesLabel.Text = data.Expenses.ToString("C0");
                ProfitLabel.Text = data.NetProfit.ToString("C0");

                ProjectsList.Children.Clear();
                foreach (var p in data.RecentProjects.Take(3))
                {
                    var row = new Border
                    {
                        BackgroundColor = Color.FromArgb("#0d1f38"),
                        Stroke = Color.FromArgb("#1e3a5f"),
                        StrokeThickness = 1,
                        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
                        Padding = new Thickness(14, 10)
                    };
                    var grid = new Grid
                    {
                        ColumnDefinitions =
                        {
                            new ColumnDefinition { Width = GridLength.Star },
                            new ColumnDefinition { Width = GridLength.Auto }
                        }
                    };
                    var nameLabel = new Label
                    {
                        Text = p.Name,
                        FontSize = 13,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White
                    };
                    var statusLabel = new Label
                    {
                        Text = p.Status,
                        FontSize = 11,
                        TextColor = Color.FromArgb("#c8e63c"),
                        HorizontalOptions = LayoutOptions.End,
                        VerticalOptions = LayoutOptions.Center
                    };
                    Grid.SetColumn(nameLabel, 0);
                    Grid.SetColumn(statusLabel, 1);
                    grid.Children.Add(nameLabel);
                    grid.Children.Add(statusLabel);
                    row.Content = grid;
                    ProjectsList.Children.Add(row);
                }
            }
        }
        catch { }
        finally
        {
            Loading.IsRunning = false;
            Loading.IsVisible = false;
        }
    }

    // NATIVE: navigate to in-app project creation form
    private async void OnNewProject(object sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("ProjectCreatePage");

    // NATIVE: navigate to in-app expense logging form
    private async void OnLogExpense(object sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("ExpenseCreatePage");

    // NOT YET NATIVE: Invoices don't have a create API endpoint yet.
    private async void OnNewInvoice(object sender, TappedEventArgs e)
        => await Browser.OpenAsync("https://mezanocm.com/Invoices/Create", BrowserLaunchMode.SystemPreferred);

    // NOT YET NATIVE: Estimates don't have a create API endpoint yet.
    private async void OnNewEstimate(object sender, TappedEventArgs e)
        => await Browser.OpenAsync("https://mezanocm.com/Estimates/Create", BrowserLaunchMode.SystemPreferred);

    private async void OnViewAllProjects(object sender, TappedEventArgs e)
    {
        var projectsPage = Handler?.MauiContext?.Services.GetService<ProjectsPage>();
        if (projectsPage != null)
            await Navigation.PushAsync(projectsPage);
    }
}
'@ | Set-Content -Path $path -Encoding UTF8

Write-Host "DONE: Dashboardpage.xaml.cs overwritten with native navigation" -ForegroundColor Green