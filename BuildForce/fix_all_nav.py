import os

ROOT  = r"C:\Users\mezan\source\repos\BuildForce\BuildForce"
VIEWS = os.path.join(ROOT, "Views")

def write(path, text):
    with open(path, "wb") as f:
        f.write(text.encode("utf-8"))
    print("OK  " + path)

# ── TimesheetPage.xaml.cs ──────────────────────────────────────
TS_CS = os.path.join(VIEWS, "TimesheetPage.xaml.cs")
TS_CS_TEXT = """#pragma warning disable CA1416
using BuildForce.Models;
using BuildForce.ViewModels;

namespace BuildForce.Views;

public partial class TimesheetPage : ContentPage
{
    readonly TimesheetViewModel _vm;

    public TimesheetPage(TimesheetViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
        DateLabel.Text = DateTime.Now.ToString("dddd, d MMMM");
    }

    private async void TimesheetList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not TimesheetDto selected)
            return;
        ((CollectionView)sender).SelectedItem = null;
        await Shell.Current.GoToAsync($"TimesheetDetailPage?timesheetId={selected.TimesheetId}");
    }

    async void OnAddTapped(object sender, EventArgs e)
        => await DisplayAlert("New Entry", "Manual time entry coming soon!", "OK");

    async void OnOptionsTapped(object sender, EventArgs e)
    {
        var action = await DisplayActionSheet("Options", "Cancel", null, "Export Timesheets", "Settings", "Help");
        if (action == "Export Timesheets")
            await DisplayAlert("Export", "Export coming soon!", "OK");
    }

    async void OnDockDashTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//DashboardPage");

    async void OnDockProjectsTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//ProjectsPage");

    async void OnDockInvoicesTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//InvoicesPage");

    async void OnDockMoreTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//MorePage");
}
"""

