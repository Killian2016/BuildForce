$modelsPath = "C:\Users\mezan\source\repos\BuildForce\BuildForce\Services\ApiModels.cs"
$apiServicePath = "C:\Users\mezan\source\repos\BuildForce\BuildForce\Services\ApiService.cs"

@'
namespace BuildForce.Services;
public class DashboardData
{
    public decimal TotalRevenue { get; set; }
    public decimal OutstandingBalance { get; set; }
    public int ActiveProjects { get; set; }
    public int TotalProjects { get; set; }
    public int TotalCustomers { get; set; }
    public int PendingInvoices { get; set; }
    public int PaidInvoices { get; set; }
    public decimal Expenses { get; set; }
    public List<ProjectSummary> RecentProjects { get; set; } = new();
    public decimal Revenue => TotalRevenue;
    public decimal Pending => OutstandingBalance;
    public decimal NetProfit => TotalRevenue - Expenses;
}
public class ProjectSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? CustomerName { get; set; }
    public string Client => CustomerName ?? "";
    public string Status { get; set; } = "";
    public string Location { get; set; } = "";
}
public class InvoiceSummary
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = "";
    public string Number => InvoiceNumber;
    public string? ProjectName { get; set; }
    public string Project => ProjectName ?? "";
    public string? CustomerName { get; set; }
    public string Client => CustomerName ?? "";
    public decimal TotalAmount { get; set; }
    public decimal Amount => TotalAmount;
    public string Status { get; set; } = "";
    public DateTime InvoiceDate { get; set; }
    public DateTime Date => InvoiceDate;
}
public class LoginResult
{
    public bool Success { get; set; }
    public string Token { get; set; } = "";
    public string Message { get; set; } = "";
}
public class TimesheetEntry
{
    public int TimesheetId { get; set; }
    public int EmployeeId { get; set; }
    public int ProjectId { get; set; }
    public DateTime Date { get; set; }
    public decimal HoursWorked { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal TotalHours => HoursWorked + OvertimeHours;
    public string? Description { get; set; }
    public string Status { get; set; } = "";
    public DateTime CreatedDate { get; set; }
    public DateTime? ClockInTime { get; set; }
    public DateTime? ClockOutTime { get; set; }
    public double? ClockInLatitude { get; set; }
    public double? ClockInLongitude { get; set; }
    public double? ClockOutLatitude { get; set; }
    public double? ClockOutLongitude { get; set; }
    public string? EmployeeName { get; set; }
    public string? ProjectName { get; set; }
}
public class ClockInResult
{
    public int TimesheetId { get; set; }
    public int ProjectId { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = "";
    public DateTime? ClockInTime { get; set; }
    public string Message { get; set; } = "";
}
public class ClockOutResult
{
    public int TimesheetId { get; set; }
    public decimal HoursWorked { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal TotalHours { get; set; }
    public string Status { get; set; } = "";
    public string Message { get; set; } = "";
}
public class TimesheetSummary
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalRegularHours { get; set; }
    public decimal TotalOvertimeHours { get; set; }
    public decimal TotalHours { get; set; }
    public int DaysWorked { get; set; }
    public decimal AverageHoursPerDay { get; set; }
}
public class CustomerSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Company { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
public class ProjectCreateResult
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Status { get; set; } = "";
    public string Message { get; set; } = "";
}
public class ExpenseCreateResult
{
    public int Id { get; set; }
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
    public string Message { get; set; } = "";
}
'@ | Set-Content -Path $modelsPath -Encoding UTF8

@'
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
            BaseAddress = new Uri("https://mezanoconstructionmanagementplatform.com"),
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
        try { RefreshToken(); return await _client.GetFromJsonAsync<DashboardData>("/api/mobile/dashboard"); }
        catch { return null; }
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
'@ | Set-Content -Path $apiServicePath -Encoding UTF8

Write-Host "DONE: ApiModels.cs and ApiService.cs overwritten successfully" -ForegroundColor Green