@echo off
setlocal
cd /d "%~dp0.."

powershell -ExecutionPolicy Bypass -File "%~dp0Update-Catalog-From-Git.ps1" %*

if %errorlevel% neq 0 (
    echo.
    echo [ERROR] Update failed.
    exit /b %errorlevel%
)

echo.
echo Now you can run scripts\Build-All.bat to compile the new catalog into FleetForge.exe and FleetForge-Setup.exe!
echo.

