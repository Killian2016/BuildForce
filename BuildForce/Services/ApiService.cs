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

    // [SW6] Three-way: true=active, false=server says no shift, null=network/unknown.
    public async Task<bool?> IsShiftActiveAsync()
    {
        try
        {
            RefreshToken();
            var resp = await _client.GetAsync("/api/mobile/timesheets/active");
            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound) return false;
            if (!resp.IsSuccessStatusCode) return null;
            var body = await resp.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body) || body.Trim() == "null") return false;
            return true;
        }
        catch { return null; }
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
            System.Diagnostics.Debug.WriteLine("StartBreak response: " + json);
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
            System.Diagnostics.Debug.WriteLine("EndBreak response: " + json);
            return System.Text.Json.JsonSerializer.Deserialize<BreakResult>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return null;
        }
    }

    public async Task<ClockInResult?> ClockInAsync(int projectId, double lat, double lng, string? description = null, string? photoBase64 = null, string? clientPunchId = null, DateTime? occurredAt = null)
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
                photoBase64,
                clientPunchId,
                occurredAt,
                deviceNow = DateTime.UtcNow
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

    public async Task<ClockOutResult?> ClockOutAsync(int timesheetId, double lat, double lng, bool injuryReported = false, string? injuryDetails = null, string? photoBase64 = null, bool autoClockOut = false, DateTime? exitedAt = null, string? clientPunchId = null, string? clockInClientPunchId = null, DateTime? occurredAt = null)
    {
        try
        {
            RefreshToken();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));   // [OFF3d] punch only; shared client stays 90s for receipt uploads
            var response = await _client.PostAsJsonAsync($"/api/mobile/timesheets/clockout/{timesheetId}", new
            {
                latitude = lat,
                longitude = lng,
                injuryReported,
                injuryDetails,
                photoBase64,
                autoClockOut,
                exitedAt,
                clientPunchId,
                clockInClientPunchId,
                occurredAt,
                deviceNow = DateTime.UtcNow
            }, cts.Token);
            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"ClockOut response ({response.StatusCode}): {json}");
            if (!response.IsSuccessStatusCode)
            {
                // [SW2b-4] Never show raw JSON to a worker on a jobsite. Record
                // the reason and let the calling page decide what to say.
                LastError = TryParseError(json) ?? ("Server returned " + (int)response.StatusCode);
                System.Diagnostics.Debug.WriteLine("ClockOut failed: " + response.StatusCode + " " + json);
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
                // [OFF3a] Record WHY it failed. Without this a network drop
                // leaves LastError holding a stale message from an earlier
                // call, and the page cannot tell no-signal from server-refused.
                LastError = ex.Message;
            System.Diagnostics.Debug.WriteLine($"ClockOut exception: {ex.Message}");
            return null;
        }
    }

    public async Task<SwitchJobResult?> SwitchJobAsync(int projectId, double lat, double lng, string? photoBase64 = null, int materialRunCount = 0, int materialRunMinutes = 0)
    {
        try
        {
            LastError = null;
            RefreshToken();
            var response = await _client.PostAsJsonAsync("/api/mobile/timesheets/switch", new
            {
                projectId,
                latitude = lat,
                longitude = lng,
                photoBase64,
                materialRunCount,
                materialRunMinutes
            });
            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"SwitchJob response ({response.StatusCode}): {json}");
            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<SwitchJobResult>(json, options);
            }
            try
            {
                using var errDoc = JsonDocument.Parse(json);
                if (errDoc.RootElement.TryGetProperty("error", out var errEl))
                    LastError = errEl.GetString();
            }
            catch { }
            if (string.IsNullOrEmpty(LastError)) LastError = "Switch failed (" + (int)response.StatusCode + ")";
            return null;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            System.Diagnostics.Debug.WriteLine($"SwitchJob exception: {ex.Message}");
            return null;
        }
    }

    // Two-tap job switch with travel time.
    public async Task<LeaveJobResult?> LeaveJobAsync(double lat, double lng, int materialRunCount = 0, int materialRunMinutes = 0, string? photoBase64 = null)
    {
        try
        {
            LastError = null;
            RefreshToken();
            var response = await _client.PostAsJsonAsync("/api/mobile/timesheets/leave", new
            {
                latitude = lat,
                longitude = lng,
                materialRunCount,
                materialRunMinutes,
                photoBase64
            });
            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Leave response ({response.StatusCode}): {json}");
            if (response.IsSuccessStatusCode)
                return JsonSerializer.Deserialize<LeaveJobResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            try { using var d = JsonDocument.Parse(json); if (d.RootElement.TryGetProperty("error", out var er)) LastError = er.GetString(); } catch { }
            if (string.IsNullOrEmpty(LastError)) LastError = "Leave failed (" + (int)response.StatusCode + ")";
            return null;
        }
        catch (Exception ex) { LastError = ex.Message; return null; }
    }

    public async Task<SwitchJobResult?> ArriveJobAsync(int closedTimesheetId, int projectId, int travelMinutes, double lat, double lng, string? photoBase64 = null)
    {
        try
        {
            LastError = null;
            RefreshToken();
            var response = await _client.PostAsJsonAsync("/api/mobile/timesheets/arrive", new
            {
                closedTimesheetId,
                projectId,
                travelMinutes,
                latitude = lat,
                longitude = lng,
                photoBase64
            });
            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Arrive response ({response.StatusCode}): {json}");
            if (response.IsSuccessStatusCode)
                return JsonSerializer.Deserialize<SwitchJobResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            try { using var d = JsonDocument.Parse(json); if (d.RootElement.TryGetProperty("error", out var er)) LastError = er.GetString(); } catch { }
            if (string.IsNullOrEmpty(LastError)) LastError = "Arrive failed (" + (int)response.StatusCode + ")";
            return null;
        }
        catch (Exception ex) { LastError = ex.Message; return null; }
    }

    public async Task<LeaveJobResult?> CancelLeaveAsync(int closedTimesheetId)
    {
        try
        {
            LastError = null;
            RefreshToken();
            var response = await _client.PostAsJsonAsync("/api/mobile/timesheets/cancel-leave", new { closedTimesheetId });
            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"CancelLeave response ({response.StatusCode}): {json}");
            if (response.IsSuccessStatusCode)
                return JsonSerializer.Deserialize<LeaveJobResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            try { using var d = JsonDocument.Parse(json); if (d.RootElement.TryGetProperty("error", out var er)) LastError = er.GetString(); } catch { }
            if (string.IsNullOrEmpty(LastError)) LastError = "Cancel failed (" + (int)response.StatusCode + ")";
            return null;
        }
        catch (Exception ex) { LastError = ex.Message; return null; }
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

    public async Task<bool> FileSafetyInspectionAsync(SafetyInspectionSend req)
    {
        LastError = null;
        try
        {
            RefreshToken();
            var response = await _client.PostAsJsonAsync("/api/mobile/safety", req);
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                LastError = TryParseError(json) ?? ("Server returned " + (int)response.StatusCode);
                return false;
            }
            return true;
        }
        catch (Exception ex) { LastError = ex.Message; return false; }
    }
    public async Task<SiteLog?> GetSiteLogTodayAsync(int projectId)
    {
        try
        {
            RefreshToken();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var json = await _client.GetStringAsync("/api/mobile/sitelogs/today/" + projectId);
            if (string.IsNullOrWhiteSpace(json) || json == "null") return null;
            return JsonSerializer.Deserialize<SiteLog>(json, options);
        }
        catch { return null; }
    }

    public async Task<bool> SaveSiteLogAsync(SiteLogSave req)
    {
        LastError = null;
        try
        {
            RefreshToken();
            var response = await _client.PostAsJsonAsync("/api/mobile/sitelogs", req);
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                LastError = TryParseError(json) ?? ("Server returned " + (int)response.StatusCode);
                return false;
            }
            return true;
        }
        catch (Exception ex) { LastError = ex.Message; return false; }
    }
    public async Task<List<ScheduleItem>?> GetMyScheduleAsync(DateTime? date = null)
    {
        LastError = null;
        try
        {
            RefreshToken();
            var url = "/api/mobile/timesheets/schedule";
            if (date.HasValue) url += $"?date={date:yyyy-MM-dd}";
            var response = await _client.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<ScheduleItem>>(json, options) ?? new List<ScheduleItem>();
            }
            LastError = TryParseError(json) ?? ("Server returned " + (int)response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return null;
        }
    }
    public async Task<List<CrewMember>?> GetActiveCrewAsync()
    {
        LastError = null;
        try
        {
            RefreshToken();
            var response = await _client.GetAsync("/api/mobile/timesheets/crew/active");
            var json = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<CrewMember>>(json, options) ?? new List<CrewMember>();
            }
            LastError = TryParseError(json) ?? ("Server returned " + (int)response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return null;
        }
    }

    // ============================================
    // PROJECT PHOTOS (Step 3)
    // GET  /api/mobile/projects/{id}/photos   -> list newest-first
    // POST /api/mobile/projects/{id}/photos   -> upload base64, optional AI tag
    // ============================================
    public async Task<List<ProjectPhoto>?> GetProjectPhotosAsync(int projectId)
    {
        LastError = null;
        try
        {
            RefreshToken();
            var response = await _client.GetAsync($"/api/mobile/projects/{projectId}/photos");
            var json = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<ProjectPhoto>>(json, options) ?? new List<ProjectPhoto>();
            }
            LastError = TryParseError(json) ?? ("Server returned " + (int)response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetProjectPhotos exception: {ex.Message}");
            LastError = ex.Message;
            return null;
        }
    }

    public async Task<ProjectPhotoUploadResult?> UploadProjectPhotoAsync(
        int projectId, string photoBase64, string? caption, string? category,
        bool analyzeWithAI, double? latitude, double? longitude)
    {
        LastError = null;
        try
        {
            RefreshToken();
            var response = await _client.PostAsJsonAsync($"/api/mobile/projects/{projectId}/photos", new
            {
                photoBase64,
                caption,
                category,
                analyzeWithAI,
                latitude,
                longitude
            });
            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"UploadProjectPhoto response ({response.StatusCode}): {json}");
            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<ProjectPhotoUploadResult>(json, options);
            }
            LastError = TryParseError(json) ?? $"Server returned {(int)response.StatusCode}";
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UploadProjectPhoto exception: {ex.Message}");
            LastError = ex.Message;
            return null;
        }
    }

    // GET /api/mobile/projects/photos/{photoId}/image -> raw jpeg bytes.
    // The blob container is private; this authenticated proxy is the only
    // way the app can display project photos.
    public async Task<byte[]?> GetProjectPhotoImageAsync(int photoId)
    {
        try
        {
            RefreshToken();
            var response = await _client.GetAsync($"/api/mobile/projects/photos/{photoId}/image");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetProjectPhotoImage exception: {ex.Message}");
            return null;
        }
    }

    // Deletes a project photo (server removes the blob + DB row). Returns null
    // on success, or an error message to show the user (e.g. the owner-only 403).
    public async Task<string?> DeleteProjectPhotoAsync(int photoId)
    {
        try
        {
            RefreshToken();
            var response = await _client.DeleteAsync($"/api/mobile/projects/photos/{photoId}");
            if (response.IsSuccessStatusCode) return null;
            var body = await response.Content.ReadAsStringAsync();
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("error", out var errEl))
                {
                    var msg = errEl.GetString();
                    if (!string.IsNullOrWhiteSpace(msg)) return msg;
                }
            }
            catch { }
            return "Delete failed (" + (int)response.StatusCode + ")";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DeleteProjectPhoto exception: {ex.Message}");
            return "Delete failed: " + ex.Message;
        }
    }

    // ============================================
    // MY PROFILE  [PRF1]
    // GET  /api/mobile/profile              -> worker info (ProfileInfo)
    // GET  /api/mobile/profile/photo/image  -> raw jpeg bytes (auth proxy)
    // POST /api/mobile/profile/photo        -> upload base64 photo
    // ============================================
    public async Task<ProfileInfo?> GetMyProfileAsync()
    {
        LastError = null;
        try
        {
            RefreshToken();
            var response = await _client.GetAsync("/api/mobile/profile");
            var json = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<ProfileInfo>(json, options);
            }
            LastError = TryParseError(json) ?? ("Server returned " + (int)response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return null;
        }
    }

    public async Task<byte[]?> GetProfilePhotoImageAsync()
    {
        try
        {
            RefreshToken();
            var response = await _client.GetAsync("/api/mobile/profile/photo/image");
            if (!response.IsSuccessStatusCode)
            {
                // Fallback route in case the server exposes the image at /photo
                response = await _client.GetAsync("/api/mobile/profile/photo");
                if (!response.IsSuccessStatusCode) return null;
            }
            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("GetProfilePhotoImage exception: " + ex.Message);
            return null;
        }
    }

    public async Task<bool> UploadProfilePhotoAsync(string base64)
    {
        LastError = null;
        try
        {
            RefreshToken();
            // Redundant property names on purpose: the server DTO binds whichever
            // matches; extra JSON properties are ignored by the model binder.
            var payload = new { PhotoBase64 = base64, Base64 = base64, ImageBase64 = base64, FileName = "profile.jpg" };
            var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/mobile/profile/photo", content);
            if (response.IsSuccessStatusCode) return true;
            var json = await response.Content.ReadAsStringAsync();
            LastError = TryParseError(json) ?? ("Server returned " + (int)response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return false;
        }
    }
    // ============================================
    // ============================================
    // SEARCH [SRCH2]
    // GET /api/mobile/search?q=&take= -> flat list of company-scoped hits
    // across projects, submittals, site logs, blueprints, photos, safety
    // inspections and expenses. Returns null (not an empty list) when the
    // endpoint is unreachable or not published yet, so callers can tell
    // "no matches" apart from "content search unavailable".
    // ============================================
    public async Task<List<SearchHit>?> SearchAsync(string query, int take = 5)
    {
        LastError = null;
        try
        {
            RefreshToken();
            var response = await _client.GetAsync($"/api/mobile/search?q={Uri.EscapeDataString(query ?? "")}&take={take}");
            var json = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<SearchHit>>(json, options) ?? new List<SearchHit>();
            }
            LastError = TryParseError(json) ?? ("Server returned " + (int)response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return null;
        }
    }

    // ============================================
    // NOTIFICATIONS [NOT2]
    // ============================================
    public async Task<List<NotificationItem>?> GetNotificationsAsync(int take = 50)
    {
        LastError = null;
        try
        {
            RefreshToken();
            var response = await _client.GetAsync($"/api/mobile/notifications?take={take}");
            var json = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<NotificationItem>>(json, options) ?? new List<NotificationItem>();
            }
            LastError = TryParseError(json) ?? ("Server returned " + (int)response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return null;
        }
    }

    public async Task<int> GetUnreadNotificationCountAsync()
    {
        try
        {
            RefreshToken();
            var response = await _client.GetAsync("/api/mobile/notifications/unread-count");
            if (!response.IsSuccessStatusCode) return 0;
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("count", out var c) ? c.GetInt32() : 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<bool> MarkNotificationReadAsync(int id)
    {
        try
        {
            RefreshToken();
            var response = await _client.PostAsync($"/api/mobile/notifications/{id}/read", null);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // [NOT4] Delete the notifications this user has already read. Unread ones
    // are left alone so nobody loses an alert they have not seen.
    public async Task<int> ClearReadNotificationsAsync()
    {
        try
        {
            LastError = null;
            RefreshToken();
            var response = await _client.PostAsync("/api/mobile/notifications/clear-read", null);
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                LastError = TryParseError(json) ?? ("Server returned " + (int)response.StatusCode);
                return 0;
            }
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("deleted", out var d) ? d.GetInt32() : 0;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return 0;
        }
    }

    public async Task<bool> MarkAllNotificationsReadAsync()
    {
        try
        {
            RefreshToken();
            var response = await _client.PostAsync("/api/mobile/notifications/read-all", null);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
    // BLUEPRINTS [BLP3]
    // GET /api/mobile/projects/{id}/blueprints -> sheet list
    // GET /api/mobile/blueprints/{id}/file     -> raw file bytes
    // ============================================
    public async Task<List<BlueprintItem>?> GetBlueprintsAsync(int projectId)
    {
        LastError = null;
        try
        {
            RefreshToken();
            var response = await _client.GetAsync($"/api/mobile/projects/{projectId}/blueprints");
            var json = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<BlueprintItem>>(json, options) ?? new List<BlueprintItem>();
            }
            LastError = TryParseError(json) ?? ("Server returned " + (int)response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return null;
        }
    }

        // ============================================
    // SUBMITTALS [SUB3a]
    // GET /api/mobile/projects/{id}/submittals -> register list
    // GET /api/mobile/submittals/{id}/file     -> raw file bytes
    // ============================================
    public async Task<List<SubmittalItem>?> GetSubmittalsAsync(int projectId)
    {
        LastError = null;
        try
        {
            RefreshToken();
            var response = await _client.GetAsync($"/api/mobile/projects/{projectId}/submittals");
            var json = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<SubmittalItem>>(json, options) ?? new List<SubmittalItem>();
            }
            LastError = TryParseError(json) ?? ("Server returned " + (int)response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return null;
        }
    }

    public async Task<byte[]?> GetSubmittalFileAsync(int id)
    {
        try
        {
            RefreshToken();
            var response = await _client.GetAsync($"/api/mobile/submittals/{id}/file");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("GetSubmittalFile exception: " + ex.Message);
            return null;
        }
    }
    public async Task<byte[]?> GetBlueprintFileAsync(int id)
    {
        try
        {
            RefreshToken();
            var response = await _client.GetAsync($"/api/mobile/blueprints/{id}/file");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("GetBlueprintFile exception: " + ex.Message);
            return null;
        }
    }

    // ---------- [BFJOB1] quick job scheduling (gated on the server) ----------
    public async Task<List<CrewPick>> GetVisitCrewAsync()
    {
        try
        {
            RefreshToken();
            var response = await _client.GetAsync("/api/mobile/visits/crew");
            if (!response.IsSuccessStatusCode) return new List<CrewPick>();
            return await response.Content.ReadFromJsonAsync<List<CrewPick>>() ?? new List<CrewPick>();
        }
        catch { return new List<CrewPick>(); }
    }

    public async Task<VisitCreateResult?> CreateVisitAsync(VisitCreateRequest req)
    {
        LastError = null;
        try
        {
            RefreshToken();
            var response = await _client.PostAsJsonAsync("/api/mobile/visits", req);
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                LastError = TryParseMessage(json) ?? ("Server error " + (int)response.StatusCode);
                return null;
            }
            return JsonSerializer.Deserialize<VisitCreateResult>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex) { LastError = ex.Message; return null; }
    }

    private static string? TryParseMessage(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                if (doc.RootElement.TryGetProperty("message", out var m1)) return m1.GetString();
                if (doc.RootElement.TryGetProperty("error", out var e1)) return e1.GetString();
            }
        }
        catch { }
        return null;
    }

    // ---------- [BFVIS1] my service visits + status updates ----------
    public async Task<List<VisitItem>?> GetVisitsAsync(DateTime? date = null)
    {
        LastError = null;
        try
        {
            RefreshToken();
            var url = "/api/mobile/visits";
            if (date.HasValue) url += "?date=" + date.Value.ToString("yyyy-MM-dd");
            var response = await _client.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                LastError = TryParseMessage(json) ?? ("Server returned " + (int)response.StatusCode);
                return null;
            }
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<List<VisitItem>>(json, options) ?? new List<VisitItem>();
        }
        catch (Exception ex) { LastError = ex.Message; return null; }
    }

    // [LIVEETA1] crew phone location ping while a visit is OnTheWay (server stores last position for the tracking page)
    public async Task<bool> SendVisitLocationAsync(int id, double lat, double lng)
    {
        try
        {
            RefreshToken();
            var response = await _client.PostAsJsonAsync("/api/mobile/visits/" + id + "/location", new { lat, lng });
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<VisitStatusResult?> SetVisitStatusAsync(int id, string status, int? etaMinutes, bool notify)
    {
        LastError = null;
        try
        {
            RefreshToken();
            var response = await _client.PostAsJsonAsync("/api/mobile/visits/" + id + "/status", new { status, etaMinutes, notify });
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                LastError = TryParseMessage(json) ?? ("Server returned " + (int)response.StatusCode);
                return null;
            }
            return JsonSerializer.Deserialize<VisitStatusResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex) { LastError = ex.Message; return null; }
    }
}



public class SiteLog
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public DateTime Date { get; set; }
    public string? Weather { get; set; }
    public string? CrewSummary { get; set; }
    public int CrewCount { get; set; }
    public string? WorkCompleted { get; set; }
    public string? IssuesDelays { get; set; }
    public string? MaterialsDelivered { get; set; }
    public string? Notes { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

public class SiteLogSave
{
    public int ProjectId { get; set; }
    public string? Weather { get; set; }
    public string? CrewSummary { get; set; }
    public int CrewCount { get; set; }
    public string? WorkCompleted { get; set; }
    public string? IssuesDelays { get; set; }
    public string? MaterialsDelivered { get; set; }
    public string? Notes { get; set; }
}
public class SafetyInspectionSend
{
    public int ProjectId { get; set; }
    public string? InspectionType { get; set; }
    public List<SafetyItemSend> Items { get; set; } = new();
    public bool FollowUpRequired { get; set; }
    public string? Notes { get; set; }
}

public class SafetyItemSend
{
    public string? Id { get; set; }
    public string? Label { get; set; }
    public string? Result { get; set; }
    public string? Note { get; set; }
}

public class ProfileInfo
{
    public int Id { get; set; }
    public string? FullName { get; set; }
    public string? Position { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public bool HasPhoto { get; set; }
}
public class SubmittalItem
{
    public int Id { get; set; }
    public string? SubmittalNumber { get; set; }
    public int Revision { get; set; }
    public string? Title { get; set; }
    public string? SubmittalType { get; set; }
    public string? SpecSection { get; set; }
    public string? Status { get; set; }
    public string? SubcontractorName { get; set; }
    public DateTime? DateSubmitted { get; set; }
    public DateTime? DateRequired { get; set; }
    public DateTime? DateReturned { get; set; }
    public string? ReviewedByName { get; set; }
    public string? ReviewComments { get; set; }
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public long FileSizeBytes { get; set; }
    public bool HasFile { get; set; }
    public DateTime CreatedDate { get; set; }
}
// [SRCH2] one row of search results. Kind drives which page a tap opens.
public class SearchHit
{
    public string? Kind { get; set; }
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public DateTime Date { get; set; }
}

public class NotificationItem
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public string? Type { get; set; }
    public string? RelatedEntityType { get; set; }
    public int? RelatedEntityId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedDate { get; set; }
}
public class BlueprintItem
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public long FileSizeBytes { get; set; }
    public string? UploadedByName { get; set; }
    public DateTime CreatedDate { get; set; }
}
