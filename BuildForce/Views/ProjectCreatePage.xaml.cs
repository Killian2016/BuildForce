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

    public ProjectCreatePage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        StartDatePicker.Date = DateTime.Today;
        LoadCustomers();
        SetupStatusPicker();
    }

    private void SetupStatusPicker()
    {
        foreach (var s in _statuses)
            StatusPicker.Items.Add(s);
        StatusPicker.SelectedIndex = 0;
    }

    private async void LoadCustomers()
    {
        try
        {
            _customers = await _api.GetCustomersAsync();
            CustomerPicker.Items.Clear();
            CustomerPicker.Items.Add("Select Customer");
            foreach (var c in _customers)
                CustomerPicker.Items.Add(c.Name);
            CustomerPicker.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not load customers: {ex.Message}", "OK");
        }
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

    private async void OnCreate(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            await DisplayAlert("Error", "Please enter a project name.", "OK");
            return;
        }

        int customerIdx = CustomerPicker.SelectedIndex - 1;
        if (customerIdx < 0 || customerIdx >= _customers.Count)
        {
            await DisplayAlert("Error", "Please select a customer.", "OK");
            return;
        }

        decimal budget = 0;
        if (!string.IsNullOrWhiteSpace(BudgetEntry.Text))
            decimal.TryParse(BudgetEntry.Text, out budget);

        var status = StatusPicker.SelectedIndex >= 0
            ? _statuses[StatusPicker.SelectedIndex]
            : "Planning";

        CreateBtn.IsEnabled = false;
        StatusLabel.IsVisible = true;
        StatusLabel.Text = "Creating project...";
        StatusLabel.TextColor = Color.FromArgb("#f59e0b");

        try
        {
            var result = await _api.CreateProjectAsync(
                _customers[customerIdx].Id,
                NameEntry.Text,
                DescriptionEditor.Text,
                LocationEntry.Text,
                status,
                budget,
                StartDatePicker.Date,
                null);

            if (result != null)
            {
                StatusLabel.Text = "Project created!";
                StatusLabel.TextColor = Color.FromArgb("#22c55e");
                await DisplayAlert("Success", $"Project \"{result.Name}\" created.", "OK");
                await ClosePageAsync();
            }
            else
            {
                CreateBtn.IsEnabled = true;
                StatusLabel.Text = "Could not create project. Please try again.";
                StatusLabel.TextColor = Color.FromArgb("#ef4444");
            }
        }
        catch (Exception ex)
        {
            CreateBtn.IsEnabled = true;
            StatusLabel.Text = "Could not create project.";
            StatusLabel.TextColor = Color.FromArgb("#ef4444");
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnCancel(object sender, EventArgs e)
    {
        await ClosePageAsync();
    }
}