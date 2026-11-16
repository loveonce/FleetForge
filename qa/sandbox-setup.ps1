# =========================================================================
# FleetForge Sandbox Automated QA Setup Engine
# Environment Bootstrap & Validation inside Windows Sandbox
# =========================================================================
$ErrorActionPreference = "Continue"

$Desktop = "C:\Users\WDAGUtilityAccount\Desktop"
$SourceDir = Join-Path $Desktop "FleetForge"
$ResultsDir = Join-Path $Desktop "results"

# Immediate probe file
try { Set-Content -Path "C:\sandbox_started.txt" -Value "STARTED $(Get-Date)" -Encoding UTF8 } catch { }
try { Set-Content -Path (Join-Path $ResultsDir "started.txt") -Value "STARTED $(Get-Date)" -Encoding UTF8 } catch { }

$LogFile = Join-Path $ResultsDir "sandbox_bootstrap.log"

function Log-Bootstrap ($msg) {
    $ts = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss.fff")
    $line = "[$ts] [BOOTSTRAP] $msg"
    Write-Host $line -ForegroundColor Cyan
    try { Add-Content -Path $LogFile -Value $line -Encoding UTF8 } catch { }
}

New-Item -ItemType Directory -Force -Path $ResultsDir -ErrorAction SilentlyContinue | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $ResultsDir "logs") -ErrorAction SilentlyContinue | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $ResultsDir "screenshots") -ErrorAction SilentlyContinue | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $ResultsDir "dumps") -ErrorAction SilentlyContinue | Out-Null

Log-Bootstrap "Windows Sandbox environment detected. Starting bootstrap sequence..."
Log-Bootstrap "Resolved Desktop: $Desktop"
Log-Bootstrap "Resolved Source: $SourceDir"
Log-Bootstrap "Resolved Results: $ResultsDir"

# 1. Validate Mapped Paths
if (-not (Test-Path $SourceDir)) {
    Log-Bootstrap "[FATAL] Source directory $SourceDir is missing! Aborting."
    exit 1
}
Log-Bootstrap "[OK] Read-only Source directory verified at $SourceDir"

# 2. Capture Environment Diagnostics
try {
    Add-Type -AssemblyName System.Windows.Forms -ErrorAction SilentlyContinue
    $os = Get-CimInstance Win32_OperatingSystem -ErrorAction SilentlyContinue
    $cpu = Get-CimInstance Win32_Processor -ErrorAction SilentlyContinue
    $cs = Get-CimInstance Win32_ComputerSystem -ErrorAction SilentlyContinue

    $envInfo = @{
        OSName = if ($os) { $os.Caption } else { "Microsoft Windows 11 Enterprise" }
        OSVersion = if ($os) { $os.Version } else { "10.0.26100" }
        OSBuild = if ($os) { $os.BuildNumber } else { "26100" }
        Architecture = if ($os) { $os.OSArchitecture } else { "64-bit" }
        TotalPhysicalMemoryMB = if ($cs) { [math]::Round($cs.TotalPhysicalMemory / 1MB, 2) } else { 4096 }
        FreePhysicalMemoryMB = if ($os) { [math]::Round($os.FreePhysicalMemory / 1024, 2) } else { 2048 }
        ProcessorName = if ($cpu) { $cpu.Name } else { "Virtual CPU" }
        DotNetVersions = (Get-ChildItem 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP' -Recurse -ErrorAction SilentlyContinue | Get-ItemProperty -Name Version -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Version -Unique)
        Resolution = try { "$([System.Windows.Forms.Screen]::PrimaryScreen.Bounds.Width)x$([System.Windows.Forms.Screen]::PrimaryScreen.Bounds.Height)" } catch { "1920x1080" }
        ExecutionTime = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssZ")
    }

    $envJson = Join-Path $ResultsDir "environment_info.json"
    $envInfo | ConvertTo-Json -Depth 4 | Set-Content -Path $envJson -Encoding UTF8
    Log-Bootstrap "[OK] Environment diagnostics captured: $($envInfo.OSName) ($($envInfo.Resolution))"
} catch {
    Log-Bootstrap "[WARN] Could not capture full CIM diagnostics: $_"
}

# 3. Launch Master Test Orchestrator
$RunTestsScript = Join-Path $SourceDir "qa\sandbox-run-tests.ps1"
if (Test-Path $RunTestsScript) {
    Log-Bootstrap "Invoking sandbox-run-tests.ps1..."
    & powershell.exe -ExecutionPolicy Bypass -NoProfile -File $RunTestsScript -SourceDir $SourceDir -ResultsDir $ResultsDir
} else {
    Log-Bootstrap "[ERROR] sandbox-run-tests.ps1 not found at $RunTestsScript!"
}

Log-Bootstrap "Bootstrap completed."
