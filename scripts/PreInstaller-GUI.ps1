<#
================================================================================
  Innovana Pre Installer (PowerShell GUI Edition)
  Automated Machine Setup & Software Deployment
  Features Dark / Light Themes, Role Bundles & Cloud Sync
================================================================================
#>

[CmdletBinding()]
param()

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $ScriptDir

Add-Type -AssemblyName PresentationFramework, PresentationCore, WindowsBase

# ------------------------------------------------------------------------------
# 1. Locate Winget Engine
# ------------------------------------------------------------------------------
function Get-WingetPath {
    if (Get-Command winget -ErrorAction SilentlyContinue) {
        return "winget"
    }

    $userAlias = "$env:LOCALAPPDATA\Microsoft\WindowsApps\winget.exe"
    if (Test-Path $userAlias) {
        return $userAlias
    }

    $winApps = Get-ChildItem -Path "C:\Program Files\WindowsApps" -Filter "*DesktopAppInstaller*" -ErrorAction SilentlyContinue |
               Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if ($winApps) {
        $candidate = Join-Path $winApps.FullName "winget.exe"
        if (Test-Path $candidate) {
            return $candidate
        }
    }

    return $null
}

$WingetCmd = Get-WingetPath
$BaseDir = $ScriptDir
if (Test-Path (Join-Path (Split-Path -Parent $ScriptDir) "data\apps.json")) {
    $BaseDir = Split-Path -Parent $ScriptDir
}
$LogFilePath = Join-Path $BaseDir "logs\install-log.txt"
if (-not (Test-Path (Join-Path $BaseDir "logs"))) {
    $LogFilePath = Join-Path $BaseDir "install-log.txt"
}
$ConfigPath = Join-Path $BaseDir "data\apps.json"
if (-not (Test-Path $ConfigPath)) {
    $ConfigPath = Join-Path $BaseDir "apps.json"
}
$SettingsPath = Join-Path $BaseDir "config\settings.json"
if (-not (Test-Path $SettingsPath)) {
    $SettingsPath = Join-Path $BaseDir "settings.json"
}

# ------------------------------------------------------------------------------
# 2. Cloud URL Parser & Settings
# ------------------------------------------------------------------------------
function Convert-ToDirectDownloadUrl([string]$rawUrl) {
    if ([string]::IsNullOrWhiteSpace($rawUrl)) { return "" }
    $url = $rawUrl.Trim()

    if ($url -match "github\.com/([^/]+)/([^/]+)/blob/([^/]+)/(.+)") {
        return "https://raw.githubusercontent.com/$($Matches[1])/$($Matches[2])/$($Matches[3])/$($Matches[4])"
    }

    if ($url -match "drive\.google\.com/(?:file/d/|open\?id=)([a-zA-Z0-9_-]+)") {
        return "https://drive.google.com/uc?export=download&id=$($Matches[1])"
    }

    return $url
}

function Load-Settings {
    if (Test-Path $SettingsPath) {
        try {
            return Get-Content -Path $SettingsPath -Raw -Encoding UTF8 | ConvertFrom-Json
        } catch {}
    }
    return [PSCustomObject]@{
        cloudConfigUrl   = ""
        autoSyncOnLaunch = $true
        lastSyncTime     = "Never"
        lastSyncStatus   = "Local Cache"
        theme            = "system"
    }
}

function Save-Settings($settingsObj) {
    try {
        $json = $settingsObj | ConvertTo-Json -Depth 5
        [System.IO.File]::WriteAllText($SettingsPath, $json, [System.Text.Encoding]::UTF8)
    } catch {}
}

$Script:Settings = Load-Settings
if (-not $Script:Settings.theme) { $Script:Settings.theme = "system" }
$Script:CloudSyncSuccess = $false

function Sync-ConfigFromCloud([string]$targetUrl) {
    $directUrl = Convert-ToDirectDownloadUrl $targetUrl
    if ([string]::IsNullOrWhiteSpace($directUrl)) {
        return @{ Success = $false; Message = "No Cloud URL configured" }
    }

    try {
        $wc = New-Object System.Net.WebClient
        $wc.Headers.Add("User-Agent", "InnovanaPreInstaller/2.0")
        $downloadedJsonText = $wc.DownloadString($directUrl)
        $parsed = $downloadedJsonText | ConvertFrom-Json

        if ($parsed.groups -and $parsed.apps) {
            [System.IO.File]::WriteAllText($ConfigPath, $downloadedJsonText, [System.Text.Encoding]::UTF8)

            $Script:Settings.cloudConfigUrl = $targetUrl
            $Script:Settings.lastSyncTime = (Get-Date -Format "yyyy-MM-dd HH:mm:ss")
            $Script:Settings.lastSyncStatus = "Synced Successfully"
            Save-Settings $Script:Settings

            return @{ Success = $true; Data = $parsed; Message = "Synced from Cloud at $(Get-Date -Format 'HH:mm:ss')" }
        } else {
            return @{ Success = $false; Message = "Downloaded JSON missing 'groups' or 'apps' properties" }
        }
    } catch {
        return @{ Success = $false; Message = "Cloud sync failed: $($_.Exception.Message)" }
    }
}

if ($Script:Settings.autoSyncOnLaunch -and $Script:Settings.cloudConfigUrl -and ($Script:Settings.cloudConfigUrl -notlike "*your-username*")) {
    $syncResult = Sync-ConfigFromCloud $Script:Settings.cloudConfigUrl
    if ($syncResult.Success) {
        $ConfigJson = $syncResult.Data
        $Script:CloudSyncSuccess = $true
    }
}

if (-not $ConfigJson) {
    if (-not (Test-Path $ConfigPath)) {
        [System.Windows.MessageBox]::Show("Configuration file 'apps.json' not found in $ScriptDir!", "Error", [System.Windows.MessageBoxButton]::OK, [System.Windows.MessageBoxImage]::Error)
        exit 1
    }
    $ConfigJson = Get-Content -Path $ConfigPath -Raw -Encoding UTF8 | ConvertFrom-Json
}

