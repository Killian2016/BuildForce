import os

ROOT  = r"C:\Users\mezan\source\repos\BuildForce\BuildForce"
VIEWS = os.path.join(ROOT, "Views")

def write(path, text):
    with open(path, "wb") as f:
        f.write(text.encode("utf-8"))
    print("OK  " + path)

# ─────────────────────────────────────────────────────────────
# SHARED STYLE CONSTANTS
# ─────────────────────────────────────────────────────────────
# Dark:  #111111 bg, #1a1a1a card, #222222 pill
# Light: #f5f5f5 bg, #ffffff card, #e8e8e8 pill

# ─────────────────────────────────────────────────────────────
# ProjectsPage.xaml
# ─────────────────────────────────────────────────────────────
PROJ_XAML = os.path.join(VIEWS, "ProjectsPage.xaml")
PROJ_XAML_TEXT = """<?xml version="1.0" encoding="utf-8" ?>
<ContentPage
    x:Class="BuildForce.Views.ProjectsPage"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    BackgroundColor="#111111"
    Shell.NavBarIsVisible="False"
    Shell.TabBarIsVisible="False">

  <Grid RowDefinitions="*">
    <RefreshView Command="{Binding RefreshCommand}"
                 IsRefreshing="{Binding IsBusy}"
                 RefreshColor="#f0a500">
      <ScrollView>
        <VerticalStackLayout Spacing="0">

          <!-- TOP BAR -->
          <Grid ColumnDefinitions="Auto,*,Auto,Auto"
                Padding="20,56,20,16" BackgroundColor="#111111">
            <Border Grid.Column="0" BackgroundColor="#222222"
                    WidthRequest="38" HeightRequest="38">
              <Border.StrokeShape><RoundRectangle CornerRadius="12"/></Border.StrokeShape>
              <Border.GestureRecognizers>
                <TapGestureRecognizer Tapped="OnBackTapped"/>
              </Border.GestureRecognizers>
              <Label Text="&lt;" FontSize="18" FontAttributes="Bold"
                     TextColor="#ffffff" HorizontalOptions="Center"
                     VerticalOptions="Center"/>
            </Border>
            <Label Grid.Column="1"/>
            <Border Grid.Column="2" BackgroundColor="#f0a500"
                    WidthRequest="38" HeightRequest="38"
                    Margin="0,0,10,0">
              <Border.StrokeShape><RoundRectangle CornerRadius="12"/></Border.StrokeShape>
              <Border.GestureRecognizers>
                <TapGestureRecognizer Tapped="OnAddProjectTapped"/>
              </Border.GestureRecognizers>
              <Label Text="+" FontSize="22" FontAttributes="Bold"
                     TextColor="#000000" HorizontalOptions="Center"
                     VerticalOptions="Center"/>
            </Border>
            <Border Grid.Column="3" BackgroundColor="#222222"
                    WidthRequest="38" HeightRequest="38">
              <Border.StrokeShape><RoundRectangle CornerRadius="12"/></Border.StrokeShape>
              <Border.GestureRecognizers>
                <TapGestureRecognizer Tapped="OnMoreTapped"/>
              </Border.GestureRecognizers>
              <Label Text="..." FontSize="16" TextColor="#ffffff"
                     HorizontalOptions="Center" VerticalOptions="Center"/>
            </Border>
          </Grid>

          <!-- BIG TITLE -->
          <Label Text="Projects" Margin="20,0,20,20"
                 FontSize="34" FontAttributes="Bold"
                 TextColor="#ffffff"/>

          <!-- SEARCH -->
          <Border x:Name="SearchBorder"
                  BackgroundColor="#1a1a1a" Margin="20,0,20,16"
                  Padding="16,12">
            <Border.StrokeShape><RoundRectangle CornerRadius="14"/></Border.StrokeShape>
            <Entry Placeholder="Search projects..."
                   Text="{Binding Search}"
                   TextColor="#ffffff" PlaceholderColor="#555"
                   BackgroundColor="Transparent"/>
          </Border>

          <!-- FILTER TABS -->
          <ScrollView Orientation="Horizontal"
                      HorizontalScrollBarVisibility="Never"
                      Margin="0,0,0,20">
            <HorizontalStackLayout Padding="20,0" Spacing="8">
              <Border BackgroundColor="#ffffff" Padding="18,10">
                <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
                <Border.GestureRecognizers>
                  <TapGestureRecognizer Command="{Binding FilterCommand}" CommandParameter="All"/>
                </Border.GestureRecognizers>
                <Label Text="All" FontSize="14" FontAttributes="Bold" TextColor="#111111"/>
              </Border>
              <Border BackgroundColor="#222222" Padding="18,10">
                <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
                <Border.GestureRecognizers>
                  <TapGestureRecognizer Command="{Binding FilterCommand}" CommandParameter="Active"/>
                </Border.GestureRecognizers>
                <Label Text="Active" FontSize="14" TextColor="#10b981"/>
              </Border>
              <Border BackgroundColor="#222222" Padding="18,10">
                <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
                <Border.GestureRecognizers>
                  <TapGestureRecognizer Command="{Binding FilterCommand}" CommandParameter="Planning"/>
                </Border.GestureRecognizers>
                <Label Text="Planning" FontSize="14" TextColor="#0ea5e9"/>
              </Border>
              <Border BackgroundColor="#222222" Padding="18,10">
                <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
                <Border.GestureRecognizers>
                  <TapGestureRecognizer Command="{Binding FilterCommand}" CommandParameter="On Hold"/>
                </Border.GestureRecognizers>
                <Label Text="On Hold" FontSize="14" TextColor="#f0a500"/>
              </Border>
              <Border BackgroundColor="#222222" Padding="18,10">
                <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
                <Border.GestureRecognizers>
                  <TapGestureRecognizer Command="{Binding FilterCommand}" CommandParameter="Completed"/>
                </Border.GestureRecognizers>
                <Label Text="Completed" FontSize="14" TextColor="#8b5cf6"/>
              </Border>
            </HorizontalStackLayout>
          </ScrollView>

          <!-- PROJECTS LIST -->
          <CollectionView ItemsSource="{Binding Projects}"
                          SelectionMode="None" Margin="20,0">
            <CollectionView.ItemTemplate>
              <DataTemplate>
                <Border BackgroundColor="#1a1a1a"
                        Padding="20,18" Margin="0,0,0,12">
                  <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
                  <Border.GestureRecognizers>
                    <TapGestureRecognizer
                        Command="{Binding Source={RelativeSource AncestorType={x:Type ContentPage}}, Path=BindingContext.SelectCommand}"
                        CommandParameter="{Binding .}"/>
                  </Border.GestureRecognizers>
                  <VerticalStackLayout Spacing="6">
                    <Grid ColumnDefinitions="*,Auto">
                      <Label Grid.Column="0" Text="{Binding Name}"
                             FontSize="17" FontAttributes="Bold"
                             TextColor="#ffffff"/>
                      <Label Grid.Column="1" Text="{Binding Status}"
                             FontSize="12" FontAttributes="Bold"
                             TextColor="#f0a500"/>
                    </Grid>
                    <Grid ColumnDefinitions="*,Auto">
                      <Label Grid.Column="0" Text="{Binding CustomerName}"
                             FontSize="13" TextColor="#777"/>
                      <Label Grid.Column="1"
                             Text="{Binding Budget, StringFormat='${0:N0}'}"
                             FontSize="15" FontAttributes="Bold"
                             TextColor="#ffffff"/>
                    </Grid>
                    <Grid ColumnDefinitions="*,Auto">
                      <Label Grid.Column="0" Text="{Binding Location}"
                             FontSize="12" TextColor="#555"/>
                      <Label Grid.Column="1"
                             Text="{Binding StartDate, StringFormat='{0:MMM d}'}"
                             FontSize="12" TextColor="#555"/>
                    </Grid>
                  </VerticalStackLayout>
                </Border>
              </DataTemplate>
            </CollectionView.ItemTemplate>
            <CollectionView.EmptyView>
              <VerticalStackLayout HorizontalOptions="Center"
                                   Margin="0,40" Spacing="12">
                <Label Text="📁" FontSize="48" HorizontalOptions="Center"/>
                <Label Text="No projects found" TextColor="#555"
                       FontSize="15" HorizontalOptions="Center"/>
              </VerticalStackLayout>
            </CollectionView.EmptyView>
          </CollectionView>

          <BoxView HeightRequest="100" Color="Transparent"/>
        </VerticalStackLayout>
      </ScrollView>
    </RefreshView>

    <!-- BOTTOM PILL DOCK -->
    <Border VerticalOptions="End" HorizontalOptions="Center"
            Margin="0,0,0,32" BackgroundColor="#222222" Padding="8,8">
      <Border.StrokeShape><RoundRectangle CornerRadius="36"/></Border.StrokeShape>
      <HorizontalStackLayout Spacing="0">
        <VerticalStackLayout WidthRequest="60" HorizontalOptions="Center"
                             VerticalOptions="Center">
          <VerticalStackLayout.GestureRecognizers>
            <TapGestureRecognizer Tapped="OnBackTapped"/>
          </VerticalStackLayout.GestureRecognizers>
          <Label Text="⌂" FontSize="22" TextColor="#777"
                 HorizontalOptions="Center"/>
        </VerticalStackLayout>
        <Border BackgroundColor="#333" Padding="20,12">
          <Border.StrokeShape><RoundRectangle CornerRadius="28"/></Border.StrokeShape>
          <HorizontalStackLayout Spacing="8">
            <Label Text="📁" FontSize="20" VerticalOptions="Center"/>
            <Label Text="Projects" FontSize="15" FontAttributes="Bold"
                   TextColor="#ffffff" VerticalOptions="Center"/>
          </HorizontalStackLayout>
        </Border>
        <VerticalStackLayout WidthRequest="60" HorizontalOptions="Center"
                             VerticalOptions="Center">
          <VerticalStackLayout.GestureRecognizers>
            <TapGestureRecognizer Tapped="OnInvoicesTapped"/>
          </VerticalStackLayout.GestureRecognizers>
          <Label Text="🚩" FontSize="22" TextColor="#777"
                 HorizontalOptions="Center"/>
        </VerticalStackLayout>
        <VerticalStackLayout WidthRequest="60" HorizontalOptions="Center"
                             VerticalOptions="Center">
          <VerticalStackLayout.GestureRecognizers>
            <TapGestureRecognizer Tapped="OnMoreTapped"/>
          </VerticalStackLayout.GestureRecognizers>
          <Label Text="⋯" FontSize="22" TextColor="#777"
                 HorizontalOptions="Center"/>
        </VerticalStackLayout>
      </HorizontalStackLayout>
    </Border>

  </Grid>
</ContentPage>
"""

