#pragma warning disable CA1416
using BuildForce.Services;

namespace BuildForce.Views;

public partial class InvoicesPage : ContentPage
{
    private readonly ApiService _api;

    public InvoicesPage()
    {
        InitializeComponent();
        _api = new ApiService();
        LoadInvoices();
    }

    private async void LoadInvoices()
    {
        try
        {
            var invoices = await _api.GetInvoicesAsync();
            InvoiceList.Children.Clear();

            foreach (var inv in invoices)
            {
                InvoiceList.Children.Add(BuildInvoiceRow(inv));
            }

            // Add create button at bottom
            var createBtn = new Border
            {
                BackgroundColor = Color.FromArgb("#c8e63c"),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
                Stroke = Colors.Transparent,
                Margin = new Thickness(0, 8, 0, 0)
            };
            var createLbl = new Label
            {
                Text = "+ Create New Invoice",
                FontSize = 15,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Center,
                Padding = new Thickness(0, 16)
            };
            createLbl.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await OnCreateInvoiceAsync()) });
            createBtn.Content = createLbl;
            InvoiceList.Children.Add(createBtn);
        }
        catch { }
        finally
        {
            Loading.IsRunning = false;
            Loading.IsVisible = false;
        }
    }

    private View BuildInvoiceRow(InvoiceSummary inv)
    {
        var isPaid = inv.Status == "Paid";
        var amountColor = isPaid ? "#22c55e" : inv.Status == "Overdue" ? "#ef4444" : "#f59e0b";
        var badgeColor = isPaid ? "#22c55e" : inv.Status == "Overdue" ? "#ef4444" : "#f59e0b";
        var badgeBg = isPaid ? "#0d2e1a" : inv.Status == "Overdue" ? "#2e0d0d" : "#2e1f0d";

        var row = new Border
        {
            BackgroundColor = Color.FromArgb("#111f38"),
            Stroke = Color.FromArgb("#1e3a5f"),
            StrokeThickness = 1,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 },
            Padding = new Thickness(16, 14)
        };

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        var left = new VerticalStackLayout { Spacing = 2 };
        left.Add(new Label { Text = $"#{inv.Number} · {inv.Date:MMM d, yyyy}", FontSize = 12, TextColor = Color.FromArgb("#6b7280"), FontAttributes = FontAttributes.Bold });
        left.Add(new Label { Text = inv.Project, FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Colors.White });
        left.Add(new Label { Text = inv.Client, FontSize = 11, TextColor = Color.FromArgb("#6b7280") });

        var right = new VerticalStackLayout { Spacing = 4, HorizontalOptions = LayoutOptions.End };
        right.Add(new Label { Text = inv.Amount.ToString("C0"), FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb(amountColor), HorizontalOptions = LayoutOptions.End });
        right.Add(new Border
        {
            BackgroundColor = Color.FromArgb(badgeBg),
            Stroke = Color.FromArgb(badgeColor),
            StrokeThickness = 1,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 20 },
            Padding = new Thickness(8, 3),
            Content = new Label { Text = inv.Status, FontSize = 10, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb(badgeColor) }
        });

        Grid.SetColumn(left, 0);
        Grid.SetColumn(right, 1);
        grid.Children.Add(left);
        grid.Children.Add(right);
        row.Content = grid;
        return row;
    }

    private void OnFilterAll(object sender, TappedEventArgs e) { }
    private void OnFilterPaid(object sender, TappedEventArgs e) { }
    private void OnFilterPending(object sender, TappedEventArgs e) { }
    private void OnFilterOverdue(object sender, TappedEventArgs e) { }

    private async void OnCreateInvoice(object sender, TappedEventArgs e)
        => await OnCreateInvoiceAsync();

    private async Task OnCreateInvoiceAsync()
        => await Browser.OpenAsync("https://mezanoconstructionmanagementplatform.com/Invoices/Create");
}