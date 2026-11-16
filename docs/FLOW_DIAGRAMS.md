# FleetForge — System Architecture & Workflow Flowcharts

> **Platform:** FleetForge — Zero-Touch Machine Setup & Enterprise Software Deployment  
> **Author & Architect:** Love Malhotra  
> **Target Environment:** Windows 10 / 11 (x64) • .NET Framework 4.8 / WPF  
> **Document Version:** 2.6.4 • Production Architecture Reference  

---

## 📑 Table of Contents
1. [High-Level System Overview](#1-high-level-system-overview)
2. [3-Tier Catalog Merging & Smart Dual-Key Deduplication Engine](#2-3-tier-catalog-merging--smart-dual-key-deduplication-engine)
3. [Zero-Redeployment Cloud Synchronization Flow](#3-zero-redeployment-cloud-synchronization-flow)
4. [Dual-Engine Execution & Fail-Safe Fallback Matrix](#4-dual-engine-execution--fail-safe-fallback-matrix)
5. [Hybrid Web-Redirect Affiliate Monetization Workflow](#5-hybrid-web-redirect-affiliate-monetization-workflow)
6. [Company Admin Panel & Secret Access Security Topology](#6-company-admin-panel--secret-access-security-topology)
7. [Component State Machine](#7-component-state-machine)

---

## 1. High-Level System Overview

The diagram below illustrates the end-to-end ecosystem of FleetForge, connecting client workstations, cloud CDNs, the standalone HTML Admin Builder, and upstream software repositories.

```mermaid
flowchart TB
    subgraph AdminControl ["🏢 Company / Admin Control Center"]
        BuilderHTML["tools/RecommendedAppsBuilder.html<br/>(Clean HTML5 Admin Web Panel)"]
        BatchLauncher["tools/Open-Builder.bat<br/>(1-Click Launcher)"]
        BatchLauncher --> BuilderHTML
    end

    subgraph CloudInfra ["☁️ Cloud CDN & Upstream Repositories"]
        GitHubRepo["GitHub Repository (loveonce/FleetForge)"]
        JsDelivrCDN["jsDelivr Worldwide CDN Edge Cache"]
        WingetRepo["Official Microsoft Winget CDN<br/>(16,349+ Community Packages)"]
        PartnerCDN["OEM Partner Servers<br/>(iSecure, Win Riser, BitDriver, etc.)"]
        
        GitHubRepo --> JsDelivrCDN
    end

    subgraph FleetForgeClient ["💻 FleetForge Workstation Engine (FleetForge.exe)"]
        StartupWorker["Background Thread: Live Cloud Sync"]
        CatalogEngine["3-Tier Deduplication & Catalog Engine"]
        WpfUI["FleetForge WPF Modern User Interface"]
        InstallEngine["Dual-Engine Execution & Watchdog Supervisor"]
        
        StartupWorker --> CatalogEngine
        CatalogEngine --> WpfUI
        WpfUI --> InstallEngine
    end

    BuilderHTML -->|1. Export JSON| GitHubRepo
    JsDelivrCDN -->|2. Silent Background Sync| StartupWorker
    InstallEngine -->|Direct Cloud Download| PartnerCDN
    InstallEngine -->|Winget Fallback Delivery| WingetRepo

    style AdminControl fill:#F8FAFC,stroke:#7C3AED,stroke-width:2px
    style CloudInfra fill:#EFF6FF,stroke:#2563EB,stroke-width:2px
    style FleetForgeClient fill:#ECFDF5,stroke:#059669,stroke-width:2px
```

---

## 2. 3-Tier Catalog Merging & Smart Dual-Key Deduplication Engine

FleetForge merges three distinct catalogs into one unified software store with **zero duplicate entries** and **zero modifications** to the core `apps.json` database.

```mermaid
flowchart TD
    Start([Application Startup / Catalog Refresh]) --> InitMemory[Initialize Memory Sets:<br/>seenIds = HashSet String<br/>seenNames = HashSet String<br/>mergedApps = List AppModel]

    %% Tier 1
    InitMemory --> LoadT1[Tier 1: Load data/recommended_apps.json<br/>Top Choice • Non-Winget Partner Tools]
    LoadT1 --> LoopT1{For Each ra in Tier 1}
    LoopT1 --> CheckT1{ra.id in seenIds OR<br/>ra.name in seenNames?}
    CheckT1 -->|No| AddT1[Add ra to mergedApps<br/>Register ra.id & ra.name in Memory]
    CheckT1 -->|Yes| SkipT1[Skip Duplicate ra]
    AddT1 --> LoopT1
    SkipT1 --> LoopT1

    %% Tier 2
    LoopT1 -->|Completed| LoadT2[Tier 2: Load data/custom_apps.json<br/>Custom & Affiliate Winget Overrides]
    LoadT2 --> LoopT2{For Each ca in Tier 2}
    LoopT2 --> CheckT2{ca.id in seenIds OR<br/>ca.name in seenNames?}
    CheckT2 -->|No| AddT2[Add ca to mergedApps<br/>Register ca.id & ca.name in Memory]
    CheckT2 -->|Yes Match Found| UpdateT2[Update existing app in mergedApps<br/>Override downloadUrl, silentArgs, affiliateWebUrl]
    AddT2 --> LoopT2
    UpdateT2 --> LoopT2

    %% Tier 3
    LoopT2 -->|Completed| LoadT3[Tier 3: Load base data/apps.json<br/>16,349 Official Winget Packages]
    LoadT3 --> LoopT3{For Each ba in Tier 3}
    LoopT3 --> CheckT3{ba.id in seenIds OR<br/>ba.name in seenNames?}
    CheckT3 -->|No| AddT3[Add ba to mergedApps<br/>Register ba.id & ba.name in Memory]
    CheckT3 -->|Yes Duplicate Found| DropT3[❌ DROP / SKIP ba<br/>Preserve Affiliate/Custom Entry Priority]
    AddT3 --> LoopT3
    DropT3 --> LoopT3

    %% Output
    LoopT3 -->|Completed| Finalize[Output Unified config.apps Catalog<br/>Tier 1 Pinned -> Tier 2 Custom -> Tier 3 Clean Winget]
    Finalize --> End([Render 100% Unique Catalog in UI])

    style Start fill:#2563EB,color:#fff
    style End fill:#059669,color:#fff
    style DropT3 fill:#FEE2E2,stroke:#EF4444,color:#991B1B
    style UpdateT2 fill:#FEF3C7,stroke:#F59E0B,color:#92400E
```

---

## 3. Zero-Redeployment Cloud Synchronization Flow

How companies push new software, update affiliate URLs, and change pricing deals worldwide without releasing a new `.exe` installer.

```mermaid
sequenceDiagram
    autonumber
    actor Admin as Company Admin / Marketer
    participant Builder as tools/RecommendedAppsBuilder.html
    participant GitHub as GitHub Repo (data/)
    participant CDN as jsDelivr Edge CDN
    participant Client as FleetForge.exe Workstation
    actor User as End User

    Admin->>Builder: Open Open-Builder.bat in browser
    Admin->>Builder: Select/Input software & configure affiliate links
    Admin->>Builder: Click "Download JSON"
    Builder-->>Admin: Generates valid custom_apps.json / recommended_apps.json
    
    Admin->>GitHub: Commit & push updated JSON to /data directory
    GitHub->>CDN: Automatic Edge CDN Purge & Cache Update
    
    Note over Client: User launches FleetForge.exe or clicks Refresh
    Client->>Client: Load local bundled catalog (Instant offline startup)
    Client->>CDN: Background Thread: Check remote JSON via HTTP HEAD/GET
    CDN-->>Client: Returns updated JSON payload (200 OK)
    Client->>Client: Cache locally in %LOCALAPPDATA%\FleetForge
    Client->>Client: Re-run 3-Tier Catalog Merge & Deduplication
    Client->>User: UI seamlessly displays updated affiliate tools & recommendations!
```

---

## 4. Dual-Engine Execution & Fail-Safe Fallback Matrix

FleetForge guarantees 100% installation reliability through an automated two-stage execution pipeline.

```mermaid
flowchart TD
    StartInstall([User Clicks 'Install Selected Apps']) --> ReadApp[Evaluate Next Selected Package]
    
    ReadApp --> CheckWebRedirect{Is affiliateWebUrl<br/>configured?}
    CheckWebRedirect -->|Yes| LaunchBrowser[Launch Partner Deal / Signup<br/>in User's Default Browser<br/>Process.Start affiliateWebUrl]
    CheckWebRedirect -->|No| CheckDownloadUrl
    LaunchBrowser --> CheckDownloadUrl

    CheckDownloadUrl{Is direct downloadUrl<br/>configured?}
    
    %% Engine 1: Direct Cloud Delivery
    CheckDownloadUrl -->|Yes| Engine1[Engine 1: Direct Cloud Delivery<br/>Download EXE/MSI from Partner URL]
    Engine1 --> ExecInstaller[Execute Silent Background Installer<br/>Using silentArgs: /S, /SILENT, /qn, etc.]
    ExecInstaller --> WatchdogMonitor{Watchdog Supervisor<br/>Did process exit with<br/>Code 0 or 3010 within Timeout?}
    WatchdogMonitor -->|Success| RecordSuccess[Status = 'Installed'<br/>Method = 'Direct Cloud Delivery']
    WatchdogMonitor -->|Timeout / Fail / Bad Exit| Engine2Trigger[Direct Setup Failed or Timed Out]
    
    %% Fail-Safe Fallback to Engine 2
    CheckDownloadUrl -->|No URL| Engine2Trigger
    Engine2Trigger --> CheckWingetId{Does App have valid<br/>Winget Package ID?}
    
    %% Engine 2: Winget Package Engine
    CheckWingetId -->|Yes| Engine2[Engine 2: FleetForge Winget Engine<br/>winget install --id app.id -e --silent]
    Engine2 --> WingetExec{Did Winget install<br/>successfully?}
    WingetExec -->|Success| RecordWingetSuccess[Status = 'Installed'<br/>Method = 'FleetForge Engine']
    WingetExec -->|Failure| RecordFailure[Status = 'Failed'<br/>Record Error Code in Session Log]
    
    CheckWingetId -->|No| RecordFailure
    
    RecordSuccess --> NextPackage{More Packages<br/>Selected?}
    RecordWingetSuccess --> NextPackage
    RecordFailure --> NextPackage
    
    NextPackage -->|Yes| ReadApp
    NextPackage -->|No| Summary([Show Installation Summary & Notification])

    style StartInstall fill:#2563EB,color:#fff
    style RecordSuccess fill:#ECFDF5,stroke:#059669,color:#065F46
    style RecordWingetSuccess fill:#EFF6FF,stroke:#2563EB,color:#1E40AF
    style RecordFailure fill:#FEF2F2,stroke:#EF4444,color:#991B1B
    style Engine2Trigger fill:#FEF3C7,stroke:#D97706,color:#92400E
```

---

## 5. Hybrid Web-Redirect Affiliate Monetization Workflow

How FleetForge generates revenue for software publishers, MSPs, and affiliates without adding friction to the end user.

```mermaid
flowchart LR
    subgraph UserAction ["🖱️ End-User Experience"]
        SelectApp["User Selects<br/>NordVPN / Driver Tool"]
        ClickInstall["User Clicks<br/>'Install Selected Apps'"]
        SelectApp --> ClickInstall
    end

    subgraph ParallelExecution ["⚡ Simultaneous Dual Execution"]
        ClickInstall -->|Thread A: Browser Action| OpenLink["Default Web Browser Opens:<br/>https://partner.com/?coupon=FLEETFORGE<br/>(User sees exclusive coupon / signup)"]
        ClickInstall -->|Thread B: Silent Background Installer| SilentDeploy["FleetForge Silently Deploys Software:<br/>1. Downloads or calls Winget<br/>2. Runs with /silent /qn switches<br/>3. Progress logged in UI"]
    end

    subgraph MonetizationOutcome ["💰 Revenue & Conversion"]
        OpenLink --> CookieDrop["Affiliate Cookie Stored / Deal Shown<br/>User activates license or creates account"]
        SilentDeploy --> AppReady["Software Ready in Start Menu<br/>Zero setup wizard dialogs"]
        CookieDrop & AppReady --> Commission["$$$ Affiliate Commission / OEM Bounty Triggered!"]
    end

    style UserAction fill:#F8FAFC,stroke:#64748B
    style ParallelExecution fill:#EFF6FF,stroke:#2563EB
    style MonetizationOutcome fill:#ECFDF5,stroke:#059669
```

---

## 6. Company Admin Panel & Secret Access Security Topology

FleetForge completely hides catalog builder tools from everyday end-users while giving authorized administrators immediate access through three discreet pathways.

```mermaid
flowchart TD
    UserLaunch[Launch FleetForge.exe] --> PublicUI[Clean Public Interface<br/>All Apps Catalog • Quick Setup • History]
    
    PublicUI --> IsAdmin{Is Administrator<br/>Trigger Present?}

    IsAdmin -->|No Trigger| PublicMode[Public User Mode<br/>• Zero builder buttons<br/>• Locked settings<br/>• Seamless enterprise catalog]

    %% Secret In-App Gateways
    IsAdmin -->|Trigger 1: Ctrl + Shift + B| Auth1[Secret Shortcut Activated]
    IsAdmin -->|Trigger 2: Click 'Design & Develop by Love Malhotra'| Auth2[Easter Egg Activated]
    IsAdmin -->|Trigger 3: Launch with --builder CLI flag| Auth3[CLI Flag Detected]

    Auth1 & Auth2 & Auth3 --> InAppBuilderModal[Open In-App Builder Modal<br/>Live CRUD on data/custom_apps.json]

    %% Standalone External Gateway
    StandaloneTrigger[Company Admin on Any Machine] --> RunBatch[Double Click tools/Open-Builder.bat]
    RunBatch --> HTMLPanel[tools/RecommendedAppsBuilder.html<br/>Full Browser Admin Dashboard]

    InAppBuilderModal & HTMLPanel --> JSONExport[Export & Push JSON to GitHub Cloud CDN]

    style PublicMode fill:#ECFDF5,stroke:#059669
    style InAppBuilderModal fill:#F3E8FF,stroke:#7C3AED
    style HTMLPanel fill:#EFF6FF,stroke:#2563EB
```

---

## 7. Component State Machine

State transitions of a package during deployment in the FleetForge runtime engine.

```mermaid
stateDiagram-v2
    [*] --> Pending : Selected by user / role pack
    
    Pending --> Downloading : If downloadUrl is present
    Downloading --> DownloadFailed : Network error / 404 / 0-byte file
    Downloading --> InstallingDirect : Download verified (Valid EXE/MSI)
    
    Pending --> InstallingWinget : If downloadUrl is empty
    DownloadFailed --> InstallingWinget : Fail-safe automatic fallback
    
    InstallingDirect --> WatchdogMonitoring : Process started silently
    InstallingWinget --> WatchdogMonitoring : Winget process spawned
    
    WatchdogMonitoring --> Timeout : Exceeded watchdogTimeoutMinutes
    WatchdogMonitoring --> Installed : ExitCode == 0 or 3010
    WatchdogMonitoring --> Failed : ExitCode != 0 or process killed
    
    Timeout --> [*] : Session record saved
    Installed --> [*] : Session record saved
    Failed --> [*] : Session record saved
```

---

*Document generated and maintained for the **FleetForge** open enterprise setup ecosystem.*  
*Architected by **Love Malhotra**.*
