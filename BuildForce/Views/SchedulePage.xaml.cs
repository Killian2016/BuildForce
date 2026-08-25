#pragma warning disable CA1416
using BuildForce.Services;
using Microsoft.Maui.Controls.Shapes;

namespace BuildForce.Views;

// [BFVIS5] FIELD OPS schedule - date selector, tinted status cards, crew and assignments, one primary action per visit.
public partial class SchedulePage : ContentPage
{
    private readonly ApiService _api;
    private DateTime _day = DateTime.Today;
    private List<VisitItem> _visits = new();
    private List<ScheduleItem> _sched = new();
    private bool _visitsFailed;

    private static readonly Color PageBg = Color.FromArgb("#0b1220");
    private static readonly Color Fg = Color.FromArgb("#f3f4f6");
    private static readonly Color Soft = Color.FromArgb("#d1d5db");
    private static readonly Color Muted = Color.FromArgb("#9aa3b8");
    private static readonly Color Amber = Color.FromArgb("#f0a500");
    private static readonly Color Link = Color.FromArgb("#60a5fa");
    private static readonly Color Chip = Color.FromArgb("#1c2538");
    private static readonly Color ChipEdge = Color.FromArgb("#2a3650");

    private sealed class Theme
    {
        public Color Bg = Colors.Transparent; public Color Edge = Colors.Transparent; public Color Pill = Colors.Transparent;
        public string Label = ""; public string Primary = ""; public Color PrimaryBg = Colors.Transparent;
    }

    private static Theme ThemeFor(string? status) => status switch
    {
        "OnTheWay" => new Theme { Bg = Color.FromArgb("#1f1538"), Edge = Color.FromArgb("#8b5cf6"), Pill = Color.FromArgb("#8b5cf6"),
            Label = "En Route", Primary = "Arrived", PrimaryBg = Color.FromArgb("#8b5cf6") },
        "InProgress" => new Theme { Bg = Color.FromArgb("#0f2a1e"), Edge = Color.FromArgb("#22c55e"), Pill = Color.FromArgb("#22c55e"),
            Label = "On Site", Primary = "Complete Job", PrimaryBg = Color.FromArgb("#22c55e") },
        "Completed" => new Theme { Bg = Color.FromArgb("#131a28"), Edge = Color.FromArgb("#374151"), Pill = Color.FromArgb("#6b7280"),
            Label = "Completed", Primary = "", PrimaryBg = Colors.Transparent },
        "Cancelled" => new Theme { Bg = Color.FromArgb("#2a1416"), Edge = Color.FromArgb("#ef4444"), Pill = Color.FromArgb("#ef4444"),
            Label = "Cancelled", Primary = "", PrimaryBg = Colors.Transparent },
        _ => new Theme { Bg = Color.FromArgb("#0c2240"), Edge = Color.FromArgb("#3b82f6"), Pill = Color.FromArgb("#3b82f6"),
            Label = "Scheduled", Primary = "En Route", PrimaryBg = Color.FromArgb("#3b82f6") }
    };

    private static INavigation HostNav => Application.Current!.Windows[0].Page!.Navigation;

    // [LIVEETA1] while a visit is En Route, send the crew's position every 60s (only while this page is visible)
    private IDispatcherTimer? _pingTimer;
    private bool _pinging;

    private void StartPing()
    {
        if (_pingTimer != null) return;
        _pingTimer = Dispatcher.CreateTimer();
        _pingTimer.Interval = TimeSpan.FromSeconds(60);
        _pingTimer.Tick += (s, e) => _ = PingAllAsync();
        _pingTimer.Start();
    }

    private void StopPing()
    {
        _pingTimer?.Stop();
        _pingTimer = null;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopPing();
    }

    private async Task PingAllAsync()
    {
        var ids = _visits.Where(x => x.Status == "OnTheWay").Select(x => x.Id).ToList();
        if (ids.Count == 0) return;
        await PingAsync(ids);
    }

