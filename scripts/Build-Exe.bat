@echo off
setlocal
cd /d "%~dp0.."

set CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe
if not exist "%CSC%" (
    echo [ERROR] C# compiler not found at %CSC%
    exit /b 1
)

set ICON_FILE=src\assets\appicon.ico

set APP_URLS_JSON=app_urls.json
if not exist "%APP_URLS_JSON%" set APP_URLS_JSON=config\app_urls.json

set RECOMMENDED_JSON=data\recommended_apps.json
if not exist "%RECOMMENDED_JSON%" set RECOMMENDED_JSON=recommended_apps.json

echo ==========================================================
echo        Building FleetForge Desktop Executable (.EXE)
echo ==========================================================
echo [Active Icon]: %ICON_FILE%
echo [URLs Source]: %APP_URLS_JSON%
echo [Recommended]: %RECOMMENDED_JSON%
echo [1/2] Compiling C# WPF Application...
taskkill /F /IM FleetForge.exe >nul 2>&1
ping -n 2 127.0.0.1 >nul 2>&1
if exist FleetForge.exe (
    del /f /q FleetForge_old.exe >nul 2>&1
    ren FleetForge.exe FleetForge_old.exe >nul 2>&1
)

"%CSC%" /codepage:65001 /target:winexe /optimize+ /out:FleetForge.exe ^
    /win32manifest:src\app.manifest ^
    /win32icon:%ICON_FILE% ^
    /resource:%ICON_FILE%,appicon.ico ^
    /resource:%APP_URLS_JSON%,app_urls.json ^
    /resource:%RECOMMENDED_JSON%,recommended_apps.json ^
    /reference:"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF\PresentationFramework.dll" ^
    /reference:"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF\PresentationCore.dll" ^
    /reference:"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF\WindowsBase.dll" ^
    /reference:"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Xaml.dll" ^
    /reference:"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Web.Extensions.dll" ^
    /reference:"System.Windows.Forms.dll" ^
    /reference:"System.Drawing.dll" ^
    /reference:"System.dll" ^
    /reference:"System.Core.dll" ^
    src\Program.cs src\AppUrls.cs

if %errorlevel% neq 0 (
    echo.
    echo [ERROR] Build failed.
    exit /b %errorlevel%
)

if exist FleetForge_old.exe del /f /q FleetForge_old.exe >nul 2>&1

echo [2/2] Build Successful!
echo Generated standalone executable:
echo   %cd%\FleetForge.exe
echo.
echo You can now double-click FleetForge.exe directly!
echo.
