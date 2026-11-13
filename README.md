# ⚡ FleetForge — Zero-Touch Machine Setup & Enterprise Software Deployment

> **Zero-Touch Machine Setup & Enterprise Software Deployment**  
> An enterprise-grade Windows 11 Desktop Application & Installer (`.EXE`) designed with modern Fluent aesthetics, live Windows theme sync, curated role presets, and instant silent package deployment.  
> **Author & Publisher:** Love Malhotra • [GitHub Repository](https://github.com/loveonce/FleetForge) • Version 2.6.2

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
4. **📦 16,349 Pre-Loaded Software Packages:**
   - Entire enterprise catalog indexed and cached locally in `data/apps.json` for instantaneous offline and online browsing.
5. **🗂️ Expandable / Collapsible Left Navigation Menu:**
   - Seamlessly collapses into a clean 68px icon bar or expands to 250px with full labels.
6. **➕ Custom Role Bundles Builder:**
   - Create and customize roles (Developer, DevOps, Designer, SysAdmin) with custom icons and fixed software packages.
7. **☁️ Cloud Sync (GitHub & Google Drive):**
   - Synchronize catalogs and role profiles team-wide from any central JSON endpoint.

---

## 🚀 How to Run

1. Run the installer wizard:
   ```text
   FleetForge-Setup.exe
   ```
2. Select any role to load its pre-configured bundle, or check individual software in the **Applications Catalog**.
3. Click **⚡ Set up this machine** — silent zero-touch background installation begins automatically.

---