#pragma warning disable CA1416
using BuildForce.Services;
using Microsoft.Maui.Controls.Shapes;

namespace BuildForce.Views;

public partial class SafetyInspectionPage : ContentPage
{
    private readonly ApiService _api;
    private List<ProjectSummary> _projects = new();

    private static readonly (string Id, string Label, string Sub)[] Items =
    {
        ("ppe",          "PPE worn",              "Hard hats, vests, boots, eye/ear protection"),
        ("housekeeping", "Housekeeping",          "Site clean, debris cleared, walkways open"),
        ("ladders",      "Ladders & scaffolds",   "Secure, inspected, proper setup"),
        ("fall",         "Fall protection",       "Harnesses, guardrails, openings covered"),
        ("electrical",   "Electrical & cords",    "GFCI in use, cords undamaged"),
        ("tools",        "Tools condition",       "Good repair, guards in place"),
        ("firstaid",     "First aid kit",         "Stocked and accessible"),
        ("fire",         "Fire extinguisher",     "Present, charged, accessible"),
        ("guards",       "Equipment guards",      "Machine guards installed"),
        ("hazcom",       "Hazard communication",  "SDS available, containers labeled")
    };

    private readonly Dictionary<string, string> _results = new();
    private readonly Dictionary<string, List<Button>> _buttons = new();
    private readonly Dictionary<string, Entry> _noteEntries = new();
    private readonly Dictionary<string, Border> _noteBorders = new();

    private static readonly Color Green = Color.FromArgb("#10b981");
    private static readonly Color Red = Color.FromArgb("#ef4444");
    private static readonly Color Muted = Color.FromArgb("#7d8590");
    private static readonly Color Dark = Color.FromArgb("#e6edf3");
    private static readonly Color RowBg = Color.FromArgb("#0d1117");
    private static readonly Color BtnBg = Color.FromArgb("#161b22");

