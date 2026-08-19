#pragma warning disable CA1416
using System.Net.Http.Json;
using System.Text.Json;
namespace BuildForce.Services;
public class AuthService
{
    private readonly HttpClient _client;
    // [BFJOB1] office-side roles may schedule jobs from the phone (the server re-checks the JWT roles)
    public static bool CanScheduleJobs
    {
        get
        {
            var roles = Preferences.Get("roles", "").Split(",", StringSplitOptions.RemoveEmptyEntries);
            return roles.Any(r => r.Trim() == "SystemAdmin" || r.Trim() == "CompanyAdmin" || r.Trim() == "Manager");
        }
    }
    private const string BaseUrl = "https://mezanocm.com";
    public AuthService()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        _client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
    }
    // [GSIM1] Sign in with Google: the server runs the OAuth dance and hands the JWT back on buildforce://auth
    public async Task<LoginResult> LoginWithGoogleAsync()
    {
        try
        {
            var start = new Uri(BaseUrl.TrimEnd('/') + "/api/mobile/auth/google/start");
            var result = await WebAuthenticator.Default.AuthenticateAsync(start, new Uri("buildforce://auth"));
            if (result == null) return new LoginResult { Success = false, Message = "Google sign-in cancelled" };
            string P(string k) => result.Properties.TryGetValue(k, out var v) ? (v ?? "") : "";
            if (!string.IsNullOrEmpty(P("error"))) return new LoginResult { Success = false, Message = P("error") };
            var token = P("token");
            if (string.IsNullOrEmpty(token)) return new LoginResult { Success = false, Message = "Google sign-in failed" };
            var email = P("email");
            var fullName = P("fullName");
            if (string.IsNullOrEmpty(fullName)) fullName = string.IsNullOrEmpty(email) ? "User" : email.Split('@')[0];
            Preferences.Set("auth_token", token);
            Preferences.Set("full_name", fullName);
            Preferences.Set("email", email);
            Preferences.Set("employee_id", P("employeeId"));
            Preferences.Set("company_id", string.IsNullOrEmpty(P("companyId")) ? "0" : P("companyId"));
            Preferences.Set("roles", P("roles"));
            return new LoginResult { Success = true, Token = token };
        }
        catch (TaskCanceledException) { return new LoginResult { Success = false, Message = "Google sign-in cancelled" }; }
        catch (Exception ex) { return new LoginResult { Success = false, Message = ex.Message }; }
    }
    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _client.PostAsJsonAsync("/api/mobile/auth/login", new
            {
                Email = email,
                Password = password
            });
            if (response.IsSuccessStatusCode)
            {
                var rawJson = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(rawJson);
                var root = doc.RootElement;
                var token = GetString(root, "token", "Token", "access_token", "accessToken");
                var fullName = GetString(root,
                    "fullName", "FullName", "full_name",
                    "name", "Name", "userName", "UserName",
                    "displayName", "DisplayName");
                if (string.IsNullOrEmpty(fullName))
                {
                    var first = GetString(root, "firstName", "FirstName", "first_name");
                    var last = GetString(root, "lastName", "LastName", "last_name");
                    if (!string.IsNullOrEmpty(first) || !string.IsNullOrEmpty(last))
                        fullName = $"{first} {last}".Trim();
                }
                var allFields = string.Join(", ", root.EnumerateObject()
                    .Select(p => $"{p.Name}={p.Value}"));
                System.Diagnostics.Debug.WriteLine($"API FIELDS: {allFields}");
                if (string.IsNullOrEmpty(fullName))
                    fullName = email.Split('@')[0];
                var userEmail = GetString(root, "email", "Email") ?? email;
                var employeeId = GetString(root, "employeeId", "EmployeeId", "employee_id") ?? "";
                var companyId = GetString(root, "companyId", "CompanyId", "company_id") ?? "0";
                var rolesCsv = GetString(root, "role", "Role") ?? ""; // [BFJOB1]
                if ((root.TryGetProperty("roles", out var rolesEl) || root.TryGetProperty("Roles", out rolesEl))
                    && rolesEl.ValueKind == JsonValueKind.Array)
                    rolesCsv = string.Join(",", rolesEl.EnumerateArray()
                        .Where(x => x.ValueKind == JsonValueKind.String).Select(x => x.GetString()));
                if (!string.IsNullOrEmpty(token))
                {
                    Preferences.Set("auth_token", token);
                    Preferences.Set("full_name", fullName);
                    Preferences.Set("email", userEmail);
                    Preferences.Set("employee_id", employeeId);
                    Preferences.Set("company_id", companyId);
                    Preferences.Set("roles", rolesCsv); // [BFJOB1]
                    return new LoginResult { Success = true, Token = token };
                }
            }
            return new LoginResult { Success = false, Message = "Invalid email or password" };
        }
        catch (Exception ex)
        {
            return new LoginResult { Success = false, Message = ex.Message };
        }
    }
    private static string? GetString(JsonElement root, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (root.TryGetProperty(key, out var prop) &&
                prop.ValueKind == JsonValueKind.String)
            {
                var val = prop.GetString();
                if (!string.IsNullOrWhiteSpace(val))
                    return val;
            }
        }
        return null;
    }
}
public class MobileLoginResponse
{
    public string Token { get; set; } = "";
    public string Email { get; set; } = "";
    public string FullName { get; set; } = "";
    public int CompanyId { get; set; }
    public int? EmployeeId { get; set; }
}