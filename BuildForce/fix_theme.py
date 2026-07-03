import os

ROOT  = r"C:\Users\mezan\source\repos\BuildForce\BuildForce"
VIEWS = os.path.join(ROOT, "Views")

def write(path, text):
    with open(path, "wb") as f:
        f.write(text.encode("utf-8"))
    print("OK  " + path)

# Add x:Name to all cards that need theme switching
DASH_XAML = os.path.join(VIEWS, "DashboardPage.xaml")
t = open(DASH_XAML, encoding="utf-8").read()

# Name the key cards
t = t.replace(
    '<Border Margin="20,0,20,16" BackgroundColor="#1a1200" Stroke="#f0a500"',
    '<Border x:Name="PlanBadge" Margin="20,0,20,16" BackgroundColor="#1a1200" Stroke="#f0a500"'
)
t = t.replace(
    '<Border Margin="20,0,20,12" BackgroundColor="#1a1400" Stroke="#f0a500"',
    '<Border x:Name="BudgetCard" Margin="20,0,20,12" BackgroundColor="#1a1400" Stroke="#f0a500"'
)
t = t.replace(
    '<Grid ColumnDefinitions="*,Auto" Padding="20,52,20,16" BackgroundColor="#0e0e0e">',
    '<Grid x:Name="HeaderGrid" ColumnDefinitions="*,Auto" Padding="20,52,20,16" BackgroundColor="#0e0e0e">'
)
t = t.replace(
    '<Border Grid.Row="0" Grid.Column="0" BackgroundColor="#161616" Stroke="#2a2a2a" StrokeThickness="1" Padding="16,14">\n            <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>\n            <Border.GestureRecognizers><TapGestureRecognizer Tapped="OnNewInvoiceTapped"/>',
    '<Border x:Name="InvoiceCard" Grid.Row="0" Grid.Column="0" BackgroundColor="#161616" Stroke="#2a2a2a" StrokeThickness="1" Padding="16,14">\n            <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>\n            <Border.GestureRecognizers><TapGestureRecognizer Tapped="OnNewInvoiceTapped"/>'
)
t = t.replace(
    '<Border Grid.Row="0" Grid.Column="1" BackgroundColor="#161616" Stroke="#2a2a2a" StrokeThickness="1" Padding="16,14">\n            <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>\n            <Border.GestureRecognizers><TapGestureRecognizer Tapped="OnScanReceiptTapped"/>',
    '<Border x:Name="ScanCard" Grid.Row="0" Grid.Column="1" BackgroundColor="#161616" Stroke="#2a2a2a" StrokeThickness="1" Padding="16,14">\n            <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>\n            <Border.GestureRecognizers><TapGestureRecognizer Tapped="OnScanReceiptTapped"/>'
)
t = t.replace(
    'x:Name="LoadingIndicator"',
    'x:Name="LoadingIndicator" '
)
# Name dock
t = t.replace(
    '<Border BackgroundColor="#161616" Stroke="#2a2a2a" StrokeThickness="1" Padding="12,10">',
    '<Border x:Name="DockBar" BackgroundColor="#161616" Stroke="#2a2a2a" StrokeThickness="1" Padding="12,10">'
)

write(DASH_XAML, t)

