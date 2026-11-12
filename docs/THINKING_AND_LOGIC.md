# 🧠 FleetForge — Engineering Rationale & Core Logic

**Publisher:** Innovana Thinklabs Limited  
**Author:** FleetForge Core Architecture Team

---

## 1. Why FleetForge Exists

Every time an IT department or developer sets up a new PC, re-images a machine, or onboards a new engineer, they spend 3 to 6 hours downloading browsers, runtimes, IDEs, databases, communication tools, and drivers.

Traditional solutions fall into two categories:
1. **Bulky Enterprise Tools (SCCM / Intune):** Require heavy enterprise server infrastructure, complex licensing, and hours of package configuration.
2. **Ad-hoc Scripts (.bat / .ps1):** Fragile, unmaintained, lacking visual status feedback, and prone to silent failures when packages change.

**FleetForge's Core Philosophy:**
Combine the simplicity of a standalone desktop app with the power of Microsoft Winget and the speed of pre-cached offline catalogs. Zero setup, zero dependencies, zero manual intervention.

---

## 2. Key Architectural Decisions

### 2.1 Single Standalone Binary vs. Microservices / Node / Electron
- **Decision:** Pure .NET Framework 4.5+ C# with WPF compiled directly using `csc.exe`.
- **Rationale:** 
  - Windows 10 and 11 include `.NET Framework 4.x` pre-installed on 100% of consumer and enterprise editions out-of-the-box.
  - Electron apps require 150+ MB download, 300+ MB RAM, and install external Chromium runtimes. FleetForge compiles to ~6 MB and launches in milliseconds without requiring any prerequisites.

### 2.2 Microsoft Winget as Core Deployment Engine
- **Decision:** Orchestrate Microsoft Winget silently rather than maintaining custom download scrapers.
- **Rationale:**
  - Winget is the official package management standard backed by Microsoft.
  - Software vendors (Google, Mozilla, Microsoft, JetBrains, Docker) maintain their own manifest hashes, guaranteeing authentic, virus-scanned, and digitally signed binaries.
  - FleetForge adds what Winget lacks: a curated role-based GUI, mass multi-package queues, live status logs, process tree abort, and offline fallback.

### 2.3 The "Zero-Touch" Silent Switch Strategy
- Standard installer wizards prompt users for destination directories, desktop shortcuts, telemetry opt-ins, and reboot confirmations.
- FleetForge suppresses all interactivity:
  ```cmd
  --silent --disable-interactivity --accept-package-agreements --accept-source-agreements
  ```
- This ensures that a queue of 20 applications installs sequentially in the background while the technician works on other tasks.

### 2.4 Pre-Loaded Local Cache of 16,349 Packages
- Querying Winget server APIs over the internet for each search takes 2 to 5 seconds per query.
- By caching the complete index of 16,349 software packages locally in `data/apps.json`, search and filtering happen in memory in under 15 milliseconds, even when the PC is completely offline or behind a restricted firewall.

