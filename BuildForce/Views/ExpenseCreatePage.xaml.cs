using BuildForce.Services;

namespace BuildForce.Views;

public partial class ExpenseCreatePage : ContentPage
{
    private readonly ApiService _api;
    private List<ProjectSummary> _projects = new();
    private List<string> _categories = new();

    public ExpenseCreatePage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        ExpenseDatePicker.Date = DateTime.Today;
        LoadProjects();
        LoadCategories();
    }

    private async void LoadProjects()
    {
        try
        {
            _projects = await _api.GetProjectsAsync();
            ProjectPicker.Items.Clear();
            ProjectPicker.Items.Add("Select Project");
            foreach (var p in _projects)
                ProjectPicker.Items.Add(p.Name);
            ProjectPicker.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not load projects: {ex.Message}", "OK");
        }
    }

    private async void LoadCategories()
    {
        try
        {
            _categories = await _api.GetExpenseCategoriesAsync();
        }
        catch
        {
            _categories = new List<string>();
        }

        if (_categories.Count == 0)
        {
            _categories = new List<string>
            {
                "Materials", "Tools & Equipment", "Fuel", "Vehicle Maintenance",
                "Equipment Rental", "Safety Equipment", "Permits & Fees",
                "Office Supplies", "Meals & Entertainment", "Utilities",
                "Shipping", "Subcontractor", "Labor", "Insurance", "General"
            };
        }
        CategoryPicker.Items.Clear();
        foreach (var c in _categories)
            CategoryPicker.Items.Add(c);
        CategoryPicker.SelectedIndex = 0;
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

    private async void OnSave(object sender, EventArgs e)
    {
        int projectIdx = ProjectPicker.SelectedIndex - 1;
        if (projectIdx < 0 || projectIdx >= _projects.Count)
        {
            await DisplayAlert("Error", "Please select a project.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(DescriptionEntry.Text))
        {
            await DisplayAlert("Error", "Please enter a description.", "OK");
            return;
        }

        if (!decimal.TryParse(AmountEntry.Text, out decimal amount) || amount <= 0)
        {
            await DisplayAlert("Error", "Please enter a valid amount.", "OK");
            return;
        }

        var category = CategoryPicker.SelectedIndex >= 0
            ? _categories[CategoryPicker.SelectedIndex]
            : "General";

        SaveBtn.IsEnabled = false;
        StatusLabel.IsVisible = true;
        StatusLabel.Text = "Saving expense...";
        StatusLabel.TextColor = Color.FromArgb("#f59e0b");

        try
        {
            var result = await _api.CreateExpenseAsync(
                _projects[projectIdx].Id,
                DescriptionEntry.Text,
                amount,
                ExpenseDatePicker.Date,
                category,
                VendorEntry.Text,
                NotesEditor.Text);

            if (result != null)
            {
                StatusLabel.Text = "Expense logged!";
                StatusLabel.TextColor = Color.FromArgb("#22c55e");
                await DisplayAlert("Success", $"Expense of ${result.Amount:F2} logged.", "OK");
                await ClosePageAsync();
            }
            else
            {
                SaveBtn.IsEnabled = true;
                StatusLabel.Text = "Could not log expense. Please try again.";
                StatusLabel.TextColor = Color.FromArgb("#ef4444");
            }
        }
        catch (Exception ex)
        {
            SaveBtn.IsEnabled = true;
            StatusLabel.Text = "Could not log expense.";
            StatusLabel.TextColor = Color.FromArgb("#ef4444");
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnCancel(object sender, EventArgs e)
    {
        await ClosePageAsync();
    }
}