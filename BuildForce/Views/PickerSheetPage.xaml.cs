#pragma warning disable CA1416
using System.Linq;
using Microsoft.Maui.Controls.Shapes;

namespace BuildForce.Views;

// [PSH3a] Reusable dropdown sheet. Styling deliberately mirrors the job-switch
// "Leave for another job" sheet in Timeclockpage.xaml - violet accent on a dark card -
// so every dropdown in the app reads the same. Same modal-with-result idiom as
// SafetyCheckPage: callers await Result.Task (text) or IndexResult.Task (position).
// PREFER THE INDEX: two projects can legitimately share a name, so text is ambiguous.
public partial class PickerSheetPage : ContentPage
{
    // [SRCH-FIX] RunContinuationsAsynchronously: never resume the awaiting caller
    // inline, or a caller that navigates on the result races this page off the stack.
    public TaskCompletionSource<string?> Result { get; } =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource<int> IndexResult { get; } =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly List<KeyValuePair<int, string>> _all = new();
    private int _selIndex = -1;
    private const int SearchThreshold = 12;

    // Text-selected overload (kept for the existing ExpenseCreatePage category caller).
    public PickerSheetPage(string title, IEnumerable<string> options, string? selected = null, string? subtitle = null)
    {
        Init(title, options, subtitle);
        if (selected != null)
            _selIndex = _all.FindIndex(p => string.Equals(p.Value, selected, StringComparison.OrdinalIgnoreCase));
        Render(_all);
    }

    // Index-selected overload, used by MzPicker.
    public PickerSheetPage(string title, IEnumerable<string> options, int selectedIndex, string? subtitle = null)
    {
        Init(title, options, subtitle);
        _selIndex = selectedIndex;
        Render(_all);
    }

    // Opens the sheet and returns the picked POSITION, or -1 when cancelled.
    public static async Task<int> PickIndexAsync(INavigation nav, string title, IEnumerable<string> options,
                                                int selectedIndex = -1, string? subtitle = null)
    {
        var sheet = new PickerSheetPage(title, options, selectedIndex, subtitle);
        await nav.PushModalAsync(sheet);
        return await sheet.IndexResult.Task;
    }

    private void Init(string title, IEnumerable<string> options, string? subtitle)
    {
        InitializeComponent();
        TitleLabel.Text = title;
        if (!string.IsNullOrWhiteSpace(subtitle))
        {
            SubtitleLabel.Text = subtitle;
            SubtitleLabel.IsVisible = true;
        }
        int i = 0;
        foreach (var o in options ?? Enumerable.Empty<string>())
        {
            _all.Add(new KeyValuePair<int, string>(i, o ?? ""));
            i++;
        }
        SearchWrap.IsVisible = _all.Count >= SearchThreshold;
    }

    private void Render(List<KeyValuePair<int, string>> items)
    {
        OptionList.Children.Clear();

        if (items.Count == 0)
        {
            OptionList.Children.Add(new Label
            {
                Text = "No matches",
                FontSize = 13,
                TextColor = Color.FromArgb("#7d8590"),
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 14)
            });
            return;
        }

        foreach (var pair in items)
        {
            bool isSel = pair.Key == _selIndex;

            var row = new Border
            {
                BackgroundColor = Color.FromArgb("#161b22"),
                Stroke = Color.FromArgb(isSel ? "#8b5cf6" : "#1c2330"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Padding = new Thickness(14, 13)
            };

            row.Content = new Label
            {
                Text = pair.Value,
                FontSize = 14,
                FontAttributes = isSel ? FontAttributes.Bold : FontAttributes.None,
                TextColor = Color.FromArgb(isSel ? "#8b5cf6" : "#e6edf3"),
                HorizontalTextAlignment = TextAlignment.Center,
                LineBreakMode = LineBreakMode.TailTruncation
            };

            var capturedIndex = pair.Key;
            var tap = new TapGestureRecognizer();
            tap.Tapped += async (s, e) => { await CloseWith(capturedIndex); };
            row.GestureRecognizers.Add(tap);

            OptionList.Children.Add(row);
        }
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        var q = (e.NewTextValue ?? "").Trim();
        if (q.Length == 0) { Render(_all); return; }
        Render(_all.Where(p => p.Value.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList());
    }

    private async Task CloseWith(int index)
    {
        string? text = null;
        if (index >= 0)
        {
            var hit = _all.FirstOrDefault(p => p.Key == index);
            if (hit.Value != null) text = hit.Value;
        }
        // [SRCH-FIX] Pop BEFORE completing the task. Completing first resumed the
        // caller inline, which pushed its next page while this sheet was still on the
        // modal stack - then the PopModalAsync below popped THAT page instead, so a
        // search result tap opened and closed in the same frame and looked dead.
        try { await Navigation.PopModalAsync(); } catch { }
        Result.TrySetResult(text);
        IndexResult.TrySetResult(index);
    }

    private async void OnCancel(object sender, EventArgs e) => await CloseWith(-1);

    private async void OnScrimTapped(object sender, TappedEventArgs e) => await CloseWith(-1);

    protected override bool OnBackButtonPressed()
    {
        _ = CloseWith(-1);
        return true;
    }
}