# ------------------------------------------------------------------------------
# 3. Modern Innovana WPF Interface (XAML)
# ------------------------------------------------------------------------------
[xml]$xaml = @"
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Innovana Pre Installer — Machine Setup &amp; Software Deployment"
        Height="850" Width="1180"
        MinHeight="720" MinWidth="980"
        WindowStartupLocation="CenterScreen"
        Background="#181818"
        Foreground="#F3F3F3"
        FontFamily="Segoe UI Variable Display, Segoe UI">

    <Window.Resources>
        <Style TargetType="Button">
            <Setter Property="FocusVisualStyle" Value="{x:Null}"/>
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="Button">
                        <Border x:Name="border" Background="{TemplateBinding Background}"
                                BorderBrush="{TemplateBinding BorderBrush}"
                                BorderThickness="{TemplateBinding BorderThickness}"
                                CornerRadius="6" Padding="{TemplateBinding Padding}" SnapsToDevicePixels="True">
                            <ContentPresenter HorizontalAlignment="{TemplateBinding HorizontalContentAlignment}"
                                              VerticalAlignment="{TemplateBinding VerticalContentAlignment}"
                                              RecognizesAccessKey="True"/>
                        </Border>
                        <ControlTemplate.Triggers>
                            <Trigger Property="IsMouseOver" Value="True">
                                <Setter Property="Opacity" Value="0.85"/>
                            </Trigger>
                            <Trigger Property="IsPressed" Value="True">
                                <Setter Property="Opacity" Value="0.70"/>
                            </Trigger>
                            <Trigger Property="IsEnabled" Value="False">
                                <Setter Property="Opacity" Value="0.45"/>
                            </Trigger>
                        </ControlTemplate.Triggers>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>
    </Window.Resources>

    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="260"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>

        <!-- SIDEBAR -->
        <Border x:Name="BorderSidebar" Grid.Column="0" Background="#111317" BorderBrush="#20242B" BorderThickness="0,0,1,0">
            <Grid Margin="14,18,14,16">
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="*"/>
                    <RowDefinition Height="Auto"/>
                </Grid.RowDefinitions>

                <!-- Brand Header -->
                <StackPanel Grid.Row="0" Margin="6,0,6,22">
                    <StackPanel Orientation="Horizontal" VerticalAlignment="Center">
                        <Border Width="36" Height="36" Background="#0078D4" CornerRadius="9" Margin="0,0,12,0">
                            <TextBlock Text="⚡" FontSize="18" Foreground="#FFFFFF" HorizontalAlignment="Center" VerticalAlignment="Center"/>
                        </Border>
                        <StackPanel VerticalAlignment="Center">
                            <TextBlock x:Name="TxtBrandTitle" Text="INNOVANA" FontSize="15" FontWeight="Bold" Foreground="#FFFFFF" Margin="0,0,0,1"/>
                            <TextBlock x:Name="TxtBrandSubtitle" Text="Software Pre-Installer" FontSize="11" Foreground="#8F96A0"/>
                        </StackPanel>
                    </StackPanel>
                </StackPanel>

                <!-- Navigation Links -->
                <StackPanel Grid.Row="1">
                    <!-- Nav 1: Machine Configuration -->
                    <Button x:Name="NavBtnSetup" Height="44" Margin="0,0,0,5" Background="#22262E" BorderThickness="1" BorderBrush="#2E3542" Cursor="Hand" HorizontalContentAlignment="Left">
                        <Grid Margin="4,0,8,0">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="4"/>
                                <ColumnDefinition Width="34"/>
                                <ColumnDefinition Width="*"/>
                            </Grid.ColumnDefinitions>
                            <Border x:Name="NavIndSetup" Grid.Column="0" Width="3" Height="18" CornerRadius="1.5" Background="#0078D4" HorizontalAlignment="Left" VerticalAlignment="Center"/>
                            <TextBlock x:Name="NavIconSetup" Grid.Column="1" Text="&#xE770;" FontFamily="Segoe Fluent Icons, Segoe MDL2 Assets, Segoe UI Symbol" FontSize="16" Foreground="#4CC2FF" HorizontalAlignment="Center" VerticalAlignment="Center"/>
                            <TextBlock x:Name="NavTxtSetup" Grid.Column="2" Text="Machine Configuration" FontSize="13" FontWeight="SemiBold" Foreground="#FFFFFF" VerticalAlignment="Center" Margin="6,0,0,0"/>
                        </Grid>
                    </Button>

                    <!-- Nav 2: Applications Catalog -->
                    <Button x:Name="NavBtnCatalog" Height="44" Margin="0,0,0,5" Background="Transparent" BorderThickness="1" BorderBrush="Transparent" Cursor="Hand" HorizontalContentAlignment="Left">
                        <Grid Margin="4,0,8,0">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="4"/>
                                <ColumnDefinition Width="34"/>
                                <ColumnDefinition Width="*"/>
                            </Grid.ColumnDefinitions>
                            <Border x:Name="NavIndCatalog" Grid.Column="0" Width="3" Height="18" CornerRadius="1.5" Background="#0078D4" HorizontalAlignment="Left" VerticalAlignment="Center" Visibility="Collapsed"/>
                            <TextBlock x:Name="NavIconCatalog" Grid.Column="1" Text="&#xE71D;" FontFamily="Segoe Fluent Icons, Segoe MDL2 Assets, Segoe UI Symbol" FontSize="16" Foreground="#C5CCD8" HorizontalAlignment="Center" VerticalAlignment="Center"/>
                            <TextBlock x:Name="NavTxtCatalog" Grid.Column="2" Text="Applications Catalog" FontSize="13" Foreground="#D0D6E0" VerticalAlignment="Center" Margin="6,0,0,0"/>
                        </Grid>
                    </Button>

                    <!-- Nav 3: Cloud Sync -->
                    <Button x:Name="NavBtnCloud" Height="44" Margin="0,0,0,5" Background="Transparent" BorderThickness="1" BorderBrush="Transparent" Cursor="Hand" HorizontalContentAlignment="Left">
                        <Grid Margin="4,0,8,0">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="4"/>
                                <ColumnDefinition Width="34"/>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="Auto"/>
                            </Grid.ColumnDefinitions>
                            <Border x:Name="NavIndCloud" Grid.Column="0" Width="3" Height="18" CornerRadius="1.5" Background="#0078D4" HorizontalAlignment="Left" VerticalAlignment="Center" Visibility="Collapsed"/>
                            <TextBlock x:Name="NavIconCloud" Grid.Column="1" Text="&#xE753;" FontFamily="Segoe Fluent Icons, Segoe MDL2 Assets, Segoe UI Symbol" FontSize="16" Foreground="#C5CCD8" HorizontalAlignment="Center" VerticalAlignment="Center"/>
                            <TextBlock x:Name="NavTxtCloud" Grid.Column="2" Text="Cloud Sync (GitHub)" FontSize="13" Foreground="#D0D6E0" VerticalAlignment="Center" Margin="6,0,0,0"/>
                            <Border Grid.Column="3" Background="#133824" CornerRadius="4" Padding="6,2" VerticalAlignment="Center">
                                <TextBlock x:Name="TxtNavCloudStatus" Text="SYNC" FontSize="9" FontWeight="Bold" Foreground="#4ADE80"/>
                            </Border>
                        </Grid>
                    </Button>

                    <!-- Nav 4: Activity & Logs -->
                    <Button x:Name="NavBtnLogs" Height="44" Margin="0,0,0,5" Background="Transparent" BorderThickness="1" BorderBrush="Transparent" Cursor="Hand" HorizontalContentAlignment="Left">
                        <Grid Margin="4,0,8,0">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="4"/>
                                <ColumnDefinition Width="34"/>
                                <ColumnDefinition Width="*"/>
                            </Grid.ColumnDefinitions>
                            <Border x:Name="NavIndLogs" Grid.Column="0" Width="3" Height="18" CornerRadius="1.5" Background="#0078D4" HorizontalAlignment="Left" VerticalAlignment="Center" Visibility="Collapsed"/>
                            <TextBlock x:Name="NavIconLogs" Grid.Column="1" Text="&#xE756;" FontFamily="Segoe Fluent Icons, Segoe MDL2 Assets, Segoe UI Symbol" FontSize="16" Foreground="#C5CCD8" HorizontalAlignment="Center" VerticalAlignment="Center"/>
                            <TextBlock x:Name="NavTxtLogs" Grid.Column="2" Text="Activity &amp; Logs" FontSize="13" Foreground="#D0D6E0" VerticalAlignment="Center" Margin="6,0,0,0"/>
                        </Grid>
                    </Button>
                </StackPanel>

                <!-- System Environment Bottom Widget -->
                <Border x:Name="BorderSysEnv" Grid.Row="2" Background="#16191F" CornerRadius="8" Padding="14,12" BorderBrush="#222731" BorderThickness="1">
                    <StackPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,0,0,6">
                            <TextBlock Text="&#xE7F8;" FontFamily="Segoe Fluent Icons, Segoe MDL2 Assets, Segoe UI Symbol" FontSize="14" Foreground="#4CC2FF" Margin="0,0,8,0" VerticalAlignment="Center"/>
                            <TextBlock x:Name="TxtSysEnvHeader" Text="System Environment" FontSize="11" FontWeight="Bold" Foreground="#8E95A5" VerticalAlignment="Center"/>
                        </StackPanel>
                        <TextBlock x:Name="TxtSysEnvTitle" Text="Innovana Pre Installer (PowerShell)" FontSize="11" Foreground="#FFFFFF" FontWeight="SemiBold"/>
                        <StackPanel Orientation="Horizontal" Margin="0,4,0,0">
                            <TextBlock Text="● " FontSize="8" Foreground="#4ADE80" VerticalAlignment="Center"/>
                            <TextBlock x:Name="TxtWingetVersion" Text="Winget Engine: Ready" FontSize="10" Foreground="#4ADE80" VerticalAlignment="Center"/>
                        </StackPanel>
                    </StackPanel>
                </Border>
            </Grid>
        </Border>

        <!-- MAIN CONTENT -->
        <Grid Grid.Column="1">
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="*"/>
                <RowDefinition Height="Auto"/>
            </Grid.RowDefinitions>

            <!-- TOP BAR -->
            <Border x:Name="BorderTopBar" Grid.Row="0" Background="#181818" Padding="28,14" BorderBrush="#262626" BorderThickness="0,0,0,1">
                <Grid>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="Auto"/>
                    </Grid.ColumnDefinitions>

                    <StackPanel Grid.Column="0" VerticalAlignment="Center">
                        <TextBlock x:Name="TxtPageTitle" Text="Machine configuration" FontSize="20" FontWeight="Bold" Foreground="#FFFFFF"/>
                        <TextBlock x:Name="TxtPageSubtitle" Text="Set up your developer machine quickly with curated role environments."
                                   FontSize="12" Foreground="#8A8A8A" Margin="0,2,0,0"/>
                    </StackPanel>

                    <StackPanel Grid.Column="1" Orientation="Horizontal" VerticalAlignment="Center">
                        <!-- Theme Toggle Button -->
                        <Button x:Name="BtnThemeToggle" Background="#20242B" BorderBrush="#323A48" BorderThickness="1"
                                Padding="12,6" Margin="0,0,10,0" Cursor="Hand" ToolTip="Click to switch theme: Auto (Windows) / Light / Dark">
                            <StackPanel Orientation="Horizontal" VerticalAlignment="Center">
                                <TextBlock x:Name="TxtThemeIcon" Text="💻" FontSize="13" Margin="0,0,6,0" VerticalAlignment="Center"/>
                                <TextBlock x:Name="TxtThemeLabel" Text="Auto" FontSize="11" FontWeight="SemiBold" Foreground="#FFFFFF" VerticalAlignment="Center"/>
                            </StackPanel>
                        </Button>

                        <!-- Top Sync Status Pill -->
                        <Border x:Name="BorderTopSync" Background="#151C24" BorderBrush="#223242" BorderThickness="1" CornerRadius="8" Padding="12,6" VerticalAlignment="Center">
                            <StackPanel Orientation="Horizontal">
                                <TextBlock x:Name="TxtTopSyncIcon" Text="&#xE753;" FontFamily="Segoe Fluent Icons, Segoe MDL2 Assets, Segoe UI Symbol" FontSize="13" Foreground="#4ADE80" Margin="0,0,8,0" VerticalAlignment="Center"/>
                                <TextBlock x:Name="TxtTopSyncStatus" Text="Local Cache" FontSize="11" FontWeight="Bold" Foreground="#4ADE80" VerticalAlignment="Center"/>
                            </StackPanel>
                        </Border>
                    </StackPanel>
                </Grid>
            </Border>

            <!-- PAGE 1: SETUP -->
            <ScrollViewer x:Name="ViewSetup" Grid.Row="1" VerticalScrollBarVisibility="Auto" HorizontalScrollBarVisibility="Disabled" Padding="28,20">
                <StackPanel>
                    <!-- Innovana Hero Banner -->
                    <Border x:Name="BorderHero" Background="#1C212B" CornerRadius="10" Padding="20,16" BorderBrush="#2B3342" BorderThickness="1" Margin="0,0,0,20">
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="Auto"/>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="Auto"/>
                            </Grid.ColumnDefinitions>

                            <Border Grid.Column="0" Width="44" Height="44" Background="#0078D4" CornerRadius="10" Margin="0,0,16,0" VerticalAlignment="Center">
                                <TextBlock Text="&#xE756;" FontFamily="Segoe Fluent Icons, Segoe MDL2 Assets, Segoe UI Symbol" FontSize="20" Foreground="#FFFFFF" HorizontalAlignment="Center" VerticalAlignment="Center"/>
                            </Border>

                            <StackPanel Grid.Column="1" VerticalAlignment="Center">
                                <TextBlock x:Name="TxtHeroTitle" Text="Innovana End-to-End Machine Setup" FontSize="14" FontWeight="Bold" Foreground="#FFFFFF"/>
                                <TextBlock x:Name="TxtHeroDesc" Text="Pre-configure this PC with standard developer toolchains, design software, or enterprise office bundles. Packages are verified and silently deployed using the Windows Package Manager (winget)."
                                           FontSize="11" Foreground="#9AA5B8" TextWrapping="Wrap" Margin="0,3,0,0" LineHeight="16"/>
                            </StackPanel>

                            <Button x:Name="BtnHeroCatalog" Grid.Column="2" Content="Browse All Catalog →"
                                    Background="#242B38" Foreground="#4CC2FF" FontWeight="SemiBold" FontSize="11"
                                    Padding="14,8" Margin="14,0,0,0" Cursor="Hand" BorderThickness="1" BorderBrush="#353F52" VerticalAlignment="Center"/>
                        </Grid>
                    </Border>

                    <Grid Margin="0,0,0,16">
                        <StackPanel VerticalAlignment="Center">
                            <TextBlock x:Name="TxtTargetRoleTitle" Text="Target Role Environments" FontSize="16" FontWeight="Bold" Foreground="#FFFFFF"/>
                            <TextBlock x:Name="TxtTargetRoleSubtitle" Text="Select a role to fast-track setup, or create and customize role software bundles."
                                       FontSize="12" Foreground="#8A8A8A" Margin="0,2,0,0"/>
                        </StackPanel>

                        <Button x:Name="BtnCreateNewGroup" Content="➕ Create New Group"
                                Background="#10B981" Foreground="#FFFFFF" FontWeight="Bold" FontSize="12"
                                Padding="16,8" Cursor="Hand" BorderThickness="0" HorizontalAlignment="Right"/>
                    </Grid>

                    <UniformGrid x:Name="RoleCardsGrid" Columns="2" Margin="0,0,0,10"/>
                </StackPanel>
            </ScrollViewer>

            <!-- PAGE 2: CATALOG -->
            <Grid x:Name="ViewCatalog" Grid.Row="1" Visibility="Collapsed" Margin="28,16,28,16">
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="*"/>
                </Grid.RowDefinitions>

                <Grid Grid.Row="0" Margin="0,0,0,14">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="260"/>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="Auto"/>
                    </Grid.ColumnDefinitions>

                    <TextBox x:Name="TxtSearch" Grid.Column="0" Height="36" Background="#161920" Foreground="#FFFFFF"
                             CaretBrush="#4CC2FF" BorderBrush="#282F3B" BorderThickness="1" Padding="12,8"
                             FontSize="12" VerticalContentAlignment="Center" Text="Search applications..."/>

                    <ScrollViewer Grid.Column="1" HorizontalScrollBarVisibility="Auto" VerticalScrollBarVisibility="Disabled" Margin="12,0,12,0">
                        <StackPanel x:Name="CategoryChipsPanel" Orientation="Horizontal"/>
                    </ScrollViewer>

                    <StackPanel Grid.Column="2" Orientation="Horizontal" VerticalAlignment="Center">
                        <Button x:Name="BtnCatalogSelectAll" Content="Select All" Background="#222731" Foreground="#C8D0DC"
                                Padding="12,6" Margin="0,0,6,0" Cursor="Hand" BorderThickness="1" BorderBrush="#323A48" FontSize="11"/>
                        <Button x:Name="BtnCatalogClearAll" Content="Clear" Background="#222731" Foreground="#C8D0DC"
                                Padding="12,6" Cursor="Hand" BorderThickness="1" BorderBrush="#323A48" FontSize="11"/>
                    </StackPanel>
                </Grid>

                <Border x:Name="BorderCatalogContainer" Grid.Row="1" Background="#161920" CornerRadius="10" Padding="12" BorderBrush="#252A34" BorderThickness="1">
                    <ScrollViewer VerticalScrollBarVisibility="Auto" HorizontalScrollBarVisibility="Disabled">
                        <StackPanel x:Name="AppsContainer"/>
                    </ScrollViewer>
                </Border>
            </Grid>

            <!-- PAGE 3: CLOUD SYNC -->
            <ScrollViewer x:Name="ViewCloud" Grid.Row="1" Visibility="Collapsed" VerticalScrollBarVisibility="Auto" Padding="28,20">
                <StackPanel>
                    <TextBlock x:Name="TxtCloudTitle1" Text="Host &amp; Sync Software List from the Cloud" FontSize="16" FontWeight="Bold" Foreground="#FFFFFF" Margin="0,0,0,4"/>
                    <TextBlock x:Name="TxtCloudDesc1" Text="Keep your team's software list on GitHub or Google Drive. When you update it online, all team members instantly receive the latest software."
                               FontSize="12" Foreground="#8A8A8A" Margin="0,0,0,20"/>

                    <Border x:Name="BorderCloudCard1" Background="#202020" CornerRadius="10" Padding="20" BorderBrush="#333333" BorderThickness="1" Margin="0,0,0,20">
                        <StackPanel>
                            <TextBlock x:Name="TxtCloudCard1Title" Text="Cloud Configuration URL (GitHub Raw URL or Google Drive Link)" FontSize="13" FontWeight="Bold" Foreground="#FFFFFF" Margin="0,0,0,6"/>
                            <TextBox x:Name="TxtCloudUrlInput" Height="38" Background="#141414" Foreground="#4CC2FF"
                                     CaretBrush="#4CC2FF" BorderBrush="#3D3D3D" BorderThickness="1" Padding="12,8"
                                     FontSize="12" VerticalContentAlignment="Center"/>

                            <Grid Margin="0,14,0,0">
                                <CheckBox x:Name="ChkAutoSync" Content="Automatically sync from Cloud on startup" IsChecked="True"
                                          Foreground="#C8C8C8" FontSize="12" VerticalAlignment="Center" Cursor="Hand"/>

                                <StackPanel Orientation="Horizontal" HorizontalAlignment="Right">
                                    <Button x:Name="BtnSaveCloudUrl" Content="💾 Save URL" Background="#2D2D2D" Foreground="#F3F3F3"
                                            Padding="14,8" Margin="0,0,8,0" Cursor="Hand" BorderThickness="1" BorderBrush="#3D3D3D" FontSize="12"/>
                                    <Button x:Name="BtnSyncNow" Content="🔄 Sync Now" Background="#0078D4" Foreground="#FFFFFF"
                                            FontWeight="Bold" Padding="18,8" Cursor="Hand" BorderThickness="0" FontSize="12"/>
                                </StackPanel>
                            </Grid>

                            <Border x:Name="SyncResultBorder" Background="#1A2E20" CornerRadius="6" Padding="12,8" Margin="0,12,0,0" Visibility="Collapsed">
                                <TextBlock x:Name="TxtSyncResult" Text="Synced successfully!" FontSize="12" Foreground="#4ADE80"/>
                            </Border>
                        </StackPanel>
                    </Border>

                    <TextBlock x:Name="TxtCloudCard2Title" Text="How to configure GitHub or Google Drive for your team" FontSize="14" FontWeight="SemiBold" Foreground="#FFFFFF" Margin="0,0,0,10"/>

                    <Border x:Name="BorderCloudCard2" Background="#202020" CornerRadius="10" Padding="18" BorderBrush="#2D2D2D" BorderThickness="1" Margin="0,0,0,12">
                        <StackPanel>
                            <StackPanel Orientation="Horizontal" Margin="0,0,0,6">
                                <TextBlock Text="🐙" FontSize="16" Margin="0,0,8,0" VerticalAlignment="Center"/>
                                <TextBlock Text="Option A: Host on GitHub (Recommended)" FontSize="13" FontWeight="Bold" Foreground="#FFFFFF" VerticalAlignment="Center"/>
                            </StackPanel>
                            <TextBlock Text="1. Create a GitHub repository (e.g. 'company-software-installer')." FontSize="12" Foreground="#8A8A8A" Margin="0,2,0,2"/>
                            <TextBlock Text="2. Upload apps.json to your repository." FontSize="12" Foreground="#8A8A8A" Margin="0,2,0,2"/>
                            <TextBlock Text="3. Click apps.json, click 'Raw', and copy the URL from your browser." FontSize="12" Foreground="#8A8A8A" Margin="0,2,0,2"/>
                            <TextBlock Text="4. Paste that Raw URL above and click 'Sync Now'." FontSize="12" Foreground="#8A8A8A" Margin="0,2,0,6"/>
                            <TextBlock Text="Example: https://raw.githubusercontent.com/user/repo/main/apps.json" FontSize="11" Foreground="#0078D4" FontFamily="Consolas"/>
                        </StackPanel>
                    </Border>

                    <Border x:Name="BorderCloudCard3" Background="#202020" CornerRadius="10" Padding="18" BorderBrush="#2D2D2D" BorderThickness="1">
                        <StackPanel>
                            <StackPanel Orientation="Horizontal" Margin="0,0,0,6">
                                <TextBlock Text="📁" FontSize="16" Margin="0,0,8,0" VerticalAlignment="Center"/>
                                <TextBlock x:Name="TxtCloudCard3Title" Text="Option B: Host on Google Drive" FontSize="13" FontWeight="Bold" Foreground="#FFFFFF" VerticalAlignment="Center"/>
                            </StackPanel>
                            <TextBlock Text="1. Upload apps.json to Google Drive." FontSize="12" Foreground="#8A8A8A" Margin="0,2,0,2"/>
                            <TextBlock Text="2. Right-click apps.json -> Share -> Change to 'Anyone with the link can view'." FontSize="12" Foreground="#8A8A8A" Margin="0,2,0,2"/>
                            <TextBlock Text="3. Copy the link, paste it into the box above, and click 'Sync Now'." FontSize="12" Foreground="#8A8A8A" Margin="0,2,0,6"/>
                            <TextBlock Text="Our app automatically converts standard Google Drive links into direct raw downloads!" FontSize="11" Foreground="#8A8A8A" FontStyle="Italic"/>
                        </StackPanel>
                    </Border>
                </StackPanel>
            </ScrollViewer>

            <!-- PAGE 4: LOGS -->
            <Grid x:Name="ViewLogs" Grid.Row="1" Visibility="Collapsed" Margin="28,16,28,16">
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="*"/>
                </Grid.RowDefinitions>

                <Grid Grid.Row="0" Margin="0,0,0,10">
                    <TextBlock x:Name="TxtLogsHeader" Text="Live Installation Terminal &amp; Audit Trail" FontSize="14" FontWeight="SemiBold" Foreground="#FFFFFF" VerticalAlignment="Center"/>
                    <StackPanel Orientation="Horizontal" HorizontalAlignment="Right">
                        <Button x:Name="BtnOpenLogFile" Content="📂 Open Log File" Background="#2D2D2D" Foreground="#F3F3F3"
                                Padding="12,5" Margin="0,0,6,0" Cursor="Hand" BorderThickness="1" BorderBrush="#3D3D3D" FontSize="11"/>
                        <Button x:Name="BtnClearLogConsole" Content="Clear Console" Background="#2D2D2D" Foreground="#F3F3F3"
                                Padding="12,5" Cursor="Hand" BorderThickness="1" BorderBrush="#3D3D3D" FontSize="11"/>
                    </StackPanel>
                </Grid>

                <Border x:Name="BorderLogTerminal" Grid.Row="1" Background="#0C0E14" CornerRadius="8" Padding="14" BorderBrush="#2D2D2D" BorderThickness="1">
                    <ScrollViewer x:Name="LogScrollViewer" VerticalScrollBarVisibility="Auto">
                        <TextBox x:Name="TxtLog" Background="Transparent" Foreground="#6EE7B7" BorderThickness="0"
                                 FontFamily="Consolas, Lucida Console" FontSize="11" IsReadOnly="True" TextWrapping="Wrap"/>
                    </ScrollViewer>
                </Border>
            </Grid>

            <!-- BOTTOM STICKY BAR -->
            <Border x:Name="BorderBottomBar" Grid.Row="2" Background="#141414" Padding="28,14" BorderBrush="#262626" BorderThickness="0,1,0,0">
                <Grid>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="Auto"/>
                    </Grid.ColumnDefinitions>

                    <StackPanel Grid.Column="0" VerticalAlignment="Center" Margin="0,0,24,0">
                        <Grid Margin="0,0,0,6">
                            <TextBlock x:Name="TxtStatus" Text="Ready to configure and install." FontSize="12" Foreground="#8A8A8A"/>
                            <TextBlock x:Name="TxtCount" Text="Selected: 0 apps" HorizontalAlignment="Right" FontSize="12" FontWeight="Bold" Foreground="#4CC2FF"/>
                        </Grid>
                        <ProgressBar x:Name="ProgressBar" Height="6" Minimum="0" Maximum="100" Value="0"
                                     Background="#262626" Foreground="#0078D4" BorderThickness="0"/>
                    </StackPanel>

                    <StackPanel Grid.Column="1" Orientation="Horizontal" VerticalAlignment="Center">
                        <Button x:Name="BtnOpenOfflineFolder" Content="📂 Offline Folder" Background="#222222" Foreground="#C8C8C8"
                                Padding="14,10" Margin="0,0,10,0" Cursor="Hand" BorderThickness="1" BorderBrush="#333333" FontSize="12"/>
                        <Button x:Name="BtnStop" Content="⏹️ Stop Setup"
                                Background="#DC2626" Foreground="#FFFFFF" FontWeight="Bold" FontSize="13"
                                Padding="22,10" Margin="0,0,10,0" Cursor="Hand" BorderThickness="0" Visibility="Collapsed"/>
                        <Button x:Name="BtnInstall" Content="⚡ Set up this machine"
                                Background="#0078D4" Foreground="#FFFFFF" FontWeight="Bold" FontSize="13"
                                Padding="28,10" Cursor="Hand" BorderThickness="0"/>
                    </StackPanel>
                </Grid>
            </Border>
        </Grid>

        <!-- ================= GROUP MODAL OVERLAY ================= -->
        <Grid x:Name="GroupModalOverlay" Grid.ColumnSpan="2" Background="#B0000000" Visibility="Collapsed">
            <Border x:Name="BorderModalCard" Width="680" MaxHeight="760" Background="#1F1F1F" CornerRadius="12" BorderBrush="#3A3A3A" BorderThickness="1"
                    HorizontalAlignment="Center" VerticalAlignment="Center" Padding="24">
                <Grid>
                    <Grid.RowDefinitions>
                        <RowDefinition Height="Auto"/>
                        <RowDefinition Height="Auto"/>
                        <RowDefinition Height="Auto"/>
                        <RowDefinition Height="Auto"/>
                        <RowDefinition Height="Auto"/>
                        <RowDefinition Height="*" MaxHeight="320"/>
                        <RowDefinition Height="Auto"/>
                    </Grid.RowDefinitions>

                    <Grid Grid.Row="0" Margin="0,0,0,16">
                        <TextBlock x:Name="TxtModalTitle" Text="Create New Role Group" FontSize="18" FontWeight="Bold" Foreground="#FFFFFF" VerticalAlignment="Center"/>
                        <Button x:Name="BtnCloseModal" Content="✕" Background="Transparent" Foreground="#8A8A8A" FontSize="14" FontWeight="Bold"
                                Width="30" Height="30" Cursor="Hand" BorderThickness="0" HorizontalAlignment="Right" VerticalAlignment="Center"/>
                    </Grid>

                    <Grid Grid.Row="1" Margin="0,0,0,10">
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="70"/>
                            <ColumnDefinition Width="*"/>
                        </Grid.ColumnDefinitions>

                        <StackPanel Grid.Column="0" Margin="0,0,10,0">
                            <TextBlock Text="Icon" FontSize="11" FontWeight="SemiBold" Foreground="#8A8A8A" Margin="0,0,0,4"/>
                            <TextBox x:Name="TxtGroupIconInput" Text="🚀" Height="34" Background="#141414" Foreground="#FFFFFF"
                                     FontSize="16" TextAlignment="Center" VerticalContentAlignment="Center" BorderBrush="#333333" BorderThickness="1"/>
                        </StackPanel>

                        <StackPanel Grid.Column="1">
                            <TextBlock Text="Role Group Name" FontSize="11" FontWeight="SemiBold" Foreground="#8A8A8A" Margin="0,0,0,4"/>
                            <TextBox x:Name="TxtGroupNameInput" Height="34" Background="#141414" Foreground="#FFFFFF"
                                     FontSize="13" Padding="8,6" VerticalContentAlignment="Center" BorderBrush="#333333" BorderThickness="1"/>
                        </StackPanel>
                    </Grid>

                    <StackPanel Grid.Row="2" Margin="0,0,0,12">
                        <TextBlock Text="Quick Icon Select:" FontSize="10" Foreground="#707070" Margin="0,0,0,4"/>
                        <WrapPanel x:Name="EmojiChipsPanel"/>
                    </StackPanel>

                    <StackPanel Grid.Row="3" Margin="0,0,0,14">
                        <TextBlock Text="Description" FontSize="11" FontWeight="SemiBold" Foreground="#8A8A8A" Margin="0,0,0,4"/>
                        <TextBox x:Name="TxtGroupDescInput" Height="34" Background="#141414" Foreground="#FFFFFF"
                                 FontSize="12" Padding="8,6" VerticalContentAlignment="Center" BorderBrush="#333333" BorderThickness="1"/>
                    </StackPanel>

                    <Grid Grid.Row="4" Margin="0,0,0,8">
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="240"/>
                            <ColumnDefinition Width="*"/>
                        </Grid.ColumnDefinitions>

                        <TextBox x:Name="TxtModalAppSearch" Grid.Column="0" Height="30" Background="#141414" Foreground="#FFFFFF"
                                 FontSize="11" Padding="8,5" VerticalContentAlignment="Center" BorderBrush="#333333" BorderThickness="1"
                                 Text="Filter software..."/>

                        <StackPanel Grid.Column="1" Orientation="Horizontal" HorizontalAlignment="Right" VerticalAlignment="Center">
                            <TextBlock x:Name="TxtModalSelectedCount" Text="0 selected" FontSize="11" Foreground="#4CC2FF" FontWeight="SemiBold" VerticalAlignment="Center" Margin="0,0,12,0"/>
                            <Button x:Name="BtnModalSelectAll" Content="Select All" Background="#2B2B2B" Foreground="#CCCCCC"
                                    Padding="8,4" Margin="0,0,6,0" Cursor="Hand" BorderThickness="1" BorderBrush="#3D3D3D" FontSize="11"/>
                            <Button x:Name="BtnModalClearAll" Content="Clear" Background="#2B2B2B" Foreground="#CCCCCC"
                                    Padding="8,4" Cursor="Hand" BorderThickness="1" BorderBrush="#3D3D3D" FontSize="11"/>
                        </StackPanel>
                    </Grid>

                    <Border x:Name="BorderModalAppsContainer" Grid.Row="5" Background="#141414" CornerRadius="8" Padding="10" BorderBrush="#2D2D2D" BorderThickness="1" Margin="0,0,0,16">
                        <ScrollViewer VerticalScrollBarVisibility="Auto" HorizontalScrollBarVisibility="Disabled">
                            <StackPanel x:Name="ModalAppsContainer"/>
                        </ScrollViewer>
                    </Border>

                    <Grid Grid.Row="6">
                        <Button x:Name="BtnDeleteGroup" Content="🗑️ Delete Group" Background="#3D1A1A" Foreground="#F87171"
                                FontWeight="SemiBold" Padding="14,8" Cursor="Hand" BorderThickness="1" BorderBrush="#7F1D1D"
                                FontSize="12" HorizontalAlignment="Left" Visibility="Collapsed"/>

                        <StackPanel Orientation="Horizontal" HorizontalAlignment="Right">
                            <Button x:Name="BtnCancelModal" Content="Cancel" Background="#2B2B2B" Foreground="#F3F3F3"
                                    Padding="18,8" Margin="0,0,10,0" Cursor="Hand" BorderThickness="1" BorderBrush="#3D3D3D" FontSize="12"/>
                            <Button x:Name="BtnSaveGroup" Content="💾 Save Group" Background="#0078D4" Foreground="#FFFFFF"
                                    FontWeight="Bold" Padding="22,8" Cursor="Hand" BorderThickness="0" FontSize="12"/>
                        </StackPanel>
                    </Grid>
                </Grid>
            </Border>
        </Grid>
    </Grid>
