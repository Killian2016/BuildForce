#pragma warning disable CA1416
using BuildForce.Services;

namespace BuildForce.Views;

public partial class ExpenseDetailPage : ContentPage
{
    private readonly ApiService _api;
    private readonly int _id;

    public ExpenseDetailPage(ApiService api, int id)
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
            var d = await _api.GetExpenseDetailAsync(_id);
            if (d == null)
            {
                TitleLabel.Text = "Could not load expense";
                await DisplayAlert("Error", "Could not load expense details.", "OK");
                return;
            }

            TitleLabel.Text = d.Description;
            AmountLabel.Text = d.Amount.ToString("C2");
            DateLabel.Text = d.ExpenseDate.ToString("MMM d, yyyy");
            CategoryLabel.Text = string.IsNullOrWhiteSpace(d.Category) ? "\u2014" : d.Category;
            VendorLabel.Text = string.IsNullOrWhiteSpace(d.Vendor) ? "\u2014" : d.Vendor;
            ProjectLabel.Text = string.IsNullOrWhiteSpace(d.ProjectName) ? "\u2014" : d.ProjectName;
            ReceiptLabel.Text = d.HasReceipt ? "Attached \u2713" : "None";
            ReceiptLabel.TextColor = d.HasReceipt ? Color.FromArgb("#0d7a4f") : Color.FromArgb("#8a93a8");
            NotesLabel.Text = string.IsNullOrWhiteSpace(d.Notes) ? "\u2014" : d.Notes;

            InfoCard.IsVisible = true;
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