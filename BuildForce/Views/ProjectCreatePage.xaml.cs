#pragma warning disable CA1416
using BuildForce.Services;

namespace BuildForce.Views;

public partial class ProjectCreatePage : ContentPage
{
    private readonly ApiService _api;
    private List<CustomerSummary> _customers = new();
    private readonly List<string> _statuses = new()
    {
        "Planning", "Active", "In Progress", "On Hold", "Completed", "Cancelled"
    };
    private bool _saving = false;

    public ProjectCreatePage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        StartDatePicker.Date = DateTime.Today;
        EndDatePicker.Date = DateTime.Today.AddDays(30);
        foreach (var s in _statuses) StatusPicker.Items.Add(s);
        StatusPicker.SelectedIndex = 0;
        LoadCustomers();
    }

    private async void LoadCustomers()
    {
        try
        {
            _customers = await _api.GetCustomersAsync();
            CustomerPicker.Items.Clear();
            CustomerPicker.Items.Add("Select Customer");
            foreach (var c in _customers)
                CustomerPicker.Items.Add(string.IsNullOrWhiteSpace(c.Company) ? c.Name : $"{c.Name} ({c.Company})");
            CustomerPicker.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not load customers: {ex.Message}", "OK");
        }
    }

    private void OnEndDateToggle(object sender, CheckedChangedEventArgs e)
    {
        EndDatePicker.IsEnabled = e.Value;
    }

    private async Task ClosePageAsync()
    {
        try
        {
            await Application.Current!.MainPage!.Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ClosePage error: {ex}");
        }
    }

    private async void OnSave(object sender, TappedEventArgs e)
    {
        if (_saving) return;

        int customerIdx = CustomerPicker.SelectedIndex - 1;
        if (customerIdx < 0 || customerIdx >= _customers.Count)
        {
            await DisplayAlert("Error", "Please select a customer.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            await DisplayAlert("Error", "Please enter a project name.", "OK");
            return;
        }

        decimal budget = 0;
        if (!string.IsNullOrWhiteSpace(BudgetEntry.Text) &&
            (!decimal.TryParse(BudgetEntry.Text, out budget) || budget < 0))
        {
            await DisplayAlert("Error", "Please enter a valid budget amount.", "OK");
            return;
        }

        var status = StatusPicker.SelectedIndex >= 0 ? _statuses[StatusPicker.SelectedIndex] : "Planning";
        DateTime? endDate = EndDateCheck.IsChecked ? EndDatePicker.Date : (DateTime?)null;

        _saving = true;
        StatusLabel.IsVisible = true;
        StatusLabel.Text = "Creating project...";
        StatusLabel.TextColor = Color.FromArgb("#b45309");

        try
        {
            var result = await _api.CreateProjectAsync(
                _customers[customerIdx].Id,
                NameEntry.Text.Trim(),
                DescriptionEditor.Text,
                LocationEntry.Text,
                status,
                budget,
                StartDatePicker.Date,
                endDate);

            if (result != null)
            {
                StatusLabel.Text = "Project created!";
                StatusLabel.TextColor = Color.FromArgb("#0d7a4f");
                await DisplayAlert("Success", $"Project \"{result.Name}\" created.", "OK");
                await ClosePageAsync();
            }
            else
            {
                _saving = false;
                StatusLabel.Text = "Could not create project. Please try again.";
                StatusLabel.TextColor = Color.FromArgb("#b3261e");
            }
        }
        catch (Exception ex)
        {
            _saving = false;
            StatusLabel.Text = "Could not create project.";
            StatusLabel.TextColor = Color.FromArgb("#b3261e");
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnCancel(object sender, TappedEventArgs e)
    {
        await ClosePageAsync();
    }
}