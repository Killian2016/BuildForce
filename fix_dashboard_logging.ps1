$path = "C:\Users\mezan\source\repos\BuildForce\BuildForce\Services\ApiService.cs"
$content = Get-Content -Path $path -Raw

$old = @'
    public async Task<DashboardData?> GetDashboardAsync()
    {
        try { RefreshToken(); return await _client.GetFromJsonAsync<DashboardData>("/api/mobile/dashboard"); }
        catch { return null; }
    }
'@

$new = @'
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
'@

if ($content.Contains($old)) {
    $content = $content.Replace($old, $new)
    Set-Content -Path $path -Value $content -Encoding UTF8
    Write-Host "DONE: GetDashboardAsync now logs errors" -ForegroundColor Green
} else {
    Write-Host "WARNING: exact pattern not found - file may already differ. Check manually." -ForegroundColor Yellow
}