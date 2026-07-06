#pragma warning disable CA1416
using BuildForce.Services;
using Microsoft.Maui.Controls.Shapes;

namespace BuildForce.Views;

public partial class EstimatesPage : ContentPage
{
    private readonly ApiService _api;
    private List<EstimateSummary> _all = new();
    private string _statusFilter = "All";
    private readonly string[] _filters = { "All", "Draft", "Sent", "Approved", "Rejected", "Converted", "Expired" };
    private readonly Dictionary<string, Border> _filterPills = new();

    public EstimatesPage() : this(new ApiService()) { }

    public EstimatesPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        BuildFilterRow();
        LoadEstimates();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadEstimates();
    }

    private void BuildFilterRow()
    {
        FilterRow.Children.Clear();
        _filterPills.Clear();
        foreach (var f in _filters)
        {
            var filter = f;
            var pill = new Border
            {
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#d8deeb"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 20 },
                Padding = new Thickness(14, 6),
                Content = new Label
                {
                    Text = filter,
                    FontSize = 12,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#6b7280")
                }
            };
            var tap = new TapGestureRecognizer();
            tap.Tapped += (s, e) => { _statusFilter = filter; StyleFilters(); Render(SearchEntry.Text); };
            pill.GestureRecognizers.Add(tap);
            _filterPills[filter] = pill;
            FilterRow.Children.Add(pill);
        }
        StyleFilters();
    }

    private void StyleFilters()
    {
        foreach (var kv in _filterPills)
        {
            var active = kv.Key == _statusFilter;
            kv.Value.BackgroundColor = active ? Color.FromArgb("#fdf0d2") : Colors.White;
            kv.Value.Stroke = active ? Color.FromArgb("#f0a500") : Color.FromArgb("#d8deeb");
            if (kv.Value.Content is Label l)
                l.TextColor = active ? Color.FromArgb("#8a6100") : Color.FromArgb("#6b7280");
        }
    }

    private async void LoadEstimates()
    {
        try
        {
            _all = await _api.GetEstimatesAsync();
            SubtitleLabel.Text = $"{_all.Count} estimates \u00B7 {_all.Sum(x => x.TotalAmount):C0} quoted";
            Render(SearchEntry.Text);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"EstimatesPage load error: {ex}");
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
        EstimateList.Children.Clear();
        var term = (filter ?? "").Trim().ToLowerInvariant();
        var items = _all.AsEnumerable();

        if (_statusFilter != "All")
            items = items.Where(x => x.Status == _statusFilter);

        if (!string.IsNullOrEmpty(term))
            items = items.Where(x =>
                x.EstimateNumber.ToLowerInvariant().Contains(term) ||
                (x.ProjectName ?? "").ToLowerInvariant().Contains(term) ||
                (x.CustomerName ?? "").ToLowerInvariant().Contains(term));

        var list = items.ToList();
        EmptyLabel.IsVisible = list.Count == 0 && _all.Count > 0;

        foreach (var est in list)
        {
            string amtColor, pillText, pillBg;
            switch (est.Status)
            {
                case "Approved":
                case "Converted": amtColor = "#0d7a4f"; pillText = "#0d7a4f"; pillBg = "#d8f5e8"; break;
                case "Rejected":
                case "Expired":   amtColor = "#b3261e"; pillText = "#b3261e"; pillBg = "#fde3e1"; break;
                case "Sent":      amtColor = "#1e50a0"; pillText = "#1e50a0"; pillBg = "#dde9fb"; break;
                default:          amtColor = "#5b6472"; pillText = "#5b6472"; pillBg = "#e8ecf3"; break;
            }

            var card = new Border
            {
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#e2e7f0"),
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
            left.Add(new Label { Text = $"{est.EstimateNumber} \u00B7 {est.EstimateDate:MMM d, yyyy}", FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#8a93a8") });
            left.Add(new Label { Text = string.IsNullOrWhiteSpace(est.ProjectName) ? "(no project)" : est.ProjectName, FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#1a2340"), LineBreakMode = LineBreakMode.TailTruncation });
            if (!string.IsNullOrWhiteSpace(est.CustomerName))
                left.Add(new Label { Text = est.CustomerName, FontSize = 11, TextColor = Color.FromArgb("#6b7280") });

            var right = new VerticalStackLayout { Spacing = 4, HorizontalOptions = LayoutOptions.End };
            right.Add(new Label { Text = est.TotalAmount.ToString("C0"), FontSize = 17, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb(amtColor), HorizontalOptions = LayoutOptions.End });
            right.Add(new Border
            {
                BackgroundColor = Color.FromArgb(pillBg),
                Stroke = Colors.Transparent,
                StrokeShape = new RoundRectangle { CornerRadius = 20 },
                Padding = new Thickness(9, 3),
                HorizontalOptions = LayoutOptions.End,
                Content = new Label { Text = est.Status, FontSize = 10, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb(pillText) }
            });

            Grid.SetColumn(left, 0);
            Grid.SetColumn(right, 1);
            grid.Children.Add(left);
            grid.Children.Add(right);
            card.Content = grid;

            var estId = est.Id;
            var tap = new TapGestureRecognizer();
            tap.Tapped += async (s, e) => await OpenDetailAsync(estId);
            card.GestureRecognizers.Add(tap);

            EstimateList.Children.Add(card);
        }
    }

    private async Task OpenDetailAsync(int id)
    {
        try
        {
            await Application.Current!.MainPage!.Navigation.PushModalAsync(new EstimateDetailPage(_api, id));
        }
        catch (Exception ex)
        {
            var host = Application.Current?.MainPage;
            if (host != null)
                await host.DisplayAlert("Navigation Error", ex.Message, "OK");
        }
    }

    private async void OnCreateEstimate(object sender, TappedEventArgs e)
    {
        try
        {
            await Application.Current!.MainPage!.Navigation.PushModalAsync(new EstimateCreatePage(_api));
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