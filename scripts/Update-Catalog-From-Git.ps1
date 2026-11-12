param(
    [string]$CustomUrl = ""
)

$ErrorActionPreference = "Stop"
$ScriptDir = $PSScriptRoot
if ([string]::IsNullOrEmpty($ScriptDir)) { $ScriptDir = (Get-Location).Path }
$ProjectRoot = Split-Path -Parent $ScriptDir
if ([string]::IsNullOrEmpty($ProjectRoot)) { $ProjectRoot = (Get-Location).Path }

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "      FleetForge Catalog Sync from GitHub / Git Link" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. Determine Target URL
$TargetUrl = $CustomUrl
if ([string]::IsNullOrWhiteSpace($TargetUrl)) {
    $UrlsFile = Join-Path $ProjectRoot "app_urls.json"
    if (-not (Test-Path $UrlsFile)) {
        $UrlsFile = Join-Path $ProjectRoot "config\app_urls.json"
    }
    if (Test-Path $UrlsFile) {
        try {
            $urlsJson = Get-Content $UrlsFile -Raw -Encoding UTF8
            $parsed = ($urlsJson | ConvertFrom-Json)
            if ($parsed -and $parsed.cloudSyncUrl) {
                $TargetUrl = $parsed.cloudSyncUrl
            }
        } catch { }
    }
}

if ([string]::IsNullOrWhiteSpace($TargetUrl)) {
    Write-Host "[ERROR] No Git / Cloud URL specified." -ForegroundColor Red
    Write-Host "Please provide a URL as parameter or configure 'cloudSyncUrl' in app_urls.json." -ForegroundColor Yellow
    Write-Host "Example:" -ForegroundColor Gray
    Write-Host "  Update-Catalog-From-Git.bat https://raw.githubusercontent.com/user/repo/main/data/apps.json" -ForegroundColor Gray
    exit 1
}

[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12 -bor [System.Net.SecurityProtocolType]::Tls11 -bor [System.Net.SecurityProtocolType]::Tls

# 2. Convert GitHub blob URL to raw URL if needed
$DirectUrl = $TargetUrl.Trim()
if ($DirectUrl -match "github\.com/([^/]+)/([^/]+)/blob/([^/]+)/(.+)") {
    $DirectUrl = "https://raw.githubusercontent.com/$($matches[1])/$($matches[2])/$($matches[3])/$($matches[4])"
} elseif ($DirectUrl -match "drive\.google\.com/(?:file/d/|open\?id=)([a-zA-Z0-9_-]+)") {
    $DirectUrl = "https://drive.google.com/uc?export=download&id=$($matches[1])"
}

Write-Host "[Target URL]: $DirectUrl" -ForegroundColor White
Write-Host "[1/3] Downloading latest apps.json from Git link..." -ForegroundColor Cyan

$TempFile = Join-Path $env:TEMP ("fleetforge_sync_" + [Guid]::NewGuid().ToString() + ".json")
try {
    $wc = New-Object System.Net.WebClient
    $wc.Headers.Add("User-Agent", "FleetForge-Sync/2.4.0")
    $wc.DownloadFile($DirectUrl, $TempFile)
} catch {
    Write-Host "[ERROR] Failed to download from URL: $($_.Exception.Message)" -ForegroundColor Red
    if (Test-Path $TempFile) { Remove-Item $TempFile -Force -ErrorAction SilentlyContinue }
    exit 1
}

Write-Host "[2/3] Validating downloaded JSON catalog..." -ForegroundColor Cyan
try {
    $rawText = [System.IO.File]::ReadAllText($TempFile, [System.Text.Encoding]::UTF8)
    $catalogObj = $rawText | ConvertFrom-Json
    if (-not $catalogObj.apps -or $catalogObj.apps.Count -eq 0) {
        throw "Catalog does not contain any valid 'apps' entries."
    }
    $appCount = $catalogObj.apps.Count
    $groupCount = if ($catalogObj.groups) { $catalogObj.groups.Count } else { 0 }
    $catCount = if ($catalogObj.categories) { $catalogObj.categories.Count } else { 0 }
    Write-Host "  ✓ Valid catalog format verified!" -ForegroundColor Green
    Write-Host "  • Packages:   $appCount" -ForegroundColor Green
    Write-Host "  • Roles:      $groupCount" -ForegroundColor Green
    Write-Host "  • Categories: $catCount" -ForegroundColor Green
} catch {
    Write-Host "[ERROR] Validation failed: $($_.Exception.Message)" -ForegroundColor Red
    if (Test-Path $TempFile) { Remove-Item $TempFile -Force -ErrorAction SilentlyContinue }
    exit 1
}

Write-Host "[3/3] Updating local catalog files..." -ForegroundColor Cyan

$DataApps = Join-Path $ProjectRoot "data\apps.json"
$RootApps = Join-Path $ProjectRoot "apps.json"

if (Test-Path $DataApps) {
    Copy-Item $DataApps (Join-Path $ProjectRoot "data\apps.backup.json") -Force
}
Copy-Item $TempFile $DataApps -Force

if (Test-Path $RootApps) {
    Copy-Item $TempFile $RootApps -Force
}

Remove-Item $TempFile -Force -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "==========================================================" -ForegroundColor Green
Write-Host "     SUCCESS: Local apps.json updated successfully!" -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green
Write-Host "Catalog updated with $appCount software packages." -ForegroundColor White
Write-Host "File saved to: $DataApps" -ForegroundColor Gray
Write-Host ""
