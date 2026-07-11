#pragma warning disable CA1416
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;

namespace BuildForce.Services;

public class ApiService
{
    private readonly HttpClient _client;

    // Human-readable error from the last failed create/scan call (parsed from { error } body)
    public string? LastError { get; private set; }

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
            Timeout = TimeSpan.FromSeconds(90) // raised from 30s: receipt scan uploads full-size photos as base64
        };
    }

    private void RefreshToken()
    {
        var token = Preferences.Get("auth_token", "");
        _client.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token)
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
    }

    private static string? TryParseError(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                doc.RootElement.TryGetProperty("error", out var err))
                return err.GetString();
        }
        catch { }
        return null;
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

    // ============================================
    // NEW: Invoice create (POST /api/mobile/invoices)
    // ============================================
    public async Task<InvoiceCreateResult?> CreateInvoiceAsync(
        int projectId, DateTime? invoiceDate, DateTime? dueDate,
        decimal discountPercentage, string? notes, List<MobileLineItem> lineItems)
    {
        LastError = null;
        try
        {
            RefreshToken();
            var response = await _client.PostAsJsonAsync("/api/mobile/invoices", new
            {
                projectId,
                invoiceDate,
                dueDate,
                discountPercentage,
                notes,
                lineItems
            });
            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"CreateInvoice response ({response.StatusCode}): {json}");
            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<InvoiceCreateResult>(json, options);
            }
            LastError = TryParseError(json) ?? $"Server returned {(int)response.StatusCode}";
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CreateInvoice exception: {ex.Message}");
            LastError = ex.Message;
            return null;
        }
    }

    // ============================================
    // NEW: Estimate create (POST /api/mobile/estimates)
    // ============================================
    public async Task<EstimateCreateResult?> CreateEstimateAsync(
        int projectId, DateTime? estimateDate, DateTime? validUntil,
        decimal discountPercentage, string? notes, List<MobileLineItem> lineItems)
    {
        LastError = null;
        try
        {
            RefreshToken();
            var response = await _client.PostAsJsonAsync("/api/mobile/estimates", new
            {
                projectId,
                estimateDate,
                validUntil,
                discountPercentage,
                notes,
                lineItems
            });
            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"CreateEstimate response ({response.StatusCode}): {json}");
            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<EstimateCreateResult>(json, options);
            }
            LastError = TryParseError(json) ?? $"Server returned {(int)response.StatusCode}";
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CreateEstimate exception: {ex.Message}");
            LastError = ex.Message;
            return null;
        }
    }

    // ============================================
    // NEW: Receipt scan preview (POST /api/mobile/expenses/scan/preview)
    // Does NOT create an expense - returns extracted fields for form prefill
    // ============================================
    public async Task<ReceiptScanPreview?> ScanReceiptPreviewAsync(string receiptBase64, string? fileName)
    {
        LastError = null;
        try
        {
            RefreshToken();
            var response = await _client.PostAsJsonAsync("/api/mobile/expenses/scan/preview", new
            {
                receiptBase64,
                fileName
            });
            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"ScanReceiptPreview response ({response.StatusCode}): {json}");
            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<ReceiptScanPreview>(json, options);
            }
            LastError = TryParseError(json) ?? $"Server returned {(int)response.StatusCode}";
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ScanReceiptPreview exception: {ex.Message}");
            LastError = ex.Message;
            return null;
        }
    }

    public async Task<ProjectDetail?> GetProjectDetailAsync(int id)
    {
        try
        {
            RefreshToken();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var json = await _client.GetStringAsync($"/api/mobile/projects/{id}");
            return JsonSerializer.Deserialize<ProjectDetail>(json, options);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetProjectDetail exception: {ex.Message}");
            return null;
        }
    }

    public async Task<InvoiceDetail?> GetInvoiceDetailAsync(int id)
    {
        try
        {
            RefreshToken();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var json = await _client.GetStringAsync($"/api/mobile/invoices/{id}");
            return JsonSerializer.Deserialize<InvoiceDetail>(json, options);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetInvoiceDetail exception: {ex.Message}");
            return null;
        }
    }

    public async Task<List<EstimateSummary>> GetEstimatesAsync()
    {
        try { RefreshToken(); return await _client.GetFromJsonAsync<List<EstimateSummary>>("/api/mobile/estimates") ?? new(); }
        catch { return new(); }
    }

    public async Task<EstimateDetail?> GetEstimateDetailAsync(int id)
    {
        try
        {
            RefreshToken();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var json = await _client.GetStringAsync($"/api/mobile/estimates/{id}");
            return JsonSerializer.Deserialize<EstimateDetail>(json, options);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetEstimateDetail exception: {ex.Message}");
            return null;
        }
    }

    public async Task<List<ExpenseSummary>> GetExpensesAsync()
    {
        try { RefreshToken(); return await _client.GetFromJsonAsync<List<ExpenseSummary>>("/api/mobile/expenses") ?? new(); }
        catch { return new(); }
    }

    public async Task<ExpenseDetail?> GetExpenseDetailAsync(int id)
    {
        try
        {
            RefreshToken();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var json = await _client.GetStringAsync($"/api/mobile/expenses/{id}");
            return JsonSerializer.Deserialize<ExpenseDetail>(json, options);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetExpenseDetail exception: {ex.Message}");
            return null;
        }
    }

    // ============================================
    // Document delivery: PDF download + email send
    // ============================================
    public async Task<byte[]?> GetEstimatePdfAsync(int id) => await GetPdfAsync($"/api/mobile/documents/estimates/{id}/pdf");
    public async Task<byte[]?> GetInvoicePdfAsync(int id) => await GetPdfAsync($"/api/mobile/documents/invoices/{id}/pdf");

    private async Task<byte[]?> GetPdfAsync(string url)
    {
        LastError = null;
        try
        {
            RefreshToken();
            var response = await _client.GetAsync(url);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsByteArrayAsync();
            var json = await response.Content.ReadAsStringAsync();
            LastError = TryParseError(json) ?? $"Server returned {(int)response.StatusCode}";
            return null;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return null;
        }
    }

    public async Task<string?> EmailEstimateAsync(int id, string? recipientEmail) =>
        await EmailDocumentAsync($"/api/mobile/documents/estimates/{id}/email", recipientEmail);
    public async Task<string?> EmailInvoiceAsync(int id, string? recipientEmail) =>
        await EmailDocumentAsync($"/api/mobile/documents/invoices/{id}/email", recipientEmail);

    // Returns the success message, or null (check LastError)
    private async Task<string?> EmailDocumentAsync(string url, string? recipientEmail)
    {
        LastError = null;
        try
        {
            RefreshToken();
            var response = await _client.PostAsJsonAsync(url, new { recipientEmail });
            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"EmailDocument response ({response.StatusCode}): {json}");
            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("message", out var msg))
                    return msg.GetString();
                if (doc.RootElement.TryGetProperty("Message", out var msg2))
                    return msg2.GetString();
                return "Email sent.";
            }
            LastError = TryParseError(json) ?? $"Server returned {(int)response.StatusCode}";
            return null;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
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

    public async Task<BreakResult?> StartBreakAsync(int timesheetId)
    {
        try
        {
            RefreshToken();
            var response = await _client.PostAsync($"/api/mobile/timesheets/break/start/{timesheetId}", null);
            if (!response.IsSuccessStatusCode)
            {
                LastError = await response.Content.ReadAsStringAsync();
                return null;
            }
            var json = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<BreakResult>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return null;
        }
    }

    public async Task<BreakResult?> EndBreakAsync(int timesheetId)
    {
        try
        {
            RefreshToken();
            var response = await _client.PostAsync($"/api/mobile/timesheets/break/end/{timesheetId}", null);
            if (!response.IsSuccessStatusCode)
            {
                LastError = await response.Content.ReadAsStringAsync();
                return null;
            }
            var json = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<BreakResult>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return null;
        }
    }

    public async Task<ClockInResult?> ClockInAsync(int projectId, double lat, double lng, string? description = null, string? photoBase64 = null)
    {
        try
        {
            RefreshToken();
            var response = await _client.PostAsJsonAsync("/api/mobile/timesheets/clockin", new
            {
                projectId,
                latitude = lat,
                longitude = lng,
                description,
                photoBase64
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

    public async Task<ClockOutResult?> ClockOutAsync(int timesheetId, double lat, double lng, bool injuryReported = false, string? injuryDetails = null, string? photoBase64 = null)
    {
        try
        {
            RefreshToken();
            var response = await _client.PostAsJsonAsync($"/api/mobile/timesheets/clockout/{timesheetId}", new
            {
                latitude = lat,
                longitude = lng,
                injuryReported,
                injuryDetails,
                photoBase64
            });
            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"ClockOut response ({response.StatusCode}): {json}");
            if (!response.IsSuccessStatusCode)
            {
                await Microsoft.Maui.Controls.Application.Current!.MainPage!.DisplayAlert("Clock Out Failed",
                    $"Status: {response.StatusCode}\n{(json.Length > 300 ? json.Substring(0, 300) : json)}", "OK");
            }
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