# DashboardPage.xaml.cs with full theme switching
DASH_CS = os.path.join(VIEWS, "DashboardPage.xaml.cs")
DASH_CS_TEXT = """#pragma warning disable CA1416
using BuildForce.ViewModels;

namespace BuildForce.Views;

public partial class DashboardPage : ContentPage
{
    readonly DashboardViewModel _vm;
    int _themeIndex = 0;

    // Theme definitions: Dark, Navy, Light
    readonly string[] _themeNames  = { "Dark",    "Navy",    "Light"   };
    readonly string[] _pageBg      = { "#0e0e0e", "#0a1628", "#f0f2f5" };
    readonly string[] _headerBg    = { "#0e0e0e", "#0a1628", "#f0f2f5" };
    readonly string[] _cardBg      = { "#161616", "#0f2040", "#ffffff" };
    readonly string[] _cardStroke  = { "#2a2a2a", "#1e3560", "#e2e8f0" };
    readonly string[] _budgetBg    = { "#1a1400", "#061020", "#fffbeb" };
    readonly string[] _planBg      = { "#1a1200", "#061020", "#fffbeb" };
    readonly string[] _dockBg      = { "#161616", "#0a1628", "#ffffff" };
    readonly string[] _dockStroke  = { "#2a2a2a", "#1e3560", "#e2e8f0" };
    readonly string[] _textPrimary = { "#ffffff", "#ffffff", "#0f172a" };
    readonly string[] _textSecond  = { "#888888", "#7a9cc4", "#64748b" };
    readonly string[] _textThird   = { "#444444", "#2a4a7a", "#94a3b8" };
    readonly string[] _labelColor  = { "#f0a500", "#f0a500", "#d4900a" };

    public DashboardPage(DashboardViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
        UpdateGreeting();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
        UpdateGreeting();
    }

    void UpdateGreeting()
    {
        var hour = DateTime.Now.Hour;
        GreetingLabel.Text = hour < 12 ? "Good morning" : hour < 17 ? "Good afternoon" : "Good evening";
    }

    void OnThemeToggleTapped(object sender, EventArgs e)
    {
        _themeIndex = (_themeIndex + 1) % _themeNames.Length;
        ApplyTheme();
    }

    void ApplyTheme()
    {
        int i = _themeIndex;

        ThemeLabel.Text = _themeNames[i];

        // Page background
        BackgroundColor = Color.FromArgb(_pageBg[i]);

        // Header
        HeaderGrid.BackgroundColor = Color.FromArgb(_headerBg[i]);
        GreetingLabel.TextColor = Color.FromArgb(_textSecond[i]);

        // Plan badge
        PlanBadge.BackgroundColor = Color.FromArgb(_planBg[i]);

        // Budget card
        BudgetCard.BackgroundColor = Color.FromArgb(_budgetBg[i]);
        UnpaidAmountLabel.TextColor = Color.FromArgb(_labelColor[i]);
        TotalOutstandingLabel.TextColor = Color.FromArgb(_textPrimary[i]);

        // Quick action cards
        InvoiceCard.BackgroundColor = Color.FromArgb(_cardBg[i]);
        InvoiceCard.Stroke = new SolidColorBrush(Color.FromArgb(_cardStroke[i]));
        ScanCard.BackgroundColor = Color.FromArgb(_cardBg[i]);
        ScanCard.Stroke = new SolidColorBrush(Color.FromArgb(_cardStroke[i]));

        // Section labels
        // Dock
        DockBar.BackgroundColor = Color.FromArgb(_dockBg[i]);
        DockBar.Stroke = new SolidColorBrush(Color.FromArgb(_dockStroke[i]));
    }

    // QUICK ACTIONS
    async void OnNewInvoiceTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("CreateInvoicePage");

    async void OnScanReceiptTapped(object sender, EventArgs e)
        => await Shell.Current.DisplayAlert("Scan Receipt", "AI receipt scanning coming soon!", "OK");

    async void OnTimeClockTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//TimesheetPage");

    async void OnCollectPayTapped(object sender, EventArgs e)
        => await Shell.Current.DisplayAlert("Collect Payment", "Select an invoice to collect payment.", "OK");

    async void OnSeeAllProjectsTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//ProjectsPage");

    // FLOATING DOCK
    async void OnDockProjectsTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//ProjectsPage");

    async void OnDockInvoicesTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//InvoicesPage");

    async void OnDockMoreTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//MorePage");
}
"""

write(DASH_CS, DASH_CS_TEXT)
print("\\nDone! Build and deploy to Samsung.")