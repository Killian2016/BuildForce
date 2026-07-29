#pragma warning disable CA1416
using BuildForce.Services;
using Microsoft.Maui.Controls.Shapes;

namespace BuildForce.Views;

public partial class SchedulePage : ContentPage
{
    private readonly ApiService _api;

    public SchedulePage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        LoadSchedule();
    }

    public void LoadSchedule()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Loading.IsRunning = true;
            Loading.IsVisible = true;
            EmptyLabel.IsVisible = false;
            RetryBtn.IsVisible = false;
            SchedList.Children.Clear();
        });
        _ = LoadScheduleAsync();
    }

    private async Task LoadScheduleAsync()
    {
        var items = await _api.GetMyScheduleAsync(DateTime.Today);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Loading.IsRunning = false;
            Loading.IsVisible = false;
            SubLabel.Text = DateTime.Now.ToString("dddd, MMMM d");

            if (items == null)
            {
                HeaderLabel.Text = "Today";
                SubLabel.Text = "Could not reach the server";
                var err = _api.LastError ?? "";
                EmptyLabel.Text = string.IsNullOrEmpty(err)
                    ? "Check your connection and try again."
                    : err;
                EmptyLabel.IsVisible = true;
                RetryBtn.IsVisible = true;
                return;
            }

            if (items.Count == 0)
            {
                HeaderLabel.Text = "Nothing scheduled";
                EmptyLabel.Text = "You have no assignments today.";
                EmptyLabel.IsVisible = true;
                RetryBtn.IsVisible = true;
                return;
            }

            HeaderLabel.Text = items.Count == 1 ? "1 assignment" : items.Count + " assignments";

            foreach (var s in items)
            {
                var row = new Border
                {
                    BackgroundColor = Color.FromArgb("#0d1117"),
                    Stroke = Color.FromArgb("#10b981"),
                    StrokeThickness = 1,
                    StrokeShape = new RoundRectangle { CornerRadius = 12 },
                    Padding = new Thickness(14, 12)
                };

                var stack = new VerticalStackLayout { Spacing = 3 };

                stack.Children.Add(new Label
                {
                    Text = s.ProjectName ?? ("Project #" + s.ProjectId),
                    FontSize = 15,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#e6edf3")
                });

                if (!string.IsNullOrWhiteSpace(s.ProjectLocation))
                {
                    var locLabel = new Label
                    {
                        FontSize = 12,
                        TextColor = Color.FromArgb("#0ea5e9")
                    };
                    locLabel.FormattedText = new FormattedString
                    {
                        Spans =
                        {
                            new Span { Text = s.ProjectLocation },
                            new Span { Text = "   ->  Directions", FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#f0a500"), FontSize = 11 }
                        }
                    };
                    var addr = s.ProjectLocation;
                    var tap = new TapGestureRecognizer();
                    tap.Tapped += async (sender, args) => await OpenDirections(addr);
                    locLabel.GestureRecognizers.Add(tap);
                    stack.Children.Add(locLabel);
                }

                var timeText = "All day";
                if (!string.IsNullOrWhiteSpace(s.StartTime))
                    timeText = s.StartTime + (string.IsNullOrWhiteSpace(s.EndTime) ? "" : " - " + s.EndTime);
                stack.Children.Add(new Label
                {
                    Text = timeText,
                    FontSize = 12,
                    TextColor = Color.FromArgb("#7d8590")
                });

                if (!string.IsNullOrWhiteSpace(s.Notes))
                    stack.Children.Add(new Label
                    {
                        Text = s.Notes,
                        FontSize = 12,
                        TextColor = Color.FromArgb("#b1bac4")
                    });

                row.Content = stack;
                SchedList.Children.Add(row);
            }

            RetryBtn.IsVisible = true;
        });
    }

    private async Task OpenDirections(string address)
    {
        try
        {
            var placemark = new Placemark { Thoroughfare = address };
            await Map.OpenAsync(placemark, new MapLaunchOptions { Name = address });
        }
        catch
        {
            try
            {
                var url = "https://www.google.com/maps/search/?api=1&query=" + Uri.EscapeDataString(address);
                await Launcher.OpenAsync(url);
            }
            catch { }
        }
    }

    private void OnRefresh(object sender, EventArgs e) => LoadSchedule();
}

