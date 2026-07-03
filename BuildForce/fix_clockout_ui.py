import sys

path = r"C:\Users\mezan\source\repos\BuildForce\BuildForce\Views\TimeClockPage.xaml.cs"

with open(path, "r", encoding="utf-8") as f:
    content = f.read()

# Fix 1: Wrap StopTimer UI updates in MainThread.BeginInvokeOnMainThread
old_stop = '''    public void StopTimer()
    {
        _timer?.Stop();
        _timer = null;
        _isClockedIn = false;
        _activeTimesheetId = 0;

        StatusLabel.Text = "NOT CLOCKED IN";
        StatusLabel.TextColor = Color.FromArgb("#6b7280");
        TimerLabel.Text = "00:00:00";
        TimerLabel.TextColor = Colors.White;
        ClockedInBadge.IsVisible = false;
        ClockInBtn.IsVisible = true;
        ClockInBtn.IsEnabled = true;
        ClockOutBtn.IsVisible = false;
        ClockOutBtn.IsEnabled = true;
        ActiveCostCodeLabel.Text = "None";
        _isOnSite = false;
        UpdateClockInButton();
    }'''

new_stop = '''    public void StopTimer()
    {
        _timer?.Stop();
        _timer = null;
        _isClockedIn = false;
        _activeTimesheetId = 0;
        _isOnSite = false;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            StatusLabel.Text = "NOT CLOCKED IN";
            StatusLabel.TextColor = Color.FromArgb("#6b7280");
            TimerLabel.Text = "00:00:00";
            TimerLabel.TextColor = Colors.White;
            ClockedInBadge.IsVisible = false;
            ClockInBtn.IsVisible = true;
            ClockInBtn.IsEnabled = true;
            ClockOutBtn.IsVisible = false;
            ClockOutBtn.IsEnabled = true;
            ActiveCostCodeLabel.Text = "None";
            UpdateClockInButton();
        });
    }'''

# Fix 2: Also ensure OnClockOut calls StopTimer before the alert so UI updates immediately
old_clockout = '''        if (result != null)
        {
            await DisplayAlert("Clocked Out",
                $"Total: {result.TotalHours:F2}h\\nRegular: {result.HoursWorked:F2}h\\nOvertime: {result.OvertimeHours:F2}h",
                "OK");
            StopTimer();
            await LoadSummary();
        }
        else
        {
            ClockOutBtn.IsEnabled = true;
            await DisplayAlert("Error", "Could not clock out. Please try again.", "OK");
        }'''

new_clockout = '''        if (result != null)
        {
            StopTimer();
            await LoadSummary();
            await DisplayAlert("Clocked Out",
                $"Total: {result.TotalHours:F2}h\\nRegular: {result.HoursWorked:F2}h\\nOvertime: {result.OvertimeHours:F2}h",
                "OK");
        }
        else
        {
            ClockOutBtn.IsEnabled = true;
            await DisplayAlert("Error", "Could not clock out. Please try again.", "OK");
        }'''

# Also fix the emoji version
old_clockout_emoji = '''        if (result != null)
        {
            await DisplayAlert("\u2705 Clocked Out",
                $"Total: {result.TotalHours:F2}h\\nRegular: {result.HoursWorked:F2}h\\nOvertime: {result.OvertimeHours:F2}h",
                "OK");
            StopTimer();
            await LoadSummary();
        }
        else
        {
            ClockOutBtn.IsEnabled = true;
            await DisplayAlert("Error", "Could not clock out. Please try again.", "OK");
        }'''

new_clockout_emoji = '''        if (result != null)
        {
            StopTimer();
            await LoadSummary();
            await DisplayAlert("Clocked Out",
                $"Total: {result.TotalHours:F2}h\\nRegular: {result.HoursWorked:F2}h\\nOvertime: {result.OvertimeHours:F2}h",
                "OK");
        }
        else
        {
            ClockOutBtn.IsEnabled = true;
            await DisplayAlert("Error", "Could not clock out. Please try again.", "OK");
        }'''

if old_stop in content:
    content = content.replace(old_stop, new_stop)
    print("OK: StopTimer wrapped in MainThread")
else:
    print("WARN: StopTimer pattern not found - check manually")

if old_clockout in content:
    content = content.replace(old_clockout, new_clockout)
    print("OK: OnClockOut order fixed (StopTimer before alert)")
elif old_clockout_emoji in content:
    content = content.replace(old_clockout_emoji, new_clockout_emoji)
    print("OK: OnClockOut order fixed (emoji variant)")
else:
    print("WARN: OnClockOut pattern not found - check manually")

with open(path, "w", encoding="utf-8") as f:
    f.write(content)

print("Done. Rebuild and test.")