</Window>
"@

$reader = New-Object System.Xml.XmlNodeReader $xaml
$window = [System.Windows.Markup.XamlReader]::Load($reader)

# Set icon if exists
$iconPath = Join-Path $ScriptDir "src\app.ico"
if (-not (Test-Path $iconPath)) { $iconPath = Join-Path $ScriptDir "app.ico" }
if (Test-Path $iconPath) {
    try {
        $window.Icon = New-Object System.Windows.Media.Imaging.BitmapImage (New-Object System.Uri $iconPath)
    } catch {}
}

# Map Elements
$NavBtnSetup          = $window.FindName("NavBtnSetup")
$NavBtnCatalog        = $window.FindName("NavBtnCatalog")
$NavBtnCloud          = $window.FindName("NavBtnCloud")
$NavBtnLogs           = $window.FindName("NavBtnLogs")

$NavIndSetup          = $window.FindName("NavIndSetup")
$NavIndCatalog        = $window.FindName("NavIndCatalog")
$NavIndCloud          = $window.FindName("NavIndCloud")
$NavIndLogs           = $window.FindName("NavIndLogs")

$NavIconSetup         = $window.FindName("NavIconSetup")
$NavIconCatalog       = $window.FindName("NavIconCatalog")
$NavIconCloud         = $window.FindName("NavIconCloud")
$NavIconLogs          = $window.FindName("NavIconLogs")

$NavTxtSetup          = $window.FindName("NavTxtSetup")
$NavTxtCatalog        = $window.FindName("NavTxtCatalog")
$NavTxtCloud          = $window.FindName("NavTxtCloud")
$NavTxtLogs           = $window.FindName("NavTxtLogs")

$ViewSetup            = $window.FindName("ViewSetup")
$ViewCatalog          = $window.FindName("ViewCatalog")
$ViewCloud            = $window.FindName("ViewCloud")
$ViewLogs             = $window.FindName("ViewLogs")

$TxtPageTitle         = $window.FindName("TxtPageTitle")
$TxtPageSubtitle      = $window.FindName("TxtPageSubtitle")
$TxtTopSyncStatus     = $window.FindName("TxtTopSyncStatus")
$TxtNavCloudStatus    = $window.FindName("TxtNavCloudStatus")
$TxtWingetVersion     = $window.FindName("TxtWingetVersion")

# Theme & Container references
$BorderSidebar        = $window.FindName("BorderSidebar")
$TxtBrandTitle        = $window.FindName("TxtBrandTitle")
$TxtBrandSubtitle     = $window.FindName("TxtBrandSubtitle")
$BorderSysEnv         = $window.FindName("BorderSysEnv")
$TxtSysEnvHeader      = $window.FindName("TxtSysEnvHeader")
$TxtSysEnvTitle       = $window.FindName("TxtSysEnvTitle")
$BorderTopBar         = $window.FindName("BorderTopBar")
$BtnThemeToggle       = $window.FindName("BtnThemeToggle")
$TxtThemeIcon         = $window.FindName("TxtThemeIcon")
$TxtThemeLabel        = $window.FindName("TxtThemeLabel")
$BorderTopSync        = $window.FindName("BorderTopSync")
$TxtTopSyncIcon       = $window.FindName("TxtTopSyncIcon")
$BorderHero           = $window.FindName("BorderHero")
$TxtHeroTitle         = $window.FindName("TxtHeroTitle")
$TxtHeroDesc          = $window.FindName("TxtHeroDesc")
$BtnHeroCatalog       = $window.FindName("BtnHeroCatalog")
$TxtTargetRoleTitle   = $window.FindName("TxtTargetRoleTitle")
$TxtTargetRoleSubtitle= $window.FindName("TxtTargetRoleSubtitle")
$BorderCatalogContainer=$window.FindName("BorderCatalogContainer")
$BtnCatalogSelectAll  = $window.FindName("BtnCatalogSelectAll")
$BtnCatalogClearAll   = $window.FindName("BtnCatalogClearAll")
$BorderCloudCard1     = $window.FindName("BorderCloudCard1")
$BorderCloudCard2     = $window.FindName("BorderCloudCard2")
$BorderCloudCard3     = $window.FindName("BorderCloudCard3")
$TxtCloudTitle1       = $window.FindName("TxtCloudTitle1")
$TxtCloudDesc1        = $window.FindName("TxtCloudDesc1")
$TxtCloudCard1Title   = $window.FindName("TxtCloudCard1Title")
$TxtCloudCard2Title   = $window.FindName("TxtCloudCard2Title")
$TxtCloudCard3Title   = $window.FindName("TxtCloudCard3Title")
$TxtLogsHeader        = $window.FindName("TxtLogsHeader")
$BorderLogTerminal    = $window.FindName("BorderLogTerminal")
$BorderBottomBar      = $window.FindName("BorderBottomBar")
$BorderModalCard      = $window.FindName("BorderModalCard")
$BorderModalAppsContainer=$window.FindName("BorderModalAppsContainer")

