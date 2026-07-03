import os

ROOT  = r"C:\Users\mezan\source\repos\BuildForce\BuildForce"
VIEWS = os.path.join(ROOT, "Views")

def write(path, text):
    with open(path, "wb") as f:
        f.write(text.encode("utf-8"))
    print("OK  " + path)

DASH_XAML = os.path.join(VIEWS, "DashboardPage.xaml")
DASH_TEXT = """<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="BuildForce.Views.DashboardPage"
             x:Name="PageRoot"
             BackgroundColor="#0e0e0e"
             Shell.NavBarIsVisible="False"
             Shell.TabBarIsVisible="False">

  <Grid RowDefinitions="*">

    <ScrollView>
      <VerticalStackLayout Spacing="0">

        <!-- HERO HEADER -->
        <Grid x:Name="HeaderGrid" ColumnDefinitions="*,Auto"
              Padding="24,56,24,20" BackgroundColor="#0e0e0e">
          <VerticalStackLayout Grid.Column="0" Spacing="4">
            <Label x:Name="GreetingLabel" Text="Good evening"
                   FontSize="13" TextColor="#555"/>
            <Label Text="BuildForce"
                   FontSize="34" FontAttributes="Bold"
                   TextColor="#f0a500"/>
            <HorizontalStackLayout Spacing="8">
              <Border BackgroundColor="#1a1200" Padding="10,4">
                <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
                <Label Text="PRO BUILDER" FontSize="10"
                       FontAttributes="Bold" TextColor="#f0a500"/>
              </Border>
              <Label Text="Mezano Consulting" FontSize="12" TextColor="#555"
                     VerticalOptions="Center"/>
            </HorizontalStackLayout>
          </VerticalStackLayout>
          <Grid Grid.Column="1" ColumnDefinitions="Auto,Auto"
                ColumnSpacing="10" VerticalOptions="Center">
            <Border Grid.Column="0" x:Name="ThemePill"
                    BackgroundColor="#1a1a1a" Stroke="#2a2a2a"
                    StrokeThickness="1" Padding="12,7">
              <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
              <Border.GestureRecognizers>
                <TapGestureRecognizer Tapped="OnThemeToggleTapped"/>
              </Border.GestureRecognizers>
              <Label x:Name="ThemeLabel" Text="Dark" FontSize="11"
                     FontAttributes="Bold" TextColor="#f0a500"/>
            </Border>
            <Border Grid.Column="1" BackgroundColor="#1a1a1a"
                    Stroke="#f0a500" StrokeThickness="1"
                    WidthRequest="46" HeightRequest="46">
              <Border.StrokeShape><RoundRectangle CornerRadius="15"/></Border.StrokeShape>
              <Label Text="MC" FontSize="14" FontAttributes="Bold"
                     TextColor="#f0a500"
                     HorizontalOptions="Center" VerticalOptions="Center"/>
            </Border>
          </Grid>
        </Grid>

        <!-- HERO REVENUE CARD -->
        <Border x:Name="HeroCard" Margin="20,0,20,16"
                BackgroundColor="#1a1400" Stroke="#f0a500"
                StrokeThickness="1.5" Padding="24,22">
          <Border.StrokeShape><RoundRectangle CornerRadius="28"/></Border.StrokeShape>
          <VerticalStackLayout Spacing="16">

            <Grid ColumnDefinitions="*,Auto">
              <VerticalStackLayout Grid.Column="0" Spacing="4">
                <Label Text="TOTAL REVENUE" FontSize="10"
                       FontAttributes="Bold" TextColor="#555"
                       CharacterSpacing="2"/>
                <Label x:Name="UnpaidAmountLabel" Text="$0.00"
                       FontSize="42" FontAttributes="Bold"
                       TextColor="#f0a500" CharacterSpacing="-1"/>
                <Label Text="Outstanding balance" FontSize="12"
                       TextColor="#666"/>
              </VerticalStackLayout>
              <Border Grid.Column="1" x:Name="OverdueBadge"
                      BackgroundColor="#2a1800" Stroke="#f0a500"
                      StrokeThickness="0.5" Padding="12,8"
                      VerticalOptions="Start">
                <Border.StrokeShape><RoundRectangle CornerRadius="12"/></Border.StrokeShape>
                <Label x:Name="OverdueCountLabel" Text="0 Overdue"
                       FontSize="12" TextColor="#f0a500"
                       FontAttributes="Bold"/>
              </Border>
            </Grid>

            <!-- Progress bar -->
            <VerticalStackLayout Spacing="6">
              <Border x:Name="ProgressBg" BackgroundColor="#2a2a2a" HeightRequest="8">
                <Border.StrokeShape><RoundRectangle CornerRadius="4"/></Border.StrokeShape>
                <BoxView x:Name="PaidProgressBar" BackgroundColor="#22c55e"
                         HeightRequest="8" HorizontalOptions="Start" WidthRequest="0"/>
              </Border>
              <Grid ColumnDefinitions="Auto,*,Auto">
                <Label Grid.Column="0" Text="Overdue"
                       FontSize="11" TextColor="#f0a500"/>
                <Label Grid.Column="2" Text="Paid"
                       FontSize="11" TextColor="#22c55e"/>
              </Grid>
            </VerticalStackLayout>

            <!-- Paid / Outstanding row -->
            <Grid ColumnDefinitions="*,1,*" ColumnSpacing="0">
              <VerticalStackLayout Grid.Column="0" Spacing="4">
                <Label Text="PAID" FontSize="9" FontAttributes="Bold"
                       TextColor="#555" CharacterSpacing="1.5"/>
                <Label x:Name="PaidAmountLabel" Text="$0.00"
                       FontSize="24" FontAttributes="Bold"
                       TextColor="#22c55e"/>
              </VerticalStackLayout>
              <BoxView Grid.Column="1" BackgroundColor="#2a2a2a" WidthRequest="1"/>
              <VerticalStackLayout Grid.Column="2" Spacing="4" Margin="16,0,0,0">
                <Label Text="OUTSTANDING" FontSize="9" FontAttributes="Bold"
                       TextColor="#555" CharacterSpacing="1.5"/>
                <Label x:Name="TotalOutstandingLabel" Text="$0.00"
                       FontSize="24" FontAttributes="Bold"
                       TextColor="#ffffff"/>
              </VerticalStackLayout>
            </Grid>

          </VerticalStackLayout>
        </Border>

        <!-- STATS ROW — 3 wide pills -->
        <Grid Margin="20,0,20,20" ColumnDefinitions="*,*,*" ColumnSpacing="10">
          <Border Grid.Column="0" x:Name="ActiveCard"
                  BackgroundColor="#0c1e2e" Stroke="#0ea5e9"
                  StrokeThickness="1" Padding="0,16">
            <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
            <VerticalStackLayout Spacing="4" HorizontalOptions="Center">
              <Label x:Name="ActiveJobsLabel" Text="0"
                     FontSize="32" FontAttributes="Bold"
                     TextColor="#0ea5e9" HorizontalOptions="Center"/>
              <Label Text="Active" FontSize="11"
                     TextColor="#0ea5e9" HorizontalOptions="Center"/>
            </VerticalStackLayout>
          </Border>
          <Border Grid.Column="1" x:Name="UrgentCard"
                  BackgroundColor="#1a0808" Stroke="#ef4444"
                  StrokeThickness="1" Padding="0,16">
            <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
            <VerticalStackLayout Spacing="4" HorizontalOptions="Center">
              <Label x:Name="NeedsAttentionLabel" Text="0"
                     FontSize="32" FontAttributes="Bold"
                     TextColor="#ef4444" HorizontalOptions="Center"/>
              <Label Text="Urgent" FontSize="11"
                     TextColor="#ef4444" HorizontalOptions="Center"/>
            </VerticalStackLayout>
          </Border>
          <Border Grid.Column="2" x:Name="PendingCard"
                  BackgroundColor="#100820" Stroke="#8b5cf6"
                  StrokeThickness="1" Padding="0,16">
            <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
            <VerticalStackLayout Spacing="4" HorizontalOptions="Center">
              <Label x:Name="EstimatesLabel" Text="0"
                     FontSize="32" FontAttributes="Bold"
                     TextColor="#8b5cf6" HorizontalOptions="Center"/>
              <Label Text="Pending" FontSize="11"
                     TextColor="#8b5cf6" HorizontalOptions="Center"/>
            </VerticalStackLayout>
          </Border>
        </Grid>

        <!-- QUICK ACTIONS — 2 tall cards -->
        <Label Text="QUICK ACTIONS" Margin="24,0,24,12"
               FontSize="10" FontAttributes="Bold"
               TextColor="#444" CharacterSpacing="2"/>

        <Grid Margin="20,0,20,20" ColumnDefinitions="*,*"
              ColumnSpacing="12">

          <!-- Clock In CTA — primary action -->
          <Border Grid.Column="0" x:Name="ClockCard"
                  BackgroundColor="#0d1a0d" Stroke="#22c55e"
                  StrokeThickness="1.5" Padding="20,24"
                  HeightRequest="180">
            <Border.StrokeShape><RoundRectangle CornerRadius="24"/></Border.StrokeShape>
            <Border.GestureRecognizers>
              <TapGestureRecognizer Tapped="OnTimeClockTapped"/>
            </Border.GestureRecognizers>
            <VerticalStackLayout Spacing="12" VerticalOptions="Center">
              <Label Text="⏱" FontSize="40"
                     HorizontalOptions="Center"/>
              <Label Text="Clock In" FontSize="18"
                     FontAttributes="Bold" TextColor="#22c55e"
                     HorizontalOptions="Center"/>
              <Label Text="GPS verified" FontSize="12"
                     TextColor="#1a5c1a"
                     HorizontalOptions="Center"/>
            </VerticalStackLayout>
          </Border>

          <!-- Right column — 2 stacked -->
          <VerticalStackLayout Grid.Column="1" Spacing="12">
            <Border x:Name="InvoiceCard"
                    BackgroundColor="#161616" Stroke="#2a2a2a"
                    StrokeThickness="1" Padding="20,20"
                    HeightRequest="84">
              <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
              <Border.GestureRecognizers>
                <TapGestureRecognizer Tapped="OnNewInvoiceTapped"/>
              </Border.GestureRecognizers>
              <Grid ColumnDefinitions="Auto,*">
                <Label Grid.Column="0" Text="🧾" FontSize="28"
                       VerticalOptions="Center"/>
                <VerticalStackLayout Grid.Column="1" Spacing="2"
                                     Margin="12,0,0,0"
                                     VerticalOptions="Center">
                  <Label Text="Invoice" FontSize="15"
                         FontAttributes="Bold" TextColor="#ffffff"/>
                  <Label Text="60 sec" FontSize="11"
                         TextColor="#555"/>
                </VerticalStackLayout>
              </Grid>
            </Border>

            <Border x:Name="CollectCard"
                    BackgroundColor="#1a1200" Stroke="#f0a500"
                    StrokeThickness="1" Padding="20,20"
                    HeightRequest="84">
              <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
              <Border.GestureRecognizers>
                <TapGestureRecognizer Tapped="OnCollectPayTapped"/>
              </Border.GestureRecognizers>
              <Grid ColumnDefinitions="Auto,*">
                <Label Grid.Column="0" Text="💳" FontSize="28"
                       VerticalOptions="Center"/>
                <VerticalStackLayout Grid.Column="1" Spacing="2"
                                     Margin="12,0,0,0"
                                     VerticalOptions="Center">
                  <Label Text="Collect" FontSize="15"
                         FontAttributes="Bold" TextColor="#f0a500"/>
                  <Label Text="On-site" FontSize="11"
                         TextColor="#555"/>
                </VerticalStackLayout>
              </Grid>
            </Border>
          </VerticalStackLayout>
        </Grid>

        <!-- ACTIVE JOBS -->
        <Grid Margin="24,0,24,12" ColumnDefinitions="*,Auto">
          <Label Grid.Column="0" Text="ACTIVE JOBS"
                 FontSize="10" FontAttributes="Bold"
                 TextColor="#444" CharacterSpacing="2"/>
          <Label Grid.Column="1" Text="See all →"
                 FontSize="13" TextColor="#f0a500">
            <Label.GestureRecognizers>
              <TapGestureRecognizer Tapped="OnSeeAllProjectsTapped"/>
            </Label.GestureRecognizers>
          </Label>
        </Grid>

        <CollectionView x:Name="ActiveJobsCollection"
                        Margin="20,0,20,0" SelectionMode="None">
          <CollectionView.ItemTemplate>
            <DataTemplate>
              <Border x:Name="JobCard" BackgroundColor="#161616"
                      Stroke="#2a2a2a" StrokeThickness="1"
                      Padding="20,16" Margin="0,0,0,12">
                <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
                <Grid ColumnDefinitions="Auto,*,Auto" ColumnSpacing="14">
                  <BoxView Grid.Column="0" BackgroundColor="#f0a500"
                           WidthRequest="3" CornerRadius="2"
                           VerticalOptions="Fill"/>
                  <VerticalStackLayout Grid.Column="1" Spacing="4">
                    <Label Text="{Binding Name}" FontSize="16"
                           FontAttributes="Bold" TextColor="#ffffff"/>
                    <Label Text="{Binding CustomerName}"
                           FontSize="12" TextColor="#555"/>
                  </VerticalStackLayout>
                  <VerticalStackLayout Grid.Column="2"
                                       HorizontalOptions="End"
                                       Spacing="6">
                    <Label Text="{Binding BudgetFormatted}"
                           FontSize="17" FontAttributes="Bold"
                           TextColor="#f0a500"
                           HorizontalOptions="End"/>
                    <Border BackgroundColor="#1a1200" Padding="8,4">
                      <Border.StrokeShape>
                        <RoundRectangle CornerRadius="8"/>
                      </Border.StrokeShape>
                      <Label Text="{Binding Status}" FontSize="10"
                             FontAttributes="Bold" TextColor="#f0a500"/>
                    </Border>
                  </VerticalStackLayout>
                </Grid>
              </Border>
            </DataTemplate>
          </CollectionView.ItemTemplate>
        </CollectionView>

        <ActivityIndicator x:Name="LoadingIndicator"
                           IsRunning="False" IsVisible="False"
                           Color="#f0a500" HorizontalOptions="Center"
                           Margin="0,10,0,0"/>

        <BoxView HeightRequest="120" Color="Transparent"/>
      </VerticalStackLayout>
    </ScrollView>

    <!-- FLOATING DOCK -->
    <Border x:Name="DockBar"
            Margin="20,0,20,32"
            VerticalOptions="End"
            BackgroundColor="#161616"
            Stroke="#2a2a2a" StrokeThickness="1"
            Padding="16,14">
      <Border.StrokeShape><RoundRectangle CornerRadius="32"/></Border.StrokeShape>
      <Grid ColumnDefinitions="*,*,Auto,*,*" ColumnSpacing="0">

        <VerticalStackLayout Grid.Column="0" Spacing="4"
                             HorizontalOptions="Center" Padding="6,0">
          <Border BackgroundColor="#1a1200" Padding="8,6"
                  HorizontalOptions="Center">
            <Border.StrokeShape><RoundRectangle CornerRadius="12"/></Border.StrokeShape>
            <Label Text="⊞" FontSize="18" TextColor="#f0a500"
                   HorizontalOptions="Center"/>
          </Border>
          <Label Text="DASH" FontSize="8" FontAttributes="Bold"
                 TextColor="#f0a500" HorizontalOptions="Center"/>
        </VerticalStackLayout>

        <VerticalStackLayout Grid.Column="1" Spacing="4"
                             HorizontalOptions="Center" Padding="6,0">
          <VerticalStackLayout.GestureRecognizers>
            <TapGestureRecognizer Tapped="OnDockProjectsTapped"/>
          </VerticalStackLayout.GestureRecognizers>
          <Label Text="📁" FontSize="22" HorizontalOptions="Center"/>
          <Label Text="JOBS" FontSize="8" FontAttributes="Bold"
                 TextColor="#444" HorizontalOptions="Center"/>
        </VerticalStackLayout>

        <!-- CENTER BUTTON -->
        <Border Grid.Column="2" BackgroundColor="#f0a500"
                WidthRequest="60" HeightRequest="60"
                Margin="4,-14,4,0" VerticalOptions="End">
          <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
          <Border.GestureRecognizers>
            <TapGestureRecognizer Tapped="OnTimeClockTapped"/>
          </Border.GestureRecognizers>
          <Label Text="⏱" FontSize="26"
                 HorizontalOptions="Center"
                 VerticalOptions="Center"/>
        </Border>

        <VerticalStackLayout Grid.Column="3" Spacing="4"
                             HorizontalOptions="Center" Padding="6,0">
          <VerticalStackLayout.GestureRecognizers>
            <TapGestureRecognizer Tapped="OnDockInvoicesTapped"/>
          </VerticalStackLayout.GestureRecognizers>
          <Label Text="📄" FontSize="22" HorizontalOptions="Center"/>
          <Label Text="PAY" FontSize="8" FontAttributes="Bold"
                 TextColor="#444" HorizontalOptions="Center"/>
        </VerticalStackLayout>

        <VerticalStackLayout Grid.Column="4" Spacing="4"
                             HorizontalOptions="Center" Padding="6,0">
          <VerticalStackLayout.GestureRecognizers>
            <TapGestureRecognizer Tapped="OnDockMoreTapped"/>
          </VerticalStackLayout.GestureRecognizers>
          <Label Text="⋯" FontSize="22" TextColor="#666"
                 HorizontalOptions="Center"/>
          <Label Text="MORE" FontSize="8" FontAttributes="Bold"
                 TextColor="#444" HorizontalOptions="Center"/>
        </VerticalStackLayout>

      </Grid>
    </Border>

  </Grid>
</ContentPage>
"""