    public SafetyInspectionPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        DateLabel.Text = DateTime.Today.ToString("dddd, MMMM d, yyyy");
        TypePicker.Items.Add("Daily");
        TypePicker.Items.Add("Weekly");
        TypePicker.SelectedIndex = 0;
        BuildRows();
        LoadProjects();
    }

    private async void LoadProjects()
    {
        _projects = await _api.GetProjectsAsync();
        var active = await _api.GetActiveTimesheetAsync();
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ProjectPicker.Items.Clear();
            foreach (var p in _projects) ProjectPicker.Items.Add(p.Name);
            int idx = active != null ? _projects.FindIndex(p => p.Id == active.ProjectId) : -1;
            if (idx < 0 && _projects.Count > 0) idx = 0;
            ProjectPicker.SelectedIndex = idx;
        });
    }

    private void BuildRows()
    {
        foreach (var item in Items)
        {
            var row = new Border
            {
                BackgroundColor = RowBg,
                Stroke = Color.FromArgb("#1c2330"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Padding = new Thickness(12, 10)
            };

            var stack = new VerticalStackLayout { Spacing = 8 };
            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            var labels = new VerticalStackLayout { Spacing = 1 };
            labels.Children.Add(new Label { Text = item.Label, FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Dark });
            labels.Children.Add(new Label { Text = item.Sub, FontSize = 10, TextColor = Muted });
            grid.Children.Add(labels);

            var btnRow = new HorizontalStackLayout { Spacing = 6, VerticalOptions = LayoutOptions.Center };
            var btns = new List<Button>();
            foreach (var (val, text) in new[] { ("pass", "Pass"), ("fail", "Fail"), ("na", "N/A") })
            {
                var b = new Button
                {
                    Text = text,
                    FontSize = 11,
                    FontAttributes = FontAttributes.Bold,
                    HeightRequest = 32,
                    WidthRequest = 52,
                    CornerRadius = 8,
                    Padding = 0,
                    BackgroundColor = BtnBg,
                    TextColor = Muted
                };
                var itemId = item.Id;
                var value = val;
                b.Clicked += (s, e) => SetResult(itemId, value);
                btns.Add(b);
                btnRow.Children.Add(b);
            }
            _buttons[item.Id] = btns;
            Grid.SetColumn(btnRow, 1);
            grid.Children.Add(btnRow);
            stack.Children.Add(grid);

            var noteEntry = new Entry
            {
                Placeholder = "What failed / corrective action",
                TextColor = Dark,
                PlaceholderColor = Muted,
                BackgroundColor = Colors.Transparent,
                FontSize = 12
            };
            var noteBorder = new Border
            {
                BackgroundColor = Color.FromArgb("#161b22"),
                Stroke = Red,
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                Padding = new Thickness(10, 0),
                IsVisible = false,
                Content = noteEntry
            };
            _noteEntries[item.Id] = noteEntry;
            _noteBorders[item.Id] = noteBorder;
            stack.Children.Add(noteBorder);

            row.Content = stack;
            ItemsHost.Children.Add(row);
        }
    }

    private void SetResult(string itemId, string value)
    {
        _results[itemId] = value;
        var btns = _buttons[itemId];
        for (int i = 0; i < btns.Count; i++)
        {
            var isSel = (i == 0 && value == "pass") || (i == 1 && value == "fail") || (i == 2 && value == "na");
            var selColor = i == 0 ? Green : i == 1 ? Red : Muted;
            btns[i].BackgroundColor = isSel ? selColor : BtnBg;
            btns[i].TextColor = isSel ? Color.FromArgb("#080b10") : Muted;
        }
        _noteBorders[itemId].IsVisible = value == "fail";
        SetStatus("", null);
    }

    private void OnPassAll(object sender, EventArgs e)
    {
        foreach (var item in Items) SetResult(item.Id, "pass");
    }

    private async void OnFile(object sender, EventArgs e)
    {
        if (ProjectPicker.SelectedIndex < 0 || ProjectPicker.SelectedIndex >= _projects.Count)
        { SetStatus("Select a project first.", false); return; }

        var missing = Items.Count(i => !_results.ContainsKey(i.Id));
        if (missing > 0)
        { SetStatus(missing + " item" + (missing == 1 ? "" : "s") + " not answered yet.", false); return; }

        var project = _projects[ProjectPicker.SelectedIndex];
        var items = Items.Select(i => new SafetyItemSend
        {
            Id = i.Id,
            Label = i.Label,
            Result = _results[i.Id],
            Note = _results[i.Id] == "fail" ? _noteEntries[i.Id].Text : null
        }).ToList();

        FileBtn.IsEnabled = false;
        SetStatus("Filing...", null);

        var ok = await _api.FileSafetyInspectionAsync(new SafetyInspectionSend
        {
            ProjectId = project.Id,
            InspectionType = TypePicker.SelectedIndex == 1 ? "Weekly" : "Daily",
            Items = items,
            FollowUpRequired = FollowUpSwitch.IsToggled,
            Notes = NotesEditor.Text
        });

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            FileBtn.IsEnabled = true;
            if (ok)
            {
                var fails = items.Count(i => i.Result == "fail");
                SetStatus("Filed \u2713  " + project.Name + (fails > 0 ? " - " + fails + " item(s) failed" : " - all clear"), true);
                await Task.Delay(1400);
                try { await Navigation.PopModalAsync(); } catch { }
            }
            else SetStatus(_api.LastError ?? "Could not file - try again.", false);
        });
    }

    private void SetStatus(string msg, bool? good)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StatusLabel.Text = msg;
            StatusLabel.IsVisible = !string.IsNullOrEmpty(msg);
            StatusLabel.TextColor = good == true ? Green : good == false ? Red : Color.FromArgb("#f0a500");
        });
    }

    private async void OnClose(object sender, EventArgs e)
    {
        try { await Navigation.PopModalAsync(); } catch { }
    }
}