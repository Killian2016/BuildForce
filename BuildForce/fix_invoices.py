import os

ROOT  = r"C:\Users\mezan\source\repos\BuildForce\BuildForce"
VIEWS = os.path.join(ROOT, "Views")

def write(path, text):
    with open(path, "wb") as f:
        f.write(text.encode("utf-8"))
    print("OK  " + path)

INV_XAML = os.path.join(VIEWS, "InvoicesPage.xaml")
INV_TEXT = """<?xml version="1.0" encoding="utf-8" ?>
<ContentPage
    x:Class="BuildForce.Views.InvoicesPage"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    Title="Invoices"
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
          <Grid ColumnDefinitions="Auto,*,Auto"
                Padding="20,56,20,16" BackgroundColor="#111111">
            <Border Grid.Column="0" BackgroundColor="#222222"
                    WidthRequest="38" HeightRequest="38">
              <Border.StrokeShape><RoundRectangle CornerRadius="12"/></Border.StrokeShape>
              <Border.GestureRecognizers>
                <TapGestureRecognizer Tapped="OnBackTapped"/>
              </Border.GestureRecognizers>
              <Label Text="&lt;" FontSize="18" FontAttributes="Bold"
                     TextColor="#ffffff"
                     HorizontalOptions="Center" VerticalOptions="Center"/>
            </Border>
            <Label Grid.Column="1" Text="Invoices"
                   FontSize="18" FontAttributes="Bold"
                   TextColor="#ffffff" HorizontalOptions="Center"
                   VerticalOptions="Center"/>
            <Border Grid.Column="2" BackgroundColor="#f0a500"
                    WidthRequest="38" HeightRequest="38">
              <Border.StrokeShape><RoundRectangle CornerRadius="12"/></Border.StrokeShape>
              <Border.GestureRecognizers>
                <TapGestureRecognizer Command="{Binding NewInvoiceCommand}"/>
              </Border.GestureRecognizers>
              <Label Text="+" FontSize="22" FontAttributes="Bold"
                     TextColor="#000000"
                     HorizontalOptions="Center" VerticalOptions="Center"/>
            </Border>
          </Grid>

          <!-- BIG TITLE -->
          <Label Text="Invoices" Margin="20,0,20,20"
                 FontSize="34" FontAttributes="Bold"
                 TextColor="#ffffff"/>

          <!-- SUMMARY CARDS -->
          <Grid ColumnDefinitions="*,*,*" Margin="20,0,20,16"
                ColumnSpacing="10">
            <Border Grid.Column="0" BackgroundColor="#1a1a1a"
                    Padding="14,12">
              <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
              <VerticalStackLayout Spacing="4">
                <Label Text="OUTSTANDING" FontSize="8"
                       FontAttributes="Bold" TextColor="#f0a500"
                       CharacterSpacing="1"/>
                <Label Text="{Binding TotalOutstanding, StringFormat='{0:C0}'}"
                       FontSize="16" FontAttributes="Bold"
                       TextColor="#ffffff"/>
              </VerticalStackLayout>
            </Border>
            <Border Grid.Column="1" BackgroundColor="#1a1a1a"
                    Padding="14,12">
              <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
              <VerticalStackLayout Spacing="4">
                <Label Text="PAID" FontSize="8"
                       FontAttributes="Bold" TextColor="#22c55e"
                       CharacterSpacing="1"/>
                <Label Text="{Binding TotalPaid, StringFormat='{0:C0}'}"
                       FontSize="16" FontAttributes="Bold"
                       TextColor="#ffffff"/>
              </VerticalStackLayout>
            </Border>
            <Border Grid.Column="2" BackgroundColor="#1a1a1a"
                    Padding="14,12">
              <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
              <VerticalStackLayout Spacing="4">
                <Label Text="OVERDUE" FontSize="8"
                       FontAttributes="Bold" TextColor="#ef4444"
                       CharacterSpacing="1"/>
                <Label Text="{Binding TotalOverdue, StringFormat='{0:C0}'}"
                       FontSize="16" FontAttributes="Bold"
                       TextColor="#ffffff"/>
              </VerticalStackLayout>
            </Border>
          </Grid>

          <!-- FILTER CHIPS -->
          <ScrollView Orientation="Horizontal"
                      HorizontalScrollBarVisibility="Never"
                      Margin="0,0,0,16">
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
                  <TapGestureRecognizer Command="{Binding FilterCommand}" CommandParameter="Draft"/>
                </Border.GestureRecognizers>
                <Label Text="Draft" FontSize="14" TextColor="#777"/>
              </Border>
              <Border BackgroundColor="#222222" Padding="18,10">
                <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
                <Border.GestureRecognizers>
                  <TapGestureRecognizer Command="{Binding FilterCommand}" CommandParameter="Sent"/>
                </Border.GestureRecognizers>
                <Label Text="Sent" FontSize="14" TextColor="#0ea5e9"/>
              </Border>
              <Border BackgroundColor="#222222" Padding="18,10">
                <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
                <Border.GestureRecognizers>
                  <TapGestureRecognizer Command="{Binding FilterCommand}" CommandParameter="Paid"/>
                </Border.GestureRecognizers>
                <Label Text="Paid" FontSize="14" TextColor="#22c55e"/>
              </Border>
              <Border BackgroundColor="#222222" Padding="18,10">
                <Border.StrokeShape><RoundRectangle CornerRadius="20"/></Border.StrokeShape>
                <Border.GestureRecognizers>
                  <TapGestureRecognizer Command="{Binding FilterCommand}" CommandParameter="Overdue"/>
                </Border.GestureRecognizers>
                <Label Text="Overdue" FontSize="14" TextColor="#ef4444"/>
              </Border>
            </HorizontalStackLayout>
          </ScrollView>

          <!-- NEW INVOICE BUTTON -->
          <Border Margin="20,0,20,20" BackgroundColor="#f0a500"
                  Padding="0,16">
            <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
            <Border.GestureRecognizers>
              <TapGestureRecognizer Command="{Binding NewInvoiceCommand}"/>
            </Border.GestureRecognizers>
            <Label Text="+ New Invoice" FontSize="16"
                   FontAttributes="Bold" TextColor="#000000"
                   HorizontalOptions="Center"/>
          </Border>

          <!-- INVOICE LIST -->
          <CollectionView ItemsSource="{Binding Invoices}"
                          SelectionMode="None" Margin="20,0">
            <CollectionView.ItemTemplate>
              <DataTemplate>
                <Border BackgroundColor="#1a1a1a"
                        Padding="20,18" Margin="0,0,0,10">
                  <Border.StrokeShape><RoundRectangle CornerRadius="16"/></Border.StrokeShape>
                  <Border.GestureRecognizers>
                    <TapGestureRecognizer
                        Command="{Binding Source={RelativeSource AncestorType={x:Type ContentPage}}, Path=BindingContext.SelectCommand}"
                        CommandParameter="{Binding .}"/>
                  </Border.GestureRecognizers>
                  <VerticalStackLayout Spacing="6">
                    <Grid ColumnDefinitions="*,Auto">
                      <Label Grid.Column="0"
                             Text="{Binding InvoiceNumber}"
                             FontSize="16" FontAttributes="Bold"
                             TextColor="#f0a500"/>
                      <Label Grid.Column="1"
                             Text="{Binding TotalAmount, StringFormat='{0:C2}'}"
                             FontSize="17" FontAttributes="Bold"
                             TextColor="#ffffff"/>
                    </Grid>
                    <Grid ColumnDefinitions="*,Auto">
                      <Label Grid.Column="0"
                             Text="{Binding CustomerName}"
                             FontSize="13" TextColor="#777"/>
                      <Label Grid.Column="1"
                             Text="{Binding Status}"
                             FontSize="13" FontAttributes="Bold"
                             TextColor="#f0a500"/>
                    </Grid>
                    <Grid ColumnDefinitions="*,Auto">
                      <Label Grid.Column="0"
                             Text="{Binding ProjectName}"
                             FontSize="12" TextColor="#555"/>
                      <Label Grid.Column="1"
                             Text="{Binding DueDate, StringFormat='Due {0:MMM d}'}"
                             FontSize="12" TextColor="#555"/>
                    </Grid>
                  </VerticalStackLayout>
                </Border>
              </DataTemplate>
            </CollectionView.ItemTemplate>
            <CollectionView.EmptyView>
              <VerticalStackLayout HorizontalOptions="Center"
                                   Margin="0,40" Spacing="12">
                <Label Text="🧾" FontSize="48"
                       HorizontalOptions="Center"/>
                <Label Text="No invoices found"
                       TextColor="#555" FontSize="15"
                       HorizontalOptions="Center"/>
              </VerticalStackLayout>
            </CollectionView.EmptyView>
          </CollectionView>

          <BoxView HeightRequest="80" Color="Transparent"/>
        </VerticalStackLayout>
      </ScrollView>
    </RefreshView>
  </Grid>
</ContentPage>
"""

INV_CS = os.path.join(VIEWS, "InvoicesPage.xaml.cs")
INV_CS_TEXT = """#pragma warning disable CA1416
using BuildForce.ViewModels;

namespace BuildForce.Views;

public partial class InvoicesPage : ContentPage
{
    readonly InvoicesViewModel _vm;

    public InvoicesPage(InvoicesViewModel vm)
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
}
"""

write(INV_XAML, INV_TEXT)
write(INV_CS,   INV_CS_TEXT)
print("\\nDone! Build and deploy.")