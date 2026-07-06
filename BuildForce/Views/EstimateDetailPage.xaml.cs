#pragma warning disable CA1416
using BuildForce.Services;
using Microsoft.Maui.Controls.Shapes;

namespace BuildForce.Views;

public partial class EstimateDetailPage : ContentPage
{
    private readonly ApiService _api;
    private readonly int _id;

    public EstimateDetailPage(ApiService api, int id)
    {
        InitializeComponent();
        _api = api;
        _id = id;
        LoadDetail();
    }

    private async void LoadDetail()
    {
        try
        {
            var d = await _api.GetEstimateDetailAsync(_id);
            if (d == null)
            {
                TitleLabel.Text = "Could not load estimate";
                await DisplayAlert("Error", "Could not load estimate details.", "OK");
                return;
            }

            TitleLabel.Text = d.EstimateNumber;
            StatusLabel.Text = d.Status;
            MetaLabel.Text = d.ValidUntil.HasValue ? $"Valid until {d.ValidUntil:MMM d, yyyy}" : "";

            string pillText, pillBg;
            switch (d.Status)
            {
                case "Approved":
                case "Converted": pillText = "#0d7a4f"; pillBg = "#d8f5e8"; break;
                case "Rejected":
                case "Expired":   pillText = "#b3261e"; pillBg = "#fde3e1"; break;
                case "Sent":      pillText = "#1e50a0"; pillBg = "#dde9fb"; break;
                default:          pillText = "#5b6472"; pillBg = "#e8ecf3"; break;
            }
            StatusPill.BackgroundColor = Color.FromArgb(pillBg);
            StatusLabel.TextColor = Color.FromArgb(pillText);

            ProjectLabel.Text = string.IsNullOrWhiteSpace(d.ProjectName) ? "\u2014" : d.ProjectName;
            CustomerLabel.Text = string.IsNullOrWhiteSpace(d.CustomerName) ? "\u2014" : d.CustomerName;
            DateLabel.Text = d.EstimateDate.ToString("MMM d, yyyy");
            ValidLabel.Text = d.ValidUntil?.ToString("MMM d, yyyy") ?? "\u2014";
            NotesLabel.Text = string.IsNullOrWhiteSpace(d.Notes) ? "\u2014" : d.Notes;

            ItemsList.Children.Clear();
            if (d.LineItems != null)
            {
                foreach (var li in d.LineItems)
                {
                    var rowGrid = new Grid
                    {
                        ColumnDefinitions =
                        {
                            new ColumnDefinition { Width = GridLength.Star },
                            new ColumnDefinition { Width = GridLength.Auto }
                        }
                    };
                    var left = new VerticalStackLayout { Spacing = 1 };
                    left.Add(new Label { Text = li.Description, FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#1a2340"), LineBreakMode = LineBreakMode.WordWrap });
                    left.Add(new Label { Text = $"{li.Quantity:0.##} \u00D7 {li.UnitPrice:C2}", FontSize = 11, TextColor = Color.FromArgb("#8a93a8") });
                    var amt = new Label { Text = li.Amount.ToString("C2"), FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#1a2340"), VerticalOptions = LayoutOptions.Center };
                    Grid.SetColumn(left, 0);
                    Grid.SetColumn(amt, 1);
                    rowGrid.Children.Add(left);
                    rowGrid.Children.Add(amt);

                    ItemsList.Children.Add(new Border
                    {
                        BackgroundColor = Color.FromArgb("#f7f9fd"),
                        Stroke = Color.FromArgb("#e2e7f0"),
                        StrokeThickness = 1,
                        StrokeShape = new RoundRectangle { CornerRadius = 10 },
                        Padding = new Thickness(12, 9),
                        Content = rowGrid
                    });
                }
            }

            SubtotalLabel.Text = d.Subtotal.ToString("C2");
            DiscountLabel.Text = "-" + d.DiscountAmount.ToString("C2");
            TaxLabel.Text = d.TaxAmount.ToString("C2");
            TotalLabel.Text = d.TotalAmount.ToString("C2");

            _customerEmail = null;
            _docNumber = d.EstimateNumber;
            DeliveryRow.IsVisible = true;
            InfoCard.IsVisible = true;
            ItemsCard.IsVisible = true;
            TotalsCard.IsVisible = true;
        }
        catch (Exception ex)
        {
            TitleLabel.Text = "Error";
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            Loading.IsRunning = false;
            Loading.IsVisible = false;
        }
    }

    private string? _customerEmail;
    private string? _docNumber;
    private bool _delivering = false;

    private async void OnEmail(object sender, TappedEventArgs e)
    {
        if (_delivering) return;
        try
        {
            var target = await DisplayPromptAsync("Email Estimate",
                "Send to:", "Send", "Cancel",
                initialValue: _customerEmail ?? "", keyboard: Keyboard.Email);
            if (string.IsNullOrWhiteSpace(target)) return;

            _delivering = true;
            DeliveryStatus.IsVisible = true;
            DeliveryStatus.Text = "Sending email with PDF attached...";
            DeliveryStatus.TextColor = Color.FromArgb("#b45309");

            var msg = await _api.EmailEstimateAsync(_id, target.Trim());
            _delivering = false;

            if (msg != null)
            {
                DeliveryStatus.Text = "Email sent!";
                DeliveryStatus.TextColor = Color.FromArgb("#0d7a4f");
                await DisplayAlert("Sent", msg, "OK");
                LoadDetail();
            }
            else
            {
                DeliveryStatus.Text = "Email failed.";
                DeliveryStatus.TextColor = Color.FromArgb("#b3261e");
                await DisplayAlert("Email Failed", _api.LastError ?? "Could not send. Try again.", "OK");
            }
        }
        catch (Exception ex)
        {
            _delivering = false;
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnDownloadPdf(object sender, TappedEventArgs e)
    {
        if (_delivering) return;
        try
        {
            _delivering = true;
            DeliveryStatus.IsVisible = true;
            DeliveryStatus.Text = "Generating PDF...";
            DeliveryStatus.TextColor = Color.FromArgb("#b45309");

            var bytes = await _api.GetEstimatePdfAsync(_id);
            _delivering = false;

            if (bytes == null)
            {
                DeliveryStatus.Text = "PDF failed.";
                DeliveryStatus.TextColor = Color.FromArgb("#b3261e");
                await DisplayAlert("PDF Failed", _api.LastError ?? "Could not generate PDF.", "OK");
                return;
            }

            var fileName = $"Estimate_{_docNumber ?? _id.ToString()}.pdf";
            var path = System.IO.Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllBytesAsync(path, bytes);

            DeliveryStatus.Text = "PDF ready - choose where to share it.";
            DeliveryStatus.TextColor = Color.FromArgb("#0d7a4f");

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = fileName,
                File = new ShareFile(path)
            });
        }
        catch (Exception ex)
        {
            _delivering = false;
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnBack(object sender, TappedEventArgs e)
    {
        try
        {
            await Application.Current!.MainPage!.Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Back error: {ex}");
        }
    }
}