# 📋 FleetForge — Changelog & Release Notes

All notable changes to the FleetForge enterprise workstation deployment suite are documented in this file.

---

## [2.7.0] — 2026-11-17

### Added & Certified
- **Unified Single Search with Automatic Online Fallback:** Redesigned the search architecture into a single, common, universal search box in All Applications. Queries the local 16,349 package database first; if an application is not available locally, FleetForge automatically queries the Microsoft Winget cloud repository in the background and renders the results directly into the **same table** with seamless checkbox selection, package ID inspection, and one-click installation.
- **Eliminated Redundant Action Steps:** Completely removed the separate "Search Online Apps" bottom button and prompt banner, eliminating modal popups and multi-step workflows in favor of a fast, unified inline search experience.
- **Enterprise Quality Assurance & Sandbox Test Suite:** Comprehensive automated QA verification suite across 10 distinct phases (Installation, Functional, UI Automation, .NET Deep Engine Diagnostics, JavaScript Tools, Filesystem, Network Offline, Security, Performance, and Crash Recovery) executing 64 automated test cases with a 100% pass rate in Windows Sandbox.
- **Automatic Display Resolution & UI Scaling Engine:** Dynamically measures the active display resolution, DPI scaling factor, and available Windows WorkArea bounds. Automatically scales down to 0.80x on compact screens (1366x768, 1280x720, 1024x600) to ensure zero control clipping, zero text truncation, and full accessibility of bottom action buttons.
- **User Display Zoom Preferences:** Added dedicated display scaling modes in Settings (`Auto`, `100%`, `90%`, `80%`) with live active scale feedback.
- **Dedicated Online Search Modal Popup:** Instant modal popup for live Winget online search directly from the All Applications catalog, featuring real-time status indicators, package metadata inspection, and one-click cloud additions.
- **Unified Modern Item Card Design:** Completely standardized table row cards in the Edit App Pack modal to match the All Applications catalog (10px rounded borders, active light-blue selection glow, 40x40 Fluent glyph badges, 2-line title/description hierarchy, and green status pill badges).
- **Dedicated Top Choice Branding:** Distinct amber star rosette icon (`recommended.png`) for Top Choice • Recommended pack with isolated visual identity.
- **Clean White-Label Monetization:** Stripped all internal affiliate and direct tags from software cards across the catalog, search modal, and pack editor.
- **Refined Branding Typography:** Updated the "Love Malhotra" creator attribution signature to an elegant muted slate blue-gray (`#5f6d84`).
- **Comprehensive QA Deliverables:** Published 14 formal enterprise QA deliverables, execution telemetry logs, and test evidence under `QA-Reports/`.
- **Version Lock:** Official release locked to **v2.7.0** across all binaries, manifests, setup wizard, uninstaller, QA test suite, and metadata.

## [2.6.5] — 2026-11-16

### Added & Improved
- **Automatic Display Resolution & UI Scaling Engine:** Dynamically queries the active display resolution, DPI scaling, and available Windows WorkArea bounds. Automatically scales the entire UI (fonts, cards, buttons, sidebars, and modals) down for compact screens (1366x768, 1280x720, 1080p @ 150% scaling) so controls are never oversized and buttons are never pushed beneath the taskbar.
- **Display Scaling Preferences in Settings:** Added a dedicated Display Resolution & UI Scaling preference in the Settings modal with selectable modes: Auto (Adaptive), 100% (Standard), 90% (Compact), and 80% (Small Displays) with live active scale feedback.
- **Responsive Modal Overlays & Flexible Viewports:** Standardized modal card dialogs with responsive maximum dimensions and flexible row heights so that inner checklist and search results scroll smoothly on short screens without clipping modal headers or action buttons.
- **Dedicated Online Search Modal Popup:** Introduced an instant modal popup for live Winget online search directly from the All Applications catalog, featuring live status indicators, package metadata inspection, and one-click cloud additions.
- **Unified Modern Item Card Design:** Completely standardized the table row cards in the Edit App Pack modal to match the All Applications catalog (10px rounded borders, active light-blue selection glow, 40x40 Fluent glyph badges, 2-line title/description hierarchy, and green status pill badges).
- **Dedicated Top Choice 3D Icon:** Assigned a dedicated golden rosette award ribbon icon (`recommended.png`) to the Top Choice • Recommended pack, eliminating icon sharing with Minimal Essentials.
- **Clean White-Label Monetization:** Removed revealing `⚡ AFFILIATE / DIRECT` and `🌐 AFFILIATE / REDIRECT` badges from all software cards across the catalog, search modal, and pack editor. Cleaned custom package descriptions.
- **Refined Branding Typography:** Updated the "Love Malhotra" creator attribution signature to an elegant muted slate blue-gray (`#5f6d84`), blending with modern enterprise UI themes.
- **Streamlined Pack Editor UX:** Removed redundant segmented buttons and export action from the modal, added a pinned master checkbox header bar with live selection counter.
- **Version Lock:** Official release locked to **v2.6.5** across all binaries, manifests, setup wizard, uninstaller, and metadata.

## [2.6.4] — 2026-11-16

### Added & Improved
- **Permanent Multi-Source Update Detection:** Redesigned update checking engine with direct GitHub Raw origin priority (`raw.githubusercontent.com`), nonced timestamp query parameters (`?_cb=ticks`), and HTTP no-cache headers. Evaluates all reachable endpoints for the highest release version to eliminate false "No Update Found" notices caused by CDN caching delays.
- **GitHub REST API Fallback:** Added native parsing support for `api.github.com/repos/loveonce/FleetForge/releases/latest` to cross-reference release tags directly against GitHub Releases.
- **Reports Folder Elimination:** Removed creation of `reports/` and `logs/` folders across the desktop app, setup installer, and AppData directory.
- **Rooted AppData Logging:** All runtime deployment output (`install-log.txt`) and error logs (`error-log.txt`) reside directly in `%LOCALAPPDATA%\FleetForge\`. If required, compliance reports save directly into the root AppData directory without creating subfolders.
- **Self-Cleaning Legacy Purge:** Application startup and setup wizard automatically detect and remove legacy `reports/` directories left from earlier versions.
- **Automated CDN Invalidation Tooling:** Introduced `scripts/Purge-Cdn.bat` and `scripts/Release-Update.bat` to invalidate jsDelivr edge caches globally across all endpoints during release.
- **Version Lock:** Official release locked to **v2.6.4** across all binaries, manifests, setup wizard, uninstaller, and documentation.

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

