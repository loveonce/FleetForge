using System;
using System.IO;
using System.Reflection;
using System.Web.Script.Serialization;

namespace FleetForge
{
    public class AppUrlsModel
    {
        public string cloudSyncUrl { get; set; }
        public string updateCheckUrl { get; set; }
        public string updateDownloadUrl { get; set; }
        public string companyWebsiteUrl { get; set; }
    }

    public static class AppUrls
    {
        // ========================================================================
        // FLEETFORGE CLOUD SYNC & VERSION UPDATE LINKS CONFIGURATION
        // ========================================================================
        // You can change the URLs directly in this file OR in 'app_urls.json' /
        // 'config/app_urls.json' before building the setup.

        // 1. Cloud Synchronization URL (Raw JSON catalog link)
        public static string CloudSyncUrl = "https://raw.githubusercontent.com/loveonce/FleetForge/main/data/apps.json";

        // 2. Version Check URL (version.json containing latest version & changelog)
        public static string UpdateCheckUrl = "https://raw.githubusercontent.com/loveonce/FleetForge/main/data/version.json";

        // 3. Setup Installer Download URL (Executable downloaded when updating)
        public static string UpdateDownloadUrl = "https://raw.githubusercontent.com/loveonce/FleetForge/main/FleetForge-Setup.exe";

        // 4. Company Official Website
        public static string CompanyWebsiteUrl = "https://www.innovanathinklabs.com";

        // ========================================================================
        // LOADER: Automatically loads and merges from app_urls.json if available
        // ========================================================================
        public static void Initialize(string baseDir = null)
        {
            string json = null;

            // 1. Check local file paths first (for developer overrides & portable mode)
            try
            {
                if (!string.IsNullOrEmpty(baseDir))
                {
                    string pRoot = Path.Combine(baseDir, "app_urls.json");
                    string pConfig = Path.Combine(baseDir, "config", "app_urls.json");
                    string pData = Path.Combine(baseDir, "data", "app_urls.json");

                    if (File.Exists(pRoot)) json = File.ReadAllText(pRoot, System.Text.Encoding.UTF8);
                    else if (File.Exists(pConfig)) json = File.ReadAllText(pConfig, System.Text.Encoding.UTF8);
                    else if (File.Exists(pData)) json = File.ReadAllText(pData, System.Text.Encoding.UTF8);
                }
            }
            catch { }

            // 2. If not found on disk, read embedded assembly resource
            if (string.IsNullOrEmpty(json))
            {
                try
                {
                    Assembly asm = Assembly.GetExecutingAssembly();
                    string[] resNames = asm.GetManifestResourceNames();
                    foreach (string name in resNames)
                    {
                        if (name.EndsWith("app_urls.json", StringComparison.OrdinalIgnoreCase))
                        {
                            using (Stream s = asm.GetManifestResourceStream(name))
                            {
                                if (s != null)
                                {
                                    using (StreamReader r = new StreamReader(s, System.Text.Encoding.UTF8))
                                    {
                                        json = r.ReadToEnd();
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
                catch { }
            }

            // 3. Parse JSON and apply overrides
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    JavaScriptSerializer ser = new JavaScriptSerializer();
                    AppUrlsModel model = ser.Deserialize<AppUrlsModel>(json);
                    if (model != null)
                    {
                        if (!string.IsNullOrEmpty(model.cloudSyncUrl)) CloudSyncUrl = model.cloudSyncUrl.Trim();
                        if (!string.IsNullOrEmpty(model.updateCheckUrl)) UpdateCheckUrl = model.updateCheckUrl.Trim();
                        if (!string.IsNullOrEmpty(model.updateDownloadUrl)) UpdateDownloadUrl = model.updateDownloadUrl.Trim();
                        if (!string.IsNullOrEmpty(model.companyWebsiteUrl)) CompanyWebsiteUrl = model.companyWebsiteUrl.Trim();
                    }
                }
                catch { }
            }
        }
    }
}