# ─────────────────────────────────────────────────────────────
# ProjectsPage.xaml.cs
# ─────────────────────────────────────────────────────────────
PROJ_CS = os.path.join(VIEWS, "ProjectsPage.xaml.cs")
PROJ_CS_TEXT = """#pragma warning disable CA1416
using BuildForce.ViewModels;

namespace BuildForce.Views;

public partial class ProjectsPage : ContentPage
{
    readonly ProjectsViewModel _vm;

    public ProjectsPage(ProjectsViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }

    async void OnBackTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//DashboardPage");

    async void OnAddProjectTapped(object sender, EventArgs e)
        => await DisplayAlert("New Project", "Coming soon.", "OK");

    async void OnInvoicesTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//InvoicesPage");

    async void OnMoreTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//MorePage");
}
"""

# ─────────────────────────────────────────────────────────────
# DashboardPage.xaml — fix data binding + consistent style
# ─────────────────────────────────────────────────────────────
DASH_XAML = os.path.join(VIEWS, "DashboardPage.xaml")
DASH_TEXT = """<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="BuildForce.Views.DashboardPage"
             x:Name="PageRoot"
             BackgroundColor="#111111"
             Shell.NavBarIsVisible="False"
             Shell.TabBarIsVisible="False">

  <Grid RowDefinitions="*">
    <ScrollView>
      <VerticalStackLayout Spacing="0">

        <!-- TOP BAR -->
        <Grid x:Name="HeaderGrid" ColumnDefinitions="*,Auto,Auto"
              Padding="20,56,20,16" BackgroundColor="#111111">
          <Label Grid.Column="0"/>
          <Border Grid.Column="1" x:Name="ThemePill"
                  BackgroundColor="#222222" WidthRequest="38"
                  HeightRequest="38" Margin="0,0,10,0">
            <Border.StrokeShape><RoundRectangle CornerRadius="12"/></Border.StrokeShape>
            <Border.GestureRecognizers>
              <TapGestureRecognizer Tapped="OnThemeToggleTapped"/>
            </Border.GestureRecognizers>
            <Label x:Name="ThemeLabel" Text="D" FontSize="13"
                   FontAttributes="Bold" TextColor="#f0a500"
                   HorizontalOptions="Center" VerticalOptions="Center"/>
          </Border>
          <Border Grid.Column="2" BackgroundColor="#f0a500"
                  WidthRequest="38" HeightRequest="38">
            <Border.StrokeShape><RoundRectangle CornerRadius="12"/></Border.StrokeShape>
            <Label Text="MC" FontSize="12" FontAttributes="Bold"
                   TextColor="#000000" HorizontalOptions="Center"
                   VerticalOptions="Center"/>
          </Border>
        </Grid>

        <!-- HERO TITLE -->
        <VerticalStackLayout x:Name="TitleStack"
                             Padding="20,0,20,8" Spacing="6">
          <Label x:Name="GreetingLabel" Text="Good evening"
                 FontSize="15" TextColor="#777"/>
          <Label Text="Mezano Consulting" FontSize="34"
                 FontAttributes="Bold" TextColor="#ffffff"
                 LineBreakMode="WordWrap"/>
          <Border x:Name="PlanBadge" BackgroundColor="#1a1a1a"
                  Padding="12,6" HorizontalOptions="Start"
                  Margin="0,4,0,0">
            <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
            <Label Text="PRO BUILDER · Mezano Consulting LLC"
                   FontSize="12" TextColor="#f0a500"/>
          </Border>
        </VerticalStackLayout>

        <!-- TABS -->
        <ScrollView Orientation="Horizontal"
                    HorizontalScrollBarVisibility="Never"
                    Margin="0,16,0,24">
          <HorizontalStackLayout Padding="20,0" Spacing="8">
            <Border BackgroundColor="#ffffff" Padding="18,10">
              <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
              <Label Text="Overview" FontSize="14"
                     FontAttributes="Bold" TextColor="#111111"/>
            </Border>
            <Border BackgroundColor="#222222" Padding="18,10">
              <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
              <Border.GestureRecognizers>
                <TapGestureRecognizer Tapped="OnDockProjectsTapped"/>
              </Border.GestureRecognizers>
              <Label Text="Projects" FontSize="14" TextColor="#777"/>
            </Border>
            <Border BackgroundColor="#222222" Padding="18,10">
              <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
              <Border.GestureRecognizers>
                <TapGestureRecognizer Tapped="OnDockInvoicesTapped"/>
              </Border.GestureRecognizers>
              <Label Text="Invoices" FontSize="14" TextColor="#777"/>
            </Border>
            <Border BackgroundColor="#222222" Padding="18,10">
              <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
              <Border.GestureRecognizers>
                <TapGestureRecognizer Tapped="OnDockMoreTapped"/>
              </Border.GestureRecognizers>
              <Label Text="More" FontSize="14" TextColor="#777"/>
            </Border>
          </HorizontalStackLayout>
        </ScrollView>

        <!-- FINANCIALS CARD -->
        <Border x:Name="HeroCard" Margin="20,0,20,16"
                BackgroundColor="#1a1a1a" Padding="22,20">
          <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
          <VerticalStackLayout Spacing="16">
            <Label Text="FINANCIALS" FontSize="11" FontAttributes="Bold"
                   TextColor="#555" CharacterSpacing="2"/>
            <Grid ColumnDefinitions="*,Auto">
              <VerticalStackLayout Grid.Column="0" Spacing="4">
                <Label Text="Outstanding" FontSize="13" TextColor="#777"/>
                <Label x:Name="UnpaidAmountLabel"
                       Text="{Binding OutstandingBalance, StringFormat='{0:C2}'}"
                       FontSize="38" FontAttributes="Bold"
                       TextColor="#ffffff"/>
              </VerticalStackLayout>
              <Border Grid.Column="1" BackgroundColor="#2a2a2a"
                      Padding="12,8" VerticalOptions="Center">
                <Border.StrokeShape><RoundRectangle CornerRadius="10"/></Border.StrokeShape>
                <Label x:Name="OverdueCountLabel"
                       Text="{Binding OverdueInvoices, StringFormat='{0} Overdue'}"
                       FontSize="12" TextColor="#f0a500" FontAttributes="Bold"/>
              </Border>
            </Grid>
            <BoxView x:Name="Divider1" HeightRequest="1" BackgroundColor="#2a2a2a"/>
            <Grid ColumnDefinitions="*,*" ColumnSpacing="20">
              <VerticalStackLayout Grid.Column="0" Spacing="4">
                <Label Text="REVENUE" FontSize="10" FontAttributes="Bold"
                       TextColor="#555" CharacterSpacing="1.5"/>
                <Label x:Name="PaidAmountLabel"
                       Text="{Binding TotalRevenue, StringFormat='{0:C2}'}"
                       FontSize="22" FontAttributes="Bold"
                       TextColor="#22c55e"/>
              </VerticalStackLayout>
              <VerticalStackLayout Grid.Column="1" Spacing="4">
                <Label Text="ACTIVE JOBS" FontSize="10" FontAttributes="Bold"
                       TextColor="#555" CharacterSpacing="1.5"/>
                <Label x:Name="TotalOutstandingLabel"
                       Text="{Binding ActiveProjects}"
                       FontSize="22" FontAttributes="Bold"
                       TextColor="#ffffff"/>
              </VerticalStackLayout>
            </Grid>
          </VerticalStackLayout>
        </Border>

        <!-- STATS ROW -->
        <Grid x:Name="StatsGrid" Margin="20,0,20,20"
              ColumnDefinitions="*,*,*" ColumnSpacing="10">
          <Border Grid.Column="0" x:Name="ActiveCard"
                  BackgroundColor="#1a1a1a" Padding="0,18">
            <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
            <VerticalStackLayout Spacing="4" HorizontalOptions="Center">
              <Label x:Name="ActiveJobsLabel"
                     Text="{Binding ActiveProjects}"
                     FontSize="30" FontAttributes="Bold"
                     TextColor="#0ea5e9" HorizontalOptions="Center"/>
              <Label Text="Active" FontSize="12" TextColor="#555"
                     HorizontalOptions="Center"/>
            </VerticalStackLayout>
          </Border>
          <Border Grid.Column="1" x:Name="UrgentCard"
                  BackgroundColor="#1a1a1a" Padding="0,18">
            <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
            <VerticalStackLayout Spacing="4" HorizontalOptions="Center">
              <Label x:Name="NeedsAttentionLabel"
                     Text="{Binding OverdueInvoices}"
                     FontSize="30" FontAttributes="Bold"
                     TextColor="#ef4444" HorizontalOptions="Center"/>
              <Label Text="Overdue" FontSize="12" TextColor="#555"
                     HorizontalOptions="Center"/>
            </VerticalStackLayout>
          </Border>
          <Border Grid.Column="2" x:Name="PendingCard"
                  BackgroundColor="#1a1a1a" Padding="0,18">
            <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
            <VerticalStackLayout Spacing="4" HorizontalOptions="Center">
              <Label x:Name="EstimatesLabel"
                     Text="{Binding PendingInvoices}"
                     FontSize="30" FontAttributes="Bold"
                     TextColor="#8b5cf6" HorizontalOptions="Center"/>
              <Label Text="Pending" FontSize="12" TextColor="#555"
                     HorizontalOptions="Center"/>
            </VerticalStackLayout>
          </Border>
        </Grid>

        <!-- QUICK ACTIONS -->
        <Label x:Name="QuickActionsLabel" Text="Quick actions"
               Margin="20,0,20,12" FontSize="17"
               FontAttributes="Bold" TextColor="#ffffff"/>

        <Grid x:Name="ActionsGrid" Margin="20,0,20,20"
              ColumnDefinitions="*,*" ColumnSpacing="12"
              RowDefinitions="Auto,Auto" RowSpacing="12">
          <Border Grid.Row="0" Grid.Column="0" x:Name="ClockCard"
                  BackgroundColor="#1a1a1a" Padding="18,16">
            <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
            <Border.GestureRecognizers>
              <TapGestureRecognizer Tapped="OnTimeClockTapped"/>
            </Border.GestureRecognizers>
            <VerticalStackLayout Spacing="10">
              <Label Text="⏱" FontSize="28"/>
              <Label Text="Time Clock" FontSize="15"
                     FontAttributes="Bold" TextColor="#ffffff"/>
              <Label Text="GPS verified" FontSize="12" TextColor="#555"/>
            </VerticalStackLayout>
          </Border>
          <Border Grid.Row="0" Grid.Column="1" x:Name="InvoiceCard"
                  BackgroundColor="#1a1a1a" Padding="18,16">
            <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
            <Border.GestureRecognizers>
              <TapGestureRecognizer Tapped="OnNewInvoiceTapped"/>
            </Border.GestureRecognizers>
            <VerticalStackLayout Spacing="10">
              <Label Text="🧾" FontSize="28"/>
              <Label Text="New Invoice" FontSize="15"
                     FontAttributes="Bold" TextColor="#ffffff"/>
              <Label Text="60 seconds" FontSize="12" TextColor="#555"/>
            </VerticalStackLayout>
          </Border>
          <Border Grid.Row="1" Grid.Column="0" x:Name="ScanCard"
                  BackgroundColor="#1a1a1a" Padding="18,16">
            <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
            <Border.GestureRecognizers>
              <TapGestureRecognizer Tapped="OnScanReceiptTapped"/>
            </Border.GestureRecognizers>
            <VerticalStackLayout Spacing="10">
              <Label Text="📷" FontSize="28"/>
              <Label Text="Scan Receipt" FontSize="15"
                     FontAttributes="Bold" TextColor="#ffffff"/>
              <Label Text="AI auto-fill" FontSize="12" TextColor="#555"/>
            </VerticalStackLayout>
          </Border>
          <Border Grid.Row="1" Grid.Column="1" x:Name="CollectCard"
                  BackgroundColor="#1a1a1a" Padding="18,16">
            <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
            <Border.GestureRecognizers>
              <TapGestureRecognizer Tapped="OnCollectPayTapped"/>
            </Border.GestureRecognizers>
            <VerticalStackLayout Spacing="10">
              <Label Text="💳" FontSize="28"/>
              <Label Text="Collect Pay" FontSize="15"
                     FontAttributes="Bold" TextColor="#ffffff"/>
              <Label Text="On-site" FontSize="12" TextColor="#555"/>
            </VerticalStackLayout>
          </Border>
        </Grid>

        <!-- ACTIVE JOBS -->
        <Grid ColumnDefinitions="*,Auto" Margin="20,0,20,12">
          <Label Grid.Column="0" Text="Active jobs" FontSize="17"
                 FontAttributes="Bold" TextColor="#ffffff"/>
          <Label Grid.Column="1" Text="See all →" FontSize="14"
                 TextColor="#777">
            <Label.GestureRecognizers>
              <TapGestureRecognizer Tapped="OnSeeAllProjectsTapped"/>
            </Label.GestureRecognizers>
          </Label>
        </Grid>

        <CollectionView x:Name="ActiveJobsCollection"
                        Margin="20,0,20,0" SelectionMode="None">
          <CollectionView.ItemTemplate>
            <DataTemplate>
              <Border BackgroundColor="#1a1a1a"
                      Padding="20,18" Margin="0,0,0,10">
                <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
                <Grid ColumnDefinitions="*,Auto">
                  <VerticalStackLayout Grid.Column="0" Spacing="4">
                    <Label Text="{Binding Name}" FontSize="16"
                           FontAttributes="Bold" TextColor="#ffffff"/>
                    <Label Text="{Binding CustomerName}"
                           FontSize="13" TextColor="#777"/>
                  </VerticalStackLayout>
                  <VerticalStackLayout Grid.Column="1"
                                       HorizontalOptions="End" Spacing="6">
                    <Label Text="{Binding BudgetFormatted}"
                           FontSize="15" FontAttributes="Bold"
                           TextColor="#ffffff" HorizontalOptions="End"/>
                    <Border BackgroundColor="#2a2a2a" Padding="8,4">
                      <Border.StrokeShape><RoundRectangle CornerRadius="8"/></Border.StrokeShape>
                      <Label Text="{Binding Status}" FontSize="11"
                             TextColor="#777" FontAttributes="Bold"/>
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

    <!-- BOTTOM PILL DOCK -->
    <Border x:Name="DockBar" VerticalOptions="End"
            HorizontalOptions="Center" Margin="0,0,0,32"
            BackgroundColor="#222222" Padding="8,8">
      <Border.StrokeShape><RoundRectangle CornerRadius="36"/></Border.StrokeShape>
      <HorizontalStackLayout Spacing="0">
        <Border BackgroundColor="#333" Padding="16,12" Margin="0,0,4,0">
          <Border.StrokeShape><RoundRectangle CornerRadius="28"/></Border.StrokeShape>
          <HorizontalStackLayout Spacing="6">
            <Label Text="⌂" FontSize="20" TextColor="#ffffff"
                   VerticalOptions="Center"/>
            <Label Text="Home" FontSize="14" FontAttributes="Bold"
                   TextColor="#ffffff" VerticalOptions="Center"/>
          </HorizontalStackLayout>
        </Border>
        <VerticalStackLayout WidthRequest="60" HorizontalOptions="Center"
                             VerticalOptions="Center">
          <VerticalStackLayout.GestureRecognizers>
            <TapGestureRecognizer Tapped="OnDockProjectsTapped"/>
          </VerticalStackLayout.GestureRecognizers>
          <Label Text="📁" FontSize="22" TextColor="#777"
                 HorizontalOptions="Center"/>
        </VerticalStackLayout>
        <VerticalStackLayout WidthRequest="60" HorizontalOptions="Center"
                             VerticalOptions="Center">
          <VerticalStackLayout.GestureRecognizers>
            <TapGestureRecognizer Tapped="OnDockInvoicesTapped"/>
          </VerticalStackLayout.GestureRecognizers>
          <Label Text="🚩" FontSize="22" TextColor="#777"
                 HorizontalOptions="Center"/>
        </VerticalStackLayout>
        <VerticalStackLayout WidthRequest="60" HorizontalOptions="Center"
                             VerticalOptions="Center">
          <VerticalStackLayout.GestureRecognizers>
            <TapGestureRecognizer Tapped="OnDockMoreTapped"/>
          </VerticalStackLayout.GestureRecognizers>
          <Label Text="⋯" FontSize="22" TextColor="#777"
                 HorizontalOptions="Center"/>
        </VerticalStackLayout>
      </HorizontalStackLayout>
    </Border>

  </Grid>
</ContentPage>
"""

