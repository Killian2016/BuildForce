#pragma warning disable CA1416
using BuildForce.Services;
using System.Text.Json;

namespace BuildForce.Views;

public class RingDrawable : IDrawable
{
    public float Progress { get; set; } = 0f;
    public Color RingColor { get; set; } = Color.FromArgb("#cbd5e1");

    public void Draw(ICanvas canvas, RectF rect)
    {
        float stroke = 12f;
        float pad = stroke / 2 + 2;
        var r = new RectF(rect.X + pad, rect.Y + pad, rect.Width - pad * 2, rect.Height - pad * 2);

        canvas.StrokeSize = stroke;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeColor = Color.FromArgb("#e2e8f0");
        canvas.DrawEllipse(r);

        if (Progress > 0.005f)
        {
            canvas.StrokeColor = RingColor;
            float sweep = 360f * Math.Min(Progress, 1f);
            canvas.DrawArc(r, 90f, 90f - sweep, true, false);
        }
    }
}

public partial class TimeClockPage : ContentPage
{
    private readonly ApiService _api;
    private System.Timers.Timer? _timer;
    private DateTime _clockInTime;
    private bool _isClockedIn;
    private string _selectedCostCode = "";
    private List<ProjectSummary> _projects = new();
    private ProjectSummary? _selectedProject;
    private double _workerLat;
    private double _workerLng;
    private bool _isOnSite = false;
    private bool _gpsReady = false;
    private string _employeeName = "";
    private int _activeTimesheetId = 0;

    private bool _onBreak = false;
    private DateTime _breakStartLocal;
    private int _breakMinutesAccum = 0;

    private readonly RingDrawable _ring = new();

    private static readonly Color Green = Color.FromArgb("#10b981");
    private static readonly Color Amber = Color.FromArgb("#f0a500");
    private static readonly Color Blue = Color.FromArgb("#0ea5e9");
    private static readonly Color Red = Color.FromArgb("#ef4444");
    private static readonly Color Muted = Color.FromArgb("#64748b");
    private static readonly Color Dark = Color.FromArgb("#1a2233");
    private static readonly Color DisabledBg = Color.FromArgb("#cbd5e1");

    private readonly List<string> _costCodes = new()
    {
        "General", "Framing", "Electrical", "Plumbing", "HVAC",
        "Drywall", "Flooring", "Roofing", "Concrete", "Painting",
        "Excavation", "Landscaping", "Cleanup", "Inspection", "Other"
    };