$BtnCreateNewGroup    = $window.FindName("BtnCreateNewGroup")
$RoleCardsGrid        = $window.FindName("RoleCardsGrid")

$TxtSearch            = $window.FindName("TxtSearch")
$CategoryChipsPanel   = $window.FindName("CategoryChipsPanel")
$AppsContainer        = $window.FindName("AppsContainer")

$TxtCloudUrlInput     = $window.FindName("TxtCloudUrlInput")
$ChkAutoSync          = $window.FindName("ChkAutoSync")
$BtnSaveCloudUrl      = $window.FindName("BtnSaveCloudUrl")
$BtnSyncNow           = $window.FindName("BtnSyncNow")
$SyncResultBorder     = $window.FindName("SyncResultBorder")
$TxtSyncResult        = $window.FindName("TxtSyncResult")

$LogScrollViewer      = $window.FindName("LogScrollViewer")
$TxtLog               = $window.FindName("TxtLog")
$BtnOpenLogFile       = $window.FindName("BtnOpenLogFile")
$BtnClearLogConsole   = $window.FindName("BtnClearLogConsole")

$TxtStatus            = $window.FindName("TxtStatus")
$TxtCount             = $window.FindName("TxtCount")
$ProgressBar          = $window.FindName("ProgressBar")
$BtnOpenOfflineFolder = $window.FindName("BtnOpenOfflineFolder")
$BtnInstall           = $window.FindName("BtnInstall")
$BtnStop              = $window.FindName("BtnStop")

# Modal Elements
$GroupModalOverlay    = $window.FindName("GroupModalOverlay")
$TxtModalTitle        = $window.FindName("TxtModalTitle")
$BtnCloseModal        = $window.FindName("BtnCloseModal")
$TxtGroupNameInput    = $window.FindName("TxtGroupNameInput")
$TxtGroupIconInput    = $window.FindName("TxtGroupIconInput")
$EmojiChipsPanel      = $window.FindName("EmojiChipsPanel")
$TxtGroupDescInput    = $window.FindName("TxtGroupDescInput")
$TxtModalAppSearch    = $window.FindName("TxtModalAppSearch")
$BtnModalSelectAll    = $window.FindName("BtnModalSelectAll")
$BtnModalClearAll     = $window.FindName("BtnModalClearAll")
$TxtModalSelectedCount= $window.FindName("TxtModalSelectedCount")
$ModalAppsContainer   = $window.FindName("ModalAppsContainer")
$BtnDeleteGroup       = $window.FindName("BtnDeleteGroup")
$BtnCancelModal       = $window.FindName("BtnCancelModal")
$BtnSaveGroup         = $window.FindName("BtnSaveGroup")

$Script:AppRows = [System.Collections.Generic.List[PSObject]]::new()
$Script:RoleCardViews = [System.Collections.Generic.List[PSObject]]::new()
$Script:ModalAppRows = [System.Collections.Generic.List[PSObject]]::new()
$Script:ActiveRoleId = "developer"
$Script:CurrentEditingGroupId = $null
$Script:CurrentView = "setup"
$Script:CurrentThemeIsLight = $false

$BrushConv = [System.Windows.Media.BrushConverter]::new()
function HexBrush([string]$hex) {
    return $BrushConv.ConvertFromString($hex)
}

# ------------------------------------------------------------------------------
# 4. Windows Theme Detection & Dynamic Theme Switcher
# ------------------------------------------------------------------------------
function Get-WindowsAppsUseLightTheme {
    try {
        $regVal = Get-ItemPropertyValue -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize" -Name "AppsUseLightTheme" -ErrorAction SilentlyContinue
        return ($regVal -eq 1)
    } catch {
        return $false
    }
}

function Get-CurrentThemeIsLight {
    $mode = ($Script:Settings.theme).ToLower()
    if ($mode -eq "light") { return $true }
    if ($mode -eq "dark") { return $false }
    return (Get-WindowsAppsUseLightTheme)
}

function Apply-Theme([bool]$isLight) {
    $Script:CurrentThemeIsLight = $isLight

    # Window Base
    $window.Background = if ($isLight) { HexBrush "#F3F3F3" } else { HexBrush "#181818" }
    $window.Foreground = if ($isLight) { HexBrush "#0F172A" } else { HexBrush "#F3F3F3" }

    # Sidebar
    if ($BorderSidebar) {
        $BorderSidebar.Background = if ($isLight) { HexBrush "#E8ECF2" } else { HexBrush "#111317" }
        $BorderSidebar.BorderBrush = if ($isLight) { HexBrush "#D2D8E2" } else { HexBrush "#20242B" }
    }
    if ($TxtBrandTitle) { $TxtBrandTitle.Foreground = if ($isLight) { HexBrush "#0F172A" } else { HexBrush "#FFFFFF" } }
    if ($TxtBrandSubtitle) { $TxtBrandSubtitle.Foreground = if ($isLight) { HexBrush "#64748B" } else { HexBrush "#8F96A0" } }
    if ($BorderSysEnv) {
        $BorderSysEnv.Background = if ($isLight) { HexBrush "#DDE3ED" } else { HexBrush "#16191F" }
        $BorderSysEnv.BorderBrush = if ($isLight) { HexBrush "#CBD5E1" } else { HexBrush "#222731" }
    }
    if ($TxtSysEnvHeader) { $TxtSysEnvHeader.Foreground = if ($isLight) { HexBrush "#64748B" } else { HexBrush "#8E95A5" } }
    if ($TxtSysEnvTitle) { $TxtSysEnvTitle.Foreground = if ($isLight) { HexBrush "#0F172A" } else { HexBrush "#FFFFFF" } }

    # Top Bar
    if ($BorderTopBar) {
        $BorderTopBar.Background = if ($isLight) { HexBrush "#FFFFFF" } else { HexBrush "#181818" }
        $BorderTopBar.BorderBrush = if ($isLight) { HexBrush "#E2E8F0" } else { HexBrush "#262626" }
    }
    if ($TxtPageTitle) { $TxtPageTitle.Foreground = if ($isLight) { HexBrush "#0F172A" } else { HexBrush "#FFFFFF" } }
    if ($TxtPageSubtitle) { $TxtPageSubtitle.Foreground = if ($isLight) { HexBrush "#64748B" } else { HexBrush "#8A8A8A" } }

    # Theme Toggle Button
    if ($BtnThemeToggle -and $TxtThemeIcon -and $TxtThemeLabel) {
        $mode = ($Script:Settings.theme).ToLower()
        if ($mode -eq "system") {
            $TxtThemeIcon.Text = "💻"
            $TxtThemeLabel.Text = if ($isLight) { "Auto (Light)" } else { "Auto (Dark)" }
        } elseif ($mode -eq "light") {
            $TxtThemeIcon.Text = "☀️"
            $TxtThemeLabel.Text = "Light"
        } else {
            $TxtThemeIcon.Text = "🌙"
            $TxtThemeLabel.Text = "Dark"
        }
        $BtnThemeToggle.Background = if ($isLight) { HexBrush "#F1F5F9" } else { HexBrush "#20242B" }
        $BtnThemeToggle.BorderBrush = if ($isLight) { HexBrush "#CBD5E1" } else { HexBrush "#323A48" }
        $TxtThemeLabel.Foreground = if ($isLight) { HexBrush "#0F172A" } else { HexBrush "#FFFFFF" }
    }

    # Top Sync Status Pill
    if ($BorderTopSync) {
        $BorderTopSync.Background = if ($isLight) { HexBrush "#E6F4EA" } else { HexBrush "#151C24" }
        $BorderTopSync.BorderBrush = if ($isLight) { HexBrush "#CEEAD6" } else { HexBrush "#223242" }
    }
    if ($TxtTopSyncIcon) { $TxtTopSyncIcon.Foreground = if ($isLight) { HexBrush "#137333" } else { HexBrush "#4ADE80" } }
    if ($TxtTopSyncStatus) { $TxtTopSyncStatus.Foreground = if ($isLight) { HexBrush "#137333" } else { HexBrush "#4ADE80" } }

    # Update Nav Items
    Update-Nav-Item $NavBtnSetup $NavIndSetup $NavIconSetup $NavTxtSetup ($Script:CurrentView -eq "setup")
    Update-Nav-Item $NavBtnCatalog $NavIndCatalog $NavIconCatalog $NavTxtCatalog ($Script:CurrentView -eq "catalog")
    Update-Nav-Item $NavBtnCloud $NavIndCloud $NavIconCloud $NavTxtCloud ($Script:CurrentView -eq "cloud")
    Update-Nav-Item $NavBtnLogs $NavIndLogs $NavIconLogs $NavTxtLogs ($Script:CurrentView -eq "logs")

    # Setup Page Hero
    if ($BorderHero) {
        $BorderHero.Background = if ($isLight) { HexBrush "#F0F6FF" } else { HexBrush "#1C212B" }
        $BorderHero.BorderBrush = if ($isLight) { HexBrush "#BFDBFE" } else { HexBrush "#2B3342" }
    }
    if ($TxtHeroTitle) { $TxtHeroTitle.Foreground = if ($isLight) { HexBrush "#0F172A" } else { HexBrush "#FFFFFF" } }
    if ($TxtHeroDesc) { $TxtHeroDesc.Foreground = if ($isLight) { HexBrush "#475569" } else { HexBrush "#9AA5B8" } }
    if ($BtnHeroCatalog) {
        $BtnHeroCatalog.Background = if ($isLight) { HexBrush "#FFFFFF" } else { HexBrush "#242B38" }
        $BtnHeroCatalog.Foreground = if ($isLight) { HexBrush "#0078D4" } else { HexBrush "#4CC2FF" }
        $BtnHeroCatalog.BorderBrush = if ($isLight) { HexBrush "#BFDBFE" } else { HexBrush "#353F52" }
    }
    if ($TxtTargetRoleTitle) { $TxtTargetRoleTitle.Foreground = if ($isLight) { HexBrush "#0F172A" } else { HexBrush "#FFFFFF" } }
    if ($TxtTargetRoleSubtitle) { $TxtTargetRoleSubtitle.Foreground = if ($isLight) { HexBrush "#64748B" } else { HexBrush "#8A8A8A" } }

    # Catalog Page
    if ($TxtSearch) {
        $TxtSearch.Background = if ($isLight) { HexBrush "#FFFFFF" } else { HexBrush "#161920" }
        $TxtSearch.Foreground = if ($isLight) { HexBrush "#0F172A" } else { HexBrush "#FFFFFF" }
        $TxtSearch.BorderBrush = if ($isLight) { HexBrush "#CBD5E1" } else { HexBrush "#282F3B" }
    }
    if ($BorderCatalogContainer) {
        $BorderCatalogContainer.Background = if ($isLight) { HexBrush "#FFFFFF" } else { HexBrush "#161920" }
        $BorderCatalogContainer.BorderBrush = if ($isLight) { HexBrush "#E2E8F0" } else { HexBrush "#252A34" }
    }
    if ($BtnCatalogSelectAll) {
        $BtnCatalogSelectAll.Background = if ($isLight) { HexBrush "#F1F5F9" } else { HexBrush "#222731" }
        $BtnCatalogSelectAll.BorderBrush = if ($isLight) { HexBrush "#CBD5E1" } else { HexBrush "#323A48" }
        $BtnCatalogSelectAll.Foreground = if ($isLight) { HexBrush "#1E293B" } else { HexBrush "#C8D0DC" }
    }
    if ($BtnCatalogClearAll) {
        $BtnCatalogClearAll.Background = if ($isLight) { HexBrush "#F1F5F9" } else { HexBrush "#222731" }
        $BtnCatalogClearAll.BorderBrush = if ($isLight) { HexBrush "#CBD5E1" } else { HexBrush "#323A48" }
        $BtnCatalogClearAll.Foreground = if ($isLight) { HexBrush "#1E293B" } else { HexBrush "#C8D0DC" }
    }

    # Cloud Sync Page
    if ($TxtCloudTitle1) { $TxtCloudTitle1.Foreground = if ($isLight) { HexBrush "#0F172A" } else { HexBrush "#FFFFFF" } }
    if ($TxtCloudDesc1) { $TxtCloudDesc1.Foreground = if ($isLight) { HexBrush "#64748B" } else { HexBrush "#8A8A8A" } }
    if ($BorderCloudCard1) {
        $BorderCloudCard1.Background = if ($isLight) { HexBrush "#FFFFFF" } else { HexBrush "#202020" }
        $BorderCloudCard1.BorderBrush = if ($isLight) { HexBrush "#E2E8F0" } else { HexBrush "#333333" }
    }
    if ($BorderCloudCard2) {
        $BorderCloudCard2.Background = if ($isLight) { HexBrush "#FFFFFF" } else { HexBrush "#202020" }
        $BorderCloudCard2.BorderBrush = if ($isLight) { HexBrush "#E2E8F0" } else { HexBrush "#2D2D2D" }
    }
    if ($BorderCloudCard3) {
        $BorderCloudCard3.Background = if ($isLight) { HexBrush "#FFFFFF" } else { HexBrush "#202020" }
        $BorderCloudCard3.BorderBrush = if ($isLight) { HexBrush "#E2E8F0" } else { HexBrush "#2D2D2D" }
    }
    if ($TxtCloudCard1Title) { $TxtCloudCard1Title.Foreground = if ($isLight) { HexBrush "#0F172A" } else { HexBrush "#FFFFFF" } }
    if ($TxtCloudCard2Title) { $TxtCloudCard2Title.Foreground = if ($isLight) { HexBrush "#0F172A" } else { HexBrush "#FFFFFF" } }
    if ($TxtCloudCard3Title) { $TxtCloudCard3Title.Foreground = if ($isLight) { HexBrush "#0F172A" } else { HexBrush "#FFFFFF" } }
    if ($TxtCloudUrlInput) {
        $TxtCloudUrlInput.Background = if ($isLight) { HexBrush "#F8FAFC" } else { HexBrush "#141414" }
        $TxtCloudUrlInput.BorderBrush = if ($isLight) { HexBrush "#CBD5E1" } else { HexBrush "#3D3D3D" }
        $TxtCloudUrlInput.Foreground = if ($isLight) { HexBrush "#0078D4" } else { HexBrush "#4CC2FF" }
    }
    if ($ChkAutoSync) { $ChkAutoSync.Foreground = if ($isLight) { HexBrush "#334155" } else { HexBrush "#C8C8C8" } }
    if ($BtnSaveCloudUrl) {
        $BtnSaveCloudUrl.Background = if ($isLight) { HexBrush "#F1F5F9" } else { HexBrush "#2D2D2D" }
        $BtnSaveCloudUrl.BorderBrush = if ($isLight) { HexBrush "#CBD5E1" } else { HexBrush "#3D3D3D" }
        $BtnSaveCloudUrl.Foreground = if ($isLight) { HexBrush "#1E293B" } else { HexBrush "#F3F3F3" }
    }

    # Logs Page
    if ($TxtLogsHeader) { $TxtLogsHeader.Foreground = if ($isLight) { HexBrush "#0F172A" } else { HexBrush "#FFFFFF" } }
    if ($BorderLogTerminal) {
        $BorderLogTerminal.BorderBrush = if ($isLight) { HexBrush "#CBD5E1" } else { HexBrush "#2D2D2D" }
    }
    if ($BtnOpenLogFile) {
        $BtnOpenLogFile.Background = if ($isLight) { HexBrush "#F1F5F9" } else { HexBrush "#2D2D2D" }
        $BtnOpenLogFile.BorderBrush = if ($isLight) { HexBrush "#CBD5E1" } else { HexBrush "#3D3D3D" }
        $BtnOpenLogFile.Foreground = if ($isLight) { HexBrush "#1E293B" } else { HexBrush "#F3F3F3" }
    }
    if ($BtnClearLogConsole) {
        $BtnClearLogConsole.Background = if ($isLight) { HexBrush "#F1F5F9" } else { HexBrush "#2D2D2D" }
        $BtnClearLogConsole.BorderBrush = if ($isLight) { HexBrush "#CBD5E1" } else { HexBrush "#3D3D3D" }
        $BtnClearLogConsole.Foreground = if ($isLight) { HexBrush "#1E293B" } else { HexBrush "#F3F3F3" }
    }

    # Bottom Bar
    if ($BorderBottomBar) {
        $BorderBottomBar.Background = if ($isLight) { HexBrush "#FFFFFF" } else { HexBrush "#141414" }
        $BorderBottomBar.BorderBrush = if ($isLight) { HexBrush "#E2E8F0" } else { HexBrush "#262626" }
    }
    if ($TxtStatus) { $TxtStatus.Foreground = if ($isLight) { HexBrush "#64748B" } else { HexBrush "#8A8A8A" } }
    if ($TxtCount) { $TxtCount.Foreground = if ($isLight) { HexBrush "#0078D4" } else { HexBrush "#4CC2FF" } }
    if ($ProgressBar) { $ProgressBar.Background = if ($isLight) { HexBrush "#E2E8F0" } else { HexBrush "#262626" } }
    if ($BtnOpenOfflineFolder) {
        $BtnOpenOfflineFolder.Background = if ($isLight) { HexBrush "#F1F5F9" } else { HexBrush "#222222" }
        $BtnOpenOfflineFolder.BorderBrush = if ($isLight) { HexBrush "#CBD5E1" } else { HexBrush "#333333" }
        $BtnOpenOfflineFolder.Foreground = if ($isLight) { HexBrush "#1E293B" } else { HexBrush "#C8C8C8" }
    }

    # Modal Card
    if ($BorderModalCard) {
        $BorderModalCard.Background = if ($isLight) { HexBrush "#FFFFFF" } else { HexBrush "#1F1F1F" }
        $BorderModalCard.BorderBrush = if ($isLight) { HexBrush "#CBD5E1" } else { HexBrush "#3A3A3A" }
    }
    if ($TxtModalTitle) { $TxtModalTitle.Foreground = if ($isLight) { HexBrush "#0F172A" } else { HexBrush "#FFFFFF" } }
    if ($BorderModalAppsContainer) {
        $BorderModalAppsContainer.Background = if ($isLight) { HexBrush "#F8FAFC" } else { HexBrush "#141414" }
        $BorderModalAppsContainer.BorderBrush = if ($isLight) { HexBrush "#CBD5E1" } else { HexBrush "#2D2D2D" }
    }
    if ($TxtGroupIconInput) {
        $TxtGroupIconInput.Background = if ($isLight) { HexBrush "#F8FAFC" } else { HexBrush "#141414" }
        $TxtGroupIconInput.BorderBrush = if ($isLight) { HexBrush "#CBD5E1" } else { HexBrush "#333333" }
        $TxtGroupIconInput.Foreground = if ($isLight) { HexBrush "#0F172A" } else { HexBrush "#FFFFFF" }
    }
    if ($TxtGroupNameInput) {
        $TxtGroupNameInput.Background = if ($isLight) { HexBrush "#F8FAFC" } else { HexBrush "#141414" }
        $TxtGroupNameInput.BorderBrush = if ($isLight) { HexBrush "#CBD5E1" } else { HexBrush "#333333" }
        $TxtGroupNameInput.Foreground = if ($isLight) { HexBrush "#0F172A" } else { HexBrush "#FFFFFF" }
    }
    if ($TxtGroupDescInput) {
        $TxtGroupDescInput.Background = if ($isLight) { HexBrush "#F8FAFC" } else { HexBrush "#141414" }
        $TxtGroupDescInput.BorderBrush = if ($isLight) { HexBrush "#CBD5E1" } else { HexBrush "#333333" }
        $TxtGroupDescInput.Foreground = if ($isLight) { HexBrush "#0F172A" } else { HexBrush "#FFFFFF" }
    }
    if ($TxtModalAppSearch) {
        $TxtModalAppSearch.Background = if ($isLight) { HexBrush "#F8FAFC" } else { HexBrush "#141414" }
        $TxtModalAppSearch.BorderBrush = if ($isLight) { HexBrush "#CBD5E1" } else { HexBrush "#333333" }
        $TxtModalAppSearch.Foreground = if ($isLight) { HexBrush "#0F172A" } else { HexBrush "#FFFFFF" }
    }
    if ($BtnModalSelectAll) {
        $BtnModalSelectAll.Background = if ($isLight) { HexBrush "#F1F5F9" } else { HexBrush "#2B2B2B" }
        $BtnModalSelectAll.BorderBrush = if ($isLight) { HexBrush "#CBD5E1" } else { HexBrush "#3D3D3D" }
        $BtnModalSelectAll.Foreground = if ($isLight) { HexBrush "#1E293B" } else { HexBrush "#CCCCCC" }
    }
    if ($BtnModalClearAll) {
        $BtnModalClearAll.Background = if ($isLight) { HexBrush "#F1F5F9" } else { HexBrush "#2B2B2B" }
        $BtnModalClearAll.BorderBrush = if ($isLight) { HexBrush "#CBD5E1" } else { HexBrush "#3D3D3D" }
        $BtnModalClearAll.Foreground = if ($isLight) { HexBrush "#1E293B" } else { HexBrush "#CCCCCC" }
    }
    if ($BtnCancelModal) {
        $BtnCancelModal.Background = if ($isLight) { HexBrush "#F1F5F9" } else { HexBrush "#2B2B2B" }
        $BtnCancelModal.BorderBrush = if ($isLight) { HexBrush "#CBD5E1" } else { HexBrush "#3D3D3D" }
        $BtnCancelModal.Foreground = if ($isLight) { HexBrush "#1E293B" } else { HexBrush "#F3F3F3" }
    }

    # Re-render Role Cards & Catalog for Active Theme
    Build-RoleCards
    Build-Catalog
    Select-Role $Script:ActiveRoleId
}

