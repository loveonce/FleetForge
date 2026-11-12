# 📁 FleetForge — Project Files & Directory Index

**Publisher:** Innovana Thinklabs Limited  
**Repository Structure Standard:** Enterprise Clean

---

## Complete Project Directory Map

```text
Software Pre Installer/
│
├── FleetForge.exe                      # Standalone Portable Application Executable
├── FleetForge-Setup.exe                # Standalone Self-Extracting Windows Setup Wizard
├── CHANGELOG.md                        # Official Release & Version History
├── README.md                           # Main Project Documentation & Setup Guide
├── Software Pre Installer.code-workspace
│
├── 📂 data/                            # Application Catalogs & Presets
│   ├── apps.json                       # 16,349 Packages Master Winget Catalog Index
│   ├── apps_full.json                  # Complete Winget Raw Database Backup
│   ├── sample_apps.json                # Curated Default Workstation Presets
│   └── version.json                    # GitHub Auto-Update Release Manifest
│
├── 📂 config/                          # Configuration & Enterprise Settings
│   └── settings.json                   # Theme, silent engine flags, and cloud sync settings
│
├── 📂 src/                             # Core Application Source Code
│   ├── Program.cs                      # Main FleetForge WPF Client Source Code
│   ├── app.manifest                    # Windows Administrator UAC Execution Manifest
│   └── 📂 assets/                      # Graphic & Visual Assets
│       └── appicon.ico                 # Master Multi-Resolution App Icon (16x16 to 768x768)
│
├── 📂 scripts/                         # Automation & Build Scripts
│   ├── Build-All.bat                   # Master 1-Click Compiler (Builds App + Setup Wizard)
│   ├── Build-Exe.bat                   # Rapid Standalone Compiler for FleetForge.exe
│   ├── PreInstaller-GUI.ps1            # Fallback PowerShell WPF GUI Edition
│   └── PreInstaller-CLI.ps1            # Headless Unattended CLI Deployment Engine
│
├── 📂 installer/                       # Dedicated Installer & Packaging Subsystem
│   ├── Build-Installer.bat             # Compiles & packages Setup Wizard
│   ├── SetupProgram.cs                 # Self-extracting Setup Wizard Source Code
│   ├── UninstallProgram.cs             # Clean Windows Uninstaller Source Code
│   └── Uninstall.exe                   # Compiled Standalone Uninstaller Binary
│
├── 📂 logs/                            # Deployment & Audit Logging
│   └── install-log.txt                 # Timestamped Execution & Setup Activity Log
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

