namespace BuildForce.Views;

[QueryProperty(nameof(TimesheetId), "timesheetId")]
public partial class TimesheetDetailPage : ContentPage
{
    private readonly Services.ApiService _api;
    private int _timesheetId;

    public int TimesheetId
    {
        set
        {
            _timesheetId = value;
            LoadDetail();
        }
    }

    public TimesheetDetailPage(Services.ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    private async void LoadDetail()
    {
        try
        {
            var start = DateTime.Today.AddDays(-60);
            var timesheets = await _api.GetTimesheetsAsync(start, DateTime.Today);
            var entry = timesheets.FirstOrDefault(t => t.TimesheetId == _timesheetId);

            if (entry == null)
            {
                await DisplayAlert("Error", "Timesheet not found.", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            var statusColor = entry.Status == "Completed" ? "#22c55e" : "#f59e0b";
            StatusLabel.Text = entry.Status.ToUpper();
            StatusLabel.TextColor = Color.FromArgb(statusColor);

            HoursLabel.Text = $"{entry.TotalHours:F1}h";
            DateLabel.Text = entry.Date.ToString("dddd, MMMM d, yyyy");
            ProjectNameLabel.Text = entry.ProjectName ?? "No Project";

            ClockInLabel.Text = entry.ClockInTime?.ToLocalTime().ToString("h:mm tt") ?? "--";
            ClockOutLabel.Text = entry.ClockOutTime?.ToLocalTime().ToString("h:mm tt") ?? "Active";

            RegularHoursLabel.Text = $"{entry.HoursWorked:F1}h";
            OvertimeHoursLabel.Text = $"{entry.OvertimeHours:F1}h";

            ClockInLocationLabel.Text = entry.ClockInLatitude.HasValue
                ? $"Clock In: {entry.ClockInLatitude:F4}, {entry.ClockInLongitude:F4}"
                : "Clock In: Not recorded";

            ClockOutLocationLabel.Text = entry.ClockOutLatitude.HasValue
                ? $"Clock Out: {entry.ClockOutLatitude:F4}, {entry.ClockOutLongitude:F4}"
                : "Clock Out: Not recorded";

            DescriptionLabel.Text = string.IsNullOrEmpty(entry.Description)
                ? "No notes" : entry.Description;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not load timesheet: {ex.Message}", "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}