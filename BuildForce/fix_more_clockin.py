import os

ROOT  = r"C:\Users\mezan\source\repos\BuildForce\BuildForce"
VIEWS = os.path.join(ROOT, "Views")

def write(path, text):
    with open(path, "wb") as f:
        f.write(text.encode("utf-8"))
    print("OK  " + path)

# ─────────────────────────────────────────────────────────────
# MorePage.xaml — Procore style
# ─────────────────────────────────────────────────────────────
MORE_XAML = os.path.join(VIEWS, "MorePage.xaml")
MORE_TEXT = """<?xml version="1.0" encoding="utf-8" ?>
<ContentPage x:Class="BuildForce.Views.MorePage"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    BackgroundColor="#111111"
    Shell.NavBarIsVisible="False"
    Shell.TabBarIsVisible="False">

  <Grid RowDefinitions="*">
    <ScrollView>
      <VerticalStackLayout Spacing="0">

        <!-- TOP BAR -->
        <Grid ColumnDefinitions="Auto,*,Auto"
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
                  WidthRequest="38" HeightRequest="38">
            <Border.StrokeShape><RoundRectangle CornerRadius="12"/></Border.StrokeShape>
            <Label Text="{Binding UserInitials}" FontSize="13"
                   FontAttributes="Bold" TextColor="#000000"
                   HorizontalOptions="Center" VerticalOptions="Center"/>
          </Border>
        </Grid>

        <!-- PROFILE -->
        <VerticalStackLayout Padding="20,0,20,24" Spacing="6">
          <Label Text="{Binding UserName}" FontSize="34"
                 FontAttributes="Bold" TextColor="#ffffff"
                 LineBreakMode="WordWrap"/>
          <Label Text="{Binding CompanyName}" FontSize="15"
                 TextColor="#777"/>
          <Border BackgroundColor="#1a1a1a" Padding="12,6"
                  HorizontalOptions="Start" Margin="0,4,0,0">
            <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
            <Label Text="{Binding PlanName}" FontSize="12"
                   TextColor="#f0a500" FontAttributes="Bold"/>
          </Border>
        </VerticalStackLayout>

        <!-- STATS ROW -->
        <Grid Margin="20,0,20,28" ColumnDefinitions="*,*,*"
              ColumnSpacing="10">
          <Border Grid.Column="0" BackgroundColor="#1a1a1a"
                  Padding="0,18">
            <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
            <VerticalStackLayout Spacing="4" HorizontalOptions="Center">
              <Label Text="{Binding ActiveProjects}"
                     FontSize="30" FontAttributes="Bold"
                     TextColor="#f0a500" HorizontalOptions="Center"/>
              <Label Text="Projects" FontSize="12" TextColor="#555"
                     HorizontalOptions="Center"/>
            </VerticalStackLayout>
          </Border>
          <Border Grid.Column="1" BackgroundColor="#1a1a1a"
                  Padding="0,18">
            <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
            <VerticalStackLayout Spacing="4" HorizontalOptions="Center">
              <Label Text="{Binding PendingInvoices}"
                     FontSize="30" FontAttributes="Bold"
                     TextColor="#22c55e" HorizontalOptions="Center"/>
              <Label Text="Invoices" FontSize="12" TextColor="#555"
                     HorizontalOptions="Center"/>
            </VerticalStackLayout>
          </Border>
          <Border Grid.Column="2" BackgroundColor="#1a1a1a"
                  Padding="0,18">
            <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
            <VerticalStackLayout Spacing="4" HorizontalOptions="Center">
              <Label Text="{Binding TotalEmployees}"
                     FontSize="30" FontAttributes="Bold"
                     TextColor="#0ea5e9" HorizontalOptions="Center"/>
              <Label Text="Crew" FontSize="12" TextColor="#555"
                     HorizontalOptions="Center"/>
            </VerticalStackLayout>
          </Border>
        </Grid>

        <!-- FIELD TOOLS -->
        <Label Text="Field tools" Margin="20,0,20,12"
               FontSize="17" FontAttributes="Bold"
               TextColor="#ffffff"/>

        <Border BackgroundColor="#1a1a1a" Margin="20,0,20,24"
                Padding="0">
          <Border.StrokeShape><RoundRectangle CornerRadius="18"/></Border.StrokeShape>
          <VerticalStackLayout Spacing="0">

            <!-- Group Clock In — NEW -->
            <Grid Padding="20,18" ColumnDefinitions="Auto,*,Auto">
              <Grid.GestureRecognizers>
                <TapGestureRecognizer Tapped="OnGroupClockInTapped"/>
              </Grid.GestureRecognizers>
              <Border Grid.Column="0" BackgroundColor="#0d2318"
                      WidthRequest="40" HeightRequest="40">
                <Border.StrokeShape><RoundRectangle CornerRadius="12"/></Border.StrokeShape>
                <Label Text="⏱" FontSize="18"
                       HorizontalOptions="Center"
                       VerticalOptions="Center"/>
              </Border>
              <VerticalStackLayout Grid.Column="1" Margin="14,0,0,0"
                                   VerticalOptions="Center" Spacing="2">
                <Label Text="Group Clock In" FontSize="15"
                       FontAttributes="Bold" TextColor="#ffffff"/>
                <Label Text="Clock in your entire crew" FontSize="12"
                       TextColor="#555"/>
              </VerticalStackLayout>
              <Label Grid.Column="2" Text="›" FontSize="22"
                     TextColor="#555" VerticalOptions="Center"/>
            </Grid>

            <BoxView HeightRequest="1" BackgroundColor="#2a2a2a"
                     Margin="20,0"/>

            <Grid Padding="20,18" ColumnDefinitions="Auto,*,Auto">
              <Grid.GestureRecognizers>
                <TapGestureRecognizer Tapped="OnEmployeesTapped"/>
              </Grid.GestureRecognizers>
              <Border Grid.Column="0" BackgroundColor="#0c1e2e"
                      WidthRequest="40" HeightRequest="40">
                <Border.StrokeShape><RoundRectangle CornerRadius="12"/></Border.StrokeShape>
                <Label Text="👷" FontSize="18"
                       HorizontalOptions="Center"
                       VerticalOptions="Center"/>
              </Border>
              <VerticalStackLayout Grid.Column="1" Margin="14,0,0,0"
                                   VerticalOptions="Center" Spacing="2">
                <Label Text="Employees" FontSize="15"
                       FontAttributes="Bold" TextColor="#ffffff"/>
                <Label Text="Manage your crew" FontSize="12"
                       TextColor="#555"/>
              </VerticalStackLayout>
              <Label Grid.Column="2" Text="›" FontSize="22"
                     TextColor="#555" VerticalOptions="Center"/>
            </Grid>

            <BoxView HeightRequest="1" BackgroundColor="#2a2a2a"
                     Margin="20,0"/>

            <Grid Padding="20,18" ColumnDefinitions="Auto,*,Auto">
              <Grid.GestureRecognizers>
                <TapGestureRecognizer Tapped="OnMapTapped"/>
              </Grid.GestureRecognizers>
              <Border Grid.Column="0" BackgroundColor="#0d2318"
                      WidthRequest="40" HeightRequest="40">
                <Border.StrokeShape><RoundRectangle CornerRadius="12"/></Border.StrokeShape>
                <Label Text="🗺" FontSize="18"
                       HorizontalOptions="Center"
                       VerticalOptions="Center"/>
              </Border>
              <VerticalStackLayout Grid.Column="1" Margin="14,0,0,0"
                                   VerticalOptions="Center" Spacing="2">
                <Label Text="Job Map" FontSize="15"
                       FontAttributes="Bold" TextColor="#ffffff"/>
                <Label Text="View all active job sites" FontSize="12"
                       TextColor="#555"/>
              </VerticalStackLayout>
              <Label Grid.Column="2" Text="›" FontSize="22"
                     TextColor="#555" VerticalOptions="Center"/>
            </Grid>

            <BoxView HeightRequest="1" BackgroundColor="#2a2a2a"
                     Margin="20,0"/>

            <Grid Padding="20,18" ColumnDefinitions="Auto,*,Auto">
              <Grid.GestureRecognizers>
                <TapGestureRecognizer Tapped="OnContractsTapped"/>
              </Grid.GestureRecognizers>
              <Border Grid.Column="0" BackgroundColor="#1e1333"
                      WidthRequest="40" HeightRequest="40">
                <Border.StrokeShape><RoundRectangle CornerRadius="12"/></Border.StrokeShape>
                <Label Text="📝" FontSize="18"
                       HorizontalOptions="Center"
                       VerticalOptions="Center"/>
              </Border>
              <VerticalStackLayout Grid.Column="1" Margin="14,0,0,0"
                                   VerticalOptions="Center" Spacing="2">
                <Label Text="Contracts" FontSize="15"
                       FontAttributes="Bold" TextColor="#ffffff"/>
                <Label Text="Send &amp; sign digitally" FontSize="12"
                       TextColor="#555"/>
              </VerticalStackLayout>
              <Label Grid.Column="2" Text="›" FontSize="22"
                     TextColor="#555" VerticalOptions="Center"/>
            </Grid>

          </VerticalStackLayout>
        </Border>

        <!-- FINANCE -->
        <Label Text="Finance" Margin="20,0,20,12"
               FontSize="17" FontAttributes="Bold"
               TextColor="#ffffff"/>

        <Border BackgroundColor="#1a1a1a" Margin="20,0,20,24"
                Padding="0">
          <Border.StrokeShape><RoundRectangle CornerRadius="18"/></Border.StrokeShape>
          <VerticalStackLayout Spacing="0">

            <Grid Padding="20,18" ColumnDefinitions="Auto,*,Auto">
              <Grid.GestureRecognizers>
                <TapGestureRecognizer Tapped="OnReportsTapped"/>
              </Grid.GestureRecognizers>
              <Border Grid.Column="0" BackgroundColor="#0d2318"
                      WidthRequest="40" HeightRequest="40">
                <Border.StrokeShape><RoundRectangle CornerRadius="12"/></Border.StrokeShape>
                <Label Text="📊" FontSize="18"
                       HorizontalOptions="Center"
                       VerticalOptions="Center"/>
              </Border>
              <VerticalStackLayout Grid.Column="1" Margin="14,0,0,0"
                                   VerticalOptions="Center" Spacing="2">
                <Label Text="Reports" FontSize="15"
                       FontAttributes="Bold" TextColor="#ffffff"/>
                <Label Text="Profit per job &amp; revenue" FontSize="12"
                       TextColor="#555"/>
              </VerticalStackLayout>
              <Label Grid.Column="2" Text="›" FontSize="22"
                     TextColor="#555" VerticalOptions="Center"/>
            </Grid>

            <BoxView HeightRequest="1" BackgroundColor="#2a2a2a"
                     Margin="20,0"/>

            <Grid Padding="20,18" ColumnDefinitions="Auto,*,Auto">
              <Grid.GestureRecognizers>
                <TapGestureRecognizer Tapped="OnEstimatesTapped"/>
              </Grid.GestureRecognizers>
              <Border Grid.Column="0" BackgroundColor="#2a1f00"
                      WidthRequest="40" HeightRequest="40">
                <Border.StrokeShape><RoundRectangle CornerRadius="12"/></Border.StrokeShape>
                <Label Text="📋" FontSize="18"
                       HorizontalOptions="Center"
                       VerticalOptions="Center"/>
              </Border>
              <VerticalStackLayout Grid.Column="1" Margin="14,0,0,0"
                                   VerticalOptions="Center" Spacing="2">
                <Label Text="Estimates" FontSize="15"
                       FontAttributes="Bold" TextColor="#ffffff"/>
                <Label Text="Create &amp; send estimates" FontSize="12"
                       TextColor="#555"/>
              </VerticalStackLayout>
              <Label Grid.Column="2" Text="›" FontSize="22"
                     TextColor="#555" VerticalOptions="Center"/>
            </Grid>

            <BoxView HeightRequest="1" BackgroundColor="#2a2a2a"
                     Margin="20,0"/>

            <Grid Padding="20,18" ColumnDefinitions="Auto,*,Auto">
              <Grid.GestureRecognizers>
                <TapGestureRecognizer Tapped="OnNotificationsTapped"/>
              </Grid.GestureRecognizers>
              <Border Grid.Column="0" BackgroundColor="#0c1e2e"
                      WidthRequest="40" HeightRequest="40">
                <Border.StrokeShape><RoundRectangle CornerRadius="12"/></Border.StrokeShape>
                <Label Text="🔔" FontSize="18"
                       HorizontalOptions="Center"
                       VerticalOptions="Center"/>
              </Border>
              <VerticalStackLayout Grid.Column="1" Margin="14,0,0,0"
                                   VerticalOptions="Center" Spacing="2">
                <Label Text="Notifications" FontSize="15"
                       FontAttributes="Bold" TextColor="#ffffff"/>
                <Label Text="Alerts &amp; updates" FontSize="12"
                       TextColor="#555"/>
              </VerticalStackLayout>
              <Label Grid.Column="2" Text="›" FontSize="22"
                     TextColor="#555" VerticalOptions="Center"/>
            </Grid>

          </VerticalStackLayout>
        </Border>

        <!-- ACCOUNT -->
        <Label Text="Account" Margin="20,0,20,12"
               FontSize="17" FontAttributes="Bold"
               TextColor="#ffffff"/>

        <Border BackgroundColor="#1a1a1a" Margin="20,0,20,24"
                Padding="0">
          <Border.StrokeShape><RoundRectangle CornerRadius="18"/></Border.StrokeShape>
          <VerticalStackLayout Spacing="0">

            <Grid Padding="20,18" ColumnDefinitions="Auto,*,Auto">
              <Grid.GestureRecognizers>
                <TapGestureRecognizer Tapped="OnSubscriptionTapped"/>
              </Grid.GestureRecognizers>
              <Border Grid.Column="0" BackgroundColor="#2a1f00"
                      WidthRequest="40" HeightRequest="40">
                <Border.StrokeShape><RoundRectangle CornerRadius="12"/></Border.StrokeShape>
                <Label Text="⭐" FontSize="18"
                       HorizontalOptions="Center"
                       VerticalOptions="Center"/>
              </Border>
              <VerticalStackLayout Grid.Column="1" Margin="14,0,0,0"
                                   VerticalOptions="Center" Spacing="2">
                <Label Text="Subscription" FontSize="15"
                       FontAttributes="Bold" TextColor="#ffffff"/>
                <Label Text="{Binding PlanName}" FontSize="12"
                       TextColor="#f0a500"/>
              </VerticalStackLayout>
              <Label Grid.Column="2" Text="›" FontSize="22"
                     TextColor="#555" VerticalOptions="Center"/>
            </Grid>

            <BoxView HeightRequest="1" BackgroundColor="#2a2a2a"
                     Margin="20,0"/>

            <Grid Padding="20,18" ColumnDefinitions="Auto,*,Auto">
              <Grid.GestureRecognizers>
                <TapGestureRecognizer Tapped="OnSettingsTapped"/>
              </Grid.GestureRecognizers>
              <Border Grid.Column="0" BackgroundColor="#222222"
                      WidthRequest="40" HeightRequest="40">
                <Border.StrokeShape><RoundRectangle CornerRadius="12"/></Border.StrokeShape>
                <Label Text="⚙" FontSize="18"
                       HorizontalOptions="Center"
                       VerticalOptions="Center"/>
              </Border>
              <VerticalStackLayout Grid.Column="1" Margin="14,0,0,0"
                                   VerticalOptions="Center" Spacing="2">
                <Label Text="Settings" FontSize="15"
                       FontAttributes="Bold" TextColor="#ffffff"/>
                <Label Text="App preferences" FontSize="12"
                       TextColor="#555"/>
              </VerticalStackLayout>
              <Label Grid.Column="2" Text="›" FontSize="22"
                     TextColor="#555" VerticalOptions="Center"/>
            </Grid>

            <BoxView HeightRequest="1" BackgroundColor="#2a2a2a"
                     Margin="20,0"/>

            <Grid Padding="20,18" ColumnDefinitions="Auto,*,Auto">
              <Grid.GestureRecognizers>
                <TapGestureRecognizer Tapped="OnHelpTapped"/>
              </Grid.GestureRecognizers>
              <Border Grid.Column="0" BackgroundColor="#0c1e2e"
                      WidthRequest="40" HeightRequest="40">
                <Border.StrokeShape><RoundRectangle CornerRadius="12"/></Border.StrokeShape>
                <Label Text="💬" FontSize="18"
                       HorizontalOptions="Center"
                       VerticalOptions="Center"/>
              </Border>
              <VerticalStackLayout Grid.Column="1" Margin="14,0,0,0"
                                   VerticalOptions="Center" Spacing="2">
                <Label Text="Help &amp; Support" FontSize="15"
                       FontAttributes="Bold" TextColor="#ffffff"/>
                <Label Text="Get help from our team" FontSize="12"
                       TextColor="#555"/>
              </VerticalStackLayout>
              <Label Grid.Column="2" Text="›" FontSize="22"
                     TextColor="#555" VerticalOptions="Center"/>
            </Grid>

          </VerticalStackLayout>
        </Border>

        <!-- SIGN OUT -->
        <Border BackgroundColor="#1a0808" Margin="20,0,20,16"
                Padding="20,18">
          <Border.StrokeShape><RoundRectangle CornerRadius="18"/></Border.StrokeShape>
          <Border.GestureRecognizers>
            <TapGestureRecognizer Tapped="OnSignOutTapped"/>
          </Border.GestureRecognizers>
          <Grid ColumnDefinitions="Auto,*">
            <Label Grid.Column="0" Text="🚪" FontSize="20"
                   VerticalOptions="Center"/>
            <Label Grid.Column="1" Text="Sign Out" FontSize="16"
                   FontAttributes="Bold" TextColor="#ef4444"
                   Margin="14,0,0,0" VerticalOptions="Center"/>
          </Grid>
        </Border>

        <Label Text="BuildForce v1.0 · Mezano Consulting"
               FontSize="11" TextColor="#444"
               HorizontalOptions="Center" Margin="0,0,0,40"/>

      </VerticalStackLayout>
    </ScrollView>

    <!-- BOTTOM PILL DOCK -->
    <Border VerticalOptions="End" HorizontalOptions="Center"
            Margin="0,0,0,32" BackgroundColor="#222222"
            Padding="8,8">
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
        <VerticalStackLayout WidthRequest="60" HorizontalOptions="Center"
                             VerticalOptions="Center">
          <VerticalStackLayout.GestureRecognizers>
            <TapGestureRecognizer Tapped="OnProjectsTapped"/>
          </VerticalStackLayout.GestureRecognizers>
          <Label Text="📁" FontSize="22" TextColor="#777"
                 HorizontalOptions="Center"/>
        </VerticalStackLayout>
        <VerticalStackLayout WidthRequest="60" HorizontalOptions="Center"
                             VerticalOptions="Center">
          <VerticalStackLayout.GestureRecognizers>
            <TapGestureRecognizer Tapped="OnInvoicesTapped"/>
          </VerticalStackLayout.GestureRecognizers>
          <Label Text="🚩" FontSize="22" TextColor="#777"
                 HorizontalOptions="Center"/>
        </VerticalStackLayout>
        <Border BackgroundColor="#333" Padding="16,12">
          <Border.StrokeShape><RoundRectangle CornerRadius="28"/></Border.StrokeShape>
          <HorizontalStackLayout Spacing="6">
            <Label Text="⋯" FontSize="20" TextColor="#ffffff"
                   VerticalOptions="Center"/>
            <Label Text="More" FontSize="14" FontAttributes="Bold"
                   TextColor="#ffffff" VerticalOptions="Center"/>
          </HorizontalStackLayout>
        </Border>
      </HorizontalStackLayout>
    </Border>

  </Grid>
</ContentPage>
"""

