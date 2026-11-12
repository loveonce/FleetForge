# 🚀 FleetForge — Project Projection & Strategic Roadmap

**Publisher:** Innovana Thinklabs Limited  
**Target Market:** Enterprise IT Operations, System Integrators, Managed Service Providers (MSPs), Development Agencies

---

## 1. Project Vision

To become the de facto zero-touch workstation provisioning and deployment standard for Windows workstations worldwide. FleetForge eliminates manual workstation setups, reducing onboarding turnaround times from 4 hours per machine to under 15 minutes of fully automated execution.

---

## 2. Projection Phases & Strategic Milestones

### Phase 1: Core Foundation (Completed — v1.0 to v2.4)
- [x] Native WPF C# Single-File Executable (`FleetForge.exe`) without external runtime dependencies.
- [x] Single-file self-extracting setup wizard (`FleetForge-Setup.exe`).
- [x] Pre-cached local index containing 16,349 Microsoft Winget applications.
- [x] Live Windows OS Light/Dark theme synchronization engine.
- [x] Collapsible sidebar navigation (68px collapsed mode / 250px expanded mode).
- [x] Custom role group builder with dynamic software checklist and JSON export.
- [x] Top bar Help menu with Check for Updates, Settings, and About dialogs.

### Phase 2: Enterprise Network & Automated Updates (Current — v2.5)
- [x] Automatic GitHub Releases query engine on application launch.
- [x] 1-Click automatic background download and in-place upgrade.
- [x] Strict project folder separation (`data/`, `config/`, `scripts/`, `logs/`, `docs/`, `installer/`).
- [x] Fast, non-blocking splash screen with background catalog initialization.
- [ ] Active Directory (AD) / Intune administrative deployment template (.admx).
- [ ] Enterprise JSON manifest signing (SHA-256 integrity verification).

### Phase 3: Fleet Management & Centralized Telemetry (Future Projection — v3.0)
- [ ] FleetForge Server / Cloud Dashboard: Central web console to push role definitions to all office PCs.
- [ ] Multi-Machine P2P Caching: Workstations on the same local subnet share downloaded installer binaries to conserve external WAN bandwidth.
- [ ] Machine Hardware Profiler: Automatically recommends workstation bundles based on detected CPU, RAM, and GPU capabilities.
- [ ] Automated Post-Setup Scripting: PowerShell post-install scripts (e.g., domain joining, VPN certificate enrollment, wallpaper provisioning).

---

## 3. Key Performance Indicators (KPIs)

| Metric | Target Goal | FleetForge v2.4 Performance |
| :--- | :--- | :--- |
| **Startup to Interactive Time** | < 1.0 second | ~0.7 seconds (Fast Splash Engine) |
| **Catalog Search Latency** | < 50ms across 16,000+ apps | ~12ms (Virtual Index Filtering) |
| **Workstation Setup Time (15 apps)**| < 20 minutes | 12-18 minutes (Network dependent) |
| **Memory Footprint at Idle** | < 80 MB RAM | ~52 MB RAM |
| **Installer Binary Size** | < 25 MB complete | ~20.6 MB (Includes full 16,349 apps DB) |