function Toggle-Theme {
    $cur = ($Script:Settings.theme).ToLower()
    if ($cur -eq "system") {
        $Script:Settings.theme = "light"
    } elseif ($cur -eq "light") {
        $Script:Settings.theme = "dark"
    } else {
        $Script:Settings.theme = "system"
    }
    Save-Settings $Script:Settings
    Apply-Theme (Get-CurrentThemeIsLight)
}

if ($BtnThemeToggle) {
    $BtnThemeToggle.Add_Click({ Toggle-Theme })
}

# Navigation Styling Helper
function Update-Nav-Item($Btn, $Ind, $Icon, $Txt, [bool]$IsActive) {
    if (-not $Btn) { return }
    $isLight = $Script:CurrentThemeIsLight
    if ($IsActive) {
        $Btn.Background = if ($isLight) { HexBrush "#D0DBEA" } else { HexBrush "#262E3B" }
        $Btn.BorderBrush = if ($isLight) { HexBrush "#B8C6DA" } else { HexBrush "#343F50" }
        if ($Ind) {
            $Ind.Visibility = [System.Windows.Visibility]::Visible
            $Ind.Background = HexBrush "#0078D4"
        }
        if ($Icon) { $Icon.Foreground = HexBrush "#0078D4" }
        if ($Txt) {
            $Txt.Foreground = if ($isLight) { HexBrush "#0F172A" } else { HexBrush "#FFFFFF" }
            $Txt.FontWeight = [System.Windows.FontWeights]::SemiBold
        }
    } else {
        $Btn.Background = [System.Windows.Media.Brushes]::Transparent
        $Btn.BorderBrush = [System.Windows.Media.Brushes]::Transparent
        if ($Ind) { $Ind.Visibility = [System.Windows.Visibility]::Collapsed }
        if ($Icon) { $Icon.Foreground = if ($isLight) { HexBrush "#64748B" } else { HexBrush "#C5CCD8" } }
        if ($Txt) {
            $Txt.Foreground = if ($isLight) { HexBrush "#475569" } else { HexBrush "#D0D6E0" }
            $Txt.FontWeight = [System.Windows.FontWeights]::Normal
        }
    }
}

function Attach-Nav-Hover($Btn, [string]$TargetView) {
    if (-not $Btn) { return }
    $Btn.Add_MouseEnter({
        if ($Script:CurrentView -ne $TargetView) {
            $Btn.Background = if ($Script:CurrentThemeIsLight) { HexBrush "#DDE3EC" } else { HexBrush "#1F2530" }
        }
    })
    $Btn.Add_MouseLeave({
        if ($Script:CurrentView -ne $TargetView) {
            $Btn.Background = [System.Windows.Media.Brushes]::Transparent
        }
    })
}

# Navigation System
function Switch-View([string]$ViewName) {
    $Script:CurrentView = $ViewName
    $ViewSetup.Visibility   = [System.Windows.Visibility]::Collapsed
    $ViewCatalog.Visibility = [System.Windows.Visibility]::Collapsed
    $ViewCloud.Visibility   = [System.Windows.Visibility]::Collapsed
    $ViewLogs.Visibility    = [System.Windows.Visibility]::Collapsed

    Update-Nav-Item $NavBtnSetup $NavIndSetup $NavIconSetup $NavTxtSetup ($ViewName -eq "setup")
    Update-Nav-Item $NavBtnCatalog $NavIndCatalog $NavIconCatalog $NavTxtCatalog ($ViewName -eq "catalog")
    Update-Nav-Item $NavBtnCloud $NavIndCloud $NavIconCloud $NavTxtCloud ($ViewName -eq "cloud")
    Update-Nav-Item $NavBtnLogs $NavIndLogs $NavIconLogs $NavTxtLogs ($ViewName -eq "logs")

    switch ($ViewName) {
        "setup" {
            $ViewSetup.Visibility = [System.Windows.Visibility]::Visible
            $TxtPageTitle.Text = "Machine configuration"
            $TxtPageSubtitle.Text = "Set up your developer machine quickly with curated role environments."
        }
        "catalog" {
            $ViewCatalog.Visibility = [System.Windows.Visibility]::Visible
            $TxtPageTitle.Text = "Applications catalog"
            $TxtPageSubtitle.Text = "Browse, search, and select individual software packages for this system."
        }
        "cloud" {
            $ViewCloud.Visibility = [System.Windows.Visibility]::Visible
            $TxtPageTitle.Text = "Cloud synchronization"
            $TxtPageSubtitle.Text = "Connect to GitHub or Google Drive to share one centralized software list."
        }
        "logs" {
            $ViewLogs.Visibility = [System.Windows.Visibility]::Visible
            $TxtPageTitle.Text = "Activity & Installation logs"
            $TxtPageSubtitle.Text = "Live terminal stream and detailed historical records."
        }
    }
}

$NavBtnSetup.Add_Click({ Switch-View "setup" })
$NavBtnCatalog.Add_Click({ Switch-View "catalog" })
$NavBtnCloud.Add_Click({ Switch-View "cloud" })
$NavBtnLogs.Add_Click({ Switch-View "logs" })

Attach-Nav-Hover $NavBtnSetup "setup"
Attach-Nav-Hover $NavBtnCatalog "catalog"
Attach-Nav-Hover $NavBtnCloud "cloud"
Attach-Nav-Hover $NavBtnLogs "logs"

if ($BtnHeroCatalog) { $BtnHeroCatalog.Add_Click({ Switch-View "catalog" }) }
if ($BtnCatalogSelectAll) {
    $BtnCatalogSelectAll.Add_Click({
        foreach ($r in $Script:AppRows) {
            if ($r.Card.Visibility -eq [System.Windows.Visibility]::Visible) { $r.CheckBox.IsChecked = $true }
        }
        Update-Count
    })
}
if ($BtnCatalogClearAll) {
    $BtnCatalogClearAll.Add_Click({
        foreach ($r in $Script:AppRows) { $r.CheckBox.IsChecked = $false }
        Update-Count
    })
}

function DoEvents {
    $frame = New-Object System.Windows.Threading.DispatcherFrame
    [System.Windows.Threading.Dispatcher]::CurrentDispatcher.BeginInvoke(
        [System.Windows.Threading.DispatcherPriority]::Background,
        [System.Action[object]]{ param($f) $f.Continue = $false },
        $frame
    ) | Out-Null
    [System.Windows.Threading.Dispatcher]::PushFrame($frame)
}

function Append-Log([string]$message) {
    $timestamp = Get-Date -Format "HH:mm:ss"
    $line = "[$timestamp] $message`r`n"
    $TxtLog.AppendText($line)
    $LogScrollViewer.ScrollToEnd()
    Add-Content -Path $LogFilePath -Value "[$timestamp] $message" -ErrorAction SilentlyContinue
    DoEvents
}

# Emoji Chips
function Build-EmojiChips {
    $EmojiChipsPanel.Children.Clear()
    $emojis = @("💻", "🎨", "🧪", "📄", "🚀", "📊", "🛠️", "🎬", "🤖", "📱", "🔒", "⚡", "🌐", "🎮", "💼")
    foreach ($em in $emojis) {
        $eb = New-Object System.Windows.Controls.Button
        $eb.Content = $em
        $eb.FontSize = 14
        $eb.Width = 32
        $eb.Height = 28
        $eb.Margin = "0,0,4,4"
        $eb.Background = if ($Script:CurrentThemeIsLight) { HexBrush "#F1F5F9" } else { HexBrush "#2B2B2B" }
        $eb.BorderBrush = if ($Script:CurrentThemeIsLight) { HexBrush "#CBD5E1" } else { HexBrush "#3D3D3D" }
        $eb.BorderThickness = 1
        $eb.Cursor = [System.Windows.Input.Cursors]::Hand
        $eb.Tag = $em
        $eb.Add_Click({ param($s,$e) $TxtGroupIconInput.Text = $s.Tag })
        $EmojiChipsPanel.Children.Add($eb) | Out-Null
    }
}

