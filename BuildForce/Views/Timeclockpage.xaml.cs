#pragma warning disable CA1416
using BuildForce.Services;
using System.Text.Json;

namespace BuildForce.Views;

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

            StatusLabel.Text = "CLOCKED IN";
            StatusLabel.TextColor = Color.FromArgb("#22c55e");
            TimerLabel.TextColor = Color.FromArgb("#22c55e");
            ClockedInBadge.IsVisible = true;
            ClockInBtn.IsVisible = false;
            ClockOutBtn.IsVisible = true;

            var proj = _projects.FirstOrDefault(p => p.Id == active.ProjectId);
            if (proj != null)
            {
                _selectedProject = proj;
                ProjectPicker.SelectedIndex = _projects.IndexOf(proj) + 1;
            }

            if (active.ClockInLatitude.HasValue)
            {
                LocationPill.IsVisible = true;
                LocationLabel.Text = $"Lat {active.ClockInLatitude:F4}, Lng {active.ClockInLongitude:F4}";
            }

            StartLocalTimer();
        }

        await LoadSummary();
    }

    private async Task LoadSummary()
    {
        var summary = await _api.GetTimesheetSummaryAsync();
        if (summary != null)
            WeekHoursLabel.Text = $"{summary.TotalHours:F1}h";

        var history = await _api.GetTimesheetsAsync(DateTime.Today, DateTime.Today);
        var todayHours = history.Where(t => t.Status != "Active").Sum(t => t.TotalHours);
        TodayHoursLabel.Text = $"{todayHours:F1}h";

        await LoadHistory();
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
                BackgroundColor = Color.FromArgb("#111f38"),
                Stroke = Color.FromArgb("#1e3a5f"),
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
                TextColor = Colors.White
            };
            var projLabel = new Label
            {
                Text = entry.ProjectName ?? "No Project",
                FontSize = 11,
                TextColor = Color.FromArgb("#c8e63c")
            };
            Grid.SetRow(projLabel, 1);

            var statusColor = entry.Status == "Completed" ? "#22c55e" : "#f59e0b";
            var statusLabel = new Label
            {
                Text = entry.Status,
                FontSize = 10,
                TextColor = Color.FromArgb(statusColor)
            };
            Grid.SetRow(statusLabel, 2);

            var clockIn = entry.ClockInTime?.ToLocalTime().ToString("h:mm tt") ?? "--";
            var clockOut = entry.ClockOutTime?.ToLocalTime().ToString("h:mm tt") ?? "Active";
            var hoursLabel = new Label
            {
                Text = $"{entry.TotalHours:F1}h\n{clockIn} - {clockOut}",
                FontSize = 11,
                TextColor = Color.FromArgb("#6b7280"),
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
                LocationPill.IsVisible = true;
                LocationLabel.Text = "Getting location...";
                LocationLabel.TextColor = Color.FromArgb("#f59e0b");
                RetryLocationBtn.IsVisible = false;
            });

            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LocationLabel.Text = "Location permission denied";
                    LocationLabel.TextColor = Color.FromArgb("#ef4444");
                    RetryLocationBtn.IsVisible = true;
                });
                UpdateClockInButton();
                return;
            }

            Location? location = null;

            // 1 — Last known (instant)
            location = await Geolocation.GetLastKnownLocationAsync();

            // 2 — Medium accuracy GPS (30s)
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

            // 3 — Low accuracy cell/WiFi — works indoors
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

            // 4 — Lowest accuracy last resort
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
                    LocationLabel.TextColor = Color.FromArgb("#22c55e");
                    RetryLocationBtn.IsVisible = false;
                });

                // Run geofence check now that GPS is ready
                if (_selectedProject != null)
                    await CheckGeofence(_workerLat, _workerLng, _selectedProject.Location);

                UpdateClockInButton();
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LocationLabel.Text = "Cannot get location";
                    LocationLabel.TextColor = Color.FromArgb("#ef4444");
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
                LocationLabel.TextColor = Color.FromArgb("#ef4444");
                RetryLocationBtn.IsVisible = true;
            });
            System.Diagnostics.Debug.WriteLine($"GPS error: {ex.Message}");
            _gpsReady = false;
            UpdateClockInButton();
        }
    }

    // Retry button handler
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
            ClockInBtn.BackgroundColor = canClockIn
                ? Color.FromArgb("#22c55e")
                : Color.FromArgb("#374151");
            ClockInBtn.TextColor = canClockIn
                ? Colors.Black
                : Color.FromArgb("#6b7280");

            if (!projectOk)
            {
                StatusLabel.Text = "Select a project";
                StatusLabel.TextColor = Color.FromArgb("#6b7280");
            }
            else if (!costCodeOk)
            {
                StatusLabel.Text = "Select a cost code";
                StatusLabel.TextColor = Color.FromArgb("#6b7280");
            }
            else if (!_gpsReady)
            {
                StatusLabel.Text = "Getting GPS location...";
                StatusLabel.TextColor = Color.FromArgb("#f59e0b");
            }
            else if (!_isOnSite)
            {
                StatusLabel.Text = "NOT AT JOB SITE — Cannot clock in";
                StatusLabel.TextColor = Color.FromArgb("#ef4444");
            }
            else
            {
                StatusLabel.Text = "READY TO CLOCK IN";
                StatusLabel.TextColor = Color.FromArgb("#22c55e");
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

        ClockInBtn.IsEnabled = false;
        StatusLabel.Text = "Clocking in...";
        StatusLabel.TextColor = Color.FromArgb("#f59e0b");

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

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    StatusLabel.Text = "CLOCKED IN";
                    StatusLabel.TextColor = Color.FromArgb("#22c55e");
                    TimerLabel.TextColor = Color.FromArgb("#22c55e");
                    ClockedInBadge.IsVisible = true;
                    ClockInBtn.IsVisible = false;
                    ClockOutBtn.IsVisible = true;
                    ActiveCostCodeLabel.Text = _selectedCostCode;
                    RetryLocationBtn.IsVisible = false;
                });

                StartLocalTimer();
                await DisplayAlert("Clocked In",
                    $"Project: {_selectedProject.Name}\nCost Code: {_selectedCostCode}\nTime: {_clockInTime:h:mm tt}",
                    "OK");
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ClockInBtn.IsEnabled = true;
                    StatusLabel.Text = "READY TO CLOCK IN";
                    StatusLabel.TextColor = Color.FromArgb("#22c55e");
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

    private async void OnClockOut(object sender, EventArgs e)
    {
        if (_activeTimesheetId == 0)
        {
            await DisplayAlert("Error", "No active timesheet found.", "OK");
            return;
        }

        ClockOutBtn.IsEnabled = false;

        try
        {
            var result = await _api.ClockOutAsync(_activeTimesheetId, _workerLat, _workerLng);

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
            var elapsed = DateTime.Now - _clockInTime;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                TimerLabel.Text = elapsed.ToString(@"hh\:mm\:ss");
                TodayHoursLabel.Text = $"{elapsed.TotalHours:F1}h";
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
                    GeofencePill.IsVisible = true;
                    GeofenceLabel.Text = "No address on project";
                    GeofenceLabel.TextColor = Color.FromArgb("#ef4444");
                    GeofencePill.Stroke = Color.FromArgb("#ef4444");
                });
                _isOnSite = false;
                UpdateClockInButton();
                return;
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                GeofencePill.IsVisible = true;
                GeofenceLabel.Text = "Checking location...";
                GeofenceLabel.TextColor = Color.FromArgb("#f59e0b");
                GeofencePill.Stroke = Color.FromArgb("#f59e0b");
            });

            // Strip leading name prefix — find first digit (street number)
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
                    GeofenceLabel.TextColor = Color.FromArgb("#ef4444");
                    GeofencePill.Stroke = Color.FromArgb("#ef4444");
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

            System.Diagnostics.Debug.WriteLine($"Distance to job site: {distMeters:F0}m — OnSite: {_isOnSite}");

            MainThread.BeginInvokeOnMainThread(() =>
            {
                GeofencePill.IsVisible = true;
                if (_isOnSite)
                {
                    GeofenceLabel.Text = $"✓ On Site ({distMeters:F0}m)";
                    GeofenceLabel.TextColor = Color.FromArgb("#22c55e");
                    GeofencePill.Stroke = Color.FromArgb("#22c55e");
                    GeofencePill.BackgroundColor = Color.FromArgb("#0d2e1a");
                }
                else
                {
                    GeofenceLabel.Text = $"✗ Off Site ({distMeters:F0}m away)";
                    GeofenceLabel.TextColor = Color.FromArgb("#ef4444");
                    GeofencePill.Stroke = Color.FromArgb("#ef4444");
                    GeofencePill.BackgroundColor = Color.FromArgb("#2e0d0d");
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
                GeofenceLabel.TextColor = Color.FromArgb("#ef4444");
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

        MainThread.BeginInvokeOnMainThread(() =>
        {
            StatusLabel.Text = "NOT CLOCKED IN";
            StatusLabel.TextColor = Color.FromArgb("#6b7280");
            TimerLabel.Text = "00:00:00";
            TimerLabel.TextColor = Colors.White;
            ClockedInBadge.IsVisible = false;
            ClockInBtn.IsVisible = true;
            ClockInBtn.IsEnabled = false;
            ClockOutBtn.IsVisible = false;
            ClockOutBtn.IsEnabled = true;
            ActiveCostCodeLabel.Text = "None";
            RetryLocationBtn.IsVisible = false;
        });

        GetLocation();
    }

    public bool IsClockedIn => _isClockedIn;
}