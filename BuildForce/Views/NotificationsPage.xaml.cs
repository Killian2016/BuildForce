#pragma warning disable CA1416
using System.Linq;
using BuildForce.Services;
using Microsoft.Maui.Controls.Shapes;

namespace BuildForce.Views;

// In-app notifications [NOT3]. Read-only list; tapping an unread card marks it
// read. Rows are created server-side (timesheet approved/rejected today).
public partial class NotificationsPage : ContentPage
{
    private readonly ApiService _api;

    public NotificationsPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        Load();
    }

    private async void Load()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            SetLoading(true);
            EmptyLabel.IsVisible = false;
            NotifList.Children.Clear();
        });

        var items = await _api.GetNotificationsAsync(100);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            SetLoading(false);

            if (items == null)
            {
                SubLabel.Text = "Could not load";
                ShowEmpty(_api.LastError ?? "Notifications are unavailable right now.");
                return;
            }
            if (items.Count == 0)
            {
                SubLabel.Text = "Nothing yet";
                ShowEmpty("No notifications yet.\nTimesheet approvals will show up here.");
                return;
            }

            int unread = items.Count(i => !i.IsRead);
            SubLabel.Text = unread > 0 ? unread + " unread" : "All caught up";
            MarkAllBtn.IsVisible = unread > 0;
            ClearBtn.IsVisible = items.Count > unread;   // [NOT4] something read to clear

            foreach (var n in items)
                NotifList.Children.Add(BuildCard(n));
        });
    }

    private static Color TypeColor(string? type)
    {
        switch (type)
        {
            case "Success": return Color.FromArgb("#10b981");
            case "Warning": return Color.FromArgb("#f0a500");
            case "Error":   return Color.FromArgb("#ef4444");
            default:        return Color.FromArgb("#8b5cf6");
        }
    }

    private static string Ago(DateTime utc)
    {
        var span = DateTime.UtcNow - utc;
        if (span.TotalMinutes < 1) return "just now";
        if (span.TotalMinutes < 60) return (int)span.TotalMinutes + "m ago";
        if (span.TotalHours < 24) return (int)span.TotalHours + "h ago";
        if (span.TotalDays < 7) return (int)span.TotalDays + "d ago";
        return utc.ToLocalTime().ToString("MMM d");
    }

    private View BuildCard(NotificationItem n)
    {
        var col = TypeColor(n.Type);

        var border = new Border
        {
            BackgroundColor = Color.FromArgb("#0d1117"),
            Stroke = n.IsRead ? Color.FromArgb("#1c2330") : col,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 14 },
            Padding = new Thickness(14, 12)
        };

        var stack = new VerticalStackLayout { Spacing = 5 };

        var top = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = 8
        };

        top.Children.Add(new BoxView
        {
            Color = col,
            WidthRequest = 3,
            HeightRequest = 16,
            CornerRadius = 2,
            VerticalOptions = LayoutOptions.Center
        });

        var title = new Label
        {
            Text = n.Title ?? "Notification",
            FontSize = 14,
            FontAttributes = n.IsRead ? FontAttributes.None : FontAttributes.Bold,
            TextColor = Color.FromArgb("#e6edf3"),
            VerticalOptions = LayoutOptions.Center,
            LineBreakMode = LineBreakMode.TailTruncation
        };
        Grid.SetColumn(title, 1);
        top.Children.Add(title);

        var when = new Label
        {
            Text = Ago(n.CreatedDate),
            FontSize = 10,
            TextColor = Color.FromArgb("#7d8590"),
            VerticalOptions = LayoutOptions.Center
        };
        Grid.SetColumn(when, 2);
        top.Children.Add(when);

        stack.Children.Add(top);

        stack.Children.Add(new Label
        {
            Text = n.Message ?? "",
            FontSize = 12,
            TextColor = Color.FromArgb("#b1bac4")
        });

        border.Content = stack;

        if (!n.IsRead)
        {
            var captured = n;
            var tap = new TapGestureRecognizer();
            tap.Tapped += async (s, e) =>
            {
                captured.IsRead = true;
                border.Stroke = Color.FromArgb("#1c2330");
                await _api.MarkNotificationReadAsync(captured.Id);
            };
            border.GestureRecognizers.Add(tap);
        }

        return border;
    }

    private async void OnMarkAll(object sender, EventArgs e)
    {
        MarkAllBtn.IsVisible = false;
        await _api.MarkAllNotificationsReadAsync();
        Load();
    }


    // [NOT4] Removes only notifications already read - unread alerts stay.
    private async void OnClearRead(object sender, EventArgs e)
    {
        bool go = await DisplayAlert("Clear notifications",
            "Remove the notifications you have already read? Unread ones will stay.",
            "Clear", "Cancel");
        if (!go) return;
        ClearBtn.IsVisible = false;
        await _api.ClearReadNotificationsAsync();
        Load();
    }

    private void SetLoading(bool on) { Loading.IsRunning = on; Loading.IsVisible = on; }
    private void ShowEmpty(string text) { EmptyLabel.Text = text; EmptyLabel.IsVisible = true; }

    private async void OnClose(object sender, EventArgs e)
    {
        try { await Navigation.PopModalAsync(); } catch { }
    }
}