function Build-RoleCards {
    $RoleCardsGrid.Children.Clear()
    $Script:RoleCardViews.Clear()
    $isLight = $Script:CurrentThemeIsLight

    foreach ($grp in $ConfigJson.groups) {
        $card = New-Object System.Windows.Controls.Border
        $card.Background = if ($isLight) { HexBrush "#FFFFFF" } else { HexBrush "#1A1D24" }
        $card.CornerRadius = [System.Windows.CornerRadius]::new(8)
        $card.Margin = [System.Windows.Thickness]::new(6)
        $card.Padding = [System.Windows.Thickness]::new(18, 16, 18, 16)
        $card.BorderThickness = [System.Windows.Thickness]::new(1)
        $card.BorderBrush = if ($isLight) { HexBrush "#E2E8F0" } else { HexBrush "#282F3B" }

        $grid = New-Object System.Windows.Controls.Grid
        $grid.RowDefinitions.Add((New-Object System.Windows.Controls.RowDefinition -Property @{ Height = [System.Windows.GridLength]::Auto }))
        $grid.RowDefinitions.Add((New-Object System.Windows.Controls.RowDefinition -Property @{ Height = [System.Windows.GridLength]::Auto }))
        $grid.RowDefinitions.Add((New-Object System.Windows.Controls.RowDefinition -Property @{ Height = (New-Object System.Windows.GridLength 1, [System.Windows.GridUnitType]::Star) }))
        $grid.RowDefinitions.Add((New-Object System.Windows.Controls.RowDefinition -Property @{ Height = [System.Windows.GridLength]::Auto }))

        # Header
        $hdr = New-Object System.Windows.Controls.Grid
        $hdr.ColumnDefinitions.Add((New-Object System.Windows.Controls.ColumnDefinition -Property @{ Width = (New-Object System.Windows.GridLength 1, [System.Windows.GridUnitType]::Star) }))
        $hdr.ColumnDefinitions.Add((New-Object System.Windows.Controls.ColumnDefinition -Property @{ Width = [System.Windows.GridLength]::Auto }))

        $titleStack = New-Object System.Windows.Controls.StackPanel
        $titleStack.Orientation = [System.Windows.Controls.Orientation]::Horizontal
        $titleStack.VerticalAlignment = [System.Windows.VerticalAlignment]::Center

        $iconBox = New-Object System.Windows.Controls.Border
        $iconBox.Width = 36
        $iconBox.Height = 36
        $iconBox.CornerRadius = [System.Windows.CornerRadius]::new(8)
        $iconBox.Background = if ($isLight) { HexBrush "#F1F5F9" } else { HexBrush "#222834" }
        $iconBox.BorderBrush = if ($isLight) { HexBrush "#CBD5E1" } else { HexBrush "#2E3748" }
        $iconBox.BorderThickness = [System.Windows.Thickness]::new(1)
        $iconBox.Margin = [System.Windows.Thickness]::new(0, 0, 10, 0)

        $tIcon = New-Object System.Windows.Controls.TextBlock
        $tIcon.Text = $grp.icon
        $tIcon.FontSize = 18
        $tIcon.HorizontalAlignment = [System.Windows.HorizontalAlignment]::Center
        $tIcon.VerticalAlignment = [System.Windows.VerticalAlignment]::Center
        $iconBox.Child = $tIcon
        $titleStack.Children.Add($iconBox) | Out-Null

        $tName = New-Object System.Windows.Controls.TextBlock
        $tName.Text = $grp.name
        $tName.FontSize = 15
        $tName.FontWeight = [System.Windows.FontWeights]::Bold
        $tName.Foreground = if ($isLight) { HexBrush "#0F172A" } else { HexBrush "#FFFFFF" }
        $tName.VerticalAlignment = [System.Windows.VerticalAlignment]::Center
        $titleStack.Children.Add($tName) | Out-Null
        [System.Windows.Controls.Grid]::SetColumn($titleStack, 0)
        $hdr.Children.Add($titleStack) | Out-Null

        $cBadge = New-Object System.Windows.Controls.Border
        $cBadge.Background = if ($isLight) { HexBrush "#E0F2FE" } else { HexBrush "#152538" }
        $cBadge.BorderBrush = if ($isLight) { HexBrush "#BAE6FD" } else { HexBrush "#203B57" }
        $cBadge.BorderThickness = [System.Windows.Thickness]::new(1)
        $cBadge.CornerRadius = [System.Windows.CornerRadius]::new(5)
        $cBadge.Padding = [System.Windows.Thickness]::new(8, 3, 8, 3)
        $cBadge.VerticalAlignment = [System.Windows.VerticalAlignment]::Center

        $cTxt = New-Object System.Windows.Controls.TextBlock
        $cTxt.Text = "$($grp.apps.Count) apps"
        $cTxt.FontSize = 11
        $cTxt.FontWeight = [System.Windows.FontWeights]::SemiBold
        $cTxt.Foreground = if ($isLight) { HexBrush "#0369A1" } else { HexBrush "#4CC2FF" }
        $cBadge.Child = $cTxt
        [System.Windows.Controls.Grid]::SetColumn($cBadge, 1)
        $hdr.Children.Add($cBadge) | Out-Null

        [System.Windows.Controls.Grid]::SetRow($hdr, 0)
        $grid.Children.Add($hdr) | Out-Null

        # Desc
        $desc = New-Object System.Windows.Controls.TextBlock
        $desc.Text = $grp.description
        $desc.FontSize = 12
        $desc.Foreground = if ($isLight) { HexBrush "#64748B" } else { HexBrush "#8E95A5" }
        $desc.Margin = [System.Windows.Thickness]::new(0, 8, 0, 10)
        $desc.TextWrapping = [System.Windows.TextWrapping]::Wrap
        $desc.LineHeight = 16
        [System.Windows.Controls.Grid]::SetRow($desc, 1)
        $grid.Children.Add($desc) | Out-Null

        # Pills preview
        $wrap = New-Object System.Windows.Controls.WrapPanel
        $wrap.Margin = [System.Windows.Thickness]::new(0, 0, 0, 14)
        $count = 0
        foreach ($appId in $grp.apps) {
            if ($count -ge 5) { break }
            $matchApp = $ConfigJson.apps | Where-Object { $_.id -eq $appId }
            if ($matchApp) {
                $p = New-Object System.Windows.Controls.Border
                $p.Background = if ($isLight) { HexBrush "#F1F5F9" } else { HexBrush "#1D222B" }
                $p.BorderBrush = if ($isLight) { HexBrush "#E2E8F0" } else { HexBrush "#2B3340" }
                $p.BorderThickness = [System.Windows.Thickness]::new(1)
                $p.CornerRadius = [System.Windows.CornerRadius]::new(4)
                $p.Padding = [System.Windows.Thickness]::new(7, 3, 7, 3)
                $p.Margin = [System.Windows.Thickness]::new(0, 0, 5, 5)

                $pt = New-Object System.Windows.Controls.TextBlock
                $pt.Text = $matchApp.name
                $pt.FontSize = 11
                $pt.Foreground = if ($isLight) { HexBrush "#334155" } else { HexBrush "#D0D5DF" }
                $p.Child = $pt
                $wrap.Children.Add($p) | Out-Null
                $count++
            }
        }
        if ($grp.apps.Count -gt 5) {
            $extraBorder = New-Object System.Windows.Controls.Border
            $extraBorder.Background = if ($isLight) { HexBrush "#E0F2FE" } else { HexBrush "#152538" }
            $extraBorder.BorderBrush = if ($isLight) { HexBrush "#BAE6FD" } else { HexBrush "#203B57" }
            $extraBorder.BorderThickness = [System.Windows.Thickness]::new(1)
            $extraBorder.CornerRadius = [System.Windows.CornerRadius]::new(4)
            $extraBorder.Padding = [System.Windows.Thickness]::new(6, 3, 6, 3)
            $extraBorder.Margin = [System.Windows.Thickness]::new(0, 0, 5, 5)

            $extra = New-Object System.Windows.Controls.TextBlock
            $extra.Text = "+$($grp.apps.Count - 5) more"
            $extra.FontSize = 10
            $extra.FontWeight = [System.Windows.FontWeights]::Bold
            $extra.Foreground = if ($isLight) { HexBrush "#0369A1" } else { HexBrush "#4CC2FF" }
            $extraBorder.Child = $extra
            $wrap.Children.Add($extraBorder) | Out-Null
        }
        [System.Windows.Controls.Grid]::SetRow($wrap, 2)
        $grid.Children.Add($wrap) | Out-Null

        # Action Buttons Row: [ Apply Role ]  [ ✏️ Edit ]
        $btnRow = New-Object System.Windows.Controls.Grid
        $btnRow.ColumnDefinitions.Add((New-Object System.Windows.Controls.ColumnDefinition -Property @{ Width = (New-Object System.Windows.GridLength 1, [System.Windows.GridUnitType]::Star) }))
        $btnRow.ColumnDefinitions.Add((New-Object System.Windows.Controls.ColumnDefinition -Property @{ Width = [System.Windows.GridLength]::Auto }))

        $selBtn = New-Object System.Windows.Controls.Button
        $selBtn.Content = "Apply Role"
        $selBtn.Background = if ($isLight) { HexBrush "#F1F5F9" } else { HexBrush "#222731" }
        $selBtn.Foreground = if ($isLight) { HexBrush "#1E293B" } else { HexBrush "#FFFFFF" }
        $selBtn.FontWeight = [System.Windows.FontWeights]::SemiBold
        $selBtn.FontSize = 12
        $selBtn.Padding = [System.Windows.Thickness]::new(12, 6, 12, 6)
        $selBtn.BorderThickness = [System.Windows.Thickness]::new(1)
        $selBtn.BorderBrush = if ($isLight) { HexBrush "#CBD5E1" } else { HexBrush "#323A48" }
        $selBtn.Cursor = [System.Windows.Input.Cursors]::Hand
        $selBtn.Tag = $grp.id
        $selBtn.Add_Click({ param($s,$e) Select-Role $s.Tag })
        [System.Windows.Controls.Grid]::SetColumn($selBtn, 0)
        $btnRow.Children.Add($selBtn) | Out-Null

        $editBtn = New-Object System.Windows.Controls.Button
        $editBtn.Content = "✏️ Edit"
        $editBtn.Background = if ($isLight) { HexBrush "#F1F5F9" } else { HexBrush "#222731" }
        $editBtn.Foreground = if ($isLight) { HexBrush "#475569" } else { HexBrush "#C0C6D2" }
        $editBtn.FontSize = 12
        $editBtn.Padding = [System.Windows.Thickness]::new(12, 6, 12, 6)
        $editBtn.Margin = [System.Windows.Thickness]::new(8, 0, 0, 0)
        $editBtn.BorderThickness = [System.Windows.Thickness]::new(1)
        $editBtn.BorderBrush = if ($isLight) { HexBrush "#CBD5E1" } else { HexBrush "#323A48" }
        $editBtn.Cursor = [System.Windows.Input.Cursors]::Hand
        $editBtn.Tag = $grp.id
        $editBtn.Add_Click({ param($s,$e) Open-EditGroupModal $s.Tag })
        [System.Windows.Controls.Grid]::SetColumn($editBtn, 1)
        $btnRow.Children.Add($editBtn) | Out-Null

        [System.Windows.Controls.Grid]::SetRow($btnRow, 3)
        $grid.Children.Add($btnRow) | Out-Null

        $card.Child = $grid

        $currentGrpId = $grp.id
        $card.Add_MouseEnter({
            if ($Script:ActiveRoleId -ne $currentGrpId) {
                $card.Background = if ($Script:CurrentThemeIsLight) { HexBrush "#F8FAFC" } else { HexBrush "#1F242F" }
                $card.BorderBrush = if ($Script:CurrentThemeIsLight) { HexBrush "#CBD5E1" } else { HexBrush "#3E485A" }
            }
        })
        $card.Add_MouseLeave({
            if ($Script:ActiveRoleId -ne $currentGrpId) {
                $card.Background = if ($Script:CurrentThemeIsLight) { HexBrush "#FFFFFF" } else { HexBrush "#1A1D24" }
                $card.BorderBrush = if ($Script:CurrentThemeIsLight) { HexBrush "#E2E8F0" } else { HexBrush "#282F3B" }
            }
        })

        $RoleCardsGrid.Children.Add($card) | Out-Null

        $Script:RoleCardViews.Add([PSCustomObject]@{
            Id = $grp.id
            Card = $card
            SelectButton = $selBtn
            EditButton = $editBtn
            CountText = $cTxt
        })
    }
}

function Select-Role([string]$roleId) {
    $Script:ActiveRoleId = $roleId
    $targetGrp = $ConfigJson.groups | Where-Object { $_.id -eq $roleId }
    if (-not $targetGrp) {
        if ($ConfigJson.groups.Count -gt 0) { $targetGrp = $ConfigJson.groups[0] }
        else { return }
    }
    $isLight = $Script:CurrentThemeIsLight

    foreach ($rc in $Script:RoleCardViews) {
        if ($rc.Id -eq $targetGrp.id) {
            $rc.Card.BorderBrush = HexBrush "#0078D4"
            $rc.Card.BorderThickness = [System.Windows.Thickness]::new(2)
            $rc.Card.Background = if ($isLight) { HexBrush "#EFF6FF" } else { HexBrush "#16202C" }
            $rc.SelectButton.Content = "✓ Active Environment"
            $rc.SelectButton.Background = HexBrush "#0078D4"
            $rc.SelectButton.Foreground = HexBrush "#FFFFFF"
            $rc.SelectButton.BorderThickness = [System.Windows.Thickness]::new(0)
        } else {
            $rc.Card.BorderBrush = if ($isLight) { HexBrush "#E2E8F0" } else { HexBrush "#282F3B" }
            $rc.Card.BorderThickness = [System.Windows.Thickness]::new(1)
            $rc.Card.Background = if ($isLight) { HexBrush "#FFFFFF" } else { HexBrush "#1A1D24" }
            $rc.SelectButton.Content = "Apply Role"
            $rc.SelectButton.Background = if ($isLight) { HexBrush "#F1F5F9" } else { HexBrush "#222731" }
            $rc.SelectButton.BorderBrush = if ($isLight) { HexBrush "#CBD5E1" } else { HexBrush "#323A48" }
            $rc.SelectButton.Foreground = if ($isLight) { HexBrush "#1E293B" } else { HexBrush "#FFFFFF" }
            $rc.SelectButton.BorderThickness = [System.Windows.Thickness]::new(1)
        }
    }

    foreach ($row in $Script:AppRows) {
        $row.CheckBox.IsChecked = ($targetGrp.apps -contains $row.Id)
    }

    Update-Count
    $TxtStatus.Text = "Role selected: $($targetGrp.name)"
}

