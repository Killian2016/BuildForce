namespace BuildForce.Views;

public partial class SafetyCheckPage : ContentPage
{
    public TaskCompletionSource<bool> Result { get; } = new();

    public SafetyCheckPage()
    {
        InitializeComponent();
        PpeSwitch.Toggled += OnSwitchToggled;
        FitSwitch.Toggled += OnSwitchToggled;
        SafetyPlanSwitch.Toggled += OnSwitchToggled;
        WorkAreaSwitch.Toggled += OnSwitchToggled;
    }

    private void OnSwitchToggled(object? sender, ToggledEventArgs e)
    {
        bool allConfirmed = PpeSwitch.IsToggled &&
                            FitSwitch.IsToggled &&
                            SafetyPlanSwitch.IsToggled &&
                            WorkAreaSwitch.IsToggled;
        ProceedBtn.IsEnabled = allConfirmed;
        ProceedBtn.BackgroundColor = allConfirmed
            ? Color.FromArgb("#22c55e")
            : Color.FromArgb("#374151");
        ProceedBtn.TextColor = allConfirmed ? Colors.Black : Color.FromArgb("#6b7280");
        StatusLabel.Text = allConfirmed
            ? "All safety items confirmed - ready to clock in"
            : "Please confirm all safety items above";
        StatusLabel.TextColor = allConfirmed
            ? Color.FromArgb("#22c55e")
            : Color.FromArgb("#f59e0b");
    }

    private async void OnProceed(object sender, EventArgs e)
    {
        Result.TrySetResult(true);
        await Navigation.PopModalAsync();
    }

    private async void OnCancel(object sender, EventArgs e)
    {
        Result.TrySetResult(false);
        await Navigation.PopModalAsync();
    }

    protected override bool OnBackButtonPressed()
    {
        // Swallow back: cancelling a punch must be an explicit Cancel tap.
        return true;
    }
}