DASH_CS = os.path.join(VIEWS, "DashboardPage.xaml.cs")
DASH_CS_TEXT = """#pragma warning disable CA1416
using BuildForce.ViewModels;

namespace BuildForce.Views;

public partial class DashboardPage : ContentPage
{
    readonly DashboardViewModel _vm;
    int _themeIndex = 0;

    readonly string[] _themeNames = { "Dark", "Navy", "Light" };
    readonly string[] _pageBg     = { "#0e0e0e", "#0a1628", "#f0f2f5" };
    readonly string[] _headerBg   = { "#0e0e0e", "#0a1628", "#f0f2f5" };
    readonly string[] _heroBg     = { "#1a1400", "#061530", "#fffbeb" };
    readonly string[] _cardBg     = { "#161616", "#0f2040", "#ffffff" };
    readonly string[] _cardStroke = { "#2a2a2a", "#1e3560", "#e2e8f0" };
    readonly string[] _dockBg     = { "#161616", "#0a1628", "#ffffff" };
    readonly string[] _dockStroke = { "#2a2a2a", "#1e3560", "#e2e8f0" };
    readonly string[] _textMain   = { "#ffffff", "#ffffff", "#0f172a" };
    readonly string[] _textSub    = { "#555555", "#2a4a7a", "#94a3b8" };
    readonly string[] _gold       = { "#f0a500", "#f0a500", "#d4900a" };
    readonly string[] _sectionLbl = { "#444444", "#2a4a7a", "#94a3b8" };

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
        var h = DateTime.Now.Hour;
        GreetingLabel.Text = h < 12 ? "Good morning" : h < 17 ? "Good afternoon" : "Good evening";
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
        BackgroundColor = Color.FromArgb(_pageBg[i]);
        HeaderGrid.BackgroundColor = Color.FromArgb(_headerBg[i]);
        HeroCard.BackgroundColor = Color.FromArgb(_heroBg[i]);
        InvoiceCard.BackgroundColor = Color.FromArgb(_cardBg[i]);
        InvoiceCard.Stroke = new SolidColorBrush(Color.FromArgb(_cardStroke[i]));
        ClockCard.BackgroundColor = Color.FromArgb(i == 2 ? "#e8f5e8" : "#0d1a0d");
        CollectCard.BackgroundColor = Color.FromArgb(i == 2 ? "#fffbeb" : "#1a1200");
        DockBar.BackgroundColor = Color.FromArgb(_dockBg[i]);
        DockBar.Stroke = new SolidColorBrush(Color.FromArgb(_dockStroke[i]));
        ActiveCard.BackgroundColor = Color.FromArgb(i == 2 ? "#eff6ff" : "#0c1e2e");
        UrgentCard.BackgroundColor = Color.FromArgb(i == 2 ? "#fff1f2" : "#1a0808");
        PendingCard.BackgroundColor = Color.FromArgb(i == 2 ? "#f5f3ff" : "#100820");
        GreetingLabel.TextColor = Color.FromArgb(_textSub[i]);
        TotalOutstandingLabel.TextColor = Color.FromArgb(_textMain[i]);
    }

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
    async void OnDockProjectsTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//ProjectsPage");
    async void OnDockInvoicesTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//InvoicesPage");
    async void OnDockMoreTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//MorePage");
}
"""

write(DASH_XAML, DASH_TEXT)
write(DASH_CS,   DASH_CS_TEXT)
print("\\nDone! Build and deploy to Samsung.")