# Modal Controls
function Open-CreateGroupModal {
    $Script:CurrentEditingGroupId = $null
    $TxtModalTitle.Text = "Create New Role Group"
    $TxtGroupNameInput.Text = ""
    $TxtGroupIconInput.Text = "🚀"
    $TxtGroupDescInput.Text = ""
    $BtnDeleteGroup.Visibility = [System.Windows.Visibility]::Collapsed
    Populate-ModalApps @()
    $GroupModalOverlay.Visibility = [System.Windows.Visibility]::Visible
    $TxtGroupNameInput.Focus()
}

function Open-EditGroupModal([string]$groupId) {
    $Script:CurrentEditingGroupId = $groupId
    $grp = $ConfigJson.groups | Where-Object { $_.id -eq $groupId }
    if (-not $grp) { return }

    $TxtModalTitle.Text = "Edit Role Group: $($grp.name)"
    $TxtGroupNameInput.Text = $grp.name
    $TxtGroupIconInput.Text = $grp.icon
    $TxtGroupDescInput.Text = $grp.description
    $BtnDeleteGroup.Visibility = [System.Windows.Visibility]::Visible
    Populate-ModalApps $grp.apps
    $GroupModalOverlay.Visibility = [System.Windows.Visibility]::Visible
    $TxtGroupNameInput.Focus()
}

function Close-GroupModal {
    $GroupModalOverlay.Visibility = [System.Windows.Visibility]::Collapsed
}

function Populate-ModalApps([array]$selectedAppIds) {
    $ModalAppsContainer.Children.Clear()
    $Script:ModalAppRows.Clear()
    $TxtModalAppSearch.Text = "Filter software..."
    $TxtModalAppSearch.Foreground = if ($Script:CurrentThemeIsLight) { HexBrush "#94A3B8" } else { HexBrush "#707070" }
    $isLight = $Script:CurrentThemeIsLight

    foreach ($app in $ConfigJson.apps) {
        $b = New-Object System.Windows.Controls.Border
        $b.Background = if ($isLight) { HexBrush "#FFFFFF" } else { HexBrush "#181818" }
        $b.CornerRadius = 6
        $b.Margin = "0,0,0,4"
        $b.Padding = "10,6"
        $b.BorderThickness = 1
        $b.BorderBrush = if ($isLight) { HexBrush "#E2E8F0" } else { HexBrush "#2B2B2B" }

        $g = New-Object System.Windows.Controls.Grid
        $g.ColumnDefinitions.Add((New-Object System.Windows.Controls.ColumnDefinition -Property @{ Width = [System.Windows.GridLength]::Auto }))
        $g.ColumnDefinitions.Add((New-Object System.Windows.Controls.ColumnDefinition -Property @{ Width = (New-Object System.Windows.GridLength 1, [System.Windows.GridUnitType]::Star) }))

        $cb = New-Object System.Windows.Controls.CheckBox
        $cb.VerticalAlignment = [System.Windows.VerticalAlignment]::Center
        $cb.Margin = "0,0,10,0"
        $cb.Cursor = [System.Windows.Input.Cursors]::Hand
        $cb.IsChecked = ($selectedAppIds -contains $app.id)
        $cb.Add_Checked({ Update-ModalCount })
        $cb.Add_Unchecked({ Update-ModalCount })
        [System.Windows.Controls.Grid]::SetColumn($cb, 0)
        $g.Children.Add($cb) | Out-Null

        $sp = New-Object System.Windows.Controls.StackPanel
        $tRow = New-Object System.Windows.Controls.StackPanel
        $tRow.Orientation = [System.Windows.Controls.Orientation]::Horizontal

        $nt = New-Object System.Windows.Controls.TextBlock
        $nt.Text = $app.name
        $nt.FontSize = 12
        $nt.FontWeight = [System.Windows.FontWeights]::SemiBold
        $nt.Foreground = if ($isLight) { HexBrush "#0F172A" } else { HexBrush "#FFFFFF" }
        $tRow.Children.Add($nt) | Out-Null

        $badge = New-Object System.Windows.Controls.Border
        $badge.Background = if ($isLight) { HexBrush "#E2E8F0" } else { HexBrush "#262626" }
        $badge.CornerRadius = 4
        $badge.Margin = "8,0,0,0"
        $badge.Padding = "5,1"
        $bt = New-Object System.Windows.Controls.TextBlock
        $bt.Text = $app.category.ToUpper()
        $bt.FontSize = 9
        $bt.FontWeight = [System.Windows.FontWeights]::Bold
        $bt.Foreground = if ($isLight) { HexBrush "#475569" } else { HexBrush "#8A8A8A" }
        $badge.Child = $bt
        $tRow.Children.Add($badge) | Out-Null
        $sp.Children.Add($tRow) | Out-Null

        $dt = New-Object System.Windows.Controls.TextBlock
        $dt.Text = $app.description
        $dt.FontSize = 10
        $dt.Foreground = if ($isLight) { HexBrush "#64748B" } else { HexBrush "#707070" }
        $dt.Margin = "0,2,0,0"
        $sp.Children.Add($dt) | Out-Null

        [System.Windows.Controls.Grid]::SetColumn($sp, 1)
        $g.Children.Add($sp) | Out-Null
        $b.Child = $g
        $ModalAppsContainer.Children.Add($b) | Out-Null

        $Script:ModalAppRows.Add([PSCustomObject]@{
            App = $app
            Border = $b
            CheckBox = $cb
        })
    }
    Update-ModalCount
}

function Update-ModalCount {
    $sel = ($Script:ModalAppRows | Where-Object { $_.CheckBox.IsChecked -eq $true }).Count
    $TxtModalSelectedCount.Text = "$sel selected"
}

function Filter-ModalApps {
    $q = $TxtModalAppSearch.Text.Trim().ToLower()
    if ($q -eq "filter software...") { $q = "" }
    foreach ($m in $Script:ModalAppRows) {
        $match = ($q -eq "") -or
                 ($m.App.name.ToLower().Contains($q)) -or
                 ($m.App.description.ToLower().Contains($q)) -or
                 ($m.App.category.ToLower().Contains($q))
        if ($match) { $m.Border.Visibility = [System.Windows.Visibility]::Visible }
        else { $m.Border.Visibility = [System.Windows.Visibility]::Collapsed }
    }
}

$TxtModalAppSearch.Add_GotFocus({
    if ($TxtModalAppSearch.Text -eq "Filter software...") {
        $TxtModalAppSearch.Text = ""
        $TxtModalAppSearch.Foreground = if ($Script:CurrentThemeIsLight) { HexBrush "#0F172A" } else { HexBrush "#FFFFFF" }
    }
})
$TxtModalAppSearch.Add_LostFocus({
    if ([string]::IsNullOrWhiteSpace($TxtModalAppSearch.Text)) {
        $TxtModalAppSearch.Text = "Filter software..."
        $TxtModalAppSearch.Foreground = if ($Script:CurrentThemeIsLight) { HexBrush "#94A3B8" } else { HexBrush "#707070" }
    }
})
$TxtModalAppSearch.Add_TextChanged({ Filter-ModalApps })

$BtnModalSelectAll.Add_Click({
    foreach ($m in $Script:ModalAppRows) {
        if ($m.Border.Visibility -eq [System.Windows.Visibility]::Visible) { $m.CheckBox.IsChecked = $true }
    }
    Update-ModalCount
})
$BtnModalClearAll.Add_Click({
    foreach ($m in $Script:ModalAppRows) { $m.CheckBox.IsChecked = $false }
    Update-ModalCount
})

$BtnCreateNewGroup.Add_Click({ Open-CreateGroupModal })
$BtnCloseModal.Add_Click({ Close-GroupModal })
$BtnCancelModal.Add_Click({ Close-GroupModal })

$BtnSaveGroup.Add_Click({
    $name = $TxtGroupNameInput.Text.Trim()
    if ([string]::IsNullOrEmpty($name)) {
        [System.Windows.MessageBox]::Show("Please enter a group name.", "Missing Name", [System.Windows.MessageBoxButton]::OK, [System.Windows.MessageBoxImage]::Warning)
        return
    }

    $icon = $TxtGroupIconInput.Text.Trim()
    if ([string]::IsNullOrEmpty($icon)) { $icon = "🚀" }
    $desc = $TxtGroupDescInput.Text.Trim()

    $selectedIds = @($Script:ModalAppRows | Where-Object { $_.CheckBox.IsChecked -eq $true } | ForEach-Object { $_.App.id })

    if ($Script:CurrentEditingGroupId -eq $null) {
        $newId = ($name.ToLower() -replace '[^a-z0-9]', '_') + "_" + (Get-Random -Minimum 100 -Maximum 999)
        $newGrp = [PSCustomObject]@{
            id = $newId
            name = $name
            icon = $icon
            description = $desc
            accentColor = "#0078D4"
            apps = $selectedIds
        }
        $ConfigJson.groups += $newGrp
        $Script:CurrentEditingGroupId = $newId
    } else {
        $existing = $ConfigJson.groups | Where-Object { $_.id -eq $Script:CurrentEditingGroupId }
        if ($existing) {
            $existing.name = $name
            $existing.icon = $icon
            $existing.description = $desc
            $existing.apps = $selectedIds
        }
    }

    $json = $ConfigJson | ConvertTo-Json -Depth 5
    [System.IO.File]::WriteAllText($ConfigPath, $json, [System.Text.Encoding]::UTF8)

    Close-GroupModal
    Build-RoleCards
    Select-Role $Script:CurrentEditingGroupId
    $TxtStatus.Text = "Group '$name' saved with $($selectedIds.Count) packages."
})

$BtnDeleteGroup.Add_Click({
    if ($Script:CurrentEditingGroupId -eq $null) { return }
    if ($ConfigJson.groups.Count -le 1) {
        [System.Windows.MessageBox]::Show("You cannot delete the only remaining role group. At least one group is required.", "Cannot Delete", [System.Windows.MessageBoxButton]::OK, [System.Windows.MessageBoxImage]::Warning)
        return
    }

    $target = $ConfigJson.groups | Where-Object { $_.id -eq $Script:CurrentEditingGroupId }
    if (-not $target) { return }

    $res = [System.Windows.MessageBox]::Show("Are you sure you want to delete the role group '$($target.name)'?", "Confirm Deletion", [System.Windows.MessageBoxButton]::YesNo, [System.Windows.MessageBoxImage]::Question)
    if ($res -ne [System.Windows.MessageBoxResult]::Yes) { return }

    $ConfigJson.groups = @($ConfigJson.groups | Where-Object { $_.id -ne $Script:CurrentEditingGroupId })
    $json = $ConfigJson | ConvertTo-Json -Depth 5
    [System.IO.File]::WriteAllText($ConfigPath, $json, [System.Text.Encoding]::UTF8)

    Close-GroupModal
    Build-RoleCards
    if ($ConfigJson.groups.Count -gt 0) { Select-Role $ConfigJson.groups[0].id }
    $TxtStatus.Text = "Group '$($target.name)' deleted."
})

# Catalog Builder
function Build-Catalog {
    $AppsContainer.Children.Clear()
    $Script:AppRows.Clear()
    $CategoryChipsPanel.Children.Clear()
    $isLight = $Script:CurrentThemeIsLight

    $allChip = New-Object System.Windows.Controls.Button
    $allChip.Content = "All ($($ConfigJson.apps.Count))"
    $allChip.Padding = "12,6"
    $allChip.Margin = "0,0,6,0"
    $allChip.Background = HexBrush "#0078D4"
    $allChip.Foreground = HexBrush "#FFFFFF"
    $allChip.BorderThickness = 0
    $allChip.Cursor = [System.Windows.Input.Cursors]::Hand
    $allChip.FontSize = 12
    $allChip.Tag = "all"
    $allChip.Add_Click({ Set-CategoryFilter "all" })
    $CategoryChipsPanel.Children.Add($allChip) | Out-Null

    foreach ($cat in $ConfigJson.categories) {
        $catCount = ($ConfigJson.apps | Where-Object { $_.category -eq $cat.id }).Count
        $cChip = New-Object System.Windows.Controls.Button
        $cChip.Content = "$($cat.icon) $($cat.name) ($catCount)"
        $cChip.Padding = "12,6"
        $cChip.Margin = "0,0,6,0"
        $cChip.Background = if ($isLight) { HexBrush "#E2E8F0" } else { HexBrush "#2A2A2A" }
        $cChip.Foreground = if ($isLight) { HexBrush "#475569" } else { HexBrush "#A0A0A0" }
        $cChip.BorderThickness = 0
        $cChip.Cursor = [System.Windows.Input.Cursors]::Hand
        $cChip.FontSize = 12
        $cChip.Tag = $cat.id
        $cChip.Add_Click({ param($s,$e) Set-CategoryFilter $s.Tag })
        $CategoryChipsPanel.Children.Add($cChip) | Out-Null
    }

    foreach ($app in $ConfigJson.apps) {
        $card = New-Object System.Windows.Controls.Border
        $card.Background = if ($isLight) { HexBrush "#FFFFFF" } else { HexBrush "#181818" }
        $card.CornerRadius = 8
        $card.Margin = "0,0,0,6"
        $card.Padding = "14,10"
        $card.BorderThickness = 1
        $card.BorderBrush = if ($isLight) { HexBrush "#E2E8F0" } else { HexBrush "#2B2B2B" }

        $grid = New-Object System.Windows.Controls.Grid
        $grid.ColumnDefinitions.Add((New-Object System.Windows.Controls.ColumnDefinition -Property @{ Width = [System.Windows.GridLength]::Auto }))
        $grid.ColumnDefinitions.Add((New-Object System.Windows.Controls.ColumnDefinition -Property @{ Width = (New-Object System.Windows.GridLength 1, [System.Windows.GridUnitType]::Star) }))
        $grid.ColumnDefinitions.Add((New-Object System.Windows.Controls.ColumnDefinition -Property @{ Width = [System.Windows.GridLength]::Auto }))

        $cb = New-Object System.Windows.Controls.CheckBox
        $cb.VerticalAlignment = [System.Windows.VerticalAlignment]::Center
        $cb.Margin = "0,0,14,0"
        $cb.Cursor = [System.Windows.Input.Cursors]::Hand
        $cb.Add_Checked({ Update-Count })
        $cb.Add_Unchecked({ Update-Count })
        [System.Windows.Controls.Grid]::SetColumn($cb, 0)
        $grid.Children.Add($cb) | Out-Null

        $infoStack = New-Object System.Windows.Controls.StackPanel
        $titleStack = New-Object System.Windows.Controls.StackPanel
        $titleStack.Orientation = [System.Windows.Controls.Orientation]::Horizontal

        $txtName = New-Object System.Windows.Controls.TextBlock
        $txtName.Text = $app.name
        $txtName.FontSize = 13
        $txtName.FontWeight = [System.Windows.FontWeights]::SemiBold
        $txtName.Foreground = if ($isLight) { HexBrush "#0F172A" } else { HexBrush "#FFFFFF" }
        $titleStack.Children.Add($txtName) | Out-Null

        $badge = New-Object System.Windows.Controls.Border
        $badge.Background = if ($isLight) { HexBrush "#E2E8F0" } else { HexBrush "#262626" }
        $badge.CornerRadius = 4
        $badge.Margin = "8,0,0,0"
        $badge.Padding = "6,1"
        $txtBadge = New-Object System.Windows.Controls.TextBlock
        $txtBadge.Text = $app.category.ToUpper()
        $txtBadge.FontSize = 9
        $txtBadge.FontWeight = [System.Windows.FontWeights]::Bold
        $txtBadge.Foreground = if ($isLight) { HexBrush "#475569" } else { HexBrush "#8A8A8A" }
        $badge.Child = $txtBadge
        $titleStack.Children.Add($badge) | Out-Null
        $infoStack.Children.Add($titleStack) | Out-Null

        $txtDesc = New-Object System.Windows.Controls.TextBlock
        $txtDesc.Text = $app.description
        $txtDesc.FontSize = 11
        $txtDesc.Foreground = if ($isLight) { HexBrush "#64748B" } else { HexBrush "#8A8A8A" }
        $txtDesc.Margin = "0,3,0,0"
        $infoStack.Children.Add($txtDesc) | Out-Null
        [System.Windows.Controls.Grid]::SetColumn($infoStack, 1)
        $grid.Children.Add($infoStack) | Out-Null

        $statusStack = New-Object System.Windows.Controls.StackPanel
        $statusStack.Orientation = [System.Windows.Controls.Orientation]::Horizontal
        $statusStack.VerticalAlignment = [System.Windows.VerticalAlignment]::Center

        $statusIcon = New-Object System.Windows.Controls.TextBlock
        $statusIcon.Text = "⚪"
        $statusIcon.FontSize = 11
        $statusIcon.Margin = "0,0,6,0"
        $statusStack.Children.Add($statusIcon) | Out-Null

        $statusText = New-Object System.Windows.Controls.TextBlock
        $statusText.Text = "Ready"
        $statusText.FontSize = 11
        $statusText.FontWeight = [System.Windows.FontWeights]::SemiBold
        $statusText.Foreground = if ($isLight) { HexBrush "#64748B" } else { HexBrush "#707070" }
        $statusStack.Children.Add($statusText) | Out-Null
        [System.Windows.Controls.Grid]::SetColumn($statusStack, 2)
        $grid.Children.Add($statusStack) | Out-Null

        $card.Child = $grid
        $AppsContainer.Children.Add($card) | Out-Null

        $Script:AppRows.Add([PSCustomObject]@{
            App = $app
            Card = $card
            CheckBox = $cb
            StatusIcon = $statusIcon
            StatusText = $statusText
            Name = $app.name
            Category = $app.category
            Description = $app.description
            Id = $app.id
        })
    }
}

