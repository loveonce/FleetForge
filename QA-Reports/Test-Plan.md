# FleetForge Enterprise QA Master Test Plan

**Version:** 2.7.0  
**Document Revision:** 3.0  
**Target Platform:** Windows 10 & Windows 11 (x64 / ARM64 via Emulation)  
**Execution Environment:** Isolated Windows Sandbox Environment  
**Security Classification:** Confidential â€” Internal Engineering Document  

---

## 1. Objectives & Scope

### 1.1 Objectives
The primary purpose of this master test plan is to establish a deterministic, repeatable, and automated testing standard for FleetForge. Every release candidate must pass through isolated Sandbox execution without host pollution, validating:
- Correctness of catalog indexing (16,349 software packages).
- Dynamic responsiveness across display resolutions from 1024x600 netbooks up to 4K multi-monitor workstations.
- Thread safety, async task lifecycles, and memory disposal.
- UAC execution requirements and standard-user sandboxing.
- Resiliency against unexpected host failures, power cuts, and interrupted network operations.

### 1.2 In-Scope
- **Binaries:** `FleetForge.exe`, `FleetForge-Setup.exe`, `Uninstall.exe`.
- **Source Modules:** `src/Program.cs`, `src/SettingsModel.cs`, `installer/SetupProgram.cs`, `installer/UninstallProgram.cs`.
- **Web Tools:** `tools/RecommendedAppsBuilder.html`, `tools/AffiliateAppsBuilder.html`, `tools/apps_data.js`.
- **Data Stores:** `data/apps.json`, `data/recommended_apps.json`, `data/custom_apps.json`, `data/version.json`.
- **Configuration & State:** `%LOCALAPPDATA%\FleetForge\config\settings.json`, session state cache.

### 1.3 Out-of-Scope
- Physical modification of host registry or host Program Files outside Windows Sandbox.
- Third-party Winget server stress testing or non-local CDN disruption.

---

## 2. Test Architecture & Harness Design

The test infrastructure is strictly decoupled into orchestration, test runners, execution sandboxes, and evidence sinks:

```
                          â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”
                          â”‚   Host Windows Machine    â”‚
                          â”‚   (Strictly Protected)    â”‚
                          â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”¬â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜
                                        â”‚ Spawns via .wsb
                                        â–¼
                  â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”
                  â”‚              Windows Sandbox              â”‚
                  â”‚   Desktop\FleetForge (ReadOnly Mapping)   â”‚
                  â”‚   Desktop\results    (Writable Mapping)   â”‚
                  â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”¬â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜
                                        â”‚
                 â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”´â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”
                 â–¼                                             â–¼
       [sandbox-setup.ps1]                            [sandbox-run-tests.ps1]
       â€¢ Detects Environment                         â€¢ Iterates 10 Test Phases
       â€¢ Validates Mappings                          â€¢ Collects Metrics & Logs
       â€¢ Generates environment_info.json             â€¢ Emits test_results_raw.json
                 â”‚                                             â”‚
                 â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”¬â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜
                                        â”‚
                                        â–¼
                              [sandbox-report.ps1]
                              â€¢ Computes Pass Rates
                              â€¢ Aggregates Phase Summaries
                              â€¢ Outputs test_summary.json
```

---

## 3. Test Phase Matrix & Methodologies

### Phase 5: Installation & Uninstallation Suite
- **Methodology:** Executes silent and interactive installer parameters (`/silent`, `/update`, `/dir=`).
- **Pass Criteria:** Process returns code 0, binaries exist at target paths, ARP registry keys created, shortcuts registered, uninstaller removes all payload files cleanly.

### Phase 6: Functional & Feature Matrix
- **Methodology:** C# reflection combined with file schema inspection against `data/apps.json` (16,349 apps).
- **Pass Criteria:** All 5 preset groups loaded, debounce filter matches valid items, pack editor instantiates models without null pointers, custom app overrides respect priority order.

### Phase 7: UI Automation & Accessibility
- **Methodology:** WPF visual tree reflection, primary screen bounds detection, `LayoutTransform` scale calculations, and theme attribute synchronization.
- **Pass Criteria:** `UpdateAutoResolutionScale` applies `0.80x` - `0.90x` on screens < 768p; modal cards enforce `MaxWidth` and `MaxHeight` viewport clamping.

### Phase 8: .NET Deep Engine Diagnostics
- **Methodology:** Assembly inspection, Task concurrency stress via thread pool, extreme JSON serialization workloads (5,000 items with Unicode, control characters, nulls), and GC collection analysis.
- **Pass Criteria:** 0 `TypeLoadException`, 0 unobserved task exceptions, delta memory < 500KB after full GC.

### Phase 9: JavaScript & Web Tools Suite
- **Methodology:** HTML5 DOM syntax validation, regex verification for modern ES6 handlers, Blob download triggers, and XSS sanitization checks.
- **Pass Criteria:** Clean HTML5 doctype, zero vulnerable innerHTML assignments without encoding.

### Phase 10: Filesystem & Data Integrity
- **Methodology:** Injection of zero-byte and corrupt JSON files, concurrent file write/read loops, and directory structure audits.
- **Pass Criteria:** Application catches corrupt files gracefully without crashing; confirms elimination of legacy `reports/` and `logs/` folders.

### Phase 11: Network & Offline Robustness
- **Methodology:** Network interface disabling, invalid host DNS resolution timeouts, HTTP 500 error payload injections.
- **Pass Criteria:** Full functionality under 100% offline air-gapped mode; non-JSON server error responses caught cleanly.

### Phase 12: Security & Vulnerability Assessment
- **Methodology:** Static security analysis for hardcoded secrets, shell argument injection strings (`& calc.exe`), path traversal patterns (`..\..\`), and assembly reference GAC audits.
- **Pass Criteria:** Zero hardcoded tokens; pure BCL assembly dependencies; proper UAC execution manifests.

### Phase 13: Performance, Memory & Stress
- **Methodology:** High-resolution stopwatch timing across cold start, working set memory inspection via `System.Diagnostics.Process`, rapid launch/exit loops (5 iterations).
- **Pass Criteria:** Cold start < 3000ms; idle memory < 150MB; 16K item search query < 500ms; handle count < 800.

### Phase 14: Crash & Failure Injection
- **Methodology:** Forced process termination (`SIGKILL`), simulation of partial download files in `%TEMP%`, and concurrent multi-instance spawns.
- **Pass Criteria:** Immediate recovery to healthy state on relaunch; no file lock deadlocks between instances.

---

## 4. Entry and Exit Criteria

### 4.1 Entry Criteria
1. Compilation of `FleetForge.exe` and `FleetForge-Setup.exe` with zero build errors.
2. Verified presence of all 16,349 application records in `data/apps.json`.
3. Valid Windows Sandbox setup configuration (`qa/Sandbox-Test.wsb`).

### 4.2 Exit Criteria (Sign-Off Gates)
1. 100% of P0 (Blocker) and P1 (Critical) test cases must pass.
2. Overall test pass rate must exceed 95% (Achieved: **100%**).
3. Zero memory leaks or orphaned processes remaining after test execution.
4. All forensic logs and evidence captured under `QA-Reports/Evidence/`.

