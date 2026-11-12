<#
================================================================================
  Team Software Pre-Installer (CLI / Unattended Engine)
  Automated command-line multi-software installation for fresh Windows setups.

  Usage Examples:
    .\PreInstaller-CLI.ps1 -Group developer
    .\PreInstaller-CLI.ps1 -Group tester -Silent
    .\PreInstaller-CLI.ps1 -Group designer
    .\PreInstaller-CLI.ps1 -List
    .\PreInstaller-CLI.ps1 -Sync
    .\PreInstaller-CLI.ps1 -Apps @("Google.Chrome", "7zip.7zip")
================================================================================
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [ValidateSet("developer", "designer", "tester", "office", "all")]
    [string]$Group,

    [Parameter(Mandatory = $false)]
    [string[]]$Apps,

    [Parameter(Mandatory = $false)]
    [switch]$Silent,

    [Parameter(Mandatory = $false)]
    [switch]$List,

    [Parameter(Mandatory = $false)]
    [switch]$Sync
)

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $ScriptDir

# Locate Winget
function Get-WingetPath {
    if (Get-Command winget -ErrorAction SilentlyContinue) {
        return "winget"
    }

    $userAlias = "$env:LOCALAPPDATA\Microsoft\WindowsApps\winget.exe"
    if (Test-Path $userAlias) {
        return $userAlias
    }

    $winApps = Get-ChildItem -Path "C:\Program Files\WindowsApps" -Filter "*DesktopAppInstaller*" -ErrorAction SilentlyContinue |
               Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if ($winApps) {
        $candidate = Join-Path $winApps.FullName "winget.exe"
        if (Test-Path $candidate) {
            return $candidate
        }
    }

    return $null
}

$WingetCmd = Get-WingetPath
$BaseDir = $ScriptDir
if (Test-Path (Join-Path (Split-Path -Parent $ScriptDir) "data\apps.json")) {
    $BaseDir = Split-Path -Parent $ScriptDir
}
$ConfigPath = Join-Path $BaseDir "data\apps.json"
if (-not (Test-Path $ConfigPath)) {
    $ConfigPath = Join-Path $BaseDir "apps.json"
}
$SettingsPath = Join-Path $BaseDir "config\settings.json"
if (-not (Test-Path $SettingsPath)) {
    $SettingsPath = Join-Path $BaseDir "settings.json"
}
$LogFilePath = Join-Path $BaseDir "logs\install-log.txt"
if (-not (Test-Path (Join-Path $BaseDir "logs"))) {
    $LogFilePath = Join-Path $BaseDir "install-log.txt"
}

# Cloud Sync Helper
function Convert-ToDirectDownloadUrl([string]$rawUrl) {
    if ([string]::IsNullOrWhiteSpace($rawUrl)) { return "" }
    $url = $rawUrl.Trim()
    if ($url -match "github\.com/([^/]+)/([^/]+)/blob/([^/]+)/(.+)") {
        return "https://raw.githubusercontent.com/$($Matches[1])/$($Matches[2])/$($Matches[3])/$($Matches[4])"
    }
    if ($url -match "drive\.google\.com/(?:file/d/|open\?id=)([a-zA-Z0-9_-]+)") {
        return "https://drive.google.com/uc?export=download&id=$($Matches[1])"
    }
    return $url
}

if (Test-Path $SettingsPath) {
    try {
        $settings = Get-Content -Path $SettingsPath -Raw -Encoding UTF8 | ConvertFrom-Json
        if (($Sync -or $settings.autoSyncOnLaunch) -and $settings.cloudConfigUrl -and ($settings.cloudConfigUrl -notlike "*your-username*")) {
            $directUrl = Convert-ToDirectDownloadUrl $settings.cloudConfigUrl
            Write-Host "Syncing software list from Cloud ($directUrl)..." -ForegroundColor Cyan
            try {
                $wc = New-Object System.Net.WebClient
                $wc.Headers.Add("User-Agent", "SoftwarePreInstaller/2.0")
                $json = $wc.DownloadString($directUrl)
                $testParsed = $json | ConvertFrom-Json
                if ($testParsed.groups -and $testParsed.apps) {
                    [System.IO.File]::WriteAllText($ConfigPath, $json, [System.Text.Encoding]::UTF8)
                    Write-Host "Cloud sync successful! Saved to apps.json." -ForegroundColor Green
                }
            } catch {
                Write-Host "Cloud sync failed: $($_.Exception.Message). Falling back to local apps.json." -ForegroundColor Yellow
            }
        }
    } catch {}
}

if (-not (Test-Path $ConfigPath)) {
    Write-Error "Config file 'apps.json' not found in $ScriptDir."
    exit 1
}

$ConfigJson = Get-Content -Path $ConfigPath -Raw -Encoding UTF8 | ConvertFrom-Json

