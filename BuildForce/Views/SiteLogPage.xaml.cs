#pragma warning disable CA1416
using BuildForce.Services;
using System.Text.Json;

namespace BuildForce.Views;

public partial class SiteLogPage : ContentPage
{
    private readonly ApiService _api;
    private List<ProjectSummary> _projects = new();
    private bool _wired = false;

    public SiteLogPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        DateLabel.Text = DateTime.Today.ToString("dddd, MMMM d, yyyy");
        LoadAsync();
    }

    private ProjectSummary? Sel =>
        (ProjectPicker.SelectedIndex >= 0 && ProjectPicker.SelectedIndex < _projects.Count)
            ? _projects[ProjectPicker.SelectedIndex] : null;

    private async void LoadAsync()
    {
        _projects = await _api.GetProjectsAsync();
        var active = await _api.GetActiveTimesheetAsync();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            ProjectPicker.Items.Clear();
            foreach (var p in _projects) ProjectPicker.Items.Add(p.Name);
            int idx = active != null ? _projects.FindIndex(p => p.Id == active.ProjectId) : -1;
            if (idx < 0 && _projects.Count > 0) idx = 0;
            if (!_wired)
            {
                ProjectPicker.SelectedIndexChanged += (s, e) => _ = OnProjectChangedAsync();
                _wired = true;
            }
            ProjectPicker.SelectedIndex = idx; // fires OnProjectChangedAsync
        });

        _ = FillWeatherAsync();
    }

    private async Task OnProjectChangedAsync()
    {
        var p = Sel;
        if (p == null) return;

        var log = await _api.GetSiteLogTodayAsync(p.Id);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (log != null)
            {
                if (!string.IsNullOrWhiteSpace(log.Weather)) WeatherEntry.Text = log.Weather;
                CrewEntry.Text = log.CrewSummary ?? "";
                CrewCountEntry.Text = log.CrewCount > 0 ? log.CrewCount.ToString() : "";
                WorkEditor.Text = log.WorkCompleted ?? "";
                IssuesEditor.Text = log.IssuesDelays ?? "";
                MaterialsEditor.Text = log.MaterialsDelivered ?? "";
                NotesEntry.Text = log.Notes ?? "";
                SetStatus("Editing today's log for " + p.Name, null);
            }
        });

        // Crew autofill (managers get the live list; others skip silently)
        var crew = await _api.GetActiveCrewAsync();
        if (crew != null)
        {
            var onProject = crew.Where(c => c.ProjectId == p.Id)
                                .Select(c => c.EmployeeName ?? "")
                                .Where(n => n.Length > 0)
                                .ToList();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (string.IsNullOrWhiteSpace(CrewEntry.Text) && onProject.Count > 0)
                {
                    CrewEntry.Text = string.Join(", ", onProject);
                    CrewCountEntry.Text = onProject.Count.ToString();
                }
            });
        }
    }

    private async Task FillWeatherAsync()
    {
        try
        {
            var loc = await Geolocation.GetLastKnownLocationAsync();
            if (loc == null)
            {
                try
                {
                    loc = await Geolocation.GetLocationAsync(
                        new GeolocationRequest(GeolocationAccuracy.Low, TimeSpan.FromSeconds(15)));
                }
                catch { }
            }
            if (loc == null) return;

            using var http = new HttpClient();
            var url = "https://api.open-meteo.com/v1/forecast?latitude=" + loc.Latitude.ToString("F4", System.Globalization.CultureInfo.InvariantCulture) +
                      "&longitude=" + loc.Longitude.ToString("F4", System.Globalization.CultureInfo.InvariantCulture) +
                      "&current=temperature_2m,weather_code&temperature_unit=fahrenheit";
            var json = await http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            var cur = doc.RootElement.GetProperty("current");
            var temp = cur.GetProperty("temperature_2m").GetDouble();
            var code = cur.GetProperty("weather_code").GetInt32();

            string desc;
            if (code == 0) desc = "Clear";
            else if (code <= 2) desc = "Partly cloudy";
            else if (code == 3) desc = "Overcast";
            else if (code == 45 || code == 48) desc = "Fog";
            else if (code >= 51 && code <= 67) desc = "Rain";
            else if (code >= 71 && code <= 77) desc = "Snow";
            else if (code >= 80 && code <= 82) desc = "Showers";
            else if (code >= 95) desc = "Thunderstorm";
            else desc = "";

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (string.IsNullOrWhiteSpace(WeatherEntry.Text))
                    WeatherEntry.Text = temp.ToString("F0") + "F" + (desc.Length > 0 ? ", " + desc : "");
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Weather error: " + ex.Message);
        }
    }

    private async void OnSave(object sender, EventArgs e)
    {
        var p = Sel;
        if (p == null) { SetStatus("Select a project first.", false); return; }

        SaveBtn.IsEnabled = false;
        SetStatus("Saving...", null);
        int.TryParse(CrewCountEntry.Text, out int crewCount);

        var ok = await _api.SaveSiteLogAsync(new SiteLogSave
        {
            ProjectId = p.Id,
            Weather = WeatherEntry.Text,
            CrewSummary = CrewEntry.Text,
            CrewCount = crewCount,
            WorkCompleted = WorkEditor.Text,
            IssuesDelays = IssuesEditor.Text,
            MaterialsDelivered = MaterialsEditor.Text,
            Notes = NotesEntry.Text
        });

        MainThread.BeginInvokeOnMainThread(() =>
        {
            SaveBtn.IsEnabled = true;
            if (ok) SetStatus("Saved \u2713  " + p.Name + " - " + DateTime.Today.ToString("MMM d"), true);
            else SetStatus(_api.LastError ?? "Could not save - try again.", false);
        });
    }

    private void SetStatus(string msg, bool? good)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            SaveStatusLabel.Text = msg;
            SaveStatusLabel.IsVisible = !string.IsNullOrEmpty(msg);
            SaveStatusLabel.TextColor = good == true ? Color.FromArgb("#10b981")
                : good == false ? Color.FromArgb("#ef4444")
                : Color.FromArgb("#f0a500");
        });
    }

    private async void OnClose(object sender, EventArgs e)
    {
        try { await Navigation.PopModalAsync(); } catch { }
    }
}