import os

ROOT  = r"C:\Users\mezan\source\repos\BuildForce\BuildForce"
VIEWS = os.path.join(ROOT, "Views")

def write(path, text):
    with open(path, "wb") as f:
        f.write(text.encode("utf-8"))
    print("OK  " + path)

# Fix: StringFormat must use single quotes in XAML: StringFormat='{}{0:F1}h'
# In Python strings we write that as: StringFormat=\'{}{0:F1}h\'
# But since we write as bytes we just use the literal characters

TS_XAML = os.path.join(VIEWS, "TimesheetPage.xaml")
TS_XAML_TEXT = """<?xml version="1.0" encoding="utf-8" ?>
<ContentPage
    x:Class="BuildForce.Views.TimesheetPage"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    Title="Time Clock"
    BackgroundColor="#080806">
  <RefreshView Command="{Binding RefreshCommand}" IsRefreshing="{Binding IsBusy}" RefreshColor="#f0a500">
    <ScrollView>
      <VerticalStackLayout Padding="20,16" Spacing="20">

        <!-- CLOCK CARD -->
        <Border BackgroundColor="#0d0b05" Stroke="#f0a500" StrokeThickness="1.5" Padding="24,28">
          <Border.StrokeShape><RoundRectangle CornerRadius="24"/></Border.StrokeShape>
          <VerticalStackLayout Spacing="20">

            <Label Text="TIME CLOCK" FontSize="11" FontAttributes="Bold" TextColor="#f0a500"
                   HorizontalOptions="Center"/>

            <Label Text="{Binding ClockDuration}" FontSize="56" FontAttributes="Bold"
                   TextColor="#f0a500" HorizontalOptions="Center"
                   IsVisible="{Binding IsClockedIn}"/>

            <Label Text="00:00:00" FontSize="56" FontAttributes="Bold"
                   TextColor="#1a1800" HorizontalOptions="Center"
                   IsVisible="{Binding IsNotClockedIn}"/>

            <VerticalStackLayout Spacing="8" IsVisible="{Binding IsNotClockedIn}">
              <Label Text="SELECT PROJECT" FontSize="10" FontAttributes="Bold" TextColor="#f0a500"/>
              <Border BackgroundColor="#0a0800" Stroke="#f0a500" StrokeThickness="0.5" Padding="16,12">
                <Border.StrokeShape><RoundRectangle CornerRadius="12"/></Border.StrokeShape>
                <Picker ItemsSource="{Binding Projects}" SelectedItem="{Binding SelectedProject}"
                        ItemDisplayBinding="{Binding Name}" TextColor="#ffffff"
                        TitleColor="#555" Title="Choose a project..."
                        BackgroundColor="Transparent"/>
              </Border>
            </VerticalStackLayout>

            <VerticalStackLayout Spacing="6" IsVisible="{Binding IsClockedIn}" HorizontalOptions="Center">
              <Label Text="WORKING ON" FontSize="10" TextColor="#555" HorizontalOptions="Center"/>
              <Label Text="{Binding SelectedProject.Name}" FontSize="20" FontAttributes="Bold"
                     TextColor="#ffffff" HorizontalOptions="Center"/>
              <Label Text="{Binding CurrentEmployeeName}" FontSize="13" TextColor="#888"
                     HorizontalOptions="Center"/>
            </VerticalStackLayout>

            <Button Text="{Binding ClockButtonText}"
                    Command="{Binding ClockToggleCommand}"
                    BackgroundColor="{Binding ClockButtonColor}"
                    TextColor="#ffffff" FontAttributes="Bold" FontSize="20"
                    CornerRadius="18" HeightRequest="72"
                    Margin="0,4,0,0"/>

            <Label Text="GPS location will be recorded" FontSize="11" TextColor="#333"
                   HorizontalOptions="Center"/>

            <Label Text="{Binding ErrorMessage}" FontSize="13" TextColor="#ef4444"
                   HorizontalOptions="Center" IsVisible="{Binding HasError}"
                   HorizontalTextAlignment="Center"/>

          </VerticalStackLayout>
        </Border>

        <Label Text="THIS WEEK" FontSize="11" FontAttributes="Bold" TextColor="#f0a500"/>

        <Grid ColumnDefinitions="*,*,*" ColumnSpacing="10">
          <Border Grid.Column="0" BackgroundColor="#0d0b05" Stroke="#f0a500" StrokeThickness="0.5" Padding="14">
            <Border.StrokeShape><RoundRectangle CornerRadius="14"/></Border.StrokeShape>
            <VerticalStackLayout Spacing="4" HorizontalOptions="Center">
              <Label Text="REGULAR" FontSize="9" FontAttributes="Bold" TextColor="#f0a500" HorizontalOptions="Center"/>
              <Label Text="{Binding WeeklySummary.TotalRegularHours, StringFormat='{}{0:F1}h'}" FontSize="24"
                     FontAttributes="Bold" TextColor="#ffffff" HorizontalOptions="Center"/>
            </VerticalStackLayout>
          </Border>
          <Border Grid.Column="1" BackgroundColor="#0d0808" Stroke="#ef4444" StrokeThickness="0.5" Padding="14">
            <Border.StrokeShape><RoundRectangle CornerRadius="14"/></Border.StrokeShape>
            <VerticalStackLayout Spacing="4" HorizontalOptions="Center">
              <Label Text="OVERTIME" FontSize="9" FontAttributes="Bold" TextColor="#ef4444" HorizontalOptions="Center"/>
              <Label Text="{Binding WeeklySummary.TotalOvertimeHours, StringFormat='{}{0:F1}h'}" FontSize="24"
                     FontAttributes="Bold" TextColor="#ffffff" HorizontalOptions="Center"/>
            </VerticalStackLayout>
          </Border>
          <Border Grid.Column="2" BackgroundColor="#080d08" Stroke="#22c55e" StrokeThickness="0.5" Padding="14">
            <Border.StrokeShape><RoundRectangle CornerRadius="14"/></Border.StrokeShape>
            <VerticalStackLayout Spacing="4" HorizontalOptions="Center">
              <Label Text="TOTAL" FontSize="9" FontAttributes="Bold" TextColor="#22c55e" HorizontalOptions="Center"/>
              <Label Text="{Binding WeeklySummary.TotalHours, StringFormat='{}{0:F1}h'}" FontSize="24"
                     FontAttributes="Bold" TextColor="#ffffff" HorizontalOptions="Center"/>
            </VerticalStackLayout>
          </Border>
        </Grid>

        <Label Text="RECENT ENTRIES" FontSize="11" FontAttributes="Bold" TextColor="#f0a500"/>

        <CollectionView x:Name="TimesheetList"
                        ItemsSource="{Binding Timesheets}"
                        SelectionMode="Single"
                        SelectionChanged="TimesheetList_SelectionChanged">
          <CollectionView.ItemTemplate>
            <DataTemplate>
              <Border BackgroundColor="#0d0b05" Stroke="#1a1500" StrokeThickness="1"
                      Padding="18,14" Margin="0,0,0,10">
                <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
                <Grid ColumnDefinitions="*,Auto">
                  <VerticalStackLayout Grid.Column="0" Spacing="4">
                    <Label Text="{Binding ProjectName}" FontAttributes="Bold"
                           FontSize="15" TextColor="#ffffff"/>
                    <Label Text="{Binding EmployeeName}" FontSize="12" TextColor="#888"/>
                    <Label Text="{Binding Date, StringFormat='{}{0:MMM d, yyyy}'}" FontSize="12" TextColor="#444"/>
                  </VerticalStackLayout>
                  <VerticalStackLayout Grid.Column="1" HorizontalOptions="End" Spacing="4">
                    <Label Text="{Binding HoursWorked, StringFormat='{}{0:F1}h'}" FontAttributes="Bold"
                           FontSize="18" TextColor="#f0a500" HorizontalOptions="End"/>
                    <Label Text="{Binding Status}" FontSize="11" TextColor="#555" HorizontalOptions="End"/>
                    <Label Text="&gt;" FontSize="14" TextColor="#f0a500" HorizontalOptions="End"/>
                  </VerticalStackLayout>
                </Grid>
              </Border>
            </DataTemplate>
          </CollectionView.ItemTemplate>
        </CollectionView>

        <BoxView HeightRequest="80" Color="Transparent"/>
      </VerticalStackLayout>
    </ScrollView>
  </RefreshView>
</ContentPage>
"""

