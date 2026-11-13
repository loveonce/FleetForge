@echo off
setlocal
cd /d "%~dp0.."
title FleetForge Release and CDN Pipeline

echo ==========================================================
echo        FleetForge Automated Release and CDN Pipeline
echo ==========================================================
echo.

echo [1/3] Building master binaries (FleetForge.exe + FleetForge-Setup.exe)...
call "%~dp0Build-All.bat"
if %errorlevel% neq 0 (
    echo [ERROR] Master build failed!
    exit /b %errorlevel%
)

echo.
echo [2/3] Invalidating CDN edge caches globally...
call "%~dp0Purge-Cdn.bat"

echo.
echo [3/3] Verifying live endpoints...
powershell -NoProfile -Command ^
    "$t = (Get-Date).Ticks;" ^
    "$raw = Invoke-RestMethod -Uri ('https://raw.githubusercontent.com/loveonce/FleetForge/main/data/version.json?_t=' + $t);" ^
    "$cdn = Invoke-RestMethod -Uri ('https://cdn.jsdelivr.net/gh/loveonce/FleetForge@main/data/version.json?_t=' + $t);" ^
    "Write-Host ('[RAW GitHub Origin] Version: ' + $raw.version + ' (Code: ' + $raw.versionCode + ')') -ForegroundColor Cyan;" ^
    "Write-Host ('[jsDelivr Edge CDN] Version: ' + $cdn.version + ' (Code: ' + $cdn.versionCode + ')') -ForegroundColor Cyan;" ^
    "if ($raw.version -eq $cdn.version) {" ^
    "  Write-Host '[SUCCESS] CDNs are fully synced with origin!' -ForegroundColor Green;" ^
    "} else {" ^
    "  Write-Host '[NOTICE] CDN propagation in progress...' -ForegroundColor Yellow;" ^
    "}"

echo.
echo ==========================================================
echo        FleetForge Release Pipeline Succeeded!
echo ==========================================================
echo.