    public TimeClockPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        RingView.Drawable = _ring;
        LoadEmployeeInfo();
        SetupCostCodePicker();
        LoadProjectsAndCheck();
        GetLocation();
    }

    private void LoadEmployeeInfo()
    {
        var fullName = Preferences.Get("full_name", "");
        var email = Preferences.Get("email", "");
        _employeeName = string.IsNullOrEmpty(fullName) ? email.Split('@')[0] : fullName;
        EmployeeNameLabel.Text = _employeeName;

        var parts = _employeeName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var initials = parts.Length >= 2
            ? $"{parts[0][0]}{parts[1][0]}"
            : (_employeeName.Length >= 2 ? _employeeName.Substring(0, 2) : _employeeName);
        AvatarLabel.Text = initials.ToUpper();
        NowDateLabel.Text = DateTime.Today.ToString("dddd, MMMM d, yyyy");
    }

    private void SetupCostCodePicker()
    {
        foreach (var code in _costCodes)
            CostCodePicker.Items.Add(code);

        CostCodePicker.SelectedIndexChanged += (s, e) =>
        {
            _selectedCostCode = CostCodePicker.SelectedIndex >= 0
                ? _costCodes[CostCodePicker.SelectedIndex]
                : "";
            UpdateClockInButton();
        };
    }

    private async void LoadProjectsAndCheck()
    {
        _projects = await _api.GetProjectsAsync();
        ProjectPicker.Items.Clear();
        ProjectPicker.Items.Add("Select Project");
        foreach (var p in _projects)
            ProjectPicker.Items.Add(p.Name);
        ProjectPicker.SelectedIndex = 0;

        ProjectPicker.SelectedIndexChanged += (s, e) =>
        {
            int idx = ProjectPicker.SelectedIndex - 1;
            _selectedProject = (idx >= 0 && idx < _projects.Count) ? _projects[idx] : null;
            UpdateClockInButton();
            if (_selectedProject != null && _gpsReady)
                _ = CheckGeofence(_workerLat, _workerLng, _selectedProject.Location);
        };

        var active = await _api.GetActiveTimesheetAsync();
        if (active != null && active.Status == "Active")
        {
            _activeTimesheetId = active.TimesheetId;
            _clockInTime = (active.ClockInTime ?? active.Date).ToLocalTime();
            _isClockedIn = true;
            _breakMinutesAccum = active.BreakMinutes;

            if (active.BreakStartTime.HasValue)
            {
                _onBreak = true;
                _breakStartLocal = active.BreakStartTime.Value.ToLocalTime();
            }

            var proj = _projects.FirstOrDefault(p => p.Id == active.ProjectId);
            if (proj != null)
            {
                _selectedProject = proj;
                ProjectPicker.SelectedIndex = _projects.IndexOf(proj) + 1;
            }

            var desc = active.Description ?? "";
            if (desc.StartsWith("Cost Code: "))
            {
                var cc = desc.Substring("Cost Code: ".Length).Trim();
                var ccIdx = _costCodes.IndexOf(cc);
                if (ccIdx >= 0)
                {
                    _selectedCostCode = cc;
                    CostCodePicker.SelectedIndex = ccIdx;
                }
            }

            ApplyClockedInUi();
            StartLocalTimer();
        }

        await LoadSummary();
    }

    private void ApplyClockedInUi()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ClockedInBadge.IsVisible = true;
            ClockInBtn.IsVisible = false;
            ClockOutBtn.IsEnabled = true;
            ClockOutBtn.BackgroundColor = Red;
            BreakBtn.IsEnabled = true;
            ActiveCostCodeLabel.Text = string.IsNullOrEmpty(_selectedCostCode)
                ? "Cost code: none"
                : $"Cost code: {_selectedCostCode}";
            BreakTotalLabel.Text = $"{_breakMinutesAccum}m";

            if (_onBreak)
            {
                StatusLabel.Text = "ON BREAK";
                StatusLabel.TextColor = Amber;
                SetChip("ON BREAK", Color.FromArgb("#fdf3dd"), Amber);
                SetBreakBtnActive(true);
                _ring.RingColor = Amber;
            }
            else
            {
                StatusLabel.Text = "CLOCKED IN";
                StatusLabel.TextColor = Green;
                SetChip("ON THE CLOCK", Color.FromArgb("#e7f8f1"), Green);
                SetBreakBtnActive(false);
                _ring.RingColor = Green;
            }
            RingView.Invalidate();
        });
    }

    private void SetChip(string text, Color bg, Color fg)
    {
        StatusChip.BackgroundColor = bg;
        StatusChipLabel.Text = text;
        StatusChipLabel.TextColor = fg;
    }

    private void SetBreakBtnActive(bool active)
    {
        if (active)
        {
            BreakBtn.Text = "End Break";
            BreakBtn.BackgroundColor = Color.FromArgb("#fdf3dd");
            BreakBtn.TextColor = Amber;
            BreakBtn.BorderColor = Amber;
        }
        else
        {
            BreakBtn.Text = "Break";
            BreakBtn.BackgroundColor = Colors.White;
            BreakBtn.TextColor = Dark;
            BreakBtn.BorderColor = Color.FromArgb("#e2e8f0");
        }
    }

    private TimeSpan CurrentElapsed()
    {
        var elapsed = DateTime.Now - _clockInTime;
        elapsed -= TimeSpan.FromMinutes(_breakMinutesAccum);
        if (_onBreak)
            elapsed -= (DateTime.Now - _breakStartLocal);
        if (elapsed < TimeSpan.Zero) elapsed = TimeSpan.Zero;
        return elapsed;
    }

    private async Task LoadSummary()
    {
        var summary = await _api.GetTimesheetSummaryAsync();
        if (summary != null)
            WeekHoursLabel.Text = $"{summary.TotalHours:F1}h";

        var history = await _api.GetTimesheetsAsync(DateTime.Today, DateTime.Today);
        var todayHours = history.Where(t => t.Status != "Active").Sum(t => t.TotalHours);
        if (!_isClockedIn)
            TodayHoursLabel.Text = $"{todayHours:F1}h";

        if (!_isClockedIn)
        {
            var todayBreak = history.Sum(t => t.BreakMinutes);
            BreakTotalLabel.Text = $"{todayBreak}m";
        }

        await LoadWeekBars();
        await LoadHistory();
    }

    private async Task LoadWeekBars()
    {
        try
        {
            var weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            var entries = await _api.GetTimesheetsAsync(weekStart, DateTime.Today);

            var hoursByDay = new decimal[7];
            foreach (var t in entries.Where(t => t.Status != "Active"))
            {
                var d = (int)t.Date.DayOfWeek;
                hoursByDay[d] += t.TotalHours;
            }

            var maxHours = Math.Max(8m, hoursByDay.Max());
            string[] dayNames = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
            int todayIdx = (int)DateTime.Today.DayOfWeek;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                WeekBarsGrid.Children.Clear();
                WeekBarsGrid.ColumnDefinitions.Clear();

                for (int i = 0; i < 7; i++)
                {
                    WeekBarsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

                    var stack = new VerticalStackLayout { Spacing = 4, VerticalOptions = LayoutOptions.End };

                    stack.Children.Add(new Label
                    {
                        Text = hoursByDay[i] > 0 ? $"{hoursByDay[i]:F1}" : "-",
                        FontSize = 9,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = i == todayIdx ? Amber : Muted,
                        HorizontalOptions = LayoutOptions.Center
                    });

                    double barHeight = Math.Max(3, (double)(hoursByDay[i] / maxHours) * 60);
                    stack.Children.Add(new BoxView
                    {
                        HeightRequest = barHeight,
                        WidthRequest = 22,
                        CornerRadius = new CornerRadius(4, 4, 2, 2),
                        Color = i == todayIdx ? Amber : Blue,
                        HorizontalOptions = LayoutOptions.Center
                    });

                    stack.Children.Add(new Label
                    {
                        Text = dayNames[i],
                        FontSize = 9,
                        TextColor = Muted,
                        HorizontalOptions = LayoutOptions.Center
                    });

                    Grid.SetColumn(stack, i);
                    WeekBarsGrid.Children.Add(stack);
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Week bars error: {ex.Message}");
        }
    }

    private async Task LoadHistory()
    {
        var start = DateTime.Today.AddDays(-30);
        var history = await _api.GetTimesheetsAsync(start, DateTime.Today);

        HistoryList.Children.Clear();
        foreach (var entry in history.Take(5))
        {
            var row = new Border
            {
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#e2e8f0"),
                StrokeThickness = 1,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
                Padding = new Thickness(14, 12)
            };

            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                }
            };

            var dateLabel = new Label
            {
                Text = entry.Date.ToString("ddd, MMM d"),
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Dark
            };
            var projLabel = new Label
            {
                Text = entry.ProjectName ?? "No Project",
                FontSize = 11,
                TextColor = Blue
            };
            Grid.SetRow(projLabel, 1);

            var statusColor = entry.Status == "Completed" ? Green
                : entry.Status == "Approved" ? Blue
                : entry.Status == "Rejected" ? Red
                : Amber;
            var statusText = entry.Status;
            if (entry.BreakMinutes > 0)
                statusText += $"  |  Break {entry.BreakMinutes}m";
            var statusLabel = new Label
            {
                Text = statusText,
                FontSize = 10,
                TextColor = statusColor
            };
            Grid.SetRow(statusLabel, 2);

            var clockIn = entry.ClockInTime?.ToLocalTime().ToString("h:mm tt") ?? "--";
            var clockOut = entry.ClockOutTime?.ToLocalTime().ToString("h:mm tt") ?? "Active";
            var hoursLabel = new Label
            {
                Text = $"{entry.TotalHours:F1}h\n{clockIn} - {clockOut}",
                FontSize = 11,
                TextColor = Muted,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.End
            };
            Grid.SetColumn(hoursLabel, 1);
            Grid.SetRowSpan(hoursLabel, 3);

            grid.Children.Add(dateLabel);
            grid.Children.Add(projLabel);
            grid.Children.Add(statusLabel);
            grid.Children.Add(hoursLabel);

            var tap = new TapGestureRecognizer();
            var entryId = entry.TimesheetId;
            tap.Tapped += async (s, args) =>
                await Shell.Current.GoToAsync($"TimesheetDetailPage?timesheetId={entryId}");
            row.GestureRecognizers.Add(tap);
            row.Content = grid;
            HistoryList.Children.Add(row);
        }
    }

    private async void GetLocation()
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LocationLabel.Text = "Getting location...";
                GeofenceLabel.Text = "Waiting for GPS...";
                GeofenceLabel.TextColor = Amber;
                RetryLocationBtn.IsVisible = false;
            });

            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LocationLabel.Text = "Location permission denied";
                    GeofenceLabel.Text = "Permission required";
                    GeofenceLabel.TextColor = Red;
                    RetryLocationBtn.IsVisible = true;
                });
                UpdateClockInButton();
                return;
            }

            Location? location = null;

            location = await Geolocation.GetLastKnownLocationAsync();

            if (location == null)
            {
                try
                {
                    location = await Geolocation.GetLocationAsync(
                        new GeolocationRequest(GeolocationAccuracy.Medium,
                            TimeSpan.FromSeconds(30)));
                }
                catch { }
            }

            if (location == null)
            {
                try
                {
                    location = await Geolocation.GetLocationAsync(
                        new GeolocationRequest(GeolocationAccuracy.Low,
                            TimeSpan.FromSeconds(30)));
                }
                catch { }
            }

            if (location == null)
            {
                try
                {
                    location = await Geolocation.GetLocationAsync(
                        new GeolocationRequest(GeolocationAccuracy.Lowest,
                            TimeSpan.FromSeconds(30)));
                }
                catch { }
            }

            if (location != null)
            {
                _workerLat = location.Latitude;
                _workerLng = location.Longitude;
                _gpsReady = true;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LocationLabel.Text = $"Lat {_workerLat:F4}, Lng {_workerLng:F4}";
                    RetryLocationBtn.IsVisible = false;
                });

                if (_selectedProject != null)
                    await CheckGeofence(_workerLat, _workerLng, _selectedProject.Location);

                UpdateClockInButton();
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LocationLabel.Text = "Cannot get location";
                    GeofenceLabel.Text = "GPS unavailable";
                    GeofenceLabel.TextColor = Red;
                    RetryLocationBtn.IsVisible = true;
                });
                _gpsReady = false;
                UpdateClockInButton();
            }
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LocationLabel.Text = "Location error";
                GeofenceLabel.Text = "Location check failed";
                GeofenceLabel.TextColor = Red;
                RetryLocationBtn.IsVisible = true;
            });
            System.Diagnostics.Debug.WriteLine($"GPS error: {ex.Message}");
            _gpsReady = false;
            UpdateClockInButton();
        }
    }

    private void OnRetryLocation(object sender, EventArgs e)
    {
        _gpsReady = false;
        _isOnSite = false;
        GetLocation();
    }

    private void UpdateClockInButton()
    {
        if (_isClockedIn) return;

        bool projectOk = _selectedProject != null;
        bool costCodeOk = !string.IsNullOrEmpty(_selectedCostCode);
        bool locationOk = _gpsReady && _isOnSite;
        bool canClockIn = projectOk && costCodeOk && locationOk;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            ClockInBtn.IsEnabled = canClockIn;
            ClockInBtn.BackgroundColor = canClockIn ? Green : DisabledBg;
            ClockInBtn.TextColor = canClockIn ? Colors.White : Muted;

            if (!projectOk)
            {
                StatusLabel.Text = "SELECT A PROJECT";
                StatusLabel.TextColor = Muted;
            }
            else if (!costCodeOk)
            {
                StatusLabel.Text = "SELECT A COST CODE";
                StatusLabel.TextColor = Muted;
            }
            else if (!_gpsReady)
            {
                StatusLabel.Text = "GETTING GPS LOCATION...";
                StatusLabel.TextColor = Amber;
            }
            else if (!_isOnSite)
            {
                StatusLabel.Text = "NOT AT JOB SITE - CANNOT CLOCK IN";
                StatusLabel.TextColor = Red;
            }
            else
            {
                StatusLabel.Text = "READY TO CLOCK IN";
                StatusLabel.TextColor = Green;
            }
        });
    }

    private async void OnClockIn(object sender, EventArgs e)
    {
        if (_selectedProject == null)
        {
            await DisplayAlert("Error", "Please select a project.", "OK");
            return;
        }

        if (string.IsNullOrEmpty(_selectedCostCode))
        {
            await DisplayAlert("Error", "Please select a cost code.", "OK");
            return;
        }

        if (!_gpsReady)
        {
            await DisplayAlert("Location Required",
                "Your location could not be determined. Please enable location services and try again.",
                "OK");
            return;
        }

        if (!_isOnSite)
        {
            await DisplayAlert("Not at Job Site",
                $"You must be at the job site to clock in.\n\nProject: {_selectedProject.Name}\n\nPlease go to the job site and try again.",
                "OK");
            return;
        }

        // Safety check gate
        var safetyPage = new SafetyCheckPage();
        await Application.Current!.MainPage!.Navigation.PushModalAsync(safetyPage);
        var safetyPassed = await safetyPage.Result.Task;
        if (!safetyPassed) return;

        ClockInBtn.IsEnabled = false;
        StatusLabel.Text = "CLOCKING IN...";
        StatusLabel.TextColor = Amber;

        try
        {
            var result = await _api.ClockInAsync(
                _selectedProject.Id,
                _workerLat, _workerLng,
                $"Cost Code: {_selectedCostCode}");

            if (result != null)
            {
                _activeTimesheetId = result.TimesheetId;
                _clockInTime = (result.ClockInTime ?? DateTime.UtcNow).ToLocalTime();
                _isClockedIn = true;
                _onBreak = false;
                _breakMinutesAccum = 0;

                ApplyClockedInUi();
                StartLocalTimer();
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ClockInBtn.IsEnabled = true;
                    StatusLabel.Text = "READY TO CLOCK IN";
                    StatusLabel.TextColor = Green;
                });
                await DisplayAlert("Clock In Failed",
                    "Could not clock in. You may already be clocked in today.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ClockInBtn.IsEnabled = true;
                StatusLabel.Text = "READY TO CLOCK IN";
            });
            await DisplayAlert("Error", $"Clock in error: {ex.Message}", "OK");
        }
    }

    private async void OnBreak(object sender, EventArgs e)
    {
        if (!_isClockedIn || _activeTimesheetId == 0) return;

        BreakBtn.IsEnabled = false;
        try
        {
            if (!_onBreak)
            {
                var result = await _api.StartBreakAsync(_activeTimesheetId);
                if (result != null)
                {
                    _onBreak = true;
                    _breakStartLocal = DateTime.Now;
                    ApplyClockedInUi();
                }
                else
                {
                    await DisplayAlert("Break", _api.LastError ?? "Could not start break.", "OK");
                }
            }
            else
            {
                var result = await _api.EndBreakAsync(_activeTimesheetId);
                if (result != null)
                {
                    _onBreak = false;
                    _breakMinutesAccum = result.BreakMinutes;
                    ApplyClockedInUi();
                }
                else
                {
                    await DisplayAlert("Break", _api.LastError ?? "Could not end break.", "OK");
                }
            }
        }
        finally
        {
            MainThread.BeginInvokeOnMainThread(() => BreakBtn.IsEnabled = _isClockedIn);
        }
    }

    private async void OnClockOut(object sender, EventArgs e)
    {
        if (_activeTimesheetId == 0)
        {
            await DisplayAlert("Error", "No active timesheet found.", "OK");
            return;
        }

        // Injury report gate
        var injuryPage = new InjuryReportPage
        {
            ElapsedHours = TimerLabel.Text,
            ProjectName = _selectedProject?.Name ?? "",
            TimesheetId = _activeTimesheetId.ToString()
        };
        await Application.Current!.MainPage!.Navigation.PushModalAsync(injuryPage);
        var injuryResult = await injuryPage.Result.Task;
        if (!injuryResult.Confirmed) return;

        ClockOutBtn.IsEnabled = false;

        try
        {
            var result = await _api.ClockOutAsync(
                _activeTimesheetId, _workerLat, _workerLng,
                injuryResult.InjuryReported, injuryResult.InjuryDetails);

            if (result != null)
            {
                StopTimer();
                await LoadSummary();
                await DisplayAlert("Clocked Out",
                    $"Total: {result.TotalHours:F2}h\nRegular: {result.HoursWorked:F2}h\nOvertime: {result.OvertimeHours:F2}h",
                    "OK");
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() => ClockOutBtn.IsEnabled = true);
                await DisplayAlert("Error", "Could not clock out. Please try again.", "OK");
            }
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(() => ClockOutBtn.IsEnabled = true);
            await DisplayAlert("Error", $"Clock out error: {ex.Message}", "OK");
        }
    }

    private void StartLocalTimer()
    {
        _timer?.Stop();
        _timer = new System.Timers.Timer(1000);
        _timer.Elapsed += (s, e) =>
        {
            var elapsed = CurrentElapsed();
            var breakLive = _breakMinutesAccum + (_onBreak ? (int)(DateTime.Now - _breakStartLocal).TotalMinutes : 0);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                TimerLabel.Text = elapsed.ToString(@"hh\:mm\:ss");
                TodayHoursLabel.Text = $"{elapsed.TotalHours:F1}h";
                BreakTotalLabel.Text = $"{breakLive}m";
                _ring.Progress = (float)(elapsed.TotalHours / 8.0);
                RingView.Invalidate();
            });
        };
        _timer.Start();
    }

    public async Task StartTimer(double lat, double lng)
    {
        _workerLat = lat;
        _workerLng = lng;
        OnClockIn(this, EventArgs.Empty);
        await Task.CompletedTask;
    }

    private async Task CheckGeofence(double workerLat, double workerLng, string projectAddress)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(projectAddress))
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    GeofenceLabel.Text = "No address on project";
                    GeofenceLabel.TextColor = Red;
                    GeoIconBox.BackgroundColor = Color.FromArgb("#fde8e8");
                    GeoIconLabel.TextColor = Red;
                });
                _isOnSite = false;
                UpdateClockInButton();
                return;
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                GeofenceLabel.Text = "Checking location...";
                GeofenceLabel.TextColor = Amber;
                GeoIconBox.BackgroundColor = Color.FromArgb("#fdf3dd");
                GeoIconLabel.TextColor = Amber;
            });

            var cleanAddress = projectAddress.Trim();
            var digitIndex = cleanAddress.IndexOfAny("0123456789".ToCharArray());
            if (digitIndex > 0)
                cleanAddress = cleanAddress.Substring(digitIndex);

            System.Diagnostics.Debug.WriteLine($"Geocoding address: {cleanAddress}");

            var encoded = Uri.EscapeDataString(cleanAddress);
            using var http = new System.Net.Http.HttpClient();
            http.DefaultRequestHeaders.Add("User-Agent", "BuildForce/1.0");
            var json = await http.GetStringAsync(
                $"https://nominatim.openstreetmap.org/search?q={encoded}&format=json&limit=1");

            System.Diagnostics.Debug.WriteLine($"Geocode result: {json}");

            using var doc = JsonDocument.Parse(json);
            var results = doc.RootElement;

            if (results.GetArrayLength() == 0)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    GeofenceLabel.Text = "Address not found";
                    GeofenceLabel.TextColor = Red;
                    GeoIconBox.BackgroundColor = Color.FromArgb("#fde8e8");
                    GeoIconLabel.TextColor = Red;
                });
                _isOnSite = false;
                UpdateClockInButton();
                return;
            }

            var first = results[0];
            double projLat = double.Parse(first.GetProperty("lat").GetString()!,
                System.Globalization.CultureInfo.InvariantCulture);
            double projLng = double.Parse(first.GetProperty("lon").GetString()!,
                System.Globalization.CultureInfo.InvariantCulture);

            double distMeters = HaversineMeters(workerLat, workerLng, projLat, projLng);
            _isOnSite = distMeters <= 500;

            System.Diagnostics.Debug.WriteLine($"Distance to job site: {distMeters:F0}m - OnSite: {_isOnSite}");

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (_isOnSite)
                {
                    GeofenceLabel.Text = $"On site ({distMeters:F0}m from center)";
                    GeofenceLabel.TextColor = Green;
                    GeoIconBox.BackgroundColor = Color.FromArgb("#e7f8f1");
                    GeoIconLabel.TextColor = Green;
                }
                else
                {
                    GeofenceLabel.Text = $"Off site ({distMeters:F0}m away)";
                    GeofenceLabel.TextColor = Red;
                    GeoIconBox.BackgroundColor = Color.FromArgb("#fde8e8");
                    GeoIconLabel.TextColor = Red;
                }
            });

            UpdateClockInButton();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Geofence error: {ex.Message}");
            MainThread.BeginInvokeOnMainThread(() =>
            {
                GeofenceLabel.Text = "Location check failed";
                GeofenceLabel.TextColor = Red;
            });
            _isOnSite = false;
            UpdateClockInButton();
        }
    }

    private static double HaversineMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    public void StopTimer()
    {
        _timer?.Stop();
        _timer = null;
        _isClockedIn = false;
        _activeTimesheetId = 0;
        _isOnSite = false;
        _gpsReady = false;
        _onBreak = false;
        _breakMinutesAccum = 0;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            StatusLabel.Text = "NOT CLOCKED IN";
            StatusLabel.TextColor = Muted;
            TimerLabel.Text = "00:00:00";
            SetChip("CLOCKED OUT", Color.FromArgb("#eef1f8"), Muted);
            ClockedInBadge.IsVisible = false;
            ClockInBtn.IsVisible = true;
            ClockInBtn.IsEnabled = false;
            ClockInBtn.BackgroundColor = DisabledBg;
            ClockOutBtn.IsEnabled = false;
            ClockOutBtn.BackgroundColor = Color.FromArgb("#fca5a5");
            BreakBtn.IsEnabled = false;
            SetBreakBtnActive(false);
            _ring.Progress = 0f;
            _ring.RingColor = DisabledBg;
            RingView.Invalidate();
        });

        GetLocation();
    }

    public bool IsClockedIn => _isClockedIn;
}