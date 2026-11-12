# ⚡ FleetForge — Zero-Touch Machine Setup & Enterprise Software Deployment

> **Zero-Touch Machine Setup & Enterprise Software Deployment**  
> An enterprise-grade Windows 11 Desktop Application & Installer (`.EXE`) designed with modern Fluent aesthetics, live Windows theme sync, curated role presets, and instant Winget package deployment.  
> **Author & Publisher:** Love Malhotra • [GitHub Repository](https://github.com/loveonce/FleetForge) • Version 2.5.0

---

## 📦 Standalone Installer Executable (`FleetForge-Setup.exe`)

A single, self-contained Windows setup wizard that deploys FleetForge onto any PC:
* **Single-File Setup:** Share `FleetForge-Setup.exe` across IT teams via USB, Google Drive, OneDrive, or network share.
* **Standard Windows Installation:** Installs to `C:\Program Files\FleetForge`.
* **Desktop & Start Menu Shortcuts:** Creates clean shortcuts with custom high-resolution icon (`src/assets/appicon.ico`).
* **Windows Installed Apps Integration:** Registers in `Settings > Apps > Installed apps` with an official clean uninstaller (`Uninstall.exe`).

---

## 🌟 Key Features

1. **🚀 Startup Splash Screen:**
   - Frameless, high-resolution branding with progress animation and status steps before launch.
2. **❔ Help Menu (Top Right near Minimize):**
   - **🔄 Check for Updates:** Check release status, view changelog highlights, and verify catalog definitions.
   - **⚙️ Settings:** Configure silent mode flags, agreement acceptance, offline installer directory with folder browser, cache maintenance, and catalog JSON export.
   - **ℹ️ About FleetForge:** Comprehensive system and build information with large high-resolution logo.
3. **🌓 Live Windows OS Theme Sync:**
   - Automatically synchronizes with your Windows theme in real-time (`WM_SETTINGCHANGE`) without restarting.
4. **📦 16,349 Pre-Loaded Winget Packages:**
   - Microsoft Winget's entire public catalog indexed and cached locally in `data/apps.json` for instantaneous offline and online browsing.
5. **🗂️ Expandable / Collapsible Left Navigation Menu:**
   - Seamlessly collapses into a clean 68px icon bar or expands to 250px with full labels.
6. **➕ Custom Role Bundles Builder:**
   - Create and customize roles (Developer, DevOps, Designer, SysAdmin) with custom icons and fixed software packages.
7. **☁️ Cloud Sync (GitHub & Google Drive):**
   - Synchronize catalogs and role profiles team-wide from any central JSON endpoint.

---

## 🚀 How to Run

1. Double-click:
   ```text
   FleetForge.exe
   ```
   Or run the installer wizard:
   ```text
   FleetForge-Setup.exe
   ```
2. Select any role to load its pre-configured bundle, or check individual software in the **Applications Catalog**.
3. Click **⚡ Set up this machine** — silent zero-touch background installation begins automatically.

---

## 🛠️ How to Rebuild the Executables

* **To rebuild both Portable App & Setup Installer in 1 click:**
  ```cmd
  scripts\Build-All.bat
  ```
* **To rebuild only the portable app (`FleetForge.exe`):**
  ```cmd
  scripts\Build-Exe.bat
  ```
* **To rebuild only the Setup Installer Wizard (`FleetForge-Setup.exe`):**
  ```cmd
  cd installer
  Build-Installer.bat
  ```

---

## 📁 Clean & Sorted Project Structure

```text
FleetForge/
│
├── FleetForge.exe                      # ⭐ Main Standalone Desktop Client (Ready to Run)
├── FleetForge-Setup.exe                # ⭐ Full Windows Setup Installer Wizard
├── CHANGELOG.md                        # Version Release History & Change Records
├── README.md                           # Complete Documentation & Architecture Guide
├── FleetForge.code-workspace           # Multi-root Workspace Configuration
│
├── src/                                # Core Application Source & Graphics
│   ├── Program.cs                      # Main FleetForge WPF C# Desktop Application
│   ├── AppUrls.cs                      # Centralized Sync & Update Link Configuration
│   ├── app.manifest                    # Windows Administrator UAC Manifest
│   └── assets/                         # Application Visual Assets & Icons
│       └── appicon.ico                 # Master Application Icon (Single Source of Truth)
│
├── scripts/                            # Automation & Build Scripts
│   ├── Build-All.bat                   # 1-Click Master Build (Builds both App & Setup)
│   ├── Build-Exe.bat                   # Quick compiler for portable client
│   ├── PreInstaller-CLI.ps1            # Headless Unattended Provisioning CLI Engine
│   ├── Update-Catalog-From-Git.bat     # Quick catalog updater from remote GitHub
│   └── Update-Catalog-From-Git.ps1     # PowerShell script for remote catalog synchronization
│
├── installer/                          # Setup Wizard & Uninstaller Engine
│   ├── SetupProgram.cs                 # Self-Extracting WPF Installer Wizard Source
│   ├── UninstallProgram.cs             # Windows Add/Remove Programs Uninstaller
│   └── Build-Installer.bat             # Compiles & Packages FleetForge-Setup.exe
│
├── config/                             # User Preferences & Deployment Settings
│   ├── app_urls.json                   # Cloud Sync & Auto-Update URLs Configuration
│   └── settings.json                   # Theme, Silent Flags, Cloud URL Configuration
│
├── data/                               # Enterprise Software Catalogs & Databases
│   ├── apps.json                       # Active Catalog (Curated Roles, Categories & Packages)
│   ├── sample_apps.json                # Starter Presets & Reference Definitions
│   └── version.json                    # Release Metadata & Automatic Update Manifest
│
├── docs/                               # Comprehensive Technical Documentation
│   ├── SYSTEM_DESIGN.md                # System Architecture & Flow
│   ├── PROJECT_PROJECTION.md           # Business & Operational Roadmap
│   ├── THINKING_AND_LOGIC.md           # Engineering Rationale
│   ├── DATABASE_AND_CATALOG.md         # Data Catalog Specs
│   ├── AUTO_UPDATE_SYSTEM.md           # Auto-Update Architecture
│   └── PROJECT_FILES_INDEX.md          # Comprehensive File Catalog
│
├── logs/                               # Execution & Deployment Audit Logs
│   └── install-log.txt                 # Real-time Background Deployment Log
│
├── reports/                            # Machine Compliance Audit Reports (HTML & JSON)
│
└── offline_installers/                 # Air-Gapped Local Installer Cache
    └── README.md                       # Guide on caching offline MSIs/EXEs
```
