$path = "C:\Users\mezan\source\repos\BuildForce\BuildForce\Services\ApiService.cs"
$content = Get-Content -Path $path -Raw

$old = 'BaseAddress = new Uri("https://mezanoconstructionmanagementplatform.com"),'
$new = 'BaseAddress = new Uri("https://mezanocm.com"),'

if ($content.Contains($old)) {
    $content = $content.Replace($old, $new)
    Set-Content -Path $path -Value $content -Encoding UTF8
    Write-Host "DONE: ApiService.cs now points to mezanocm.com" -ForegroundColor Green
} else {
    Write-Host "WARNING: pattern not found - check file manually" -ForegroundColor Yellow
}

$path2 = "C:\Users\mezan\source\repos\BuildForce\BuildForce\MauiProgram.cs"
$content2 = Get-Content -Path $path2 -Raw

$old2 = 'BaseAddress = new Uri("https://mezanoconstructionmanagementplatform.com"),'
$new2 = 'BaseAddress = new Uri("https://mezanocm.com"),'

if ($content2.Contains($old2)) {
    $content2 = $content2.Replace($old2, $new2)
    Set-Content -Path $path2 -Value $content2 -Encoding UTF8
    Write-Host "DONE: MauiProgram.cs now points to mezanocm.com" -ForegroundColor Green
} else {
    Write-Host "INFO: MauiProgram.cs pattern not found or already correct" -ForegroundColor Cyan
}