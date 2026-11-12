# 📐 FleetForge — System Architecture & Design Specification

**Publisher:** Innovana Thinklabs Limited  
**Product:** FleetForge Enterprise  
**Version:** 2.4.0 (Enterprise IT Edition)  
**Target Platform:** Windows 10 / Windows 11 (x64)

---

## 1. Executive Architectural Overview

FleetForge is an enterprise workstation provisioning and silent deployment engine designed to automate the initial configuration of Windows desktop systems. It provides zero-touch software installation for fresh operating system deployments, hardware refreshes, and standardized role-based employee onboarding.

```
+-------------------------------------------------------------------------+
|                         FleetForge Desktop Client                        |
+-------------------------------------------------------------------------+
|  [Presentation Layer]                                                   |
|    - WPF Hardware-Accelerated UI (Fluent Design Palette)                |
|    - Dynamic Dark / Light Theme Sync Engine (HwndSource Hook)           |
|    - Virtual Batch Rendering Engine (High-Performance App Catalog)      |
|    - Interactive Modal Overlays (Settings, Updates, Role Builder, About)|
+-------------------------------------------------------------------------+
|  [Application Logic Layer]                                              |
|    - Role Configuration Manager & Preset Profile Loader                 |
|    - Asynchronous Catalog Query & Filter Subsystem                     |
|    - Auto-Update Dispatcher & GitHub Release Client                     |
|    - Cloud Configuration Synchronizer (GitHub Raw / GDrive API)         |
+-------------------------------------------------------------------------+
|  [Execution & Orchestration Layer]                                      |
|    - Microsoft Winget Native CLI Process Orchestrator                   |
|    - Silent Switch Injector (--silent --disable-interactivity)          |
|    - Process-Tree Monitor & Cancellation Watchdog (taskkill /T /F)      |
|    - Local Offline Installer Cache Interceptor                          |
+-------------------------------------------------------------------------+
|  [Data & Storage Layer]                                                 |
|    - data/apps.json (16,349 Index Packages)                            |
|    - config/settings.json (User Preferences & Engine Options)           |
|    - logs/install-log.txt (Audit Trail & Installation Telemetry)        |
+-------------------------------------------------------------------------+
```

---

## 2. Presentation Layer & UI Architecture

### 2.1 Frameless Splash & Smooth Transition
- **SplashScreenWindow:** Borderless translucent window (`WindowStyle=None`, `AllowsTransparency=True`).
- Renders the official high-resolution application icon (`128x128` Bgra32 frame) from embedded resources.
- Non-blocking asynchronous initialization: Assembles UI elements while background threads index packages and verify cloud updates.

### 2.2 Live Windows OS Theme Synchronization
- Integrates with Windows `SystemEvents.UserPreferenceChanged` and Windows Message Hook (`WM_SETTINGCHANGE` / `0x001A`).
- Live-queries `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize\AppsUseLightTheme`.
- Seamlessly morphs interface brushes between Dark Palette (`#181818`, `#111317`, `#20242B`) and Light Palette (`#F3F3F3`, `#FFFFFF`, `#E8ECF2`) without requiring application restart.

### 2.3 Virtual Batch Rendering for 16,349 Packages
- Loading 16,349 WPF elements simultaneously causes UI thread freezing and high memory consumption.
- FleetForge implements **Virtual Batch Rendering**:
  - Initializes the view with the first 60 packages instantly.
  - Dynamically hooks `ScrollViewer.ScrollChanged` to stream the next 60 packages when the user scrolls within 250px of the boundary.
  - Keeps UI response time under 16ms (60 FPS fluid rendering).

---

## 3. Deployment Engine & Execution Flow

1. **Package Verification:** Determines if the target package has an offline installer in `offline_installers/`.
2. **Winget Invocation:** If no local installer exists, dispatches Microsoft Windows Package Manager with silent enterprise parameters:
   ```cmd
   winget install --id <PackageId> --exact --silent --disable-interactivity --accept-package-agreements --accept-source-agreements
   ```
3. **Process Supervision:** Tracks stdout/stderr in real-time and pipes timestamped logs to `logs/install-log.txt` and the interactive in-app terminal.
4. **Emergency Abort:** User can press **⏹️ Stop Setup** at any time. The engine utilizes process tree traversal (`taskkill /T /F`) to terminate both the Winget orchestrator and underlying third-party setup child processes.

---

## 4. Security & Compliance
- **Administrator Elevation:** Built-in application manifest (`requireAdministrator`) ensures permissions to write to `%ProgramFiles%` and register registry entries.
- **Offline Integrity:** Does not transmit workstation telemetry or user credentials to external servers.