# ─────────────────────────────────────────────────────────────
# DashboardPage.xaml.cs — full theme switch on ALL cards
# ─────────────────────────────────────────────────────────────
DASH_CS = os.path.join(VIEWS, "DashboardPage.xaml.cs")
DASH_CS_TEXT = """#pragma warning disable CA1416
using BuildForce.ViewModels;

namespace BuildForce.Views;

public partial class DashboardPage : ContentPage
{
    readonly DashboardViewModel _vm;
    bool _isLight = false;

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
        GreetingLabel.Text = h < 12 ? "Good morning"
                           : h < 17 ? "Good afternoon"
                           : "Good evening";
    }

    void OnThemeToggleTapped(object sender, EventArgs e)
    {
        _isLight = !_isLight;
        ApplyTheme();
    }

    void ApplyTheme()
    {
        if (_isLight)
        {
            ThemeLabel.Text = "L";
            var pageBg   = Color.FromArgb("#f5f5f5");
            var cardBg   = Color.FromArgb("#ffffff");
            var dockBg   = Color.FromArgb("#e8e8e8");
            var textMain = Color.FromArgb("#0f172a");
            var textSub  = Color.FromArgb("#64748b");
            var divider  = Color.FromArgb("#e2e8f0");

            BackgroundColor = pageBg;
            HeaderGrid.BackgroundColor = pageBg;
            HeroCard.BackgroundColor = cardBg;
            ActiveCard.BackgroundColor = cardBg;
            UrgentCard.BackgroundColor = cardBg;
            PendingCard.BackgroundColor = cardBg;
            ClockCard.BackgroundColor = cardBg;
            InvoiceCard.BackgroundColor = cardBg;
            ScanCard.BackgroundColor = cardBg;
            CollectCard.BackgroundColor = cardBg;
            DockBar.BackgroundColor = dockBg;
            PlanBadge.BackgroundColor = Color.FromArgb("#f0f0f0");
            Divider1.BackgroundColor = divider;
            UnpaidAmountLabel.TextColor = textMain;
            TotalOutstandingLabel.TextColor = textMain;
            GreetingLabel.TextColor = textSub;
            QuickActionsLabel.TextColor = textMain;
        }
        else
        {
            ThemeLabel.Text = "D";
            var pageBg   = Color.FromArgb("#111111");
            var cardBg   = Color.FromArgb("#1a1a1a");
            var dockBg   = Color.FromArgb("#222222");
            var textMain = Color.FromArgb("#ffffff");
            var textSub  = Color.FromArgb("#777777");
            var divider  = Color.FromArgb("#2a2a2a");

            BackgroundColor = pageBg;
            HeaderGrid.BackgroundColor = pageBg;
            HeroCard.BackgroundColor = cardBg;
            ActiveCard.BackgroundColor = cardBg;
            UrgentCard.BackgroundColor = cardBg;
            PendingCard.BackgroundColor = cardBg;
            ClockCard.BackgroundColor = cardBg;
            InvoiceCard.BackgroundColor = cardBg;
            ScanCard.BackgroundColor = cardBg;
            CollectCard.BackgroundColor = cardBg;
            DockBar.BackgroundColor = dockBg;
            PlanBadge.BackgroundColor = Color.FromArgb("#1a1a1a");
            Divider1.BackgroundColor = divider;
            UnpaidAmountLabel.TextColor = textMain;
            TotalOutstandingLabel.TextColor = textMain;
            GreetingLabel.TextColor = textSub;
            QuickActionsLabel.TextColor = textMain;
        }
    }

    async void OnNewInvoiceTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("CreateInvoicePage");
    async void OnScanReceiptTapped(object sender, EventArgs e)
        => await DisplayAlert("Scan Receipt", "Coming soon!", "OK");
    async void OnTimeClockTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//TimesheetPage");
    async void OnCollectPayTapped(object sender, EventArgs e)
        => await DisplayAlert("Collect Payment", "Select an invoice.", "OK");
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

write(PROJ_XAML, PROJ_XAML_TEXT)
write(PROJ_CS,   PROJ_CS_TEXT)
write(DASH_XAML, DASH_TEXT)
write(DASH_CS,   DASH_CS_TEXT)
print("\\nAll 4 files written. Build and deploy to Samsung!")