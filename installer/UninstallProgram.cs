using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using Microsoft.Win32;

[assembly: AssemblyTitle("FleetForge Uninstaller")]
[assembly: AssemblyDescription("FleetForge - Uninstallation Utility")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Love Malhotra")]
[assembly: AssemblyProduct("FleetForge")]
[assembly: AssemblyCopyright("Copyright © 2026 Love Malhotra. All rights reserved.")]
[assembly: AssemblyTrademark("FleetForge™")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("2.6.1.0")]
[assembly: AssemblyFileVersion("2.6.1.0")]
[assembly: AssemblyInformationalVersion("2.6.1")]

namespace FleetForge
{
    public class UninstallProgram
    {
        [STAThread]
        public static void Main(string[] args)
        {
            MessageBoxResult res = MessageBox.Show(
                "Are you sure you want to uninstall FleetForge from your computer?",
                "Uninstall FleetForge",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (res != MessageBoxResult.Yes) return;

            try
            {
                // Kill running processes
                string[] procNames = new string[] { "FleetForge", "InnovanaPreInstaller" };
                foreach (string pName in procNames)
                {
                    foreach (Process p in Process.GetProcessesByName(pName))
                    {
                        try { p.Kill(); } catch { }
                    }
                }

                string appDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\', '/');

                // Remove Desktop Shortcuts
                string[] deskShortcuts = new string[] {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "FleetForge.lnk"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), "FleetForge.lnk"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Innovana Pre Installer.lnk"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), "Innovana Pre Installer.lnk")
                };
                foreach (string s in deskShortcuts)
                {
                    if (File.Exists(s)) try { File.Delete(s); } catch { }
                }

                // Remove Start Menu Shortcuts
                string[] startShortcuts = new string[] {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "FleetForge.lnk"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms), "FleetForge.lnk"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "Innovana Pre Installer.lnk"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms), "Innovana Pre Installer.lnk")
                };
                foreach (string s in startShortcuts)
                {
                    if (File.Exists(s)) try { File.Delete(s); } catch { }
                }

                // Remove Registry Keys
                try { Registry.LocalMachine.DeleteSubKeyTree(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\FleetForge", false); } catch { }
                try { Registry.LocalMachine.DeleteSubKeyTree(@"Software\FleetForge", false); } catch { }
                try { Registry.CurrentUser.DeleteSubKeyTree(@"Software\FleetForge", false); } catch { }
                try { Registry.CurrentUser.DeleteSubKeyTree(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\FleetForge", false); } catch { }
                try { Registry.LocalMachine.DeleteSubKeyTree(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\InnovanaPreInstaller", false); } catch { }

                // Self-deleting cmd trick
                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", string.Format("/c timeout /t 1 /nobreak >nul & rmdir /s /q \"{0}\"", appDir));
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                Process.Start(psi);

                MessageBox.Show(
                    "FleetForge has been successfully uninstalled from this computer.",
                    "Uninstall Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Uninstall error: " + ex.Message, "Uninstall Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
