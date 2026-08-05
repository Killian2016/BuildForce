#pragma warning disable CA1416
using BuildForce.Services;

namespace BuildForce.Views;

public partial class ProjectDetailPage : ContentPage
{
    private readonly ApiService _api;
    private readonly int _projectId;

    public ProjectDetailPage(ApiService api, int projectId)
    {
        InitializeComponent();
        _api = api;
        _projectId = projectId;
        LoadDetail();
    }

    private async void LoadDetail()
    {
        try
        {
            var d = await _api.GetProjectDetailAsync(_projectId);
            if (d == null)
            {
                TitleLabel.Text = "Could not load project";
                await DisplayAlert("Error", "Could not load project details. Check your connection and try again.", "OK");
                return;
            }

            TitleLabel.Text = d.Name;
            CustomerLabel.Text = d.CustomerName ?? "";
            StatusLabel.Text = d.Status;

            string pillText, pillBg;
            switch (d.Status)
            {
                case "Active":
                case "In Progress": pillText = "#3fb950"; pillBg = "#12261a"; break;
                case "Planning":    pillText = "#f0a500"; pillBg = "#2a2008"; break;
                case "On Hold":     pillText = "#f0883e"; pillBg = "#2b1607"; break;
                case "Completed":   pillText = "#58a6ff"; pillBg = "#0d1f33"; break;
                default:            pillText = "#7d8590"; pillBg = "#161b22"; break;
            }
            StatusPill.BackgroundColor = Color.FromArgb(pillBg);
            StatusLabel.TextColor = Color.FromArgb(pillText);

            InvoicedLabel.Text = d.TotalInvoiced.ToString("C0");
            InvoiceCountLabel.Text = $"{d.InvoiceCount} invoice{(d.InvoiceCount == 1 ? "" : "s")}";
            ExpensesLabel.Text = d.TotalExpenses.ToString("C0");
            ExpenseCountLabel.Text = $"{d.ExpenseCount} expense{(d.ExpenseCount == 1 ? "" : "s")}";

            LocationLabel.Text = string.IsNullOrWhiteSpace(d.Location) ? "\u2014" : d.Location;
            DescriptionLabel.Text = string.IsNullOrWhiteSpace(d.Description) ? "\u2014" : d.Description;
            BudgetLabel.Text = d.Budget > 0 ? d.Budget.ToString("C0") : "\u2014";

            var start = d.StartDate?.ToString("MMM d, yyyy") ?? "\u2014";
            var end = d.EndDate?.ToString("MMM d, yyyy") ?? "ongoing";
            DatesLabel.Text = $"{start} \u2192 {end}";

            MoneyRow.IsVisible = true;
            InfoCard.IsVisible = true;
            ActionRow.IsVisible = true;
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
            if (Navigation.ModalStack.Count > 0)
                await Navigation.PopModalAsync();
            else if (Navigation.NavigationStack.Count > 1)
                await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Back error: {ex}");
        }
    }

    private async void OnNewInvoice(object sender, TappedEventArgs e)
    {
        try
        {
            await Application.Current!.MainPage!.Navigation.PushModalAsync(new InvoiceCreatePage(_api));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Navigation Error", ex.Message, "OK");
        }
    }

    private async void OnNewExpense(object sender, TappedEventArgs e)
    {
        try
        {
            await Application.Current!.MainPage!.Navigation.PushModalAsync(new ExpenseCreatePage(_api));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Navigation Error", ex.Message, "OK");
        }
    }
}