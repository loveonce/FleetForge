@echo off
setlocal
cd /d "%~dp0"

set CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe
if not exist "%CSC%" (
    echo [ERROR] C# compiler not found at %CSC%
    pause
    exit /b 1
)

set ICON_FILE=..\src\assets\appicon.ico
if not exist "%ICON_FILE%" set ICON_FILE=..\src\appicon.ico
if not exist "%ICON_FILE%" set ICON_FILE=src\appicon.ico

set APPS_JSON=..\data\apps.json
if not exist "%APPS_JSON%" set APPS_JSON=..\apps.json

set SAMPLE_JSON=..\data\sample_apps.json
if not exist "%SAMPLE_JSON%" set SAMPLE_JSON=..\sample_apps.json

set SETTINGS_JSON=..\config\settings.json
if not exist "%SETTINGS_JSON%" set SETTINGS_JSON=..\settings.json

set APP_URLS_JSON=..\app_urls.json
if not exist "%APP_URLS_JSON%" set APP_URLS_JSON=..\config\app_urls.json

echo ==========================================================
echo        Building FleetForge Setup Wizard (.EXE)
echo ==========================================================
echo [Active Icon]: %ICON_FILE%
echo [Data Source]: %APPS_JSON%
echo.

taskkill /F /IM FleetForge.exe >nul 2>&1
taskkill /F /IM FleetForge-Setup.exe >nul 2>&1
ping -n 2 127.0.0.1 >nul 2>&1

echo [1/3] Compiling Uninstaller utility (Uninstall.exe)...
"%CSC%" /codepage:65001 /target:winexe /optimize+ /out:Uninstall.exe ^
    /win32icon:%ICON_FILE% ^
    /win32manifest:..\src\app.manifest ^
    /reference:"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF\PresentationFramework.dll" ^
    /reference:"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF\PresentationCore.dll" ^
    /reference:"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF\WindowsBase.dll" ^
    /reference:"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Xaml.dll" ^
    /reference:"System.dll" ^
    UninstallProgram.cs

if %errorlevel% neq 0 (
    echo [ERROR] Failed to compile uninstaller.
    exit /b %errorlevel%
)

echo [2/3] Compiling Main Application (FleetForge.exe)...
taskkill /F /IM FleetForge.exe >nul 2>&1
taskkill /F /IM FleetForge-Setup.exe >nul 2>&1
ping -n 2 127.0.0.1 >nul 2>&1

"%CSC%" /codepage:65001 /target:winexe /optimize+ /out:..\FleetForge.exe ^
    /win32manifest:..\src\app.manifest ^
    /win32icon:%ICON_FILE% ^
    /resource:%ICON_FILE%,appicon.ico ^
    /resource:%APP_URLS_JSON%,app_urls.json ^
    /reference:"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF\PresentationFramework.dll" ^
    /reference:"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF\PresentationCore.dll" ^
    /reference:"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF\WindowsBase.dll" ^
    /reference:"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Xaml.dll" ^
    /reference:"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Web.Extensions.dll" ^
    /reference:"System.dll" ^
    /reference:"System.Core.dll" ^
    ..\src\Program.cs ..\src\AppUrls.cs

if %errorlevel% neq 0 (
    echo [ERROR] Failed to compile main application.
    exit /b %errorlevel%
)

echo [3/3] Packaging all components into FleetForge-Setup.exe...
taskkill /F /IM FleetForge-Setup.exe >nul 2>&1
ping -n 2 127.0.0.1 >nul 2>&1
if exist ..\FleetForge-Setup.exe (
    del /f /q ..\FleetForge-Setup_old.exe >nul 2>&1
    ren ..\FleetForge-Setup.exe FleetForge-Setup_old.exe >nul 2>&1
)

"%CSC%" /codepage:65001 /target:winexe /optimize+ /out:..\FleetForge-Setup.exe ^
    /win32icon:%ICON_FILE% ^
    /win32manifest:..\src\app.manifest ^
    /resource:..\FleetForge.exe,FleetForge.exe ^
    /resource:%APPS_JSON%,apps.json ^
    /resource:%SAMPLE_JSON%,sample_apps.json ^
    /resource:%SETTINGS_JSON%,settings.json ^
    /resource:%APP_URLS_JSON%,app_urls.json ^
    /resource:%ICON_FILE%,appicon.ico ^
    /resource:Uninstall.exe,Uninstall.exe ^
    /reference:"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF\PresentationFramework.dll" ^
    /reference:"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF\PresentationCore.dll" ^
    /reference:"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF\WindowsBase.dll" ^
    /reference:"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Xaml.dll" ^
    /reference:"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Windows.Forms.dll" ^
    /reference:"System.dll" ^
    /reference:"System.Drawing.dll" ^
    SetupProgram.cs

if %errorlevel% neq 0 (
    echo [ERROR] Setup compilation failed.
    exit /b %errorlevel%
)

if exist ..\FleetForge-Setup_old.exe del /f /q ..\FleetForge-Setup_old.exe >nul 2>&1
if exist ..\FleetForge_old.exe del /f /q ..\FleetForge_old.exe >nul 2>&1
if exist Uninstall.exe del /f /q Uninstall.exe >nul 2>&1

echo.
echo ==========================================================
echo           FleetForge Setup Built Successfully!
echo ==========================================================
echo Standalone Installer Executable:
echo   %~dp0..\FleetForge-Setup.exe
echo.
echo.
