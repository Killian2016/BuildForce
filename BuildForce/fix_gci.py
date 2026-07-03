import os

ROOT  = r"C:\Users\mezan\source\repos\BuildForce\BuildForce"
VIEWS = os.path.join(ROOT, "Views")

def write(path, text):
    with open(path, "wb") as f:
        f.write(text.encode("utf-8"))
    print("OK  " + path)

# Fix GroupClockInPage.xaml - remove converter, use simple checkbox
GCI_XAML = os.path.join(VIEWS, "GroupClockInPage.xaml")
GCI_TEXT = """<?xml version="1.0" encoding="utf-8" ?>
<ContentPage
    x:Class="BuildForce.Views.GroupClockInPage"
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
               FontSize="17" FontAttributes="Bold" TextColor="#ffffff"/>

        <Border BackgroundColor="#1a1a1a" Margin="20,0,20,24" Padding="16,14">
          <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
          <Picker x:Name="ProjectPicker"
                  TextColor="#ffffff" TitleColor="#555"
                  Title="Choose a project..."
                  BackgroundColor="Transparent"/>
        </Border>

        <!-- SELECT ALL ROW -->
        <Grid ColumnDefinitions="*,Auto" Margin="20,0,20,12">
          <Label Grid.Column="0" Text="Crew members"
                 FontSize="17" FontAttributes="Bold" TextColor="#ffffff"/>
          <Border Grid.Column="1" BackgroundColor="#222222" Padding="14,8">
            <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
            <Border.GestureRecognizers>
              <TapGestureRecognizer Tapped="OnSelectAllTapped"/>
            </Border.GestureRecognizers>
            <Label x:Name="SelectAllLabel" Text="Select all"
                   FontSize="13" TextColor="#f0a500" FontAttributes="Bold"/>
          </Border>
        </Grid>

        <!-- CREW LIST -->
        <CollectionView x:Name="CrewList" SelectionMode="None" Margin="20,0,20,20">
          <CollectionView.ItemTemplate>
            <DataTemplate>
              <Border x:Name="EmployeeBorder" BackgroundColor="#1a1a1a"
                      Padding="18,16" Margin="0,0,0,10">
                <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
                <Border.GestureRecognizers>
                  <TapGestureRecognizer Tapped="OnEmployeeTapped"/>
                </Border.GestureRecognizers>
                <Grid ColumnDefinitions="Auto,*,Auto" ColumnSpacing="14">
                  <Border Grid.Column="0" BackgroundColor="#f0a500"
                          WidthRequest="44" HeightRequest="44">
                    <Border.StrokeShape><RoundRectangle CornerRadius="14"/></Border.StrokeShape>
                    <Label Text="{Binding Initials}" FontSize="15"
                           FontAttributes="Bold" TextColor="#000000"
                           HorizontalOptions="Center" VerticalOptions="Center"/>
                  </Border>
                  <VerticalStackLayout Grid.Column="1" Spacing="3"
                                       VerticalOptions="Center">
                    <Label Text="{Binding FullName}" FontSize="16"
                           FontAttributes="Bold" TextColor="#ffffff"/>
                    <Label Text="{Binding Role}" FontSize="12" TextColor="#555"/>
                  </VerticalStackLayout>
                  <!-- Simple checkbox border -->
                  <Border Grid.Column="2" BackgroundColor="#2a2a2a"
                          WidthRequest="28" HeightRequest="28"
                          VerticalOptions="Center">
                    <Border.StrokeShape><RoundRectangle CornerRadius="8"/></Border.StrokeShape>
                    <Label Text="" FontSize="14" FontAttributes="Bold"
                           TextColor="#ffffff"
                           HorizontalOptions="Center" VerticalOptions="Center"/>
                  </Border>
                </Grid>
              </Border>
            </DataTemplate>
          </CollectionView.ItemTemplate>
        </CollectionView>

        <BoxView HeightRequest="120" Color="Transparent"/>
      </VerticalStackLayout>
    </ScrollView>

    <!-- CLOCK IN BUTTON -->
    <Border VerticalOptions="End" Margin="20,0,20,40"
            BackgroundColor="#22c55e" Padding="20,20">
      <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
      <Border.GestureRecognizers>
        <TapGestureRecognizer Tapped="OnGroupClockInTapped"/>
      </Border.GestureRecognizers>
      <Grid ColumnDefinitions="*,Auto">
        <VerticalStackLayout Grid.Column="0" Spacing="2">
          <Label Text="Clock In Crew" FontSize="18"
                 FontAttributes="Bold" TextColor="#ffffff"/>
          <Label x:Name="SelectedCountLabel" Text="0 members selected"
                 FontSize="13" TextColor="#86efac"/>
        </VerticalStackLayout>
        <Label Grid.Column="1" Text="&#x25B6;" FontSize="20"
               TextColor="#ffffff" VerticalOptions="Center"/>
      </Grid>
    </Border>

  </Grid>
</ContentPage>
"""

