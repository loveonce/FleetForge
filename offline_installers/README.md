# Offline Installers Folder (Optional)

If your team has slow internet, or you want instant offline installation from a USB drive without re-downloading software every time:

1. Download the offline standalone installer (`.exe` or `.msi`) for any software.
2. Place the installer file directly into this folder (`offline_installers\`).
3. Check `apps.json` to see or set the matching `offlineInstaller` file name:
   - For Google Chrome: name it `ChromeStandaloneSetup64.exe`
   - For VS Code: name it `VSCodeSetup-x64.exe`
   - For Git: name it `Git-64-bit.exe`
   - For 7-Zip: name it `7z-x64.exe`
   - For Python: name it `python-3.12-amd64.exe`
   - For VLC: name it `vlc-win64.exe`
   - For Notepad++: name it `npp.Installer.x64.exe`

### How it works:
When you click **"Install"**, the tool checks this folder first:
- If a local installer is found, it installs immediately **offline with 0 internet download**.
- If no offline file is found, it automatically falls back to **Winget** over the internet.

