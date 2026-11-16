@echo off
setlocal
echo ==========================================================
echo        FleetForge CDN Edge Cache Invalidation
echo ==========================================================
echo Purging jsDelivr edge caches across global PoPs...
echo.

powershell -NoProfile -Command ^
    "$urls = @(" ^
    "  'https://purge.jsdelivr.net/gh/loveonce/FleetForge@main/data/version.json'," ^
    "  'https://purge.jsdelivr.net/gh/loveonce/FleetForge@main/data/apps.json'," ^
    "  'https://purge.jsdelivr.net/gh/loveonce/FleetForge@main/data/recommended_apps.json'," ^
    "  'https://purge.jsdelivr.net/gh/loveonce/FleetForge@main/FleetForge-Setup.exe'" ^
    ");" ^
    "foreach ($u in $urls) {" ^
    "  try {" ^
    "    $res = Invoke-RestMethod -Uri $u -Method Get -TimeoutSec 10;" ^
    "    Write-Host ('[PURGED] ' + $u + ' -> status: ' + $res.status) -ForegroundColor Green;" ^
    "  } catch {" ^
    "    Write-Host ('[WARN] Cache purge failed for ' + $u + ': ' + $_.Exception.Message) -ForegroundColor Yellow;" ^
    "  }" ^
    "}"

echo.
echo ==========================================================
echo          CDN Edge Cache Invalidation Complete!
echo ==========================================================

