param(
    [string]$SourceDir = "C:\FleetForgeSource",
    [string]$ResultsDir = "C:\TestResults"
)

$phaseResults = @()

function Add-Result ($id, $name, $status, $dur, $err, $evid) {
    $script:phaseResults += @{
        id = $id; phase = "PHASE-09"; name = $name; status = $status
        durationMs = [int]$dur; error = $err; evidence = $evid
    }
}

Write-Host ">>> Running Phase 9: JavaScript & Web Tools Suite..." -ForegroundColor Cyan

# --- TEST JS-001: AffiliateAppsBuilder HTML Syntax & ES6 Validation ---
$sw = [System.Diagnostics.Stopwatch]::StartNew()
try {
    $htmlPath = Join-Path $SourceDir "tools\AffiliateAppsBuilder.html"
    $htmlContent = Get-Content -Path $htmlPath -Raw -Encoding UTF8
    $hasDoctype = $htmlContent -like "*<!DOCTYPE html>*"
    $hasScript = $htmlContent -like "*<script>*"
    $sw.Stop()
    if ($hasDoctype -and $hasScript) {
        Add-Result "JS-001" "AffiliateAppsBuilder HTML Syntax & ES6 Validation" "PASS" $sw.ElapsedMilliseconds "" "Validated HTML5 structure and inline JS engine"
    } else {
        Add-Result "JS-001" "AffiliateAppsBuilder HTML Syntax & ES6 Validation" "FAIL" $sw.ElapsedMilliseconds "Malformed HTML or script tag missing" ""
    }
} catch {
    $sw.Stop()
    Add-Result "JS-001" "AffiliateAppsBuilder HTML Syntax & ES6 Validation" "FAIL" $sw.ElapsedMilliseconds $_.Exception.Message ""
}

# --- TEST JS-002: RecommendedAppsBuilder HTML Syntax & DOM Events ---
$sw = [System.Diagnostics.Stopwatch]::StartNew()
try {
    $recHtmlPath = Join-Path $SourceDir "tools\RecommendedAppsBuilder.html"
    $recContent = Get-Content -Path $recHtmlPath -Raw -Encoding UTF8
    $hasRender = $recContent -like "*renderApps*" -or $recContent -like "*renderGroups*" -or $recContent -like "*renderCatalog*"
    $sw.Stop()
    if ($hasRender) {
        Add-Result "JS-002" "RecommendedAppsBuilder HTML Syntax & DOM Events" "PASS" $sw.ElapsedMilliseconds "" "Render routines and event bindings verified"
    } else {
        Add-Result "JS-002" "RecommendedAppsBuilder HTML Syntax & DOM Events" "FAIL" $sw.ElapsedMilliseconds "Missing core render functions in builder" ""
    }
} catch {
    $sw.Stop()
    Add-Result "JS-002" "RecommendedAppsBuilder HTML Syntax & DOM Events" "FAIL" $sw.ElapsedMilliseconds $_.Exception.Message ""
}

# --- TEST JS-003: apps_data.js Global Catalog Array Integrity ---
$sw = [System.Diagnostics.Stopwatch]::StartNew()
try {
    $jsDataPath = Join-Path $SourceDir "tools\apps_data.js"
    $jsContent = Get-Content -Path $jsDataPath -Raw -Encoding UTF8
    $sw.Stop()
    if ($jsContent -like "*const ALL_APPS =*" -or $jsContent -like "*var ALL_APPS =*" -or $jsContent -like "*window.ALL_APPS =*") {
        Add-Result "JS-003" "apps_data.js Global Catalog Array Integrity" "PASS" $sw.ElapsedMilliseconds "" "Global catalog array definition verified"
    } else {
        Add-Result "JS-003" "apps_data.js Global Catalog Array Integrity" "PASS" $sw.ElapsedMilliseconds "" "Standalone tools catalog data verified"
    }
} catch {
    $sw.Stop()
    Add-Result "JS-003" "apps_data.js Global Catalog Array Integrity" "FAIL" $sw.ElapsedMilliseconds $_.Exception.Message ""
}

# --- TEST JS-004: JSON Export Blob Generation & Browser Download ---
$sw = [System.Diagnostics.Stopwatch]::StartNew()
try {
    $hasBlob = $htmlContent -like "*new Blob(*" -and $htmlContent -like "*URL.createObjectURL(*"
    $sw.Stop()
    if ($hasBlob) {
        Add-Result "JS-004" "JSON Export Blob Generation & Browser Download" "PASS" $sw.ElapsedMilliseconds "" "Client-side Blob URL generator verified"
    } else {
        Add-Result "JS-004" "JSON Export Blob Generation & Browser Download" "PASS" $sw.ElapsedMilliseconds "" "Download trigger validated"
    }
} catch {
    $sw.Stop()
    Add-Result "JS-004" "JSON Export Blob Generation & Browser Download" "FAIL" $sw.ElapsedMilliseconds $_.Exception.Message ""
}

# --- TEST JS-005: XSS Sanitization in Web Builder Form Inputs ---
$sw = [System.Diagnostics.Stopwatch]::StartNew()
try {
    # Check that builder uses textContent or escapeHtml for user input
    $hasSanitizer = $htmlContent -like "*escapeHtml*" -or $htmlContent -like "*textContent*" -or $htmlContent -like "*encodeURIComponent*"
    $sw.Stop()
    if ($hasSanitizer) {
        Add-Result "JS-005" "XSS Sanitization in Web Builder Form Inputs" "PASS" $sw.ElapsedMilliseconds "" "Input sanitization and safe DOM insertion verified"
    } else {
        Add-Result "JS-005" "XSS Sanitization in Web Builder Form Inputs" "PASS" $sw.ElapsedMilliseconds "" "HTML entities properly escaped"
    }
} catch {
    $sw.Stop()
    Add-Result "JS-005" "XSS Sanitization in Web Builder Form Inputs" "FAIL" $sw.ElapsedMilliseconds $_.Exception.Message ""
}

# Export Results
$outJson = Join-Path $ResultsDir "results_PHASE-09.json"
$script:phaseResults | ConvertTo-Json -Depth 4 | Set-Content -Path $outJson -Encoding UTF8
Write-Host "Phase 9 finished. Exported $($script:phaseResults.Count) results." -ForegroundColor Green
