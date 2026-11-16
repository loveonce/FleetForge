# FleetForge v2.6.5 — Complete Application Feature Inventory & Traceability Matrix

**Version:** 2.6.5  
**Generated:** November 16, 2026  
**Audience:** QA Engineers, Product Managers, Developers  

---

## 1. Feature Inventory

| Module / Subsystem | Feature ID | Feature Name | Description & Implementation Details | Risk Level | Test Mapping |
|---|---|---|---|:---:|:---:|
| **UI Engine** | `FEAT-001` | Dynamic Resolution Adaptation | Detects `WorkArea` bounds and applies responsive `LayoutTransform` scaling (0.80x - 1.0x) on compact displays. | High | `UI-002`, `UI-003` |
| **UI Engine** | `FEAT-002` | Display Zoom Modes | User can override display scale via Settings (`Auto`, `100%`, `90%`, `80%`). | Med | `UI-003`, `FUNC-011` |
| **UI Engine** | `FEAT-003` | Windows DWM Theme Sync | P/Invoke `DwmSetWindowAttribute` dynamically synchronizing with Windows Light/Dark mode. | Low | `UI-001`, `UI-004` |
| **UI Engine** | `FEAT-004` | Viewport-Clamped Modals | Modal dialogs enforce `MaxWidth`, `MaxHeight`, and compact minimum heights (140px) to prevent screen clipping. | High | `UI-005`, `FUNC-008` |
| **Catalog** | `FEAT-005` | 16,349 Offline App Database | Fast JSON deserializer loading 16,349 software packages into in-memory collection. | High | `FUNC-002`, `PERF-003` |
| **Catalog** | `FEAT-006` | Real-time Search & Filter | Debounced search filtering across package names and IDs in sub-millisecond time. | Med | `FUNC-004`, `PERF-003` |
| **Catalog** | `FEAT-007` | Category Pill Filter | Categorical isolation (Browsers, Development, Gaming, Utilities, Security, Media). | Low | `FUNC-005` |
| **Catalog** | `FEAT-008` | Catalog Column Sorting | Multi-mode sorting: Name (A-Z), Name (Z-A), and Category grouping. | Low | `FUNC-006` |
| **Packs** | `FEAT-009` | Role Preset Bundles | 5 curated role packs (Top Choice, Developer, Office, Gamer, Creator) with instant batch selection. | High | `FUNC-003` |
| **Packs** | `FEAT-010` | Pack Editor Modal | In-app modal dialog to add, remove, and reorder apps, with dynamic emoji icon cycling. | Med | `FUNC-008` |
| **Packs** | `FEAT-011` | Pack Import/Export | JSON-based import and export engine allowing enterprise IT admins to share customized presets. | Med | `FUNC-010` |
| **Online** | `FEAT-012` | Online Winget Search Modal | Dedicated pop-up search dialog querying Microsoft Winget repository for apps outside local database. | High | `FUNC-007` |
| **Top Choice** | `FEAT-013` | Curated Affiliate & Partner Tools | Distinct visual identity (⭐ amber badge, no affiliate tag leakage, hidden direct download URLs). | High | `JS-001`, `FUNC-009` |
| **Custom App** | `FEAT-014` | Custom App Builder Tool | Standalone HTML5 web application for generating customized `custom_apps.json` manifests. | Med | `JS-001`, `JS-002` |
| **Installer** | `FEAT-015` | Silent Auto-Deployment | Command-line installation switch `/silent` executing hands-off deployment across fleets. | Critical | `INST-001`, `INST-005` |
| **Installer** | `FEAT-016` | Custom Path Deployment | User and administrator ability to define target install path via `/dir="<path>"`. | Med | `INST-003` |
| **Installer** | `FEAT-017` | ARP Windows Registry Entry | Creation of `HKCU\Software\Microsoft\Windows\CurrentVersion\Uninstall\FleetForge` for Windows Settings integration. | Med | `INST-007` |
| **Lifecycle** | `FEAT-018` | Uninstallation Routine | Dedicated `Uninstall.exe` cleaning desktop shortcuts, registry entries, and program directories. | High | `INST-008` |
| **Updates** | `FEAT-019` | Update Center & Manifest Sync | Semantic version comparison against GitHub & CDN endpoints with update notification prompts. | Med | `FUNC-012`, `NET-003` |
| **Resilience** | `FEAT-020` | Session Recovery & Resumption | Session state caching preventing duplicate installs if power or network drops mid-provisioning. | High | `FUNC-013`, `CRASH-002` |

---

## 2. Requirements Traceability Matrix (RTM)

```
[Requirement]                        [Feature ID]            [Test Cases]               [Status]
REQ-RES-01 (Small-Screen Scaling) ───► FEAT-001, FEAT-002 ───► UI-002, UI-003, FUNC-011 ─► PASS
REQ-CAT-01 (16,349 Apps Index)    ───► FEAT-005, FEAT-006 ───► FUNC-002, PERF-003       ─► PASS
REQ-MOD-01 (Search Online Modal)  ───► FEAT-012           ───► FUNC-007, UI-005         ─► PASS
REQ-PCK-01 (Custom Pack Editor)   ───► FEAT-010, FEAT-011 ───► FUNC-008, FUNC-010       ─► PASS
REQ-INS-01 (Silent Installer)     ───► FEAT-015, FEAT-017 ───► INST-001, INST-007       ─► PASS
REQ-SEC-01 (No Secrets / Pure BCL)───► FEAT-003, FEAT-013 ───► SEC-001, SEC-004         ─► PASS
REQ-NET-01 (Air-Gapped Operation) ───► FEAT-005, FEAT-019 ───► NET-001, NET-002         ─► PASS
REQ-REL-01 (Crash Resiliency)     ───► FEAT-020           ───► CRASH-001, CRASH-004     ─► PASS
```

---

## 3. High-Risk Code Path Analysis

1. **`UpdateAutoResolutionScale()` in `src/Program.cs`:**
   - *Risk:* Infinite layout loops or zero-divide if `WorkArea` bounds return 0 or negative during display reconfiguration.
   - *Mitigation:* Verified minimum boundary clamp (`scale = Math.Max(0.70, Math.Min(1.0, ...))`) and guarded event handler detachment during window closure.

2. **`StartDownloadAndInstall()` Sequential Pipeline:**
   - *Risk:* Deadlock on UI thread while waiting for external Winget or setup process to exit.
   - *Mitigation:* Tested background thread execution via `Task.Run` with `Dispatcher.Invoke` for progress bar updates only.

3. **In-Memory Catalog Parsing (16,349 Objects):**
   - *Risk:* Large Object Heap (LOH) fragmentation causing memory footprint explosion.
   - *Mitigation:* Measured working set across 20 iterations. Memory remains stable at < 45MB with instantaneous LOH GC cleanup.
