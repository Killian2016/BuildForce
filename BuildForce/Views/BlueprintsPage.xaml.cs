#pragma warning disable CA1416
using BuildForce.Services;
using Microsoft.Maui.Controls.Shapes;

namespace BuildForce.Views;

public partial class BlueprintsPage : ContentPage
{
    private readonly ApiService _api;
    private List<ProjectSummary> _projects = new();
    private int _selectedProjectId = 0;
    private bool _ready = false;

    public BlueprintsPage(ApiService api)
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
        SubLabel.Text = "Plans and sheets";
        LoadSheets();
    }

    private async void LoadSheets()
    {
        if (_selectedProjectId <= 0) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            SetLoading(true);
            EmptyLabel.IsVisible = false;
            SheetList.Children.Clear();
        });

        var sheets = await _api.GetBlueprintsAsync(_selectedProjectId);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            SetLoading(false);
            if (sheets == null)
            {
                ShowEmpty(_api.LastError ?? "Could not load blueprints.");
                return;
            }
            if (sheets.Count == 0)
            {
                ShowEmpty("No plan sheets yet for this project.\nUpload them on Mezano CM under Blueprints.");
                return;
            }
            foreach (var s in sheets)
                SheetList.Children.Add(BuildSheetCard(s));
        });
    }

    private View BuildSheetCard(BlueprintItem s)
    {
        var border = new Border
        {
            BackgroundColor = Color.FromArgb("#0d1117"),
            Stroke = Color.FromArgb("#1c2330"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 14 },
            Padding = new Thickness(14, 12)
        };

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = 12
        };

        bool isPdf = s.ContentType == "application/pdf";
        var badge = new Border
        {
            BackgroundColor = isPdf ? Color.FromArgb("#2a1210") : Color.FromArgb("#0e2231"),
            Stroke = isPdf ? Color.FromArgb("#ef4444") : Color.FromArgb("#0ea5e9"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            WidthRequest = 44, HeightRequest = 44,
            Content = new Label
            {
                Text = isPdf ? "PDF" : "IMG",
                FontSize = 11, FontAttributes = FontAttributes.Bold,
                TextColor = isPdf ? Color.FromArgb("#ef4444") : Color.FromArgb("#0ea5e9"),
                CharacterSpacing = 1,
                HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center
            }
        };
        grid.Children.Add(badge);

        var stack = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
        stack.Children.Add(new Label
        {
            Text = s.Title ?? s.FileName ?? "Sheet",
            FontSize = 15, FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#e6edf3"),
            LineBreakMode = LineBreakMode.TailTruncation
        });
        var by = string.IsNullOrWhiteSpace(s.UploadedByName) ? "" : "  \u2022  " + s.UploadedByName;
        stack.Children.Add(new Label
        {
            Text = FmtSize(s.FileSizeBytes) + "  \u2022  " + s.CreatedDate.ToLocalTime().ToString("MMM d, yyyy") + by,
            FontSize = 11, TextColor = Color.FromArgb("#7d8590"),
            LineBreakMode = LineBreakMode.TailTruncation
        });
        Grid.SetColumn(stack, 1);
        grid.Children.Add(stack);

        var chev = new Label
        {
            Text = "\u203a", FontSize = 22, TextColor = Color.FromArgb("#7d8590"),
            VerticalOptions = LayoutOptions.Center
        };
        Grid.SetColumn(chev, 2);
        grid.Children.Add(chev);

        border.Content = grid;

        var local = s;
        var tap = new TapGestureRecognizer();
        tap.Tapped += async (o, e2) =>
        {
            try
            {
                var host = Application.Current?.MainPage;
                if (host != null)
                    await host.Navigation.PushModalAsync(new BlueprintViewerPage(_api, local));
            }
            catch (Exception ex)
            {
                var host2 = Application.Current?.MainPage;
                if (host2 != null) await host2.DisplayAlert("Error", ex.Message, "OK");
            }
        };
        border.GestureRecognizers.Add(tap);
        return border;
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