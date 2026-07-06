#pragma warning disable CA1416
using BuildForce.Services;
using Microsoft.Maui.Controls.Shapes;

namespace BuildForce.Views;

public partial class ProjectsPage : ContentPage
{
    private readonly ApiService _api;
    private List<ProjectSummary> _all = new();

    public ProjectsPage() : this(new ApiService()) { }

    public ProjectsPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        LoadProjects();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadProjects();
    }

    private async void LoadProjects()
    {
        try
        {
            _all = await _api.GetProjectsAsync();
            SubtitleLabel.Text = $"{_all.Count} jobs for your company";
            Render(SearchEntry.Text);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ProjectsPage LoadProjects error: {ex}");
        }
        finally
        {
            Loading.IsRunning = false;
            Loading.IsVisible = false;
        }
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e) => Render(e.NewTextValue);

    private void Render(string? filter)
    {
        ProjectList.Children.Clear();
        var term = (filter ?? "").Trim().ToLowerInvariant();
        var items = string.IsNullOrEmpty(term)
            ? _all
            : _all.Where(p =>
                p.Name.ToLowerInvariant().Contains(term) ||
                p.Client.ToLowerInvariant().Contains(term) ||
                p.Location.ToLowerInvariant().Contains(term)).ToList();

        EmptyLabel.IsVisible = items.Count == 0 && _all.Count > 0;

        foreach (var p in items)
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

            var card = new Border
            {
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#e2e7f0"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 14 },
                Padding = new Thickness(14, 12)
            };
            card.Shadow = new Shadow { Brush = new SolidColorBrush(Color.FromArgb("#1a2340")), Offset = new Point(0, 1), Radius = 6, Opacity = 0.06f };

            var grid = new Grid
            {
                ColumnSpacing = 12,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(42) },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            var icon = new Border
            {
                BackgroundColor = Color.FromArgb("#fdf0d2"),
                Stroke = Colors.Transparent,
                StrokeShape = new RoundRectangle { CornerRadius = 10 },
                WidthRequest = 42, HeightRequest = 42,
                Content = new Label
                {
                    Text = "\u25A6", FontSize = 18, TextColor = Color.FromArgb("#f0a500"),
                    HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center
                }
            };

            var info = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
            info.Add(new Label { Text = p.Name, FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#1a2340"), LineBreakMode = LineBreakMode.TailTruncation });
            if (!string.IsNullOrWhiteSpace(p.Client))
                info.Add(new Label { Text = p.Client, FontSize = 11, TextColor = Color.FromArgb("#6b7280") });
            if (!string.IsNullOrWhiteSpace(p.Location))
                info.Add(new Label { Text = p.Location, FontSize = 11, TextColor = Color.FromArgb("#8a93a8"), LineBreakMode = LineBreakMode.TailTruncation });

            var pill = new Border
            {
                BackgroundColor = Color.FromArgb(pillBg),
                Stroke = Colors.Transparent,
                StrokeShape = new RoundRectangle { CornerRadius = 20 },
                Padding = new Thickness(10, 4),
                VerticalOptions = LayoutOptions.Center,
                Content = new Label { Text = p.Status, FontSize = 10, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb(pillText) }
            };

            Grid.SetColumn(icon, 0);
            Grid.SetColumn(info, 1);
            Grid.SetColumn(pill, 2);
            grid.Children.Add(icon);
            grid.Children.Add(info);
            grid.Children.Add(pill);
            card.Content = grid;

            var projectId = p.Id;
            var tap = new TapGestureRecognizer();
            tap.Tapped += async (s, e) => await OpenDetailAsync(projectId);
            card.GestureRecognizers.Add(tap);

            ProjectList.Children.Add(card);
        }
    }

    private async Task OpenDetailAsync(int projectId)
    {
        try
        {
            var host = Application.Current?.MainPage
                ?? throw new InvalidOperationException("No main page available.");
            await host.Navigation.PushModalAsync(new ProjectDetailPage(_api, projectId));
        }
        catch (Exception ex)
        {
            var host = Application.Current?.MainPage;
            if (host != null)
                await host.DisplayAlert("Navigation Error", $"Could not open project: {ex.Message}", "OK");
        }
    }

    private async void OnCreateProject(object? sender, TappedEventArgs e)
    {
        try
        {
            var host = Application.Current?.MainPage
                ?? throw new InvalidOperationException("No main page available.");
            await host.Navigation.PushModalAsync(new ProjectCreatePage(_api));
        }
        catch (Exception ex)
        {
            var host = Application.Current?.MainPage;
            if (host != null)
                await host.DisplayAlert("Navigation Error", $"Could not open Project form: {ex.Message}", "OK");
        }
    }
}