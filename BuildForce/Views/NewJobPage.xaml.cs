#pragma warning disable CA1416
using BuildForce.Services;
using Microsoft.Maui.Controls.Shapes;
namespace BuildForce.Views;

// [BFJOB1] Jobber-style quick job (service visit). Entry point is gated to office roles; the server re-checks.
public partial class NewJobPage : ContentPage
{
    private readonly ApiService _api;
    private List<CustomerSummary> _customers = new();
    private CustomerSummary? _customer;
    private DateTime _month = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
    private DateTime _selected = DateTime.Today;
    private TimeSpan? _start;
    private TimeSpan? _end;
    private readonly List<CrewPick> _crew = new();
    private readonly HashSet<int> _picked = new();
    private bool _notify = true;
    private static readonly List<TimeSpan?> SlotVals = new();
    private static readonly List<string> SlotNames = new();
    private static readonly Color Amber = Color.FromArgb("#f0a500");
    private static readonly Color Card = Color.FromArgb("#161b22");
    private static readonly Color Edge = Color.FromArgb("#1c2330");
    private static readonly Color Fg = Color.FromArgb("#e6edf3");
    private static readonly Color Muted = Color.FromArgb("#7d8590");
    private static readonly Color Bg = Color.FromArgb("#080b10");

    public NewJobPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        BuildDow();
        BuildCalendar();
        _ = LoadListsAsync();
    }

    private async Task LoadListsAsync()
    {
        try
        {
            _customers = await _api.GetCustomersAsync();
            var crew = await _api.GetVisitCrewAsync();
            _crew.Clear();
            _crew.AddRange(crew);
        }
        catch { }
        MainThread.BeginInvokeOnMainThread(BuildCrewChips);
    }

    private static Border Pill(View content, Color bg, Color stroke, double radius)
    {
        return new Border
        {
            Content = content, BackgroundColor = bg, Stroke = new SolidColorBrush(stroke), StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(radius) }
        };
    }

    private void BuildDow()
    {
        DowGrid.Children.Clear();
        var names = new[] { "S", "M", "T", "W", "T", "F", "S" };
        for (int i = 0; i < 7; i++)
        {
            var lbl = new Label { Text = names[i], FontSize = 12, TextColor = Muted, HorizontalOptions = LayoutOptions.Center };
            Grid.SetColumn(lbl, i);
            DowGrid.Children.Add(lbl);
        }
    }

    private void BuildCalendar()
    {
        MonthLabel.Text = _month.ToString("MMMM yyyy");
        CalGrid.Children.Clear();
        int offset = (int)_month.DayOfWeek;
        int days = DateTime.DaysInMonth(_month.Year, _month.Month);
        for (int d = 1; d <= days; d++)
        {
            int cell = offset + d - 1;
            var date = new DateTime(_month.Year, _month.Month, d);
            bool sel = date == _selected.Date;
            bool today = date == DateTime.Today;
            bool past = date < DateTime.Today;
            var lbl = new Label
            {
                Text = d.ToString(), FontSize = 14, TextColor = sel ? Bg : (past ? Muted : Fg),
                HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center,
                FontAttributes = (sel || today) ? FontAttributes.Bold : FontAttributes.None
            };
            var cellBorder = Pill(lbl, sel ? Amber : Colors.Transparent, (today && !sel) ? Amber : Colors.Transparent, 18);
            cellBorder.WidthRequest = 36;
            cellBorder.HeightRequest = 36;
            cellBorder.HorizontalOptions = LayoutOptions.Center;
            var pick = date;
            var tap = new TapGestureRecognizer();
            tap.Tapped += (s, e) => { _selected = pick; BuildCalendar(); };
            cellBorder.GestureRecognizers.Add(tap);
            Grid.SetRow(cellBorder, cell / 7);
            Grid.SetColumn(cellBorder, cell % 7);
            CalGrid.Children.Add(cellBorder);
        }
    }

    private void OnPrevMonth(object sender, TappedEventArgs e) { _month = _month.AddMonths(-1); BuildCalendar(); }
    private void OnNextMonth(object sender, TappedEventArgs e) { _month = _month.AddMonths(1); BuildCalendar(); }

    private void BuildCrewChips()
    {
        CrewWrap.Children.Clear();
        CrewHint.Text = _crew.Count == 0 ? "No active employees found." : "Tap to assign";
        foreach (var c in _crew)
        {
            bool on = _picked.Contains(c.Id);
            var lbl = new Label { Text = c.Name, FontSize = 13, TextColor = on ? Bg : Fg, VerticalOptions = LayoutOptions.Center };
            var chip = Pill(lbl, on ? Amber : Card, on ? Amber : Edge, 16);
            chip.Padding = new Thickness(12, 7);
            chip.Margin = new Thickness(0, 0, 8, 8);
            var id = c.Id;
            var tap = new TapGestureRecognizer();
            tap.Tapped += (s, e) => { if (!_picked.Remove(id)) _picked.Add(id); BuildCrewChips(); };
            chip.GestureRecognizers.Add(tap);
            CrewWrap.Children.Add(chip);
        }
    }

    private async void OnPickCustomer(object sender, TappedEventArgs e)
    {
        if (_customers.Count == 0) { ShowError("No customers yet - add one on the web first."); return; }
        var names = _customers
            .Select(c => string.IsNullOrWhiteSpace(c.Company) ? c.Name : c.Name + " (" + c.Company + ")").ToList();
        int cur = _customer == null ? -1 : _customers.IndexOf(_customer);
        int idx = await PickerSheetPage.PickIndexAsync(Navigation, "Customer", names, cur);
        if (idx < 0 || idx >= _customers.Count) return;
        _customer = _customers[idx];
        CustomerLabel.Text = names[idx];
        CustomerLabel.TextColor = Fg;
        if (string.IsNullOrWhiteSpace(TitleEntry.Text)) TitleEntry.Text = "Service visit - " + _customer.Name;
    }

    private static void EnsureSlots()
    {
        if (SlotNames.Count > 0) return;
        SlotVals.Add(null);
        SlotNames.Add("Any time");
        for (int h = 6; h <= 19; h++)
            for (int m = 0; m < 60; m += 30)
            {
                var ts = new TimeSpan(h, m, 0);
                SlotVals.Add(ts);
                SlotNames.Add(DateTime.Today.Add(ts).ToString("h:mm tt"));
            }
    }

    private async void OnPickStart(object sender, TappedEventArgs e)
    {
        EnsureSlots();
        int idx = await PickerSheetPage.PickIndexAsync(Navigation, "Arrival window - from", SlotNames, SlotVals.IndexOf(_start));
        if (idx < 0) return;
        _start = SlotVals[idx];
        StartLabel.Text = SlotNames[idx];
    }

    private async void OnPickEnd(object sender, TappedEventArgs e)
    {
        EnsureSlots();
        int idx = await PickerSheetPage.PickIndexAsync(Navigation, "Arrival window - to", SlotNames, SlotVals.IndexOf(_end));
        if (idx < 0) return;
        _end = SlotVals[idx];
        EndLabel.Text = SlotNames[idx];
    }

    private void OnToggleNotify(object sender, TappedEventArgs e)
    {
        _notify = !_notify;
        NotifyBox.BackgroundColor = _notify ? Amber : Card;
        NotifyBox.Stroke = new SolidColorBrush(_notify ? Amber : Edge);
        NotifyTick.IsVisible = _notify;
    }

    private void ShowError(string msg) { ErrorLabel.Text = msg; ErrorLabel.IsVisible = true; }

    private async void OnClose(object sender, TappedEventArgs e)
    {
        if (Navigation.ModalStack.Count > 0) await Navigation.PopModalAsync();
    }

    private async void OnSave(object sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;
        if (_customer == null) { ShowError("Pick a customer first."); return; }
        if (_start.HasValue && _end.HasValue && _end.Value <= _start.Value)
        {
            ShowError("Arrival window end must be after start.");
            return;
        }
        SaveBtn.IsEnabled = false;
        SaveBtn.Text = "Saving...";
        var req = new VisitCreateRequest
        {
            CustomerId = _customer.Id,
            Title = string.IsNullOrWhiteSpace(TitleEntry.Text) ? "Service visit" : TitleEntry.Text.Trim(),
            ServiceAddress = string.IsNullOrWhiteSpace(AddressEntry.Text) ? null : AddressEntry.Text.Trim(),
            VisitDate = _selected.ToString("yyyy-MM-dd"),
            WindowStart = _start.HasValue ? _start.Value.ToString(@"hh\:mm") : null,
            WindowEnd = _end.HasValue ? _end.Value.ToString(@"hh\:mm") : null,
            CrewEmployeeIds = _picked.ToList(),
            Notes = string.IsNullOrWhiteSpace(NotesEditor.Text) ? null : NotesEditor.Text.Trim(),
            NotifyEmail = _notify, NotifySms = _notify, SendConfirmation = _notify
        };
        var res = await _api.CreateVisitAsync(req);
        if (res == null || !res.Success)
        {
            SaveBtn.IsEnabled = true;
            SaveBtn.Text = "Save job";
            ShowError(_api.LastError ?? "Could not save the job.");
            return;
        }
        SaveBtn.Text = "Scheduled " + (char)0x2713 + (res.Sent > 0 ? "  (customer notified)" : "");
        await Task.Delay(900);
        if (Navigation.ModalStack.Count > 0) await Navigation.PopModalAsync();
    }
}