    private async Task PingAsync(List<int> ids)
    {
        if (_pinging) return;
        _pinging = true;
        try
        {
            var loc = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(20)));
            if (loc == null) return;
            foreach (var id in ids) await _api.SendVisitLocationAsync(id, loc.Latitude, loc.Longitude);
        }
        catch { }
        finally { _pinging = false; }
    }

    public SchedulePage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        QuickAddBtn.IsVisible = AuthService.CanScheduleJobs;
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
            VisitList.Children.Clear();
            DoneList.Children.Clear();
            DoneRow.IsVisible = false;
            SchedList.Children.Clear();
            SchedHeader.IsVisible = false;
            DateChip.Text = _day.ToString("ddd, MMM d, yyyy");
            DateSel.Text = _day.ToString("ddd, MMM d, yyyy");
        });
        _ = LoadAllAsync();
    }

    private void OnRefresh(object sender, EventArgs e) => LoadSchedule();
    private void OnPrevDay(object sender, TappedEventArgs e) { _day = _day.AddDays(-1); LoadSchedule(); }
    private void OnNextDay(object sender, TappedEventArgs e) { _day = _day.AddDays(1); LoadSchedule(); }
    private void OnToday(object sender, TappedEventArgs e) { _day = DateTime.Today; LoadSchedule(); }

    private async void OnQuickAdd(object sender, TappedEventArgs e)
    {
        try { await HostNav.PushModalAsync(new NewJobPage(_api)); }
        catch { }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        StartPing(); // [LIVEETA1]
        QuickAddBtn.IsVisible = AuthService.CanScheduleJobs;
    }

    private async Task LoadAllAsync()
    {
        var visitsTask = _api.GetVisitsAsync(_day);
        var schedTask = _api.GetMyScheduleAsync(_day);
        var visits = await visitsTask;
        var sched = await schedTask;
        _visitsFailed = visits == null;
        _visits = visits ?? new List<VisitItem>();
        _sched = sched ?? new List<ScheduleItem>();
        MainThread.BeginInvokeOnMainThread(Render);
    }

    private void Render()
    {
        Loading.IsRunning = false;
        Loading.IsVisible = false;
        VisitList.Children.Clear();
        DoneList.Children.Clear();
        SchedList.Children.Clear();

        var live = _visits.Where(v => v.Status != "Completed" && v.Status != "Cancelled").ToList();
        var done = _visits.Where(v => v.Status == "Completed" || v.Status == "Cancelled").ToList();
        PendingSpan.Text = live.Count + " Pending";
        DoneSpan.Text = done.Count + " Completed";

        if (_visitsFailed)
        {
            EmptyLabel.Text = _api.LastError ?? "Could not load visits. Check your connection and try again.";
            EmptyLabel.IsVisible = true;
            RetryBtn.IsVisible = true;
        }
        else if (live.Count == 0)
        {
            EmptyLabel.Text = done.Count > 0 ? "All visits for this day are completed." : "No service visits scheduled for this day.";
            EmptyLabel.IsVisible = true;
        }
        foreach (var v in live) VisitList.Children.Add(BuildVisitCard(v, false));

        if (done.Count > 0)
        {
            DoneRow.IsVisible = true;
            DoneLabel.Text = "Completed - " + done.Count;
            foreach (var v in done) DoneList.Children.Add(BuildVisitCard(v, true));
        }
        else
        {
            DoneRow.IsVisible = false;
            DoneList.IsVisible = false;
        }

        if (_sched.Count > 0)
        {
            SchedHeader.IsVisible = true;
            foreach (var s in _sched) SchedList.Children.Add(BuildAssignmentCard(s));
        }
    }

    private static string Initials(string name)
    {
        var parts = name.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return "?";
        if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpperInvariant();
        return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
    }

    private static Border Pill(string text, Color bg, Color fg)
    {
        return new Border
        {
            BackgroundColor = bg, StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 10 }, Padding = new Thickness(10, 3),
            VerticalOptions = LayoutOptions.Center,
            Content = new Label { Text = text, FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = fg }
        };
    }

    private static Label Small(string text, Color color, double size = 12) => new Label { Text = text, FontSize = size, TextColor = color };

    private View BuildVisitCard(VisitItem v, bool muted)
    {
        var t = ThemeFor(v.Status);
        var stack = new VerticalStackLayout { Spacing = 6 };
        var card = new Border
        {
            Content = stack, BackgroundColor = t.Bg, Stroke = new SolidColorBrush(t.Edge), StrokeThickness = 1.5,
            StrokeShape = new RoundRectangle { CornerRadius = 16 }, Padding = new Thickness(14, 12, 14, 12), Opacity = muted ? 0.75 : 1
        };

        var head = new HorizontalStackLayout { Spacing = 8 };
        head.Children.Add(Pill(t.Label, t.Pill, PageBg));
        var titleLbl = new Label { FontSize = 13, TextColor = Soft, VerticalOptions = LayoutOptions.Center,
            LineBreakMode = LineBreakMode.TailTruncation };
        titleLbl.FormattedText = new FormattedString
        {
            Spans = { new Span { Text = "Service Visit: ", TextColor = Muted }, new Span { Text = v.Title ?? "Visit", TextColor = Fg,
                FontAttributes = FontAttributes.Bold } }
        };
        head.Children.Add(titleLbl);
        stack.Children.Add(head);

        stack.Children.Add(new Label { Text = string.IsNullOrWhiteSpace(v.CustomerName) ? "Customer" : v.CustomerName, FontSize = 22,
            FontAttributes = FontAttributes.Bold, TextColor = Fg, LineBreakMode = LineBreakMode.TailTruncation });

        var info = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(1),
            new ColumnDefinition(GridLength.Star) },
            ColumnSpacing = 12, Margin = new Thickness(0, 2, 0, 2) };
        var left = new VerticalStackLayout { Spacing = 1 };
        left.Children.Add(Small("Scheduled Time", Muted));
        left.Children.Add(new Label { Text = string.IsNullOrWhiteSpace(v.Window) ? "Any time" : v.Window, FontSize = 14, TextColor = Fg });
        info.Add(left, 0, 0);
        info.Add(new BoxView { Color = ChipEdge, WidthRequest = 1, VerticalOptions = LayoutOptions.Fill }, 1, 0);
        var right = new VerticalStackLayout { Spacing = 1 };
        var etaText = string.IsNullOrWhiteSpace(v.Eta) ? "--" : v.Eta;
        right.Children.Add(new Label { Text = "ETA " + etaText, FontSize = 14, TextColor = Fg });