# Handle -List parameter
if ($List) {
    Write-Host "`n========================================================" -ForegroundColor Cyan
    Write-Host "               AVAILABLE ROLE GROUPS" -ForegroundColor Cyan
    Write-Host "========================================================" -ForegroundColor Cyan
    foreach ($grp in $ConfigJson.groups) {
        Write-Host "  [$($grp.id)] $($grp.icon) $($grp.name) ($($grp.apps.Count) apps)" -ForegroundColor Yellow
        Write-Host "      $($grp.description)" -ForegroundColor Gray
    }

    Write-Host "`n========================================================" -ForegroundColor Green
    Write-Host "               AVAILABLE SOFTWARE PACKAGES" -ForegroundColor Green
    Write-Host "========================================================" -ForegroundColor Green
    foreach ($app in $ConfigJson.apps) {
        Write-Host "  $($app.name) [$($app.id)]" -ForegroundColor White
        Write-Host "      Category: $($app.category) | $($app.description)" -ForegroundColor Gray
    }
    Write-Host ""
    exit 0
}

# Determine target apps to install
$targetAppIds = @()

if ($Apps -and $Apps.Count -gt 0) {
    $targetAppIds = $Apps
} elseif ($Group) {
    if ($Group -eq "all") {
        $targetAppIds = $ConfigJson.apps | ForEach-Object { $_.id }
    } else {
        $matchedGrp = $ConfigJson.groups | Where-Object { $_.id -eq $Group }
        if ($matchedGrp) {
            $targetAppIds = $matchedGrp.apps
            Write-Host "`nSelected Role Group: $($matchedGrp.icon) $($matchedGrp.name)" -ForegroundColor Cyan
            Write-Host "Description: $($matchedGrp.description)" -ForegroundColor Gray
        } else {
            Write-Error "Group '$Group' not found in apps.json."
            exit 1
        }
    }
} else {
    Write-Host "`nNo group specified! Available groups: developer, designer, tester, office, all" -ForegroundColor Yellow
    Write-Host "Usage: .\PreInstaller-CLI.ps1 -Group <developer|designer|tester|office> [-Silent]" -ForegroundColor Yellow
    Write-Host "Or run .\Start-Installer.bat for the Dev Home GUI." -ForegroundColor Yellow
    exit 1
}

$selectedApps = $ConfigJson.apps | Where-Object { $targetAppIds -contains $_.id }

Write-Host "`n========================================================" -ForegroundColor Cyan
Write-Host "           SOFTWARE PRE-INSTALLATION BATCH" -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "Target Packages ($($selectedApps.Count)):" -ForegroundColor White
foreach ($app in $selectedApps) {
    Write-Host "  * $($app.name) [$($app.id)]" -ForegroundColor Gray
}
Write-Host "========================================================`n" -ForegroundColor Cyan

if (-not $Silent) {
    $response = Read-Host "Proceed with installation? (Y/N)"
    if ($response -notmatch '^[Yy]$') {
        Write-Host "Aborted by user." -ForegroundColor Yellow
        exit 0
    }
}

function Log([string]$msg) {
    $timestamp = Get-Date -Format "HH:mm:ss"
    $line = "[$timestamp] $msg"
    Write-Host $line
    Add-Content -Path $LogFilePath -Value $line -ErrorAction SilentlyContinue
}

$successCount = 0
$failCount = 0

for ($i = 0; $i -lt $selectedApps.Count; $i++) {
    $app = $selectedApps[$i]
    $idx = $i + 1
    Write-Host "`n[$idx/$($selectedApps.Count)] Processing $($app.name)..." -ForegroundColor Yellow

    # Offline installer check
    $offlineInstalled = $false
    $offlineDir = Join-Path $ScriptDir "offline_installers"
    if ($app.offlineInstaller -and (Test-Path $offlineDir)) {
        $offlineFile = Join-Path $offlineDir $app.offlineInstaller
        if (Test-Path $offlineFile) {
            Log "Found offline installer: $offlineFile"
            Log "Executing with args: $($app.silentArgs)"
            try {
                $p = Start-Process -FilePath $offlineFile -ArgumentList $app.silentArgs -Wait -PassThru -NoNewWindow
                if ($p.ExitCode -eq 0 -or $p.ExitCode -eq 3010) {
                    Log "Offline installer completed successfully."
                    $offlineInstalled = $true
                } else {
                    Log "Offline installer returned ExitCode $($p.ExitCode). Falling back to Winget..."
                }
            } catch {
                Log "Offline installer failed: $_"
            }
        }
    }

    if (-not $offlineInstalled) {
        if (-not $WingetCmd) {
            Log "ERROR: Winget not available."
            $failCount++
            continue
        }

        Log "Running winget install for $($app.id)..."
        $wingetArgs = "install --id `"$($app.id)`" -e --silent --accept-package-agreements --accept-source-agreements --disable-interactivity"

        try {
            $p = Start-Process -FilePath $WingetCmd -ArgumentList $wingetArgs -Wait -PassThru -NoNewWindow
            $code = $p.ExitCode
            if ($code -eq 0 -or $code -eq 3010 -or $code -eq -1978335189 -or $code -eq 2316632107) {
                Log "Installed successfully."
                $successCount++
            } else {
                Log "Winget exited with error code: $code"
                $failCount++
            }
        } catch {
            Log "Installation exception: $_"
            $failCount++
        }
    } else {
        $successCount++
    }
}

Write-Host "`n========================================================" -ForegroundColor Cyan
Write-Host "                INSTALLATION COMPLETED" -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "Succeeded: $successCount" -ForegroundColor Green
Write-Host "Failed:    $failCount" -ForegroundColor Red
Write-Host "Log file:  $LogFilePath" -ForegroundColor Gray
Write-Host "========================================================`n" -ForegroundColor Cyan
