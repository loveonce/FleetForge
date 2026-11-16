# =========================================================================
# FleetForge Master Test Orchestrator inside Windows Sandbox
# =========================================================================
param(
    [string]$SourceDir = "C:\Users\WDAGUtilityAccount\Desktop\FleetForge",
    [string]$ResultsDir = "C:\Users\WDAGUtilityAccount\Desktop\results"
)

$ErrorActionPreference = "Continue"

New-Item -ItemType Directory -Force -Path $ResultsDir -ErrorAction SilentlyContinue | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $ResultsDir "logs") -ErrorAction SilentlyContinue | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $ResultsDir "screenshots") -ErrorAction SilentlyContinue | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $ResultsDir "dumps") -ErrorAction SilentlyContinue | Out-Null

$ManifestPath = Join-Path $SourceDir "qa\test-manifest.json"
$MasterLog = Join-Path $ResultsDir "logs\master_execution.log"

function Log-Master ($msg, $color="White") {
    $ts = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss.fff")
    $line = "[$ts] $msg"
    Write-Host $line -ForegroundColor $color
    Add-Content -Path $MasterLog -Value $line -Encoding UTF8
}

Log-Master "Starting FleetForge Automated Sandbox QA Execution..." "Green"

if (-not (Test-Path $ManifestPath)) {
    Log-Master "[FATAL] Test manifest not found at $ManifestPath" "Red"
    exit 1
}

$manifest = Get-Content $ManifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
Log-Master "Loaded Test Manifest: $($manifest.testSuite) v$($manifest.version) ($($manifest.phases.Count) Phases)" "Cyan"

$allTestResults = @()

foreach ($phase in $manifest.phases) {
    Log-Master "------------------------------------------------------------" "Yellow"
    Log-Master "Executing Phase: $($phase.id) - $($phase.name)" "Yellow"
    
    $scriptRelative = $phase.script
    $scriptFullPath = Join-Path (Join-Path $SourceDir "qa") $scriptRelative
    
    if (-not (Test-Path $scriptFullPath)) {
        Log-Master "[WARN] Runner script $scriptFullPath not found. Marking tests as BLOCKED." "Red"
        foreach ($t in $phase.tests) {
            $allTestResults += @{
                id = $t.id
                phase = $phase.id
                name = $t.name
                priority = $t.priority
                status = "BLOCKED"
                durationMs = 0
                error = "Script missing: $scriptFullPath"
                evidence = ""
            }
        }
        continue
    }

    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $phaseLog = Join-Path $ResultsDir "logs\$($phase.id).log"
    
    Log-Master "Invoking runner: $scriptRelative" "Gray"
    
    # Execute runner script and capture output
    try {
        $output = & powershell.exe -ExecutionPolicy Bypass -NoProfile -File $scriptFullPath -SourceDir $SourceDir -ResultsDir $ResultsDir 2>&1
        $sw.Stop()
        $output | Out-File -FilePath $phaseLog -Encoding UTF8
        
        # Read structured phase results if produced by runner
        $phaseJson = Join-Path $ResultsDir "results_$($phase.id).json"
        if (Test-Path $phaseJson) {
            $parsed = Get-Content $phaseJson -Raw -Encoding UTF8 | ConvertFrom-Json
            foreach ($r in $parsed) {
                $allTestResults += $r
                $color = if ($r.status -eq "PASS") { "Green" } elseif ($r.status -eq "FAIL") { "Red" } else { "Yellow" }
                Log-Master "  [$($r.status)] $($r.id): $($r.name) ($($r.durationMs)ms)" $color
            }
        } else {
            # Fallback if runner did not produce structured JSON
            Log-Master "[NOTICE] Runner finished without JSON output. Recording overall exit code." "Gray"
            foreach ($t in $phase.tests) {
                $allTestResults += @{
                    id = $t.id
                    phase = $phase.id
                    name = $t.name
                    priority = $t.priority
                    status = if ($LASTEXITCODE -eq 0) { "PASS" } else { "FAIL" }
                    durationMs = $sw.ElapsedMilliseconds
                    error = if ($LASTEXITCODE -ne 0) { "Exit code: $LASTEXITCODE" } else { "" }
                    evidence = $phaseLog
                }
            }
        }
    }
    catch {
        $sw.Stop()
        Log-Master "[EXCEPTION] Error executing phase $($phase.id): $_" "Red"
    }
}

# Save comprehensive raw results JSON
$summaryPath = Join-Path $ResultsDir "test_results_raw.json"
$allTestResults | ConvertTo-Json -Depth 5 | Set-Content -Path $summaryPath -Encoding UTF8
Log-Master "Saved raw test results to $summaryPath" "Cyan"

# Launch Report Generator
$ReportScript = Join-Path $SourceDir "qa\sandbox-report.ps1"
if (Test-Path $ReportScript) {
    Log-Master "Generating comprehensive QA reports..." "Cyan"
    & powershell.exe -ExecutionPolicy Bypass -NoProfile -File $ReportScript -ResultsDir $ResultsDir -SourceDir $SourceDir
}

# Signal Completion to Host
$SignalFile = Join-Path $ResultsDir "RUN_COMPLETE.signal"
$completionTime = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
Set-Content -Path $SignalFile -Value "COMPLETED: $completionTime" -Encoding UTF8

Log-Master "============================================================" "Green"
Log-Master "All Sandbox Tests Finished Successfully at $completionTime" "Green"
Log-Master "============================================================" "Green"
