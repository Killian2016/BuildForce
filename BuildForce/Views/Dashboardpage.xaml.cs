#pragma warning disable CA1416
using BuildForce.Services;
using Microsoft.Maui.Controls.Shapes;

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
        DateLabel.Text = DateTime.Now.ToString("dddd, MMMM d, yyyy");
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
                RevBadge.Text = $"{data.PaidInvoices} paid invoices";
                PendingLabel.Text = data.OutstandingBalance.ToString("C0");
                PendBadge.Text = $"{data.PendingInvoices} invoices out";
                ExpensesLabel.Text = data.Expenses.ToString("C0");
                ProfitLabel.Text = data.NetProfit.ToString("C0");

                ProjectsList.Children.Clear();
                foreach (var p in data.RecentProjects.Take(3))
                {
                    string pillText, pillBg;
                    switch (p.Status)
                    {
                        case "Active":
                        case "In Progress": pillText = "#0d7a4f"; pillBg = "#d8f5e8"; break;
                        case "Planning":    pillText = "#8a6100"; pillBg = "#fdf0d2"; break;
                        case "On Hold":     pillText = "#9a3412"; pillBg = "#ffe8d9"; break;
                        case "Completed":   pillText = "#1e50a0"; pillBg = "#dde9fb"; break;
                        default:            pillText = "#5b6472"; pillBg = "#e8ecf3"; break;
                    }

                    var row = new Border
                    {
                        BackgroundColor = Color.FromArgb("#f7f9fd"),
                        Stroke = Color.FromArgb("#e2e7f0"),
                        StrokeThickness = 1,
                        StrokeShape = new RoundRectangle { CornerRadius = 12 },
                        Padding = new Thickness(12, 10)
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
                        TextColor = Color.FromArgb("#1a2340"),
                        LineBreakMode = LineBreakMode.TailTruncation,
                        VerticalOptions = LayoutOptions.Center
                    };
                    var pill = new Border
                    {
                        BackgroundColor = Color.FromArgb(pillBg),
                        Stroke = Colors.Transparent,
                        StrokeShape = new RoundRectangle { CornerRadius = 20 },
                        Padding = new Thickness(9, 3),
                        VerticalOptions = LayoutOptions.Center,
                        Content = new Label
                        {
                            Text = p.Status,
                            FontSize = 10,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Color.FromArgb(pillText)
                        }
                    };
                    Grid.SetColumn(nameLabel, 0);
                    Grid.SetColumn(pill, 1);
                    grid.Children.Add(nameLabel);
                    grid.Children.Add(pill);
                    row.Content = grid;
                    ProjectsList.Children.Add(row);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Dashboard LoadData error: {ex}");
        }
        finally
        {
            Loading.IsRunning = false;
            Loading.IsVisible = false;
        }
    }

    private static Page? HostPage => Application.Current?.MainPage;

    private static async Task ShowErrorAsync(string title, string message)
    {
        var host = HostPage;
        if (host != null)
            await host.DisplayAlert(title, message, "OK");
    }

    private async void OnNewProject(object sender, TappedEventArgs e)
    {
        try
        {
            var host = HostPage ?? throw new InvalidOperationException("No main page available.");
            await host.Navigation.PushModalAsync(new ProjectCreatePage(_api));
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Navigation Error", $"Could not open Project form: {ex.Message}");
        }
    }

    private async void OnLogExpense(object sender, TappedEventArgs e)
    {
        try
        {
            var host = HostPage ?? throw new InvalidOperationException("No main page available.");
            await host.Navigation.PushModalAsync(new ExpenseCreatePage(_api));
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Navigation Error", $"Could not open Expense form: {ex.Message}");
        }
    }

    private async void OnNewInvoice(object sender, TappedEventArgs e)
    {
        try
        {
            var host = HostPage ?? throw new InvalidOperationException("No main page available.");
            await host.Navigation.PushModalAsync(new InvoiceCreatePage(_api));
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Navigation Error", $"Could not open Invoice form: {ex.Message}");
        }
    }

    private async void OnNewEstimate(object sender, TappedEventArgs e)
    {
        try
        {
            var host = HostPage ?? throw new InvalidOperationException("No main page available.");
            await host.Navigation.PushModalAsync(new EstimateCreatePage(_api));
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Navigation Error", $"Could not open Estimate form: {ex.Message}");
        }
    }

    private async void OnViewAllProjects(object sender, TappedEventArgs e)
    {
        try
        {
            var host = HostPage ?? throw new InvalidOperationException("No main page available.");
            await host.Navigation.PushModalAsync(new ProjectsPage(_api));
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Navigation Error", $"Could not open Projects: {ex.Message}");
        }
    }
}