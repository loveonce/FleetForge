# 🔄 FleetForge — Automated Update Architecture & Release Pipeline

**Publisher:** Love Malhotra  
**Target Mechanism:** Silent Self-Updating Enterprise Pipeline

---

## 1. Overview & Objectives

FleetForge features an automated update engine that ensures client workstations run the latest deployment definitions, Winget parameter improvements, and security patches without requiring manual re-installation by IT administrators.

---

## 2. Update Discovery & Version Manifest

FleetForge uses a centralized GitHub Release Manifest (`version.json`) hosted on GitHub Raw or GitHub API:

```json
{
  "version": "2.6.0",
  "buildDate": "2026-11-13",
  "title": "FleetForge v2.6.0 Enterprise IT Edition",
  "company": "Love Malhotra",
  "changelog": [
    "Zero-touch silent installation engine for Microsoft Winget",
    "Restored clean UTF-8 unicode categories and role icons",
    "Single-instance enforcement with active window foreground restoration",
    "Live Windows OS Light/Dark theme synchronization",
    "Dedicated Help & Update Center with auto-download",
    "Modular role configuration builder"
  ],
  "downloadUrl": "https://raw.githubusercontent.com/loveonce/FleetForge/main/FleetForge-Setup.exe"
}
```

---

## 3. Two-Tier Update Checking

### Tier 1: Non-Blocking Background Check on Startup
1. When `FleetForge.exe` is launched, a lightweight background worker (`ThreadPool.QueueUserWorkItem`) pings the GitHub version manifest without blocking the splash screen or UI rendering.
2. If a newer semantic version is detected (`serverVersion > currentVersion`):
   - Sets a global update indicator.
   - The top bar **`[ ❔ Help ▾ ]`** button displays an active notification dot: `Update Available (v2.6.0)`.

### Tier 2: Interactive In-App Update Center
1. The user opens the **Check for Updates** dialog from the Help menu.
2. An interactive asynchronous check pings the release server.
3. If an update is found:
   - Displays new release highlights and changelog bullets.
   - Prompts with **"🚀 Download & Install Update Automatically"**.

---

## 4. Automatic In-Place Update Execution Flow

```
[User clicks "Download & Install Update"]
             |
             v
[Download target Setup (.EXE) to %TEMP%\FleetForge-Setup-Update.exe with Progress Bar]
             |
             v
[Verify downloaded binary size & integrity]
             |
             v
[Dispatch Process.Start("%TEMP%\\FleetForge-Setup-Update.exe")]
             |
             v
[Current FleetForge.exe gracefully exits via Application.Current.Shutdown()]
             |
             v
[Setup Wizard extracts updated binaries to C:\Program Files\FleetForge]
             |
             v
[Setup Wizard relaunches updated FleetForge.exe]
```

This guarantees 100% automated, zero-touch in-place updating without file locks or orphaned processes!