function Set-CategoryFilter([string]$cat) {
    foreach ($child in $CategoryChipsPanel.Children) {
        if ($child.Tag -eq $cat) {
            $child.Background = HexBrush "#0078D4"
            $child.Foreground = HexBrush "#FFFFFF"
        } else {
            $child.Background = if ($Script:CurrentThemeIsLight) { HexBrush "#E2E8F0" } else { HexBrush "#2A2A2A" }
            $child.Foreground = if ($Script:CurrentThemeIsLight) { HexBrush "#475569" } else { HexBrush "#A0A0A0" }
        }
    }
    Apply-Filters $cat
}

function Apply-Filters([string]$cat = "all") {
    $q = $TxtSearch.Text.Trim().ToLower()
    if ($q -eq "search applications...") { $q = "" }
    foreach ($row in $Script:AppRows) {
        $matchCat = ($cat -eq "all") -or ($row.Category -eq $cat)
        $matchSearch = ($q -eq "") -or
                       ($row.Name.ToLower().Contains($q)) -or
                       ($row.Description.ToLower().Contains($q)) -or
                       ($row.Id.ToLower().Contains($q))
        if ($matchCat -and $matchSearch) { $row.Card.Visibility = [System.Windows.Visibility]::Visible }
        else { $row.Card.Visibility = [System.Windows.Visibility]::Collapsed }
    }
}

function Update-Count {
    $sel = ($Script:AppRows | Where-Object { $_.CheckBox.IsChecked -eq $true }).Count
    $TxtCount.Text = "Selected: $sel of $($Script:AppRows.Count) apps"
}

$TxtSearch.Add_GotFocus({
    if ($TxtSearch.Text -eq "Search applications...") {
        $TxtSearch.Text = ""
        $TxtSearch.Foreground = if ($Script:CurrentThemeIsLight) { HexBrush "#0F172A" } else { HexBrush "#FFFFFF" }
    }
})
$TxtSearch.Add_LostFocus({
    if ([string]::IsNullOrWhiteSpace($TxtSearch.Text)) {
        $TxtSearch.Text = "Search applications..."
        $TxtSearch.Foreground = if ($Script:CurrentThemeIsLight) { HexBrush "#94A3B8" } else { HexBrush "#707070" }
    }
})
$TxtSearch.Add_TextChanged({ Apply-Filters })

$BtnOpenOfflineFolder.Add_Click({
    $offlinePath = Join-Path $ScriptDir "offline_installers"
    if (-not (Test-Path $offlinePath)) { New-Item -ItemType Directory -Path $offlinePath -Force | Out-Null }
    Start-Process "explorer.exe" $offlinePath
})
$BtnOpenLogFile.Add_Click({
    if (-not (Test-Path $LogFilePath)) { New-Item -ItemType File -Path $LogFilePath -Force | Out-Null }
    Start-Process "notepad.exe" $LogFilePath
})
$BtnClearLogConsole.Add_Click({ $TxtLog.Text = "" })

# Installation Engine
$Script:IsCancelled = $false
$Script:CurrentProcess = $null

$BtnStop.Add_Click({
    $Script:IsCancelled = $true
    $BtnStop.IsEnabled = $false
    $BtnStop.Content = "Stopping..."
    Append-Log "[CANCEL] Stop requested by user. Terminating active setup process..."
    if ($Script:CurrentProcess) {
        try {
            taskkill /PID $Script:CurrentProcess.Id /T /F 2>&1 | Out-Null
        } catch {}
    }
})

$BtnInstall.Add_Click({
    $selectedRows = @($Script:AppRows | Where-Object { $_.CheckBox.IsChecked -eq $true })
    if ($selectedRows.Count -eq 0) {
        [System.Windows.MessageBox]::Show("Please select a role environment or check at least one software package.", "No Software Selected", [System.Windows.MessageBoxButton]::OK, [System.Windows.MessageBoxImage]::Information)
        return
    }

    $res = [System.Windows.MessageBox]::Show("Ready to configure machine with $($selectedRows.Count) selected application(s).`n`nDo you want to begin now?", "Start Machine Setup", [System.Windows.MessageBoxButton]::YesNo, [System.Windows.MessageBoxImage]::Question)
    if ($res -ne [System.Windows.MessageBoxResult]::Yes) { return }

    Switch-View "logs"

    $Script:IsCancelled = $false
    $Script:CurrentProcess = $null
    $BtnInstall.Visibility = [System.Windows.Visibility]::Collapsed
    $BtnStop.Visibility = [System.Windows.Visibility]::Visible
    $BtnStop.IsEnabled = $true
    $BtnStop.Content = "⏹️ Stop Setup"

    $BtnCreateNewGroup.IsEnabled = $false
    foreach ($rc in $Script:RoleCardViews) {
        $rc.SelectButton.IsEnabled = $false
        $rc.EditButton.IsEnabled = $false
    }

    $ProgressBar.Value = 0
    $ProgressBar.Maximum = $selectedRows.Count
    $success = 0
    $failed = 0

    Append-Log "=========================================================="
    Append-Log "Innovana Pre Installer Setup Started: $($selectedRows.Count) packages"
    Append-Log "Timestamp: $(Get-Date)"
    Append-Log "=========================================================="

    for ($i = 0; $i -lt $selectedRows.Count; $i++) {
        if ($Script:IsCancelled) {
            Append-Log "[STOPPED] Installation process stopped by user."
            break
        }

        $item = $selectedRows[$i]
        $app = $item.App

        $item.StatusIcon.Text = "⏳"
        $item.StatusText.Text = "Installing..."
        $item.StatusText.Foreground = HexBrush "#0078D4"
        $TxtStatus.Text = "[$($i + 1)/$($selectedRows.Count)] Installing $($app.name)..."
        DoEvents

        Append-Log "[$($i + 1)/$($selectedRows.Count)] Processing: $($app.name) ($($app.id))"

        $offlineInstalled = $false
        $offlineDir = Join-Path $ScriptDir "offline_installers"
        if ($app.offlineInstaller -and (Test-Path $offlineDir)) {
            $offlineFile = Join-Path $offlineDir $app.offlineInstaller
            if (Test-Path $offlineFile) {
                Append-Log "Found offline setup: $offlineFile"
                try {
                    $p = Start-Process -FilePath $offlineFile -ArgumentList $app.silentArgs -PassThru -NoNewWindow
                    $Script:CurrentProcess = $p
                    $p.WaitForExit()
                    $Script:CurrentProcess = $null
                    if ($p.ExitCode -eq 0 -or $p.ExitCode -eq 3010) {
                        Append-Log "Offline setup completed successfully."
                        $offlineInstalled = $true
                    }
                } catch {
                    Append-Log "Offline setup error: $_"
                } finally {
                    $Script:CurrentProcess = $null
                }
            }
        }

        $installed = $offlineInstalled

        if (-not $offlineInstalled -and -not $Script:IsCancelled) {
            if (-not $WingetCmd) {
                Append-Log "ERROR: Winget not available."
                $installed = $false
            } else {
                Append-Log "Running winget install for $($app.id)..."
                $wingetArgs = "install --id `"$($app.id)`" -e --silent --accept-package-agreements --accept-source-agreements --disable-interactivity"

                try {
                    $psi = New-Object System.Diagnostics.ProcessStartInfo
                    $psi.FileName = $WingetCmd
                    $psi.Arguments = $wingetArgs
                    $psi.RedirectStandardOutput = $true
                    $psi.RedirectStandardError = $true
                    $psi.UseShellExecute = $false
                    $psi.CreateNoWindow = $true

                    $proc = [System.Diagnostics.Process]::Start($psi)
                    $Script:CurrentProcess = $proc

                    while (-not $proc.HasExited -and -not $Script:IsCancelled) {
                        $line = $proc.StandardOutput.ReadLine()
                        if ($line) { Append-Log "  > $line" }
                        DoEvents
                    }

                    if (-not $Script:IsCancelled) {
                        $rest = $proc.StandardOutput.ReadToEnd()
                        if ($rest) {
                            foreach ($l in ($rest -split "`r?`n")) {
                                if ($l.Trim()) { Append-Log "  > $l" }
                            }
                        }
                    }

                    $code = $proc.ExitCode
                    Append-Log "Winget finished with ExitCode: $code"

                    if ($code -eq 0 -or $code -eq 3010 -or $code -eq -1978335189 -or $code -eq 2316632107) {
                        $installed = $true
                    } else {
                        $err = $proc.StandardError.ReadToEnd()
                        if ($err) { Append-Log "  [STDERR] $err" }
                        $installed = $false
                    }
                } catch {
                    Append-Log "Winget exception: $_"
                    $installed = $false
                } finally {
                    $Script:CurrentProcess = $null
                }
            }
        }

        if ($installed) {
            $item.StatusIcon.Text = "✅"
            $item.StatusText.Text = "Installed"
            $item.StatusText.Foreground = HexBrush "#10B981"
            $success++
        } else {
            $item.StatusIcon.Text = if ($Script:IsCancelled) { "⏹️" } else { "❌" }
            $item.StatusText.Text = if ($Script:IsCancelled) { "Cancelled" } else { "Failed" }
            $statusColor = if ($Script:IsCancelled) { "#F59E0B" } else { "#F87171" }
            $item.StatusText.Foreground = HexBrush $statusColor
            $failed++
        }

        $ProgressBar.Value = $i + 1
        DoEvents

        if ($Script:IsCancelled) {
            Append-Log "[STOPPED] Machine setup cancelled by user."
            break
        }
    }

    Append-Log "=========================================================="
    Append-Log "Setup Ended! Succeeded: $success, Failed/Cancelled: $failed"
    Append-Log "Log file saved to: $LogFilePath"
    Append-Log "=========================================================="

    $BtnStop.Visibility = [System.Windows.Visibility]::Collapsed
    $BtnInstall.Visibility = [System.Windows.Visibility]::Visible
    $BtnInstall.IsEnabled = $true
    $BtnCreateNewGroup.IsEnabled = $true
    foreach ($rc in $Script:RoleCardViews) {
        $rc.SelectButton.IsEnabled = $true
        $rc.EditButton.IsEnabled = $true
    }

    if ($Script:IsCancelled) {
        $rem = $selectedRows.Count - $success - $failed
        $TxtStatus.Text = "Setup Stopped ($success Installed, $rem Remaining)"
        [System.Windows.MessageBox]::Show("Machine Setup was stopped by user.`n`nInstalled: $success`nFailed/Cancelled: $failed`nRemaining: $rem", "Setup Stopped", [System.Windows.MessageBoxButton]::OK, [System.Windows.MessageBoxImage]::Warning)
    } else {
        $TxtStatus.Text = "Setup Complete! ($success Succeeded, $failed Failed)"
        [System.Windows.MessageBox]::Show("Machine Setup Finished!`n`nSucceeded: $success`nFailed: $failed`n`nA complete log has been saved to:`n$LogFilePath", "Setup Complete", [System.Windows.MessageBoxButton]::OK, [System.Windows.MessageBoxImage]::Information)
    }
})

# System Theme Watcher Registration
try {
    $themeWatcher = {
        param($s, $e)
        if ($e.Category -eq [Microsoft.Win32.UserPreferenceCategory]::General -or $e.Category -eq [Microsoft.Win32.UserPreferenceCategory]::Color) {
            if ($Script:Settings.theme -eq "system") {
                $window.Dispatcher.Invoke([System.Action]{
                    Apply-Theme (Get-CurrentThemeIsLight)
                })
            }
        }
    }
    [Microsoft.Win32.SystemEvents]::add_UserPreferenceChanged($themeWatcher)
    $window.Add_Closed({
        try { [Microsoft.Win32.SystemEvents]::remove_UserPreferenceChanged($themeWatcher) } catch {}
    })
} catch {}

# Initial Theme & View Application
Build-EmojiChips
Apply-Theme (Get-CurrentThemeIsLight)

$window.ShowDialog() | Out-Null
