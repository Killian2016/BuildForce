import os

ROOT  = r"C:\Users\mezan\source\repos\BuildForce\BuildForce"
VIEWS = os.path.join(ROOT, "Views")

def write(path, text):
    with open(path, "wb") as f:
        f.write(text.encode("utf-8"))
    print("OK  " + path)

# ═══════════════════════════════════════════════════════════════
# TimesheetPage.xaml — Procore style
# ═══════════════════════════════════════════════════════════════
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
          <Grid ColumnDefinitions="Auto,*,Auto,Auto"
                Padding="20,56,20,16" BackgroundColor="#111111">
            <Border Grid.Column="0" BackgroundColor="#222222"
                    WidthRequest="38" HeightRequest="38">
              <Border.StrokeShape><RoundRectangle CornerRadius="12"/></Border.StrokeShape>
              <Label Text="&lt;" FontSize="16" FontAttributes="Bold"
                     TextColor="#ffffff"
                     HorizontalOptions="Center" VerticalOptions="Center"/>
            </Border>
            <Label Grid.Column="2" Text="+" FontSize="24"
                   TextColor="#ffffff" VerticalOptions="Center"
                   Margin="0,0,16,0"/>
            <Border Grid.Column="3" BackgroundColor="#222222"
                    WidthRequest="38" HeightRequest="38">
              <Border.StrokeShape><RoundRectangle CornerRadius="12"/></Border.StrokeShape>
              <Label Text="..." FontSize="16" TextColor="#ffffff"
                     HorizontalOptions="Center" VerticalOptions="Center"/>
            </Border>
          </Grid>

          <!-- BIG TITLE -->
          <VerticalStackLayout Padding="20,0,20,24" Spacing="8">
            <Label Text="Time Clock" FontSize="36" FontAttributes="Bold"
                   TextColor="#ffffff" LineBreakMode="WordWrap"/>
            <Label Text="{Binding CurrentEmployeeName}"
                   FontSize="15" TextColor="#777"/>
          </VerticalStackLayout>

          <!-- TABS -->
          <ScrollView Orientation="Horizontal"
                      HorizontalScrollBarVisibility="Never"
                      Margin="0,0,0,20">
            <HorizontalStackLayout Padding="20,0" Spacing="8">
              <Border BackgroundColor="#222222" Padding="18,10">
                <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
                <Label Text="Overview" FontSize="14"
                       TextColor="#777"/>
              </Border>
              <Border BackgroundColor="#ffffff" Padding="18,10">
                <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
                <Label Text="Time" FontSize="14" FontAttributes="Bold"
                       TextColor="#111111"/>
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

          <!-- DATE + TOTAL HOURS -->
          <Grid ColumnDefinitions="*,Auto"
                Padding="20,0,20,16">
            <Label Grid.Column="0"
                   Text="{Binding Source={x:Static sys:DateTime.Now}, StringFormat='{0:dddd, d MMMM}'}"
                   FontSize="17" FontAttributes="Bold" TextColor="#ffffff"/>
            <Label Grid.Column="1"
                   Text="{Binding WeeklySummary.TotalHours, StringFormat='{0:F2}h total'}"
                   FontSize="15" TextColor="#777"/>
          </Grid>

          <!-- WEEK STRIP -->
          <ScrollView Orientation="Horizontal"
                      HorizontalScrollBarVisibility="Never"
                      Margin="0,0,0,24">
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

          <!-- CLOCK IN CARD -->
          <Border Margin="20,0,20,12"
                  BackgroundColor="#1a1a1a" Padding="20,20">
            <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
            <Grid ColumnDefinitions="*,Auto">
              <VerticalStackLayout Grid.Column="0" Spacing="6">
                <Label Text="{Binding ClockDuration}"
                       FontSize="22" FontAttributes="Bold"
                       TextColor="#ffffff"
                       IsVisible="{Binding IsClockedIn}"/>
                <Label Text="Not clocked in"
                       FontSize="18" FontAttributes="Bold"
                       TextColor="#777"
                       IsVisible="{Binding IsNotClockedIn}"/>
                <Label Text="{Binding SelectedProject.Name}"
                       FontSize="14" TextColor="#777"/>
              </VerticalStackLayout>
              <Border Grid.Column="1"
                      BackgroundColor="{Binding ClockButtonColor}"
                      WidthRequest="44" HeightRequest="44"
                      VerticalOptions="Center">
                <Border.StrokeShape><RoundRectangle CornerRadius="12"/></Border.StrokeShape>
                <Border.GestureRecognizers>
                  <TapGestureRecognizer Command="{Binding ClockToggleCommand}"/>
                </Border.GestureRecognizers>
                <Label Text="■" FontSize="16" TextColor="#ffffff"
                       HorizontalOptions="Center" VerticalOptions="Center"
                       IsVisible="{Binding IsClockedIn}"/>
                <Label Text="▶" FontSize="16" TextColor="#ffffff"
                       HorizontalOptions="Center" VerticalOptions="Center"
                       IsVisible="{Binding IsNotClockedIn}"/>
              </Border>
            </Grid>
          </Border>

          <!-- PROJECT PICKER (when not clocked in) -->
          <Border Margin="20,0,20,12"
                  BackgroundColor="#1a1a1a" Padding="16,14"
                  IsVisible="{Binding IsNotClockedIn}">
            <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
            <VerticalStackLayout Spacing="6">
              <Label Text="SELECT PROJECT" FontSize="10"
                     FontAttributes="Bold" TextColor="#555"
                     CharacterSpacing="1.5"/>
              <Picker ItemsSource="{Binding Projects}"
                      SelectedItem="{Binding SelectedProject}"
                      ItemDisplayBinding="{Binding Name}"
                      TextColor="#ffffff" TitleColor="#555"
                      Title="Choose a project..."
                      BackgroundColor="Transparent"/>
            </VerticalStackLayout>
          </Border>

          <!-- ERROR -->
          <Label Margin="20,0,20,12"
                 Text="{Binding ErrorMessage}"
                 FontSize="13" TextColor="#ef4444"
                 IsVisible="{Binding HasError}"
                 HorizontalTextAlignment="Center"/>

          <!-- RECENT ENTRIES LABEL -->
          <Grid ColumnDefinitions="*,Auto" Padding="20,8,20,12">
            <Label Grid.Column="0" Text="Recent entries"
                   FontSize="17" FontAttributes="Bold"
                   TextColor="#ffffff"/>
            <Label Grid.Column="1" Text="Total hours"
                   FontSize="14" TextColor="#777"
                   VerticalOptions="Center"/>
          </Grid>

          <!-- ENTRIES LIST -->
          <CollectionView x:Name="TimesheetList"
                          ItemsSource="{Binding Timesheets}"
                          SelectionMode="Single"
                          SelectionChanged="TimesheetList_SelectionChanged"
                          Margin="20,0,20,0">
            <CollectionView.ItemTemplate>
              <DataTemplate>
                <Border BackgroundColor="#1a1a1a"
                        Padding="20,18" Margin="0,0,0,10">
                  <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
                  <VerticalStackLayout Spacing="6">
                    <Grid ColumnDefinitions="*,Auto">
                      <Label Grid.Column="0"
                             Text="{Binding ProjectName}"
                             FontSize="17" FontAttributes="Bold"
                             TextColor="#ffffff"/>
                      <Label Grid.Column="1"
                             Text="{Binding HoursWorked, StringFormat='{0:F2}h'}"
                             FontSize="16" FontAttributes="Bold"
                             TextColor="#ffffff"/>
                    </Grid>
                    <Grid ColumnDefinitions="*,Auto">
                      <Label Grid.Column="0"
                             Text="{Binding Date, StringFormat='Start: {0:h:mm tt}'}"
                             FontSize="13" TextColor="#777"/>
                      <Label Grid.Column="1"
                             Text="{Binding Status}"
                             FontSize="13" TextColor="#555"/>
                    </Grid>
                    <Label Text="Breaktime: None"
                           FontSize="12" TextColor="#444"/>
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
            Margin="0,0,0,32"
            BackgroundColor="#222222" Padding="8,8">
      <Border.StrokeShape><RoundRectangle CornerRadius="36"/></Border.StrokeShape>
      <HorizontalStackLayout Spacing="0">

        <VerticalStackLayout Spacing="4" WidthRequest="64"
                             HorizontalOptions="Center" Padding="0,4">
          <VerticalStackLayout.GestureRecognizers>
            <TapGestureRecognizer Tapped="OnDockDashTapped"/>
          </VerticalStackLayout.GestureRecognizers>
          <Label Text="⌂" FontSize="22" TextColor="#777"
                 HorizontalOptions="Center"/>
        </VerticalStackLayout>

        <Border BackgroundColor="#333" Padding="20,12">
          <Border.StrokeShape><RoundRectangle CornerRadius="28"/></Border.StrokeShape>
          <HorizontalStackLayout Spacing="8">
            <Label Text="📁" FontSize="20"
                   VerticalOptions="Center"/>
            <Label Text="Projects" FontSize="15"
                   FontAttributes="Bold" TextColor="#ffffff"
                   VerticalOptions="Center"/>
          </HorizontalStackLayout>
        </Border>

        <VerticalStackLayout Spacing="4" WidthRequest="64"
                             HorizontalOptions="Center" Padding="0,4">
          <VerticalStackLayout.GestureRecognizers>
            <TapGestureRecognizer Tapped="OnDockInvoicesTapped"/>
          </VerticalStackLayout.GestureRecognizers>
          <Label Text="🚩" FontSize="22" TextColor="#777"
                 HorizontalOptions="Center"/>
        </VerticalStackLayout>

        <VerticalStackLayout Spacing="4" WidthRequest="64"
                             HorizontalOptions="Center" Padding="0,4">
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

