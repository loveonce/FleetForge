@echo off
setlocal
cd /d "%~dp0.."
title FleetForge Master Build

echo ==========================================================
echo          FleetForge Master Build Engine
echo ==========================================================
echo.
echo [Step 1/2] Compiling Portable Application (FleetForge.exe)...
call "%~dp0Build-Exe.bat"
if %errorlevel% neq 0 (
    echo [ERROR] Portable build failed.
    exit /b %errorlevel%
)

echo.
echo [Step 2/2] Compiling Setup Wizard (FleetForge-Setup.exe)...
cd installer
call Build-Installer.bat
cd ..
if %errorlevel% neq 0 (
    echo [ERROR] Setup wizard packaging failed.
    exit /b %errorlevel%
)

echo.
echo ==========================================================
echo           ALL BUILDS COMPLETED SUCCESSFULLY!
echo ==========================================================
echo Generated binaries:
echo   [1] Portable Client:  %cd%\FleetForge.exe
echo   [2] Setup Wizard:     %cd%\FleetForge-Setup.exe
echo.
