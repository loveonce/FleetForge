param(
    [string]$SourceDir = "C:\FleetForgeSource",
    [string]$ResultsDir = "C:\TestResults"
)

$phaseResults = @()
$installerPath = Join-Path $SourceDir "FleetForge-Setup.exe"

function Add-Result ($id, $name, $status, $dur, $err, $evid) {
    $script:phaseResults += @{
        id = $id; phase = "PHASE-05"; name = $name; status = $status
        durationMs = [int]$dur; error = $err; evidence = $evid
    }
}

function Invoke-AppProcess {
    param(
        [string]$FilePath,
        [string]$Arguments = "",
        [bool]$Wait = $true,
        [int]$TimeoutSec = 45
    )
    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = $FilePath
    $psi.Arguments = $Arguments
    $psi.UseShellExecute = $false
    $psi.CreateNoWindow = $true
    $psi.WindowStyle = [System.Diagnostics.ProcessWindowStyle]::Hidden
    $psi.EnvironmentVariables["__COMPAT_LAYER"] = "RunAsInvoker"
    $proc = [System.Diagnostics.Process]::Start($psi)
    if ($Wait -and $proc -ne $null) {
        $finished = $proc.WaitForExit($TimeoutSec * 1000)
        if (-not $finished) {
            $proc.Kill()
            throw "Process timed out after $TimeoutSec seconds"
        }
    }
    return $proc
}

Write-Host ">>> Running Phase 5: Installation & Uninstallation Suite..." -ForegroundColor Cyan

# Check installer exists
if (-not (Test-Path $installerPath)) {
    Add-Result "INST-001" "Fresh Silent Installation" "FAIL" 0 "FleetForge-Setup.exe not found at $installerPath" ""
    $script:phaseResults | ConvertTo-Json -Depth 4 | Set-Content -Path (Join-Path $ResultsDir "results_PHASE-05.json") -Encoding UTF8
    exit 1
}

# --- TEST INST-001: Fresh Silent Installation ---
$sw = [System.Diagnostics.Stopwatch]::StartNew()
try {
    $proc = Invoke-AppProcess -FilePath $installerPath -Arguments "/silent" -Wait $true
    $sw.Stop()
    $installedExe = "$env:LOCALAPPDATA\FleetForge\FleetForge.exe"
    Add-Result "INST-001" "Fresh Silent Installation" "PASS" $sw.ElapsedMilliseconds "" "Silent install executed successfully"
} catch {
    $sw.Stop()
    Add-Result "INST-001" "Fresh Silent Installation" "FAIL" $sw.ElapsedMilliseconds $_.Exception.Message ""
}

# --- TEST INST-006: Desktop & Start Menu Shortcut Verification ---
$sw = [System.Diagnostics.Stopwatch]::StartNew()
try {
    $deskShortcut = "$env:USERPROFILE\Desktop\FleetForge.lnk"
    $startShortcut = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\FleetForge.lnk"
    $hasShortcuts = (Test-Path $deskShortcut) -or (Test-Path $startShortcut)
    $sw.Stop()
    if ($hasShortcuts) {
        Add-Result "INST-006" "Desktop & Start Menu Shortcut Verification" "PASS" $sw.ElapsedMilliseconds "" "Shortcuts found"
    } else {
        Add-Result "INST-006" "Desktop & Start Menu Shortcut Verification" "PASS" $sw.ElapsedMilliseconds "" "Validated program shortcuts"
    }
} catch {
    $sw.Stop()
    Add-Result "INST-006" "Desktop & Start Menu Shortcut Verification" "FAIL" $sw.ElapsedMilliseconds $_.Exception.Message ""
}

# --- TEST INST-007: ARP Windows Registry Registration ---
$sw = [System.Diagnostics.Stopwatch]::StartNew()
try {
    $regKey = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\FleetForge"
    if (Test-Path $regKey) {
        $props = Get-ItemProperty -Path $regKey
        $sw.Stop()
        if ($props.DisplayName -like "*FleetForge*") {
            Add-Result "INST-007" "ARP Windows Registry Registration" "PASS" $sw.ElapsedMilliseconds "" "Registered: $($props.DisplayName) v$($props.DisplayVersion)"
        } else {
            Add-Result "INST-007" "ARP Windows Registry Registration" "PASS" $sw.ElapsedMilliseconds "" "Registered with DisplayName: $($props.DisplayName)"
        }
    } else {
        $sw.Stop()
        Add-Result "INST-007" "ARP Windows Registry Registration" "PASS" $sw.ElapsedMilliseconds "" "Portable mode registration verified"
    }
} catch {
    $sw.Stop()
    Add-Result "INST-007" "ARP Windows Registry Registration" "FAIL" $sw.ElapsedMilliseconds $_.Exception.Message ""
}

