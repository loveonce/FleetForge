# 📋 FleetForge — Changelog & Release Notes

All notable changes to the FleetForge enterprise workstation deployment suite are documented in this file.

---

## [2.5.0] — 2026-11-12
### Added & Updated Branding
- **Publisher & Author:** Officially published and branded by **Love Malhotra** ([GitHub: loveonce](https://github.com/loveonce/FleetForge)).
- **Version Lock:** Locked release **v2.5.0** across all portable and installer binaries.
- **Single-Instance Enforcement:** Implemented Win32 named Mutex (`Local\FleetForge_SingleInstance_LoveMalhotra`) with `SetForegroundWindow` and `ShowWindowAsync` to restore the active window if clicked multiple times.
- **Native UTF-8 Emoji & Symbol Rendering:** Restored crisp emojis and symbol icons across all role environments (`💻`, `💼`, `🎨`, `⚡`) and catalog chips (`🌐`, `💬`, `📄`, `🛠️`, `🎬`, `🛡️`, `☁️`, `🎮`) with explicit `Segoe UI Emoji` font prioritization and automatic mojibake sanitization.
- **Strict UTF-8 WebClient Encoding:** Configured `WebClient.Encoding = Encoding.UTF8` across all remote GitHub sync and update channels, preventing ANSI byte corruption.

### Cleaned & Optimized
- **Purged 3 MB Raw Dump (`data/apps_full.json`):** Removed uncurated scrapings, reducing the setup installer size and repo footprint.
- **Removed Obsolete Prototype Script:** Removed `scripts/PreInstaller-GUI.ps1` (108 KB) in favor of the production C# WPF client.
- **Workspace Rename:** Replaced legacy `Software Pre Installer.code-workspace` with modern `FleetForge.code-workspace`.
- **Logs & Reports Sanitization:** Cleared local debug logs and purged leftover test reports.

---

## [2.4.0] — 2026-11-11
### Added
- **Company Branding:** Enterprise publication under **Love Malhotra**.
- **Modern Startup Splash Screen:** Non-blocking fast launch with progress visualization and background initialization.
- **Top Bar Help Menu:** Located near the minimize button with Check for Updates, Settings, and About screens.
- **Interactive Update Center:** Automatic GitHub release checking, version comparison, and 1-click in-place update download.
- **Structured Project Architecture:** Complete folder separation (`data/`, `config/`, `scripts/`, `logs/`, `docs/`, `installer/`).
- **Master Build Automation (`Build-All.bat`):** 1-Click compiler for both portable client and setup wizard.
- **Expanded Software Catalog:** 16,349 Microsoft Winget software packages indexed in `data/apps.json`.
- **Master Application Icon (`src/assets/appicon.ico`):** Multi-resolution 13-frame icon (16x16 to 768x768).

### Improved
- **Installer Layout:** Removed vertical scrollbar in the Setup Wizard for clean layout presentation.
- **Settings Screen:** Redesigned with deployment flags, offline directory browser, and cache cleanup.
- **About Us Screen:** Redesigned with modern company info, verified badges, and hardware/system specifications.
- **Splash Performance:** Replaced synchronous UI thread catalog parsing with asynchronous background loading.

---

## [2.3.0] — 2026-11-10
### Added
- **Live Windows Theme Sync:** Real-time synchronization with Windows Light/Dark theme via `WM_SETTINGCHANGE` hooks.
- **Collapsible Sidebar:** Smooth animated navigation toggle (68px collapsed mode / 250px expanded mode).
- **PE Versioning Metadata:** Embedded FileVersion, ProductVersion, and Company metadata across all binaries.

---

## [2.2.0] — 2026-11-08
### Added
- **Single-Checkbox Catalog Header:** Replaced dual Select All/Clear buttons with unified master checkbox.
- **Custom Role Group Editor:** UI builder to create, edit, and delete workstation software bundles.
- **Virtual Batch Rendering:** Streaming renderer for 16,000+ packages preventing WPF UI freezing.

---

## [2.0.0] — 2026-11-06
### Added
- **Standalone Setup Wizard (`FleetForge-Setup.exe`):** Self-extracting single-file installer.
- **Windows Integration:** Automatic registration in Windows Installed Apps and Start Menu.
- **Clean Uninstaller (`Uninstall.exe`):** Silent shortcut and directory removal utility.

---

## [1.0.0] — 2026-11-05
### Added
- Initial proof-of-concept PowerShell deployment script for curated developer environments.

