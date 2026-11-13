# 📁 FleetForge — Project Files & Directory Index

**Publisher:** Love Malhotra  
**Repository Structure Standard:** Enterprise Clean

---

## Complete Project Directory Map

```text
FleetForge/
│
├── FleetForge.exe                      # Standalone Portable Application Executable
├── FleetForge-Setup.exe                # Standalone Self-Extracting Windows Setup Wizard
├── CHANGELOG.md                        # Official Release & Version History
├── README.md                           # Main Project Documentation & Setup Guide
├── FleetForge.code-workspace           # Multi-root Workspace Configuration
│
├── 📂 data/                            # Application Catalogs & Presets
│   ├── apps.json                       # Curated Roles, Categories & Packages Catalog Index
│   ├── sample_apps.json                # Starter Presets & Reference Definitions
│   └── version.json                    # GitHub Auto-Update Release Manifest (v2.6.1)
│
├── 📂 config/                          # Configuration & Enterprise Settings
│   ├── app_urls.json                   # Cloud Sync & Auto-Update URLs Configuration
│   └── settings.json                   # Theme, silent engine flags, and cloud sync settings
│
├── 📂 src/                             # Core Application Source Code
│   ├── Program.cs                      # Main FleetForge WPF Client Source Code
│   ├── AppUrls.cs                      # Centralized Sync & Update Link Configuration
│   ├── app.manifest                    # Windows Administrator UAC Execution Manifest
│   └── 📂 assets/                      # Graphic & Visual Assets
│       └── appicon.ico                 # Master Multi-Resolution App Icon (16x16 to 768x768)
│
├── 📂 scripts/                         # Automation & Build Scripts
│   ├── Build-All.bat                   # Master 1-Click Compiler (Builds App + Setup Wizard)
│   ├── Build-Exe.bat                   # Rapid Standalone Compiler for FleetForge.exe
│   ├── PreInstaller-CLI.ps1            # Headless Unattended CLI Deployment Engine
│   ├── Update-Catalog-From-Git.bat     # Quick catalog updater from remote GitHub
│   └── Update-Catalog-From-Git.ps1     # PowerShell script for remote catalog synchronization
│
├── 📂 installer/                       # Dedicated Installer & Packaging Subsystem
│   ├── Build-Installer.bat             # Compiles & packages Setup Wizard
│   ├── SetupProgram.cs                 # Self-extracting Setup Wizard Source Code
│   └── UninstallProgram.cs             # Clean Windows Uninstaller Source Code
│
├── 📂 logs/                            # Deployment & Audit Logging
│   └── install-log.txt                 # Timestamped Execution & Setup Activity Log
│
├── 📂 reports/                         # Machine Compliance Audit Reports (HTML & JSON)
│
├── 📂 docs/                            # Enterprise Architectural Documentation
│   ├── SYSTEM_DESIGN.md                # System Architecture & WPF Component Hierarchy
│   ├── PROJECT_PROJECTION.md           # Product Roadmap & Strategic Milestones
│   ├── THINKING_AND_LOGIC.md           # Engineering Principles & Rationale
│   ├── DATABASE_AND_CATALOG.md         # Database Schema & Taxonomy Models
│   ├── AUTO_UPDATE_SYSTEM.md           # Auto-Update Mechanism & GitHub Release Flow
│   └── PROJECT_FILES_INDEX.md          # Complete Project File Index & Responsibilities
│
└── 📂 offline_installers/              # Offline Local Installer Binaries Cache
    └── README.md                       # Offline Caching Guidelines & Documentation
```