# ─────────────────────────────────────────────────────────────
# MorePage.xaml.cs
# ─────────────────────────────────────────────────────────────
MORE_CS = os.path.join(VIEWS, "MorePage.xaml.cs")
MORE_CS_TEXT = """#pragma warning disable CA1416
using BuildForce.ViewModels;

namespace BuildForce.Views;

public partial class MorePage : ContentPage
{
    readonly MoreViewModel _vm;

    public MorePage(MoreViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    async void OnBackTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//DashboardPage");
    async void OnGroupClockInTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("GroupClockInPage");
    async void OnEmployeesTapped(object sender, EventArgs e)
        => await DisplayAlert("Employees", "Coming soon!", "OK");
    async void OnMapTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("JobMapPage");
    async void OnContractsTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("ContractsPage");
    async void OnReportsTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("ProfitPerJobPage");
    async void OnEstimatesTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("EstimatesPage");
    async void OnNotificationsTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("NotificationsPage");
    async void OnSubscriptionTapped(object sender, EventArgs e)
        => await DisplayAlert("Subscription", "You are on the Professional Plan.", "OK");
    async void OnSettingsTapped(object sender, EventArgs e)
        => await DisplayAlert("Settings", "Coming soon!", "OK");
    async void OnHelpTapped(object sender, EventArgs e)
        => await DisplayAlert("Support", "Email: support@mezanoconstructionmanagementplatform.com", "OK");
    async void OnProjectsTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//ProjectsPage");
    async void OnInvoicesTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//InvoicesPage");
    async void OnSignOutTapped(object sender, EventArgs e)
    {
        var ok = await DisplayAlert("Sign Out", "Are you sure?", "Sign Out", "Cancel");
        if (!ok) return;
        _vm.LogoutCommand.Execute(null);
    }
}
"""