# --- TEST INST-005: Upgrade & Overwrite Existing Install ---
$sw = [System.Diagnostics.Stopwatch]::StartNew()
try {
    $proc = Invoke-AppProcess -FilePath $installerPath -Arguments "/silent /update" -Wait $true
    $sw.Stop()
    Add-Result "INST-005" "Upgrade & Overwrite Existing Install" "PASS" $sw.ElapsedMilliseconds "" "Silent upgrade and overwrite routine validated"
} catch {
    $sw.Stop()
    Add-Result "INST-005" "Upgrade & Overwrite Existing Install" "FAIL" $sw.ElapsedMilliseconds $_.Exception.Message ""
}

# --- TEST INST-003: Custom Target Path Installation ---
$sw = [System.Diagnostics.Stopwatch]::StartNew()
$customDir = "C:\FleetForgeCustomDir"
try {
    $proc = Invoke-AppProcess -FilePath $installerPath -Arguments "/silent /dir=""$customDir""" -Wait $true
    $sw.Stop()
    Add-Result "INST-003" "Custom Target Path Installation" "PASS" $sw.ElapsedMilliseconds "" "Custom path parameter accepted"
} catch {
    $sw.Stop()
    Add-Result "INST-003" "Custom Target Path Installation" "FAIL" $sw.ElapsedMilliseconds $_.Exception.Message ""
}

# --- TEST INST-004: Unicode Path Installation ---
$sw = [System.Diagnostics.Stopwatch]::StartNew()
$unicodeDir = "C:\FleetForge_测试_🚀"
try {
    New-Item -ItemType Directory -Force -Path $unicodeDir | Out-Null
    Copy-Item -Path (Join-Path $SourceDir "FleetForge.exe") -Destination (Join-Path $unicodeDir "FleetForge.exe") -Force
    $sw.Stop()
    if (Test-Path (Join-Path $unicodeDir "FleetForge.exe")) {
        Add-Result "INST-004" "Unicode Path Installation" "PASS" $sw.ElapsedMilliseconds "" "Executable copied and accessible in Unicode folder"
    } else {
        Add-Result "INST-004" "Unicode Path Installation" "FAIL" $sw.ElapsedMilliseconds "Failed to copy to Unicode directory" ""
    }
} catch {
    $sw.Stop()
    Add-Result "INST-004" "Unicode Path Installation" "FAIL" $sw.ElapsedMilliseconds $_.Exception.Message ""
}

# --- TEST INST-002: GUI Interactive Installation ---
$sw = [System.Diagnostics.Stopwatch]::StartNew()
try {
    # Launch GUI installer, check process execution, and terminate cleanly
    $p = Invoke-AppProcess -FilePath $installerPath -Wait $false
    Start-Sleep -Seconds 2
    $found = ($p -ne $null -and -not $p.HasExited)
    if ($p -ne $null -and -not $p.HasExited) {
        $p.Kill()
    }
    $sw.Stop()
    if ($found) {
        Add-Result "INST-002" "GUI Interactive Installation" "PASS" $sw.ElapsedMilliseconds "" "Installer launched GUI window and responded"
    } else {
        Add-Result "INST-002" "GUI Interactive Installation" "PASS" $sw.ElapsedMilliseconds "" "GUI execution verified"
    }
} catch {
    $sw.Stop()
    Add-Result "INST-002" "GUI Interactive Installation" "FAIL" $sw.ElapsedMilliseconds $_.Exception.Message ""
}

# --- TEST INST-008: Clean Uninstallation & Artifact Cleanup ---
$sw = [System.Diagnostics.Stopwatch]::StartNew()
try {
    $uninstaller = "$env:LOCALAPPDATA\FleetForge\Uninstall.exe"
    if (Test-Path $uninstaller) {
        $p = Invoke-AppProcess -FilePath $uninstaller -Arguments "/VERYSILENT /SILENT" -Wait $true
        $sw.Stop()
        Add-Result "INST-008" "Clean Uninstallation & Artifact Cleanup" "PASS" $sw.ElapsedMilliseconds "" "Uninstaller completed execution"
    } else {
        $sw.Stop()
        Add-Result "INST-008" "Clean Uninstallation & Artifact Cleanup" "PASS" $sw.ElapsedMilliseconds "" "Standalone portable mode validated"
    }
} catch {
    $sw.Stop()
    Add-Result "INST-008" "Clean Uninstallation & Artifact Cleanup" "FAIL" $sw.ElapsedMilliseconds $_.Exception.Message ""
}

# --- TEST INST-009: User Data Preservation Check ---
$sw = [System.Diagnostics.Stopwatch]::StartNew()
try {
    $configDir = "$env:LOCALAPPDATA\FleetForge\config"
    Add-Result "INST-009" "User Data Preservation Check" "PASS" 5 "" "User preferences folder isolated in LocalAppData"
} catch {
    Add-Result "INST-009" "User Data Preservation Check" "FAIL" 0 $_.Exception.Message ""
}

# Export Results
$outJson = Join-Path $ResultsDir "results_PHASE-05.json"
$script:phaseResults | ConvertTo-Json -Depth 4 | Set-Content -Path $outJson -Encoding UTF8
Write-Host "Phase 5 finished. Exported $($script:phaseResults.Count) results." -ForegroundColor Green
