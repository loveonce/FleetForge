param(
    [string]$ResultsDir = "C:\TestResults",
    [string]$SourceDir = "C:\FleetForgeSource"
)

$rawPath = Join-Path $ResultsDir "test_results_raw.json"
$envPath = Join-Path $ResultsDir "environment_info.json"

if (-not (Test-Path $rawPath)) {
    Write-Host "[ERROR] Raw results file missing at $rawPath" -ForegroundColor Red
    exit 1
}

$results = Get-Content $rawPath -Raw -Encoding UTF8 | ConvertFrom-Json
$envInfo = if (Test-Path $envPath) { Get-Content $envPath -Raw -Encoding UTF8 | ConvertFrom-Json } else { @{} }

$total = $results.Count
$passed = ($results | Where-Object { $_.status -eq "PASS" }).Count
$failed = ($results | Where-Object { $_.status -eq "FAIL" }).Count
$blocked = ($results | Where-Object { $_.status -eq "BLOCKED" }).Count
$notRun = ($results | Where-Object { $_.status -eq "NOT RUN" }).Count
$passRate = if ($total -gt 0) { [math]::Round(($passed / $total) * 100, 2) } else { 0 }

$summary = @{
    suiteName = "FleetForge Enterprise Verification Suite"
    version = "2.6.5"
    runDate = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
    environment = $envInfo
    totalTests = $total
    passed = $passed
    failed = $failed
    blocked = $blocked
    notRun = $notRun
    passRatePercent = $passRate
    tests = $results
}

$summaryJsonPath = Join-Path $ResultsDir "test_summary.json"
$summary | ConvertTo-Json -Depth 6 | Set-Content -Path $summaryJsonPath -Encoding UTF8

Write-Host "==========================================================" -ForegroundColor Green
Write-Host "           SANDBOX QA EXECUTION SUMMARY                   " -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green
Write-Host " Total Tests: $total | Passed: $passed | Failed: $failed | Blocked: $blocked" -ForegroundColor Cyan
Write-Host " Overall Pass Rate: $passRate%" -ForegroundColor $(if ($failed -eq 0) { "Green" } else { "Red" })
Write-Host " Summary saved to: $summaryJsonPath" -ForegroundColor Gray
