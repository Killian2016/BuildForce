#pragma warning disable CA1416
using BuildForce.Services;
using Microsoft.Maui.Graphics.Platform;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace BuildForce.Views;

public partial class ExpenseCreatePage : ContentPage
{
    private readonly ApiService _api;
    private List<ProjectSummary> _projects = new();
    private List<string> _categories = new();
    private bool _busy = false;

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

    private async void OnScanReceipt(object sender, TappedEventArgs e)
    {
        if (_busy) return;
        try
        {
            string action = await DisplayActionSheet("Scan Receipt", "Cancel", null, "Take Photo", "Choose from Gallery");
            FileResult? photo = null;

            if (action == "Take Photo")
            {
                var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                    status = await Permissions.RequestAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlert("Permission Needed", "Camera permission is required to take a photo of the receipt.", "OK");
                    return;
                }
                photo = await MediaPicker.Default.CapturePhotoAsync();
            }
            else if (action == "Choose from Gallery")
            {
                photo = await MediaPicker.Default.PickPhotoAsync();
            }

            if (photo == null) return;

            _busy = true;
            StatusLabel.IsVisible = true;
            StatusLabel.TextColor = Color.FromArgb("#b45309");
            StatusLabel.Text = "Preparing photo...";

            string base64;
            try
            {
                base64 = await PrepareImageAsync(photo);
            }
            catch (Exception prepEx)
            {
                _busy = false;
                StatusLabel.Text = "Could not process the photo.";
                StatusLabel.TextColor = Color.FromArgb("#b3261e");
                await DisplayAlert("Photo Error", $"Could not process the photo: {prepEx.Message}", "OK");
                return;
            }

            StatusLabel.Text = "Scanning receipt with AI... (5-20 seconds)";

            var result = await _api.ScanReceiptPreviewAsync(base64, "receipt.jpg");
            _busy = false;

            if (result == null)
            {
                StatusLabel.Text = "Could not read that receipt.";
                StatusLabel.TextColor = Color.FromArgb("#b3261e");
                await DisplayAlert("Scan Failed", _api.LastError ?? "The server could not read the receipt. Try a clearer, well-lit photo.", "OK");
                return;
            }

            if (result.Total.HasValue && result.Total.Value > 0)
                AmountEntry.Text = result.Total.Value.ToString("F2");
            if (!string.IsNullOrWhiteSpace(result.MerchantName))
                VendorEntry.Text = result.MerchantName;
            if (result.TransactionDate.HasValue)
                ExpenseDatePicker.Date = result.TransactionDate.Value;
            if (!string.IsNullOrWhiteSpace(result.SuggestedDescription))
                DescriptionEntry.Text = result.SuggestedDescription;
            if (!string.IsNullOrWhiteSpace(result.SuggestedCategory))
            {
                var idx = _categories.FindIndex(c =>
                    string.Equals(c, result.SuggestedCategory, StringComparison.OrdinalIgnoreCase));
                if (idx >= 0) CategoryPicker.SelectedIndex = idx;
            }

            StatusLabel.Text = "Receipt scanned - review the details and save.";
            StatusLabel.TextColor = Color.FromArgb("#0d7a4f");
        }
        catch (Exception ex)
        {
            _busy = false;
            StatusLabel.IsVisible = true;
            StatusLabel.Text = "Scan failed.";
            StatusLabel.TextColor = Color.FromArgb("#b3261e");
            await DisplayAlert("Scan Error", ex.Message, "OK");
        }
    }

    private static async Task<string> PrepareImageAsync(FileResult photo)
    {
        using var source = await photo.OpenReadAsync();
        IImage? img = PlatformImage.FromStream(source);
        if (img == null)
            throw new Exception("Unsupported image format.");

        const float maxDim = 1600f;
        byte[] bytes;
        if (img.Width > maxDim || img.Height > maxDim)
        {
            using var resized = img.Downsize(maxDim, true);
            using var ms = new MemoryStream();
            await resized.SaveAsync(ms, Microsoft.Maui.Graphics.ImageFormat.Jpeg, 0.8f);
            bytes = ms.ToArray();
        }
        else
        {
            using var ms = new MemoryStream();
            await img.SaveAsync(ms, Microsoft.Maui.Graphics.ImageFormat.Jpeg, 0.8f);
            bytes = ms.ToArray();
        }
        img.Dispose();
        System.Diagnostics.Debug.WriteLine($"Receipt prepared: {bytes.Length / 1024} KB");
        return Convert.ToBase64String(bytes);
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
        if (_busy) return;

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

        _busy = true;
        StatusLabel.IsVisible = true;
        StatusLabel.Text = "Saving expense...";
        StatusLabel.TextColor = Color.FromArgb("#b45309");

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
                StatusLabel.TextColor = Color.FromArgb("#0d7a4f");
                await DisplayAlert("Success", $"Expense of ${result.Amount:F2} logged.", "OK");
                await ClosePageAsync();
            }
            else
            {
                _busy = false;
                StatusLabel.Text = "Could not log expense. Please try again.";
                StatusLabel.TextColor = Color.FromArgb("#b3261e");
            }
        }
        catch (Exception ex)
        {
            _busy = false;
            StatusLabel.Text = "Could not log expense.";
            StatusLabel.TextColor = Color.FromArgb("#b3261e");
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnCancel(object sender, TappedEventArgs e)
    {
        await ClosePageAsync();
    }
}