# 🗄️ FleetForge — Database & Catalog Schema Specification

**Publisher:** Love Malhotra  
**Data Storage Format:** JSON (JavaScript Object Notation)  
**Primary Database File:** `data/apps.json` (Curated Roles, Categories & Packages)

---

## 1. Catalog Architecture

FleetForge separates application data into two primary collections within a unified JSON manifest:
1. **`groups`**: Role-based software profiles (e.g., Software Developer, UI/UX Designer, DevOps Engineer).
2. **`apps`**: Complete indexed catalog of all available Winget packages with metadata and installation parameters.

```
+------------------------------------------------------------------+
|                            ConfigModel                           |
+------------------------------------------------------------------+
| - groups: List<GroupModel>                                       |
|     + id: string                                                 |
|     + name: string                                               |
|     + icon: string (Emoji or Glyph)                              |
|     + description: string                                        |
|     + apps: List<string> (Foreign Key IDs mapping to AppModel.id)|
+------------------------------------------------------------------+
| - apps: List<AppModel>                                           |
|     + id: string (Winget Package Identifier, e.g. "Git.Git")     |
|     + name: string (Human-Readable Software Name)                |
|     + category: string (Taxonomy Bucket)                         |
|     + description: string                                        |
|     + offlineInstaller: string (Filename in offline_installers/) |
|     + silentArgs: string (Custom Silent Parameters if any)       |
+------------------------------------------------------------------+
```

---

## 2. JSON Schema Definition

### 2.1 Group Entity Schema
```json
{
  "id": "developer",
  "name": "Software Developer",
  "icon": "💻",
  "description": "Essential development environments, version control, compilers, and debuggers.",
  "apps": [
    "Microsoft.VisualStudioCode",
    "Git.Git",
    "Docker.DockerDesktop",
    "OpenJS.NodeJS.LTS",
    "Python.Python.3.12",
    "Postman.Postman",
    "Microsoft.WindowsTerminal",
    "Microsoft.PowerToys"
  ]
}
```

### 2.2 App Entity Schema
```json
{
  "id": "Microsoft.VisualStudioCode",
  "name": "Visual Studio Code",
  "category": "development",
  "description": "Code editor redefined and optimized for building modern web and cloud apps",
  "offlineInstaller": "VSCodeUserSetup-x64.exe",
  "silentArgs": "/VERYSILENT /NORESTART"
}
```

---

## 3. Taxonomy Categories (Taxonomy Engine)

Every package in the 16,349-item index is categorized into one of the following primary taxonomy buckets:

| Category ID | Display Name | Icon | Typical Packages |
| :--- | :--- | :--- | :--- |
| `utilities` | Utilities & System Tools | 🛠️ | 7-Zip, PowerToys, Rufus, TreeSize, CPU-Z |
| `browsers` | Web Browsers | 🌐 | Google Chrome, Mozilla Firefox, Brave, Opera |
| `development`| Development & Code | 💻 | VS Code, Git, Node.js, Python, Docker Desktop |
| `office` | Office & Productivity | 💼 | Acrobat Reader, Slack, Zoom, LibreOffice, Notion |
| `creative` | Design & Media | 🎨 | Blender, Figma, GIMP, OBS Studio, DaVinci Resolve |
| `gaming` | Gaming & Emulation | 🎮 | Steam, Epic Games Launcher, Discord, RetroArch |
| `security` | Security & Privacy | 🛡️ | Wireshark, Bitwarden, KeePass, ProtonVPN |
| `all` | All Packages | 🌐 | Aggregated search across all 16,349 indexed items |

---

## 4. Multi-Tier Cache Lookup Strategy

When FleetForge resolves catalogs at runtime:
1. **Tier 1:** Local Disk Cache (`data/apps.json`).
2. **Tier 2:** Root Fallback (`apps.json`).
3. **Tier 3:** Embedded Binary Cache (embedded during compilation).
4. **Tier 4:** Cloud Synchronized Index (GitHub Raw / GDrive API if configured in `config/settings.json`).
5. **Tier 5:** Dynamic Online Winget Search (Real-time live queries against Microsoft's package index for uncataloged packages).

