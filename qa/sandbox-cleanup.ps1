# =========================================================================
# FleetForge Sandbox Cleanup Utility
# =========================================================================
$ErrorActionPreference = "SilentlyContinue"

Write-Host "Cleaning up test artifacts inside Sandbox..." -ForegroundColor Gray

# Terminate any orphaned FleetForge processes
Get-Process -Name "FleetForge", "FleetForge-Setup", "Uninstall" -ErrorAction SilentlyContinue | Stop-Process -Force

# Clean test temporary folders created during QA
Remove-Item -Path "C:\FleetForgeTestTemp" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "$env:TEMP\FleetForgeDownloads" -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Cleanup completed." -ForegroundColor Green
