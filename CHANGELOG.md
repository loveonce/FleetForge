# 📋 FleetForge — Changelog & Release Notes

All notable changes to the FleetForge enterprise workstation deployment suite are documented in this file.

---

## [2.6.3] — 2026-11-13

### Added
- **System Tray Docking:** Minimizing the window docks FleetForge into the Windows System Tray (`NotifyIcon`) with custom icon, live progress tooltip, and native context menu (`Open FleetForge`, live status, `Exit FleetForge`).
- **Windows 11 Fluent Toaster Notification Popup:** Active background downloads and installations display a sleek, interactive Windows 11 toast popup in the lower-right work area showing current app, real-time progress bar, and percentage. Clicking the toast restores FleetForge.
- **Windows 11 Colorful Fluent Icon Badges:** Navigation rail and App Pack cards now feature vibrant theme-adaptive colorful icon badges (Coding Blue, Office Amber, Creative Purple, Emerald Green).
- **Graceful Background Setup Prompt:** Closing FleetForge during active installation offers the choice to minimize to the System Tray without terminating ongoing setups.
- **Directory Cleanup & Optimization:** Purged obsolete duplicates and verified zero-runtime standalone build pipeline.
- **Version Lock:** Official release locked to **v2.6.3** across all binaries, setup wizard, and metadata.

## [2.6.2] — 2026-11-13

### Changed & Improved
- **Clean Catalog Navigation:** Navigating to "All Applications" (Catalog view) now presents an unselected, clean checklist (0 selected apps) instead of pre-selecting tools from the active Quick Setup app package.
- **Independent Selection Architecture:** App pack installations in Quick Setup install directly from the pack card, while catalog checkboxes remain entirely dedicated to user-driven custom application selections.
- **Recommended Visibility:** Top Choice recommended tools (Win Riser, Bit Driver Updater, iSecure VPN) remain highlighted at the top of the catalog and their respective categories in an unselected state ready for one-click selection.
- **Version Lock:** Official release locked to **v2.6.2** across all portable binaries, setup wizards, and documentation.

## [2.6.1] — 2026-11-13
### Fixed & Improved
- **Silent Background System Tray Auto-Installer:** When updating an existing installation, FleetForge displays the download progress directly inside the app, and transitions seamlessly to the Windows System Tray (`NotifyIcon`) at installation time. The installer screen is completely suppressed in favor of native tray notifications and background silent installation, automatically relaunching the updated FleetForge application upon completion.
- **Prevent Duplicate App Packs on Update:** Fixed installer catalog merge logic by registering `"minimal"` in the built-in system pack whitelist and enforcing unique pack IDs and names during setup, permanently eliminating duplicate "Minimal Essentials" cards.
- **Self-Healing Runtime Catalog Deduplication:** Added automatic startup validation in `EnsureCategories()` and `LoadConfigWithAutoSync()` that scans and sanitizes any duplicate groups from local `data/apps.json`.
- **Interactive Check for Updates & Prompt Notifications:** The "Check for Updates" button remains active and interactive during manual checks without disabling. Added clear dialog prompts notifying the user when their installation is fully up to date or offering 1-click update installation when a new version is released.
- **Smart Semantic Version Comparison:** Upgraded update comparison to dynamic semantic version numbers (`ParseVersionNumber`) supporting minor, major, and patch version transitions.
- **Version Lock:** Official release locked to **v2.6.1** across all portable binaries, setup wizards, and documentation.

---

## [2.6.0] — 2026-11-13
### Added & Redesigned
- **Windows 11 Fluent Designer Splash Screen:** Completely redesigned startup splash with a frosted Mica floating card (`480×270`), squircle brand container with gentle breathing scale pulse (`1.0` ↔ `1.055`), glowing halo animation, fluid progress bar gliding (`DoubleAnimation` on `ProgressBar.ValueProperty`), and silky smooth entrance/exit transitions.
- **Dynamic Windows 11 Theme & Accent Synchronization:** Automatic detection of Windows Light and Dark themes via `AppsUseLightTheme` registry query and live accent color extraction from Windows DWM (`AccentColor`), with adaptive luminance compensation ensuring high-contrast clarity across any Windows theme.
- **Enhanced Windows 11 Search Capsule:** Redesigned top search bar into a prominent `440px` pill capsule with `18px` corner radius, instant live search filtering, and high contrast across both Light and Dark modes.
- **Streamlined Navigation & Header:** Relocated "How to Use" guide into the top-right "Help ▾" menu, removed manual theme switch button for pure automatic OS theme sync, and removed redundant profile card and Quick Setup hero blocks.
- **Interactive First-Time Tutorial Modal:** High-contrast 3-step walkthrough modal for non-technical and first-time users with crisp typography in both Light and Dark themes.
- **Multi-Drive Installation Detection:** Automated detection of existing software installations across secondary storage drives (e.g., `D:\`) prior to setup execution, preventing duplicate installations and managing overwrite behaviors.
- **Ready-Made App Pack Import & Export:** Built-in JSON export for custom app packs and an Import dialog with collision detection, duplicate warnings, and overwrite/ignore choices.
- **Version Lock:** Official release locked to **v2.6.0** across all portable binaries, setup wizards, and documentation.

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
- **Expanded Software Catalog:** 16,349 software packages indexed in `data/apps.json`.
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

