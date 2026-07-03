#pragma warning disable CA1416
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;

namespace BuildForce.Services;

public class ApiService
{
    private readonly HttpClient _client;

    public ApiService()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        _client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://mezanocm.com"),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    private void RefreshToken()
    {
        var token = Preferences.Get("auth_token", "");
        _client.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token)
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<DashboardData?> GetDashboardAsync()
    {
        try
        {
            RefreshToken();
            var response = await _client.GetAsync("/api/mobile/dashboard");
            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"GetDashboard response ({response.StatusCode}): {json}");
            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<DashboardData>(json, options);
            }
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetDashboard exception: {ex.Message}");
            return null;
        }
    }

    public async Task<List<InvoiceSummary>> GetInvoicesAsync()
    {
        try { RefreshToken(); return await _client.GetFromJsonAsync<List<InvoiceSummary>>("/api/mobile/invoices") ?? new(); }
        catch { return new(); }
    }

    public async Task<List<ProjectSummary>> GetProjectsAsync()
    {
        try { RefreshToken(); return await _client.GetFromJsonAsync<List<ProjectSummary>>("/api/mobile/projects") ?? new(); }
        catch { return new(); }
    }

    public async Task<List<CustomerSummary>> GetCustomersAsync()
    {
        try { RefreshToken(); return await _client.GetFromJsonAsync<List<CustomerSummary>>("/api/mobile/customers") ?? new(); }
        catch { return new(); }
    }

    public async Task<ProjectCreateResult?> CreateProjectAsync(
        int customerId, string name, string? description, string? location,
        string? status, decimal budget, DateTime? startDate, DateTime? endDate)
    {
        try
        {
            RefreshToken();
            var response = await _client.PostAsJsonAsync("/api/mobile/projects", new
            {
                customerId,
                name,
                description,
                location,
                status,
                budget,
                startDate,
                endDate
            });
            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"CreateProject response ({response.StatusCode}): {json}");
            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<ProjectCreateResult>(json, options);
            }
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CreateProject exception: {ex.Message}");
            return null;
        }
    }

    public async Task<List<string>> GetExpenseCategoriesAsync()
    {
        try
        {
            var json = await _client.GetStringAsync("/api/mobile/expenses/categories");
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<List<string>>(json, options) ?? new();
        }
        catch { return new(); }
    }

    public async Task<ExpenseCreateResult?> CreateExpenseAsync(
        int projectId, string description, decimal amount, DateTime? expenseDate,
        string? category, string? vendor, string? notes)
    {
        try
        {
            RefreshToken();
            var response = await _client.PostAsJsonAsync("/api/mobile/expenses", new
            {
                projectId,
                description,
                amount,
                expenseDate,
                category,
                vendor,
                notes
            });
            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"CreateExpense response ({response.StatusCode}): {json}");
            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<ExpenseCreateResult>(json, options);
            }
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CreateExpense exception: {ex.Message}");
            return null;
        }
    }

    public async Task<TimesheetEntry?> GetActiveTimesheetAsync()
    {
        try
        {
            RefreshToken();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var json = await _client.GetStringAsync("/api/mobile/timesheets/active");
            if (string.IsNullOrWhiteSpace(json) || json == "null") return null;
            return JsonSerializer.Deserialize<TimesheetEntry>(json, options);
        }
        catch { return null; }
    }

    public async Task<ClockInResult?> ClockInAsync(int projectId, double lat, double lng, string? description = null)
    {
        try
        {
            RefreshToken();
            var response = await _client.PostAsJsonAsync("/api/mobile/timesheets/clockin", new
            {
                projectId,
                latitude = lat,
                longitude = lng,
                description
            });
            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"ClockIn response ({response.StatusCode}): {json}");
            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<ClockInResult>(json, options);
            }
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ClockIn exception: {ex.Message}");
            return null;
        }
    }

    public async Task<ClockOutResult?> ClockOutAsync(int timesheetId, double lat, double lng)
    {
        try
        {
            RefreshToken();
            var response = await _client.PostAsJsonAsync($"/api/mobile/timesheets/clockout/{timesheetId}", new
            {
                latitude = lat,
                longitude = lng
            });
            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"ClockOut response ({response.StatusCode}): {json}");
            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<ClockOutResult>(json, options);
            }
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ClockOut exception: {ex.Message}");
            return null;
        }
    }

    public async Task<List<TimesheetEntry>> GetTimesheetsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            RefreshToken();
            var url = "/api/mobile/timesheets";
            if (startDate.HasValue) url += $"?startDate={startDate:yyyy-MM-dd}";
            if (endDate.HasValue) url += (url.Contains("?") ? "&" : "?") + $"endDate={endDate:yyyy-MM-dd}";
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var json = await _client.GetStringAsync(url);
            return JsonSerializer.Deserialize<List<TimesheetEntry>>(json, options) ?? new();
        }
        catch { return new(); }
    }

    public async Task<TimesheetSummary?> GetTimesheetSummaryAsync()
    {
        try
        {
            RefreshToken();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var json = await _client.GetStringAsync("/api/mobile/timesheets/summary");
            return JsonSerializer.Deserialize<TimesheetSummary>(json, options);
        }
        catch { return null; }
    }
}