if (v.Status == "OnTheWay") // [VIS2c] tap ETA to update it
{
    var vv = v; var tapEta = new TapGestureRecognizer();
    tapEta.Tapped += async (s2, e2) =>
    {
        var o2 = new List<string> { "15 min", "30 min", "45 min", "60 min", "No ETA" };
        int i2 = await PickerSheetPage.PickIndexAsync(HostNav, "Update ETA", o2, 1);
        if (i2 < 0) return;
        int? e3 = i2 switch { 0 => 15, 1 => 30, 2 => 45, 3 => 60, _ => (int?)null };
        await SetStatus(vv, "OnTheWay", e3);
    };
    right.GestureRecognizers.Add(tapEta);
}
        right.Children.Add(new Label { Text = string.IsNullOrWhiteSpace(v.Eta) ? "no ETA sent yet" : "sent to customer", FontSize = 12,
            TextColor = string.IsNullOrWhiteSpace(v.Eta) ? Muted : Amber });
        info.Add(right, 2, 0);
        stack.Children.Add(info);

        stack.Children.Add(new BoxView { Color = ChipEdge, HeightRequest = 1, Margin = new Thickness(0, 4, 0, 2) });

        var crewHeader = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            Padding = new Thickness(6, 4), MinimumHeightRequest = 44 }; // [CHV1] whole row tappable, press flash
        var chevron = new Label { Text = ((char)0x25B4).ToString(), FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Fg,
            HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(0, -2, 0, 0) };
        var chevChip = new Border { WidthRequest = 30, HeightRequest = 30, BackgroundColor = Chip, Stroke = new SolidColorBrush(ChipEdge),
            StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 15 }, Content = chevron,
            VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.End };
        crewHeader.Add(new Label { Text = "Crew & Assignments", FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Fg,
            VerticalOptions = LayoutOptions.Center }, 0, 0);
        crewHeader.Add(chevChip, 1, 0);
        stack.Children.Add(crewHeader);
        stack.Children.Add(new BoxView { Color = ChipEdge, HeightRequest = 1, Margin = new Thickness(0, 0, 0, 6) });

        var detail = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) },
            ColumnSpacing = 12 };
        var avatars = new HorizontalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Start };
        var crew = (v.Crew ?? new List<string>()).Where(n => !string.IsNullOrWhiteSpace(n)).Take(5).ToList();
        for (int i = 0; i < crew.Count; i++)
        {
            avatars.Children.Add(new Border
            {
                WidthRequest = 34, HeightRequest = 34, BackgroundColor = Chip, Stroke = new SolidColorBrush(t.Bg), StrokeThickness = 2,
                StrokeShape = new RoundRectangle { CornerRadius = 17 }, Margin = new Thickness(i == 0 ? 0 : -10, 0, 0, 0),
                Content = new Label { Text = Initials(crew[i]), FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = Fg,
                    HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center }
            });
        }
        if (crew.Count == 0) avatars.Children.Add(Small("No crew assigned", Muted, 12));
        detail.Add(avatars, 0, 0);
        var addrStack = new VerticalStackLayout { Spacing = 2 };
        if (!string.IsNullOrWhiteSpace(v.Address))
        {
            var addr = v.Address;
            addrStack.Children.Add(new Label { Text = addr, FontSize = 13, TextColor = Soft });
            var dirLink = new Label { Text = "Directions", FontSize = 13, TextColor = Link, TextDecorations = TextDecorations.Underline };
            var tapLink = new TapGestureRecognizer();
            tapLink.Tapped += async (s, e) => await OpenDirections(addr);
            dirLink.GestureRecognizers.Add(tapLink);
            addrStack.Children.Add(dirLink);
        }
        if (!string.IsNullOrWhiteSpace(v.Notes))
            addrStack.Children.Add(new Label { Text = v.Notes, FontSize = 12, TextColor = Muted, LineBreakMode = LineBreakMode.TailTruncation,
                MaxLines = 2 });
        detail.Add(addrStack, 1, 0);
        stack.Children.Add(detail);

        var tapCrew = new TapGestureRecognizer();
        tapCrew.Tapped += async (s, e) =>
        {
            detail.IsVisible = !detail.IsVisible;
            chevron.Text = (detail.IsVisible ? (char)0x25B4 : (char)0x25BE).ToString();
            crewHeader.BackgroundColor = Chip; await Task.Delay(120); crewHeader.BackgroundColor = Colors.Transparent;
        };
        crewHeader.GestureRecognizers.Add(tapCrew);

        var actions = new VerticalStackLayout { Spacing = 8, Margin = new Thickness(0, 6, 0, 0) }; // [ACT1] primary full-width
        var secondary = new Grid { ColumnSpacing = 8, ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) } };
        if (!muted && t.Primary.Length > 0)
        {
            var primary = new Button { Text = t.Primary, BackgroundColor = t.PrimaryBg, TextColor = PageBg, FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                HeightRequest = 42, CornerRadius = 12, Padding = new Thickness(18, 0), BorderWidth = 0 };
            var status = v.Status;
            primary.Clicked += async (s, e) =>
            {
                primary.IsEnabled = false;
                try
                {
                    if (status == "Scheduled") await OnMyWay(v);
                    else if (status == "OnTheWay") await SetStatus(v, "InProgress", null);
                    else if (status == "InProgress") await SetStatus(v, "Completed", null);
                }
                finally { primary.IsEnabled = true; }
            };
            actions.Children.Add(primary);
        }
        if (!string.IsNullOrWhiteSpace(v.CustomerPhone))
        {
            var phone = v.CustomerPhone;
            secondary.Add(Outline(((char)0x260E) + "  Call Client", () => { try { PhoneDialer.Default.Open(phone); } catch { } }), 0, 0);
        }
        if (!string.IsNullOrWhiteSpace(v.Address))
        {
            var addr2 = v.Address;
            secondary.Add(OutlineAsync(((char)0x2316) + "  Directions", async () => await OpenDirections(addr2)), 1, 0);
        }
        if (!muted && !string.IsNullOrWhiteSpace(v.CustomerPhone)) // [MSGVIS] customer visits only
        {
            secondary.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            var vv = v;
            secondary.Add(OutlineAsync(((char)0x2709) + "  Message", async () => await MessageClient(vv)), secondary.Children.Count, 0);
        }
        if (secondary.Children.Count == 1) Grid.SetColumnSpan((BindableObject)secondary.Children[0], secondary.ColumnDefinitions.Count);
        if (secondary.Children.Count > 0) actions.Children.Add(secondary);
        if (!muted && (v.Status == "Scheduled" || v.Status == "OnTheWay")) // [CANCEL1]
        {
            var vc = v;
            var cancelBtn = OutlineAsync(((char)0x2715) + "  Cancel Visit", async () =>
            {
                bool yes = await Application.Current!.Windows[0].Page!.DisplayAlert("Cancel this visit?",
                    "The visit will be marked cancelled for " + (vc.CustomerName ?? "the customer") + ".",
                    "Yes, cancel", "Keep visit");
                if (yes) await SetStatus(vc, "Cancelled", null);
            });
            cancelBtn.TextColor = Color.FromArgb("#ef4444");
            cancelBtn.BorderColor = Color.FromArgb("#ef4444");
            actions.Children.Add(cancelBtn);
        }
        stack.Children.Add(actions);
        return card;
    }

    private static Button Outline(string text, Action onTap)
    {
        var b = new Button { Text = text, BackgroundColor = Colors.Transparent, TextColor = Fg, BorderColor = ChipEdge, BorderWidth = 1,
            FontSize = 13, HeightRequest = 42, CornerRadius = 12, Padding = new Thickness(14, 0) };
        b.Clicked += (s, e) => onTap();
        return b;
    }

    private static Button OutlineAsync(string text, Func<Task> onTap)
    {
        var b = new Button { Text = text, BackgroundColor = Colors.Transparent, TextColor = Fg, BorderColor = ChipEdge, BorderWidth = 1,
            FontSize = 13, HeightRequest = 42, CornerRadius = 12, Padding = new Thickness(14, 0) };
        b.Clicked += async (s, e) => await onTap();
        return b;
    }

    // [MSGVIS] manual message to the customer from the card
    private async Task MessageClient(VisitItem v)
    {
        var opts = new List<string> { "Running about 15 min late", "On my way now", "Just wrapped up - thank you!", "Custom message..." };
        int idx = await PickerSheetPage.PickIndexAsync(HostNav, "Message " + (v.CustomerName ?? "customer"), opts, -1);
        if (idx < 0) return;
        string note = opts[idx];
        if (idx == 3)
        {
            var typed = await Application.Current!.Windows[0].Page!.DisplayPromptAsync("Message", "Text to send to " + (v.CustomerName ?? "the customer") + ":",
                "Send", "Cancel", maxLength: 500);
            if (string.IsNullOrWhiteSpace(typed)) return;
            note = typed.Trim();
        }
        bool okSend = await _api.SendVisitMessageAsync(v.Id, note);
        await DisplayAlert(okSend ? "Sent" : "Not sent",
            okSend ? "Your message is on its way." : (_api.LastError ?? "Could not send - check the visit log on the web."), "OK");
    }

    private async Task OnMyWay(VisitItem v)
    {
        var opts = new List<string> { "15 min", "30 min", "45 min", "60 min", "No ETA" };
        int idx = await PickerSheetPage.PickIndexAsync(HostNav, "ETA for " + (v.CustomerName ?? "customer"), opts, 1);
        if (idx < 0) return;
        int? eta = idx switch { 0 => 15, 1 => 30, 2 => 45, 3 => 60, _ => (int?)null };
        await SetStatus(v, "OnTheWay", eta);
    }

    private async Task SetStatus(VisitItem v, string status, int? eta)
    {
        var res = await _api.SetVisitStatusAsync(v.Id, status, eta, true);
        if (res != null && status == "OnTheWay") _ = PingAsync(new List<int> { v.Id }); // [LIVEETA1] first ping right away
        if (res != null && res.Success) { if (status == "OnTheWay") VisitPingService.Track(v.Id); else VisitPingService.Untrack(v.Id); } // [VISPING1] foreground trip pings
        if (res != null && status == "OnTheWay" && !string.IsNullOrWhiteSpace(v.Address)) _ = OpenDirections(v.Address); // [VIS2c] auto-open map
        if (res == null || !res.Success)
        {
            EmptyLabel.Text = _api.LastError ?? "Could not update the visit.";
            await DisplayAlert("Not updated", EmptyLabel.Text, "OK"); // [STATERR1]
            EmptyLabel.IsVisible = true;
            return;
        }
        _ = LoadAllAsync();
    }

    private void OnToggleDone(object sender, TappedEventArgs e)
    {
        DoneList.IsVisible = !DoneList.IsVisible;
        DoneChevron.Text = (DoneList.IsVisible ? (char)0x2303 : (char)0x2304).ToString();
    }

    private View BuildAssignmentCard(ScheduleItem s)
    {
        var row = new Border { BackgroundColor = Color.FromArgb("#111a2c"), Stroke = new SolidColorBrush(ChipEdge), StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 12 }, Padding = new Thickness(14, 12) };
        var stack = new VerticalStackLayout { Spacing = 3 };
        stack.Children.Add(new Label { Text = s.ProjectName ?? ("Project #" + s.ProjectId), FontSize = 15, FontAttributes = FontAttributes.Bold,
            TextColor = Fg });
        if (!string.IsNullOrWhiteSpace(s.ProjectLocation))
        {
            var addr = s.ProjectLocation;
            var loc = new Label { Text = addr, FontSize = 12, TextColor = Link };
            var tap = new TapGestureRecognizer();
            tap.Tapped += async (sender, args) => await OpenDirections(addr);
            loc.GestureRecognizers.Add(tap);
            stack.Children.Add(loc);
        }
        var timeText = "All day";
        if (!string.IsNullOrWhiteSpace(s.StartTime)) timeText = s.StartTime + (string.IsNullOrWhiteSpace(s.EndTime) ? "" : " - " + s.EndTime);
        stack.Children.Add(Small(timeText, Muted));
        if (!string.IsNullOrWhiteSpace(s.Notes)) stack.Children.Add(Small(s.Notes, Soft));
        row.Content = stack;
        return row;
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
}