# Fix GroupClockInPage.xaml.cs - use tap on row with visual feedback
GCI_CS = os.path.join(VIEWS, "GroupClockInPage.xaml.cs")
GCI_CS_TEXT = """#pragma warning disable CA1416
using BuildForce.Models;
using BuildForce.Services;

namespace BuildForce.Views;

public partial class GroupClockInPage : ContentPage
{
    readonly ApiService _api;
    List<EmployeeDto> _employees = new();
    readonly HashSet<int> _selectedIds = new();
    List<ProjectDto> _projects = new();

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
            CrewList.ItemsSource = null;
            CrewList.ItemsSource = _employees;

            _projects = await _api.GetActiveProjectsAsync();
            ProjectPicker.ItemsSource = _projects;
            ProjectPicker.ItemDisplayBinding = new Binding("Name");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    void OnSelectAllTapped(object sender, EventArgs e)
    {
        if (_selectedIds.Count == _employees.Count)
        {
            _selectedIds.Clear();
            SelectAllLabel.Text = "Select all";
        }
        else
        {
            foreach (var emp in _employees)
                _selectedIds.Add(emp.EmployeeId);
            SelectAllLabel.Text = "Deselect all";
        }
        UpdateCount();
    }

    void OnEmployeeTapped(object sender, EventArgs e)
    {
        if (sender is Border border && border.BindingContext is EmployeeDto emp)
        {
            if (_selectedIds.Contains(emp.EmployeeId))
            {
                _selectedIds.Remove(emp.EmployeeId);
                border.BackgroundColor = Color.FromArgb("#1a1a1a");
            }
            else
            {
                _selectedIds.Add(emp.EmployeeId);
                border.BackgroundColor = Color.FromArgb("#0d2318");
            }
            UpdateCount();
        }
    }

    void UpdateCount()
    {
        var c = _selectedIds.Count;
        SelectedCountLabel.Text = $"{c} member{(c == 1 ? "" : "s")} selected";
    }

    async void OnGroupClockInTapped(object sender, EventArgs e)
    {
        if (_selectedIds.Count == 0)
        {
            await DisplayAlert("No crew selected", "Please select at least one crew member.", "OK");
            return;
        }
        if (ProjectPicker.SelectedItem is not ProjectDto project)
        {
            await DisplayAlert("No project selected", "Please select a project.", "OK");
            return;
        }

        var ok = await DisplayAlert(
            "Clock In Crew",
            $"Clock in {_selectedIds.Count} crew member{(_selectedIds.Count == 1 ? "" : "s")} on {project.Name}?",
            "Clock In", "Cancel");

        if (!ok) return;

        try
        {
            int success = 0;
            foreach (var id in _selectedIds)
            {
                var result = await _api.ClockInAsync(project.Id);
                if (result.Success) success++;
            }
            await DisplayAlert("Done!", $"{success} crew members clocked in on {project.Name}.", "OK");
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

write(GCI_XAML, GCI_TEXT)
write(GCI_CS,   GCI_CS_TEXT)
print("Done! Build and deploy.")