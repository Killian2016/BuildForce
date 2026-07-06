using BuildForce.Services;
using Microsoft.Maui.Controls.Shapes;

namespace BuildForce.Views;

public partial class InvoiceCreatePage : ContentPage
{
    private sealed class LineItemRow
    {
        public Border Container = null!;
        public Entry Description = null!;
        public Entry Quantity = null!;
        public Entry Unit = null!;
        public Entry UnitPrice = null!;
        public CheckBox Taxable = null!;
        public Label AmountLabel = null!;
    }

    private readonly ApiService _api;
    private List<ProjectSummary> _projects = new();
    private readonly List<LineItemRow> _rows = new();
    private bool _saving = false;

    public InvoiceCreatePage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        InvoiceDatePicker.Date = DateTime.Today;
        DueDatePicker.Date = DateTime.Today.AddDays(30);
        AddLineItemRow();
        LoadProjects();
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

    private static Border Wrap(View inner, Thickness? padding = null) => new Border
    {
        BackgroundColor = Colors.White,
        Stroke = Color.FromArgb("#d8deeb"),
        StrokeThickness = 1,
        StrokeShape = new RoundRectangle { CornerRadius = 8 },
        Padding = padding ?? new Thickness(10, 0),
        Content = inner
    };

    private static Entry MakeEntry(string placeholder, Keyboard? keyboard = null) => new Entry
    {
        Placeholder = placeholder,
        PlaceholderColor = Color.FromArgb("#9aa3b8"),
        TextColor = Color.FromArgb("#1a2340"),
        BackgroundColor = Colors.Transparent,
        FontSize = 13,
        Keyboard = keyboard ?? Keyboard.Default
    };

    private void OnAddLineItem(object sender, TappedEventArgs e) => AddLineItemRow();