# ── TimesheetPage.xaml ─────────────────────────────────────────
TS_XAML = os.path.join(VIEWS, "TimesheetPage.xaml")
TS_TEXT = """<?xml version="1.0" encoding="utf-8" ?>
<ContentPage
    x:Class="BuildForce.Views.TimesheetPage"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    BackgroundColor="#111111"
    Shell.NavBarIsVisible="False"
    Shell.TabBarIsVisible="False">

  <Grid RowDefinitions="*">
    <RefreshView Command="{Binding RefreshCommand}" IsRefreshing="{Binding IsBusy}" RefreshColor="#f0a500">
      <ScrollView>
        <VerticalStackLayout Spacing="0">

          <!-- TOP BAR -->
          <Grid ColumnDefinitions="Auto,*,Auto,Auto" Padding="20,56,20,16" BackgroundColor="#111111">
            <Border Grid.Column="0" BackgroundColor="#222222" WidthRequest="38" HeightRequest="38">
              <Border.StrokeShape><RoundRectangle CornerRadius="12"/></Border.StrokeShape>
              <Border.GestureRecognizers>
                <TapGestureRecognizer Tapped="OnDockDashTapped"/>
              </Border.GestureRecognizers>
              <Label Text="&lt;" FontSize="18" FontAttributes="Bold" TextColor="#ffffff"
                     HorizontalOptions="Center" VerticalOptions="Center"/>
            </Border>
            <Label Grid.Column="1"/>
            <Border Grid.Column="2" BackgroundColor="#222222" WidthRequest="38" HeightRequest="38" Margin="0,0,10,0">
              <Border.StrokeShape><RoundRectangle CornerRadius="12"/></Border.StrokeShape>
              <Border.GestureRecognizers>
                <TapGestureRecognizer Tapped="OnAddTapped"/>
              </Border.GestureRecognizers>
              <Label Text="+" FontSize="22" FontAttributes="Bold" TextColor="#ffffff"
                     HorizontalOptions="Center" VerticalOptions="Center"/>
            </Border>
            <Border Grid.Column="3" BackgroundColor="#222222" WidthRequest="38" HeightRequest="38">
              <Border.StrokeShape><RoundRectangle CornerRadius="12"/></Border.StrokeShape>
              <Border.GestureRecognizers>
                <TapGestureRecognizer Tapped="OnOptionsTapped"/>
              </Border.GestureRecognizers>
              <Label Text="..." FontSize="16" TextColor="#ffffff"
                     HorizontalOptions="Center" VerticalOptions="Center"/>
            </Border>
          </Grid>

          <!-- BIG TITLE -->
          <VerticalStackLayout Padding="20,0,20,24" Spacing="8">
            <Label Text="Time Clock" FontSize="36" FontAttributes="Bold" TextColor="#ffffff"/>
            <Label Text="{Binding CurrentEmployeeName}" FontSize="15" TextColor="#777"/>
          </VerticalStackLayout>

          <!-- TABS -->
          <ScrollView Orientation="Horizontal" HorizontalScrollBarVisibility="Never" Margin="0,0,0,20">
            <HorizontalStackLayout Padding="20,0" Spacing="8">
              <Border BackgroundColor="#222222" Padding="18,10">
                <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
                <Label Text="Overview" FontSize="14" TextColor="#777"/>
              </Border>
              <Border BackgroundColor="#ffffff" Padding="18,10">
                <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
                <Label Text="Time" FontSize="14" FontAttributes="Bold" TextColor="#111111"/>
              </Border>
              <Border BackgroundColor="#222222" Padding="18,10">
                <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
                <Label Text="Projects" FontSize="14" TextColor="#777"/>
              </Border>
              <Border BackgroundColor="#222222" Padding="18,10">
                <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
                <Label Text="More" FontSize="14" TextColor="#777"/>
              </Border>
            </HorizontalStackLayout>
          </ScrollView>

          <!-- DATE + TOTAL -->
          <Grid ColumnDefinitions="*,Auto" Padding="20,0,20,16">
            <Label Grid.Column="0" x:Name="DateLabel" Text="Today"
                   FontSize="17" FontAttributes="Bold" TextColor="#ffffff"/>
            <Label Grid.Column="1"
                   Text="{Binding WeeklySummary.TotalHours, StringFormat='{0:F2}h total'}"
                   FontSize="15" TextColor="#777"/>
          </Grid>

          <!-- WEEK STRIP -->
          <ScrollView Orientation="Horizontal" HorizontalScrollBarVisibility="Never" Margin="0,0,0,24">
            <HorizontalStackLayout Padding="20,0" Spacing="8">
              <VerticalStackLayout Spacing="4" HorizontalOptions="Center" WidthRequest="52">
                <Label Text="Sun" FontSize="12" TextColor="#555" HorizontalOptions="Center"/>
                <Border BackgroundColor="#222" WidthRequest="44" HeightRequest="44">
                  <Border.StrokeShape><RoundRectangle CornerRadius="14"/></Border.StrokeShape>
                  <Label Text="23" FontSize="16" FontAttributes="Bold" TextColor="#777"
                         HorizontalOptions="Center" VerticalOptions="Center"/>
                </Border>
              </VerticalStackLayout>
              <VerticalStackLayout Spacing="4" HorizontalOptions="Center" WidthRequest="52">
                <Label Text="Mon" FontSize="12" TextColor="#0ea5e9" HorizontalOptions="Center"/>
                <Border BackgroundColor="#0c2a3a" WidthRequest="44" HeightRequest="44"
                        Stroke="#0ea5e9" StrokeThickness="1.5">
                  <Border.StrokeShape><RoundRectangle CornerRadius="14"/></Border.StrokeShape>
                  <Label Text="24" FontSize="16" FontAttributes="Bold" TextColor="#0ea5e9"
                         HorizontalOptions="Center" VerticalOptions="Center"/>
                </Border>
              </VerticalStackLayout>
              <VerticalStackLayout Spacing="4" HorizontalOptions="Center" WidthRequest="52">
                <Label Text="Tue" FontSize="12" TextColor="#555" HorizontalOptions="Center"/>
                <Border BackgroundColor="#222" WidthRequest="44" HeightRequest="44">
                  <Border.StrokeShape><RoundRectangle CornerRadius="14"/></Border.StrokeShape>
                  <Label Text="25" FontSize="16" FontAttributes="Bold" TextColor="#777"
                         HorizontalOptions="Center" VerticalOptions="Center"/>
                </Border>
              </VerticalStackLayout>
              <VerticalStackLayout Spacing="4" HorizontalOptions="Center" WidthRequest="52">
                <Label Text="Wed" FontSize="12" TextColor="#555" HorizontalOptions="Center"/>
                <Border BackgroundColor="#222" WidthRequest="44" HeightRequest="44">
                  <Border.StrokeShape><RoundRectangle CornerRadius="14"/></Border.StrokeShape>
                  <Label Text="26" FontSize="16" FontAttributes="Bold" TextColor="#777"
                         HorizontalOptions="Center" VerticalOptions="Center"/>
                </Border>
              </VerticalStackLayout>
              <VerticalStackLayout Spacing="4" HorizontalOptions="Center" WidthRequest="52">
                <Label Text="Thu" FontSize="12" TextColor="#555" HorizontalOptions="Center"/>
                <Border BackgroundColor="#222" WidthRequest="44" HeightRequest="44">
                  <Border.StrokeShape><RoundRectangle CornerRadius="14"/></Border.StrokeShape>
                  <Label Text="27" FontSize="16" FontAttributes="Bold" TextColor="#777"
                         HorizontalOptions="Center" VerticalOptions="Center"/>
                </Border>
              </VerticalStackLayout>
              <VerticalStackLayout Spacing="4" HorizontalOptions="Center" WidthRequest="52">
                <Label Text="Fri" FontSize="12" TextColor="#555" HorizontalOptions="Center"/>
                <Border BackgroundColor="#222" WidthRequest="44" HeightRequest="44">
                  <Border.StrokeShape><RoundRectangle CornerRadius="14"/></Border.StrokeShape>
                  <Label Text="28" FontSize="16" FontAttributes="Bold" TextColor="#777"
                         HorizontalOptions="Center" VerticalOptions="Center"/>
                </Border>
              </VerticalStackLayout>
            </HorizontalStackLayout>
          </ScrollView>

          <!-- CLOCK CARD -->
          <Border BackgroundColor="#1a1a1a" Margin="20,0,20,12" Padding="20,20">
            <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
            <Grid ColumnDefinitions="*,Auto">
              <VerticalStackLayout Grid.Column="0" Spacing="6">
                <Label Text="{Binding ClockDuration}" FontSize="22" FontAttributes="Bold"
                       TextColor="#ffffff" IsVisible="{Binding IsClockedIn}"/>
                <Label Text="Not clocked in" FontSize="18" FontAttributes="Bold"
                       TextColor="#777" IsVisible="{Binding IsNotClockedIn}"/>
                <Label Text="{Binding SelectedProject.Name}" FontSize="14" TextColor="#777"/>
              </VerticalStackLayout>
              <Border Grid.Column="1" BackgroundColor="{Binding ClockButtonColor}"
                      WidthRequest="48" HeightRequest="48" VerticalOptions="Center">
                <Border.StrokeShape><RoundRectangle CornerRadius="14"/></Border.StrokeShape>
                <Border.GestureRecognizers>
                  <TapGestureRecognizer Command="{Binding ClockToggleCommand}"/>
                </Border.GestureRecognizers>
                <Grid>
                  <Label Text="■" FontSize="18" TextColor="#ffffff"
                         HorizontalOptions="Center" VerticalOptions="Center"
                         IsVisible="{Binding IsClockedIn}"/>
                  <Label Text="▶" FontSize="18" TextColor="#ffffff"
                         HorizontalOptions="Center" VerticalOptions="Center"
                         IsVisible="{Binding IsNotClockedIn}"/>
                </Grid>
              </Border>
            </Grid>
          </Border>

          <!-- PROJECT PICKER -->
          <Border BackgroundColor="#1a1a1a" Margin="20,0,20,12" Padding="16,14"
                  IsVisible="{Binding IsNotClockedIn}">
            <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
            <VerticalStackLayout Spacing="6">
              <Label Text="SELECT PROJECT" FontSize="10" FontAttributes="Bold"
                     TextColor="#555" CharacterSpacing="1.5"/>
              <Picker ItemsSource="{Binding Projects}" SelectedItem="{Binding SelectedProject}"
                      ItemDisplayBinding="{Binding Name}" TextColor="#ffffff"
                      TitleColor="#555" Title="Choose a project..."
                      BackgroundColor="Transparent"/>
            </VerticalStackLayout>
          </Border>

          <!-- ERROR -->
          <Label Margin="20,0,20,12" Text="{Binding ErrorMessage}"
                 FontSize="13" TextColor="#ef4444"
                 IsVisible="{Binding HasError}" HorizontalTextAlignment="Center"/>

          <!-- RECENT ENTRIES -->
          <Grid ColumnDefinitions="*,Auto" Padding="20,8,20,12">
            <Label Grid.Column="0" Text="Recent entries"
                   FontSize="17" FontAttributes="Bold" TextColor="#ffffff"/>
            <Label Grid.Column="1" Text="Total hours" FontSize="14" TextColor="#777"
                   VerticalOptions="Center"/>
          </Grid>

          <CollectionView x:Name="TimesheetList" ItemsSource="{Binding Timesheets}"
                          SelectionMode="Single"
                          SelectionChanged="TimesheetList_SelectionChanged"
                          Margin="20,0,20,0">
            <CollectionView.ItemTemplate>
              <DataTemplate>
                <Border BackgroundColor="#1a1a1a" Padding="20,18" Margin="0,0,0,10">
                  <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
                  <VerticalStackLayout Spacing="6">
                    <Grid ColumnDefinitions="*,Auto">
                      <Label Grid.Column="0" Text="{Binding ProjectName}"
                             FontSize="17" FontAttributes="Bold" TextColor="#ffffff"/>
                      <Label Grid.Column="1"
                             Text="{Binding HoursWorked, StringFormat='{0:F2}h'}"
                             FontSize="16" FontAttributes="Bold" TextColor="#ffffff"/>
                    </Grid>
                    <Grid ColumnDefinitions="*,Auto">
                      <Label Grid.Column="0"
                             Text="{Binding Date, StringFormat='Start: {0:h:mm tt}'}"
                             FontSize="13" TextColor="#777"/>
                      <Label Grid.Column="1" Text="{Binding Status}"
                             FontSize="13" TextColor="#555"/>
                    </Grid>
                    <Label Text="Breaktime: None" FontSize="12" TextColor="#444"/>
                  </VerticalStackLayout>
                </Border>
              </DataTemplate>
            </CollectionView.ItemTemplate>
          </CollectionView>

          <BoxView HeightRequest="120" Color="Transparent"/>
        </VerticalStackLayout>
      </ScrollView>
    </RefreshView>

    <!-- BOTTOM PILL DOCK -->
    <Border VerticalOptions="End" HorizontalOptions="Center"
            Margin="0,0,0,32" BackgroundColor="#222222" Padding="8,8">
      <Border.StrokeShape><RoundRectangle CornerRadius="36"/></Border.StrokeShape>
      <HorizontalStackLayout Spacing="0">

        <VerticalStackLayout WidthRequest="60" HorizontalOptions="Center" VerticalOptions="Center">
          <VerticalStackLayout.GestureRecognizers>
            <TapGestureRecognizer Tapped="OnDockDashTapped"/>
          </VerticalStackLayout.GestureRecognizers>
          <Label Text="⌂" FontSize="22" TextColor="#777" HorizontalOptions="Center"/>
        </VerticalStackLayout>

        <Border BackgroundColor="#333" Padding="20,12">
          <Border.StrokeShape><RoundRectangle CornerRadius="28"/></Border.StrokeShape>
          <HorizontalStackLayout Spacing="8">
            <Label Text="⏱" FontSize="20" VerticalOptions="Center"/>
            <Label Text="Time" FontSize="15" FontAttributes="Bold"
                   TextColor="#ffffff" VerticalOptions="Center"/>
          </HorizontalStackLayout>
        </Border>

        <VerticalStackLayout WidthRequest="60" HorizontalOptions="Center" VerticalOptions="Center">
          <VerticalStackLayout.GestureRecognizers>
            <TapGestureRecognizer Tapped="OnDockProjectsTapped"/>
          </VerticalStackLayout.GestureRecognizers>
          <Label Text="📁" FontSize="22" TextColor="#777" HorizontalOptions="Center"/>
        </VerticalStackLayout>

        <VerticalStackLayout WidthRequest="60" HorizontalOptions="Center" VerticalOptions="Center">
          <VerticalStackLayout.GestureRecognizers>
            <TapGestureRecognizer Tapped="OnDockMoreTapped"/>
          </VerticalStackLayout.GestureRecognizers>
          <Label Text="⋯" FontSize="22" TextColor="#777" HorizontalOptions="Center"/>
        </VerticalStackLayout>

      </HorizontalStackLayout>
    </Border>

  </Grid>
</ContentPage>
"""

write(TS_XAML, TS_TEXT)
write(TS_CS, TS_CS_TEXT)
print("Done! Build and deploy.")