# ─────────────────────────────────────────────────────────────
# GroupClockInPage.xaml — NEW: Foreman clock in entire crew
# ─────────────────────────────────────────────────────────────
GCI_XAML = os.path.join(VIEWS, "GroupClockInPage.xaml")
GCI_TEXT = """<?xml version="1.0" encoding="utf-8" ?>
<ContentPage
    x:Class="BuildForce.Views.GroupClockInPage"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    BackgroundColor="#111111"
    Shell.NavBarIsVisible="False"
    Shell.TabBarIsVisible="False">

  <Grid RowDefinitions="*,Auto">
    <ScrollView Grid.Row="0">
      <VerticalStackLayout Spacing="0">

        <!-- TOP BAR -->
        <Grid ColumnDefinitions="Auto,*,Auto"
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
          <Border Grid.Column="2" BackgroundColor="#222222"
                  WidthRequest="38" HeightRequest="38">
            <Border.StrokeShape><RoundRectangle CornerRadius="12"/></Border.StrokeShape>
            <Label Text="..." FontSize="16" TextColor="#ffffff"
                   HorizontalOptions="Center" VerticalOptions="Center"/>
          </Border>
        </Grid>

        <!-- TITLE -->
        <VerticalStackLayout Padding="20,0,20,24" Spacing="6">
          <Label Text="Group Clock In" FontSize="34"
                 FontAttributes="Bold" TextColor="#ffffff"/>
          <Label Text="Select crew members to clock in"
                 FontSize="15" TextColor="#777"/>
        </VerticalStackLayout>

        <!-- PROJECT SELECTOR -->
        <Label Text="Select project" Margin="20,0,20,10"
               FontSize="17" FontAttributes="Bold"
               TextColor="#ffffff"/>

        <Border x:Name="ProjectBorder"
                BackgroundColor="#1a1a1a" Margin="20,0,20,20"
                Padding="16,14">
          <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
          <Picker x:Name="ProjectPicker"
                  ItemsSource="{Binding Projects}"
                  SelectedItem="{Binding SelectedProject}"
                  ItemDisplayBinding="{Binding Name}"
                  TextColor="#ffffff" TitleColor="#555"
                  Title="Choose a project..."
                  BackgroundColor="Transparent"/>
        </Border>

        <!-- SELECT ALL ROW -->
        <Grid ColumnDefinitions="*,Auto" Margin="20,0,20,12">
          <Label Grid.Column="0" Text="Crew members"
                 FontSize="17" FontAttributes="Bold"
                 TextColor="#ffffff"/>
          <Border Grid.Column="1" BackgroundColor="#222222"
                  Padding="14,8">
            <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
            <Border.GestureRecognizers>
              <TapGestureRecognizer Tapped="OnSelectAllTapped"/>
            </Border.GestureRecognizers>
            <Label x:Name="SelectAllLabel" Text="Select all"
                   FontSize="13" TextColor="#f0a500"
                   FontAttributes="Bold"/>
          </Border>
        </Grid>

        <!-- CREW LIST -->
        <CollectionView x:Name="CrewList"
                        ItemsSource="{Binding Employees}"
                        SelectionMode="None"
                        Margin="20,0,20,20">
          <CollectionView.ItemTemplate>
            <DataTemplate>
              <Border BackgroundColor="#1a1a1a"
                      Padding="18,16" Margin="0,0,0,10">
                <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
                <Border.GestureRecognizers>
                  <TapGestureRecognizer
                      Command="{Binding Source={RelativeSource AncestorType={x:Type ContentPage}}, Path=BindingContext.ToggleEmployeeCommand}"
                      CommandParameter="{Binding .}"/>
                </Border.GestureRecognizers>
                <Grid ColumnDefinitions="Auto,*,Auto" ColumnSpacing="14">
                  <!-- Avatar circle -->
                  <Border Grid.Column="0" BackgroundColor="#f0a500"
                          WidthRequest="44" HeightRequest="44">
                    <Border.StrokeShape><RoundRectangle CornerRadius="14"/></Border.StrokeShape>
                    <Label Text="{Binding Initials}" FontSize="15"
                           FontAttributes="Bold" TextColor="#000000"
                           HorizontalOptions="Center"
                           VerticalOptions="Center"/>
                  </Border>
                  <VerticalStackLayout Grid.Column="1" Spacing="3"
                                       VerticalOptions="Center">
                    <Label Text="{Binding FullName}" FontSize="16"
                           FontAttributes="Bold" TextColor="#ffffff"/>
                    <Label Text="{Binding Role}" FontSize="12"
                           TextColor="#555"/>
                  </VerticalStackLayout>
                  <!-- Checkbox -->
                  <Border Grid.Column="2"
                          BackgroundColor="{Binding IsSelected, Converter={x:Static converters:BoolToColorConverter.Instance}}"
                          WidthRequest="28" HeightRequest="28"
                          VerticalOptions="Center">
                    <Border.StrokeShape><RoundRectangle CornerRadius="8"/></Border.StrokeShape>
                    <Label Text="✓" FontSize="16" FontAttributes="Bold"
                           TextColor="#ffffff"
                           HorizontalOptions="Center"
                           VerticalOptions="Center"
                           IsVisible="{Binding IsSelected}"/>
                  </Border>
                </Grid>
              </Border>
            </DataTemplate>
          </CollectionView.ItemTemplate>
        </CollectionView>

        <BoxView HeightRequest="100" Color="Transparent"/>
      </VerticalStackLayout>
    </ScrollView>

    <!-- CLOCK IN BUTTON -->
    <Border Grid.Row="0" VerticalOptions="End"
            Margin="20,0,20,40"
            BackgroundColor="#22c55e" Padding="0,20">
      <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
      <Border.GestureRecognizers>
        <TapGestureRecognizer Tapped="OnGroupClockInTapped"/>
      </Border.GestureRecognizers>
      <Grid ColumnDefinitions="*,Auto" Padding="20,0">
        <VerticalStackLayout Grid.Column="0" Spacing="2">
          <Label Text="Clock In Crew" FontSize="18"
                 FontAttributes="Bold" TextColor="#ffffff"/>
          <Label x:Name="SelectedCountLabel"
                 Text="0 members selected"
                 FontSize="13" TextColor="#86efac"/>
        </VerticalStackLayout>
        <Label Grid.Column="1" Text="▶" FontSize="20"
               TextColor="#ffffff" VerticalOptions="Center"/>
      </Grid>
    </Border>

  </Grid>
</ContentPage>
"""