TSD_XAML = os.path.join(VIEWS, "TimesheetDetailPage.xaml")
TSD_XAML_TEXT = """<?xml version="1.0" encoding="utf-8" ?>
<ContentPage
    x:Class="BuildForce.Views.TimesheetDetailPage"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    Title="Timesheet Detail"
    BackgroundColor="#080806"
    Shell.NavBarIsVisible="True"
    Shell.TabBarIsVisible="False">
  <ScrollView>
    <VerticalStackLayout Padding="20,16" Spacing="16">

      <!-- HEADER CARD -->
      <Border BackgroundColor="#0d0b05" Stroke="#f0a500" StrokeThickness="1.5" Padding="24,22">
        <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
        <VerticalStackLayout Spacing="12">
          <Label Text="{Binding ProjectName}" FontSize="22" FontAttributes="Bold"
                 TextColor="#ffffff" HorizontalOptions="Center"/>
          <Label Text="{Binding DateDisplay}" FontSize="13" TextColor="#888"
                 HorizontalOptions="Center"/>
          <Border BackgroundColor="#0a0800" Padding="16,8" HorizontalOptions="Center"
                  Stroke="{Binding StatusColor}">
            <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
            <Label Text="{Binding StatusDisplay}" FontSize="13" FontAttributes="Bold"
                   TextColor="{Binding StatusColor}" HorizontalOptions="Center"/>
          </Border>
          <Label Text="{Binding DurationDisplay}" FontSize="42" FontAttributes="Bold"
                 TextColor="#f0a500" HorizontalOptions="Center" Margin="0,8,0,0"/>
          <Label Text="TOTAL DURATION" FontSize="10" TextColor="#555" HorizontalOptions="Center"/>
        </VerticalStackLayout>
      </Border>

      <!-- EMPLOYEE CARD -->
      <Border BackgroundColor="#0d0b05" Stroke="#1a1500" StrokeThickness="1" Padding="20,16">
        <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
        <VerticalStackLayout Spacing="10">
          <Label Text="EMPLOYEE" FontSize="10" FontAttributes="Bold" TextColor="#f0a500"/>
          <Label Text="{Binding EmployeeName}" FontSize="17" FontAttributes="Bold" TextColor="#ffffff"/>
        </VerticalStackLayout>
      </Border>

      <!-- CLOCK IN / OUT -->
      <Border BackgroundColor="#0d0b05" Stroke="#1a1500" StrokeThickness="1" Padding="20,16">
        <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
        <VerticalStackLayout Spacing="14">
          <Label Text="TIME DETAILS" FontSize="10" FontAttributes="Bold" TextColor="#f0a500"/>
          <Grid ColumnDefinitions="*,*" RowDefinitions="Auto,Auto" RowSpacing="4" ColumnSpacing="12">
            <Label Grid.Column="0" Grid.Row="0" Text="CLOCK IN" FontSize="10" TextColor="#555"/>
            <Label Grid.Column="1" Grid.Row="0" Text="CLOCK OUT" FontSize="10" TextColor="#555"/>
            <Label Grid.Column="0" Grid.Row="1" Text="{Binding ClockInDisplay}"
                   FontSize="20" FontAttributes="Bold" TextColor="#10b981"/>
            <Label Grid.Column="1" Grid.Row="1" Text="{Binding ClockOutDisplay}"
                   FontSize="20" FontAttributes="Bold" TextColor="#ef4444"/>
          </Grid>
        </VerticalStackLayout>
      </Border>

      <!-- HOURS BREAKDOWN -->
      <Border BackgroundColor="#0d0b05" Stroke="#1a1500" StrokeThickness="1" Padding="20,16">
        <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
        <VerticalStackLayout Spacing="14">
          <Label Text="HOURS BREAKDOWN" FontSize="10" FontAttributes="Bold" TextColor="#f0a500"/>
          <Grid ColumnDefinitions="*,*,*" ColumnSpacing="10">
            <VerticalStackLayout Grid.Column="0" Spacing="4" HorizontalOptions="Center">
              <Label Text="REGULAR" FontSize="9" TextColor="#f0a500" HorizontalOptions="Center"/>
              <Label Text="{Binding RegularHours}" FontSize="22" FontAttributes="Bold"
                     TextColor="#ffffff" HorizontalOptions="Center"/>
            </VerticalStackLayout>
            <VerticalStackLayout Grid.Column="1" Spacing="4" HorizontalOptions="Center">
              <Label Text="OVERTIME" FontSize="9" TextColor="#ef4444" HorizontalOptions="Center"/>
              <Label Text="{Binding OvertimeHours}" FontSize="22" FontAttributes="Bold"
                     TextColor="#ffffff" HorizontalOptions="Center"/>
            </VerticalStackLayout>
            <VerticalStackLayout Grid.Column="2" Spacing="4" HorizontalOptions="Center">
              <Label Text="TOTAL" FontSize="9" TextColor="#22c55e" HorizontalOptions="Center"/>
              <Label Text="{Binding TotalHours}" FontSize="22" FontAttributes="Bold"
                     TextColor="#ffffff" HorizontalOptions="Center"/>
            </VerticalStackLayout>
          </Grid>
        </VerticalStackLayout>
      </Border>

      <!-- ACTION BUTTONS -->
      <Button Text="Approve Timesheet"
              Command="{Binding ApproveCommand}"
              BackgroundColor="#8b5cf6"
              TextColor="#ffffff" FontAttributes="Bold" FontSize="16"
              CornerRadius="14" HeightRequest="56"
              IsVisible="{Binding IsCompleted}"/>

      <Button Text="Delete Entry"
              Command="{Binding DeleteCommand}"
              BackgroundColor="#1a0505" BorderColor="#ef4444" BorderWidth="1"
              TextColor="#ef4444" FontSize="15"
              CornerRadius="14" HeightRequest="48"/>

      <BoxView HeightRequest="40" Color="Transparent"/>
    </VerticalStackLayout>
  </ScrollView>
</ContentPage>
"""

write(TS_XAML,  TS_XAML_TEXT)
write(TSD_XAML, TSD_XAML_TEXT)

print("\nDone! Only XAML files updated. Build and deploy to Samsung.")