# ═══════════════════════════════════════════════════════════════
# DashboardPage.xaml — Procore style
# ═══════════════════════════════════════════════════════════════
DASH_XAML = os.path.join(VIEWS, "DashboardPage.xaml")
DASH_TEXT = """<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="BuildForce.Views.DashboardPage"
             BackgroundColor="#111111"
             Shell.NavBarIsVisible="False"
             Shell.TabBarIsVisible="False">

  <Grid RowDefinitions="*">
    <ScrollView>
      <VerticalStackLayout Spacing="0">

        <!-- TOP BAR -->
        <Grid ColumnDefinitions="*,Auto,Auto"
              Padding="20,56,20,16" BackgroundColor="#111111">
          <Label Grid.Column="0" Text="" />
          <Border Grid.Column="1" BackgroundColor="#222222"
                  WidthRequest="38" HeightRequest="38"
                  Margin="0,0,12,0">
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
                   TextColor="#000000"
                   HorizontalOptions="Center" VerticalOptions="Center"/>
          </Border>
        </Grid>

        <!-- BIG TITLE -->
        <VerticalStackLayout Padding="20,0,20,8" Spacing="6">
          <Label x:Name="GreetingLabel" Text="Good evening"
                 FontSize="15" TextColor="#777"/>
          <Label Text="Mezano Consulting"
                 FontSize="34" FontAttributes="Bold"
                 TextColor="#ffffff" LineBreakMode="WordWrap"/>
          <Border BackgroundColor="#1a1a1a" Padding="12,6"
                  HorizontalOptions="Start" Margin="0,4,0,0">
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
              <Label Text="Projects" FontSize="14" TextColor="#777"/>
            </Border>
            <Border BackgroundColor="#222222" Padding="18,10">
              <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
              <Label Text="Invoices" FontSize="14" TextColor="#777"/>
            </Border>
            <Border BackgroundColor="#222222" Padding="18,10">
              <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
              <Label Text="More" FontSize="14" TextColor="#777"/>
            </Border>
          </HorizontalStackLayout>
        </ScrollView>

        <!-- FINANCIALS CARD -->
        <Border x:Name="HeroCard" Margin="20,0,20,12"
                BackgroundColor="#1a1a1a" Padding="22,20">
          <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
          <VerticalStackLayout Spacing="16">
            <Label Text="FINANCIALS" FontSize="11"
                   FontAttributes="Bold" TextColor="#555"
                   CharacterSpacing="2"/>
            <Grid ColumnDefinitions="*,Auto">
              <VerticalStackLayout Grid.Column="0" Spacing="4">
                <Label Text="Outstanding" FontSize="13"
                       TextColor="#777"/>
                <Label x:Name="UnpaidAmountLabel" Text="$0.00"
                       FontSize="38" FontAttributes="Bold"
                       TextColor="#ffffff"/>
              </VerticalStackLayout>
              <Border Grid.Column="1"
                      BackgroundColor="#2a1800"
                      Padding="12,8" VerticalOptions="Center">
                <Border.StrokeShape><RoundRectangle CornerRadius="10"/></Border.StrokeShape>
                <Label x:Name="OverdueCountLabel"
                       Text="0 Overdue" FontSize="12"
                       TextColor="#f0a500" FontAttributes="Bold"/>
              </Border>
            </Grid>
            <BoxView HeightRequest="1" BackgroundColor="#2a2a2a"/>
            <Grid ColumnDefinitions="*,*" ColumnSpacing="20">
              <VerticalStackLayout Grid.Column="0" Spacing="4">
                <Label Text="PAID" FontSize="10"
                       FontAttributes="Bold" TextColor="#555"
                       CharacterSpacing="1.5"/>
                <Label x:Name="PaidAmountLabel" Text="$0.00"
                       FontSize="22" FontAttributes="Bold"
                       TextColor="#22c55e"/>
              </VerticalStackLayout>
              <VerticalStackLayout Grid.Column="1" Spacing="4">
                <Label Text="TOTAL" FontSize="10"
                       FontAttributes="Bold" TextColor="#555"
                       CharacterSpacing="1.5"/>
                <Label x:Name="TotalOutstandingLabel" Text="$0.00"
                       FontSize="22" FontAttributes="Bold"
                       TextColor="#ffffff"/>
              </VerticalStackLayout>
            </Grid>
          </VerticalStackLayout>
        </Border>

        <!-- STATS ROW -->
        <Grid Margin="20,0,20,20"
              ColumnDefinitions="*,*,*" ColumnSpacing="10">
          <Border Grid.Column="0" BackgroundColor="#1a1a1a"
                  Padding="0,18">
            <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
            <VerticalStackLayout Spacing="4" HorizontalOptions="Center">
              <Label x:Name="ActiveJobsLabel" Text="0"
                     FontSize="30" FontAttributes="Bold"
                     TextColor="#0ea5e9"
                     HorizontalOptions="Center"/>
              <Label Text="Active" FontSize="12"
                     TextColor="#555"
                     HorizontalOptions="Center"/>
            </VerticalStackLayout>
          </Border>
          <Border Grid.Column="1" BackgroundColor="#1a1a1a"
                  Padding="0,18">
            <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
            <VerticalStackLayout Spacing="4" HorizontalOptions="Center">
              <Label x:Name="NeedsAttentionLabel" Text="0"
                     FontSize="30" FontAttributes="Bold"
                     TextColor="#ef4444"
                     HorizontalOptions="Center"/>
              <Label Text="Urgent" FontSize="12"
                     TextColor="#555"
                     HorizontalOptions="Center"/>
            </VerticalStackLayout>
          </Border>
          <Border Grid.Column="2" BackgroundColor="#1a1a1a"
                  Padding="0,18">
            <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
            <VerticalStackLayout Spacing="4" HorizontalOptions="Center">
              <Label x:Name="EstimatesLabel" Text="0"
                     FontSize="30" FontAttributes="Bold"
                     TextColor="#8b5cf6"
                     HorizontalOptions="Center"/>
              <Label Text="Pending" FontSize="12"
                     TextColor="#555"
                     HorizontalOptions="Center"/>
            </VerticalStackLayout>
          </Border>
        </Grid>

        <!-- QUICK ACTIONS -->
        <Label Text="Quick actions" Margin="20,0,20,12"
               FontSize="17" FontAttributes="Bold"
               TextColor="#ffffff"/>

        <Grid Margin="20,0,20,20"
              ColumnDefinitions="*,*" ColumnSpacing="12"
              RowDefinitions="Auto,Auto" RowSpacing="12">

          <Border Grid.Row="0" Grid.Column="0"
                  BackgroundColor="#1a1a1a" Padding="18,16">
            <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
            <Border.GestureRecognizers>
              <TapGestureRecognizer Tapped="OnTimeClockTapped"/>
            </Border.GestureRecognizers>
            <VerticalStackLayout Spacing="10">
              <Label Text="⏱" FontSize="28"/>
              <Label Text="Time Clock" FontSize="15"
                     FontAttributes="Bold" TextColor="#ffffff"/>
              <Label Text="GPS verified" FontSize="12"
                     TextColor="#555"/>
            </VerticalStackLayout>
          </Border>

          <Border Grid.Row="0" Grid.Column="1"
                  BackgroundColor="#1a1a1a" Padding="18,16">
            <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
            <Border.GestureRecognizers>
              <TapGestureRecognizer Tapped="OnNewInvoiceTapped"/>
            </Border.GestureRecognizers>
            <VerticalStackLayout Spacing="10">
              <Label Text="🧾" FontSize="28"/>
              <Label Text="New Invoice" FontSize="15"
                     FontAttributes="Bold" TextColor="#ffffff"/>
              <Label Text="60 seconds" FontSize="12"
                     TextColor="#555"/>
            </VerticalStackLayout>
          </Border>

          <Border Grid.Row="1" Grid.Column="0"
                  BackgroundColor="#1a1a1a" Padding="18,16">
            <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
            <Border.GestureRecognizers>
              <TapGestureRecognizer Tapped="OnScanReceiptTapped"/>
            </Border.GestureRecognizers>
            <VerticalStackLayout Spacing="10">
              <Label Text="📷" FontSize="28"/>
              <Label Text="Scan Receipt" FontSize="15"
                     FontAttributes="Bold" TextColor="#ffffff"/>
              <Label Text="AI auto-fill" FontSize="12"
                     TextColor="#555"/>
            </VerticalStackLayout>
          </Border>

          <Border Grid.Row="1" Grid.Column="1"
                  BackgroundColor="#1a1a1a" Padding="18,16">
            <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
            <Border.GestureRecognizers>
              <TapGestureRecognizer Tapped="OnCollectPayTapped"/>
            </Border.GestureRecognizers>
            <VerticalStackLayout Spacing="10">
              <Label Text="💳" FontSize="28"/>
              <Label Text="Collect Pay" FontSize="15"
                     FontAttributes="Bold" TextColor="#ffffff"/>
              <Label Text="On-site" FontSize="12"
                     TextColor="#555"/>
            </VerticalStackLayout>
          </Border>
        </Grid>

        <!-- ACTIVE JOBS -->
        <Grid ColumnDefinitions="*,Auto" Margin="20,0,20,12">
          <Label Grid.Column="0" Text="Active jobs"
                 FontSize="17" FontAttributes="Bold"
                 TextColor="#ffffff"/>
          <Label Grid.Column="1" Text="See all →"
                 FontSize="14" TextColor="#777">
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
                    <Label Text="{Binding Name}"
                           FontSize="16" FontAttributes="Bold"
                           TextColor="#ffffff"/>
                    <Label Text="{Binding CustomerName}"
                           FontSize="13" TextColor="#777"/>
                  </VerticalStackLayout>
                  <VerticalStackLayout Grid.Column="1"
                                       HorizontalOptions="End"
                                       Spacing="6">
                    <Label Text="{Binding BudgetFormatted}"
                           FontSize="15" FontAttributes="Bold"
                           TextColor="#ffffff"
                           HorizontalOptions="End"/>
                    <Border BackgroundColor="#2a2a2a" Padding="8,4">
                      <Border.StrokeShape>
                        <RoundRectangle CornerRadius="8"/>
                      </Border.StrokeShape>
                      <Label Text="{Binding Status}"
                             FontSize="11" TextColor="#777"
                             FontAttributes="Bold"/>
                    </Border>
                  </VerticalStackLayout>
                </Grid>
              </Border>
            </DataTemplate>
          </CollectionView.ItemTemplate>
        </CollectionView>

        <ActivityIndicator x:Name="LoadingIndicator"
                           IsRunning="False" IsVisible="False"
                           Color="#f0a500"
                           HorizontalOptions="Center"
                           Margin="0,10,0,0"/>

        <BoxView HeightRequest="120" Color="Transparent"/>
      </VerticalStackLayout>
    </ScrollView>

    <!-- BOTTOM PILL DOCK -->
    <Border VerticalOptions="End" HorizontalOptions="Center"
            Margin="0,0,0,32"
            BackgroundColor="#222222" Padding="8,8">
      <Border.StrokeShape><RoundRectangle CornerRadius="36"/></Border.StrokeShape>
      <HorizontalStackLayout Spacing="0">

        <Border BackgroundColor="#333" Padding="16,12"
                Margin="0,0,4,0">
          <Border.StrokeShape><RoundRectangle CornerRadius="28"/></Border.StrokeShape>
          <HorizontalStackLayout Spacing="6">
            <Label Text="⌂" FontSize="20" TextColor="#ffffff"
                   VerticalOptions="Center"/>
            <Label Text="Home" FontSize="14"
                   FontAttributes="Bold" TextColor="#ffffff"
                   VerticalOptions="Center"/>
          </HorizontalStackLayout>
        </Border>

        <VerticalStackLayout WidthRequest="60"
                             HorizontalOptions="Center"
                             VerticalOptions="Center">
          <VerticalStackLayout.GestureRecognizers>
            <TapGestureRecognizer Tapped="OnDockProjectsTapped"/>
          </VerticalStackLayout.GestureRecognizers>
          <Label Text="📁" FontSize="22" TextColor="#777"
                 HorizontalOptions="Center"/>
        </VerticalStackLayout>

        <VerticalStackLayout WidthRequest="60"
                             HorizontalOptions="Center"
                             VerticalOptions="Center">
          <VerticalStackLayout.GestureRecognizers>
            <TapGestureRecognizer Tapped="OnDockInvoicesTapped"/>
          </VerticalStackLayout.GestureRecognizers>
          <Label Text="🚩" FontSize="22" TextColor="#777"
                 HorizontalOptions="Center"/>
        </VerticalStackLayout>

        <VerticalStackLayout WidthRequest="60"
                             HorizontalOptions="Center"
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

DASH_CS = os.path.join(VIEWS, "DashboardPage.xaml.cs")
DASH_CS_TEXT = """#pragma warning disable CA1416
using BuildForce.ViewModels;

