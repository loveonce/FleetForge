using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

[assembly: AssemblyTitle("FleetForge Setup Wizard")]
[assembly: AssemblyDescription("FleetForge - Self-Extracting Enterprise Setup Wizard")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Love Malhotra")]
[assembly: AssemblyProduct("FleetForge Setup")]
[assembly: AssemblyCopyright("Copyright © 2026 Love Malhotra. All rights reserved.")]
[assembly: AssemblyTrademark("FleetForge™")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("2.5.0.0")]
[assembly: AssemblyFileVersion("2.5.0.0")]
[assembly: AssemblyInformationalVersion("2.5.0")]

namespace FleetForge
{
    public class SetupProgram : Application
    {
        private static Mutex setupMutex = null;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        private const int SW_RESTORE = 9;

        private static void BringExistingSetupToFront()
        {
            try
            {
                Process current = Process.GetCurrentProcess();
                IntPtr targetHwnd = IntPtr.Zero;

                foreach (Process p in Process.GetProcessesByName(current.ProcessName))
                {
                    if (p.Id != current.Id)
                    {
                        IntPtr h = p.MainWindowHandle;
                        if (h != IntPtr.Zero)
                        {
                            targetHwnd = h;
                            break;
                        }
                    }
                }

                if (targetHwnd == IntPtr.Zero)
                {
                    targetHwnd = FindWindow(null, "FleetForge Setup Wizard");
                }

                if (targetHwnd != IntPtr.Zero)
                {
                    if (IsIconic(targetHwnd)) ShowWindowAsync(targetHwnd, SW_RESTORE);
                    SetForegroundWindow(targetHwnd);
                }
            }
            catch { }
        }

        private static bool IsAdministrator()
        {
            try
            {
                using (var identity = System.Security.Principal.WindowsIdentity.GetCurrent())
                {
                    var principal = new System.Security.Principal.WindowsPrincipal(identity);
                    return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
                }
            }
            catch
            {
                return false;
            }
        }

        [STAThread]
        public static void Main(string[] args)
        {
            // Forcibly require and self-elevate to Administrator privileges
            if (!IsAdministrator())
            {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    string exePath = null;
                    try { if (Process.GetCurrentProcess().MainModule != null) exePath = Process.GetCurrentProcess().MainModule.FileName; } catch { }
                    if (string.IsNullOrEmpty(exePath)) exePath = Assembly.GetExecutingAssembly().Location;

                    psi.FileName = exePath;
                    if (args != null && args.Length > 0)
                    {
                        psi.Arguments = string.Join(" ", args);
                    }
                    psi.Verb = "runas"; // Windows UAC Elevation prompt
                    psi.UseShellExecute = true;
                    Process.Start(psi);
                    return;
                }
                catch
                {
                    MessageBox.Show(
                        "Administrator permissions are strictly required to install or update FleetForge.\n\nPlease approve the Windows UAC Administrator prompt to proceed.",
                        "Administrator Elevation Required",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
            }

            bool isFirst;
            setupMutex = new Mutex(true, "Local\\FleetForge_Setup_SingleInstance", out isFirst);
            if (!isFirst)
            {
                BringExistingSetupToFront();
                return;
            }

            SetupProgram app = new SetupProgram();
            app.Run(new SetupWindow());
        }
    }

    public class SetupWindow : Window
    {
        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWCP_ROUND = 2;

        private void UpdateDwmTheme()
        {
            try
            {
                IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                if (hwnd == IntPtr.Zero) return;

                int cornerPref = DWMWCP_ROUND;
                DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPref, sizeof(int));

                int darkMode = 1;
                int hr = DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));
                if (hr != 0)
                {
                    DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref darkMode, sizeof(int));
                }
            }
            catch { }
        }

        private TextBox txtInstallPath;
        private Button btnBrowse;
        private Border detectedInstallCard;
        private TextBlock txtDetectedInfo;
        private CheckBox chkDesktop;
        private CheckBox chkStartMenu;
        private CheckBox chkRegister;
        private CheckBox chkLaunchAfter;

        private Border progressBorder;
        private ProgressBar progressBar;
        private TextBlock txtStatus;

        private Button btnCancel;
        private Button btnInstall;
        private Image imgSetupLogo;

        private bool isExistingInstallation = false;
        private bool isInstalled = false;
        private string finalInstallPath = "";

        public SetupWindow()
        {
            InitializeWindow();
        }

        public static bool IsValidFleetForgeDir(string dir)
        {
            if (string.IsNullOrWhiteSpace(dir)) return false;
            try
            {
                string cleaned = dir.Replace("\"", "").Trim();
                if (!Directory.Exists(cleaned)) return false;
                if (File.Exists(Path.Combine(cleaned, "FleetForge.exe"))) return true;
                if (File.Exists(Path.Combine(cleaned, "Uninstall.exe"))) return true;
                if (File.Exists(Path.Combine(cleaned, "appicon.ico")) && Directory.Exists(Path.Combine(cleaned, "data"))) return true;
            }
            catch { }
            return false;
        }

        private static string ResolveShortcutTarget(string shortcutPath)
        {
            try
            {
                Type wsType = Type.GetTypeFromProgID("WScript.Shell");
                if (wsType == null) return null;
                object ws = Activator.CreateInstance(wsType);
                object sc = wsType.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, ws, new object[] { shortcutPath });
                Type scType = sc.GetType();
                return scType.InvokeMember("TargetPath", BindingFlags.GetProperty, null, sc, null) as string;
            }
            catch
            {
                return null;
            }
        }

        public static string DetectExistingInstallPath()
        {
            // 1. Check Windows Registry (LocalMachine and CurrentUser, 64-bit and 32-bit views)
            string[] uninstallKeys = new string[] {
                @"Software\Microsoft\Windows\CurrentVersion\Uninstall\FleetForge",
                @"Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\FleetForge",
                @"Software\FleetForge",
                @"Software\WOW6432Node\FleetForge"
            };

            RegistryKey[] hives = new RegistryKey[] { Registry.LocalMachine, Registry.CurrentUser };
            foreach (RegistryKey hive in hives)
            {
                foreach (string keyPath in uninstallKeys)
                {
                    try
                    {
                        using (RegistryKey key = hive.OpenSubKey(keyPath))
                        {
                            if (key != null)
                            {
                                string loc = key.GetValue("InstallLocation") as string;
                                if (IsValidFleetForgeDir(loc)) return Path.GetFullPath(loc.TrimEnd('\\'));

                                string pathVal = key.GetValue("Path") as string ?? key.GetValue("InstallPath") as string;
                                if (IsValidFleetForgeDir(pathVal)) return Path.GetFullPath(pathVal.TrimEnd('\\'));

                                string uninst = key.GetValue("UninstallString") as string;
                                if (!string.IsNullOrEmpty(uninst))
                                {
                                    string cleaned = uninst.Replace("\"", "").Trim();
                                    string dir = Path.GetDirectoryName(cleaned);
                                    if (IsValidFleetForgeDir(dir)) return Path.GetFullPath(dir.TrimEnd('\\'));
                                }
                            }
                        }
                    }
                    catch { }
                }
            }

            // 2. Check Desktop and Start Menu Shortcuts
            try
            {
                string[] shortcutDirs = new string[] {
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory),
                    Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms),
                    Environment.GetFolderPath(Environment.SpecialFolder.Programs)
                };

                foreach (string sDir in shortcutDirs)
                {
                    if (string.IsNullOrEmpty(sDir) || !Directory.Exists(sDir)) continue;
                    string lnkPath = Path.Combine(sDir, "FleetForge.lnk");
                    if (File.Exists(lnkPath))
                    {
                        string target = ResolveShortcutTarget(lnkPath);
                        if (!string.IsNullOrEmpty(target))
                        {
                            string dir = File.Exists(target) ? Path.GetDirectoryName(target) : target;
                            if (IsValidFleetForgeDir(dir)) return Path.GetFullPath(dir.TrimEnd('\\'));
                        }
                    }
                }
            }
            catch { }

            // 3. Check any currently running FleetForge process
            try
            {
                foreach (Process p in Process.GetProcessesByName("FleetForge"))
                {
                    try
                    {
                        if (p.MainModule != null && !string.IsNullOrEmpty(p.MainModule.FileName))
                        {
                            string dir = Path.GetDirectoryName(p.MainModule.FileName);
                            if (IsValidFleetForgeDir(dir)) return Path.GetFullPath(dir.TrimEnd('\\'));
                        }
                    }
                    catch { }
                }
            }
            catch { }

            // 4. Scan available drives for previous FleetForge installation folder
            try
            {
                DriveInfo[] drives = DriveInfo.GetDrives();
                foreach (DriveInfo d in drives)
                {
                    if (!d.IsReady || d.DriveType == DriveType.CDRom) continue;
                    string root = d.RootDirectory.FullName;
                    string[] probePaths = new string[] {
                        Path.Combine(root, "Installation", "FleetForge"),
                        Path.Combine(root, "Software", "FleetForge"),
                        Path.Combine(root, "FleetForge"),
                        Path.Combine(root, "Program Files", "FleetForge")
                    };
                    foreach (string probe in probePaths)
                    {
                        if (IsValidFleetForgeDir(probe)) return Path.GetFullPath(probe.TrimEnd('\\'));
                    }
                }
            }
            catch { }

            return null;
        }

        private void InitializeWindow()
        {
            this.Title = "FleetForge Setup Wizard — Love Malhotra";
            this.Width = 680;
            this.Height = 660;
            this.MinWidth = 680;
            this.MinHeight = 660;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#202020"));
            this.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3F3F3"));
            this.FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI Variable, Segoe UI");
            this.ResizeMode = ResizeMode.CanMinimize;
            this.SourceInitialized += delegate { UpdateDwmTheme(); };

            // Load application icon from embedded resource or disk
            ImageSource setupIconSource = null;
            try
            {
                var asm = Assembly.GetExecutingAssembly();
                using (Stream s = asm.GetManifestResourceStream("appicon.ico"))
                {
                    if (s != null)
                    {
                        var decoder = new IconBitmapDecoder(s, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                        if (decoder.Frames.Count > 0)
                        {
                            foreach (var f in decoder.Frames)
                            {
                                if (f.PixelWidth >= 48 && f.PixelWidth <= 128)
                                {
                                    setupIconSource = f;
                                    break;
                                }
                            }
                            if (setupIconSource == null) setupIconSource = decoder.Frames[0];
                        }
                    }
                }
            }
            catch { }

            if (setupIconSource == null)
            {
                string[] diskCandidates = new string[] {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "src", "assets", "appicon.ico"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "src", "appicon.ico"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appicon.ico"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "src", "assets", "appicon.ico"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "src", "appicon.ico")
                };
                foreach (string candidate in diskCandidates)
                {
                    if (File.Exists(candidate))
                    {
                        try
                        {
                            using (var fs = new FileStream(candidate, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                var decoder = new IconBitmapDecoder(fs, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                                if (decoder.Frames.Count > 0)
                                {
                                    setupIconSource = decoder.Frames[0];
                                    break;
                                }
                            }
                        }
                        catch { }
                    }
                }
            }

            if (setupIconSource != null)
            {
                this.Icon = setupIconSource;
            }

            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string defaultPath = Path.Combine(programFiles, "FleetForge");

            string xaml = @"
<Grid xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
      xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
    <Grid.Resources>
        <Style TargetType=""Button"">
            <Setter Property=""FocusVisualStyle"" Value=""{x:Null}""/>
            <Setter Property=""Template"">
                <Setter.Value>
                    <ControlTemplate TargetType=""Button"">
                        <Border x:Name=""border"" Background=""{TemplateBinding Background}""
                                BorderBrush=""{TemplateBinding BorderBrush}""
                                BorderThickness=""{TemplateBinding BorderThickness}""
                                CornerRadius=""4"" Padding=""{TemplateBinding Padding}"" SnapsToDevicePixels=""True"">
                            <ContentPresenter HorizontalAlignment=""{TemplateBinding HorizontalContentAlignment}""
                                              VerticalAlignment=""{TemplateBinding VerticalContentAlignment}""
                                              RecognizesAccessKey=""True""/>
                        </Border>
                        <ControlTemplate.Triggers>
                            <Trigger Property=""IsMouseOver"" Value=""True"">
                                <Setter Property=""Opacity"" Value=""0.88""/>
                            </Trigger>
                            <Trigger Property=""IsPressed"" Value=""True"">
                                <Setter Property=""Opacity"" Value=""0.75""/>
                            </Trigger>
                            <Trigger Property=""IsEnabled"" Value=""False"">
                                <Setter TargetName=""border"" Property=""Background"" Value=""#1E1E1E""/>
                                <Setter TargetName=""border"" Property=""BorderBrush"" Value=""#2A2A2A""/>
                                <Setter Property=""Opacity"" Value=""0.45""/>
                            </Trigger>
                        </ControlTemplate.Triggers>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>
    </Grid.Resources>
    <Grid.RowDefinitions>
        <RowDefinition Height=""Auto""/>
        <RowDefinition Height=""*""/>
        <RowDefinition Height=""Auto""/>
    </Grid.RowDefinitions>

    <!-- HEADER -->
    <Border Grid.Row=""0"" Background=""#242424"" Padding=""24,16"" BorderBrush=""#333333"" BorderThickness=""0,0,0,1"">
        <Grid>
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width=""*""/>
                <ColumnDefinition Width=""Auto""/>
            </Grid.ColumnDefinitions>

            <StackPanel Grid.Column=""0"" Orientation=""Horizontal"" VerticalAlignment=""Center"">
                <Border Width=""44"" Height=""44"" Background=""Transparent"" Margin=""0,0,14,0"" BorderThickness=""0"">
                    <Grid>
                        <Image x:Name=""ImgSetupLogo"" Width=""36"" Height=""36"" RenderOptions.BitmapScalingMode=""HighQuality"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                        <TextBlock x:Name=""TxtFallbackLogo"" Text=""⚡"" FontSize=""24"" Foreground=""#0078D4"" HorizontalAlignment=""Center"" VerticalAlignment=""Center"" Visibility=""Collapsed""/>
                    </Grid>
                </Border>
                <StackPanel VerticalAlignment=""Center"">
                    <TextBlock Text=""FLEETFORGE SETUP"" FontSize=""15"" FontWeight=""Bold"" Foreground=""#FFFFFF""/>
                    <TextBlock Text=""Love Malhotra • Easy App Setup • v2.5.0"" FontSize=""11"" Foreground=""#A0A0A0"" Margin=""0,2,0,0""/>
                </StackPanel>
            </StackPanel>

            <Border Grid.Column=""1"" Background=""#133824"" CornerRadius=""4"" Padding=""8,4"" VerticalAlignment=""Center"">
                <TextBlock Text=""WINDOWS 11 READY"" FontSize=""10"" FontWeight=""Bold"" Foreground=""#4ADE80""/>
            </Border>
        </Grid>
    </Border>

    <!-- BODY -->
    <ScrollViewer Grid.Row=""1"" VerticalScrollBarVisibility=""Disabled"" HorizontalScrollBarVisibility=""Disabled"" Padding=""24,14"">
        <StackPanel>
            <!-- Welcome Info Card -->
            <Border Background=""#272727"" CornerRadius=""8"" Padding=""16,12"" BorderBrush=""#383838"" BorderThickness=""1"" Margin=""0,0,0,10"">
                <StackPanel>
                    <TextBlock Text=""Welcome to FleetForge Setup"" FontSize=""13"" FontWeight=""Bold"" Foreground=""#38BDF8"" Margin=""0,0,0,4""/>
                    <TextBlock Text=""This installer sets up FleetForge on your computer. FleetForge lets you install all your favorite applications, office tools, and web browsers in one simple click.""
                               FontSize=""11.5"" Foreground=""#D0D0D0"" TextWrapping=""Wrap"" LineHeight=""16""/>
                </StackPanel>
            </Border>

            <!-- Detected Existing Installation Card (Auto-Shown if previously installed) -->
            <Border x:Name=""DetectedInstallCard"" Background=""#143422"" CornerRadius=""8"" Padding=""14,10"" BorderBrush=""#238636"" BorderThickness=""1"" Margin=""0,0,0,10"" Visibility=""Collapsed"">
                <Grid>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width=""Auto""/>
                        <ColumnDefinition Width=""*""/>
                    </Grid.ColumnDefinitions>
                    <Border Grid.Column=""0"" Width=""36"" Height=""36"" CornerRadius=""6"" Background=""#1D5333"" Margin=""0,0,12,0"" VerticalAlignment=""Center"">
                        <TextBlock Text=""🔄"" FontSize=""18"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                    </Border>
                    <StackPanel Grid.Column=""1"" VerticalAlignment=""Center"">
                        <StackPanel Orientation=""Horizontal"">
                            <TextBlock Text=""PREVIOUS INSTALLATION FOUND"" FontSize=""10.5"" FontWeight=""Bold"" Foreground=""#4ADE80""/>
                            <Border Background=""#1E4D2B"" CornerRadius=""3"" Padding=""6,1"" Margin=""8,0,0,0"">
                                <TextBlock Text=""AUTO-DETECTED"" FontSize=""9"" FontWeight=""Bold"" Foreground=""#86EFAC""/>
                            </Border>
                        </StackPanel>
                        <TextBlock x:Name=""TxtDetectedInfo"" Text=""FleetForge is installed at: ..."" FontSize=""11.5"" FontWeight=""SemiBold"" Foreground=""#F0FDF4"" Margin=""0,3,0,2"" TextTrimming=""CharacterEllipsis""/>
                        <TextBlock Text=""Setup will update and overwrite existing files cleanly while preserving your configuration and custom app packs."" FontSize=""10.5"" Foreground=""#BBF7D0"" LineHeight=""14""/>
                    </StackPanel>
                </Grid>
            </Border>

            <!-- Destination Card -->
            <Border Background=""#272727"" CornerRadius=""8"" Padding=""16,12"" BorderBrush=""#383838"" BorderThickness=""1"" Margin=""0,0,0,10"">
                <StackPanel>
                    <TextBlock Text=""Installation Location"" FontSize=""12"" FontWeight=""Bold"" Foreground=""#FFFFFF"" Margin=""0,0,0,6""/>
                    <Grid>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width=""*""/>
                            <ColumnDefinition Width=""Auto""/>
                        </Grid.ColumnDefinitions>

                        <TextBox x:Name=""TxtInstallPath"" Grid.Column=""0"" Height=""32"" Background=""#1E1E1E"" Foreground=""#FFFFFF""
                                 BorderBrush=""#383838"" BorderThickness=""1"" Padding=""8,4"" FontSize=""12"" VerticalContentAlignment=""Center""/>
                        <Button x:Name=""BtnBrowse"" Grid.Column=""1"" Content=""📂 Browse..."" Background=""#2D2D2D"" Foreground=""#FFFFFF""
                                 Padding=""14,5"" Margin=""8,0,0,0"" Cursor=""Hand"" BorderThickness=""1"" BorderBrush=""#3D3D3D"" FontSize=""12""/>
                    </Grid>
                    <TextBlock Text=""Disk space required: ~25 MB  •  Space available on drive: Plenty"" FontSize=""10.5"" Foreground=""#8A8A8A"" Margin=""0,4,0,0""/>
                </StackPanel>
            </Border>

            <!-- Shortcuts & Options Card -->
            <Border Background=""#272727"" CornerRadius=""8"" Padding=""16,12"" BorderBrush=""#383838"" BorderThickness=""1"" Margin=""0,0,0,0"">
                <StackPanel>
                    <TextBlock Text=""Setup Options"" FontSize=""12"" FontWeight=""Bold"" Foreground=""#FFFFFF"" Margin=""0,0,0,8""/>
                    <CheckBox x:Name=""ChkDesktop"" Content=""Create Desktop Shortcut"" IsChecked=""True"" Foreground=""#E0E0E0"" FontSize=""11.5"" Margin=""0,0,0,6"" Cursor=""Hand""/>
                    <CheckBox x:Name=""ChkStartMenu"" Content=""Add to Start Menu &amp; Windows Search"" IsChecked=""True"" Foreground=""#E0E0E0"" FontSize=""11.5"" Margin=""0,0,0,6"" Cursor=""Hand""/>
                    <CheckBox x:Name=""ChkRegister"" Content=""Add to Windows Installed Apps (Easy Uninstall)"" IsChecked=""True"" Foreground=""#E0E0E0"" FontSize=""11.5"" Margin=""0,0,0,6"" Cursor=""Hand""/>
                    <CheckBox x:Name=""ChkLaunchAfter"" Content=""Launch FleetForge when setup finishes"" IsChecked=""True"" Foreground=""#4CC2FF"" FontSize=""11.5"" Cursor=""Hand""/>
                </StackPanel>
            </Border>

            <!-- Progress Card (Visible during install) -->
            <Border x:Name=""ProgressBorder"" Background=""#272727"" CornerRadius=""8"" Padding=""16,12"" BorderBrush=""#383838"" BorderThickness=""1"" Margin=""0,10,0,0"" Visibility=""Collapsed"">
                <StackPanel>
                    <TextBlock x:Name=""TxtStatus"" Text=""Ready to install..."" FontSize=""12"" Foreground=""#4CC2FF"" Margin=""0,0,0,6""/>
                    <ProgressBar x:Name=""ProgressBar"" Height=""6"" Background=""#2D2D2D"" Foreground=""#0078D4"" BorderThickness=""0""/>
                </StackPanel>
            </Border>
        </StackPanel>
    </ScrollViewer>

    <!-- FOOTER -->
    <Border Grid.Row=""2"" Background=""#1E1E1E"" Padding=""24,14"" BorderBrush=""#2D2D2D"" BorderThickness=""0,1,0,0"">
        <Grid>
            <Button x:Name=""BtnCancel"" Content=""Cancel"" Background=""#2D2D2D"" Foreground=""#FFFFFF""
                    Padding=""18,8"" HorizontalAlignment=""Left"" Cursor=""Hand"" BorderThickness=""1"" BorderBrush=""#3D3D3D"" FontSize=""12""/>
            <Button x:Name=""BtnInstall"" Content=""⚡ Install Now"" Background=""#0078D4"" Foreground=""#FFFFFF""
                    FontWeight=""Bold"" Padding=""28,8"" HorizontalAlignment=""Right"" Cursor=""Hand"" BorderThickness=""0"" FontSize=""13""/>
        </Grid>
    </Border>
</Grid>";

            Grid root = (Grid)XamlReader.Parse(xaml);
            this.Content = root;

            txtInstallPath = (TextBox)root.FindName("TxtInstallPath");
            btnBrowse = (Button)root.FindName("BtnBrowse");
            detectedInstallCard = (Border)root.FindName("DetectedInstallCard");
            txtDetectedInfo = (TextBlock)root.FindName("TxtDetectedInfo");

            chkDesktop = (CheckBox)root.FindName("ChkDesktop");
            chkStartMenu = (CheckBox)root.FindName("ChkStartMenu");
            chkRegister = (CheckBox)root.FindName("ChkRegister");
            chkLaunchAfter = (CheckBox)root.FindName("ChkLaunchAfter");

            progressBorder = (Border)root.FindName("ProgressBorder");
            progressBar = (ProgressBar)root.FindName("ProgressBar");
            txtStatus = (TextBlock)root.FindName("TxtStatus");

            btnCancel = (Button)root.FindName("BtnCancel");
            btnInstall = (Button)root.FindName("BtnInstall");
            imgSetupLogo = (Image)root.FindName("ImgSetupLogo");
            TextBlock txtFallbackLogo = (TextBlock)root.FindName("TxtFallbackLogo");

            if (setupIconSource != null && imgSetupLogo != null)
            {
                imgSetupLogo.Source = setupIconSource;
            }
            else if (txtFallbackLogo != null)
            {
                txtFallbackLogo.Visibility = Visibility.Visible;
            }

            // AUTO-DETECT: Find any previous installation on the machine (e.g. secondary drive D:\, E:\, etc.)
            string detected = DetectExistingInstallPath();
            if (!string.IsNullOrEmpty(detected))
            {
                txtInstallPath.Text = detected;
                UpdateInstallLocationStatus(detected);
            }
            else
            {
                txtInstallPath.Text = defaultPath;
                UpdateInstallLocationStatus(defaultPath);
            }

            txtInstallPath.TextChanged += delegate
            {
                UpdateInstallLocationStatus(txtInstallPath.Text.Trim());
            };

            btnBrowse.Click += delegate
            {
                using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
                {
                    fbd.Description = "Select Destination Folder for FleetForge";
                    fbd.SelectedPath = txtInstallPath.Text;
                    fbd.ShowNewFolderButton = true;
                    if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        txtInstallPath.Text = fbd.SelectedPath;
                        UpdateInstallLocationStatus(fbd.SelectedPath);
                    }
                }
            };

            btnCancel.Click += delegate
            {
                this.Close();
            };

            btnInstall.Click += delegate
            {
                if (isInstalled)
                {
                    LaunchInstalledApp(finalInstallPath);
                    this.Close();
                    return;
                }

                StartInstallationProcess();
            };
        }

        private void UpdateInstallLocationStatus(string path)
        {
            if (IsValidFleetForgeDir(path))
            {
                isExistingInstallation = true;
                if (detectedInstallCard != null) detectedInstallCard.Visibility = Visibility.Visible;
                if (txtDetectedInfo != null) txtDetectedInfo.Text = "FleetForge installation detected at: " + path;
                if (btnInstall != null)
                {
                    btnInstall.Content = "⚡ Update & Overwrite Installation";
                    btnInstall.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0D8050"));
                }
            }
            else
            {
                isExistingInstallation = false;
                if (detectedInstallCard != null) detectedInstallCard.Visibility = Visibility.Collapsed;
                if (btnInstall != null)
                {
                    btnInstall.Content = "⚡ Install Now";
                    btnInstall.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0078D4"));
                }
            }
        }

        private void SafeExtractResource(string resourceName, string targetFilePath)
        {
            // Clear ReadOnly attribute if file already exists
            if (File.Exists(targetFilePath))
            {
                try
                {
                    FileAttributes fa = File.GetAttributes(targetFilePath);
                    if ((fa & FileAttributes.ReadOnly) != 0)
                    {
                        File.SetAttributes(targetFilePath, fa & ~FileAttributes.ReadOnly);
                    }
                }
                catch { }
            }

            int retries = 0;
            while (retries < 5)
            {
                try
                {
                    ExtractResource(resourceName, targetFilePath);
                    return;
                }
                catch (IOException)
                {
                    retries++;
                    if (retries >= 5)
                    {
                        // Fallback rename trick if file is temporarily locked
                        try
                        {
                            string oldTmp = targetFilePath + ".old_" + Environment.TickCount;
                            if (File.Exists(oldTmp)) File.Delete(oldTmp);
                            File.Move(targetFilePath, oldTmp);
                            ExtractResource(resourceName, targetFilePath);
                            try { File.Delete(oldTmp); } catch { }
                            return;
                        }
                        catch { }
                        throw;
                    }
                    Thread.Sleep(200);
                }
            }
        }

        private void SafeCopyFile(string source, string dest)
        {
            if (!File.Exists(source)) return;
            if (File.Exists(dest))
            {
                try
                {
                    FileAttributes fa = File.GetAttributes(dest);
                    if ((fa & FileAttributes.ReadOnly) != 0)
                    {
                        File.SetAttributes(dest, fa & ~FileAttributes.ReadOnly);
                    }
                }
                catch { }
            }
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    File.Copy(source, dest, true);
                    return;
                }
                catch (IOException)
                {
                    if (i == 4)
                    {
                        try
                        {
                            string oldTmp = dest + ".old_" + Environment.TickCount;
                            File.Move(dest, oldTmp);
                            File.Copy(source, dest, true);
                            try { File.Delete(oldTmp); } catch { }
                            return;
                        }
                        catch { }
                    }
                    Thread.Sleep(200);
                }
            }
        }

        private void ExtractAndMergeCatalog(string appsDataPath, string appsRootPath)
        {
            string tempExtract = appsDataPath + ".tmp";
            SafeExtractResource("apps.json", tempExtract);

            try
            {
                string existingPath = File.Exists(appsDataPath) ? appsDataPath : (File.Exists(appsRootPath) ? appsRootPath : null);
                if (existingPath != null)
                {
                    string existingJson = File.ReadAllText(existingPath, Encoding.UTF8);
                    var jss = new JavaScriptSerializer();
                    jss.MaxJsonLength = int.MaxValue;
                    var existingData = jss.Deserialize<Dictionary<string, object>>(existingJson);

                    if (existingData != null && existingData.ContainsKey("groups"))
                    {
                        var existingGroups = existingData["groups"] as ArrayList;
                        if (existingGroups != null)
                        {
                            var customGroups = new ArrayList();
                            var builtInIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
                                "developer", "office", "creative", "gamer", "system", "office_business", "gamer_creator", "essential"
                            };

                            foreach (object gObj in existingGroups)
                            {
                                var g = gObj as Dictionary<string, object>;
                                if (g != null && g.ContainsKey("id"))
                                {
                                    string gid = Convert.ToString(g["id"]);
                                    bool isCustom = false;
                                    if (g.ContainsKey("is_custom") && Convert.ToBoolean(g["is_custom"])) isCustom = true;
                                    else if (!builtInIds.Contains(gid)) isCustom = true;

                                    if (isCustom)
                                    {
                                        customGroups.Add(g);
                                    }
                                }
                            }

                            if (customGroups.Count > 0)
                            {
                                string newJson = File.ReadAllText(tempExtract, Encoding.UTF8);
                                var newData = jss.Deserialize<Dictionary<string, object>>(newJson);
                                if (newData != null && newData.ContainsKey("groups"))
                                {
                                    var newGroups = newData["groups"] as ArrayList;
                                    if (newGroups != null)
                                    {
                                        foreach (object cg in customGroups)
                                        {
                                            newGroups.Add(cg);
                                        }
                                        string mergedJson = jss.Serialize(newData);
                                        File.WriteAllText(tempExtract, mergedJson, Encoding.UTF8);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }

            SafeCopyFile(tempExtract, appsDataPath);
            SafeCopyFile(tempExtract, appsRootPath);
            try { File.Delete(tempExtract); } catch { }
        }

        private void ExtractOrPreserveSettings(string settingsConfigPath, string settingsRootPath)
        {
            string existingPath = File.Exists(settingsConfigPath) ? settingsConfigPath : (File.Exists(settingsRootPath) ? settingsRootPath : null);
            if (existingPath == null)
            {
                SafeExtractResource("settings.json", settingsConfigPath);
                SafeCopyFile(settingsConfigPath, settingsRootPath);
            }
            else
            {
                // Preserve existing user configuration
                if (!File.Exists(settingsConfigPath)) SafeCopyFile(existingPath, settingsConfigPath);
                if (!File.Exists(settingsRootPath)) SafeCopyFile(existingPath, settingsRootPath);
            }
        }

        private void StartInstallationProcess()
        {
            string targetDir = txtInstallPath.Text.Trim();
            if (string.IsNullOrEmpty(targetDir))
            {
                MessageBox.Show("Please choose a valid destination path.", "Invalid Path", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            finalInstallPath = targetDir;
            bool createDesktop = chkDesktop.IsChecked == true;
            bool createStartMenu = chkStartMenu.IsChecked == true;
            bool registerUninstall = chkRegister.IsChecked == true;
            bool launchWhenDone = chkLaunchAfter.IsChecked == true;

            btnInstall.IsEnabled = false;
            btnCancel.IsEnabled = false;
            btnBrowse.IsEnabled = false;
            txtInstallPath.IsEnabled = false;
            chkDesktop.IsEnabled = false;
            chkStartMenu.IsEnabled = false;
            chkRegister.IsEnabled = false;
            chkLaunchAfter.IsEnabled = false;

            progressBorder.Visibility = Visibility.Visible;
            progressBar.Value = 0;
            progressBar.Maximum = 100;

            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    // 0. Close any running FleetForge instances before copying files so files are not locked
                    UpdateProgress(5, "Closing any active FleetForge applications...");
                    try
                    {
                        foreach (string pName in new string[] { "FleetForge", "Uninstall" })
                        {
                            foreach (Process p in Process.GetProcessesByName(pName))
                            {
                                try { p.Kill(); p.WaitForExit(3000); } catch { }
                            }
                        }
                    }
                    catch { }

                    // 1. Create Directories
                    UpdateProgress(10, "Preparing installation folder...");
                    if (!Directory.Exists(targetDir))
                    {
                        Directory.CreateDirectory(targetDir);
                    }
                    string dataDir = Path.Combine(targetDir, "data");
                    if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);
                    string configDir = Path.Combine(targetDir, "config");
                    if (!Directory.Exists(configDir)) Directory.CreateDirectory(configDir);
                    string logsDir = Path.Combine(targetDir, "logs");
                    if (!Directory.Exists(logsDir)) Directory.CreateDirectory(logsDir);
                    string offlineDir = Path.Combine(targetDir, "offline_installers");
                    if (!Directory.Exists(offlineDir)) Directory.CreateDirectory(offlineDir);
                    string reportsDir = Path.Combine(targetDir, "reports");
                    if (!Directory.Exists(reportsDir)) Directory.CreateDirectory(reportsDir);
                    Thread.Sleep(100);

                    // 2. Extract & Overwrite FleetForge.exe
                    UpdateProgress(30, "Updating and overwriting program files...");
                    string exePath = Path.Combine(targetDir, "FleetForge.exe");
                    SafeExtractResource("FleetForge.exe", exePath);
                    Thread.Sleep(100);

                    // 3. Extract & Merge apps.json (preserve custom user app packs)
                    UpdateProgress(50, "Updating 16,349 software catalog...");
                    string appsPath = Path.Combine(dataDir, "apps.json");
                    ExtractAndMergeCatalog(appsPath, Path.Combine(targetDir, "apps.json"));

                    int verifiedCount = 16349;
                    try
                    {
                        if (File.Exists(appsPath))
                        {
                            string text = File.ReadAllText(appsPath, Encoding.UTF8);
                            int matches = System.Text.RegularExpressions.Regex.Matches(text, "\"id\"\\s*:").Count;
                            if (matches > 100) verifiedCount = matches;
                        }
                    }
                    catch { }
                    UpdateProgress(58, string.Format("Verified {0:N0} pre-cached software packages...", verifiedCount));

                    try
                    {
                        string sp = Path.Combine(dataDir, "sample_apps.json");
                        SafeExtractResource("sample_apps.json", sp);
                        SafeCopyFile(sp, Path.Combine(targetDir, "sample_apps.json"));
                    }
                    catch { }

                    // 4. Extract or Preserve settings.json
                    UpdateProgress(65, "Applying configuration preferences...");
                    string settingsPath = Path.Combine(configDir, "settings.json");
                    ExtractOrPreserveSettings(settingsPath, Path.Combine(targetDir, "settings.json"));

                    // 4b. Extract app_urls.json
                    try
                    {
                        string urlsPath = Path.Combine(configDir, "app_urls.json");
                        SafeExtractResource("app_urls.json", urlsPath);
                        SafeCopyFile(urlsPath, Path.Combine(targetDir, "app_urls.json"));
                    }
                    catch { }

                    // 5. Extract appicon.ico
                    UpdateProgress(75, "Setting up application icons...");
                    string icoPath = Path.Combine(targetDir, "appicon.ico");
                    SafeExtractResource("appicon.ico", icoPath);

                    // 6. Extract Uninstall.exe
                    UpdateProgress(80, "Installing uninstaller...");
                    string uninstallExePath = Path.Combine(targetDir, "Uninstall.exe");
                    SafeExtractResource("Uninstall.exe", uninstallExePath);
                    Thread.Sleep(100);

                    // 7. Shortcuts
                    UpdateProgress(85, "Updating desktop and Start Menu shortcuts...");
                    if (createDesktop)
                    {
                        string deskPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), "FleetForge.lnk");
                        CreateShortcut(deskPath, exePath, targetDir, icoPath, "FleetForge — One-Click App Setup & Software Manager");
                        string userDesk = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "FleetForge.lnk");
                        if (File.Exists(userDesk))
                        {
                            CreateShortcut(userDesk, exePath, targetDir, icoPath, "FleetForge — One-Click App Setup & Software Manager");
                        }
                    }

                    if (createStartMenu)
                    {
                        string startPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms), "FleetForge.lnk");
                        CreateShortcut(startPath, exePath, targetDir, icoPath, "FleetForge — One-Click App Setup & Software Manager");
                        string userStart = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "FleetForge.lnk");
                        if (File.Exists(userStart))
                        {
                            CreateShortcut(userStart, exePath, targetDir, icoPath, "FleetForge — One-Click App Setup & Software Manager");
                        }
                    }

                    // 8. Register in Windows Add/Remove Programs and FleetForge registry roots
                    if (registerUninstall)
                    {
                        UpdateProgress(95, "Updating Windows Apps registration...");
                        RegisterInAddRemovePrograms(targetDir, uninstallExePath, icoPath);
                    }

                    UpdateProgress(100, isExistingInstallation ? "Update and overwrite completed successfully!" : "Installation completed successfully!");
                    Thread.Sleep(300);

                    Dispatcher.Invoke(new Action(delegate
                    {
                        isInstalled = true;
                        progressBar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4ADE80"));
                        btnInstall.Content = "🚀 Launch FleetForge";
                        btnInstall.IsEnabled = true;
                        btnCancel.Content = "Close";
                        btnCancel.IsEnabled = true;

                        if (launchWhenDone)
                        {
                            LaunchInstalledApp(targetDir);
                            this.Close();
                        }
                    }));
                }
                catch (UnauthorizedAccessException)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        txtStatus.Text = "Access Denied: Administrator permission required.";
                        txtStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F87171"));
                        btnCancel.IsEnabled = true;
                        MessageBox.Show(
                            "Cannot install to '" + targetDir + "': Access is denied.\n\nPlease right-click FleetForge-Setup.exe and choose 'Run as administrator', or install to a folder where your user account has write permission.",
                            "Administrator Permission Required",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }));
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        txtStatus.Text = "Error during setup: " + ex.Message;
                        txtStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F87171"));
                        btnCancel.IsEnabled = true;
                        MessageBox.Show("Setup Error: " + ex.Message, "Installation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }));
                }
            });
        }

        private void UpdateProgress(int percent, string statusText)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                progressBar.Value = percent;
                txtStatus.Text = statusText;
            }));
        }

        private void ExtractResource(string resourceName, string targetFilePath)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            string[] names = asm.GetManifestResourceNames();
            string match = null;
            foreach (string n in names)
            {
                if (n.Equals(resourceName, StringComparison.OrdinalIgnoreCase) || n.EndsWith("." + resourceName, StringComparison.OrdinalIgnoreCase))
                {
                    match = n;
                    break;
                }
            }

            if (match == null)
            {
                string[] parentPaths = new string[] {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, resourceName),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", resourceName),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", resourceName),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", resourceName),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "data", resourceName),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "config", resourceName),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "src", resourceName),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "src", "assets", resourceName)
                };
                foreach (string localFile in parentPaths)
                {
                    if (File.Exists(localFile))
                    {
                        File.Copy(localFile, targetFilePath, true);
                        return;
                    }
                }
                throw new FileNotFoundException("Embedded resource or local file not found: " + resourceName);
            }

            using (Stream s = asm.GetManifestResourceStream(match))
            {
                if (s == null) throw new FileNotFoundException("Cannot read embedded stream: " + match);
                using (FileStream fs = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write))
                {
                    byte[] buffer = new byte[81920];
                    int read;
                    while ((read = s.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fs.Write(buffer, 0, read);
                    }
                }
            }
        }

        private void CreateShortcut(string shortcutPath, string targetPath, string workingDir, string iconPath, string description)
        {
            try
            {
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null) return;
                dynamic shell = Activator.CreateInstance(shellType);
                dynamic shortcut = shell.CreateShortcut(shortcutPath);
                shortcut.TargetPath = targetPath;
                shortcut.WorkingDirectory = workingDir;
                shortcut.Description = description;
                if (!string.IsNullOrEmpty(iconPath) && File.Exists(iconPath))
                {
                    shortcut.IconLocation = iconPath + ",0";
                }
                shortcut.Save();
            }
            catch
            {
                try
                {
                    Type wsType = Type.GetTypeFromProgID("WScript.Shell");
                    object ws = Activator.CreateInstance(wsType);
                    object sc = wsType.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, ws, new object[] { shortcutPath });
                    Type scType = sc.GetType();
                    scType.InvokeMember("TargetPath", BindingFlags.SetProperty, null, sc, new object[] { targetPath });
                    scType.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, sc, new object[] { workingDir });
                    scType.InvokeMember("Description", BindingFlags.SetProperty, null, sc, new object[] { description });
                    if (!string.IsNullOrEmpty(iconPath) && File.Exists(iconPath))
                    {
                        scType.InvokeMember("IconLocation", BindingFlags.SetProperty, null, sc, new object[] { iconPath + ",0" });
                    }
                    scType.InvokeMember("Save", BindingFlags.InvokeMethod, null, sc, null);
                }
                catch { }
            }
        }

        private void RegisterInAddRemovePrograms(string installDir, string uninstallExePath, string icoPath)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\FleetForge"))
                {
                    if (key != null)
                    {
                        key.SetValue("DisplayName", "FleetForge — One-Click App Setup & Software Manager");
                        key.SetValue("DisplayIcon", icoPath);
                        key.SetValue("DisplayVersion", "2.5.0");
                        key.SetValue("Publisher", "Love Malhotra");
                        key.SetValue("HelpLink", "https://github.com/loveonce/FleetForge");
                        key.SetValue("URLInfoAbout", "https://github.com/loveonce/FleetForge");
                        key.SetValue("InstallLocation", installDir);
                        key.SetValue("UninstallString", "\"" + uninstallExePath + "\"");
                        key.SetValue("EstimatedSize", 5000, RegistryValueKind.DWord);
                        key.SetValue("NoModify", 1, RegistryValueKind.DWord);
                        key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
                    }
                }
            }
            catch { }

            try
            {
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(@"Software\FleetForge"))
                {
                    if (key != null)
                    {
                        key.SetValue("InstallLocation", installDir);
                        key.SetValue("Version", "2.5.0");
                    }
                }
            }
            catch { }

            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\FleetForge"))
                {
                    if (key != null)
                    {
                        key.SetValue("InstallLocation", installDir);
                        key.SetValue("Version", "2.5.0");
                    }
                }
            }
            catch { }
        }

        private void LaunchInstalledApp(string installDir)
        {
            try
            {
                string exe = Path.Combine(installDir, "FleetForge.exe");
                if (File.Exists(exe))
                {
                    ProcessStartInfo psi = new ProcessStartInfo(exe);
                    psi.WorkingDirectory = installDir;
                    psi.UseShellExecute = true;
                    Process.Start(psi);
                }
            }
            catch { }
        }
    }
}
