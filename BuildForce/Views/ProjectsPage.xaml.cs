#pragma warning disable CA1416
using BuildForce.Services;

namespace BuildForce.Views;

public partial class ProjectsPage : ContentPage
{
    private readonly ApiService _api;

    public ProjectsPage()
    {
        InitializeComponent();
        _api = new ApiService();
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
            var projects = await _api.GetProjectsAsync();
            ProjectList.Children.Clear();

            foreach (var p in projects)
            {
                var badgeColor = p.Status == "Active" ? "#22c55e" :
                                 p.Status == "Pending" ? "#f59e0b" : "#6b7280";
                var badgeBg = p.Status == "Active" ? "#0d2e1a" :
                              p.Status == "Pending" ? "#2e1f0d" : "#1a1a2e";

                var row = new Border
                {
                    BackgroundColor = Color.FromArgb("#111f38"),
                    Stroke = Color.FromArgb("#1e3a5f"),
                    StrokeThickness = 1,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 },
                    Padding = new Thickness(14)
                };

                var grid = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = new GridLength(40) },
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Auto }
                    },
                    ColumnSpacing = 12
                };

                var icon = new Border
                {
                    BackgroundColor = Color.FromArgb("#1a2e14"),
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
                    Stroke = Colors.Transparent,
                    Padding = new Thickness(8),
                    Content = new Label { Text = "\U0001F3E2", FontSize = 16 }
                };

                var info = new VerticalStackLayout { Spacing = 2 };
                info.Add(new Label { Text = p.Name, FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, LineBreakMode = LineBreakMode.TailTruncation });
                info.Add(new Label { Text = p.Client, FontSize = 11, TextColor = Color.FromArgb("#6b7280") });
                info.Add(new Label { Text = p.Location, FontSize = 11, TextColor = Color.FromArgb("#6b7280"), LineBreakMode = LineBreakMode.TailTruncation });

                var badge = new Border
                {
                    BackgroundColor = Color.FromArgb(badgeBg),
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 20 },
                    Stroke = Color.FromArgb(badgeColor),
                    StrokeThickness = 1,
                    Padding = new Thickness(8, 3),
                    VerticalOptions = LayoutOptions.Center,
                    Content = new Label { Text = p.Status, FontSize = 10, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb(badgeColor) }
                };

                Grid.SetColumn(icon, 0);
                Grid.SetColumn(info, 1);
                Grid.SetColumn(badge, 2);
                grid.Children.Add(icon);
                grid.Children.Add(info);
                grid.Children.Add(badge);
                row.Content = grid;
                ProjectList.Children.Add(row);
            }

            var createBtn = new Border
            {
                BackgroundColor = Color.FromArgb("#c8e63c"),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
                Stroke = Colors.Transparent,
                Margin = new Thickness(0, 8, 0, 0),
                Content = new Label
                {
                    Text = "+ Create New Project",
                    FontSize = 15,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.Black,
                    HorizontalOptions = LayoutOptions.Center,
                    Padding = new Thickness(0, 16)
                }
            };
            var tap = new TapGestureRecognizer();
            tap.Tapped += OnCreateProject;
            createBtn.GestureRecognizers.Add(tap);
            ProjectList.Children.Add(createBtn);
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

    // NATIVE: open in-app project creation form (modal - same pattern as Dashboard)
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