    private void AddLineItemRow()
    {
        var row = new LineItemRow
        {
            Description = MakeEntry("Description (e.g. Framing labor)"),
            Quantity = MakeEntry("Qty", Keyboard.Numeric),
            Unit = MakeEntry("Unit"),
            UnitPrice = MakeEntry("Price", Keyboard.Numeric),
            Taxable = new CheckBox
            {
                IsChecked = true,
                Color = Color.FromArgb("#0d7a4f"),
                VerticalOptions = LayoutOptions.Center
            },
            AmountLabel = new Label
            {
                Text = "$0.00",
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#1a2340"),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End
            }
        };
        row.Quantity.Text = "1";
        row.Unit.Text = "each";

        row.Quantity.TextChanged += (s, e) => RecalcTotals();
        row.UnitPrice.TextChanged += (s, e) => RecalcTotals();
        row.Taxable.CheckedChanged += (s, e) => RecalcTotals();

        var removeLbl = new Label
        {
            Text = "Remove",
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#b3261e"),
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.End
        };
        var removeTap = new TapGestureRecognizer();
        removeTap.Tapped += async (s, e) => await RemoveRowAsync(row);
        removeLbl.GestureRecognizers.Add(removeTap);

        var qtyGrid = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1.4, GridUnitType.Star) }
            }
        };
        var qtyBorder = Wrap(row.Quantity);
        var unitBorder = Wrap(row.Unit);
        var priceBorder = Wrap(row.UnitPrice);
        Grid.SetColumn(qtyBorder, 0);
        Grid.SetColumn(unitBorder, 1);
        Grid.SetColumn(priceBorder, 2);
        qtyGrid.Children.Add(qtyBorder);
        qtyGrid.Children.Add(unitBorder);
        qtyGrid.Children.Add(priceBorder);

        var taxableLabel = new Label
        {
            Text = "Taxable",
            FontSize = 12,
            TextColor = Color.FromArgb("#6b7280"),
            VerticalOptions = LayoutOptions.Center
        };
        var footerGrid = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };
        Grid.SetColumn(row.Taxable, 0);
        Grid.SetColumn(taxableLabel, 1);
        Grid.SetColumn(row.AmountLabel, 2);
        Grid.SetColumn(removeLbl, 3);
        footerGrid.Children.Add(row.Taxable);
        footerGrid.Children.Add(taxableLabel);
        footerGrid.Children.Add(row.AmountLabel);
        footerGrid.Children.Add(removeLbl);

        var stack = new VerticalStackLayout { Spacing = 8 };
        stack.Children.Add(Wrap(row.Description));
        stack.Children.Add(qtyGrid);
        stack.Children.Add(footerGrid);

        row.Container = new Border
        {
            BackgroundColor = Color.FromArgb("#f7f9fd"),
            Stroke = Color.FromArgb("#e2e7f0"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Padding = new Thickness(12, 10),
            Content = stack
        };

        _rows.Add(row);
        LineItemsList.Children.Add(row.Container);
        RecalcTotals();
    }

    private async Task RemoveRowAsync(LineItemRow row)
    {
        if (_rows.Count <= 1)
        {
            await DisplayAlert("Line Items", "An invoice needs at least one line item.", "OK");
            return;
        }
        _rows.Remove(row);
        LineItemsList.Children.Remove(row.Container);
        RecalcTotals();
    }

    private void OnDiscountChanged(object sender, TextChangedEventArgs e) => RecalcTotals();

    private void RecalcTotals()
    {
        decimal subtotal = 0;
        foreach (var r in _rows)
        {
            decimal qty = decimal.TryParse(r.Quantity.Text, out var q) ? q : 0;
            decimal price = decimal.TryParse(r.UnitPrice.Text, out var p) ? p : 0;
            var amount = qty * price;
            r.AmountLabel.Text = amount.ToString("C2");
            subtotal += amount;
        }
        decimal discountPct = decimal.TryParse(DiscountEntry.Text, out var d) ? d : 0;
        if (discountPct < 0) discountPct = 0;
        if (discountPct > 100) discountPct = 100;
        var discountAmount = subtotal * discountPct / 100m;

        SubtotalLabel.Text = subtotal.ToString("C2");
        DiscountLabel.Text = "-" + discountAmount.ToString("C2");
        TotalLabel.Text = (subtotal - discountAmount).ToString("C2");
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
        try
        {
            int projectIdx = ProjectPicker.SelectedIndex - 1;
            if (projectIdx < 0 || projectIdx >= _projects.Count)
            {
                await DisplayAlert("Error", "Please select a project.", "OK");
                return;
            }

            var items = new List<MobileLineItem>();
            foreach (var r in _rows)
            {
                if (string.IsNullOrWhiteSpace(r.Description.Text)) continue;
                decimal qty = decimal.TryParse(r.Quantity.Text, out var q) ? q : 0;
                decimal price = decimal.TryParse(r.UnitPrice.Text, out var p) ? p : 0;
                if (qty <= 0) continue;
                items.Add(new MobileLineItem
                {
                    Description = r.Description.Text.Trim(),
                    Quantity = qty,
                    Unit = string.IsNullOrWhiteSpace(r.Unit.Text) ? "each" : r.Unit.Text.Trim(),
                    UnitPrice = price,
                    IsTaxable = r.Taxable.IsChecked
                });
            }

            if (items.Count == 0)
            {
                await DisplayAlert("Error", "Add at least one line item with a description and quantity.", "OK");
                return;
            }

            decimal discountPct = decimal.TryParse(DiscountEntry.Text, out var dp) ? dp : 0;
            if (discountPct < 0 || discountPct > 100)
            {
                await DisplayAlert("Error", "Discount must be between 0 and 100.", "OK");
                return;
            }

            _saving = true;
            StatusLabel.IsVisible = true;
            StatusLabel.Text = "Creating invoice...";
            StatusLabel.TextColor = Color.FromArgb("#b45309");

            var result = await _api.CreateInvoiceAsync(
                _projects[projectIdx].Id,
                InvoiceDatePicker.Date,
                DueDatePicker.Date,
                discountPct,
                NotesEditor.Text,
                items);

            if (result != null)
            {
                StatusLabel.Text = "Invoice created!";
                StatusLabel.TextColor = Color.FromArgb("#0d7a4f");
                await DisplayAlert("Success",
                    $"{result.InvoiceNumber} created.\nSubtotal: {result.Subtotal:C2}\nTax: {result.TaxAmount:C2}\nTotal: {result.TotalAmount:C2}", "OK");
                await ClosePageAsync();
            }
            else
            {
                _saving = false;
                StatusLabel.Text = "Could not create invoice.";
                StatusLabel.TextColor = Color.FromArgb("#b3261e");
                await DisplayAlert("Error", _api.LastError ?? "Could not create invoice. Please try again.", "OK");
            }
        }
        catch (Exception ex)
        {
            _saving = false;
            StatusLabel.IsVisible = true;
            StatusLabel.Text = "Could not create invoice.";
            StatusLabel.TextColor = Color.FromArgb("#b3261e");
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnCancel(object sender, TappedEventArgs e)
    {
        await ClosePageAsync();
    }
}