namespace BuildForce.Views;

public partial class DashboardPage : ContentPage
{
    readonly DashboardViewModel _vm;
    int _themeIndex = 0;
    readonly string[] _themes = { "D", "N", "L" };
    readonly string[] _pageBg = { "#111111", "#0a1628", "#f5f5f5" };
    readonly string[] _cardBg = { "#1a1a1a", "#0f2040", "#ffffff" };

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
        _themeIndex = (_themeIndex + 1) % _themes.Length;
        ThemeLabel.Text = _themes[_themeIndex];
        BackgroundColor = Color.FromArgb(_pageBg[_themeIndex]);
        HeroCard.BackgroundColor = Color.FromArgb(_cardBg[_themeIndex]);
    }

    async void OnNewInvoiceTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("CreateInvoicePage");
    async void OnScanReceiptTapped(object sender, EventArgs e)
        => await Shell.Current.DisplayAlert("Scan Receipt", "Coming soon!", "OK");
    async void OnTimeClockTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//TimesheetPage");
    async void OnCollectPayTapped(object sender, EventArgs e)
        => await Shell.Current.DisplayAlert("Collect Payment", "Select an invoice.", "OK");
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
    }

    private async void TimesheetList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not TimesheetDto selected)
            return;
        ((CollectionView)sender).SelectedItem = null;
        await Shell.Current.GoToAsync($"TimesheetDetailPage?timesheetId={selected.TimesheetId}");
    }

    async void OnDockDashTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//DashboardPage");
    async void OnDockInvoicesTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//InvoicesPage");
    async void OnDockMoreTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//MorePage");
}
"""

write(TS_XAML,  TS_TEXT)
write(DASH_XAML, DASH_TEXT)
write(DASH_CS,   DASH_CS_TEXT)
write(TS_CS,     TS_CS_TEXT)
print("\\nDone! 4 files written. Build and deploy to Samsung.")