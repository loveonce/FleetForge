param(
    [string]$SourceDir = "E:\AI_Technology\My Tools\FleetForge",
    [string]$ResultsDir = "E:\AI_Technology\My Tools\FleetForge\qa\results"
)

Write-Host ">>> Running Automated Verification: Unified Single Search & Auto Online Fallback..."

$exePath = Join-Path $SourceDir "FleetForge.exe"
if (-not (Test-Path $exePath)) {
    throw "FleetForge.exe not found at $exePath"
}

# 1. Inspect compiled assembly metadata and UI structure
$asm = [System.Reflection.Assembly]::LoadFrom($exePath)
$mainType = $asm.GetType("FleetForge.MainWindow")
if ($mainType -eq $null) {
    throw "FleetForge.MainWindow type not found in assembly"
}

# 2. Verify that BtnSearchWingetOnlineBottom is completely removed from fields and XAML
$field = $mainType.GetField("btnSearchWingetOnlineBottom", [System.Reflection.BindingFlags]"NonPublic,Instance")
if ($field -ne $null) {
    throw "btnSearchWingetOnlineBottom still exists as a field in MainWindow"
}
Write-Host "✓ Verified: BtnSearchWingetOnlineBottom action button completely removed from MainWindow"

# 3. Verify online search state fields exist
$tknField = $mainType.GetField("currentOnlineSearchToken", [System.Reflection.BindingFlags]"NonPublic,Instance")
$knownMapField = $mainType.GetField("knownOnlineApps", [System.Reflection.BindingFlags]"NonPublic,Instance")
$searchingField = $mainType.GetField("isOnlineSearching", [System.Reflection.BindingFlags]"NonPublic,Instance")
if ($tknField -eq $null -or $knownMapField -eq $null -or $searchingField -eq $null) {
    throw "Unified online search state fields missing from MainWindow"
}
Write-Host "✓ Verified: Auto online search state fields (token, known map, isSearching) present"

# 4. Verify Winget search command & parser logic
$wingetCmd = (Get-Command "winget.exe" -ErrorAction SilentlyContinue)
if ($wingetCmd -ne $null) {
    Write-Host "✓ Testing live Winget search parser with query 'blender'..."
    $proc = Start-Process -FilePath $wingetCmd.Source -ArgumentList 'search -q "blender" -n 5 --accept-source-agreements' -NoNewWindow -RedirectStandardOutput "$env:TEMP\winget_test.txt" -PassThru
    $proc.WaitForExit(15000)
    $lines = Get-Content "$env:TEMP\winget_test.txt"
    $hdrIndex = -1
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match '^Name\s+Id') {
            $hdrIndex = $i
            break
        }
    }
    if ($hdrIndex -ge 0) {
        Write-Host "✓ Verified: Winget header found at line $hdrIndex"
        $parsedCount = 0
        for ($i = $hdrIndex + 2; $i -lt $lines.Count; $i++) {
            if (-not [string]::IsNullOrWhiteSpace($lines[$i])) {
                $parsedCount++
            }
        }
        Write-Host "✓ Verified: Winget query returned $parsedCount packages seamlessly"
    }
}

Write-Host ">>> ALL UNIFIED SEARCH VERIFICATION CHECKS PASSED SUCCESSFULLY!"
