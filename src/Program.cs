using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

[assembly: AssemblyTitle("FleetForge")]
[assembly: AssemblyDescription("FleetForge - Zero-Touch Machine Setup & Enterprise Software Deployment")]
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
    // =========================================================================
    // Data Models
    // =========================================================================
    public class CategoryModel
    {
        public string id { get; set; }
        public string name { get; set; }
        public string icon { get; set; }
    }

    public class GroupModel
    {
        public string id { get; set; }
        public string name { get; set; }
        public string icon { get; set; }
        public string description { get; set; }
        public string accentColor { get; set; }
        public List<string> apps { get; set; }

        public GroupModel()
        {
            apps = new List<string>();
        }
    }

    public class AppModel
    {
        public string id { get; set; }
        public string name { get; set; }
        public string category { get; set; }
        public string description { get; set; }
        public string offlineInstaller { get; set; }
        public string silentArgs { get; set; }
    }

    public class PresetProfileItem
    {
        public string Id { get; set; }
        public string Icon { get; set; }
        public string Title { get; set; }
        public string Badge { get; set; }
        public string Desc { get; set; }
        public string Url { get; set; }

        public PresetProfileItem(string id, string icon, string title, string badge, string desc, string url)
        {
            Id = id;
            Icon = icon;
            Title = title;
            Badge = badge;
            Desc = desc;
            Url = url;
        }
    }

    public class ConfigModel
    {
        public List<GroupModel> groups { get; set; }
        public List<CategoryModel> categories { get; set; }
        public List<AppModel> apps { get; set; }

        public ConfigModel()
        {
            groups = new List<GroupModel>();
            categories = new List<CategoryModel>();
            apps = new List<AppModel>();
        }
    }

    public class SettingsModel
    {
        public string cloudConfigUrl { get; set; }
        public bool autoSyncOnLaunch { get; set; }
        public string lastSyncTime { get; set; }
        public string lastSyncStatus { get; set; }
        public string theme { get; set; }
        public bool silent { get; set; }
        public bool accept_agreements { get; set; }
        public string offline_dir { get; set; }
        public bool tutorialCompleted { get; set; }

        public SettingsModel()
        {
            cloudConfigUrl = AppUrls.CloudSyncUrl;
            autoSyncOnLaunch = true;
            lastSyncTime = "Never";
            lastSyncStatus = "Local Cache";
            theme = "system";
            silent = true;
            accept_agreements = true;
            offline_dir = "offline_installers";
            tutorialCompleted = false;
        }
    }

    public class AppRowView
    {
        public AppModel App { get; set; }
        public Border Card { get; set; }
        public CheckBox CheckBox { get; set; }
        public TextBlock StatusIcon { get; set; }
        public TextBlock StatusText { get; set; }
    }

    public class RoleCardView
    {
        public GroupModel Group { get; set; }
        public Border Card { get; set; }
        public Button SelectButton { get; set; }
        public Button EditButton { get; set; }
        public Button ExportButton { get; set; }
        public TextBlock CountText { get; set; }
    }

    public class ModalAppItemView
    {
        public AppModel App { get; set; }
        public Border Container { get; set; }
        public CheckBox CheckBox { get; set; }
        public Border StatusBadge { get; set; }
        public TextBlock StatusBadgeText { get; set; }
    }

    public class UpdateInfo
    {
        public string version { get; set; }
        public int versionCode { get; set; }
        public string buildDate { get; set; }
        public string title { get; set; }
        public string company { get; set; }
        public string downloadUrl { get; set; }
        public List<string> changelog { get; set; }

        public UpdateInfo()
        {
            version = "2.6.1";
            versionCode = 261;
            buildDate = "2026-11-13";
            title = "FleetForge v2.6.1 Enterprise IT Edition";
            company = "Love Malhotra";
            downloadUrl = AppUrls.UpdateDownloadUrl;
            changelog = new List<string>();
        }
    }

    public class AppDeploymentRecord
    {
        public string appId { get; set; }
        public string appName { get; set; }
        public string category { get; set; }
        public string status { get; set; } // "Installed", "Failed", "Timeout", "Cancelled", "Pending"
        public string installMethod { get; set; } // "Offline Installer", "Winget Silent"
        public int exitCode { get; set; }
        public double durationSeconds { get; set; }
        public string timestamp { get; set; }
        public string errorDetails { get; set; }

        public AppDeploymentRecord()
        {
            status = "Pending";
            installMethod = "Winget Silent";
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }

    public class DeploymentSessionState
    {
        public string sessionId { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        public string roleId { get; set; }
        public string roleName { get; set; }
        public bool isCompleted { get; set; }
        public int totalCount { get; set; }
        public int successCount { get; set; }
        public int failedCount { get; set; }
        public List<AppDeploymentRecord> records { get; set; }

        public DeploymentSessionState()
        {
            sessionId = Guid.NewGuid().ToString("N").Substring(0, 8);
            startTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            records = new List<AppDeploymentRecord>();
        }
    }

    public static class DeploymentReportGenerator
    {
        public static string GenerateHtmlReport(DeploymentSessionState session, string logFilePath, string customPath = null)
        {
            try
            {
                string reportsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "reports");
                if (!Directory.Exists(reportsDir)) Directory.CreateDirectory(reportsDir);

                string filePath = customPath;
                if (string.IsNullOrEmpty(filePath))
                {
                    string safeTime = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                    filePath = Path.Combine(reportsDir, string.Format("Deployment-Report-{0}.html", safeTime));
                }

                StringBuilder rows = new StringBuilder();
                int idx = 1;
                foreach (AppDeploymentRecord r in session.records)
                {
                    string badgeBg = "#064E3B";
                    string badgeFg = "#34D399";
                    string statusText = r.status.ToUpper();

                    if (r.status == "Failed")
                    {
                        badgeBg = "#7F1D1D";
                        badgeFg = "#F87171";
                    }
                    else if (r.status == "Timeout" || r.status == "Cancelled")
                    {
                        badgeBg = "#78350F";
                        badgeFg = "#FBBF24";
                    }

                    rows.Append("<tr>");
                    rows.Append(string.Format("<td>{0}</td>", idx++));
                    rows.Append(string.Format("<td><strong>{0}</strong></td>", System.Security.SecurityElement.Escape(r.appName ?? r.appId ?? "")));
                    rows.Append(string.Format("<td style=\"font-family:Consolas,monospace;font-size:12px;color:#94A3B8;\">{0}</td>", System.Security.SecurityElement.Escape(r.appId ?? "")));
                    rows.Append(string.Format("<td><span style=\"background:{0};color:{1};padding:3px 8px;border-radius:4px;font-weight:bold;font-size:11px;\">{2}</span></td>", badgeBg, badgeFg, statusText));
                    rows.Append(string.Format("<td>{0}</td>", System.Security.SecurityElement.Escape(r.installMethod ?? "Winget")));
                    rows.Append(string.Format("<td style=\"font-family:Consolas,monospace;\">{0}</td>", r.exitCode));
                    rows.Append(string.Format("<td>{0:F1}s</td>", r.durationSeconds));
                    rows.Append(string.Format("<td style=\"color:#94A3B8;font-size:11px;\">{0}</td>", System.Security.SecurityElement.Escape(r.timestamp ?? "")));
                    rows.Append("</tr>\n");
                }

                double successRate = session.totalCount > 0 ? ((double)session.successCount / session.totalCount) * 100.0 : 0.0;

                string html = string.Format(@"<!DOCTYPE html>
<html lang=""en"">
<head>
<meta charset=""UTF-8""/>
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0""/>
<title>FleetForge Deployment Report - {0}</title>
<style>
  * {{ box-sizing: border-box; margin: 0; padding: 0; }}
  body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI Variable Text', 'Segoe UI', Roboto, sans-serif; background: #12141A; color: #E2E8F0; padding: 32px 24px; line-height: 1.5; }}
  .container {{ max-width: 1040px; margin: 0 auto; }}
  .header {{ display: flex; justify-content: space-between; align-items: center; border-bottom: 1px solid #232938; padding-bottom: 20px; margin-bottom: 24px; }}
  .brand-title {{ font-size: 22px; font-weight: 800; color: #FFFFFF; letter-spacing: -0.5px; }}
  .brand-sub {{ font-size: 12px; color: #94A3B8; margin-top: 2px; }}
  .badge-org {{ background: #1E293B; border: 1px solid #334155; color: #38BDF8; font-weight: 600; font-size: 11px; padding: 6px 12px; border-radius: 6px; text-align: right; }}
  .stats-grid {{ display: grid; grid-template-columns: repeat(4, 1fr); gap: 14px; margin-bottom: 24px; }}
  .stat-card {{ background: #181D26; border: 1px solid #283142; border-radius: 8px; padding: 16px; text-align: center; }}
  .stat-val {{ font-size: 26px; font-weight: bold; margin-bottom: 4px; }}
  .stat-lbl {{ font-size: 11px; color: #94A3B8; text-transform: uppercase; letter-spacing: 0.5px; }}
  .sys-card {{ background: #181D26; border: 1px solid #283142; border-radius: 8px; padding: 16px 20px; margin-bottom: 24px; display: grid; grid-template-columns: repeat(3, 1fr); gap: 12px; font-size: 12px; }}
  .sys-item strong {{ color: #FFFFFF; display: block; margin-bottom: 2px; }}
  .sys-item span {{ color: #94A3B8; }}
  .table-card {{ background: #181D26; border: 1px solid #283142; border-radius: 8px; overflow: hidden; margin-bottom: 24px; }}
  table {{ width: 100%; border-collapse: collapse; font-size: 12.5px; text-align: left; }}
  th {{ background: #11141C; color: #94A3B8; font-weight: 600; padding: 12px 14px; border-bottom: 1px solid #283142; }}
  td {{ padding: 10px 14px; border-bottom: 1px solid #212836; vertical-align: middle; }}
  tr:last-child td {{ border-bottom: none; }}
  tr:hover td {{ background: #1F2532; }}
  .btn {{ background: #0078D4; color: #FFFFFF; border: none; padding: 8px 16px; border-radius: 4px; font-weight: 600; font-size: 12px; cursor: pointer; text-decoration: none; }}
  .footer {{ text-align: center; color: #64748B; font-size: 11px; margin-top: 32px; padding-top: 16px; border-top: 1px solid #232938; }}
</style>
</head>
<body>
<div class=""container"">
  <div class=""header"">
    <div>
      <div class=""brand-title"">⚡ FleetForge Machine Deployment Report</div>
      <div class=""brand-sub"">Enterprise Workstation Zero-Touch Provisioning Audit Trail</div>
    </div>
    <div class=""badge-org"">
      <div>LOVE MALHOTRA</div>
      <div style=""color:#94A3B8;font-weight:normal;font-size:10px;"">FleetForge v2.6.1 Engine</div>
    </div>
  </div>

  <div class=""stats-grid"">
    <div class=""stat-card"">
      <div class=""stat-val"" style=""color:#FFFFFF;"">{1}</div>
      <div class=""stat-lbl"">Total Packages</div>
    </div>
    <div class=""stat-card"">
      <div class=""stat-val"" style=""color:#34D399;"">{2}</div>
      <div class=""stat-lbl"">Succeeded</div>
    </div>
    <div class=""stat-card"">
      <div class=""stat-val"" style=""color:#F87171;"">{3}</div>
      <div class=""stat-lbl"">Failed / Stopped</div>
    </div>
    <div class=""stat-card"">
      <div class=""stat-val"" style=""color:#38BDF8;"">{4:F0}%</div>
      <div class=""stat-lbl"">Compliance Rate</div>
    </div>
  </div>

  <div class=""sys-card"">
    <div class=""sys-item"">
      <strong>Target Machine:</strong>
      <span>{5} (User: {6})</span>
    </div>
    <div class=""sys-item"">
      <strong>Operating System:</strong>
      <span>{7}</span>
    </div>
    <div class=""sys-item"">
      <strong>Session Timestamp:</strong>
      <span>{8}</span>
    </div>
  </div>

  <div class=""table-card"">
    <table>
      <thead>
        <tr>
          <th>#</th>
          <th>Package Name</th>
          <th>Winget Identifier</th>
          <th>Status</th>
          <th>Install Method</th>
          <th>Exit Code</th>
          <th>Duration</th>
          <th>Completed</th>
        </tr>
      </thead>
      <tbody>
{9}
      </tbody>
    </table>
  </div>

  <div style=""text-align:right;"">
    <button class=""btn"" onclick=""window.print()"">🖨️ Print / Save as PDF</button>
  </div>

  <div class=""footer"">
    FleetForge Enterprise Deployment Platform • Love Malhotra • All rights reserved.
  </div>
</div>
</body>
</html>",
                    session.sessionId,
                    session.totalCount,
                    session.successCount,
                    session.failedCount,
                    successRate,
                    Environment.MachineName,
                    Environment.UserName,
                    Environment.OSVersion.VersionString,
                    session.startTime,
                    rows.ToString());

                File.WriteAllText(filePath, html, Encoding.UTF8);
                return filePath;
            }
            catch
            {
                return null;
            }
        }

        public static string GenerateJsonReport(DeploymentSessionState session, string customPath = null)
        {
            try
            {
                string reportsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "reports");
                if (!Directory.Exists(reportsDir)) Directory.CreateDirectory(reportsDir);

                string filePath = customPath;
                if (string.IsNullOrEmpty(filePath))
                {
                    string safeTime = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                    filePath = Path.Combine(reportsDir, string.Format("Deployment-Report-{0}.json", safeTime));
                }

                JavaScriptSerializer ser = new JavaScriptSerializer();
                string json = ser.Serialize(session);
                File.WriteAllText(filePath, json, Encoding.UTF8);
                return filePath;
            }
            catch
            {
                return null;
            }
        }
    }

    // =========================================================================
    // Startup Splash Screen & Main Entry Point
    // =========================================================================
    // WINDOWS 11 FLUENT DESIGNER SPLASH SCREEN WITH SMOOTH ANIMATIONS
    // =========================================================================
    public class FleetForgeSplashScreen : Window
    {
        private ProgressBar progressBar;
        private TextBlock txtStatus;
        private TextBlock txtPercent;
        private Image imgSplashLogo;
        private DispatcherTimer timer;
        private int step = 0;
        private Action<ConfigModel, UpdateInfo> onComplete;
        private ConfigModel preloadedConfig = null;
        private UpdateInfo preloadedUpdate = null;
        private bool isPreloadComplete = false;
        private bool isExiting = false;

        private ScaleTransform cardScale;
        private TranslateTransform cardTranslate;
        private ScaleTransform logoScale;
        private DropShadowEffect logoGlow;

        public FleetForgeSplashScreen(Action<ConfigModel, UpdateInfo> onComplete)
        {
            this.onComplete = onComplete;
            this.WindowStyle = WindowStyle.None;
            this.AllowsTransparency = true;
            this.Background = Brushes.Transparent;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Width = 540;
            this.Height = 330;
            this.Topmost = true;
            this.ShowInTaskbar = false;
            this.Opacity = 0.0;

            this.MouseLeftButtonDown += (s, e) =>
            {
                try { this.DragMove(); } catch { }
            };

            bool isLight = MainWindow.IsWindowsLightTheme();
            Color accentColor = MainWindow.GetWindowsAccentColor();
            SolidColorBrush accentBrush = new SolidColorBrush(accentColor);
            SolidColorBrush accentVividBrush;
            if (isLight)
            {
                accentVividBrush = accentBrush;
            }
            else
            {
                double lum = (0.299 * accentColor.R + 0.587 * accentColor.G + 0.114 * accentColor.B) / 255.0;
                if (lum < 0.45)
                {
                    byte rV = (byte)Math.Min(255, (int)accentColor.R + 65);
                    byte gV = (byte)Math.Min(255, (int)accentColor.G + 85);
                    byte bV = (byte)Math.Min(255, (int)accentColor.B + 95);
                    accentVividBrush = new SolidColorBrush(Color.FromRgb(rV, gV, bV));
                }
                else
                {
                    accentVividBrush = accentBrush;
                }
            }

            Grid root = new Grid();
            root.Background = Brushes.Transparent;

            Border mainCard = new Border();
            mainCard.Width = 480;
            mainCard.Height = 270;
            mainCard.CornerRadius = new CornerRadius(16);
            mainCard.HorizontalAlignment = HorizontalAlignment.Center;
            mainCard.VerticalAlignment = VerticalAlignment.Center;
            mainCard.Padding = new Thickness(30, 22, 30, 18);
            mainCard.RenderTransformOrigin = new Point(0.5, 0.5);

            TransformGroup cardTg = new TransformGroup();
            cardScale = new ScaleTransform(0.93, 0.93);
            cardTranslate = new TranslateTransform(0, 12);
            cardTg.Children.Add(cardScale);
            cardTg.Children.Add(cardTranslate);
            mainCard.RenderTransform = cardTg;

            DropShadowEffect cardShadow = new DropShadowEffect();
            cardShadow.BlurRadius = isLight ? 28 : 34;
            cardShadow.Direction = 270;
            cardShadow.ShadowDepth = 8;
            cardShadow.Color = Colors.Black;
            cardShadow.Opacity = isLight ? 0.20 : 0.50;
            mainCard.Effect = cardShadow;

            if (isLight)
            {
                LinearGradientBrush bg = new LinearGradientBrush();
                bg.StartPoint = new Point(0, 0);
                bg.EndPoint = new Point(0, 1);
                bg.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FFFFFF"), 0.0));
                bg.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#F4F6F9"), 1.0));
                mainCard.Background = bg;

                LinearGradientBrush border = new LinearGradientBrush();
                border.StartPoint = new Point(0, 0);
                border.EndPoint = new Point(0, 1);
                border.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#E2E8F0"), 0.0));
                border.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#CBD5E1"), 1.0));
                mainCard.BorderBrush = border;
                mainCard.BorderThickness = new Thickness(1);
            }
            else
            {
                LinearGradientBrush bg = new LinearGradientBrush();
                bg.StartPoint = new Point(0, 0);
                bg.EndPoint = new Point(0, 1);
                bg.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#222225"), 0.0));
                bg.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#171719"), 1.0));
                mainCard.Background = bg;

                LinearGradientBrush border = new LinearGradientBrush();
                border.StartPoint = new Point(0, 0);
                border.EndPoint = new Point(0, 1);
                border.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#44444A"), 0.0));
                border.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#2B2B30"), 1.0));
                mainCard.BorderBrush = border;
                mainCard.BorderThickness = new Thickness(1);
            }

            Grid contentGrid = new Grid();
            contentGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            contentGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            // 1. Brand Section
            StackPanel brandStack = new StackPanel();
            brandStack.VerticalAlignment = VerticalAlignment.Center;
            brandStack.HorizontalAlignment = HorizontalAlignment.Center;

            Border logoBorder = new Border();
            logoBorder.Width = 52;
            logoBorder.Height = 52;
            logoBorder.CornerRadius = new CornerRadius(14);
            logoBorder.BorderThickness = new Thickness(1.5);
            logoBorder.HorizontalAlignment = HorizontalAlignment.Center;
            logoBorder.Margin = new Thickness(0, 0, 0, 8);
            logoBorder.RenderTransformOrigin = new Point(0.5, 0.5);

            logoScale = new ScaleTransform(1.0, 1.0);
            logoBorder.RenderTransform = logoScale;

            logoGlow = new DropShadowEffect();
            logoGlow.BlurRadius = 16;
            logoGlow.Direction = 0;
            logoGlow.ShadowDepth = 0;
            logoGlow.Color = accentColor;
            logoGlow.Opacity = isLight ? 0.30 : 0.45;
            logoBorder.Effect = logoGlow;

            if (isLight)
            {
                LinearGradientBrush lbg = new LinearGradientBrush();
                lbg.StartPoint = new Point(0, 0);
                lbg.EndPoint = new Point(0, 1);
                lbg.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#F8FAFC"), 0.0));
                lbg.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#EEF2F6"), 1.0));
                logoBorder.Background = lbg;
                logoBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CBD5E1"));
            }
            else
            {
                LinearGradientBrush lbg = new LinearGradientBrush();
                lbg.StartPoint = new Point(0, 0);
                lbg.EndPoint = new Point(0, 1);
                lbg.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#2C2C30"), 0.0));
                lbg.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#1D1D20"), 1.0));
                logoBorder.Background = lbg;
                logoBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4B4B52"));
            }

            Grid logoGrid = new Grid();
            imgSplashLogo = new Image();
            imgSplashLogo.Width = 40;
            imgSplashLogo.Height = 40;
            RenderOptions.SetBitmapScalingMode(imgSplashLogo, BitmapScalingMode.HighQuality);

            TextBlock txtFallback = new TextBlock();
            txtFallback.Text = "⚡";
            txtFallback.FontSize = 26;
            txtFallback.Foreground = accentVividBrush;
            txtFallback.HorizontalAlignment = HorizontalAlignment.Center;
            txtFallback.VerticalAlignment = VerticalAlignment.Center;
            txtFallback.Visibility = Visibility.Collapsed;

            ImageSource iconSrc = MainWindow.LoadApplicationIcon();
            if (iconSrc != null)
            {
                imgSplashLogo.Source = iconSrc;
                this.Icon = iconSrc;
            }
            else
            {
                txtFallback.Visibility = Visibility.Visible;
            }

            logoGrid.Children.Add(imgSplashLogo);
            logoGrid.Children.Add(txtFallback);
            logoBorder.Child = logoGrid;
            brandStack.Children.Add(logoBorder);

            // Title Line: FLEET + FORGE + v2.6.1
            StackPanel titleLine = new StackPanel();
            titleLine.Orientation = Orientation.Horizontal;
            titleLine.HorizontalAlignment = HorizontalAlignment.Center;

            TextBlock txtFleet = new TextBlock();
            txtFleet.Text = "FLEET";
            txtFleet.FontSize = 23;
            txtFleet.FontWeight = FontWeights.Bold;
            txtFleet.FontFamily = new FontFamily("Segoe UI Variable Display, Segoe UI Variable, Segoe UI");
            txtFleet.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLight ? "#0F172A" : "#FFFFFF"));
            titleLine.Children.Add(txtFleet);

            TextBlock txtForge = new TextBlock();
            txtForge.Text = "FORGE";
            txtForge.FontSize = 23;
            txtForge.FontWeight = FontWeights.Bold;
            txtForge.FontFamily = new FontFamily("Segoe UI Variable Display, Segoe UI Variable, Segoe UI");
            txtForge.Foreground = accentVividBrush;
            titleLine.Children.Add(txtForge);

            Border verBadge = new Border();
            verBadge.BorderThickness = new Thickness(1);
            verBadge.CornerRadius = new CornerRadius(5);
            verBadge.Padding = new Thickness(6, 2, 6, 2);
            verBadge.Margin = new Thickness(8, 0, 0, 0);
            verBadge.VerticalAlignment = VerticalAlignment.Center;
            verBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLight ? "#F1F5F9" : "#28282D"));
            verBadge.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLight ? "#CBD5E1" : "#3E3E46"));

            TextBlock txtVer = new TextBlock();
            txtVer.Text = "v2.6.1";
            txtVer.FontSize = 10;
            txtVer.FontWeight = FontWeights.Bold;
            txtVer.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLight ? "#475569" : "#A1A1AA"));
            verBadge.Child = txtVer;
            titleLine.Children.Add(verBadge);
            brandStack.Children.Add(titleLine);

            // Designer attribution line
            StackPanel desLine = new StackPanel();
            desLine.Orientation = Orientation.Horizontal;
            desLine.HorizontalAlignment = HorizontalAlignment.Center;
            desLine.Margin = new Thickness(0, 3, 0, 0);

            TextBlock txtDes1 = new TextBlock();
            txtDes1.Text = "Designed & Developed by ";
            txtDes1.FontSize = 11;
            txtDes1.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLight ? "#64748B" : "#94A3B8"));
            desLine.Children.Add(txtDes1);

            TextBlock txtDes2 = new TextBlock();
            txtDes2.Text = "Love Malhotra";
            txtDes2.FontSize = 11;
            txtDes2.FontWeight = FontWeights.SemiBold;
            txtDes2.Foreground = accentVividBrush;
            desLine.Children.Add(txtDes2);
            brandStack.Children.Add(desLine);

            Grid.SetRow(brandStack, 0);
            contentGrid.Children.Add(brandStack);

            // 2. Loading Step & Progress
            StackPanel progStack = new StackPanel();
            progStack.Margin = new Thickness(0, 14, 0, 0);

            Grid labelGrid = new Grid();
            labelGrid.Margin = new Thickness(0, 0, 0, 6);

            txtStatus = new TextBlock();
            txtStatus.Text = "Initializing FleetForge runtime...";
            txtStatus.FontSize = 11.5;
            txtStatus.FontWeight = FontWeights.SemiBold;
            txtStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLight ? "#0F172A" : "#E2E8F0"));
            labelGrid.Children.Add(txtStatus);

            txtPercent = new TextBlock();
            txtPercent.Text = "20%";
            txtPercent.FontSize = 11.5;
            txtPercent.FontWeight = FontWeights.SemiBold;
            txtPercent.Foreground = accentVividBrush;
            txtPercent.HorizontalAlignment = HorizontalAlignment.Right;
            labelGrid.Children.Add(txtPercent);
            progStack.Children.Add(labelGrid);

            // Custom modern rounded ProgressBar
            progressBar = new ProgressBar();
            progressBar.Height = 6;
            progressBar.Maximum = 100;
            progressBar.Value = 20;
            progressBar.BorderThickness = new Thickness(0);
            progressBar.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLight ? "#E2E8F0" : "#28282D"));
            progressBar.Foreground = accentVividBrush;

            ControlTemplate template = new ControlTemplate(typeof(ProgressBar));
            FrameworkElementFactory track = new FrameworkElementFactory(typeof(Border), "PART_Track");
            track.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(ProgressBar.BackgroundProperty));
            track.SetValue(Border.CornerRadiusProperty, new CornerRadius(3));

            FrameworkElementFactory indicator = new FrameworkElementFactory(typeof(Border), "PART_Indicator");
            indicator.SetValue(Border.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            indicator.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(ProgressBar.ForegroundProperty));
            indicator.SetValue(Border.CornerRadiusProperty, new CornerRadius(3));
            track.AppendChild(indicator);

            template.VisualTree = track;
            progressBar.Template = template;
            progStack.Children.Add(progressBar);

            Grid.SetRow(progStack, 1);
            contentGrid.Children.Add(progStack);

            // 3. Footer tag
            TextBlock txtFooter = new TextBlock();
            txtFooter.Text = "Windows 11 Edition • Fast Enterprise Deployment";
            txtFooter.FontSize = 10;
            txtFooter.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLight ? "#94A3B8" : "#71717A"));
            txtFooter.HorizontalAlignment = HorizontalAlignment.Center;
            txtFooter.Margin = new Thickness(0, 12, 0, 0);

            Grid.SetRow(txtFooter, 2);
            contentGrid.Children.Add(txtFooter);

            mainCard.Child = contentGrid;
            root.Children.Add(mainCard);
            this.Content = root;

            // Setup entrance and pulse animations on Loaded
            this.Loaded += (s, e) =>
            {
                // Entrance animations
                DoubleAnimation fadeIn = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(320));
                fadeIn.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
                this.BeginAnimation(Window.OpacityProperty, fadeIn);

                DoubleAnimation scaleXAnim = new DoubleAnimation(0.93, 1.0, TimeSpan.FromMilliseconds(350));
                scaleXAnim.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
                cardScale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnim);

                DoubleAnimation scaleYAnim = new DoubleAnimation(0.93, 1.0, TimeSpan.FromMilliseconds(350));
                scaleYAnim.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
                cardScale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnim);

                DoubleAnimation transYAnim = new DoubleAnimation(12.0, 0.0, TimeSpan.FromMilliseconds(350));
                transYAnim.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
                cardTranslate.BeginAnimation(TranslateTransform.YProperty, transYAnim);

                // Breathing pulse animation on brand icon
                DoubleAnimation pulseX = new DoubleAnimation(1.0, 1.055, TimeSpan.FromMilliseconds(1300));
                pulseX.AutoReverse = true;
                pulseX.RepeatBehavior = RepeatBehavior.Forever;
                pulseX.EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut };
                logoScale.BeginAnimation(ScaleTransform.ScaleXProperty, pulseX);

                DoubleAnimation pulseY = new DoubleAnimation(1.0, 1.055, TimeSpan.FromMilliseconds(1300));
                pulseY.AutoReverse = true;
                pulseY.RepeatBehavior = RepeatBehavior.Forever;
                pulseY.EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut };
                logoScale.BeginAnimation(ScaleTransform.ScaleYProperty, pulseY);

                double baseGlow = logoGlow.Opacity;
                DoubleAnimation glowPulse = new DoubleAnimation(baseGlow, baseGlow * 1.5, TimeSpan.FromMilliseconds(1300));
                glowPulse.AutoReverse = true;
                glowPulse.RepeatBehavior = RepeatBehavior.Forever;
                glowPulse.EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut };
                logoGlow.BeginAnimation(DropShadowEffect.OpacityProperty, glowPulse);
            };

            // Preload 2.8MB catalog & version info in background worker
            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    AppUrls.Initialize(baseDir);
                    string cPath = Path.Combine(baseDir, "data", "apps.json");
                    if (!File.Exists(cPath)) cPath = Path.Combine(baseDir, "apps.json");
                    if (File.Exists(cPath))
                    {
                        var ser = new JavaScriptSerializer();
                        ser.MaxJsonLength = int.MaxValue;
                        string json = File.ReadAllText(cPath, Encoding.UTF8);
                        preloadedConfig = ser.Deserialize<ConfigModel>(json);
                    }

                    string vPath = Path.Combine(baseDir, "data", "version.json");
                    if (!File.Exists(vPath)) vPath = Path.Combine(baseDir, "version.json");
                    if (File.Exists(vPath))
                    {
                        var ser = new JavaScriptSerializer();
                        string vJson = File.ReadAllText(vPath, Encoding.UTF8);
                        preloadedUpdate = ser.Deserialize<UpdateInfo>(vJson);
                    }
                }
                catch { }
                finally
                {
                    isPreloadComplete = true;
                }
            });

            string[] steps = new string[] {
                "Initializing FleetForge runtime...",
                "Validating Windows Package Manager (Winget)...",
                "Indexing enterprise catalog (16,349 packages)...",
                "Synchronizing live Windows environment theme...",
                "Ready! Launching FleetForge..."
            };
            int[] percents = new int[] { 25, 50, 75, 90, 100 };

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(160);
            timer.Tick += (s, e) =>
            {
                step++;
                if (step < steps.Length - 1)
                {
                    txtStatus.Text = steps[step];
                    AnimateProgress(percents[step]);
                    if (txtPercent != null) txtPercent.Text = percents[step] + "%";
                }
                else
                {
                    if (!isPreloadComplete && step < 25)
                    {
                        txtStatus.Text = "Loading enterprise catalog (16,349 packages)...";
                        AnimateProgress(92);
                        if (txtPercent != null) txtPercent.Text = "92%";
                        return;
                    }

                    if (isExiting) return;
                    isExiting = true;
                    timer.Stop();
                    txtStatus.Text = steps[steps.Length - 1];
                    AnimateProgress(100);
                    if (txtPercent != null) txtPercent.Text = "100%";

                    // Exit animation
                    DoubleAnimation fadeOut = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(200));
                    fadeOut.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseIn };

                    DoubleAnimation exitScaleX = new DoubleAnimation(1.0, 1.03, TimeSpan.FromMilliseconds(200));
                    exitScaleX.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseIn };
                    DoubleAnimation exitScaleY = new DoubleAnimation(1.0, 1.03, TimeSpan.FromMilliseconds(200));
                    exitScaleY.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseIn };

                    fadeOut.Completed += (s2, e2) =>
                    {
                        try
                        {
                            if (onComplete != null)
                            {
                                onComplete(preloadedConfig, preloadedUpdate);
                            }
                        }
                        catch { }
                    };

                    cardScale.BeginAnimation(ScaleTransform.ScaleXProperty, exitScaleX);
                    cardScale.BeginAnimation(ScaleTransform.ScaleYProperty, exitScaleY);
                    this.BeginAnimation(Window.OpacityProperty, fadeOut);
                }
            };
            timer.Start();
        }

        private void AnimateProgress(double targetValue)
        {
            if (progressBar != null)
            {
                DoubleAnimation pAnim = new DoubleAnimation(targetValue, TimeSpan.FromMilliseconds(150));
                pAnim.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
                progressBar.BeginAnimation(ProgressBar.ValueProperty, pAnim);
            }
        }
    }

    public class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        private const int STD_OUTPUT_HANDLE = -11;
        private const int STD_ERROR_HANDLE = -12;
        private const int ATTACH_PARENT_PROCESS = -1;

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

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        private const int SW_RESTORE = 9;

        private static Mutex singleInstanceMutex = null;

        private static void BringExistingInstanceToFront()
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
                    targetHwnd = FindWindow(null, "FleetForge — Zero-Touch Machine Setup & Enterprise Software Deployment");
                }
                if (targetHwnd == IntPtr.Zero)
                {
                    targetHwnd = FindWindow(null, "FleetForge");
                }

                if (targetHwnd != IntPtr.Zero)
                {
                    if (IsIconic(targetHwnd))
                    {
                        ShowWindowAsync(targetHwnd, SW_RESTORE);
                    }

                    uint activeThreadId = GetCurrentThreadId();
                    uint windowThreadId = GetWindowThreadProcessId(targetHwnd, IntPtr.Zero);

                    if (activeThreadId != windowThreadId && windowThreadId != 0)
                    {
                        AttachThreadInput(activeThreadId, windowThreadId, true);
                        SetForegroundWindow(targetHwnd);
                        AttachThreadInput(activeThreadId, windowThreadId, false);
                    }
                    else
                    {
                        SetForegroundWindow(targetHwnd);
                    }
                }
            }
            catch { }
        }

        private static void InitConsole()
        {
            try
            {
                if (AttachConsole(ATTACH_PARENT_PROCESS) || AllocConsole())
                {
                    IntPtr stdHandle = GetStdHandle(STD_OUTPUT_HANDLE);
                    SafeFileHandle safeHandle = new SafeFileHandle(stdHandle, false);
                    FileStream fs = new FileStream(safeHandle, FileAccess.Write);
                    StreamWriter standardOutput = new StreamWriter(fs, Console.OutputEncoding);
                    standardOutput.AutoFlush = true;
                    Console.SetOut(standardOutput);
                    Console.SetError(standardOutput);
                }
            }
            catch { }
        }

        private static void PrintCliHelp()
        {
            Console.WriteLine();
            Console.WriteLine("================================================================================");
            Console.WriteLine("  FleetForge — Zero-Touch Machine Setup & Enterprise Software Deployment");
            Console.WriteLine("  Love Malhotra • Windows 11 Automated Workstation Provisioning");
            Console.WriteLine("================================================================================");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  FleetForge.exe [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --role <role_id>     Deploy all software defined in the specified role environment");
            Console.WriteLine("                       (e.g., developer, devops, designer, ai_ml, gaming, enterprise)");
            Console.WriteLine("  --apps <id1,id2..>   Comma-separated list of application IDs or Winget packages to install");
            Console.WriteLine("  --silent, -s         Run headlessly without interactive GUI (ideal for GPO / Intune)");
            Console.WriteLine("  --resume             Resume the most recent interrupted deployment session");
            Console.WriteLine("  --timeout <minutes>  Watchdog timeout per application installer (default: 10 minutes)");
            Console.WriteLine("  --report <path>      Save HTML/JSON compliance audit report to custom destination");
            Console.WriteLine("  --sync               Synchronize application catalog from cloud URL before deployment");
            Console.WriteLine("  --help, -h, /?       Show this command-line reference and exit");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  FleetForge.exe --role developer --silent");
            Console.WriteLine("  FleetForge.exe --apps Google.Chrome,Git.Git,Microsoft.VisualStudioCode --silent");
            Console.WriteLine("  FleetForge.exe --resume --silent --timeout 15");
            Console.WriteLine("  FleetForge.exe --role enterprise --report C:\\DeployLogs\\WorkstationAudit.html");
            Console.WriteLine();
        }

        private static int RunCliAutomation(string[] args, string baseDir)
        {
            Console.WriteLine();
            Console.WriteLine("================================================================================");
            Console.WriteLine("  FleetForge Enterprise CLI Provisioning Engine v2.6.1");
            Console.WriteLine("  Love Malhotra • Windows 11 Automated Deployment");
            Console.WriteLine("================================================================================");
            Console.WriteLine();

            string targetRoleId = null;
            string customApps = null;
            string customReportPath = null;
            bool doResume = false;
            bool doSync = false;
            int timeoutMins = 10;

            for (int i = 0; i < args.Length; i++)
            {
                string a = args[i];
                if (a.Equals("--role", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    targetRoleId = args[++i];
                }
                else if (a.StartsWith("--role=", StringComparison.OrdinalIgnoreCase))
                {
                    targetRoleId = a.Substring(7);
                }
                else if (a.Equals("--apps", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    customApps = args[++i];
                }
                else if (a.StartsWith("--apps=", StringComparison.OrdinalIgnoreCase))
                {
                    customApps = a.Substring(7);
                }
                else if (a.Equals("--report", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    customReportPath = args[++i];
                }
                else if (a.StartsWith("--report=", StringComparison.OrdinalIgnoreCase))
                {
                    customReportPath = a.Substring(9);
                }
                else if (a.Equals("--timeout", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    int.TryParse(args[++i], out timeoutMins);
                }
                else if (a.StartsWith("--timeout=", StringComparison.OrdinalIgnoreCase))
                {
                    int.TryParse(a.Substring(10), out timeoutMins);
                }
                else if (a.Equals("--resume", StringComparison.OrdinalIgnoreCase))
                {
                    doResume = true;
                }
                else if (a.Equals("--sync", StringComparison.OrdinalIgnoreCase))
                {
                    doSync = true;
                }
            }

            if (timeoutMins <= 0) timeoutMins = 10;

            // Load Config
            string configPath = Path.Combine(baseDir, "data", "apps.json");
            if (!File.Exists(configPath)) configPath = Path.Combine(baseDir, "apps.json");

            if (!File.Exists(configPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ERROR] Applications catalog not found: " + configPath);
                Console.ResetColor();
                return 1;
            }

            // Sync if requested
            if (doSync)
            {
                Console.WriteLine("[SYNC] Synchronizing application catalog from remote cloud...");
                try
                {
                    using (WebClient wc = new WebClient())
                    {
                        wc.Encoding = Encoding.UTF8;
                        string json = wc.DownloadString(AppUrls.CloudSyncUrl);
                        if (!string.IsNullOrEmpty(json) && json.Contains("apps"))
                        {
                            File.WriteAllText(configPath, json, Encoding.UTF8);
                            Console.WriteLine("[SYNC] Catalog updated successfully from " + AppUrls.CloudSyncUrl);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[SYNC WARNING] Cloud synchronization failed: " + ex.Message + " (using local cache)");
                }
            }

            JavaScriptSerializer ser = new JavaScriptSerializer();
            ser.MaxJsonLength = int.MaxValue;
            ConfigModel config = null;
            try
            {
                string json = File.ReadAllText(configPath, Encoding.UTF8);
                config = ser.Deserialize<ConfigModel>(json);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ERROR] Failed to parse applications catalog: " + ex.Message);
                Console.ResetColor();
                return 1;
            }

            string sessionPath = Path.Combine(baseDir, "data", "deployment_session.json");
            DeploymentSessionState session = null;
            List<AppModel> appsToInstall = new List<AppModel>();

            if (doResume && File.Exists(sessionPath))
            {
                try
                {
                    string sessJson = File.ReadAllText(sessionPath, Encoding.UTF8);
                    session = ser.Deserialize<DeploymentSessionState>(sessJson);
                    if (session != null && session.records != null)
                    {
                        Console.WriteLine("[RESUME] Resuming deployment session: " + session.sessionId);
                        foreach (AppDeploymentRecord rec in session.records)
                        {
                            if (rec.status == "Pending" || rec.status == "Running" || rec.status == "Failed" || rec.status == "Timeout")
                            {
                                AppModel found = (config != null && config.apps != null) ?
                                    config.apps.Find(delegate(AppModel a) { return a.id == rec.appId; }) : null;
                                if (found != null)
                                {
                                    appsToInstall.Add(found);
                                }
                                else
                                {
                                    AppModel manual = new AppModel();
                                    manual.id = rec.appId;
                                    manual.name = rec.appName ?? rec.appId;
                                    manual.category = rec.category ?? "General";
                                    appsToInstall.Add(manual);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[RESUME WARNING] Could not read previous session: " + ex.Message);
                }
            }

            if (appsToInstall.Count == 0)
            {
                if (!string.IsNullOrEmpty(targetRoleId))
                {
                    GroupModel group = (config != null && config.groups != null) ?
                        config.groups.Find(delegate(GroupModel g) { return g.id.Equals(targetRoleId, StringComparison.OrdinalIgnoreCase); }) : null;

                    if (group == null)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[ERROR] Role environment '" + targetRoleId + "' not found in catalog.");
                        Console.WriteLine("Available roles:");
                        if (config != null && config.groups != null)
                        {
                            foreach (GroupModel g in config.groups)
                            {
                                Console.WriteLine(string.Format("  - {0,-15} ({1}, {2} apps)", g.id, g.name, g.apps != null ? g.apps.Count : 0));
                            }
                        }
                        Console.ResetColor();
                        return 1;
                    }

                    Console.WriteLine(string.Format("[ROLE] Selected Role: {0} ({1}) — {2} packages configured", group.name, group.id, group.apps != null ? group.apps.Count : 0));

                    session = new DeploymentSessionState();
                    session.roleId = group.id;
                    session.roleName = group.name;

                    if (group.apps != null && config.apps != null)
                    {
                        foreach (string id in group.apps)
                        {
                            AppModel app = config.apps.Find(delegate(AppModel a) { return a.id == id; });
                            if (app != null) appsToInstall.Add(app);
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(customApps))
                {
                    session = new DeploymentSessionState();
                    session.roleId = "custom";
                    session.roleName = "Custom CLI Selection";

                    string[] ids = customApps.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string rawId in ids)
                    {
                        string id = rawId.Trim();
                        AppModel app = (config != null && config.apps != null) ?
                            config.apps.Find(delegate(AppModel a) { return a.id.Equals(id, StringComparison.OrdinalIgnoreCase); }) : null;
                        if (app != null)
                        {
                            appsToInstall.Add(app);
                        }
                        else
                        {
                            AppModel wingetApp = new AppModel();
                            wingetApp.id = id;
                            wingetApp.name = id;
                            wingetApp.category = "CLI Manual";
                            appsToInstall.Add(wingetApp);
                        }
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("[INFO] No role or application list specified. Use --role <id> or --apps <list>.");
                    Console.WriteLine("Run 'FleetForge.exe --help' for command syntax.");
                    Console.ResetColor();
                    return 0;
                }
            }

            if (appsToInstall.Count == 0)
            {
                Console.WriteLine("[INFO] No packages to install. Exiting.");
                return 0;
            }

            if (session == null)
            {
                session = new DeploymentSessionState();
                session.roleId = targetRoleId ?? "custom";
                session.roleName = targetRoleId ?? "Custom Workstation";
            }

            session.totalCount = appsToInstall.Count;
            if (session.records == null) session.records = new List<AppDeploymentRecord>();

            // Find Winget
            string wingetPath = null;
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("winget", "--version");
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                using (Process p = Process.Start(psi))
                {
                    p.WaitForExit();
                    if (p.ExitCode == 0) wingetPath = "winget";
                }
            }
            catch { }

            string logDir = Path.Combine(baseDir, "logs");
            if (!Directory.Exists(logDir)) { try { Directory.CreateDirectory(logDir); } catch { } }
            string logFilePath = Path.Combine(logDir, "install-log.txt");

            Console.WriteLine(string.Format("[DEPLOY] Starting deployment of {0} package(s)... (Watchdog timeout: {1}m)", appsToInstall.Count, timeoutMins));
            Console.WriteLine("--------------------------------------------------------------------------------");

            int success = 0;
            int failed = 0;

            for (int i = 0; i < appsToInstall.Count; i++)
            {
                AppModel app = appsToInstall[i];
                Console.Write(string.Format("[{0}/{1}] Installing {2} ({3})... ", i + 1, appsToInstall.Count, app.name, app.id));

                AppDeploymentRecord rec = session.records.Find(delegate(AppDeploymentRecord r) { return r != null && r.appId == app.id; });
                if (rec == null)
                {
                    rec = new AppDeploymentRecord();
                    rec.appId = app.id;
                    rec.appName = app.name;
                    rec.category = app.category;
                    session.records.Add(rec);
                }

                rec.status = "Installing";
                rec.timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                DateTime startPkg = DateTime.Now;

                // Save session state progress
                try
                {
                    File.WriteAllText(sessionPath, ser.Serialize(session), Encoding.UTF8);
                }
                catch { }

                bool installed = false;
                string offlineDir = Path.Combine(baseDir, "offline_installers");

                if (!string.IsNullOrEmpty(app.offlineInstaller) && Directory.Exists(offlineDir))
                {
                    string offlineFile = Path.Combine(offlineDir, app.offlineInstaller);
                    if (File.Exists(offlineFile))
                    {
                        try
                        {
                            ProcessStartInfo psi = new ProcessStartInfo(offlineFile, app.silentArgs ?? "");
                            psi.CreateNoWindow = true;
                            psi.UseShellExecute = false;
                            using (Process p = Process.Start(psi))
                            {
                                int timeoutMs = timeoutMins * 60 * 1000;
                                DateTime startProc = DateTime.Now;
                                bool timedOut = false;
                                while (!p.HasExited)
                                {
                                    if ((DateTime.Now - startProc).TotalMilliseconds > timeoutMs)
                                    {
                                        timedOut = true;
                                        try { p.Kill(); } catch { }
                                        break;
                                    }
                                    Thread.Sleep(500);
                                }

                                rec.durationSeconds = (DateTime.Now - startPkg).TotalSeconds;
                                rec.installMethod = "Offline Installer";

                                if (timedOut)
                                {
                                    rec.status = "Timeout";
                                    rec.exitCode = -1;
                                    rec.errorDetails = "Exceeded watchdog timeout limit";
                                }
                                else if (p.ExitCode == 0 || p.ExitCode == 3010)
                                {
                                    rec.status = "Installed";
                                    rec.exitCode = p.ExitCode;
                                    installed = true;
                                }
                                else
                                {
                                    rec.status = "Failed";
                                    rec.exitCode = p.ExitCode;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            rec.status = "Failed";
                            rec.errorDetails = ex.Message;
                        }
                    }
                }

                if (!installed && rec.status != "Timeout")
                {
                    if (string.IsNullOrEmpty(wingetPath))
                    {
                        rec.status = "Failed";
                        rec.errorDetails = "Winget not found on system";
                    }
                    else
                    {
                        string wArgs = string.Format("install --id \"{0}\" -e --silent --accept-package-agreements --accept-source-agreements --disable-interactivity", app.id);
                        try
                        {
                            ProcessStartInfo psi = new ProcessStartInfo(wingetPath, wArgs);
                            psi.CreateNoWindow = true;
                            psi.UseShellExecute = false;
                            psi.RedirectStandardOutput = true;
                            psi.RedirectStandardError = true;

                            using (Process proc = Process.Start(psi))
                            {
                                int timeoutMs = timeoutMins * 60 * 1000;
                                DateTime startProc = DateTime.Now;
                                bool timedOut = false;
                                while (!proc.HasExited)
                                {
                                    if ((DateTime.Now - startProc).TotalMilliseconds > timeoutMs)
                                    {
                                        timedOut = true;
                                        try { proc.Kill(); } catch { }
                                        break;
                                    }
                                    Thread.Sleep(500);
                                }

                                rec.durationSeconds = (DateTime.Now - startPkg).TotalSeconds;
                                rec.installMethod = "Winget Silent";

                                if (timedOut)
                                {
                                    rec.status = "Timeout";
                                    rec.exitCode = -1;
                                    rec.errorDetails = "Exceeded watchdog timeout limit";
                                }
                                else
                                {
                                    int code = proc.ExitCode;
                                    rec.exitCode = code;
                                    uint uCode = unchecked((uint)code);
                                    if (code == 0 || code == 3010 || code == -1978335189 || uCode == 2316632107U)
                                    {
                                        rec.status = "Installed";
                                        installed = true;
                                    }
                                    else
                                    {
                                        rec.status = "Failed";
                                        string err = proc.StandardError.ReadToEnd();
                                        if (!string.IsNullOrEmpty(err)) rec.errorDetails = err.Trim();
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            rec.status = "Failed";
                            rec.errorDetails = ex.Message;
                        }
                    }
                }

                if (installed)
                {
                    success++;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(string.Format("SUCCESS ({0:F1}s)", rec.durationSeconds));
                    Console.ResetColor();
                }
                else
                {
                    failed++;
                    if (rec.status == "Timeout")
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(string.Format("TIMEOUT (Watchdog killed after {0}m)", timeoutMins));
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(string.Format("FAILED (ExitCode: {0})", rec.exitCode));
                    }
                    Console.ResetColor();
                }

                session.successCount = success;
                session.failedCount = failed;
                try
                {
                    File.WriteAllText(sessionPath, ser.Serialize(session), Encoding.UTF8);
                }
                catch { }
            }

            session.endTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            session.isCompleted = (failed == 0);

            try
            {
                if (session.isCompleted)
                {
                    if (File.Exists(sessionPath)) File.Delete(sessionPath);
                }
                else
                {
                    File.WriteAllText(sessionPath, ser.Serialize(session), Encoding.UTF8);
                }
            }
            catch { }

            string reportHtml = DeploymentReportGenerator.GenerateHtmlReport(session, logFilePath, customReportPath);
            string reportJson = DeploymentReportGenerator.GenerateJsonReport(session);

            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine(string.Format("Deployment Complete!  Succeeded: {0}  |  Failed/Timeout: {1}", success, failed));
            if (!string.IsNullOrEmpty(reportHtml))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Compliance HTML Report: " + reportHtml);
                Console.ResetColor();
            }
            if (!string.IsNullOrEmpty(reportJson))
            {
                Console.WriteLine("Audit Log JSON:        " + reportJson);
            }
            Console.WriteLine("================================================================================");
            Console.WriteLine();

            return (failed == 0) ? 0 : 1;
        }

        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                AppUrls.Initialize(baseDir);

                if (args != null && args.Length > 0)
                {
                    bool isHelp = false;
                    bool isSilent = false;
                    bool hasRole = false;
                    bool hasApps = false;
                    bool hasResume = false;

                    foreach (string arg in args)
                    {
                        string lower = arg.ToLowerInvariant();
                        if (lower == "--help" || lower == "-h" || lower == "/?" || lower == "-?") isHelp = true;
                        if (lower == "--silent" || lower == "-s") isSilent = true;
                        if (lower == "--role" || lower.StartsWith("--role=")) hasRole = true;
                        if (lower == "--apps" || lower.StartsWith("--apps=")) hasApps = true;
                        if (lower == "--resume") hasResume = true;
                    }

                    if (isHelp)
                    {
                        InitConsole();
                        PrintCliHelp();
                        Environment.Exit(0);
                        return;
                    }

                    if (isSilent || hasRole || hasApps || hasResume)
                    {
                        InitConsole();
                        int exitCode = RunCliAutomation(args, baseDir);
                        Environment.Exit(exitCode);
                        return;
                    }
                }

                // Single Instance Application Check (GUI Mode)
                bool isFirstInstance;
                singleInstanceMutex = new Mutex(true, "Local\\FleetForge_SingleInstance_LoveMalhotra", out isFirstInstance);
                if (!isFirstInstance)
                {
                    BringExistingInstanceToFront();
                    return;
                }

                Application app = new Application();
                app.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                FleetForgeSplashScreen splash = null;
                splash = new FleetForgeSplashScreen((preloadedConfig, preloadedUpdate) =>
                {
                    try
                    {
                        MainWindow win = new MainWindow(preloadedConfig, preloadedUpdate);
                        app.MainWindow = win;
                        win.Show();
                        win.Activate();
                        if (splash != null) splash.Close();
                        app.ShutdownMode = ShutdownMode.OnMainWindowClose;
                    }
                    catch (Exception ex)
                    {
                        if (splash != null) splash.Close();
                        MessageBox.Show(
                            "Error during window initialization:\n\n" + ex.Message + "\n\n" + ex.StackTrace,
                            "FleetForge Startup Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        Application.Current.Shutdown();
                    }
                });

                splash.Show();
                app.Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Fatal Startup Error:\n\n" + ex.Message + "\n\n" + ex.StackTrace,
                    "FleetForge Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

    public class MainWindow : Window
    {
        private string appDir;
        private string configPath;
        private string settingsPath;
        private string logFilePath;
        private string sessionStatePath;
        private string reportsDir;
        private string wingetCmd;
        private DeploymentSessionState currentSession = null;
        private string lastGeneratedReportPath = null;
        private int watchdogTimeoutMinutes = 10;

        private ConfigModel config;
        private SettingsModel settings;
        private JavaScriptSerializer jsonSerializer;

        // UI References - Navigation & Views
        private string currentView = "setup";
        private Button navBtnSetup;
        private Button navBtnCatalog;
        private Button navBtnCloud;
        private Button navBtnLogs;

        private Border navIndSetup;
        private Border navIndCatalog;
        private Border navIndCloud;
        private Border navIndLogs;

        private TextBlock navIconSetup;
        private TextBlock navIconCatalog;
        private TextBlock navIconCloud;
        private TextBlock navIconLogs;

        private TextBlock navTxtSetup;
        private TextBlock navTxtCatalog;
        private TextBlock navTxtCloud;
        private TextBlock navTxtLogs;

        private ScrollViewer viewSetup;
        private Grid viewCatalog;
        private ScrollViewer viewCloud;
        private Grid viewLogs;

        private TextBlock txtPageTitle;
        private TextBlock txtPageSubtitle;
        private TextBlock txtTopSyncStatus;
        private TextBlock txtNavCloudStatus;
        private TextBlock txtWingetVersion;

        // Theme & Container UI References
        private bool currentThemeIsLight = false;

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWCP_ROUND = 2;

        private void UpdateDwmTheme(bool isLight)
        {
            try
            {
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                if (hwnd == IntPtr.Zero) return;

                int cornerPref = DWMWCP_ROUND;
                DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPref, sizeof(int));

                int darkMode = isLight ? 0 : 1;
                int hr = DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));
                if (hr != 0)
                {
                    DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref darkMode, sizeof(int));
                }
            }
            catch { }
        }

        private Border borderSidebar;
        private Border borderBrandLogo;
        private StackPanel stackBrandText;
        private TextBlock txtBrandTitle;
        private TextBlock txtBrandSubtitle;
        private Button btnToggleSidebar;
        private TextBlock txtToggleSidebarIcon;
        private Border borderNavCloudBadge;
        private Border borderSysEnv;
        private TextBlock txtSysEnvHeader;
        private StackPanel stackSysEnvDetails;
        private TextBlock txtSysEnvTitle;
        private StackPanel stackSidebarFooter;
        private TextBlock txtSidebarFooterBy;
        private TextBlock txtSidebarFooterCompany;
        private Border borderTopBar;
        private bool isSidebarCollapsed = false;

        // Windows 11 Fluent Profile & System Colors
        private Border borderUserProfile;
        private Border borderUserAvatar;
        private TextBlock txtUserName;
        private TextBlock txtUserEmail;
        private TextBlock txtUserAvatarIcon;

        private TextBlock txtPageBigHeading;
        private TextBlock txtPageBigSubtitle;
        private Border borderHeroStatusCard;
        private Border borderHeroStatusIcon;
        private TextBlock txtHeroStatusIcon;
        private TextBlock txtHeroStatusTitle;
        private TextBlock txtHeroStatusSubtitle;
        private Button btnHeroInstall;

        private Border borderStep1Circle;
        private Border borderStep2Circle;
        private Border borderStep3Circle;

        private TextBlock txtCatalogBigHeading;
        private TextBlock txtCatalogBigSubtitle;
        private TextBlock txtCloudBigHeading;
        private TextBlock txtCloudBigSubtitle;
        private TextBlock txtLogsBigHeading;
        private TextBlock txtLogsBigSubtitle;

        private Color windowsAccentColor = (Color)ColorConverter.ConvertFromString("#0078D4");
        private SolidColorBrush windowsAccentBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0078D4"));
        private SolidColorBrush windowsAccentSubtleBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EBF3FB"));
        private SolidColorBrush windowsAccentBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CCE4F7"));

        public static Color GetWindowsAccentColor()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM"))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("AccentColor");
                        if (val != null)
                        {
                            uint abgr = Convert.ToUInt32(val);
                            byte b = (byte)((abgr >> 16) & 0xFF);
                            byte g = (byte)((abgr >> 8) & 0xFF);
                            byte r = (byte)(abgr & 0xFF);
                            if (r != 0 || g != 0 || b != 0) return Color.FromArgb(255, r, g, b);
                        }
                        object colVal = key.GetValue("ColorizationColor");
                        if (colVal != null)
                        {
                            uint argb = Convert.ToUInt32(colVal);
                            byte r = (byte)((argb >> 16) & 0xFF);
                            byte g = (byte)((argb >> 8) & 0xFF);
                            byte b = (byte)(argb & 0xFF);
                            if (r != 0 || g != 0 || b != 0) return Color.FromArgb(255, r, g, b);
                        }
                    }
                }
            }
            catch { }

            try
            {
                Color glass = SystemParameters.WindowGlassColor;
                if (glass.R != 0 || glass.G != 0 || glass.B != 0)
                {
                    return Color.FromArgb(255, glass.R, glass.G, glass.B);
                }
            }
            catch { }

            return (Color)ColorConverter.ConvertFromString("#0078D4");
        }
        private Border borderHero;
        private TextBlock txtHeroTitle;
        private TextBlock txtHeroDesc;
        private Button btnHeroCatalog;
        private TextBlock txtTargetRoleTitle;
        private TextBlock txtTargetRoleSubtitle;
        private Border borderCatalogContainer;
        private Border borderCatalogHeaderBar;
        private CheckBox chkSelectAllCatalog;
        private TextBlock txtCatalogListHeaderCount;
        private Button btnCatalogSort;
        private string catalogSortMode = "selected_top";
        private Dictionary<string, int> appDefaultIndices = null;
        private bool catalogNeedsRefresh = false;
        private ScrollViewer catalogScrollViewer;
        private StackPanel localCardsContainer;
        private Button btnLoadMore;
        private Button btnLoadAll;
        private StackPanel catalogPaginationPanel;
        private bool isStreamingAllApps = false;
        private HashSet<string> selectedAppIds = new HashSet<string>();
        private List<AppModel> activeFilteredApps = new List<AppModel>();
        private int currentRenderedCount = 0;
        private bool isUpdatingMasterCheckbox = false;
        private bool isUpdatingSelection = false;
        private Border borderCloudCard1;
        private Border borderCloudCard2;
        private Border borderCloudCard3;
        private TextBlock txtCloudTitle1;
        private TextBlock txtCloudDesc1;
        private TextBlock txtCloudCard1Title;
        private TextBlock txtCloudCard2Title;
        private TextBlock txtCloudCard3Title;
        private TextBlock txtLogsHeader;
        private Border borderLogTerminal;
        private Border borderBottomBar;
        private Border borderModalCard;
        private Border borderModalAppsContainer;

        // Setup Page
        private Button btnImportPack;
        private Button btnCreateNewGroup;
        private System.Windows.Controls.Primitives.UniformGrid roleCardsGrid;

        // Catalog Page
        private Border borderHeaderSearch;
        private TextBlock txtHeaderSearchIcon;
        private TextBox txtSearch;
        private Button btnClearSearch;
        private DispatcherTimer searchDebounceTimer;
        private Border borderCategoryBar;
        private WrapPanel categoryChipsPanel;
        private StackPanel appsContainer;
        private StackPanel localAppsPanel;
        private StackPanel wingetOnlinePanel;
        private Border bannerSearchWingetPrompt;
        private TextBlock txtSearchWingetPromptText;
        private TextBox txtOnlineSearchInput;
        private TextBlock txtOnlineStatus;
        private StackPanel onlineResultsPanel;
        private Button btnExecuteOnlineSearch;

        // Cloud Page
        private TextBox txtCloudUrlInput;
        private CheckBox chkAutoSync;
        private Button btnSaveCloudUrl;
        private Button btnSyncNow;
        private Border syncResultBorder;
        private TextBlock txtSyncResult;
        private Button btnExportSampleJson;
        private Button btnOpenLocalCatalog;
        private Border borderCloudPresets;
        private TextBlock txtPresetsTitle;
        private TextBlock txtPresetsDesc;
        private StackPanel presetsContainer;

        // Logs Page
        private ScrollViewer logScrollViewer;
        private TextBox txtLog;
        private Button btnViewReport;
        private Button btnOpenLogFile;
        private Button btnClearLogConsole;

        // Resume Alert Banner
        private Border bannerResumeSetup;
        private TextBlock txtResumeBannerTitle;
        private TextBlock txtResumeBannerDesc;
        private Button btnResumeSession;
        private Button btnDismissResume;

        // Sticky Bottom Bar
        private Border badgeAppVersion;
        private TextBlock txtAppVersion;
        private TextBlock txtStatus;
        private TextBlock txtCount;
        private ProgressBar progressBar;
        private Button btnSearchWingetOnlineBottom;
        private Button btnOpenOfflineFolder;
        private Button btnInstall;
        private Button btnStop;
        private Process currentProc;
        private volatile bool isCancelled;

        // Group Editor Modal References
        private Grid groupModalOverlay;
        private TextBlock txtModalTitle;
        private Button btnCloseModal;
        private TextBox txtGroupNameInput;
        private TextBox txtGroupIconInput;
        private WrapPanel emojiChipsPanel;
        private TextBox txtGroupDescInput;
        private TextBox txtModalAppSearch;
        private Button btnModalFilterTabAll;
        private Button btnModalFilterTabSelected;
        private Button btnModalSelectAll;
        private Button btnModalClearAll;
        private TextBlock txtModalSelectedCount;
        private ScrollViewer modalAppsScrollViewer;
        private StackPanel modalAppsContainer;
        private Button btnModalLoadMore;
        private Button btnDeleteGroup;
        private Button btnModalExportGroup;
        private Button btnCancelModal;
        private Button btnSaveGroup;

        // Help Menu, Theme Toggle & Brand Icon
        private Grid rootGrid;
        private Button btnToggleTheme;
        private TextBlock txtToggleThemeIcon;
        private Button btnHelpMenu;
        private Image imgBrandLogo;

        // Modal 1: Check for Updates
        private Grid modalUpdatesOverlay;
        private Border borderUpdatesCard;
        private TextBlock txtModalUpdatesTitle;
        private Border borderModalUpdatesIcon;
        private System.Windows.Shapes.Path pathModalUpdatesIcon;
        private Button btnCloseUpdates;
        private Button btnCheckUpdatesAgain;
        private Button btnDownloadInstall;
        private Border borderUpdateStatusCard;
        private Border borderUpdateIconBadge;
        private System.Windows.Shapes.Path pathUpdateStatusIcon;
        private TextBlock txtUpdateTitle;
        private TextBlock txtUpdateDetail;
        private Border borderChangelogCard;
        private TextBlock txtChangelogHeading;
        private StackPanel pnlChangelogList;
        private StackPanel pnlDownloadSection;
        private ProgressBar pbUpdateDownload;
        private TextBlock txtDownloadStatus;
        private UpdateInfo currentUpdateInfo = null;
        private bool isUpdateAvailable = false;

        // Modal 2: Settings
        private Grid modalSettingsOverlay;
        private Border borderSettingsCard;
        private TextBlock txtModalSettingsTitle;
        private Border badgeSettingsConfig;
        private TextBlock txtSettingsConfig;
        private Button btnCloseSettings;
        private Border borderSettingsSec1;
        private TextBlock txtSettingsSec1Title;
        private CheckBox chkSettingSilent;
        private CheckBox chkSettingAgreements;
        private CheckBox chkSettingAutoTheme;
        private Border borderSettingsSec2;
        private TextBlock txtSettingsSec2Title;
        private TextBlock txtSettingOfflineDirLbl;
        private TextBox txtSettingOfflineDir;
        private Button btnBrowseOfflineDir;
        private Button btnClearCache;
        private TextBlock txtCacheStatus;
        private Border borderSettingsSec3;
        private TextBlock txtSettingsSec3Title;
        private TextBlock txtSettingCatalogInfo;
        private TextBlock txtSettingCatalogSource;
        private Button btnExportCatalog;
        private Button btnCancelSettings;
        private Button btnSaveSettings;

        // Modal 3: About FleetForge
        private Grid modalAboutOverlay;
        private Border borderAboutCard;
        private TextBlock txtModalAboutTitle;
        private Button btnCloseAbout;
        private Border borderAboutLogoBox;
        private Image imgAboutLogo;
        private TextBlock txtAboutFleet;
        private TextBlock txtAboutForge;
        private TextBlock txtAboutTagline;
        private Border badgeAboutCompany;
        private TextBlock txtAboutCompany;
        private Border badgeAboutEdition;
        private TextBlock txtAboutEdition;
        private Border borderAboutBio;
        private TextBlock txtAboutBioText;
        private Border borderAboutSpecs;
        private TextBlock txtSpecOrgLbl;
        private TextBlock txtSpecOrgVal;
        private TextBlock txtSpecEngLbl;
        private TextBlock txtSpecEngVal;
        private TextBlock txtSpecLibLbl;
        private TextBlock txtSpecLibVal;
        private TextBlock txtSpecArchLbl;
        private TextBlock txtSpecArchVal;
        private TextBlock txtSpecChanLbl;
        private TextBlock txtSpecChanVal;
        private TextBlock txtSpecLicLbl;
        private TextBlock txtSpecLicVal;
        private TextBlock txtAboutCopyright;

        // Step Guide & Tutorial Modal
        private Button btnQuickGuide;
        private Border borderStepsGuide;
        private TextBlock txtStep1Title;
        private TextBlock txtStep1Desc;
        private TextBlock txtStep2Title;
        private TextBlock txtStep2Desc;
        private TextBlock txtStep3Title;
        private TextBlock txtStep3Desc;
        private TextBlock txtStepArrow1;
        private TextBlock txtStepArrow2;

        private Grid modalTutorialOverlay;
        private Border borderTutorialCard;
        private TextBlock txtModalTutorialTitle;
        private TextBlock txtModalTutorialSubtitle;
        private Button btnCloseTutorial;

        // Tutorial Stepper Pills
        private Border badgeTutStep1;
        private TextBlock txtTutStep1Badge;
        private Border badgeTutStep2;
        private TextBlock txtTutStep2Badge;
        private Border badgeTutStep3;
        private TextBlock txtTutStep3Badge;

        private StackPanel pnlTutorialStep1;
        private StackPanel pnlTutorialStep2;
        private StackPanel pnlTutorialStep3;

        // Step 1
        private Border cardTutStep1Hero;
        private TextBlock txtTutStep1HeroTitle;
        private TextBlock txtTutStep1HeroDesc;
        private Border cardTutStep1Box1;
        private TextBlock txtTutStep1Box1Title;
        private TextBlock txtTutStep1Box1Desc;
        private Border cardTutStep1Box2;
        private TextBlock txtTutStep1Box2Title;
        private TextBlock txtTutStep1Box2Desc;
        private Border cardTutStep1Box3;
        private TextBlock txtTutStep1Box3Title;
        private TextBlock txtTutStep1Box3Desc;

        // Step 2
        private Border cardTutStep2Box1;
        private Border borderTutStep2Num1;
        private TextBlock txtTutStep2Box1Title;
        private TextBlock txtTutStep2Box1Desc;
        private Border cardTutStep2Box2;
        private Border borderTutStep2Num2;
        private TextBlock txtTutStep2Box2Title;
        private TextBlock txtTutStep2Box2Desc;
        private Border cardTutStep2Box3;
        private Border borderTutStep2Num3;
        private TextBlock txtTutStep2Box3Title;
        private TextBlock txtTutStep2Box3Desc;

        // Step 3
        private Border cardTutStep3Box1;
        private TextBlock txtTutStep3Box1Title;
        private TextBlock txtTutStep3Box1Desc;
        private Border cardTutStep3Box2;
        private TextBlock txtTutStep3Box2Title;
        private TextBlock txtTutStep3Box2Desc;
        private Border cardTutStep3Box3;
        private TextBlock txtTutStep3Box3Title;
        private TextBlock txtTutStep3Box3Desc;

        // Footer
        private Border borderTutorialStepIndicator;
        private TextBlock txtTutorialStepIndicator;
        private Button btnTutorialBack;
        private Button btnTutorialNext;
        private CheckBox chkDontShowTutorialAgain;
        private int currentTutorialStep = 1;

        // Runtime State
        private List<AppRowView> appRows;
        private List<RoleCardView> roleCards;
        private List<ModalAppItemView> modalAppRows;
        private HashSet<string> modalSelectedAppIds = new HashSet<string>();
        private List<AppModel> modalFilteredApps = new List<AppModel>();
        private List<AppModel> modalUnselectedApps = new List<AppModel>();
        private int modalUnselectedRenderedCount = 0;
        private bool modalShowSelectedOnly = false;
        private string activeCategory = "all";
        private string activeRoleId = "developer";
        private string currentEditingGroupId = null;

        public MainWindow(ConfigModel preloadedConfig = null, UpdateInfo preloadedUpdate = null)
        {
            if (preloadedConfig != null) this.config = preloadedConfig;
            if (preloadedUpdate != null) this.currentUpdateInfo = preloadedUpdate;
            InitializePaths();
            InitializeWindow();
            LoadSettings();
            LoadConfigWithAutoSync();
            ApplyTheme(GetCurrentThemeIsLight());
            RegisterSystemThemeWatcher();
            CheckBackgroundUpdateOnStartup();
            CheckResumeSession();
            CheckFirstTimeTutorial();

            this.SourceInitialized += delegate { UpdateDwmTheme(currentThemeIsLight); };
            this.Loaded += delegate { UpdateDwmTheme(currentThemeIsLight); };
        }

        public static ImageSource LoadApplicationIcon()
        {
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
                                    return f;
                                }
                            }
                            return decoder.Frames[0];
                        }
                    }
                }
            }
            catch { }

            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string[] candidates = new string[] {
                    Path.Combine(baseDir, "src", "assets", "appicon.ico"),
                    Path.Combine(baseDir, "src", "appicon.ico"),
                    Path.Combine(baseDir, "assets", "appicon.ico"),
                    Path.Combine(baseDir, "appicon.ico"),
                    Path.Combine(baseDir, "..", "src", "assets", "appicon.ico"),
                    Path.Combine(baseDir, "..", "src", "appicon.ico")
                };
                foreach (string candidate in candidates)
                {
                    if (File.Exists(candidate))
                    {
                        using (var fs = new FileStream(candidate, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            var decoder = new IconBitmapDecoder(fs, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                            if (decoder.Frames.Count > 0)
                            {
                                foreach (var f in decoder.Frames)
                                {
                                    if (f.PixelWidth >= 48 && f.PixelWidth <= 128)
                                    {
                                        return f;
                                    }
                                }
                                return decoder.Frames[0];
                            }
                        }
                    }
                }
            }
            catch { }

            return null;
        }

        private void InitializePaths()
        {
            appDir = AppDomain.CurrentDomain.BaseDirectory;
            AppUrls.Initialize(appDir);

            // 1. Catalog path (apps.json in data/ or root)
            configPath = Path.Combine(appDir, "data", "apps.json");
            if (!File.Exists(configPath)) configPath = Path.Combine(appDir, "apps.json");

            // 2. Settings path (settings.json in config/ or root)
            settingsPath = Path.Combine(appDir, "config", "settings.json");
            if (!File.Exists(settingsPath)) settingsPath = Path.Combine(appDir, "settings.json");

            // 3. Log path (install-log.txt in logs/ or root)
            string logsDir = Path.Combine(appDir, "logs");
            if (!Directory.Exists(logsDir))
            {
                try { Directory.CreateDirectory(logsDir); } catch { }
            }
            logFilePath = Path.Combine(logsDir, "install-log.txt");

            // 4. Session Persistence & Reports directories
            string dataDir = Path.Combine(appDir, "data");
            if (!Directory.Exists(dataDir))
            {
                try { Directory.CreateDirectory(dataDir); } catch { }
            }
            sessionStatePath = Path.Combine(dataDir, "deployment_session.json");

            reportsDir = Path.Combine(appDir, "reports");
            if (!Directory.Exists(reportsDir))
            {
                try { Directory.CreateDirectory(reportsDir); } catch { }
            }

            jsonSerializer = new JavaScriptSerializer();
            jsonSerializer.MaxJsonLength = int.MaxValue;
            appRows = new List<AppRowView>();
            roleCards = new List<RoleCardView>();
            modalAppRows = new List<ModalAppItemView>();

            wingetCmd = FindWinget();
        }

        private string FindWinget()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("winget", "--version");
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                using (Process p = Process.Start(psi))
                {
                    p.WaitForExit();
                    if (p.ExitCode == 0) return "winget";
                }
            }
            catch { }

            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string userAlias = Path.Combine(localAppData, @"Microsoft\WindowsApps\winget.exe");
            if (File.Exists(userAlias)) return userAlias;

            string winAppsDir = @"C:\Program Files\WindowsApps";
            if (Directory.Exists(winAppsDir))
            {
                try
                {
                    string[] candidates = Directory.GetDirectories(winAppsDir, "*DesktopAppInstaller*");
                    if (candidates.Length > 0)
                    {
                        string candidateExe = Path.Combine(candidates[candidates.Length - 1], "winget.exe");
                        if (File.Exists(candidateExe)) return candidateExe;
                    }
                }
                catch { }
            }

            return null;
        }

        private SolidColorBrush HexBrush(string hex)
        {
            return new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex));
        }

        private void InitializeWindow()
        {
            this.Title = "FleetForge — Zero-Touch Machine Setup & Enterprise Software Deployment";
            this.Width = 1180;
            this.Height = 850;
            this.MinWidth = 980;
            this.MinHeight = 720;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Background = HexBrush("#202020");
            this.Foreground = HexBrush("#FFFFFF");
            this.FontFamily = new System.Windows.Media.FontFamily("Segoe UI Variable Text, Segoe UI Variable, Segoe UI, sans-serif");

            try
            {
                ImageSource appIcon = LoadApplicationIcon();
                if (appIcon != null)
                {
                    this.Icon = appIcon;
                }
            }
            catch { }

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
                                <Setter Property=""Opacity"" Value=""0.85""/>
                            </Trigger>
                            <Trigger Property=""IsPressed"" Value=""True"">
                                <Setter Property=""Opacity"" Value=""0.70""/>
                            </Trigger>
                            <Trigger Property=""IsEnabled"" Value=""False"">
                                <Setter Property=""Opacity"" Value=""0.45""/>
                            </Trigger>
                        </ControlTemplate.Triggers>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>
    </Grid.Resources>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width=""Auto""/>
        <ColumnDefinition Width=""*""/>
    </Grid.ColumnDefinitions>

    <!-- SIDEBAR -->
    <Border x:Name=""BorderSidebar"" Grid.Column=""0"" Width=""250"" Background=""#1E1E1E"" BorderBrush=""#2E2E2E"" BorderThickness=""0,0,1,0"">
        <Grid Margin=""10,14,10,14"">
            <Grid.RowDefinitions>
                <RowDefinition Height=""Auto""/>
                <RowDefinition Height=""*""/>
                <RowDefinition Height=""Auto""/>
            </Grid.RowDefinitions>

            <!-- Brand Header -->
            <StackPanel Grid.Row=""0"" Margin=""2,0,2,14"">
                <!-- App Title Row with Collapse Button -->
                <Grid Margin=""0,0,0,6"">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width=""Auto""/>
                        <ColumnDefinition Width=""*""/>
                        <ColumnDefinition Width=""Auto""/>
                    </Grid.ColumnDefinitions>

                    <Border x:Name=""BorderBrandLogo"" Grid.Column=""0"" Width=""28"" Height=""28"" Background=""Transparent"" BorderThickness=""0"" VerticalAlignment=""Center"">
                        <Grid>
                            <Image x:Name=""ImgBrandLogo"" Width=""26"" Height=""26"" RenderOptions.BitmapScalingMode=""HighQuality"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                            <TextBlock x:Name=""TxtFallbackBrand"" Text=""⚡"" FontSize=""18"" Foreground=""#0078D4"" HorizontalAlignment=""Center"" VerticalAlignment=""Center"" Visibility=""Collapsed""/>
                        </Grid>
                    </Border>

                    <StackPanel x:Name=""StackBrandText"" Grid.Column=""1"" Margin=""8,0,4,0"" VerticalAlignment=""Center"">
                        <TextBlock x:Name=""TxtBrandTitle"" Text=""FleetForge"" FontSize=""14"" FontWeight=""Bold"" Foreground=""#FFFFFF""/>
                    </StackPanel>

                    <Button x:Name=""BtnToggleSidebar"" Grid.Column=""2"" Width=""28"" Height=""28"" Background=""Transparent"" BorderThickness=""0"" Cursor=""Hand""
                            ToolTip=""Collapse / Expand Menu"" HorizontalAlignment=""Right"" VerticalAlignment=""Center"">
                        <TextBlock x:Name=""TxtToggleSidebarIcon"" Text=""&#xE700;"" FontFamily=""Segoe Fluent Icons, Segoe MDL2 Assets, Segoe UI Symbol""
                                   FontSize=""13"" Foreground=""#8E95A5"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                    </Button>
                </Grid>
            </StackPanel>

            <!-- Navigation Links -->
            <StackPanel Grid.Row=""1"">
                <!-- Nav 1: Machine Configuration -->
                <Button x:Name=""NavBtnSetup"" Height=""40"" Margin=""0,0,0,4"" Background=""#2C2C2C"" BorderThickness=""0"" Cursor=""Hand"" HorizontalContentAlignment=""Left"" ToolTip=""Quick Setup"">
                    <Grid Margin=""2,0,4,0"">
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width=""4""/>
                            <ColumnDefinition Width=""36""/>
                            <ColumnDefinition Width=""*""/>
                        </Grid.ColumnDefinitions>
                        <Border x:Name=""NavIndSetup"" Grid.Column=""0"" Width=""3"" Height=""16"" CornerRadius=""1.5"" Background=""#0078D4"" HorizontalAlignment=""Left"" VerticalAlignment=""Center""/>
                        <TextBlock x:Name=""NavIconSetup"" Grid.Column=""1"" Text=""&#xE770;"" FontFamily=""Segoe Fluent Icons, Segoe MDL2 Assets, Segoe UI Symbol"" FontSize=""16"" Foreground=""#4CC2FF"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                        <TextBlock x:Name=""NavTxtSetup"" Grid.Column=""2"" Text=""Quick Setup"" FontSize=""13"" FontWeight=""SemiBold"" Foreground=""#FFFFFF"" VerticalAlignment=""Center"" Margin=""6,0,0,0""/>
                    </Grid>
                </Button>

                <!-- Nav 2: Applications Catalog -->
                <Button x:Name=""NavBtnCatalog"" Height=""40"" Margin=""0,0,0,4"" Background=""Transparent"" BorderThickness=""0"" Cursor=""Hand"" HorizontalContentAlignment=""Left"" ToolTip=""All Apps"">
                    <Grid Margin=""2,0,4,0"">
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width=""4""/>
                            <ColumnDefinition Width=""36""/>
                            <ColumnDefinition Width=""*""/>
                        </Grid.ColumnDefinitions>
                        <Border x:Name=""NavIndCatalog"" Grid.Column=""0"" Width=""3"" Height=""16"" CornerRadius=""1.5"" Background=""#0078D4"" HorizontalAlignment=""Left"" VerticalAlignment=""Center"" Visibility=""Collapsed""/>
                        <TextBlock x:Name=""NavIconCatalog"" Grid.Column=""1"" Text=""&#xE71D;"" FontFamily=""Segoe Fluent Icons, Segoe MDL2 Assets, Segoe UI Symbol"" FontSize=""16"" Foreground=""#C5CCD8"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                        <TextBlock x:Name=""NavTxtCatalog"" Grid.Column=""2"" Text=""All Apps"" FontSize=""13"" Foreground=""#D0D6E0"" VerticalAlignment=""Center"" Margin=""6,0,0,0""/>
                    </Grid>
                </Button>

                <!-- Nav 3: Cloud Sync -->
                <Button x:Name=""NavBtnCloud"" Height=""40"" Margin=""0,0,0,4"" Background=""Transparent"" BorderThickness=""0"" Cursor=""Hand"" HorizontalContentAlignment=""Left"" ToolTip=""Cloud Sync"">
                    <Grid Margin=""2,0,4,0"">
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width=""4""/>
                            <ColumnDefinition Width=""36""/>
                            <ColumnDefinition Width=""*""/>
                        </Grid.ColumnDefinitions>
                        <Border x:Name=""NavIndCloud"" Grid.Column=""0"" Width=""3"" Height=""16"" CornerRadius=""1.5"" Background=""#0078D4"" HorizontalAlignment=""Left"" VerticalAlignment=""Center"" Visibility=""Collapsed""/>
                        <TextBlock x:Name=""NavIconCloud"" Grid.Column=""1"" Text=""&#xE753;"" FontFamily=""Segoe Fluent Icons, Segoe MDL2 Assets, Segoe UI Symbol"" FontSize=""16"" Foreground=""#C5CCD8"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                        <TextBlock x:Name=""NavTxtCloud"" Grid.Column=""2"" Text=""Cloud Sync"" FontSize=""13"" Foreground=""#D0D6E0"" VerticalAlignment=""Center"" Margin=""6,0,0,0""/>
                    </Grid>
                </Button>

                <!-- Nav 4: Activity & Logs -->
                <Button x:Name=""NavBtnLogs"" Height=""40"" Margin=""0,0,0,4"" Background=""Transparent"" BorderThickness=""0"" Cursor=""Hand"" HorizontalContentAlignment=""Left"" ToolTip=""Install History &amp; Logs"">
                    <Grid Margin=""2,0,4,0"">
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width=""4""/>
                            <ColumnDefinition Width=""36""/>
                            <ColumnDefinition Width=""*""/>
                        </Grid.ColumnDefinitions>
                        <Border x:Name=""NavIndLogs"" Grid.Column=""0"" Width=""3"" Height=""16"" CornerRadius=""1.5"" Background=""#0078D4"" HorizontalAlignment=""Left"" VerticalAlignment=""Center"" Visibility=""Collapsed""/>
                        <TextBlock x:Name=""NavIconLogs"" Grid.Column=""1"" Text=""&#xE756;"" FontFamily=""Segoe Fluent Icons, Segoe MDL2 Assets, Segoe UI Symbol"" FontSize=""16"" Foreground=""#C5CCD8"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                        <TextBlock x:Name=""NavTxtLogs"" Grid.Column=""2"" Text=""Install History"" FontSize=""13"" Foreground=""#D0D6E0"" VerticalAlignment=""Center"" Margin=""6,0,0,0""/>
                    </Grid>
                </Button>
            </StackPanel>

            <!-- Left Panel Bottom: Developer Branding -->
            <StackPanel x:Name=""StackSidebarFooter"" Grid.Row=""2"" HorizontalAlignment=""Center"" VerticalAlignment=""Bottom"" Margin=""0,0,0,10"" ToolTip=""FleetForge by Love Malhotra"">
                <TextBlock x:Name=""TxtSidebarFooterBy"" Text=""Design and Develop by""
                           FontSize=""11"" Foreground=""#8A8A8A""
                           HorizontalAlignment=""Center"" TextAlignment=""Center"" Margin=""0,0,0,3""/>
                <TextBlock x:Name=""TxtSidebarFooterCompany"" Text=""LOVE MALHOTRA""
                           FontSize=""11"" FontWeight=""Bold"" Foreground=""#EF4444""
                           HorizontalAlignment=""Center"" TextAlignment=""Center"" TextWrapping=""Wrap""/>
            </StackPanel>
        </Grid>
    </Border>

    <!-- MAIN CONTENT -->
    <Grid Grid.Column=""1"">
        <Grid.RowDefinitions>
            <RowDefinition Height=""Auto""/>
            <RowDefinition Height=""*""/>
            <RowDefinition Height=""Auto""/>
        </Grid.RowDefinitions>

        <!-- TOP BAR (Windows 11 Style) -->
        <Border x:Name=""BorderTopBar"" Grid.Row=""0"" Background=""#F3F3F3"" Padding=""28,12"" BorderBrush=""#E5E5E5"" BorderThickness=""0,0,0,1"">
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width=""*""/>
                    <ColumnDefinition Width=""Auto""/>
                </Grid.ColumnDefinitions>

                <!-- Windows 11 Fluent Search Box (Distinct, High-Contrast Capsule) -->
                <Border x:Name=""BorderHeaderSearch"" Grid.Column=""0"" Width=""440"" Height=""36"" CornerRadius=""18""
                        Background=""#FFFFFF"" BorderBrush=""#B0B8C4"" BorderThickness=""1""
                        HorizontalAlignment=""Left"" VerticalAlignment=""Center"">
                    <Grid Margin=""14,0,12,0"">
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width=""Auto""/>
                            <ColumnDefinition Width=""*""/>
                            <ColumnDefinition Width=""Auto""/>
                        </Grid.ColumnDefinitions>
                        <TextBlock x:Name=""TxtHeaderSearchIcon"" Text=""🔍"" FontSize=""12.5"" Foreground=""#475569"" VerticalAlignment=""Center"" Margin=""0,0,10,0""/>
                        <TextBox x:Name=""TxtSearch"" Grid.Column=""1"" Height=""28"" Background=""Transparent""
                                 Foreground=""#0F172A"" CaretBrush=""#0067C0"" BorderThickness=""0""
                                 FontSize=""12.5"" VerticalContentAlignment=""Center"" Text=""Find an app, setting or pack...""/>
                        <Button x:Name=""BtnClearSearch"" Grid.Column=""2"" Content=""✕"" Width=""20"" Height=""20""
                                Background=""Transparent"" Foreground=""#64748B"" BorderThickness=""0""
                                Cursor=""Hand"" Visibility=""Collapsed"" Margin=""4,0,0,0"" ToolTip=""Clear search""/>
                    </Grid>
                </Border>

                <!-- Actions on Right (Clean Windows 11 Help Menu button only) -->
                <StackPanel Grid.Column=""1"" Orientation=""Horizontal"" VerticalAlignment=""Center"">
                    <Button x:Name=""BtnHelpMenu"" Content=""Help ▾"" Background=""#FFFFFF"" Foreground=""#0F172A"" BorderBrush=""#D1D5DB"" BorderThickness=""1""
                            FontWeight=""SemiBold"" FontSize=""12"" Padding=""18,7"" Cursor=""Hand"" ToolTip=""How to Use, Settings, Updates &amp; About"" VerticalAlignment=""Center""/>
                </StackPanel>
            </Grid>
        </Border>

        <!-- PAGE 1: SETUP (Windows 11 Layout) -->
        <ScrollViewer x:Name=""ViewSetup"" Grid.Row=""1"" VerticalScrollBarVisibility=""Auto"" HorizontalScrollBarVisibility=""Disabled"" Margin=""28,18,28,18"" Padding=""0,0,10,0"">
            <StackPanel>
                <!-- BIG HEADING (Windows 11 Settings Style) -->
                <TextBlock x:Name=""TxtPageBigHeading"" Text=""Quick Setup"" FontSize=""28"" FontWeight=""Bold"" Foreground=""#FFFFFF"" Margin=""0,0,0,4""/>
                <TextBlock x:Name=""TxtPageBigSubtitle"" Text=""Choose an app pack below or review individual applications to set up this computer in one click.""
                           FontSize=""12.5"" Foreground=""#A0A0A0"" Margin=""0,0,0,18""/>

                <!-- Session Resume Alert Banner (Conditional) -->
                <Border x:Name=""BannerResumeSetup"" Background=""#2B2313"" CornerRadius=""8"" Padding=""16,12"" BorderBrush=""#854D0E"" BorderThickness=""1"" Margin=""0,0,0,16"" Visibility=""Collapsed"">
                    <Grid>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width=""Auto""/>
                            <ColumnDefinition Width=""*""/>
                            <ColumnDefinition Width=""Auto""/>
                        </Grid.ColumnDefinitions>
                        <Border Grid.Column=""0"" Width=""36"" Height=""36"" Background=""#D97706"" CornerRadius=""6"" Margin=""0,0,14,0"" VerticalAlignment=""Center"">
                            <TextBlock Text=""&#xE7BA;"" FontFamily=""Segoe Fluent Icons, Segoe MDL2 Assets, Segoe UI Symbol"" FontSize=""18"" Foreground=""#FFFFFF"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                        </Border>
                        <StackPanel Grid.Column=""1"" VerticalAlignment=""Center"">
                            <TextBlock x:Name=""TxtResumeBannerTitle"" Text=""Unfinished Installation Found"" FontSize=""13"" FontWeight=""Bold"" Foreground=""#FDE047""/>
                            <TextBlock x:Name=""TxtResumeBannerDesc"" Text=""A previous installation was stopped before finishing. Click Resume to install the remaining apps."" FontSize=""11"" Foreground=""#E2E8F0"" Margin=""0,2,0,0"" TextWrapping=""Wrap""/>
                        </StackPanel>
                        <StackPanel Grid.Column=""2"" Orientation=""Horizontal"" VerticalAlignment=""Center"" Margin=""12,0,0,0"">
                            <Button x:Name=""BtnResumeSession"" Content=""Resume Installation"" Background=""#F59E0B"" Foreground=""#000000"" FontWeight=""Bold"" FontSize=""11"" Padding=""12,6"" Margin=""0,0,8,0"" Cursor=""Hand"" BorderThickness=""0""/>
                            <Button x:Name=""BtnDismissResume"" Content=""Dismiss"" Background=""#374151"" Foreground=""#E2E8F0"" FontSize=""11"" Padding=""10,6"" Cursor=""Hand"" BorderThickness=""0""/>
                        </StackPanel>
                    </Grid>
                </Border>

                <!-- Friendly 3-Step Guide Strip for Non-Technical Users -->
                <Border x:Name=""BorderStepsGuide"" Background=""#162032"" BorderBrush=""#1E3A5F"" BorderThickness=""1"" CornerRadius=""8"" Padding=""16,12"" Margin=""0,0,0,18"">
                    <Grid>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width=""*""/>
                            <ColumnDefinition Width=""Auto""/>
                            <ColumnDefinition Width=""*""/>
                            <ColumnDefinition Width=""Auto""/>
                            <ColumnDefinition Width=""*""/>
                        </Grid.ColumnDefinitions>

                        <!-- Step 1 -->
                        <Grid Grid.Column=""0"">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width=""Auto""/>
                                <ColumnDefinition Width=""*""/>
                            </Grid.ColumnDefinitions>
                            <Border x:Name=""BorderStep1Circle"" Grid.Column=""0"" Width=""28"" Height=""28"" CornerRadius=""14"" Background=""#0078D4"" VerticalAlignment=""Center"" Margin=""0,0,10,0"">
                                <TextBlock Text=""1"" FontWeight=""Bold"" FontSize=""12"" Foreground=""#FFFFFF"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                            </Border>
                            <StackPanel Grid.Column=""1"" VerticalAlignment=""Center"">
                                <TextBlock x:Name=""TxtStep1Title"" Text=""Step 1: Pick an App Pack"" FontWeight=""Bold"" FontSize=""12"" Foreground=""#FFFFFF""/>
                                <TextBlock x:Name=""TxtStep1Desc"" Text=""Click any pack card below"" FontSize=""11"" Foreground=""#94A3B8""/>
                            </StackPanel>
                        </Grid>

                        <!-- Arrow 1 -->
                        <TextBlock x:Name=""TxtStepArrow1"" Grid.Column=""1"" Text=""➔"" FontSize=""13"" Foreground=""#475569"" VerticalAlignment=""Center"" Margin=""10,0,10,0""/>

                        <!-- Step 2 -->
                        <Grid Grid.Column=""2"">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width=""Auto""/>
                                <ColumnDefinition Width=""*""/>
                            </Grid.ColumnDefinitions>
                            <Border x:Name=""BorderStep2Circle"" Grid.Column=""0"" Width=""28"" Height=""28"" CornerRadius=""14"" Background=""#0284C7"" VerticalAlignment=""Center"" Margin=""0,0,10,0"">
                                <TextBlock Text=""2"" FontWeight=""Bold"" FontSize=""12"" Foreground=""#FFFFFF"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                            </Border>
                            <StackPanel Grid.Column=""1"" VerticalAlignment=""Center"">
                                <TextBlock x:Name=""TxtStep2Title"" Text=""Step 2: Review Apps"" FontWeight=""Bold"" FontSize=""12"" Foreground=""#FFFFFF""/>
                                <TextBlock x:Name=""TxtStep2Desc"" Text=""Click Customize Apps if needed"" FontSize=""11"" Foreground=""#94A3B8""/>
                            </StackPanel>
                        </Grid>

                        <!-- Arrow 2 -->
                        <TextBlock x:Name=""TxtStepArrow2"" Grid.Column=""3"" Text=""➔"" FontSize=""13"" Foreground=""#475569"" VerticalAlignment=""Center"" Margin=""10,0,10,0""/>

                        <!-- Step 3 -->
                        <Grid Grid.Column=""4"">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width=""Auto""/>
                                <ColumnDefinition Width=""*""/>
                            </Grid.ColumnDefinitions>
                            <Border x:Name=""BorderStep3Circle"" Grid.Column=""0"" Width=""28"" Height=""28"" CornerRadius=""14"" Background=""#10B981"" VerticalAlignment=""Center"" Margin=""0,0,10,0"">
                                <TextBlock Text=""3"" FontWeight=""Bold"" FontSize=""12"" Foreground=""#FFFFFF"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                            </Border>
                            <StackPanel Grid.Column=""1"" VerticalAlignment=""Center"">
                                <TextBlock x:Name=""TxtStep3Title"" Text=""Step 3: Click Install"" FontWeight=""Bold"" FontSize=""12"" Foreground=""#FFFFFF""/>
                                <TextBlock x:Name=""TxtStep3Desc"" Text=""Installs quietly in the background"" FontSize=""11"" Foreground=""#94A3B8""/>
                            </StackPanel>
                        </Grid>
                    </Grid>
                </Border>

                <!-- Section Header: More Options / Available App Packs -->
                <Grid Margin=""0,0,0,14"">
                    <StackPanel VerticalAlignment=""Center"">
                        <TextBlock x:Name=""TxtTargetRoleTitle"" Text=""Available App Packs"" FontSize=""15"" FontWeight=""Bold"" Foreground=""#FFFFFF""/>
                        <TextBlock x:Name=""TxtTargetRoleSubtitle"" Text=""Select a ready-made pack, or customize and export packs as needed.""
                                   FontSize=""11.5"" Foreground=""#A0A0A0"" Margin=""0,2,0,0""/>
                    </StackPanel>

                    <StackPanel Orientation=""Horizontal"" HorizontalAlignment=""Right"" VerticalAlignment=""Center"">
                        <Button x:Name=""BtnImportPack"" Content=""Import Pack""
                                Background=""#2D2D2D"" Foreground=""#FFFFFF"" BorderBrush=""#3D3D3D"" BorderThickness=""1""
                                FontWeight=""SemiBold"" FontSize=""12"" Padding=""14,8"" Margin=""0,0,8,0""
                                Cursor=""Hand"" ToolTip=""Import a custom app pack (.json)""/>
                        <Button x:Name=""BtnCreateNewGroup"" Content=""Create New Pack""
                                Background=""#10B981"" Foreground=""#FFFFFF"" FontWeight=""Bold"" FontSize=""12""
                                Padding=""16,8"" Cursor=""Hand"" BorderThickness=""0""
                                ToolTip=""Create a new custom app pack""/>
                    </StackPanel>
                </Grid>

                <UniformGrid x:Name=""RoleCardsGrid"" Columns=""2"" Margin=""0,0,0,10""/>
            </StackPanel>
        </ScrollViewer>

        <!-- PAGE 2: CATALOG -->
        <Grid x:Name=""ViewCatalog"" Grid.Row=""1"" Visibility=""Collapsed"" Margin=""28,18,28,18"">
            <Grid.RowDefinitions>
                <RowDefinition Height=""Auto""/>
                <RowDefinition Height=""Auto""/>
                <RowDefinition Height=""*""/>
            </Grid.RowDefinitions>

            <!-- BIG HEADING -->
            <StackPanel Grid.Row=""0"" Margin=""0,0,0,14"">
                <TextBlock x:Name=""TxtCatalogBigHeading"" Text=""All Applications"" FontSize=""28"" FontWeight=""Bold"" Foreground=""#FFFFFF"" Margin=""0,0,0,4""/>
                <TextBlock x:Name=""TxtCatalogBigSubtitle"" Text=""Browse 16,349 software packages with instant search and one-click selection.""
                           FontSize=""12.5"" Foreground=""#A0A0A0""/>
            </StackPanel>

            <!-- Expanded Modern Category Tabs Deck -->
            <Border x:Name=""BorderCategoryBar"" Grid.Row=""1"" Background=""#1E222A"" CornerRadius=""8""
                    Padding=""8,8,2,2"" BorderBrush=""#2D3342"" BorderThickness=""1"" Margin=""0,0,0,14"">
                <WrapPanel x:Name=""CategoryChipsPanel"" Orientation=""Horizontal""/>
            </Border>

            <Border x:Name=""BorderCatalogContainer"" Grid.Row=""2"" Background=""#272727"" CornerRadius=""8"" Padding=""14,10"" BorderBrush=""#383838"" BorderThickness=""1"">
                <Grid>
                    <Grid.RowDefinitions>
                        <RowDefinition Height=""Auto""/>
                        <RowDefinition Height=""*""/>
                    </Grid.RowDefinitions>

                    <!-- Top Software List Header Bar with Single Master CheckBox and Sort Dropdown -->
                    <Border x:Name=""BorderCatalogHeaderBar"" Grid.Row=""0"" Margin=""0,0,0,8"" Padding=""4,4,4,8"" BorderThickness=""0,0,0,1"" BorderBrush=""#383838"">
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width=""Auto""/>
                                <ColumnDefinition Width=""*""/>
                                <ColumnDefinition Width=""Auto""/>
                            </Grid.ColumnDefinitions>
                            <CheckBox x:Name=""ChkSelectAllCatalog"" Grid.Column=""0"" Content=""Select All Apps"" FontSize=""12"" FontWeight=""SemiBold"" Foreground=""#FFFFFF"" VerticalAlignment=""Center"" Cursor=""Hand""/>
                            <TextBlock x:Name=""TxtCatalogListHeaderCount"" Grid.Column=""1"" HorizontalAlignment=""Right"" VerticalAlignment=""Center"" FontSize=""11"" Foreground=""#A0A0A0"" Margin=""0,0,14,0"" Text=""""/>
                            <Button x:Name=""BtnCatalogSort"" Grid.Column=""2"" Content=""Sort Apps ▾""
                                    Background=""#2D2D2D"" Foreground=""#4CC2FF"" FontWeight=""SemiBold"" FontSize=""11""
                                    Padding=""10,4"" Cursor=""Hand"" BorderThickness=""1"" BorderBrush=""#3D3D3D"" VerticalAlignment=""Center""/>
                        </Grid>
                    </Border>

                    <!-- Software Cards List -->
                    <ScrollViewer x:Name=""CatalogScrollViewer"" Grid.Row=""1"" VerticalScrollBarVisibility=""Auto"" HorizontalScrollBarVisibility=""Disabled"">
                        <StackPanel x:Name=""AppsContainer""/>
                    </ScrollViewer>
                </Grid>
            </Border>
        </Grid>

        <!-- PAGE 3: CLOUD SYNC -->
        <ScrollViewer x:Name=""ViewCloud"" Grid.Row=""1"" Visibility=""Collapsed"" VerticalScrollBarVisibility=""Auto"" Margin=""28,18,28,18"" Padding=""0,0,10,0"">
            <StackPanel>
                <!-- BIG HEADING -->
                <TextBlock x:Name=""TxtCloudBigHeading"" Text=""Cloud Sync"" FontSize=""28"" FontWeight=""Bold"" Foreground=""#FFFFFF"" Margin=""0,0,0,4""/>
                <TextBlock x:Name=""TxtCloudBigSubtitle"" Text=""Synchronize your software catalog from GitHub or remote URLs.""
                           FontSize=""12.5"" Foreground=""#A0A0A0"" Margin=""0,0,0,16""/>

                <Border x:Name=""BorderCloudCard1"" Background=""#202020"" CornerRadius=""10"" Padding=""20"" BorderBrush=""#333333"" BorderThickness=""1"" Margin=""0,0,0,20"">
                    <StackPanel>
                        <TextBlock x:Name=""TxtCloudCard1Title"" Text=""Online App List Link (GitHub or Google Drive Link)"" FontSize=""13"" FontWeight=""Bold"" Foreground=""#FFFFFF"" Margin=""0,0,0,6""/>
                        <TextBox x:Name=""TxtCloudUrlInput"" Height=""38"" Background=""#141414"" Foreground=""#4CC2FF""
                                 CaretBrush=""#4CC2FF"" BorderBrush=""#3D3D3D"" BorderThickness=""1"" Padding=""12,8""
                                 FontSize=""12"" VerticalContentAlignment=""Center""/>

                        <Grid Margin=""0,14,0,0"">
                            <CheckBox x:Name=""ChkAutoSync"" Content=""Check for list updates on startup"" IsChecked=""True""
                                      Foreground=""#C8C8C8"" FontSize=""12"" VerticalAlignment=""Center"" Cursor=""Hand""/>

                            <StackPanel Orientation=""Horizontal"" HorizontalAlignment=""Right"">
                                <Button x:Name=""BtnSaveCloudUrl"" Content=""Save Link"" Background=""#2D2D2D"" Foreground=""#F3F3F3""
                                        Padding=""14,8"" Margin=""0,0,8,0"" Cursor=""Hand"" BorderThickness=""1"" BorderBrush=""#3D3D3D"" FontSize=""12""/>
                                <Button x:Name=""BtnSyncNow"" Content=""Update List Now"" Background=""#0078D4"" Foreground=""#FFFFFF""
                                        FontWeight=""Bold"" Padding=""18,8"" Cursor=""Hand"" BorderThickness=""0"" FontSize=""12""/>
                            </StackPanel>
                        </Grid>

                        <Border x:Name=""SyncResultBorder"" Background=""#1A2E20"" CornerRadius=""6"" Padding=""12,8"" Margin=""0,12,0,0"" Visibility=""Collapsed"">
                            <TextBlock x:Name=""TxtSyncResult"" Text=""Synced successfully!"" FontSize=""12"" Foreground=""#4ADE80""/>
                        </Border>
                    </StackPanel>
                </Border>

                <!-- Quick Actions: Export Sample & Edit Local -->
                <StackPanel Orientation=""Horizontal"">
                    <Button x:Name=""BtnExportSampleJson"" Content=""Export App List"" Background=""#2D2D2D"" Foreground=""#F3F3F3""
                            Padding=""14,8"" Margin=""0,0,10,0"" Cursor=""Hand"" BorderThickness=""1"" BorderBrush=""#3D3D3D"" FontSize=""12"" FontWeight=""SemiBold""/>
                    <Button x:Name=""BtnOpenLocalCatalog"" Content=""Open App List"" Background=""#2D2D2D"" Foreground=""#F3F3F3""
                            Padding=""14,8"" Cursor=""Hand"" BorderThickness=""1"" BorderBrush=""#3D3D3D"" FontSize=""12"" FontWeight=""SemiBold""/>
                </StackPanel>
            </StackPanel>
        </ScrollViewer>

        <!-- PAGE 4: LOGS -->
        <Grid x:Name=""ViewLogs"" Grid.Row=""1"" Visibility=""Collapsed"" Margin=""28,18,28,18"">
            <Grid.RowDefinitions>
                <RowDefinition Height=""Auto""/>
                <RowDefinition Height=""Auto""/>
                <RowDefinition Height=""*""/>
            </Grid.RowDefinitions>

            <!-- BIG HEADING -->
            <StackPanel Grid.Row=""0"" Margin=""0,0,0,14"">
                <TextBlock x:Name=""TxtLogsBigHeading"" Text=""Install History"" FontSize=""28"" FontWeight=""Bold"" Foreground=""#FFFFFF"" Margin=""0,0,0,4""/>
                <TextBlock x:Name=""TxtLogsBigSubtitle"" Text=""Live deployment watchdog, stdout streaming, and audit logs.""
                           FontSize=""12.5"" Foreground=""#A0A0A0""/>
            </StackPanel>

            <Grid Grid.Row=""1"" Margin=""0,0,0,10"">
                <TextBlock x:Name=""TxtLogsHeader"" Text=""Installation Progress &amp; History"" FontSize=""14"" FontWeight=""SemiBold"" Foreground=""#FFFFFF"" VerticalAlignment=""Center""/>
                <StackPanel Orientation=""Horizontal"" HorizontalAlignment=""Right"">
                    <Button x:Name=""BtnViewReport"" Content=""Summary Report"" Background=""#0078D4"" Foreground=""#FFFFFF""
                            Padding=""12,5"" Margin=""0,0,6,0"" Cursor=""Hand"" BorderThickness=""0"" FontSize=""11"" Visibility=""Collapsed""/>
                    <Button x:Name=""BtnOpenLogFile"" Content=""Open Log File"" Background=""#2D2D2D"" Foreground=""#F3F3F3""
                            Padding=""12,5"" Margin=""0,0,6,0"" Cursor=""Hand"" BorderThickness=""1"" BorderBrush=""#3D3D3D"" FontSize=""11""/>
                    <Button x:Name=""BtnClearLogConsole"" Content=""Clear Screen"" Background=""#2D2D2D"" Foreground=""#F3F3F3""
                            Padding=""12,5"" Cursor=""Hand"" BorderThickness=""1"" BorderBrush=""#3D3D3D"" FontSize=""11""/>
                </StackPanel>
            </Grid>

            <Border x:Name=""BorderLogTerminal"" Grid.Row=""2"" Background=""#0C0E14"" CornerRadius=""8"" Padding=""14"" BorderBrush=""#2D2D2D"" BorderThickness=""1"">
                <ScrollViewer x:Name=""LogScrollViewer"" VerticalScrollBarVisibility=""Auto"">
                    <TextBox x:Name=""TxtLog"" Background=""Transparent"" Foreground=""#6EE7B7"" BorderThickness=""0""
                             FontFamily=""Consolas, Lucida Console"" FontSize=""11"" IsReadOnly=""True"" TextWrapping=""Wrap""/>
                </ScrollViewer>
            </Border>
        </Grid>

        <!-- BOTTOM STICKY ACTION BAR -->
        <Border x:Name=""BorderBottomBar"" Grid.Row=""2"" Background=""#1F1F1F"" Padding=""24,14"" BorderBrush=""#333333"" BorderThickness=""0,1,0,0"">
            <Grid VerticalAlignment=""Center"">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width=""Auto""/>
                    <ColumnDefinition Width=""*""/>
                    <ColumnDefinition Width=""Auto""/>
                </Grid.ColumnDefinitions>
                <Button x:Name=""BtnSearchWingetOnlineBottom"" Grid.Column=""0"" Content=""Search Online Apps""
                        Background=""#7C3AED"" Foreground=""#FFFFFF"" FontWeight=""SemiBold""
                        Padding=""14,10"" Cursor=""Hand"" BorderThickness=""0"" FontSize=""12""
                        HorizontalAlignment=""Left"" VerticalAlignment=""Center""
                        Visibility=""Collapsed""/>
                <StackPanel Grid.Column=""2"" Orientation=""Horizontal"" HorizontalAlignment=""Right"" VerticalAlignment=""Center"">
                    <Button x:Name=""BtnOpenOfflineFolder"" Content=""Saved Installers"" Background=""#222222"" Foreground=""#C8C8C8""
                            Padding=""14,10"" Margin=""0,0,10,0"" Cursor=""Hand"" BorderThickness=""1"" BorderBrush=""#333333"" FontSize=""12""/>
                    <Button x:Name=""BtnStop"" Content=""Stop Install""
                            Background=""#DC2626"" Foreground=""#FFFFFF"" FontWeight=""Bold"" FontSize=""13""
                            Padding=""22,10"" Margin=""0,0,10,0"" Cursor=""Hand"" BorderThickness=""0"" Visibility=""Collapsed""/>
                    <Button x:Name=""BtnInstall"" Content=""Install Selected Apps""
                            Background=""#0078D4"" Foreground=""#FFFFFF"" FontWeight=""Bold"" FontSize=""13""
                            Padding=""28,10"" Cursor=""Hand"" BorderThickness=""0""/>
                </StackPanel>
            </Grid>
        </Border>
    </Grid>

    <!-- ========================================================================= -->
    <!-- GROUP EDITOR MODAL OVERLAY (CREATE / UPDATE GROUP)                        -->
    <!-- ========================================================================= -->
    <Grid x:Name=""GroupModalOverlay"" Grid.ColumnSpan=""2"" Background=""#B0000000"" Visibility=""Collapsed"">
        <Border x:Name=""BorderModalCard"" Width=""960"" MaxHeight=""780"" Background=""#1F1F1F"" CornerRadius=""12"" BorderBrush=""#3A3A3A"" BorderThickness=""1""
                HorizontalAlignment=""Center"" VerticalAlignment=""Center"" Padding=""28,22,28,22"">
            <Grid>
                <Grid.RowDefinitions>
                    <RowDefinition Height=""Auto""/> <!-- Title & Close -->
                    <RowDefinition Height=""Auto""/> <!-- Name & Icon -->
                    <RowDefinition Height=""Auto""/> <!-- Emoji Quick Select Chips -->
                    <RowDefinition Height=""Auto""/> <!-- Description -->
                    <RowDefinition Height=""Auto""/> <!-- App Search & Quick Controls -->
                    <RowDefinition Height=""*"" MinHeight=""360"" MaxHeight=""440""/> <!-- App Checklist -->
                    <RowDefinition Height=""Auto""/> <!-- Bottom Actions -->
                </Grid.RowDefinitions>

                <!-- 0. Header -->
                <Grid Grid.Row=""0"" Margin=""0,0,0,16"">
                    <TextBlock x:Name=""TxtModalTitle"" Text=""Create New App Pack"" FontSize=""19"" FontWeight=""Bold"" Foreground=""#FFFFFF"" VerticalAlignment=""Center""/>
                    <Button x:Name=""BtnCloseModal"" Content=""✕"" Background=""Transparent"" Foreground=""#8A8A8A"" FontSize=""14"" FontWeight=""Bold""
                            Width=""30"" Height=""30"" Cursor=""Hand"" BorderThickness=""0"" HorizontalAlignment=""Right"" VerticalAlignment=""Center""/>
                </Grid>

                <!-- 1. Name & Icon -->
                <Grid Grid.Row=""1"" Margin=""0,0,0,10"">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width=""70""/>
                        <ColumnDefinition Width=""*""/>
                    </Grid.ColumnDefinitions>

                    <StackPanel Grid.Column=""0"" Margin=""0,0,10,0"">
                        <TextBlock Text=""Icon"" FontSize=""11"" FontWeight=""SemiBold"" Foreground=""#8A8A8A"" Margin=""0,0,0,4""/>
                        <TextBox x:Name=""TxtGroupIconInput"" Text=""🚀"" Height=""34"" Background=""#141414"" Foreground=""#FFFFFF""
                                 FontFamily=""Segoe UI Emoji, Segoe UI Symbol, Segoe UI""
                                 FontSize=""16"" TextAlignment=""Center"" VerticalContentAlignment=""Center"" BorderBrush=""#333333"" BorderThickness=""1""/>
                    </StackPanel>

                    <StackPanel Grid.Column=""1"">
                        <TextBlock Text=""Pack Name"" FontSize=""11"" FontWeight=""SemiBold"" Foreground=""#8A8A8A"" Margin=""0,0,0,4""/>
                        <TextBox x:Name=""TxtGroupNameInput"" Height=""34"" Background=""#141414"" Foreground=""#FFFFFF""
                                 FontSize=""13"" Padding=""8,6"" VerticalContentAlignment=""Center"" BorderBrush=""#333333"" BorderThickness=""1""/>
                    </StackPanel>
                </Grid>

                <!-- 2. Emoji Chips -->
                <StackPanel Grid.Row=""2"" Margin=""0,0,0,12"">
                    <TextBlock Text=""Quick Icon Select:"" FontSize=""10"" Foreground=""#707070"" Margin=""0,0,0,4""/>
                    <WrapPanel x:Name=""EmojiChipsPanel""/>
                </StackPanel>

                <!-- 3. Description -->
                <StackPanel Grid.Row=""3"" Margin=""0,0,0,14"">
                    <TextBlock Text=""Description"" FontSize=""11"" FontWeight=""SemiBold"" Foreground=""#8A8A8A"" Margin=""0,0,0,4""/>
                    <TextBox x:Name=""TxtGroupDescInput"" Height=""34"" Background=""#141414"" Foreground=""#FFFFFF""
                             FontSize=""12"" Padding=""8,6"" VerticalContentAlignment=""Center"" BorderBrush=""#333333"" BorderThickness=""1""/>
                </StackPanel>

                <!-- 4. App Checklist Header & Search -->
                <Grid Grid.Row=""4"" Margin=""0,0,0,8"">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width=""280""/>
                        <ColumnDefinition Width=""*""/>
                    </Grid.ColumnDefinitions>

                    <TextBox x:Name=""TxtModalAppSearch"" Grid.Column=""0"" Height=""32"" Background=""#141414"" Foreground=""#FFFFFF""
                             FontSize=""12"" Padding=""10,6"" VerticalContentAlignment=""Center"" BorderBrush=""#333333"" BorderThickness=""1""
                             Text=""Filter software...""/>

                    <StackPanel Grid.Column=""1"" Orientation=""Horizontal"" HorizontalAlignment=""Right"" VerticalAlignment=""Center"">
                        <Button x:Name=""BtnModalFilterTabAll"" Content=""All Software"" Background=""#0078D4"" Foreground=""#FFFFFF""
                                Padding=""9,5"" Margin=""0,0,5,0"" Cursor=""Hand"" BorderThickness=""0"" FontSize=""11"" FontWeight=""SemiBold""/>
                        <Button x:Name=""BtnModalFilterTabSelected"" Content=""Selected (0)"" Background=""#2B2B2B"" Foreground=""#CCCCCC""
                                Padding=""9,5"" Margin=""0,0,12,0"" Cursor=""Hand"" BorderThickness=""1"" BorderBrush=""#3D3D3D"" FontSize=""11""/>
                        <TextBlock x:Name=""TxtModalSelectedCount"" Text=""0 selected"" FontSize=""11"" Foreground=""#4CC2FF"" FontWeight=""SemiBold"" VerticalAlignment=""Center"" Margin=""0,0,10,0""/>
                        <Button x:Name=""BtnModalSelectAll"" Content=""Select All"" Background=""#2B2B2B"" Foreground=""#CCCCCC""
                                Padding=""8,5"" Margin=""0,0,5,0"" Cursor=""Hand"" BorderThickness=""1"" BorderBrush=""#3D3D3D"" FontSize=""11""/>
                        <Button x:Name=""BtnModalClearAll"" Content=""Clear"" Background=""#2B2B2B"" Foreground=""#CCCCCC""
                                Padding=""8,5"" Cursor=""Hand"" BorderThickness=""1"" BorderBrush=""#3D3D3D"" FontSize=""11""/>
                    </StackPanel>
                </Grid>

                <!-- 5. App Checklist -->
                <Border x:Name=""BorderModalAppsContainer"" Grid.Row=""5"" Background=""#141414"" CornerRadius=""8"" Padding=""10"" BorderBrush=""#2D2D2D"" BorderThickness=""1"" Margin=""0,0,0,16"">
                    <ScrollViewer x:Name=""ModalAppsScrollViewer"" VerticalScrollBarVisibility=""Auto"" HorizontalScrollBarVisibility=""Disabled"">
                        <StackPanel x:Name=""ModalAppsContainer""/>
                    </ScrollViewer>
                </Border>

                <!-- 6. Footer Actions -->
                <Grid Grid.Row=""6"">
                    <StackPanel Orientation=""Horizontal"" HorizontalAlignment=""Left"">
                        <Button x:Name=""BtnDeleteGroup"" Content=""Delete Pack"" Background=""#3D1A1A"" Foreground=""#F87171""
                                FontWeight=""SemiBold"" Padding=""14,8"" Cursor=""Hand"" BorderThickness=""1"" BorderBrush=""#7F1D1D""
                                FontSize=""12"" Visibility=""Collapsed""/>
                        <Button x:Name=""BtnModalExportGroup"" Content=""Export Pack"" Background=""#2B2B2B"" Foreground=""#E0E0E0""
                                FontWeight=""SemiBold"" Padding=""14,8"" Margin=""8,0,0,0"" Cursor=""Hand"" BorderThickness=""1"" BorderBrush=""#3D3D3D""
                                FontSize=""12"" Visibility=""Collapsed"" ToolTip=""Export this pack to a .json file""/>
                    </StackPanel>

                    <StackPanel Orientation=""Horizontal"" HorizontalAlignment=""Right"">
                        <Button x:Name=""BtnCancelModal"" Content=""Cancel"" Background=""#2B2B2B"" Foreground=""#F3F3F3""
                                Padding=""18,8"" Margin=""0,0,10,0"" Cursor=""Hand"" BorderThickness=""1"" BorderBrush=""#3D3D3D"" FontSize=""12""/>
                        <Button x:Name=""BtnSaveGroup"" Content=""Save Pack"" Background=""#0078D4"" Foreground=""#FFFFFF""
                                FontWeight=""Bold"" Padding=""22,8"" Cursor=""Hand"" BorderThickness=""0"" FontSize=""12""/>
                    </StackPanel>
                </Grid>
            </Grid>
        </Border>
    </Grid>

    <!-- ========================================================================= -->
    <!-- MODAL 1: CHECK FOR UPDATES                                                -->
    <!-- ========================================================================= -->
    <Grid x:Name=""ModalUpdatesOverlay"" Grid.ColumnSpan=""2"" Background=""#75000000"" Visibility=""Collapsed"">
        <Border x:Name=""BorderUpdatesCard"" Width=""580"" Background=""#181B22"" CornerRadius=""16"" BorderBrush=""#2D3342"" BorderThickness=""1""
                HorizontalAlignment=""Center"" VerticalAlignment=""Center"" Padding=""28"">
            <StackPanel>
                <!-- Title Row -->
                <Grid Margin=""0,0,0,18"">
                    <StackPanel Orientation=""Horizontal"" VerticalAlignment=""Center"">
                        <Border x:Name=""BorderModalUpdatesIcon"" Width=""34"" Height=""34"" Background=""#0078D4"" CornerRadius=""8"" Margin=""0,0,12,0"" VerticalAlignment=""Center"">
                            <Path x:Name=""PathModalUpdatesIcon"" Data=""M12 4V1L8 5l4 4V6c3.31 0 6 2.69 6 6 0 1.01-.25 1.97-.7 2.8l1.46 1.46C19.54 15.03 20 13.57 20 12c0-4.42-3.58-8-8-8zm0 14c-3.31 0-6-2.69-6-6 0-1.01.25-1.97.7-2.8L5.24 7.74C4.46 8.97 4 10.43 4 12c0 4.42 3.58 8 8 8v3l4-4-4-4v3z""
                                  Fill=""#FFFFFF"" Width=""16"" Height=""16"" Stretch=""Uniform"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                        </Border>
                        <TextBlock x:Name=""TxtModalUpdatesTitle"" Text=""FleetForge Update Center"" FontSize=""18"" FontWeight=""Bold"" Foreground=""#FFFFFF"" VerticalAlignment=""Center""/>
                    </StackPanel>
                    <Button x:Name=""BtnCloseUpdates"" Content=""✕"" Background=""Transparent"" Foreground=""#8A8A8A"" FontSize=""15"" FontWeight=""Bold""
                            Width=""32"" Height=""32"" Cursor=""Hand"" BorderThickness=""0"" HorizontalAlignment=""Right"" VerticalAlignment=""Center""/>
                </Grid>

                <!-- Status Card -->
                <Border x:Name=""BorderUpdateStatusCard"" Background=""#11141A"" CornerRadius=""10"" Padding=""16"" BorderBrush=""#232938"" BorderThickness=""1"" Margin=""0,0,0,14"">
                    <Grid>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width=""Auto""/>
                            <ColumnDefinition Width=""*""/>
                        </Grid.ColumnDefinitions>
                        <Border x:Name=""BorderUpdateIconBadge"" Grid.Column=""0"" Width=""44"" Height=""44"" Background=""#064E3B"" CornerRadius=""10"" BorderBrush=""#059669"" BorderThickness=""1"" Margin=""0,0,14,0"" VerticalAlignment=""Center"">
                            <Path x:Name=""PathUpdateStatusIcon"" Data=""M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z""
                                  Fill=""#10B981"" Width=""22"" Height=""22"" Stretch=""Uniform"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                        </Border>
                        <StackPanel Grid.Column=""1"" VerticalAlignment=""Center"">
                            <TextBlock x:Name=""TxtUpdateTitle"" Text=""You are running the latest version!"" FontSize=""14"" FontWeight=""Bold"" Foreground=""#34D399""/>
                            <TextBlock x:Name=""TxtUpdateDetail"" Text=""FleetForge v2.6.1 (Enterprise IT Edition • Love Malhotra) is up to date."" FontSize=""11.5"" Foreground=""#94A3B8"" Margin=""0,3,0,0""/>
                        </StackPanel>
                    </Grid>
                </Border>

                <!-- What's New / Changelog Section -->
                <Border x:Name=""BorderChangelogCard"" Background=""#11141A"" CornerRadius=""10"" Padding=""16"" BorderBrush=""#232938"" BorderThickness=""1"" Margin=""0,0,0,16"">
                    <StackPanel>
                        <TextBlock x:Name=""TxtChangelogHeading"" Text=""Release Highlights &amp; Changelog:"" FontSize=""12"" FontWeight=""SemiBold"" Foreground=""#E2E8F0"" Margin=""0,0,0,10""/>
                        <StackPanel x:Name=""PnlChangelogList"">
                            <TextBlock Text=""• Zero-touch silent installation engine for Microsoft Winget"" FontSize=""11.5"" Foreground=""#94A3B8"" Margin=""0,2.5,0,2.5""/>
                            <TextBlock Text=""• Pre-cached enterprise catalog with 16,349 software packages"" FontSize=""11.5"" Foreground=""#94A3B8"" Margin=""0,2.5,0,2.5""/>
                            <TextBlock Text=""• Real-time synchronization with Windows Light / Dark themes"" FontSize=""11.5"" Foreground=""#94A3B8"" Margin=""0,2.5,0,2.5""/>
                            <TextBlock Text=""• Offline deployment packaging &amp; local caching"" FontSize=""11.5"" Foreground=""#94A3B8"" Margin=""0,2.5,0,2.5""/>
                            <TextBlock Text=""• Automatic background update checks &amp; one-click auto-updater"" FontSize=""11.5"" Foreground=""#94A3B8"" Margin=""0,2.5,0,2.5""/>
                        </StackPanel>
                    </StackPanel>
                </Border>

                <!-- Download Progress Bar (Visible during active download) -->
                <StackPanel x:Name=""PnlDownloadSection"" Margin=""0,0,0,16"" Visibility=""Collapsed"">
                    <Grid Margin=""0,0,0,6"">
                        <TextBlock x:Name=""TxtDownloadStatus"" Text=""Downloading update package..."" FontSize=""11.5"" Foreground=""#38BDF8""/>
                    </Grid>
                    <ProgressBar x:Name=""PbUpdateDownload"" Height=""6"" Background=""#11141A"" Foreground=""#0078D4"" Value=""0"" Maximum=""100"" BorderThickness=""0""/>
                </StackPanel>

                <!-- Action Buttons -->
                <Grid>
                    <StackPanel Orientation=""Horizontal"" HorizontalAlignment=""Center"">
                        <Button x:Name=""BtnCheckUpdatesAgain"" Content=""Check for Updates"" Background=""#1E293B"" Foreground=""#E2E8F0"" FontWeight=""SemiBold""
                                Padding=""22,8"" Cursor=""Hand"" BorderThickness=""1"" BorderBrush=""#334155"" FontSize=""12""/>
                        <Button x:Name=""BtnDownloadInstall"" Content=""Download &amp; Install Update"" Background=""#0078D4"" Foreground=""#FFFFFF"" FontWeight=""Bold""
                                Padding=""22,8"" Margin=""12,0,0,0"" Cursor=""Hand"" BorderThickness=""0"" FontSize=""12"" Visibility=""Collapsed""/>
                    </StackPanel>
                </Grid>
            </StackPanel>
        </Border>
    </Grid>

    <!-- ========================================================================= -->
    <!-- MODAL 2: SETTINGS                                                         -->
    <!-- ========================================================================= -->
    <Grid x:Name=""ModalSettingsOverlay"" Grid.ColumnSpan=""2"" Background=""#75000000"" Visibility=""Collapsed"">
        <Border x:Name=""BorderSettingsCard"" Width=""660"" Background=""#181B22"" CornerRadius=""16"" BorderBrush=""#2D3342"" BorderThickness=""1""
                HorizontalAlignment=""Center"" VerticalAlignment=""Center"" Padding=""28"">
            <StackPanel>
                <!-- Title Row -->
                <Grid Margin=""0,0,0,18"">
                    <StackPanel Orientation=""Horizontal"" VerticalAlignment=""Center"">
                        <Border Width=""34"" Height=""34"" Background=""#0284C7"" CornerRadius=""8"" Margin=""0,0,12,0"" VerticalAlignment=""Center"">
                            <TextBlock Text=""⚙️"" FontSize=""16"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                        </Border>
                        <TextBlock x:Name=""TxtModalSettingsTitle"" Text=""FleetForge Settings"" FontSize=""18"" FontWeight=""Bold"" Foreground=""#FFFFFF"" VerticalAlignment=""Center""/>
                        <Border x:Name=""BadgeSettingsConfig"" Background=""#1E293B"" CornerRadius=""5"" Padding=""8,3"" Margin=""12,0,0,0"" VerticalAlignment=""Center"" BorderThickness=""1"" BorderBrush=""#334155"">
                            <TextBlock x:Name=""TxtSettingsConfig"" Text=""Preferences &amp; Options"" FontSize=""10.5"" FontWeight=""SemiBold"" Foreground=""#38BDF8""/>
                        </Border>
                    </StackPanel>
                    <Button x:Name=""BtnCloseSettings"" Content=""✕"" Background=""Transparent"" Foreground=""#8A8A8A"" FontSize=""15"" FontWeight=""Bold""
                            Width=""32"" Height=""32"" Cursor=""Hand"" BorderThickness=""0"" HorizontalAlignment=""Right"" VerticalAlignment=""Center""/>
                </Grid>

                <!-- Section 1: Engine -->
                <TextBlock x:Name=""TxtSettingsSec1Title"" Text=""⚡ INSTALLATION OPTIONS"" FontSize=""11"" FontWeight=""Bold"" Foreground=""#38BDF8"" Margin=""0,0,0,8""/>
                <Border x:Name=""BorderSettingsSec1"" Background=""#11141A"" CornerRadius=""10"" Padding=""14"" BorderBrush=""#232938"" BorderThickness=""1"" Margin=""0,0,0,14"">
                    <StackPanel>
                        <CheckBox x:Name=""ChkSettingSilent"" Content=""Automatic Silent Install (No popups or manual next-clicks)"" IsChecked=""True"" Foreground=""#FFFFFF"" FontSize=""12"" Margin=""0,0,0,8"" Cursor=""Hand""/>
                        <CheckBox x:Name=""ChkSettingAgreements"" Content=""Automatically accept license agreements"" IsChecked=""True"" Foreground=""#FFFFFF"" FontSize=""12"" Margin=""0,0,0,8"" Cursor=""Hand""/>
                        <CheckBox x:Name=""ChkSettingAutoTheme"" Content=""Match Windows theme automatically (Light / Dark)"" IsChecked=""True"" Foreground=""#FFFFFF"" FontSize=""12"" Cursor=""Hand""/>
                    </StackPanel>
                </Border>

                <!-- Section 2: Storage -->
                <TextBlock x:Name=""TxtSettingsSec2Title"" Text=""📦 DOWNLOADED FILES &amp; STORAGE"" FontSize=""11"" FontWeight=""Bold"" Foreground=""#38BDF8"" Margin=""0,0,0,8""/>
                <Border x:Name=""BorderSettingsSec2"" Background=""#11141A"" CornerRadius=""10"" Padding=""14"" BorderBrush=""#232938"" BorderThickness=""1"" Margin=""0,0,0,14"">
                    <StackPanel>
                        <TextBlock x:Name=""TxtSettingOfflineDirLbl"" Text=""Downloaded Installers Folder:"" FontSize=""11"" Foreground=""#94A3B8"" Margin=""0,0,0,6""/>
                        <Grid Margin=""0,0,0,10"">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width=""*""/>
                                <ColumnDefinition Width=""Auto""/>
                            </Grid.ColumnDefinitions>
                            <TextBox x:Name=""TxtSettingOfflineDir"" Grid.Column=""0"" Height=""34"" Background=""#0B0D11"" Foreground=""#FFFFFF""
                                     BorderBrush=""#252B38"" BorderThickness=""1"" Padding=""10,6"" FontSize=""11.5"" VerticalContentAlignment=""Center""/>
                            <Button x:Name=""BtnBrowseOfflineDir"" Grid.Column=""1"" Content=""Browse..."" Background=""#1E293B"" Foreground=""#FFFFFF""
                                    Padding=""14,6"" Margin=""8,0,0,0"" Cursor=""Hand"" BorderThickness=""1"" BorderBrush=""#334155"" FontSize=""11.5""/>
                        </Grid>
                        <StackPanel Orientation=""Horizontal"">
                            <Button x:Name=""BtnClearCache"" Content=""Clear Downloaded Files &amp; Logs"" Background=""#1E293B"" Foreground=""#94A3B8""
                                    Padding=""14,6"" Cursor=""Hand"" BorderThickness=""1"" BorderBrush=""#334155"" FontSize=""11"" Margin=""0,0,10,0""/>
                            <TextBlock x:Name=""TxtCacheStatus"" Text=""Cache: Ready"" FontSize=""11"" Foreground=""#64748B"" VerticalAlignment=""Center""/>
                        </StackPanel>
                    </StackPanel>
                </Border>

                <!-- Section 3: Catalog -->
                <TextBlock x:Name=""TxtSettingsSec3Title"" Text=""APP CATALOG &amp; DATA"" FontSize=""11"" FontWeight=""Bold"" Foreground=""#38BDF8"" Margin=""0,0,0,8""/>
                <Border x:Name=""BorderSettingsSec3"" Background=""#11141A"" CornerRadius=""10"" Padding=""14"" BorderBrush=""#232938"" BorderThickness=""1"" Margin=""0,0,0,18"">
                    <Grid>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width=""*""/>
                            <ColumnDefinition Width=""Auto""/>
                        </Grid.ColumnDefinitions>
                        <StackPanel Grid.Column=""0"" VerticalAlignment=""Center"">
                            <TextBlock x:Name=""TxtSettingCatalogInfo"" Text=""16,349 Apps Available in Local Library"" FontSize=""12.5"" FontWeight=""SemiBold"" Foreground=""#FFFFFF""/>
                            <TextBlock x:Name=""TxtSettingCatalogSource"" Text=""Source: Official Microsoft Windows App Store"" FontSize=""11"" Foreground=""#64748B"" Margin=""0,2,0,0""/>
                        </StackPanel>
                        <Button x:Name=""BtnExportCatalog"" Grid.Column=""1"" Content=""Export App List"" Background=""#1E293B"" Foreground=""#FFFFFF""
                                Padding=""14,6"" Cursor=""Hand"" BorderThickness=""1"" BorderBrush=""#334155"" FontSize=""11.5"" VerticalAlignment=""Center""/>
                    </Grid>
                </Border>

                <!-- Bottom Actions -->
                <Grid>
                    <Button x:Name=""BtnCancelSettings"" Content=""Cancel"" Background=""#262D3D"" Foreground=""#A0AEC0""
                            Padding=""20,8"" HorizontalAlignment=""Left"" Cursor=""Hand"" BorderThickness=""1"" BorderBrush=""#353E50"" FontSize=""12""/>
                    <Button x:Name=""BtnSaveSettings"" Content=""Save Settings"" Background=""#0078D4"" Foreground=""#FFFFFF""
                            FontWeight=""Bold"" Padding=""26,8"" HorizontalAlignment=""Right"" Cursor=""Hand"" BorderThickness=""0"" FontSize=""12""/>
                </Grid>
            </StackPanel>
        </Border>
    </Grid>

    <!-- ========================================================================= -->
    <!-- MODAL 3: ABOUT FLEETFORGE                                                 -->
    <!-- ========================================================================= -->
    <Grid x:Name=""ModalAboutOverlay"" Grid.ColumnSpan=""2"" Background=""#75000000"" Visibility=""Collapsed"">
        <Border x:Name=""BorderAboutCard"" Width=""580"" Background=""#181B22"" CornerRadius=""16"" BorderBrush=""#2D3342"" BorderThickness=""1""
                HorizontalAlignment=""Center"" VerticalAlignment=""Center"" Padding=""28"">
            <StackPanel>
                <!-- Title Row -->
                <Grid Margin=""0,0,0,16"">
                    <TextBlock x:Name=""TxtModalAboutTitle"" Text=""About FleetForge"" FontSize=""18"" FontWeight=""Bold"" Foreground=""#FFFFFF"" VerticalAlignment=""Center""/>
                    <Button x:Name=""BtnCloseAbout"" Content=""✕"" Background=""Transparent"" Foreground=""#8A8A8A"" FontSize=""15"" FontWeight=""Bold""
                            Width=""32"" Height=""32"" Cursor=""Hand"" BorderThickness=""0"" HorizontalAlignment=""Right"" VerticalAlignment=""Center""/>
                </Grid>

                <!-- Brand Hero -->
                <StackPanel HorizontalAlignment=""Center"" Margin=""0,2,0,16"">
                    <Border x:Name=""BorderAboutLogoBox"" Width=""56"" Height=""56"" Background=""Transparent"" BorderThickness=""0"" HorizontalAlignment=""Center"" Margin=""0,0,0,10"">
                        <Grid>
                            <Image x:Name=""ImgAboutLogo"" Width=""52"" Height=""52"" RenderOptions.BitmapScalingMode=""HighQuality"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                            <TextBlock x:Name=""TxtFallbackAbout"" Text=""⚡"" FontSize=""28"" Foreground=""#0078D4"" HorizontalAlignment=""Center"" VerticalAlignment=""Center"" Visibility=""Collapsed""/>
                        </Grid>
                    </Border>
                    <StackPanel Orientation=""Horizontal"" HorizontalAlignment=""Center"">
                        <TextBlock x:Name=""TxtAboutFleet"" Text=""FLEET"" FontSize=""22"" FontWeight=""Bold"" Foreground=""#FFFFFF""/>
                        <TextBlock x:Name=""TxtAboutForge"" Text=""FORGE"" FontSize=""22"" FontWeight=""Bold"" Foreground=""#38BDF8""/>
                    </StackPanel>
                    <TextBlock x:Name=""TxtAboutTagline"" Text=""One-Click Software Installer for Windows"" FontSize=""11.5"" Foreground=""#94A3B8"" HorizontalAlignment=""Center"" Margin=""0,4,0,0""/>
                    <StackPanel Orientation=""Horizontal"" HorizontalAlignment=""Center"" Margin=""0,8,0,0"">
                        <Border x:Name=""BadgeAboutCompany"" Background=""#1E293B"" CornerRadius=""5"" Padding=""8,3"" Margin=""0,0,8,0"" BorderThickness=""1"" BorderBrush=""#334155"">
                            <TextBlock x:Name=""TxtAboutCompany"" Text=""Love Malhotra"" FontSize=""10.5"" FontWeight=""Bold"" Foreground=""#38BDF8""/>
                        </Border>
                        <Border x:Name=""BadgeAboutEdition"" Background=""#1E293B"" CornerRadius=""5"" Padding=""8,3"" BorderThickness=""1"" BorderBrush=""#334155"">
                            <TextBlock x:Name=""TxtAboutEdition"" Text=""v2.6.1 • Personal &amp; Work Edition"" FontSize=""10.5"" FontWeight=""Bold"" Foreground=""#94A3B8""/>
                        </Border>
                    </StackPanel>
                </StackPanel>

                <!-- Bio Summary -->
                <Border x:Name=""BorderAboutBio"" Background=""#11141A"" CornerRadius=""10"" Padding=""14"" BorderBrush=""#232938"" BorderThickness=""1"" Margin=""0,0,0,16"">
                    <TextBlock x:Name=""TxtAboutBioText"" Text=""FleetForge is a simple, one-click software installer made by Love Malhotra. It helps you quickly install your favorite everyday apps, office tools, and development software on any Windows PC without any hassle.""
                               FontSize=""11.5"" Foreground=""#CBD5E1"" TextWrapping=""Wrap"" LineHeight=""17.5""/>
                </Border>

                <!-- Technical Specs Grid -->
                <Border x:Name=""BorderAboutSpecs"" Background=""#11141A"" CornerRadius=""10"" Padding=""14"" BorderBrush=""#232938"" BorderThickness=""1"" Margin=""0,0,0,18"">
                    <Grid>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width=""*""/>
                            <ColumnDefinition Width=""*""/>
                        </Grid.ColumnDefinitions>
                        <StackPanel Grid.Column=""0"" Margin=""0,0,10,0"">
                            <TextBlock x:Name=""TxtSpecOrgLbl"" Text=""Created by:"" FontSize=""10.5"" Foreground=""#64748B""/>
                            <TextBlock x:Name=""TxtSpecOrgVal"" Text=""Love Malhotra"" FontSize=""11.5"" FontWeight=""SemiBold"" Foreground=""#FFFFFF"" Margin=""0,1,0,8""/>

                            <TextBlock x:Name=""TxtSpecEngLbl"" Text=""Installer Engine:"" FontSize=""10.5"" Foreground=""#64748B""/>
                            <TextBlock x:Name=""TxtSpecEngVal"" Text=""Microsoft Winget v1.8+"" FontSize=""11.5"" FontWeight=""SemiBold"" Foreground=""#FFFFFF"" Margin=""0,1,0,8""/>

                            <TextBlock x:Name=""TxtSpecLibLbl"" Text=""App Library:"" FontSize=""10.5"" Foreground=""#64748B""/>
                            <TextBlock x:Name=""TxtSpecLibVal"" Text=""Curated Catalog &amp; Winget Engine"" FontSize=""11.5"" FontWeight=""SemiBold"" Foreground=""#FFFFFF""/>
                        </StackPanel>
                        <StackPanel Grid.Column=""1"">
                            <TextBlock x:Name=""TxtSpecArchLbl"" Text=""Supported Windows:"" FontSize=""10.5"" Foreground=""#64748B""/>
                            <TextBlock x:Name=""TxtSpecArchVal"" Text=""Windows 10 / 11 (64-bit)"" FontSize=""11.5"" FontWeight=""SemiBold"" Foreground=""#FFFFFF"" Margin=""0,1,0,8""/>

                            <TextBlock x:Name=""TxtSpecChanLbl"" Text=""Update Channel:"" FontSize=""10.5"" Foreground=""#64748B""/>
                            <TextBlock x:Name=""TxtSpecChanVal"" Text=""GitHub Releases (loveonce)"" FontSize=""11.5"" FontWeight=""SemiBold"" Foreground=""#FFFFFF"" Margin=""0,1,0,8""/>

                            <TextBlock x:Name=""TxtSpecLicLbl"" Text=""License:"" FontSize=""10.5"" Foreground=""#64748B""/>
                            <TextBlock x:Name=""TxtSpecLicVal"" Text=""Enterprise Proprietary"" FontSize=""11.5"" FontWeight=""SemiBold"" Foreground=""#FFFFFF""/>
                        </StackPanel>
                    </Grid>
                </Border>

                <!-- Footer -->
                <TextBlock x:Name=""TxtAboutCopyright"" Text=""Copyright © 2026 Love Malhotra. All rights reserved."" FontSize=""11"" Foreground=""#64748B"" HorizontalAlignment=""Center"" Margin=""0,6,0,0""/>
            </StackPanel>
        </Border>
    </Grid>

    <!-- ========================================================================= -->
    <!-- MODAL 4: INTERACTIVE FIRST-TIME TUTORIAL & QUICK GUIDE                    -->
    <!-- ========================================================================= -->
    <Grid x:Name=""ModalTutorialOverlay"" Grid.ColumnSpan=""2"" Background=""#80000000"" Visibility=""Collapsed"">
        <Border x:Name=""BorderTutorialCard"" Width=""680"" Background=""#181B22"" CornerRadius=""16"" BorderBrush=""#2D3342"" BorderThickness=""1""
                HorizontalAlignment=""Center"" VerticalAlignment=""Center"" Padding=""28"">
            <StackPanel>
                <!-- Title Row -->
                <Grid Margin=""0,0,0,16"">
                    <StackPanel Orientation=""Horizontal"" VerticalAlignment=""Center"">
                        <Border Width=""34"" Height=""34"" CornerRadius=""8"" Background=""#0078D4"" Margin=""0,0,12,0"" VerticalAlignment=""Center"">
                            <TextBlock Text=""🎓"" FontSize=""18"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                        </Border>
                        <StackPanel VerticalAlignment=""Center"">
                            <TextBlock x:Name=""TxtModalTutorialTitle"" Text=""Welcome to FleetForge!"" FontSize=""18"" FontWeight=""Bold"" Foreground=""#FFFFFF""/>
                            <TextBlock x:Name=""TxtModalTutorialSubtitle"" Text=""Learn how to set up your PC in 3 simple steps"" FontSize=""12"" Foreground=""#94A3B8"" Margin=""0,2,0,0""/>
                        </StackPanel>
                    </StackPanel>
                    <Button x:Name=""BtnCloseTutorial"" Content=""✕"" Background=""Transparent"" Foreground=""#8A8A8A"" FontSize=""15"" FontWeight=""Bold""
                            Width=""32"" Height=""32"" Cursor=""Hand"" BorderThickness=""0"" HorizontalAlignment=""Right"" VerticalAlignment=""Center""/>
                </Grid>

                <!-- Interactive 3-Step Breadcrumb Bar -->
                <Grid Margin=""0,0,0,16"">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width=""*""/>
                        <ColumnDefinition Width=""*""/>
                        <ColumnDefinition Width=""*""/>
                    </Grid.ColumnDefinitions>

                    <Border x:Name=""BadgeTutStep1"" Grid.Column=""0"" CornerRadius=""6"" Padding=""10,7"" Margin=""0,0,4,0"" Cursor=""Hand"">
                        <TextBlock x:Name=""TxtTutStep1Badge"" Text=""1. Welcome &amp; Benefits"" FontSize=""11"" FontWeight=""SemiBold"" HorizontalAlignment=""Center""/>
                    </Border>
                    <Border x:Name=""BadgeTutStep2"" Grid.Column=""1"" CornerRadius=""6"" Padding=""10,7"" Margin=""2,0,2,0"" Cursor=""Hand"">
                        <TextBlock x:Name=""TxtTutStep2Badge"" Text=""2. How It Works"" FontSize=""11"" FontWeight=""SemiBold"" HorizontalAlignment=""Center""/>
                    </Border>
                    <Border x:Name=""BadgeTutStep3"" Grid.Column=""2"" CornerRadius=""6"" Padding=""10,7"" Margin=""4,0,0,0"" Cursor=""Hand"">
                        <TextBlock x:Name=""TxtTutStep3Badge"" Text=""3. Useful Pro Tips"" FontSize=""11"" FontWeight=""SemiBold"" HorizontalAlignment=""Center""/>
                    </Border>
                </Grid>

                <!-- STEP 1 CONTENT: WHAT IS FLEETFORGE -->
                <StackPanel x:Name=""PnlTutorialStep1"" Visibility=""Visible"">
                    <Border x:Name=""CardTutStep1Hero"" Background=""#11141A"" CornerRadius=""10"" Padding=""18,14"" BorderBrush=""#232938"" BorderThickness=""1"" Margin=""0,0,0,14"">
                        <StackPanel>
                            <TextBlock x:Name=""TxtTutStep1HeroTitle"" Text=""The Easiest Way to Set Up Your Windows PC"" FontSize=""14"" FontWeight=""Bold"" Foreground=""#38BDF8"" Margin=""0,0,0,6""/>
                            <TextBlock x:Name=""TxtTutStep1HeroDesc"" Text=""No more downloading installers one by one, clicking 'Next, Next, Next', or worrying about fake download buttons. FleetForge downloads and installs all your favorite programs safely in 1 click.""
                                       FontSize=""12"" Foreground=""#CBD5E1"" TextWrapping=""Wrap"" LineHeight=""19""/>
                        </StackPanel>
                    </Border>

                    <Grid Margin=""0,0,0,18"">
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width=""*""/>
                            <ColumnDefinition Width=""*""/>
                            <ColumnDefinition Width=""*""/>
                        </Grid.ColumnDefinitions>

                        <!-- Box 1 -->
                        <Border x:Name=""CardTutStep1Box1"" Grid.Column=""0"" Background=""#11141A"" CornerRadius=""8"" Padding=""14,12"" BorderBrush=""#232938"" BorderThickness=""1"" Margin=""0,0,6,0"">
                            <StackPanel>
                                <TextBlock x:Name=""TxtTutStep1Box1Title"" Text=""100% Verified Safe"" FontWeight=""Bold"" FontSize=""12.5"" Foreground=""#34D399"" Margin=""0,0,0,4""/>
                                <TextBlock x:Name=""TxtTutStep1Box1Desc"" Text=""Downloads directly from official Microsoft and vendor sources."" FontSize=""11"" Foreground=""#94A3B8"" TextWrapping=""Wrap"" LineHeight=""16""/>
                            </StackPanel>
                        </Border>

                        <!-- Box 2 -->
                        <Border x:Name=""CardTutStep1Box2"" Grid.Column=""1"" Background=""#11141A"" CornerRadius=""8"" Padding=""14,12"" BorderBrush=""#232938"" BorderThickness=""1"" Margin=""3,0,3,0"">
                            <StackPanel>
                                <TextBlock x:Name=""TxtTutStep1Box2Title"" Text=""Zero Popups &amp; Silent"" FontWeight=""Bold"" FontSize=""12.5"" Foreground=""#38BDF8"" Margin=""0,0,0,4""/>
                                <TextBlock x:Name=""TxtTutStep1Box2Desc"" Text=""Installs quietly in the background without annoying questions or toolbars."" FontSize=""11"" Foreground=""#94A3B8"" TextWrapping=""Wrap"" LineHeight=""16""/>
                            </StackPanel>
                        </Border>

                        <!-- Box 3 -->
                        <Border x:Name=""CardTutStep1Box3"" Grid.Column=""2"" Background=""#11141A"" CornerRadius=""8"" Padding=""14,12"" BorderBrush=""#232938"" BorderThickness=""1"" Margin=""6,0,0,0"">
                            <StackPanel>
                                <TextBlock x:Name=""TxtTutStep1Box3Title"" Text=""Completely Free &amp; Open"" FontWeight=""Bold"" FontSize=""12.5"" Foreground=""#FBBF24"" Margin=""0,0,0,4""/>
                                <TextBlock x:Name=""TxtTutStep1Box3Desc"" Text=""No registration, no subscriptions, zero ads, and 100% free forever."" FontSize=""11"" Foreground=""#94A3B8"" TextWrapping=""Wrap"" LineHeight=""16""/>
                            </StackPanel>
                        </Border>
                    </Grid>
                </StackPanel>

                <!-- STEP 2 CONTENT: HOW IT WORKS IN 3 STEPS -->
                <StackPanel x:Name=""PnlTutorialStep2"" Visibility=""Collapsed"">
                    <!-- Step Item 1 -->
                    <Border x:Name=""CardTutStep2Box1"" Background=""#11141A"" CornerRadius=""8"" Padding=""14,10"" BorderBrush=""#232938"" BorderThickness=""1"" Margin=""0,0,0,8"">
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width=""Auto""/>
                                <ColumnDefinition Width=""*""/>
                            </Grid.ColumnDefinitions>
                            <Border x:Name=""BorderTutStep2Num1"" Width=""28"" Height=""28"" CornerRadius=""14"" Background=""#0078D4"" Margin=""0,0,12,0"" VerticalAlignment=""Center"">
                                <TextBlock Text=""1"" FontWeight=""Bold"" FontSize=""13"" Foreground=""#FFFFFF"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                            </Border>
                            <StackPanel Grid.Column=""1"" VerticalAlignment=""Center"">
                                <TextBlock x:Name=""TxtTutStep2Box1Title"" Text=""Choose an App Pack"" FontWeight=""Bold"" FontSize=""12.5"" Foreground=""#FFFFFF""/>
                                <TextBlock x:Name=""TxtTutStep2Box1Desc"" Text=""Pick a pack that fits what you use this PC for (Home, Office, Developer, etc.). It pre-selects the best apps for you."" FontSize=""11"" Foreground=""#94A3B8"" TextWrapping=""Wrap"" Margin=""0,2,0,0""/>
                            </StackPanel>
                        </Grid>
                    </Border>

                    <!-- Step Item 2 -->
                    <Border x:Name=""CardTutStep2Box2"" Background=""#11141A"" CornerRadius=""8"" Padding=""14,10"" BorderBrush=""#232938"" BorderThickness=""1"" Margin=""0,0,0,8"">
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width=""Auto""/>
                                <ColumnDefinition Width=""*""/>
                            </Grid.ColumnDefinitions>
                            <Border x:Name=""BorderTutStep2Num2"" Width=""28"" Height=""28"" CornerRadius=""14"" Background=""#0284C7"" Margin=""0,0,12,0"" VerticalAlignment=""Center"">
                                <TextBlock Text=""2"" FontWeight=""Bold"" FontSize=""13"" Foreground=""#FFFFFF"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                            </Border>
                            <StackPanel Grid.Column=""1"" VerticalAlignment=""Center"">
                                <TextBlock x:Name=""TxtTutStep2Box2Title"" Text=""Add or Remove Apps (You're in Control)"" FontWeight=""Bold"" FontSize=""12.5"" Foreground=""#FFFFFF""/>
                                <TextBlock x:Name=""TxtTutStep2Box2Desc"" Text=""Want to customize? Click 'Change Apps' on any card, or use the 'All Apps' tab to search over 16,000 software packages."" FontSize=""11"" Foreground=""#94A3B8"" TextWrapping=""Wrap"" Margin=""0,2,0,0""/>
                            </StackPanel>
                        </Grid>
                    </Border>

                    <!-- Step Item 3 -->
                    <Border x:Name=""CardTutStep2Box3"" Background=""#11141A"" CornerRadius=""8"" Padding=""14,10"" BorderBrush=""#232938"" BorderThickness=""1"" Margin=""0,0,0,16"">
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width=""Auto""/>
                                <ColumnDefinition Width=""*""/>
                            </Grid.ColumnDefinitions>
                            <Border x:Name=""BorderTutStep2Num3"" Width=""28"" Height=""28"" CornerRadius=""14"" Background=""#10B981"" Margin=""0,0,12,0"" VerticalAlignment=""Center"">
                                <TextBlock Text=""3"" FontWeight=""Bold"" FontSize=""13"" Foreground=""#FFFFFF"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                            </Border>
                            <StackPanel Grid.Column=""1"" VerticalAlignment=""Center"">
                                <TextBlock x:Name=""TxtTutStep2Box3Title"" Text=""Click 'Install Selected Apps' and Relax"" FontWeight=""Bold"" FontSize=""12.5"" Foreground=""#FFFFFF""/>
                                <TextBlock x:Name=""TxtTutStep2Box3Desc"" Text=""Hit the big blue Install button at the bottom. FleetForge downloads and installs everything automatically. Grab a cup of tea!"" FontSize=""11"" Foreground=""#94A3B8"" TextWrapping=""Wrap"" Margin=""0,2,0,0""/>
                            </StackPanel>
                        </Grid>
                    </Border>
                </StackPanel>

                <!-- STEP 3 CONTENT: HELPFUL TIPS -->
                <StackPanel x:Name=""PnlTutorialStep3"" Visibility=""Collapsed"">
                    <!-- Tip 1 -->
                    <Border x:Name=""CardTutStep3Box1"" Background=""#11141A"" CornerRadius=""8"" Padding=""14,10"" BorderBrush=""#232938"" BorderThickness=""1"" Margin=""0,0,0,8"">
                        <StackPanel>
                            <TextBlock x:Name=""TxtTutStep3Box1Title"" Text=""🔍 Looking for a Specific Program?"" FontWeight=""Bold"" FontSize=""12.5"" Foreground=""#38BDF8"" Margin=""0,0,0,2""/>
                            <TextBlock x:Name=""TxtTutStep3Box1Desc"" Text=""Click 'All Apps' on the left menu. You can search for WhatsApp, Zoom, Discord, Adobe Reader, or anything else you need."" FontSize=""11"" Foreground=""#94A3B8"" TextWrapping=""Wrap""/>
                        </StackPanel>
                    </Border>

                    <!-- Tip 2 -->
                    <Border x:Name=""CardTutStep3Box2"" Background=""#11141A"" CornerRadius=""8"" Padding=""14,10"" BorderBrush=""#232938"" BorderThickness=""1"" Margin=""0,0,0,8"">
                        <StackPanel>
                            <TextBlock x:Name=""TxtTutStep3Box2Title"" Text=""💾 Save or Share Your Favorite Packs"" FontWeight=""Bold"" FontSize=""12.5"" Foreground=""#34D399"" Margin=""0,0,0,2""/>
                            <TextBlock x:Name=""TxtTutStep3Box2Desc"" Text=""Click the 'Export' button on any pack to save it as a file. You can bring it to a new computer and click 'Import Pack' anytime."" FontSize=""11"" Foreground=""#94A3B8"" TextWrapping=""Wrap""/>
                        </StackPanel>
                    </Border>

                    <!-- Tip 3 -->
                    <Border x:Name=""CardTutStep3Box3"" Background=""#11141A"" CornerRadius=""8"" Padding=""14,10"" BorderBrush=""#232938"" BorderThickness=""1"" Margin=""0,0,0,16"">
                        <StackPanel>
                            <TextBlock x:Name=""TxtTutStep3Box3Title"" Text=""🌙 Dark or Light Theme Anytime"" FontWeight=""Bold"" FontSize=""12.5"" Foreground=""#FBBF24"" Margin=""0,0,0,2""/>
                            <TextBlock x:Name=""TxtTutStep3Box3Desc"" Text=""Click the Sun / Moon icon at the top right anytime to pick whichever theme is most comfortable for your eyes."" FontSize=""11"" Foreground=""#94A3B8"" TextWrapping=""Wrap""/>
                        </StackPanel>
                    </Border>
                </StackPanel>

                <!-- Footer Navigation Row -->
                <Grid>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width=""Auto""/>
                        <ColumnDefinition Width=""*""/>
                        <ColumnDefinition Width=""Auto""/>
                    </Grid.ColumnDefinitions>

                    <!-- Don't Show Again Checkbox -->
                    <CheckBox x:Name=""ChkDontShowTutorialAgain"" Grid.Column=""0"" Content=""Don't show this guide on startup""
                              FontSize=""11"" Foreground=""#8A8A8A"" VerticalAlignment=""Center"" Cursor=""Hand""/>

                    <!-- Step Indicator -->
                    <Border x:Name=""BorderTutorialStepIndicator"" Grid.Column=""1"" CornerRadius=""12"" Padding=""12,4"" HorizontalAlignment=""Center"" VerticalAlignment=""Center"">
                        <TextBlock x:Name=""TxtTutorialStepIndicator"" Text=""Step 1 of 3""
                                   FontSize=""11"" FontWeight=""SemiBold"" Foreground=""#38BDF8"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                    </Border>

                    <!-- Action Buttons -->
                    <StackPanel Grid.Column=""2"" Orientation=""Horizontal"" VerticalAlignment=""Center"">
                        <Button x:Name=""BtnTutorialBack"" Content=""Back"" Background=""#2B2B2B"" Foreground=""#CCCCCC""
                                Padding=""14,6"" Margin=""0,0,8,0"" Cursor=""Hand"" BorderThickness=""1"" BorderBrush=""#3D3D3D"" FontSize=""11"" Visibility=""Collapsed""/>
                        <Button x:Name=""BtnTutorialNext"" Content=""Next Step"" Background=""#0078D4"" Foreground=""#FFFFFF""
                                FontWeight=""Bold"" Padding=""18,6"" Cursor=""Hand"" BorderThickness=""0"" FontSize=""11""/>
                    </StackPanel>
                </Grid>
            </StackPanel>
        </Border>
    </Grid>
</Grid>
";

            Grid root = (Grid)XamlReader.Parse(xaml);
            this.Content = root;
            rootGrid = root;

            // Bind Elements
            navBtnSetup = (Button)root.FindName("NavBtnSetup");
            navBtnCatalog = (Button)root.FindName("NavBtnCatalog");
            navBtnCloud = (Button)root.FindName("NavBtnCloud");
            navBtnLogs = (Button)root.FindName("NavBtnLogs");

            navIndSetup = (Border)root.FindName("NavIndSetup");
            navIndCatalog = (Border)root.FindName("NavIndCatalog");
            navIndCloud = (Border)root.FindName("NavIndCloud");
            navIndLogs = (Border)root.FindName("NavIndLogs");

            navIconSetup = (TextBlock)root.FindName("NavIconSetup");
            navIconCatalog = (TextBlock)root.FindName("NavIconCatalog");
            navIconCloud = (TextBlock)root.FindName("NavIconCloud");
            navIconLogs = (TextBlock)root.FindName("NavIconLogs");

            navTxtSetup = (TextBlock)root.FindName("NavTxtSetup");
            navTxtCatalog = (TextBlock)root.FindName("NavTxtCatalog");
            navTxtCloud = (TextBlock)root.FindName("NavTxtCloud");
            navTxtLogs = (TextBlock)root.FindName("NavTxtLogs");

            viewSetup = (ScrollViewer)root.FindName("ViewSetup");
            viewCatalog = (Grid)root.FindName("ViewCatalog");
            viewCloud = (ScrollViewer)root.FindName("ViewCloud");
            viewLogs = (Grid)root.FindName("ViewLogs");

            txtPageTitle = (TextBlock)root.FindName("TxtPageTitle");
            txtPageSubtitle = (TextBlock)root.FindName("TxtPageSubtitle");
            txtTopSyncStatus = (TextBlock)root.FindName("TxtTopSyncStatus");
            txtNavCloudStatus = (TextBlock)root.FindName("TxtNavCloudStatus");
            txtWingetVersion = (TextBlock)root.FindName("TxtWingetVersion");

            // Theme & Containers
            borderSidebar = (Border)root.FindName("BorderSidebar");
            borderBrandLogo = (Border)root.FindName("BorderBrandLogo");
            stackBrandText = (StackPanel)root.FindName("StackBrandText");
            txtBrandTitle = (TextBlock)root.FindName("TxtBrandTitle");
            txtBrandSubtitle = (TextBlock)root.FindName("TxtBrandSubtitle");
            btnToggleSidebar = (Button)root.FindName("BtnToggleSidebar");
            txtToggleSidebarIcon = (TextBlock)root.FindName("TxtToggleSidebarIcon");

            // Windows 11 Profile Block
            borderUserProfile = (Border)root.FindName("BorderUserProfile");
            borderUserAvatar = (Border)root.FindName("BorderUserAvatar");
            txtUserName = (TextBlock)root.FindName("TxtUserName");
            txtUserEmail = (TextBlock)root.FindName("TxtUserEmail");
            txtUserAvatarIcon = (TextBlock)root.FindName("TxtUserAvatarIcon");

            // Windows 11 Big Headings & Hero Status Card
            txtPageBigHeading = (TextBlock)root.FindName("TxtPageBigHeading");
            txtPageBigSubtitle = (TextBlock)root.FindName("TxtPageBigSubtitle");
            borderHeroStatusCard = (Border)root.FindName("BorderHeroStatusCard");
            borderHeroStatusIcon = (Border)root.FindName("BorderHeroStatusIcon");
            txtHeroStatusIcon = (TextBlock)root.FindName("TxtHeroStatusIcon");
            txtHeroStatusTitle = (TextBlock)root.FindName("TxtHeroStatusTitle");
            txtHeroStatusSubtitle = (TextBlock)root.FindName("TxtHeroStatusSubtitle");
            btnHeroInstall = (Button)root.FindName("BtnHeroInstall");
            if (btnHeroInstall != null)
            {
                btnHeroInstall.Click += delegate { StartInstallation(); };
            }

            borderStep1Circle = (Border)root.FindName("BorderStep1Circle");
            borderStep2Circle = (Border)root.FindName("BorderStep2Circle");
            borderStep3Circle = (Border)root.FindName("BorderStep3Circle");

            txtCatalogBigHeading = (TextBlock)root.FindName("TxtCatalogBigHeading");
            txtCatalogBigSubtitle = (TextBlock)root.FindName("TxtCatalogBigSubtitle");
            txtCloudBigHeading = (TextBlock)root.FindName("TxtCloudBigHeading");
            txtCloudBigSubtitle = (TextBlock)root.FindName("TxtCloudBigSubtitle");
            txtLogsBigHeading = (TextBlock)root.FindName("TxtLogsBigHeading");
            txtLogsBigSubtitle = (TextBlock)root.FindName("TxtLogsBigSubtitle");

            try
            {
                Microsoft.Win32.SystemEvents.UserPreferenceChanged += (s, e) =>
                {
                    if (e.Category == Microsoft.Win32.UserPreferenceCategory.General || e.Category == Microsoft.Win32.UserPreferenceCategory.Color)
                    {
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ApplyTheme(GetCurrentThemeIsLight());
                        }));
                    }
                };
            }
            catch { }

            borderNavCloudBadge = (Border)root.FindName("BorderNavCloudBadge");
            borderSysEnv = (Border)root.FindName("BorderSysEnv");
            txtSysEnvHeader = (TextBlock)root.FindName("TxtSysEnvHeader");
            stackSysEnvDetails = (StackPanel)root.FindName("StackSysEnvDetails");
            txtSysEnvTitle = (TextBlock)root.FindName("TxtSysEnvTitle");
            stackSidebarFooter = (StackPanel)root.FindName("StackSidebarFooter");
            txtSidebarFooterBy = (TextBlock)root.FindName("TxtSidebarFooterBy");
            txtSidebarFooterCompany = (TextBlock)root.FindName("TxtSidebarFooterCompany");
            borderTopBar = (Border)root.FindName("BorderTopBar");
            borderHero = (Border)root.FindName("BorderHero");
            txtHeroTitle = (TextBlock)root.FindName("TxtHeroTitle");
            txtHeroDesc = (TextBlock)root.FindName("TxtHeroDesc");
            btnHeroCatalog = (Button)root.FindName("BtnHeroCatalog");
            txtTargetRoleTitle = (TextBlock)root.FindName("TxtTargetRoleTitle");
            txtTargetRoleSubtitle = (TextBlock)root.FindName("TxtTargetRoleSubtitle");
            borderCatalogContainer = (Border)root.FindName("BorderCatalogContainer");
            borderCatalogHeaderBar = (Border)root.FindName("BorderCatalogHeaderBar");
            chkSelectAllCatalog = (CheckBox)root.FindName("ChkSelectAllCatalog");
            txtCatalogListHeaderCount = (TextBlock)root.FindName("TxtCatalogListHeaderCount");
            btnCatalogSort = (Button)root.FindName("BtnCatalogSort");
            catalogScrollViewer = (ScrollViewer)root.FindName("CatalogScrollViewer");
            borderCloudCard1 = (Border)root.FindName("BorderCloudCard1");
            borderCloudCard2 = (Border)root.FindName("BorderCloudCard2");
            borderCloudCard3 = (Border)root.FindName("BorderCloudCard3");
            txtCloudTitle1 = (TextBlock)root.FindName("TxtCloudTitle1");
            txtCloudDesc1 = (TextBlock)root.FindName("TxtCloudDesc1");
            txtCloudCard1Title = (TextBlock)root.FindName("TxtCloudCard1Title");
            txtCloudCard2Title = (TextBlock)root.FindName("TxtCloudCard2Title");
            txtCloudCard3Title = (TextBlock)root.FindName("TxtCloudCard3Title");
            txtLogsHeader = (TextBlock)root.FindName("TxtLogsHeader");
            borderLogTerminal = (Border)root.FindName("BorderLogTerminal");
            borderBottomBar = (Border)root.FindName("BorderBottomBar");
            borderModalCard = (Border)root.FindName("BorderModalCard");
            borderModalAppsContainer = (Border)root.FindName("BorderModalAppsContainer");

            btnImportPack = (Button)root.FindName("BtnImportPack");
            btnCreateNewGroup = (Button)root.FindName("BtnCreateNewGroup");
            roleCardsGrid = (System.Windows.Controls.Primitives.UniformGrid)root.FindName("RoleCardsGrid");

            borderHeaderSearch = (Border)root.FindName("BorderHeaderSearch");
            txtHeaderSearchIcon = (TextBlock)root.FindName("TxtHeaderSearchIcon");
            txtSearch = (TextBox)root.FindName("TxtSearch");
            btnClearSearch = (Button)root.FindName("BtnClearSearch");
            borderCategoryBar = (Border)root.FindName("BorderCategoryBar");
            categoryChipsPanel = (WrapPanel)root.FindName("CategoryChipsPanel");

            if (btnClearSearch != null)
            {
                btnClearSearch.Click += delegate
                {
                    if (txtSearch != null)
                    {
                        txtSearch.Text = "";
                        ApplyFilters();
                        btnClearSearch.Visibility = Visibility.Collapsed;
                    }
                };
            }
            appsContainer = (StackPanel)root.FindName("AppsContainer");

            txtCloudUrlInput = (TextBox)root.FindName("TxtCloudUrlInput");
            chkAutoSync = (CheckBox)root.FindName("ChkAutoSync");
            btnSaveCloudUrl = (Button)root.FindName("BtnSaveCloudUrl");
            btnSyncNow = (Button)root.FindName("BtnSyncNow");
            syncResultBorder = (Border)root.FindName("SyncResultBorder");
            txtSyncResult = (TextBlock)root.FindName("TxtSyncResult");

            btnExportSampleJson = (Button)root.FindName("BtnExportSampleJson");
            btnOpenLocalCatalog = (Button)root.FindName("BtnOpenLocalCatalog");
            borderCloudPresets = (Border)root.FindName("BorderCloudPresets");
            txtPresetsTitle = (TextBlock)root.FindName("TxtPresetsTitle");
            txtPresetsDesc = (TextBlock)root.FindName("TxtPresetsDesc");
            presetsContainer = (StackPanel)root.FindName("PresetsContainer");

            logScrollViewer = (ScrollViewer)root.FindName("LogScrollViewer");
            txtLog = (TextBox)root.FindName("TxtLog");
            btnViewReport = (Button)root.FindName("BtnViewReport");
            btnOpenLogFile = (Button)root.FindName("BtnOpenLogFile");
            btnClearLogConsole = (Button)root.FindName("BtnClearLogConsole");

            bannerResumeSetup = (Border)root.FindName("BannerResumeSetup");
            txtResumeBannerTitle = (TextBlock)root.FindName("TxtResumeBannerTitle");
            txtResumeBannerDesc = (TextBlock)root.FindName("TxtResumeBannerDesc");
            btnResumeSession = (Button)root.FindName("BtnResumeSession");
            btnDismissResume = (Button)root.FindName("BtnDismissResume");

            badgeAppVersion = (Border)root.FindName("BadgeAppVersion");
            txtAppVersion = (TextBlock)root.FindName("TxtAppVersion");
            txtStatus = (TextBlock)root.FindName("TxtStatus");
            txtCount = (TextBlock)root.FindName("TxtCount");
            progressBar = (ProgressBar)root.FindName("ProgressBar");
            btnSearchWingetOnlineBottom = (Button)root.FindName("BtnSearchWingetOnlineBottom");
            btnOpenOfflineFolder = (Button)root.FindName("BtnOpenOfflineFolder");
            btnInstall = (Button)root.FindName("BtnInstall");
            btnStop = (Button)root.FindName("BtnStop");

            // Modal Elements
            groupModalOverlay = (Grid)root.FindName("GroupModalOverlay");
            txtModalTitle = (TextBlock)root.FindName("TxtModalTitle");
            btnCloseModal = (Button)root.FindName("BtnCloseModal");
            txtGroupNameInput = (TextBox)root.FindName("TxtGroupNameInput");
            txtGroupIconInput = (TextBox)root.FindName("TxtGroupIconInput");
            emojiChipsPanel = (WrapPanel)root.FindName("EmojiChipsPanel");
            txtGroupDescInput = (TextBox)root.FindName("TxtGroupDescInput");
            txtModalAppSearch = (TextBox)root.FindName("TxtModalAppSearch");
            btnModalFilterTabAll = (Button)root.FindName("BtnModalFilterTabAll");
            btnModalFilterTabSelected = (Button)root.FindName("BtnModalFilterTabSelected");
            btnModalSelectAll = (Button)root.FindName("BtnModalSelectAll");
            btnModalClearAll = (Button)root.FindName("BtnModalClearAll");
            txtModalSelectedCount = (TextBlock)root.FindName("TxtModalSelectedCount");
            modalAppsScrollViewer = (ScrollViewer)root.FindName("ModalAppsScrollViewer");
            modalAppsContainer = (StackPanel)root.FindName("ModalAppsContainer");
            btnDeleteGroup = (Button)root.FindName("BtnDeleteGroup");
            btnModalExportGroup = (Button)root.FindName("BtnModalExportGroup");
            btnCancelModal = (Button)root.FindName("BtnCancelModal");
            btnSaveGroup = (Button)root.FindName("BtnSaveGroup");

            // Brand Logo & Help Menu
            imgBrandLogo = (Image)root.FindName("ImgBrandLogo");
            TextBlock txtFallbackBrand = (TextBlock)root.FindName("TxtFallbackBrand");
            if (this.Icon != null && imgBrandLogo != null)
            {
                imgBrandLogo.Source = this.Icon;
            }
            else if (txtFallbackBrand != null)
            {
                txtFallbackBrand.Visibility = Visibility.Visible;
            }

            btnToggleTheme = (Button)root.FindName("BtnToggleTheme");
            txtToggleThemeIcon = (TextBlock)root.FindName("TxtToggleThemeIcon");
            if (btnToggleTheme != null)
            {
                btnToggleTheme.Click += delegate
                {
                    ToggleTheme();
                };
            }

            btnHelpMenu = (Button)root.FindName("BtnHelpMenu");
            if (btnHelpMenu != null)
            {
                btnHelpMenu.Click += (s, e) =>
                {
                    ContextMenu menu = new ContextMenu();
                    menu.Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#1E222A");
                    menu.BorderBrush = currentThemeIsLight ? HexBrush("#CBD5E1") : HexBrush("#333C4D");
                    menu.Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#FFFFFF");
                    menu.FontSize = 12.5;

                    MenuItem itmTutorial = new MenuItem() { Header = "How to Use (Interactive Guide)...", FontWeight = FontWeights.SemiBold };
                    MenuItem itmUpdates = new MenuItem() { Header = isUpdateAvailable ? "Check for Updates (New Version Available!)..." : "Check for Updates..." };
                    MenuItem itmSettings = new MenuItem() { Header = "Settings..." };
                    MenuItem itmAbout = new MenuItem() { Header = "About FleetForge..." };

                    itmTutorial.Click += (ms, me) => ShowTutorialModal();
                    itmUpdates.Click += (ms, me) => ShowUpdatesModal();
                    itmSettings.Click += (ms, me) => ShowSettingsModal();
                    itmAbout.Click += (ms, me) => ShowAboutModal();

                    menu.Items.Add(itmTutorial);
                    menu.Items.Add(new Separator());
                    menu.Items.Add(itmUpdates);
                    menu.Items.Add(itmSettings);
                    menu.Items.Add(new Separator());
                    menu.Items.Add(itmAbout);

                    menu.PlacementTarget = btnHelpMenu;
                    menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                    menu.IsOpen = true;
                };
            }

            // Modal 1: Updates
            modalUpdatesOverlay = (Grid)root.FindName("ModalUpdatesOverlay");
            borderUpdatesCard = (Border)root.FindName("BorderUpdatesCard");
            txtModalUpdatesTitle = (TextBlock)root.FindName("TxtModalUpdatesTitle");
            borderModalUpdatesIcon = (Border)root.FindName("BorderModalUpdatesIcon");
            pathModalUpdatesIcon = (System.Windows.Shapes.Path)root.FindName("PathModalUpdatesIcon");
            borderUpdateStatusCard = (Border)root.FindName("BorderUpdateStatusCard");
            borderUpdateIconBadge = (Border)root.FindName("BorderUpdateIconBadge");
            pathUpdateStatusIcon = (System.Windows.Shapes.Path)root.FindName("PathUpdateStatusIcon");
            txtUpdateTitle = (TextBlock)root.FindName("TxtUpdateTitle");
            txtUpdateDetail = (TextBlock)root.FindName("TxtUpdateDetail");
            borderChangelogCard = (Border)root.FindName("BorderChangelogCard");
            txtChangelogHeading = (TextBlock)root.FindName("TxtChangelogHeading");
            pnlChangelogList = (StackPanel)root.FindName("PnlChangelogList");
            pnlDownloadSection = (StackPanel)root.FindName("PnlDownloadSection");
            pbUpdateDownload = (ProgressBar)root.FindName("PbUpdateDownload");
            txtDownloadStatus = (TextBlock)root.FindName("TxtDownloadStatus");

            btnCloseUpdates = (Button)root.FindName("BtnCloseUpdates");
            btnCheckUpdatesAgain = (Button)root.FindName("BtnCheckUpdatesAgain");
            btnDownloadInstall = (Button)root.FindName("BtnDownloadInstall");

            if (btnCloseUpdates != null) btnCloseUpdates.Click += (s, e) => modalUpdatesOverlay.Visibility = Visibility.Collapsed;
            if (modalUpdatesOverlay != null)
            {
                modalUpdatesOverlay.MouseDown += (s, e) =>
                {
                    if (e.OriginalSource == modalUpdatesOverlay)
                        modalUpdatesOverlay.Visibility = Visibility.Collapsed;
                };
            }
            if (btnCheckUpdatesAgain != null)
            {
                btnCheckUpdatesAgain.Click += (s, e) =>
                {
                    RunInteractiveUpdateCheck();
                };
            }
            if (btnDownloadInstall != null)
            {
                btnDownloadInstall.Click += (s, e) =>
                {
                    string targetUrl = currentUpdateInfo != null && !string.IsNullOrEmpty(currentUpdateInfo.downloadUrl) ? currentUpdateInfo.downloadUrl : AppUrls.UpdateDownloadUrl;
                    StartAutomaticUpdateDownload(targetUrl);
                };
            }

            // Modal 2: Settings
            modalSettingsOverlay = (Grid)root.FindName("ModalSettingsOverlay");
            borderSettingsCard = (Border)root.FindName("BorderSettingsCard");
            txtModalSettingsTitle = (TextBlock)root.FindName("TxtModalSettingsTitle");
            badgeSettingsConfig = (Border)root.FindName("BadgeSettingsConfig");
            txtSettingsConfig = (TextBlock)root.FindName("TxtSettingsConfig");
            btnCloseSettings = (Button)root.FindName("BtnCloseSettings");

            borderSettingsSec1 = (Border)root.FindName("BorderSettingsSec1");
            txtSettingsSec1Title = (TextBlock)root.FindName("TxtSettingsSec1Title");
            chkSettingSilent = (CheckBox)root.FindName("ChkSettingSilent");
            chkSettingAgreements = (CheckBox)root.FindName("ChkSettingAgreements");
            chkSettingAutoTheme = (CheckBox)root.FindName("ChkSettingAutoTheme");

            borderSettingsSec2 = (Border)root.FindName("BorderSettingsSec2");
            txtSettingsSec2Title = (TextBlock)root.FindName("TxtSettingsSec2Title");
            txtSettingOfflineDirLbl = (TextBlock)root.FindName("TxtSettingOfflineDirLbl");
            txtSettingOfflineDir = (TextBox)root.FindName("TxtSettingOfflineDir");
            btnBrowseOfflineDir = (Button)root.FindName("BtnBrowseOfflineDir");
            btnClearCache = (Button)root.FindName("BtnClearCache");
            txtCacheStatus = (TextBlock)root.FindName("TxtCacheStatus");

            borderSettingsSec3 = (Border)root.FindName("BorderSettingsSec3");
            txtSettingsSec3Title = (TextBlock)root.FindName("TxtSettingsSec3Title");
            txtSettingCatalogInfo = (TextBlock)root.FindName("TxtSettingCatalogInfo");
            txtSettingCatalogSource = (TextBlock)root.FindName("TxtSettingCatalogSource");
            btnExportCatalog = (Button)root.FindName("BtnExportCatalog");

            btnCancelSettings = (Button)root.FindName("BtnCancelSettings");
            btnSaveSettings = (Button)root.FindName("BtnSaveSettings");

            if (btnCloseSettings != null) btnCloseSettings.Click += (s, e) => modalSettingsOverlay.Visibility = Visibility.Collapsed;
            if (btnCancelSettings != null) btnCancelSettings.Click += (s, e) => modalSettingsOverlay.Visibility = Visibility.Collapsed;
            if (modalSettingsOverlay != null)
            {
                modalSettingsOverlay.MouseDown += (s, e) =>
                {
                    if (e.OriginalSource == modalSettingsOverlay)
                        modalSettingsOverlay.Visibility = Visibility.Collapsed;
                };
            }
            if (btnBrowseOfflineDir != null)
            {
                btnBrowseOfflineDir.Click += (s, e) =>
                {
                    using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
                    {
                        fbd.Description = "Select FleetForge Offline Installers Folder";
                        fbd.SelectedPath = txtSettingOfflineDir.Text;
                        fbd.ShowNewFolderButton = true;
                        if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            txtSettingOfflineDir.Text = fbd.SelectedPath;
                        }
                    }
                };
            }
            if (btnClearCache != null)
            {
                btnClearCache.Click += (s, e) =>
                {
                    try
                    {
                        string offlineDir = txtSettingOfflineDir != null ? txtSettingOfflineDir.Text.Trim() : "";
                        if (!string.IsNullOrEmpty(offlineDir) && Directory.Exists(offlineDir))
                        {
                            string[] files = Directory.GetFiles(offlineDir);
                            if (txtCacheStatus != null) txtCacheStatus.Text = string.Format("Cache: {0} files found", files.Length);
                        }
                        else
                        {
                            if (txtCacheStatus != null) txtCacheStatus.Text = "Cache: Folder is clean";
                        }
                    }
                    catch { if (txtCacheStatus != null) txtCacheStatus.Text = "Cache: Ready"; }
                };
            }
            if (btnExportCatalog != null)
            {
                btnExportCatalog.Click += (s, e) =>
                {
                    try
                    {
                        string exportPath = Path.Combine(appDir, "apps_backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json");
                        if (File.Exists(configPath))
                        {
                            File.Copy(configPath, exportPath, true);
                            MessageBox.Show("Catalog JSON exported successfully to:\n" + exportPath, "Catalog Exported", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Export error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                };
            }
            if (btnSaveSettings != null)
            {
                btnSaveSettings.Click += (s, e) =>
                {
                    try
                    {
                        if (settings == null) settings = new SettingsModel();
                        if (chkSettingSilent != null) settings.silent = chkSettingSilent.IsChecked == true;
                        if (chkSettingAgreements != null) settings.accept_agreements = chkSettingAgreements.IsChecked == true;
                        if (txtSettingOfflineDir != null) settings.offline_dir = txtSettingOfflineDir.Text.Trim();
                        if (chkSettingAutoTheme != null && chkSettingAutoTheme.IsChecked == true)
                        {
                            settings.theme = "system";
                        }
                        else
                        {
                            settings.theme = currentThemeIsLight ? "light" : "dark";
                        }
                        SaveSettings();
                        ApplyTheme(GetCurrentThemeIsLight());
                        if (modalSettingsOverlay != null) modalSettingsOverlay.Visibility = Visibility.Collapsed;
                        MessageBox.Show("Preferences saved successfully!", "Settings Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error saving settings: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                };
            }

            // Modal 3: About
            modalAboutOverlay = (Grid)root.FindName("ModalAboutOverlay");
            borderAboutCard = (Border)root.FindName("BorderAboutCard");
            txtModalAboutTitle = (TextBlock)root.FindName("TxtModalAboutTitle");
            btnCloseAbout = (Button)root.FindName("BtnCloseAbout");

            borderAboutLogoBox = (Border)root.FindName("BorderAboutLogoBox");
            imgAboutLogo = (Image)root.FindName("ImgAboutLogo");
            TextBlock txtFallbackAbout = (TextBlock)root.FindName("TxtFallbackAbout");
            txtAboutFleet = (TextBlock)root.FindName("TxtAboutFleet");
            txtAboutForge = (TextBlock)root.FindName("TxtAboutForge");
            txtAboutTagline = (TextBlock)root.FindName("TxtAboutTagline");
            badgeAboutCompany = (Border)root.FindName("BadgeAboutCompany");
            txtAboutCompany = (TextBlock)root.FindName("TxtAboutCompany");
            badgeAboutEdition = (Border)root.FindName("BadgeAboutEdition");
            txtAboutEdition = (TextBlock)root.FindName("TxtAboutEdition");

            borderAboutBio = (Border)root.FindName("BorderAboutBio");
            txtAboutBioText = (TextBlock)root.FindName("TxtAboutBioText");

            borderAboutSpecs = (Border)root.FindName("BorderAboutSpecs");
            txtSpecOrgLbl = (TextBlock)root.FindName("TxtSpecOrgLbl");
            txtSpecOrgVal = (TextBlock)root.FindName("TxtSpecOrgVal");
            txtSpecEngLbl = (TextBlock)root.FindName("TxtSpecEngLbl");
            txtSpecEngVal = (TextBlock)root.FindName("TxtSpecEngVal");
            txtSpecLibLbl = (TextBlock)root.FindName("TxtSpecLibLbl");
            txtSpecLibVal = (TextBlock)root.FindName("TxtSpecLibVal");
            txtSpecArchLbl = (TextBlock)root.FindName("TxtSpecArchLbl");
            txtSpecArchVal = (TextBlock)root.FindName("TxtSpecArchVal");
            txtSpecChanLbl = (TextBlock)root.FindName("TxtSpecChanLbl");
            txtSpecChanVal = (TextBlock)root.FindName("TxtSpecChanVal");
            txtSpecLicLbl = (TextBlock)root.FindName("TxtSpecLicLbl");
            txtSpecLicVal = (TextBlock)root.FindName("TxtSpecLicVal");

            txtAboutCopyright = (TextBlock)root.FindName("TxtAboutCopyright");

            if (this.Icon != null && imgAboutLogo != null)
            {
                imgAboutLogo.Source = this.Icon;
            }
            else if (txtFallbackAbout != null)
            {
                txtFallbackAbout.Visibility = Visibility.Visible;
            }

            if (btnCloseAbout != null) btnCloseAbout.Click += (s, e) => modalAboutOverlay.Visibility = Visibility.Collapsed;
            if (modalAboutOverlay != null)
            {
                modalAboutOverlay.MouseDown += (s, e) =>
                {
                    if (e.OriginalSource == modalAboutOverlay)
                        modalAboutOverlay.Visibility = Visibility.Collapsed;
                };
            }
            if (badgeAboutCompany != null)
            {
                badgeAboutCompany.Cursor = Cursors.Hand;
                badgeAboutCompany.ToolTip = "Visit Creator Profile (" + AppUrls.CompanyWebsiteUrl + ")";
                badgeAboutCompany.MouseLeftButtonUp += (s, e) =>
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(AppUrls.CompanyWebsiteUrl) { UseShellExecute = true });
                    }
                    catch { }
                };
            }

            if (wingetCmd == null && txtWingetVersion != null)
            {
                txtWingetVersion.Text = "Winget: Not Detected";
                txtWingetVersion.Foreground = System.Windows.Media.Brushes.Orange;
            }

            // Wire Navigation
            navBtnSetup.Click += delegate { SwitchView("setup"); };
            navBtnCatalog.Click += delegate { SwitchView("catalog"); };
            navBtnCloud.Click += delegate { SwitchView("cloud"); };
            navBtnLogs.Click += delegate { SwitchView("logs"); };

            AttachNavHover(navBtnSetup, "setup");
            AttachNavHover(navBtnCatalog, "catalog");
            AttachNavHover(navBtnCloud, "cloud");
            AttachNavHover(navBtnLogs, "logs");

            // Tutorial & Step Guide Controls
            btnQuickGuide = (Button)root.FindName("BtnQuickGuide");
            borderStepsGuide = (Border)root.FindName("BorderStepsGuide");
            txtStep1Title = (TextBlock)root.FindName("TxtStep1Title");
            txtStep1Desc = (TextBlock)root.FindName("TxtStep1Desc");
            txtStep2Title = (TextBlock)root.FindName("TxtStep2Title");
            txtStep2Desc = (TextBlock)root.FindName("TxtStep2Desc");
            txtStep3Title = (TextBlock)root.FindName("TxtStep3Title");
            txtStep3Desc = (TextBlock)root.FindName("TxtStep3Desc");
            txtStepArrow1 = (TextBlock)root.FindName("TxtStepArrow1");
            txtStepArrow2 = (TextBlock)root.FindName("TxtStepArrow2");

            modalTutorialOverlay = (Grid)root.FindName("ModalTutorialOverlay");
            borderTutorialCard = (Border)root.FindName("BorderTutorialCard");
            txtModalTutorialTitle = (TextBlock)root.FindName("TxtModalTutorialTitle");
            txtModalTutorialSubtitle = (TextBlock)root.FindName("TxtModalTutorialSubtitle");
            btnCloseTutorial = (Button)root.FindName("BtnCloseTutorial");

            // Tutorial Stepper Pills
            badgeTutStep1 = (Border)root.FindName("BadgeTutStep1");
            txtTutStep1Badge = (TextBlock)root.FindName("TxtTutStep1Badge");
            badgeTutStep2 = (Border)root.FindName("BadgeTutStep2");
            txtTutStep2Badge = (TextBlock)root.FindName("TxtTutStep2Badge");
            badgeTutStep3 = (Border)root.FindName("BadgeTutStep3");
            txtTutStep3Badge = (TextBlock)root.FindName("TxtTutStep3Badge");

            pnlTutorialStep1 = (StackPanel)root.FindName("PnlTutorialStep1");
            pnlTutorialStep2 = (StackPanel)root.FindName("PnlTutorialStep2");
            pnlTutorialStep3 = (StackPanel)root.FindName("PnlTutorialStep3");

            // Step 1 controls
            cardTutStep1Hero = (Border)root.FindName("CardTutStep1Hero");
            txtTutStep1HeroTitle = (TextBlock)root.FindName("TxtTutStep1HeroTitle");
            txtTutStep1HeroDesc = (TextBlock)root.FindName("TxtTutStep1HeroDesc");
            cardTutStep1Box1 = (Border)root.FindName("CardTutStep1Box1");
            txtTutStep1Box1Title = (TextBlock)root.FindName("TxtTutStep1Box1Title");
            txtTutStep1Box1Desc = (TextBlock)root.FindName("TxtTutStep1Box1Desc");
            cardTutStep1Box2 = (Border)root.FindName("CardTutStep1Box2");
            txtTutStep1Box2Title = (TextBlock)root.FindName("TxtTutStep1Box2Title");
            txtTutStep1Box2Desc = (TextBlock)root.FindName("TxtTutStep1Box2Desc");
            cardTutStep1Box3 = (Border)root.FindName("CardTutStep1Box3");
            txtTutStep1Box3Title = (TextBlock)root.FindName("TxtTutStep1Box3Title");
            txtTutStep1Box3Desc = (TextBlock)root.FindName("TxtTutStep1Box3Desc");

            // Step 2 controls
            cardTutStep2Box1 = (Border)root.FindName("CardTutStep2Box1");
            borderTutStep2Num1 = (Border)root.FindName("BorderTutStep2Num1");
            txtTutStep2Box1Title = (TextBlock)root.FindName("TxtTutStep2Box1Title");
            txtTutStep2Box1Desc = (TextBlock)root.FindName("TxtTutStep2Box1Desc");
            cardTutStep2Box2 = (Border)root.FindName("CardTutStep2Box2");
            borderTutStep2Num2 = (Border)root.FindName("BorderTutStep2Num2");
            txtTutStep2Box2Title = (TextBlock)root.FindName("TxtTutStep2Box2Title");
            txtTutStep2Box2Desc = (TextBlock)root.FindName("TxtTutStep2Box2Desc");
            cardTutStep2Box3 = (Border)root.FindName("CardTutStep2Box3");
            borderTutStep2Num3 = (Border)root.FindName("BorderTutStep2Num3");
            txtTutStep2Box3Title = (TextBlock)root.FindName("TxtTutStep2Box3Title");
            txtTutStep2Box3Desc = (TextBlock)root.FindName("TxtTutStep2Box3Desc");

            // Step 3 controls
            cardTutStep3Box1 = (Border)root.FindName("CardTutStep3Box1");
            txtTutStep3Box1Title = (TextBlock)root.FindName("TxtTutStep3Box1Title");
            txtTutStep3Box1Desc = (TextBlock)root.FindName("TxtTutStep3Box1Desc");
            cardTutStep3Box2 = (Border)root.FindName("CardTutStep3Box2");
            txtTutStep3Box2Title = (TextBlock)root.FindName("TxtTutStep3Box2Title");
            txtTutStep3Box2Desc = (TextBlock)root.FindName("TxtTutStep3Box2Desc");
            cardTutStep3Box3 = (Border)root.FindName("CardTutStep3Box3");
            txtTutStep3Box3Title = (TextBlock)root.FindName("TxtTutStep3Box3Title");
            txtTutStep3Box3Desc = (TextBlock)root.FindName("TxtTutStep3Box3Desc");

            // Footer controls
            borderTutorialStepIndicator = (Border)root.FindName("BorderTutorialStepIndicator");
            txtTutorialStepIndicator = (TextBlock)root.FindName("TxtTutorialStepIndicator");
            btnTutorialBack = (Button)root.FindName("BtnTutorialBack");
            btnTutorialNext = (Button)root.FindName("BtnTutorialNext");
            chkDontShowTutorialAgain = (CheckBox)root.FindName("ChkDontShowTutorialAgain");

            if (badgeTutStep1 != null) badgeTutStep1.MouseLeftButtonUp += delegate { currentTutorialStep = 1; UpdateTutorialStepView(); };
            if (badgeTutStep2 != null) badgeTutStep2.MouseLeftButtonUp += delegate { currentTutorialStep = 2; UpdateTutorialStepView(); };
            if (badgeTutStep3 != null) badgeTutStep3.MouseLeftButtonUp += delegate { currentTutorialStep = 3; UpdateTutorialStepView(); };

            if (btnQuickGuide != null) btnQuickGuide.Click += delegate { ShowTutorialModal(); };
            if (btnCloseTutorial != null) btnCloseTutorial.Click += delegate { CloseTutorialModal(); };
            if (modalTutorialOverlay != null)
            {
                modalTutorialOverlay.MouseDown += delegate(object s, MouseButtonEventArgs e)
                {
                    if (e.OriginalSource == modalTutorialOverlay) CloseTutorialModal();
                };
            }
            if (btnTutorialNext != null)
            {
                btnTutorialNext.Click += delegate
                {
                    if (currentTutorialStep < 3)
                    {
                        currentTutorialStep++;
                        UpdateTutorialStepView();
                    }
                    else
                    {
                        if (settings != null)
                        {
                            settings.tutorialCompleted = true;
                            SaveSettings();
                        }
                        CloseTutorialModal();
                    }
                };
            }
            if (btnTutorialBack != null)
            {
                btnTutorialBack.Click += delegate
                {
                    if (currentTutorialStep > 1)
                    {
                        currentTutorialStep--;
                        UpdateTutorialStepView();
                    }
                };
            }

            // Sidebar Toggle Event
            if (btnToggleSidebar != null)
            {
                btnToggleSidebar.Click += delegate { ToggleSidebar(); };
            }

            if (btnHeroCatalog != null) btnHeroCatalog.Click += delegate { SwitchView("catalog"); };

            if (chkSelectAllCatalog != null)
            {
                chkSelectAllCatalog.Click += delegate { OnMasterCheckboxClicked(); };
            }

            if (btnCatalogSort != null)
            {
                btnCatalogSort.Click += delegate
                {
                    ContextMenu menu = new ContextMenu();
                    menu.Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#1E222A");
                    menu.BorderBrush = currentThemeIsLight ? HexBrush("#CBD5E1") : HexBrush("#333C4D");
                    menu.Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#FFFFFF");
                    menu.FontSize = 12;

                    MenuItem itmSelectedTop = new MenuItem() { Header = (catalogSortMode == "selected_top" ? "✓  " : "    ") + "⭐ Selected Apps First" };
                    MenuItem itmAlphaAsc = new MenuItem() { Header = (catalogSortMode == "alpha_asc" ? "✓  " : "    ") + "🔤 Name (A to Z)" };
                    MenuItem itmAlphaDesc = new MenuItem() { Header = (catalogSortMode == "alpha_desc" ? "✓  " : "    ") + "🔤 Name (Z to A)" };
                    MenuItem itmPopular = new MenuItem() { Header = (catalogSortMode == "popular" ? "✓  " : "    ") + "🔥 Most Popular" };
                    MenuItem itmCategory = new MenuItem() { Header = (catalogSortMode == "category" ? "✓  " : "    ") + "🏷️ By Category" };

                    itmSelectedTop.Click += delegate { SetCatalogSort("selected_top", "Selected on Top"); };
                    itmAlphaAsc.Click += delegate { SetCatalogSort("alpha_asc", "A → Z"); };
                    itmAlphaDesc.Click += delegate { SetCatalogSort("alpha_desc", "Z → A"); };
                    itmPopular.Click += delegate { SetCatalogSort("popular", "Popular"); };
                    itmCategory.Click += delegate { SetCatalogSort("category", "Category"); };

                    menu.Items.Add(itmSelectedTop);
                    menu.Items.Add(itmAlphaAsc);
                    menu.Items.Add(itmAlphaDesc);
                    menu.Items.Add(new Separator());
                    menu.Items.Add(itmPopular);
                    menu.Items.Add(itmCategory);

                    menu.PlacementTarget = btnCatalogSort;
                    menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                    menu.IsOpen = true;
                };
            }

            if (catalogScrollViewer != null)
            {
                catalogScrollViewer.ScrollChanged += delegate (object sender, ScrollChangedEventArgs e)
                {
                    if (activeCategory != "winget_online" && e.VerticalChange > 0 && !isStreamingAllApps)
                    {
                        if (e.VerticalOffset + e.ViewportHeight >= e.ExtentHeight - 600)
                        {
                            if (currentRenderedCount < activeFilteredApps.Count)
                            {
                                RenderNextBatch(250);
                            }
                        }
                    }
                };
            }

            // Search events
            txtSearch.GotFocus += delegate
            {
                if (borderHeaderSearch != null)
                {
                    borderHeaderSearch.BorderBrush = currentThemeIsLight ? HexBrush("#0067C0") : HexBrush("#38BDF8");
                    borderHeaderSearch.BorderThickness = new Thickness(1.5);
                }
                if (txtSearch.Text == "Find an app, setting or pack..." || txtSearch.Text == "Search applications...")
                {
                    txtSearch.Text = "";
                    txtSearch.Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#FFFFFF");
                }
            };
            txtSearch.LostFocus += delegate
            {
                if (borderHeaderSearch != null)
                {
                    borderHeaderSearch.BorderBrush = currentThemeIsLight ? HexBrush("#B0B8C4") : HexBrush("#484848");
                    borderHeaderSearch.BorderThickness = new Thickness(1);
                }
                if (string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    txtSearch.Text = "Find an app, setting or pack...";
                    txtSearch.Foreground = currentThemeIsLight ? HexBrush("#475569") : HexBrush("#94A3B8");
                }
            };
            if (borderHeaderSearch != null)
            {
                borderHeaderSearch.MouseEnter += delegate
                {
                    if (txtSearch != null && !txtSearch.IsFocused)
                    {
                        borderHeaderSearch.BorderBrush = currentThemeIsLight ? HexBrush("#64748B") : HexBrush("#606060");
                    }
                };
                borderHeaderSearch.MouseLeave += delegate
                {
                    if (txtSearch != null && !txtSearch.IsFocused)
                    {
                        borderHeaderSearch.BorderBrush = currentThemeIsLight ? HexBrush("#B0B8C4") : HexBrush("#484848");
                    }
                };
            }
            // Search Debounce Timer (250ms interval to prevent UI stutter over 16,349 packages)
            searchDebounceTimer = new DispatcherTimer();
            searchDebounceTimer.Interval = TimeSpan.FromMilliseconds(250);
            searchDebounceTimer.Tick += delegate
            {
                searchDebounceTimer.Stop();
                ApplyFilters();
            };

            txtSearch.TextChanged += delegate
            {
                string t = txtSearch.Text;
                bool hasText = !string.IsNullOrEmpty(t) && t != "Find an app, setting or pack..." && t != "Search applications...";
                if (hasText && currentView != "catalog")
                {
                    SwitchView("catalog");
                }
                if (btnClearSearch != null)
                {
                    btnClearSearch.Visibility = hasText ? Visibility.Visible : Visibility.Collapsed;
                }
                if (searchDebounceTimer != null)
                {
                    searchDebounceTimer.Stop();
                    searchDebounceTimer.Start();
                }
                else
                {
                    ApplyFilters();
                }
            };
            txtSearch.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Enter)
                {
                    string q = txtSearch.Text.Trim();
                    if (!string.IsNullOrEmpty(q) && q != "search applications...")
                    {
                        SetCategoryFilter("winget_online");
                        if (txtOnlineSearchInput != null) txtOnlineSearchInput.Text = q;
                        RunWingetOnlineSearch(q);
                    }
                }
            };

            // Cloud Sync Events
            btnSaveCloudUrl.Click += delegate
            {
                settings.cloudConfigUrl = txtCloudUrlInput.Text.Trim();
                settings.autoSyncOnLaunch = chkAutoSync.IsChecked == true;
                SaveSettings();

                syncResultBorder.Visibility = Visibility.Visible;
                txtSyncResult.Text = "Settings saved successfully.";
                txtSyncResult.Foreground = HexBrush("#4ADE80");
            };

            btnSyncNow.Click += delegate { SyncFromCloudAsync(txtCloudUrlInput.Text.Trim()); };

            btnExportSampleJson.Click += delegate { ExportSampleJson(); };
            btnOpenLocalCatalog.Click += delegate
            {
                if (!File.Exists(configPath))
                {
                    File.WriteAllText(configPath, "{\n  \"groups\": [],\n  \"apps\": []\n}", Encoding.UTF8);
                }
                Process.Start("notepad.exe", configPath);
            };

            // Bottom Search Winget Online Button
            if (btnSearchWingetOnlineBottom != null)
            {
                btnSearchWingetOnlineBottom.Click += delegate
                {
                    if (activeCategory == "winget_online")
                    {
                        SetCategoryFilter("all");
                    }
                    else
                    {
                        string q = txtSearch != null ? txtSearch.Text.Trim() : "";
                        if (q == "Search applications...") q = "";
                        SetCategoryFilter("winget_online");
                        if (!string.IsNullOrEmpty(q) && txtOnlineSearchInput != null)
                        {
                            txtOnlineSearchInput.Text = q;
                            RunWingetOnlineSearch(q);
                        }
                    }
                };
            }

            // Offline Folder & Log events
            btnOpenOfflineFolder.Click += delegate
            {
                string offlinePath = Path.Combine(appDir, "offline_installers");
                if (!Directory.Exists(offlinePath)) Directory.CreateDirectory(offlinePath);
                Process.Start("explorer.exe", offlinePath);
            };

            btnOpenLogFile.Click += delegate
            {
                if (!File.Exists(logFilePath)) File.WriteAllText(logFilePath, "");
                Process.Start("notepad.exe", logFilePath);
            };

            btnClearLogConsole.Click += delegate { txtLog.Text = ""; };

            if (btnViewReport != null)
            {
                btnViewReport.Click += delegate
                {
                    if (!string.IsNullOrEmpty(lastGeneratedReportPath) && File.Exists(lastGeneratedReportPath))
                    {
                        try { Process.Start(new ProcessStartInfo(lastGeneratedReportPath) { UseShellExecute = true }); } catch { }
                    }
                };
            }

            if (btnResumeSession != null) btnResumeSession.Click += delegate { ResumeInterruptedSession(); };
            if (btnDismissResume != null) btnDismissResume.Click += delegate { DismissResumeSession(); };

            // Start Installation Event
            btnInstall.Click += delegate { StartInstallation(); };
            btnStop.Click += delegate { RequestStopInstallation(); };

            // Create New Group & Import Pack Events
            if (btnImportPack != null) btnImportPack.Click += delegate { ImportGroup(); };
            btnCreateNewGroup.Click += delegate { OpenCreateGroupModal(); };

            // Modal Controls
            btnCloseModal.Click += delegate { CloseGroupModal(); };
            btnCancelModal.Click += delegate { CloseGroupModal(); };
            btnSaveGroup.Click += delegate { SaveGroupModal(); };
            btnDeleteGroup.Click += delegate { DeleteGroupModal(); };
            if (btnModalExportGroup != null)
            {
                btnModalExportGroup.Click += delegate
                {
                    if (currentEditingGroupId != null)
                    {
                        GroupModel cur = config.groups.Find(delegate(GroupModel g) { return g.id == currentEditingGroupId; });
                        if (cur != null) ExportGroup(cur);
                    }
                };
            }

            groupModalOverlay.MouseDown += delegate(object s, MouseButtonEventArgs e)
            {
                if (e.OriginalSource == groupModalOverlay)
                {
                    CloseGroupModal();
                }
            };

            if (btnModalFilterTabAll != null)
            {
                btnModalFilterTabAll.Click += delegate
                {
                    modalShowSelectedOnly = false;
                    UpdateModalTabStyles();
                    FilterModalApps();
                };
            }

            if (btnModalFilterTabSelected != null)
            {
                btnModalFilterTabSelected.Click += delegate
                {
                    modalShowSelectedOnly = true;
                    UpdateModalTabStyles();
                    FilterModalApps();
                };
            }

            if (modalAppsScrollViewer != null)
            {
                modalAppsScrollViewer.ScrollChanged += delegate(object s, ScrollChangedEventArgs e)
                {
                    if (e.VerticalOffset + e.ViewportHeight >= e.ExtentHeight - 120)
                    {
                        if (!modalShowSelectedOnly && modalUnselectedApps != null && modalUnselectedRenderedCount < modalUnselectedApps.Count)
                        {
                            RenderNextModalCatalogBatch(40);
                        }
                    }
                };
            }

            btnModalSelectAll.Click += delegate
            {
                if (modalFilteredApps != null)
                {
                    foreach (AppModel a in modalFilteredApps)
                    {
                        if (a != null && a.id != null) modalSelectedAppIds.Add(a.id);
                    }
                }
                foreach (ModalAppItemView m in modalAppRows)
                {
                    m.CheckBox.IsChecked = true;
                    m.Container.Background = currentThemeIsLight ? HexBrush("#F0F9FF") : HexBrush("#132537");
                    m.Container.BorderBrush = currentThemeIsLight ? HexBrush("#0284C7") : HexBrush("#38BDF8");
                    if (m.StatusBadge != null) m.StatusBadge.Background = currentThemeIsLight ? HexBrush("#E0F2FE") : HexBrush("#0C4A6E");
                    if (m.StatusBadgeText != null)
                    {
                        m.StatusBadgeText.Text = "✓ SELECTED";
                        m.StatusBadgeText.Foreground = currentThemeIsLight ? HexBrush("#0284C7") : HexBrush("#38BDF8");
                    }
                }
                UpdateModalCount();
            };

            btnModalClearAll.Click += delegate
            {
                modalSelectedAppIds.Clear();
                foreach (ModalAppItemView m in modalAppRows)
                {
                    m.CheckBox.IsChecked = false;
                    m.Container.Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#181818");
                    m.Container.BorderBrush = currentThemeIsLight ? HexBrush("#E2E8F0") : HexBrush("#2B2B2B");
                    if (m.StatusBadge != null) m.StatusBadge.Background = currentThemeIsLight ? HexBrush("#F1F5F9") : HexBrush("#222222");
                    if (m.StatusBadgeText != null)
                    {
                        m.StatusBadgeText.Text = "+ ADD";
                        m.StatusBadgeText.Foreground = currentThemeIsLight ? HexBrush("#64748B") : HexBrush("#777777");
                    }
                }
                UpdateModalCount();
            };

            txtModalAppSearch.GotFocus += delegate
            {
                if (txtModalAppSearch.Text == "Filter software...")
                {
                    txtModalAppSearch.Text = "";
                    txtModalAppSearch.Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#FFFFFF");
                }
            };
            txtModalAppSearch.LostFocus += delegate
            {
                if (string.IsNullOrWhiteSpace(txtModalAppSearch.Text))
                {
                    txtModalAppSearch.Text = "Filter software...";
                    txtModalAppSearch.Foreground = currentThemeIsLight ? HexBrush("#94A3B8") : HexBrush("#707070");
                }
            };
            txtModalAppSearch.TextChanged += delegate { FilterModalApps(); };

            BuildEmojiChips();
            BuildCloudPresets();
        }

        // =========================================================================
        // THEME MANAGEMENT & WINDOWS THEME DETECTION
        // =========================================================================
        public static bool IsWindowsLightTheme()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("AppsUseLightTheme");
                        if (val != null)
                        {
                            return Convert.ToInt32(val) == 1;
                        }
                    }
                }
            }
            catch { }
            return false;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            try
            {
                HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
                if (source != null)
                {
                    source.AddHook(WndProc);
                }
            }
            catch { }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_SETTINGCHANGE = 0x001A;
            if (msg == WM_SETTINGCHANGE)
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    ApplyTheme(IsWindowsLightTheme());
                }));
            }
            return IntPtr.Zero;
        }

        private void ToggleTheme()
        {
            bool newIsLight = !currentThemeIsLight;
            if (settings == null) settings = new SettingsModel();
            settings.theme = newIsLight ? "light" : "dark";
            SaveSettings();
            ApplyTheme(newIsLight);
        }

        private bool GetCurrentThemeIsLight()
        {
            return IsWindowsLightTheme();
        }

        private void RegisterSystemThemeWatcher()
        {
            try
            {
                SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
                this.Closed += delegate
                {
                    try { SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged; } catch { }
                };
            }
            catch { }
        }

        private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General || e.Category == UserPreferenceCategory.Color)
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    ApplyTheme(IsWindowsLightTheme());
                }));
            }
        }

        private void ShowUpdatesModal()
        {
            if (modalUpdatesOverlay != null)
            {
                modalUpdatesOverlay.Visibility = Visibility.Visible;
                RunInteractiveUpdateCheck();
            }
        }

        private static int ParseVersionNumber(string ver)
        {
            if (string.IsNullOrEmpty(ver)) return 0;
            try
            {
                string digitsOnly = System.Text.RegularExpressions.Regex.Replace(ver, "[^0-9.]", "");
                string[] parts = digitsOnly.Split('.');
                int major = parts.Length > 0 ? int.Parse(parts[0]) : 0;
                int minor = parts.Length > 1 ? int.Parse(parts[1]) : 0;
                int patch = parts.Length > 2 ? int.Parse(parts[2]) : 0;
                return major * 10000 + minor * 100 + patch;
            }
            catch { return 0; }
        }

        private void RunInteractiveUpdateCheck()
        {
            if (btnCheckUpdatesAgain != null)
            {
                btnCheckUpdatesAgain.Content = "Checking...";
            }
            if (pathUpdateStatusIcon != null)
            {
                pathUpdateStatusIcon.Data = Geometry.Parse("M12 4V1L8 5l4 4V6c3.31 0 6 2.69 6 6 0 1.01-.25 1.97-.7 2.8l1.46 1.46C19.54 15.03 20 13.57 20 12c0-4.42-3.58-8-8-8zm0 14c-3.31 0-6-2.69-6-6 0-1.01.25-1.97.7-2.8L5.24 7.74C4.46 8.97 4 10.43 4 12c0 4.42 3.58 8 8 8v3l4-4-4-4v3z");
                pathUpdateStatusIcon.Fill = currentThemeIsLight ? HexBrush("#0284C7") : HexBrush("#38BDF8");
            }
            if (borderUpdateIconBadge != null)
            {
                borderUpdateIconBadge.Background = currentThemeIsLight ? HexBrush("#EFF6FF") : HexBrush("#0C4A6E");
                borderUpdateIconBadge.BorderBrush = currentThemeIsLight ? HexBrush("#BAE6FD") : HexBrush("#0284C7");
            }
            if (txtUpdateTitle != null)
            {
                txtUpdateTitle.Text = "Checking release server...";
                txtUpdateTitle.Foreground = currentThemeIsLight ? HexBrush("#0284C7") : HexBrush("#38BDF8");
            }
            if (txtUpdateDetail != null) txtUpdateDetail.Text = "Querying live version manifests and GitHub releases...";
            if (pnlDownloadSection != null) pnlDownloadSection.Visibility = Visibility.Collapsed;

            CheckForUpdates(true, delegate (bool hasUpdate, UpdateInfo info)
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    if (btnCheckUpdatesAgain != null)
                    {
                        btnCheckUpdatesAgain.Content = "Check for Updates";
                    }

                    if (info != null)
                    {
                        currentUpdateInfo = info;
                        if (pnlChangelogList != null && info.changelog != null && info.changelog.Count > 0)
                        {
                            pnlChangelogList.Children.Clear();
                            foreach (string ch in info.changelog)
                            {
                                TextBlock tb = new TextBlock()
                                {
                                    Text = "• " + ch,
                                    FontSize = 11,
                                    Foreground = currentThemeIsLight ? HexBrush("#475569") : HexBrush("#94A3B8"),
                                    Margin = new Thickness(0, 2, 0, 2)
                                };
                                pnlChangelogList.Children.Add(tb);
                            }
                        }
                    }

                    if (hasUpdate && info != null)
                    {
                        isUpdateAvailable = true;
                        if (pathUpdateStatusIcon != null)
                        {
                            pathUpdateStatusIcon.Data = Geometry.Parse("M19.35 10.04C18.67 6.59 15.64 4 12 4 9.11 4 6.6 5.64 5.35 8.04 2.34 8.36 0 10.91 0 14c0 3.31 2.69 6 6 6h13c2.76 0 5-2.24 5-5 0-2.64-2.05-4.78-4.65-4.96zM17 13l-5 5-5-5h3V9h4v4h3z");
                            pathUpdateStatusIcon.Fill = currentThemeIsLight ? HexBrush("#0078D4") : HexBrush("#38BDF8");
                        }
                        if (borderUpdateIconBadge != null)
                        {
                            borderUpdateIconBadge.Background = currentThemeIsLight ? HexBrush("#EFF6FF") : HexBrush("#172554");
                            borderUpdateIconBadge.BorderBrush = currentThemeIsLight ? HexBrush("#93C5FD") : HexBrush("#1D4ED8");
                        }
                        if (txtUpdateTitle != null)
                        {
                            txtUpdateTitle.Text = "New Update Available! (v" + info.version + ")";
                            txtUpdateTitle.Foreground = currentThemeIsLight ? HexBrush("#0078D4") : HexBrush("#38BDF8");
                        }
                        if (txtUpdateDetail != null)
                        {
                            txtUpdateDetail.Text = "FleetForge v" + info.version + " released on " + (info.buildDate ?? "today") + " is ready for installation.";
                        }
                        if (btnDownloadInstall != null)
                        {
                            btnDownloadInstall.Visibility = Visibility.Visible;
                            btnDownloadInstall.Content = "Download & Install v" + info.version;
                        }

                        // User prompt for update available
                        var promptRes = MessageBox.Show(
                            string.Format("A new version of FleetForge is available!\n\nVersion: v{0}\nRelease Date: {1}\n\nWould you like to download and install this update now?", info.version, info.buildDate ?? "Today"),
                            "Update Available — FleetForge",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Information);

                        if (promptRes == MessageBoxResult.Yes)
                        {
                            StartAutomaticUpdateDownload(info.downloadUrl);
                        }
                    }
                    else
                    {
                        if (pathUpdateStatusIcon != null)
                        {
                            pathUpdateStatusIcon.Data = Geometry.Parse("M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z");
                            pathUpdateStatusIcon.Fill = currentThemeIsLight ? HexBrush("#16A34A") : HexBrush("#34D399");
                        }
                        if (borderUpdateIconBadge != null)
                        {
                            borderUpdateIconBadge.Background = currentThemeIsLight ? HexBrush("#DCFCE7") : HexBrush("#064E3B");
                            borderUpdateIconBadge.BorderBrush = currentThemeIsLight ? HexBrush("#86EFAC") : HexBrush("#059669");
                        }
                        if (txtUpdateTitle != null)
                        {
                            txtUpdateTitle.Text = "You are running the latest version!";
                            txtUpdateTitle.Foreground = currentThemeIsLight ? HexBrush("#15803D") : HexBrush("#34D399");
                        }
                        if (txtUpdateDetail != null)
                        {
                            txtUpdateDetail.Text = "FleetForge v2.6.1 (Enterprise IT Edition • Love Malhotra) is up to date.";
                        }
                        if (btnDownloadInstall != null)
                        {
                            btnDownloadInstall.Visibility = Visibility.Collapsed;
                        }

                        // User prompt when no update found
                        if (info != null)
                        {
                            MessageBox.Show(
                                "You are running the latest version of FleetForge (v2.6.1).\n\nNo newer updates were found.",
                                "No Update Found — FleetForge",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show(
                                "Could not connect to the release server.\n\nPlease check your internet connection and try again.",
                                "Update Check — FleetForge",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                        }
                    }
                }));
            });
        }

        private void CheckForUpdates(bool isManual, Action<bool, UpdateInfo> callback)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                UpdateInfo info = null;
                bool updateFound = false;

                // 1. Try remote GitHub version.json
                string updateUrl = AppUrls.UpdateCheckUrl;
                try
                {
                    using (WebClient wc = new WebClient())
                    {
                        wc.Encoding = Encoding.UTF8;
                        wc.Headers.Add("User-Agent", "FleetForge/2.6.1");
                        string json = wc.DownloadString(updateUrl);
                        if (!string.IsNullOrEmpty(json))
                        {
                            var ser = new JavaScriptSerializer();
                            info = ser.Deserialize<UpdateInfo>(json);
                        }
                    }
                }
                catch { }

                // 2. If remote not accessible, check local version.json for testing
                if (info == null)
                {
                    try
                    {
                        string vPath = Path.Combine(appDir, "data", "version.json");
                        if (!File.Exists(vPath)) vPath = Path.Combine(appDir, "version.json");
                        if (File.Exists(vPath))
                        {
                            var ser = new JavaScriptSerializer();
                            string json = File.ReadAllText(vPath, Encoding.UTF8);
                            info = ser.Deserialize<UpdateInfo>(json);
                        }
                    }
                    catch { }
                }

                if (info != null)
                {
                    int currentVersionNum = ParseVersionNumber("2.6.1");
                    int remoteVersionNum = ParseVersionNumber(info.version);
                    if (remoteVersionNum > currentVersionNum || info.versionCode > 261)
                    {
                        updateFound = true;
                    }
                }

                if (callback != null)
                {
                    callback(updateFound, info);
                }
            });
        }

        private void StartAutomaticUpdateDownload(string downloadUrl)
        {
            if (string.IsNullOrEmpty(downloadUrl))
            {
                downloadUrl = AppUrls.UpdateDownloadUrl;
            }

            if (btnDownloadInstall != null) btnDownloadInstall.IsEnabled = false;
            if (btnCheckUpdatesAgain != null) btnCheckUpdatesAgain.IsEnabled = false;
            if (pnlDownloadSection != null) pnlDownloadSection.Visibility = Visibility.Visible;
            if (txtDownloadStatus != null) txtDownloadStatus.Text = "Connecting to release server...";
            if (pbUpdateDownload != null) pbUpdateDownload.Value = 5;

            string tempInstaller = Path.Combine(Path.GetTempPath(), "FleetForge-Setup-Update.exe");

            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    using (WebClient wc = new WebClient())
                    {
                        wc.Headers.Add("User-Agent", "FleetForge-Updater/2.6.1");
                        wc.DownloadProgressChanged += (s, e) =>
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {
                                if (pbUpdateDownload != null) pbUpdateDownload.Value = e.ProgressPercentage;
                                double mbRec = e.BytesReceived / (1024.0 * 1024.0);
                                double mbTot = e.TotalBytesToReceive / (1024.0 * 1024.0);
                                if (txtDownloadStatus != null)
                                {
                                    txtDownloadStatus.Text = string.Format("Downloading setup package: {0}% ({1:0.0} MB / {2:0.0} MB)",
                                        e.ProgressPercentage, mbRec, mbTot > 0 ? mbTot : mbRec);
                                }
                            }));
                        };

                        wc.DownloadFileCompleted += (s, e) =>
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {
                                if (e.Error != null)
                                {
                                    if (txtDownloadStatus != null) txtDownloadStatus.Text = "Download error: " + e.Error.Message;
                                    if (btnDownloadInstall != null) btnDownloadInstall.IsEnabled = true;
                                    if (btnCheckUpdatesAgain != null) btnCheckUpdatesAgain.IsEnabled = true;

                                    // Fallback test: if local FleetForge-Setup.exe exists, offer to launch it
                                    string localSetup = Path.Combine(appDir, "FleetForge-Setup.exe");
                                    if (!File.Exists(localSetup)) localSetup = Path.Combine(appDir, "..", "FleetForge-Setup.exe");
                                    if (File.Exists(localSetup))
                                    {
                                        var res = MessageBox.Show("Remote GitHub release download could not complete (" + e.Error.Message + ").\n\nWould you like to test the update workflow using the local setup package?", "Auto-Updater", MessageBoxButton.YesNo, MessageBoxImage.Question);
                                        if (res == MessageBoxResult.Yes)
                                        {
                                            Process.Start(new ProcessStartInfo(localSetup) { UseShellExecute = true });
                                            Application.Current.Shutdown();
                                        }
                                    }
                                    return;
                                }

                                if (txtDownloadStatus != null) txtDownloadStatus.Text = "Download complete! Launching FleetForge Setup...";
                                if (pbUpdateDownload != null) pbUpdateDownload.Value = 100;
                                Thread.Sleep(500);

                                try
                                {
                                    ProcessStartInfo psi = new ProcessStartInfo(tempInstaller);
                                    psi.UseShellExecute = true;
                                    Process.Start(psi);
                                    Application.Current.Shutdown();
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Could not launch downloaded setup: " + ex.Message, "Launch Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }));
                        };

                        wc.DownloadFileAsync(new Uri(downloadUrl), tempInstaller);
                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        if (txtDownloadStatus != null) txtDownloadStatus.Text = "Connection error: " + ex.Message;
                        if (btnDownloadInstall != null) btnDownloadInstall.IsEnabled = true;
                        if (btnCheckUpdatesAgain != null) btnCheckUpdatesAgain.IsEnabled = true;
                    }));
                }
            });
        }

        private void CheckBackgroundUpdateOnStartup()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    CheckForUpdates(false, delegate (bool updateFound, UpdateInfo info)
                    {
                        if (updateFound && info != null)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {
                                this.isUpdateAvailable = true;
                                this.currentUpdateInfo = info;
                                if (txtTopSyncStatus != null)
                                {
                                    txtTopSyncStatus.Text = "⚡ Update v" + info.version;
                                    txtTopSyncStatus.Foreground = HexBrush("#38BDF8");
                                }
                            }));
                        }
                    });
                }
                catch { }
            });
        }

        private void ShowSettingsModal()
        {
            if (modalSettingsOverlay != null)
            {
                if (settings != null)
                {
                    if (chkSettingAutoTheme != null) chkSettingAutoTheme.IsChecked = (settings.theme == "system" || string.IsNullOrEmpty(settings.theme));
                    if (chkSettingSilent != null) chkSettingSilent.IsChecked = settings.silent;
                    if (chkSettingAgreements != null) chkSettingAgreements.IsChecked = settings.accept_agreements;
                    if (txtSettingOfflineDir != null) txtSettingOfflineDir.Text = settings.offline_dir ?? "";
                }
                modalSettingsOverlay.Visibility = Visibility.Visible;
            }
        }

        private void ShowAboutModal()
        {
            if (modalAboutOverlay != null)
            {
                modalAboutOverlay.Visibility = Visibility.Visible;
            }
        }

        private void ShowTutorialModal()
        {
            if (modalTutorialOverlay != null)
            {
                currentTutorialStep = 1;
                UpdateTutorialStepView();
                modalTutorialOverlay.Visibility = Visibility.Visible;
            }
        }

        private void CloseTutorialModal()
        {
            if (chkDontShowTutorialAgain != null && chkDontShowTutorialAgain.IsChecked == true)
            {
                if (settings != null)
                {
                    settings.tutorialCompleted = true;
                    SaveSettings();
                }
            }
            if (modalTutorialOverlay != null)
            {
                modalTutorialOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateTutorialStepView()
        {
            if (pnlTutorialStep1 == null || pnlTutorialStep2 == null || pnlTutorialStep3 == null) return;

            pnlTutorialStep1.Visibility = currentTutorialStep == 1 ? Visibility.Visible : Visibility.Collapsed;
            pnlTutorialStep2.Visibility = currentTutorialStep == 2 ? Visibility.Visible : Visibility.Collapsed;
            pnlTutorialStep3.Visibility = currentTutorialStep == 3 ? Visibility.Visible : Visibility.Collapsed;

            // Update Stepper Pills
            UpdateTutorialStepperPill(badgeTutStep1, txtTutStep1Badge, currentTutorialStep == 1);
            UpdateTutorialStepperPill(badgeTutStep2, txtTutStep2Badge, currentTutorialStep == 2);
            UpdateTutorialStepperPill(badgeTutStep3, txtTutStep3Badge, currentTutorialStep == 3);

            if (txtTutorialStepIndicator != null)
            {
                txtTutorialStepIndicator.Text = string.Format("Step {0} of 3", currentTutorialStep);
                txtTutorialStepIndicator.Foreground = currentThemeIsLight ? HexBrush("#0284C7") : HexBrush("#38BDF8");
            }

            if (borderTutorialStepIndicator != null)
            {
                borderTutorialStepIndicator.Background = currentThemeIsLight ? HexBrush("#E0F2FE") : HexBrush("#0C2438");
                borderTutorialStepIndicator.BorderBrush = currentThemeIsLight ? HexBrush("#BAE6FD") : HexBrush("#0369A1");
                borderTutorialStepIndicator.BorderThickness = new Thickness(1);
            }

            if (btnTutorialBack != null)
            {
                btnTutorialBack.Visibility = currentTutorialStep > 1 ? Visibility.Visible : Visibility.Collapsed;
            }

            if (btnTutorialNext != null)
            {
                btnTutorialNext.Content = currentTutorialStep == 3 ? "Got It, Let's Start" : "Next Step";
                btnTutorialNext.Background = currentTutorialStep == 3 ? HexBrush("#10B981") : (currentThemeIsLight ? HexBrush("#0067C0") : HexBrush("#0078D4"));
            }
        }

        private void UpdateTutorialStepperPill(Border badge, TextBlock label, bool isActive)
        {
            if (badge == null || label == null) return;
            if (isActive)
            {
                badge.Background = currentThemeIsLight ? HexBrush("#0067C0") : HexBrush("#0078D4");
                badge.BorderBrush = currentThemeIsLight ? HexBrush("#005FB8") : HexBrush("#38BDF8");
                badge.BorderThickness = new Thickness(1);
                label.Foreground = HexBrush("#FFFFFF");
                label.FontWeight = FontWeights.Bold;
            }
            else
            {
                badge.Background = currentThemeIsLight ? HexBrush("#F1F5F9") : HexBrush("#1E222A");
                badge.BorderBrush = currentThemeIsLight ? HexBrush("#E2E8F0") : HexBrush("#2D3342");
                badge.BorderThickness = new Thickness(1);
                label.Foreground = currentThemeIsLight ? HexBrush("#475569") : HexBrush("#94A3B8");
                label.FontWeight = FontWeights.SemiBold;
            }
        }

        private void CheckFirstTimeTutorial()
        {
            if (settings != null && !settings.tutorialCompleted)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(delegate
                {
                    ShowTutorialModal();
                }));
            }
        }

        private void ToggleSidebar()
        {
            isSidebarCollapsed = !isSidebarCollapsed;
            double fromWidth = borderSidebar.ActualWidth > 0 ? borderSidebar.ActualWidth : (isSidebarCollapsed ? 250 : 68);
            double toWidth = isSidebarCollapsed ? 68.0 : 250.0;

            if (!isSidebarCollapsed)
            {
                SetSidebarContentCollapsed(false);
            }

            DoubleAnimation anim = new DoubleAnimation()
            {
                From = fromWidth,
                To = toWidth,
                Duration = TimeSpan.FromMilliseconds(200),
                DecelerationRatio = 0.85
            };

            anim.Completed += delegate
            {
                borderSidebar.BeginAnimation(FrameworkElement.WidthProperty, null);
                borderSidebar.Width = toWidth;
                if (isSidebarCollapsed)
                {
                    SetSidebarContentCollapsed(true);
                }
            };

            borderSidebar.BeginAnimation(FrameworkElement.WidthProperty, anim);
        }

        private void SetSidebarContentCollapsed(bool collapsed)
        {
            Visibility v = collapsed ? Visibility.Collapsed : Visibility.Visible;
            if (stackBrandText != null) stackBrandText.Visibility = v;
            if (borderBrandLogo != null) borderBrandLogo.Visibility = collapsed ? Visibility.Collapsed : Visibility.Visible;
            if (navTxtSetup != null) navTxtSetup.Visibility = v;
            if (navTxtCatalog != null) navTxtCatalog.Visibility = v;
            if (navTxtCloud != null) navTxtCloud.Visibility = v;
            if (borderNavCloudBadge != null) borderNavCloudBadge.Visibility = v;
            if (txtNavCloudStatus != null) txtNavCloudStatus.Visibility = v;
            if (navTxtLogs != null) navTxtLogs.Visibility = v;
            if (txtSysEnvHeader != null) txtSysEnvHeader.Visibility = v;
            if (stackSysEnvDetails != null) stackSysEnvDetails.Visibility = v;
            if (stackSidebarFooter != null) stackSidebarFooter.Visibility = v;

            if (btnToggleSidebar != null)
            {
                btnToggleSidebar.ToolTip = collapsed ? "Expand Menu" : "Collapse Menu";
                if (collapsed)
                {
                    Grid.SetColumn(btnToggleSidebar, 0);
                    btnToggleSidebar.HorizontalAlignment = HorizontalAlignment.Center;
                }
                else
                {
                    Grid.SetColumn(btnToggleSidebar, 2);
                    btnToggleSidebar.HorizontalAlignment = HorizontalAlignment.Right;
                }
            }
        }

        private void ApplyTheme(bool isLight)
        {
            currentThemeIsLight = isLight;

            // Dynamically recalculate Windows Accent Color & Tints
            windowsAccentColor = GetWindowsAccentColor();
            byte rVal = windowsAccentColor.R;
            byte gVal = windowsAccentColor.G;
            byte bVal = windowsAccentColor.B;
            windowsAccentBrush = new SolidColorBrush(windowsAccentColor);
            windowsAccentSubtleBrush = new SolidColorBrush(Color.FromArgb((byte)(isLight ? 28 : 44), rVal, gVal, bVal));
            windowsAccentBorderBrush = new SolidColorBrush(Color.FromArgb((byte)(isLight ? 95 : 120), rVal, gVal, bVal));

            // Window base
            this.Background = isLight ? HexBrush("#F3F3F3") : HexBrush("#202020");
            this.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (rootGrid != null) rootGrid.Background = isLight ? HexBrush("#F3F3F3") : HexBrush("#202020");

            // Sidebar
            if (borderSidebar != null)
            {
                borderSidebar.Background = isLight ? HexBrush("#F8F9FA") : HexBrush("#1E1E1E");
                borderSidebar.BorderBrush = isLight ? HexBrush("#E5E5E5") : HexBrush("#2E2E2E");
            }
            if (txtBrandTitle != null) txtBrandTitle.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (txtBrandSubtitle != null) txtBrandSubtitle.Foreground = isLight ? HexBrush("#5D5D5D") : HexBrush("#9D9D9D");
            if (txtToggleSidebarIcon != null) txtToggleSidebarIcon.Foreground = isLight ? HexBrush("#5D5D5D") : HexBrush("#B0B0B0");

            // Windows 11 User Profile Block
            if (borderUserProfile != null)
            {
                borderUserProfile.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#242424");
                borderUserProfile.BorderBrush = isLight ? HexBrush("#E5E5E5") : HexBrush("#333333");
            }
            if (borderUserAvatar != null)
            {
                borderUserAvatar.Background = windowsAccentSubtleBrush;
            }
            if (txtUserAvatarIcon != null)
            {
                txtUserAvatarIcon.Foreground = windowsAccentBrush;
            }
            if (txtUserName != null) txtUserName.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (txtUserEmail != null) txtUserEmail.Foreground = isLight ? HexBrush("#5D5D5D") : HexBrush("#A0A0A0");

            if (borderSysEnv != null)
            {
                borderSysEnv.Background = isLight ? HexBrush("#F0F0F0") : HexBrush("#1E1E1E");
                borderSysEnv.BorderBrush = isLight ? HexBrush("#E5E5E5") : HexBrush("#2E2E2E");
            }
            if (txtSysEnvHeader != null) txtSysEnvHeader.Foreground = isLight ? HexBrush("#5D5D5D") : HexBrush("#8E95A5");
            if (txtSysEnvTitle != null) txtSysEnvTitle.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (txtSidebarFooterBy != null)
            {
                txtSidebarFooterBy.Foreground = isLight ? HexBrush("#5D5D5D") : HexBrush("#8A8A8A");
            }
            if (txtSidebarFooterCompany != null)
            {
                txtSidebarFooterCompany.Foreground = isLight ? HexBrush("#DC2626") : HexBrush("#EF4444");
            }

            // Top Bar
            if (borderTopBar != null)
            {
                borderTopBar.Background = isLight ? HexBrush("#F3F3F3") : HexBrush("#202020");
                borderTopBar.BorderBrush = isLight ? HexBrush("#E5E5E5") : HexBrush("#2D2D2D");
            }
            if (txtPageTitle != null) txtPageTitle.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (txtPageSubtitle != null) txtPageSubtitle.Foreground = isLight ? HexBrush("#5D5D5D") : HexBrush("#A0A0A0");

            // Windows 11 Big Headings across views
            if (txtPageBigHeading != null) txtPageBigHeading.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (txtPageBigSubtitle != null) txtPageBigSubtitle.Foreground = isLight ? HexBrush("#5D5D5D") : HexBrush("#A0A0A0");
            if (txtCatalogBigHeading != null) txtCatalogBigHeading.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (txtCatalogBigSubtitle != null) txtCatalogBigSubtitle.Foreground = isLight ? HexBrush("#5D5D5D") : HexBrush("#A0A0A0");
            if (txtCloudBigHeading != null) txtCloudBigHeading.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (txtCloudBigSubtitle != null) txtCloudBigSubtitle.Foreground = isLight ? HexBrush("#5D5D5D") : HexBrush("#A0A0A0");
            if (txtLogsBigHeading != null) txtLogsBigHeading.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (txtLogsBigSubtitle != null) txtLogsBigSubtitle.Foreground = isLight ? HexBrush("#5D5D5D") : HexBrush("#A0A0A0");

            // Windows 11 Hero Status Card
            if (borderHeroStatusCard != null)
            {
                borderHeroStatusCard.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#272727");
                borderHeroStatusCard.BorderBrush = isLight ? HexBrush("#E5E5E5") : HexBrush("#383838");
            }
            if (borderHeroStatusIcon != null)
            {
                borderHeroStatusIcon.Background = windowsAccentSubtleBrush;
            }
            if (txtHeroStatusIcon != null)
            {
                txtHeroStatusIcon.Foreground = windowsAccentBrush;
            }
            if (txtHeroStatusTitle != null) txtHeroStatusTitle.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (txtHeroStatusSubtitle != null) txtHeroStatusSubtitle.Foreground = isLight ? HexBrush("#5D5D5D") : HexBrush("#A0A0A0");
            if (btnHeroInstall != null)
            {
                btnHeroInstall.Background = windowsAccentBrush;
                btnHeroInstall.Foreground = HexBrush("#FFFFFF");
            }

            // Quick Setup Step circles
            if (borderStep1Circle != null) borderStep1Circle.Background = windowsAccentBrush;
            if (borderStep2Circle != null) borderStep2Circle.Background = windowsAccentBrush;
            if (borderStep3Circle != null) borderStep3Circle.Background = windowsAccentBrush;

            // Sidebar Toggle Button
            if (txtToggleSidebarIcon != null)
            {
                txtToggleSidebarIcon.Foreground = isLight ? HexBrush("#5D5D5D") : HexBrush("#B0B0B0");
            }

            // Update Navigation items
            UpdateNavState(navBtnSetup, navIndSetup, navIconSetup, navTxtSetup, currentView == "setup");
            UpdateNavState(navBtnCatalog, navIndCatalog, navIconCatalog, navTxtCatalog, currentView == "catalog");
            UpdateNavState(navBtnCloud, navIndCloud, navIconCloud, navTxtCloud, currentView == "cloud");
            UpdateNavState(navBtnLogs, navIndLogs, navIconLogs, navTxtLogs, currentView == "logs");

            // Setup Page Hero Banner
            if (borderHero != null)
            {
                borderHero.Background = isLight ? HexBrush("#F0F7FF") : HexBrush("#1F2633");
                borderHero.BorderBrush = isLight ? HexBrush("#CCE4F7") : HexBrush("#2A3B52");
            }
            if (txtHeroTitle != null) txtHeroTitle.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (txtHeroDesc != null) txtHeroDesc.Foreground = isLight ? HexBrush("#5D5D5D") : HexBrush("#A0A0A0");
            if (btnHeroCatalog != null)
            {
                btnHeroCatalog.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#263244");
                btnHeroCatalog.Foreground = isLight ? HexBrush("#0067C0") : HexBrush("#4CC2FF");
                btnHeroCatalog.BorderBrush = isLight ? HexBrush("#CCE4F7") : HexBrush("#354A6B");
            }
            if (txtTargetRoleTitle != null) txtTargetRoleTitle.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (txtTargetRoleSubtitle != null) txtTargetRoleSubtitle.Foreground = isLight ? HexBrush("#5D5D5D") : HexBrush("#A0A0A0");
            if (btnImportPack != null)
            {
                btnImportPack.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2D2D2D");
                btnImportPack.BorderBrush = isLight ? HexBrush("#D1D5DB") : HexBrush("#3D3D3D");
                btnImportPack.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            }

            // Header Search Box
            if (borderHeaderSearch != null)
            {
                borderHeaderSearch.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2B2B2B");
                borderHeaderSearch.BorderBrush = isLight ? HexBrush("#B0B8C4") : HexBrush("#484848");
                borderHeaderSearch.BorderThickness = new Thickness(1);
            }
            if (txtHeaderSearchIcon != null)
            {
                txtHeaderSearchIcon.Foreground = isLight ? HexBrush("#475569") : HexBrush("#94A3B8");
            }
            if (txtSearch != null)
            {
                bool isPlaceholder = (txtSearch.Text == "Find an app, setting or pack..." || txtSearch.Text == "Search applications...");
                if (isPlaceholder)
                {
                    txtSearch.Foreground = isLight ? HexBrush("#475569") : HexBrush("#94A3B8");
                }
                else
                {
                    txtSearch.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#FFFFFF");
                }
                txtSearch.CaretBrush = isLight ? HexBrush("#0067C0") : HexBrush("#38BDF8");
            }
            if (btnClearSearch != null)
            {
                btnClearSearch.Foreground = isLight ? HexBrush("#64748B") : HexBrush("#94A3B8");
            }
            if (borderCategoryBar != null)
            {
                borderCategoryBar.Background = isLight ? HexBrush("#F1F5F9") : HexBrush("#1E222A");
                borderCategoryBar.BorderBrush = isLight ? HexBrush("#E2E8F0") : HexBrush("#2D3342");
            }
            if (categoryChipsPanel != null)
            {
                foreach (UIElement elem in categoryChipsPanel.Children)
                {
                    Button btn = elem as Button;
                    if (btn != null)
                    {
                        string tag = (string)btn.Tag;
                        UpdateCategoryTabVisual(btn, tag == activeCategory);
                    }
                }
            }
            if (borderCatalogContainer != null)
            {
                borderCatalogContainer.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#272727");
                borderCatalogContainer.BorderBrush = isLight ? HexBrush("#E5E5E5") : HexBrush("#383838");
            }
            if (borderCatalogHeaderBar != null)
            {
                borderCatalogHeaderBar.BorderBrush = isLight ? HexBrush("#E5E5E5") : HexBrush("#383838");
            }
            if (chkSelectAllCatalog != null)
            {
                chkSelectAllCatalog.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            }
            if (txtCatalogListHeaderCount != null)
            {
                txtCatalogListHeaderCount.Foreground = isLight ? HexBrush("#5D5D5D") : HexBrush("#A0A0A0");
            }
            if (btnCatalogSort != null)
            {
                btnCatalogSort.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2D2D2D");
                btnCatalogSort.BorderBrush = isLight ? HexBrush("#D1D5DB") : HexBrush("#3D3D3D");
                btnCatalogSort.Foreground = isLight ? HexBrush("#0067C0") : HexBrush("#4CC2FF");
            }

            // Cloud Sync Page
            if (txtCloudTitle1 != null) txtCloudTitle1.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (txtCloudDesc1 != null) txtCloudDesc1.Foreground = isLight ? HexBrush("#5D5D5D") : HexBrush("#A0A0A0");
            if (borderCloudCard1 != null)
            {
                borderCloudCard1.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#272727");
                borderCloudCard1.BorderBrush = isLight ? HexBrush("#E5E5E5") : HexBrush("#383838");
            }
            if (borderCloudCard2 != null)
            {
                borderCloudCard2.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#272727");
                borderCloudCard2.BorderBrush = isLight ? HexBrush("#E5E5E5") : HexBrush("#383838");
            }
            if (borderCloudCard3 != null)
            {
                borderCloudCard3.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#272727");
                borderCloudCard3.BorderBrush = isLight ? HexBrush("#E5E5E5") : HexBrush("#383838");
            }
            if (txtCloudCard1Title != null) txtCloudCard1Title.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (txtCloudCard2Title != null) txtCloudCard2Title.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (txtCloudCard3Title != null) txtCloudCard3Title.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (txtCloudUrlInput != null)
            {
                txtCloudUrlInput.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#1E1E1E");
                txtCloudUrlInput.BorderBrush = isLight ? HexBrush("#D1D5DB") : HexBrush("#383838");
                txtCloudUrlInput.Foreground = isLight ? HexBrush("#0067C0") : HexBrush("#4CC2FF");
            }
            if (chkAutoSync != null) chkAutoSync.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#C8C8C8");
            if (btnSaveCloudUrl != null)
            {
                btnSaveCloudUrl.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2E2E2E");
                btnSaveCloudUrl.BorderBrush = isLight ? HexBrush("#D1D5DB") : HexBrush("#3D3D3D");
                btnSaveCloudUrl.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            }
            if (borderCloudPresets != null)
            {
                borderCloudPresets.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#272727");
                borderCloudPresets.BorderBrush = isLight ? HexBrush("#E5E5E5") : HexBrush("#383838");
            }
            if (txtPresetsTitle != null) txtPresetsTitle.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (txtPresetsDesc != null) txtPresetsDesc.Foreground = isLight ? HexBrush("#5D5D5D") : HexBrush("#A0A0A0");
            if (btnExportSampleJson != null)
            {
                btnExportSampleJson.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2E2E2E");
                btnExportSampleJson.BorderBrush = isLight ? HexBrush("#D1D5DB") : HexBrush("#3D3D3D");
                btnExportSampleJson.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            }
            if (btnOpenLocalCatalog != null)
            {
                btnOpenLocalCatalog.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2E2E2E");
                btnOpenLocalCatalog.BorderBrush = isLight ? HexBrush("#D1D5DB") : HexBrush("#3D3D3D");
                btnOpenLocalCatalog.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            }

            // Logs Page
            if (txtLogsHeader != null) txtLogsHeader.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (borderLogTerminal != null)
            {
                borderLogTerminal.Background = isLight ? HexBrush("#F8F9FA") : HexBrush("#141414");
                borderLogTerminal.BorderBrush = isLight ? HexBrush("#E5E5E5") : HexBrush("#2D2D2D");
            }
            if (txtLog != null)
            {
                txtLog.Foreground = isLight ? HexBrush("#047857") : HexBrush("#6EE7B7");
            }
            if (btnOpenLogFile != null)
            {
                btnOpenLogFile.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2E2E2E");
                btnOpenLogFile.BorderBrush = isLight ? HexBrush("#D1D5DB") : HexBrush("#3D3D3D");
                btnOpenLogFile.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            }
            if (btnClearLogConsole != null)
            {
                btnClearLogConsole.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2E2E2E");
                btnClearLogConsole.BorderBrush = isLight ? HexBrush("#D1D5DB") : HexBrush("#3D3D3D");
                btnClearLogConsole.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            }

            // Bottom Bar
            if (borderBottomBar != null)
            {
                borderBottomBar.Background = isLight ? HexBrush("#F8F9FA") : HexBrush("#1F1F1F");
                borderBottomBar.BorderBrush = isLight ? HexBrush("#E5E5E5") : HexBrush("#333333");
            }
            if (badgeAppVersion != null)
            {
                badgeAppVersion.Background = isLight ? HexBrush("#EBF3FB") : HexBrush("#1E293B");
            }
            if (txtAppVersion != null)
            {
                txtAppVersion.Foreground = isLight ? HexBrush("#005FB8") : HexBrush("#38BDF8");
            }
            if (txtStatus != null) txtStatus.Foreground = isLight ? HexBrush("#5D5D5D") : HexBrush("#A0A0A0");
            if (txtCount != null) txtCount.Foreground = isLight ? HexBrush("#0067C0") : HexBrush("#4CC2FF");
            if (progressBar != null) progressBar.Background = isLight ? HexBrush("#E5E5E5") : HexBrush("#262626");
            if (btnSearchWingetOnlineBottom != null)
            {
                if (activeCategory == "winget_online")
                {
                    btnSearchWingetOnlineBottom.Background = isLight ? HexBrush("#0067C0") : HexBrush("#0369A1");
                }
                else
                {
                    btnSearchWingetOnlineBottom.Background = isLight ? HexBrush("#7C3AED") : HexBrush("#6D28D9");
                }
                btnSearchWingetOnlineBottom.Foreground = HexBrush("#FFFFFF");
            }
            if (btnOpenOfflineFolder != null)
            {
                btnOpenOfflineFolder.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2D2D2D");
                btnOpenOfflineFolder.BorderBrush = isLight ? HexBrush("#D1D5DB") : HexBrush("#3D3D3D");
                btnOpenOfflineFolder.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            }
            if (btnInstall != null)
            {
                btnInstall.Background = isLight ? HexBrush("#0067C0") : HexBrush("#0078D4");
                btnInstall.Foreground = HexBrush("#FFFFFF");
            }

            // Modal Card
            if (borderModalCard != null)
            {
                borderModalCard.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#272727");
                borderModalCard.BorderBrush = isLight ? HexBrush("#E5E5E5") : HexBrush("#383838");
            }
            if (txtModalTitle != null) txtModalTitle.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (borderModalAppsContainer != null)
            {
                borderModalAppsContainer.Background = isLight ? HexBrush("#F9F9F9") : HexBrush("#1E1E1E");
                borderModalAppsContainer.BorderBrush = isLight ? HexBrush("#E5E5E5") : HexBrush("#383838");
            }
            if (txtGroupIconInput != null)
            {
                txtGroupIconInput.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#1E1E1E");
                txtGroupIconInput.BorderBrush = isLight ? HexBrush("#D1D5DB") : HexBrush("#383838");
                txtGroupIconInput.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            }
            if (txtGroupNameInput != null)
            {
                txtGroupNameInput.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#1E1E1E");
                txtGroupNameInput.BorderBrush = isLight ? HexBrush("#D1D5DB") : HexBrush("#383838");
                txtGroupNameInput.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            }
            if (txtGroupDescInput != null)
            {
                txtGroupDescInput.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#1E1E1E");
                txtGroupDescInput.BorderBrush = isLight ? HexBrush("#D1D5DB") : HexBrush("#383838");
                txtGroupDescInput.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            }
            if (txtModalAppSearch != null)
            {
                txtModalAppSearch.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#1E1E1E");
                txtModalAppSearch.BorderBrush = isLight ? HexBrush("#D1D5DB") : HexBrush("#383838");
                txtModalAppSearch.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            }
            UpdateModalTabStyles();
            if (btnModalSelectAll != null)
            {
                btnModalSelectAll.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2E2E2E");
                btnModalSelectAll.BorderBrush = isLight ? HexBrush("#D1D5DB") : HexBrush("#3D3D3D");
                btnModalSelectAll.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#CCCCCC");
            }
            if (btnModalClearAll != null)
            {
                btnModalClearAll.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2E2E2E");
                btnModalClearAll.BorderBrush = isLight ? HexBrush("#D1D5DB") : HexBrush("#3D3D3D");
                btnModalClearAll.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#CCCCCC");
            }
            if (btnModalLoadMore != null)
            {
                btnModalLoadMore.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2E2E2E");
                btnModalLoadMore.Foreground = isLight ? HexBrush("#0067C0") : HexBrush("#38BDF8");
                btnModalLoadMore.BorderBrush = isLight ? HexBrush("#D1D5DB") : HexBrush("#3A3A3A");
            }
            if (btnCancelModal != null)
            {
                btnCancelModal.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2E2E2E");
                btnCancelModal.BorderBrush = isLight ? HexBrush("#D1D5DB") : HexBrush("#3D3D3D");
                btnCancelModal.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            }
            if (btnModalExportGroup != null)
            {
                btnModalExportGroup.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2B2B2B");
                btnModalExportGroup.BorderBrush = isLight ? HexBrush("#D1D5DB") : HexBrush("#3D3D3D");
                btnModalExportGroup.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#E0E0E0");
            }

            // Help Button
            if (btnHelpMenu != null)
            {
                btnHelpMenu.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2D2D2D");
                btnHelpMenu.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
                btnHelpMenu.BorderBrush = isLight ? HexBrush("#D1D5DB") : HexBrush("#3D3D3D");
            }

            // Updates Modal
            if (borderUpdatesCard != null)
            {
                borderUpdatesCard.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#272727");
                borderUpdatesCard.BorderBrush = isLight ? HexBrush("#E5E5E5") : HexBrush("#383838");
            }
            if (txtModalUpdatesTitle != null) txtModalUpdatesTitle.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (borderModalUpdatesIcon != null)
            {
                borderModalUpdatesIcon.Background = isLight ? HexBrush("#0067C0") : HexBrush("#0078D4");
            }
            if (btnCloseUpdates != null) btnCloseUpdates.Foreground = isLight ? HexBrush("#5D5D5D") : HexBrush("#8A8A8A");

            if (borderUpdateStatusCard != null)
            {
                borderUpdateStatusCard.Background = isLight ? HexBrush("#F9F9F9") : HexBrush("#1E1E1E");
                borderUpdateStatusCard.BorderBrush = isLight ? HexBrush("#E5E5E5") : HexBrush("#383838");
            }
            if (borderUpdateIconBadge != null)
            {
                borderUpdateIconBadge.Background = isLight ? HexBrush("#DCFCE7") : HexBrush("#064E3B");
                borderUpdateIconBadge.BorderBrush = isLight ? HexBrush("#86EFAC") : HexBrush("#059669");
            }
            if (pathUpdateStatusIcon != null)
            {
                pathUpdateStatusIcon.Fill = isLight ? HexBrush("#16A34A") : HexBrush("#34D399");
            }
            if (txtUpdateDetail != null) txtUpdateDetail.Foreground = isLight ? HexBrush("#5D5D5D") : HexBrush("#94A3B8");

            if (borderChangelogCard != null)
            {
                borderChangelogCard.Background = isLight ? HexBrush("#F9F9F9") : HexBrush("#1E1E1E");
                borderChangelogCard.BorderBrush = isLight ? HexBrush("#E5E5E5") : HexBrush("#383838");
            }
            if (txtChangelogHeading != null) txtChangelogHeading.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (pnlChangelogList != null)
            {
                foreach (UIElement child in pnlChangelogList.Children)
                {
                    TextBlock tb = child as TextBlock;
                    if (tb != null) tb.Foreground = isLight ? HexBrush("#5D5D5D") : HexBrush("#A0A0A0");
                }
            }
            if (btnCheckUpdatesAgain != null)
            {
                btnCheckUpdatesAgain.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2E2E2E");
                btnCheckUpdatesAgain.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
                btnCheckUpdatesAgain.BorderBrush = isLight ? HexBrush("#D1D5DB") : HexBrush("#3D3D3D");
            }

            // Settings Modal
            if (borderSettingsCard != null)
            {
                borderSettingsCard.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#272727");
                borderSettingsCard.BorderBrush = isLight ? HexBrush("#E5E5E5") : HexBrush("#383838");
            }
            if (txtModalSettingsTitle != null) txtModalSettingsTitle.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (badgeSettingsConfig != null)
            {
                badgeSettingsConfig.Background = isLight ? HexBrush("#EBF3FB") : HexBrush("#1E293B");
                badgeSettingsConfig.BorderBrush = isLight ? HexBrush("#CCE4F7") : HexBrush("#334155");
            }
            if (txtSettingsConfig != null) txtSettingsConfig.Foreground = isLight ? HexBrush("#005FB8") : HexBrush("#38BDF8");
            if (btnCloseSettings != null) btnCloseSettings.Foreground = isLight ? HexBrush("#5D5D5D") : HexBrush("#8A8A8A");

            if (txtSettingsSec1Title != null) txtSettingsSec1Title.Foreground = isLight ? HexBrush("#0067C0") : HexBrush("#38BDF8");
            if (borderSettingsSec1 != null)
            {
                borderSettingsSec1.Background = isLight ? HexBrush("#F9F9F9") : HexBrush("#1E1E1E");
                borderSettingsSec1.BorderBrush = isLight ? HexBrush("#E5E5E5") : HexBrush("#383838");
            }
            if (chkSettingSilent != null) chkSettingSilent.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (chkSettingAgreements != null) chkSettingAgreements.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (chkSettingAutoTheme != null) chkSettingAutoTheme.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");

            if (txtSettingsSec2Title != null) txtSettingsSec2Title.Foreground = isLight ? HexBrush("#0067C0") : HexBrush("#38BDF8");
            if (borderSettingsSec2 != null)
            {
                borderSettingsSec2.Background = isLight ? HexBrush("#F9F9F9") : HexBrush("#1E1E1E");
                borderSettingsSec2.BorderBrush = isLight ? HexBrush("#E5E5E5") : HexBrush("#383838");
            }
            if (txtSettingOfflineDirLbl != null) txtSettingOfflineDirLbl.Foreground = isLight ? HexBrush("#5D5D5D") : HexBrush("#94A3B8");
            if (txtSettingOfflineDir != null)
            {
                txtSettingOfflineDir.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#0B0D11");
                txtSettingOfflineDir.BorderBrush = isLight ? HexBrush("#D1D5DB") : HexBrush("#252B38");
                txtSettingOfflineDir.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            }
            if (btnBrowseOfflineDir != null)
            {
                btnBrowseOfflineDir.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2E2E2E");
                btnBrowseOfflineDir.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
                btnBrowseOfflineDir.BorderBrush = isLight ? HexBrush("#D1D5DB") : HexBrush("#3D3D3D");
            }
            if (btnClearCache != null)
            {
                btnClearCache.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2E2E2E");
                btnClearCache.Foreground = isLight ? HexBrush("#5D5D5D") : HexBrush("#94A3B8");
                btnClearCache.BorderBrush = isLight ? HexBrush("#D1D5DB") : HexBrush("#3D3D3D");
            }

            if (txtSettingsSec3Title != null) txtSettingsSec3Title.Foreground = isLight ? HexBrush("#0067C0") : HexBrush("#38BDF8");
            if (borderSettingsSec3 != null)
            {
                borderSettingsSec3.Background = isLight ? HexBrush("#F9F9F9") : HexBrush("#1E1E1E");
                borderSettingsSec3.BorderBrush = isLight ? HexBrush("#E5E5E5") : HexBrush("#383838");
            }
            if (txtSettingCatalogInfo != null) txtSettingCatalogInfo.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (txtSettingCatalogSource != null) txtSettingCatalogSource.Foreground = isLight ? HexBrush("#5D5D5D") : HexBrush("#64748B");
            if (btnExportCatalog != null)
            {
                btnExportCatalog.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2E2E2E");
                btnExportCatalog.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
                btnExportCatalog.BorderBrush = isLight ? HexBrush("#D1D5DB") : HexBrush("#3D3D3D");
            }

            if (btnCancelSettings != null)
            {
                btnCancelSettings.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#262D3D");
                btnCancelSettings.Foreground = isLight ? HexBrush("#5D5D5D") : HexBrush("#A0AEC0");
                btnCancelSettings.BorderBrush = isLight ? HexBrush("#D1D5DB") : HexBrush("#353E50");
            }

            // About Modal
            if (borderAboutCard != null)
            {
                borderAboutCard.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#272727");
                borderAboutCard.BorderBrush = isLight ? HexBrush("#E5E5E5") : HexBrush("#383838");
            }
            if (txtModalAboutTitle != null) txtModalAboutTitle.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (btnCloseAbout != null) btnCloseAbout.Foreground = isLight ? HexBrush("#5D5D5D") : HexBrush("#8A8A8A");

            if (borderBrandLogo != null)
            {
                borderBrandLogo.Background = Brushes.Transparent;
                borderBrandLogo.BorderBrush = Brushes.Transparent;
                borderBrandLogo.BorderThickness = new Thickness(0);
            }

            if (borderAboutLogoBox != null)
            {
                borderAboutLogoBox.Background = Brushes.Transparent;
                borderAboutLogoBox.BorderBrush = Brushes.Transparent;
                borderAboutLogoBox.BorderThickness = new Thickness(0);
            }
            if (txtAboutFleet != null) txtAboutFleet.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (txtAboutForge != null) txtAboutForge.Foreground = isLight ? HexBrush("#0067C0") : HexBrush("#38BDF8");
            if (txtAboutTagline != null) txtAboutTagline.Foreground = isLight ? HexBrush("#5D5D5D") : HexBrush("#94A3B8");

            if (badgeAboutCompany != null)
            {
                badgeAboutCompany.Background = isLight ? HexBrush("#EBF3FB") : HexBrush("#1E293B");
                badgeAboutCompany.BorderBrush = isLight ? HexBrush("#CCE4F7") : HexBrush("#334155");
            }
            if (txtAboutCompany != null) txtAboutCompany.Foreground = isLight ? HexBrush("#005FB8") : HexBrush("#38BDF8");

            if (badgeAboutEdition != null)
            {
                badgeAboutEdition.Background = isLight ? HexBrush("#F0F0F0") : HexBrush("#1E293B");
                badgeAboutEdition.BorderBrush = isLight ? HexBrush("#E5E5E5") : HexBrush("#334155");
            }
            if (txtAboutEdition != null) txtAboutEdition.Foreground = isLight ? HexBrush("#5D5D5D") : HexBrush("#94A3B8");

            if (borderAboutBio != null)
            {
                borderAboutBio.Background = isLight ? HexBrush("#F9F9F9") : HexBrush("#1E1E1E");
                borderAboutBio.BorderBrush = isLight ? HexBrush("#E5E5E5") : HexBrush("#383838");
            }
            if (txtAboutBioText != null) txtAboutBioText.Foreground = isLight ? HexBrush("#333333") : HexBrush("#CBD5E1");

            if (borderAboutSpecs != null)
            {
                borderAboutSpecs.Background = isLight ? HexBrush("#F9F9F9") : HexBrush("#1E1E1E");
                borderAboutSpecs.BorderBrush = isLight ? HexBrush("#E5E5E5") : HexBrush("#383838");
            }
            if (txtSpecOrgLbl != null) txtSpecOrgLbl.Foreground = HexBrush("#64748B");
            if (txtSpecOrgVal != null) txtSpecOrgVal.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (txtSpecEngLbl != null) txtSpecEngLbl.Foreground = HexBrush("#64748B");
            if (txtSpecEngVal != null) txtSpecEngVal.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (txtSpecLibLbl != null) txtSpecLibLbl.Foreground = HexBrush("#64748B");
            if (txtSpecLibVal != null) txtSpecLibVal.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");

            if (txtSpecArchLbl != null) txtSpecArchLbl.Foreground = HexBrush("#64748B");
            if (txtSpecArchVal != null) txtSpecArchVal.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (txtSpecChanLbl != null) txtSpecChanLbl.Foreground = HexBrush("#64748B");
            if (txtSpecChanVal != null) txtSpecChanVal.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (txtSpecLicLbl != null) txtSpecLicLbl.Foreground = HexBrush("#64748B");
            if (txtSpecLicVal != null) txtSpecLicVal.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");

            if (txtAboutCopyright != null) txtAboutCopyright.Foreground = HexBrush("#64748B");

            // Steps Guide on Quick Setup
            if (borderStepsGuide != null)
            {
                borderStepsGuide.Background = isLight ? HexBrush("#F0F7FF") : HexBrush("#162032");
                borderStepsGuide.BorderBrush = isLight ? HexBrush("#CCE4F7") : HexBrush("#1E3A5F");
            }
            if (txtStep1Title != null) txtStep1Title.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (txtStep2Title != null) txtStep2Title.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (txtStep3Title != null) txtStep3Title.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            if (txtStep1Desc != null) txtStep1Desc.Foreground = isLight ? HexBrush("#5D5D5D") : HexBrush("#94A3B8");
            if (txtStep2Desc != null) txtStep2Desc.Foreground = isLight ? HexBrush("#5D5D5D") : HexBrush("#94A3B8");
            if (txtStep3Desc != null) txtStep3Desc.Foreground = isLight ? HexBrush("#5D5D5D") : HexBrush("#94A3B8");
            if (txtStepArrow1 != null) txtStepArrow1.Foreground = isLight ? HexBrush("#94A3B8") : HexBrush("#475569");
            if (txtStepArrow2 != null) txtStepArrow2.Foreground = isLight ? HexBrush("#94A3B8") : HexBrush("#475569");

            // Tutorial Modal
            if (borderTutorialCard != null)
            {
                borderTutorialCard.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#181B22");
                borderTutorialCard.BorderBrush = isLight ? HexBrush("#E2E8F0") : HexBrush("#2D3342");
            }
            if (txtModalTutorialTitle != null) txtModalTutorialTitle.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#FFFFFF");
            if (txtModalTutorialSubtitle != null) txtModalTutorialSubtitle.Foreground = isLight ? HexBrush("#475569") : HexBrush("#94A3B8");
            if (btnCloseTutorial != null) btnCloseTutorial.Foreground = isLight ? HexBrush("#64748B") : HexBrush("#8A8A8A");

            // Tutorial Step 1
            if (cardTutStep1Hero != null)
            {
                cardTutStep1Hero.Background = isLight ? HexBrush("#F0F9FF") : HexBrush("#11141A");
                cardTutStep1Hero.BorderBrush = isLight ? HexBrush("#BAE6FD") : HexBrush("#232938");
            }
            if (txtTutStep1HeroTitle != null) txtTutStep1HeroTitle.Foreground = isLight ? HexBrush("#0369A1") : HexBrush("#38BDF8");
            if (txtTutStep1HeroDesc != null) txtTutStep1HeroDesc.Foreground = isLight ? HexBrush("#334155") : HexBrush("#CBD5E1");

            if (txtTutStep1Box1Title != null) txtTutStep1Box1Title.Foreground = isLight ? HexBrush("#047857") : HexBrush("#34D399");
            if (txtTutStep1Box1Desc != null) txtTutStep1Box1Desc.Foreground = isLight ? HexBrush("#334155") : HexBrush("#94A3B8");
            if (txtTutStep1Box2Title != null) txtTutStep1Box2Title.Foreground = isLight ? HexBrush("#0284C7") : HexBrush("#38BDF8");
            if (txtTutStep1Box2Desc != null) txtTutStep1Box2Desc.Foreground = isLight ? HexBrush("#334155") : HexBrush("#94A3B8");
            if (txtTutStep1Box3Title != null) txtTutStep1Box3Title.Foreground = isLight ? HexBrush("#D97706") : HexBrush("#FBBF24");
            if (txtTutStep1Box3Desc != null) txtTutStep1Box3Desc.Foreground = isLight ? HexBrush("#334155") : HexBrush("#94A3B8");

            // Tutorial Step 2
            if (txtTutStep2Box1Title != null) txtTutStep2Box1Title.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#FFFFFF");
            if (txtTutStep2Box1Desc != null) txtTutStep2Box1Desc.Foreground = isLight ? HexBrush("#334155") : HexBrush("#94A3B8");
            if (txtTutStep2Box2Title != null) txtTutStep2Box2Title.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#FFFFFF");
            if (txtTutStep2Box2Desc != null) txtTutStep2Box2Desc.Foreground = isLight ? HexBrush("#334155") : HexBrush("#94A3B8");
            if (txtTutStep2Box3Title != null) txtTutStep2Box3Title.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#FFFFFF");
            if (txtTutStep2Box3Desc != null) txtTutStep2Box3Desc.Foreground = isLight ? HexBrush("#334155") : HexBrush("#94A3B8");

            // Tutorial Step 3
            if (txtTutStep3Box1Title != null) txtTutStep3Box1Title.Foreground = isLight ? HexBrush("#0284C7") : HexBrush("#38BDF8");
            if (txtTutStep3Box1Desc != null) txtTutStep3Box1Desc.Foreground = isLight ? HexBrush("#334155") : HexBrush("#94A3B8");
            if (txtTutStep3Box2Title != null) txtTutStep3Box2Title.Foreground = isLight ? HexBrush("#047857") : HexBrush("#34D399");
            if (txtTutStep3Box2Desc != null) txtTutStep3Box2Desc.Foreground = isLight ? HexBrush("#334155") : HexBrush("#94A3B8");
            if (txtTutStep3Box3Title != null) txtTutStep3Box3Title.Foreground = isLight ? HexBrush("#D97706") : HexBrush("#FBBF24");
            if (txtTutStep3Box3Desc != null) txtTutStep3Box3Desc.Foreground = isLight ? HexBrush("#334155") : HexBrush("#94A3B8");

            Border[] tutBoxes = new Border[] {
                cardTutStep1Box1, cardTutStep1Box2, cardTutStep1Box3,
                cardTutStep2Box1, cardTutStep2Box2, cardTutStep2Box3,
                cardTutStep3Box1, cardTutStep3Box2, cardTutStep3Box3
            };
            foreach (Border box in tutBoxes)
            {
                if (box != null)
                {
                    box.Background = isLight ? HexBrush("#F8FAFC") : HexBrush("#11141A");
                    box.BorderBrush = isLight ? HexBrush("#E2E8F0") : HexBrush("#232938");
                }
            }
            if (btnTutorialBack != null)
            {
                btnTutorialBack.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2B2B2B");
                btnTutorialBack.BorderBrush = isLight ? HexBrush("#D1D5DB") : HexBrush("#3D3D3D");
                btnTutorialBack.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#CCCCCC");
            }
            if (btnTutorialNext != null)
            {
                btnTutorialNext.Background = currentTutorialStep == 3 ? HexBrush("#10B981") : (isLight ? HexBrush("#0067C0") : HexBrush("#0078D4"));
            }
            if (chkDontShowTutorialAgain != null)
            {
                chkDontShowTutorialAgain.Foreground = isLight ? HexBrush("#5D5D5D") : HexBrush("#8A8A8A");
            }
            if (btnQuickGuide != null)
            {
                btnQuickGuide.Background = isLight ? HexBrush("#0067C0") : HexBrush("#0078D4");
                btnQuickGuide.Foreground = HexBrush("#FFFFFF");
            }

            UpdateTutorialStepView();

            if (txtToggleThemeIcon != null)
            {
                txtToggleThemeIcon.Text = isLight ? "🌙" : "☀️";
            }
            if (btnToggleTheme != null)
            {
                btnToggleTheme.ToolTip = isLight ? "Switch to Dark Theme" : "Switch to Light Theme";
                btnToggleTheme.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2D2D2D");
                btnToggleTheme.BorderBrush = isLight ? HexBrush("#D1D5DB") : HexBrush("#3D3D3D");
                btnToggleTheme.Foreground = isLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
            }
            if (btnHelpMenu != null)
            {
                btnHelpMenu.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2B2B2B");
                btnHelpMenu.BorderBrush = isLight ? HexBrush("#D1D5DB") : HexBrush("#404040");
                btnHelpMenu.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#FFFFFF");
            }

            BuildEmojiChips();
            BuildCloudPresets();

            // Re-render role cards and catalog with active theme
            if (config != null)
            {
                EnsureCategories();
                BuildRoleCards();
                BuildCatalog();
                SelectRole(activeRoleId);
            }

            UpdateDwmTheme(isLight);
        }

        private void BuildEmojiChips()
        {
            if (emojiChipsPanel == null) return;
            emojiChipsPanel.Children.Clear();
            string[] emojis = new string[] { "💻", "🎨", "🧪", "📄", "🚀", "📊", "🛠️", "🎬", "🤖", "📱", "🔒", "⚡", "🌐", "🎮", "💼" };
            foreach (string emoji in emojis)
            {
                string curEmoji = emoji;
                Button b = new Button()
                {
                    Content = curEmoji,
                    FontFamily = new FontFamily("Segoe UI Emoji, Segoe UI Symbol, Segoe UI"),
                    FontSize = 14,
                    Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC"),
                    Width = 32,
                    Height = 28,
                    Margin = new Thickness(0, 0, 4, 4),
                    Background = currentThemeIsLight ? HexBrush("#F1F5F9") : HexBrush("#2B2B2B"),
                    BorderBrush = currentThemeIsLight ? HexBrush("#CBD5E1") : HexBrush("#3D3D3D"),
                    BorderThickness = new Thickness(1),
                    Cursor = Cursors.Hand
                };
                b.Click += delegate
                {
                    txtGroupIconInput.Text = curEmoji;
                };
                emojiChipsPanel.Children.Add(b);
            }
        }

        private void AttachNavHover(Button btn, string viewName)
        {
            if (btn == null) return;
            btn.MouseEnter += delegate
            {
                if (currentView != viewName)
                {
                    btn.Background = currentThemeIsLight ? HexBrush("#EAEAEA") : HexBrush("#262626");
                }
            };
            btn.MouseLeave += delegate
            {
                if (currentView != viewName)
                {
                    btn.Background = Brushes.Transparent;
                }
            };
        }

        private void UpdateNavState(Button btn, Border ind, TextBlock icon, TextBlock txt, bool isActive)
        {
            if (btn == null) return;
            if (isActive)
            {
                btn.Background = currentThemeIsLight ? HexBrush("#EAEAEA") : HexBrush("#2C2C2C");
                btn.BorderBrush = Brushes.Transparent;
                btn.BorderThickness = new Thickness(0);
                if (ind != null)
                {
                    ind.Visibility = Visibility.Visible;
                    ind.Background = windowsAccentBrush;
                }
                if (icon != null) icon.Foreground = windowsAccentBrush;
                if (txt != null)
                {
                    txt.Foreground = currentThemeIsLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
                    txt.FontWeight = FontWeights.SemiBold;
                }
            }
            else
            {
                btn.Background = Brushes.Transparent;
                btn.BorderBrush = Brushes.Transparent;
                btn.BorderThickness = new Thickness(0);
                if (ind != null) ind.Visibility = Visibility.Collapsed;
                if (icon != null) icon.Foreground = currentThemeIsLight ? HexBrush("#5D5D5D") : HexBrush("#B0B0B0");
                if (txt != null)
                {
                    txt.Foreground = currentThemeIsLight ? HexBrush("#5D5D5D") : HexBrush("#D0D0D0");
                    txt.FontWeight = FontWeights.Normal;
                }
            }
        }

        private void SwitchView(string viewName)
        {
            currentView = viewName;
            viewSetup.Visibility = Visibility.Collapsed;
            viewCatalog.Visibility = Visibility.Collapsed;
            viewCloud.Visibility = Visibility.Collapsed;
            viewLogs.Visibility = Visibility.Collapsed;

            UpdateNavState(navBtnSetup, navIndSetup, navIconSetup, navTxtSetup, viewName == "setup");
            UpdateNavState(navBtnCatalog, navIndCatalog, navIconCatalog, navTxtCatalog, viewName == "catalog");
            UpdateNavState(navBtnCloud, navIndCloud, navIconCloud, navTxtCloud, viewName == "cloud");
            UpdateNavState(navBtnLogs, navIndLogs, navIconLogs, navTxtLogs, viewName == "logs");

            if (borderHeaderSearch != null)
            {
                borderHeaderSearch.Visibility = Visibility.Visible;
            }

            if (viewName == "setup")
            {
                viewSetup.Visibility = Visibility.Visible;
                if (txtPageTitle != null) txtPageTitle.Text = "Quick setup";
                if (txtPageSubtitle != null) txtPageSubtitle.Text = "Choose an app pack or select individual apps to install on your computer in one click.";
            }
            else if (viewName == "catalog")
            {
                viewCatalog.Visibility = Visibility.Visible;
                if (txtPageTitle != null) txtPageTitle.Text = "All applications";
                if (txtPageSubtitle != null) txtPageSubtitle.Text = "Search and select from 16,349 apps available to install.";
                if (catalogNeedsRefresh)
                {
                    catalogNeedsRefresh = false;
                    ApplyFilters();
                }
            }
            else if (viewName == "cloud")
            {
                viewCloud.Visibility = Visibility.Visible;
                if (txtPageTitle != null) txtPageTitle.Text = "Cloud backup & sync";
                if (txtPageSubtitle != null) txtPageSubtitle.Text = "Save or share your app list online to easily set up other computers.";
            }
            else if (viewName == "logs")
            {
                viewLogs.Visibility = Visibility.Visible;
                if (txtPageTitle != null) txtPageTitle.Text = "Install history & logs";
                if (txtPageSubtitle != null) txtPageSubtitle.Text = "Watch live installation progress and see the status of all your apps.";
            }

            if (btnSearchWingetOnlineBottom != null)
            {
                btnSearchWingetOnlineBottom.Visibility = (viewName == "catalog") ? Visibility.Visible : Visibility.Collapsed;
            }

            UpdateBottomBarVisibility();
        }

        private void UpdateBottomBarVisibility()
        {
            bool isLogsView = (currentView == "logs");
            bool isInstalling = (btnStop != null && btnStop.Visibility == Visibility.Visible);

            if (btnOpenOfflineFolder != null)
            {
                btnOpenOfflineFolder.Visibility = isLogsView ? Visibility.Collapsed : Visibility.Visible;
            }

            if (btnInstall != null)
            {
                btnInstall.Visibility = (isLogsView || isInstalling) ? Visibility.Collapsed : Visibility.Visible;
            }

            if (borderBottomBar != null)
            {
                if (isLogsView)
                {
                    borderBottomBar.Visibility = isInstalling ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    borderBottomBar.Visibility = Visibility.Visible;
                }
            }
        }

        private void LoadSettings()
        {
            if (File.Exists(settingsPath))
            {
                try
                {
                    string json = File.ReadAllText(settingsPath, Encoding.UTF8);
                    settings = jsonSerializer.Deserialize<SettingsModel>(json);
                }
                catch { }
            }

            if (settings == null) settings = new SettingsModel();

            if (string.IsNullOrWhiteSpace(settings.cloudConfigUrl))
            {
                settings.cloudConfigUrl = AppUrls.CloudSyncUrl;
            }

            txtCloudUrlInput.Text = settings.cloudConfigUrl ?? "";
            chkAutoSync.IsChecked = settings.autoSyncOnLaunch;
        }

        private void SaveSettings()
        {
            try
            {
                string json = jsonSerializer.Serialize(settings);
                File.WriteAllText(settingsPath, json, Encoding.UTF8);
            }
            catch { }
        }

        private void SaveConfigToFile()
        {
            try
            {
                string json = jsonSerializer.Serialize(config);
                File.WriteAllText(configPath, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving apps.json: " + ex.Message, "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string ConvertDirectUrl(string rawUrl)
        {
            if (string.IsNullOrWhiteSpace(rawUrl)) return "";
            string url = rawUrl.Trim();

            Match ghMatch = Regex.Match(url, @"github\.com/([^/]+)/([^/]+)/blob/([^/]+)/(.+)");
            if (ghMatch.Success)
            {
                return string.Format("https://raw.githubusercontent.com/{0}/{1}/{2}/{3}",
                    ghMatch.Groups[1].Value, ghMatch.Groups[2].Value, ghMatch.Groups[3].Value, ghMatch.Groups[4].Value);
            }

            Match gdMatch = Regex.Match(url, @"drive\.google\.com/(?:file/d/|open\?id=)([a-zA-Z0-9_-]+)");
            if (gdMatch.Success)
            {
                return string.Format("https://drive.google.com/uc?export=download&id={0}", gdMatch.Groups[1].Value);
            }

            return url;
        }

        public static string FixMojibake(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            if (text.Contains("ð") || text.Contains("â") || text.Contains("Ã"))
            {
                try
                {
                    byte[] bytes = Encoding.GetEncoding(1252).GetBytes(text);
                    string decoded = Encoding.UTF8.GetString(bytes);
                    if (!string.IsNullOrEmpty(decoded) && !decoded.Contains("\uFFFD"))
                    {
                        return decoded;
                    }
                }
                catch { }
            }
            return text;
        }

        public static string GetSafeGroupIcon(GroupModel grp)
        {
            if (grp == null) return "📦";
            string icon = FixMojibake(grp.icon);
            if (!string.IsNullOrWhiteSpace(icon) && !icon.Contains("ð") && !icon.Contains("â") && !icon.Contains("?"))
            {
                return icon.Trim();
            }
            string id = (grp.id ?? "").ToLowerInvariant();
            if (id.Contains("dev") || id.Contains("code")) return "💻";
            if (id.Contains("off") || id.Contains("prod")) return "💼";
            if (id.Contains("creat") || id.Contains("design") || id.Contains("media")) return "🎨";
            if (id.Contains("min") || id.Contains("light") || id.Contains("ess")) return "⚡";
            if (id.Contains("game") || id.Contains("play")) return "🎮";
            if (id.Contains("cloud") || id.Contains("sync")) return "☁️";
            if (id.Contains("sec") || id.Contains("shield")) return "🛡️";
            return "📦";
        }

        public static string GetSafeCategoryIcon(CategoryModel cat)
        {
            if (cat == null) return "📁";
            string icon = FixMojibake(cat.icon);
            if (!string.IsNullOrWhiteSpace(icon) && !icon.Contains("ð") && !icon.Contains("â") && !icon.Contains("?"))
            {
                return icon.Trim();
            }
            string id = (cat.id ?? "").ToLowerInvariant();
            if (id.Contains("browser")) return "🌐";
            if (id.Contains("dev")) return "💻";
            if (id.Contains("commun") || id.Contains("chat")) return "💬";
            if (id.Contains("product") || id.Contains("off")) return "📄";
            if (id.Contains("util") || id.Contains("tool")) return "🛠️";
            if (id.Contains("media") || id.Contains("creat")) return "🎬";
            if (id.Contains("sec")) return "🛡️";
            if (id.Contains("cloud")) return "☁️";
            if (id.Contains("game")) return "🎮";
            return "📁";
        }

        private void DeduplicateGroups()
        {
            if (config == null || config.groups == null || config.groups.Count <= 1) return;

            var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var uniqueGroups = new List<GroupModel>();
            bool hasDuplicates = false;

            foreach (GroupModel g in config.groups)
            {
                if (g == null) continue;
                string gid = (g.id ?? "").Trim();
                string gname = (g.name ?? "").Trim();

                if (!string.IsNullOrEmpty(gid) && !seenIds.Contains(gid) && !seenNames.Contains(gname))
                {
                    seenIds.Add(gid);
                    seenNames.Add(gname);
                    uniqueGroups.Add(g);
                }
                else
                {
                    hasDuplicates = true;
                }
            }

            if (hasDuplicates)
            {
                config.groups = uniqueGroups;
                try
                {
                    if (File.Exists(configPath))
                    {
                        string cleanedJson = jsonSerializer.Serialize(config);
                        File.WriteAllText(configPath, cleanedJson, Encoding.UTF8);
                    }
                }
                catch { }
            }
        }

        private void EnsureCategories()
        {
            if (config == null) config = new ConfigModel();
            if (config.groups == null) config.groups = new List<GroupModel>();
            if (config.categories == null) config.categories = new List<CategoryModel>();
            if (config.apps == null) config.apps = new List<AppModel>();

            DeduplicateGroups();

            foreach (GroupModel g in config.groups)
            {
                g.icon = GetSafeGroupIcon(g);
            }

            if (config.categories.Count == 0)
            {
                config.categories.Add(new CategoryModel() { id = "browsers", name = "Web Browsers", icon = "🌐" });
                config.categories.Add(new CategoryModel() { id = "development", name = "Development & Code", icon = "💻" });
                config.categories.Add(new CategoryModel() { id = "communication", name = "Communication & Collab", icon = "💬" });
                config.categories.Add(new CategoryModel() { id = "productivity", name = "Productivity & Office", icon = "📄" });
                config.categories.Add(new CategoryModel() { id = "utilities", name = "Utilities & Tools", icon = "🛠️" });
                config.categories.Add(new CategoryModel() { id = "media", name = "Media & Creative", icon = "🎬" });
                config.categories.Add(new CategoryModel() { id = "security", name = "Security & Network", icon = "🛡️" });
                config.categories.Add(new CategoryModel() { id = "cloud", name = "Cloud & Storage", icon = "☁️" });
                config.categories.Add(new CategoryModel() { id = "gaming", name = "Gaming & Engines", icon = "🎮" });
            }
            else
            {
                foreach (CategoryModel c in config.categories)
                {
                    c.icon = GetSafeCategoryIcon(c);
                }
            }
        }

        private void LoadConfigWithAutoSync()
        {
            if (config == null || config.apps == null || config.apps.Count == 0)
            {
                if (File.Exists(configPath))
                {
                    try
                    {
                        string json = File.ReadAllText(configPath, Encoding.UTF8);
                        config = jsonSerializer.Deserialize<ConfigModel>(json);
                    }
                    catch { }
                }
            }
            if (config == null) config = new ConfigModel();
            EnsureCategories();

            if (txtTopSyncStatus != null) txtTopSyncStatus.Text = "Local Cache";
            if (txtNavCloudStatus != null) txtNavCloudStatus.Text = "Local";

            // Asynchronously sync from cloud Git link on launch if enabled
            if (settings.autoSyncOnLaunch && !string.IsNullOrWhiteSpace(settings.cloudConfigUrl) && !settings.cloudConfigUrl.Contains("your-username"))
            {
                ThreadPool.QueueUserWorkItem(delegate
                {
                    try
                    {
                        string directUrl = ConvertDirectUrl(settings.cloudConfigUrl);
                        using (WebClient wc = new WebClient())
                        {
                            wc.Encoding = Encoding.UTF8;
                            wc.Headers.Add("User-Agent", "FleetForge/2.6.1");
                            string json = wc.DownloadString(directUrl);
                            var ser = new JavaScriptSerializer();
                            ser.MaxJsonLength = int.MaxValue;
                            ConfigModel downloaded = ser.Deserialize<ConfigModel>(json);
                            if (downloaded != null && downloaded.apps != null && downloaded.apps.Count > 0)
                            {
                                // CRITICAL SAFETY: Never replace a 16,000+ local catalog with a tiny sample (e.g. 25 apps)
                                if (config != null && config.apps != null && config.apps.Count > downloaded.apps.Count)
                                {
                                    return;
                                }

                                Dispatcher.Invoke(new Action(delegate
                                {
                                    config = downloaded;
                                    EnsureCategories();
                                    appDefaultIndices = null;
                                    try { File.WriteAllText(configPath, json, Encoding.UTF8); } catch { }
                                    if (txtTopSyncStatus != null)
                                    {
                                        txtTopSyncStatus.Text = "Catalog Synced";
                                        txtTopSyncStatus.Foreground = currentThemeIsLight ? HexBrush("#137333") : HexBrush("#4ADE80");
                                    }
                                    BuildRoleCards();
                                    BuildCatalog();
                                    SelectRole(activeRoleId);
                                }));
                            }
                        }
                    }
                    catch { }
                });
            }
        }

        private void SyncFromCloudAsync(string targetUrl)
        {
            if (string.IsNullOrWhiteSpace(targetUrl))
            {
                MessageBox.Show("Please enter a GitHub Raw URL or Google Drive link first.", "Missing URL", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            btnSyncNow.IsEnabled = false;
            btnSyncNow.Content = "Syncing...";

            ThreadPool.QueueUserWorkItem(delegate
            {
                string directUrl = ConvertDirectUrl(targetUrl);
                bool success = false;
                string message = "";
                ConfigModel newConfig = null;

                try
                {
                    try
                    {
                        ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | (SecurityProtocolType)12288 | (SecurityProtocolType)768 | SecurityProtocolType.Tls;
                    }
                    catch
                    {
                        try { ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | SecurityProtocolType.Tls; } catch { }
                    }

                    using (WebClient wc = new WebClient())
                    {
                        wc.Encoding = Encoding.UTF8;
                        wc.Headers.Add("User-Agent", "FleetForge/2.6.1");
                        string json = wc.DownloadString(directUrl);
                        newConfig = jsonSerializer.Deserialize<ConfigModel>(json);
                        if (newConfig != null && newConfig.apps != null && newConfig.apps.Count > 0)
                        {
                            File.WriteAllText(configPath, json, Encoding.UTF8);
                            settings.cloudConfigUrl = targetUrl;
                            settings.lastSyncTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            settings.lastSyncStatus = "Synced Successfully";
                            SaveSettings();
                            success = true;
                            message = string.Format("✓ Synced successfully! {0} packages loaded.", newConfig.apps.Count);
                        }
                        else
                        {
                            message = "Downloaded file is missing apps or groups.";
                        }
                    }
                }
                catch (WebException wex)
                {
                    HttpWebResponse resp = wex.Response as HttpWebResponse;
                    if (resp != null && resp.StatusCode == HttpStatusCode.NotFound)
                    {
                        message = "Sync failed (404 Not Found): Your GitHub repository appears to be Private. Change repository visibility to Public in GitHub (Settings > Danger Zone) to allow raw file syncing.";
                    }
                    else if (resp != null)
                    {
                        message = string.Format("Sync failed ({0}): {1}", (int)resp.StatusCode, resp.StatusDescription);
                    }
                    else
                    {
                        message = "Sync failed: " + wex.Message;
                    }
                }
                catch (Exception ex)
                {
                    message = "Sync failed: " + ex.Message;
                }

                Dispatcher.Invoke(new Action(delegate
                {
                    btnSyncNow.IsEnabled = true;
                    btnSyncNow.Content = "Sync Now";
                    syncResultBorder.Visibility = Visibility.Visible;

                    if (success)
                    {
                        config = newConfig;
                        EnsureCategories();
                        appDefaultIndices = null;
                        txtSyncResult.Text = message;
                        txtSyncResult.Foreground = currentThemeIsLight ? HexBrush("#137333") : HexBrush("#4ADE80");
                        if (txtTopSyncStatus != null)
                        {
                            txtTopSyncStatus.Text = "GitHub Synced";
                            txtTopSyncStatus.Foreground = currentThemeIsLight ? HexBrush("#137333") : HexBrush("#4ADE80");
                        }
                        if (txtNavCloudStatus != null) txtNavCloudStatus.Text = "Online";

                        BuildRoleCards();
                        BuildCatalog();
                        SelectRole(activeRoleId);
                    }
                    else
                    {
                        txtSyncResult.Text = "❌ " + message;
                        txtSyncResult.Foreground = HexBrush("#F87171");
                    }
                }));
            });
        }

        private void BuildCloudPresets()
        {
            if (presetsContainer == null) return;
            presetsContainer.Children.Clear();

            List<PresetProfileItem> presets = new List<PresetProfileItem>();
            presets.Add(new PresetProfileItem("full", "\uD83C\uDF10", "Complete Microsoft Winget Library", "16,349 PACKAGES", "Entire official Winget server repository with 16,349 packages across all 9 categories (Utilities, Browsers, Dev, Gaming, Office, etc.).", AppUrls.CloudSyncUrl));
            presets.Add(new PresetProfileItem("developer", "\uD83D\uDCBB", "Software Developer & DevOps Setup", "8 DEV TOOLS", "Curated workstation setup: VS Code, Git, Docker Desktop, Node.js LTS, Python 3.12, Postman, Windows Terminal, and PowerToys.", AppUrls.CloudSyncUrl));
            presets.Add(new PresetProfileItem("office", "\uD83D\uDCBC", "Enterprise Office & Productivity Pack", "8 APPS", "Corporate environment setup: Google Chrome, Mozilla Firefox, Slack, Zoom, Microsoft Teams, Adobe Acrobat Reader 64-bit, 7-Zip, Notion.", AppUrls.CloudSyncUrl));
            presets.Add(new PresetProfileItem("creative", "\uD83C\uDFA8", "Designer & Creative Media Studio", "7 APPS", "Digital media & graphics suite: Blender 3D, Figma, DaVinci Resolve, OBS Studio, GIMP, VLC Media Player, HandBrake.", AppUrls.CloudSyncUrl));
            presets.Add(new PresetProfileItem("minimal", "\u26A1", "Ultra-Lightweight Minimal Essentials", "5 TOOLS", "Fast, bare-minimum setup: Google Chrome, 7-Zip Archiver, VLC Media Player, Notepad++, Everything Search.", AppUrls.CloudSyncUrl));

            foreach (PresetProfileItem p in presets)
            {
                PresetProfileItem cur = p;
                Border card = new Border()
                {
                    Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#272727"),
                    BorderBrush = currentThemeIsLight ? HexBrush("#E5E5E5") : HexBrush("#383838"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(14, 12, 14, 12),
                    Margin = new Thickness(0, 0, 0, 8)
                };

                Grid g = new Grid();
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                TextBlock txtIcon = new TextBlock()
                {
                    Text = cur.Icon,
                    FontFamily = new FontFamily("Segoe UI Emoji, Segoe UI Symbol, Segoe UI"),
                    FontSize = 22,
                    Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC"),
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 14, 0)
                };
                Grid.SetColumn(txtIcon, 0);
                g.Children.Add(txtIcon);

                StackPanel info = new StackPanel() { VerticalAlignment = VerticalAlignment.Center };
                StackPanel titleRow = new StackPanel() { Orientation = Orientation.Horizontal };
                TextBlock txtTitle = new TextBlock()
                {
                    Text = cur.Title,
                    FontSize = 13,
                    FontWeight = FontWeights.Bold,
                    Foreground = currentThemeIsLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF")
                };
                titleRow.Children.Add(txtTitle);

                Border badge = new Border()
                {
                    Background = currentThemeIsLight ? HexBrush("#EBF3FB") : HexBrush("#1E293B"),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(6, 1, 6, 1),
                    Margin = new Thickness(8, 0, 0, 0)
                };
                TextBlock txtBadge = new TextBlock()
                {
                    Text = cur.Badge,
                    FontSize = 9,
                    FontWeight = FontWeights.Bold,
                    Foreground = currentThemeIsLight ? HexBrush("#005FB8") : HexBrush("#38BDF8")
                };
                badge.Child = txtBadge;
                titleRow.Children.Add(badge);
                info.Children.Add(titleRow);

                TextBlock txtDesc = new TextBlock()
                {
                    Text = cur.Desc,
                    FontSize = 11,
                    Foreground = currentThemeIsLight ? HexBrush("#5D5D5D") : HexBrush("#A0A0A0"),
                    Margin = new Thickness(0, 3, 0, 0),
                    TextWrapping = TextWrapping.Wrap
                };
                info.Children.Add(txtDesc);
                Grid.SetColumn(info, 1);
                g.Children.Add(info);

                StackPanel actions = new StackPanel() { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(14, 0, 0, 0) };

                Button btnSetUrl = new Button()
                {
                    Content = "Use URL",
                    Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#2E2E2E"),
                    Foreground = currentThemeIsLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF"),
                    BorderBrush = currentThemeIsLight ? HexBrush("#D1D5DB") : HexBrush("#3D3D3D"),
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(10, 6, 10, 6),
                    Margin = new Thickness(0, 0, 6, 0),
                    Cursor = Cursors.Hand,
                    FontSize = 11
                };
                btnSetUrl.Click += delegate
                {
                    txtCloudUrlInput.Text = cur.Url;
                    syncResultBorder.Visibility = Visibility.Visible;
                    txtSyncResult.Text = "Preset URL copied to configuration box. Click 'Sync Now' or 'Save URL'.";
                    txtSyncResult.Foreground = HexBrush("#38BDF8");
                };
                actions.Children.Add(btnSetUrl);

                Button btnApply = new Button()
                {
                    Content = "Load Preset",
                    Background = currentThemeIsLight ? HexBrush("#0067C0") : HexBrush("#0078D4"),
                    Foreground = HexBrush("#FFFFFF"),
                    BorderThickness = new Thickness(0),
                    FontWeight = FontWeights.SemiBold,
                    Padding = new Thickness(12, 6, 12, 6),
                    Cursor = Cursors.Hand,
                    FontSize = 11
                };
                btnApply.Click += delegate
                {
                    ApplyPresetProfile(cur.Id, cur.Title, cur.Url);
                };
                actions.Children.Add(btnApply);

                Grid.SetColumn(actions, 2);
                g.Children.Add(actions);

                card.Child = g;
                presetsContainer.Children.Add(card);
            }
        }

        private void ApplyPresetProfile(string presetId, string presetTitle, string presetUrl)
        {
            try
            {
                ConfigModel targetConfig = null;
                if (targetConfig == null)
                {
                    string samplePath = Path.Combine(appDir, "data", "sample_apps.json");
                    if (!File.Exists(samplePath)) samplePath = Path.Combine(appDir, "sample_apps.json");
                    string sampleJson = "";
                    if (File.Exists(samplePath))
                    {
                        sampleJson = File.ReadAllText(samplePath, Encoding.UTF8);
                    }
                    else
                    {
                        sampleJson = GetSampleJsonTemplate();
                    }
                    ConfigModel sampleModel = jsonSerializer.Deserialize<ConfigModel>(sampleJson);

                    if (presetId == "full")
                    {
                        targetConfig = sampleModel;
                    }
                    else if (sampleModel != null)
                    {
                        targetConfig = new ConfigModel();
                        targetConfig.groups = new List<GroupModel>();
                        GroupModel matched = null;
                        foreach (GroupModel g in sampleModel.groups)
                        {
                            if (g.id == presetId)
                            {
                                matched = g;
                                break;
                            }
                        }
                        if (matched != null)
                        {
                            targetConfig.groups.Add(matched);
                        }
                        else
                        {
                            targetConfig.groups.AddRange(sampleModel.groups);
                        }
                        targetConfig.apps = sampleModel.apps;
                        if (sampleModel.categories != null)
                        {
                            targetConfig.categories = new List<CategoryModel>(sampleModel.categories);
                        }
                    }
                }

                if (targetConfig != null && targetConfig.apps != null && targetConfig.apps.Count > 0)
                {
                    config = targetConfig;
                    EnsureCategories();
                    appDefaultIndices = null;
                    string outJson = jsonSerializer.Serialize(config);
                    File.WriteAllText(configPath, outJson, Encoding.UTF8);

                    settings.cloudConfigUrl = presetUrl;
                    settings.lastSyncTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    settings.lastSyncStatus = "Preset: " + presetTitle;
                    SaveSettings();

                    syncResultBorder.Visibility = Visibility.Visible;
                    txtSyncResult.Text = string.Format("✓ Profile '{0}' loaded successfully! ({1} packages, {2} role environments)",
                        presetTitle, config.apps.Count, config.groups.Count);
                    txtSyncResult.Foreground = currentThemeIsLight ? HexBrush("#137333") : HexBrush("#4ADE80");

                    if (txtTopSyncStatus != null)
                    {
                        txtTopSyncStatus.Text = "Catalog Synced";
                        txtTopSyncStatus.Foreground = currentThemeIsLight ? HexBrush("#137333") : HexBrush("#4ADE80");
                    }

                    BuildRoleCards();
                    BuildCatalog();
                    SelectRole(activeRoleId);
                    UpdateCount();

                    MessageBox.Show(
                        string.Format("Profile '{0}' has been loaded into your Application Catalog!\n\n• Packages: {1}\n• Roles: {2}\n• Saved to: apps.json",
                        presetTitle, config.apps.Count, config.groups.Count),
                        "Profile Activated",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading preset: " + ex.Message, "Preset Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportSampleJson()
        {
            try
            {
                string samplePath = Path.Combine(appDir, "data", "sample_apps.json");
                if (!File.Exists(samplePath)) samplePath = Path.Combine(appDir, "sample_apps.json");
                if (!File.Exists(samplePath))
                {
                    string dir = Path.GetDirectoryName(samplePath);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    File.WriteAllText(samplePath, GetSampleJsonTemplate(), Encoding.UTF8);
                }
                syncResultBorder.Visibility = Visibility.Visible;
                txtSyncResult.Text = "✓ sample_apps.json ready at: " + samplePath;
                txtSyncResult.Foreground = currentThemeIsLight ? HexBrush("#137333") : HexBrush("#4ADE80");
                Process.Start("explorer.exe", string.Format("/select,\"{0}\"", samplePath));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting sample JSON: " + ex.Message, "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetSampleJsonTemplate()
        {
            string samplePath = Path.Combine(appDir, "data", "sample_apps.json");
            if (!File.Exists(samplePath)) samplePath = Path.Combine(appDir, "sample_apps.json");
            if (File.Exists(samplePath))
            {
                return File.ReadAllText(samplePath, Encoding.UTF8);
            }
            return "{\n  \"groups\": [\n    {\n      \"id\": \"developer\",\n      \"name\": \"Software Developer\",\n      \"icon\": \"💻\",\n      \"description\": \"Essential development environments, version control, compilers, and debuggers.\",\n      \"apps\": [\n        \"Microsoft.VisualStudioCode\",\n        \"Git.Git\",\n        \"Docker.DockerDesktop\",\n        \"OpenJS.NodeJS.LTS\",\n        \"Python.Python.3.12\",\n        \"Postman.Postman\",\n        \"Microsoft.WindowsTerminal\",\n        \"Microsoft.PowerToys\"\n      ]\n    },\n    {\n      \"id\": \"office\",\n      \"name\": \"Office & Productivity\",\n      \"icon\": \"💼\",\n      \"description\": \"Standard business tools, document editors, messaging, and communication.\",\n      \"apps\": [\n        \"Google.Chrome\",\n        \"Mozilla.Firefox\",\n        \"SlackTechnologies.Slack\",\n        \"Zoom.Zoom\",\n        \"Microsoft.Teams\",\n        \"Adobe.Acrobat.Reader.64-bit\",\n        \"7zip.7zip\",\n        \"Notion.Notion\"\n      ]\n    }\n  ],\n  \"apps\": [\n    {\n      \"id\": \"Google.Chrome\",\n      \"name\": \"Google Chrome\",\n      \"category\": \"browsers\",\n      \"description\": \"Fast, secure web browser built by Google\",\n      \"offlineInstaller\": \"ChromeStandaloneSetup64.exe\",\n      \"silentArgs\": \"/silent /install\"\n    },\n    {\n      \"id\": \"Microsoft.VisualStudioCode\",\n      \"name\": \"Visual Studio Code\",\n      \"category\": \"development\",\n      \"description\": \"Code editor redefined and optimized for building modern web and cloud apps\",\n      \"offlineInstaller\": \"VSCodeUserSetup-x64.exe\",\n      \"silentArgs\": \"/VERYSILENT /NORESTART\"\n    },\n    {\n      \"id\": \"Git.Git\",\n      \"name\": \"Git for Windows\",\n      \"category\": \"development\",\n      \"description\": \"Fast, scalable, distributed revision control system\",\n      \"offlineInstaller\": \"\",\n      \"silentArgs\": \"/VERYSILENT /NORESTART\"\n    },\n    {\n      \"id\": \"7zip.7zip\",\n      \"name\": \"7-Zip\",\n      \"category\": \"utilities\",\n      \"description\": \"High-compression file archiver utility\",\n      \"offlineInstaller\": \"7z-x64.exe\",\n      \"silentArgs\": \"/S\"\n    }\n  ]\n}";
        }

        // =========================================================================
        // GROUP BUILDER & MANAGEMENT
        // =========================================================================
        private void BuildRoleCards()
        {
            roleCardsGrid.Children.Clear();
            roleCards.Clear();

            foreach (GroupModel grp in config.groups)
            {
                GroupModel currentGrp = grp;
                Border card = new Border();
                card.Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#272727");
                card.CornerRadius = new CornerRadius(8);
                card.Margin = new Thickness(6);
                card.Padding = new Thickness(18, 16, 18, 16);
                card.BorderThickness = new Thickness(1);
                card.BorderBrush = currentThemeIsLight ? HexBrush("#E5E5E5") : HexBrush("#383838");

                Grid g = new Grid();
                g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                // Header
                Grid hdr = new Grid();
                hdr.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                hdr.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                StackPanel titleStack = new StackPanel() { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
                Border iconBox = new Border()
                {
                    Width = 36,
                    Height = 36,
                    CornerRadius = new CornerRadius(6),
                    Background = currentThemeIsLight ? HexBrush("#F1F5F9") : HexBrush("#1E293B"),
                    BorderBrush = currentThemeIsLight ? HexBrush("#E2E8F0") : HexBrush("#384358"),
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(0, 0, 10, 0)
                };
                TextBlock tIcon = new TextBlock()
                {
                    Text = GetSafeGroupIcon(currentGrp),
                    FontFamily = new FontFamily("Segoe UI Emoji, Segoe UI Symbol, Segoe UI"),
                    FontSize = 18,
                    Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                iconBox.Child = tIcon;
                titleStack.Children.Add(iconBox);

                TextBlock tName = new TextBlock()
                {
                    Text = currentGrp.name,
                    FontSize = 15,
                    FontWeight = FontWeights.Bold,
                    Foreground = currentThemeIsLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF"),
                    VerticalAlignment = VerticalAlignment.Center
                };
                titleStack.Children.Add(tName);
                Grid.SetColumn(titleStack, 0);
                hdr.Children.Add(titleStack);

                Border badge = new Border();
                badge.Background = currentThemeIsLight ? HexBrush("#EBF3FB") : HexBrush("#1E293B");
                badge.BorderBrush = currentThemeIsLight ? HexBrush("#CCE4F7") : HexBrush("#334155");
                badge.BorderThickness = new Thickness(1);
                badge.CornerRadius = new CornerRadius(4);
                badge.Padding = new Thickness(8, 3, 8, 3);
                badge.VerticalAlignment = VerticalAlignment.Center;
                TextBlock cTxt = new TextBlock()
                {
                    Text = currentGrp.apps.Count + " apps",
                    FontSize = 11,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = currentThemeIsLight ? HexBrush("#005FB8") : HexBrush("#38BDF8")
                };
                badge.Child = cTxt;
                Grid.SetColumn(badge, 1);
                hdr.Children.Add(badge);

                Grid.SetRow(hdr, 0);
                g.Children.Add(hdr);

                // Description
                TextBlock desc = new TextBlock()
                {
                    Text = currentGrp.description,
                    FontSize = 12,
                    Foreground = currentThemeIsLight ? HexBrush("#5D5D5D") : HexBrush("#A0A0A0"),
                    Margin = new Thickness(0, 8, 0, 10),
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 16
                };
                Grid.SetRow(desc, 1);
                g.Children.Add(desc);

                // App pills preview
                WrapPanel wrap = new WrapPanel() { Margin = new Thickness(0, 0, 0, 14) };
                int shown = 0;
                foreach (string appId in currentGrp.apps)
                {
                    if (shown >= 5) break;
                    AppModel matchApp = config.apps.Find(delegate (AppModel a) { return a.id == appId; });
                    if (matchApp != null)
                    {
                        Border p = new Border();
                        p.Background = currentThemeIsLight ? HexBrush("#F3F3F3") : HexBrush("#202020");
                        p.BorderBrush = currentThemeIsLight ? HexBrush("#E5E5E5") : HexBrush("#383838");
                        p.BorderThickness = new Thickness(1);
                        p.CornerRadius = new CornerRadius(4);
                        p.Padding = new Thickness(7, 3, 7, 3);
                        p.Margin = new Thickness(0, 0, 5, 5);

                        TextBlock pt = new TextBlock()
                        {
                            Text = matchApp.name,
                            FontSize = 11,
                            Foreground = currentThemeIsLight ? HexBrush("#1B1B1B") : HexBrush("#D0D0D0")
                        };
                        p.Child = pt;
                        wrap.Children.Add(p);
                        shown++;
                    }
                }
                if (currentGrp.apps.Count > 5)
                {
                    Border extraBorder = new Border();
                    extraBorder.Background = currentThemeIsLight ? HexBrush("#EBF3FB") : HexBrush("#1E293B");
                    extraBorder.BorderBrush = currentThemeIsLight ? HexBrush("#CCE4F7") : HexBrush("#334155");
                    extraBorder.BorderThickness = new Thickness(1);
                    extraBorder.CornerRadius = new CornerRadius(4);
                    extraBorder.Padding = new Thickness(6, 3, 6, 3);
                    extraBorder.Margin = new Thickness(0, 0, 5, 5);

                    TextBlock extra = new TextBlock()
                    {
                        Text = "+" + (currentGrp.apps.Count - 5) + " more",
                        FontSize = 10,
                        FontWeight = FontWeights.Bold,
                        Foreground = currentThemeIsLight ? HexBrush("#005FB8") : HexBrush("#38BDF8")
                    };
                    extraBorder.Child = extra;
                    wrap.Children.Add(extraBorder);
                }
                Grid.SetRow(wrap, 2);
                g.Children.Add(wrap);

                // Action Buttons Row: [ Choose This Pack ]  [ ✏️ Change Apps ]  [ 📤 Export ]
                Grid btnRow = new Grid();
                btnRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                btnRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                btnRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                Button selectBtn = new Button()
                {
                    Content = "Select Pack",
                    Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#2E2E2E"),
                    Foreground = currentThemeIsLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF"),
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 12,
                    Padding = new Thickness(12, 6, 12, 6),
                    BorderThickness = new Thickness(1),
                    BorderBrush = currentThemeIsLight ? HexBrush("#D1D5DB") : HexBrush("#3D3D3D"),
                    Cursor = Cursors.Hand
                };
                selectBtn.Click += delegate { SelectRole(currentGrp.id); };
                Grid.SetColumn(selectBtn, 0);
                btnRow.Children.Add(selectBtn);

                Button editBtn = new Button()
                {
                    Content = "Customize Apps",
                    Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#2E2E2E"),
                    Foreground = currentThemeIsLight ? HexBrush("#5D5D5D") : HexBrush("#D0D0D0"),
                    FontSize = 12,
                    Padding = new Thickness(10, 6, 10, 6),
                    Margin = new Thickness(6, 0, 0, 0),
                    BorderThickness = new Thickness(1),
                    BorderBrush = currentThemeIsLight ? HexBrush("#D1D5DB") : HexBrush("#3D3D3D"),
                    Cursor = Cursors.Hand,
                    ToolTip = "Add or remove apps from this pack"
                };
                editBtn.Click += delegate { OpenEditGroupModal(currentGrp); };
                Grid.SetColumn(editBtn, 1);
                btnRow.Children.Add(editBtn);

                Button exportBtn = new Button()
                {
                    Content = "Export Pack",
                    Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#2E2E2E"),
                    Foreground = currentThemeIsLight ? HexBrush("#5D5D5D") : HexBrush("#D0D0D0"),
                    FontSize = 12,
                    Padding = new Thickness(10, 6, 10, 6),
                    Margin = new Thickness(6, 0, 0, 0),
                    BorderThickness = new Thickness(1),
                    BorderBrush = currentThemeIsLight ? HexBrush("#D1D5DB") : HexBrush("#3D3D3D"),
                    Cursor = Cursors.Hand,
                    ToolTip = "Export this app pack to a .json file"
                };
                exportBtn.Click += delegate { ExportGroup(currentGrp); };
                Grid.SetColumn(exportBtn, 2);
                btnRow.Children.Add(exportBtn);

                Grid.SetRow(btnRow, 3);
                g.Children.Add(btnRow);

                card.Child = g;

                card.MouseEnter += delegate
                {
                    if (activeRoleId != currentGrp.id)
                    {
                        card.Background = currentThemeIsLight ? HexBrush("#F9F9F9") : HexBrush("#2C2C2C");
                        card.BorderBrush = currentThemeIsLight ? HexBrush("#D1D5DB") : HexBrush("#444444");
                    }
                };
                card.MouseLeave += delegate
                {
                    if (activeRoleId != currentGrp.id)
                    {
                        card.Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#272727");
                        card.BorderBrush = currentThemeIsLight ? HexBrush("#E5E5E5") : HexBrush("#383838");
                    }
                };
                roleCardsGrid.Children.Add(card);

                roleCards.Add(new RoleCardView()
                {
                    Group = currentGrp,
                    Card = card,
                    SelectButton = selectBtn,
                    EditButton = editBtn,
                    ExportButton = exportBtn,
                    CountText = cTxt
                });
            }
        }

        private void SelectRole(string roleId)
        {
            activeRoleId = roleId;
            GroupModel targetGrp = config.groups.Find(delegate (GroupModel g) { return g.id == roleId; });
            if (targetGrp == null)
            {
                if (config.groups.Count > 0) targetGrp = config.groups[0];
                else return;
            }

            foreach (RoleCardView rc in roleCards)
            {
                if (rc.Group.id == targetGrp.id)
                {
                    rc.Card.BorderBrush = windowsAccentBrush;
                    rc.Card.BorderThickness = new Thickness(2);
                    rc.Card.Background = windowsAccentSubtleBrush;
                    rc.SelectButton.Content = "Selected";
                    rc.SelectButton.Background = windowsAccentBrush;
                    rc.SelectButton.Foreground = HexBrush("#FFFFFF");
                    rc.SelectButton.BorderThickness = new Thickness(0);
                }
                else
                {
                    rc.Card.BorderBrush = currentThemeIsLight ? HexBrush("#E5E5E5") : HexBrush("#383838");
                    rc.Card.BorderThickness = new Thickness(1);
                    rc.Card.Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#272727");
                    rc.SelectButton.Content = "Select Pack";
                    rc.SelectButton.Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#2E2E2E");
                    rc.SelectButton.BorderBrush = currentThemeIsLight ? HexBrush("#D1D5DB") : HexBrush("#3D3D3D");
                    rc.SelectButton.Foreground = currentThemeIsLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF");
                    rc.SelectButton.BorderThickness = new Thickness(1);
                }
            }

            isUpdatingSelection = true;
            try
            {
                selectedAppIds.Clear();
                if (targetGrp != null)
                {
                    foreach (string id in targetGrp.apps) selectedAppIds.Add(id);
                }

                if (localCardsContainer != null)
                {
                    foreach (UIElement child in localCardsContainer.Children)
                    {
                        Border card = child as Border;
                        if (card != null && card.Tag is AppModel)
                        {
                            AppModel app = card.Tag as AppModel;
                            Grid g = card.Child as Grid;
                            if (g != null && g.Children.Count > 0)
                            {
                                CheckBox cb = g.Children[0] as CheckBox;
                                if (cb != null)
                                {
                                    bool isSel = selectedAppIds.Contains(app.id);
                                    cb.IsChecked = isSel;
                                    if (isSel)
                                    {
                                        card.Background = currentThemeIsLight ? HexBrush("#F0F7FF") : HexBrush("#1C2E42");
                                        card.BorderBrush = currentThemeIsLight ? HexBrush("#0067C0") : HexBrush("#4CC2FF");
                                    }
                                    else
                                    {
                                        card.Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#272727");
                                        card.BorderBrush = currentThemeIsLight ? HexBrush("#E5E5E5") : HexBrush("#383838");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                isUpdatingSelection = false;
            }

            UpdateCount();
            UpdateMasterCheckboxState();
            UpdateHeaderCount();
            if (txtStatus != null) txtStatus.Text = "Role selected: " + targetGrp.name;

            if (currentView == "catalog")
            {
                ApplyFilters();
            }
            else
            {
                catalogNeedsRefresh = true;
            }
        }

        // =========================================================================
        // CREATE / EDIT / DELETE MODAL LOGIC
        // =========================================================================
        private void OpenCreateGroupModal()
        {
            currentEditingGroupId = null;
            txtModalTitle.Text = "Create New App Pack";
            txtGroupNameInput.Text = "";
            txtGroupIconInput.Text = "🚀";
            txtGroupDescInput.Text = "";
            btnDeleteGroup.Visibility = Visibility.Collapsed;
            if (btnModalExportGroup != null) btnModalExportGroup.Visibility = Visibility.Collapsed;

            PopulateModalAppChecklist(new List<string>());
            groupModalOverlay.Visibility = Visibility.Visible;
            txtGroupNameInput.Focus();
        }

        private void OpenEditGroupModal(GroupModel grp)
        {
            currentEditingGroupId = grp.id;
            txtModalTitle.Text = "Edit App Pack: " + grp.name;
            txtGroupNameInput.Text = grp.name;
            txtGroupIconInput.Text = GetSafeGroupIcon(grp);
            txtGroupDescInput.Text = grp.description;
            btnDeleteGroup.Visibility = Visibility.Visible;
            if (btnModalExportGroup != null) btnModalExportGroup.Visibility = Visibility.Visible;

            PopulateModalAppChecklist(grp.apps);
            groupModalOverlay.Visibility = Visibility.Visible;
            txtGroupNameInput.Focus();
        }

        private void UpdateModalTabStyles()
        {
            if (btnModalFilterTabAll != null && btnModalFilterTabSelected != null)
            {
                if (modalShowSelectedOnly)
                {
                    btnModalFilterTabSelected.Background = currentThemeIsLight ? HexBrush("#0067C0") : HexBrush("#0078D4");
                    btnModalFilterTabSelected.Foreground = HexBrush("#FFFFFF");
                    btnModalFilterTabSelected.BorderThickness = new Thickness(0);

                    btnModalFilterTabAll.Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#2E2E2E");
                    btnModalFilterTabAll.Foreground = currentThemeIsLight ? HexBrush("#1B1B1B") : HexBrush("#CCCCCC");
                    btnModalFilterTabAll.BorderThickness = new Thickness(1);
                    btnModalFilterTabAll.BorderBrush = currentThemeIsLight ? HexBrush("#D1D5DB") : HexBrush("#3D3D3D");
                }
                else
                {
                    btnModalFilterTabAll.Background = currentThemeIsLight ? HexBrush("#0067C0") : HexBrush("#0078D4");
                    btnModalFilterTabAll.Foreground = HexBrush("#FFFFFF");
                    btnModalFilterTabAll.BorderThickness = new Thickness(0);

                    btnModalFilterTabSelected.Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#2E2E2E");
                    btnModalFilterTabSelected.Foreground = currentThemeIsLight ? HexBrush("#1B1B1B") : HexBrush("#CCCCCC");
                    btnModalFilterTabSelected.BorderThickness = new Thickness(1);
                    btnModalFilterTabSelected.BorderBrush = currentThemeIsLight ? HexBrush("#D1D5DB") : HexBrush("#3D3D3D");
                }
            }
        }

        private void CloseGroupModal()
        {
            groupModalOverlay.Visibility = Visibility.Collapsed;
            modalAppRows.Clear();
            modalAppsContainer.Children.Clear();
            modalFilteredApps.Clear();
            modalUnselectedApps.Clear();
            modalSelectedAppIds.Clear();
            modalShowSelectedOnly = false;
        }

        private void PopulateModalAppChecklist(List<string> selectedAppIds)
        {
            modalShowSelectedOnly = false;
            modalSelectedAppIds = new HashSet<string>(selectedAppIds ?? new List<string>());
            txtModalAppSearch.Text = "Filter software...";
            txtModalAppSearch.Foreground = currentThemeIsLight ? HexBrush("#94A3B8") : HexBrush("#707070");
            UpdateModalTabStyles();
            FilterModalApps();
        }

        private void FilterModalApps()
        {
            string q = txtModalAppSearch.Text != null ? txtModalAppSearch.Text.Trim() : "";
            if (q.Equals("filter software...", StringComparison.OrdinalIgnoreCase)) q = "";

            Dictionary<string, AppModel> appDict = new Dictionary<string, AppModel>(StringComparer.OrdinalIgnoreCase);
            foreach (AppModel a in config.apps)
            {
                if (a != null && a.id != null && !appDict.ContainsKey(a.id))
                {
                    appDict[a.id] = a;
                }
            }

            List<AppModel> selectedAppsList = new List<AppModel>();
            List<AppModel> unselectedAppsList = new List<AppModel>();

            if (string.IsNullOrEmpty(q))
            {
                HashSet<string> addedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // 1. Gather all selected apps
                foreach (string id in modalSelectedAppIds)
                {
                    AppModel foundApp;
                    if (appDict.TryGetValue(id, out foundApp))
                    {
                        selectedAppsList.Add(foundApp);
                        addedIds.Add(foundApp.id);
                    }
                }

                // 2. Gather remaining catalog apps
                if (!modalShowSelectedOnly)
                {
                    foreach (AppModel a in config.apps)
                    {
                        if (a != null && a.id != null && !addedIds.Contains(a.id))
                        {
                            unselectedAppsList.Add(a);
                        }
                    }
                }
            }
            else
            {
                // Filter by query
                foreach (AppModel a in config.apps)
                {
                    if (a == null) continue;
                    bool match = (a.name != null && a.name.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                 (a.id != null && a.id.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                 (a.description != null && a.description.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                 (a.category != null && a.category.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0);

                    if (match)
                    {
                        if (modalSelectedAppIds.Contains(a.id))
                        {
                            selectedAppsList.Add(a);
                        }
                        else if (!modalShowSelectedOnly)
                        {
                            unselectedAppsList.Add(a);
                        }
                    }
                }
            }

            // SORT BOTH LISTS ALPHABETICALLY BY NAME (A TO Z)
            selectedAppsList.Sort(delegate (AppModel x, AppModel y)
            {
                return string.Compare(x.name ?? "", y.name ?? "", StringComparison.OrdinalIgnoreCase);
            });

            unselectedAppsList.Sort(delegate (AppModel x, AppModel y)
            {
                return string.Compare(x.name ?? "", y.name ?? "", StringComparison.OrdinalIgnoreCase);
            });

            modalFilteredApps = new List<AppModel>();
            modalFilteredApps.AddRange(selectedAppsList);
            modalFilteredApps.AddRange(unselectedAppsList);

            modalAppRows.Clear();
            modalAppsContainer.Children.Clear();
            if (modalAppsScrollViewer != null) modalAppsScrollViewer.ScrollToTop();

            RenderModalChecklist(selectedAppsList, unselectedAppsList);
            UpdateModalCount();
        }

        private void RenderModalChecklist(List<AppModel> selectedApps, List<AppModel> unselectedApps)
        {
            // Section 1: Selected Software (Always placed on top!)
            if (selectedApps.Count > 0)
            {
                Border selHeader = CreateModalSectionHeader(
                    string.Format("⭐ SELECTED SOFTWARE IN THIS ROLE ({0})", selectedApps.Count),
                    "These packages are currently assigned and will be installed for this role",
                    true);
                modalAppsContainer.Children.Add(selHeader);

                foreach (AppModel a in selectedApps)
                {
                    Border card = CreateModalAppCard(a);
                    modalAppsContainer.Children.Add(card);
                }
            }
            else
            {
                Border emptyBox = new Border()
                {
                    Background = currentThemeIsLight ? HexBrush("#F8FAFC") : HexBrush("#181818"),
                    BorderBrush = currentThemeIsLight ? HexBrush("#E2E8F0") : HexBrush("#2D2D2D"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(12, 9, 12, 9),
                    Margin = new Thickness(0, 0, 0, 8)
                };
                TextBlock emptyText = new TextBlock()
                {
                    Text = "ℹ️ No software packages selected yet. Check packages from the catalog below to add them to this role.",
                    FontSize = 11,
                    Foreground = currentThemeIsLight ? HexBrush("#64748B") : HexBrush("#8A8A8A")
                };
                emptyBox.Child = emptyText;
                modalAppsContainer.Children.Add(emptyBox);
            }

            // Section 2: Catalog Software (Unselected)
            if (!modalShowSelectedOnly && unselectedApps.Count > 0)
            {
                Border catHeader = CreateModalSectionHeader(
                    string.Format("📦 AVAILABLE SOFTWARE CATALOG ({0:N0} packages)", unselectedApps.Count),
                    "Click any software package to add it to this role",
                    false);
                modalAppsContainer.Children.Add(catHeader);

                modalUnselectedApps = unselectedApps;
                modalUnselectedRenderedCount = 0;
                RenderNextModalCatalogBatch(50);
            }
        }

        private Border CreateModalSectionHeader(string title, string subtitle, bool isSelectedSection)
        {
            Border b = new Border()
            {
                Background = isSelectedSection ?
                    (currentThemeIsLight ? HexBrush("#E0F2FE") : HexBrush("#0C2A40")) :
                    (currentThemeIsLight ? HexBrush("#F1F5F9") : HexBrush("#202020")),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10, 6, 10, 6),
                Margin = new Thickness(0, isSelectedSection ? 0 : 10, 0, 6),
                BorderThickness = new Thickness(1),
                BorderBrush = isSelectedSection ?
                    (currentThemeIsLight ? HexBrush("#BAE6FD") : HexBrush("#1E4976")) :
                    (currentThemeIsLight ? HexBrush("#CBD5E1") : HexBrush("#333333"))
            };

            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            StackPanel sp = new StackPanel();
            TextBlock txtTitle = new TextBlock()
            {
                Text = title,
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = isSelectedSection ?
                    (currentThemeIsLight ? HexBrush("#0369A1") : HexBrush("#38BDF8")) :
                    (currentThemeIsLight ? HexBrush("#334155") : HexBrush("#D1D5DB"))
            };
            sp.Children.Add(txtTitle);

            if (!string.IsNullOrEmpty(subtitle))
            {
                TextBlock txtSub = new TextBlock()
                {
                    Text = subtitle,
                    FontSize = 10,
                    Foreground = isSelectedSection ?
                        (currentThemeIsLight ? HexBrush("#0284C7") : HexBrush("#7DD3FC")) :
                        (currentThemeIsLight ? HexBrush("#64748B") : HexBrush("#8A8A8A")),
                    Margin = new Thickness(0, 1, 0, 0)
                };
                sp.Children.Add(txtSub);
            }
            Grid.SetColumn(sp, 0);
            g.Children.Add(sp);

            Border pill = new Border()
            {
                Background = isSelectedSection ? HexBrush("#0078D4") :
                    (currentThemeIsLight ? HexBrush("#E2E8F0") : HexBrush("#2B2B2B")),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(7, 2, 7, 2),
                VerticalAlignment = VerticalAlignment.Center
            };
            TextBlock pillText = new TextBlock()
            {
                Text = isSelectedSection ? "ACTIVE IN ROLE" : "CATALOG",
                FontSize = 9,
                FontWeight = FontWeights.Bold,
                Foreground = isSelectedSection ? HexBrush("#FFFFFF") :
                    (currentThemeIsLight ? HexBrush("#475569") : HexBrush("#A0A0A0"))
            };
            pill.Child = pillText;
            Grid.SetColumn(pill, 1);
            g.Children.Add(pill);

            b.Child = g;
            return b;
        }

        private void RenderNextModalCatalogBatch(int count)
        {
            if (modalUnselectedApps == null) return;
            int toRender = Math.Min(count, modalUnselectedApps.Count - modalUnselectedRenderedCount);
            if (toRender <= 0)
            {
                if (btnModalLoadMore != null) btnModalLoadMore.Visibility = Visibility.Collapsed;
                return;
            }

            if (btnModalLoadMore != null && modalAppsContainer.Children.Contains(btnModalLoadMore))
            {
                modalAppsContainer.Children.Remove(btnModalLoadMore);
            }

            for (int i = 0; i < toRender; i++)
            {
                AppModel curApp = modalUnselectedApps[modalUnselectedRenderedCount++];
                Border card = CreateModalAppCard(curApp);
                modalAppsContainer.Children.Add(card);
            }

            int remaining = modalUnselectedApps.Count - modalUnselectedRenderedCount;
            if (remaining > 0)
            {
                if (btnModalLoadMore == null)
                {
                    btnModalLoadMore = new Button()
                    {
                        Margin = new Thickness(0, 8, 0, 4),
                        Padding = new Thickness(14, 6, 14, 6),
                        Background = currentThemeIsLight ? HexBrush("#F1F5F9") : HexBrush("#252525"),
                        Foreground = currentThemeIsLight ? HexBrush("#0284C7") : HexBrush("#38BDF8"),
                        BorderBrush = currentThemeIsLight ? HexBrush("#CBD5E1") : HexBrush("#3A3A3A"),
                        BorderThickness = new Thickness(1),
                        Cursor = Cursors.Hand,
                        FontWeight = FontWeights.SemiBold,
                        FontSize = 11,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    btnModalLoadMore.Click += delegate
                    {
                        RenderNextModalCatalogBatch(50);
                    };
                }
                else
                {
                    btnModalLoadMore.Background = currentThemeIsLight ? HexBrush("#F1F5F9") : HexBrush("#252525");
                    btnModalLoadMore.Foreground = currentThemeIsLight ? HexBrush("#0284C7") : HexBrush("#38BDF8");
                    btnModalLoadMore.BorderBrush = currentThemeIsLight ? HexBrush("#CBD5E1") : HexBrush("#3A3A3A");
                }

                btnModalLoadMore.Content = string.Format("⬇ Load More Catalog Software ({0:N0} remaining)", remaining);
                btnModalLoadMore.Visibility = Visibility.Visible;
                modalAppsContainer.Children.Add(btnModalLoadMore);
            }
            else
            {
                if (btnModalLoadMore != null) btnModalLoadMore.Visibility = Visibility.Collapsed;
            }
        }

        private Border CreateModalAppCard(AppModel curApp)
        {
            bool isChecked = modalSelectedAppIds.Contains(curApp.id);
            Border b = new Border()
            {
                Background = isChecked ?
                    (currentThemeIsLight ? HexBrush("#F0F7FF") : HexBrush("#1C2E42")) :
                    (currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#272727")),
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(0, 0, 0, 4),
                Padding = new Thickness(10, 7, 10, 7),
                BorderThickness = new Thickness(1),
                BorderBrush = isChecked ?
                    (currentThemeIsLight ? HexBrush("#0067C0") : HexBrush("#4CC2FF")) :
                    (currentThemeIsLight ? HexBrush("#E5E5E5") : HexBrush("#383838")),
                Cursor = Cursors.Hand
            };

            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            CheckBox cb = new CheckBox()
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0),
                Cursor = Cursors.Hand,
                IsChecked = isChecked
            };
            Grid.SetColumn(cb, 0);
            g.Children.Add(cb);

            StackPanel sp = new StackPanel();
            StackPanel titleRow = new StackPanel() { Orientation = Orientation.Horizontal };
            TextBlock nameText = new TextBlock()
            {
                Text = curApp.name,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = currentThemeIsLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF")
            };
            titleRow.Children.Add(nameText);

            Border catPill = new Border()
            {
                Background = currentThemeIsLight ? HexBrush("#EBF3FB") : HexBrush("#1E293B"),
                CornerRadius = new CornerRadius(4),
                Margin = new Thickness(8, 0, 0, 0),
                Padding = new Thickness(5, 1, 5, 1)
            };
            TextBlock catText = new TextBlock()
            {
                Text = (curApp.category ?? "TOOLS").ToUpper(),
                FontSize = 9,
                FontWeight = FontWeights.Bold,
                Foreground = currentThemeIsLight ? HexBrush("#005FB8") : HexBrush("#38BDF8")
            };
            catPill.Child = catText;
            titleRow.Children.Add(catPill);
            sp.Children.Add(titleRow);

            TextBlock descText = new TextBlock()
            {
                Text = curApp.description,
                FontSize = 10,
                Foreground = currentThemeIsLight ? HexBrush("#5D5D5D") : HexBrush("#A0A0A0"),
                Margin = new Thickness(0, 2, 0, 0),
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            sp.Children.Add(descText);

            Grid.SetColumn(sp, 1);
            g.Children.Add(sp);

            Border statusBadge = new Border()
            {
                Background = isChecked ?
                    (currentThemeIsLight ? HexBrush("#E0F2FE") : HexBrush("#0C4A6E")) :
                    (currentThemeIsLight ? HexBrush("#F1F5F9") : HexBrush("#222222")),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(8, 3, 8, 3),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(8, 0, 0, 0)
            };
            TextBlock statusBadgeText = new TextBlock()
            {
                Text = isChecked ? "✓ SELECTED" : "+ ADD",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = isChecked ?
                    (currentThemeIsLight ? HexBrush("#0284C7") : HexBrush("#38BDF8")) :
                    (currentThemeIsLight ? HexBrush("#64748B") : HexBrush("#777777"))
            };
            statusBadge.Child = statusBadgeText;
            Grid.SetColumn(statusBadge, 2);
            g.Children.Add(statusBadge);

            b.Child = g;

            string appId = curApp.id;
            Border curBorder = b;
            Border curBadge = statusBadge;
            TextBlock curBadgeText = statusBadgeText;

            Action updateVisualState = new Action(delegate
            {
                bool active = cb.IsChecked == true;
                curBorder.Background = active ?
                    (currentThemeIsLight ? HexBrush("#F0F9FF") : HexBrush("#132537")) :
                    (currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#181818"));
                curBorder.BorderBrush = active ?
                    (currentThemeIsLight ? HexBrush("#0284C7") : HexBrush("#38BDF8")) :
                    (currentThemeIsLight ? HexBrush("#E2E8F0") : HexBrush("#2B2B2B"));
                curBadge.Background = active ?
                    (currentThemeIsLight ? HexBrush("#E0F2FE") : HexBrush("#0C4A6E")) :
                    (currentThemeIsLight ? HexBrush("#F1F5F9") : HexBrush("#222222"));
                curBadgeText.Text = active ? "✓ SELECTED" : "+ ADD";
                curBadgeText.Foreground = active ?
                    (currentThemeIsLight ? HexBrush("#0284C7") : HexBrush("#38BDF8")) :
                    (currentThemeIsLight ? HexBrush("#64748B") : HexBrush("#777777"));
            });

            cb.Checked += delegate
            {
                modalSelectedAppIds.Add(appId);
                updateVisualState();
                UpdateModalCount();
            };
            cb.Unchecked += delegate
            {
                modalSelectedAppIds.Remove(appId);
                updateVisualState();
                UpdateModalCount();
            };

            b.MouseDown += delegate (object sender, MouseButtonEventArgs e)
            {
                if (e.OriginalSource != cb && !(e.OriginalSource is System.Windows.Shapes.Path))
                {
                    cb.IsChecked = !cb.IsChecked;
                }
            };

            modalAppRows.Add(new ModalAppItemView()
            {
                App = curApp,
                Container = b,
                CheckBox = cb,
                StatusBadge = statusBadge,
                StatusBadgeText = statusBadgeText
            });

            return b;
        }

        private void UpdateModalCount()
        {
            if (txtModalSelectedCount != null)
            {
                txtModalSelectedCount.Text = modalSelectedAppIds.Count + " selected";
            }
            if (btnModalFilterTabSelected != null)
            {
                btnModalFilterTabSelected.Content = string.Format("Selected ({0})", modalSelectedAppIds.Count);
            }
        }

        private void SaveGroupModal()
        {
            string name = txtGroupNameInput.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter a group name.", "Missing Name", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string icon = txtGroupIconInput.Text.Trim();
            if (string.IsNullOrEmpty(icon)) icon = "🚀";

            string desc = txtGroupDescInput.Text.Trim();

            List<string> selectedIds = new List<string>(modalSelectedAppIds);

            if (currentEditingGroupId == null)
            {
                // Create New Group
                string newId = Regex.Replace(name.ToLower(), @"[^a-z0-9]", "_") + "_" + (DateTime.Now.Ticks % 1000);
                GroupModel newGrp = new GroupModel()
                {
                    id = newId,
                    name = name,
                    icon = icon,
                    description = desc,
                    accentColor = "#0078D4",
                    apps = selectedIds
                };
                config.groups.Add(newGrp);
                currentEditingGroupId = newId;
            }
            else
            {
                // Update Existing Group
                GroupModel existing = config.groups.Find(delegate (GroupModel g) { return g.id == currentEditingGroupId; });
                if (existing != null)
                {
                    existing.name = name;
                    existing.icon = icon;
                    existing.description = desc;
                    existing.apps = selectedIds;
                }
            }

            SaveConfigToFile();
            CloseGroupModal();

            BuildRoleCards();
            BuildCatalog();
            SelectRole(currentEditingGroupId);

            if (txtStatus != null) txtStatus.Text = string.Format("Group '{0}' saved with {1} packages.", name, selectedIds.Count);
        }

        private void DeleteGroupModal()
        {
            if (currentEditingGroupId == null) return;
            if (config.groups.Count <= 1)
            {
                MessageBox.Show("You cannot delete the only remaining role group. At least one group is required.", "Cannot Delete", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            GroupModel target = config.groups.Find(delegate (GroupModel g) { return g.id == currentEditingGroupId; });
            if (target == null) return;

            MessageBoxResult res = MessageBox.Show(
                string.Format("Are you sure you want to delete the role group '{0}'?", target.name),
                "Confirm Deletion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (res != MessageBoxResult.Yes) return;

            config.groups.Remove(target);
            SaveConfigToFile();
            CloseGroupModal();

            BuildRoleCards();
            BuildCatalog();
            if (config.groups.Count > 0) SelectRole(config.groups[0].id);

            if (txtStatus != null) txtStatus.Text = string.Format("Group '{0}' deleted.", target.name);
        }

        // =========================================================================
        // APP PACK IMPORT & EXPORT
        // =========================================================================
        private string PrettyPrintJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return json;
            StringBuilder sb = new StringBuilder();
            bool inQuote = false;
            int indent = 0;
            for (int i = 0; i < json.Length; i++)
            {
                char ch = json[i];
                if (ch == '\"')
                {
                    bool isEscaped = false;
                    int k = i - 1;
                    while (k >= 0 && json[k] == '\\') { isEscaped = !isEscaped; k--; }
                    if (!isEscaped) inQuote = !inQuote;
                    sb.Append(ch);
                }
                else if (inQuote)
                {
                    sb.Append(ch);
                }
                else
                {
                    if (ch == '{' || ch == '[')
                    {
                        sb.Append(ch);
                        sb.AppendLine();
                        indent++;
                        sb.Append(new string(' ', indent * 2));
                    }
                    else if (ch == '}' || ch == ']')
                    {
                        sb.AppendLine();
                        indent--;
                        if (indent < 0) indent = 0;
                        sb.Append(new string(' ', indent * 2));
                        sb.Append(ch);
                    }
                    else if (ch == ',')
                    {
                        sb.Append(ch);
                        sb.AppendLine();
                        sb.Append(new string(' ', indent * 2));
                    }
                    else if (ch == ':')
                    {
                        sb.Append(": ");
                    }
                    else if (!char.IsWhiteSpace(ch))
                    {
                        sb.Append(ch);
                    }
                }
            }
            return sb.ToString();
        }

        private void ExportGroup(GroupModel grp)
        {
            if (grp == null) return;
            try
            {
                Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
                string safeName = Regex.Replace(grp.name, @"[^\w\s-]", "").Trim().Replace(" ", "_");
                if (string.IsNullOrEmpty(safeName)) safeName = "app_pack";
                sfd.FileName = safeName + "-pack.json";
                sfd.Filter = "FleetForge App Pack (*.json)|*.json|All Files (*.*)|*.*";
                sfd.Title = "Export App Pack: " + grp.name;

                bool? result = sfd.ShowDialog();
                if (result == true)
                {
                    string rawJson = jsonSerializer.Serialize(grp);
                    string prettyJson = PrettyPrintJson(rawJson);
                    File.WriteAllText(sfd.FileName, prettyJson, Encoding.UTF8);

                    MessageBox.Show(
                        string.Format("App Pack '{0}' successfully exported to:\n\n{1}", grp.name, sfd.FileName),
                        "Pack Exported Successfully",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    if (txtStatus != null) txtStatus.Text = string.Format("Exported pack '{0}'.", grp.name);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to export app pack: " + ex.Message, "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportGroup()
        {
            try
            {
                Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
                ofd.Filter = "FleetForge App Pack (*.json)|*.json|All Files (*.*)|*.*";
                ofd.Title = "Import App Pack";

                bool? result = ofd.ShowDialog();
                if (result != true) return;

                if (!File.Exists(ofd.FileName)) return;

                string content = File.ReadAllText(ofd.FileName, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(content))
                {
                    MessageBox.Show("The selected file is empty.", "Import Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                GroupModel importedGroup = null;
                try
                {
                    importedGroup = jsonSerializer.Deserialize<GroupModel>(content);
                }
                catch { }

                if (importedGroup == null || string.IsNullOrWhiteSpace(importedGroup.name) || importedGroup.apps == null || importedGroup.apps.Count == 0)
                {
                    try
                    {
                        List<GroupModel> groupList = jsonSerializer.Deserialize<List<GroupModel>>(content);
                        if (groupList != null && groupList.Count > 0 && !string.IsNullOrWhiteSpace(groupList[0].name))
                        {
                            importedGroup = groupList[0];
                        }
                    }
                    catch { }
                }

                if (importedGroup == null || string.IsNullOrWhiteSpace(importedGroup.name) || importedGroup.apps == null || importedGroup.apps.Count == 0)
                {
                    try
                    {
                        ConfigModel cfg = jsonSerializer.Deserialize<ConfigModel>(content);
                        if (cfg != null && cfg.groups != null && cfg.groups.Count > 0 && !string.IsNullOrWhiteSpace(cfg.groups[0].name))
                        {
                            importedGroup = cfg.groups[0];
                        }
                    }
                    catch { }
                }

                if (importedGroup == null || string.IsNullOrWhiteSpace(importedGroup.name))
                {
                    MessageBox.Show("Invalid or unsupported app pack file format. Please choose a valid FleetForge pack .json file.", "Invalid Format", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (importedGroup.apps == null) importedGroup.apps = new List<string>();
                if (string.IsNullOrEmpty(importedGroup.icon)) importedGroup.icon = "📦";
                if (string.IsNullOrEmpty(importedGroup.accentColor)) importedGroup.accentColor = "#0078D4";
                if (string.IsNullOrEmpty(importedGroup.description)) importedGroup.description = "";

                // Check if pack with same id or same name exists (case-insensitive)
                GroupModel existing = config.groups.Find(delegate(GroupModel g) {
                    if (g == null) return false;
                    bool idMatch = !string.IsNullOrEmpty(importedGroup.id) &&
                                   !string.IsNullOrEmpty(g.id) &&
                                   string.Equals(g.id.Trim(), importedGroup.id.Trim(), StringComparison.OrdinalIgnoreCase);
                    bool nameMatch = !string.IsNullOrEmpty(importedGroup.name) &&
                                     !string.IsNullOrEmpty(g.name) &&
                                     string.Equals(g.name.Trim(), importedGroup.name.Trim(), StringComparison.OrdinalIgnoreCase);
                    return idMatch || nameMatch;
                });

                if (existing != null)
                {
                    string message = string.Format(
                        "An app pack with the name '{0}' already exists in FleetForge.\n\n" +
                        "Do you want to REPLACE the existing pack with the imported version?\n\n" +
                        "• Click YES to REPLACE the existing pack.\n" +
                        "• Click NO to IGNORE and keep your current pack.",
                        existing.name);

                    MessageBoxResult answer = MessageBox.Show(
                        message,
                        "Pack Already Exists - Replace or Ignore?",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (answer != MessageBoxResult.Yes)
                    {
                        if (txtStatus != null) txtStatus.Text = string.Format("Import of pack '{0}' was ignored.", importedGroup.name);
                        return; // Ignore - do not import
                    }

                    // User selected Yes -> Replace existing pack
                    existing.name = importedGroup.name;
                    existing.icon = !string.IsNullOrEmpty(importedGroup.icon) ? importedGroup.icon : existing.icon;
                    existing.description = importedGroup.description ?? "";
                    if (!string.IsNullOrEmpty(importedGroup.accentColor)) existing.accentColor = importedGroup.accentColor;
                    existing.apps = importedGroup.apps ?? new List<string>();
                    importedGroup = existing;
                }
                else
                {
                    // New Pack
                    if (string.IsNullOrEmpty(importedGroup.id) || config.groups.Exists(delegate(GroupModel g) { return g.id == importedGroup.id; }))
                    {
                        importedGroup.id = Regex.Replace(importedGroup.name.ToLower(), @"[^a-z0-9]", "_") + "_" + (DateTime.Now.Ticks % 10000);
                    }
                    config.groups.Add(importedGroup);
                }

                SaveConfigToFile();
                BuildRoleCards();
                BuildCatalog();
                SelectRole(importedGroup.id);

                MessageBox.Show(
                    string.Format("App Pack '{0}' with {1} apps has been successfully imported!", importedGroup.name, importedGroup.apps.Count),
                    "Import Successful",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                if (txtStatus != null) txtStatus.Text = string.Format("Imported pack '{0}'.", importedGroup.name);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error importing app pack: " + ex.Message, "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // =========================================================================
        // MODERN CATEGORY TABS & CATALOG BUILDER
        // =========================================================================
        private Button CreateCategoryTabButton(string id, string icon, string title, int count, bool isActive)
        {
            Button btn = new Button()
            {
                Height = 36,
                Padding = new Thickness(12, 0, 12, 0),
                Margin = new Thickness(0, 0, 6, 6),
                Cursor = Cursors.Hand,
                BorderThickness = new Thickness(1),
                Tag = id
            };

            StackPanel sp = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };

            if (!string.IsNullOrEmpty(icon))
            {
                TextBlock txtIcon = new TextBlock()
                {
                    Text = icon,
                    FontFamily = new FontFamily("Segoe UI Emoji, Segoe UI Symbol, Segoe UI"),
                    FontSize = 13.5,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 7, 0)
                };
                sp.Children.Add(txtIcon);
            }

            TextBlock txtTitle = new TextBlock()
            {
                Text = title,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center
            };
            sp.Children.Add(txtTitle);

            Border badge = new Border()
            {
                CornerRadius = new CornerRadius(9),
                Padding = new Thickness(7, 1.5, 7, 1.5),
                Margin = new Thickness(8, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            TextBlock txtBadge = new TextBlock()
            {
                Text = count.ToString("N0"),
                FontSize = 10.5,
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center
            };
            badge.Child = txtBadge;
            sp.Children.Add(badge);

            btn.Content = sp;
            btn.Click += delegate { SetCategoryFilter(id); };

            UpdateCategoryTabVisual(btn, isActive);
            return btn;
        }

        private void UpdateCategoryTabVisual(Button btn, bool isActive)
        {
            if (btn == null) return;
            string tag = (string)btn.Tag;
            bool isOnline = tag == "winget_online";

            StackPanel sp = btn.Content as StackPanel;
            TextBlock txtIcon = null;
            TextBlock txtTitle = null;
            Border badge = null;
            TextBlock txtBadge = null;

            if (sp != null)
            {
                int idx = 0;
                if (sp.Children.Count >= 3)
                {
                    txtIcon = sp.Children[idx++] as TextBlock;
                }
                if (idx < sp.Children.Count) txtTitle = sp.Children[idx++] as TextBlock;
                if (idx < sp.Children.Count)
                {
                    badge = sp.Children[idx++] as Border;
                    if (badge != null) txtBadge = badge.Child as TextBlock;
                }
            }

            if (isActive)
            {
                btn.Background = isOnline ? HexBrush("#7C3AED") : windowsAccentBrush;
                btn.BorderBrush = isOnline ? HexBrush("#A855F7") : windowsAccentBorderBrush;
                btn.BorderThickness = new Thickness(1);
                if (txtIcon != null) txtIcon.Foreground = HexBrush("#FFFFFF");
                if (txtTitle != null) txtTitle.Foreground = HexBrush("#FFFFFF");
                if (badge != null) badge.Background = HexBrush("#35FFFFFF");
                if (txtBadge != null) txtBadge.Foreground = HexBrush("#FFFFFF");
            }
            else
            {
                if (isOnline)
                {
                    btn.Background = currentThemeIsLight ? HexBrush("#F3E8FF") : HexBrush("#2D1B4E");
                    btn.BorderBrush = HexBrush("#8B5CF6");
                    btn.BorderThickness = new Thickness(1);
                    if (txtIcon != null) txtIcon.Foreground = currentThemeIsLight ? HexBrush("#7C3AED") : HexBrush("#C084FC");
                    if (txtTitle != null) txtTitle.Foreground = currentThemeIsLight ? HexBrush("#7C3AED") : HexBrush("#C084FC");
                    if (badge != null) badge.Background = currentThemeIsLight ? HexBrush("#EDE9FE") : HexBrush("#3B2166");
                    if (txtBadge != null) txtBadge.Foreground = currentThemeIsLight ? HexBrush("#7C3AED") : HexBrush("#E9D5FF");
                }
                else
                {
                    btn.Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#232A36");
                    btn.BorderBrush = currentThemeIsLight ? HexBrush("#CBD5E1") : HexBrush("#333C4D");
                    btn.BorderThickness = new Thickness(1);
                    if (txtIcon != null) txtIcon.Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
                    if (txtTitle != null) txtTitle.Foreground = currentThemeIsLight ? HexBrush("#334155") : HexBrush("#CBD5E1");
                    if (badge != null) badge.Background = currentThemeIsLight ? HexBrush("#E2E8F0") : HexBrush("#181F2A");
                    if (txtBadge != null) txtBadge.Foreground = currentThemeIsLight ? HexBrush("#64748B") : HexBrush("#94A3B8");
                }
            }
        }

        private void BuildCatalog()
        {
            appsContainer.Children.Clear();
            appRows.Clear();
            categoryChipsPanel.Children.Clear();

            Button allTab = CreateCategoryTabButton("all", "🌐", "All", config.apps.Count, activeCategory == "all" || string.IsNullOrEmpty(activeCategory));
            categoryChipsPanel.Children.Add(allTab);

            foreach (CategoryModel cat in config.categories)
            {
                CategoryModel curCat = cat;
                int count = config.apps.FindAll(delegate (AppModel a) { return a.category == curCat.id; }).Count;
                Button cTab = CreateCategoryTabButton(curCat.id, GetSafeCategoryIcon(curCat), curCat.name, count, activeCategory == curCat.id);
                categoryChipsPanel.Children.Add(cTab);
            }

            localAppsPanel = new StackPanel();

            // Dynamic Prompt Banner to search Winget Online when typing in search box
            bannerSearchWingetPrompt = new Border()
            {
                Background = currentThemeIsLight ? HexBrush("#F5F3FF") : HexBrush("#1E1B4B"),
                BorderBrush = HexBrush("#8B5CF6"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(14, 10, 14, 10),
                Margin = new Thickness(0, 0, 0, 10),
                Visibility = Visibility.Collapsed,
                Cursor = Cursors.Hand
            };
            txtSearchWingetPromptText = new TextBlock()
            {
                Text = "🔍 Query live Winget online repository →",
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = HexBrush("#8B5CF6")
            };
            bannerSearchWingetPrompt.Child = txtSearchWingetPromptText;
            bannerSearchWingetPrompt.MouseLeftButtonUp += delegate
            {
                string q = txtSearch.Text.Trim();
                if (q.Equals("find an app, setting or pack...", StringComparison.OrdinalIgnoreCase) || q.Equals("search applications...", StringComparison.OrdinalIgnoreCase)) q = "";
                SetCategoryFilter("winget_online");
                if (!string.IsNullOrEmpty(q))
                {
                    if (txtOnlineSearchInput != null) txtOnlineSearchInput.Text = q;
                    RunWingetOnlineSearch(q);
                }
            };
            localAppsPanel.Children.Add(bannerSearchWingetPrompt);

            localCardsContainer = new StackPanel();
            localAppsPanel.Children.Add(localCardsContainer);

            catalogPaginationPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 14, 0, 14),
                Visibility = Visibility.Collapsed
            };

            btnLoadMore = new Button()
            {
                Content = "\u26A1 Load More (+250)",
                Background = currentThemeIsLight ? HexBrush("#F1F5F9") : HexBrush("#202530"),
                Foreground = currentThemeIsLight ? HexBrush("#0067C0") : HexBrush("#38BDF8"),
                BorderThickness = new Thickness(1),
                BorderBrush = currentThemeIsLight ? HexBrush("#CBD5E1") : HexBrush("#2F3746"),
                Padding = new Thickness(18, 10, 18, 10),
                Margin = new Thickness(0, 0, 10, 0),
                Cursor = Cursors.Hand,
                FontWeight = FontWeights.SemiBold,
                FontSize = 12
            };
            btnLoadMore.Click += delegate
            {
                RenderNextBatch(250);
            };
            catalogPaginationPanel.Children.Add(btnLoadMore);

            btnLoadAll = new Button()
            {
                Content = "\uD83D\uDE80 Show All Apps",
                Background = currentThemeIsLight ? HexBrush("#0067C0") : HexBrush("#0078D4"),
                Foreground = HexBrush("#FFFFFF"),
                BorderThickness = new Thickness(0),
                Padding = new Thickness(20, 10, 20, 10),
                Cursor = Cursors.Hand,
                FontWeight = FontWeights.Bold,
                FontSize = 12
            };
            btnLoadAll.Click += delegate
            {
                ToggleStreamAllApps();
            };
            catalogPaginationPanel.Children.Add(btnLoadAll);

            localAppsPanel.Children.Add(catalogPaginationPanel);

            BuildWingetOnlinePanel();

            appsContainer.Children.Add(localAppsPanel);
            appsContainer.Children.Add(wingetOnlinePanel);

            ApplyFilters();
        }

        private void ToggleStreamAllApps()
        {
            if (activeFilteredApps == null) return;
            if (isStreamingAllApps)
            {
                isStreamingAllApps = false;
                if (btnLoadAll != null) btnLoadAll.Content = string.Format("\uD83D\uDE80 Show All Apps ({0:N0} Total)", activeFilteredApps.Count);
                return;
            }

            isStreamingAllApps = true;
            if (btnLoadAll != null)
            {
                btnLoadAll.Content = string.Format("\u23F9 Stop Loading ({0:N0} of {1:N0})...", currentRenderedCount, activeFilteredApps.Count);
            }

            StreamAllAppsStep();
        }

        private void StreamAllAppsStep()
        {
            if (!isStreamingAllApps || activeFilteredApps == null || currentRenderedCount >= activeFilteredApps.Count)
            {
                isStreamingAllApps = false;
                if (btnLoadAll != null)
                {
                    btnLoadAll.Content = string.Format("\u2713 All {0:N0} Apps Loaded", activeFilteredApps != null ? activeFilteredApps.Count : 0);
                    btnLoadAll.IsEnabled = false;
                }
                if (btnLoadMore != null) btnLoadMore.Visibility = Visibility.Collapsed;
                return;
            }

            RenderNextBatch(300);

            if (btnLoadAll != null && isStreamingAllApps)
            {
                btnLoadAll.Content = string.Format("\u23F9 Loading... ({0:N0} of {1:N0})", currentRenderedCount, activeFilteredApps.Count);
            }

            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(delegate
            {
                StreamAllAppsStep();
            }));
        }

        private void RenderNextBatch(int count)
        {
            if (activeFilteredApps == null) return;
            int toRender = Math.Min(count, activeFilteredApps.Count - currentRenderedCount);
            if (toRender <= 0)
            {
                if (catalogPaginationPanel != null) catalogPaginationPanel.Visibility = Visibility.Collapsed;
                return;
            }

            for (int i = 0; i < toRender; i++)
            {
                AppModel curApp = activeFilteredApps[currentRenderedCount++];
                Border card = CreateAppCard(curApp);
                localCardsContainer.Children.Add(card);
            }

            UpdateHeaderCount();

            if (catalogPaginationPanel != null)
            {
                if (currentRenderedCount < activeFilteredApps.Count)
                {
                    catalogPaginationPanel.Visibility = Visibility.Visible;
                    if (btnLoadMore != null)
                    {
                        btnLoadMore.Visibility = Visibility.Visible;
                        btnLoadMore.Content = string.Format("\u26A1 Load More (+250) \u2014 Showing {0:N0} of {1:N0}", currentRenderedCount, activeFilteredApps.Count);
                    }
                    if (btnLoadAll != null && !isStreamingAllApps)
                    {
                        btnLoadAll.Visibility = Visibility.Visible;
                        btnLoadAll.IsEnabled = true;
                        btnLoadAll.Content = string.Format("\uD83D\uDE80 Show All Apps ({0:N0} Total)", activeFilteredApps.Count);
                    }
                }
                else
                {
                    if (btnLoadMore != null) btnLoadMore.Visibility = Visibility.Collapsed;
                    if (btnLoadAll != null)
                    {
                        btnLoadAll.Content = string.Format("\u2713 All {0:N0} Apps Loaded", activeFilteredApps.Count);
                        btnLoadAll.IsEnabled = false;
                    }
                }
            }
        }

        private Border CreateAppCard(AppModel curApp)
        {
            Border card = new Border()
            {
                Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#272727"),
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 0, 0, 6),
                Padding = new Thickness(14, 10, 14, 10),
                BorderThickness = new Thickness(1),
                BorderBrush = currentThemeIsLight ? HexBrush("#E5E5E5") : HexBrush("#383838"),
                Cursor = Cursors.Hand,
                Tag = curApp
            };

            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            CheckBox cb = new CheckBox()
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 14, 0),
                Cursor = Cursors.Hand,
                IsChecked = selectedAppIds.Contains(curApp.id)
            };

            Action updateCardVisual = delegate
            {
                bool isSel = cb.IsChecked == true;
                if (isSel)
                {
                    card.Background = currentThemeIsLight ? HexBrush("#F0F7FF") : HexBrush("#1C2E42");
                    card.BorderBrush = currentThemeIsLight ? HexBrush("#0067C0") : HexBrush("#4CC2FF");
                }
                else
                {
                    card.Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#272727");
                    card.BorderBrush = currentThemeIsLight ? HexBrush("#E5E5E5") : HexBrush("#383838");
                }
            };

            cb.Checked += delegate
            {
                updateCardVisual();
                if (isUpdatingSelection) return;
                selectedAppIds.Add(curApp.id);
                UpdateCount();
                UpdateMasterCheckboxState();
                UpdateHeaderCount();
            };
            cb.Unchecked += delegate
            {
                updateCardVisual();
                if (isUpdatingSelection) return;
                selectedAppIds.Remove(curApp.id);
                UpdateCount();
                UpdateMasterCheckboxState();
                UpdateHeaderCount();
            };
            updateCardVisual();

            card.MouseLeftButtonUp += delegate (object sender, MouseButtonEventArgs e)
            {
                if (!e.Handled && !(e.OriginalSource is CheckBox))
                {
                    cb.IsChecked = !cb.IsChecked;
                }
            };

            Grid.SetColumn(cb, 0);
            g.Children.Add(cb);

            StackPanel infoStack = new StackPanel();
            StackPanel titleStack = new StackPanel() { Orientation = Orientation.Horizontal };
            TextBlock txtName = new TextBlock()
            {
                Text = curApp.name,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = currentThemeIsLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF")
            };
            titleStack.Children.Add(txtName);

            Border badge = new Border()
            {
                Background = currentThemeIsLight ? HexBrush("#EBF3FB") : HexBrush("#1E293B"),
                CornerRadius = new CornerRadius(4),
                Margin = new Thickness(8, 0, 0, 0),
                Padding = new Thickness(6, 1, 6, 1)
            };
            TextBlock txtBadge = new TextBlock()
            {
                Text = (curApp.category ?? "UTILITIES").ToUpper(),
                FontSize = 9,
                FontWeight = FontWeights.Bold,
                Foreground = currentThemeIsLight ? HexBrush("#005FB8") : HexBrush("#38BDF8")
            };
            badge.Child = txtBadge;
            titleStack.Children.Add(badge);
            infoStack.Children.Add(titleStack);

            TextBlock txtDesc = new TextBlock()
            {
                Text = curApp.description ?? ("Winget package (" + curApp.id + ")"),
                FontSize = 11,
                Foreground = currentThemeIsLight ? HexBrush("#5D5D5D") : HexBrush("#A0A0A0"),
                Margin = new Thickness(0, 3, 0, 0)
            };
            infoStack.Children.Add(txtDesc);
            Grid.SetColumn(infoStack, 1);
            g.Children.Add(infoStack);

            StackPanel statusStack = new StackPanel() { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            TextBlock statusIcon = new TextBlock() { Text = "⚪", FontSize = 11, Margin = new Thickness(0, 0, 6, 0) };
            TextBlock statusText = new TextBlock()
            {
                Text = "Ready",
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = currentThemeIsLight ? HexBrush("#5D5D5D") : HexBrush("#A0A0A0")
            };
            statusStack.Children.Add(statusIcon);
            statusStack.Children.Add(statusText);
            Grid.SetColumn(statusStack, 2);
            g.Children.Add(statusStack);

            card.Child = g;

            appRows.Add(new AppRowView()
            {
                App = curApp,
                Card = card,
                CheckBox = cb,
                StatusIcon = statusIcon,
                StatusText = statusText
            });

            return card;
        }

        private void BuildWingetOnlinePanel()
        {
            wingetOnlinePanel = new StackPanel() { Visibility = Visibility.Collapsed };

            // 1. Header Banner
            Border banner = new Border()
            {
                Background = currentThemeIsLight ? HexBrush("#F5F3FF") : HexBrush("#1E1B4B"),
                BorderBrush = HexBrush("#8B5CF6"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 14)
            };

            StackPanel bannerStack = new StackPanel();
            TextBlock txtBannerTitle = new TextBlock()
            {
                Text = "🌐 Search Millions of Windows Apps Online",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = currentThemeIsLight ? HexBrush("#5B21B6") : HexBrush("#DDD6FE")
            };
            bannerStack.Children.Add(txtBannerTitle);

            TextBlock txtBannerDesc = new TextBlock()
            {
                Text = "Can't find what you need in the list? Search millions of software packages online and install them with one simple click.",
                FontSize = 11,
                Foreground = currentThemeIsLight ? HexBrush("#6D28D9") : HexBrush("#A78BFA"),
                Margin = new Thickness(0, 4, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
            bannerStack.Children.Add(txtBannerDesc);
            banner.Child = bannerStack;
            wingetOnlinePanel.Children.Add(banner);

            // 2. Search Input Row
            Grid searchGrid = new Grid() { Margin = new Thickness(0, 0, 0, 10) };
            searchGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            searchGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            txtOnlineSearchInput = new TextBox()
            {
                Height = 38,
                Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#161920"),
                Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#FFFFFF"),
                CaretBrush = HexBrush("#7C3AED"),
                BorderBrush = currentThemeIsLight ? HexBrush("#CBD5E1") : HexBrush("#382956"),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(12, 8, 12, 8),
                FontSize = 12,
                VerticalContentAlignment = VerticalAlignment.Center,
                Text = "Enter app name (e.g. Chrome, WhatsApp, Spotify, Zoom, Steam)..."
            };

            txtOnlineSearchInput.GotFocus += delegate
            {
                if (txtOnlineSearchInput.Text == "Enter app name (e.g. Chrome, WhatsApp, Spotify, Zoom, Steam)...")
                {
                    txtOnlineSearchInput.Text = "";
                    txtOnlineSearchInput.Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#FFFFFF");
                }
            };
            txtOnlineSearchInput.LostFocus += delegate
            {
                if (string.IsNullOrWhiteSpace(txtOnlineSearchInput.Text))
                {
                    txtOnlineSearchInput.Text = "Enter app name (e.g. Chrome, WhatsApp, Spotify, Zoom, Steam)...";
                    txtOnlineSearchInput.Foreground = currentThemeIsLight ? HexBrush("#94A3B8") : HexBrush("#707070");
                }
            };
            txtOnlineSearchInput.KeyDown += delegate (object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Enter)
                {
                    RunWingetOnlineSearch(txtOnlineSearchInput.Text.Trim());
                }
            };
            Grid.SetColumn(txtOnlineSearchInput, 0);
            searchGrid.Children.Add(txtOnlineSearchInput);

            btnExecuteOnlineSearch = new Button()
            {
                Content = "Search Online",
                Background = HexBrush("#7C3AED"),
                Foreground = HexBrush("#FFFFFF"),
                Padding = new Thickness(16, 0, 16, 0),
                Margin = new Thickness(8, 0, 0, 0),
                Cursor = Cursors.Hand,
                FontWeight = FontWeights.SemiBold,
                BorderThickness = new Thickness(0),
                FontSize = 12
            };
            btnExecuteOnlineSearch.Click += delegate
            {
                RunWingetOnlineSearch(txtOnlineSearchInput.Text.Trim());
            };
            Grid.SetColumn(btnExecuteOnlineSearch, 1);
            searchGrid.Children.Add(btnExecuteOnlineSearch);

            wingetOnlinePanel.Children.Add(searchGrid);

            // 3. Quick Tag Chips
            WrapPanel tagsPanel = new WrapPanel() { Margin = new Thickness(0, 0, 0, 14) };
            string[] quickTags = new string[] {
                "Steam", "Wireshark", "Spotify", "Audacity", "Ghidra", "Bitwarden",
                "Rust", "Podman", "SumatraPDF", "WireGuard", "Nginx", "KeePass"
            };
            foreach (string tag in quickTags)
            {
                string curTag = tag;
                Button tagBtn = new Button()
                {
                    Content = curTag,
                    Padding = new Thickness(10, 4, 10, 4),
                    Margin = new Thickness(0, 0, 6, 6),
                    Background = currentThemeIsLight ? HexBrush("#F1F5F9") : HexBrush("#241D35"),
                    Foreground = currentThemeIsLight ? HexBrush("#475569") : HexBrush("#C4B5FD"),
                    BorderThickness = new Thickness(1),
                    BorderBrush = currentThemeIsLight ? HexBrush("#CBD5E1") : HexBrush("#3B2E58"),
                    Cursor = Cursors.Hand,
                    FontSize = 11
                };
                tagBtn.Click += delegate
                {
                    txtOnlineSearchInput.Text = curTag;
                    RunWingetOnlineSearch(curTag);
                };
                tagsPanel.Children.Add(tagBtn);
            }
            wingetOnlinePanel.Children.Add(tagsPanel);

            // 4. Status Text
            txtOnlineStatus = new TextBlock()
            {
                Text = "Type any software name above and press Enter to search the entire Winget directory.",
                FontSize = 12,
                Foreground = currentThemeIsLight ? HexBrush("#64748B") : HexBrush("#94A3B8"),
                Margin = new Thickness(2, 0, 0, 10)
            };
            wingetOnlinePanel.Children.Add(txtOnlineStatus);

            // 5. Results Container
            onlineResultsPanel = new StackPanel();
            wingetOnlinePanel.Children.Add(onlineResultsPanel);
        }

        private void RunWingetOnlineSearch(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query == "Enter app name (e.g. Chrome, WhatsApp, Spotify, Zoom, Steam)...")
            {
                txtOnlineStatus.Text = "Please enter an app name (e.g. Chrome, WhatsApp, Spotify, Zoom).";
                return;
            }

            if (string.IsNullOrEmpty(wingetCmd))
            {
                txtOnlineStatus.Text = "Windows App Store is not available on this system.";
                return;
            }

            txtOnlineStatus.Text = string.Format("⏳ Searching online for '{0}'... Please wait.", query);
            onlineResultsPanel.Children.Clear();
            btnExecuteOnlineSearch.IsEnabled = false;

            ThreadPool.QueueUserWorkItem(delegate
            {
                List<AppModel> foundApps = new List<AppModel>();
                string errorMsg = null;
                try
                {
                    string args = string.Format("search -q \"{0}\" -n 50 --accept-source-agreements", query);
                    ProcessStartInfo psi = new ProcessStartInfo(wingetCmd, args);
                    psi.CreateNoWindow = true;
                    psi.UseShellExecute = false;
                    psi.RedirectStandardOutput = true;
                    psi.RedirectStandardError = true;

                    List<string> lines = new List<string>();
                    using (Process proc = Process.Start(psi))
                    {
                        while (!proc.StandardOutput.EndOfStream)
                        {
                            lines.Add(proc.StandardOutput.ReadLine());
                        }
                        proc.WaitForExit();
                    }

                    int hdrIndex = -1;
                    for (int i = 0; i < lines.Count; i++)
                    {
                        if (lines[i] != null && System.Text.RegularExpressions.Regex.IsMatch(lines[i], @"^Name\s+Id"))
                        {
                            hdrIndex = i;
                            break;
                        }
                    }

                    if (hdrIndex >= 0 && lines.Count > hdrIndex + 2)
                    {
                        string hdr = lines[hdrIndex];
                        int idPos = hdr.IndexOf("Id");
                        int verPos = hdr.IndexOf("Version");
                        int matchPos = hdr.IndexOf("Match");
                        int srcPos = hdr.IndexOf("Source");

                        for (int i = hdrIndex + 2; i < lines.Count; i++)
                        {
                            string line = lines[i];
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            string name = (idPos > 0 && line.Length >= idPos) ? line.Substring(0, idPos).Trim() : line.Trim();
                            string id = "";
                            if (idPos > 0 && line.Length > idPos)
                            {
                                int end = (verPos > idPos) ? Math.Min(line.Length - idPos, verPos - idPos) : line.Length - idPos;
                                id = line.Substring(idPos, end).Trim();
                            }

                            string ver = "";
                            if (verPos > 0 && line.Length > verPos)
                            {
                                int limit = (matchPos > verPos) ? matchPos : ((srcPos > verPos) ? srcPos : line.Length);
                                int end = Math.Min(line.Length - verPos, limit - verPos);
                                ver = line.Substring(verPos, end).Trim();
                            }

                            if (!string.IsNullOrEmpty(id))
                            {
                                foundApps.Add(new AppModel()
                                {
                                    id = id,
                                    name = string.IsNullOrEmpty(name) ? id : name,
                                    category = "utilities",
                                    description = string.IsNullOrEmpty(ver) ? "Winget package (" + id + ")" : "Version: " + ver + " • Winget ID: " + id
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    errorMsg = ex.Message;
                }

                Dispatcher.Invoke(new Action(delegate
                {
                    btnExecuteOnlineSearch.IsEnabled = true;

                    if (errorMsg != null)
                    {
                        txtOnlineStatus.Text = "Search error: " + errorMsg;
                        return;
                    }

                    if (foundApps.Count == 0)
                    {
                        txtOnlineStatus.Text = string.Format("No apps found matching '{0}'. Try searching with another name.", query);
                        return;
                    }

                    txtOnlineStatus.Text = string.Format("Found {0} app(s) online matching '{1}':", foundApps.Count, query);

                    foreach (AppModel app in foundApps)
                    {
                        AppModel curApp = app;
                        Border card = CreateWingetAppCard(curApp);
                        onlineResultsPanel.Children.Add(card);
                    }
                }));
            });
        }

        private Border CreateWingetAppCard(AppModel curApp)
        {
            Border card = new Border()
            {
                Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#272727"),
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 0, 0, 6),
                Padding = new Thickness(14, 10, 14, 10),
                BorderThickness = new Thickness(1),
                BorderBrush = currentThemeIsLight ? HexBrush("#E5E5E5") : HexBrush("#383838"),
                Cursor = Cursors.Hand
            };

            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            CheckBox cb = new CheckBox()
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 14, 0),
                Cursor = Cursors.Hand
            };

            Action updateCardVisual = delegate
            {
                bool isSel = cb.IsChecked == true;
                if (isSel)
                {
                    card.Background = currentThemeIsLight ? HexBrush("#F0F7FF") : HexBrush("#1C2E42");
                    card.BorderBrush = currentThemeIsLight ? HexBrush("#0067C0") : HexBrush("#4CC2FF");
                }
                else
                {
                    card.Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#272727");
                    card.BorderBrush = currentThemeIsLight ? HexBrush("#E5E5E5") : HexBrush("#383838");
                }
            };

            AppRowView existingRow = appRows.Find(delegate (AppRowView r) { return r.App.id == curApp.id; });
            if (existingRow != null)
            {
                cb.IsChecked = existingRow.CheckBox.IsChecked;
            }

            cb.Checked += delegate
            {
                updateCardVisual();
                AppRowView row = appRows.Find(delegate (AppRowView r) { return r.App.id == curApp.id; });
                if (row == null)
                {
                    row = new AppRowView()
                    {
                        App = curApp,
                        Card = card,
                        CheckBox = cb,
                        StatusIcon = new TextBlock() { Text = "⚪" },
                        StatusText = new TextBlock() { Text = "Ready" }
                    };
                    appRows.Add(row);
                }
                else
                {
                    row.CheckBox = cb;
                }
                UpdateCount();
            };
            cb.Unchecked += delegate
            {
                updateCardVisual();
                UpdateCount();
            };
            updateCardVisual();

            card.MouseLeftButtonUp += delegate (object sender, MouseButtonEventArgs e)
            {
                if (!e.Handled && !(e.OriginalSource is CheckBox) && !(e.OriginalSource is Button))
                {
                    cb.IsChecked = !cb.IsChecked;
                }
            };

            Grid.SetColumn(cb, 0);
            g.Children.Add(cb);

            StackPanel infoStack = new StackPanel();
            StackPanel titleStack = new StackPanel() { Orientation = Orientation.Horizontal };
            TextBlock txtName = new TextBlock()
            {
                Text = curApp.name,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = currentThemeIsLight ? HexBrush("#1B1B1B") : HexBrush("#FFFFFF")
            };
            titleStack.Children.Add(txtName);

            Border badge = new Border()
            {
                Background = currentThemeIsLight ? HexBrush("#F3E8FF") : HexBrush("#3B0764"),
                CornerRadius = new CornerRadius(4),
                Margin = new Thickness(8, 0, 0, 0),
                Padding = new Thickness(6, 1, 6, 1)
            };
            TextBlock txtBadge = new TextBlock()
            {
                Text = "ONLINE",
                FontSize = 9,
                FontWeight = FontWeights.Bold,
                Foreground = currentThemeIsLight ? HexBrush("#7C3AED") : HexBrush("#C084FC")
            };
            badge.Child = txtBadge;
            titleStack.Children.Add(badge);
            infoStack.Children.Add(titleStack);

            TextBlock txtDesc = new TextBlock()
            {
                Text = curApp.description,
                FontSize = 11,
                Foreground = currentThemeIsLight ? HexBrush("#5D5D5D") : HexBrush("#A0A0A0"),
                Margin = new Thickness(0, 3, 0, 0)
            };
            infoStack.Children.Add(txtDesc);
            Grid.SetColumn(infoStack, 1);
            g.Children.Add(infoStack);

            StackPanel actionStack = new StackPanel() { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };

            bool alreadyInCatalog = config.apps.Exists(delegate (AppModel a) { return a.id == curApp.id; });
            Button btnAddCatalog = new Button()
            {
                Content = alreadyInCatalog ? "✓ In List" : "➕ Add to My List",
                IsEnabled = !alreadyInCatalog,
                Background = alreadyInCatalog ? (currentThemeIsLight ? HexBrush("#E2E8F0") : HexBrush("#242B38")) : HexBrush("#0078D4"),
                Foreground = HexBrush("#FFFFFF"),
                Padding = new Thickness(10, 5, 10, 5),
                Margin = new Thickness(0, 0, 6, 0),
                FontSize = 11,
                Cursor = alreadyInCatalog ? Cursors.Arrow : Cursors.Hand,
                BorderThickness = new Thickness(0)
            };
            btnAddCatalog.Click += delegate
            {
                if (!config.apps.Exists(delegate (AppModel a) { return a.id == curApp.id; }))
                {
                    config.apps.Add(curApp);
                    SaveConfigToFile();
                    btnAddCatalog.Content = "In List";
                    btnAddCatalog.IsEnabled = false;
                    btnAddCatalog.Background = HexBrush("#16A34A");
                    if (txtStatus != null) txtStatus.Text = string.Format("Added '{0}' to your app list.", curApp.name);
                }
            };
            actionStack.Children.Add(btnAddCatalog);

            Button btnInstallNow = new Button()
            {
                Content = "Install Now",
                Background = HexBrush("#059669"),
                Foreground = HexBrush("#FFFFFF"),
                Padding = new Thickness(10, 5, 10, 5),
                FontSize = 11,
                Cursor = Cursors.Hand,
                BorderThickness = new Thickness(0)
            };
            btnInstallNow.Click += delegate
            {
                InstallSingleApp(curApp);
            };
            actionStack.Children.Add(btnInstallNow);

            Grid.SetColumn(actionStack, 2);
            g.Children.Add(actionStack);

            card.Child = g;
            return card;
        }

        private void InstallSingleApp(AppModel curApp)
        {
            if (string.IsNullOrEmpty(wingetCmd))
            {
                MessageBox.Show("Winget is not available on this system.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBoxResult res = MessageBox.Show(
                string.Format("Do you want to install '{0}' now?", curApp.name),
                "Install App",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );
            if (res != MessageBoxResult.Yes) return;

            SwitchView("logs");
            AppendLog("==========================================================");
            AppendLog(string.Format("Single Package Silent Install: {0} ({1})", curApp.name, curApp.id));
            AppendLog("Timestamp: " + DateTime.Now.ToString());
            AppendLog("==========================================================");

            ThreadPool.QueueUserWorkItem(delegate
            {
                string args = string.Format("install --id \"{0}\" -e --silent --accept-package-agreements --accept-source-agreements --disable-interactivity", curApp.id);
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo(wingetCmd, args);
                    psi.CreateNoWindow = true;
                    psi.UseShellExecute = false;
                    psi.RedirectStandardOutput = true;
                    psi.RedirectStandardError = true;

                    using (Process proc = Process.Start(psi))
                    {
                        while (!proc.HasExited)
                        {
                            string line = proc.StandardOutput.ReadLine();
                            if (!string.IsNullOrEmpty(line)) AppendLog("  > " + line);
                        }
                        string rest = proc.StandardOutput.ReadToEnd();
                        if (!string.IsNullOrEmpty(rest))
                        {
                            foreach (string l in rest.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                AppendLog("  > " + l);
                            }
                        }
                        int code = proc.ExitCode;
                        AppendLog("Winget finished with ExitCode: " + code);
                        uint uCode = unchecked((uint)code);
                        if (code == 0 || code == 3010 || code == -1978335189 || uCode == 2316632107U)
                        {
                            AppendLog("SUCCESS: " + curApp.name + " installed successfully!");
                            Dispatcher.Invoke(new Action(delegate
                            {
                                MessageBox.Show(curApp.name + " installed successfully!", "Installation Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                            }));
                        }
                        else
                        {
                            AppendLog("ERROR: Installation exited with code: " + code);
                        }
                    }
                }
                catch (Exception ex)
                {
                    AppendLog("Install Exception: " + ex.Message);
                }
            });
        }

        private void SetCategoryFilter(string cat)
        {
            activeCategory = cat;
            foreach (UIElement elem in categoryChipsPanel.Children)
            {
                Button btn = elem as Button;
                if (btn != null)
                {
                    string tag = (string)btn.Tag;
                    UpdateCategoryTabVisual(btn, tag == cat);
                }
            }

            if (cat == "winget_online")
            {
                localAppsPanel.Visibility = Visibility.Collapsed;
                wingetOnlinePanel.Visibility = Visibility.Visible;
                if (borderCatalogHeaderBar != null) borderCatalogHeaderBar.Visibility = Visibility.Collapsed;
            }
            else
            {
                localAppsPanel.Visibility = Visibility.Visible;
                wingetOnlinePanel.Visibility = Visibility.Collapsed;
                if (borderCatalogHeaderBar != null) borderCatalogHeaderBar.Visibility = Visibility.Visible;
                ApplyFilters();
            }

            if (btnSearchWingetOnlineBottom != null)
            {
                if (cat == "winget_online")
                {
                    btnSearchWingetOnlineBottom.Content = "Back to App Catalog";
                    btnSearchWingetOnlineBottom.Background = currentThemeIsLight ? HexBrush("#0284C7") : HexBrush("#0369A1");
                }
                else
                {
                    btnSearchWingetOnlineBottom.Content = "Search Online Apps";
                    btnSearchWingetOnlineBottom.Background = currentThemeIsLight ? HexBrush("#7C3AED") : HexBrush("#6D28D9");
                }
            }
        }

        private void SetCatalogSort(string mode, string label)
        {
            catalogSortMode = mode;
            if (btnCatalogSort != null)
            {
                btnCatalogSort.Content = "↕ Sort: " + label + " ▾";
            }
            ApplyFilters();
        }

        private void EnsureAppDefaultIndices(bool forceRefresh = false)
        {
            if (forceRefresh || appDefaultIndices == null)
            {
                appDefaultIndices = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                if (config != null && config.apps != null)
                {
                    for (int i = 0; i < config.apps.Count; i++)
                    {
                        if (config.apps[i] != null && !string.IsNullOrEmpty(config.apps[i].id))
                        {
                            if (!appDefaultIndices.ContainsKey(config.apps[i].id))
                                appDefaultIndices[config.apps[i].id] = i;
                        }
                    }
                }
            }
        }

        private void SortFilteredApps()
        {
            if (activeFilteredApps == null || activeFilteredApps.Count <= 1) return;

            switch (catalogSortMode)
            {
                case "selected_top":
                    activeFilteredApps.Sort(delegate (AppModel a, AppModel b)
                    {
                        bool aSel = (a.id != null && selectedAppIds.Contains(a.id));
                        bool bSel = (b.id != null && selectedAppIds.Contains(b.id));
                        if (aSel != bSel) return aSel ? -1 : 1;
                        return string.Compare(a.name ?? "", b.name ?? "", StringComparison.OrdinalIgnoreCase);
                    });
                    break;

                case "alpha_asc":
                    activeFilteredApps.Sort(delegate (AppModel a, AppModel b)
                    {
                        return string.Compare(a.name ?? "", b.name ?? "", StringComparison.OrdinalIgnoreCase);
                    });
                    break;

                case "alpha_desc":
                    activeFilteredApps.Sort(delegate (AppModel a, AppModel b)
                    {
                        return string.Compare(b.name ?? "", a.name ?? "", StringComparison.OrdinalIgnoreCase);
                    });
                    break;

                case "category":
                    activeFilteredApps.Sort(delegate (AppModel a, AppModel b)
                    {
                        int catComp = string.Compare(a.category ?? "", b.category ?? "", StringComparison.OrdinalIgnoreCase);
                        if (catComp != 0) return catComp;
                        return string.Compare(a.name ?? "", b.name ?? "", StringComparison.OrdinalIgnoreCase);
                    });
                    break;

                case "popular":
                default:
                    EnsureAppDefaultIndices();
                    if (appDefaultIndices != null)
                    {
                        activeFilteredApps.Sort(delegate (AppModel a, AppModel b)
                        {
                            int idxA = (a.id != null && appDefaultIndices.ContainsKey(a.id)) ? appDefaultIndices[a.id] : int.MaxValue;
                            int idxB = (b.id != null && appDefaultIndices.ContainsKey(b.id)) ? appDefaultIndices[b.id] : int.MaxValue;
                            return idxA.CompareTo(idxB);
                        });
                    }
                    break;
            }
        }

        private void ApplyFilters()
        {
            if (activeCategory == "winget_online") return;

            string query = txtSearch.Text.Trim().ToLower();
            if (query == "find an app, setting or pack..." || query == "search applications...") query = "";

            if (!string.IsNullOrEmpty(query))
            {
                bannerSearchWingetPrompt.Visibility = Visibility.Visible;
                txtSearchWingetPromptText.Text = string.Format("🔍 Can't find '{0}'? Search millions of apps online →", txtSearch.Text.Trim());
            }
            else
            {
                bannerSearchWingetPrompt.Visibility = Visibility.Collapsed;
            }

            activeFilteredApps = config.apps.FindAll(delegate (AppModel a)
            {
                bool matchCat = (activeCategory == "all") || (a.category == activeCategory);
                if (!matchCat) return false;

                if (string.IsNullOrEmpty(query)) return true;

                return (a.name != null && a.name.ToLower().Contains(query)) ||
                       (a.id != null && a.id.ToLower().Contains(query)) ||
                       (a.description != null && a.description.ToLower().Contains(query));
            });

            SortFilteredApps();

            isStreamingAllApps = false;
            currentRenderedCount = 0;
            appRows.Clear();
            if (localCardsContainer != null) localCardsContainer.Children.Clear();

            UpdateMasterCheckboxState();
            UpdateHeaderCount();

            RenderNextBatch(250);
        }

        private void OnMasterCheckboxClicked()
        {
            if (isUpdatingSelection || isUpdatingMasterCheckbox || chkSelectAllCatalog == null) return;
            isUpdatingSelection = true;
            isUpdatingMasterCheckbox = true;

            try
            {
                bool targetState = (chkSelectAllCatalog.IsChecked == true);

                if (targetState)
                {
                    foreach (AppModel a in activeFilteredApps) selectedAppIds.Add(a.id);
                }
                else
                {
                    foreach (AppModel a in activeFilteredApps) selectedAppIds.Remove(a.id);
                }

                if (localCardsContainer != null)
                {
                    foreach (UIElement child in localCardsContainer.Children)
                    {
                        Border card = child as Border;
                        if (card != null)
                        {
                            Grid g = card.Child as Grid;
                            if (g != null && g.Children.Count > 0)
                            {
                                CheckBox cb = g.Children[0] as CheckBox;
                                if (cb != null) cb.IsChecked = targetState;
                            }
                        }
                    }
                }
            }
            finally
            {
                isUpdatingSelection = false;
                isUpdatingMasterCheckbox = false;
            }

            UpdateCount();
            UpdateHeaderCount();
        }

        private int GetSelectedInFilterCount()
        {
            if (activeFilteredApps == null || activeFilteredApps.Count == 0 || selectedAppIds == null || selectedAppIds.Count == 0)
                return 0;

            // Instant O(1) path: When showing full catalog (no search or category filter active), count is simply selectedAppIds.Count
            if (config != null && config.apps != null && activeFilteredApps.Count == config.apps.Count)
                return selectedAppIds.Count;

            int count = 0;
            if (selectedAppIds.Count <= 100)
            {
                // Fast path for role groups (typically 10-20 apps): check small selected set against active filter
                HashSet<string> filteredSet = new HashSet<string>();
                foreach (AppModel a in activeFilteredApps) filteredSet.Add(a.id);
                foreach (string id in selectedAppIds)
                {
                    if (filteredSet.Contains(id)) count++;
                }
            }
            else
            {
                foreach (AppModel a in activeFilteredApps)
                {
                    if (selectedAppIds.Contains(a.id)) count++;
                }
            }
            return count;
        }

        private void UpdateMasterCheckboxState()
        {
            if (chkSelectAllCatalog == null || isUpdatingMasterCheckbox) return;
            isUpdatingMasterCheckbox = true;

            try
            {
                if (activeFilteredApps == null || activeFilteredApps.Count == 0)
                {
                    chkSelectAllCatalog.IsChecked = false;
                    chkSelectAllCatalog.IsEnabled = false;
                    return;
                }

                chkSelectAllCatalog.IsEnabled = true;
                int selectedInFilter = GetSelectedInFilterCount();

                if (selectedInFilter == activeFilteredApps.Count)
                {
                    chkSelectAllCatalog.IsChecked = true;
                }
                else if (selectedInFilter == 0)
                {
                    chkSelectAllCatalog.IsChecked = false;
                }
                else
                {
                    chkSelectAllCatalog.IsChecked = null; // Indeterminate
                }
            }
            finally
            {
                isUpdatingMasterCheckbox = false;
            }
        }

        private void UpdateHeaderCount()
        {
            if (txtCatalogListHeaderCount != null && activeFilteredApps != null)
            {
                int selectedInFilter = GetSelectedInFilterCount();

                if (selectedInFilter > 0)
                {
                    txtCatalogListHeaderCount.Text = string.Format("Showing {0:N0} of {1:N0} apps ({2:N0} selected)", currentRenderedCount, activeFilteredApps.Count, selectedInFilter);
                }
                else
                {
                    txtCatalogListHeaderCount.Text = string.Format("Showing {0:N0} of {1:N0} apps", currentRenderedCount, activeFilteredApps.Count);
                }
            }
        }

        private void UpdateCount()
        {
            if (txtCount != null) txtCount.Text = string.Format("Selected: {0:N0} of {1:N0} apps", selectedAppIds.Count, config.apps.Count);
            if (btnInstall != null)
            {
                if (selectedAppIds.Count > 0)
                {
                    btnInstall.Content = string.Format("Install Selected Apps ({0})", selectedAppIds.Count);
                }
                else
                {
                    btnInstall.Content = "Install Selected Apps";
                }
                btnInstall.Background = windowsAccentBrush;
            }
            if (btnHeroInstall != null)
            {
                if (selectedAppIds.Count > 0)
                {
                    btnHeroInstall.Content = string.Format("Install Selected Apps ({0})", selectedAppIds.Count);
                }
                else
                {
                    btnHeroInstall.Content = "Install Selected Apps";
                }
                btnHeroInstall.Background = windowsAccentBrush;
            }
            if (txtHeroStatusSubtitle != null)
            {
                GroupModel act = config != null && config.groups != null ? config.groups.Find(delegate(GroupModel g) { return g.id == activeRoleId; }) : null;
                string packName = act != null ? act.name : "Custom Selection";
                txtHeroStatusSubtitle.Text = string.Format("{0} apps selected from \"{1}\". Click Install to proceed.", selectedAppIds.Count, packName);
            }
        }

        private void AppendLog(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string line = string.Format("[{0}] {1}\r\n", timestamp, message);

            Dispatcher.Invoke(new Action(delegate
            {
                txtLog.AppendText(line);
                logScrollViewer.ScrollToEnd();
            }));

            try
            {
                File.AppendAllText(logFilePath, string.Format("[{0}] {1}\r\n", timestamp, message));
            }
            catch { }
        }

        // =========================================================================
        // INSTALLATION ENGINE & PROCESS MANAGEMENT
        // =========================================================================
        private void KillProcessTree(Process p)
        {
            if (p == null) return;
            try
            {
                int pid = p.Id;
                ProcessStartInfo killPsi = new ProcessStartInfo("taskkill", string.Format("/PID {0} /T /F", pid));
                killPsi.CreateNoWindow = true;
                killPsi.UseShellExecute = false;
                using (Process kp = Process.Start(killPsi))
                {
                    kp.WaitForExit(3000);
                }
            }
            catch { }
            try
            {
                if (!p.HasExited) p.Kill();
            }
            catch { }
        }

        private void RequestStopInstallation()
        {
            isCancelled = true;
            btnStop.IsEnabled = false;
            btnStop.Content = "Stopping...";
            AppendLog("[CANCEL] Stop requested by user. Terminating active setup process...");

            ThreadPool.QueueUserWorkItem(delegate
            {
                KillProcessTree(currentProc);
            });
        }

        private void SaveSessionState()
        {
            try
            {
                if (currentSession != null && !string.IsNullOrEmpty(sessionStatePath))
                {
                    JavaScriptSerializer ser = new JavaScriptSerializer();
                    string json = ser.Serialize(currentSession);
                    File.WriteAllText(sessionStatePath, json, Encoding.UTF8);
                }
            }
            catch { }
        }

        private void CheckResumeSession()
        {
            try
            {
                if (File.Exists(sessionStatePath))
                {
                    string json = File.ReadAllText(sessionStatePath, Encoding.UTF8);
                    if (!string.IsNullOrEmpty(json))
                    {
                        JavaScriptSerializer ser = new JavaScriptSerializer();
                        ser.MaxJsonLength = int.MaxValue;
                        DeploymentSessionState savedSession = ser.Deserialize<DeploymentSessionState>(json);
                        if (savedSession != null && !savedSession.isCompleted && savedSession.records != null && savedSession.records.Count > 0)
                        {
                            int pending = 0;
                            foreach (AppDeploymentRecord r in savedSession.records)
                            {
                                if (r.status == "Pending" || r.status == "Running" || r.status == "Failed" || r.status == "Timeout")
                                {
                                    pending++;
                                }
                            }
                            if (pending > 0 && bannerResumeSetup != null)
                            {
                                currentSession = savedSession;
                                if (txtResumeBannerTitle != null)
                                {
                                    txtResumeBannerTitle.Text = string.Format("Interrupted Deployment Session Detected ({0} Pending)", pending);
                                }
                                if (txtResumeBannerDesc != null)
                                {
                                    txtResumeBannerDesc.Text = string.Format("Previous session for '{0}' has {1} installed and {2} pending software package(s). Resume where you left off?",
                                        string.IsNullOrEmpty(savedSession.roleName) ? (savedSession.roleId ?? "Workstation") : savedSession.roleName,
                                        savedSession.successCount,
                                        pending);
                                }
                                bannerResumeSetup.Visibility = Visibility.Visible;
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private void DismissResumeSession()
        {
            if (bannerResumeSetup != null) bannerResumeSetup.Visibility = Visibility.Collapsed;
            try
            {
                if (File.Exists(sessionStatePath))
                {
                    File.Delete(sessionStatePath);
                }
            }
            catch { }
        }

        private void ResumeInterruptedSession()
        {
            if (currentSession == null || currentSession.records == null) return;
            if (bannerResumeSetup != null) bannerResumeSetup.Visibility = Visibility.Collapsed;

            selectedAppIds.Clear();
            foreach (AppDeploymentRecord rec in currentSession.records)
            {
                if (rec.status == "Pending" || rec.status == "Running" || rec.status == "Failed" || rec.status == "Timeout")
                {
                    selectedAppIds.Add(rec.appId);
                }
            }
            UpdateCount();
            StartInstallation(true);
        }

        private void StartInstallation()
        {
            StartInstallation(false);
        }

        private void StartInstallation(bool isResume)
        {
            List<AppModel> selectedApps = new List<AppModel>();
            if (config != null && config.apps != null)
            {
                foreach (AppModel a in config.apps)
                {
                    if (selectedAppIds.Contains(a.id)) selectedApps.Add(a);
                }
            }

            if (selectedApps.Count == 0)
            {
                MessageBox.Show("Please select an app pack or check at least one app to install.", "No Apps Selected", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!isResume)
            {
                MessageBoxResult res = MessageBox.Show(
                    string.Format("Ready to install {0} selected app(s) on your computer.\n\nDo you want to start now?", selectedApps.Count),
                    "Install Apps",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (res != MessageBoxResult.Yes) return;
            }

            SwitchView("logs");

            isCancelled = false;
            currentProc = null;

            btnInstall.Visibility = Visibility.Collapsed;
            btnStop.Visibility = Visibility.Visible;
            btnStop.IsEnabled = true;
            btnStop.Content = "⏹️ Stop Install";
            UpdateBottomBarVisibility();
            if (btnViewReport != null) btnViewReport.Visibility = Visibility.Collapsed;

            btnCreateNewGroup.IsEnabled = false;
            if (btnImportPack != null) btnImportPack.IsEnabled = false;
            foreach (RoleCardView rc in roleCards)
            {
                rc.SelectButton.IsEnabled = false;
                rc.EditButton.IsEnabled = false;
                if (rc.ExportButton != null) rc.ExportButton.IsEnabled = false;
            }

            if (progressBar != null)
            {
                progressBar.Value = 0;
                progressBar.Maximum = selectedApps.Count;
            }

            // Initialize or resume session state
            if (!isResume || currentSession == null)
            {
                currentSession = new DeploymentSessionState();
                currentSession.roleId = activeRoleId ?? "custom";
                currentSession.roleName = (activeRoleId != null && config != null && config.groups != null) ?
                    (config.groups.Find(delegate(GroupModel g) { return g.id == activeRoleId; }) != null ?
                     config.groups.Find(delegate(GroupModel g) { return g.id == activeRoleId; }).name : activeRoleId) : "Custom Workstation";
                currentSession.totalCount = selectedApps.Count;
                foreach (AppModel a in selectedApps)
                {
                    AppDeploymentRecord rec = new AppDeploymentRecord();
                    rec.appId = a.id;
                    rec.appName = a.name;
                    rec.category = a.category;
                    rec.installMethod = !string.IsNullOrEmpty(a.offlineInstaller) ? "Offline Installer" : "Winget Silent";
                    rec.status = "Pending";
                    currentSession.records.Add(rec);
                }
            }
            else
            {
                currentSession.isCompleted = false;
            }
            SaveSessionState();

            ThreadPool.QueueUserWorkItem(delegate
            {
                AppendLog("==========================================================");
                AppendLog(string.Format("FleetForge Setup Started: {0} packages (Watchdog Timeout: {1}m)", selectedApps.Count, watchdogTimeoutMinutes));
                AppendLog("Timestamp: " + DateTime.Now.ToString());
                AppendLog("==========================================================");

                int success = 0;
                int failed = 0;

                for (int i = 0; i < selectedApps.Count; i++)
                {
                    if (isCancelled)
                    {
                        AppendLog("[STOPPED] Installation queue stopped by user.");
                        break;
                    }

                    AppModel app = selectedApps[i];
                    AppRowView item = appRows.Find(delegate(AppRowView r) { return r != null && r.App != null && r.App.id == app.id; });

                    Dispatcher.Invoke(new Action(delegate
                    {
                        if (item != null)
                        {
                            item.StatusIcon.Text = "⏳";
                            item.StatusText.Text = "Installing...";
                            item.StatusText.Foreground = HexBrush("#0078D4");
                        }
                        if (txtStatus != null) txtStatus.Text = string.Format("[{0}/{1}] Installing {2}...", i + 1, selectedApps.Count, app.name);
                    }));

                    AppendLog(string.Format("[{0}/{1}] Processing: {2} ({3})", i + 1, selectedApps.Count, app.name, app.id));

                    AppDeploymentRecord rec = currentSession.records.Find(delegate(AppDeploymentRecord r) { return r != null && r.appId == app.id; });
                    if (rec == null)
                    {
                        rec = new AppDeploymentRecord();
                        rec.appId = app.id;
                        rec.appName = app.name;
                        rec.category = app.category;
                        currentSession.records.Add(rec);
                    }
                    rec.status = "Installing";
                    rec.timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    DateTime startTime = DateTime.Now;
                    SaveSessionState();

                    bool installed = false;
                    string offlineDir = Path.Combine(appDir, "offline_installers");

                    if (!string.IsNullOrEmpty(app.offlineInstaller) && Directory.Exists(offlineDir))
                    {
                        string offlineFile = Path.Combine(offlineDir, app.offlineInstaller);
                        if (File.Exists(offlineFile))
                        {
                            AppendLog("Found offline setup: " + offlineFile);
                            try
                            {
                                ProcessStartInfo psi = new ProcessStartInfo(offlineFile, app.silentArgs ?? "");
                                psi.CreateNoWindow = true;
                                psi.UseShellExecute = false;
                                using (Process p = Process.Start(psi))
                                {
                                    currentProc = p;
                                    int timeoutMs = watchdogTimeoutMinutes * 60 * 1000;
                                    DateTime startProc = DateTime.Now;
                                    bool timedOut = false;
                                    while (!p.HasExited)
                                    {
                                        if (isCancelled) { try { p.Kill(); } catch { } break; }
                                        if ((DateTime.Now - startProc).TotalMilliseconds > timeoutMs)
                                        {
                                            timedOut = true;
                                            try { p.Kill(); } catch { }
                                            AppendLog(string.Format("[WATCHDOG TIMEOUT] Offline setup for {0} exceeded {1} minute limit. Terminated.", app.name, watchdogTimeoutMinutes));
                                            break;
                                        }
                                        Thread.Sleep(500);
                                    }

                                    rec.durationSeconds = (DateTime.Now - startTime).TotalSeconds;
                                    rec.installMethod = "Offline Installer";

                                    if (timedOut)
                                    {
                                        rec.status = "Timeout";
                                        rec.exitCode = -1;
                                        rec.errorDetails = "Installer exceeded watchdog timeout limit";
                                        installed = false;
                                    }
                                    else if (!isCancelled && p.HasExited)
                                    {
                                        rec.exitCode = p.ExitCode;
                                        if (p.ExitCode == 0 || p.ExitCode == 3010)
                                        {
                                            rec.status = "Installed";
                                            installed = true;
                                            AppendLog("Offline setup completed successfully.");
                                        }
                                        else
                                        {
                                            rec.status = "Failed";
                                            rec.errorDetails = "Exit code: " + p.ExitCode;
                                            installed = false;
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                rec.status = "Failed";
                                rec.errorDetails = ex.Message;
                                AppendLog("Offline setup error: " + ex.Message);
                            }
                            finally
                            {
                                currentProc = null;
                            }
                        }
                    }

                    if (!installed && !isCancelled && rec.status != "Timeout")
                    {
                        if (string.IsNullOrEmpty(wingetCmd))
                        {
                            rec.status = "Failed";
                            rec.errorDetails = "Winget not available";
                            AppendLog("ERROR: Winget not available.");
                            installed = false;
                        }
                        else
                        {
                            AppendLog("Running winget install for " + app.id);
                            string args = string.Format("install --id \"{0}\" -e --silent --accept-package-agreements --accept-source-agreements --disable-interactivity", app.id);
                            try
                            {
                                ProcessStartInfo psi = new ProcessStartInfo(wingetCmd, args);
                                psi.CreateNoWindow = true;
                                psi.UseShellExecute = false;
                                psi.RedirectStandardOutput = true;
                                psi.RedirectStandardError = true;

                                using (Process proc = Process.Start(psi))
                                {
                                    currentProc = proc;
                                    int timeoutMs = watchdogTimeoutMinutes * 60 * 1000;
                                    DateTime startProc = DateTime.Now;
                                    bool timedOut = false;
                                    while (!proc.HasExited && !isCancelled)
                                    {
                                        if ((DateTime.Now - startProc).TotalMilliseconds > timeoutMs)
                                        {
                                            timedOut = true;
                                            try { proc.Kill(); } catch { }
                                            AppendLog(string.Format("[WATCHDOG TIMEOUT] Winget install for {0} exceeded {1} minute limit. Terminated.", app.name, watchdogTimeoutMinutes));
                                            break;
                                        }

                                        string line = proc.StandardOutput.ReadLine();
                                        if (!string.IsNullOrEmpty(line)) AppendLog("  > " + line);
                                    }

                                    rec.durationSeconds = (DateTime.Now - startTime).TotalSeconds;
                                    rec.installMethod = "Winget Silent";

                                    if (timedOut)
                                    {
                                        rec.status = "Timeout";
                                        rec.exitCode = -1;
                                        rec.errorDetails = "Installation exceeded watchdog timeout";
                                        installed = false;
                                    }
                                    else if (!isCancelled && proc.HasExited)
                                    {
                                        string rest = proc.StandardOutput.ReadToEnd();
                                        if (!string.IsNullOrEmpty(rest))
                                        {
                                            foreach (string l in rest.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                                            {
                                                AppendLog("  > " + l);
                                            }
                                        }
                                        int code = proc.ExitCode;
                                        rec.exitCode = code;
                                        AppendLog("Winget finished with ExitCode: " + code);

                                        uint uCode = unchecked((uint)code);
                                        if (code == 0 || code == 3010 || code == -1978335189 || uCode == 2316632107U)
                                        {
                                            rec.status = "Installed";
                                            installed = true;
                                        }
                                        else
                                        {
                                            rec.status = "Failed";
                                            string err = proc.StandardError.ReadToEnd();
                                            if (!string.IsNullOrEmpty(err))
                                            {
                                                AppendLog("  [STDERR] " + err);
                                                rec.errorDetails = err.Trim();
                                            }
                                            installed = false;
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                rec.status = "Failed";
                                rec.errorDetails = ex.Message;
                                AppendLog("Winget exception: " + ex.Message);
                                installed = false;
                            }
                            finally
                            {
                                currentProc = null;
                            }
                        }
                    }

                    if (installed)
                    {
                        success++;
                        Dispatcher.Invoke(new Action(delegate
                        {
                            if (item != null)
                            {
                                item.StatusIcon.Text = "✅";
                                item.StatusText.Text = "Installed";
                                item.StatusText.Foreground = HexBrush("#10B981");
                            }
                        }));
                    }
                    else
                    {
                        failed++;
                        Dispatcher.Invoke(new Action(delegate
                        {
                            if (item != null)
                            {
                                item.StatusIcon.Text = isCancelled ? "⏹️" : (rec.status == "Timeout" ? "⏱️" : "❌");
                                item.StatusText.Text = isCancelled ? "Cancelled" : (rec.status == "Timeout" ? "Timeout" : "Failed");
                                item.StatusText.Foreground = HexBrush(isCancelled ? "#F59E0B" : (rec.status == "Timeout" ? "#F59E0B" : "#F87171"));
                            }
                        }));
                    }

                    currentSession.successCount = success;
                    currentSession.failedCount = failed;
                    SaveSessionState();

                    Dispatcher.Invoke(new Action(delegate
                    {
                        if (progressBar != null) progressBar.Value = i + 1;
                    }));

                    if (isCancelled)
                    {
                        AppendLog("[STOPPED] Machine setup cancelled by user.");
                        break;
                    }
                }

                currentSession.endTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                currentSession.isCompleted = !isCancelled;
                if (!isCancelled)
                {
                    try { if (File.Exists(sessionStatePath)) File.Delete(sessionStatePath); } catch { }
                }
                else
                {
                    SaveSessionState();
                }

                // Generate standalone compliance audit reports (HTML & JSON)
                lastGeneratedReportPath = DeploymentReportGenerator.GenerateHtmlReport(currentSession, logFilePath);
                DeploymentReportGenerator.GenerateJsonReport(currentSession);

                AppendLog("==========================================================");
                AppendLog(string.Format("Setup Ended! Succeeded: {0}, Failed/Timeout: {1}", success, failed));
                AppendLog("Log file saved to: " + logFilePath);
                if (!string.IsNullOrEmpty(lastGeneratedReportPath))
                {
                    AppendLog("Compliance Audit Report: " + lastGeneratedReportPath);
                }
                AppendLog("==========================================================");

                Dispatcher.Invoke(new Action(delegate
                {
                    btnStop.Visibility = Visibility.Collapsed;
                    btnStop.IsEnabled = true;
                    btnStop.Content = "⏹️ Stop Install";
                    btnInstall.IsEnabled = true;
                    UpdateBottomBarVisibility();

                    if (btnViewReport != null && !string.IsNullOrEmpty(lastGeneratedReportPath))
                    {
                        btnViewReport.Visibility = Visibility.Visible;
                    }

                    btnCreateNewGroup.IsEnabled = true;
                    if (btnImportPack != null) btnImportPack.IsEnabled = true;
                    foreach (RoleCardView rc in roleCards)
                    {
                        rc.SelectButton.IsEnabled = true;
                        rc.EditButton.IsEnabled = true;
                        if (rc.ExportButton != null) rc.ExportButton.IsEnabled = true;
                    }

                    if (isCancelled)
                    {
                        int remaining = selectedApps.Count - success - failed;
                        if (txtStatus != null) txtStatus.Text = string.Format("Setup Stopped ({0} Installed, {1} Remaining)", success, remaining);
                        MessageBox.Show(
                            string.Format("Installation was paused.\n\nInstalled: {0}\nRemaining: {2}\n\nYour progress is saved! You can resume anytime.", success, failed, remaining),
                            "Installation Paused",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                    else
                    {
                        if (txtStatus != null) txtStatus.Text = string.Format("Setup Complete! ({0} Succeeded, {1} Failed)", success, failed);
                        MessageBoxResult reportChoice = MessageBox.Show(
                            string.Format("All Done! App Installation Finished.\n\nInstalled Successfully: {0}\nFailed / Skipped: {1}\n\nInstallation summary saved to:\n{2}\n\nWould you like to view the installation summary now?",
                                success, failed, lastGeneratedReportPath ?? logFilePath),
                            "Installation Finished — FleetForge",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Information);

                        if (reportChoice == MessageBoxResult.Yes && !string.IsNullOrEmpty(lastGeneratedReportPath) && File.Exists(lastGeneratedReportPath))
                        {
                            try { Process.Start(new ProcessStartInfo(lastGeneratedReportPath) { UseShellExecute = true }); } catch { }
                        }
                    }
                }));
            });
        }
    }
}
