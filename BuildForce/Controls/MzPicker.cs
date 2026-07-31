#pragma warning disable CA1416
using System.Linq;
using BuildForce.Views;

namespace BuildForce.Controls;

// [PSH3a] A Picker that opens the Mezano violet sheet instead of the grey Android dialog.
// Deliberately a Picker SUBCLASS: every existing Items.Add / SelectedIndex /
// SelectedIndexChanged line in the code-behind keeps working untouched, so converting a
// page is just a tag rename in XAML. Setting SelectedIndex here raises the same
// SelectedIndexChanged the native dialog raised, so page reload logic still fires.
public class MzPicker : Picker
{
    public static readonly BindableProperty SheetTitleProperty =
        BindableProperty.Create(nameof(SheetTitle), typeof(string), typeof(MzPicker), null);

    // Optional override for the sheet heading. Falls back to Title with the
    // em-dash decoration stripped, then to "Select".
    public string? SheetTitle
    {
        get => (string?)GetValue(SheetTitleProperty);
        set => SetValue(SheetTitleProperty, value);
    }

    public async Task ShowSheetAsync()
    {
        var items = Items?.ToList() ?? new List<string>();
        if (items.Count == 0) return;

        // MainShellPage steals page content (PageContent.Content = page.Content), so a
        // hosted page's own Navigation can be detached - always push from the window root.
        var nav = Application.Current?.Windows?.FirstOrDefault()?.Page?.Navigation;
        if (nav == null) return;

        var heading = SheetTitle;
        if (string.IsNullOrWhiteSpace(heading)) heading = Clean(Title);
        if (string.IsNullOrWhiteSpace(heading)) heading = "Select";

        int picked = await PickerSheetPage.PickIndexAsync(nav, heading, items, SelectedIndex);
        if (picked >= 0 && picked != SelectedIndex) SelectedIndex = picked;
    }

    private static string Clean(string? s)
    {
        var t = (s ?? "").Trim();
        return t.Trim('\u2014', '\u2013', '-', ' ').Trim();
    }
}