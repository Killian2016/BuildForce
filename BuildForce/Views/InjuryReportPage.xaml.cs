namespace BuildForce.Views;

[QueryProperty(nameof(ElapsedHours), "hours")]
[QueryProperty(nameof(ProjectName), "project")]
[QueryProperty(nameof(TimesheetId), "timesheetId")]
public partial class InjuryReportPage : ContentPage
{
    public string ElapsedHours
    {
        set => HoursLabel.Text = value;
    }

    public string ProjectName
    {
        set => ProjectLabel.Text = value;
    }

    public string TimesheetId { get; set; } = "";

    public InjuryReportPage()
    {
        InitializeComponent();
        AccurateSwitch.Toggled += OnSwitchToggled;
        NoInjurySwitch.Toggled += OnSwitchToggled;
    }

    private void OnSwitchToggled(object? sender, ToggledEventArgs e)
    {
        UpdateButton();
    }

    private void OnInjuryToggled(object? sender, ToggledEventArgs e)
    {
        InjuryDetailPanel.IsVisible = !NoInjurySwitch.IsToggled;
        UpdateButton();
    }

    private void UpdateButton()
    {
        bool ready = AccurateSwitch.IsToggled &&
                     (NoInjurySwitch.IsToggled ||
                      (!NoInjurySwitch.IsToggled && !string.IsNullOrWhiteSpace(InjuryEditor.Text)));

        ClockOutBtn.IsEnabled = ready;
        ClockOutBtn.BackgroundColor = ready
            ? Color.FromArgb("#ef4444")
            : Color.FromArgb("#374151");
        ClockOutBtn.TextColor = ready ? Colors.White : Color.FromArgb("#6b7280");

        StatusLabel.Text = ready
            ? "Ready to complete clock out"
            : "Please confirm all items above";
        StatusLabel.TextColor = ready
            ? Color.FromArgb("#22c55e")
            : Color.FromArgb("#f59e0b");
    }

    private async void OnClockOut(object sender, EventArgs e)
    {
        bool injuryReported = !NoInjurySwitch.IsToggled;
        string injuryDetails = InjuryEditor.Text ?? "";

        if (injuryReported)
        {
            await DisplayAlert("Injury Reported",
                "Your supervisor has been notified. Please report to the site office before leaving.",
                "OK");
        }

        await Shell.Current.GoToAsync("..", new Dictionary<string, object>
        {
            { "ClockOutConfirmed", true },
            { "InjuryReported", injuryReported },
            { "InjuryDetails", injuryDetails }
        });
    }

    private async void OnCancel(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}