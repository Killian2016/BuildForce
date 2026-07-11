namespace BuildForce.Views;

public class InjuryReportResult
{
    public bool Confirmed { get; set; }
    public bool InjuryReported { get; set; }
    public string InjuryDetails { get; set; } = "";
}

public partial class InjuryReportPage : ContentPage
{
    public TaskCompletionSource<InjuryReportResult> Result { get; } = new();

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
        Result.TrySetResult(new InjuryReportResult
        {
            Confirmed = true,
            InjuryReported = injuryReported,
            InjuryDetails = injuryDetails
        });
        await Navigation.PopModalAsync();
    }

    private async void OnCancel(object sender, EventArgs e)
    {
        Result.TrySetResult(new InjuryReportResult { Confirmed = false });
        await Navigation.PopModalAsync();
    }

    protected override bool OnBackButtonPressed()
    {
        // Swallow back: cancelling a punch must be an explicit Cancel tap.
        return true;
    }
}