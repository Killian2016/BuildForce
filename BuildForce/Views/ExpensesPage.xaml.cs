#pragma warning disable CA1416
using BuildForce.Services;
using Microsoft.Maui.Controls.Shapes;

namespace BuildForce.Views;

public partial class ExpensesPage : ContentPage
{
    private readonly ApiService _api;
    private List<ExpenseSummary> _all = new();

    public ExpensesPage() : this(new ApiService()) { }

    public ExpensesPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        LoadExpenses();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadExpenses();
    }

    private async void LoadExpenses()
    {
        try
        {
            _all = await _api.GetExpensesAsync();
            SubtitleLabel.Text = $"{_all.Count} expenses \u00B7 {_all.Sum(x => x.Amount):C0} total";
            Render(SearchEntry.Text);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ExpensesPage load error: {ex}");
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
        ExpenseList.Children.Clear();
        var term = (filter ?? "").Trim().ToLowerInvariant();
        var items = string.IsNullOrEmpty(term)
            ? _all
            : _all.Where(x =>
                x.Description.ToLowerInvariant().Contains(term) ||
                (x.Vendor ?? "").ToLowerInvariant().Contains(term) ||
                (x.Category ?? "").ToLowerInvariant().Contains(term) ||
                (x.ProjectName ?? "").ToLowerInvariant().Contains(term)).ToList();

        EmptyLabel.IsVisible = items.Count == 0 && _all.Count > 0;

        foreach (var exp in items)
        {
            var card = new Border
            {
                BackgroundColor = Color.FromArgb("#0d1117"),
                Stroke = Color.FromArgb("#1c2330"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 14 },
                Padding = new Thickness(16, 13)
            };
            card.Shadow = new Shadow { Brush = new SolidColorBrush(Color.FromArgb("#1a2340")), Offset = new Point(0, 1), Radius = 6, Opacity = 0.06f };

            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            var left = new VerticalStackLayout { Spacing = 2 };
            left.Add(new Label { Text = $"{exp.ExpenseDate:MMM d, yyyy}{(string.IsNullOrWhiteSpace(exp.Vendor) ? "" : $" \u00B7 {exp.Vendor}")}", FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#7d8590") });
            left.Add(new Label { Text = exp.Description, FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#e6edf3"), LineBreakMode = LineBreakMode.TailTruncation });
            if (!string.IsNullOrWhiteSpace(exp.ProjectName))
                left.Add(new Label { Text = exp.ProjectName, FontSize = 11, TextColor = Color.FromArgb("#9aa3b8") });

            var right = new VerticalStackLayout { Spacing = 4, HorizontalOptions = LayoutOptions.End };
            right.Add(new Label { Text = exp.Amount.ToString("C2"), FontSize = 17, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#f85149"), HorizontalOptions = LayoutOptions.End });
            var badges = new HorizontalStackLayout { Spacing = 4, HorizontalOptions = LayoutOptions.End };
            if (!string.IsNullOrWhiteSpace(exp.Category))
            {
                badges.Add(new Border
                {
                    BackgroundColor = Color.FromArgb("#161b22"),
                    Stroke = Colors.Transparent,
                    StrokeShape = new RoundRectangle { CornerRadius = 20 },
                    Padding = new Thickness(9, 3),
                    Content = new Label { Text = exp.Category, FontSize = 10, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#7d8590") }
                });
            }
            if (exp.HasReceipt)
            {
                badges.Add(new Border
                {
                    BackgroundColor = Color.FromArgb("#12261a"),
                    Stroke = Colors.Transparent,
                    StrokeShape = new RoundRectangle { CornerRadius = 20 },
                    Padding = new Thickness(9, 3),
                    Content = new Label { Text = "Receipt", FontSize = 10, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#3fb950") }
                });
            }
            right.Add(badges);

            Grid.SetColumn(left, 0);
            Grid.SetColumn(right, 1);
            grid.Children.Add(left);
            grid.Children.Add(right);
            card.Content = grid;

            var expId = exp.Id;
            var tap = new TapGestureRecognizer();
            tap.Tapped += async (s, e) => await OpenDetailAsync(expId);
            card.GestureRecognizers.Add(tap);

            ExpenseList.Children.Add(card);
        }
    }

    private async Task OpenDetailAsync(int id)
    {
        try
        {
            await Application.Current!.MainPage!.Navigation.PushModalAsync(new ExpenseDetailPage(_api, id));
        }
        catch (Exception ex)
        {
            var host = Application.Current?.MainPage;
            if (host != null)
                await host.DisplayAlert("Navigation Error", ex.Message, "OK");
        }
    }

    private async void OnCreateExpense(object sender, TappedEventArgs e)
    {
        try
        {
            await Application.Current!.MainPage!.Navigation.PushModalAsync(new ExpenseCreatePage(_api));
        }
        catch (Exception ex)
        {
            var host = Application.Current?.MainPage;
            if (host != null)
                await host.DisplayAlert("Navigation Error", ex.Message, "OK");
        }
    }

    private async void OnBack(object sender, TappedEventArgs e)
    {
        try
        {
            await Application.Current!.MainPage!.Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Back error: {ex}");
        }
    }
}