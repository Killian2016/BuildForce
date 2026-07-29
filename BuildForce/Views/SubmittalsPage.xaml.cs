#pragma warning disable CA1416
using BuildForce.Services;
using Microsoft.Maui.Controls.Shapes;

namespace BuildForce.Views;

// Submittal register [SUB3b] - read-only. Crews check status and open the
// file; logging and reviewing happen on Mezano CM.
public partial class SubmittalsPage : ContentPage
{
    private readonly ApiService _api;
    private List<ProjectSummary> _projects = new();
    private int _selectedProjectId = 0;
    private bool _ready = false;

    public SubmittalsPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        InitAsync();
    }

    private async void InitAsync()
    {
        SetLoading(true);
        var projectsTask = _api.GetProjectsAsync();
        var activeTask = _api.GetActiveTimesheetAsync();
        await Task.WhenAll(projectsTask, activeTask);

        _projects = projectsTask.Result ?? new List<ProjectSummary>();
        var active = activeTask.Result;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            ProjectPicker.Items.Clear();
            foreach (var p in _projects)
                ProjectPicker.Items.Add(p.Name ?? ("Project " + p.Id));

            if (_projects.Count == 0)
            {
                SubLabel.Text = "No projects found";
                SetLoading(false);
                ShowEmpty("Create a project on Mezano CM first.");
                _ready = true;
                return;
            }

            int defaultIndex = 0;
            if (active != null && active.ProjectId > 0)
            {
                var idx = _projects.FindIndex(p => p.Id == active.ProjectId);
                if (idx >= 0) defaultIndex = idx;
            }

            _ready = true;
            ProjectPicker.SelectedIndex = defaultIndex;
        });
    }

    private void OnProjectChanged(object sender, EventArgs e)
    {
        if (!_ready) return;
        var idx = ProjectPicker.SelectedIndex;
        if (idx < 0 || idx >= _projects.Count) return;

        _selectedProjectId = _projects[idx].Id;
        HeaderLabel.Text = _projects[idx].Name ?? ("Project " + _selectedProjectId);
        SubLabel.Text = "Approvals and transmittals";
        LoadSubmittals();
    }

    private async void LoadSubmittals()
    {
        if (_selectedProjectId <= 0) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            SetLoading(true);
            EmptyLabel.IsVisible = false;
            SubList.Children.Clear();
        });

        var items = await _api.GetSubmittalsAsync(_selectedProjectId);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            SetLoading(false);
            if (items == null)
            {
                ShowEmpty(_api.LastError ?? "Could not load submittals.");
                return;
            }
            if (items.Count == 0)
            {
                ShowEmpty("No submittals yet for this project.\nLog them on Mezano CM under Submittals.");
                return;
            }
            foreach (var s in items)
                SubList.Children.Add(BuildCard(s));
        });
    }

    private static Color StatusColor(string? status)
    {
        switch (status)
        {
            case "Approved": return Color.FromArgb("#10b981");
            case "Approved as Noted": return Color.FromArgb("#0ea5e9");
            case "Revise and Resubmit": return Color.FromArgb("#f0a500");
            case "Rejected": return Color.FromArgb("#ef4444");
            case "Submitted": return Color.FromArgb("#8b5cf6");
            case "Under Review": return Color.FromArgb("#8b5cf6");
            default: return Color.FromArgb("#7d8590");
        }
    }

    private View BuildCard(SubmittalItem s)
    {
        var col = StatusColor(s.Status);

        var border = new Border
        {
            BackgroundColor = Color.FromArgb("#0d1117"),
            Stroke = Color.FromArgb("#1c2330"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 14 },
            Padding = new Thickness(14, 12)
        };

        var outer = new VerticalStackLayout { Spacing = 6 };

        var top = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = 10
        };

        var numRow = new HorizontalStackLayout { Spacing = 6 };
        numRow.Children.Add(new Label
        {
            Text = s.SubmittalNumber ?? "-",
            FontSize = 13, FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#f0a500"),
            VerticalOptions = LayoutOptions.Center
        });
        if (s.Revision > 0)
        {
            numRow.Children.Add(new Border
            {
                BackgroundColor = Color.FromArgb("#1c2330"),
                Stroke = Color.FromArgb("#2c3444"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 6 },
                Padding = new Thickness(6, 1),
                VerticalOptions = LayoutOptions.Center,
                Content = new Label
                {
                    Text = "REV " + s.Revision,
                    FontSize = 9, FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#b1bac4")
                }
            });
        }
        top.Children.Add(numRow);

        var statusBadge = new Border
        {
            BackgroundColor = Color.FromArgb("#0d1117"),
            Stroke = col,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            Padding = new Thickness(8, 2),
            VerticalOptions = LayoutOptions.Center,
            Content = new Label
            {
                Text = s.Status ?? "Draft",
                FontSize = 9, FontAttributes = FontAttributes.Bold,
                TextColor = col, CharacterSpacing = 0.5
            }
        };
        Grid.SetColumn(statusBadge, 1);
        top.Children.Add(statusBadge);
        outer.Children.Add(top);

        outer.Children.Add(new Label
        {
            Text = s.Title ?? "Submittal",
            FontSize = 15, FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#e6edf3"),
            LineBreakMode = LineBreakMode.TailTruncation
        });

        var meta = s.SubmittalType ?? "";
        if (!string.IsNullOrWhiteSpace(s.SubcontractorName)) meta += "  \u2022  " + s.SubcontractorName;
        if (s.DateRequired.HasValue) meta += "  \u2022  Due " + s.DateRequired.Value.ToString("MMM d");
        if (s.DateReturned.HasValue) meta += "  \u2022  Returned " + s.DateReturned.Value.ToLocalTime().ToString("MMM d");
        outer.Children.Add(new Label
        {
            Text = meta,
            FontSize = 11, TextColor = Color.FromArgb("#7d8590"),
            LineBreakMode = LineBreakMode.TailTruncation
        });

        if (!string.IsNullOrWhiteSpace(s.ReviewComments))
        {
            outer.Children.Add(new Border
            {
                BackgroundColor = Color.FromArgb("#161b22"),
                Stroke = Color.FromArgb("#1c2330"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                Padding = new Thickness(10, 7),
                Content = new Label
                {
                    Text = (string.IsNullOrWhiteSpace(s.ReviewedByName) ? "" : s.ReviewedByName + ": ") + s.ReviewComments,
                    FontSize = 11, TextColor = Color.FromArgb("#b1bac4")
                }
            });
        }

        outer.Children.Add(new Label
        {
            Text = s.HasFile ? ("Tap to open  \u2022  " + FmtSize(s.FileSizeBytes)) : "No file attached",
            FontSize = 10,
            TextColor = Color.FromArgb(s.HasFile ? "#0ea5e9" : "#5a6069")
        });

        border.Content = outer;

        var local = s;
        var tap = new TapGestureRecognizer();
        tap.Tapped += async (o, e2) => { await OpenFileAsync(local); };
        border.GestureRecognizers.Add(tap);
        return border;
    }

    private async Task OpenFileAsync(SubmittalItem s)
    {
        var host = Application.Current?.MainPage;
        if (host == null) return;
        if (!s.HasFile)
        {
            await host.DisplayAlert(s.SubmittalNumber ?? "Submittal", "No file attached to this revision.", "OK");
            return;
        }

        try
        {
            SetLoading(true);
            var bytes = await _api.GetSubmittalFileAsync(s.Id);
            SetLoading(false);

            if (bytes == null || bytes.Length == 0)
            {
                await host.DisplayAlert("Error", "Could not download the file.", "OK");
                return;
            }

            var name = s.FileName;
            if (string.IsNullOrWhiteSpace(name)) name = "submittal-" + s.Id + ".pdf";
            foreach (var bad in System.IO.Path.GetInvalidFileNameChars())
                name = name.Replace(bad, '_');

            var path = System.IO.Path.Combine(FileSystem.CacheDirectory, name);
            System.IO.File.WriteAllBytes(path, bytes);

            await Launcher.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(path),
                Title = s.SubmittalNumber ?? "Submittal"
            });
        }
        catch (Exception ex)
        {
            SetLoading(false);
            await host.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private static string FmtSize(long b)
    {
        if (b >= 1048576) return (b / 1048576.0).ToString("F1") + " MB";
        if (b >= 1024) return (b / 1024.0).ToString("F0") + " KB";
        return b + " B";
    }

    private void SetLoading(bool on) { Loading.IsRunning = on; Loading.IsVisible = on; }
    private void ShowEmpty(string text) { EmptyLabel.Text = text; EmptyLabel.IsVisible = true; }

    private async void OnClose(object sender, EventArgs e)
    {
        try { await Navigation.PopModalAsync(); } catch { }
    }
}