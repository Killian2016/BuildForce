#pragma warning disable CA1416
using BuildForce.Services;
using Microsoft.Maui.Controls.Shapes;

namespace BuildForce.Views;

public partial class CrewPage : ContentPage
{
    private readonly ApiService _api;

    public CrewPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        LoadCrew();
    }

    private async void LoadCrew()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Loading.IsRunning = true;
            Loading.IsVisible = true;
            EmptyLabel.IsVisible = false;
            RetryBtn.IsVisible = false;
            CrewList.Children.Clear();
        });

        var crew = await _api.GetActiveCrewAsync();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Loading.IsRunning = false;
            Loading.IsVisible = false;

            if (crew == null)
            {
                var err = _api.LastError ?? "";
                if (err.Contains("admins and managers"))
                {
                    HeaderLabel.Text = "Managers only";
                    SubLabel.Text = "Crew monitor is available to company admins and managers.";
                    EmptyLabel.Text = "Ask your administrator for access.";
                }
                else
                {
                    SubLabel.Text = "Could not reach the server";
                    EmptyLabel.Text = string.IsNullOrEmpty(err)
                        ? "Check your connection and try again."
                        : err;
                    RetryBtn.IsVisible = true;
                }
                EmptyLabel.IsVisible = true;
                return;
            }

            SubLabel.Text = DateTime.Now.ToString("dddd, MMMM d");

            if (crew.Count == 0)
            {
                EmptyLabel.Text = "No one is on the clock right now.";
                EmptyLabel.IsVisible = true;
                RetryBtn.IsVisible = true;
                return;
            }

            HeaderLabel.Text = crew.Count == 1
                ? "1 on the clock"
                : crew.Count + " on the clock";

            foreach (var m in crew)
            {
                var row = new Border
                {
                    BackgroundColor = Color.FromArgb("#0d1117"),
                    Stroke = Color.FromArgb(m.OnBreak ? "#f0a500" : "#10b981"),
                    StrokeThickness = 1,
                    StrokeShape = new RoundRectangle { CornerRadius = 12 },
                    Padding = new Thickness(14, 12)
                };

                var grid = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Auto }
                    },
                    RowDefinitions =
                    {
                        new RowDefinition { Height = GridLength.Auto },
                        new RowDefinition { Height = GridLength.Auto }
                    }
                };

                var nameLabel = new Label
                {
                    Text = m.EmployeeName ?? "Unknown",
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#e6edf3")
                };
                var projLabel = new Label
                {
                    Text = m.ProjectName ?? "No project",
                    FontSize = 11,
                    TextColor = Color.FromArgb("#0ea5e9")
                };
                Grid.SetRow(projLabel, 1);

                var inLocal = m.ClockInTime.HasValue
                    ? m.ClockInTime.Value.ToLocalTime().ToString("h:mm tt")
                    : "--";
                var status = m.OnBreak
                    ? "On break | in " + inLocal
                    : "In " + inLocal;
                if (m.BreakMinutes > 0 && !m.OnBreak)
                    status = status + " | break " + m.BreakMinutes + "m";

                var statusLabel = new Label
                {
                    Text = status,
                    FontSize = 11,
                    TextColor = Color.FromArgb(m.OnBreak ? "#f0a500" : "#10b981"),
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Center
                };
                Grid.SetColumn(statusLabel, 1);
                Grid.SetRowSpan(statusLabel, 2);

                grid.Children.Add(nameLabel);
                grid.Children.Add(projLabel);
                grid.Children.Add(statusLabel);
                row.Content = grid;
                CrewList.Children.Add(row);
            }

            RetryBtn.IsVisible = true;
            RetryBtn.Text = "Refresh";
        });
    }

    private void OnRefresh(object sender, EventArgs e)
    {
        LoadCrew();
    }

    private async void OnClose(object sender, EventArgs e)
    {
        try { await Navigation.PopModalAsync(); } catch { }
    }
}