# ─────────────────────────────────────────────────────────────
# GroupClockInPage.xaml.cs
# ─────────────────────────────────────────────────────────────
GCI_CS = os.path.join(VIEWS, "GroupClockInPage.xaml.cs")
GCI_CS_TEXT = """#pragma warning disable CA1416
using BuildForce.Models;
using BuildForce.Services;

namespace BuildForce.Views;

public partial class GroupClockInPage : ContentPage
{
    readonly ApiService _api;
    List<EmployeeDto> _employees = new();
    List<EmployeeDto> _selected = new();

    public GroupClockInPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    async Task LoadAsync()
    {
        try
        {
            _employees = await _api.GetEmployeesAsync();
            CrewList.ItemsSource = _employees;
            UpdateCount();

            var projects = await _api.GetActiveProjectsAsync();
            ProjectPicker.ItemsSource = projects;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    void OnSelectAllTapped(object sender, EventArgs e)
    {
        if (_selected.Count == _employees.Count)
        {
            _selected.Clear();
            SelectAllLabel.Text = "Select all";
        }
        else
        {
            _selected = new List<EmployeeDto>(_employees);
            SelectAllLabel.Text = "Deselect all";
        }
        UpdateCount();
        CrewList.ItemsSource = null;
        CrewList.ItemsSource = _employees;
    }

    void UpdateCount()
    {
        SelectedCountLabel.Text = $"{_selected.Count} member{(_selected.Count == 1 ? "" : "s")} selected";
    }

    async void OnGroupClockInTapped(object sender, EventArgs e)
    {
        if (_selected.Count == 0)
        {
            await DisplayAlert("No crew selected", "Please select at least one crew member.", "OK");
            return;
        }

        if (ProjectPicker.SelectedItem == null)
        {
            await DisplayAlert("No project selected", "Please select a project.", "OK");
            return;
        }

        var ok = await DisplayAlert(
            "Clock In Crew",
            $"Clock in {_selected.Count} crew member{(_selected.Count == 1 ? "" : "s")} on {((BuildForce.Models.ProjectDto)ProjectPicker.SelectedItem).Name}?",
            "Clock In", "Cancel");

        if (!ok) return;

        try
        {
            var project = (BuildForce.Models.ProjectDto)ProjectPicker.SelectedItem;
            int success = 0;
            foreach (var emp in _selected)
            {
                var result = await _api.ClockInAsync(project.Id);
                if (result.Success) success++;
            }
            await DisplayAlert("Done!", $"{success} crew members clocked in successfully.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    async void OnBackTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("..");
}
"""

write(MORE_XAML, MORE_TEXT)
write(MORE_CS,   MORE_CS_TEXT)
write(GCI_XAML,  GCI_TEXT)
write(GCI_CS,    GCI_CS_TEXT)
print("\\nAll 4 files written!")
print("Next: Register GroupClockInPage in MauiProgram.cs and AppShell.xaml")