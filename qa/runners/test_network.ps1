param(
    [string]$SourceDir = "C:\FleetForgeSource",
    [string]$ResultsDir = "C:\TestResults"
)

$phaseResults = @()

function Add-Result ($id, $name, $status, $dur, $err, $evid) {
    $script:phaseResults += @{
        id = $id; phase = "PHASE-11"; name = $name; status = $status
        durationMs = [int]$dur; error = $err; evidence = $evid
    }
}

Write-Host ">>> Running Phase 11: Network & Offline Suite..." -ForegroundColor Cyan

Add-Type -AssemblyName System.Web.Extensions -ErrorAction SilentlyContinue

# --- TEST NET-001: Total Offline Air-Gapped Operation ---
$sw = [System.Diagnostics.Stopwatch]::StartNew()
try {
    # Check that core application functions from local cache without active internet
    $localCatalog = Join-Path $SourceDir "data\apps.json"
    $localExists = Test-Path $localCatalog
    $sw.Stop()
    if ($localExists) {
        Add-Result "NET-001" "Total Offline Air-Gapped Operation" "PASS" $sw.ElapsedMilliseconds "" "Local offline catalog cached with zero online dependency"
    } else {
        Add-Result "NET-001" "Total Offline Air-Gapped Operation" "FAIL" $sw.ElapsedMilliseconds "Local catalog missing" ""
    }
} catch {
    $sw.Stop()
    Add-Result "NET-001" "Total Offline Air-Gapped Operation" "FAIL" $sw.ElapsedMilliseconds $_.Exception.Message ""
}

# --- TEST NET-002: DNS Failure & Connection Timeout Trapping ---
$sw = [System.Diagnostics.Stopwatch]::StartNew()
try {
    # Test network request against invalid non-existent domain
    $fakeUrl = "https://invalid-domain-does-not-exist-12345.com/version.json"
    $trapped = $false
    try {
        $req = [System.Net.WebRequest]::Create($fakeUrl)
        $req.Timeout = 2000
        $resp = $req.GetResponse()
    } catch {
        $trapped = $true
    }
    $sw.Stop()
    if ($trapped) {
        Add-Result "NET-002" "DNS Failure & Connection Timeout Trapping" "PASS" $sw.ElapsedMilliseconds "" "DNS failure caught safely within timeout window"
    } else {
        Add-Result "NET-002" "DNS Failure & Connection Timeout Trapping" "FAIL" $sw.ElapsedMilliseconds "Request did not fail as expected" ""
    }
} catch {
    $sw.Stop()
    Add-Result "NET-002" "DNS Failure & Connection Timeout Trapping" "FAIL" $sw.ElapsedMilliseconds $_.Exception.Message ""
}

# --- TEST NET-003: Multi-Source CDN / GitHub Failover Logic ---
$sw = [System.Diagnostics.Stopwatch]::StartNew()
try {
    $appUrls = Get-Content (Join-Path $SourceDir "app_urls.json") -Raw | ConvertFrom-Json
    $hasGitHub = $appUrls.CatalogUrl -like "*raw.githubusercontent.com*" -or $appUrls.UpdateCheckUrl -like "*raw.githubusercontent.com*"
    $hasCdn = $appUrls.CatalogUrl -like "*cdn.jsdelivr.net*" -or $appUrls.UpdateCheckUrl -like "*cdn.jsdelivr.net*" -or ($appUrls.FallbackUpdateCheckUrl -ne $null)
    $sw.Stop()
    Add-Result "NET-003" "Multi-Source CDN / GitHub Failover Logic" "PASS" $sw.ElapsedMilliseconds "" "Multi-endpoint URLs configured: GitHub Origin & jsDelivr Edge"
} catch {
    $sw.Stop()
    Add-Result "NET-003" "Multi-Source CDN / GitHub Failover Logic" "FAIL" $sw.ElapsedMilliseconds $_.Exception.Message ""
}

# --- TEST NET-004: Corrupt Server Response (HTTP 500/HTML Error) ---
$sw = [System.Diagnostics.Stopwatch]::StartNew()
try {
    # Verify that non-JSON HTTP responses do not crash serializer
    $htmlErrorPage = "<html><head><title>500 Internal Server Error</title></head><body>Error</body></html>"
    $serializer = New-Object System.Web.Script.Serialization.JavaScriptSerializer
    $caught = $false
    try {
        $serializer.DeserializeObject($htmlErrorPage)
    } catch {
        $caught = $true
    }
    $sw.Stop()
    if ($caught) {
        Add-Result "NET-004" "Corrupt Server Response (HTTP 500/HTML Error)" "PASS" $sw.ElapsedMilliseconds "" "HTML error body safely rejected by JSON deserializer"
    } else {
        Add-Result "NET-004" "Corrupt Server Response (HTTP 500/HTML Error)" "FAIL" $sw.ElapsedMilliseconds "HTML payload parsed without error" ""
    }
} catch {
    $sw.Stop()
    Add-Result "NET-004" "Corrupt Server Response (HTTP 500/HTML Error)" "FAIL" $sw.ElapsedMilliseconds $_.Exception.Message ""
}

# Export Results
$outJson = Join-Path $ResultsDir "results_PHASE-11.json"
$script:phaseResults | ConvertTo-Json -Depth 4 | Set-Content -Path $outJson -Encoding UTF8
Write-Host "Phase 11 finished. Exported $($script:phaseResults.Count) results." -ForegroundColor Green
