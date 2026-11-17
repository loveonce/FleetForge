using System;
using System.Globalization;
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
using WinForms = System.Windows.Forms;
using WinDrawing = System.Drawing;

[assembly: AssemblyTitle("FleetForge")]
[assembly: AssemblyDescription("FleetForge - Zero-Touch Machine Setup & Enterprise Software Deployment")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Love Malhotra")]
[assembly: AssemblyProduct("FleetForge")]
[assembly: AssemblyCopyright("Copyright © 2026 Love Malhotra. All rights reserved.")]
[assembly: AssemblyTrademark("FleetForge™")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("2.7.0.0")]
[assembly: AssemblyFileVersion("2.7.0.0")]
[assembly: AssemblyInformationalVersion("2.7.0")]

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
        public string downloadUrl { get; set; }
        public string affiliateWebUrl { get; set; }
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

    public class RecommendedDataModel
    {
        public string version { get; set; }
        public GroupModel group { get; set; }
        public List<AppModel> apps { get; set; }

        public RecommendedDataModel()
        {
            group = new GroupModel();
            apps = new List<AppModel>();
        }
    }

    public class CustomCatalogModel
    {
        public string version { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public List<AppModel> apps { get; set; }

        public CustomCatalogModel()
        {
            version = "1.0";
            name = "Custom & Affiliate Apps Catalog";
            description = "User-defined software packages and customized direct download affiliate URLs.";
            apps = new List<AppModel>();
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

    public class LanguageDefinition
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string NativeName { get; set; }
        public string Flag { get; set; }

        public LanguageDefinition(string code, string name, string nativeName, string flag)
        {
            Code = code;
            Name = name;
            NativeName = nativeName;
            Flag = flag;
        }
    }

    public static class Localization
    {
        public static string CurrentLanguage = "en";

        public static readonly List<LanguageDefinition> SupportedLanguages = new List<LanguageDefinition>()
        {
            new LanguageDefinition("en", "English", "English", "\uD83C\uDDFA\uD83C\uDDF8"),
            new LanguageDefinition("hi", "Hindi", "\u0939\u093F\u0928\u094D\u0926\u0940", "\uD83C\uDDEE\uD83C\uDDF3"),
            new LanguageDefinition("es", "Spanish", "Espa\u00F1ol", "\uD83C\uDDEA\uD83C\uDDF8"),
            new LanguageDefinition("fr", "French", "Fran\u00E7ais", "\uD83C\uDDEB\uD83C\uDDF7"),
            new LanguageDefinition("de", "German", "Deutsch", "\uD83C\uDDE9\uD83C\uDDEA"),
            new LanguageDefinition("ru", "Russian", "\u0420\u0443\u0441\u0441\u043A\u0438\u0439", "\uD83C\uDDF7\uD83C\uDDFA"),
            new LanguageDefinition("zh", "Chinese", "\u7B80\u4F53\u4E2D\u6587", "\uD83C\uDDE8\uD83C\uDDF3"),
            new LanguageDefinition("ja", "Japanese", "\u65E5\u672C\u8A9E", "\uD83C\uDDEF\uD83C\uDDF5"),
            new LanguageDefinition("pt", "Portuguese", "Portugu\u00EAs", "\uD83C\uDDE7\uD83C\uDDF7"),
            new LanguageDefinition("ar", "Arabic", "\u0627\u0644\u0639\u0631\u0628\u064A\u0629", "\uD83C\uDDF8\uD83C\uDDE6"),
            new LanguageDefinition("it", "Italian", "Italiano", "\uD83C\uDDEE\uD83C\uDDF9"),
            new LanguageDefinition("ko", "Korean", "\uD55C\uAD6D\uC5B4", "\uD83C\uDDF0\uD83C\uDDF7"),
            new LanguageDefinition("tr", "Turkish", "T\u00FCrk\u00E7e", "\uD83C\uDDF9\uD83C\uDDF7"),
            new LanguageDefinition("nl", "Dutch", "Nederlands", "\uD83C\uDDF3\uD83C\uDDF1"),
            new LanguageDefinition("pl", "Polish", "Polski", "\uD83C\uDDF5\uD83C\uDDF1"),
            new LanguageDefinition("vi", "Vietnamese", "Ti\u1EBFng Vi\u1EC7t", "\uD83C\uDDFB\uD83C\uDDF3"),
        };

        private static Dictionary<string, Dictionary<string, string>> _translations = null;

        public static void InitializeTranslations()
        {
            if (_translations != null) return;
            _translations = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

            // Language: en
            Dictionary<string, string> dict_en = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            dict_en["btn_select_pack"] = "Select Pack";
            dict_en["btn_customize_pack"] = "Customize Apps";
            dict_en["btn_export_pack"] = "Export Pack";
            dict_en["nav_quick_setup"] = "Quick Setup";
            dict_en["nav_all_apps"] = "All Apps";
            dict_en["nav_cloud_sync"] = "Cloud Sync";
            dict_en["nav_history"] = "Install History";
            dict_en["search_placeholder"] = "Find an app, setting or pack...";
            dict_en["help_button"] = "Help ?";
            dict_en["lang_auto"] = "Auto (System Default)";
            dict_en["menu_guide"] = "How to Use (Interactive Guide)...";
            dict_en["menu_updates"] = "Check for Updates...";
            dict_en["menu_settings"] = "Settings...";
            dict_en["menu_about"] = "About FleetForge...";
            dict_en["quick_setup_title"] = "Quick Setup";
            dict_en["quick_setup_subtitle"] = "Choose a pack, pick apps and get started in one click.";
            dict_en["step1_title"] = "Pick a Pack";
            dict_en["step1_desc"] = "Choose your needs";
            dict_en["step2_title"] = "Review Apps";
            dict_en["step2_desc"] = "Check selection";
            dict_en["step3_title"] = "Install";
            dict_en["step3_desc"] = "Start using";
            dict_en["available_packs_title"] = "Available App Packs";
            dict_en["available_packs_subtitle"] = "Select a ready-made pack or choose individual apps.";
            dict_en["top_choice_badge"] = "TOP CHOICE";
            dict_en["recommended_badge"] = "RECOMMENDED";
            dict_en["custom_badge"] = "CUSTOM PACK";
            dict_en["partner_apps_count"] = "{0} partner apps";
            dict_en["apps_count"] = "{0} apps";
            dict_en["catalog_title"] = "All Applications";
            dict_en["catalog_subtitle"] = "Browse 16,349 software packages with instant search and one-click selection.";
            dict_en["select_all"] = "Select All Apps";
            dict_en["sort_button"] = "Sort Apps \u25BE";
            dict_en["sort_selected_top"] = "Selected & Recommended";
            dict_en["sort_name_az"] = "Name (A to Z)";
            dict_en["sort_name_za"] = "Name (Z to A)";
            dict_en["sort_category"] = "By Category";
            dict_en["cat_all"] = "All";
            dict_en["cat_browsers"] = "Web Browsers";
            dict_en["cat_dev"] = "Development & Code";
            dict_en["cat_collab"] = "Communication & Collab";
            dict_en["cat_office"] = "Productivity & Office";
            dict_en["cat_tools"] = "Utilities & Tools";
            dict_en["cat_media"] = "Media & Creative";
            dict_en["cat_sec"] = "Security & Network";
            dict_en["cat_cloud"] = "Cloud & Storage";
            dict_en["cat_games"] = "Gaming & Engines";
            dict_en["btn_install"] = "\uD83D\uDCE5  Install Selected Apps ({0})";
            dict_en["btn_install_no_count"] = "\uD83D\uDCE5  Install Selected Apps";
            dict_en["btn_stop"] = "Stop Install";
            dict_en["btn_import_pack"] = "\uD83D\uDCE5 Import Pack";
            dict_en["btn_create_pack"] = "+ Create New Pack";
            dict_en["badge_ready"] = "Ready";
            dict_en["badge_installed"] = "Installed";
            dict_en["badge_installing"] = "Installing...";
            dict_en["badge_failed"] = "Failed";
            dict_en["showing_count"] = "Showing {0:N0} of {1:N0} apps";
            dict_en["showing_count_selected"] = "Showing {0:N0} of {1:N0} apps ({2:N0} selected)";
            dict_en["showing_local_online"] = "Showing {0} local + {1} online apps for \"{2}\"";
            dict_en["showing_local_online_selected"] = "Showing {0} local + {1} online apps for \"{2}\" ({3:N0} selected)";
            dict_en["found_online_count"] = "Found {0:N0} online apps for \"{1}\"";
            dict_en["found_online_count_selected"] = "Found {0:N0} online apps for \"{1}\" ({2:N0} selected)";
            dict_en["showing_query_count"] = "Showing {0:N0} apps for \"{1}\"";
            dict_en["showing_query_count_selected"] = "Showing {0:N0} apps for \"{1}\" ({2:N0} selected)";
            dict_en["searching_online"] = "\uD83D\uDD0D Searching online Winget repository for \"{0}\"...";
            dict_en["no_apps_found"] = "No software packages found for \"{0}\"";
            dict_en["no_apps_found_desc"] = "Not found in local database or online Winget repository. Check spelling or try a publisher name.";
            dict_en["type_to_search"] = "Please type at least 2 characters to search applications.";
            dict_en["no_winget_title"] = "No local applications found for \"{0}\"";
            dict_en["no_winget_desc"] = "Microsoft Winget is not available on this system to search online repository.";
            dict_en["bottom_tip"] = "Tip: You can customize any pack, add or remove apps, and export your setup for later use.";
            dict_en["pack_selected_prefix"] = "App pack selected: {0}";
            dict_en["logs_title"] = "Install History";
            dict_en["logs_subtitle"] = "Live deployment watchdog, stdout streaming, and audit logs.";
            dict_en["btn_summary_report"] = "Summary Report";
            dict_en["btn_open_logfile"] = "Open Log File";
            dict_en["btn_clear_screen"] = "Clear Screen";
            _translations["en"] = dict_en;

            // Language: hi
            Dictionary<string, string> dict_hi = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            dict_hi["btn_select_pack"] = "\u092A\u0948\u0915 \u091A\u0941\u0928\u0947\u0902";
            dict_hi["btn_customize_pack"] = "\u0910\u092A\u094D\u0938 \u0915\u0938\u094D\u091F\u092E\u093E\u0907\u091C\u093C \u0915\u0930\u0947\u0902";
            dict_hi["btn_export_pack"] = "\u092A\u0948\u0915 \u0928\u093F\u0930\u094D\u092F\u093E\u0924 \u0915\u0930\u0947\u0902";
            dict_hi["nav_quick_setup"] = "\u0924\u094D\u0935\u0930\u093F\u0924 \u0938\u0947\u091F\u0905\u092A";
            dict_hi["nav_all_apps"] = "\u0938\u092D\u0940 \u0910\u092A\u094D\u0938";
            dict_hi["nav_cloud_sync"] = "\u0915\u094D\u0932\u093E\u0909\u0921 \u0938\u093F\u0902\u0915";
            dict_hi["nav_history"] = "\u0938\u094D\u0925\u093E\u092A\u0928\u093E \u0907\u0924\u093F\u0939\u093E\u0938";
            dict_hi["search_placeholder"] = "\u0915\u094B\u0908 \u0910\u092A, \u0938\u0947\u091F\u093F\u0902\u0917 \u092F\u093E \u092A\u0948\u0915 \u0916\u094B\u091C\u0947\u0902...";
            dict_hi["help_button"] = "\u0938\u0939\u093E\u092F\u0924\u093E ?";
            dict_hi["lang_auto"] = "\u0938\u094D\u0935\u0924\u0903 (\u0938\u093F\u0938\u094D\u091F\u092E \u0921\u093F\u092B\u093C\u0949\u0932\u094D\u091F)";
            dict_hi["menu_guide"] = "\u0909\u092A\u092F\u094B\u0917 \u0915\u0948\u0938\u0947 \u0915\u0930\u0947\u0902 (\u0917\u093E\u0907\u0921)...";
            dict_hi["menu_updates"] = "\u0905\u092A\u0921\u0947\u091F \u0915\u0940 \u091C\u093E\u0902\u091A \u0915\u0930\u0947\u0902...";
            dict_hi["menu_settings"] = "\u0938\u0947\u091F\u093F\u0902\u0917\u094D\u0938...";
            dict_hi["menu_about"] = "FleetForge \u0915\u0947 \u092C\u093E\u0930\u0947 \u092E\u0947\u0902...";
            dict_hi["quick_setup_title"] = "\u0924\u094D\u0935\u0930\u093F\u0924 \u0938\u0947\u091F\u0905\u092A";
            dict_hi["quick_setup_subtitle"] = "\u090F\u0915 \u092A\u0948\u0915 \u091A\u0941\u0928\u0947\u0902, \u0910\u092A\u094D\u0938 \u091A\u0941\u0928\u0947\u0902 \u0914\u0930 \u090F\u0915 \u0915\u094D\u0932\u093F\u0915 \u092E\u0947\u0902 \u0936\u0941\u0930\u0942 \u0915\u0930\u0947\u0902\u0964";
            dict_hi["step1_title"] = "\u092A\u0948\u0915 \u091A\u0941\u0928\u0947\u0902";
            dict_hi["step1_desc"] = "\u0905\u092A\u0928\u0940 \u0906\u0935\u0936\u094D\u092F\u0915\u0924\u093E \u091A\u0941\u0928\u0947\u0902";
            dict_hi["step2_title"] = "\u0910\u092A\u094D\u0938 \u0915\u0940 \u0938\u092E\u0940\u0915\u094D\u0937\u093E \u0915\u0930\u0947\u0902";
            dict_hi["step2_desc"] = "\u091A\u092F\u0928 \u0915\u0940 \u092A\u0941\u0937\u094D\u091F\u093F \u0915\u0930\u0947\u0902";
            dict_hi["step3_title"] = "\u0907\u0902\u0938\u094D\u091F\u0949\u0932 \u0915\u0930\u0947\u0902";
            dict_hi["step3_desc"] = "\u0909\u092A\u092F\u094B\u0917 \u0936\u0941\u0930\u0942 \u0915\u0930\u0947\u0902";
            dict_hi["available_packs_title"] = "\u0909\u092A\u0932\u092C\u094D\u0927 \u0910\u092A \u092A\u0948\u0915";
            dict_hi["available_packs_subtitle"] = "\u0924\u0948\u092F\u093E\u0930 \u092A\u0948\u0915 \u091A\u0941\u0928\u0947\u0902 \u092F\u093E \u0905\u0932\u0917-\u0905\u0932\u0917 \u0910\u092A\u094D\u0938 \u091A\u0941\u0928\u0947\u0902\u0964";
            dict_hi["top_choice_badge"] = "\u0938\u0930\u094D\u0935\u0936\u094D\u0930\u0947\u0937\u094D\u0920 \u0935\u093F\u0915\u0932\u094D\u092A";
            dict_hi["recommended_badge"] = "\u0905\u0928\u0941\u0936\u0902\u0938\u093F\u0924";
            dict_hi["custom_badge"] = "\u0915\u0938\u094D\u091F\u092E \u092A\u0948\u0915";
            dict_hi["partner_apps_count"] = "{0} \u092A\u093E\u0930\u094D\u091F\u0928\u0930 \u0910\u092A\u094D\u0938";
            dict_hi["apps_count"] = "{0} \u0910\u092A\u094D\u0938";
            dict_hi["catalog_title"] = "\u0938\u092D\u0940 \u090F\u092A\u094D\u0932\u093F\u0915\u0947\u0936\u0928";
            dict_hi["catalog_subtitle"] = "\u0924\u094D\u0935\u0930\u093F\u0924 \u0916\u094B\u091C \u0914\u0930 \u090F\u0915-\u0915\u094D\u0932\u093F\u0915 \u091A\u092F\u0928 \u0915\u0947 \u0938\u093E\u0925 16,349 \u0938\u0949\u092B\u094D\u091F\u0935\u0947\u092F\u0930 \u092A\u0948\u0915\u0947\u091C \u092C\u094D\u0930\u093E\u0909\u091C\u093C \u0915\u0930\u0947\u0902\u0964";
            dict_hi["select_all"] = "\u0938\u092D\u0940 \u0910\u092A\u094D\u0938 \u091A\u0941\u0928\u0947\u0902";
            dict_hi["sort_button"] = "\u0910\u092A\u094D\u0938 \u0915\u094D\u0930\u092E\u092C\u0926\u094D\u0927 \u0915\u0930\u0947\u0902 \u25BE";
            dict_hi["sort_selected_top"] = "\u091A\u092F\u0928\u093F\u0924 \u0914\u0930 \u0905\u0928\u0941\u0936\u0902\u0938\u093F\u0924";
            dict_hi["sort_name_az"] = "\u0928\u093E\u092E (A \u0938\u0947 Z)";
            dict_hi["sort_name_za"] = "\u0928\u093E\u092E (Z \u0938\u0947 A)";
            dict_hi["sort_category"] = "\u0936\u094D\u0930\u0947\u0923\u0940 \u0905\u0928\u0941\u0938\u093E\u0930";
            dict_hi["cat_all"] = "\u0938\u092D\u0940";
            dict_hi["cat_browsers"] = "\u0935\u0947\u092C \u092C\u094D\u0930\u093E\u0909\u091C\u093C\u0930";
            dict_hi["cat_dev"] = "\u0921\u0947\u0935\u0932\u092A\u092E\u0947\u0902\u091F \u0914\u0930 \u0915\u094B\u0921";
            dict_hi["cat_collab"] = "\u0938\u0902\u091A\u093E\u0930 \u0914\u0930 \u0938\u0939\u092F\u094B\u0917";
            dict_hi["cat_office"] = "\u0909\u0924\u094D\u092A\u093E\u0926\u0915\u0924\u093E \u0914\u0930 \u0915\u093E\u0930\u094D\u092F\u093E\u0932\u092F";
            dict_hi["cat_tools"] = "\u0909\u092A\u092F\u094B\u0917\u093F\u0924\u093E\u090F\u0901 \u0914\u0930 \u0909\u092A\u0915\u0930\u0923";
            dict_hi["cat_media"] = "\u092E\u0940\u0921\u093F\u092F\u093E \u0914\u0930 \u0930\u091A\u0928\u093E\u0924\u094D\u092E\u0915";
            dict_hi["cat_sec"] = "\u0938\u0941\u0930\u0915\u094D\u0937\u093E \u0914\u0930 \u0928\u0947\u091F\u0935\u0930\u094D\u0915";
            dict_hi["cat_cloud"] = "\u0915\u094D\u0932\u093E\u0909\u0921 \u0914\u0930 \u0938\u094D\u091F\u094B\u0930\u0947\u091C";
            dict_hi["cat_games"] = "\u0917\u0947\u092E\u093F\u0902\u0917 \u0914\u0930 \u0907\u0902\u091C\u0928";
            dict_hi["btn_install"] = "\uD83D\uDCE5  \u091A\u0941\u0928\u093F\u0902\u0926\u093E \u0910\u092A\u094D\u0938 \u0907\u0902\u0938\u094D\u091F\u0949\u0932 \u0915\u0930\u0947\u0902 ({0})";
            dict_hi["btn_install_no_count"] = "\uD83D\uDCE5  \u091A\u0941\u0928\u093F\u0902\u0926\u093E \u0910\u092A\u094D\u0938 \u0907\u0902\u0938\u094D\u091F\u0949\u0932 \u0915\u0930\u0947\u0902";
            dict_hi["btn_stop"] = "\u0938\u094D\u0925\u093E\u092A\u0928\u093E \u0930\u094B\u0915\u0947\u0902";
            dict_hi["btn_import_pack"] = "\uD83D\uDCE5 \u092A\u0948\u0915 \u0906\u092F\u093E\u0924 \u0915\u0930\u0947\u0902";
            dict_hi["btn_create_pack"] = "+ \u0928\u092F\u093E \u092A\u0948\u0915 \u092C\u0928\u093E\u090F\u0902";
            dict_hi["badge_ready"] = "\u0924\u0948\u092F\u093E\u0930";
            dict_hi["badge_installed"] = "\u0938\u094D\u0925\u093E\u092A\u093F\u0924";
            dict_hi["badge_installing"] = "\u0907\u0902\u0938\u094D\u091F\u0949\u0932 \u0939\u094B \u0930\u0939\u093E \u0939\u0948...";
            dict_hi["badge_failed"] = "\u0935\u093F\u092B\u0932";
            dict_hi["showing_count"] = "{1:N0} \u092E\u0947\u0902 \u0938\u0947 {0:N0} \u0910\u092A\u094D\u0938 \u0926\u093F\u0916\u093E\u090F \u091C\u093E \u0930\u0939\u0947 \u0939\u0948\u0902";
            dict_hi["showing_count_selected"] = "{1:N0} \u092E\u0947\u0902 \u0938\u0947 {0:N0} \u0910\u092A\u094D\u0938 ({2:N0} \u091A\u092F\u0928\u093F\u0924)";
            dict_hi["showing_local_online"] = "\"{2}\" \u0915\u0947 \u0932\u093F\u090F {0} \u0938\u094D\u0925\u093E\u0928\u0940\u092F + {1} \u0911\u0928\u0932\u093E\u0907\u0928 \u0910\u092A\u094D\u0938";
            dict_hi["showing_local_online_selected"] = "\"{2}\" \u0915\u0947 \u0932\u093F\u090F {0} \u0938\u094D\u0925\u093E\u0928\u0940\u092F + {1} \u0911\u0928\u0932\u093E\u0907\u0928 \u0910\u092A\u094D\u0938 ({3:N0} \u091A\u092F\u0928\u093F\u0924)";
            dict_hi["found_online_count"] = "\"{1}\" \u0915\u0947 \u0932\u093F\u090F {0:N0} \u0911\u0928\u0932\u093E\u0907\u0928 \u0910\u092A\u094D\u0938 \u092E\u093F\u0932\u0947";
            dict_hi["found_online_count_selected"] = "\"{1}\" \u0915\u0947 \u0932\u093F\u090F {0:N0} \u0911\u0928\u0932\u093E\u0907\u0928 \u0910\u092A\u094D\u0938 \u092E\u093F\u0932\u0947 ({2:N0} \u091A\u092F\u0928\u093F\u0924)";
            dict_hi["showing_query_count"] = "\"{1}\" \u0915\u0947 \u0932\u093F\u090F {0:N0} \u0910\u092A\u094D\u0938 \u092A\u094D\u0930\u0926\u0930\u094D\u0936\u093F\u0924";
            dict_hi["showing_query_count_selected"] = "\"{1}\" \u0915\u0947 \u0932\u093F\u090F {0:N0} \u0910\u092A\u094D\u0938 \u092A\u094D\u0930\u0926\u0930\u094D\u0936\u093F\u0924 ({2:N0} \u091A\u092F\u0928\u093F\u0924)";
            dict_hi["searching_online"] = "\uD83D\uDD0D \"{0}\" \u0915\u0947 \u0932\u093F\u090F \u0911\u0928\u0932\u093E\u0907\u0928 \u0935\u093F\u0902\u0917\u0947\u091F \u0930\u093F\u092A\u0949\u091C\u093F\u091F\u0930\u0940 \u092E\u0947\u0902 \u0916\u094B\u091C \u091C\u093E\u0930\u0940...";
            dict_hi["no_apps_found"] = "\"{0}\" \u0915\u0947 \u0932\u093F\u090F \u0915\u094B\u0908 \u0938\u0949\u092B\u094D\u091F\u0935\u0947\u092F\u0930 \u092A\u0948\u0915\u0947\u091C \u0928\u0939\u0940\u0902 \u092E\u093F\u0932\u093E";
            dict_hi["no_apps_found_desc"] = "\u0938\u094D\u0925\u093E\u0928\u0940\u092F \u0921\u0947\u091F\u093E\u092C\u0947\u0938 \u092F\u093E \u0935\u093F\u0902\u0917\u0947\u091F \u092E\u0947\u0902 \u0909\u092A\u0932\u092C\u094D\u0927 \u0928\u0939\u0940\u0902 \u0939\u0948\u0964 \u0938\u094D\u092A\u0947\u0932\u093F\u0902\u0917 \u091C\u093E\u0902\u091A\u0947\u0902 \u092F\u093E \u092A\u094D\u0930\u0915\u093E\u0936\u0915 \u0915\u093E \u0928\u093E\u092E \u0906\u091C\u093C\u092E\u093E\u090F\u0902\u0964";
            dict_hi["type_to_search"] = "\u090F\u092A\u094D\u0932\u093F\u0915\u0947\u0936\u0928 \u0916\u094B\u091C\u0928\u0947 \u0915\u0947 \u0932\u093F\u090F \u0915\u0943\u092A\u092F\u093E \u0915\u092E \u0938\u0947 \u0915\u092E 2 \u0905\u0915\u094D\u0937\u0930 \u091F\u093E\u0907\u092A \u0915\u0930\u0947\u0902\u0964";
            dict_hi["no_winget_title"] = "\"{0}\" \u0915\u0947 \u0932\u093F\u090F \u0915\u094B\u0908 \u0938\u094D\u0925\u093E\u0928\u0940\u092F \u0910\u092A \u0928\u0939\u0940\u0902 \u092E\u093F\u0932\u093E";
            dict_hi["no_winget_desc"] = "\u0911\u0928\u0932\u093E\u0907\u0928 \u0930\u093F\u092A\u0949\u091C\u093F\u091F\u0930\u0940 \u0916\u094B\u091C\u0928\u0947 \u0915\u0947 \u0932\u093F\u090F Microsoft Winget \u0909\u092A\u0932\u092C\u094D\u0927 \u0928\u0939\u0940\u0902 \u0939\u0948\u0964";
            dict_hi["bottom_tip"] = "\u0938\u0941\u091D\u093E\u0935: \u0906\u092A \u0915\u093F\u0938\u0940 \u092D\u0940 \u092A\u0948\u0915 \u0915\u094B \u0915\u0938\u094D\u091F\u092E\u093E\u0907\u091C\u093C \u0915\u0930 \u0938\u0915\u0924\u0947 \u0939\u0948\u0902, \u0910\u092A\u094D\u0938 \u091C\u094B\u0921\u093C \u092F\u093E \u0939\u091F\u093E \u0938\u0915\u0924\u0947 \u0939\u0948\u0902\u0964";
            dict_hi["pack_selected_prefix"] = "\u091A\u092F\u0928\u093F\u0924 \u0910\u092A \u092A\u0948\u0915: {0}";
            dict_hi["logs_title"] = "\u0938\u094D\u0925\u093E\u092A\u0928\u093E \u0907\u0924\u093F\u0939\u093E\u0938";
            dict_hi["logs_subtitle"] = "\u0932\u093E\u0907\u0935 \u092A\u0930\u093F\u0928\u093F\u092F\u094B\u091C\u0928 \u0928\u093F\u0917\u0930\u093E\u0928\u0940 \u0914\u0930 \u0911\u0921\u093F\u091F \u0932\u0949\u0917\u0964";
            dict_hi["btn_summary_report"] = "\u0938\u093E\u0930\u093E\u0902\u0936 \u0930\u093F\u092A\u094B\u0930\u094D\u091F";
            dict_hi["btn_open_logfile"] = "\u0932\u0949\u0917 \u092B\u093C\u093E\u0907\u0932 \u0916\u094B\u0932\u0947\u0902";
            dict_hi["btn_clear_screen"] = "\u0938\u094D\u0915\u094D\u0930\u0940\u0928 \u0938\u093E\u092B\u093C \u0915\u0930\u0947\u0902";
            _translations["hi"] = dict_hi;

            // Language: es
            Dictionary<string, string> dict_es = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            dict_es["btn_select_pack"] = "Seleccionar pack";
            dict_es["btn_customize_pack"] = "Personalizar apps";
            dict_es["btn_export_pack"] = "Exportar pack";
            dict_es["nav_quick_setup"] = "Configuraci\u00F3n r\u00E1pida";
            dict_es["nav_all_apps"] = "Todas las aplicaciones";
            dict_es["nav_cloud_sync"] = "Sincronizaci\u00F3n en la nube";
            dict_es["nav_history"] = "Historial de instalaci\u00F3n";
            dict_es["search_placeholder"] = "Buscar aplicaci\u00F3n, ajuste o paquete...";
            dict_es["help_button"] = "Ayuda ?";
            dict_es["lang_auto"] = "Autom\u00E1tico (Predeterminado del sistema)";
            dict_es["menu_guide"] = "C\u00F3mo usar (Gu\u00EDa interactiva)...";
            dict_es["menu_updates"] = "Buscar actualizaciones...";
            dict_es["menu_settings"] = "Configuraci\u00F3n...";
            dict_es["menu_about"] = "Acerca de FleetForge...";
            dict_es["quick_setup_title"] = "Configuraci\u00F3n r\u00E1pida";
            dict_es["quick_setup_subtitle"] = "Elige un paquete, selecciona aplicaciones y comienza con un clic.";
            dict_es["step1_title"] = "Elige un paquete";
            dict_es["step1_desc"] = "Seg\u00FAn tus necesidades";
            dict_es["step2_title"] = "Revisar aplicaciones";
            dict_es["step2_desc"] = "Comprueba la selecci\u00F3n";
            dict_es["step3_title"] = "Instalar";
            dict_es["step3_desc"] = "Comenzar a usar";
            dict_es["available_packs_title"] = "Paquetes de aplicaciones disponibles";
            dict_es["available_packs_subtitle"] = "Selecciona un paquete listo o aplicaciones individuales.";
            dict_es["top_choice_badge"] = "MEJOR OPCI\u00D3N";
            dict_es["recommended_badge"] = "RECOMENDADO";
            dict_es["custom_badge"] = "PAQUETE PERSONALIZADO";
            dict_es["partner_apps_count"] = "{0} apps asociadas";
            dict_es["apps_count"] = "{0} aplicaciones";
            dict_es["catalog_title"] = "Todas las aplicaciones";
            dict_es["catalog_subtitle"] = "Explora 16.349 paquetes de software con b\u00FAsqueda instant\u00E1nea y selecci\u00F3n con un clic.";
            dict_es["select_all"] = "Seleccionar todas";
            dict_es["sort_button"] = "Ordenar aplicaciones \u25BE";
            dict_es["sort_selected_top"] = "Seleccionadas y recomendadas";
            dict_es["sort_name_az"] = "Nombre (A a Z)";
            dict_es["sort_name_za"] = "Nombre (Z a A)";
            dict_es["sort_category"] = "Por categor\u00EDa";
            dict_es["cat_all"] = "Todas";
            dict_es["cat_browsers"] = "Navegadores web";
            dict_es["cat_dev"] = "Desarrollo y c\u00F3digo";
            dict_es["cat_collab"] = "Comunicaci\u00F3n y colaboraci\u00F3n";
            dict_es["cat_office"] = "Productividad y oficina";
            dict_es["cat_tools"] = "Utilidades y herramientas";
            dict_es["cat_media"] = "Medios y creatividad";
            dict_es["cat_sec"] = "Seguridad y redes";
            dict_es["cat_cloud"] = "Nube y almacenamiento";
            dict_es["cat_games"] = "Juegos y motores";
            dict_es["btn_install"] = "\uD83D\uDCE5  Instalar aplicaciones seleccionadas ({0})";
            dict_es["btn_install_no_count"] = "\uD83D\uDCE5  Instalar aplicaciones seleccionadas";
            dict_es["btn_stop"] = "Detener instalaci\u00F3n";
            dict_es["btn_import_pack"] = "\uD83D\uDCE5 Importar paquete";
            dict_es["btn_create_pack"] = "+ Crear nuevo paquete";
            dict_es["badge_ready"] = "Listo";
            dict_es["badge_installed"] = "Instalado";
            dict_es["badge_installing"] = "Instalando...";
            dict_es["badge_failed"] = "Fall\u00F3";
            dict_es["showing_count"] = "Mostrando {0:N0} de {1:N0} apps";
            dict_es["showing_count_selected"] = "Mostrando {0:N0} de {1:N0} apps ({2:N0} seleccionadas)";
            dict_es["showing_local_online"] = "Mostrando {0} locales + {1} en l\u00EDnea para \"{2}\"";
            dict_es["showing_local_online_selected"] = "Mostrando {0} locales + {1} en l\u00EDnea para \"{2}\" ({3:N0} seleccionadas)";
            dict_es["found_online_count"] = "Se encontraron {0:N0} aplicaciones en l\u00EDnea para \"{1}\"";
            dict_es["found_online_count_selected"] = "Se encontraron {0:N0} aplicaciones en l\u00EDnea para \"{1}\" ({2:N0} seleccionadas)";
            dict_es["showing_query_count"] = "Mostrando {0:N0} aplicaciones para \"{1}\"";
            dict_es["showing_query_count_selected"] = "Mostrando {0:N0} aplicaciones para \"{1}\" ({2:N0} seleccionadas)";
            dict_es["searching_online"] = "\uD83D\uDD0D Buscando en repositorio Winget en l\u00EDnea para \"{0}\"...";
            dict_es["no_apps_found"] = "No se encontraron paquetes de software para \"{0}\"";
            dict_es["no_apps_found_desc"] = "No encontrado localmente ni en Winget en l\u00EDnea. Verifique la ortograf\u00EDa.";
            dict_es["type_to_search"] = "Escriba al menos 2 caracteres para buscar aplicaciones.";
            dict_es["no_winget_title"] = "No se encontraron aplicaciones locales para \"{0}\"";
            dict_es["no_winget_desc"] = "Microsoft Winget no est\u00E1 disponible para buscar en l\u00EDnea.";
            dict_es["bottom_tip"] = "Consejo: Puedes personalizar cualquier paquete, agregar o eliminar aplicaciones.";
            dict_es["pack_selected_prefix"] = "Paquete de aplicaciones seleccionado: {0}";
            dict_es["logs_title"] = "Historial de instalaci\u00F3n";
            dict_es["logs_subtitle"] = "Supervisi\u00F3n de despliegue en vivo y registros de auditor\u00EDa.";
            dict_es["btn_summary_report"] = "Informe de resumen";
            dict_es["btn_open_logfile"] = "Abrir archivo de registro";
            dict_es["btn_clear_screen"] = "Limpiar pantalla";
            _translations["es"] = dict_es;

            // Language: fr
            Dictionary<string, string> dict_fr = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            dict_fr["btn_select_pack"] = "S\u00E9lectionner";
            dict_fr["btn_customize_pack"] = "Personnaliser";
            dict_fr["btn_export_pack"] = "Exporter";
            dict_fr["nav_quick_setup"] = "Configuration rapide";
            dict_fr["nav_all_apps"] = "Toutes les applications";
            dict_fr["nav_cloud_sync"] = "Synchronisation Cloud";
            dict_fr["nav_history"] = "Historique d'installation";
            dict_fr["search_placeholder"] = "Rechercher une application, r\u00E9glage ou pack...";
            dict_fr["help_button"] = "Aide ?";
            dict_fr["lang_auto"] = "Automatique (Syst\u00E8me)";
            dict_fr["menu_guide"] = "Guide d'utilisation interactif...";
            dict_fr["menu_updates"] = "V\u00E9rifier les mises \u00E0 jour...";
            dict_fr["menu_settings"] = "Param\u00E8tres...";
            dict_fr["menu_about"] = "\u00C0 propos de FleetForge...";
            dict_fr["quick_setup_title"] = "Configuration rapide";
            dict_fr["quick_setup_subtitle"] = "Choisissez un pack, s\u00E9lectionnez des applications et d\u00E9marrez en un clic.";
            dict_fr["step1_title"] = "Choisir un pack";
            dict_fr["step1_desc"] = "Selon vos besoins";
            dict_fr["step2_title"] = "V\u00E9rifier les apps";
            dict_fr["step2_desc"] = "V\u00E9rifiez la s\u00E9lection";
            dict_fr["step3_title"] = "Installer";
            dict_fr["step3_desc"] = "Commencer \u00E0 utiliser";
            dict_fr["available_packs_title"] = "Packs d'applications disponibles";
            dict_fr["available_packs_subtitle"] = "S\u00E9lectionnez un pack pr\u00EAt \u00E0 l'emploi ou des applications individuelles.";
            dict_fr["top_choice_badge"] = "MEILLEUR CHOIX";
            dict_fr["recommended_badge"] = "RECOMMAND\u00C9";
            dict_fr["custom_badge"] = "PACK PERSONNALIS\u00C9";
            dict_fr["partner_apps_count"] = "{0} apps partenaires";
            dict_fr["apps_count"] = "{0} applications";
            dict_fr["catalog_title"] = "Toutes les applications";
            dict_fr["catalog_subtitle"] = "Parcourez 16 349 logiciels avec recherche instantan\u00E9e et s\u00E9lection en un clic.";
            dict_fr["select_all"] = "Tout s\u00E9lectionner";
            dict_fr["sort_button"] = "Trier les applications \u25BE";
            dict_fr["sort_selected_top"] = "S\u00E9lectionn\u00E9es et recommand\u00E9es";
            dict_fr["sort_name_az"] = "Nom (A \u00E0 Z)";
            dict_fr["sort_name_za"] = "Nom (Z \u00E0 A)";
            dict_fr["sort_category"] = "Par cat\u00E9gorie";
            dict_fr["cat_all"] = "Tous";
            dict_fr["cat_browsers"] = "Navigateurs Web";
            dict_fr["cat_dev"] = "D\u00E9veloppement et code";
            dict_fr["cat_collab"] = "Communication & Collab";
            dict_fr["cat_office"] = "Productivit\u00E9 & Bureau";
            dict_fr["cat_tools"] = "Utilitaires et outils";
            dict_fr["cat_media"] = "M\u00E9dias et cr\u00E9ation";
            dict_fr["cat_sec"] = "S\u00E9curit\u00E9 et r\u00E9seau";
            dict_fr["cat_cloud"] = "Cloud et stockage";
            dict_fr["cat_games"] = "Jeux et moteurs";
            dict_fr["btn_install"] = "\uD83D\uDCE5  Installer les applications s\u00E9lectionn\u00E9es ({0})";
            dict_fr["btn_install_no_count"] = "\uD83D\uDCE5  Installer les applications s\u00E9lectionn\u00E9es";
            dict_fr["btn_stop"] = "Arr\u00EAter l'installation";
            dict_fr["btn_import_pack"] = "\uD83D\uDCE5 Importer un pack";
            dict_fr["btn_create_pack"] = "+ Cr\u00E9er un nouveau pack";
            dict_fr["badge_ready"] = "Pr\u00EAt";
            dict_fr["badge_installed"] = "Install\u00E9";
            dict_fr["badge_installing"] = "Installation...";
            dict_fr["badge_failed"] = "\u00C9chou\u00E9";
            dict_fr["showing_count"] = "Affichage de {0:N0} sur {1:N0} apps";
            dict_fr["showing_count_selected"] = "Affichage de {0:N0} sur {1:N0} apps ({2:N0} s\u00E9lectionn\u00E9es)";
            dict_fr["showing_local_online"] = "Affichage de {0} local(es) + {1} en ligne pour \"{2}\"";
            dict_fr["showing_local_online_selected"] = "Affichage de {0} local(es) + {1} en ligne pour \"{2}\" ({3:N0} s\u00E9lectionn\u00E9es)";
            dict_fr["found_online_count"] = "{0:N0} application(s) trouv\u00E9e(s) en ligne pour \"{1}\"";
            dict_fr["found_online_count_selected"] = "{0:N0} application(s) trouv\u00E9e(s) en ligne pour \"{1}\" ({2:N0} s\u00E9lectionn\u00E9es)";
            dict_fr["showing_query_count"] = "Affichage de {0:N0} apps pour \"{1}\"";
            dict_fr["showing_query_count_selected"] = "Affichage de {0:N0} apps pour \"{1}\" ({2:N0} s\u00E9lectionn\u00E9es)";
            dict_fr["searching_online"] = "\uD83D\uDD0D Recherche en ligne dans Winget pour \"{0}\"...";
            dict_fr["no_apps_found"] = "Aucun logiciel trouv\u00E9 pour \"{0}\"";
            dict_fr["no_apps_found_desc"] = "Introuvable dans la base locale ou Winget. V\u00E9rifiez l'orthographe.";
            dict_fr["type_to_search"] = "Veuillez saisir au moins 2 caract\u00E8res pour rechercher.";
            dict_fr["no_winget_title"] = "Aucune application locale trouv\u00E9e pour \"{0}\"";
            dict_fr["no_winget_desc"] = "Microsoft Winget n'est pas disponible pour la recherche en ligne.";
            dict_fr["bottom_tip"] = "Astuce : Vous pouvez personnaliser n'importe quel pack et exporter votre s\u00E9lection.";
            dict_fr["pack_selected_prefix"] = "Pack d'applications s\u00E9lectionn\u00E9 : {0}";
            dict_fr["logs_title"] = "Historique d'installation";
            dict_fr["logs_subtitle"] = "Surveillance du d\u00E9ploiement en direct et journaux d'audit.";
            dict_fr["btn_summary_report"] = "Rapport r\u00E9capitulatif";
            dict_fr["btn_open_logfile"] = "Ouvrir le fichier journal";
            dict_fr["btn_clear_screen"] = "Effacer l'\u00E9cran";
            _translations["fr"] = dict_fr;

            // Language: de
            Dictionary<string, string> dict_de = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            dict_de["btn_select_pack"] = "Paket w\u00E4hlen";
            dict_de["btn_customize_pack"] = "Anpassen";
            dict_de["btn_export_pack"] = "Exportieren";
            dict_de["nav_quick_setup"] = "Schnelleinrichtung";
            dict_de["nav_all_apps"] = "Alle Apps";
            dict_de["nav_cloud_sync"] = "Cloud-Synchronisierung";
            dict_de["nav_history"] = "Installationsverlauf";
            dict_de["search_placeholder"] = "App, Einstellung oder Paket suchen...";
            dict_de["help_button"] = "Hilfe ?";
            dict_de["lang_auto"] = "Automatisch (Systemstandard)";
            dict_de["menu_guide"] = "Bedienungsanleitung (Interaktiv)...";
            dict_de["menu_updates"] = "Nach Updates suchen...";
            dict_de["menu_settings"] = "Einstellungen...";
            dict_de["menu_about"] = "\u00DCber FleetForge...";
            dict_de["quick_setup_title"] = "Schnelleinrichtung";
            dict_de["quick_setup_subtitle"] = "W\u00E4hlen Sie ein Paket, Apps ausw\u00E4hlen und mit einem Klick starten.";
            dict_de["step1_title"] = "Paket w\u00E4hlen";
            dict_de["step1_desc"] = "Nach Ihrem Bedarf";
            dict_de["step2_title"] = "Apps pr\u00FCfen";
            dict_de["step2_desc"] = "Auswahl pr\u00FCfen";
            dict_de["step3_title"] = "Installieren";
            dict_de["step3_desc"] = "Sofort loslegen";
            dict_de["available_packs_title"] = "Verf\u00FCgbare App-Pakete";
            dict_de["available_packs_subtitle"] = "W\u00E4hlen Sie ein vorgefertigtes Paket oder einzelne Apps.";
            dict_de["top_choice_badge"] = "TOP-AUSWAHL";
            dict_de["recommended_badge"] = "EMPFOHLEN";
            dict_de["custom_badge"] = "BENUTZERDEFINIERTES PAKET";
            dict_de["partner_apps_count"] = "{0} Partner-Apps";
            dict_de["apps_count"] = "{0} Apps";
            dict_de["catalog_title"] = "Alle Anwendungen";
            dict_de["catalog_subtitle"] = "Durchsuchen Sie 16.349 Softwarepakete mit Sofortsuche und Ein-Klick-Auswahl.";
            dict_de["select_all"] = "Alle Apps ausw\u00E4hlen";
            dict_de["sort_button"] = "Apps sortieren \u25BE";
            dict_de["sort_selected_top"] = "Ausgew\u00E4hlt & Empfohlen";
            dict_de["sort_name_az"] = "Name (A bis Z)";
            dict_de["sort_name_za"] = "Name (Z bis A)";
            dict_de["sort_category"] = "Nach Kategorie";
            dict_de["cat_all"] = "Alle";
            dict_de["cat_browsers"] = "Webbrowser";
            dict_de["cat_dev"] = "Entwicklung & Code";
            dict_de["cat_collab"] = "Kommunikation & Team";
            dict_de["cat_office"] = "Produktivit\u00E4t & B\u00FCro";
            dict_de["cat_tools"] = "Dienstprogramme & Tools";
            dict_de["cat_media"] = "Medien & Kreatives";
            dict_de["cat_sec"] = "Sicherheit & Netzwerk";
            dict_de["cat_cloud"] = "Cloud & Speicher";
            dict_de["cat_games"] = "Gaming & Engines";
            dict_de["btn_install"] = "\uD83D\uDCE5  Ausgew\u00E4hlte Apps installieren ({0})";
            dict_de["btn_install_no_count"] = "\uD83D\uDCE5  Ausgew\u00E4hlte Apps installieren";
            dict_de["btn_stop"] = "Installation anhalten";
            dict_de["btn_import_pack"] = "\uD83D\uDCE5 Paket importieren";
            dict_de["btn_create_pack"] = "+ Neues Paket erstellen";
            dict_de["badge_ready"] = "Bereit";
            dict_de["badge_installed"] = "Installiert";
            dict_de["badge_installing"] = "Wird installiert...";
            dict_de["badge_failed"] = "Fehlgeschlagen";
            dict_de["showing_count"] = "{0:N0} von {1:N0} Apps angezeigt";
            dict_de["showing_count_selected"] = "{0:N0} von {1:N0} Apps angezeigt ({2:N0} ausgew\u00E4hlt)";
            dict_de["showing_local_online"] = "{0} lokale + {1} Online-Apps f\u00FCr \u201E{2}\u201C";
            dict_de["showing_local_online_selected"] = "{0} lokale + {1} Online-Apps f\u00FCr \u201E{2}\u201C ({3:N0} ausgew\u00E4hlt)";
            dict_de["found_online_count"] = "{0:N0} Online-Apps f\u00FCr \u201E{1}\u201C gefunden";
            dict_de["found_online_count_selected"] = "{0:N0} Online-Apps f\u00FCr \u201E{1}\u201C gefunden ({2:N0} ausgew\u00E4hlt)";
            dict_de["showing_query_count"] = "{0:N0} Apps f\u00FCr \u201E{1}\u201C angezeigt";
            dict_de["showing_query_count_selected"] = "{0:N0} Apps f\u00FCr \u201E{1}\u201C angezeigt ({2:N0} ausgew\u00E4hlt)";
            dict_de["searching_online"] = "\uD83D\uDD0D Suche im Online-Winget-Repository nach \u201E{0}\u201C...";
            dict_de["no_apps_found"] = "Keine Softwarepakete f\u00FCr \u201E{0}\u201C gefunden";
            dict_de["no_apps_found_desc"] = "Weder lokal noch im Online-Repository gefunden. Bitte Schreibweise pr\u00FCfen.";
            dict_de["type_to_search"] = "Bitte mindestens 2 Zeichen eingeben, um zu suchen.";
            dict_de["no_winget_title"] = "Keine lokalen Apps f\u00FCr \u201E{0}\u201C gefunden";
            dict_de["no_winget_desc"] = "Microsoft Winget ist f\u00FCr die Online-Suche nicht verf\u00FCgbar.";
            dict_de["bottom_tip"] = "Tipp: Sie k\u00F6nnen jedes Paket anpassen, Apps hinzuf\u00FCgen oder entfernen.";
            dict_de["pack_selected_prefix"] = "Ausgew\u00E4hltes App-Paket: {0}";
            dict_de["logs_title"] = "Installationsverlauf";
            dict_de["logs_subtitle"] = "Live-Bereitstellungs\u00FCberwachung und \u00DCberwachungsprotokolle.";
            dict_de["btn_summary_report"] = "Zusammenfassender Bericht";
            dict_de["btn_open_logfile"] = "Protokolldatei \u00F6ffnen";
            dict_de["btn_clear_screen"] = "Bildschirm leeren";
            _translations["de"] = dict_de;

            // Language: ru
            Dictionary<string, string> dict_ru = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            dict_ru["btn_select_pack"] = "\u0412\u044B\u0431\u0440\u0430\u0442\u044C \u043F\u0430\u043A\u0435\u0442";
            dict_ru["btn_customize_pack"] = "\u041D\u0430\u0441\u0442\u0440\u043E\u0438\u0442\u044C";
            dict_ru["btn_export_pack"] = "\u042D\u043A\u0441\u043F\u043E\u0440\u0442";
            dict_ru["nav_quick_setup"] = "\u0411\u044B\u0441\u0442\u0440\u0430\u044F \u043D\u0430\u0441\u0442\u0440\u043E\u0439\u043A\u0430";
            dict_ru["nav_all_apps"] = "\u0412\u0441\u0435 \u043F\u0440\u0438\u043B\u043E\u0436\u0435\u043D\u0438\u044F";
            dict_ru["nav_cloud_sync"] = "\u041E\u0431\u043B\u0430\u0447\u043D\u0430\u044F \u0441\u0438\u043D\u0445\u0440\u043E\u043D\u0438\u0437\u0430\u0446\u0438\u044F";
            dict_ru["nav_history"] = "\u0418\u0441\u0442\u043E\u0440\u0438\u044F \u0443\u0441\u0442\u0430\u043D\u043E\u0432\u043A\u0438";
            dict_ru["search_placeholder"] = "\u041D\u0430\u0439\u0442\u0438 \u043F\u0440\u0438\u043B\u043E\u0436\u0435\u043D\u0438\u0435, \u043D\u0430\u0441\u0442\u0440\u043E\u0439\u043A\u0443 \u0438\u043B\u0438 \u043F\u0430\u043A\u0435\u0442...";
            dict_ru["help_button"] = "\u0421\u043F\u0440\u0430\u0432\u043A\u0430 ?";
            dict_ru["lang_auto"] = "\u0410\u0432\u0442\u043E (\u041F\u043E \u0443\u043C\u043E\u043B\u0447\u0430\u043D\u0438\u044E)";
            dict_ru["menu_guide"] = "\u0420\u0443\u043A\u043E\u0432\u043E\u0434\u0441\u0442\u0432\u043E \u043F\u043E\u043B\u044C\u0437\u043E\u0432\u0430\u0442\u0435\u043B\u044F...";
            dict_ru["menu_updates"] = "\u041F\u0440\u043E\u0432\u0435\u0440\u0438\u0442\u044C \u043E\u0431\u043D\u043E\u0432\u043B\u0435\u043D\u0438\u044F...";
            dict_ru["menu_settings"] = "\u041D\u0430\u0441\u0442\u0440\u043E\u0439\u043A\u0438...";
            dict_ru["menu_about"] = "\u041E \u043F\u0440\u043E\u0433\u0440\u0430\u043C\u043C\u0435 FleetForge...";
            dict_ru["quick_setup_title"] = "\u0411\u044B\u0441\u0442\u0440\u0430\u044F \u043D\u0430\u0441\u0442\u0440\u043E\u0439\u043A\u0430";
            dict_ru["quick_setup_subtitle"] = "\u0412\u044B\u0431\u0435\u0440\u0438\u0442\u0435 \u043F\u0430\u043A\u0435\u0442, \u043E\u0442\u043C\u0435\u0442\u044C\u0442\u0435 \u043F\u0440\u0438\u043B\u043E\u0436\u0435\u043D\u0438\u044F \u0438 \u043D\u0430\u0447\u043D\u0438\u0442\u0435 \u0432 \u043E\u0434\u0438\u043D \u043A\u043B\u0438\u043A.";
            dict_ru["step1_title"] = "\u0412\u044B\u0431\u0435\u0440\u0438\u0442\u0435 \u043F\u0430\u043A\u0435\u0442";
            dict_ru["step1_desc"] = "\u041F\u043E \u0432\u0430\u0448\u0438\u043C \u043F\u043E\u0442\u0440\u0435\u0431\u043D\u043E\u0441\u0442\u044F\u043C";
            dict_ru["step2_title"] = "\u041F\u0440\u043E\u0441\u043C\u043E\u0442\u0440 \u043F\u0440\u0438\u043B\u043E\u0436\u0435\u043D\u0438\u0439";
            dict_ru["step2_desc"] = "\u041F\u0440\u043E\u0432\u0435\u0440\u044C\u0442\u0435 \u0432\u044B\u0431\u043E\u0440";
            dict_ru["step3_title"] = "\u0423\u0441\u0442\u0430\u043D\u043E\u0432\u0438\u0442\u044C";
            dict_ru["step3_desc"] = "\u041D\u0430\u0447\u043D\u0438\u0442\u0435 \u0438\u0441\u043F\u043E\u043B\u044C\u0437\u043E\u0432\u0430\u0442\u044C";
            dict_ru["available_packs_title"] = "\u0414\u043E\u0441\u0442\u0443\u043F\u043D\u044B\u0435 \u043F\u0430\u043A\u0435\u0442\u044B \u043F\u0440\u0438\u043B\u043E\u0436\u0435\u043D\u0438\u0439";
            dict_ru["available_packs_subtitle"] = "\u0412\u044B\u0431\u0435\u0440\u0438\u0442\u0435 \u0433\u043E\u0442\u043E\u0432\u044B\u0439 \u043F\u0430\u043A\u0435\u0442 \u0438\u043B\u0438 \u043E\u0442\u0434\u0435\u043B\u044C\u043D\u044B\u0435 \u043F\u0440\u0438\u043B\u043E\u0436\u0435\u043D\u0438\u044F.";
            dict_ru["top_choice_badge"] = "\u041B\u0423\u0427\u0428\u0418\u0419 \u0412\u042B\u0411\u041E\u0420";
            dict_ru["recommended_badge"] = "\u0420\u0415\u041A\u041E\u041C\u0415\u041D\u0414\u0423\u0415\u0422\u0421\u042F";
            dict_ru["custom_badge"] = "\u041F\u041E\u041B\u042C\u0417\u041E\u0412\u0410\u0422\u0415\u041B\u042C\u0421\u041A\u0418\u0419 \u041F\u0410\u041A\u0415\u0422";
            dict_ru["partner_apps_count"] = "{0} \u043F\u0430\u0440\u0442\u043D\u0435\u0440\u0441\u043A\u0438\u0445 \u043F\u0440\u0438\u043B\u043E\u0436\u0435\u043D\u0438\u0439";
            dict_ru["apps_count"] = "{0} \u043F\u0440\u0438\u043B\u043E\u0436\u0435\u043D\u0438\u0439";
            dict_ru["catalog_title"] = "\u0412\u0441\u0435 \u043F\u0440\u0438\u043B\u043E\u0436\u0435\u043D\u0438\u044F";
            dict_ru["catalog_subtitle"] = "\u041F\u0440\u043E\u0441\u043C\u0430\u0442\u0440\u0438\u0432\u0430\u0439\u0442\u0435 16 349 \u043F\u0430\u043A\u0435\u0442\u043E\u0432 \u041F\u041E \u0441 \u043C\u0433\u043D\u043E\u0432\u0435\u043D\u043D\u044B\u043C \u043F\u043E\u0438\u0441\u043A\u043E\u043C \u0438 \u0432\u044B\u0431\u043E\u0440\u043E\u043C \u0432 \u043E\u0434\u0438\u043D \u043A\u043B\u0438\u043A.";
            dict_ru["select_all"] = "\u0412\u044B\u0431\u0440\u0430\u0442\u044C \u0432\u0441\u0435 \u043F\u0440\u0438\u043B\u043E\u0436\u0435\u043D\u0438\u044F";
            dict_ru["sort_button"] = "\u0421\u043E\u0440\u0442\u0438\u0440\u043E\u0432\u0430\u0442\u044C \u25BE";
            dict_ru["sort_selected_top"] = "\u0412\u044B\u0431\u0440\u0430\u043D\u043D\u044B\u0435 \u0438 \u0440\u0435\u043A\u043E\u043C\u0435\u043D\u0434\u0443\u0435\u043C\u044B\u0435";
            dict_ru["sort_name_az"] = "\u0418\u043C\u044F (\u0410-\u042F)";
            dict_ru["sort_name_za"] = "\u0418\u043C\u044F (\u042F-\u0410)";
            dict_ru["sort_category"] = "\u041F\u043E \u043A\u0430\u0442\u0435\u0433\u043E\u0440\u0438\u0438";
            dict_ru["cat_all"] = "\u0412\u0441\u0435";
            dict_ru["cat_browsers"] = "\u0412\u0435\u0431-\u0431\u0440\u0430\u0443\u0437\u0435\u0440\u044B";
            dict_ru["cat_dev"] = "\u0420\u0430\u0437\u0440\u0430\u0431\u043E\u0442\u043A\u0430 \u0438 \u043A\u043E\u0434";
            dict_ru["cat_collab"] = "\u0421\u0432\u044F\u0437\u044C \u0438 \u0441\u043E\u0442\u0440\u0443\u0434\u043D\u0438\u0447\u0435\u0441\u0442\u0432\u043E";
            dict_ru["cat_office"] = "\u041E\u0444\u0438\u0441 \u0438 \u043F\u0440\u043E\u0434\u0443\u043A\u0442\u0438\u0432\u043D\u043E\u0441\u0442\u044C";
            dict_ru["cat_tools"] = "\u0423\u0442\u0438\u043B\u0438\u0442\u044B \u0438 \u0438\u043D\u0441\u0442\u0440\u0443\u043C\u0435\u043D\u0442\u044B";
            dict_ru["cat_media"] = "\u041C\u0435\u0434\u0438\u0430 \u0438 \u0442\u0432\u043E\u0440\u0447\u0435\u0441\u0442\u0432\u043E";
            dict_ru["cat_sec"] = "\u0411\u0435\u0437\u043E\u043F\u0430\u0441\u043D\u043E\u0441\u0442\u044C \u0438 \u0441\u0435\u0442\u044C";
            dict_ru["cat_cloud"] = "\u041E\u0431\u043B\u0430\u043A\u043E \u0438 \u0445\u0440\u0430\u043D\u0438\u043B\u0438\u0449\u0435";
            dict_ru["cat_games"] = "\u0418\u0433\u0440\u044B \u0438 \u0434\u0432\u0438\u0436\u043A\u0438";
            dict_ru["btn_install"] = "\uD83D\uDCE5  \u0423\u0441\u0442\u0430\u043D\u043E\u0432\u0438\u0442\u044C \u0432\u044B\u0431\u0440\u0430\u043D\u043D\u044B\u0435 \u043F\u0440\u0438\u043B\u043E\u0436\u0435\u043D\u0438\u044F ({0})";
            dict_ru["btn_install_no_count"] = "\uD83D\uDCE5  \u0423\u0441\u0442\u0430\u043D\u043E\u0432\u0438\u0442\u044C \u0432\u044B\u0431\u0440\u0430\u043D\u043D\u044B\u0435 \u043F\u0440\u0438\u043B\u043E\u0436\u0435\u043D\u0438\u044F";
            dict_ru["btn_stop"] = "\u041E\u0441\u0442\u0430\u043D\u043E\u0432\u0438\u0442\u044C \u0443\u0441\u0442\u0430\u043D\u043E\u0432\u043A\u0443";
            dict_ru["btn_import_pack"] = "\uD83D\uDCE5 \u0418\u043C\u043F\u043E\u0440\u0442 \u043F\u0430\u043A\u0435\u0442\u0430";
            dict_ru["btn_create_pack"] = "+ \u0421\u043E\u0437\u0434\u0430\u0442\u044C \u043D\u043E\u0432\u044B\u0439 \u043F\u0430\u043A\u0435\u0442";
            dict_ru["badge_ready"] = "\u0413\u043E\u0442\u043E\u0432\u043E";
            dict_ru["badge_installed"] = "\u0423\u0441\u0442\u0430\u043D\u043E\u0432\u043B\u0435\u043D\u043E";
            dict_ru["badge_installing"] = "\u0423\u0441\u0442\u0430\u043D\u043E\u0432\u043A\u0430...";
            dict_ru["badge_failed"] = "\u041E\u0448\u0438\u0431\u043A\u0430";
            dict_ru["showing_count"] = "\u041E\u0442\u043E\u0431\u0440\u0430\u0436\u0435\u043D\u0438\u0435 {0:N0} \u0438\u0437 {1:N0} \u043F\u0440\u0438\u043B\u043E\u0436\u0435\u043D\u0438\u0439";
            dict_ru["showing_count_selected"] = "\u041E\u0442\u043E\u0431\u0440\u0430\u0436\u0435\u043D\u0438\u0435 {0:N0} \u0438\u0437 {1:N0} \u043F\u0440\u0438\u043B\u043E\u0436\u0435\u043D\u0438\u0439 ({2:N0} \u0432\u044B\u0431\u0440\u0430\u043D\u043E)";
            dict_ru["showing_local_online"] = "\u041D\u0430\u0439\u0434\u0435\u043D\u043E {0} \u043B\u043E\u043A\u0430\u043B\u044C\u043D\u044B\u0445 + {1} \u043E\u043D\u043B\u0430\u0439\u043D \u0434\u043B\u044F \"{2}\"";
            dict_ru["showing_local_online_selected"] = "\u041D\u0430\u0439\u0434\u0435\u043D\u043E {0} \u043B\u043E\u043A\u0430\u043B\u044C\u043D\u044B\u0445 + {1} \u043E\u043D\u043B\u0430\u0439\u043D \u0434\u043B\u044F \"{2}\" ({3:N0} \u0432\u044B\u0431\u0440\u0430\u043D\u043E)";
            dict_ru["found_online_count"] = "\u041D\u0430\u0439\u0434\u0435\u043D\u043E {0:N0} \u043E\u043D\u043B\u0430\u0439\u043D-\u043F\u0440\u0438\u043B\u043E\u0436\u0435\u043D\u0438\u0439 \u0434\u043B\u044F \"{1}\"";
            dict_ru["found_online_count_selected"] = "\u041D\u0430\u0439\u0434\u0435\u043D\u043E {0:N0} \u043E\u043D\u043B\u0430\u0439\u043D-\u043F\u0440\u0438\u043B\u043E\u0436\u0435\u043D\u0438\u0439 \u0434\u043B\u044F \"{1}\" ({2:N0} \u0432\u044B\u0431\u0440\u0430\u043D\u043E)";
            dict_ru["showing_query_count"] = "\u041F\u043E\u043A\u0430\u0437\u0430\u043D\u043E {0:N0} \u043F\u0440\u0438\u043B\u043E\u0436\u0435\u043D\u0438\u0439 \u043F\u043E \u0437\u0430\u043F\u0440\u043E\u0441\u0443 \"{1}\"";
            dict_ru["showing_query_count_selected"] = "\u041F\u043E\u043A\u0430\u0437\u0430\u043D\u043E {0:N0} \u043F\u0440\u0438\u043B\u043E\u0436\u0435\u043D\u0438\u0439 \u043F\u043E \u0437\u0430\u043F\u0440\u043E\u0441\u0443 \"{1}\" ({2:N0} \u0432\u044B\u0431\u0440\u0430\u043D\u043E)";
            dict_ru["searching_online"] = "\uD83D\uDD0D \u041F\u043E\u0438\u0441\u043A \u0432 \u043E\u043D\u043B\u0430\u0439\u043D-\u0440\u0435\u043F\u043E\u0437\u0438\u0442\u043E\u0440\u0438\u0438 Winget \u0434\u043B\u044F \"{0}\"...";
            dict_ru["no_apps_found"] = "\u041D\u0435 \u043D\u0430\u0439\u0434\u0435\u043D\u043E \u043F\u0440\u043E\u0433\u0440\u0430\u043C\u043C\u043D\u044B\u0445 \u043F\u0430\u043A\u0435\u0442\u043E\u0432 \u0434\u043B\u044F \"{0}\"";
            dict_ru["no_apps_found_desc"] = "\u041D\u0435 \u043D\u0430\u0439\u0434\u0435\u043D\u043E \u0432 \u0431\u0430\u0437\u0435 \u0438\u043B\u0438 \u043E\u043D\u043B\u0430\u0439\u043D Winget. \u041F\u0440\u043E\u0432\u0435\u0440\u044C\u0442\u0435 \u043F\u0440\u0430\u0432\u0438\u043B\u044C\u043D\u043E\u0441\u0442\u044C \u043D\u0430\u043F\u0438\u0441\u0430\u043D\u0438\u044F.";
            dict_ru["type_to_search"] = "\u0412\u0432\u0435\u0434\u0438\u0442\u0435 \u043D\u0435 \u043C\u0435\u043D\u0435\u0435 2 \u0441\u0438\u043C\u0432\u043E\u043B\u043E\u0432 \u0434\u043B\u044F \u043F\u043E\u0438\u0441\u043A\u0430 \u043F\u0440\u0438\u043B\u043E\u0436\u0435\u043D\u0438\u0439.";
            dict_ru["no_winget_title"] = "\u041D\u0435 \u043D\u0430\u0439\u0434\u0435\u043D\u043E \u043B\u043E\u043A\u0430\u043B\u044C\u043D\u044B\u0445 \u043F\u0440\u0438\u043B\u043E\u0436\u0435\u043D\u0438\u0439 \u0434\u043B\u044F \"{0}\"";
            dict_ru["no_winget_desc"] = "Microsoft Winget \u043D\u0435\u0434\u043E\u0441\u0442\u0443\u043F\u0435\u043D \u0434\u043B\u044F \u043F\u043E\u0438\u0441\u043A\u0430 \u0432 \u043E\u043D\u043B\u0430\u0439\u043D-\u0440\u0435\u043F\u043E\u0437\u0438\u0442\u043E\u0440\u0438\u0438.";
            dict_ru["bottom_tip"] = "\u0421\u043E\u0432\u0435\u0442: \u0412\u044B \u043C\u043E\u0436\u0435\u0442\u0435 \u043D\u0430\u0441\u0442\u0440\u043E\u0438\u0442\u044C \u043B\u044E\u0431\u043E\u0439 \u043F\u0430\u043A\u0435\u0442, \u0434\u043E\u0431\u0430\u0432\u043B\u044F\u0442\u044C \u0438\u043B\u0438 \u0443\u0434\u0430\u043B\u044F\u0442\u044C \u043F\u0440\u0438\u043B\u043E\u0436\u0435\u043D\u0438\u044F.";
            dict_ru["pack_selected_prefix"] = "\u0412\u044B\u0431\u0440\u0430\u043D\u043D\u044B\u0439 \u043F\u0430\u043A\u0435\u0442: {0}";
            dict_ru["logs_title"] = "\u0418\u0441\u0442\u043E\u0440\u0438\u044F \u0443\u0441\u0442\u0430\u043D\u043E\u0432\u043A\u0438";
            dict_ru["logs_subtitle"] = "\u041C\u043E\u043D\u0438\u0442\u043E\u0440\u0438\u043D\u0433 \u0440\u0430\u0437\u0432\u0435\u0440\u0442\u044B\u0432\u0430\u043D\u0438\u044F \u0432 \u0440\u0435\u0430\u043B\u044C\u043D\u043E\u043C \u0432\u0440\u0435\u043C\u0435\u043D\u0438 \u0438 \u0436\u0443\u0440\u043D\u0430\u043B\u044B \u0430\u0443\u0434\u0438\u0442\u0430.";
            dict_ru["btn_summary_report"] = "\u0421\u0432\u043E\u0434\u043D\u044B\u0439 \u043E\u0442\u0447\u0435\u0442";
            dict_ru["btn_open_logfile"] = "\u041E\u0442\u043A\u0440\u044B\u0442\u044C \u0444\u0430\u0439\u043B \u0436\u0443\u0440\u043D\u0430\u043B\u0430";
            dict_ru["btn_clear_screen"] = "\u041E\u0447\u0438\u0441\u0442\u0438\u0442\u044C \u044D\u043A\u0440\u0430\u043D";
            _translations["ru"] = dict_ru;

            // Language: zh
            Dictionary<string, string> dict_zh = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            dict_zh["btn_select_pack"] = "\u9009\u62E9\u8F6F\u4EF6\u5305";
            dict_zh["btn_customize_pack"] = "\u81EA\u5B9A\u4E49\u5E94\u7528";
            dict_zh["btn_export_pack"] = "\u5BFC\u51FA\u8F6F\u4EF6\u5305";
            dict_zh["nav_quick_setup"] = "\u5FEB\u901F\u8BBE\u7F6E";
            dict_zh["nav_all_apps"] = "\u6240\u6709\u5E94\u7528";
            dict_zh["nav_cloud_sync"] = "\u4E91\u540C\u6B65";
            dict_zh["nav_history"] = "\u5B89\u88C5\u5386\u53F2";
            dict_zh["search_placeholder"] = "\u641C\u7D22\u5E94\u7528\u3001\u8BBE\u7F6E\u6216\u8F6F\u4EF6\u5305...";
            dict_zh["help_button"] = "\u5E2E\u52A9 ?";
            dict_zh["lang_auto"] = "\u81EA\u52A8 (\u7CFB\u7EDF\u9ED8\u8BA4)";
            dict_zh["menu_guide"] = "\u4F7F\u7528\u6307\u5357 (\u4EA4\u4E92\u5F0F\u6559\u7A0B)...";
            dict_zh["menu_updates"] = "\u68C0\u67E5\u66F4\u65B0...";
            dict_zh["menu_settings"] = "\u8BBE\u7F6E...";
            dict_zh["menu_about"] = "\u5173\u4E8E FleetForge...";
            dict_zh["quick_setup_title"] = "\u5FEB\u901F\u8BBE\u7F6E";
            dict_zh["quick_setup_subtitle"] = "\u9009\u62E9\u4E00\u4E2A\u8F6F\u4EF6\u5305\uFF0C\u6311\u9009\u5E94\u7528\uFF0C\u4E00\u952E\u5F00\u542F\u3002";
            dict_zh["step1_title"] = "\u9009\u62E9\u8F6F\u4EF6\u5305";
            dict_zh["step1_desc"] = "\u6839\u636E\u60A8\u7684\u9700\u6C42";
            dict_zh["step2_title"] = "\u5BA1\u67E5\u5E94\u7528";
            dict_zh["step2_desc"] = "\u68C0\u67E5\u5DF2\u9009\u9879\u76EE";
            dict_zh["step3_title"] = "\u5B89\u88C5";
            dict_zh["step3_desc"] = "\u5F00\u59CB\u4F7F\u7528";
            dict_zh["available_packs_title"] = "\u53EF\u7528\u5E94\u7528\u5305";
            dict_zh["available_packs_subtitle"] = "\u9009\u62E9\u73B0\u6210\u8F6F\u4EF6\u5305\u6216\u81EA\u7531\u6311\u9009\u72EC\u7ACB\u5E94\u7528\u3002";
            dict_zh["top_choice_badge"] = "\u6700\u4F73\u9009\u62E9";
            dict_zh["recommended_badge"] = "\u63A8\u8350";
            dict_zh["custom_badge"] = "\u81EA\u5B9A\u4E49\u5E94\u7528\u5305";
            dict_zh["partner_apps_count"] = "{0} \u4E2A\u5408\u4F5C\u4F19\u4F34\u5E94\u7528";
            dict_zh["apps_count"] = "{0} \u6B3E\u5E94\u7528";
            dict_zh["catalog_title"] = "\u6240\u6709\u5E94\u7528\u7A0B\u5E8F";
            dict_zh["catalog_subtitle"] = "\u6D4F\u89C8 16,349 \u6B3E\u8F6F\u4EF6\u5305\uFF0C\u652F\u6301\u5373\u65F6\u641C\u7D22\u4E0E\u4E00\u952E\u9009\u53D6\u3002";
            dict_zh["select_all"] = "\u9009\u62E9\u6240\u6709\u5E94\u7528";
            dict_zh["sort_button"] = "\u5E94\u7528\u6392\u5E8F \u25BE";
            dict_zh["sort_selected_top"] = "\u5DF2\u9009\u4E0E\u63A8\u8350";
            dict_zh["sort_name_az"] = "\u540D\u79F0 (A \u5230 Z)";
            dict_zh["sort_name_za"] = "\u540D\u79F0 (Z \u5230 A)";
            dict_zh["sort_category"] = "\u6309\u7C7B\u522B";
            dict_zh["cat_all"] = "\u5168\u90E8";
            dict_zh["cat_browsers"] = "\u7F51\u9875\u6D4F\u89C8\u5668";
            dict_zh["cat_dev"] = "\u5F00\u53D1\u4E0E\u7F16\u7A0B";
            dict_zh["cat_collab"] = "\u901A\u8BAF\u4E0E\u534F\u4F5C";
            dict_zh["cat_office"] = "\u751F\u4EA7\u529B\u4E0E\u529E\u516C";
            dict_zh["cat_tools"] = "\u5B9E\u7528\u5DE5\u5177";
            dict_zh["cat_media"] = "\u5A92\u4F53\u4E0E\u521B\u4F5C";
            dict_zh["cat_sec"] = "\u5B89\u5168\u4E0E\u7F51\u7EDC";
            dict_zh["cat_cloud"] = "\u4E91\u7AEF\u4E0E\u5B58\u50A8";
            dict_zh["cat_games"] = "\u6E38\u620F\u4E0E\u5F15\u64CE";
            dict_zh["btn_install"] = "\uD83D\uDCE5  \u5B89\u88C5\u5DF2\u9009\u5E94\u7528 ({0})";
            dict_zh["btn_install_no_count"] = "\uD83D\uDCE5  \u5B89\u88C5\u5DF2\u9009\u5E94\u7528";
            dict_zh["btn_stop"] = "\u505C\u6B62\u5B89\u88C5";
            dict_zh["btn_import_pack"] = "\uD83D\uDCE5 \u5BFC\u5165\u8F6F\u4EF6\u5305";
            dict_zh["btn_create_pack"] = "+ \u521B\u5EFA\u65B0\u8F6F\u4EF6\u5305";
            dict_zh["badge_ready"] = "\u5C31\u7EEA";
            dict_zh["badge_installed"] = "\u5DF2\u5B89\u88C5";
            dict_zh["badge_installing"] = "\u6B63\u5728\u5B89\u88C5...";
            dict_zh["badge_failed"] = "\u5931\u8D25";
            dict_zh["showing_count"] = "\u663E\u793A {0:N0} / {1:N0} \u6B3E\u5E94\u7528";
            dict_zh["showing_count_selected"] = "\u663E\u793A {0:N0} / {1:N0} \u6B3E\u5E94\u7528 ({2:N0} \u4E2A\u5DF2\u9009)";
            dict_zh["showing_local_online"] = "\u4E3A\u201C{2}\u201D\u663E\u793A {0} \u4E2A\u672C\u5730 + {1} \u4E2A\u5728\u7EBF\u5E94\u7528";
            dict_zh["showing_local_online_selected"] = "\u4E3A\u201C{2}\u201D\u663E\u793A {0} \u4E2A\u672C\u5730 + {1} \u4E2A\u5728\u7EBF\u5E94\u7528 ({3:N0} \u4E2A\u5DF2\u9009)";
            dict_zh["found_online_count"] = "\u627E\u5230 {0:N0} \u4E2A\u9488\u5BF9\u201C{1}\u201D\u7684\u5728\u7EBF\u5E94\u7528";
            dict_zh["found_online_count_selected"] = "\u627E\u5230 {0:N0} \u4E2A\u9488\u5BF9\u201C{1}\u201D\u7684\u5728\u7EBF\u5E94\u7528 ({2:N0} \u4E2A\u5DF2\u9009)";
            dict_zh["showing_query_count"] = "\u663E\u793A {0:N0} \u6B3E\u7B26\u5408\u201C{1}\u201D\u7684\u5E94\u7528";
            dict_zh["showing_query_count_selected"] = "\u663E\u793A {0:N0} \u6B3E\u7B26\u5408\u201C{1}\u201D\u7684\u5E94\u7528 ({2:N0} \u4E2A\u5DF2\u9009)";
            dict_zh["searching_online"] = "\uD83D\uDD0D \u6B63\u5728\u5728\u7EBF Winget \u8F6F\u4EF6\u5E93\u4E2D\u641C\u7D22\u201C{0}\u201D...";
            dict_zh["no_apps_found"] = "\u672A\u627E\u5230\u4E0E\u201C{0}\u201D\u76F8\u5173\u7684\u8F6F\u4EF6\u5305";
            dict_zh["no_apps_found_desc"] = "\u672C\u5730\u6570\u636E\u5E93\u6216\u5728\u7EBF Winget \u5747\u672A\u627E\u5230\u3002\u8BF7\u68C0\u67E5\u62FC\u5199\u6216\u5C1D\u8BD5\u53D1\u5E03\u8005\u540D\u79F0\u3002";
            dict_zh["type_to_search"] = "\u8BF7\u81F3\u5C11\u8F93\u5165 2 \u4E2A\u5B57\u7B26\u4EE5\u641C\u7D22\u5E94\u7528\u3002";
            dict_zh["no_winget_title"] = "\u672A\u627E\u5230\u672C\u5730\u5E94\u7528\u201C{0}\u201D";
            dict_zh["no_winget_desc"] = "\u6B64\u7CFB\u7EDF\u672A\u68C0\u6D4B\u5230 Microsoft Winget\uFF0C\u65E0\u6CD5\u6267\u884C\u5728\u7EBF\u641C\u7D22\u3002";
            dict_zh["bottom_tip"] = "\u63D0\u793A\uFF1A\u60A8\u53EF\u4EE5\u81EA\u5B9A\u4E49\u4EFB\u4F55\u8F6F\u4EF6\u5305\uFF0C\u6DFB\u52A0\u6216\u5220\u9664\u5E94\u7528\u5E76\u5BFC\u51FA\u5907\u4EFD\u3002";
            dict_zh["pack_selected_prefix"] = "\u5DF2\u9009\u5E94\u7528\u5305\uFF1A{0}";
            dict_zh["logs_title"] = "\u5B89\u88C5\u5386\u53F2";
            dict_zh["logs_subtitle"] = "\u5B9E\u65F6\u90E8\u7F72\u76D1\u63A7\u3001\u8F93\u51FA\u6D41\u4E0E\u5BA1\u8BA1\u65E5\u5FD7\u3002";
            dict_zh["btn_summary_report"] = "\u603B\u7ED3\u62A5\u544A";
            dict_zh["btn_open_logfile"] = "\u6253\u5F00\u65E5\u5FD7\u6587\u4EF6";
            dict_zh["btn_clear_screen"] = "\u6E05\u9664\u5C4F\u5E55";
            _translations["zh"] = dict_zh;

            // Language: ja
            Dictionary<string, string> dict_ja = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            dict_ja["btn_select_pack"] = "\u30D1\u30C3\u30AF\u3092\u9078\u629E";
            dict_ja["btn_customize_pack"] = "\u30A2\u30D7\u30EA\u3092\u30AB\u30B9\u30BF\u30DE\u30A4\u30BA";
            dict_ja["btn_export_pack"] = "\u30A8\u30AF\u30B9\u30DD\u30FC\u30C8";
            dict_ja["nav_quick_setup"] = "\u30AF\u30A4\u30C3\u30AF\u30BB\u30C3\u30C8\u30A2\u30C3\u30D7";
            dict_ja["nav_all_apps"] = "\u3059\u3079\u3066\u306E\u30A2\u30D7\u30EA";
            dict_ja["nav_cloud_sync"] = "\u30AF\u30E9\u30A6\u30C9\u540C\u671F";
            dict_ja["nav_history"] = "\u30A4\u30F3\u30B9\u30C8\u30FC\u30EB\u5C65\u6B74";
            dict_ja["search_placeholder"] = "\u30A2\u30D7\u30EA\u3001\u8A2D\u5B9A\u3001\u30D1\u30C3\u30AF\u3092\u691C\u7D22...";
            dict_ja["help_button"] = "\u30D8\u30EB\u30D7 ?";
            dict_ja["lang_auto"] = "\u81EA\u52D5 (\u30B7\u30B9\u30C6\u30E0\u65E2\u5B9A)";
            dict_ja["menu_guide"] = "\u4F7F\u3044\u65B9 (\u30A4\u30F3\u30BF\u30E9\u30AF\u30C6\u30A3\u30D6\u30AC\u30A4\u30C9)...";
            dict_ja["menu_updates"] = "\u66F4\u65B0\u3092\u78BA\u8A8D...";
            dict_ja["menu_settings"] = "\u8A2D\u5B9A...";
            dict_ja["menu_about"] = "FleetForge \u306B\u3064\u3044\u3066...";
            dict_ja["quick_setup_title"] = "\u30AF\u30A4\u30C3\u30AF\u30BB\u30C3\u30C8\u30A2\u30C3\u30D7";
            dict_ja["quick_setup_subtitle"] = "\u30D1\u30C3\u30AF\u3092\u9078\u629E\u3057\u3001\u30A2\u30D7\u30EA\u3092\u9078\u3093\u3067\u30EF\u30F3\u30AF\u30EA\u30C3\u30AF\u3067\u958B\u59CB\u3002";
            dict_ja["step1_title"] = "\u30D1\u30C3\u30AF\u3092\u9078\u629E";
            dict_ja["step1_desc"] = "\u30CB\u30FC\u30BA\u306B\u5408\u308F\u305B\u3066";
            dict_ja["step2_title"] = "\u30A2\u30D7\u30EA\u306E\u78BA\u8A8D";
            dict_ja["step2_desc"] = "\u9078\u629E\u5185\u5BB9\u3092\u78BA\u8A8D";
            dict_ja["step3_title"] = "\u30A4\u30F3\u30B9\u30C8\u30FC\u30EB";
            dict_ja["step3_desc"] = "\u5229\u7528\u958B\u59CB";
            dict_ja["available_packs_title"] = "\u5229\u7528\u53EF\u80FD\u306A\u30A2\u30D7\u30EA\u30D1\u30C3\u30AF";
            dict_ja["available_packs_subtitle"] = "\u3059\u3050\u306B\u4F7F\u3048\u308B\u30D1\u30C3\u30AF\u3092\u9078\u3076\u304B\u3001\u500B\u5225\u306E\u30A2\u30D7\u30EA\u3092\u9078\u629E\u3002";
            dict_ja["top_choice_badge"] = "\u304A\u3059\u3059\u3081";
            dict_ja["recommended_badge"] = "\u63A8\u5968";
            dict_ja["custom_badge"] = "\u30AB\u30B9\u30BF\u30E0\u30D1\u30C3\u30AF";
            dict_ja["partner_apps_count"] = "{0} \u500B\u306E\u30D1\u30FC\u30C8\u30CA\u30FC\u30A2\u30D7\u30EA";
            dict_ja["apps_count"] = "{0} \u500B\u306E\u30A2\u30D7\u30EA";
            dict_ja["catalog_title"] = "\u3059\u3079\u3066\u306E\u30A2\u30D7\u30EA\u30B1\u30FC\u30B7\u30E7\u30F3";
            dict_ja["catalog_subtitle"] = "\u30A4\u30F3\u30B9\u30BF\u30F3\u30C8\u691C\u7D22\u3068\u30EF\u30F3\u30AF\u30EA\u30C3\u30AF\u9078\u629E\u3067 16,349 \u500B\u306E\u30D1\u30C3\u30B1\u30FC\u30B8\u3092\u95B2\u89A7\u3002";
            dict_ja["select_all"] = "\u3059\u3079\u3066\u306E\u30A2\u30D7\u30EA\u3092\u9078\u629E";
            dict_ja["sort_button"] = "\u30A2\u30D7\u30EA\u4E26\u3079\u66FF\u3048 \u25BE";
            dict_ja["sort_selected_top"] = "\u9078\u629E\u4E2D\uFF06\u304A\u3059\u3059\u3081";
            dict_ja["sort_name_az"] = "\u540D\u524D (A\u301CZ)";
            dict_ja["sort_name_za"] = "\u540D\u524D (Z\u301CA)";
            dict_ja["sort_category"] = "\u30AB\u30C6\u30B4\u30EA\u5225";
            dict_ja["cat_all"] = "\u3059\u3079\u3066";
            dict_ja["cat_browsers"] = "\u30A6\u30A7\u30D6\u30D6\u30E9\u30A6\u30B6";
            dict_ja["cat_dev"] = "\u958B\u767A\u3068\u30B3\u30FC\u30C9";
            dict_ja["cat_collab"] = "\u30B3\u30DF\u30E5\u30CB\u30B1\u30FC\u30B7\u30E7\u30F3";
            dict_ja["cat_office"] = "\u751F\u7523\u6027\u3068\u30AA\u30D5\u30A3\u30B9";
            dict_ja["cat_tools"] = "\u30E6\u30FC\u30C6\u30A3\u30EA\u30C6\u30A3\u3068\u30C4\u30FC\u30EB";
            dict_ja["cat_media"] = "\u30E1\u30C7\u30A3\u30A2\u3068\u30AF\u30EA\u30A8\u30A4\u30C6\u30A3\u30D6";
            dict_ja["cat_sec"] = "\u30BB\u30AD\u30E5\u30EA\u30C6\u30A3\u3068\u30CD\u30C3\u30C8\u30EF\u30FC\u30AF";
            dict_ja["cat_cloud"] = "\u30AF\u30E9\u30A6\u30C9\u3068\u30B9\u30C8\u30EC\u30FC\u30B8";
            dict_ja["cat_games"] = "\u30B2\u30FC\u30E0\u3068\u30A8\u30F3\u30B8\u30F3";
            dict_ja["btn_install"] = "\uD83D\uDCE5  \u9078\u629E\u3057\u305F\u30A2\u30D7\u30EA\u3092\u30A4\u30F3\u30B9\u30C8\u30FC\u30EB ({0})";
            dict_ja["btn_install_no_count"] = "\uD83D\uDCE5  \u9078\u629E\u3057\u305F\u30A2\u30D7\u30EA\u3092\u30A4\u30F3\u30B9\u30C8\u30FC\u30EB";
            dict_ja["btn_stop"] = "\u30A4\u30F3\u30B9\u30C8\u30FC\u30EB\u505C\u6B62";
            dict_ja["btn_import_pack"] = "\uD83D\uDCE5 \u30D1\u30C3\u30AF\u3092\u30A4\u30F3\u30DD\u30FC\u30C8";
            dict_ja["btn_create_pack"] = "+ \u65B0\u898F\u30D1\u30C3\u30AF\u4F5C\u6210";
            dict_ja["badge_ready"] = "\u6E96\u5099\u5B8C\u4E86";
            dict_ja["badge_installed"] = "\u30A4\u30F3\u30B9\u30C8\u30FC\u30EB\u6E08\u307F";
            dict_ja["badge_installing"] = "\u30A4\u30F3\u30B9\u30C8\u30FC\u30EB\u4E2D...";
            dict_ja["badge_failed"] = "\u5931\u6557";
            dict_ja["showing_count"] = "{1:N0} \u4EF6\u4E2D {0:N0} \u4EF6\u306E\u30A2\u30D7\u30EA\u3092\u8868\u793A";
            dict_ja["showing_count_selected"] = "{1:N0} \u4EF6\u4E2D {0:N0} \u4EF6\u3092\u8868\u793A ({2:N0} \u500B\u9078\u629E\u4E2D)";
            dict_ja["showing_local_online"] = "\u300C{2}\u300D\u306E {0} \u500B\u306E\u30ED\u30FC\u30AB\u30EB + {1} \u500B\u306E\u30AA\u30F3\u30E9\u30A4\u30F3 \u30A2\u30D7\u30EA";
            dict_ja["showing_local_online_selected"] = "\u300C{2}\u300D\u306E {0} \u500B\u306E\u30ED\u30FC\u30AB\u30EB + {1} \u500B\u306E\u30AA\u30F3\u30E9\u30A4\u30F3 \u30A2\u30D7\u30EA ({3:N0} \u500B\u9078\u629E\u4E2D)";
            dict_ja["found_online_count"] = "\u300C{1}\u300D\u306E\u30AA\u30F3\u30E9\u30A4\u30F3 \u30A2\u30D7\u30EA\u304C {0:N0} \u4EF6\u898B\u3064\u304B\u308A\u307E\u3057\u305F";
            dict_ja["found_online_count_selected"] = "\u300C{1}\u300D\u306E\u30AA\u30F3\u30E9\u30A4\u30F3 \u30A2\u30D7\u30EA\u304C {0:N0} \u4EF6\u898B\u3064\u304B\u308A\u307E\u3057\u305F ({2:N0} \u500B\u9078\u629E\u4E2D)";
            dict_ja["showing_query_count"] = "\u300C{1}\u300D\u306B\u4E00\u81F4\u3059\u308B {0:N0} \u4EF6\u306E\u30A2\u30D7\u30EA";
            dict_ja["showing_query_count_selected"] = "\u300C{1}\u300D\u306B\u4E00\u81F4\u3059\u308B {0:N0} \u4EF6\u306E\u30A2\u30D7\u30EA ({2:N0} \u500B\u9078\u629E\u4E2D)";
            dict_ja["searching_online"] = "\uD83D\uDD0D \u30AA\u30F3\u30E9\u30A4\u30F3 Winget \u30EA\u30DD\u30B8\u30C8\u30EA\u3067\u300C{0}\u300D\u3092\u691C\u7D22\u4E2D...";
            dict_ja["no_apps_found"] = "\u300C{0}\u300D\u306B\u4E00\u81F4\u3059\u308B\u30BD\u30D5\u30C8\u30A6\u30A7\u30A2 \u30D1\u30C3\u30B1\u30FC\u30B8\u304C\u898B\u3064\u304B\u308A\u307E\u305B\u3093";
            dict_ja["no_apps_found_desc"] = "\u30ED\u30FC\u30AB\u30EB \u30C7\u30FC\u30BF\u30D9\u30FC\u30B9\u304A\u3088\u3073\u30AA\u30F3\u30E9\u30A4\u30F3 Winget \u306B\u898B\u3064\u304B\u308A\u307E\u305B\u3093\u3067\u3057\u305F\u3002\u7DB4\u308A\u3092\u78BA\u8A8D\u3057\u3066\u304F\u3060\u3055\u3044\u3002";
            dict_ja["type_to_search"] = "\u691C\u7D22\u3059\u308B\u306B\u306F\u5C11\u306A\u304F\u3068\u3082 2 \u6587\u5B57\u5165\u529B\u3057\u3066\u304F\u3060\u3055\u3044\u3002";
            dict_ja["no_winget_title"] = "\u300C{0}\u300D\u306E\u30ED\u30FC\u30AB\u30EB \u30A2\u30D7\u30EA\u304C\u898B\u3064\u304B\u308A\u307E\u305B\u3093";
            dict_ja["no_winget_desc"] = "\u30AA\u30F3\u30E9\u30A4\u30F3 \u30EA\u30DD\u30B8\u30C8\u30EA\u3092\u691C\u7D22\u3059\u308B\u305F\u3081\u306E Microsoft Winget \u304C\u5229\u7528\u3067\u304D\u307E\u305B\u3093\u3002";
            dict_ja["bottom_tip"] = "\u30D2\u30F3\u30C8: \u4EFB\u610F\u306E\u30D1\u30C3\u30AF\u3092\u30AB\u30B9\u30BF\u30DE\u30A4\u30BA\u3057\u3001\u30A2\u30D7\u30EA\u3092\u8FFD\u52A0\u307E\u305F\u306F\u524A\u9664\u3067\u304D\u307E\u3059\u3002";
            dict_ja["pack_selected_prefix"] = "\u9078\u629E\u3055\u308C\u305F\u30A2\u30D7\u30EA\u30D1\u30C3\u30AF: {0}";
            dict_ja["logs_title"] = "\u30A4\u30F3\u30B9\u30C8\u30FC\u30EB\u5C65\u6B74";
            dict_ja["logs_subtitle"] = "\u30EA\u30A2\u30EB\u30BF\u30A4\u30E0\u306E\u30C7\u30D7\u30ED\u30A4\u76E3\u8996\u3068\u76E3\u67FB\u30ED\u30B0\u3002";
            dict_ja["btn_summary_report"] = "\u30B5\u30DE\u30EA\u30FC\u30EC\u30DD\u30FC\u30C8";
            dict_ja["btn_open_logfile"] = "\u30ED\u30B0\u30D5\u30A1\u30A4\u30EB\u3092\u958B\u304F";
            dict_ja["btn_clear_screen"] = "\u753B\u9762\u3092\u30AF\u30EA\u30A2";
            _translations["ja"] = dict_ja;

            // Language: pt
            Dictionary<string, string> dict_pt = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            dict_pt["btn_select_pack"] = "Selecionar pacote";
            dict_pt["btn_customize_pack"] = "Personalizar";
            dict_pt["btn_export_pack"] = "Exportar";
            dict_pt["nav_quick_setup"] = "Configura\u00E7\u00E3o r\u00E1pida";
            dict_pt["nav_all_apps"] = "Todos os aplicativos";
            dict_pt["nav_cloud_sync"] = "Sincroniza\u00E7\u00E3o na nuvem";
            dict_pt["nav_history"] = "Hist\u00F3rico de instala\u00E7\u00E3o";
            dict_pt["search_placeholder"] = "Buscar aplicativo, configura\u00E7\u00E3o ou pacote...";
            dict_pt["help_button"] = "Ajuda ?";
            dict_pt["lang_auto"] = "Autom\u00E1tico (Padr\u00E3o)";
            dict_pt["menu_guide"] = "Como usar (Guia interativo)...";
            dict_pt["menu_updates"] = "Verificar atualiza\u00E7\u00F5es...";
            dict_pt["menu_settings"] = "Configura\u00E7\u00F5es...";
            dict_pt["menu_about"] = "Sobre o FleetForge...";
            dict_pt["quick_setup_title"] = "Configura\u00E7\u00E3o r\u00E1pida";
            dict_pt["quick_setup_subtitle"] = "Escolha um pacote, selecione aplicativos e comece em um clique.";
            dict_pt["step1_title"] = "Escolha um pacote";
            dict_pt["step1_desc"] = "De acordo com suas necessidades";
            dict_pt["step2_title"] = "Revisar aplicativos";
            dict_pt["step2_desc"] = "Verifique a sele\u00E7\u00E3o";
            dict_pt["step3_title"] = "Instalar";
            dict_pt["step3_desc"] = "Comece a usar";
            dict_pt["available_packs_title"] = "Pacotes de aplicativos dispon\u00EDveis";
            dict_pt["available_packs_subtitle"] = "Selecione um pacote pronto ou escolha aplicativos individuais.";
            dict_pt["top_choice_badge"] = "MELHOR ESCOLHA";
            dict_pt["recommended_badge"] = "RECOMENDADO";
            dict_pt["custom_badge"] = "PACOTE PERSONALIZADO";
            dict_pt["partner_apps_count"] = "{0} apps parceiros";
            dict_pt["apps_count"] = "{0} aplicativos";
            dict_pt["catalog_title"] = "Todos os aplicativos";
            dict_pt["catalog_subtitle"] = "Explore 16.349 pacotes de software com busca instant\u00E2nea e sele\u00E7\u00E3o com um clique.";
            dict_pt["select_all"] = "Selecionar todos os aplicativos";
            dict_pt["sort_button"] = "Ordenar aplicativos \u25BE";
            dict_pt["sort_selected_top"] = "Selecionados e recomendados";
            dict_pt["sort_name_az"] = "Nome (A a Z)";
            dict_pt["sort_name_za"] = "Nome (Z a A)";
            dict_pt["sort_category"] = "Por categoria";
            dict_pt["cat_all"] = "Todos";
            dict_pt["cat_browsers"] = "Navegadores web";
            dict_pt["cat_dev"] = "Desenvolvimento e c\u00F3digo";
            dict_pt["cat_collab"] = "Comunica\u00E7\u00E3o e colabora\u00E7\u00E3o";
            dict_pt["cat_office"] = "Produtividade e escrit\u00F3rio";
            dict_pt["cat_tools"] = "Utilit\u00E1rios e ferramentas";
            dict_pt["cat_media"] = "M\u00EDdia e criatividade";
            dict_pt["cat_sec"] = "Seguran\u00E7a e rede";
            dict_pt["cat_cloud"] = "Nuvem e armazenamento";
            dict_pt["cat_games"] = "Jogos e motores";
            dict_pt["btn_install"] = "\uD83D\uDCE5  Instalar aplicativos selecionados ({0})";
            dict_pt["btn_install_no_count"] = "\uD83D\uDCE5  Instalar aplicativos selecionados";
            dict_pt["btn_stop"] = "Parar instala\u00E7\u00E3o";
            dict_pt["btn_import_pack"] = "\uD83D\uDCE5 Importar pacote";
            dict_pt["btn_create_pack"] = "+ Criar novo pacote";
            dict_pt["badge_ready"] = "Pronto";
            dict_pt["badge_installed"] = "Instalado";
            dict_pt["badge_installing"] = "Instalando...";
            dict_pt["badge_failed"] = "Falhou";
            dict_pt["showing_count"] = "Mostrando {0:N0} de {1:N0} apps";
            dict_pt["showing_count_selected"] = "Mostrando {0:N0} de {1:N0} apps ({2:N0} selecionados)";
            dict_pt["showing_local_online"] = "Mostrando {0} locais + {1} online para \"{2}\"";
            dict_pt["showing_local_online_selected"] = "Mostrando {0} locais + {1} online para \"{2}\" ({3:N0} selecionados)";
            dict_pt["found_online_count"] = "Encontrados {0:N0} aplicativos online para \"{1}\"";
            dict_pt["found_online_count_selected"] = "Encontrados {0:N0} aplicativos online para \"{1}\" ({2:N0} selecionados)";
            dict_pt["showing_query_count"] = "Mostrando {0:N0} aplicativos para \"{1}\"";
            dict_pt["showing_query_count_selected"] = "Mostrando {0:N0} aplicativos para \"{1}\" ({2:N0} selecionados)";
            dict_pt["searching_online"] = "\uD83D\uDD0D Buscando no reposit\u00F3rio Winget online por \"{0}\"...";
            dict_pt["no_apps_found"] = "Nenhum pacote de software encontrado para \"{0}\"";
            dict_pt["no_apps_found_desc"] = "N\u00E3o encontrado localmente nem no Winget online. Verifique a ortografia.";
            dict_pt["type_to_search"] = "Digite pelo menos 2 caracteres para pesquisar.";
            dict_pt["no_winget_title"] = "Nenhum aplicativo local encontrado para \"{0}\"";
            dict_pt["no_winget_desc"] = "O Microsoft Winget n\u00E3o est\u00E1 dispon\u00EDvel para busca online.";
            dict_pt["bottom_tip"] = "Dica: Voc\u00EA pode personalizar qualquer pacote, adicionar ou remover aplicativos.";
            dict_pt["pack_selected_prefix"] = "Pacote de aplicativos selecionado: {0}";
            dict_pt["logs_title"] = "Hist\u00F3rico de instala\u00E7\u00E3o";
            dict_pt["logs_subtitle"] = "Monitoramento de implanta\u00E7\u00E3o ao vivo e logs de auditoria.";
            dict_pt["btn_summary_report"] = "Relat\u00F3rio resumido";
            dict_pt["btn_open_logfile"] = "Abrir arquivo de log";
            dict_pt["btn_clear_screen"] = "Limpar tela";
            _translations["pt"] = dict_pt;

            // Language: ar
            Dictionary<string, string> dict_ar = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            dict_ar["btn_select_pack"] = "\u0627\u062E\u062A\u064A\u0627\u0631 \u0627\u0644\u062D\u0632\u0645\u0629";
            dict_ar["btn_customize_pack"] = "\u062A\u062E\u0635\u064A\u0635 \u0627\u0644\u062A\u0637\u0628\u064A\u0642\u0627\u062A";
            dict_ar["btn_export_pack"] = "\u062A\u0635\u062F\u064A\u0631 \u0627\u0644\u062D\u0632\u0645\u0629";
            dict_ar["nav_quick_setup"] = "\u0625\u0639\u062F\u0627\u062F \u0633\u0631\u064A\u0639";
            dict_ar["nav_all_apps"] = "\u062C\u0645\u064A\u0639 \u0627\u0644\u062A\u0637\u0628\u064A\u0642\u0627\u062A";
            dict_ar["nav_cloud_sync"] = "\u0645\u0632\u0627\u0645\u0646\u0629 \u0633\u062D\u0627\u0628\u064A\u0629";
            dict_ar["nav_history"] = "\u0633\u062C\u0644 \u0627\u0644\u062A\u062B\u0628\u064A\u062A";
            dict_ar["search_placeholder"] = "\u0627\u0628\u062D\u062B \u0639\u0646 \u062A\u0637\u0628\u064A\u0642 \u0623\u0648 \u0625\u0639\u062F\u0627\u062F \u0623\u0648 \u062D\u0632\u0645\u0629...";
            dict_ar["help_button"] = "\u0645\u0633\u0627\u0639\u062F\u0629 \u061F";
            dict_ar["lang_auto"] = "\u062A\u0644\u0642\u0627\u0626\u064A (\u0627\u0641\u062A\u0631\u0627\u0636\u064A \u0644\u0644\u0646\u0638\u0627\u0645)";
            dict_ar["menu_guide"] = "\u0643\u064A\u0641\u064A\u0629 \u0627\u0644\u0627\u0633\u062A\u062E\u062F\u0627\u0645 (\u062F\u0644\u064A\u0644)...";
            dict_ar["menu_updates"] = "\u0627\u0644\u062A\u062D\u0642\u0642 \u0645\u0646 \u0648\u062C\u0648\u062F \u062A\u062D\u062F\u064A\u062B\u0627\u062A...";
            dict_ar["menu_settings"] = "\u0627\u0644\u0625\u0639\u062F\u0627\u062F\u0627\u062A...";
            dict_ar["menu_about"] = "\u062D\u0648\u0644 FleetForge...";
            dict_ar["quick_setup_title"] = "\u0625\u0639\u062F\u0627\u062F \u0633\u0631\u064A\u0639";
            dict_ar["quick_setup_subtitle"] = "\u0627\u062E\u062A\u0631 \u062D\u0632\u0645\u0629\u060C \u0648\u062D\u062F\u062F \u0627\u0644\u062A\u0637\u0628\u064A\u0642\u0627\u062A \u0648\u0627\u0628\u062F\u0623 \u0628\u0646\u0642\u0631\u0629 \u0648\u0627\u062D\u062F\u0629.";
            dict_ar["step1_title"] = "\u0627\u062E\u062A\u0631 \u062D\u0632\u0645\u0629";
            dict_ar["step1_desc"] = "\u062D\u0633\u0628 \u0627\u062D\u062A\u064A\u0627\u062C\u0627\u062A\u0643";
            dict_ar["step2_title"] = "\u0645\u0631\u0627\u062C\u0639\u0629 \u0627\u0644\u062A\u0637\u0628\u064A\u0642\u0627\u062A";
            dict_ar["step2_desc"] = "\u062A\u062D\u0642\u0642 \u0645\u0646 \u0627\u0644\u0627\u062E\u062A\u064A\u0627\u0631";
            dict_ar["step3_title"] = "\u062A\u062B\u0628\u064A\u062A";
            dict_ar["step3_desc"] = "\u0627\u0628\u062F\u0623 \u0627\u0644\u0627\u0633\u062A\u062E\u062F\u0627\u0645";
            dict_ar["available_packs_title"] = "\u062D\u0632\u0645 \u0627\u0644\u062A\u0637\u0628\u064A\u0642\u0627\u062A \u0627\u0644\u0645\u062A\u0627\u062D\u0629";
            dict_ar["available_packs_subtitle"] = "\u0627\u062E\u062A\u0631 \u062D\u0632\u0645\u0629 \u062C\u0627\u0647\u0632\u0629 \u0623\u0648 \u0627\u062E\u062A\u0631 \u062A\u0637\u0628\u064A\u0642\u0627\u062A \u0641\u0631\u062F\u064A\u0629.";
            dict_ar["top_choice_badge"] = "\u0627\u0644\u062E\u064A\u0627\u0631 \u0627\u0644\u0623\u0641\u0636\u0644";
            dict_ar["recommended_badge"] = "\u0645\u0648\u0635\u0649 \u0628\u0647";
            dict_ar["custom_badge"] = "\u062D\u0632\u0645\u0629 \u0645\u062E\u0635\u0635\u0629";
            dict_ar["partner_apps_count"] = "{0} \u062A\u0637\u0628\u064A\u0642\u0627\u062A \u0634\u0631\u064A\u0643\u0629";
            dict_ar["apps_count"] = "{0} \u062A\u0637\u0628\u064A\u0642\u0627\u062A";
            dict_ar["catalog_title"] = "\u062C\u0645\u064A\u0639 \u0627\u0644\u062A\u0637\u0628\u064A\u0642\u0627\u062A";
            dict_ar["catalog_subtitle"] = "\u062A\u0635\u0641\u062D 16,349 \u062D\u0632\u0645\u0629 \u0628\u0631\u0645\u062C\u064A\u0629 \u0645\u0639 \u0628\u062D\u062B \u0641\u0648\u0631\u064A \u0648\u0627\u062E\u062A\u064A\u0627\u0631 \u0628\u0646\u0642\u0631\u0629 \u0648\u0627\u062D\u062F\u0629.";
            dict_ar["select_all"] = "\u062A\u062D\u062F\u064A\u062F \u062C\u0645\u064A\u0639 \u0627\u0644\u062A\u0637\u0628\u064A\u0642\u0627\u062A";
            dict_ar["sort_button"] = "\u0641\u0631\u0632 \u0627\u0644\u062A\u0637\u0628\u064A\u0642\u0627\u062A \u25BE";
            dict_ar["sort_selected_top"] = "\u0627\u0644\u0645\u062E\u062A\u0627\u0631\u0629 \u0648\u0627\u0644\u0645\u0648\u0635\u0649 \u0628\u0647\u0627";
            dict_ar["sort_name_az"] = "\u0627\u0644\u0627\u0633\u0645 (\u0645\u0646 \u0623 \u0625\u0644\u0649 \u064A)";
            dict_ar["sort_name_za"] = "\u0627\u0644\u0627\u0633\u0645 (\u0645\u0646 \u064A \u0625\u0644\u0649 \u0623)";
            dict_ar["sort_category"] = "\u062D\u0633\u0628 \u0627\u0644\u0641\u0626\u0629";
            dict_ar["cat_all"] = "\u0627\u0644\u0643\u0644";
            dict_ar["cat_browsers"] = "\u0645\u062A\u0635\u0641\u062D\u0627\u062A \u0627\u0644\u0648\u064A\u0628";
            dict_ar["cat_dev"] = "\u0627\u0644\u062A\u0637\u0648\u064A\u0631 \u0648\u0627\u0644\u0628\u0631\u0645\u062C\u0629";
            dict_ar["cat_collab"] = "\u0627\u0644\u062A\u0648\u0627\u0635\u0644 \u0648\u0627\u0644\u062A\u0639\u0627\u0648\u0646";
            dict_ar["cat_office"] = "\u0627\u0644\u0625\u0646\u062A\u0627\u062C\u064A\u0629 \u0648\u0627\u0644\u0645\u0643\u062A\u0628";
            dict_ar["cat_tools"] = "\u0627\u0644\u0623\u062F\u0648\u0627\u062A \u0627\u0644\u0645\u0633\u0627\u0639\u062F\u0629";
            dict_ar["cat_media"] = "\u0627\u0644\u0648\u0633\u0627\u0626\u0637 \u0648\u0627\u0644\u0625\u0628\u062F\u0627\u0639";
            dict_ar["cat_sec"] = "\u0627\u0644\u0623\u0645\u0627\u0646 \u0648\u0627\u0644\u0634\u0628\u0643\u0627\u062A";
            dict_ar["cat_cloud"] = "\u0627\u0644\u0633\u062D\u0627\u0628\u0629 \u0648\u0627\u0644\u062A\u062E\u0632\u064A\u0646";
            dict_ar["cat_games"] = "\u0627\u0644\u0623\u0644\u0639\u0627\u0628 \u0648\u0627\u0644\u0645\u062D\u0631\u0643\u0627\u062A";
            dict_ar["btn_install"] = "\uD83D\uDCE5  \u062A\u062B\u0628\u064A\u062A \u0627\u0644\u062A\u0637\u0628\u064A\u0642\u0627\u062A \u0627\u0644\u0645\u062D\u062F\u062F\u0629 ({0})";
            dict_ar["btn_install_no_count"] = "\uD83D\uDCE5  \u062A\u062B\u0628\u064A\u062A \u0627\u0644\u062A\u0637\u0628\u064A\u0642\u0627\u062A \u0627\u0644\u0645\u062D\u062F\u062F\u0629";
            dict_ar["btn_stop"] = "\u0625\u064A\u0642\u0627\u0641 \u0627\u0644\u062A\u062B\u0628\u064A\u062A";
            dict_ar["btn_import_pack"] = "\uD83D\uDCE5 \u0627\u0633\u062A\u064A\u0631\u0627\u062F \u062D\u0632\u0645\u0629";
            dict_ar["btn_create_pack"] = "+ \u0625\u0646\u0634\u0627\u0621 \u062D\u0632\u0645\u0629 \u062C\u062F\u064A\u062F\u0629";
            dict_ar["badge_ready"] = "\u062C\u0627\u0647\u0632";
            dict_ar["badge_installed"] = "\u0645\u062B\u0628\u062A";
            dict_ar["badge_installing"] = "\u062C\u0627\u0631\u064A \u0627\u0644\u062A\u062B\u0628\u064A\u062A...";
            dict_ar["badge_failed"] = "\u0641\u0634\u0644";
            dict_ar["showing_count"] = "\u0639\u0631\u0636 {0:N0} \u0645\u0646 {1:N0} \u062A\u0637\u0628\u064A\u0642";
            dict_ar["showing_count_selected"] = "\u0639\u0631\u0636 {0:N0} \u0645\u0646 {1:N0} \u062A\u0637\u0628\u064A\u0642 ({2:N0} \u0645\u062D\u062F\u062F)";
            dict_ar["showing_local_online"] = "\u0639\u0631\u0636 {0} \u0645\u062D\u0644\u064A + {1} \u0639\u0628\u0631 \u0627\u0644\u0625\u0646\u062A\u0631\u0646\u062A \u0644\u0640 \"{2}\"";
            dict_ar["showing_local_online_selected"] = "\u0639\u0631\u0636 {0} \u0645\u062D\u0644\u064A + {1} \u0639\u0628\u0631 \u0627\u0644\u0625\u0646\u062A\u0631\u0646\u062A \u0644\u0640 \"{2}\" ({3:N0} \u0645\u062D\u062F\u062F)";
            dict_ar["found_online_count"] = "\u062A\u0645 \u0627\u0644\u0639\u062B\u0648\u0631 \u0639\u0644\u0649 {0:N0} \u062A\u0637\u0628\u064A\u0642 \u0639\u0628\u0631 \u0627\u0644\u0625\u0646\u062A\u0631\u0646\u062A \u0644\u0640 \"{1}\"";
            dict_ar["found_online_count_selected"] = "\u062A\u0645 \u0627\u0644\u0639\u062B\u0648\u0631 \u0639\u0644\u0649 {0:N0} \u062A\u0637\u0628\u064A\u0642 \u0639\u0628\u0631 \u0627\u0644\u0625\u0646\u062A\u0631\u0646\u062A \u0644\u0640 \"{1}\" ({2:N0} \u0645\u062D\u062F\u062F)";
            dict_ar["showing_query_count"] = "\u0639\u0631\u0636 {0:N0} \u062A\u0637\u0628\u064A\u0642 \u0644\u0640 \"{1}\"";
            dict_ar["showing_query_count_selected"] = "\u0639\u0631\u0636 {0:N0} \u062A\u0637\u0628\u064A\u0642 \u0644\u0640 \"{1}\" ({2:N0} \u0645\u062D\u062F\u062F)";
            dict_ar["searching_online"] = "\uD83D\uDD0D \u0627\u0644\u0628\u062D\u062B \u0641\u064A \u0645\u0633\u062A\u0648\u062F\u0639 Winget \u0639\u0628\u0631 \u0627\u0644\u0625\u0646\u062A\u0631\u0646\u062A \u0639\u0646 \"{0}\"...";
            dict_ar["no_apps_found"] = "\u0644\u0645 \u064A\u062A\u0645 \u0627\u0644\u0639\u062B\u0648\u0631 \u0639\u0644\u0649 \u062D\u0632\u0645 \u0628\u0631\u0645\u062C\u064A\u0629 \u0644\u0640 \"{0}\"";
            dict_ar["no_apps_found_desc"] = "\u0644\u0645 \u064A\u062A\u0645 \u0627\u0644\u0639\u062B\u0648\u0631 \u0639\u0644\u064A\u0647\u0627 \u0645\u062D\u0644\u064A\u0627\u064B \u0623\u0648 \u0641\u064A \u0645\u0633\u062A\u0648\u062F\u0639 Winget. \u064A\u0631\u062C\u0649 \u0627\u0644\u062A\u062D\u0642\u0642 \u0645\u0646 \u0627\u0644\u062A\u0647\u062C\u0626\u0629.";
            dict_ar["type_to_search"] = "\u064A\u0631\u062C\u0649 \u0643\u062A\u0627\u0628\u0629 \u062D\u0631\u0641\u064A\u0646 \u0639\u0644\u0649 \u0627\u0644\u0623\u0642\u0644 \u0644\u0644\u0628\u062D\u062B \u0639\u0646 \u0627\u0644\u062A\u0637\u0628\u064A\u0642\u0627\u062A.";
            dict_ar["no_winget_title"] = "\u0644\u0645 \u064A\u062A\u0645 \u0627\u0644\u0639\u062B\u0648\u0631 \u0639\u0644\u0649 \u062A\u0637\u0628\u064A\u0642\u0627\u062A \u0645\u062D\u0644\u064A\u0629 \u0644\u0640 \"{0}\"";
            dict_ar["no_winget_desc"] = "Microsoft Winget \u063A\u064A\u0631 \u0645\u062A\u0648\u0641\u0631 \u0639\u0644\u0649 \u0647\u0630\u0627 \u0627\u0644\u0646\u0638\u0627\u0645 \u0644\u0644\u0628\u062D\u062B \u0639\u0628\u0631 \u0627\u0644\u0625\u0646\u062A\u0631\u0646\u062A.";
            dict_ar["bottom_tip"] = "\u062A\u0644\u0645\u064A\u062D: \u064A\u0645\u0643\u0646\u0643 \u062A\u062E\u0635\u064A\u0635 \u0623\u064A \u062D\u0632\u0645\u0629 \u0648\u0625\u0636\u0627\u0641\u0629 \u0623\u0648 \u0625\u0632\u0627\u0644\u0629 \u0627\u0644\u062A\u0637\u0628\u064A\u0642\u0627\u062A \u0648\u062A\u0635\u062F\u064A\u0631\u0647\u0627.";
            dict_ar["pack_selected_prefix"] = "\u062D\u0632\u0645\u0629 \u0627\u0644\u062A\u0637\u0628\u064A\u0642\u0627\u062A \u0627\u0644\u0645\u062D\u062F\u062F\u0629: {0}";
            dict_ar["logs_title"] = "\u0633\u062C\u0644 \u0627\u0644\u062A\u062B\u0628\u064A\u062A";
            dict_ar["logs_subtitle"] = "\u0645\u0631\u0627\u0642\u0628\u0629 \u0627\u0644\u0646\u0634\u0631 \u0627\u0644\u0645\u0628\u0627\u0634\u0631 \u0648\u0633\u062C\u0644\u0627\u062A \u0627\u0644\u062A\u062F\u0642\u064A\u0642.";
            dict_ar["btn_summary_report"] = "\u062A\u0642\u0631\u064A\u0631 \u0645\u0644\u062E\u0635";
            dict_ar["btn_open_logfile"] = "\u0641\u062A\u062D \u0645\u0644\u0641 \u0627\u0644\u0633\u062C\u0644";
            dict_ar["btn_clear_screen"] = "\u0645\u0633\u062D \u0627\u0644\u0634\u0627\u0634\u0629";
            _translations["ar"] = dict_ar;

            // Language: it
            Dictionary<string, string> dict_it = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            dict_it["btn_select_pack"] = "Seleziona pacchetto";
            dict_it["btn_customize_pack"] = "Personalizza";
            dict_it["btn_export_pack"] = "Esporta";
            dict_it["nav_quick_setup"] = "Configurazione rapida";
            dict_it["nav_all_apps"] = "Tutte le applicazioni";
            dict_it["nav_cloud_sync"] = "Sincronizzazione cloud";
            dict_it["nav_history"] = "Cronologia installazioni";
            dict_it["search_placeholder"] = "Cerca app, impostazioni o pacchetto...";
            dict_it["help_button"] = "Aiuto ?";
            dict_it["lang_auto"] = "Automatico (Predefinito di sistema)";
            dict_it["menu_guide"] = "Come usare (Guida interattiva)...";
            dict_it["menu_updates"] = "Verifica aggiornamenti...";
            dict_it["menu_settings"] = "Impostazioni...";
            dict_it["menu_about"] = "Informazioni su FleetForge...";
            dict_it["quick_setup_title"] = "Configurazione rapida";
            dict_it["quick_setup_subtitle"] = "Scegli un pacchetto, seleziona le app e inizia con un clic.";
            dict_it["step1_title"] = "Scegli un pacchetto";
            dict_it["step1_desc"] = "In base alle tue esigenze";
            dict_it["step2_title"] = "Revisiona app";
            dict_it["step2_desc"] = "Controlla selezione";
            dict_it["step3_title"] = "Installa";
            dict_it["step3_desc"] = "Inizia a usare";
            dict_it["available_packs_title"] = "Pacchetti app disponibili";
            dict_it["available_packs_subtitle"] = "Seleziona un pacchetto pronto o singole applicazioni.";
            dict_it["top_choice_badge"] = "SCELTA MIGLIORE";
            dict_it["recommended_badge"] = "CONSIGLIATO";
            dict_it["custom_badge"] = "PACCHETTO PERSONALIZZATO";
            dict_it["partner_apps_count"] = "{0} app partner";
            dict_it["apps_count"] = "{0} applicazioni";
            dict_it["catalog_title"] = "Tutte le applicazioni";
            dict_it["catalog_subtitle"] = "Esplora 16.349 pacchetti software con ricerca istantanea e selezione con un clic.";
            dict_it["select_all"] = "Seleziona tutte le app";
            dict_it["sort_button"] = "Ordina app \u25BE";
            dict_it["sort_selected_top"] = "Selezionati e consigliati";
            dict_it["sort_name_az"] = "Nome (A-Z)";
            dict_it["sort_name_za"] = "Nome (Z-A)";
            dict_it["sort_category"] = "Per categoria";
            dict_it["cat_all"] = "Tutti";
            dict_it["cat_browsers"] = "Browser Web";
            dict_it["cat_dev"] = "Sviluppo e codice";
            dict_it["cat_collab"] = "Comunicazione e team";
            dict_it["cat_office"] = "Produttivit\u00E0 e ufficio";
            dict_it["cat_tools"] = "Utilit\u00E0 e strumenti";
            dict_it["cat_media"] = "Media e grafica";
            dict_it["cat_sec"] = "Sicurezza e rete";
            dict_it["cat_cloud"] = "Cloud e archiviazione";
            dict_it["cat_games"] = "Giochi e motori";
            dict_it["btn_install"] = "\uD83D\uDCE5  Installa app selezionate ({0})";
            dict_it["btn_install_no_count"] = "\uD83D\uDCE5  Installa app selezionate";
            dict_it["btn_stop"] = "Interrompi installazione";
            dict_it["btn_import_pack"] = "\uD83D\uDCE5 Importa pacchetto";
            dict_it["btn_create_pack"] = "+ Crea nuovo pacchetto";
            dict_it["badge_ready"] = "Pronto";
            dict_it["badge_installed"] = "Installato";
            dict_it["badge_installing"] = "Installazione...";
            dict_it["badge_failed"] = "Non riuscito";
            dict_it["showing_count"] = "Visualizzazione di {0:N0} su {1:N0} app";
            dict_it["showing_count_selected"] = "Visualizzazione di {0:N0} su {1:N0} app ({2:N0} selezionate)";
            dict_it["showing_local_online"] = "Mostra {0} locali + {1} online per \"{2}\"";
            dict_it["showing_local_online_selected"] = "Mostra {0} locali + {1} online per \"{2}\" ({3:N0} selezionate)";
            dict_it["found_online_count"] = "Trovate {0:N0} app online per \"{1}\"";
            dict_it["found_online_count_selected"] = "Trovate {0:N0} app online per \"{1}\" ({2:N0} selezionate)";
            dict_it["showing_query_count"] = "Visualizzazione di {0:N0} app per \"{1}\"";
            dict_it["showing_query_count_selected"] = "Visualizzazione di {0:N0} app per \"{1}\" ({2:N0} selezionate)";
            dict_it["searching_online"] = "\uD83D\uDD0D Ricerca nel repository Winget online per \"{0}\"...";
            dict_it["no_apps_found"] = "Nessun pacchetto software trovato per \"{0}\"";
            dict_it["no_apps_found_desc"] = "Non trovato in locale n\u00E9 in Winget online. Controllare l'ortografia.";
            dict_it["type_to_search"] = "Digita almeno 2 caratteri per cercare le app.";
            dict_it["no_winget_title"] = "Nessuna app locale trovata per \"{0}\"";
            dict_it["no_winget_desc"] = "Microsoft Winget non \u00E8 disponibile su questo sistema.";
            dict_it["bottom_tip"] = "Suggerimento: Puoi personalizzare qualsiasi pacchetto, aggiungere o rimuovere app.";
            dict_it["pack_selected_prefix"] = "Pacchetto app selezionato: {0}";
            dict_it["logs_title"] = "Cronologia installazioni";
            dict_it["logs_subtitle"] = "Monitoraggio dell'installazione dal vivo e registri di controllo.";
            dict_it["btn_summary_report"] = "Riepilogo";
            dict_it["btn_open_logfile"] = "Apri file di registro";
            dict_it["btn_clear_screen"] = "Cancella schermo";
            _translations["it"] = dict_it;

            // Language: ko
            Dictionary<string, string> dict_ko = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            dict_ko["btn_select_pack"] = "\uD328\uD0A4\uC9C0 \uC120\uD0DD";
            dict_ko["btn_customize_pack"] = "\uC0AC\uC6A9\uC790 \uC815\uC758";
            dict_ko["btn_export_pack"] = "\uB0B4\uBCF4\uB0B4\uAE30";
            dict_ko["nav_quick_setup"] = "\uBE60\uB978 \uC124\uC815";
            dict_ko["nav_all_apps"] = "\uBAA8\uB4E0 \uC571";
            dict_ko["nav_cloud_sync"] = "\uD074\uB77C\uC6B0\uB4DC \uB3D9\uAE30\uD654";
            dict_ko["nav_history"] = "\uC124\uCE58 \uAE30\uB85D";
            dict_ko["search_placeholder"] = "\uC571, \uC124\uC815 \uB610\uB294 \uD328\uD0A4\uC9C0 \uAC80\uC0C9...";
            dict_ko["help_button"] = "\uB3C4\uC6C0\uB9D0 ?";
            dict_ko["lang_auto"] = "\uC790\uB3D9 (\uC2DC\uC2A4\uD15C \uAE30\uBCF8\uAC12)";
            dict_ko["menu_guide"] = "\uC0AC\uC6A9 \uBC29\uBC95 (\uB300\uD654\uD615 \uAC00\uC774\uB4DC)...";
            dict_ko["menu_updates"] = "\uC5C5\uB370\uC774\uD2B8 \uD655\uC778...";
            dict_ko["menu_settings"] = "\uC124\uC815...";
            dict_ko["menu_about"] = "FleetForge \uC815\uBCF4...";
            dict_ko["quick_setup_title"] = "\uBE60\uB978 \uC124\uC815";
            dict_ko["quick_setup_subtitle"] = "\uD328\uD0A4\uC9C0\uB97C \uC120\uD0DD\uD558\uACE0 \uC571\uC744 \uACE8\uB77C \uD55C \uBC88\uC758 \uD074\uB9AD\uC73C\uB85C \uC2DC\uC791\uD558\uC138\uC694.";
            dict_ko["step1_title"] = "\uD328\uD0A4\uC9C0 \uC120\uD0DD";
            dict_ko["step1_desc"] = "\uD544\uC694\uC5D0 \uB9DE\uAC8C";
            dict_ko["step2_title"] = "\uC571 \uAC80\uD1A0";
            dict_ko["step2_desc"] = "\uC120\uD0DD \uD655\uC778";
            dict_ko["step3_title"] = "\uC124\uCE58";
            dict_ko["step3_desc"] = "\uBC14\uB85C \uC0AC\uC6A9";
            dict_ko["available_packs_title"] = "\uC0AC\uC6A9 \uAC00\uB2A5\uD55C \uC571 \uD328\uD0A4\uC9C0";
            dict_ko["available_packs_subtitle"] = "\uC0AC\uC804 \uAD6C\uC131\uB41C \uD328\uD0A4\uC9C0\uB97C \uC120\uD0DD\uD558\uAC70\uB098 \uAC1C\uBCC4 \uC571\uC744 \uC120\uD0DD\uD558\uC138\uC694.";
            dict_ko["top_choice_badge"] = "\uCD5C\uACE0\uC758 \uC120\uD0DD";
            dict_ko["recommended_badge"] = "\uCD94\uCC9C";
            dict_ko["custom_badge"] = "\uC0AC\uC6A9\uC790 \uC9C0\uC815 \uD328\uD0A4\uC9C0";
            dict_ko["partner_apps_count"] = "{0}\uAC1C \uD30C\uD2B8\uB108 \uC571";
            dict_ko["apps_count"] = "{0}\uAC1C \uC571";
            dict_ko["catalog_title"] = "\uBAA8\uB4E0 \uC560\uD50C\uB9AC\uCF00\uC774\uC158";
            dict_ko["catalog_subtitle"] = "\uC989\uC2DC \uAC80\uC0C9 \uBC0F \uC6D0\uD074\uB9AD \uC120\uD0DD\uC73C\uB85C 16,349\uAC1C\uC758 \uC18C\uD504\uD2B8\uC6E8\uC5B4 \uD328\uD0A4\uC9C0\uB97C \uD0D0\uC0C9\uD558\uC138\uC694.";
            dict_ko["select_all"] = "\uBAA8\uB4E0 \uC571 \uC120\uD0DD";
            dict_ko["sort_button"] = "\uC571 \uC815\uB82C \u25BE";
            dict_ko["sort_selected_top"] = "\uC120\uD0DD\uB428 \uBC0F \uCD94\uCC9C";
            dict_ko["sort_name_az"] = "\uC774\uB984 (A-Z)";
            dict_ko["sort_name_za"] = "\uC774\uB984 (Z-A)";
            dict_ko["sort_category"] = "\uCE74\uD14C\uACE0\uB9AC\uBCC4";
            dict_ko["cat_all"] = "\uC804\uCCB4";
            dict_ko["cat_browsers"] = "\uC6F9 \uBE0C\uB77C\uC6B0\uC800";
            dict_ko["cat_dev"] = "\uAC1C\uBC1C \uBC0F \uCF54\uB4DC";
            dict_ko["cat_collab"] = "\uCEE4\uBBA4\uB2C8\uCF00\uC774\uC158 \uBC0F \uD611\uC5C5";
            dict_ko["cat_office"] = "\uC0DD\uC0B0\uC131 \uBC0F \uC624\uD53C\uC2A4";
            dict_ko["cat_tools"] = "\uC720\uD2F8\uB9AC\uD2F0 \uBC0F \uB3C4\uAD6C";
            dict_ko["cat_media"] = "\uBBF8\uB514\uC5B4 \uBC0F \uD06C\uB9AC\uC5D0\uC774\uD2F0\uBE0C";
            dict_ko["cat_sec"] = "\uBCF4\uC548 \uBC0F \uB124\uD2B8\uC6CC\uD06C";
            dict_ko["cat_cloud"] = "\uD074\uB77C\uC6B0\uB4DC \uBC0F \uC2A4\uD1A0\uB9AC\uC9C0";
            dict_ko["cat_games"] = "\uAC8C\uC784 \uBC0F \uC5D4\uC9C4";
            dict_ko["btn_install"] = "\uD83D\uDCE5  \uC120\uD0DD\uD55C \uC571 \uC124\uCE58 ({0})";
            dict_ko["btn_install_no_count"] = "\uD83D\uDCE5  \uC120\uD0DD\uD55C \uC571 \uC124\uCE58";
            dict_ko["btn_stop"] = "\uC124\uCE58 \uC911\uC9C0";
            dict_ko["btn_import_pack"] = "\uD83D\uDCE5 \uD328\uD0A4\uC9C0 \uAC00\uC838\uC624\uAE30";
            dict_ko["btn_create_pack"] = "+ \uC0C8 \uD328\uD0A4\uC9C0 \uB9CC\uB4E4\uAE30";
            dict_ko["badge_ready"] = "\uC900\uBE44\uB428";
            dict_ko["badge_installed"] = "\uC124\uCE58\uB428";
            dict_ko["badge_installing"] = "\uC124\uCE58 \uC911...";
            dict_ko["badge_failed"] = "\uC2E4\uD328";
            dict_ko["showing_count"] = "{1:N0}\uAC1C \uC911 {0:N0}\uAC1C \uC571 \uD45C\uC2DC \uC911";
            dict_ko["showing_count_selected"] = "{1:N0}\uAC1C \uC911 {0:N0}\uAC1C \uC571 \uD45C\uC2DC \uC911 ({2:N0}\uAC1C \uC120\uD0DD\uB428)";
            dict_ko["showing_local_online"] = "\"{2}\"\uC5D0 \uB300\uD574 {0}\uAC1C \uB85C\uCEEC + {1}\uAC1C \uC628\uB77C\uC778 \uC571 \uD45C\uC2DC";
            dict_ko["showing_local_online_selected"] = "\"{2}\"\uC5D0 \uB300\uD574 {0}\uAC1C \uB85C\uCEEC + {1}\uAC1C \uC628\uB77C\uC778 \uC571 \uD45C\uC2DC ({3:N0}\uAC1C \uC120\uD0DD\uB428)";
            dict_ko["found_online_count"] = "\"{1}\"\uC5D0 \uB300\uD55C \uC628\uB77C\uC778 \uC571 {0:N0}\uAC1C \uBC1C\uACAC";
            dict_ko["found_online_count_selected"] = "\"{1}\"\uC5D0 \uB300\uD55C \uC628\uB77C\uC778 \uC571 {0:N0}\uAC1C \uBC1C\uACAC ({2:N0}\uAC1C \uC120\uD0DD\uB428)";
            dict_ko["showing_query_count"] = "\"{1}\"\uC5D0 \uB300\uD55C {0:N0}\uAC1C \uC571 \uD45C\uC2DC \uC911";
            dict_ko["showing_query_count_selected"] = "\"{1}\"\uC5D0 \uB300\uD55C {0:N0}\uAC1C \uC571 \uD45C\uC2DC \uC911 ({2:N0}\uAC1C \uC120\uD0DD\uB428)";
            dict_ko["searching_online"] = "\uD83D\uDD0D \"{0}\"\uC5D0 \uB300\uD574 \uC628\uB77C\uC778 Winget \uC800\uC7A5\uC18C \uAC80\uC0C9 \uC911...";
            dict_ko["no_apps_found"] = "\"{0}\"\uC5D0 \uB300\uD55C \uC18C\uD504\uD2B8\uC6E8\uC5B4 \uD328\uD0A4\uC9C0\uB97C \uCC3E\uC744 \uC218 \uC5C6\uC2B5\uB2C8\uB2E4";
            dict_ko["no_apps_found_desc"] = "\uB85C\uCEEC \uB370\uC774\uD130\uBCA0\uC774\uC2A4\uB098 \uC628\uB77C\uC778 Winget\uC5D0\uC11C \uCC3E\uC744 \uC218 \uC5C6\uC2B5\uB2C8\uB2E4. \uCCA0\uC790\uB97C \uD655\uC778\uD558\uC138\uC694.";
            dict_ko["type_to_search"] = "\uC571\uC744 \uAC80\uC0C9\uD558\uB824\uBA74 \uCD5C\uC18C 2\uC790\uB97C \uC785\uB825\uD558\uC138\uC694.";
            dict_ko["no_winget_title"] = "\"{0}\"\uC5D0 \uB300\uD55C \uB85C\uCEEC \uC571\uC744 \uCC3E\uC744 \uC218 \uC5C6\uC2B5\uB2C8\uB2E4";
            dict_ko["no_winget_desc"] = "\uC628\uB77C\uC778 \uC800\uC7A5\uC18C\uB97C \uAC80\uC0C9\uD558\uAE30 \uC704\uD55C Microsoft Winget\uC744 \uC0AC\uC6A9\uD560 \uC218 \uC5C6\uC2B5\uB2C8\uB2E4.";
            dict_ko["bottom_tip"] = "\uD301: \uD328\uD0A4\uC9C0\uB97C \uB9DE\uCDA4\uC124\uC815\uD558\uACE0, \uC571\uC744 \uCD94\uAC00/\uC0AD\uC81C\uD558\uC5EC \uC124\uC815\uC744 \uB0B4\uBCF4\uB0BC \uC218 \uC788\uC2B5\uB2C8\uB2E4.";
            dict_ko["pack_selected_prefix"] = "\uC120\uD0DD\uB41C \uC571 \uD328\uD0A4\uC9C0: {0}";
            dict_ko["logs_title"] = "\uC124\uCE58 \uAE30\uB85D";
            dict_ko["logs_subtitle"] = "\uC2E4\uC2DC\uAC04 \uBC30\uD3EC \uAC10\uC2DC \uBC0F \uAC10\uC0AC \uB85C\uADF8.";
            dict_ko["btn_summary_report"] = "\uC694\uC57D \uBCF4\uACE0\uC11C";
            dict_ko["btn_open_logfile"] = "\uB85C\uADF8 \uD30C\uC77C \uC5F4\uAE30";
            dict_ko["btn_clear_screen"] = "\uD654\uBA74 \uC9C0\uC6B0\uAE30";
            _translations["ko"] = dict_ko;

            // Language: tr
            Dictionary<string, string> dict_tr = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            dict_tr["btn_select_pack"] = "Paket Se\u00E7";
            dict_tr["btn_customize_pack"] = "\u00D6zelle\u015Ftir";
            dict_tr["btn_export_pack"] = "D\u0131\u015Fa Aktar";
            dict_tr["nav_quick_setup"] = "H\u0131zl\u0131 Kurulum";
            dict_tr["nav_all_apps"] = "T\u00FCm Uygulamalar";
            dict_tr["nav_cloud_sync"] = "Bulut Senkronizasyonu";
            dict_tr["nav_history"] = "Kurulum Ge\u00E7mi\u015Fi";
            dict_tr["search_placeholder"] = "Uygulama, ayar veya paket ara...";
            dict_tr["help_button"] = "Yard\u0131m ?";
            dict_tr["lang_auto"] = "Otomatik (Sistem Varsay\u0131lan\u0131)";
            dict_tr["menu_guide"] = "Nas\u0131l Kullan\u0131l\u0131r (K\u0131lavuz)...";
            dict_tr["menu_updates"] = "G\u00FCncellemeleri Denetle...";
            dict_tr["menu_settings"] = "Ayarlar...";
            dict_tr["menu_about"] = "FleetForge Hakk\u0131nda...";
            dict_tr["quick_setup_title"] = "H\u0131zl\u0131 Kurulum";
            dict_tr["quick_setup_subtitle"] = "Bir paket se\u00E7in, uygulamalar\u0131 belirleyin ve tek t\u0131kla ba\u015Flay\u0131n.";
            dict_tr["step1_title"] = "Paket Se\u00E7";
            dict_tr["step1_desc"] = "\u0130htiyac\u0131n\u0131za g\u00F6re";
            dict_tr["step2_title"] = "Uygulamalar\u0131 \u0130ncele";
            dict_tr["step2_desc"] = "Se\u00E7imi kontrol et";
            dict_tr["step3_title"] = "Y\u00FCkle";
            dict_tr["step3_desc"] = "Kullanmaya ba\u015Flay\u0131n";
            dict_tr["available_packs_title"] = "Mevcut Uygulama Paketleri";
            dict_tr["available_packs_subtitle"] = "Haz\u0131r bir paket se\u00E7in veya ba\u011F\u0131ms\u0131z uygulamalar\u0131 belirleyin.";
            dict_tr["top_choice_badge"] = "EN \u0130Y\u0130 SE\u00C7\u0130M";
            dict_tr["recommended_badge"] = "\u00D6NER\u0130LEN";
            dict_tr["custom_badge"] = "\u00D6ZEL PAKET";
            dict_tr["partner_apps_count"] = "{0} i\u015F orta\u011F\u0131 uygulamas\u0131";
            dict_tr["apps_count"] = "{0} uygulama";
            dict_tr["catalog_title"] = "T\u00FCm Uygulamalar";
            dict_tr["catalog_subtitle"] = "An\u0131nda arama ve tek t\u0131kla se\u00E7imle 16.349 yaz\u0131l\u0131m paketine g\u00F6z at\u0131n.";
            dict_tr["select_all"] = "T\u00FCm Uygulamalar\u0131 Se\u00E7";
            dict_tr["sort_button"] = "Uygulamalar\u0131 S\u0131rala \u25BE";
            dict_tr["sort_selected_top"] = "Se\u00E7ilenler ve \u00D6nerilenler";
            dict_tr["sort_name_az"] = "\u0130sim (A'dan Z'ye)";
            dict_tr["sort_name_za"] = "\u0130sim (Z'den A'ya)";
            dict_tr["sort_category"] = "Kategoriye G\u00F6re";
            dict_tr["cat_all"] = "T\u00FCm\u00FC";
            dict_tr["cat_browsers"] = "Web Taray\u0131c\u0131lar\u0131";
            dict_tr["cat_dev"] = "Geli\u015Ftirme ve Kod";
            dict_tr["cat_collab"] = "\u0130leti\u015Fim ve \u0130\u015Fbirli\u011Fi";
            dict_tr["cat_office"] = "\u00DCretkenlik ve Ofis";
            dict_tr["cat_tools"] = "Yard\u0131mc\u0131 Programlar";
            dict_tr["cat_media"] = "Medya ve Yarat\u0131c\u0131l\u0131k";
            dict_tr["cat_sec"] = "G\u00FCvenlik ve A\u011F";
            dict_tr["cat_cloud"] = "Bulut ve Depolama";
            dict_tr["cat_games"] = "Oyun ve Motorlar";
            dict_tr["btn_install"] = "\uD83D\uDCE5  Se\u00E7ilen Uygulamalar\u0131 Y\u00FCkle ({0})";
            dict_tr["btn_install_no_count"] = "\uD83D\uDCE5  Se\u00E7ilen Uygulamalar\u0131 Y\u00FCkle";
            dict_tr["btn_stop"] = "Kurulumu Durdur";
            dict_tr["btn_import_pack"] = "\uD83D\uDCE5 Paket \u0130\u00E7e Aktar";
            dict_tr["btn_create_pack"] = "+ Yeni Paket Olu\u015Ftur";
            dict_tr["badge_ready"] = "Haz\u0131r";
            dict_tr["badge_installed"] = "Y\u00FCklendi";
            dict_tr["badge_installing"] = "Y\u00FCkleniyor...";
            dict_tr["badge_failed"] = "Ba\u015Far\u0131s\u0131z";
            dict_tr["showing_count"] = "{1:N0} uygulamadan {0:N0} tanesi g\u00F6steriliyor";
            dict_tr["showing_count_selected"] = "{1:N0} uygulamadan {0:N0} tanesi g\u00F6steriliyor ({2:N0} se\u00E7ildi)";
            dict_tr["showing_local_online"] = "\"{2}\" i\u00E7in {0} yerel + {1} \u00E7evrimi\u00E7i uygulama";
            dict_tr["showing_local_online_selected"] = "\"{2}\" i\u00E7in {0} yerel + {1} \u00E7evrimi\u00E7i uygulama ({3:N0} se\u00E7ildi)";
            dict_tr["found_online_count"] = "\"{1}\" i\u00E7in {0:N0} \u00E7evrimi\u00E7i uygulama bulundu";
            dict_tr["found_online_count_selected"] = "\"{1}\" i\u00E7in {0:N0} \u00E7evrimi\u00E7i uygulama bulundu ({2:N0} se\u00E7ildi)";
            dict_tr["showing_query_count"] = "\"{1}\" i\u00E7in {0:N0} uygulama g\u00F6steriliyor";
            dict_tr["showing_query_count_selected"] = "\"{1}\" i\u00E7in {0:N0} uygulama g\u00F6steriliyor ({2:N0} se\u00E7ildi)";
            dict_tr["searching_online"] = "\uD83D\uDD0D \u00C7evrimi\u00E7i Winget deposunda \"{0}\" aran\u0131yor...";
            dict_tr["no_apps_found"] = "\"{0}\" i\u00E7in yaz\u0131l\u0131m paketi bulunamad\u0131";
            dict_tr["no_apps_found_desc"] = "Yerel veritaban\u0131nda veya Winget deposunda bulunamad\u0131. Yaz\u0131m\u0131 kontrol edin.";
            dict_tr["type_to_search"] = "Arama yapmak i\u00E7in l\u00FCtfen en az 2 karakter yaz\u0131n.";
            dict_tr["no_winget_title"] = "\"{0}\" i\u00E7in yerel uygulama bulunamad\u0131";
            dict_tr["no_winget_desc"] = "\u00C7evrimi\u00E7i arama i\u00E7in Microsoft Winget bu sistemde mevcut de\u011Fil.";
            dict_tr["bottom_tip"] = "\u0130pucu: Herhangi bir paketi \u00F6zelle\u015Ftirebilir, uygulama ekleyip \u00E7\u0131karabilirsiniz.";
            dict_tr["pack_selected_prefix"] = "Se\u00E7ilen uygulama paketi: {0}";
            dict_tr["logs_title"] = "Kurulum Ge\u00E7mi\u015Fi";
            dict_tr["logs_subtitle"] = "Canl\u0131 da\u011F\u0131t\u0131m izleme ve denetim g\u00FCnl\u00FCkleri.";
            dict_tr["btn_summary_report"] = "\u00D6zet Rapor";
            dict_tr["btn_open_logfile"] = "G\u00FCnl\u00FCk Dosyas\u0131n\u0131 A\u00E7";
            dict_tr["btn_clear_screen"] = "Ekran\u0131 Temizle";
            _translations["tr"] = dict_tr;

            // Language: nl
            Dictionary<string, string> dict_nl = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            dict_nl["btn_select_pack"] = "Pakket selecteren";
            dict_nl["btn_customize_pack"] = "Aanpassen";
            dict_nl["btn_export_pack"] = "Exporteren";
            dict_nl["nav_quick_setup"] = "Snelle installatie";
            dict_nl["nav_all_apps"] = "Alle apps";
            dict_nl["nav_cloud_sync"] = "Cloudsynchronisatie";
            dict_nl["nav_history"] = "Installatiegeschiedenis";
            dict_nl["search_placeholder"] = "Zoek app, instelling of pakket...";
            dict_nl["help_button"] = "Help ?";
            dict_nl["lang_auto"] = "Automatisch (Systeemstandaard)";
            dict_nl["menu_guide"] = "Hoe te gebruiken (Handleiding)...";
            dict_nl["menu_updates"] = "Controleren op updates...";
            dict_nl["menu_settings"] = "Instellingen...";
            dict_nl["menu_about"] = "Over FleetForge...";
            dict_nl["quick_setup_title"] = "Snelle installatie";
            dict_nl["quick_setup_subtitle"] = "Kies een pakket, selecteer apps en begin met \u00E9\u00E9n klik.";
            dict_nl["step1_title"] = "Kies een pakket";
            dict_nl["step1_desc"] = "Kies naar behoefte";
            dict_nl["step2_title"] = "Apps bekijken";
            dict_nl["step2_desc"] = "Selectie controleren";
            dict_nl["step3_title"] = "Installeren";
            dict_nl["step3_desc"] = "Beginnen met gebruiken";
            dict_nl["available_packs_title"] = "Beschikbare app-pakketten";
            dict_nl["available_packs_subtitle"] = "Kies een kant-en-klaar pakket of kies individuele apps.";
            dict_nl["top_choice_badge"] = "TOPKEUZE";
            dict_nl["recommended_badge"] = "AANBEVOLEN";
            dict_nl["custom_badge"] = "AANGEPAST PAKKET";
            dict_nl["partner_apps_count"] = "{0} partner-apps";
            dict_nl["apps_count"] = "{0} apps";
            dict_nl["catalog_title"] = "Alle applicaties";
            dict_nl["catalog_subtitle"] = "Blader door 16.349 softwarepakketten met direct zoeken en \u00E9\u00E9n-klik selectie.";
            dict_nl["select_all"] = "Selecteer alle apps";
            dict_nl["sort_button"] = "Apps sorteren \u25BE";
            dict_nl["sort_selected_top"] = "Geselecteerd en aanbevolen";
            dict_nl["sort_name_az"] = "Naam (A tot Z)";
            dict_nl["sort_name_za"] = "Naam (Z tot A)";
            dict_nl["sort_category"] = "Op categorie";
            dict_nl["cat_all"] = "Alles";
            dict_nl["cat_browsers"] = "Webbrowsers";
            dict_nl["cat_dev"] = "Ontwikkeling & Code";
            dict_nl["cat_collab"] = "Communicatie & Samenwerking";
            dict_nl["cat_office"] = "Productiviteit & Kantoor";
            dict_nl["cat_tools"] = "Hulpprogramma's & Tools";
            dict_nl["cat_media"] = "Media & Creatief";
            dict_nl["cat_sec"] = "Beveiliging & Netwerk";
            dict_nl["cat_cloud"] = "Cloud & Opslag";
            dict_nl["cat_games"] = "Gaming & Engines";
            dict_nl["btn_install"] = "\uD83D\uDCE5  Geselecteerde apps installeren ({0})";
            dict_nl["btn_install_no_count"] = "\uD83D\uDCE5  Geselecteerde apps installeren";
            dict_nl["btn_stop"] = "Installatie stoppen";
            dict_nl["btn_import_pack"] = "\uD83D\uDCE5 Pakket importeren";
            dict_nl["btn_create_pack"] = "+ Nieuw pakket maken";
            dict_nl["badge_ready"] = "Gereed";
            dict_nl["badge_installed"] = "Ge\u00EFnstalleerd";
            dict_nl["badge_installing"] = "Bezig met installeren...";
            dict_nl["badge_failed"] = "Mislukt";
            dict_nl["showing_count"] = "{0:N0} van {1:N0} apps worden weergegeven";
            dict_nl["showing_count_selected"] = "{0:N0} van {1:N0} apps worden weergegeven ({2:N0} geselecteerd)";
            dict_nl["showing_local_online"] = "Weergeven van {0} lokale + {1} online apps voor \"{2}\"";
            dict_nl["showing_local_online_selected"] = "Weergeven van {0} lokale + {1} online apps voor \"{2}\" ({3:N0} geselecteerd)";
            dict_nl["found_online_count"] = "{0:N0} online apps gevonden voor \"{1}\"";
            dict_nl["found_online_count_selected"] = "{0:N0} online apps gevonden voor \"{1}\" ({2:N0} geselecteerd)";
            dict_nl["showing_query_count"] = "{0:N0} apps weergegeven voor \"{1}\"";
            dict_nl["showing_query_count_selected"] = "{0:N0} apps weergegeven voor \"{1}\" ({2:N0} geselecteerd)";
            dict_nl["searching_online"] = "\uD83D\uDD0D Online Winget-opslagplaats doorzoeken voor \"{0}\"...";
            dict_nl["no_apps_found"] = "Geen softwarepakketten gevonden voor \"{0}\"";
            dict_nl["no_apps_found_desc"] = "Niet gevonden in lokale database of Winget. Controleer spelling.";
            dict_nl["type_to_search"] = "Typ minimaal 2 tekens om te zoeken.";
            dict_nl["no_winget_title"] = "Geen lokale apps gevonden voor \"{0}\"";
            dict_nl["no_winget_desc"] = "Microsoft Winget is niet beschikbaar voor online zoeken.";
            dict_nl["bottom_tip"] = "Tip: U kunt elk pakket aanpassen, apps toevoegen of verwijderen.";
            dict_nl["pack_selected_prefix"] = "Geselecteerd app-pakket: {0}";
            dict_nl["logs_title"] = "Installatiegeschiedenis";
            dict_nl["logs_subtitle"] = "Live implementatiebewaking en auditlogboeken.";
            dict_nl["btn_summary_report"] = "Samenvattend rapport";
            dict_nl["btn_open_logfile"] = "Logbestand openen";
            dict_nl["btn_clear_screen"] = "Scherm wissen";
            _translations["nl"] = dict_nl;

            // Language: pl
            Dictionary<string, string> dict_pl = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            dict_pl["btn_select_pack"] = "Wybierz pakiet";
            dict_pl["btn_customize_pack"] = "Dostosuj";
            dict_pl["btn_export_pack"] = "Eksportuj";
            dict_pl["nav_quick_setup"] = "Szybka konfiguracja";
            dict_pl["nav_all_apps"] = "Wszystkie aplikacje";
            dict_pl["nav_cloud_sync"] = "Synchronizacja z chmur\u0105";
            dict_pl["nav_history"] = "Historia instalacji";
            dict_pl["search_placeholder"] = "Znajd\u017A aplikacj\u0119, ustawienie lub pakiet...";
            dict_pl["help_button"] = "Pomoc ?";
            dict_pl["lang_auto"] = "Automatyczny (Domy\u015Blny systemu)";
            dict_pl["menu_guide"] = "Jak u\u017Cywa\u0107 (Przewodnik)...";
            dict_pl["menu_updates"] = "Sprawd\u017A aktualizacje...";
            dict_pl["menu_settings"] = "Ustawienia...";
            dict_pl["menu_about"] = "O programie FleetForge...";
            dict_pl["quick_setup_title"] = "Szybka konfiguracja";
            dict_pl["quick_setup_subtitle"] = "Wybierz pakiet, wska\u017C aplikacje i zacznij jednym klikni\u0119ciem.";
            dict_pl["step1_title"] = "Wybierz pakiet";
            dict_pl["step1_desc"] = "Wed\u0142ug potrzeb";
            dict_pl["step2_title"] = "Przejrzyj aplikacje";
            dict_pl["step2_desc"] = "Sprawd\u017A wyb\u00F3r";
            dict_pl["step3_title"] = "Zainstaluj";
            dict_pl["step3_desc"] = "Zacznij u\u017Cywa\u0107";
            dict_pl["available_packs_title"] = "Dost\u0119pne pakiety aplikacji";
            dict_pl["available_packs_subtitle"] = "Wybierz gotowy pakiet lub pojedyncze aplikacje.";
            dict_pl["top_choice_badge"] = "NAJLEPSZY WYB\u00D3R";
            dict_pl["recommended_badge"] = "POLECANE";
            dict_pl["custom_badge"] = "PAKIET NIESTANDARDOWY";
            dict_pl["partner_apps_count"] = "{0} aplikacji partnerskich";
            dict_pl["apps_count"] = "{0} aplikacji";
            dict_pl["catalog_title"] = "Wszystkie aplikacje";
            dict_pl["catalog_subtitle"] = "Przegl\u0105daj 16 349 pakiet\u00F3w oprogramowania z natychmiastowym wyszukiwaniem i wyborem jednym klikni\u0119ciem.";
            dict_pl["select_all"] = "Zaznacz wszystkie";
            dict_pl["sort_button"] = "Sortuj aplikacje \u25BE";
            dict_pl["sort_selected_top"] = "Zaznaczone i polecane";
            dict_pl["sort_name_az"] = "Nazwa (A do Z)";
            dict_pl["sort_name_za"] = "Nazwa (Z do A)";
            dict_pl["sort_category"] = "Wed\u0142ug kategorii";
            dict_pl["cat_all"] = "Wszystkie";
            dict_pl["cat_browsers"] = "Przegl\u0105darki internetowe";
            dict_pl["cat_dev"] = "Programowanie i kod";
            dict_pl["cat_collab"] = "Komunikacja i wsp\u00F3\u0142praca";
            dict_pl["cat_office"] = "Biuro i produktywno\u015B\u0107";
            dict_pl["cat_tools"] = "Narz\u0119dzia i u\u0142atwienia";
            dict_pl["cat_media"] = "Media i tw\u00F3rczo\u015B\u0107";
            dict_pl["cat_sec"] = "Bezpiecze\u0144stwo i sie\u0107";
            dict_pl["cat_cloud"] = "Chmura i pami\u0119\u0107";
            dict_pl["cat_games"] = "Gry i silniki";
            dict_pl["btn_install"] = "\uD83D\uDCE5  Zainstaluj wybrane aplikacje ({0})";
            dict_pl["btn_install_no_count"] = "\uD83D\uDCE5  Zainstaluj wybrane aplikacje";
            dict_pl["btn_stop"] = "Zatrzymaj instalacj\u0119";
            dict_pl["btn_import_pack"] = "\uD83D\uDCE5 Importuj pakiet";
            dict_pl["btn_create_pack"] = "+ Utw\u00F3rz nowy pakiet";
            dict_pl["badge_ready"] = "Gotowy";
            dict_pl["badge_installed"] = "Zainstalowano";
            dict_pl["badge_installing"] = "Instalowanie...";
            dict_pl["badge_failed"] = "Niepowodzenie";
            dict_pl["showing_count"] = "Wy\u015Bwietlanie {0:N0} z {1:N0} aplikacji";
            dict_pl["showing_count_selected"] = "Wy\u015Bwietlanie {0:N0} z {1:N0} aplikacji ({2:N0} zaznaczonych)";
            dict_pl["showing_local_online"] = "Wy\u015Bwietlanie {0} lokalnych + {1} online dla \"{2}\"";
            dict_pl["showing_local_online_selected"] = "Wy\u015Bwietlanie {0} lokalnych + {1} online dla \"{2}\" ({3:N0} zaznaczonych)";
            dict_pl["found_online_count"] = "Znaleziono {0:N0} aplikacji online dla \"{1}\"";
            dict_pl["found_online_count_selected"] = "Znaleziono {0:N0} aplikacji online dla \"{1}\" ({2:N0} zaznaczonych)";
            dict_pl["showing_query_count"] = "Wy\u015Bwietlanie {0:N0} aplikacji dla \"{1}\"";
            dict_pl["showing_query_count_selected"] = "Wy\u015Bwietlanie {0:N0} aplikacji dla \"{1}\" ({2:N0} zaznaczonych)";
            dict_pl["searching_online"] = "\uD83D\uDD0D Wyszukiwanie w repozytorium online Winget dla \"{0}\"...";
            dict_pl["no_apps_found"] = "Nie znaleziono pakiet\u00F3w oprogramowania dla \"{0}\"";
            dict_pl["no_apps_found_desc"] = "Nie znaleziono w lokalnej bazie ani w repozytorium Winget. Sprawd\u017A pisowni\u0119.";
            dict_pl["type_to_search"] = "Wpisz co najmniej 2 znaki, aby wyszuka\u0107.";
            dict_pl["no_winget_title"] = "Nie znaleziono lokalnych aplikacji dla \"{0}\"";
            dict_pl["no_winget_desc"] = "Microsoft Winget nie jest dost\u0119pny w tym systemie.";
            dict_pl["bottom_tip"] = "Wskaz\u00F3wka: Mo\u017Cesz dostosowa\u0107 dowolny pakiet, dodawa\u0107 lub usuwa\u0107 aplikacje.";
            dict_pl["pack_selected_prefix"] = "Wybrany pakiet aplikacji: {0}";
            dict_pl["logs_title"] = "Historia instalacji";
            dict_pl["logs_subtitle"] = "Monitorowanie wdro\u017Cenia na \u017Cywo i dzienniki audytu.";
            dict_pl["btn_summary_report"] = "Raport podsumowuj\u0105cy";
            dict_pl["btn_open_logfile"] = "Otw\u00F3rz plik dziennika";
            dict_pl["btn_clear_screen"] = "Wyczy\u015B\u0107 ekran";
            _translations["pl"] = dict_pl;

            // Language: vi
            Dictionary<string, string> dict_vi = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            dict_vi["btn_select_pack"] = "Ch\u1ECDn g\u00F3i";
            dict_vi["btn_customize_pack"] = "T\u00F9y ch\u1EC9nh";
            dict_vi["btn_export_pack"] = "Xu\u1EA5t g\u00F3i";
            dict_vi["nav_quick_setup"] = "Thi\u1EBFt l\u1EADp nhanh";
            dict_vi["nav_all_apps"] = "T\u1EA5t c\u1EA3 \u1EE9ng d\u1EE5ng";
            dict_vi["nav_cloud_sync"] = "\u0110\u1ED3ng b\u1ED9 \u0111\u00E1m m\u00E2y";
            dict_vi["nav_history"] = "L\u1ECBch s\u1EED c\u00E0i \u0111\u1EB7t";
            dict_vi["search_placeholder"] = "T\u00ECm \u1EE9ng d\u1EE5ng, c\u00E0i \u0111\u1EB7t ho\u1EB7c g\u00F3i...";
            dict_vi["help_button"] = "Tr\u1EE3 gi\u00FAp ?";
            dict_vi["lang_auto"] = "T\u1EF1 \u0111\u1ED9ng (M\u1EB7c \u0111\u1ECBnh h\u1EC7 th\u1ED1ng)";
            dict_vi["menu_guide"] = "H\u01B0\u1EDBng d\u1EABn s\u1EED d\u1EE5ng t\u01B0\u01A1ng t\u00E1c...";
            dict_vi["menu_updates"] = "Ki\u1EC3m tra c\u1EADp nh\u1EADt...";
            dict_vi["menu_settings"] = "C\u00E0i \u0111\u1EB7t...";
            dict_vi["menu_about"] = "V\u1EC1 FleetForge...";
            dict_vi["quick_setup_title"] = "Thi\u1EBFt l\u1EADp nhanh";
            dict_vi["quick_setup_subtitle"] = "Ch\u1ECDn g\u00F3i, ch\u1ECDn \u1EE9ng d\u1EE5ng v\u00E0 b\u1EAFt \u0111\u1EA7u ch\u1EC9 v\u1EDBi m\u1ED9t c\u00FA nh\u1EA5p chu\u1ED9t.";
            dict_vi["step1_title"] = "Ch\u1ECDn g\u00F3i";
            dict_vi["step1_desc"] = "Theo nhu c\u1EA7u";
            dict_vi["step2_title"] = "Xem l\u1EA1i \u1EE9ng d\u1EE5ng";
            dict_vi["step2_desc"] = "Ki\u1EC3m tra l\u1EF1a ch\u1ECDn";
            dict_vi["step3_title"] = "C\u00E0i \u0111\u1EB7t";
            dict_vi["step3_desc"] = "B\u1EAFt \u0111\u1EA7u d\u00F9ng";
            dict_vi["available_packs_title"] = "C\u00E1c g\u00F3i \u1EE9ng d\u1EE5ng c\u00F3 s\u1EB5n";
            dict_vi["available_packs_subtitle"] = "Ch\u1ECDn g\u00F3i c\u00F3 s\u1EB5n ho\u1EB7c ch\u1ECDn t\u1EEBng \u1EE9ng d\u1EE5ng ri\u00EAng l\u1EBB.";
            dict_vi["top_choice_badge"] = "L\u1EF0A CH\u1ECCN T\u1ED0T NH\u1EA4T";
            dict_vi["recommended_badge"] = "KHUY\u00CAN D\u00D9NG";
            dict_vi["custom_badge"] = "G\u00D3I T\u00D9Y CH\u1EC8NH";
            dict_vi["partner_apps_count"] = "{0} \u1EE9ng d\u1EE5ng \u0111\u1ED1i t\u00E1c";
            dict_vi["apps_count"] = "{0} \u1EE9ng d\u1EE5ng";
            dict_vi["catalog_title"] = "T\u1EA5t c\u1EA3 \u1EE9ng d\u1EE5ng";
            dict_vi["catalog_subtitle"] = "Duy\u1EC7t qua 16.349 g\u00F3i ph\u1EA7n m\u1EC1m v\u1EDBi t\u00ECm ki\u1EBFm t\u1EE9c th\u00EC v\u00E0 ch\u1ECDn m\u1ED9t c\u00FA nh\u1EA5p.";
            dict_vi["select_all"] = "Ch\u1ECDn t\u1EA5t c\u1EA3 \u1EE9ng d\u1EE5ng";
            dict_vi["sort_button"] = "S\u1EAFp x\u1EBFp \u1EE9ng d\u1EE5ng \u25BE";
            dict_vi["sort_selected_top"] = "\u0110\u00E3 ch\u1ECDn & Khuy\u00EAn d\u00F9ng";
            dict_vi["sort_name_az"] = "T\u00EAn (A \u0111\u1EBFn Z)";
            dict_vi["sort_name_za"] = "T\u00EAn (Z \u0111\u1EBFn A)";
            dict_vi["sort_category"] = "Theo danh m\u1EE5c";
            dict_vi["cat_all"] = "T\u1EA5t c\u1EA3";
            dict_vi["cat_browsers"] = "Tr\u00ECnh duy\u1EC7t Web";
            dict_vi["cat_dev"] = "Ph\u00E1t tri\u1EC3n & M\u00E3 ngu\u1ED3n";
            dict_vi["cat_collab"] = "Giao ti\u1EBFp & H\u1EE3p t\u00E1c";
            dict_vi["cat_office"] = "N\u0103ng su\u1EA5t & V\u0103n ph\u00F2ng";
            dict_vi["cat_tools"] = "Ti\u1EC7n \u00EDch & C\u00F4ng c\u1EE5";
            dict_vi["cat_media"] = "\u0110a ph\u01B0\u01A1ng ti\u1EC7n & S\u00E1ng t\u1EA1o";
            dict_vi["cat_sec"] = "B\u1EA3o m\u1EADt & M\u1EA1ng";
            dict_vi["cat_cloud"] = "\u0110\u00E1m m\u00E2y & L\u01B0u tr\u1EEF";
            dict_vi["cat_games"] = "Tr\u00F2 ch\u01A1i & C\u00F4ng c\u1EE5";
            dict_vi["btn_install"] = "\uD83D\uDCE5  C\u00E0i \u0111\u1EB7t c\u00E1c \u1EE9ng d\u1EE5ng \u0111\u00E3 ch\u1ECDn ({0})";
            dict_vi["btn_install_no_count"] = "\uD83D\uDCE5  C\u00E0i \u0111\u1EB7t c\u00E1c \u1EE9ng d\u1EE5ng \u0111\u00E3 ch\u1ECDn";
            dict_vi["btn_stop"] = "D\u1EEBng c\u00E0i \u0111\u1EB7t";
            dict_vi["btn_import_pack"] = "\uD83D\uDCE5 Nh\u1EADp g\u00F3i";
            dict_vi["btn_create_pack"] = "+ T\u1EA1o g\u00F3i m\u1EDBi";
            dict_vi["badge_ready"] = "S\u1EB5n s\u00E0ng";
            dict_vi["badge_installed"] = "\u0110\u00E3 c\u00E0i \u0111\u1EB7t";
            dict_vi["badge_installing"] = "\u0110ang c\u00E0i \u0111\u1EB7t...";
            dict_vi["badge_failed"] = "Th\u1EA5t b\u1EA1i";
            dict_vi["showing_count"] = "\u0110ang hi\u1EC3n th\u1ECB {0:N0} tr\u00EAn t\u1ED5ng s\u1ED1 {1:N0} \u1EE9ng d\u1EE5ng";
            dict_vi["showing_count_selected"] = "\u0110ang hi\u1EC3n th\u1ECB {0:N0} tr\u00EAn t\u1ED5ng s\u1ED1 {1:N0} \u1EE9ng d\u1EE5ng ({2:N0} \u0111\u00E3 ch\u1ECDn)";
            dict_vi["showing_local_online"] = "Hi\u1EC3n th\u1ECB {0} c\u1EE5c b\u1ED9 + {1} tr\u1EF1c tuy\u1EBFn cho \"{2}\"";
            dict_vi["showing_local_online_selected"] = "Hi\u1EC3n th\u1ECB {0} c\u1EE5c b\u1ED9 + {1} tr\u1EF1c tuy\u1EBFn cho \"{2}\" ({3:N0} \u0111\u00E3 ch\u1ECDn)";
            dict_vi["found_online_count"] = "T\u00ECm th\u1EA5y {0:N0} \u1EE9ng d\u1EE5ng tr\u1EF1c tuy\u1EBFn cho \"{1}\"";
            dict_vi["found_online_count_selected"] = "T\u00ECm th\u1EA5y {0:N0} \u1EE9ng d\u1EE5ng tr\u1EF1c tuy\u1EBFn cho \"{1}\" ({2:N0} \u0111\u00E3 ch\u1ECDn)";
            dict_vi["showing_query_count"] = "Hi\u1EC3n th\u1ECB {0:N0} \u1EE9ng d\u1EE5ng ph\u00F9 h\u1EE3p cho \"{1}\"";
            dict_vi["showing_query_count_selected"] = "Hi\u1EC3n th\u1ECB {0:N0} \u1EE9ng d\u1EE5ng ph\u00F9 h\u1EE3p cho \"{1}\" ({2:N0} \u0111\u00E3 ch\u1ECDn)";
            dict_vi["searching_online"] = "\uD83D\uDD0D \u0110ang t\u00ECm ki\u1EBFm tr\u1EF1c tuy\u1EBFn trong kho Winget cho \"{0}\"...";
            dict_vi["no_apps_found"] = "Kh\u00F4ng t\u00ECm th\u1EA5y g\u00F3i ph\u1EA7n m\u1EC1m n\u00E0o cho \"{0}\"";
            dict_vi["no_apps_found_desc"] = "Kh\u00F4ng t\u00ECm th\u1EA5y trong c\u01A1 s\u1EDF d\u1EEF li\u1EC7u c\u1EE5c b\u1ED9 ho\u1EB7c Winget tr\u1EF1c tuy\u1EBFn. Vui l\u00F2ng ki\u1EC3m tra ch\u00EDnh t\u1EA3.";
            dict_vi["type_to_search"] = "Vui l\u00F2ng nh\u1EADp \u00EDt nh\u1EA5t 2 k\u00FD t\u1EF1 \u0111\u1EC3 t\u00ECm ki\u1EBFm \u1EE9ng d\u1EE5ng.";
            dict_vi["no_winget_title"] = "Kh\u00F4ng t\u00ECm th\u1EA5y \u1EE9ng d\u1EE5ng c\u1EE5c b\u1ED9 n\u00E0o cho \"{0}\"";
            dict_vi["no_winget_desc"] = "Microsoft Winget kh\u00F4ng kh\u1EA3 d\u1EE5ng tr\u00EAn h\u1EC7 th\u1ED1ng n\u00E0y.";
            dict_vi["bottom_tip"] = "M\u1EB9o: B\u1EA1n c\u00F3 th\u1EC3 t\u00F9y ch\u1EC9nh b\u1EA5t k\u1EF3 g\u00F3i n\u00E0o, th\u00EAm ho\u1EB7c x\u00F3a \u1EE9ng d\u1EE5ng.";
            dict_vi["pack_selected_prefix"] = "G\u00F3i \u1EE9ng d\u1EE5ng \u0111\u00E3 ch\u1ECDn: {0}";
            dict_vi["logs_title"] = "L\u1ECBch s\u1EED c\u00E0i \u0111\u1EB7t";
            dict_vi["logs_subtitle"] = "Gi\u00E1m s\u00E1t tri\u1EC3n khai tr\u1EF1c ti\u1EBFp v\u00E0 nh\u1EADt k\u00FD ki\u1EC3m to\u00E1n.";
            dict_vi["btn_summary_report"] = "B\u00E1o c\u00E1o t\u00F3m t\u1EAFt";
            dict_vi["btn_open_logfile"] = "M\u1EDF t\u1EC7p nh\u1EADt k\u00FD";
            dict_vi["btn_clear_screen"] = "X\u00F3a m\u00E0n h\u00ECnh";
            _translations["vi"] = dict_vi;

        }

        public static string DetectSystemLanguage()
        {
            try
            {
                string sysTwoLetter = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
                foreach (LanguageDefinition lang in SupportedLanguages)
                {
                    if (lang.Code.Equals(sysTwoLetter, StringComparison.OrdinalIgnoreCase))
                        return lang.Code;
                }
            }
            catch { }
            return "en";
        }

        public static LanguageDefinition GetLanguage(string code)
        {
            if (string.IsNullOrEmpty(code)) code = CurrentLanguage;
            if (code.Equals("auto", StringComparison.OrdinalIgnoreCase)) code = DetectSystemLanguage();
            foreach (LanguageDefinition lang in SupportedLanguages)
            {
                if (lang.Code.Equals(code, StringComparison.OrdinalIgnoreCase)) return lang;
            }
            return SupportedLanguages[0];
        }

        public static string T(string key, params object[] args)
        {
            if (_translations == null) InitializeTranslations();
            string lang = CurrentLanguage;
            if (string.IsNullOrEmpty(lang) || lang.Equals("auto", StringComparison.OrdinalIgnoreCase))
                lang = DetectSystemLanguage();

            Dictionary<string, string> dict = null;
            if (!_translations.TryGetValue(lang, out dict))
                _translations.TryGetValue("en", out dict);

            string val = null;
            if (dict == null || !dict.TryGetValue(key, out val))
            {
                Dictionary<string, string> enDict = null;
                if (_translations.TryGetValue("en", out enDict) && enDict.TryGetValue(key, out val))
                {
                    // found in fallback
                }
                else
                {
                    val = key;
                }
            }

            if (args != null && args.Length > 0)
            {
                try
                {
                    return string.Format(val, args);
                }
                catch
                {
                    return val;
                }
            }
            return val;
        }

        public static string GetLocalizedCategoryName(string catId, string defaultName)
        {
            if (string.IsNullOrEmpty(catId)) return defaultName;
            string c = catId.ToLowerInvariant();
            if (c == "all") return T("cat_all");
            if (c == "browsers" || c.Contains("browser")) return T("cat_browsers");
            if (c == "dev" || c.Contains("dev") || c.Contains("code")) return T("cat_dev");
            if (c == "communication" || c.Contains("collab") || c.Contains("chat")) return T("cat_collab");
            if (c == "office" || c.Contains("productiv")) return T("cat_office");
            if (c == "utilities" || c.Contains("tool")) return T("cat_tools");
            if (c == "media" || c.Contains("design") || c.Contains("creative")) return T("cat_media");
            if (c == "security" || c.Contains("secur") || c.Contains("privac")) return T("cat_sec");
            if (c == "cloud" || c.Contains("storage") || c.Contains("drive")) return T("cat_cloud");
            if (c == "gaming" || c.Contains("game")) return T("cat_games");
            return defaultName;
        }

        public static bool IsReadyBadgeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;
            string t = text.Trim();
            if (t.Equals("Ready", StringComparison.OrdinalIgnoreCase)) return true;
            if (_translations != null)
            {
                foreach (KeyValuePair<string, Dictionary<string, string>> pair in _translations)
                {
                    string val;
                    if (pair.Value != null && pair.Value.TryGetValue("badge_ready", out val))
                    {
                        if (t.Equals(val.Trim(), StringComparison.OrdinalIgnoreCase)) return true;
                    }
                }
            }
            return false;
        }

        // =========================================================================
        // EMBEDDED HIGH-RESOLUTION COUNTRY FLAGS & ICONS (BASE64 PNG)
        // =========================================================================
        private static Dictionary<string, string> _flagBase64 = null;
        private static readonly Dictionary<string, System.Windows.Media.Imaging.BitmapImage> _flagBitmaps = new Dictionary<string, System.Windows.Media.Imaging.BitmapImage>(StringComparer.OrdinalIgnoreCase);

        public static void InitializeFlags()
        {
            if (_flagBase64 != null) return;
            _flagBase64 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _flagBase64["us"] = "iVBORw0KGgoAAAANSUhEUgAAACgAAAAVBAMAAADGNLEtAAAALVBMVEX////GU3LZjKGzGUINM2PsxtAYPWotT3hed5enma46WoBPa46zGkMkR3Jrgp+6ntXWAAAAiklEQVQY02Nwc0lJcQMSxghgw+CRVdaWkVXWooQEGFxaPLxKWjxcGJCBn1fJExAWRAJglUuAKlEEPdyXgA1A0e62LTu9bFt2CopF3iCh9LItKE5yX5tVe31tVgmq9pL0NLcy95RQJMAAdiUQo2ifCQUoKrECY0xgw6CEBWDXLogFYBfECrBahM1JAItMTcEiMDpsAAAAAElFTkSuQmCC";
            _flagBase64["in"] = "iVBORw0KGgoAAAANSUhEUgAAACgAAAAbBAMAAAD8PtBdAAAAMFBMVEUEajj/aCD////f3/GpqNhPTa7MzOiOjMthX7f5+fxubLyFg8b4+Py+veF1dMD9/f41r+HyAAAATElEQVQoz2MQxAIYhoqgEhRoloZPgrHhgqZuKcEYgotNnK3QBfXb/J5koAsq722NuG2Ern2Z95YsDDOLTZzNMQRPuKX0YAiiuHMoAwA5/S6KfoLQ6QAAAABJRU5ErkJggg==";
            _flagBase64["es"] = "iVBORw0KGgoAAAANSUhEUgAAACgAAAAbCAMAAAA5zj1cAAAApVBMVEX6vQCtFRnBPxP3tgCkSRTxsQC9miCtkjL3ugDcdwrVkwndpwauUEi3k3ecLQyvGx01YomtXy3Zhha+gw+4LiCzn4jKeBKyYgjmoAOkXUK/RhC1kJmmKh+0oKCbYwN5OAWWHxSVi0HypQNcdGsDAgC9g1j6vgSWhnLfsSXCn169cE24eBuaNwzQaiOYagW8bxWIOhqoKhPLdq4yUHR+UgVZLWaBHkAs3esWAAAAk0lEQVQ4y+WTVxLDIAxESQwBAwb33tKc3sv9j5YbWJp8JvurNzsrzYpMkCI/BU6RIt9phqIcJSOpEGBu7c2WA+iXOWmbtTmTACh42PdelSgwp+d2nXdvTjuAc3X5vL6qJo4XQEZ9qd8PXaR7yDE5GFOf6XbNRrmB8cgcKaUraJm5z4NiE4RLH3FyIUaG6Pb85XN9AIg9CvrV7UGSAAAAAElFTkSuQmCC";
            _flagBase64["fr"] = "iVBORw0KGgoAAAANSUhEUgAAACgAAAAbBAMAAAD8PtBdAAAAD1BMVEUAJlT////OESbvr7aqtsYrjF9NAAAAGUlEQVQoz2NgAAMWQTAwUgIDhlHBUUG8ggBpKyX5efKDZQAAAABJRU5ErkJggg==";
            _flagBase64["de"] = "iVBORw0KGgoAAAANSUhEUgAAACgAAAAYAgMAAAD16ldTAAAACVBMVEUAAADdAAD/zgDGIigcAAAAFUlEQVQY02NgoBEIhQPqMlfBAVWZANYST7G6bxb2AAAAAElFTkSuQmCC";
            _flagBase64["ru"] = "iVBORw0KGgoAAAANSUhEUgAAACgAAAAbAgMAAABzfiX9AAAACVBMVEUAOabVKx7///95ANL1AAAAFUlEQVQY02NYBQcMtGLSAYTCAa2YAJQLWaccQRfwAAAAAElFTkSuQmCC";
            _flagBase64["cn"] = "iVBORw0KGgoAAAANSUhEUgAAACgAAAAbCAMAAAA5zj1cAAAAOVBMVEXuHCX3jRL//wDuISPyURvvLCH6ugrwPh7xSRz0ZxjwNyD1ehX+6QP7wgn/+AH5ow793wX3lhHzXBqicYSuAAAAX0lEQVQYGe3BSRKCQBAAweplVgYB//9YY/QMdHgmk7+kxB1janvhhjD5mzvD+XHn3CKbriIJsG3lXMqqOipTMa4MVSegvo5d+KqdC61gwmQ5GyG9E+ALMS0XYhKPR9wHPPwBnsz8tAoAAAAASUVORK5CYII=";
            _flagBase64["jp"] = "iVBORw0KGgoAAAANSUhEUgAAACgAAAAbCAMAAAA5zj1cAAAAOVBMVEW8AC3dfJPMPF7ILFHQTGvuvcjADzn////55urjkaXyzte9BTHqrbvDHEP++fr77/Hst8PmobHXZ4H4lhdsAAAAcUlEQVQ4y9XTuw6AIAyF4XKVclXf/2FNjDF0OMjgwj9/YWgLbZPRkjA4Zhc+YbB0Z8MYHp6efB7BmOgtRQyroQ6aCmEm0Q6hklBBqCXUEHoJPYRm9kUr4QkhS8gQttK70vBmHM1tRkyS/7me+Xtc+hdeTWcZDIhvaeAAAAAASUVORK5CYII=";
            _flagBase64["br"] = "iVBORw0KGgoAAAANSUhEUgAAACgAAAAcCAMAAAAkyw3kAAAAkFBMVEUAlED/ywAwJoEOlzwemjdQSI83oDHjxQZKpC2fthhGN3NAM3VZU5fywAjEmyQ/NYf5yQFoU101K4OVd0OvuRRzdKTSwQuOshx8ryFjqifCvQ+p0MFGP4y8w8q+0tKXrLjwyATrxwXltRC+lyhkXp+JdW12XlWkgji0uM+rqnXr8vDS593K4tmZflSJj7ROQ3x/jyqZAAAA70lEQVQ4y82T2RKCMAxFTaEKBSwUcQEUF9y3//87IwVxpEp9cbyPmTNZbpJO56/U7WphxghgZLRzVg9QPasFM20oZX9MOpmDFwaOE4QeLCbv08UAfUYKsT5AbKq5sY8ceQhJf6zyZIiNeawGmYeB4atThhxiPzsektku4i6SW8VQ0hOAtStE6kZZMkVyI2NPTpmDypOqMo2SLGVVdGA2QAchLtnLKWqAdemAins+eifzaaN0PUzICTYniurpWb2hwp6lQzjFjJzQnC5V9lSGr8ppXC5Wbwx/WeH1wwr1j+KLM9M/XPkKvs4r6D/Xz3QDO1IMs+uu/zEAAAAASUVORK5CYII=";
            _flagBase64["sa"] = "iVBORw0KGgoAAAANSUhEUgAAACgAAAAbCAMAAAA5zj1cAAAAQlBMVEUAVDBvnoohakoTYD9bkHkGWDU7e2ACVTFDgWaBq5kmbU95pZJRinFVjXU1d1pLhm2Os6OiwbMtclRkl4EKWje50MYuwG0pAAAAnklEQVQ4y+XTuQ4DMQgEULABj+9r9/9/NV0qK0qRajP1k9AgoEBfJdBDoRJdpD5cpEtTUtJ0HWEraXNp1sCoq0ssUo/QbY3ZyoZUKa5ice9H2GXbkI6SknCrSFHkCCtszlmQMTJ2he98Hg0Doy1nc9y5GXuzcoQ2B3oemfdwbQgyO3feYwruIu+8Nu8V60FHoZ/K+BzfueOUf3iFH8EXincH0Y/Lci4AAAAASUVORK5CYII=";
            _flagBase64["it"] = "iVBORw0KGgoAAAANSUhEUgAAACgAAAAbBAMAAAD8PtBdAAAAElBMVEXOKzcAkkb///+q28EBkkbvt7tlcfnnAAAAGUlEQVQoz2NwFAQBYSUwCGCAgFHBUUG8ggBTFC/I4rFoIQAAAABJRU5ErkJggg==";
            _flagBase64["kr"] = "iVBORw0KGgoAAAANSUhEUgAAACgAAAAbCAMAAAA5zj1cAAAAvVBMVEX///8AR6DNLjrt7e6cnJwKCgoaGhrY2NiMjIxAQEB5eXmvr6+8vLyEhIQdQ5IgICCYmJivMUn59vfExMQ6OjrGxsaJiYlkZGReXl5wcHDQR1P55OYMT6TyyczH1ur+/f0RERGVlZWhoaEhISFoiLzZ4/HmvsZnOWx7n84HRZzlk5rOND/FLj3109XOMT3Ozs5HR0ff398zMzMgXqyFNl3dcXmWkrfkjpVKPXvCgpUmYq65zOXIqLuwvtqfudvB4UIoAAABKklEQVQ4y7WTWXeCMBCFmZCEQEBRW8oiKuC+Vbvv//9nFdLIaRPl+GDvS3KG73CTmRvD+D9Zpl4zLb02vHN6aq3nXOkgawfU/fNT06WsjVVuTzjB5PZ36ZpkJA9UczvahXb0s082XbHe2OFONccFpgGtvifLGCH0mFTeDqe5Yo75yCduuUlXSGiVCnOfcuWUlmeH5TLdIqltKsw9vUGsvHI3RrXiypydaPr0QHXWCG0apvMksRYsZmjZAI4fJFfq5bkBnMNsLTmA1wZwALCQGMDHCai6tfEGtVrDo7eWfRz0a/DzaB/rydTkVxUz4o+UyeQFo5mYtTm/B+iP38WsXZqpQbMjzg7pmUwMmR7GtejuyzT6ah59grU8Gqzw9IR7esLPfzNnv8KL6RtMsRLeZtvj3wAAAABJRU5ErkJggg==";
            _flagBase64["tr"] = "iVBORw0KGgoAAAANSUhEUgAAACgAAAAbCAMAAAA5zj1cAAAARVBMVEXjChf70dPqRlDnMDvnLDflIi383N7jCxj////kFCHmJjL4v8Pwe4LvbnbtZW3sUlv+8PHlHSn0naL3trrpO0XyjZPyiZBeDlMuAAAAjUlEQVQ4y+WTyxKDIAwAw0NYQETR9v8/tQenehHl3ObE7OxAQhLx0hVeflZctOkQ/egAlf2DODhQU1zKdC/6BHk/xVvxBfWgw9AUPRAOapNYcy0aUCfVzC5ciwXWLwsVKI2nI7iTbrNTjRtl7sxRRnj3VC2SYP/qYB86o6BuIY65r9er7pse+4c7cyd+AOKjBXOHX0j9AAAAAElFTkSuQmCC";
            _flagBase64["nl"] = "iVBORw0KGgoAAAANSUhEUgAAACgAAAAbAgMAAABzfiX9AAAACVBMVEX///8hRouuHChfbmbQAAAAFUlEQVQY02NYBQcMtGLSAYTCAa2YAJQLWaccQRfwAAAAAElFTkSuQmCC";
            _flagBase64["pl"] = "iVBORw0KGgoAAAANSUhEUgAAACgAAAAZAgMAAAA+toT2AAAACVBMVEXcFDz////tip5hJkzaAAAAFElEQVQY02MIhQOGAWOuggOGQQAAG50ufcR4g+sAAAAASUVORK5CYII=";
            _flagBase64["vn"] = "iVBORw0KGgoAAAANSUhEUgAAACgAAAAbBAMAAAD8PtBdAAAAMFBMVEX/AAD/6wD/1QD/OAD/ggD/SgD/nAD/4QD/GgD/6gD/WgD/CQD/vAD/kwD/IgD/QQDiVXETAAAAY0lEQVQoz2NgoDvg3oBFkDUAiyBbAhbB7dVYBANFsQhelEUX6REEgxOoop4gsSloSvkVBQWFDNAN6BQUnIFhj6GgoDCGoKOQogiGoGL2NiF0MeYqBobl6BZxPWBg4FvAMGgBAGCuDd0kl5lmAAAAAElFTkSuQmCC";
            _flagBase64["globe"] = "iVBORw0KGgoAAAANSUhEUgAAACgAAAAcCAYAAAATFf3WAAABJElEQVR4nO2Xuw3CMBCGz4aCGkRJicQO7MAQ9LiEGaAMPUOwAzsgUVIiqNMgUBCHjBXL94iiBPE3Vpyz/fk/+QXwl04GKtBodnwU5XI1/tRt1qdXed5PVGOYKsBCuBBSA2q0YD5ctruBm/chBikBtaCA6w0HX3C+8Lv4X8SVta8cMITjSAppNXDunc7QPRTWY5wEkp1irnPa9iRAnK0WDoX9UFxkO4hKpTeWZq5s3e5xXRQ7WJcsNFwmFbA93Fkbq0SLace01sFuKgDP0nCRxFaxK6mPxeaXaxKw8Q7aVADePiiz5Qj7S91uxA5SN2BH3NBVgFW7mBPdEzmohcyZ7cmA/mxxkCyR5jC9Phz1Zs1ysAySKglcK94kBn7xVVfnuxiarifVgqNgXFZFywAAAABJRU5ErkJggg==";
        }

        public static System.Windows.Media.Imaging.BitmapImage GetFlagBitmap(string code)
        {
            if (string.IsNullOrEmpty(code)) code = CurrentLanguage;
            if (code.Equals("auto", StringComparison.OrdinalIgnoreCase)) code = "globe";
            string c = code.ToLowerInvariant();
            if (c == "en") c = "us";
            else if (c == "hi") c = "in";
            else if (c == "zh") c = "cn";
            else if (c == "ja") c = "jp";
            else if (c == "pt") c = "br";
            else if (c == "ar") c = "sa";
            else if (c == "ko") c = "kr";
            else if (c == "vi") c = "vn";

            System.Windows.Media.Imaging.BitmapImage cached;
            if (_flagBitmaps.TryGetValue(c, out cached)) return cached;

            if (_flagBase64 == null) InitializeFlags();
            string b64;
            if (_flagBase64 != null && _flagBase64.TryGetValue(c, out b64))
            {
                try
                {
                    byte[] bytes = Convert.FromBase64String(b64);
                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream(bytes))
                    {
                        System.Windows.Media.Imaging.BitmapImage bmp = new System.Windows.Media.Imaging.BitmapImage();
                        bmp.BeginInit();
                        bmp.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                        bmp.StreamSource = ms;
                        bmp.EndInit();
                        bmp.Freeze();
                        _flagBitmaps[c] = bmp;
                        return bmp;
                    }
                }
                catch { }
            }
            return null;
        }


        public static bool IsKnownPlaceholder(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return true;
            string t = text.Trim();
            if (t.Equals("Find an app, setting or pack...", StringComparison.OrdinalIgnoreCase) ||
                t.Equals("Search applications...", StringComparison.OrdinalIgnoreCase))
                return true;
            if (_translations != null)
            {
                foreach (KeyValuePair<string, Dictionary<string, string>> pair in _translations)
                {
                    string val;
                    if (pair.Value != null && pair.Value.TryGetValue("search_placeholder", out val))
                    {
                        if (t.Equals(val.Trim(), StringComparison.OrdinalIgnoreCase)) return true;
                    }
                }
            }
            return false;
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
        public bool tutorialCompleted { get; set; }
        public string displayScale { get; set; }
        public string language { get; set; }

        public SettingsModel()
        {
            cloudConfigUrl = AppUrls.CloudSyncUrl;
            autoSyncOnLaunch = true;
            lastSyncTime = "Never";
            lastSyncStatus = "Local Cache";
            theme = "system";
            silent = true;
            accept_agreements = true;
            tutorialCompleted = false;
            displayScale = "Auto";
            language = "auto";
        }
    }

    public class AppRowView
    {
        public AppModel App { get; set; }
        public Border Card { get; set; }
        public CheckBox CheckBox { get; set; }
        public Border StatusBadge { get; set; }
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
            version = "2.7.0";
            versionCode = 270;
            buildDate = "2026-11-17";
            title = "FleetForge v2.7.0 Enterprise IT Edition";
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
        public string installMethod { get; set; } // "Direct Cloud Delivery", "FleetForge Engine"
        public int exitCode { get; set; }
        public double durationSeconds { get; set; }
        public string timestamp { get; set; }
        public string errorDetails { get; set; }

        public AppDeploymentRecord()
        {
            status = "Pending";
            installMethod = "FleetForge Engine";
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
                string localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string targetFolder = Path.Combine(localApp, "FleetForge");
                if (!Directory.Exists(targetFolder))
                {
                    try { Directory.CreateDirectory(targetFolder); } catch { }
                }

                string filePath = customPath;
                if (string.IsNullOrEmpty(filePath))
                {
                    string safeTime = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                    filePath = Path.Combine(targetFolder, string.Format("Deployment-Report-{0}.html", safeTime));
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
                    rows.Append(string.Format("<td>{0}</td>", System.Security.SecurityElement.Escape(r.installMethod ?? "FleetForge Cloud")));
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
  .badge-org {{ background: #1E293B; border: 1px solid #334155; color: #5f6d84; font-weight: 600; font-size: 11px; padding: 6px 12px; border-radius: 6px; text-align: right; }}
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
      <div style=""color:#5f6d84;font-weight:bold;"">LOVE MALHOTRA</div>
      <div style=""color:#94A3B8;font-weight:normal;font-size:10px;"">FleetForge v2.7.0 Engine</div>
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
          <th>Package Identifier</th>
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
                string localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string targetFolder = Path.Combine(localApp, "FleetForge");
                if (!Directory.Exists(targetFolder))
                {
                    try { Directory.CreateDirectory(targetFolder); } catch { }
                }

                string filePath = customPath;
                if (string.IsNullOrEmpty(filePath))
                {
                    string safeTime = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                    filePath = Path.Combine(targetFolder, string.Format("Deployment-Report-{0}.json", safeTime));
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

            // Title Line: FLEET + FORGE + v2.7.0
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
            txtVer.Text = "v2.7.0";
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
                "Initializing FleetForge Package Delivery Engine...",
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
            Console.WriteLine("  --apps <id1,id2..>   Comma-separated list of application IDs or packages to install");
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
            Console.WriteLine("  FleetForge Enterprise CLI Provisioning Engine v2.7.0");
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

                // 3-Tier Merge with Deduplication for CLI mode (Recommended -> Custom -> Base)
                HashSet<string> cliSeenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                HashSet<string> cliSeenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                List<AppModel> cliMergedApps = new List<AppModel>();

                string recPath = Path.Combine(baseDir, "data", "recommended_apps.json");
                if (!File.Exists(recPath)) recPath = Path.Combine(baseDir, "recommended_apps.json");
                if (File.Exists(recPath))
                {
                    try
                    {
                        string rJson = File.ReadAllText(recPath, Encoding.UTF8);
                        RecommendedDataModel rdm = ser.Deserialize<RecommendedDataModel>(rJson);
                        if (rdm != null && rdm.apps != null)
                        {
                            foreach (AppModel ra in rdm.apps)
                            {
                                if (ra != null && !string.IsNullOrEmpty(ra.id))
                                {
                                    string nKey = !string.IsNullOrEmpty(ra.name) ? ra.name.Trim().ToLowerInvariant() : null;
                                    if (!cliSeenIds.Contains(ra.id) && (nKey == null || !cliSeenNames.Contains(nKey)))
                                    {
                                        cliSeenIds.Add(ra.id);
                                        if (nKey != null) cliSeenNames.Add(nKey);
                                        cliMergedApps.Add(ra);
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                }

                string custPath = Path.Combine(baseDir, "data", "custom_apps.json");
                if (!File.Exists(custPath)) custPath = Path.Combine(baseDir, "custom_apps.json");
                if (File.Exists(custPath))
                {
                    try
                    {
                        string cJson = File.ReadAllText(custPath, Encoding.UTF8);
                        CustomCatalogModel ccm = ser.Deserialize<CustomCatalogModel>(cJson);
                        if (ccm != null && ccm.apps != null)
                        {
                            foreach (AppModel ca in ccm.apps)
                            {
                                if (ca != null && !string.IsNullOrEmpty(ca.id))
                                {
                                    string nKey = !string.IsNullOrEmpty(ca.name) ? ca.name.Trim().ToLowerInvariant() : null;
                                    if (!cliSeenIds.Contains(ca.id) && (nKey == null || !cliSeenNames.Contains(nKey)))
                                    {
                                        cliSeenIds.Add(ca.id);
                                        if (nKey != null) cliSeenNames.Add(nKey);
                                        cliMergedApps.Add(ca);
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                }

                if (config != null && config.apps != null)
                {
                    foreach (AppModel ba in config.apps)
                    {
                        if (ba != null && !string.IsNullOrEmpty(ba.id))
                        {
                            string nKey = !string.IsNullOrEmpty(ba.name) ? ba.name.Trim().ToLowerInvariant() : null;
                            if (!cliSeenIds.Contains(ba.id) && (nKey == null || !cliSeenNames.Contains(nKey)))
                            {
                                cliSeenIds.Add(ba.id);
                                if (nKey != null) cliSeenNames.Add(nKey);
                                cliMergedApps.Add(ba);
                            }
                        }
                    }
                    config.apps = cliMergedApps;
                }
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

            string localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appDataDir = Path.Combine(localApp, "FleetForge");
            if (!Directory.Exists(appDataDir)) { try { Directory.CreateDirectory(appDataDir); } catch { } }
            string logFilePath = Path.Combine(appDataDir, "install-log.txt");

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

                if (!string.IsNullOrEmpty(app.affiliateWebUrl))
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(app.affiliateWebUrl) { UseShellExecute = true });
                        Console.WriteLine("    [PARTNER] Opened partner discount/signup page in default browser: {0}", app.affiliateWebUrl);
                    }
                    catch { }
                }

                bool installed = false;

                if (!string.IsNullOrEmpty(app.downloadUrl))
                {
                    Console.WriteLine("    [DOWNLOAD] Downloading {0} setup package from direct URL...", app.name);
                    rec.installMethod = "Direct Cloud Delivery";
                    string targetExeName = Regex.Replace(app.name, @"[^\w]", "_") + (app.downloadUrl.EndsWith(".msi", StringComparison.OrdinalIgnoreCase) ? ".msi" : ".exe");

                    string downloadDir = Path.Combine(Path.GetTempPath(), "FleetForgeDownloads");
                    try { if (!Directory.Exists(downloadDir)) Directory.CreateDirectory(downloadDir); } catch { }
                    string downloadTarget = Path.Combine(downloadDir, targetExeName);

                    try
                    {
                        string directUrl = MainWindow.ConvertDirectUrl(app.downloadUrl);
                        using (WebClient wc = new WebClient())
                        {
                            wc.Encoding = Encoding.UTF8;
                            wc.Headers.Add("User-Agent", "FleetForge-Delivery-Engine/2.7.0");
                            wc.DownloadFile(directUrl, downloadTarget);
                        }

                        if (File.Exists(downloadTarget) && new FileInfo(downloadTarget).Length > 0)
                        {
                            Console.WriteLine("    [SETUP] Launching silent background installer for {0}...", app.name);
                            ProcessStartInfo psi;
                            bool isMsi = downloadTarget.EndsWith(".msi", StringComparison.OrdinalIgnoreCase);
                            if (isMsi)
                            {
                                string msiArgs = string.Format("/i \"{0}\" /qn /norestart {1}", downloadTarget, app.silentArgs ?? "").Trim();
                                psi = new ProcessStartInfo("msiexec.exe", msiArgs);
                            }
                            else
                            {
                                string exeArgs = !string.IsNullOrEmpty(app.silentArgs) ? app.silentArgs : "/S /silent /quiet /VERYSILENT /NORESTART";
                                psi = new ProcessStartInfo(downloadTarget, exeArgs);
                            }

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
                                    Console.WriteLine("    [SUCCESS] {0} installed successfully via direct setup.", app.name);
                                }
                                else
                                {
                                    rec.status = "Failed";
                                    rec.exitCode = p.ExitCode;
                                    Console.WriteLine("    [WARNING] {0} installer exited with code {1}.", app.name, p.ExitCode);
                                }
                            }
                        }
                        else
                        {
                            rec.status = "Failed";
                            rec.errorDetails = "Downloaded installer is empty or missing";
                        }
                    }
                    catch (Exception ex)
                    {
                        rec.status = "Failed";
                        rec.errorDetails = "Direct download failed: " + ex.Message;
                        Console.WriteLine("    [ERROR] Download failed for {0}: {1}", app.name, ex.Message);
                    }
                    finally
                    {
                        try { if (File.Exists(downloadTarget)) File.Delete(downloadTarget); } catch { }
                    }
                }

                if (!installed && rec.status != "Timeout")
                {
                    if (string.IsNullOrEmpty(wingetPath))
                    {
                        rec.status = "Failed";
                        rec.errorDetails = "Package Delivery Engine not found on system";
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
                                rec.installMethod = "FleetForge Engine";

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

    // =========================================================================
    // Windows 11 Fluent Toaster Notification Popup
    // =========================================================================
    public class Windows11ToastWindow : Window
    {
        private TextBlock txtTitle;
        private TextBlock txtDetail;
        private ProgressBar pbProgress;
        private TextBlock txtPercent;
        private TextBlock txtStatusTag;
        private Border rootBorder;
        private DispatcherTimer autoCloseTimer;
        private Action onRestoreCallback;
        private Action onDismissCallback;
        private bool isClosing = false;

        public Windows11ToastWindow(bool isLight, Action onRestore, Action onDismiss, System.Windows.Media.Color accentColor)
        {
            this.onRestoreCallback = onRestore;
            this.onDismissCallback = onDismiss;
            this.WindowStyle = WindowStyle.None;
            this.AllowsTransparency = true;
            this.Background = System.Windows.Media.Brushes.Transparent;
            this.ShowInTaskbar = false;
            this.Topmost = true;
            this.Width = 380;
            this.Height = 118;
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.FontFamily = new System.Windows.Media.FontFamily("Segoe UI Variable Text, Segoe UI Variable, Segoe UI, sans-serif");

            // Position at bottom-right of primary screen work area above taskbar
            Rect wa = SystemParameters.WorkArea;
            this.Left = wa.Right - this.Width - 16;
            this.Top = wa.Bottom - this.Height - 16;

            var bgBrush = isLight ?
                new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8F9FA")) :
                new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#202020"));
            var borderBrush = isLight ?
                new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#D1D5DB")) :
                new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#383838"));
            var textBrush = isLight ?
                new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#111827")) :
                new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F3F4F6"));
            var subtextBrush = isLight ?
                new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#6B7280")) :
                new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#9CA3AF"));
            var trackBrush = isLight ?
                new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E5E7EB")) :
                new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#333333"));
            var accentBrush = new SolidColorBrush(accentColor);

            rootBorder = new Border
            {
                Background = bgBrush,
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Cursor = Cursors.Hand,
                ToolTip = "Click to restore FleetForge",
                Effect = new DropShadowEffect
                {
                    Color = System.Windows.Media.Colors.Black,
                    Direction = 270,
                    ShadowDepth = 6,
                    BlurRadius = 24,
                    Opacity = 0.35
                }
            };

            rootBorder.MouseLeftButtonUp += (s, e) =>
            {
                if (isClosing) return;
                if (onRestoreCallback != null) onRestoreCallback();
                this.Close();
            };

            Grid mainGrid = new Grid { Margin = new Thickness(14, 10, 14, 12) };
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Row 0: App Identity Header
            Grid headerGrid = new Grid();
            StackPanel headerLeft = new StackPanel { Orientation = Orientation.Horizontal };

            Image imgIcon = new Image { Width = 16, Height = 16, VerticalAlignment = VerticalAlignment.Center };
            try
            {
                ImageSource iconSource = MainWindow.LoadApplicationIcon();
                if (iconSource != null) imgIcon.Source = iconSource;
            }
            catch { }
            headerLeft.Children.Add(imgIcon);

            TextBlock txtBrand = new TextBlock
            {
                Text = "FleetForge",
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = textBrush,
                Margin = new Thickness(7, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            headerLeft.Children.Add(txtBrand);

            TextBlock txtDot = new TextBlock
            {
                Text = "•",
                FontSize = 11,
                Foreground = subtextBrush,
                Margin = new Thickness(6, 0, 6, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            headerLeft.Children.Add(txtDot);

            txtStatusTag = new TextBlock
            {
                Text = "Background Setup",
                FontSize = 11,
                Foreground = accentBrush,
                VerticalAlignment = VerticalAlignment.Center
            };
            headerLeft.Children.Add(txtStatusTag);
            headerGrid.Children.Add(headerLeft);

            Button btnClose = new Button
            {
                Content = "✕",
                FontSize = 11,
                Foreground = subtextBrush,
                Background = System.Windows.Media.Brushes.Transparent,
                BorderThickness = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = Cursors.Hand,
                Padding = new Thickness(4, 2, 4, 2)
            };
            btnClose.Click += (s, e) =>
            {
                e.Handled = true;
                if (onDismissCallback != null) onDismissCallback();
                FadeOutAndClose();
            };
            headerGrid.Children.Add(btnClose);
            Grid.SetRow(headerGrid, 0);
            mainGrid.Children.Add(headerGrid);

            // Row 1: Content details
            StackPanel bodyPanel = new StackPanel { Margin = new Thickness(0, 6, 0, 6), VerticalAlignment = VerticalAlignment.Center };
            txtTitle = new TextBlock
            {
                Text = "Preparing installation...",
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = textBrush,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            bodyPanel.Children.Add(txtTitle);

            txtDetail = new TextBlock
            {
                Text = "Initializing background task...",
                FontSize = 11,
                Foreground = subtextBrush,
                Margin = new Thickness(0, 2, 0, 0),
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            bodyPanel.Children.Add(txtDetail);
            Grid.SetRow(bodyPanel, 1);
            mainGrid.Children.Add(bodyPanel);

            // Row 2: Fluid Progress Bar
            Grid progressGrid = new Grid { Margin = new Thickness(0, 2, 0, 0) };
            progressGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            progressGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            Border pbBorder = new Border
            {
                CornerRadius = new CornerRadius(2),
                ClipToBounds = true,
                Background = trackBrush,
                Height = 4,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };

            pbProgress = new ProgressBar
            {
                Height = 4,
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Background = System.Windows.Media.Brushes.Transparent,
                Foreground = accentBrush,
                BorderThickness = new Thickness(0)
            };
            pbBorder.Child = pbProgress;
            Grid.SetColumn(pbBorder, 0);
            progressGrid.Children.Add(pbBorder);

            txtPercent = new TextBlock
            {
                Text = "0%",
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = accentBrush,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(txtPercent, 1);
            progressGrid.Children.Add(txtPercent);

            Grid.SetRow(progressGrid, 2);
            mainGrid.Children.Add(progressGrid);

            rootBorder.Child = mainGrid;
            this.Content = rootBorder;

            this.Opacity = 0;
            this.Loaded += delegate
            {
                DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(220));
                this.BeginAnimation(Window.OpacityProperty, fadeIn);
            };
        }

        public void UpdateProgress(string title, string detail, int percent, bool isFinal = false, string statusTag = null)
        {
            if (isClosing) return;
            txtTitle.Text = title ?? "";
            txtDetail.Text = detail ?? "";
            pbProgress.Value = Math.Max(0, Math.Min(100, percent));
            txtPercent.Text = pbProgress.Value + "%";

            if (!string.IsNullOrEmpty(statusTag))
            {
                txtStatusTag.Text = statusTag;
                if (statusTag.IndexOf("Interrupted", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    txtStatusTag.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F59E0B"));
                }
                else if (statusTag.IndexOf("Fail", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    txtStatusTag.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#EF4444"));
                }
                else
                {
                    txtStatusTag.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#10B981"));
                }
            }
            else if (isFinal)
            {
                txtStatusTag.Text = "Completed";
                txtStatusTag.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#10B981"));
            }

            if (isFinal)
            {
                if (autoCloseTimer == null)
                {
                    autoCloseTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(9) };
                    autoCloseTimer.Tick += (s, e) =>
                    {
                        autoCloseTimer.Stop();
                        FadeOutAndClose();
                    };
                    autoCloseTimer.Start();
                }
            }
        }

        public void FadeOutAndClose()
        {
            if (isClosing) return;
            isClosing = true;
            try
            {
                DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(250));
                fadeOut.Completed += delegate { try { this.Close(); } catch { } };
                this.BeginAnimation(Window.OpacityProperty, fadeOut);
            }
            catch
            {
                try { this.Close(); } catch { }
            }
        }
    }

    public class MainWindow : Window
    {
        public const string CurrentAppVersion = "2.7.0";
        public const int CurrentAppVersionCode = 270;

        private string appDir;
        private string configPath;
        private string settingsPath;
        private string logFilePath;
        private string errorLogPath;
        private string sessionStatePath;
        private string wingetCmd;
        private DeploymentSessionState currentSession = null;
        private string lastGeneratedReportPath = null;
        private int watchdogTimeoutMinutes = 10;

        private ConfigModel config;
        private SettingsModel settings;
        private JavaScriptSerializer jsonSerializer;

        // UI References - Navigation & Views
        private string currentView = "setup";
        private string previousViewBeforeInstall = "setup";
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

        // =========================================================================
        // System Tray Integration & Windows 11 Toaster Notifications
        // =========================================================================
        private WinForms.NotifyIcon _trayIcon = null;
        private WinForms.MenuItem _trayStatusMenuItem = null;
        private Windows11ToastWindow _toastWindow = null;
        private bool isInstallRunning = false;
        private string currentProcessingAppName = "";
        private string currentProcessingAppStatus = "";
        private int currentProcessingAppIndex = 0;
        private int currentProcessingAppTotal = 0;
        private int currentProcessingPercent = 0;
        private string lastDismissedAppName = "";

        private void InitializeTrayIcon()
        {
            try
            {
                _trayIcon = new WinForms.NotifyIcon();
                _trayIcon.Text = "FleetForge — Ready";

                WinDrawing.Icon appIco = null;
                try
                {
                    var asm = Assembly.GetExecutingAssembly();
                    using (Stream s = asm.GetManifestResourceStream("appicon.ico"))
                    {
                        if (s != null) appIco = new WinDrawing.Icon(s);
                    }
                }
                catch { }

                if (appIco == null)
                {
                    try
                    {
                        string exe = Process.GetCurrentProcess().MainModule.FileName;
                        appIco = WinDrawing.Icon.ExtractAssociatedIcon(exe);
                    }
                    catch { }
                }

                if (appIco == null) appIco = WinDrawing.SystemIcons.Application;
                _trayIcon.Icon = appIco;

                WinForms.ContextMenu menu = new WinForms.ContextMenu();

                WinForms.MenuItem openItem = new WinForms.MenuItem("Open FleetForge", delegate {
                    Dispatcher.Invoke(new Action(RestoreFromTray));
                });
                openItem.DefaultItem = true;
                menu.MenuItems.Add(openItem);

                menu.MenuItems.Add(new WinForms.MenuItem("-"));

                _trayStatusMenuItem = new WinForms.MenuItem("Status: Idle");
                _trayStatusMenuItem.Enabled = false;
                menu.MenuItems.Add(_trayStatusMenuItem);

                menu.MenuItems.Add(new WinForms.MenuItem("-"));

                WinForms.MenuItem exitItem = new WinForms.MenuItem("Exit FleetForge", delegate {
                    Dispatcher.Invoke(new Action(ExitApplicationFromTray));
                });
                menu.MenuItems.Add(exitItem);

                _trayIcon.ContextMenu = menu;

                _trayIcon.DoubleClick += delegate {
                    Dispatcher.Invoke(new Action(RestoreFromTray));
                };
                _trayIcon.Click += delegate(object s, EventArgs e) {
                    WinForms.MouseEventArgs me = e as WinForms.MouseEventArgs;
                    if (me != null && me.Button == WinForms.MouseButtons.Left)
                    {
                        Dispatcher.Invoke(new Action(RestoreFromTray));
                    }
                };

                _trayIcon.Visible = true;
            }
            catch (Exception ex)
            {
                AppendLog("[TRAY] Warning: Tray icon setup failed: " + ex.Message);
            }
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                this.Hide();

                if (_trayIcon != null) _trayIcon.Visible = true;

                if (isInstallRunning)
                {
                    ShowWindows11Toast(
                        string.Format("[{0}/{1}] {2}", currentProcessingAppIndex > 0 ? currentProcessingAppIndex : 1, currentProcessingAppTotal > 0 ? currentProcessingAppTotal : 1, string.IsNullOrEmpty(currentProcessingAppName) ? "Starting setup..." : currentProcessingAppName),
                        currentProcessingAppStatus,
                        currentProcessingPercent,
                        false);
                }
            }
            else if (this.WindowState == WindowState.Normal || this.WindowState == WindowState.Maximized)
            {
                this.ShowInTaskbar = true;
                CloseWindows11Toast();
            }
        }

        public void RestoreFromTray()
        {
            try
            {
                if (this.Visibility != Visibility.Visible)
                {
                    this.Show();
                }
                if (this.WindowState == WindowState.Minimized)
                {
                    this.WindowState = WindowState.Normal;
                }
                this.ShowInTaskbar = true;
                this.Activate();
                this.Topmost = true;
                this.Topmost = false;
                this.Focus();

                CloseWindows11Toast();
            }
            catch { }
        }

        private void ExitApplicationFromTray()
        {
            if (isInstallRunning)
            {
                RestoreFromTray();
                MessageBoxResult mbr = MessageBox.Show(
                    "Software installation is currently running.\n\nAre you sure you want to cancel the installation and exit FleetForge?",
                    "Exit FleetForge",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (mbr != MessageBoxResult.Yes) return;

                RequestStopInstallation();
            }

            DisposeTrayAndToast();
            Application.Current.Shutdown();
        }

        private void DisposeTrayAndToast()
        {
            try
            {
                if (_trayIcon != null)
                {
                    _trayIcon.Visible = false;
                    _trayIcon.Dispose();
                    _trayIcon = null;
                }
            }
            catch { }

            try
            {
                CloseWindows11Toast();
            }
            catch { }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (isInstallRunning)
            {
                MessageBoxResult mbr = MessageBox.Show(
                    "Software installation is currently running in background.\n\n• Click YES to minimize to the System Tray and continue setup.\n• Click NO to stop installation and exit.\n• Click CANCEL to stay in FleetForge.",
                    "Installation in Progress — FleetForge",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (mbr == MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                    this.WindowState = WindowState.Minimized;
                    return;
                }
                else if (mbr == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                else
                {
                    RequestStopInstallation();
                }
            }

            DisposeTrayAndToast();
            base.OnClosing(e);
        }

        private void ShowWindows11Toast(string title, string detail, int percent, bool isFinal, string statusTag = null)
        {
            try
            {
                if (_toastWindow != null)
                {
                    _toastWindow.Close();
                    _toastWindow = null;
                }

                _toastWindow = new Windows11ToastWindow(
                    currentThemeIsLight,
                    new Action(RestoreFromTray),
                    new Action(delegate { lastDismissedAppName = currentProcessingAppName; }),
                    windowsAccentColor);

                _toastWindow.Closed += delegate { _toastWindow = null; };
                _toastWindow.Show();
                _toastWindow.UpdateProgress(title, detail, percent, isFinal, statusTag);
            }
            catch { }
        }

        private void UpdateToastProgress(string title, string detail, int percent, bool isFinal = false, string statusTag = null)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                try
                {
                    currentProcessingPercent = percent;
                    currentProcessingAppStatus = detail;

                    if (_trayIcon != null)
                    {
                        string shortTitle = (title != null && title.Length > 28) ? title.Substring(0, 25) + "..." : (title ?? "FleetForge");
                        string trayTip = "FleetForge: " + shortTitle + " (" + percent + "%)";
                        if (trayTip.Length > 63) trayTip = trayTip.Substring(0, 63);
                        _trayIcon.Text = trayTip;

                        if (_trayStatusMenuItem != null)
                        {
                            _trayStatusMenuItem.Text = shortTitle + " (" + percent + "%)";
                        }
                    }

                    // Show or update toast
                    // If final completion, ALWAYS show the toast so user gets the completion summary
                    if (isFinal || this.WindowState == WindowState.Minimized || this.Visibility != Visibility.Visible)
                    {
                        if (_toastWindow == null)
                        {
                            if (isFinal || currentProcessingAppName != lastDismissedAppName)
                            {
                                ShowWindows11Toast(title, detail, percent, isFinal, statusTag);
                            }
                        }
                        else
                        {
                            _toastWindow.UpdateProgress(title, detail, percent, isFinal, statusTag);
                        }
                    }
                }
                catch { }
            }));
        }

        private void CloseWindows11Toast()
        {
            try
            {
                if (_toastWindow != null)
                {
                    _toastWindow.Close();
                    _toastWindow = null;
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

        // Windows 11 Fluent Hero & Setup Elements
        private Border borderQuickSetupHero;
        private Image imgHeroMascot;
        private TextBlock txtHeroMainTitle;
        private TextBlock txtHeroMainSubtitle;
        private Border borderWizardStep1;
        private Border borderWizardStep2;
        private Border borderWizardStep3;
        private TextBlock txtWizardStep1Title;
        private TextBlock txtWizardStep1Desc;
        private TextBlock txtWizardStep2Num;
        private TextBlock txtWizardStep2Title;
        private TextBlock txtWizardStep2Desc;
        private TextBlock txtWizardStep3Num;
        private TextBlock txtWizardStep3Title;
        private TextBlock txtWizardStep3Desc;
        private System.Windows.Shapes.Rectangle rectWizardLine1;
        private System.Windows.Shapes.Rectangle rectWizardLine2;
        private Border borderSetupTip;
        private TextBlock txtSetupTip;
        private Image imgCatalogHero;
        private StackPanel stackBottomSelectedPackInfo;
        private TextBlock txtBottomSelectedPackInfo;

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
        private Button btnInstall;
        private Button btnStop;
        private Process currentProc;
        private volatile bool isCancelled;

        // Auto Online Winget Search State
        private int currentOnlineSearchToken = 0;
        private Dictionary<string, AppModel> knownOnlineApps = new Dictionary<string, AppModel>(StringComparer.OrdinalIgnoreCase);
#pragma warning disable 0414
        private bool isOnlineSearching = false;
        private bool isShowingOnlineResults = false;
#pragma warning restore 0414

        // Group Editor Modal References
        private Grid groupModalOverlay;
        private TextBlock txtModalTitle;
        private TextBlock txtModalSubtitle;
        private Border borderModalHeaderBadge;
        private Button btnCloseModal;
        private Border borderPackIconCard;
        private Image imgPackIconPreview;
        private Border borderPackIconBadgeEdit;
        private TextBlock txtModalHeaderGlyph;
        private TextBlock txtPackIconPreview;
        private TextBox txtGroupNameInput;
        private Border borderGroupName;
        private TextBox txtGroupIconInput;
        private WrapPanel emojiChipsPanel;
        private StackPanel panelEmojiChips;
        private Button btnIconPrev;
        private Button btnIconNext;
        private TextBox txtGroupDescInput;
        private TextBlock txtGroupDescCharCount;
        private Border borderGroupDesc;
        private Border borderModalSearchBox;
        private TextBox txtModalAppSearch;
        private Button btnModalFilterTabAll;
        private Button btnModalFilterTabSelected;
        private Border borderModalTableHeader;
        private CheckBox chkModalSelectAll;
        private TextBlock txtModalTableHeaderCount;
        private Border badgeModalHeaderRole;
        private TextBlock txtModalHeaderBadgeRole;
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

        // Online Search Modal References
        private Grid modalOnlineSearchOverlay;
        private Border borderOnlineSearchCard;
        private TextBlock txtOnlineModalTitle;
        private TextBlock txtOnlineModalSubtitle;
        private Border borderOnlineModalIcon;
        private Button btnCloseOnlineModal;
        private Border borderOnlineSearchInput;
        private TextBox txtOnlineModalSearchInput;
        private Button btnOnlineClearSearch;
        private Button btnOnlineModalExecuteSearch;
        private TextBlock txtOnlineSearchBtnText;
        private WrapPanel onlineSearchTagsPanel;
        private Border borderOnlineModalStatusHeader;
        private TextBlock txtOnlineModalStatus;
        private Border borderOnlineModalResultsContainer;
        private ScrollViewer onlineModalResultsScrollViewer;
        private StackPanel onlineModalResultsPanel;
        private TextBlock txtOnlineModalSelectedCount;
        private Button btnCancelOnlineModal;
        private Button btnOnlineAddSelected;
        private Button btnOnlineInstallSelected;
        private List<AppModel> modalOnlineSelectedApps = new List<AppModel>();
        private string[] quickIcons = new string[] { "🎨", "💻", "</>", "✏️", "📄", "🚀", "📊", "🛠️", "🎬", "📷", "🧮", "🔒", "⚡", "🌐", "🎮", "💼" };

        // Help Menu, Theme Toggle & Brand Icon
        // Dynamic Resolution & UI Scaling
        private ScaleTransform uiScaleTransform = new ScaleTransform(1.0, 1.0);
        private string displayScaleSetting = "Auto";
        private Border borderSettingsSecDisplay;
        private TextBlock txtSettingsSecDisplayTitle;
        private RadioButton rbScaleAuto;
        private RadioButton rbScale100;
        private RadioButton rbScale90;
        private RadioButton rbScale80;
        private TextBlock txtCurrentScaleActive;

        private Grid rootGrid;
        private Button btnToggleTheme;
        private TextBlock txtToggleThemeIcon;
        private Button btnHelpMenu;
        private Button btnLanguageSelector;
        private Image imgLanguageFlag;
        private TextBlock txtLanguageName;
        private string activeLanguageCode = "auto";
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
        private string activeRoleId = "recommended";
        private string currentEditingGroupId = null;
        private string recommendedAppsPath = null;
        private List<string> recommendedMasterAppIds = new List<string>();
        private string customAppsPath = null;
        private List<string> customMasterAppIds = new List<string>();
        private List<AppModel> baseCatalogApps = null;
        private List<AppModel> builderCustomApps = new List<AppModel>();
        private AppModel builderEditingApp = null;

        // Modal: Custom App & Affiliate Builder
        private Grid modalCustomAppBuilderOverlay;
        private Border borderCustomAppBuilderCard;
        private TextBlock txtModalAppBuilderTitle;
        private Border borderBuilderFormCard;
        private Border borderBuilderListCard;
        private TextBox txtBuilderSearchCatalog;
        private Button btnBuilderSearchFind;
        private ListBox lstBuilderSearchResults;
        private TextBox txtBuilderAppId;
        private TextBox txtBuilderAppName;
        private ComboBox cmbBuilderCategory;
        private TextBox txtBuilderDownloadUrl;
        private TextBox txtBuilderAffiliateWebUrl;
        private TextBox txtBuilderSilentArgs;
        private TextBox txtBuilderDesc;
        private Button btnBuilderAddUpdate;
        private Button btnBuilderClearForm;
        private TextBlock txtBuilderFormStatus;
        private TextBlock txtBuilderCustomListHeader;
        private StackPanel pnlBuilderCustomAppsList;
        private Button btnBuilderImportJson;
        private Button btnBuilderExportJson;
        private Button btnBuilderCancel;
        private Button btnBuilderSaveAndApply;
        private Button btnCloseAppBuilder;

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
            CheckLiveRecommendedAppsUpdate();
            CheckResumeSession();
            CheckFirstTimeTutorial();

            this.SourceInitialized += delegate { UpdateDwmTheme(currentThemeIsLight); };
            this.Loaded += delegate { UpdateDwmTheme(currentThemeIsLight); };
            InitializeTrayIcon();
            this.StateChanged += MainWindow_StateChanged;
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
            try
            {
                string asmLoc = Assembly.GetExecutingAssembly().Location;
                if (!string.IsNullOrEmpty(asmLoc))
                {
                    appDir = Path.GetDirectoryName(asmLoc);
                }
            }
            catch { }
            if (string.IsNullOrEmpty(appDir))
            {
                appDir = AppDomain.CurrentDomain.BaseDirectory;
            }
            AppUrls.Initialize(appDir);

            // 1. Catalog path (apps.json in data/ or root)
            configPath = Path.Combine(appDir, "data", "apps.json");
            if (!File.Exists(configPath)) configPath = Path.Combine(appDir, "apps.json");

            // 1b. Recommended & affiliate software database path
            recommendedAppsPath = Path.Combine(appDir, "data", "recommended_apps.json");
            if (!File.Exists(recommendedAppsPath)) recommendedAppsPath = Path.Combine(appDir, "recommended_apps.json");

            // 1c. Custom apps & affiliate overrides database path
            customAppsPath = Path.Combine(appDir, "data", "custom_apps.json");
            if (!File.Exists(customAppsPath)) customAppsPath = Path.Combine(appDir, "custom_apps.json");

            // 2. Settings path (settings.json in config/ or root)
            settingsPath = Path.Combine(appDir, "config", "settings.json");
            if (!File.Exists(settingsPath)) settingsPath = Path.Combine(appDir, "settings.json");

            // 3. AppData Local Directory for all runtime logs, error logs, and session data
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appDataFolder = Path.Combine(localAppData, "FleetForge");
            if (!Directory.Exists(appDataFolder))
            {
                try { Directory.CreateDirectory(appDataFolder); } catch { }
            }

            // Install and Error log files in %LOCALAPPDATA%\FleetForge
            logFilePath = Path.Combine(appDataFolder, "install-log.txt");
            errorLogPath = Path.Combine(appDataFolder, "error-log.txt");

            // 4. Session Persistence in %LOCALAPPDATA%\FleetForge
            string dataDir = Path.Combine(appDataFolder, "data");
            if (!Directory.Exists(dataDir))
            {
                try { Directory.CreateDirectory(dataDir); } catch { }
            }
            sessionStatePath = Path.Combine(dataDir, "deployment_session.json");

            // Strictly do NOT create reports directory; clean up any legacy reports directory if present
            try
            {
                string legacyReportsAppDir = Path.Combine(appDir, "reports");
                if (Directory.Exists(legacyReportsAppDir))
                {
                    Directory.Delete(legacyReportsAppDir, true);
                }

                string lingeringReportsDir = Path.Combine(appDataFolder, "reports");
                if (Directory.Exists(lingeringReportsDir))
                {
                    Directory.Delete(lingeringReportsDir, true);
                }
            }
            catch { }

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

        private static readonly Dictionary<string, ImageSource> embeddedImageCache = new Dictionary<string, ImageSource>(StringComparer.OrdinalIgnoreCase);

        public static ImageSource LoadEmbeddedImage(string resourceName)
        {
            if (string.IsNullOrEmpty(resourceName)) return null;
            if (embeddedImageCache.ContainsKey(resourceName))
            {
                return embeddedImageCache[resourceName];
            }
            try
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                using (Stream s = asm.GetManifestResourceStream(resourceName))
                {
                    if (s != null)
                    {
                        PngBitmapDecoder decoder = new PngBitmapDecoder(s, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                        if (decoder.Frames.Count > 0)
                        {
                            embeddedImageCache[resourceName] = decoder.Frames[0];
                            return decoder.Frames[0];
                        }
                    }
                }

                string localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "src", "assets", resourceName);
                if (!File.Exists(localPath)) localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, resourceName);
                if (File.Exists(localPath))
                {
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri(localPath, UriKind.Absolute);
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.EndInit();
                    bi.Freeze();
                    embeddedImageCache[resourceName] = bi;
                    return bi;
                }
            }
            catch { }
            return null;
        }

        
        // =========================================================================
        // MULTILANGUAGE PLATFORM METHODS
        // =========================================================================
        private bool IsSearchPlaceholder(string text)
        {
            return Localization.IsKnownPlaceholder(text);
        }

        private void SetApplicationLanguage(string code)
        {
            activeLanguageCode = code;
            if (settings != null)
            {
                settings.language = code;
                SaveSettings();
            }

            if (code.Equals("auto", StringComparison.OrdinalIgnoreCase))
            {
                Localization.CurrentLanguage = Localization.DetectSystemLanguage();
            }
            else
            {
                Localization.CurrentLanguage = code;
            }

            UpdateLanguageButtonDisplay();
            ApplyLanguageLive();
        }

        private void UpdateLanguageButtonDisplay()
        {
            if (btnLanguageSelector == null) return;
            string langCode = Localization.CurrentLanguage;
            if (string.IsNullOrEmpty(langCode) || langCode.Equals("auto", StringComparison.OrdinalIgnoreCase))
            {
                langCode = Localization.DetectSystemLanguage();
            }

            LanguageDefinition current = Localization.GetLanguage(langCode);
            if (imgLanguageFlag != null)
            {
                imgLanguageFlag.Source = Localization.GetFlagBitmap(langCode);
            }
            if (txtLanguageName != null)
            {
                txtLanguageName.Text = current != null ? current.NativeName : "English";
            }
            btnLanguageSelector.ToolTip = string.Format("Language: {0} ({1}) - Click to change", current != null ? current.NativeName : "English", current != null ? current.Name : "English");
        }

        private void RebuildCategoryChips()
        {
            if (categoryChipsPanel == null || config == null || config.categories == null) return;
            categoryChipsPanel.Children.Clear();

            string allTitle = Localization.GetLocalizedCategoryName("all", "All");
            int totalApps = (config.apps != null) ? config.apps.Count : 0;
            Button allTab = CreateCategoryTabButton("all", "\uD83C\uDF10", allTitle, totalApps, activeCategory == "all" || string.IsNullOrEmpty(activeCategory));
            categoryChipsPanel.Children.Add(allTab);

            foreach (CategoryModel cat in config.categories)
            {
                CategoryModel curCat = cat;
                int count = (config.apps != null) ? config.apps.FindAll(delegate (AppModel a) { return MatchesCategory(a, curCat.id); }).Count : 0;
                string locName = Localization.GetLocalizedCategoryName(curCat.id, curCat.name);
                Button cTab = CreateCategoryTabButton(curCat.id, GetSafeCategoryIcon(curCat), locName, count, activeCategory == curCat.id);
                categoryChipsPanel.Children.Add(cTab);
            }
        }

        private void ApplyLanguageLive()
        {
            // 1. Navigation items
            if (navTxtSetup != null) navTxtSetup.Text = Localization.T("nav_quick_setup");
            if (navTxtCatalog != null) navTxtCatalog.Text = Localization.T("nav_all_apps");
            if (navTxtCloud != null) navTxtCloud.Text = Localization.T("nav_cloud_sync");
            if (navTxtLogs != null) navTxtLogs.Text = Localization.T("nav_history");

            // 2. Quick Setup Hero & Wizard Steps
            if (txtHeroMainTitle != null) txtHeroMainTitle.Text = Localization.T("quick_setup_title");
            if (txtHeroMainSubtitle != null) txtHeroMainSubtitle.Text = Localization.T("quick_setup_subtitle");
            if (txtWizardStep1Title != null) txtWizardStep1Title.Text = Localization.T("step1_title");
            if (txtWizardStep1Desc != null) txtWizardStep1Desc.Text = Localization.T("step1_desc");
            if (txtWizardStep2Title != null) txtWizardStep2Title.Text = Localization.T("step2_title");
            if (txtWizardStep2Desc != null) txtWizardStep2Desc.Text = Localization.T("step2_desc");
            if (txtWizardStep3Title != null) txtWizardStep3Title.Text = Localization.T("step3_title");
            if (txtWizardStep3Desc != null) txtWizardStep3Desc.Text = Localization.T("step3_desc");
            if (txtTargetRoleTitle != null) txtTargetRoleTitle.Text = Localization.T("available_packs_title");
            if (txtTargetRoleSubtitle != null) txtTargetRoleSubtitle.Text = Localization.T("available_packs_subtitle");

            // 3. Rebuild role cards with localized badges and counts
            BuildRoleCards();

            // 4. Catalog Page Hero & Header
            if (txtCatalogBigHeading != null) txtCatalogBigHeading.Text = Localization.T("catalog_title");
            if (txtCatalogBigSubtitle != null) txtCatalogBigSubtitle.Text = Localization.T("catalog_subtitle");
            if (chkSelectAllCatalog != null) chkSelectAllCatalog.Content = Localization.T("select_all");
            if (btnCatalogSort != null) btnCatalogSort.Content = Localization.T("sort_button");

            // 5. Search box placeholder
            if (txtSearch != null && IsSearchPlaceholder(txtSearch.Text))
            {
                txtSearch.Text = Localization.T("search_placeholder");
                txtSearch.Foreground = currentThemeIsLight ? HexBrush("#475569") : HexBrush("#94A3B8");
                if (btnClearSearch != null) btnClearSearch.Visibility = Visibility.Collapsed;
            }

            // 6. Category chips
            RebuildCategoryChips();

            // 7. Help button
            if (btnHelpMenu != null) btnHelpMenu.Content = Localization.T("help_button");

            // 8. Bottom bar buttons & tips
            if (btnImportPack != null) btnImportPack.Content = Localization.T("btn_import_pack");
            if (btnCreateNewGroup != null) btnCreateNewGroup.Content = Localization.T("btn_create_pack");
            if (btnStop != null) btnStop.Content = Localization.T("btn_stop");
            if (txtSetupTip != null) txtSetupTip.Text = Localization.T("bottom_tip");

            // 9. Logs Page
            if (txtLogsBigHeading != null) txtLogsBigHeading.Text = Localization.T("logs_title");
            if (txtLogsBigSubtitle != null) txtLogsBigSubtitle.Text = Localization.T("logs_subtitle");
            if (btnViewReport != null) btnViewReport.Content = Localization.T("btn_summary_report");
            if (btnOpenLogFile != null) btnOpenLogFile.Content = Localization.T("btn_open_logfile");
            if (btnClearLogConsole != null) btnClearLogConsole.Content = Localization.T("btn_clear_screen");

            // 10. Update catalog row badges
            if (appRows != null)
            {
                foreach (AppRowView row in appRows)
                {
                    if (row != null && row.StatusText != null)
                    {
                        if (Localization.IsReadyBadgeText(row.StatusText.Text))
                        {
                            row.StatusText.Text = Localization.T("badge_ready");
                        }
                    }
                }
            }

            // 11. Counters and summary stats
            UpdateCount();
            UpdateHeaderCount();
        }

        private void InitializeWindow()
        {
            this.Title = "FleetForge — Zero-Touch Machine Setup & Enterprise Software Deployment";

            // Dynamic WorkArea & Resolution Adaptation
            Rect wa = SystemParameters.WorkArea;
            const double desiredW = 1180;
            const double desiredH = 850;

            // Safe constraint against available work area (leaving margin for taskbar and borders)
            double maxAllowedW = Math.Max(720, wa.Width - 16);
            double maxAllowedH = Math.Max(480, wa.Height - 16);

            this.MinWidth = Math.Min(780, Math.Max(640, wa.Width * 0.75));
            this.MinHeight = Math.Min(500, Math.Max(440, wa.Height * 0.75));

            this.Width = Math.Min(desiredW, maxAllowedW);
            this.Height = Math.Min(desiredH, maxAllowedH);

            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Left = wa.Left + Math.Max(0.0, (wa.Width - this.Width) / 2.0);
            this.Top = wa.Top + Math.Max(0.0, (wa.Height - this.Height) / 2.0);

            // On compact screens (1366x768, 1280x720, or high DPI scaling), auto-maximize for full workspace
            if (wa.Width <= 1280 || wa.Height <= 768)
            {
                this.WindowState = WindowState.Maximized;
            }
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
                                CornerRadius=""8"" Padding=""{TemplateBinding Padding}"" SnapsToDevicePixels=""True"">
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
        <Style x:Key=""CategoryPillStyle"" TargetType=""Button"">
            <Setter Property=""FocusVisualStyle"" Value=""{x:Null}""/>
            <Setter Property=""Template"">
                <Setter.Value>
                    <ControlTemplate TargetType=""Button"">
                        <Border x:Name=""border"" Background=""{TemplateBinding Background}""
                                BorderBrush=""{TemplateBinding BorderBrush}""
                                BorderThickness=""{TemplateBinding BorderThickness}""
                                CornerRadius=""18"" Padding=""{TemplateBinding Padding}"" SnapsToDevicePixels=""True"">
                            <ContentPresenter HorizontalAlignment=""Center"" VerticalAlignment=""Center"" RecognizesAccessKey=""True""/>
                        </Border>
                        <ControlTemplate.Triggers>
                            <Trigger Property=""IsMouseOver"" Value=""True"">
                                <Setter Property=""Opacity"" Value=""0.90""/>
                            </Trigger>
                            <Trigger Property=""IsPressed"" Value=""True"">
                                <Setter Property=""Opacity"" Value=""0.75""/>
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
                            ToolTip=""Collapse / Expand Menu"" HorizontalAlignment=""Right"" VerticalAlignment=""Center"" Visibility=""Collapsed"">
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
                        <TextBlock x:Name=""NavIconSetup"" Grid.Column=""1"" Text=""⚡"" FontFamily=""Segoe UI Emoji, Segoe UI Symbol, Segoe Fluent Icons"" FontSize=""15"" Foreground=""#0066FF"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
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
                        <TextBlock x:Name=""NavIconCatalog"" Grid.Column=""1"" Text=""⊞"" FontFamily=""Segoe UI Symbol, Segoe Fluent Icons, Segoe MDL2 Assets"" FontSize=""16"" Foreground=""#64748B"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                        <TextBlock x:Name=""NavTxtCatalog"" Grid.Column=""2"" Text=""All Apps"" FontSize=""13"" Foreground=""#D0D6E0"" VerticalAlignment=""Center"" Margin=""6,0,0,0""/>
                    </Grid>
                </Button>

                <!-- Nav 3: Cloud Sync (Hidden / Removed from navigation) -->
                <Button x:Name=""NavBtnCloud"" Visibility=""Collapsed"" Height=""40"" Margin=""0,0,0,4"" Background=""Transparent"" BorderThickness=""0"" Cursor=""Hand"" HorizontalContentAlignment=""Left"" ToolTip=""Cloud Sync"">
                    <Grid Margin=""2,0,4,0"">
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width=""4""/>
                            <ColumnDefinition Width=""36""/>
                            <ColumnDefinition Width=""*""/>
                        </Grid.ColumnDefinitions>
                        <Border x:Name=""NavIndCloud"" Grid.Column=""0"" Width=""3"" Height=""16"" CornerRadius=""1.5"" Background=""#0078D4"" HorizontalAlignment=""Left"" VerticalAlignment=""Center"" Visibility=""Collapsed""/>
                        <TextBlock x:Name=""NavIconCloud"" Grid.Column=""1"" Text=""☁"" FontFamily=""Segoe UI Emoji, Segoe UI Symbol, Segoe Fluent Icons"" FontSize=""16"" Foreground=""#64748B"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
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
                        <TextBlock x:Name=""NavIconLogs"" Grid.Column=""1"" Text=""🕒"" FontFamily=""Segoe UI Emoji, Segoe UI Symbol, Segoe Fluent Icons"" FontSize=""15"" Foreground=""#64748B"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                        <TextBlock x:Name=""NavTxtLogs"" Grid.Column=""2"" Text=""Install History"" FontSize=""13"" Foreground=""#D0D6E0"" VerticalAlignment=""Center"" Margin=""6,0,0,0""/>
                    </Grid>
                </Button>
            </StackPanel>

            <!-- Left Panel Bottom: Developer Branding -->
            <StackPanel x:Name=""StackSidebarFooter"" Grid.Row=""2"" HorizontalAlignment=""Center"" VerticalAlignment=""Bottom"" Margin=""0,0,0,10"" ToolTip=""FleetForge by Love Malhotra"">
                <TextBlock x:Name=""TxtSidebarFooterBy"" Text=""Design and Develop by""
                           FontSize=""10.5"" Foreground=""#94A3B8""
                           HorizontalAlignment=""Center"" TextAlignment=""Center"" Margin=""0,0,0,3""/>
                <TextBlock x:Name=""TxtSidebarFooterCompany"" Text=""LOVE MALHOTRA""
                           FontSize=""11"" FontWeight=""Bold"" Foreground=""#5f6d84""
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
        <Border x:Name=""BorderTopBar"" Grid.Row=""0"" Background=""#F8FAFC"" Padding=""28,12"" BorderBrush=""#E2E8F0"" BorderThickness=""0,0,0,1"">
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width=""*""/>
                    <ColumnDefinition Width=""Auto""/>
                </Grid.ColumnDefinitions>

                <!-- Windows 11 Fluent Search Box (Distinct, High-Contrast Capsule) -->
                <Border x:Name=""BorderHeaderSearch"" Grid.Column=""0"" Width=""380"" Height=""36"" CornerRadius=""18""
                        Background=""#FFFFFF"" BorderBrush=""#CBD5E1"" BorderThickness=""1""
                        HorizontalAlignment=""Left"" VerticalAlignment=""Center"">
                    <Grid Margin=""14,0,12,0"">
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width=""Auto""/>
                            <ColumnDefinition Width=""*""/>
                            <ColumnDefinition Width=""Auto""/>
                        </Grid.ColumnDefinitions>
                        <TextBlock x:Name=""TxtHeaderSearchIcon"" Text=""🔍"" FontSize=""12.5"" Foreground=""#64748B"" VerticalAlignment=""Center"" Margin=""0,0,10,0""/>
                        <TextBox x:Name=""TxtSearch"" Grid.Column=""1"" Height=""28"" Background=""Transparent""
                                 Foreground=""#0F172A"" CaretBrush=""#0066FF"" BorderThickness=""0""
                                 FontSize=""12.5"" VerticalContentAlignment=""Center"" Text=""Find an app, setting or pack...""/>
                        <Button x:Name=""BtnClearSearch"" Grid.Column=""2"" Content=""✕"" Width=""20"" Height=""20""
                                Background=""Transparent"" Foreground=""#64748B"" BorderThickness=""0""
                                Cursor=""Hand"" Visibility=""Collapsed"" Margin=""4,0,0,0"" ToolTip=""Clear search""/>
                    </Grid>
                </Border>

                <!-- Actions on Right (Language Selector & Help Menu button) -->
                <StackPanel Grid.Column=""1"" Orientation=""Horizontal"" VerticalAlignment=""Center"">
                    <Button x:Name=""BtnLanguageSelector"" Background=""#FFFFFF"" Foreground=""#0F172A"" BorderBrush=""#CBD5E1"" BorderThickness=""1""
                            FontWeight=""SemiBold"" FontSize=""12"" Padding=""10,6"" Margin=""0,0,8,0"" Cursor=""Hand"" ToolTip=""Change Language / &#x092D;&#x093E;&#x0937;&#x093E; &#x092C;&#x0926;&#x0932;&#x0947;&#x0902;"" VerticalAlignment=""Center"">
                        <StackPanel Orientation=""Horizontal"" VerticalAlignment=""Center"">
                            <Border Width=""20"" Height=""14"" CornerRadius=""2"" BorderThickness=""1"" BorderBrush=""#CBD5E1"" Margin=""0,0,7,0"" VerticalAlignment=""Center"" ClipToBounds=""True"">
                                <Image x:Name=""ImgLanguageFlag"" Stretch=""UniformToFill""/>
                            </Border>
                            <TextBlock x:Name=""TxtLanguageName"" Text=""English"" FontSize=""12"" FontWeight=""SemiBold"" VerticalAlignment=""Center""/>
                            <TextBlock Text="" &#x25BE;"" FontSize=""10"" Foreground=""#64748B"" Margin=""2,0,0,0"" VerticalAlignment=""Center""/>
                        </StackPanel>
                    </Button>
                    <Button x:Name=""BtnHelpMenu"" Content=""Help ?"" Background=""#FFFFFF"" Foreground=""#0F172A"" BorderBrush=""#CBD5E1"" BorderThickness=""1""
                            FontWeight=""SemiBold"" FontSize=""12"" Padding=""16,6"" Cursor=""Hand"" ToolTip=""How to Use, Settings, Updates &amp; About"" VerticalAlignment=""Center""/>
                </StackPanel>
            </Grid>
        </Border>

        <!-- PAGE 1: SETUP (Windows 11 Layout) -->
        <ScrollViewer x:Name=""ViewSetup"" Grid.Row=""1"" VerticalScrollBarVisibility=""Auto"" HorizontalScrollBarVisibility=""Disabled"" Margin=""28,18,28,18"" Padding=""0,0,10,0"">
            <StackPanel>
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
                            <Button x:Name=""BtnResumeSession"" Content=""Resume Installation"" Background=""#0066FF"" Foreground=""#FFFFFF"" FontWeight=""SemiBold"" FontSize=""11.5"" Padding=""14,6"" Margin=""0,0,8,0"" Cursor=""Hand"" BorderThickness=""0""/>
                            <Button x:Name=""BtnDismissResume"" Content=""Dismiss"" Background=""#FFFFFF"" Foreground=""#0F172A"" BorderBrush=""#CBD5E1"" BorderThickness=""1"" FontWeight=""SemiBold"" FontSize=""11.5"" Padding=""12,6"" Cursor=""Hand""/>
                        </StackPanel>
                    </Grid>
                </Border>

                <!-- UNIFIED HERO CARD (Windows 11 Fluent with Mascot & Step Progression Wizard) -->
                <Border x:Name=""BorderQuickSetupHero"" CornerRadius=""16"" BorderThickness=""1"" Margin=""0,0,0,18"" Padding=""18,12,24,12"">
                    <Border.Background>
                        <LinearGradientBrush x:Name=""HeroGradBrush"" StartPoint=""0,0"" EndPoint=""1,0"">
                            <GradientStop x:Name=""GradStopHero1"" Color=""#E6F0FC"" Offset=""0.0""/>
                            <GradientStop x:Name=""GradStopHero2"" Color=""#EEF5FE"" Offset=""0.38""/>
                            <GradientStop x:Name=""GradStopHero3"" Color=""#F7FAFE"" Offset=""0.75""/>
                            <GradientStop x:Name=""GradStopHero4"" Color=""#FFFFFF"" Offset=""1.0""/>
                        </LinearGradientBrush>
                    </Border.Background>
                    <Border.BorderBrush>
                        <SolidColorBrush x:Name=""HeroBorderBrush"" Color=""#D5E4F7""/>
                    </Border.BorderBrush>
                    <Grid>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width=""*""/>
                            <ColumnDefinition Width=""Auto""/>
                        </Grid.ColumnDefinitions>

                        <!-- Left: Mascot + Title + Subtitle -->
                        <Grid Grid.Column=""0"" VerticalAlignment=""Center"">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width=""Auto""/>
                                <ColumnDefinition Width=""*""/>
                            </Grid.ColumnDefinitions>
                            <Border Grid.Column=""0"" Width=""108"" Height=""92"" Margin=""0,0,14,0"" VerticalAlignment=""Center"">
                                <Image x:Name=""ImgHeroMascot"" Width=""108"" Height=""92"" RenderOptions.BitmapScalingMode=""HighQuality"" Stretch=""Uniform"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                            </Border>
                            <StackPanel Grid.Column=""1"" VerticalAlignment=""Center"" Margin=""0,0,16,0"">
                                <TextBlock x:Name=""TxtHeroMainTitle"" Text=""Quick Setup"" FontSize=""26"" FontWeight=""Bold"" Foreground=""#0F172A"" Margin=""0,0,0,4""/>
                                <TextBlock x:Name=""TxtHeroMainSubtitle"" Text=""Choose a pack, pick apps and get started in one click."" FontSize=""12.5"" Foreground=""#64748B""/>
                            </StackPanel>
                        </Grid>

                        <!-- Right: Connected 3-Step Wizard Progression Tracker -->
                        <Grid Grid.Column=""1"" VerticalAlignment=""Center"" Margin=""10,0,0,0"">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width=""Auto""/>
                                <ColumnDefinition Width=""Auto""/>
                                <ColumnDefinition Width=""Auto""/>
                                <ColumnDefinition Width=""Auto""/>
                                <ColumnDefinition Width=""Auto""/>
                            </Grid.ColumnDefinitions>

                            <!-- Step 1: Active Blue -->
                            <StackPanel Grid.Column=""0"" HorizontalAlignment=""Center"" VerticalAlignment=""Center"">
                                <Border x:Name=""BorderWizardStep1"" Width=""26"" Height=""26"" CornerRadius=""13"" Background=""#0066FF"" HorizontalAlignment=""Center"" Margin=""0,0,0,6"">
                                    <TextBlock Text=""1"" FontWeight=""Bold"" FontSize=""11"" Foreground=""#FFFFFF"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                                </Border>
                                <TextBlock x:Name=""TxtWizardStep1Title"" Text=""Pick a Pack"" FontWeight=""Bold"" FontSize=""11"" Foreground=""#0F172A"" HorizontalAlignment=""Center""/>
                                <TextBlock x:Name=""TxtWizardStep1Desc"" Text=""Choose your needs"" FontSize=""9.5"" Foreground=""#64748B"" HorizontalAlignment=""Center"" Margin=""0,1,0,0""/>
                            </StackPanel>

                            <!-- Divider Line 1 -->
                            <Rectangle x:Name=""RectWizardLine1"" Grid.Column=""1"" Width=""55"" Height=""1.5"" Fill=""#CBD5E1"" VerticalAlignment=""Top"" Margin=""8,12,8,0""/>

                            <!-- Step 2 -->
                            <StackPanel Grid.Column=""2"" HorizontalAlignment=""Center"" VerticalAlignment=""Center"">
                                <Border x:Name=""BorderWizardStep2"" Width=""26"" Height=""26"" CornerRadius=""13"" Background=""#EDF4FF"" BorderBrush=""#CBD5E1"" BorderThickness=""1"" HorizontalAlignment=""Center"" Margin=""0,0,0,6"">
                                    <TextBlock x:Name=""TxtWizardStep2Num"" Text=""2"" FontWeight=""Bold"" FontSize=""11"" Foreground=""#0066FF"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                                </Border>
                                <TextBlock x:Name=""TxtWizardStep2Title"" Text=""Review Apps"" FontWeight=""Bold"" FontSize=""11"" Foreground=""#0F172A"" HorizontalAlignment=""Center""/>
                                <TextBlock x:Name=""TxtWizardStep2Desc"" Text=""Check selection"" FontSize=""9.5"" Foreground=""#64748B"" HorizontalAlignment=""Center"" Margin=""0,1,0,0""/>
                            </StackPanel>

                            <!-- Divider Line 2 -->
                            <Rectangle x:Name=""RectWizardLine2"" Grid.Column=""3"" Width=""55"" Height=""1.5"" Fill=""#CBD5E1"" VerticalAlignment=""Top"" Margin=""8,12,8,0""/>

                            <!-- Step 3 -->
                            <StackPanel Grid.Column=""4"" HorizontalAlignment=""Center"" VerticalAlignment=""Center"">
                                <Border x:Name=""BorderWizardStep3"" Width=""26"" Height=""26"" CornerRadius=""13"" Background=""#EDF4FF"" BorderBrush=""#CBD5E1"" BorderThickness=""1"" HorizontalAlignment=""Center"" Margin=""0,0,0,6"">
                                    <TextBlock x:Name=""TxtWizardStep3Num"" Text=""3"" FontWeight=""Bold"" FontSize=""11"" Foreground=""#0066FF"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                                </Border>
                                <TextBlock x:Name=""TxtWizardStep3Title"" Text=""Install"" FontWeight=""Bold"" FontSize=""11"" Foreground=""#0F172A"" HorizontalAlignment=""Center""/>
                                <TextBlock x:Name=""TxtWizardStep3Desc"" Text=""Start using"" FontSize=""9.5"" Foreground=""#64748B"" HorizontalAlignment=""Center"" Margin=""0,1,0,0""/>
                            </StackPanel>
                        </Grid>
                    </Grid>
                </Border>

                <!-- Section Header: Available App Packs -->
                <Grid Margin=""0,0,0,12"">
                    <StackPanel VerticalAlignment=""Center"">
                        <TextBlock x:Name=""TxtTargetRoleTitle"" Text=""Available App Packs"" FontSize=""16"" FontWeight=""Bold"" Foreground=""#0F172A""/>
                        <TextBlock x:Name=""TxtTargetRoleSubtitle"" Text=""Select a ready-made pack or choose individual apps.""
                                   FontSize=""12"" Foreground=""#64748B"" Margin=""0,2,0,0""/>
                    </StackPanel>
                </Grid>

                <UniformGrid x:Name=""RoleCardsGrid"" Columns=""2"" Margin=""0,0,0,10""/>
            </StackPanel>
        </ScrollViewer>

        <!-- PAGE 2: CATALOG -->
        <Grid x:Name=""ViewCatalog"" Grid.Row=""1"" Visibility=""Collapsed"" Margin=""28,14,28,14"">
            <Grid.RowDefinitions>
                <RowDefinition Height=""Auto""/>
                <RowDefinition Height=""Auto""/>
                <RowDefinition Height=""*""/>
            </Grid.RowDefinitions>

            <!-- HERO HEADER with 3D Folder Illustration -->
            <Grid Grid.Row=""0"" Margin=""0,0,0,16"">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width=""Auto""/>
                    <ColumnDefinition Width=""*""/>
                </Grid.ColumnDefinitions>
                <Border Grid.Column=""0"" Width=""122"" Height=""80"" Margin=""0,0,16,0"" VerticalAlignment=""Center"">
                    <Image x:Name=""ImgCatalogHero"" Width=""118"" Height=""76"" RenderOptions.BitmapScalingMode=""HighQuality"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                </Border>
                <StackPanel Grid.Column=""1"" VerticalAlignment=""Center"">
                    <TextBlock x:Name=""TxtCatalogBigHeading"" Text=""All Applications"" FontSize=""28"" FontWeight=""Bold"" Foreground=""#0F172A"" Margin=""0,0,0,4""/>
                    <TextBlock x:Name=""TxtCatalogBigSubtitle"" Text=""Browse 16,349 software packages with instant search and one-click selection.""
                               FontSize=""13"" Foreground=""#475569""/>
                </StackPanel>
            </Grid>

            <!-- Expanded Modern Category Tabs Deck -->
            <WrapPanel x:Name=""CategoryChipsPanel"" Grid.Row=""1"" Orientation=""Horizontal"" Margin=""0,0,0,14""/>

            <Border x:Name=""BorderCatalogContainer"" Grid.Row=""2"" Background=""#FFFFFF"" CornerRadius=""16"" Padding=""16,12"" BorderBrush=""#E2E8F0"" BorderThickness=""1"">
                <Grid>
                    <Grid.RowDefinitions>
                        <RowDefinition Height=""Auto""/>
                        <RowDefinition Height=""*""/>
                    </Grid.RowDefinitions>

                    <!-- Top Software List Header Bar with Single Master CheckBox and Sort Dropdown -->
                    <Border x:Name=""BorderCatalogHeaderBar"" Grid.Row=""0"" Margin=""0,0,0,8"" Padding=""4,4,4,12"" BorderThickness=""0,0,0,1"" BorderBrush=""#F1F5F9"">
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width=""Auto""/>
                                <ColumnDefinition Width=""*""/>
                                <ColumnDefinition Width=""Auto""/>
                            </Grid.ColumnDefinitions>
                            <CheckBox x:Name=""ChkSelectAllCatalog"" Grid.Column=""0"" Content=""Select All Apps"" FontSize=""12.5"" FontWeight=""SemiBold"" Foreground=""#0F172A"" VerticalAlignment=""Center"" Cursor=""Hand""/>
                            <TextBlock x:Name=""TxtCatalogListHeaderCount"" Grid.Column=""1"" HorizontalAlignment=""Right"" VerticalAlignment=""Center"" FontSize=""11.5"" Foreground=""#64748B"" Margin=""0,0,14,0"" Text=""Showing 250 of 16,351 apps""/>
                            <Button x:Name=""BtnCatalogSort"" Grid.Column=""2"" Content=""Sort Apps ▾""
                                    Background=""#FFFFFF"" Foreground=""#0F172A"" FontWeight=""SemiBold"" FontSize=""11.5""
                                    Padding=""12,5"" Cursor=""Hand"" BorderThickness=""1"" BorderBrush=""#CBD5E1"" VerticalAlignment=""Center""/>
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
                <StackPanel Orientation=""Horizontal"" Visibility=""Collapsed"">
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
                    <Button x:Name=""BtnViewReport"" Content=""Summary Report"" Background=""#0066FF"" Foreground=""#FFFFFF""
                            FontWeight=""SemiBold"" Padding=""14,6"" Margin=""0,0,6,0"" Cursor=""Hand"" BorderThickness=""0"" FontSize=""11.5"" Visibility=""Collapsed""/>
                    <Button x:Name=""BtnOpenLogFile"" Content=""Open Log File"" Background=""#FFFFFF"" Foreground=""#0F172A""
                            FontWeight=""SemiBold"" Padding=""14,6"" Margin=""0,0,6,0"" Cursor=""Hand"" BorderThickness=""1"" BorderBrush=""#CBD5E1"" FontSize=""11.5""/>
                    <Button x:Name=""BtnClearLogConsole"" Content=""Clear Screen"" Background=""#FFFFFF"" Foreground=""#0F172A""
                            FontWeight=""SemiBold"" Padding=""14,6"" Cursor=""Hand"" BorderThickness=""1"" BorderBrush=""#CBD5E1"" FontSize=""11.5""/>
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
        <Border x:Name=""BorderBottomBar"" Grid.Row=""2"" Background=""#1F1F1F"" Padding=""24,12"" BorderBrush=""#333333"" BorderThickness=""0,1,0,0"">
            <Grid VerticalAlignment=""Center"">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width=""Auto""/>
                    <ColumnDefinition Width=""*""/>
                    <ColumnDefinition Width=""Auto""/>
                </Grid.ColumnDefinitions>

                <!-- Left: Tip pill (matching mockup) -->
                <StackPanel Grid.Column=""0"" Orientation=""Horizontal"" VerticalAlignment=""Center"">
                    <Border x:Name=""BorderSetupTip"" Background=""#EFF6FF"" BorderBrush=""#BFDBFE"" BorderThickness=""1"" CornerRadius=""16"" Padding=""12,6"" VerticalAlignment=""Center"">
                        <StackPanel Orientation=""Horizontal"" VerticalAlignment=""Center"">
                            <TextBlock Text=""💡"" FontSize=""12"" Margin=""0,0,6,0"" VerticalAlignment=""Center""/>
                            <TextBlock x:Name=""TxtSetupTip"" Text=""Tip: You can customize any pack, add or remove apps, and export your setup for later use.""
                                       FontSize=""11.5"" Foreground=""#1D4ED8"" VerticalAlignment=""Center""/>
                        </StackPanel>
                    </Border>
                </StackPanel>

                <StackPanel Grid.Column=""1"" Orientation=""Horizontal"" VerticalAlignment=""Center"" Margin=""16,0,16,0"">
                    <TextBlock x:Name=""TxtStatus"" Text="""" FontSize=""12"" FontWeight=""SemiBold"" Foreground=""#64748B"" VerticalAlignment=""Center"" TextTrimming=""CharacterEllipsis""/>
                    <StackPanel x:Name=""StackBottomSelectedPackInfo"" Orientation=""Horizontal"" VerticalAlignment=""Center"" Visibility=""Collapsed"">
                        <TextBlock Text=""|"" FontSize=""12"" Foreground=""#CBD5E1"" VerticalAlignment=""Center"" Margin=""0,0,14,0""/>
                        <TextBlock x:Name=""TxtBottomSelectedPackInfo"" Text=""App pack selected: Top Choice • Recommended""
                                   FontSize=""12"" Foreground=""#475569"" VerticalAlignment=""Center""/>
                    </StackPanel>
                </StackPanel>

                <StackPanel Grid.Column=""2"" Orientation=""Horizontal"" HorizontalAlignment=""Right"" VerticalAlignment=""Center"">
                    <Button x:Name=""BtnImportPack"" Content=""📥 Import Pack""
                            Background=""#FFFFFF"" Foreground=""#0F172A"" BorderBrush=""#CBD5E1"" BorderThickness=""1""
                            FontWeight=""SemiBold"" FontSize=""12"" Padding=""14,8"" Margin=""0,0,8,0""
                            Cursor=""Hand"" ToolTip=""Import a custom app pack (.json)""/>
                    <Button x:Name=""BtnCreateNewGroup"" Content=""+ Create New Pack""
                            Background=""#0066FF"" Foreground=""#FFFFFF"" FontWeight=""Bold"" FontSize=""12""
                            Padding=""16,8"" Margin=""0,0,0,0"" Cursor=""Hand"" BorderThickness=""0""
                            ToolTip=""Create a new custom app pack""/>
                    <Button x:Name=""BtnStop"" Content=""Stop Install""
                            Background=""#DC2626"" Foreground=""#FFFFFF"" FontWeight=""Bold"" FontSize=""13""
                            Padding=""20,8"" Margin=""0,0,10,0"" Cursor=""Hand"" BorderThickness=""0"" Visibility=""Collapsed""/>
                    <Button x:Name=""BtnInstall"" Content=""📥  Install Selected Apps""
                            Background=""#0066FF"" Foreground=""#FFFFFF"" FontWeight=""Bold"" FontSize=""13""
                            Padding=""24,8"" Cursor=""Hand"" BorderThickness=""0"" Visibility=""Collapsed""/>
                </StackPanel>
            </Grid>
        </Border>
    </Grid>

    <!-- ========================================================================= -->
    <!-- GROUP EDITOR MODAL OVERLAY (CREATE / UPDATE GROUP)                        -->
    <!-- ========================================================================= -->
    <Grid x:Name=""GroupModalOverlay"" Grid.ColumnSpan=""2"" Background=""#90000000"" Visibility=""Collapsed"">
        <Border x:Name=""BorderModalCard"" MaxWidth=""980"" MaxHeight=""780"" Margin=""16,10,16,10"" Background=""#FFFFFF"" CornerRadius=""16""
                BorderBrush=""#E2E8F0"" BorderThickness=""1"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""
                Padding=""26,20,26,20"">
            <Border.Effect>
                <DropShadowEffect BlurRadius=""32"" ShadowDepth=""6"" Direction=""270"" Color=""#000000"" Opacity=""0.25""/>
            </Border.Effect>
            <Grid>
                <Grid.RowDefinitions>
                    <RowDefinition Height=""Auto""/> <!-- 0. Header (Title, Subtitle & Close) -->
                    <RowDefinition Height=""Auto""/> <!-- 1. Pack Identity (Icon Preview, Name & Description) -->
                    <RowDefinition Height=""Auto""/> <!-- 2. Quick Icon Select Carousel -->
                    <RowDefinition Height=""Auto""/> <!-- 3. App Search & Quick Controls -->
                    <RowDefinition Height=""*"" MinHeight=""140"" MaxHeight=""440""/> <!-- 4. App Checklist -->
                    <RowDefinition Height=""Auto""/> <!-- 5. Bottom Actions Footer -->
                </Grid.RowDefinitions>

                <!-- 0. Header Area -->
                <Grid Grid.Row=""0"" Margin=""0,0,0,16"">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width=""*""/>
                        <ColumnDefinition Width=""Auto""/>
                    </Grid.ColumnDefinitions>

                    <StackPanel Orientation=""Horizontal"" VerticalAlignment=""Center"">
                        <Border x:Name=""BorderModalHeaderBadge"" Width=""42"" Height=""42"" CornerRadius=""10"" Background=""#0066FF"" Margin=""0,0,14,0"" VerticalAlignment=""Center"">
                            <TextBlock x:Name=""TxtModalHeaderGlyph"" Text=""&#xE74C;"" FontFamily=""Segoe MDL2 Assets, Segoe Fluent Icons, Segoe UI Symbol"" FontSize=""20"" Foreground=""#FFFFFF"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                        </Border>
                        <StackPanel VerticalAlignment=""Center"">
                            <TextBlock x:Name=""TxtModalTitle"" Text=""Edit App Pack"" FontSize=""20"" FontWeight=""Bold"" Foreground=""#0F172A""/>
                            <TextBlock x:Name=""TxtModalSubtitle"" Text=""Customize pack details, icon, and included software."" FontSize=""12.5"" Foreground=""#64748B"" Margin=""0,2,0,0""/>
                        </StackPanel>
                    </StackPanel>

                    <Button x:Name=""BtnCloseModal"" Grid.Column=""1"" Content=""✕"" Background=""#F1F5F9"" Foreground=""#64748B"" FontSize=""12"" FontWeight=""Bold""
                            Width=""32"" Height=""32"" Cursor=""Hand"" BorderThickness=""0"" HorizontalAlignment=""Right"" VerticalAlignment=""Center""/>
                </Grid>

                <!-- 1. Pack Identity (Icon Preview Card + Pack Name & Description) -->
                <Grid Grid.Row=""1"" Margin=""0,0,0,12"">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width=""84""/>
                        <ColumnDefinition Width=""*""/>
                    </Grid.ColumnDefinitions>

                    <!-- Pack Icon Preview Box -->
                    <StackPanel Grid.Column=""0"">
                        <TextBlock Text=""Pack Icon"" FontSize=""11"" FontWeight=""SemiBold"" Foreground=""#475569"" Margin=""0,0,0,5""/>
                        <Border x:Name=""BorderPackIconCard"" Width=""74"" Height=""74"" CornerRadius=""12"" Background=""#F8FAFC"" BorderBrush=""#E2E8F0"" BorderThickness=""1"" Cursor=""Hand"" ToolTip=""Click to cycle icon"">
                            <Grid>
                                <Image x:Name=""ImgPackIconPreview"" Width=""46"" Height=""46"" Stretch=""Uniform"" HorizontalAlignment=""Center"" VerticalAlignment=""Center"" Visibility=""Collapsed""/>
                                <TextBlock x:Name=""TxtPackIconPreview"" Text=""🎨"" FontSize=""34"" FontFamily=""Segoe UI Emoji, Segoe UI Symbol, Segoe UI"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                                <Border x:Name=""BorderPackIconBadgeEdit"" Width=""20"" Height=""20"" CornerRadius=""10"" Background=""#0066FF"" HorizontalAlignment=""Right"" VerticalAlignment=""Bottom"" Margin=""0,0,4,4"">
                                    <TextBlock Text=""&#xE70F;"" FontFamily=""Segoe MDL2 Assets, Segoe Fluent Icons, Segoe UI Symbol"" FontSize=""9.5"" Foreground=""#FFFFFF"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                                </Border>
                            </Grid>
                        </Border>
                        <!-- Synced Input (Kept for compatibility) -->
                        <TextBox x:Name=""TxtGroupIconInput"" Text=""🎨"" Visibility=""Collapsed""/>
                    </StackPanel>

                    <!-- Name & Description Fields -->
                    <StackPanel Grid.Column=""1"" Margin=""14,0,0,0"">
                        <!-- Pack Name Row -->
                        <StackPanel Orientation=""Horizontal"" Margin=""0,0,0,4"">
                            <TextBlock Text=""Pack Name"" FontSize=""11"" FontWeight=""SemiBold"" Foreground=""#475569""/>
                            <TextBlock Text="" *"" FontSize=""11"" FontWeight=""Bold"" Foreground=""#EF4444""/>
                        </StackPanel>
                        <Border x:Name=""BorderGroupName"" Height=""38"" CornerRadius=""8"" Background=""#FFFFFF"" BorderBrush=""#E2E8F0"" BorderThickness=""1"" Margin=""0,0,0,8"">
                            <TextBox x:Name=""TxtGroupNameInput"" BorderThickness=""0"" Background=""Transparent"" Foreground=""#0F172A""
                                     FontSize=""13"" Padding=""12,7"" VerticalContentAlignment=""Center""/>
                        </Border>

                        <!-- Description Row -->
                        <Grid Margin=""0,0,0,4"">
                            <TextBlock Text=""Description"" FontSize=""11"" FontWeight=""SemiBold"" Foreground=""#475569"" HorizontalAlignment=""Left""/>
                            <TextBlock x:Name=""TxtGroupDescCharCount"" Text=""0 / 200"" FontSize=""11"" Foreground=""#94A3B8"" HorizontalAlignment=""Right""/>
                        </Grid>
                        <Border x:Name=""BorderGroupDesc"" Height=""38"" CornerRadius=""8"" Background=""#FFFFFF"" BorderBrush=""#E2E8F0"" BorderThickness=""1"">
                            <TextBox x:Name=""TxtGroupDescInput"" MaxLength=""200"" BorderThickness=""0"" Background=""Transparent"" Foreground=""#0F172A""
                                     FontSize=""12.5"" Padding=""12,7"" VerticalContentAlignment=""Center""/>
                        </Border>
                    </StackPanel>
                </Grid>

                <!-- 2. Quick Icon Select Carousel -->
                <StackPanel x:Name=""PanelEmojiChips"" Grid.Row=""2"" Margin=""0,0,0,12"">
                    <TextBlock Text=""Quick Icon Select"" FontSize=""11"" FontWeight=""SemiBold"" Foreground=""#475569"" Margin=""0,0,0,6""/>
                    <Grid>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width=""Auto""/>
                            <ColumnDefinition Width=""*""/>
                            <ColumnDefinition Width=""Auto""/>
                        </Grid.ColumnDefinitions>
                        <Button x:Name=""BtnIconPrev"" Grid.Column=""0"" Content=""&#x2039;"" FontSize=""16"" FontWeight=""Bold"" Width=""28"" Height=""38""
                                Background=""Transparent"" Foreground=""#64748B"" BorderBrush=""#E2E8F0"" BorderThickness=""1"" Cursor=""Hand"" Margin=""0,0,6,0""/>
                        <WrapPanel x:Name=""EmojiChipsPanel"" Grid.Column=""1"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                        <Button x:Name=""BtnIconNext"" Grid.Column=""2"" Content=""&#x203A;"" FontSize=""16"" FontWeight=""Bold"" Width=""28"" Height=""38""
                                Background=""Transparent"" Foreground=""#64748B"" BorderBrush=""#E2E8F0"" BorderThickness=""1"" Cursor=""Hand"" Margin=""6,0,0,0""/>
                    </Grid>
                </StackPanel>

                <!-- 3. App Search Input Row -->
                <Grid Grid.Row=""3"" Margin=""0,0,0,8"">
                    <!-- Search Box -->
                    <Border x:Name=""BorderModalSearchBox"" Height=""38"" CornerRadius=""8"" Background=""#FFFFFF"" BorderBrush=""#CBD5E1"" BorderThickness=""1"" Padding=""12,0"">
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width=""Auto""/>
                                <ColumnDefinition Width=""*""/>
                            </Grid.ColumnDefinitions>
                            <TextBlock Text=""&#xE721;"" FontFamily=""Segoe MDL2 Assets, Segoe Fluent Icons, Segoe UI Symbol"" FontSize=""12"" Foreground=""#64748B"" VerticalAlignment=""Center"" Margin=""0,0,10,0""/>
                            <TextBox x:Name=""TxtModalAppSearch"" Grid.Column=""1"" BorderThickness=""0"" Background=""Transparent"" Foreground=""#0F172A"" FontSize=""12.5"" VerticalContentAlignment=""Center"" Text=""Filter software...""/>
                        </Grid>
                    </Border>
                    <TextBlock x:Name=""TxtModalSelectedCount"" Visibility=""Collapsed""/>
                </Grid>

                <!-- 4. App Checklist Table with Pinned Header (Like All Apps) -->
                <Border x:Name=""BorderModalAppsContainer"" Grid.Row=""4"" Background=""#FFFFFF"" CornerRadius=""10"" Padding=""12,10,12,10"" BorderBrush=""#E2E8F0"" BorderThickness=""1"" Margin=""0,0,0,14"">
                    <Grid>
                        <Grid.RowDefinitions>
                            <RowDefinition Height=""Auto""/> <!-- Top Table Header Bar (like All Apps) -->
                            <RowDefinition Height=""*""/>    <!-- Scrollable Apps List -->
                        </Grid.RowDefinitions>

                        <!-- Pinned Table Header Bar with Master CheckBox and Role Badge (like All Apps) -->
                        <Border x:Name=""BorderModalTableHeader"" Grid.Row=""0"" Margin=""0,0,0,6"" Padding=""4,2,4,10"" BorderThickness=""0,0,0,1"" BorderBrush=""#F1F5F9"">
                            <Grid>
                                <Grid.ColumnDefinitions>
                                    <ColumnDefinition Width=""Auto""/>
                                    <ColumnDefinition Width=""*""/>
                                    <ColumnDefinition Width=""Auto""/>
                                </Grid.ColumnDefinitions>
                                <CheckBox x:Name=""ChkModalSelectAll"" Grid.Column=""0"" Content=""Select All Software"" IsThreeState=""True"" FontSize=""12.5"" FontWeight=""SemiBold"" Foreground=""#0F172A"" VerticalAlignment=""Center"" Cursor=""Hand""/>
                                <TextBlock x:Name=""TxtModalTableHeaderCount"" Grid.Column=""1"" HorizontalAlignment=""Right"" VerticalAlignment=""Center"" FontSize=""11.5"" Foreground=""#64748B"" Margin=""0,0,12,0"" Text=""7 of 27 software selected""/>
                                <Border x:Name=""BadgeModalHeaderRole"" Grid.Column=""2"" Background=""#0066FF"" CornerRadius=""12"" Padding=""12,3"" VerticalAlignment=""Center"">
                                    <TextBlock x:Name=""TxtModalHeaderBadgeRole"" Text=""Active in Role"" FontSize=""10"" FontWeight=""Bold"" Foreground=""#FFFFFF""/>
                                </Border>
                            </Grid>
                        </Border>

                        <ScrollViewer x:Name=""ModalAppsScrollViewer"" Grid.Row=""1"" VerticalScrollBarVisibility=""Auto"" HorizontalScrollBarVisibility=""Disabled"">
                            <StackPanel x:Name=""ModalAppsContainer""/>
                        </ScrollViewer>
                    </Grid>
                </Border>

                <!-- 5. Footer Actions (Export Removed) -->
                <Grid Grid.Row=""5"">
                    <StackPanel Orientation=""Horizontal"" HorizontalAlignment=""Left"">
                        <Button x:Name=""BtnDeleteGroup"" Content=""🗑️ Delete Pack"" Background=""#FEF2F2"" BorderBrush=""#FCA5A5"" BorderThickness=""1""
                                Foreground=""#DC2626"" FontWeight=""SemiBold"" Padding=""14,8"" Cursor=""Hand""
                                FontSize=""12"" Visibility=""Collapsed""/>
                    </StackPanel>

                    <StackPanel Orientation=""Horizontal"" HorizontalAlignment=""Right"">
                        <Button x:Name=""BtnCancelModal"" Content=""Cancel"" Background=""#FFFFFF"" Foreground=""#334155"" BorderBrush=""#CBD5E1""
                                FontWeight=""SemiBold"" Padding=""20,8"" Margin=""0,0,10,0"" Cursor=""Hand"" BorderThickness=""1"" FontSize=""12""/>
                        <Button x:Name=""BtnSaveGroup"" Content=""💾 Save Pack"" Background=""#0066FF"" Foreground=""#FFFFFF""
                                FontWeight=""Bold"" Padding=""22,8"" Cursor=""Hand"" BorderThickness=""0"" FontSize=""12""/>
                    </StackPanel>
                </Grid>
            </Grid>
        </Border>
    </Grid>


    <!-- ========================================================================= -->
    <!-- MODAL: ONLINE / WINGET CLOUD SEARCH                                       -->
    <!-- ========================================================================= -->
    <Grid x:Name=""ModalOnlineSearchOverlay"" Grid.ColumnSpan=""2"" Background=""#90000000"" Visibility=""Collapsed"">
        <Border x:Name=""BorderOnlineSearchCard"" MaxWidth=""980"" MaxHeight=""740"" Margin=""16,10,16,10"" Background=""#FFFFFF"" CornerRadius=""16""
                BorderBrush=""#E2E8F0"" BorderThickness=""1"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""
                Padding=""26,22,26,20"">
            <Border.Effect>
                <DropShadowEffect BlurRadius=""32"" ShadowDepth=""6"" Direction=""270"" Color=""#000000"" Opacity=""0.25""/>
            </Border.Effect>
            <Grid>
                <Grid.RowDefinitions>
                    <RowDefinition Height=""Auto""/> <!-- 0. Header (Title, Subtitle & Close) -->
                    <RowDefinition Height=""Auto""/> <!-- 1. Search Bar & Quick Suggestion Tags -->
                    <RowDefinition Height=""Auto""/> <!-- 2. Status & Source Indicator Bar -->
                    <RowDefinition Height=""*"" MinHeight=""140"" MaxHeight=""450""/> <!-- 3. Scrollable Results Area -->
                    <RowDefinition Height=""Auto""/> <!-- 4. Bottom Actions Footer -->
                </Grid.RowDefinitions>

                <!-- 0. Header Area -->
                <Grid Grid.Row=""0"" Margin=""0,0,0,16"">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width=""*""/>
                        <ColumnDefinition Width=""Auto""/>
                    </Grid.ColumnDefinitions>

                    <StackPanel Orientation=""Horizontal"" VerticalAlignment=""Center"">
                        <Border x:Name=""BorderOnlineModalIcon"" Width=""42"" Height=""42"" CornerRadius=""10"" Background=""#7C3AED"" Margin=""0,0,14,0"" VerticalAlignment=""Center"">
                            <TextBlock Text=""&#xE753;"" FontFamily=""Segoe MDL2 Assets, Segoe Fluent Icons, Segoe UI Symbol"" FontSize=""20"" Foreground=""#FFFFFF"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                        </Border>
                        <StackPanel VerticalAlignment=""Center"">
                            <TextBlock x:Name=""TxtOnlineModalTitle"" Text=""Search Online Applications"" FontSize=""20"" FontWeight=""Bold"" Foreground=""#0F172A""/>
                            <TextBlock x:Name=""TxtOnlineModalSubtitle"" Text=""Query millions of Windows applications in real-time from Microsoft Winget &amp; Cloud repository."" FontSize=""12.5"" Foreground=""#64748B"" Margin=""0,2,0,0""/>
                        </StackPanel>
                    </StackPanel>

                    <Button x:Name=""BtnCloseOnlineModal"" Grid.Column=""1"" Content=""✕"" Background=""#F1F5F9"" Foreground=""#64748B"" FontSize=""12"" FontWeight=""Bold""
                            Width=""32"" Height=""32"" Cursor=""Hand"" BorderThickness=""0"" HorizontalAlignment=""Right"" VerticalAlignment=""Center""/>
                </Grid>

                <!-- 1. Search Bar & Quick Suggestion Chips -->
                <StackPanel Grid.Row=""1"" Margin=""0,0,0,12"">
                    <!-- Search Input Row -->
                    <Grid Margin=""0,0,0,8"">
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width=""*""/>
                            <ColumnDefinition Width=""Auto""/>
                        </Grid.ColumnDefinitions>

                        <Border x:Name=""BorderOnlineSearchInput"" Height=""42"" CornerRadius=""8"" Background=""#FFFFFF"" BorderBrush=""#CBD5E1"" BorderThickness=""1"" Padding=""12,0"" Margin=""0,0,10,0"">
                            <Grid>
                                <Grid.ColumnDefinitions>
                                    <ColumnDefinition Width=""Auto""/>
                                    <ColumnDefinition Width=""*""/>
                                    <ColumnDefinition Width=""Auto""/>
                                </Grid.ColumnDefinitions>
                                <TextBlock Text=""🔍"" FontSize=""13"" VerticalAlignment=""Center"" Margin=""0,0,10,0""/>
                                <TextBox x:Name=""TxtOnlineModalSearchInput"" Grid.Column=""1"" BorderThickness=""0"" Background=""Transparent"" Foreground=""#0F172A""
                                         FontSize=""13.5"" VerticalContentAlignment=""Center"" Text=""Type software name (e.g. Discord, Blender, Spotify, Zoom, Steam)...""/>
                                <Button x:Name=""BtnOnlineClearSearch"" Grid.Column=""2"" Content=""✕"" Width=""22"" Height=""22"" Background=""Transparent"" Foreground=""#94A3B8""
                                        BorderThickness=""0"" Cursor=""Hand"" FontSize=""11"" Visibility=""Collapsed"" VerticalAlignment=""Center""/>
                            </Grid>
                        </Border>

                        <Button x:Name=""BtnOnlineModalExecuteSearch"" Grid.Column=""1"" Height=""42"" Padding=""22,0"" Background=""#7C3AED"" Foreground=""#FFFFFF""
                                BorderThickness=""0"" FontWeight=""Bold"" FontSize=""13"" Cursor=""Hand"">
                            <StackPanel Orientation=""Horizontal"" VerticalAlignment=""Center"">
                                <TextBlock Text=""🔍"" Margin=""0,0,8,0"" FontSize=""12""/>
                                <TextBlock x:Name=""TxtOnlineSearchBtnText"" Text=""Search Online""/>
                            </StackPanel>
                        </Button>
                    </Grid>

                    <!-- Quick Suggestion Tags -->
                    <StackPanel Orientation=""Horizontal"" VerticalAlignment=""Center"">
                        <TextBlock Text=""Popular:"" FontSize=""11.5"" FontWeight=""SemiBold"" Foreground=""#64748B"" VerticalAlignment=""Center"" Margin=""2,0,8,0""/>
                        <WrapPanel x:Name=""OnlineSearchTagsPanel"" VerticalAlignment=""Center""/>
                    </StackPanel>
                </StackPanel>

                <!-- 2. Status & Count Bar (Pinned Header Style) -->
                <Border x:Name=""BorderOnlineModalStatusHeader"" Grid.Row=""2"" Background=""#F8FAFC"" CornerRadius=""8"" Padding=""14,9"" Margin=""0,0,0,10"" BorderBrush=""#E2E8F0"" BorderThickness=""1"">
                    <Grid>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width=""*""/>
                            <ColumnDefinition Width=""Auto""/>
                        </Grid.ColumnDefinitions>
                        <TextBlock x:Name=""TxtOnlineModalStatus"" Grid.Column=""0"" Text=""Enter any software name above and press Enter or click Search Online."" FontSize=""12"" Foreground=""#64748B"" VerticalAlignment=""Center"" TextTrimming=""CharacterEllipsis""/>
                        <StackPanel Grid.Column=""1"" Orientation=""Horizontal"" VerticalAlignment=""Center"">
                            <Border Background=""#EDE9FE"" CornerRadius=""10"" Padding=""10,2"">
                                <TextBlock Text=""🌐 Microsoft Winget Cloud"" FontSize=""10.5"" FontWeight=""Bold"" Foreground=""#7C3AED""/>
                            </Border>
                        </StackPanel>
                    </Grid>
                </Border>

                <!-- 3. Scrollable Results Area -->
                <Border x:Name=""BorderOnlineModalResultsContainer"" Grid.Row=""3"" Background=""#FFFFFF"" CornerRadius=""10"" Padding=""10"" BorderBrush=""#E2E8F0"" BorderThickness=""1"" Margin=""0,0,0,14"">
                    <ScrollViewer x:Name=""OnlineModalResultsScrollViewer"" VerticalScrollBarVisibility=""Auto"" HorizontalScrollBarVisibility=""Disabled"">
                        <StackPanel x:Name=""OnlineModalResultsPanel""/>
                    </ScrollViewer>
                </Border>

                <!-- 4. Footer Actions Area -->
                <Grid Grid.Row=""4"">
                    <StackPanel Orientation=""Horizontal"" HorizontalAlignment=""Left"" VerticalAlignment=""Center"">
                        <TextBlock x:Name=""TxtOnlineModalSelectedCount"" Text=""0 apps selected"" FontSize=""12"" FontWeight=""SemiBold"" Foreground=""#64748B"" VerticalAlignment=""Center""/>
                    </StackPanel>

                    <StackPanel Orientation=""Horizontal"" HorizontalAlignment=""Right"" VerticalAlignment=""Center"">
                        <Button x:Name=""BtnCancelOnlineModal"" Content=""Close"" Background=""#FFFFFF"" Foreground=""#334155"" BorderBrush=""#CBD5E1""
                                FontWeight=""SemiBold"" Padding=""20,8"" Margin=""0,0,10,0"" Cursor=""Hand"" BorderThickness=""1"" FontSize=""12""/>
                        <Button x:Name=""BtnOnlineAddSelected"" Content=""➕ Add Selected to Catalog"" Background=""#0066FF"" Foreground=""#FFFFFF""
                                FontWeight=""Bold"" Padding=""18,8"" Margin=""0,0,10,0"" Cursor=""Hand"" BorderThickness=""0"" FontSize=""12"" Visibility=""Collapsed""/>
                        <Button x:Name=""BtnOnlineInstallSelected"" Content=""⚡ Install Selected"" Background=""#059669"" Foreground=""#FFFFFF""
                                FontWeight=""Bold"" Padding=""20,8"" Cursor=""Hand"" BorderThickness=""0"" FontSize=""12"" Visibility=""Collapsed""/>
                    </StackPanel>
                </Grid>
            </Grid>
        </Border>
    </Grid>

    <!-- ========================================================================= -->
    <!-- MODAL 1: CHECK FOR UPDATES                                                -->
    <!-- ========================================================================= -->
    <Grid x:Name=""ModalUpdatesOverlay"" Grid.ColumnSpan=""2"" Background=""#75000000"" Visibility=""Collapsed"">
        <Border x:Name=""BorderUpdatesCard"" MaxWidth=""580"" MaxHeight=""640"" Margin=""16,10,16,10"" Background=""#181B22"" CornerRadius=""16"" BorderBrush=""#2D3342"" BorderThickness=""1""
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
                            <TextBlock x:Name=""TxtUpdateDetail"" Text=""FleetForge v2.7.0 (Enterprise IT Edition • Love Malhotra) is up to date."" FontSize=""11.5"" Foreground=""#94A3B8"" Margin=""0,3,0,0""/>
                        </StackPanel>
                    </Grid>
                </Border>

                <!-- What's New / Changelog Section -->
                <Border x:Name=""BorderChangelogCard"" Background=""#11141A"" CornerRadius=""10"" Padding=""16"" BorderBrush=""#232938"" BorderThickness=""1"" Margin=""0,0,0,16"">
                    <StackPanel>
                        <TextBlock x:Name=""TxtChangelogHeading"" Text=""Release Highlights &amp; Changelog:"" FontSize=""12"" FontWeight=""SemiBold"" Foreground=""#E2E8F0"" Margin=""0,0,0,10""/>
                        <StackPanel x:Name=""PnlChangelogList"">
                            <TextBlock Text=""• Zero-touch silent enterprise software deployment engine"" FontSize=""11.5"" Foreground=""#94A3B8"" Margin=""0,2.5,0,2.5""/>
                            <TextBlock Text=""• Pre-cached enterprise catalog with 16,349 software packages"" FontSize=""11.5"" Foreground=""#94A3B8"" Margin=""0,2.5,0,2.5""/>
                            <TextBlock Text=""• Real-time synchronization with Windows Light / Dark themes"" FontSize=""11.5"" Foreground=""#94A3B8"" Margin=""0,2.5,0,2.5""/>
                            <TextBlock Text=""• High-speed direct cloud package delivery &amp; affiliate distribution"" FontSize=""11.5"" Foreground=""#94A3B8"" Margin=""0,2.5,0,2.5""/>
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
                        <Button x:Name=""BtnCheckUpdatesAgain"" Content=""Check for Updates"" Background=""#FFFFFF"" Foreground=""#0F172A"" BorderBrush=""#CBD5E1"" FontWeight=""SemiBold""
                                Padding=""22,8"" Cursor=""Hand"" BorderThickness=""1"" FontSize=""12""/>
                        <Button x:Name=""BtnDownloadInstall"" Content=""Download &amp; Install Update"" Background=""#0066FF"" Foreground=""#FFFFFF"" FontWeight=""Bold""
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
        <Border x:Name=""BorderSettingsCard"" MaxWidth=""680"" MaxHeight=""680"" Margin=""16,10,16,10"" Background=""#181B22"" CornerRadius=""16"" BorderBrush=""#2D3342"" BorderThickness=""1""
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

                <!-- Section 1.5: Display Resolution & UI Scaling -->
                <TextBlock x:Name=""TxtSettingsSecDisplayTitle"" Text=""🖥️ DISPLAY RESOLUTION &amp; UI SCALING"" FontSize=""11"" FontWeight=""Bold"" Foreground=""#38BDF8"" Margin=""0,0,0,8""/>
                <Border x:Name=""BorderSettingsSecDisplay"" Background=""#11141A"" CornerRadius=""10"" Padding=""14"" BorderBrush=""#232938"" BorderThickness=""1"" Margin=""0,0,0,14"">
                    <StackPanel>
                        <TextBlock Text=""Automatically adapt UI zoom and layout according to your screen resolution: "" FontSize=""11"" Foreground=""#94A3B8"" Margin=""0,0,0,10""/>
                        <Grid Margin=""0,0,0,4"">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width=""*""/>
                                <ColumnDefinition Width=""Auto""/>
                            </Grid.ColumnDefinitions>
                            <WrapPanel Grid.Column=""0"" VerticalAlignment=""Center"">
                                <RadioButton x:Name=""RbScaleAuto"" Content=""Auto (Adaptive)"" IsChecked=""True"" Foreground=""#FFFFFF"" FontSize=""11.5"" FontWeight=""SemiBold"" Margin=""0,0,14,4"" Cursor=""Hand""/>
                                <RadioButton x:Name=""RbScale100"" Content=""100% (Standard)"" Foreground=""#FFFFFF"" FontSize=""11.5"" Margin=""0,0,14,4"" Cursor=""Hand""/>
                                <RadioButton x:Name=""RbScale90"" Content=""90% (Compact)"" Foreground=""#FFFFFF"" FontSize=""11.5"" Margin=""0,0,14,4"" Cursor=""Hand""/>
                                <RadioButton x:Name=""RbScale80"" Content=""80% (Small Displays)"" Foreground=""#FFFFFF"" FontSize=""11.5"" Margin=""0,0,14,4"" Cursor=""Hand""/>
                            </WrapPanel>
                            <TextBlock x:Name=""TxtCurrentScaleActive"" Grid.Column=""1"" Text=""Active: 100%"" FontSize=""11"" FontWeight=""Bold"" Foreground=""#10B981"" VerticalAlignment=""Center""/>
                        </Grid>
                    </StackPanel>
                </Border>

                <!-- Section 2: Storage -->
                <TextBlock x:Name=""TxtSettingsSec2Title"" Text=""📦 DOWNLOAD CACHE &amp; CLEANUP"" FontSize=""11"" FontWeight=""Bold"" Foreground=""#38BDF8"" Margin=""0,0,0,8""/>
                <Border x:Name=""BorderSettingsSec2"" Background=""#11141A"" CornerRadius=""10"" Padding=""14"" BorderBrush=""#232938"" BorderThickness=""1"" Margin=""0,0,0,14"">
                    <StackPanel>
                        <TextBlock Text=""Temporary downloaded setup files and cache stored in %TEMP%\FleetForgeDownloads:"" FontSize=""11"" Foreground=""#94A3B8"" Margin=""0,0,0,10""/>
                        <StackPanel Orientation=""Horizontal"">
                            <Button x:Name=""BtnClearCache"" Content=""Clear Download Cache &amp; Temp Files"" Background=""#FFFFFF"" Foreground=""#0F172A"" BorderBrush=""#CBD5E1""
                                    FontWeight=""SemiBold"" Padding=""14,6"" Cursor=""Hand"" BorderThickness=""1"" FontSize=""11"" Margin=""0,0,10,0""/>
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
                        <StackPanel Grid.Column=""1"" Orientation=""Horizontal"" VerticalAlignment=""Center"">
                            <Button x:Name=""BtnExportCatalog"" Content=""Export App List"" Background=""#FFFFFF"" Foreground=""#0F172A"" BorderBrush=""#CBD5E1""
                                    FontWeight=""SemiBold"" Padding=""14,6"" Cursor=""Hand"" BorderThickness=""1"" FontSize=""11.5""/>
                        </StackPanel>
                    </Grid>
                </Border>

                <!-- Bottom Actions -->
                <Grid>
                    <Button x:Name=""BtnCancelSettings"" Content=""Cancel"" Background=""#FFFFFF"" Foreground=""#0F172A"" BorderBrush=""#CBD5E1""
                            FontWeight=""SemiBold"" Padding=""20,8"" HorizontalAlignment=""Left"" Cursor=""Hand"" BorderThickness=""1"" FontSize=""12""/>
                    <Button x:Name=""BtnSaveSettings"" Content=""Save Settings"" Background=""#0066FF"" Foreground=""#FFFFFF""
                            FontWeight=""Bold"" Padding=""26,8"" HorizontalAlignment=""Right"" Cursor=""Hand"" BorderThickness=""0"" FontSize=""12""/>
                </Grid>
            </StackPanel>
        </Border>
    </Grid>

    <!-- ========================================================================= -->
    <!-- MODAL 3: ABOUT FLEETFORGE                                                 -->
    <!-- ========================================================================= -->
    <Grid x:Name=""ModalAboutOverlay"" Grid.ColumnSpan=""2"" Background=""#75000000"" Visibility=""Collapsed"">
        <Border x:Name=""BorderAboutCard"" MaxWidth=""580"" MaxHeight=""640"" Margin=""16,10,16,10"" Background=""#181B22"" CornerRadius=""16"" BorderBrush=""#2D3342"" BorderThickness=""1""
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
                            <TextBlock x:Name=""TxtAboutCompany"" Text=""Love Malhotra"" FontSize=""10.5"" FontWeight=""Bold"" Foreground=""#5f6d84""/>
                        </Border>
                        <Border x:Name=""BadgeAboutEdition"" Background=""#1E293B"" CornerRadius=""5"" Padding=""8,3"" BorderThickness=""1"" BorderBrush=""#334155"">
                            <TextBlock x:Name=""TxtAboutEdition"" Text=""v2.7.0 • Personal &amp; Work Edition"" FontSize=""10.5"" FontWeight=""Bold"" Foreground=""#94A3B8""/>
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
                            <TextBlock x:Name=""TxtSpecOrgVal"" Text=""Love Malhotra"" FontSize=""11.5"" FontWeight=""SemiBold"" Foreground=""#5f6d84"" Margin=""0,1,0,8""/>

                            <TextBlock x:Name=""TxtSpecEngLbl"" Text=""Deployment Engine:"" FontSize=""10.5"" Foreground=""#64748B""/>
                            <TextBlock x:Name=""TxtSpecEngVal"" Text=""FleetForge Engine v2.7.0"" FontSize=""11.5"" FontWeight=""SemiBold"" Foreground=""#FFFFFF"" Margin=""0,1,0,8""/>

                            <TextBlock x:Name=""TxtSpecLibLbl"" Text=""App Library:"" FontSize=""10.5"" Foreground=""#64748B""/>
                            <TextBlock x:Name=""TxtSpecLibVal"" Text=""FleetForge Enterprise Cloud Catalog"" FontSize=""11.5"" FontWeight=""SemiBold"" Foreground=""#FFFFFF""/>
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
        <Border x:Name=""BorderTutorialCard"" MaxWidth=""680"" MaxHeight=""660"" Margin=""16,10,16,10"" Background=""#181B22"" CornerRadius=""16"" BorderBrush=""#2D3342"" BorderThickness=""1""
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

    <!-- ========================================================================= -->
    <!-- MODAL: CUSTOM APP & AFFILIATE BUILDER                                     -->
    <!-- ========================================================================= -->
    <Grid x:Name=""ModalCustomAppBuilderOverlay"" Grid.ColumnSpan=""2"" Background=""#75000000"" Visibility=""Collapsed"">
        <Border x:Name=""BorderCustomAppBuilderCard"" MaxWidth=""880"" MaxHeight=""640"" Margin=""16,10,16,10"" Background=""#181B22"" CornerRadius=""16"" BorderBrush=""#2D3342"" BorderThickness=""1""
                HorizontalAlignment=""Center"" VerticalAlignment=""Center"" Padding=""24"">
            <Grid>
                <Grid.RowDefinitions>
                    <RowDefinition Height=""Auto""/>
                    <RowDefinition Height=""Auto""/>
                    <RowDefinition Height=""*""/>
                    <RowDefinition Height=""Auto""/>
                </Grid.RowDefinitions>

                <!-- Row 0: Title &amp; Close -->
                <Grid Grid.Row=""0"" Margin=""0,0,0,14"">
                    <StackPanel Orientation=""Horizontal"" VerticalAlignment=""Center"">
                        <Border Width=""34"" Height=""34"" Background=""#7C3AED"" CornerRadius=""8"" Margin=""0,0,12,0"" VerticalAlignment=""Center"">
                            <TextBlock Text=""🛠️"" FontSize=""16"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                        </Border>
                        <StackPanel VerticalAlignment=""Center"">
                            <StackPanel Orientation=""Horizontal"">
                                <TextBlock x:Name=""TxtModalAppBuilderTitle"" Text=""Custom App &amp; Affiliate Builder"" FontSize=""17"" FontWeight=""Bold"" Foreground=""#FFFFFF"" VerticalAlignment=""Center""/>
                                <Border Background=""#2E1065"" CornerRadius=""5"" Padding=""8,2"" Margin=""10,0,0,0"" VerticalAlignment=""Center"" BorderThickness=""1"" BorderBrush=""#581C87"">
                                    <TextBlock Text=""Direct URL Override"" FontSize=""10"" FontWeight=""SemiBold"" Foreground=""#D8B4FE""/>
                                </Border>
                            </StackPanel>
                            <TextBlock Text=""Customize download URLs, silent flags, or add software without touching the main database."" FontSize=""11"" Foreground=""#94A3B8"" Margin=""0,2,0,0""/>
                        </StackPanel>
                    </StackPanel>
                    <Button x:Name=""BtnCloseAppBuilder"" Content=""✕"" Background=""Transparent"" Foreground=""#8A8A8A"" FontSize=""15"" FontWeight=""Bold""
                            Width=""32"" Height=""32"" Cursor=""Hand"" BorderThickness=""0"" HorizontalAlignment=""Right"" VerticalAlignment=""Top""/>
                </Grid>

                <!-- Row 1: Add / Edit Form Card -->
                <Border x:Name=""BorderBuilderFormCard"" Grid.Row=""1"" Background=""#11141A"" CornerRadius=""10"" Padding=""14"" BorderBrush=""#232938"" BorderThickness=""1"" Margin=""0,0,0,12"">
                    <StackPanel>
                        <!-- Auto-fill Search from 16,349 apps -->
                        <TextBlock Text=""🔍 QUICK AUTO-FILL FROM MAIN CATALOG (16,349 APPS):"" FontSize=""10.5"" FontWeight=""Bold"" Foreground=""#38BDF8"" Margin=""0,0,0,6""/>
                        <Grid Margin=""0,0,0,6"">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width=""*""/>
                                <ColumnDefinition Width=""Auto""/>
                            </Grid.ColumnDefinitions>
                            <TextBox x:Name=""TxtBuilderSearchCatalog"" Grid.Column=""0"" Height=""30"" Background=""#0B0D11"" Foreground=""#FFFFFF""
                                     BorderBrush=""#252B38"" BorderThickness=""1"" Padding=""8,4"" FontSize=""11"" VerticalContentAlignment=""Center""/>
                            <Button x:Name=""BtnBuilderSearchFind"" Grid.Column=""1"" Content=""Search Apps"" Background=""#FFFFFF"" Foreground=""#0F172A"" BorderBrush=""#CBD5E1""
                                    FontWeight=""SemiBold"" Padding=""14,4"" Margin=""8,0,0,0"" Cursor=""Hand"" BorderThickness=""1"" FontSize=""11""/>
                        </Grid>
                        <ListBox x:Name=""LstBuilderSearchResults"" MaxHeight=""100"" Background=""#0B0D11"" Foreground=""#FFFFFF""
                                 BorderBrush=""#3B82F6"" BorderThickness=""1"" FontSize=""11"" Margin=""0,0,0,8"" Visibility=""Collapsed""/>

                        <!-- Two Column Input Grid -->
                        <Grid Margin=""0,4,0,8"">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width=""*""/>
                                <ColumnDefinition Width=""16""/>
                                <ColumnDefinition Width=""*""/>
                            </Grid.ColumnDefinitions>

                            <!-- Column 0 -->
                            <StackPanel Grid.Column=""0"">
                                <TextBlock Text=""App ID (Winget or Unique ID):"" FontSize=""11"" Foreground=""#94A3B8"" Margin=""0,0,0,3""/>
                                <TextBox x:Name=""TxtBuilderAppId"" Height=""28"" Background=""#0B0D11"" Foreground=""#FFFFFF""
                                         BorderBrush=""#252B38"" BorderThickness=""1"" Padding=""6,3"" FontSize=""11"" VerticalContentAlignment=""Center"" Margin=""0,0,0,6""/>

                                <TextBlock Text=""App Name (Display Name):"" FontSize=""11"" Foreground=""#94A3B8"" Margin=""0,0,0,3""/>
                                <TextBox x:Name=""TxtBuilderAppName"" Height=""28"" Background=""#0B0D11"" Foreground=""#FFFFFF""
                                         BorderBrush=""#252B38"" BorderThickness=""1"" Padding=""6,3"" FontSize=""11"" VerticalContentAlignment=""Center"" Margin=""0,0,0,6""/>

                                <TextBlock Text=""Category:"" FontSize=""11"" Foreground=""#94A3B8"" Margin=""0,0,0,3""/>
                                <ComboBox x:Name=""CmbBuilderCategory"" Height=""28"" Background=""#0B0D11"" Foreground=""#11141A"" FontSize=""11"" Margin=""0,0,0,6""/>

                                <TextBlock Text=""Short Description:"" FontSize=""11"" Foreground=""#94A3B8"" Margin=""0,0,0,3""/>
                                <TextBox x:Name=""TxtBuilderDesc"" Height=""28"" Background=""#0B0D11"" Foreground=""#FFFFFF""
                                         BorderBrush=""#252B38"" BorderThickness=""1"" Padding=""6,3"" FontSize=""11"" VerticalContentAlignment=""Center""/>
                            </StackPanel>

                            <!-- Column 1 -->
                            <StackPanel Grid.Column=""2"">
                                <TextBlock Text=""Direct Download URL (Affiliate / CDN URL):"" FontSize=""11"" Foreground=""#94A3B8"" Margin=""0,0,0,3""/>
                                <TextBox x:Name=""TxtBuilderDownloadUrl"" Height=""28"" Background=""#0B0D11"" Foreground=""#FFFFFF""
                                         BorderBrush=""#252B38"" BorderThickness=""1"" Padding=""6,3"" FontSize=""11"" VerticalContentAlignment=""Center"" Margin=""0,0,0,6""/>

                                <TextBlock Text=""Affiliate Web Redirect URL (Browser Signup / Discount):"" FontSize=""11"" Foreground=""#94A3B8"" Margin=""0,0,0,3""/>
                                <TextBox x:Name=""TxtBuilderAffiliateWebUrl"" Height=""28"" Background=""#0B0D11"" Foreground=""#FFFFFF""
                                         BorderBrush=""#252B38"" BorderThickness=""1"" Padding=""6,3"" FontSize=""11"" VerticalContentAlignment=""Center"" Margin=""0,0,0,6""/>

                                <TextBlock Text=""Silent Install Flags (e.g. /VERYSILENT /NORESTART or /S):"" FontSize=""11"" Foreground=""#94A3B8"" Margin=""0,0,0,3""/>
                                <TextBox x:Name=""TxtBuilderSilentArgs"" Height=""28"" Background=""#0B0D11"" Foreground=""#FFFFFF""
                                         BorderBrush=""#252B38"" BorderThickness=""1"" Padding=""6,3"" FontSize=""11"" VerticalContentAlignment=""Center""/>
                            </StackPanel>
                        </Grid>

                        <!-- Form Action Row -->
                        <Grid Margin=""0,4,0,0"">
                            <StackPanel Orientation=""Horizontal"" HorizontalAlignment=""Left"" VerticalAlignment=""Center"">
                                <Button x:Name=""BtnBuilderAddUpdate"" Content=""+ Add / Update App"" Background=""#0066FF"" Foreground=""#FFFFFF""
                                        FontWeight=""Bold"" Padding=""16,6"" Cursor=""Hand"" BorderThickness=""0"" FontSize=""11.5"" Margin=""0,0,8,0""/>
                                <Button x:Name=""BtnBuilderClearForm"" Content=""Clear Form"" Background=""#FFFFFF"" Foreground=""#0F172A"" BorderBrush=""#CBD5E1""
                                        FontWeight=""SemiBold"" Padding=""14,6"" Cursor=""Hand"" BorderThickness=""1"" FontSize=""11""/>
                            </StackPanel>
                            <TextBlock x:Name=""TxtBuilderFormStatus"" Text="""" FontSize=""11"" FontWeight=""SemiBold"" Foreground=""#10B981"" HorizontalAlignment=""Right"" VerticalAlignment=""Center""/>
                        </Grid>
                    </StackPanel>
                </Border>

                <!-- Row 2: Configured Custom Apps List -->
                <Grid Grid.Row=""2"" Margin=""0,0,0,12"">
                    <Grid.RowDefinitions>
                        <RowDefinition Height=""Auto""/>
                        <RowDefinition Height=""*""/>
                    </Grid.RowDefinitions>
                    <TextBlock x:Name=""TxtBuilderCustomListHeader"" Grid.Row=""0"" Text=""CONFIGURED CUSTOM &amp; AFFILIATE APPS (0)"" FontSize=""10.5"" FontWeight=""Bold"" Foreground=""#A855F7"" Margin=""0,0,0,6""/>
                    <Border x:Name=""BorderBuilderListCard"" Grid.Row=""1"" Background=""#11141A"" CornerRadius=""8"" BorderBrush=""#232938"" BorderThickness=""1"" Padding=""6"">
                        <ScrollViewer MaxHeight=""135"" VerticalScrollBarVisibility=""Auto"">
                            <StackPanel x:Name=""PnlBuilderCustomAppsList""/>
                        </ScrollViewer>
                    </Border>
                </Grid>

                <!-- Row 3: Bottom Action Toolbar -->
                <Grid Grid.Row=""3"">
                    <StackPanel Orientation=""Horizontal"" HorizontalAlignment=""Left"" VerticalAlignment=""Center"">
                        <Button x:Name=""BtnBuilderImportJson"" Content=""📂 Import JSON"" Background=""#FFFFFF"" Foreground=""#0F172A"" BorderBrush=""#CBD5E1""
                                FontWeight=""SemiBold"" Padding=""14,6"" Cursor=""Hand"" BorderThickness=""1"" FontSize=""11.5"" Margin=""0,0,8,0""/>
                        <Button x:Name=""BtnBuilderExportJson"" Content=""💾 Export JSON"" Background=""#FFFFFF"" Foreground=""#0F172A"" BorderBrush=""#CBD5E1""
                                FontWeight=""SemiBold"" Padding=""14,6"" Cursor=""Hand"" BorderThickness=""1"" FontSize=""11.5""/>
                    </StackPanel>
                    <StackPanel Orientation=""Horizontal"" HorizontalAlignment=""Right"" VerticalAlignment=""Center"">
                        <Button x:Name=""BtnBuilderCancel"" Content=""Close"" Background=""#FFFFFF"" Foreground=""#0F172A"" BorderBrush=""#CBD5E1""
                                FontWeight=""SemiBold"" Padding=""18,6"" Cursor=""Hand"" BorderThickness=""1"" FontSize=""11.5"" Margin=""0,0,10,0""/>
                        <Button x:Name=""BtnBuilderSaveAndApply"" Content=""💾 Save &amp; Apply Changes"" Background=""#0066FF"" Foreground=""#FFFFFF""
                                FontWeight=""Bold"" Padding=""22,6"" Cursor=""Hand"" BorderThickness=""0"" FontSize=""12""/>
                    </StackPanel>
                </Grid>
            </Grid>
        </Border>
    </Grid>
</Grid>
";

            Grid root = (Grid)XamlReader.Parse(xaml);
            this.Content = root;
            rootGrid = root;
            rootGrid.LayoutTransform = uiScaleTransform;

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

            borderQuickSetupHero = (Border)root.FindName("BorderQuickSetupHero");
            imgHeroMascot = (Image)root.FindName("ImgHeroMascot");
            txtHeroMainTitle = (TextBlock)root.FindName("TxtHeroMainTitle");
            txtHeroMainSubtitle = (TextBlock)root.FindName("TxtHeroMainSubtitle");
            borderWizardStep1 = (Border)root.FindName("BorderWizardStep1");
            borderWizardStep2 = (Border)root.FindName("BorderWizardStep2");
            borderWizardStep3 = (Border)root.FindName("BorderWizardStep3");
            txtWizardStep1Title = (TextBlock)root.FindName("TxtWizardStep1Title");
            txtWizardStep1Desc = (TextBlock)root.FindName("TxtWizardStep1Desc");
            txtWizardStep2Num = (TextBlock)root.FindName("TxtWizardStep2Num");
            txtWizardStep2Title = (TextBlock)root.FindName("TxtWizardStep2Title");
            txtWizardStep2Desc = (TextBlock)root.FindName("TxtWizardStep2Desc");
            txtWizardStep3Num = (TextBlock)root.FindName("TxtWizardStep3Num");
            txtWizardStep3Title = (TextBlock)root.FindName("TxtWizardStep3Title");
            txtWizardStep3Desc = (TextBlock)root.FindName("TxtWizardStep3Desc");
            rectWizardLine1 = (System.Windows.Shapes.Rectangle)root.FindName("RectWizardLine1");
            rectWizardLine2 = (System.Windows.Shapes.Rectangle)root.FindName("RectWizardLine2");
            borderSetupTip = (Border)root.FindName("BorderSetupTip");
            txtSetupTip = (TextBlock)root.FindName("TxtSetupTip");

            if (imgHeroMascot != null)
            {
                imgHeroMascot.Source = LoadEmbeddedImage("hero_mascot.png");
            }

            imgCatalogHero = (Image)root.FindName("ImgCatalogHero");
            if (imgCatalogHero != null)
            {
                imgCatalogHero.Source = LoadEmbeddedImage("hero_catalog.png");
            }

            stackBottomSelectedPackInfo = (StackPanel)root.FindName("StackBottomSelectedPackInfo");
            txtBottomSelectedPackInfo = (TextBlock)root.FindName("TxtBottomSelectedPackInfo");

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
            btnInstall = (Button)root.FindName("BtnInstall");
            btnStop = (Button)root.FindName("BtnStop");

            // Modal Elements
            groupModalOverlay = (Grid)root.FindName("GroupModalOverlay");
            borderModalCard = (Border)root.FindName("BorderModalCard");
            borderModalHeaderBadge = (Border)root.FindName("BorderModalHeaderBadge");
            txtModalTitle = (TextBlock)root.FindName("TxtModalTitle");
            txtModalSubtitle = (TextBlock)root.FindName("TxtModalSubtitle");
            btnCloseModal = (Button)root.FindName("BtnCloseModal");
            borderPackIconCard = (Border)root.FindName("BorderPackIconCard");
            imgPackIconPreview = (Image)root.FindName("ImgPackIconPreview");
            borderPackIconBadgeEdit = (Border)root.FindName("BorderPackIconBadgeEdit");
            txtModalHeaderGlyph = (TextBlock)root.FindName("TxtModalHeaderGlyph");
            txtPackIconPreview = (TextBlock)root.FindName("TxtPackIconPreview");
            borderGroupName = (Border)root.FindName("BorderGroupName");
            txtGroupNameInput = (TextBox)root.FindName("TxtGroupNameInput");
            txtGroupIconInput = (TextBox)root.FindName("TxtGroupIconInput");
            emojiChipsPanel = (WrapPanel)root.FindName("EmojiChipsPanel");
            panelEmojiChips = (StackPanel)root.FindName("PanelEmojiChips");
            btnIconPrev = (Button)root.FindName("BtnIconPrev");
            btnIconNext = (Button)root.FindName("BtnIconNext");
            borderGroupDesc = (Border)root.FindName("BorderGroupDesc");
            txtGroupDescInput = (TextBox)root.FindName("TxtGroupDescInput");
            txtGroupDescCharCount = (TextBlock)root.FindName("TxtGroupDescCharCount");
            borderModalSearchBox = (Border)root.FindName("BorderModalSearchBox");
            txtModalAppSearch = (TextBox)root.FindName("TxtModalAppSearch");
            btnModalFilterTabAll = (Button)root.FindName("BtnModalFilterTabAll");
            btnModalFilterTabSelected = (Button)root.FindName("BtnModalFilterTabSelected");
            borderModalTableHeader = (Border)root.FindName("BorderModalTableHeader");
            chkModalSelectAll = (CheckBox)root.FindName("ChkModalSelectAll");
            txtModalTableHeaderCount = (TextBlock)root.FindName("TxtModalTableHeaderCount");
            badgeModalHeaderRole = (Border)root.FindName("BadgeModalHeaderRole");
            txtModalHeaderBadgeRole = (TextBlock)root.FindName("TxtModalHeaderBadgeRole");
            btnModalSelectAll = (Button)root.FindName("BtnModalSelectAll");
            btnModalClearAll = (Button)root.FindName("BtnModalClearAll");
            txtModalSelectedCount = (TextBlock)root.FindName("TxtModalSelectedCount");
            borderModalAppsContainer = (Border)root.FindName("BorderModalAppsContainer");
            modalAppsScrollViewer = (ScrollViewer)root.FindName("ModalAppsScrollViewer");
            modalAppsContainer = (StackPanel)root.FindName("ModalAppsContainer");
            btnDeleteGroup = (Button)root.FindName("BtnDeleteGroup");
            btnModalExportGroup = (Button)root.FindName("BtnModalExportGroup");
            btnCancelModal = (Button)root.FindName("BtnCancelModal");
            btnSaveGroup = (Button)root.FindName("BtnSaveGroup");

            // Online Search Modal
            modalOnlineSearchOverlay = (Grid)root.FindName("ModalOnlineSearchOverlay");
            borderOnlineSearchCard = (Border)root.FindName("BorderOnlineSearchCard");
            borderOnlineModalIcon = (Border)root.FindName("BorderOnlineModalIcon");
            txtOnlineModalTitle = (TextBlock)root.FindName("TxtOnlineModalTitle");
            txtOnlineModalSubtitle = (TextBlock)root.FindName("TxtOnlineModalSubtitle");
            btnCloseOnlineModal = (Button)root.FindName("BtnCloseOnlineModal");
            borderOnlineSearchInput = (Border)root.FindName("BorderOnlineSearchInput");
            txtOnlineModalSearchInput = (TextBox)root.FindName("TxtOnlineModalSearchInput");
            btnOnlineClearSearch = (Button)root.FindName("BtnOnlineClearSearch");
            btnOnlineModalExecuteSearch = (Button)root.FindName("BtnOnlineModalExecuteSearch");
            txtOnlineSearchBtnText = (TextBlock)root.FindName("TxtOnlineSearchBtnText");
            onlineSearchTagsPanel = (WrapPanel)root.FindName("OnlineSearchTagsPanel");
            borderOnlineModalStatusHeader = (Border)root.FindName("BorderOnlineModalStatusHeader");
            txtOnlineModalStatus = (TextBlock)root.FindName("TxtOnlineModalStatus");
            borderOnlineModalResultsContainer = (Border)root.FindName("BorderOnlineModalResultsContainer");
            onlineModalResultsScrollViewer = (ScrollViewer)root.FindName("OnlineModalResultsScrollViewer");
            onlineModalResultsPanel = (StackPanel)root.FindName("OnlineModalResultsPanel");
            txtOnlineModalSelectedCount = (TextBlock)root.FindName("TxtOnlineModalSelectedCount");
            btnCancelOnlineModal = (Button)root.FindName("BtnCancelOnlineModal");
            btnOnlineAddSelected = (Button)root.FindName("BtnOnlineAddSelected");
            btnOnlineInstallSelected = (Button)root.FindName("BtnOnlineInstallSelected");
            PopulateOnlineSearchSuggestionTags();

            // Brand Logo & Help Menu
            imgBrandLogo = (Image)root.FindName("ImgBrandLogo");
            TextBlock txtFallbackBrand = (TextBlock)root.FindName("TxtFallbackBrand");
            ImageSource brandCube = LoadEmbeddedImage("brand_cube.png");
            if (brandCube != null && imgBrandLogo != null)
            {
                imgBrandLogo.Source = brandCube;
            }
            else if (this.Icon != null && imgBrandLogo != null)
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

                        btnLanguageSelector = (Button)root.FindName("BtnLanguageSelector");
            imgLanguageFlag = (Image)root.FindName("ImgLanguageFlag");
            txtLanguageName = (TextBlock)root.FindName("TxtLanguageName");

            Localization.InitializeTranslations();
            Localization.InitializeFlags();
            activeLanguageCode = (settings != null && !string.IsNullOrEmpty(settings.language)) ? settings.language : "auto";
            if (activeLanguageCode.Equals("auto", StringComparison.OrdinalIgnoreCase))
            {
                Localization.CurrentLanguage = Localization.DetectSystemLanguage();
            }
            else
            {
                Localization.CurrentLanguage = activeLanguageCode;
            }

            if (btnLanguageSelector != null)
            {
                btnLanguageSelector.Click += delegate
                {
                    ContextMenu menu = new ContextMenu();
                    menu.Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#1E222A");
                    menu.BorderBrush = currentThemeIsLight ? HexBrush("#CBD5E1") : HexBrush("#333C4D");
                    menu.Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#FFFFFF");
                    menu.FontSize = 12;

                    // Item 0: Auto (System Default)
                    MenuItem itmAuto = new MenuItem();
                    bool isAuto = activeLanguageCode.Equals("auto", StringComparison.OrdinalIgnoreCase);
                    string detected = Localization.DetectSystemLanguage();
                    LanguageDefinition detectedLang = Localization.GetLanguage(detected);

                    StackPanel autoStack = new StackPanel() { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
                    TextBlock autoCheck = new TextBlock()
                    {
                        Text = isAuto ? "\u2713 " : "   ",
                        FontWeight = FontWeights.Bold,
                        Foreground = isAuto ? HexBrush("#0066FF") : Brushes.Transparent,
                        Width = 18,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    autoStack.Children.Add(autoCheck);

                    Border autoIconBorder = new Border()
                    {
                        Width = 20,
                        Height = 14,
                        CornerRadius = new CornerRadius(2),
                        Margin = new Thickness(0, 0, 8, 0),
                        VerticalAlignment = VerticalAlignment.Center,
                        ClipToBounds = true
                    };
                    Image autoIcon = new Image() { Source = Localization.GetFlagBitmap("globe"), Stretch = Stretch.Uniform };
                    autoIconBorder.Child = autoIcon;
                    autoStack.Children.Add(autoIconBorder);

                    TextBlock autoLabel = new TextBlock()
                    {
                        Text = string.Format("{0} ({1} {2})", Localization.T("lang_auto"), detectedLang != null ? detectedLang.NativeName : "", detectedLang != null ? detectedLang.Name : ""),
                        FontWeight = isAuto ? FontWeights.Bold : FontWeights.Normal,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    autoStack.Children.Add(autoLabel);

                    itmAuto.Header = autoStack;
                    itmAuto.Click += delegate
                    {
                        SetApplicationLanguage("auto");
                    };
                    menu.Items.Add(itmAuto);
                    menu.Items.Add(new Separator());

                    // All 16 Supported Languages with Real High-Res Country Flags
                    foreach (LanguageDefinition langDef in Localization.SupportedLanguages)
                    {
                        LanguageDefinition curL = langDef;
                        MenuItem itmLang = new MenuItem();
                        bool isCurrent = !activeLanguageCode.Equals("auto", StringComparison.OrdinalIgnoreCase) &&
                                         activeLanguageCode.Equals(curL.Code, StringComparison.OrdinalIgnoreCase);

                        StackPanel itmStack = new StackPanel() { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };

                        TextBlock chkTxt = new TextBlock()
                        {
                            Text = isCurrent ? "\u2713 " : "   ",
                            FontWeight = FontWeights.Bold,
                            Foreground = isCurrent ? HexBrush("#0066FF") : Brushes.Transparent,
                            Width = 18,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        itmStack.Children.Add(chkTxt);

                        Border flagBrd = new Border()
                        {
                            Width = 20,
                            Height = 14,
                            CornerRadius = new CornerRadius(2),
                            BorderThickness = new Thickness(1),
                            BorderBrush = currentThemeIsLight ? HexBrush("#CBD5E1") : HexBrush("#475569"),
                            Margin = new Thickness(0, 0, 8, 0),
                            VerticalAlignment = VerticalAlignment.Center,
                            ClipToBounds = true
                        };
                        Image flagImg = new Image()
                        {
                            Source = Localization.GetFlagBitmap(curL.Code),
                            Stretch = Stretch.UniformToFill
                        };
                        flagBrd.Child = flagImg;
                        itmStack.Children.Add(flagBrd);

                        TextBlock lblTxt = new TextBlock()
                        {
                            Text = string.Format("{0} ({1})", curL.NativeName, curL.Name),
                            FontWeight = isCurrent ? FontWeights.Bold : FontWeights.Normal,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        itmStack.Children.Add(lblTxt);

                        itmLang.Header = itmStack;
                        itmLang.Click += delegate
                        {
                            SetApplicationLanguage(curL.Code);
                        };
                        menu.Items.Add(itmLang);
                    }

                    menu.PlacementTarget = btnLanguageSelector;
                    menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                    menu.IsOpen = true;
                };
            }

            UpdateLanguageButtonDisplay();

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

                    MenuItem itmTutorial = new MenuItem() { Header = Localization.T("menu_guide"), FontWeight = FontWeights.SemiBold };
                    MenuItem itmUpdates = new MenuItem() { Header = isUpdateAvailable ? (Localization.T("menu_updates") + " (New Version Available!)...") : Localization.T("menu_updates") };
                    MenuItem itmSettings = new MenuItem() { Header = Localization.T("menu_settings") };
                    MenuItem itmAbout = new MenuItem() { Header = Localization.T("menu_about") };

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

            borderSettingsSecDisplay = (Border)root.FindName("BorderSettingsSecDisplay");
            txtSettingsSecDisplayTitle = (TextBlock)root.FindName("TxtSettingsSecDisplayTitle");
            rbScaleAuto = (RadioButton)root.FindName("RbScaleAuto");
            rbScale100 = (RadioButton)root.FindName("RbScale100");
            rbScale90 = (RadioButton)root.FindName("RbScale90");
            rbScale80 = (RadioButton)root.FindName("RbScale80");
            txtCurrentScaleActive = (TextBlock)root.FindName("TxtCurrentScaleActive");

            if (rbScaleAuto != null) rbScaleAuto.Checked += delegate { displayScaleSetting = "Auto"; UpdateAutoResolutionScale(); };
            if (rbScale100 != null) rbScale100.Checked += delegate { displayScaleSetting = "100%"; UpdateAutoResolutionScale(); };
            if (rbScale90 != null) rbScale90.Checked += delegate { displayScaleSetting = "90%"; UpdateAutoResolutionScale(); };
            if (rbScale80 != null) rbScale80.Checked += delegate { displayScaleSetting = "80%"; UpdateAutoResolutionScale(); };

            borderSettingsSec2 = (Border)root.FindName("BorderSettingsSec2");
            txtSettingsSec2Title = (TextBlock)root.FindName("TxtSettingsSec2Title");
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
            if (btnClearCache != null)
            {
                btnClearCache.Click += (s, e) =>
                {
                    try
                    {
                        string downloadDir = Path.Combine(Path.GetTempPath(), "FleetForgeDownloads");
                        if (Directory.Exists(downloadDir)) Directory.Delete(downloadDir, true);
                        if (txtCacheStatus != null) txtCacheStatus.Text = "Cache: Cleaned successfully";
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
                        if (chkSettingAutoTheme != null && chkSettingAutoTheme.IsChecked == true)
                        {
                            settings.theme = "system";
                        }
                        else
                        {
                            settings.theme = currentThemeIsLight ? "light" : "dark";
                        }
                        if (rbScale100 != null && rbScale100.IsChecked == true) displayScaleSetting = "100%";
                        else if (rbScale90 != null && rbScale90.IsChecked == true) displayScaleSetting = "90%";
                        else if (rbScale80 != null && rbScale80.IsChecked == true) displayScaleSetting = "80%";
                        else displayScaleSetting = "Auto";
                        settings.displayScale = displayScaleSetting;
                        SaveSettings();
                        UpdateAutoResolutionScale();
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
                txtWingetVersion.Text = "Engine: Standalone Cloud";
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

            // Modal: Custom App & Affiliate Builder
            modalCustomAppBuilderOverlay = (Grid)root.FindName("ModalCustomAppBuilderOverlay");
            borderCustomAppBuilderCard = (Border)root.FindName("BorderCustomAppBuilderCard");
            txtModalAppBuilderTitle = (TextBlock)root.FindName("TxtModalAppBuilderTitle");
            borderBuilderFormCard = (Border)root.FindName("BorderBuilderFormCard");
            borderBuilderListCard = (Border)root.FindName("BorderBuilderListCard");
            txtBuilderSearchCatalog = (TextBox)root.FindName("TxtBuilderSearchCatalog");
            btnBuilderSearchFind = (Button)root.FindName("BtnBuilderSearchFind");
            lstBuilderSearchResults = (ListBox)root.FindName("LstBuilderSearchResults");
            txtBuilderAppId = (TextBox)root.FindName("TxtBuilderAppId");
            txtBuilderAppName = (TextBox)root.FindName("TxtBuilderAppName");
            cmbBuilderCategory = (ComboBox)root.FindName("CmbBuilderCategory");
            txtBuilderDownloadUrl = (TextBox)root.FindName("TxtBuilderDownloadUrl");
            txtBuilderAffiliateWebUrl = (TextBox)root.FindName("TxtBuilderAffiliateWebUrl");
            txtBuilderSilentArgs = (TextBox)root.FindName("TxtBuilderSilentArgs");
            txtBuilderDesc = (TextBox)root.FindName("TxtBuilderDesc");
            btnBuilderAddUpdate = (Button)root.FindName("BtnBuilderAddUpdate");
            btnBuilderClearForm = (Button)root.FindName("BtnBuilderClearForm");
            txtBuilderFormStatus = (TextBlock)root.FindName("TxtBuilderFormStatus");
            txtBuilderCustomListHeader = (TextBlock)root.FindName("TxtBuilderCustomListHeader");
            pnlBuilderCustomAppsList = (StackPanel)root.FindName("PnlBuilderCustomAppsList");
            btnBuilderImportJson = (Button)root.FindName("BtnBuilderImportJson");
            btnBuilderExportJson = (Button)root.FindName("BtnBuilderExportJson");
            btnBuilderCancel = (Button)root.FindName("BtnBuilderCancel");
            btnBuilderSaveAndApply = (Button)root.FindName("BtnBuilderSaveAndApply");
            btnCloseAppBuilder = (Button)root.FindName("BtnCloseAppBuilder");

            if (btnCloseAppBuilder != null) btnCloseAppBuilder.Click += (s, e) => CloseCustomAppBuilderModal();
            if (btnBuilderCancel != null) btnBuilderCancel.Click += (s, e) => CloseCustomAppBuilderModal();
            if (modalCustomAppBuilderOverlay != null)
            {
                modalCustomAppBuilderOverlay.MouseDown += (s, e) =>
                {
                    if (e.OriginalSource == modalCustomAppBuilderOverlay) CloseCustomAppBuilderModal();
                };
            }
            if (btnBuilderSearchFind != null) btnBuilderSearchFind.Click += (s, e) => OnBuilderCatalogSearch();
            if (txtBuilderSearchCatalog != null)
            {
                txtBuilderSearchCatalog.KeyDown += (s, e) =>
                {
                    if (e.Key == Key.Enter) OnBuilderCatalogSearch();
                };
            }
            if (lstBuilderSearchResults != null)
            {
                lstBuilderSearchResults.SelectionChanged += (s, e) => OnBuilderSearchResultSelected();
            }
            if (btnBuilderAddUpdate != null) btnBuilderAddUpdate.Click += (s, e) => OnBuilderAddUpdate();
            if (btnBuilderClearForm != null) btnBuilderClearForm.Click += (s, e) => ClearBuilderForm();
            if (btnBuilderSaveAndApply != null) btnBuilderSaveAndApply.Click += (s, e) => OnBuilderSaveAndApply();
            if (btnBuilderImportJson != null) btnBuilderImportJson.Click += (s, e) => OnBuilderImportJson();
            if (btnBuilderExportJson != null) btnBuilderExportJson.Click += (s, e) => OnBuilderExportJson();

            // =========================================================================
            // SECRET COMPANY & DEVELOPER ACCESS (HIDDEN FROM REGULAR USERS)
            // =========================================================================
            // 1. Keyboard Shortcut: Press Ctrl + Shift + B anywhere in FleetForge
            this.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    if (modalOnlineSearchOverlay != null && modalOnlineSearchOverlay.Visibility == Visibility.Visible)
                    {
                        CloseOnlineSearchModal();
                        e.Handled = true;
                        return;
                    }
                    if (groupModalOverlay != null && groupModalOverlay.Visibility == Visibility.Visible)
                    {
                        CloseGroupModal();
                        e.Handled = true;
                        return;
                    }
                }
                if ((Keyboard.Modifiers & ModifierKeys.Control) != 0 && (Keyboard.Modifiers & ModifierKeys.Shift) != 0 && e.Key == Key.B)
                {
                    ShowCustomAppBuilderModal();
                    e.Handled = true;
                }
            };

            // 2. Secret Click on Developer Signature: "Design and Develop by LOVE MALHOTRA"
            if (stackSidebarFooter != null)
            {
                stackSidebarFooter.Cursor = Cursors.Hand;
                stackSidebarFooter.MouseDown += (s, e) =>
                {
                    ShowCustomAppBuilderModal();
                };
            }

            // 3. Command Line Argument: FleetForge.exe --builder (or -builder, /builder)
            try
            {
                string[] cmdArgs = Environment.GetCommandLineArgs();
                if (cmdArgs != null)
                {
                    for (int i = 0; i < cmdArgs.Length; i++)
                    {
                        if (string.Equals(cmdArgs[i], "--builder", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(cmdArgs[i], "-builder", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(cmdArgs[i], "/builder", StringComparison.OrdinalIgnoreCase))
                        {
                            this.Loaded += (ls, le) => ShowCustomAppBuilderModal();
                            break;
                        }
                    }
                }
            }
            catch { }

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

                    MenuItem itmSelectedTop = new MenuItem() { Header = (catalogSortMode == "selected_top" ? "\u2713  " : "    ") + "\u2B50 " + Localization.T("sort_selected_top") };
                    MenuItem itmAlphaAsc = new MenuItem() { Header = (catalogSortMode == "alpha_asc" ? "\u2713  " : "    ") + "\U0001F524 " + Localization.T("sort_name_az") };
                    MenuItem itmAlphaDesc = new MenuItem() { Header = (catalogSortMode == "alpha_desc" ? "\u2713  " : "    ") + "\U0001F524 " + Localization.T("sort_name_za") };
                    MenuItem itmPopular = new MenuItem() { Header = (catalogSortMode == "popular" ? "\u2713  " : "    ") + "\U0001F525 " + Localization.T("sort_selected_top") };
                    MenuItem itmCategory = new MenuItem() { Header = (catalogSortMode == "category" ? "\u2713  " : "    ") + "\U0001F3F7\uFE0F " + Localization.T("sort_category") };

                    itmSelectedTop.Click += delegate { SetCatalogSort("selected_top", "Selected & Recommended"); };
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
                if (IsSearchPlaceholder(txtSearch.Text))
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
                    txtSearch.Text = Localization.T("search_placeholder");
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
                bool hasText = !string.IsNullOrEmpty(t) && !IsSearchPlaceholder(t);
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
                    if (hasText)
                    {
                        searchDebounceTimer.Start();
                    }
                }
                else if (hasText)
                {
                    ApplyFilters();
                }
            };
            txtSearch.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Enter)
                {
                    if (searchDebounceTimer != null) searchDebounceTimer.Stop();
                    string t = txtSearch.Text;
                    bool hasText = !string.IsNullOrEmpty(t) && t != "Find an app, setting or pack..." && t != "Search applications...";
                    if (hasText && currentView != "catalog")
                    {
                        SwitchView("catalog");
                    }
                    ApplyFilters();
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

            // Online Search Modal Events
            if (modalOnlineSearchOverlay != null)
            {
                modalOnlineSearchOverlay.MouseDown += delegate(object s, MouseButtonEventArgs e)
                {
                    if (e.OriginalSource == modalOnlineSearchOverlay) CloseOnlineSearchModal();
                };
            }
            if (btnCloseOnlineModal != null) btnCloseOnlineModal.Click += delegate { CloseOnlineSearchModal(); };
            if (btnCancelOnlineModal != null) btnCancelOnlineModal.Click += delegate { CloseOnlineSearchModal(); };
            if (btnOnlineClearSearch != null)
            {
                btnOnlineClearSearch.Click += delegate
                {
                    if (txtOnlineModalSearchInput != null)
                    {
                        txtOnlineModalSearchInput.Text = "";
                        txtOnlineModalSearchInput.Focus();
                    }
                    btnOnlineClearSearch.Visibility = Visibility.Collapsed;
                    ShowOnlineSearchEmptyState();
                };
            }
            if (txtOnlineModalSearchInput != null)
            {
                txtOnlineModalSearchInput.GotFocus += delegate
                {
                    if (txtOnlineModalSearchInput.Text == "Type software name (e.g. Discord, Blender, Spotify, Zoom, Steam)...")
                    {
                        txtOnlineModalSearchInput.Text = "";
                        txtOnlineModalSearchInput.Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
                    }
                };
                txtOnlineModalSearchInput.LostFocus += delegate
                {
                    if (string.IsNullOrWhiteSpace(txtOnlineModalSearchInput.Text))
                    {
                        txtOnlineModalSearchInput.Text = "Type software name (e.g. Discord, Blender, Spotify, Zoom, Steam)...";
                        txtOnlineModalSearchInput.Foreground = HexBrush("#94A3B8");
                        if (btnOnlineClearSearch != null) btnOnlineClearSearch.Visibility = Visibility.Collapsed;
                    }
                };
                txtOnlineModalSearchInput.TextChanged += delegate
                {
                    string t = txtOnlineModalSearchInput.Text;
                    bool hasText = !string.IsNullOrWhiteSpace(t) && t != "Type software name (e.g. Discord, Blender, Spotify, Zoom, Steam)...";
                    if (btnOnlineClearSearch != null)
                    {
                        btnOnlineClearSearch.Visibility = hasText ? Visibility.Visible : Visibility.Collapsed;
                    }
                };
                txtOnlineModalSearchInput.KeyDown += delegate(object s, KeyEventArgs e)
                {
                    if (e.Key == Key.Enter)
                    {
                        string q = txtOnlineModalSearchInput.Text.Trim();
                        if (q == "Type software name (e.g. Discord, Blender, Spotify, Zoom, Steam)...") q = "";
                        RunWingetOnlineSearchModal(q);
                        e.Handled = true;
                    }
                };
            }
            if (btnOnlineModalExecuteSearch != null)
            {
                btnOnlineModalExecuteSearch.Click += delegate
                {
                    string q = txtOnlineModalSearchInput != null ? txtOnlineModalSearchInput.Text.Trim() : "";
                    if (q == "Type software name (e.g. Discord, Blender, Spotify, Zoom, Steam)...") q = "";
                    RunWingetOnlineSearchModal(q);
                };
            }
            if (btnOnlineAddSelected != null)
            {
                btnOnlineAddSelected.Click += delegate { AddSelectedOnlineAppsToCatalog(); };
            }
            if (btnOnlineInstallSelected != null)
            {
                btnOnlineInstallSelected.Click += delegate { InstallSelectedOnlineApps(); };
            }

            // Log events

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
            btnStop.Click += delegate
            {
                string contentStr = (btnStop.Content != null) ? btnStop.Content.ToString() : "";
                if (contentStr.IndexOf("Finish", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    string targetView = !string.IsNullOrEmpty(previousViewBeforeInstall) ? previousViewBeforeInstall : "setup";
                    SwitchView(targetView);

                    btnStop.Content = "⏹️ Stop Install";
                    btnStop.Background = HexBrush("#DC2626");
                    btnStop.Visibility = Visibility.Collapsed;
                    UpdateBottomBarVisibility();
                }
                else
                {
                    RequestStopInstallation();
                }
            };

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

            if (borderPackIconCard != null)
            {
                borderPackIconCard.MouseLeftButtonUp += delegate(object s, MouseButtonEventArgs e)
                {
                    CycleQuickIcon(1);
                };
            }
            if (btnIconPrev != null)
            {
                btnIconPrev.Click += delegate { CycleQuickIcon(-1); };
            }
            if (btnIconNext != null)
            {
                btnIconNext.Click += delegate { CycleQuickIcon(1); };
            }

            if (txtGroupDescInput != null)
            {
                txtGroupDescInput.TextChanged += delegate { UpdateDescCharCount(); };
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

            if (chkModalSelectAll != null)
            {
                chkModalSelectAll.Click += delegate
                {
                    bool shouldSelectAll = chkModalSelectAll.IsChecked == true;
                    if (modalFilteredApps != null)
                    {
                        foreach (AppModel a in modalFilteredApps)
                        {
                            if (a != null && a.id != null)
                            {
                                if (shouldSelectAll) modalSelectedAppIds.Add(a.id);
                                else modalSelectedAppIds.Remove(a.id);
                            }
                        }
                    }
                    foreach (ModalAppItemView m in modalAppRows)
                    {
                        m.CheckBox.IsChecked = shouldSelectAll;
                    }
                    UpdateModalCount();
                };
            }

            if (btnModalSelectAll != null)
            {
                btnModalSelectAll.Click += delegate
                {
                    if (chkModalSelectAll != null) chkModalSelectAll.IsChecked = true;
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
                    }
                    UpdateModalCount();
                };
            }

            if (btnModalClearAll != null)
            {
                btnModalClearAll.Click += delegate
                {
                    modalSelectedAppIds.Clear();
                    if (chkModalSelectAll != null) chkModalSelectAll.IsChecked = false;
                    foreach (ModalAppItemView m in modalAppRows)
                    {
                        m.CheckBox.IsChecked = false;
                    }
                    UpdateModalCount();
                };
            }

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

            this.Loaded += delegate { UpdateAutoResolutionScale(); };
            this.SizeChanged += delegate { UpdateAutoResolutionScale(); };
        }

        // =========================================================================
        // THEME MANAGEMENT & WINDOWS THEME DETECTION
        // =========================================================================

        // =========================================================================
        // AUTOMATIC RESOLUTION & UI SCALING ENGINE
        // =========================================================================
        private void UpdateAutoResolutionScale()
        {
            try
            {
                if (uiScaleTransform == null || rootGrid == null) return;

                // Dynamically adjust Quick Setup Role Cards column layout on narrow widths
                if (roleCardsGrid != null)
                {
                    double currentActualW = this.ActualWidth > 0 ? this.ActualWidth : this.Width;
                    roleCardsGrid.Columns = currentActualW < 840 ? 1 : 2;
                }

                if (displayScaleSetting == "100%")
                {
                    uiScaleTransform.ScaleX = 1.0;
                    uiScaleTransform.ScaleY = 1.0;
                    if (txtCurrentScaleActive != null) txtCurrentScaleActive.Text = "Active: 100%";
                    return;
                }
                if (displayScaleSetting == "90%")
                {
                    uiScaleTransform.ScaleX = 0.90;
                    uiScaleTransform.ScaleY = 0.90;
                    if (txtCurrentScaleActive != null) txtCurrentScaleActive.Text = "Active: 90%";
                    return;
                }
                if (displayScaleSetting == "80%")
                {
                    uiScaleTransform.ScaleX = 0.80;
                    uiScaleTransform.ScaleY = 0.80;
                    if (txtCurrentScaleActive != null) txtCurrentScaleActive.Text = "Active: 80%";
                    return;
                }

                // Auto Mode: dynamically calculate optimal layout scale based on window / screen bounds
                double w = this.ActualWidth > 0 ? this.ActualWidth : this.Width;
                double h = this.ActualHeight > 0 ? this.ActualHeight : this.Height;

                if (w <= 0 || h <= 0) return;

                const double refW = 1180.0;
                const double refH = 850.0;

                double scaleW = w / refW;
                double scaleH = h / refH;

                // Scale uniformly based on the most constrained dimension (typically vertical height on laptops)
                double targetScale = Math.Min(scaleW, scaleH);

                // Cap max scale to 1.0 on large displays (keep crisp 100% standard layout)
                if (targetScale > 1.0) targetScale = 1.0;

                // Minimum safe scale 0.72 so text remains easily readable even in small viewports
                if (targetScale < 0.72) targetScale = 0.72;

                uiScaleTransform.ScaleX = targetScale;
                uiScaleTransform.ScaleY = targetScale;

                if (txtCurrentScaleActive != null)
                {
                    txtCurrentScaleActive.Text = string.Format("Active: {0:P0}", targetScale);
                }
            }
            catch { }
        }

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
                    try { DisposeTrayAndToast(); } catch { }
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
                            txtUpdateDetail.Text = "FleetForge v2.7.0 (Enterprise IT Edition • Love Malhotra) is up to date.";
                        }
                        if (btnDownloadInstall != null)
                        {
                            btnDownloadInstall.Visibility = Visibility.Collapsed;
                        }

                        // User prompt when no update found
                        if (info != null)
                        {
                            MessageBox.Show(
                                "You are running the latest version of FleetForge (v2.7.0).\n\nNo newer updates were found.",
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

                try
                {
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | (SecurityProtocolType)12288 | (SecurityProtocolType)768 | SecurityProtocolType.Tls;
                }
                catch { }

                int currentVersionNum = ParseVersionNumber(CurrentAppVersion);
                int highestVersionNum = currentVersionNum;
                UpdateInfo bestCandidate = null;

                // Priority 1: GitHub Raw (Direct origin, instant updates, never stale)
                // Priority 2: Configured UpdateCheckUrl
                // Priority 3: GitHub REST API
                // Priority 4: CDNs as fallbacks (with cache-buster)
                string[] remoteEndpoints = new string[]
                {
                    "https://raw.githubusercontent.com/loveonce/FleetForge/main/data/version.json",
                    AppUrls.UpdateCheckUrl,
                    "https://api.github.com/repos/loveonce/FleetForge/releases/latest",
                    "https://cdn.jsdelivr.net/gh/loveonce/FleetForge@main/data/version.json",
                    "https://raw.githack.com/loveonce/FleetForge/main/data/version.json",
                    "https://cdn.statically.io/gh/loveonce/FleetForge/main/data/version.json"
                };

                var ser = new JavaScriptSerializer();

                foreach (string rawEndpoint in remoteEndpoints)
                {
                    if (string.IsNullOrEmpty(rawEndpoint)) continue;
                    try
                    {
                        using (WebClient wc = new WebClient())
                        {
                            wc.Encoding = Encoding.UTF8;
                            wc.Headers.Add("User-Agent", "FleetForge/" + CurrentAppVersion);
                            wc.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
                            wc.Headers.Add("Pragma", "no-cache");
                            wc.Headers.Add("Expires", "0");

                            // Append cache buster to prevent local WinINet / intermediate proxy caching
                            string queryUrl = rawEndpoint;
                            if (!queryUrl.Contains("api.github.com"))
                            {
                                string sep = queryUrl.Contains("?") ? "&" : "?";
                                queryUrl = queryUrl + sep + "_cb=" + DateTime.UtcNow.Ticks;
                            }

                            string json = wc.DownloadString(queryUrl);
                            if (string.IsNullOrEmpty(json)) continue;

                            UpdateInfo parsed = null;
                            if (rawEndpoint.Contains("api.github.com"))
                            {
                                try
                                {
                                    var dict = ser.Deserialize<Dictionary<string, object>>(json);
                                    if (dict != null && dict.ContainsKey("tag_name") && dict["tag_name"] != null)
                                    {
                                        string tag = dict["tag_name"].ToString().Trim().TrimStart('v', 'V');
                                        if (!string.IsNullOrEmpty(tag))
                                        {
                                            parsed = new UpdateInfo();
                                            parsed.version = tag;
                                            parsed.versionCode = ParseVersionNumber(tag) / 100;
                                            parsed.title = dict.ContainsKey("name") && dict["name"] != null ? dict["name"].ToString() : ("FleetForge v" + tag);
                                            parsed.downloadUrl = "https://raw.githubusercontent.com/loveonce/FleetForge/main/FleetForge-Setup.exe";
                                        }
                                    }
                                }
                                catch { }
                            }
                            else
                            {
                                parsed = ser.Deserialize<UpdateInfo>(json);
                            }

                            if (parsed != null && !string.IsNullOrEmpty(parsed.version))
                            {
                                int candidateVerNum = ParseVersionNumber(parsed.version);
                                if (candidateVerNum > highestVersionNum || (parsed.versionCode > 0 && parsed.versionCode > CurrentAppVersionCode))
                                {
                                    highestVersionNum = candidateVerNum;
                                    bestCandidate = parsed;
                                    updateFound = true;
                                    break; // Found a confirmed newer release!
                                }
                                else if (bestCandidate == null)
                                {
                                    bestCandidate = parsed;
                                }
                            }
                        }
                    }
                    catch { }
                }

                info = bestCandidate;

                // 2. If remote not accessible, check local version.json for testing
                if (info == null)
                {
                    try
                    {
                        string vPath = Path.Combine(appDir, "data", "version.json");
                        if (!File.Exists(vPath)) vPath = Path.Combine(appDir, "version.json");
                        if (File.Exists(vPath))
                        {
                            string json = File.ReadAllText(vPath, Encoding.UTF8);
                            info = ser.Deserialize<UpdateInfo>(json);
                            if (info != null)
                            {
                                int candidateVerNum = ParseVersionNumber(info.version);
                                if (candidateVerNum > currentVersionNum || (info.versionCode > 0 && info.versionCode > CurrentAppVersionCode))
                                {
                                    updateFound = true;
                                }
                            }
                        }
                    }
                    catch { }
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
                        wc.Headers.Add("User-Agent", "FleetForge-Updater/2.7.0");
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
                                            try
                                            {
                                                ProcessStartInfo psi = new ProcessStartInfo(localSetup);
                                                psi.Arguments = string.Format("/update /silent /dir \"{0}\"", appDir);
                                                psi.UseShellExecute = true;
                                                psi.Verb = "runas";
                                                Process.Start(psi);
                                                Application.Current.Shutdown();
                                            }
                                            catch (Exception ex)
                                            {
                                                MessageBox.Show("Could not launch updater: " + ex.Message, "Launch Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                            }
                                        }
                                    }
                                    return;
                                }

                                if (txtDownloadStatus != null) txtDownloadStatus.Text = "Download complete! Starting background update in system tray...";
                                if (pbUpdateDownload != null) pbUpdateDownload.Value = 100;
                                Thread.Sleep(500);

                                try
                                {
                                    ProcessStartInfo psi = new ProcessStartInfo(tempInstaller);
                                    psi.Arguments = string.Format("/update /silent /dir \"{0}\"", appDir);
                                    psi.UseShellExecute = true;
                                    psi.Verb = "runas";
                                    Process.Start(psi);
                                    Application.Current.Shutdown();
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Could not launch updater: " + ex.Message, "Launch Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    string curScale = string.IsNullOrEmpty(settings.displayScale) ? "Auto" : settings.displayScale;
                    if (rbScaleAuto != null) rbScaleAuto.IsChecked = (curScale == "Auto");
                    if (rbScale100 != null) rbScale100.IsChecked = (curScale == "100%");
                    if (rbScale90 != null) rbScale90.IsChecked = (curScale == "90%");
                    if (rbScale80 != null) rbScale80.IsChecked = (curScale == "80%");
                    if (txtCurrentScaleActive != null && uiScaleTransform != null)
                    {
                        txtCurrentScaleActive.Text = string.Format("Active: {0:P0}", uiScaleTransform.ScaleX);
                    }
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
            this.Background = isLight ? HexBrush("#F8FAFC") : HexBrush("#0F172A");
            this.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
            if (rootGrid != null) rootGrid.Background = isLight ? HexBrush("#F8FAFC") : HexBrush("#0F172A");

            // Sidebar
            if (borderSidebar != null)
            {
                borderSidebar.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#1E293B");
                borderSidebar.BorderBrush = isLight ? HexBrush("#E2E8F0") : HexBrush("#334155");
            }
            if (txtBrandTitle != null) txtBrandTitle.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#FFFFFF");
            if (txtBrandSubtitle != null) txtBrandSubtitle.Foreground = isLight ? HexBrush("#64748B") : HexBrush("#94A3B8");
            if (txtToggleSidebarIcon != null) txtToggleSidebarIcon.Foreground = isLight ? HexBrush("#64748B") : HexBrush("#94A3B8");

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
                txtSidebarFooterBy.Foreground = isLight ? HexBrush("#94A3B8") : HexBrush("#64748B");
            }
            if (txtSidebarFooterCompany != null)
            {
                txtSidebarFooterCompany.Foreground = HexBrush("#5f6d84");
            }

            // Top Bar
            if (borderTopBar != null)
            {
                borderTopBar.Background = isLight ? HexBrush("#F8FAFC") : HexBrush("#0F172A");
                borderTopBar.BorderBrush = isLight ? HexBrush("#E2E8F0") : HexBrush("#334155");
            }
            if (txtPageTitle != null) txtPageTitle.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#FFFFFF");
            if (txtPageSubtitle != null) txtPageSubtitle.Foreground = isLight ? HexBrush("#64748B") : HexBrush("#A0A0A0");

            // Windows 11 Big Headings across views
            if (txtPageBigHeading != null) txtPageBigHeading.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#FFFFFF");
            if (txtPageBigSubtitle != null) txtPageBigSubtitle.Foreground = isLight ? HexBrush("#64748B") : HexBrush("#A0A0A0");
            if (txtCatalogBigHeading != null) txtCatalogBigHeading.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#FFFFFF");
            if (txtCatalogBigSubtitle != null) txtCatalogBigSubtitle.Foreground = isLight ? HexBrush("#64748B") : HexBrush("#A0A0A0");
            if (txtCloudBigHeading != null) txtCloudBigHeading.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#FFFFFF");
            if (txtCloudBigSubtitle != null) txtCloudBigSubtitle.Foreground = isLight ? HexBrush("#64748B") : HexBrush("#A0A0A0");
            if (txtLogsBigHeading != null) txtLogsBigHeading.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#FFFFFF");
            if (txtLogsBigSubtitle != null) txtLogsBigSubtitle.Foreground = isLight ? HexBrush("#64748B") : HexBrush("#A0A0A0");

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
                txtToggleSidebarIcon.Foreground = isLight ? HexBrush("#64748B") : HexBrush("#94A3B8");
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
            if (txtHeroTitle != null) txtHeroTitle.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#FFFFFF");
            if (txtHeroDesc != null) txtHeroDesc.Foreground = isLight ? HexBrush("#64748B") : HexBrush("#A0A0A0");

            // Unified Quick Setup Hero Card
            if (borderQuickSetupHero != null)
            {
                borderQuickSetupHero.BorderBrush = isLight ? HexBrush("#D5E4F7") : HexBrush("#263B59");
                LinearGradientBrush grad = new LinearGradientBrush();
                grad.StartPoint = new Point(0, 0);
                grad.EndPoint = new Point(1, 0);
                if (isLight)
                {
                    grad.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#E6F0FC"), 0.0));
                    grad.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#EEF5FE"), 0.38));
                    grad.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#F7FAFE"), 0.75));
                    grad.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FFFFFF"), 1.0));
                }
                else
                {
                    grad.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#142136"), 0.0));
                    grad.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#1A283F"), 1.0));
                }
                borderQuickSetupHero.Background = grad;
            }
            if (txtHeroMainTitle != null) txtHeroMainTitle.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
            if (txtHeroMainSubtitle != null) txtHeroMainSubtitle.Foreground = isLight ? HexBrush("#64748B") : HexBrush("#94A3B8");
            if (txtWizardStep1Title != null) txtWizardStep1Title.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
            if (txtWizardStep1Desc != null) txtWizardStep1Desc.Foreground = isLight ? HexBrush("#64748B") : HexBrush("#94A3B8");
            if (txtWizardStep2Title != null) txtWizardStep2Title.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
            if (txtWizardStep2Desc != null) txtWizardStep2Desc.Foreground = isLight ? HexBrush("#64748B") : HexBrush("#94A3B8");
            if (txtWizardStep3Title != null) txtWizardStep3Title.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
            if (txtWizardStep3Desc != null) txtWizardStep3Desc.Foreground = isLight ? HexBrush("#64748B") : HexBrush("#94A3B8");
            if (borderWizardStep2 != null)
            {
                borderWizardStep2.Background = isLight ? HexBrush("#EDF4FF") : HexBrush("#1E293B");
                borderWizardStep2.BorderBrush = isLight ? HexBrush("#CBD5E1") : HexBrush("#334155");
            }
            if (borderWizardStep3 != null)
            {
                borderWizardStep3.Background = isLight ? HexBrush("#EDF4FF") : HexBrush("#1E293B");
                borderWizardStep3.BorderBrush = isLight ? HexBrush("#CBD5E1") : HexBrush("#334155");
            }
            if (rectWizardLine1 != null) rectWizardLine1.Fill = isLight ? HexBrush("#CBD5E1") : HexBrush("#334155");
            if (rectWizardLine2 != null) rectWizardLine2.Fill = isLight ? HexBrush("#CBD5E1") : HexBrush("#334155");

            // Section Header
            if (txtTargetRoleTitle != null) txtTargetRoleTitle.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
            if (txtTargetRoleSubtitle != null) txtTargetRoleSubtitle.Foreground = isLight ? HexBrush("#64748B") : HexBrush("#94A3B8");
            if (btnImportPack != null)
            {
                btnImportPack.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2A3547");
                btnImportPack.BorderBrush = isLight ? HexBrush("#CBD5E1") : HexBrush("#475569");
                btnImportPack.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
            }
            if (btnCreateNewGroup != null)
            {
                btnCreateNewGroup.Background = HexBrush("#0066FF");
                btnCreateNewGroup.Foreground = HexBrush("#FFFFFF");
            }

            // Header Search Box
            if (borderHeaderSearch != null)
            {
                borderHeaderSearch.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#1E293B");
                borderHeaderSearch.BorderBrush = isLight ? HexBrush("#CBD5E1") : HexBrush("#334155");
                borderHeaderSearch.BorderThickness = new Thickness(1);
            }
            if (txtHeaderSearchIcon != null)
            {
                txtHeaderSearchIcon.Foreground = isLight ? HexBrush("#64748B") : HexBrush("#94A3B8");
            }
            if (txtSearch != null)
            {
                bool isPlaceholder = (txtSearch.Text == "Find an app, setting or pack..." || txtSearch.Text == "Search applications...");
                if (isPlaceholder)
                {
                    txtSearch.Foreground = isLight ? HexBrush("#94A3B8") : HexBrush("#64748B");
                }
                else
                {
                    txtSearch.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#FFFFFF");
                }
                txtSearch.CaretBrush = isLight ? HexBrush("#0066FF") : HexBrush("#38BDF8");
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
                borderCatalogContainer.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#1E293B");
                borderCatalogContainer.BorderBrush = isLight ? HexBrush("#E2E8F0") : HexBrush("#334155");
            }
            if (borderCatalogHeaderBar != null)
            {
                borderCatalogHeaderBar.BorderBrush = isLight ? HexBrush("#F1F5F9") : HexBrush("#334155");
            }
            if (chkSelectAllCatalog != null)
            {
                chkSelectAllCatalog.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
            }
            if (txtCatalogListHeaderCount != null)
            {
                txtCatalogListHeaderCount.Foreground = isLight ? HexBrush("#64748B") : HexBrush("#94A3B8");
            }
            if (btnCatalogSort != null)
            {
                btnCatalogSort.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2A3547");
                btnCatalogSort.BorderBrush = isLight ? HexBrush("#CBD5E1") : HexBrush("#475569");
                btnCatalogSort.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
            }
            if (txtBottomSelectedPackInfo != null)
            {
                txtBottomSelectedPackInfo.Foreground = isLight ? HexBrush("#475569") : HexBrush("#94A3B8");
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
            if (btnViewReport != null)
            {
                btnViewReport.Background = HexBrush("#0066FF");
                btnViewReport.Foreground = HexBrush("#FFFFFF");
            }
            if (btnOpenLogFile != null)
            {
                btnOpenLogFile.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2A3547");
                btnOpenLogFile.BorderBrush = isLight ? HexBrush("#CBD5E1") : HexBrush("#475569");
                btnOpenLogFile.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
            }
            if (btnClearLogConsole != null)
            {
                btnClearLogConsole.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2A3547");
                btnClearLogConsole.BorderBrush = isLight ? HexBrush("#CBD5E1") : HexBrush("#475569");
                btnClearLogConsole.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
            }

            // Bottom Bar
            if (borderBottomBar != null)
            {
                borderBottomBar.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#1E293B");
                borderBottomBar.BorderBrush = isLight ? HexBrush("#E2E8F0") : HexBrush("#334155");
            }
            if (borderSetupTip != null)
            {
                borderSetupTip.Background = isLight ? HexBrush("#EFF6FF") : HexBrush("#172554");
                borderSetupTip.BorderBrush = isLight ? HexBrush("#BFDBFE") : HexBrush("#1E40AF");
            }
            if (txtSetupTip != null)
            {
                txtSetupTip.Foreground = isLight ? HexBrush("#1D4ED8") : HexBrush("#93C5FD");
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
            if (btnInstall != null)
            {
                btnInstall.Background = HexBrush("#0066FF");
                btnInstall.Foreground = HexBrush("#FFFFFF");
            }
            if (btnCreateNewGroup != null)
            {
                btnCreateNewGroup.Background = HexBrush("#0066FF");
                btnCreateNewGroup.Foreground = HexBrush("#FFFFFF");
            }
            if (btnImportPack != null)
            {
                btnImportPack.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2A3547");
                btnImportPack.BorderBrush = isLight ? HexBrush("#CBD5E1") : HexBrush("#475569");
                btnImportPack.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
            }

            // Modal Card & Controls
            if (borderModalCard != null)
            {
                borderModalCard.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#1E293B");
                borderModalCard.BorderBrush = isLight ? HexBrush("#E2E8F0") : HexBrush("#334155");
            }
            if (txtModalTitle != null) txtModalTitle.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
            if (txtModalSubtitle != null) txtModalSubtitle.Foreground = isLight ? HexBrush("#64748B") : HexBrush("#94A3B8");
            if (borderModalAppsContainer != null)
            {
                borderModalAppsContainer.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#0F172A");
                borderModalAppsContainer.BorderBrush = isLight ? HexBrush("#E2E8F0") : HexBrush("#334155");
            }
            if (borderOnlineSearchCard != null)
            {
                borderOnlineSearchCard.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#1E293B");
                borderOnlineSearchCard.BorderBrush = isLight ? HexBrush("#E2E8F0") : HexBrush("#334155");
            }
            if (txtOnlineModalTitle != null) txtOnlineModalTitle.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
            if (txtOnlineModalSubtitle != null) txtOnlineModalSubtitle.Foreground = isLight ? HexBrush("#64748B") : HexBrush("#94A3B8");
            if (borderOnlineSearchInput != null)
            {
                borderOnlineSearchInput.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#0F172A");
                borderOnlineSearchInput.BorderBrush = isLight ? HexBrush("#CBD5E1") : HexBrush("#475569");
            }
            if (txtOnlineModalSearchInput != null && txtOnlineModalSearchInput.Text != "Type software name (e.g. Discord, Blender, Spotify, Zoom, Steam)...")
            {
                txtOnlineModalSearchInput.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
            }
            if (borderOnlineModalStatusHeader != null)
            {
                borderOnlineModalStatusHeader.Background = isLight ? HexBrush("#F8FAFC") : HexBrush("#1E293B");
                borderOnlineModalStatusHeader.BorderBrush = isLight ? HexBrush("#E2E8F0") : HexBrush("#334155");
            }
            if (borderOnlineModalResultsContainer != null)
            {
                borderOnlineModalResultsContainer.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#0F172A");
                borderOnlineModalResultsContainer.BorderBrush = isLight ? HexBrush("#E2E8F0") : HexBrush("#334155");
            }
            if (borderModalTableHeader != null)
            {
                borderModalTableHeader.BorderBrush = isLight ? HexBrush("#F1F5F9") : HexBrush("#334155");
            }
            if (chkModalSelectAll != null)
            {
                chkModalSelectAll.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
            }
            if (txtModalTableHeaderCount != null)
            {
                txtModalTableHeaderCount.Foreground = isLight ? HexBrush("#64748B") : HexBrush("#94A3B8");
            }
            if (borderPackIconCard != null)
            {
                borderPackIconCard.Background = isLight ? HexBrush("#F8FAFC") : HexBrush("#243144");
                borderPackIconCard.BorderBrush = isLight ? HexBrush("#E2E8F0") : HexBrush("#334155");
            }
            if (borderGroupName != null)
            {
                borderGroupName.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#243144");
                borderGroupName.BorderBrush = isLight ? HexBrush("#E2E8F0") : HexBrush("#334155");
            }
            if (txtGroupNameInput != null)
            {
                txtGroupNameInput.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
            }
            if (borderGroupDesc != null)
            {
                borderGroupDesc.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#243144");
                borderGroupDesc.BorderBrush = isLight ? HexBrush("#E2E8F0") : HexBrush("#334155");
            }
            if (txtGroupDescInput != null)
            {
                txtGroupDescInput.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
            }
            if (borderModalSearchBox != null)
            {
                borderModalSearchBox.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#243144");
                borderModalSearchBox.BorderBrush = isLight ? HexBrush("#CBD5E1") : HexBrush("#334155");
            }
            if (txtModalAppSearch != null)
            {
                txtModalAppSearch.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
            }
            if (chkModalSelectAll != null)
            {
                chkModalSelectAll.Foreground = isLight ? HexBrush("#334155") : HexBrush("#F8FAFC");
            }
            if (btnModalClearAll != null)
            {
                btnModalClearAll.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#243144");
                btnModalClearAll.BorderBrush = isLight ? HexBrush("#CBD5E1") : HexBrush("#475569");
                btnModalClearAll.Foreground = isLight ? HexBrush("#334155") : HexBrush("#F8FAFC");
            }
            if (btnModalLoadMore != null)
            {
                btnModalLoadMore.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2A3547");
                btnModalLoadMore.BorderBrush = isLight ? HexBrush("#CBD5E1") : HexBrush("#475569");
                btnModalLoadMore.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
            }
            if (btnCancelModal != null)
            {
                btnCancelModal.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#243144");
                btnCancelModal.BorderBrush = isLight ? HexBrush("#CBD5E1") : HexBrush("#475569");
                btnCancelModal.Foreground = isLight ? HexBrush("#334155") : HexBrush("#F8FAFC");
            }
            if (btnModalExportGroup != null)
            {
                btnModalExportGroup.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#243144");
                btnModalExportGroup.BorderBrush = isLight ? HexBrush("#CBD5E1") : HexBrush("#475569");
                btnModalExportGroup.Foreground = isLight ? HexBrush("#334155") : HexBrush("#F8FAFC");
            }
            if (btnSaveGroup != null)
            {
                btnSaveGroup.Background = HexBrush("#0066FF");
                btnSaveGroup.Foreground = HexBrush("#FFFFFF");
            }
            if (btnDeleteGroup != null)
            {
                btnDeleteGroup.Background = isLight ? HexBrush("#FEF2F2") : HexBrush("#450A0A");
                btnDeleteGroup.BorderBrush = isLight ? HexBrush("#FCA5A5") : HexBrush("#991B1B");
                btnDeleteGroup.Foreground = isLight ? HexBrush("#DC2626") : HexBrush("#F87171");
            }
            UpdateModalTabStyles();

            // Help Button
            if (btnHelpMenu != null)
            {
                btnHelpMenu.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2A3547");
                btnHelpMenu.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
                btnHelpMenu.BorderBrush = isLight ? HexBrush("#CBD5E1") : HexBrush("#475569");
            }
            if (btnLanguageSelector != null)
            {
                btnLanguageSelector.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2A3547");
                btnLanguageSelector.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
                btnLanguageSelector.BorderBrush = isLight ? HexBrush("#CBD5E1") : HexBrush("#475569");
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
                btnCheckUpdatesAgain.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2A3547");
                btnCheckUpdatesAgain.BorderBrush = isLight ? HexBrush("#CBD5E1") : HexBrush("#475569");
                btnCheckUpdatesAgain.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
            }
            if (btnDownloadInstall != null)
            {
                btnDownloadInstall.Background = HexBrush("#0066FF");
                btnDownloadInstall.Foreground = HexBrush("#FFFFFF");
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
            if (btnClearCache != null)
            {
                btnClearCache.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2A3547");
                btnClearCache.BorderBrush = isLight ? HexBrush("#CBD5E1") : HexBrush("#475569");
                btnClearCache.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
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
                btnExportCatalog.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2A3547");
                btnExportCatalog.BorderBrush = isLight ? HexBrush("#CBD5E1") : HexBrush("#475569");
                btnExportCatalog.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
            }
            if (borderCustomAppBuilderCard != null)
            {
                borderCustomAppBuilderCard.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#181B22");
                borderCustomAppBuilderCard.BorderBrush = isLight ? HexBrush("#CBD5E1") : HexBrush("#2D3342");
            }
            if (txtModalAppBuilderTitle != null)
            {
                txtModalAppBuilderTitle.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#FFFFFF");
            }
            if (borderBuilderFormCard != null)
            {
                borderBuilderFormCard.Background = isLight ? HexBrush("#F8FAFC") : HexBrush("#11141A");
                borderBuilderFormCard.BorderBrush = isLight ? HexBrush("#E2E8F0") : HexBrush("#232938");
            }
            if (borderBuilderListCard != null)
            {
                borderBuilderListCard.Background = isLight ? HexBrush("#F8FAFC") : HexBrush("#11141A");
                borderBuilderListCard.BorderBrush = isLight ? HexBrush("#E2E8F0") : HexBrush("#232938");
            }
            Brush inputBg = isLight ? HexBrush("#FFFFFF") : HexBrush("#0B0D11");
            Brush inputBorder = isLight ? HexBrush("#CBD5E1") : HexBrush("#252B38");
            Brush inputFg = isLight ? HexBrush("#0F172A") : HexBrush("#FFFFFF");
            if (txtBuilderSearchCatalog != null) { txtBuilderSearchCatalog.Background = inputBg; txtBuilderSearchCatalog.BorderBrush = inputBorder; txtBuilderSearchCatalog.Foreground = inputFg; }
            if (txtBuilderAppId != null) { txtBuilderAppId.Background = inputBg; txtBuilderAppId.BorderBrush = inputBorder; txtBuilderAppId.Foreground = inputFg; }
            if (txtBuilderAppName != null) { txtBuilderAppName.Background = inputBg; txtBuilderAppName.BorderBrush = inputBorder; txtBuilderAppName.Foreground = inputFg; }
            if (txtBuilderDownloadUrl != null) { txtBuilderDownloadUrl.Background = inputBg; txtBuilderDownloadUrl.BorderBrush = inputBorder; txtBuilderDownloadUrl.Foreground = inputFg; }
            if (txtBuilderAffiliateWebUrl != null) { txtBuilderAffiliateWebUrl.Background = inputBg; txtBuilderAffiliateWebUrl.BorderBrush = inputBorder; txtBuilderAffiliateWebUrl.Foreground = inputFg; }
            if (txtBuilderSilentArgs != null) { txtBuilderSilentArgs.Background = inputBg; txtBuilderSilentArgs.BorderBrush = inputBorder; txtBuilderSilentArgs.Foreground = inputFg; }
            if (txtBuilderDesc != null) { txtBuilderDesc.Background = inputBg; txtBuilderDesc.BorderBrush = inputBorder; txtBuilderDesc.Foreground = inputFg; }
            if (cmbBuilderCategory != null) { cmbBuilderCategory.Background = inputBg; cmbBuilderCategory.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#FFFFFF"); }

            if (borderSettingsSecDisplay != null)
            {
                borderSettingsSecDisplay.Background = isLight ? HexBrush("#F8FAFC") : HexBrush("#11141A");
                borderSettingsSecDisplay.BorderBrush = isLight ? HexBrush("#E2E8F0") : HexBrush("#232938");
            }
            if (txtSettingsSecDisplayTitle != null) txtSettingsSecDisplayTitle.Foreground = isLight ? HexBrush("#0284C7") : HexBrush("#38BDF8");
            if (rbScaleAuto != null) rbScaleAuto.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#FFFFFF");
            if (rbScale100 != null) rbScale100.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#FFFFFF");
            if (rbScale90 != null) rbScale90.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#FFFFFF");
            if (rbScale80 != null) rbScale80.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#FFFFFF");

            if (btnCancelSettings != null)
            {
                btnCancelSettings.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2A3547");
                btnCancelSettings.BorderBrush = isLight ? HexBrush("#CBD5E1") : HexBrush("#475569");
                btnCancelSettings.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
            }
            if (btnSaveSettings != null)
            {
                btnSaveSettings.Background = HexBrush("#0066FF");
                btnSaveSettings.Foreground = HexBrush("#FFFFFF");
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
            if (btnLanguageSelector != null)
            {
                btnLanguageSelector.Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2B2B2B");
                btnLanguageSelector.BorderBrush = isLight ? HexBrush("#D1D5DB") : HexBrush("#404040");
                btnLanguageSelector.Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#FFFFFF");
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

            UpdateBottomBarVisibility();
            UpdateDwmTheme(isLight);
        }

        private void BuildEmojiChips()
        {
            if (emojiChipsPanel == null) return;
            emojiChipsPanel.Children.Clear();
            string curSelected = (txtGroupIconInput != null && !string.IsNullOrEmpty(txtGroupIconInput.Text))
                ? txtGroupIconInput.Text.Trim()
                : "🎨";

            foreach (string emoji in quickIcons)
            {
                string curEmoji = emoji;
                bool isCur = string.Equals(curEmoji, curSelected, StringComparison.OrdinalIgnoreCase);

                Button b = new Button()
                {
                    Content = curEmoji,
                    FontFamily = (curEmoji == "</>") ? new FontFamily("Consolas, Segoe UI Semibold, Segoe UI") : new FontFamily("Segoe UI Emoji, Segoe UI Symbol, Segoe UI"),
                    FontSize = (curEmoji == "</>") ? 12 : 14,
                    FontWeight = (curEmoji == "</>") ? FontWeights.Bold : FontWeights.Normal,
                    Foreground = isCur ? HexBrush("#0066FF") : (currentThemeIsLight ? HexBrush("#334155") : HexBrush("#F8FAFC")),
                    Width = 44,
                    Height = 38,
                    Margin = new Thickness(2, 0, 2, 0),
                    Background = isCur ? (currentThemeIsLight ? HexBrush("#EFF6FF") : HexBrush("#1E3A8A")) : (currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#243144")),
                    BorderBrush = isCur ? HexBrush("#0066FF") : (currentThemeIsLight ? HexBrush("#E2E8F0") : HexBrush("#334155")),
                    BorderThickness = new Thickness(isCur ? 2 : 1),
                    Cursor = Cursors.Hand,
                    Tag = curEmoji
                };
                b.Click += delegate
                {
                    SelectQuickIcon(curEmoji);
                };
                emojiChipsPanel.Children.Add(b);
            }
        }

        private void SelectQuickIcon(string icon)
        {
            if (imgPackIconPreview != null) imgPackIconPreview.Visibility = Visibility.Collapsed;
            if (txtPackIconPreview != null)
            {
                txtPackIconPreview.Visibility = Visibility.Visible;
                txtPackIconPreview.Text = icon;
            }
            if (txtGroupIconInput != null) txtGroupIconInput.Text = icon;
            if (emojiChipsPanel != null)
            {
                foreach (UIElement child in emojiChipsPanel.Children)
                {
                    Button b = child as Button;
                    if (b != null && b.Tag is string)
                    {
                        bool isCur = string.Equals((string)b.Tag, icon, StringComparison.OrdinalIgnoreCase);
                        b.BorderThickness = new Thickness(isCur ? 2 : 1);
                        b.BorderBrush = isCur ? HexBrush("#0066FF") : (currentThemeIsLight ? HexBrush("#E2E8F0") : HexBrush("#334155"));
                        b.Background = isCur ? (currentThemeIsLight ? HexBrush("#EFF6FF") : HexBrush("#1E3A8A")) : (currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#243144"));
                        b.Foreground = isCur ? HexBrush("#0066FF") : (currentThemeIsLight ? HexBrush("#334155") : HexBrush("#F8FAFC"));
                    }
                }
            }
        }

        private void CycleQuickIcon(int delta)
        {
            if (quickIcons == null || quickIcons.Length == 0) return;
            string cur = (txtGroupIconInput != null && !string.IsNullOrEmpty(txtGroupIconInput.Text))
                ? txtGroupIconInput.Text.Trim()
                : quickIcons[0];

            int idx = Array.IndexOf(quickIcons, cur);
            if (idx < 0) idx = 0;
            int newIdx = (idx + delta) % quickIcons.Length;
            if (newIdx < 0) newIdx += quickIcons.Length;
            SelectQuickIcon(quickIcons[newIdx]);
        }

        private void UpdateDescCharCount()
        {
            if (txtGroupDescCharCount != null && txtGroupDescInput != null)
            {
                int len = (txtGroupDescInput.Text ?? "").Length;
                txtGroupDescCharCount.Text = len + " / 200";
            }
        }

        private void AttachNavHover(Button btn, string viewName)
        {
            if (btn == null) return;
            btn.MouseEnter += delegate
            {
                if (currentView != viewName)
                {
                    btn.Background = currentThemeIsLight ? HexBrush("#F1F5F9") : HexBrush("#243144");
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
            if (btn == null || btn.Visibility == Visibility.Collapsed) return;
            if (isActive)
            {
                btn.Background = currentThemeIsLight ? HexBrush("#EDF5FF") : HexBrush("#1E3A8A");
                btn.BorderBrush = Brushes.Transparent;
                btn.BorderThickness = new Thickness(0);
                if (ind != null)
                {
                    ind.Visibility = Visibility.Collapsed;
                }
                if (icon != null)
                {
                    icon.Foreground = currentThemeIsLight ? HexBrush("#0066FF") : HexBrush("#60A5FA");
                }
                if (txt != null)
                {
                    txt.Foreground = currentThemeIsLight ? HexBrush("#0066FF") : HexBrush("#60A5FA");
                    txt.FontWeight = FontWeights.SemiBold;
                }
            }
            else
            {
                btn.Background = Brushes.Transparent;
                btn.BorderBrush = Brushes.Transparent;
                btn.BorderThickness = new Thickness(0);
                if (ind != null) ind.Visibility = Visibility.Collapsed;
                if (icon != null)
                {
                    icon.Foreground = currentThemeIsLight ? HexBrush("#475569") : HexBrush("#94A3B8");
                }
                if (txt != null)
                {
                    txt.Foreground = currentThemeIsLight ? HexBrush("#475569") : HexBrush("#94A3B8");
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
                if (borderSetupTip != null) borderSetupTip.Visibility = Visibility.Visible;
                if (txtPageTitle != null) txtPageTitle.Text = "Quick setup";
                if (txtPageSubtitle != null) txtPageSubtitle.Text = "Choose an app pack or select individual apps to install on your computer in one click.";
            }
            else
            {
                if (borderSetupTip != null) borderSetupTip.Visibility = Visibility.Collapsed;
                if (viewName == "catalog")
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
            }

            UpdateBottomBarVisibility();
        }

        private void UpdateBottomBarVisibility()
        {
            bool isSetupView = (currentView == "setup" || currentView == "roles");
            bool isCatalogView = (currentView == "catalog");
            bool isLogsView = (currentView == "logs");
            bool isCloudView = (currentView == "cloud");
            bool isInstalling = (btnStop != null && btnStop.Visibility == Visibility.Visible);

            if (btnImportPack != null)
            {
                btnImportPack.Visibility = isSetupView ? Visibility.Visible : Visibility.Collapsed;
            }
            if (btnCreateNewGroup != null)
            {
                btnCreateNewGroup.Visibility = isSetupView ? Visibility.Visible : Visibility.Collapsed;
            }

            if (stackBottomSelectedPackInfo != null)
            {
                stackBottomSelectedPackInfo.Visibility = isCatalogView ? Visibility.Visible : Visibility.Collapsed;
                if (isCatalogView && txtBottomSelectedPackInfo != null)
                {
                    GroupModel activeGroup = null;
                    if (config != null && config.groups != null && !string.IsNullOrEmpty(activeRoleId))
                    {
                        activeGroup = config.groups.Find(delegate(GroupModel g) { return g != null && g.id == activeRoleId; });
                    }
                    string rawName = (activeGroup != null && !string.IsNullOrWhiteSpace(activeGroup.name)) ? activeGroup.name : "Top Choice • Recommended";
                    txtBottomSelectedPackInfo.Text = "App pack selected: " + FixMojibake(rawName);
                    if (txtStatus != null && txtStatus.Text != null && txtStatus.Text.StartsWith("App pack selected:"))
                    {
                        txtStatus.Text = "";
                    }
                }
            }

            if (btnInstall != null)
            {
                btnInstall.Visibility = (isLogsView || isCloudView || isInstalling || isSetupView) ? Visibility.Collapsed : Visibility.Visible;
            }

            if (borderBottomBar != null)
            {
                if (isLogsView || isCloudView)
                {
                    borderBottomBar.Visibility = isInstalling ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    borderBottomBar.Visibility = Visibility.Visible;
                }
            }

            UpdateCount();
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
            if (!string.IsNullOrEmpty(settings.displayScale))
            {
                displayScaleSetting = settings.displayScale;
                UpdateAutoResolutionScale();
            }
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

        public static string ConvertDirectUrl(string rawUrl)
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
            EnsureCatalogHierarchy();

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

        private RecommendedDataModel LoadRecommendedDataFromDiskOrResource()
        {
            string json = null;
            try
            {
                if (!string.IsNullOrEmpty(recommendedAppsPath) && File.Exists(recommendedAppsPath))
                {
                    json = File.ReadAllText(recommendedAppsPath, Encoding.UTF8);
                }
            }
            catch { }

            if (string.IsNullOrEmpty(json))
            {
                try
                {
                    Assembly asm = Assembly.GetExecutingAssembly();
                    string[] resNames = asm.GetManifestResourceNames();
                    foreach (string name in resNames)
                    {
                        if (name.EndsWith("recommended_apps.json", StringComparison.OrdinalIgnoreCase))
                        {
                            using (Stream s = asm.GetManifestResourceStream(name))
                            {
                                if (s != null)
                                {
                                    using (StreamReader r = new StreamReader(s, Encoding.UTF8))
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

            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var ser = new JavaScriptSerializer();
                    ser.MaxJsonLength = int.MaxValue;
                    return ser.Deserialize<RecommendedDataModel>(json);
                }
                catch { }
            }

            var fallback = new RecommendedDataModel();
            fallback.group = new GroupModel()
            {
                id = "recommended",
                name = "Top Choice • Recommended",
                icon = "⭐",
                description = "Curated partner software, essential premium tools, and top-rated recommendations.",
                accentColor = "#F59E0B",
                apps = new List<string>() { "isolve.iSecureVPN", "bitguardian.Winriser", "BitGuardian.BitDriverUpdater" }
            };
            return fallback;
        }

        private CustomCatalogModel LoadCustomDataFromDiskOrResource()
        {
            string json = null;
            try
            {
                if (!string.IsNullOrEmpty(customAppsPath) && File.Exists(customAppsPath))
                {
                    json = File.ReadAllText(customAppsPath, Encoding.UTF8);
                }
            }
            catch { }

            if (string.IsNullOrEmpty(json))
            {
                try
                {
                    Assembly asm = Assembly.GetExecutingAssembly();
                    string[] resNames = asm.GetManifestResourceNames();
                    foreach (string name in resNames)
                    {
                        if (name.EndsWith("custom_apps.json", StringComparison.OrdinalIgnoreCase))
                        {
                            using (Stream s = asm.GetManifestResourceStream(name))
                            {
                                if (s != null)
                                {
                                    using (StreamReader r = new StreamReader(s, Encoding.UTF8))
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

            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var ser = new JavaScriptSerializer();
                    ser.MaxJsonLength = int.MaxValue;
                    return ser.Deserialize<CustomCatalogModel>(json);
                }
                catch { }
            }

            return new CustomCatalogModel();
        }

        private void EnsureCatalogHierarchy()
        {
            if (config == null) config = new ConfigModel();
            if (config.groups == null) config.groups = new List<GroupModel>();
            if (config.apps == null) config.apps = new List<AppModel>();

            if (baseCatalogApps == null || baseCatalogApps.Count == 0)
            {
                baseCatalogApps = new List<AppModel>(config.apps);
            }

            // 1. Load Recommended Apps
            RecommendedDataModel recModel = LoadRecommendedDataFromDiskOrResource();
            recommendedMasterAppIds.Clear();
            if (recModel != null && recModel.group != null)
            {
                GroupModel rg = recModel.group;
                rg.id = "recommended";
                if (string.IsNullOrEmpty(rg.name)) rg.name = "Top Choice • Recommended";
                if (string.IsNullOrEmpty(rg.icon)) rg.icon = "⭐";
                if (string.IsNullOrEmpty(rg.description)) rg.description = "Curated partner software, essential premium tools, and top-rated recommendations.";
                if (string.IsNullOrEmpty(rg.accentColor)) rg.accentColor = "#F59E0B";

                if (rg.apps != null)
                {
                    recommendedMasterAppIds = new List<string>(rg.apps);
                }

                config.groups.RemoveAll(delegate (GroupModel g) {
                    return g != null && string.Equals(g.id, "recommended", StringComparison.OrdinalIgnoreCase);
                });
                config.groups.Insert(0, rg);
            }

            // 2. Load Custom Apps
            CustomCatalogModel customModel = LoadCustomDataFromDiskOrResource();
            customMasterAppIds.Clear();
            if (customModel != null && customModel.apps != null)
            {
                foreach (AppModel ca in customModel.apps)
                {
                    if (ca != null && !string.IsNullOrEmpty(ca.id) && !customMasterAppIds.Contains(ca.id))
                    {
                        customMasterAppIds.Add(ca.id);
                    }
                }
            }

            // 3. Strict 3-Tier Merge with Dual-Key Deduplication (Priority: Recommended -> Custom -> Base apps.json)
            HashSet<string> seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<AppModel> mergedApps = new List<AppModel>();

            if (recModel != null && recModel.apps != null)
            {
                foreach (AppModel ra in recModel.apps)
                {
                    if (ra == null || string.IsNullOrEmpty(ra.id)) continue;
                    if (!string.IsNullOrEmpty(ra.category)) ra.category = NormalizeCategoryId(ra.category);
                    string nameKey = !string.IsNullOrEmpty(ra.name) ? ra.name.Trim().ToLowerInvariant() : null;
                    if (!seenIds.Contains(ra.id) && (nameKey == null || !seenNames.Contains(nameKey)))
                    {
                        seenIds.Add(ra.id);
                        if (nameKey != null) seenNames.Add(nameKey);
                        mergedApps.Add(ra);
                    }
                }
            }

            if (customModel != null && customModel.apps != null)
            {
                foreach (AppModel ca in customModel.apps)
                {
                    if (ca == null || string.IsNullOrEmpty(ca.id)) continue;
                    if (!string.IsNullOrEmpty(ca.category)) ca.category = NormalizeCategoryId(ca.category);
                    string nameKey = !string.IsNullOrEmpty(ca.name) ? ca.name.Trim().ToLowerInvariant() : null;
                    if (!seenIds.Contains(ca.id) && (nameKey == null || !seenNames.Contains(nameKey)))
                    {
                        seenIds.Add(ca.id);
                        if (nameKey != null) seenNames.Add(nameKey);
                        mergedApps.Add(ca);
                    }
                    else
                    {
                        AppModel existing = mergedApps.Find(delegate(AppModel x) { 
                            return string.Equals(x.id, ca.id, StringComparison.OrdinalIgnoreCase) ||
                                   (nameKey != null && x.name != null && string.Equals(x.name.Trim(), ca.name.Trim(), StringComparison.OrdinalIgnoreCase)); 
                        });
                        if (existing != null)
                        {
                            if (!string.IsNullOrEmpty(ca.downloadUrl)) existing.downloadUrl = ca.downloadUrl;
                            if (!string.IsNullOrEmpty(ca.silentArgs)) existing.silentArgs = ca.silentArgs;
                            if (!string.IsNullOrEmpty(ca.affiliateWebUrl)) existing.affiliateWebUrl = ca.affiliateWebUrl;
                        }
                    }
                }
            }

            if (baseCatalogApps != null)
            {
                foreach (AppModel ba in baseCatalogApps)
                {
                    if (ba == null || string.IsNullOrEmpty(ba.id)) continue;
                    string nameKey = !string.IsNullOrEmpty(ba.name) ? ba.name.Trim().ToLowerInvariant() : null;
                    if (!seenIds.Contains(ba.id) && (nameKey == null || !seenNames.Contains(nameKey)))
                    {
                        seenIds.Add(ba.id);
                        if (nameKey != null) seenNames.Add(nameKey);
                        mergedApps.Add(ba);
                    }
                }
            }

            config.apps = mergedApps;
            EnsureAppDefaultIndices(true);
        }

        private bool IsRecommendedApp(AppModel a)
        {
            if (a == null || string.IsNullOrEmpty(a.id)) return false;
            if (recommendedMasterAppIds != null && recommendedMasterAppIds.Exists(delegate(string x) { return string.Equals(x, a.id, StringComparison.OrdinalIgnoreCase); })) return true;
            if (config != null && config.groups != null && config.groups.Count > 0)
            {
                GroupModel g0 = config.groups[0];
                if (string.Equals(g0.id, "recommended", StringComparison.OrdinalIgnoreCase) && g0.apps != null && g0.apps.Exists(delegate(string x) { return string.Equals(x, a.id, StringComparison.OrdinalIgnoreCase); }))
                    return true;
            }
            return false;
        }

        private bool IsCustomApp(AppModel a)
        {
            if (a == null || string.IsNullOrEmpty(a.id)) return false;
            if (customMasterAppIds != null && customMasterAppIds.Exists(delegate(string x) { return string.Equals(x, a.id, StringComparison.OrdinalIgnoreCase); })) return true;
            return false;
        }

        // =========================================================================
        // CUSTOM APP & AFFILIATE BUILDER MODAL LOGIC
        // =========================================================================
        private void ShowCustomAppBuilderModal()
        {
            if (modalCustomAppBuilderOverlay == null) return;

            if (cmbBuilderCategory != null && cmbBuilderCategory.Items.Count == 0)
            {
                cmbBuilderCategory.Items.Add("browsers");
                cmbBuilderCategory.Items.Add("development");
                cmbBuilderCategory.Items.Add("utilities");
                cmbBuilderCategory.Items.Add("productivity");
                cmbBuilderCategory.Items.Add("communication");
                cmbBuilderCategory.Items.Add("media");
                cmbBuilderCategory.Items.Add("security");
                cmbBuilderCategory.Items.Add("cloud");
                cmbBuilderCategory.Items.Add("gaming");
                cmbBuilderCategory.SelectedIndex = 2;
            }

            CustomCatalogModel ccm = LoadCustomDataFromDiskOrResource();
            builderCustomApps = (ccm != null && ccm.apps != null) ? new List<AppModel>(ccm.apps) : new List<AppModel>();

            ClearBuilderForm();
            if (txtBuilderSearchCatalog != null) txtBuilderSearchCatalog.Text = "";
            if (lstBuilderSearchResults != null)
            {
                lstBuilderSearchResults.ItemsSource = null;
                lstBuilderSearchResults.Visibility = Visibility.Collapsed;
            }

            RenderBuilderCustomAppsList();
            modalCustomAppBuilderOverlay.Visibility = Visibility.Visible;
        }

        private void CloseCustomAppBuilderModal()
        {
            if (modalCustomAppBuilderOverlay != null)
            {
                modalCustomAppBuilderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void ClearBuilderForm()
        {
            if (txtBuilderAppId != null) txtBuilderAppId.Text = "";
            if (txtBuilderAppName != null) txtBuilderAppName.Text = "";
            if (txtBuilderDownloadUrl != null) txtBuilderDownloadUrl.Text = "";
            if (txtBuilderAffiliateWebUrl != null) txtBuilderAffiliateWebUrl.Text = "";
            if (txtBuilderSilentArgs != null) txtBuilderSilentArgs.Text = "";
            if (txtBuilderDesc != null) txtBuilderDesc.Text = "";
            if (cmbBuilderCategory != null && cmbBuilderCategory.Items.Count > 0) cmbBuilderCategory.SelectedIndex = 2;
            builderEditingApp = null;
            if (btnBuilderAddUpdate != null) btnBuilderAddUpdate.Content = "+ Add / Update App";
            if (txtBuilderFormStatus != null) txtBuilderFormStatus.Text = "";
        }

        private void OnBuilderCatalogSearch()
        {
            if (txtBuilderSearchCatalog == null || lstBuilderSearchResults == null) return;
            string query = txtBuilderSearchCatalog.Text.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(query))
            {
                lstBuilderSearchResults.ItemsSource = null;
                lstBuilderSearchResults.Visibility = Visibility.Collapsed;
                return;
            }

            List<AppModel> pool = baseCatalogApps != null ? baseCatalogApps : (config != null ? config.apps : null);
            if (pool == null) return;

            List<string> displayItems = new List<string>();
            List<AppModel> matchApps = new List<AppModel>();

            foreach (AppModel a in pool)
            {
                if (a == null) continue;
                string aName = (a.name ?? "").ToLowerInvariant();
                string aId = (a.id ?? "").ToLowerInvariant();
                if (aName.Contains(query) || aId.Contains(query))
                {
                    matchApps.Add(a);
                    displayItems.Add(string.Format("{0}  [{1}]  ({2})", a.name, a.id, a.category ?? "utilities"));
                    if (matchApps.Count >= 20) break;
                }
            }

            if (matchApps.Count > 0)
            {
                lstBuilderSearchResults.Tag = matchApps;
                lstBuilderSearchResults.ItemsSource = displayItems;
                lstBuilderSearchResults.Visibility = Visibility.Visible;
            }
            else
            {
                lstBuilderSearchResults.ItemsSource = null;
                lstBuilderSearchResults.Visibility = Visibility.Collapsed;
                if (txtBuilderFormStatus != null) txtBuilderFormStatus.Text = "No apps found matching '" + query + "'";
            }
        }

        private void OnBuilderSearchResultSelected()
        {
            if (lstBuilderSearchResults == null || lstBuilderSearchResults.SelectedIndex < 0) return;
            List<AppModel> matchApps = lstBuilderSearchResults.Tag as List<AppModel>;
            if (matchApps == null || lstBuilderSearchResults.SelectedIndex >= matchApps.Count) return;

            AppModel selected = matchApps[lstBuilderSearchResults.SelectedIndex];
            if (selected == null) return;

            if (txtBuilderAppId != null) txtBuilderAppId.Text = selected.id ?? "";
            if (txtBuilderAppName != null) txtBuilderAppName.Text = selected.name ?? "";
            if (txtBuilderDesc != null) txtBuilderDesc.Text = selected.description ?? "";
            if (txtBuilderSilentArgs != null) txtBuilderSilentArgs.Text = selected.silentArgs ?? "";

            if (cmbBuilderCategory != null && !string.IsNullOrEmpty(selected.category))
            {
                string normCat = NormalizeCategoryId(selected.category);
                for (int i = 0; i < cmbBuilderCategory.Items.Count; i++)
                {
                    if (string.Equals(cmbBuilderCategory.Items[i].ToString(), normCat, StringComparison.OrdinalIgnoreCase))
                    {
                        cmbBuilderCategory.SelectedIndex = i;
                        break;
                    }
                }
            }

            AppModel existingCust = builderCustomApps.Find(x => string.Equals(x.id, selected.id, StringComparison.OrdinalIgnoreCase));
            if (existingCust != null)
            {
                if (txtBuilderDownloadUrl != null) txtBuilderDownloadUrl.Text = existingCust.downloadUrl ?? "";
                if (txtBuilderAffiliateWebUrl != null) txtBuilderAffiliateWebUrl.Text = existingCust.affiliateWebUrl ?? "";
                if (!string.IsNullOrEmpty(existingCust.silentArgs) && txtBuilderSilentArgs != null)
                    txtBuilderSilentArgs.Text = existingCust.silentArgs;
            }
            else
            {
                if (txtBuilderAffiliateWebUrl != null) txtBuilderAffiliateWebUrl.Text = selected.affiliateWebUrl ?? "";
            }

            lstBuilderSearchResults.Visibility = Visibility.Collapsed;
            if (txtBuilderFormStatus != null) txtBuilderFormStatus.Text = "Auto-filled from " + selected.name;
        }

        private void OnBuilderAddUpdate()
        {
            if (txtBuilderAppId == null || txtBuilderAppName == null) return;
            string id = txtBuilderAppId.Text.Trim();
            string name = txtBuilderAppName.Text.Trim();

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name))
            {
                if (txtBuilderFormStatus != null) txtBuilderFormStatus.Text = "⚠ App ID and App Name are required!";
                return;
            }

            string cat = cmbBuilderCategory != null && cmbBuilderCategory.SelectedItem != null ? cmbBuilderCategory.SelectedItem.ToString() : "utilities";
            string downloadUrl = txtBuilderDownloadUrl != null ? txtBuilderDownloadUrl.Text.Trim() : "";
            string affiliateWebUrl = txtBuilderAffiliateWebUrl != null ? txtBuilderAffiliateWebUrl.Text.Trim() : "";
            string silentArgs = txtBuilderSilentArgs != null ? txtBuilderSilentArgs.Text.Trim() : "";
            string desc = txtBuilderDesc != null ? txtBuilderDesc.Text.Trim() : "";

            AppModel target = builderEditingApp;
            if (target == null)
            {
                target = builderCustomApps.Find(x => string.Equals(x.id, id, StringComparison.OrdinalIgnoreCase));
            }

            if (target != null)
            {
                target.id = id;
                target.name = name;
                target.category = cat;
                target.downloadUrl = downloadUrl;
                target.affiliateWebUrl = !string.IsNullOrEmpty(affiliateWebUrl) ? affiliateWebUrl : null;
                target.silentArgs = silentArgs;
                target.description = desc;
                if (txtBuilderFormStatus != null) txtBuilderFormStatus.Text = "✓ Updated: " + name;
            }
            else
            {
                AppModel newApp = new AppModel()
                {
                    id = id,
                    name = name,
                    category = cat,
                    downloadUrl = downloadUrl,
                    affiliateWebUrl = !string.IsNullOrEmpty(affiliateWebUrl) ? affiliateWebUrl : null,
                    silentArgs = silentArgs,
                    description = desc
                };
                builderCustomApps.Add(newApp);
                if (txtBuilderFormStatus != null) txtBuilderFormStatus.Text = "✓ Added: " + name;
            }

            ClearBuilderForm();
            RenderBuilderCustomAppsList();
        }

        private void RenderBuilderCustomAppsList()
        {
            if (pnlBuilderCustomAppsList == null) return;
            pnlBuilderCustomAppsList.Children.Clear();

            if (txtBuilderCustomListHeader != null)
            {
                txtBuilderCustomListHeader.Text = string.Format("CONFIGURED CUSTOM & AFFILIATE APPS ({0})", builderCustomApps != null ? builderCustomApps.Count : 0);
            }

            if (builderCustomApps == null || builderCustomApps.Count == 0)
            {
                TextBlock emptyTxt = new TextBlock()
                {
                    Text = "No custom apps added yet. Use the search box above to auto-fill, or click 'Import JSON'.",
                    FontSize = 11,
                    Foreground = HexBrush("#64748B"),
                    Padding = new Thickness(12, 16, 12, 16),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                pnlBuilderCustomAppsList.Children.Add(emptyTxt);
                return;
            }

            bool isLight = currentThemeIsLight;

            for (int idx = 0; idx < builderCustomApps.Count; idx++)
            {
                AppModel app = builderCustomApps[idx];
                AppModel curApp = app;

                Border rowCard = new Border()
                {
                    Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#1E222A"),
                    BorderBrush = isLight ? HexBrush("#E2E8F0") : HexBrush("#2D3748"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(10, 6, 10, 6),
                    Margin = new Thickness(0, 0, 0, 5)
                };

                Grid rowGrid = new Grid();
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(200) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                // Col 0: Name, ID, Category
                StackPanel col0 = new StackPanel() { VerticalAlignment = VerticalAlignment.Center };
                StackPanel titleRow = new StackPanel() { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
                TextBlock txtTitle = new TextBlock()
                {
                    Text = curApp.name ?? "Untitled App",
                    FontWeight = FontWeights.Bold,
                    FontSize = 11.5,
                    Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#FFFFFF"),
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    MaxWidth = 140
                };
                Border catBadge = new Border()
                {
                    Background = isLight ? HexBrush("#EFF6FF") : HexBrush("#1E293B"),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(5, 1, 5, 1),
                    Margin = new Thickness(6, 0, 0, 0)
                };
                TextBlock txtCat = new TextBlock()
                {
                    Text = (curApp.category ?? "util").ToUpperInvariant(),
                    FontSize = 8.5,
                    FontWeight = FontWeights.Bold,
                    Foreground = HexBrush("#0066FF")
                };
                catBadge.Child = txtCat;
                titleRow.Children.Add(txtTitle);
                titleRow.Children.Add(catBadge);

                TextBlock txtId = new TextBlock()
                {
                    Text = curApp.id,
                    FontSize = 10,
                    Foreground = HexBrush("#64748B"),
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    Margin = new Thickness(0, 1, 0, 0)
                };
                col0.Children.Add(titleRow);
                col0.Children.Add(txtId);
                Grid.SetColumn(col0, 0);
                rowGrid.Children.Add(col0);

                // Col 1: URL & Args
                StackPanel col1 = new StackPanel() { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 10, 0) };
                if (!string.IsNullOrEmpty(curApp.downloadUrl))
                {
                    StackPanel urlStack = new StackPanel() { Orientation = Orientation.Horizontal };
                    Border affBadge = new Border()
                    {
                        Background = isLight ? HexBrush("#F3E8FF") : HexBrush("#2E1065"),
                        CornerRadius = new CornerRadius(4),
                        Padding = new Thickness(5, 1, 5, 1),
                        Margin = new Thickness(0, 0, 6, 0)
                    };
                    TextBlock txtAff = new TextBlock() { Text = "⚡ Direct Download", FontSize = 9, FontWeight = FontWeights.Bold, Foreground = HexBrush("#7C3AED") };
                    affBadge.Child = txtAff;
                    TextBlock txtUrl = new TextBlock()
                    {
                        Text = curApp.downloadUrl,
                        FontSize = 10,
                        Foreground = isLight ? HexBrush("#475569") : HexBrush("#94A3B8"),
                        TextTrimming = TextTrimming.CharacterEllipsis,
                        MaxWidth = 320,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    urlStack.Children.Add(affBadge);
                    urlStack.Children.Add(txtUrl);
                    col1.Children.Add(urlStack);
                }

                if (!string.IsNullOrEmpty(curApp.affiliateWebUrl))
                {
                    StackPanel webStack = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 0) };
                    Border webBadge = new Border()
                    {
                        Background = isLight ? HexBrush("#ECFDF5") : HexBrush("#064E3B"),
                        CornerRadius = new CornerRadius(4),
                        Padding = new Thickness(5, 1, 5, 1),
                        Margin = new Thickness(0, 0, 6, 0)
                    };
                    TextBlock txtWebBadge = new TextBlock() { Text = "🌐 Web Redirect", FontSize = 9, FontWeight = FontWeights.Bold, Foreground = HexBrush("#059669") };
                    webBadge.Child = txtWebBadge;
                    TextBlock txtWebUrl = new TextBlock()
                    {
                        Text = curApp.affiliateWebUrl,
                        FontSize = 10,
                        Foreground = isLight ? HexBrush("#047857") : HexBrush("#34D399"),
                        TextTrimming = TextTrimming.CharacterEllipsis,
                        MaxWidth = 320,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    webStack.Children.Add(webBadge);
                    webStack.Children.Add(txtWebUrl);
                    col1.Children.Add(webStack);
                }

                if (string.IsNullOrEmpty(curApp.downloadUrl) && string.IsNullOrEmpty(curApp.affiliateWebUrl))
                {
                    TextBlock txtWinget = new TextBlock()
                    {
                        Text = "📦 Standard Winget Engine (No Direct URL Override)",
                        FontSize = 10,
                        Foreground = HexBrush("#64748B")
                    };
                    col1.Children.Add(txtWinget);
                }

                if (!string.IsNullOrEmpty(curApp.silentArgs))
                {
                    TextBlock txtArgs = new TextBlock()
                    {
                        Text = "Silent: " + curApp.silentArgs,
                        FontSize = 9.5,
                        Foreground = HexBrush("#64748B"),
                        Margin = new Thickness(0, 1, 0, 0)
                    };
                    col1.Children.Add(txtArgs);
                }
                Grid.SetColumn(col1, 1);
                rowGrid.Children.Add(col1);

                // Col 2: Action Buttons
                StackPanel col2 = new StackPanel() { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
                Button btnEdit = new Button()
                {
                    Content = "✏️ Edit",
                    Background = isLight ? HexBrush("#FFFFFF") : HexBrush("#2A3547"),
                    Foreground = isLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC"),
                    BorderBrush = isLight ? HexBrush("#CBD5E1") : HexBrush("#475569"),
                    BorderThickness = new Thickness(1),
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 10.5,
                    Padding = new Thickness(10, 3, 10, 3),
                    Margin = new Thickness(0, 0, 6, 0),
                    Cursor = Cursors.Hand
                };
                btnEdit.Click += delegate
                {
                    builderEditingApp = curApp;
                    if (txtBuilderAppId != null) txtBuilderAppId.Text = curApp.id ?? "";
                    if (txtBuilderAppName != null) txtBuilderAppName.Text = curApp.name ?? "";
                    if (txtBuilderDownloadUrl != null) txtBuilderDownloadUrl.Text = curApp.downloadUrl ?? "";
                    if (txtBuilderAffiliateWebUrl != null) txtBuilderAffiliateWebUrl.Text = curApp.affiliateWebUrl ?? "";
                    if (txtBuilderSilentArgs != null) txtBuilderSilentArgs.Text = curApp.silentArgs ?? "";
                    if (txtBuilderDesc != null) txtBuilderDesc.Text = curApp.description ?? "";
                    if (cmbBuilderCategory != null && !string.IsNullOrEmpty(curApp.category))
                    {
                        string normCat = NormalizeCategoryId(curApp.category);
                        for (int i = 0; i < cmbBuilderCategory.Items.Count; i++)
                        {
                            if (string.Equals(cmbBuilderCategory.Items[i].ToString(), normCat, StringComparison.OrdinalIgnoreCase))
                            {
                                cmbBuilderCategory.SelectedIndex = i;
                                break;
                            }
                        }
                    }
                    if (btnBuilderAddUpdate != null) btnBuilderAddUpdate.Content = "✓ Update App";
                    if (txtBuilderFormStatus != null) txtBuilderFormStatus.Text = "Editing: " + curApp.name;
                };

                Button btnDel = new Button()
                {
                    Content = "🗑️",
                    Background = isLight ? HexBrush("#FEE2E2") : HexBrush("#450A0A"),
                    Foreground = HexBrush("#DC2626"),
                    BorderBrush = isLight ? HexBrush("#FECACA") : HexBrush("#7F1D1D"),
                    BorderThickness = new Thickness(1),
                    FontWeight = FontWeights.Bold,
                    FontSize = 10.5,
                    Padding = new Thickness(8, 3, 8, 3),
                    Cursor = Cursors.Hand,
                    ToolTip = "Remove from custom list"
                };
                btnDel.Click += delegate
                {
                    builderCustomApps.Remove(curApp);
                    if (builderEditingApp == curApp) ClearBuilderForm();
                    RenderBuilderCustomAppsList();
                    if (txtBuilderFormStatus != null) txtBuilderFormStatus.Text = "Removed: " + curApp.name;
                };

                col2.Children.Add(btnEdit);
                col2.Children.Add(btnDel);
                Grid.SetColumn(col2, 2);
                rowGrid.Children.Add(col2);

                rowCard.Child = rowGrid;
                pnlBuilderCustomAppsList.Children.Add(rowCard);
            }
        }

        private void OnBuilderSaveAndApply()
        {
            try
            {
                CustomCatalogModel ccm = new CustomCatalogModel();
                ccm.apps = builderCustomApps != null ? builderCustomApps : new List<AppModel>();
                string json = jsonSerializer.Serialize(ccm);

                string targetPath = customAppsPath;
                if (string.IsNullOrEmpty(targetPath)) targetPath = Path.Combine(appDir, "data", "custom_apps.json");
                string dir = Path.GetDirectoryName(targetPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                File.WriteAllText(targetPath, json, Encoding.UTF8);

                EnsureCatalogHierarchy();
                BuildCatalog();

                CloseCustomAppBuilderModal();
                MessageBox.Show(
                    string.Format("Custom catalog successfully saved and applied!\n\n• {0} Custom / Affiliate packages loaded.\n• Deduplication applied against base catalog.\n• Your download URLs are now active.", ccm.apps.Count),
                    "Catalog Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save custom catalog: " + ex.Message, "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnBuilderImportJson()
        {
            try
            {
                using (var ofd = new System.Windows.Forms.OpenFileDialog())
                {
                    ofd.Title = "Import Custom or Affiliate Catalog JSON";
                    ofd.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";
                    if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        string content = File.ReadAllText(ofd.FileName, Encoding.UTF8);
                        var ser = new JavaScriptSerializer();
                        ser.MaxJsonLength = int.MaxValue;

                        List<AppModel> importedApps = new List<AppModel>();
                        try
                        {
                            CustomCatalogModel ccm = ser.Deserialize<CustomCatalogModel>(content);
                            if (ccm != null && ccm.apps != null && ccm.apps.Count > 0)
                            {
                                importedApps = ccm.apps;
                            }
                        }
                        catch { }

                        if (importedApps.Count == 0)
                        {
                            try
                            {
                                ConfigModel cfg = ser.Deserialize<ConfigModel>(content);
                                if (cfg != null && cfg.apps != null && cfg.apps.Count > 0)
                                {
                                    importedApps = cfg.apps;
                                }
                            }
                            catch { }
                        }

                        if (importedApps.Count == 0)
                        {
                            try
                            {
                                List<AppModel> list = ser.Deserialize<List<AppModel>>(content);
                                if (list != null && list.Count > 0) importedApps = list;
                            }
                            catch { }
                        }

                        if (importedApps.Count > 0)
                        {
                            foreach (AppModel a in importedApps)
                            {
                                if (a == null || string.IsNullOrEmpty(a.id)) continue;
                                AppModel existing = builderCustomApps.Find(x => string.Equals(x.id, a.id, StringComparison.OrdinalIgnoreCase));
                                if (existing != null)
                                {
                                    if (!string.IsNullOrEmpty(a.name)) existing.name = a.name;
                                    if (!string.IsNullOrEmpty(a.downloadUrl)) existing.downloadUrl = a.downloadUrl;
                                    if (!string.IsNullOrEmpty(a.silentArgs)) existing.silentArgs = a.silentArgs;
                                    if (!string.IsNullOrEmpty(a.affiliateWebUrl)) existing.affiliateWebUrl = a.affiliateWebUrl;
                                    if (!string.IsNullOrEmpty(a.category)) existing.category = a.category;
                                    if (!string.IsNullOrEmpty(a.description)) existing.description = a.description;
                                }
                                else
                                {
                                    builderCustomApps.Add(a);
                                }
                            }
                            RenderBuilderCustomAppsList();
                            if (txtBuilderFormStatus != null) txtBuilderFormStatus.Text = string.Format("✓ Imported {0} apps from JSON", importedApps.Count);
                        }
                        else
                        {
                            MessageBox.Show("No valid applications found in selected JSON file.", "Import Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error importing JSON: " + ex.Message, "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnBuilderExportJson()
        {
            try
            {
                using (var sfd = new System.Windows.Forms.SaveFileDialog())
                {
                    sfd.Title = "Export Custom & Affiliate Catalog JSON";
                    sfd.Filter = "JSON Files (*.json)|*.json";
                    sfd.FileName = "custom_apps.json";
                    if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        CustomCatalogModel ccm = new CustomCatalogModel();
                        ccm.apps = builderCustomApps != null ? builderCustomApps : new List<AppModel>();
                        string json = jsonSerializer.Serialize(ccm);
                        File.WriteAllText(sfd.FileName, json, Encoding.UTF8);
                        if (txtBuilderFormStatus != null) txtBuilderFormStatus.Text = "✓ Exported to " + Path.GetFileName(sfd.FileName);
                        MessageBox.Show("Custom catalog exported successfully to:\n" + sfd.FileName, "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting JSON: " + ex.Message, "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string NormalizeCategoryId(string cat)
        {
            if (string.IsNullOrWhiteSpace(cat)) return "utilities";
            string c = cat.Trim().ToLowerInvariant();

            if (c == "all") return "all";

            // Direct ID & keyword matches
            if (c == "browsers" || c.Contains("browser")) return "browsers";
            if (c == "development" || c.Contains("develop") || c.Contains("code")) return "development";
            if (c == "communication" || c.Contains("communicat") || c.Contains("collab") || c.Contains("chat")) return "communication";
            if (c == "productivity" || c.Contains("productiv") || c.Contains("office")) return "productivity";
            if (c == "utilities" || c.Contains("utilit") || c.Contains("tool")) return "utilities";
            if (c == "media" || c.Contains("creative") || c.Contains("video") || c.Contains("audio")) return "media";
            if (c == "security" || c.Contains("secur") || c.Contains("privac") || c.Contains("antivirus")) return "security";
            if (c == "cloud" || c.Contains("storage") || c.Contains("drive") || c.Contains("backup")) return "cloud";
            if (c == "gaming" || c.Contains("game") || c.Contains("engine")) return "gaming";
            if (c == "recommended" || c == "top choice" || c == "topchoice") return "utilities";

            return c;
        }

        private bool MatchesCategory(AppModel a, string targetCat)
        {
            if (a == null) return false;
            if (string.IsNullOrEmpty(targetCat) || string.Equals(targetCat, "all", StringComparison.OrdinalIgnoreCase)) return true;

            string normTarget = NormalizeCategoryId(targetCat);
            string normApp = NormalizeCategoryId(a.category);

            if (string.Equals(normTarget, normApp, StringComparison.OrdinalIgnoreCase)) return true;

            // Direct fallback
            if (string.Equals(a.category ?? "", targetCat, StringComparison.OrdinalIgnoreCase)) return true;

            return false;
        }

        private void CheckLiveRecommendedAppsUpdate()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                bool catalogChanged = false;

                // 1. Live Sync Tier 1: Recommended & OEM Partner Software
                try
                {
                    string url = AppUrls.RecommendedAppsUrl;
                    if (!string.IsNullOrEmpty(url))
                    {
                        string directUrl = ConvertDirectUrl(url);
                        using (WebClient wc = new WebClient())
                        {
                            wc.Encoding = Encoding.UTF8;
                            wc.Headers.Add("User-Agent", "FleetForge/2.7.0");
                            string json = wc.DownloadString(directUrl);
                            if (!string.IsNullOrEmpty(json))
                            {
                                var ser = new JavaScriptSerializer();
                                ser.MaxJsonLength = int.MaxValue;
                                RecommendedDataModel remoteModel = ser.Deserialize<RecommendedDataModel>(json);
                                if (remoteModel != null && remoteModel.group != null && remoteModel.group.apps != null && remoteModel.group.apps.Count > 0)
                                {
                                    try
                                    {
                                        if (!string.IsNullOrEmpty(recommendedAppsPath))
                                        {
                                            string dir = Path.GetDirectoryName(recommendedAppsPath);
                                            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                                            File.WriteAllText(recommendedAppsPath, json, Encoding.UTF8);
                                        }
                                    }
                                    catch { }
                                    catalogChanged = true;
                                }
                            }
                        }
                    }
                }
                catch { }

                // 2. Live Sync Tier 2: Dynamic Custom & Affiliate Overrides
                try
                {
                    string custUrl = AppUrls.CustomAppsUrl;
                    if (!string.IsNullOrEmpty(custUrl))
                    {
                        string directCustUrl = ConvertDirectUrl(custUrl);
                        using (WebClient wc = new WebClient())
                        {
                            wc.Encoding = Encoding.UTF8;
                            wc.Headers.Add("User-Agent", "FleetForge/2.7.0");
                            string json = wc.DownloadString(directCustUrl);
                            if (!string.IsNullOrEmpty(json))
                            {
                                var ser = new JavaScriptSerializer();
                                ser.MaxJsonLength = int.MaxValue;
                                CustomCatalogModel remoteCustModel = ser.Deserialize<CustomCatalogModel>(json);
                                if (remoteCustModel != null && remoteCustModel.apps != null && remoteCustModel.apps.Count > 0)
                                {
                                    try
                                    {
                                        if (!string.IsNullOrEmpty(customAppsPath))
                                        {
                                            string dir = Path.GetDirectoryName(customAppsPath);
                                            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                                            File.WriteAllText(customAppsPath, json, Encoding.UTF8);
                                        }
                                    }
                                    catch { }
                                    catalogChanged = true;
                                }
                            }
                        }
                    }
                }
                catch { }

                if (catalogChanged)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        EnsureCatalogHierarchy();
                        BuildRoleCards();
                        if (currentView == "catalog")
                        {
                            BuildCatalog();
                        }
                        if (activeRoleId == "recommended")
                        {
                            SelectRole("recommended");
                        }
                    }));
                }
            });
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

            // Asynchronously sync from cloud Git link in background on every launch
            string syncUrl = !string.IsNullOrWhiteSpace(settings.cloudConfigUrl) ? settings.cloudConfigUrl : AppUrls.CloudSyncUrl;
            if (!string.IsNullOrWhiteSpace(syncUrl) && !syncUrl.Contains("your-username"))
            {
                ThreadPool.QueueUserWorkItem(delegate
                {
                    try
                    {
                        string directUrl = ConvertDirectUrl(syncUrl);
                        using (WebClient wc = new WebClient())
                        {
                            wc.Encoding = Encoding.UTF8;
                            wc.Headers.Add("User-Agent", "FleetForge/2.7.0");
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
                        wc.Headers.Add("User-Agent", "FleetForge/2.7.0");
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
            presets.Add(new PresetProfileItem("recommended", "\u2B50", "Top Choice \u2022 Curated Partner Software", "FEATURED", "Live governed affiliate and partner software, essential premium tools, and top recommendations.", AppUrls.RecommendedAppsUrl));
            presets.Add(new PresetProfileItem("full", "\uD83C\uDF10", "Complete FleetForge Enterprise Library", "16,349 PACKAGES", "Entire official FleetForge cloud repository with 16,349 packages across all 9 categories (Utilities, Browsers, Dev, Gaming, Office, etc.).", AppUrls.CloudSyncUrl));
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
                bool isRecGroup = string.Equals(currentGrp.id, "recommended", StringComparison.OrdinalIgnoreCase);
                bool isSelected = (activeRoleId == currentGrp.id);

                Border card = new Border();
                card.CornerRadius = new CornerRadius(14);
                card.Margin = new Thickness(7);
                card.Padding = new Thickness(16, 14, 16, 14);
                card.BorderThickness = new Thickness(isSelected ? 2 : 1);
                card.BorderBrush = isSelected ? HexBrush("#0066FF") : (currentThemeIsLight ? HexBrush("#E2E8F0") : HexBrush("#334155"));
                card.Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#1E293B");

                Grid g = new Grid();
                g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star), MinHeight = 14 });
                g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                // Row 0: Header (Fluent 3D Icon, Title & Description, App Count Badge)
                Grid hdr = new Grid();
                hdr.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                hdr.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                hdr.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                string gid = (currentGrp.id ?? "").ToLowerInvariant();
                ImageSource packImg = null;
                if (isRecGroup || gid == "recommended" || gid.Contains("topchoice") || gid.Contains("top choice") || gid.Contains("recommend"))
                {
                    packImg = LoadEmbeddedImage("recommended.png");
                }
                else if (gid.Contains("developer") || gid.Contains("dev")) packImg = LoadEmbeddedImage("pack_dev.png");
                else if (gid.Contains("office") || gid.Contains("productiv")) packImg = LoadEmbeddedImage("pack_office.png");
                else if (gid.Contains("media") || gid.Contains("design") || gid.Contains("creative")) packImg = LoadEmbeddedImage("pack_design.png");
                else if (gid.Contains("minimal") || gid.Contains("essential")) packImg = LoadEmbeddedImage("pack_minimal.png");

                Border iconBox = new Border()
                {
                    Width = 44,
                    Height = 44,
                    CornerRadius = new CornerRadius(10),
                    Margin = new Thickness(0, 0, 12, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };

                if (packImg != null)
                {
                    iconBox.Background = Brushes.Transparent;
                    iconBox.BorderThickness = new Thickness(0);
                    Image imgElem = new Image()
                    {
                        Source = packImg,
                        Width = 44,
                        Height = 44,
                        Stretch = Stretch.Uniform,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    RenderOptions.SetBitmapScalingMode(imgElem, BitmapScalingMode.HighQuality);
                    iconBox.Child = imgElem;
                }
                else
                {
                    string iconBg = currentThemeIsLight ? "#EFF6FF" : "#1E3A8A";
                    string iconBorder = currentThemeIsLight ? "#BFDBFE" : "#1D4ED8";
                    string iconFg = currentThemeIsLight ? "#1D4ED8" : "#93C5FD";

                    iconBox.Background = HexBrush(iconBg);
                    iconBox.BorderBrush = HexBrush(iconBorder);
                    iconBox.BorderThickness = new Thickness(1);
                    TextBlock tIcon = new TextBlock()
                    {
                        Text = GetSafeGroupIcon(currentGrp),
                        FontFamily = new FontFamily("Segoe UI Emoji, Segoe UI Symbol, Segoe UI"),
                        FontSize = 20,
                        Foreground = HexBrush(iconFg),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    iconBox.Child = tIcon;
                }
                Grid.SetColumn(iconBox, 0);
                hdr.Children.Add(iconBox);

                // Middle: Title & Description
                StackPanel textStack = new StackPanel() { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) };
                TextBlock tName = new TextBlock()
                {
                    Text = currentGrp.name,
                    FontSize = 15,
                    FontWeight = FontWeights.Bold,
                    Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC")
                };
                TextBlock tDesc = new TextBlock()
                {
                    Text = currentGrp.description,
                    FontSize = 11.5,
                    Foreground = currentThemeIsLight ? HexBrush("#64748B") : HexBrush("#94A3B8"),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 2, 0, 0)
                };
                textStack.Children.Add(tName);
                textStack.Children.Add(tDesc);
                Grid.SetColumn(textStack, 1);
                hdr.Children.Add(textStack);

                // Right: Pill Badge (e.g. 8 apps)
                Border badge = new Border()
                {
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(10, 3, 10, 3),
                    VerticalAlignment = VerticalAlignment.Top,
                    Background = currentThemeIsLight ? HexBrush("#E0F2FE") : HexBrush("#0C4A6E"),
                    BorderBrush = currentThemeIsLight ? HexBrush("#BAE6FD") : HexBrush("#075985"),
                    BorderThickness = new Thickness(1)
                };
                TextBlock cTxt = new TextBlock()
                {
                    Text = isRecGroup ? Localization.T("partner_apps_count", currentGrp.apps.Count) : Localization.T("apps_count", currentGrp.apps.Count),
                    FontSize = 11,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = currentThemeIsLight ? HexBrush("#0284C7") : HexBrush("#38BDF8")
                };
                badge.Child = cTxt;
                Grid.SetColumn(badge, 2);
                hdr.Children.Add(badge);

                Grid.SetRow(hdr, 0);
                g.Children.Add(hdr);

                // Row 2: Action Buttons [ Select Pack ]  [ Customize Apps ]  |  [ Export Pack ]
                StackPanel btnStack = new StackPanel() { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };

                Button selectBtn = new Button()
                {
                    Content = Localization.T("btn_select_pack"),
                    Background = isSelected ? HexBrush("#0066FF") : (currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#2A3547")),
                    Foreground = isSelected ? HexBrush("#FFFFFF") : (currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC")),
                    FontWeight = isSelected ? FontWeights.Bold : FontWeights.SemiBold,
                    FontSize = 12,
                    Padding = new Thickness(18, 6.5, 18, 6.5),
                    BorderThickness = isSelected ? new Thickness(0) : new Thickness(1),
                    BorderBrush = currentThemeIsLight ? HexBrush("#E2E8F0") : HexBrush("#475569"),
                    Cursor = Cursors.Hand
                };
                selectBtn.Click += delegate
                {
                    SelectRole(currentGrp.id);
                };
                btnStack.Children.Add(selectBtn);

                Button editBtn = new Button()
                {
                    Content = Localization.T("btn_customize_pack"),
                    Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#2A3547"),
                    Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC"),
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 12,
                    Padding = new Thickness(14, 6.5, 14, 6.5),
                    Margin = new Thickness(8, 0, 0, 0),
                    BorderThickness = new Thickness(1),
                    BorderBrush = currentThemeIsLight ? HexBrush("#E2E8F0") : HexBrush("#475569"),
                    Cursor = Cursors.Hand,
                    ToolTip = "Add or remove apps from this pack"
                };
                editBtn.Click += delegate { OpenEditGroupModal(currentGrp); };
                btnStack.Children.Add(editBtn);

                Button exportBtn = null;
                if (!isRecGroup)
                {
                    TextBlock sep = new TextBlock()
                    {
                        Text = "|",
                        FontSize = 12,
                        Foreground = currentThemeIsLight ? HexBrush("#CBD5E1") : HexBrush("#475569"),
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(8, 0, 8, 0)
                    };
                    btnStack.Children.Add(sep);

                    exportBtn = new Button()
                    {
                        Content = Localization.T("btn_export_pack"),
                        Background = Brushes.Transparent,
                        Foreground = currentThemeIsLight ? HexBrush("#64748B") : HexBrush("#94A3B8"),
                        FontWeight = FontWeights.SemiBold,
                        FontSize = 12,
                        Padding = new Thickness(6, 6.5, 6, 6.5),
                        BorderThickness = new Thickness(0),
                        Cursor = Cursors.Hand,
                        ToolTip = "Export this app pack to a .json file"
                    };
                    exportBtn.Click += delegate { ExportGroup(currentGrp); };
                    btnStack.Children.Add(exportBtn);
                }

                Grid.SetRow(btnStack, 2);
                g.Children.Add(btnStack);

                card.Child = g;

                card.MouseLeftButtonUp += delegate (object sender, MouseButtonEventArgs e)
                {
                    if (!e.Handled && !(e.OriginalSource is Button))
                    {
                        SelectRole(currentGrp.id);
                    }
                };

                card.MouseEnter += delegate
                {
                    if (activeRoleId != currentGrp.id)
                    {
                        card.Background = currentThemeIsLight ? HexBrush("#F8FAFC") : HexBrush("#243144");
                        card.BorderBrush = currentThemeIsLight ? HexBrush("#CBD5E1") : HexBrush("#475569");
                    }
                };
                card.MouseLeave += delegate
                {
                    if (activeRoleId != currentGrp.id)
                    {
                        card.Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#1E293B");
                        card.BorderBrush = currentThemeIsLight ? HexBrush("#E2E8F0") : HexBrush("#334155");
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
                bool isSelected = (rc.Group.id == targetGrp.id);
                if (isSelected)
                {
                    rc.Card.BorderBrush = HexBrush("#0066FF");
                    rc.Card.BorderThickness = new Thickness(2);
                    rc.Card.Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#1E293B");
                    rc.SelectButton.Content = Localization.T("btn_select_pack");
                    rc.SelectButton.Background = HexBrush("#0066FF");
                    rc.SelectButton.Foreground = HexBrush("#FFFFFF");
                    rc.SelectButton.FontWeight = FontWeights.Bold;
                    rc.SelectButton.BorderThickness = new Thickness(0);
                }
                else
                {
                    rc.Card.BorderBrush = currentThemeIsLight ? HexBrush("#E2E8F0") : HexBrush("#334155");
                    rc.Card.BorderThickness = new Thickness(1);
                    rc.Card.Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#1E293B");
                    rc.SelectButton.Content = Localization.T("btn_select_pack");
                    rc.SelectButton.Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#2A3547");
                    rc.SelectButton.Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
                    rc.SelectButton.FontWeight = FontWeights.SemiBold;
                    rc.SelectButton.BorderBrush = currentThemeIsLight ? HexBrush("#E2E8F0") : HexBrush("#475569");
                    rc.SelectButton.BorderThickness = new Thickness(1);
                }
            }

            // Sync selected apps to active pack
            if (targetGrp.apps != null)
            {
                selectedAppIds.Clear();
                foreach (string appId in targetGrp.apps)
                {
                    selectedAppIds.Add(appId);
                }
                UpdateCount();
            }

            if (txtStatus != null && targetGrp != null)
            {
                if (currentView != "setup")
                {
                    txtStatus.Text = "App pack selected: " + targetGrp.name;
                }
                else
                {
                    txtStatus.Text = "";
                }
            }

            if (txtBottomSelectedPackInfo != null && targetGrp != null)
            {
                txtBottomSelectedPackInfo.Text = "App pack selected: " + FixMojibake(targetGrp.name);
            }
        }

        // =========================================================================
        // CREATE / EDIT / DELETE MODAL LOGIC
        // =========================================================================
        private void OpenCreateGroupModal()
        {
            currentEditingGroupId = null;
            txtModalTitle.Text = "Create App Pack";
            if (txtModalSubtitle != null)
            {
                txtModalSubtitle.Text = "Create custom role profile, icon, and default software package suite.";
            }
            txtGroupNameInput.Text = "";
            txtGroupNameInput.IsReadOnly = false;
            txtGroupIconInput.Text = "🎨";
            txtGroupIconInput.IsReadOnly = false;
            if (txtPackIconPreview != null) txtPackIconPreview.Text = "🎨";
            txtGroupDescInput.Text = "";
            txtGroupDescInput.IsReadOnly = false;
            UpdateDescCharCount();

            btnDeleteGroup.Visibility = Visibility.Collapsed;
            if (btnModalExportGroup != null) btnModalExportGroup.Visibility = Visibility.Collapsed;
            if (borderModalSearchBox != null) borderModalSearchBox.Visibility = Visibility.Visible;
            if (panelEmojiChips != null) panelEmojiChips.Visibility = Visibility.Visible;

            BuildEmojiChips();
            PopulateModalAppChecklist(new List<string>());
            groupModalOverlay.Visibility = Visibility.Visible;
            txtGroupNameInput.Focus();
        }

        private void OpenEditGroupModal(GroupModel grp)
        {
            currentEditingGroupId = grp.id;
            bool isRecommended = string.Equals(grp.id, "recommended", StringComparison.OrdinalIgnoreCase);

            txtModalTitle.Text = isRecommended ? "Top Choice • Recommended Software" : "Edit App Pack";
            if (txtModalSubtitle != null)
            {
                txtModalSubtitle.Text = isRecommended
                    ? "Customize curated partner packages and top-rated software recommendations."
                    : "Customize pack details, icon, and included software.";
            }

            txtGroupNameInput.Text = grp.name;
            txtGroupNameInput.IsReadOnly = isRecommended;
            string icon = GetSafeGroupIcon(grp);
            txtGroupIconInput.Text = icon;
            txtGroupIconInput.IsReadOnly = isRecommended;

            string modGid = (grp.id ?? "").ToLowerInvariant();
            ImageSource modPackImg = null;
            if (isRecommended || modGid == "recommended" || modGid.Contains("topchoice") || modGid.Contains("top choice") || modGid.Contains("recommend"))
            {
                modPackImg = LoadEmbeddedImage("recommended.png");
            }
            else if (modGid.Contains("developer") || modGid.Contains("dev")) modPackImg = LoadEmbeddedImage("pack_dev.png");
            else if (modGid.Contains("office") || modGid.Contains("productiv")) modPackImg = LoadEmbeddedImage("pack_office.png");
            else if (modGid.Contains("media") || modGid.Contains("design") || modGid.Contains("creative")) modPackImg = LoadEmbeddedImage("pack_design.png");
            else if (modGid.Contains("minimal") || modGid.Contains("essential")) modPackImg = LoadEmbeddedImage("pack_minimal.png");

            if (modPackImg != null && imgPackIconPreview != null)
            {
                imgPackIconPreview.Source = modPackImg;
                imgPackIconPreview.Visibility = Visibility.Visible;
                if (txtPackIconPreview != null) txtPackIconPreview.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (imgPackIconPreview != null) imgPackIconPreview.Visibility = Visibility.Collapsed;
                if (txtPackIconPreview != null)
                {
                    txtPackIconPreview.Visibility = Visibility.Visible;
                    txtPackIconPreview.Text = icon;
                }
            }

            if (borderPackIconBadgeEdit != null)
            {
                borderPackIconBadgeEdit.Visibility = isRecommended ? Visibility.Collapsed : Visibility.Visible;
            }
            if (borderPackIconCard != null)
            {
                borderPackIconCard.Cursor = isRecommended ? Cursors.Arrow : Cursors.Hand;
                borderPackIconCard.ToolTip = isRecommended ? "Top Choice Protected System Pack Icon" : "Click to cycle icon";
            }

            txtGroupDescInput.Text = grp.description;
            txtGroupDescInput.IsReadOnly = isRecommended;
            UpdateDescCharCount();

            if (txtModalHeaderBadgeRole != null)
            {
                txtModalHeaderBadgeRole.Text = isRecommended ? "Top Choice" : "Active in Role";
            }
            if (badgeModalHeaderRole != null)
            {
                badgeModalHeaderRole.Background = isRecommended ? HexBrush("#F59E0B") : HexBrush("#0066FF");
            }
            if (borderModalHeaderBadge != null)
            {
                borderModalHeaderBadge.Background = isRecommended ? HexBrush("#F59E0B") : HexBrush("#0066FF");
            }
            if (txtModalHeaderGlyph != null)
            {
                txtModalHeaderGlyph.Text = isRecommended ? "\uE735" : "\uE74C";
            }

            btnDeleteGroup.Visibility = isRecommended ? Visibility.Collapsed : Visibility.Visible;
            if (btnModalExportGroup != null) btnModalExportGroup.Visibility = isRecommended ? Visibility.Collapsed : Visibility.Visible;

            if (btnModalFilterTabAll != null && btnModalFilterTabSelected != null)
            {
                btnModalFilterTabAll.Visibility = isRecommended ? Visibility.Collapsed : Visibility.Visible;
                btnModalFilterTabSelected.Visibility = isRecommended ? Visibility.Collapsed : Visibility.Visible;
            }

            if (borderModalSearchBox != null)
            {
                borderModalSearchBox.Visibility = isRecommended ? Visibility.Collapsed : Visibility.Visible;
            }

            if (panelEmojiChips != null) panelEmojiChips.Visibility = isRecommended ? Visibility.Collapsed : Visibility.Visible;

            BuildEmojiChips();
            PopulateModalAppChecklist(grp.apps);
            groupModalOverlay.Visibility = Visibility.Visible;
            if (!isRecommended) txtGroupNameInput.Focus();
        }

        private void UpdateModalTabStyles()
        {
            if (btnModalFilterTabAll != null && btnModalFilterTabSelected != null)
            {
                if (modalShowSelectedOnly)
                {
                    btnModalFilterTabSelected.Background = HexBrush("#0066FF");
                    btnModalFilterTabSelected.Foreground = HexBrush("#FFFFFF");
                    btnModalFilterTabSelected.BorderThickness = new Thickness(0);

                    btnModalFilterTabAll.Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#2A3547");
                    btnModalFilterTabAll.Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
                    btnModalFilterTabAll.BorderThickness = new Thickness(1);
                    btnModalFilterTabAll.BorderBrush = currentThemeIsLight ? HexBrush("#CBD5E1") : HexBrush("#475569");
                }
                else
                {
                    btnModalFilterTabAll.Background = HexBrush("#0066FF");
                    btnModalFilterTabAll.Foreground = HexBrush("#FFFFFF");
                    btnModalFilterTabAll.BorderThickness = new Thickness(0);

                    btnModalFilterTabSelected.Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#2A3547");
                    btnModalFilterTabSelected.Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
                    btnModalFilterTabSelected.BorderThickness = new Thickness(1);
                    btnModalFilterTabSelected.BorderBrush = currentThemeIsLight ? HexBrush("#CBD5E1") : HexBrush("#475569");
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
            txtGroupNameInput.IsReadOnly = false;
            txtGroupIconInput.IsReadOnly = false;
            txtGroupDescInput.IsReadOnly = false;
            if (btnModalFilterTabAll != null) btnModalFilterTabAll.Visibility = Visibility.Visible;
            if (btnModalFilterTabSelected != null) btnModalFilterTabSelected.Visibility = Visibility.Visible;
            if (panelEmojiChips != null) panelEmojiChips.Visibility = Visibility.Visible;
            if (btnModalExportGroup != null) btnModalExportGroup.Visibility = Visibility.Visible;
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
            bool isRecommended = string.Equals(currentEditingGroupId, "recommended", StringComparison.OrdinalIgnoreCase);
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

            if (isRecommended)
            {
                List<string> candidateIds = (recommendedMasterAppIds != null && recommendedMasterAppIds.Count > 0)
                    ? recommendedMasterAppIds
                    : (config.groups.Count > 0 && config.groups[0] != null && config.groups[0].apps != null ? config.groups[0].apps : new List<string>());

                foreach (string id in candidateIds)
                {
                    AppModel foundApp;
                    if (!appDict.TryGetValue(id, out foundApp))
                    {
                        foundApp = new AppModel() { id = id, name = id, category = "utilities", description = "Recommended partner software package" };
                    }

                    if (!string.IsNullOrEmpty(q))
                    {
                        bool match = (foundApp.name != null && foundApp.name.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                     (foundApp.id != null && foundApp.id.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                     (foundApp.description != null && foundApp.description.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0);
                        if (!match) continue;
                    }

                    if (modalSelectedAppIds.Contains(foundApp.id))
                    {
                        selectedAppsList.Add(foundApp);
                    }
                    else
                    {
                        unselectedAppsList.Add(foundApp);
                    }
                }
            }
            else if (string.IsNullOrEmpty(q))
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
            bool isRecommended = string.Equals(currentEditingGroupId, "recommended", StringComparison.OrdinalIgnoreCase);

            if (isRecommended)
            {
                if (selectedApps.Count > 0)
                {
                    Border selHeader = CreateModalSectionHeader(
                        string.Format("⭐ CHECKED RECOMMENDED PACKAGES ({0})", selectedApps.Count),
                        "These partner tools are checked and will be installed on your machine",
                        true);
                    modalAppsContainer.Children.Add(selHeader);

                    foreach (AppModel a in selectedApps)
                    {
                        Border card = CreateModalAppCard(a);
                        modalAppsContainer.Children.Add(card);
                    }
                }

                if (unselectedApps.Count > 0)
                {
                    Border unselHeader = CreateModalSectionHeader(
                        string.Format("○ UNCHECKED RECOMMENDED PACKAGES ({0})", unselectedApps.Count),
                        "Click to check these recommended packages for installation",
                        false);
                    modalAppsContainer.Children.Add(unselHeader);

                    foreach (AppModel a in unselectedApps)
                    {
                        Border card = CreateModalAppCard(a);
                        modalAppsContainer.Children.Add(card);
                    }
                }
                return;
            }

            // Section 1: Selected Software (Header is pinned at the top of the table!)
            if (selectedApps.Count > 0)
            {
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

        private void SetupModalAppIcon(AppModel curApp, Border iconBadge, TextBlock txtGlyph)
        {
            if (curApp == null) return;
            string id = (curApp.id ?? "").ToLowerInvariant();
            string name = (curApp.name ?? "").ToLowerInvariant();
            string cat = (curApp.category ?? "").ToLowerInvariant();

            string glyph = "\uE74C";
            string font = "Segoe MDL2 Assets, Segoe Fluent Icons, Segoe UI Symbol";
            string bg = currentThemeIsLight ? "#EFF6FF" : "#1E293B";
            string fg = "#0066FF";

            if (name.Contains("blender") || id.Contains("blender"))
            {
                glyph = "\uE790";
                bg = currentThemeIsLight ? "#FFF7ED" : "#431407";
                fg = currentThemeIsLight ? "#EA580C" : "#FB923C";
            }
            else if (name.Contains("davinci") || id.Contains("davinci"))
            {
                glyph = "\uE714";
                bg = currentThemeIsLight ? "#F5F3FF" : "#2E1065";
                fg = currentThemeIsLight ? "#7C3AED" : "#A78BFA";
            }
            else if (name.Contains("figma") || id.Contains("figma"))
            {
                glyph = "\uE790";
                bg = currentThemeIsLight ? "#FDF2F8" : "#500724";
                fg = currentThemeIsLight ? "#DB2777" : "#F472B6";
            }
            else if (name.Contains("gimp") || id.Contains("gimp"))
            {
                glyph = "\uE790";
                bg = currentThemeIsLight ? "#FEF3C7" : "#451A03";
                fg = currentThemeIsLight ? "#D97706" : "#FBBF24";
            }
            else if (name.Contains("handbrake") || id.Contains("handbrake"))
            {
                glyph = "\uE714";
                bg = currentThemeIsLight ? "#FFF1F2" : "#4C0519";
                fg = currentThemeIsLight ? "#E11D48" : "#FB7185";
            }
            else if (name.Contains("obs") || id.Contains("obs"))
            {
                glyph = "\uE714";
                bg = currentThemeIsLight ? "#F1F5F9" : "#0F172A";
                fg = currentThemeIsLight ? "#0F172A" : "#F8FAFC";
            }
            else if (name.Contains("vlc") || id.Contains("vlc"))
            {
                glyph = "\uE714";
                bg = currentThemeIsLight ? "#FFF7ED" : "#431407";
                fg = currentThemeIsLight ? "#F97316" : "#FB923C";
            }
            else if (cat.Contains("browser") || id.Contains("chrome") || id.Contains("firefox") || id.Contains("edge") || id.Contains("brave"))
            {
                glyph = "\uE774";
                bg = currentThemeIsLight ? "#EFF6FF" : "#172554";
                fg = currentThemeIsLight ? "#2563EB" : "#60A5FA";
            }
            else if (cat.Contains("dev") || id.Contains("vscode") || id.Contains("git") || id.Contains("node") || id.Contains("python"))
            {
                glyph = "</>";
                font = "Consolas, Segoe UI Semibold, Segoe UI";
                bg = currentThemeIsLight ? "#EEF2FF" : "#1E1B4B";
                fg = currentThemeIsLight ? "#4F46E5" : "#818CF8";
            }
            else if (cat.Contains("sec") || id.Contains("vpn") || id.Contains("isecure"))
            {
                glyph = "\uEA18";
                bg = currentThemeIsLight ? "#F0F9FF" : "#082F49";
                fg = currentThemeIsLight ? "#0284C7" : "#38BDF8";
            }
            else if (cat.Contains("util") || id.Contains("clean") || id.Contains("driver") || id.Contains("riser"))
            {
                glyph = "\uE713";
                bg = currentThemeIsLight ? "#ECFDF5" : "#022C22";
                fg = currentThemeIsLight ? "#059669" : "#34D399";
            }
            else if (cat.Contains("game") || id.Contains("steam"))
            {
                glyph = "\uE7FC";
                bg = currentThemeIsLight ? "#FAF5FF" : "#3B0764";
                fg = currentThemeIsLight ? "#9333EA" : "#C084FC";
            }
            else if (cat.Contains("media") || cat.Contains("creat"))
            {
                glyph = "\uE714";
                bg = currentThemeIsLight ? "#F5F3FF" : "#2E1065";
                fg = currentThemeIsLight ? "#7C3AED" : "#A78BFA";
            }

            iconBadge.Background = HexBrush(bg);
            txtGlyph.Text = glyph;
            txtGlyph.FontFamily = new FontFamily(font);
            txtGlyph.Foreground = HexBrush(fg);
            txtGlyph.FontSize = (glyph == "</>") ? 12 : 16;
        }

        private Border CreateModalSectionHeader(string title, string subtitle, bool isSelectedSection)
        {
            Border b = new Border()
            {
                Background = isSelectedSection ?
                    (currentThemeIsLight ? HexBrush("#EFF6FF") : HexBrush("#172554")) :
                    (currentThemeIsLight ? HexBrush("#F8FAFC") : HexBrush("#1E293B")),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(14, 10, 14, 10),
                Margin = new Thickness(0, isSelectedSection ? 0 : 10, 0, 6),
                BorderThickness = new Thickness(1),
                BorderBrush = isSelectedSection ?
                    (currentThemeIsLight ? HexBrush("#BFDBFE") : HexBrush("#1E40AF")) :
                    (currentThemeIsLight ? HexBrush("#E2E8F0") : HexBrush("#334155"))
            };

            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            if (isSelectedSection)
            {
                CheckBox cb = new CheckBox()
                {
                    IsChecked = true,
                    IsEnabled = false,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 12, 0)
                };
                Grid.SetColumn(cb, 0);
                g.Children.Add(cb);
            }

            StackPanel sp = new StackPanel() { VerticalAlignment = VerticalAlignment.Center };
            TextBlock txtTitle = new TextBlock()
            {
                Text = title,
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = isSelectedSection ?
                    (currentThemeIsLight ? HexBrush("#1E3A8A") : HexBrush("#93C5FD")) :
                    (currentThemeIsLight ? HexBrush("#1E293B") : HexBrush("#F1F5F9"))
            };
            sp.Children.Add(txtTitle);

            if (!string.IsNullOrEmpty(subtitle))
            {
                TextBlock txtSub = new TextBlock()
                {
                    Text = subtitle,
                    FontSize = 11,
                    Foreground = isSelectedSection ?
                        (currentThemeIsLight ? HexBrush("#3B82F6") : HexBrush("#60A5FA")) :
                        (currentThemeIsLight ? HexBrush("#64748B") : HexBrush("#94A3B8")),
                    Margin = new Thickness(0, 2, 0, 0)
                };
                sp.Children.Add(txtSub);
            }
            Grid.SetColumn(sp, 1);
            g.Children.Add(sp);

            Border pill = new Border()
            {
                Background = isSelectedSection ? HexBrush("#0066FF") :
                    (currentThemeIsLight ? HexBrush("#E2E8F0") : HexBrush("#334155")),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(12, 3, 12, 3),
                VerticalAlignment = VerticalAlignment.Center
            };
            TextBlock pillText = new TextBlock()
            {
                Text = isSelectedSection ? "Active in Role" : "CATALOG",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = isSelectedSection ? HexBrush("#FFFFFF") :
                    (currentThemeIsLight ? HexBrush("#475569") : HexBrush("#E2E8F0"))
            };
            pill.Child = pillText;
            Grid.SetColumn(pill, 2);
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
                        Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#2A3547"),
                        Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC"),
                        BorderBrush = currentThemeIsLight ? HexBrush("#CBD5E1") : HexBrush("#475569"),
                        BorderThickness = new Thickness(1),
                        Cursor = Cursors.Hand,
                        FontWeight = FontWeights.SemiBold,
                        FontSize = 11.5,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    btnModalLoadMore.Click += delegate
                    {
                        RenderNextModalCatalogBatch(50);
                    };
                }
                else
                {
                    btnModalLoadMore.Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#2A3547");
                    btnModalLoadMore.Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
                    btnModalLoadMore.BorderBrush = currentThemeIsLight ? HexBrush("#CBD5E1") : HexBrush("#475569");
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
            Border card = new Border()
            {
                Background = isChecked ?
                    (currentThemeIsLight ? HexBrush("#F0F7FF") : HexBrush("#1C2E42")) :
                    (currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#1E293B")),
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(0, 0, 0, 6),
                Padding = new Thickness(14, 10, 14, 10),
                BorderThickness = new Thickness(1),
                BorderBrush = isChecked ?
                    (currentThemeIsLight ? HexBrush("#0066FF") : HexBrush("#38BDF8")) :
                    (currentThemeIsLight ? HexBrush("#F1F5F9") : HexBrush("#334155")),
                Cursor = Cursors.Hand,
                Tag = curApp
            };

            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); // Col 0: CheckBox
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); // Col 1: Icon Badge
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) }); // Col 2: Info Stack
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); // Col 3: Status Badge

            CheckBox cb = new CheckBox()
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 14, 0),
                Cursor = Cursors.Hand,
                IsChecked = isChecked
            };
            Grid.SetColumn(cb, 0);
            g.Children.Add(cb);

            // Icon Badge (40x40 matching All Apps card)
            string glyph = GetAppGlyphIcon(curApp);
            Border iconBadge = new Border()
            {
                Width = 40,
                Height = 40,
                CornerRadius = new CornerRadius(10),
                Background = currentThemeIsLight ? HexBrush("#EFF6FF") : HexBrush("#243144"),
                Margin = new Thickness(0, 0, 14, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            TextBlock txtGlyph = new TextBlock()
            {
                Text = glyph,
                FontFamily = (glyph == "</>") ? new FontFamily("Consolas, Segoe UI Semibold, Segoe UI") : new FontFamily("Segoe MDL2 Assets, Segoe Fluent Icons, Segoe UI Symbol"),
                FontSize = (glyph == "</>") ? 14 : 20,
                FontWeight = (glyph == "</>") ? FontWeights.Bold : FontWeights.Normal,
                Foreground = HexBrush("#0066FF"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            iconBadge.Child = txtGlyph;
            Grid.SetColumn(iconBadge, 1);
            g.Children.Add(iconBadge);

            // Info Stack (Two-line title + description matching All Apps)
            StackPanel infoStack = new StackPanel() { VerticalAlignment = VerticalAlignment.Center };
            StackPanel titleStack = new StackPanel() { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            TextBlock txtName = new TextBlock()
            {
                Text = curApp.name,
                FontSize = 13.5,
                FontWeight = FontWeights.Bold,
                Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC")
            };
            titleStack.Children.Add(txtName);

            string catText = (curApp.category ?? "").Trim();
            if (string.IsNullOrEmpty(catText) || catText.IndexOf("online", StringComparison.OrdinalIgnoreCase) >= 0 || catText.IndexOf("winget", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                catText = InferAppCategory(curApp.name, curApp.id);
            }
            catText = catText.ToUpperInvariant();

            Border badge = new Border()
            {
                Background = currentThemeIsLight ? HexBrush("#EFF6FF") : HexBrush("#1E293B"),
                CornerRadius = new CornerRadius(5),
                Margin = new Thickness(8, 0, 0, 0),
                Padding = new Thickness(7, 2, 7, 2)
            };
            TextBlock txtBadge = new TextBlock()
            {
                Text = catText,
                FontSize = 9.5,
                FontWeight = FontWeights.Bold,
                Foreground = HexBrush("#0066FF")
            };
            badge.Child = txtBadge;
            titleStack.Children.Add(badge);

            if (IsRecommendedApp(curApp))
            {
                Border recBadge = new Border()
                {
                    Background = currentThemeIsLight ? HexBrush("#FEF3C7") : HexBrush("#451A03"),
                    BorderBrush = currentThemeIsLight ? HexBrush("#FDE68A") : HexBrush("#78350F"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(5),
                    Margin = new Thickness(6, 0, 0, 0),
                    Padding = new Thickness(7, 2, 7, 2)
                };
                TextBlock txtRec = new TextBlock()
                {
                    Text = "★ RECOMMENDED",
                    FontSize = 9.5,
                    FontWeight = FontWeights.Bold,
                    Foreground = currentThemeIsLight ? HexBrush("#B45309") : HexBrush("#FBBF24")
                };
                recBadge.Child = txtRec;
                titleStack.Children.Add(recBadge);
            }


            infoStack.Children.Add(titleStack);

            TextBlock txtDesc = new TextBlock()
            {
                Text = curApp.description ?? ("Winget package (" + curApp.id + ")"),
                FontSize = 11.5,
                Foreground = currentThemeIsLight ? HexBrush("#64748B") : HexBrush("#94A3B8"),
                Margin = new Thickness(0, 3, 0, 0),
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            infoStack.Children.Add(txtDesc);
            Grid.SetColumn(infoStack, 2);
            g.Children.Add(infoStack);

            // Status Badge (Column 3)
            bool isRec = string.Equals(currentEditingGroupId, "recommended", StringComparison.OrdinalIgnoreCase);
            Border statusBadge = new Border()
            {
                Background = isChecked ?
                    (currentThemeIsLight ? HexBrush("#DCFCE7") : HexBrush("#064E3B")) :
                    (currentThemeIsLight ? HexBrush("#F1F5F9") : HexBrush("#243144")),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10, 4, 10, 4),
                VerticalAlignment = VerticalAlignment.Center
            };
            TextBlock statusBadgeText = new TextBlock()
            {
                Text = isChecked ? "✓ Selected" : (isRec ? "○ Unchecked" : "+ Add"),
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = isChecked ?
                    (currentThemeIsLight ? HexBrush("#15803D") : HexBrush("#34D399")) :
                    (currentThemeIsLight ? HexBrush("#64748B") : HexBrush("#94A3B8"))
            };
            statusBadge.Child = statusBadgeText;
            Grid.SetColumn(statusBadge, 3);
            g.Children.Add(statusBadge);

            card.Child = g;

            string appId = curApp.id;
            Border curBorder = card;
            Border curBadge = statusBadge;
            TextBlock curBadgeText = statusBadgeText;

            Action updateVisualState = delegate
            {
                bool active = cb.IsChecked == true;
                bool isRecMode = string.Equals(currentEditingGroupId, "recommended", StringComparison.OrdinalIgnoreCase);
                if (active)
                {
                    curBorder.Background = currentThemeIsLight ? HexBrush("#F0F7FF") : HexBrush("#1C2E42");
                    curBorder.BorderBrush = currentThemeIsLight ? HexBrush("#0066FF") : HexBrush("#38BDF8");

                    curBadge.Background = currentThemeIsLight ? HexBrush("#DCFCE7") : HexBrush("#064E3B");
                    curBadgeText.Text = "✓ Selected";
                    curBadgeText.Foreground = currentThemeIsLight ? HexBrush("#15803D") : HexBrush("#34D399");
                }
                else
                {
                    curBorder.Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#1E293B");
                    curBorder.BorderBrush = currentThemeIsLight ? HexBrush("#F1F5F9") : HexBrush("#334155");

                    curBadge.Background = currentThemeIsLight ? HexBrush("#F1F5F9") : HexBrush("#243144");
                    curBadgeText.Text = isRecMode ? "○ Unchecked" : "+ Add";
                    curBadgeText.Foreground = currentThemeIsLight ? HexBrush("#64748B") : HexBrush("#94A3B8");
                }
            };

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

            card.MouseEnter += delegate
            {
                if (cb.IsChecked != true)
                {
                    curBorder.Background = currentThemeIsLight ? HexBrush("#F8FAFC") : HexBrush("#243144");
                }
            };
            card.MouseLeave += delegate
            {
                if (cb.IsChecked != true)
                {
                    curBorder.Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#1E293B");
                }
            };

            card.MouseLeftButtonUp += delegate (object sender, MouseButtonEventArgs e)
            {
                if (!e.Handled && !(e.OriginalSource is CheckBox))
                {
                    cb.IsChecked = !cb.IsChecked;
                }
            };

            modalAppRows.Add(new ModalAppItemView()
            {
                App = curApp,
                Container = curBorder,
                CheckBox = cb,
                StatusBadge = curBadge,
                StatusBadgeText = curBadgeText
            });

            return card;
        }

        private void UpdateModalCount()
        {
            int selCount = modalSelectedAppIds.Count;
            int totalCount = modalFilteredApps != null ? modalFilteredApps.Count : selCount;

            if (txtModalSelectedCount != null)
            {
                txtModalSelectedCount.Text = selCount + " selected";
            }
            if (btnModalFilterTabAll != null)
            {
                btnModalFilterTabAll.Content = string.Format("All Software ({0})", totalCount);
            }
            if (btnModalFilterTabSelected != null)
            {
                btnModalFilterTabSelected.Content = string.Format("Selected ({0})", selCount);
            }
            if (txtModalTableHeaderCount != null)
            {
                if (modalShowSelectedOnly)
                {
                    txtModalTableHeaderCount.Text = string.Format("{0} software selected in role", selCount);
                }
                else
                {
                    txtModalTableHeaderCount.Text = string.Format("{0} of {1} software selected", selCount, totalCount);
                }
            }

            if (chkModalSelectAll != null && modalFilteredApps != null && modalFilteredApps.Count > 0)
            {
                int matchedSel = 0;
                foreach (AppModel a in modalFilteredApps)
                {
                    if (a != null && a.id != null && modalSelectedAppIds.Contains(a.id))
                    {
                        matchedSel++;
                    }
                }
                if (matchedSel == modalFilteredApps.Count)
                {
                    chkModalSelectAll.IsChecked = true;
                }
                else if (matchedSel == 0)
                {
                    chkModalSelectAll.IsChecked = false;
                }
                else
                {
                    chkModalSelectAll.IsChecked = null; // Indeterminate
                }
            }
        }

        private void SaveGroupModal()
        {
            if (string.Equals(currentEditingGroupId, "recommended", StringComparison.OrdinalIgnoreCase))
            {
                GroupModel recGrp = config.groups.Find(delegate (GroupModel g) { return g.id == "recommended"; });
                if (recGrp != null)
                {
                    recGrp.apps = new List<string>(modalSelectedAppIds);
                }

                SaveConfigToFile();
                CloseGroupModal();

                BuildRoleCards();
                BuildCatalog();
                SelectRole("recommended");

                if (txtStatus != null) txtStatus.Text = string.Format("Top Choice: {0} software packages selected for installation.", modalSelectedAppIds.Count);
                return;
            }

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
            if (string.Equals(currentEditingGroupId, "recommended", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("The Top Choice • Recommended pack is a protected system pack and cannot be deleted.", "Protected Pack", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
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
            if (string.Equals(grp.id, "recommended", StringComparison.OrdinalIgnoreCase)) return;
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
                Padding = new Thickness(14, 0, 14, 0),
                Margin = new Thickness(0, 0, 8, 8),
                Cursor = Cursors.Hand,
                BorderThickness = new Thickness(1),
                Tag = id
            };

            if (this.Resources.Contains("CategoryPillStyle"))
            {
                btn.Style = (Style)this.Resources["CategoryPillStyle"];
            }

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
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(8, 2, 8, 2),
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
                btn.Background = isOnline ? HexBrush("#7C3AED") : HexBrush("#0066FF");
                btn.BorderThickness = new Thickness(0);
                if (txtIcon != null) txtIcon.Foreground = HexBrush("#FFFFFF");
                if (txtTitle != null) txtTitle.Foreground = HexBrush("#FFFFFF");
                if (badge != null) badge.Background = HexBrush("#2552C8");
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
                    btn.Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#2A3547");
                    btn.BorderBrush = currentThemeIsLight ? HexBrush("#CBD5E1") : HexBrush("#475569");
                    btn.BorderThickness = new Thickness(1);
                    if (txtIcon != null) txtIcon.Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
                    if (txtTitle != null) txtTitle.Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
                    if (badge != null) badge.Background = currentThemeIsLight ? HexBrush("#EFF6FF") : HexBrush("#1E293B");
                    if (txtBadge != null) txtBadge.Foreground = currentThemeIsLight ? HexBrush("#2563EB") : HexBrush("#60A5FA");
                }
            }
        }

        private void BuildCatalog()
        {
            appsContainer.Children.Clear();
            appRows.Clear();
            RebuildCategoryChips();

            localAppsPanel = new StackPanel();
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
                Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#2A3547"),
                Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC"),
                BorderThickness = new Thickness(1),
                BorderBrush = currentThemeIsLight ? HexBrush("#CBD5E1") : HexBrush("#475569"),
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
                Background = HexBrush("#0066FF"),
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
                if (catalogPaginationPanel != null) catalogPaginationPanel.Visibility = Visibility.Collapsed;
                if (btnLoadAll != null) btnLoadAll.Visibility = Visibility.Collapsed;
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
                    catalogPaginationPanel.Visibility = Visibility.Collapsed;
                    if (btnLoadMore != null) btnLoadMore.Visibility = Visibility.Collapsed;
                    if (btnLoadAll != null) btnLoadAll.Visibility = Visibility.Collapsed;
                }
            }
        }

        private static string InferAppCategory(string name, string id)
        {
            string s = ((name ?? "") + " " + (id ?? "")).ToLowerInvariant();

            // 1. Web Browsers
            if (s.Contains("browser") || s.Contains("chrome") || s.Contains("firefox") || s.Contains("msedge") || 
                s.Contains("brave") || s.Contains("opera") || s.Contains("vivaldi") || s.Contains("torproject") || 
                s.Contains("tor browser") || s.Contains("chromium") || s.Contains("waterfox") || s.Contains("floorp") || 
                s.Contains("safari") || s.Contains("librewolf") || s.Contains("zen-browser") ||
                s.EndsWith(".chrome") || s.EndsWith(".firefox") || s.EndsWith(".edge") || s.EndsWith(".brave"))
                return "BROWSERS";

            // 2. Security & Network
            if (s.Contains("secur") || s.Contains("vpn") || s.Contains("antivirus") || s.Contains("defender") || 
                s.Contains("firewall") || s.Contains("malware") || s.Contains("protect") || s.Contains("guard") || 
                s.Contains("bitdefender") || s.Contains("kaspersky") || s.Contains("avast") || s.Contains("norton") || 
                s.Contains("malwarebytes") || s.Contains("wireguard") || s.Contains("openvpn") || s.Contains("proton") || 
                s.Contains("vault") || s.Contains("password") || s.Contains("bitwarden") || s.Contains("keepass") || 
                s.Contains("1password") || s.Contains("authenticator") || s.Contains("privacy") || s.Contains("shield") ||
                s.Contains("wireshark") || s.Contains("nmap") || s.Contains("proxy"))
                return "SECURITY";

            // 3. Development & Code
            if (s.Contains("git") || s.Contains("code") || s.Contains("develop") || s.Contains("studio") || 
                s.Contains("compiler") || s.Contains("sdk") || s.Contains("python") || s.Contains("node") || 
                s.Contains("rust") || s.Contains("golang") || s.Contains("java") || s.Contains("dotnet") || 
                s.Contains("docker") || s.Contains("k8s") || s.Contains("kubernetes") || s.Contains("terminal") || 
                s.Contains("powershell") || s.Contains("database") || s.Contains("sql") || s.Contains("dbeaver") || 
                s.Contains("postman") || s.Contains("generator") || s.Contains("report") || s.Contains("nuget") || 
                s.Contains("npm") || s.Contains("yarn") || s.Contains("cmake") || s.Contains("sublime") || 
                s.Contains("neovim") || s.Contains("vim") || s.Contains("decompiler") || s.Contains("fiddler") ||
                s.Contains("intellij") || s.Contains("pycharm") || s.Contains("webstorm") || s.Contains("rider"))
                return "DEVELOPMENT";

            // 4. Communication & Collaboration
            if (s.Contains("chat") || s.Contains("message") || s.Contains("messenger") || s.Contains("discord") || 
                s.Contains("telegram") || s.Contains("slack") || s.Contains("teams") || s.Contains("zoom") || 
                s.Contains("skype") || s.Contains("whatsapp") || s.Contains("signal") || s.Contains("wechat") || 
                s.Contains("meet") || s.Contains("collab") || s.Contains("mail") || s.Contains("email") || 
                s.Contains("outlook") || s.Contains("thunderbird") || s.Contains("vicinae"))
                return "COMMUNICATION";

            // 5. Media & Creative
            if (s.Contains("media") || s.Contains("video") || s.Contains("audio") || s.Contains("sound") || 
                s.Contains("music") || s.Contains("player") || s.Contains("editor") || s.Contains("vlc") || 
                s.Contains("spotify") || s.Contains("obs") || s.Contains("blender") || s.Contains("photoshop") || 
                s.Contains("illustrator") || s.Contains("premiere") || s.Contains("audacity") || s.Contains("handbrake") || 
                s.Contains("gimp") || s.Contains("inkscape") || s.Contains("figma") || s.Contains("canva") || 
                s.Contains("davinci") || s.Contains("resolve") || s.Contains("aftereffects") || s.Contains("lightroom") || 
                s.Contains("foobar") || s.Contains("potplayer") || s.Contains("ffmpeg") || s.Contains("codec") ||
                s.Contains("stream") || s.Contains("render"))
                return "MEDIA";

            // 6. Cloud & Storage
            if (s.Contains("cloud") || s.Contains("storage") || s.Contains("drive") || s.Contains("dropbox") || 
                s.Contains("onedrive") || s.Contains("mega") || s.Contains("sync") || s.Contains("backup") || 
                s.Contains("torrent") || s.Contains("qbittorrent") || s.Contains("filezilla") || s.Contains("winscp") ||
                s.Contains("box") || s.Contains("nextcloud"))
                return "CLOUD";

            // 7. Productivity & Office
            if (s.Contains("office") || s.Contains("word") || s.Contains("excel") || s.Contains("powerpoint") || 
                s.Contains("pdf") || s.Contains("reader") || s.Contains("note") || s.Contains("notion") || 
                s.Contains("obsidian") || s.Contains("evernote") || s.Contains("todo") || s.Contains("task") || 
                s.Contains("calendar") || s.Contains("document") || s.Contains("writer") || s.Contains("libreoffice") || 
                s.Contains("raycast") || s.Contains("launcher") || s.Contains("calculator") || s.Contains("spreadsheet"))
                return "PRODUCTIVITY";

            // 8. Gaming & Engines
            if (s.Contains("game") || s.Contains("gaming") || s.Contains("steam") || s.Contains("epic") || 
                s.Contains("gog") || s.Contains("roblox") || s.Contains("minecraft") || s.Contains("unity") || 
                s.Contains("unreal") || s.Contains("godot") || s.Contains("emulator") || s.Contains("retroarch") || 
                s.Contains("love2d") || s.Contains("love tester"))
                return "GAMING";

            // Default
            return "UTILITIES";
        }

        private static string GetAppGlyphIcon(AppModel app)
        {
            if (app == null) return "\uE74C";
            string id = (app.id ?? "").ToLowerInvariant();
            string name = (app.name ?? "").ToLowerInvariant();
            string cat = (app.category ?? "").ToLowerInvariant();
            if (string.IsNullOrEmpty(cat) || cat.Contains("online") || cat.Contains("winget"))
            {
                cat = InferAppCategory(name, id).ToLowerInvariant();
            }

            // Precedence 1: Exact matches for showcase apps from Fluent mockup
            if (name.Contains("isecure") || id.Contains("isecure") || id.Contains("vpn") || name.Contains("vpn"))
                return "\uEA18"; // Shield (0xEA18)
            if (name.Contains("win riser") || name.Contains("riser") || id.Contains("winriser") || id.Contains("riser"))
                return "\uE916"; // Stopwatch / Speed Optimizer (0xE916)
            if (name.Contains("driver updater") || name.Contains("driver") || id.Contains("driver"))
                return "\uE713"; // Settings Gear (0xE713)
            if (name.Contains("codecontract") || id.Contains("codecontract") || id.Contains("contract"))
                return "</>"; // Code Brackets
            if (name.Contains("framework") || id.Contains("framework") || id.Contains(".net.desktop"))
                return "\uE8A9"; // Windows 4-Square Tile (0xE8A9)
            if (name.Contains("reactor") || id.Contains("reactor"))
                return "\uE81E"; // 3 Stacked Layers (0xE81E)

            // Precedence 2: Category and keyword rules
            if (cat.Contains("sec") || id.Contains("antivirus") || name.Contains("defender") || id.Contains("firewall"))
                return "\uEA18"; // Shield
            if (cat.Contains("browser") || id.Contains("chrome") || id.Contains("firefox") || id.Contains("edge") || id.Contains("brave") || id.Contains("opera") || id.Contains("vivaldi"))
                return "\uE774"; // Globe
            if (cat.Contains("dev") || id.Contains("git") || id.Contains("vscode") || id.Contains("visualstudio") || id.Contains("python") || id.Contains("node") || id.Contains("rust") || id.Contains("go"))
                return "</>"; // Code Brackets
            if (cat.Contains("util") || id.Contains("clean") || id.Contains("optim") || id.Contains("speed") || id.Contains("perf") || id.Contains("tweak"))
                return "\uE916"; // Speedometer / Stopwatch
            if (cat.Contains("game") || id.Contains("steam") || id.Contains("epic") || id.Contains("play"))
                return "\uE7FC"; // Game Controller
            if (cat.Contains("media") || cat.Contains("creat") || id.Contains("vlc") || id.Contains("player") || id.Contains("video") || id.Contains("music") || id.Contains("spotify") || id.Contains("obs"))
                return "\uE714"; // Media Film
            if (cat.Contains("commun") || cat.Contains("collab") || cat.Contains("chat") || id.Contains("discord") || id.Contains("zoom") || id.Contains("slack") || id.Contains("teams") || id.Contains("telegram"))
                return "\uE8BD"; // Chat Bubble
            if (cat.Contains("cloud") || cat.Contains("stor") || id.Contains("drive") || id.Contains("dropbox") || id.Contains("onedrive") || id.Contains("sync"))
                return "\uE753"; // Cloud
            if (cat.Contains("product") || cat.Contains("off") || id.Contains("office") || id.Contains("word") || id.Contains("excel") || id.Contains("pdf"))
                return "\uE7C3"; // Document

            return "\uE74C"; // Package Box
        }

        private Border CreateAppCard(AppModel curApp)
        {
            Border card = new Border()
            {
                Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#1E293B"),
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(0, 0, 0, 6),
                Padding = new Thickness(14, 10, 14, 10),
                BorderThickness = new Thickness(1),
                BorderBrush = currentThemeIsLight ? HexBrush("#F1F5F9") : HexBrush("#334155"),
                Cursor = Cursors.Hand,
                Tag = curApp
            };

            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
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
                    card.BorderBrush = currentThemeIsLight ? HexBrush("#0066FF") : HexBrush("#38BDF8");
                }
                else
                {
                    card.Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#1E293B");
                    card.BorderBrush = currentThemeIsLight ? HexBrush("#F1F5F9") : HexBrush("#334155");
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

            card.MouseEnter += delegate
            {
                if (cb.IsChecked != true)
                {
                    card.Background = currentThemeIsLight ? HexBrush("#F8FAFC") : HexBrush("#243144");
                }
            };
            card.MouseLeave += delegate
            {
                if (cb.IsChecked != true)
                {
                    card.Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#1E293B");
                }
            };

            card.MouseLeftButtonUp += delegate (object sender, MouseButtonEventArgs e)
            {
                if (!e.Handled && !(e.OriginalSource is CheckBox))
                {
                    cb.IsChecked = !cb.IsChecked;
                }
            };

            Grid.SetColumn(cb, 0);
            g.Children.Add(cb);

            // Icon Badge
            string glyph = GetAppGlyphIcon(curApp);
            Border iconBadge = new Border()
            {
                Width = 40,
                Height = 40,
                CornerRadius = new CornerRadius(10),
                Background = currentThemeIsLight ? HexBrush("#EFF6FF") : HexBrush("#243144"),
                Margin = new Thickness(0, 0, 14, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            TextBlock txtGlyph = new TextBlock()
            {
                Text = glyph,
                FontFamily = (glyph == "</>") ? new FontFamily("Consolas, Segoe UI Semibold, Segoe UI") : new FontFamily("Segoe MDL2 Assets, Segoe Fluent Icons, Segoe UI Symbol"),
                FontSize = (glyph == "</>") ? 14 : 20,
                FontWeight = (glyph == "</>") ? FontWeights.Bold : FontWeights.Normal,
                Foreground = HexBrush("#0066FF"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            iconBadge.Child = txtGlyph;
            Grid.SetColumn(iconBadge, 1);
            g.Children.Add(iconBadge);

            StackPanel infoStack = new StackPanel() { VerticalAlignment = VerticalAlignment.Center };
            StackPanel titleStack = new StackPanel() { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            TextBlock txtName = new TextBlock()
            {
                Text = curApp.name,
                FontSize = 13.5,
                FontWeight = FontWeights.Bold,
                Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC")
            };
            titleStack.Children.Add(txtName);

            string catText = (curApp.category ?? "").Trim();
            if (string.IsNullOrEmpty(catText) || catText.IndexOf("online", StringComparison.OrdinalIgnoreCase) >= 0 || catText.IndexOf("winget", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                catText = InferAppCategory(curApp.name, curApp.id);
            }
            catText = catText.ToUpperInvariant();

            Border badge = new Border()
            {
                Background = currentThemeIsLight ? HexBrush("#EFF6FF") : HexBrush("#1E293B"),
                CornerRadius = new CornerRadius(5),
                Margin = new Thickness(8, 0, 0, 0),
                Padding = new Thickness(7, 2, 7, 2)
            };
            TextBlock txtBadge = new TextBlock()
            {
                Text = catText,
                FontSize = 9.5,
                FontWeight = FontWeights.Bold,
                Foreground = HexBrush("#0066FF")
            };
            badge.Child = txtBadge;
            titleStack.Children.Add(badge);

            if (IsRecommendedApp(curApp))
            {
                Border recBadge = new Border()
                {
                    Background = currentThemeIsLight ? HexBrush("#FEF3C7") : HexBrush("#451A03"),
                    BorderBrush = currentThemeIsLight ? HexBrush("#FDE68A") : HexBrush("#78350F"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(5),
                    Margin = new Thickness(6, 0, 0, 0),
                    Padding = new Thickness(7, 2, 7, 2)
                };
                TextBlock txtRec = new TextBlock()
                {
                    Text = "★ RECOMMENDED",
                    FontSize = 9.5,
                    FontWeight = FontWeights.Bold,
                    Foreground = currentThemeIsLight ? HexBrush("#B45309") : HexBrush("#FBBF24")
                };
                recBadge.Child = txtRec;
                titleStack.Children.Add(recBadge);
            }


            infoStack.Children.Add(titleStack);

            TextBlock txtDesc = new TextBlock()
            {
                Text = curApp.description ?? ("Winget package (" + curApp.id + ")"),
                FontSize = 11.5,
                Foreground = currentThemeIsLight ? HexBrush("#64748B") : HexBrush("#94A3B8"),
                Margin = new Thickness(0, 3, 0, 0),
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            infoStack.Children.Add(txtDesc);
            Grid.SetColumn(infoStack, 2);
            g.Children.Add(infoStack);

            StackPanel statusStack = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(14, 0, 4, 0)
            };
            Border statusBadge = new Border()
            {
                Width = 18,
                Height = 18,
                CornerRadius = new CornerRadius(9),
                Background = HexBrush("#16A34A"),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 6, 0)
            };
            TextBlock statusIcon = new TextBlock()
            {
                Text = "✔",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = HexBrush("#FFFFFF"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            statusBadge.Child = statusIcon;

            TextBlock statusText = new TextBlock()
            {
                Text = Localization.T("badge_ready"),
                FontSize = 11.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = HexBrush("#16A34A"),
                VerticalAlignment = VerticalAlignment.Center
            };
            statusStack.Children.Add(statusBadge);
            statusStack.Children.Add(statusText);
            Grid.SetColumn(statusStack, 3);
            g.Children.Add(statusStack);

            card.Child = g;

            appRows.Add(new AppRowView()
            {
                App = curApp,
                Card = card,
                CheckBox = cb,
                StatusBadge = statusBadge,
                StatusIcon = statusIcon,
                StatusText = statusText
            });

            return card;
        }


        // =========================================================================
        // ONLINE SEARCH MODAL POPUP
        // =========================================================================
        private void OpenOnlineSearchModal(string initialQuery)
        {
            if (modalOnlineSearchOverlay == null) return;
            modalOnlineSearchOverlay.Visibility = Visibility.Visible;
            modalOnlineSelectedApps.Clear();
            UpdateOnlineModalSelectionUI();

            if (!string.IsNullOrWhiteSpace(initialQuery) &&
                initialQuery != "Search applications..." &&
                initialQuery != "find an app, setting or pack...")
            {
                if (txtOnlineModalSearchInput != null)
                {
                    txtOnlineModalSearchInput.Text = initialQuery;
                    txtOnlineModalSearchInput.Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
                }
                if (btnOnlineClearSearch != null) btnOnlineClearSearch.Visibility = Visibility.Visible;
                RunWingetOnlineSearchModal(initialQuery);
            }
            else
            {
                if (txtOnlineModalSearchInput != null)
                {
                    txtOnlineModalSearchInput.Text = "Type software name (e.g. Discord, Blender, Spotify, Zoom, Steam)...";
                    txtOnlineModalSearchInput.Foreground = HexBrush("#94A3B8");
                }
                if (btnOnlineClearSearch != null) btnOnlineClearSearch.Visibility = Visibility.Collapsed;
                ShowOnlineSearchEmptyState();
            }

            if (txtOnlineModalSearchInput != null) txtOnlineModalSearchInput.Focus();
        }

        private void CloseOnlineSearchModal()
        {
            if (modalOnlineSearchOverlay != null)
            {
                modalOnlineSearchOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void PopulateOnlineSearchSuggestionTags()
        {
            if (onlineSearchTagsPanel == null) return;
            onlineSearchTagsPanel.Children.Clear();

            string[] suggestions = new string[] { "Discord", "Zoom", "Steam", "Spotify", "Blender", "VS Code", "Git", "Node.js", "Docker", "Canva", "Postman", "7-Zip" };
            foreach (string item in suggestions)
            {
                string term = item;
                Button chip = new Button()
                {
                    Content = term,
                    Height = 26,
                    Padding = new Thickness(10, 0, 10, 0),
                    Margin = new Thickness(0, 2, 6, 2),
                    FontSize = 11,
                    FontWeight = FontWeights.SemiBold,
                    Background = currentThemeIsLight ? HexBrush("#F1F5F9") : HexBrush("#334155"),
                    Foreground = currentThemeIsLight ? HexBrush("#334155") : HexBrush("#E2E8F0"),
                    BorderBrush = currentThemeIsLight ? HexBrush("#E2E8F0") : HexBrush("#475569"),
                    BorderThickness = new Thickness(1),
                    Cursor = Cursors.Hand
                };
                chip.Click += delegate
                {
                    if (txtOnlineModalSearchInput != null)
                    {
                        txtOnlineModalSearchInput.Text = term;
                        txtOnlineModalSearchInput.Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
                        if (btnOnlineClearSearch != null) btnOnlineClearSearch.Visibility = Visibility.Visible;
                    }
                    RunWingetOnlineSearchModal(term);
                };
                onlineSearchTagsPanel.Children.Add(chip);
            }
        }

        private void ShowOnlineSearchEmptyState()
        {
            if (onlineModalResultsPanel == null) return;
            onlineModalResultsPanel.Children.Clear();

            if (txtOnlineModalStatus != null)
            {
                txtOnlineModalStatus.Text = "Enter any software name above and press Enter or click Search Online.";
            }

            Border emptyCard = new Border()
            {
                Background = currentThemeIsLight ? HexBrush("#F8FAFC") : HexBrush("#1E293B"),
                BorderBrush = currentThemeIsLight ? HexBrush("#E2E8F0") : HexBrush("#334155"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(30, 36, 30, 36),
                Margin = new Thickness(10, 16, 10, 16),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            StackPanel sp = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Center };

            Border iconCircle = new Border()
            {
                Width = 56,
                Height = 56,
                CornerRadius = new CornerRadius(28),
                Background = currentThemeIsLight ? HexBrush("#EDE9FE") : HexBrush("#2E1065"),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 14)
            };
            TextBlock txtIcon = new TextBlock()
            {
                Text = "🌐",
                FontSize = 26,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            iconCircle.Child = txtIcon;
            sp.Children.Add(iconCircle);

            TextBlock txtH = new TextBlock()
            {
                Text = "Search Microsoft Winget Cloud Catalog",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC"),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 6)
            };
            sp.Children.Add(txtH);

            TextBlock txtP = new TextBlock()
            {
                Text = "Type any application name above or click popular tags to discover software from Microsoft's repository.",
                FontSize = 12.5,
                Foreground = currentThemeIsLight ? HexBrush("#64748B") : HexBrush("#94A3B8"),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                MaxWidth = 520,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 16)
            };
            sp.Children.Add(txtP);

            WrapPanel quickWrap = new WrapPanel() { HorizontalAlignment = HorizontalAlignment.Center };
            string[] popular = new string[] { "Discord", "Zoom", "Steam", "Spotify", "Blender", "VS Code", "Git", "Postman" };
            foreach (string tag in popular)
            {
                string curTag = tag;
                Button b = new Button()
                {
                    Content = "🔍 " + curTag,
                    Height = 28,
                    Padding = new Thickness(12, 0, 12, 0),
                    Margin = new Thickness(4),
                    FontSize = 11.5,
                    FontWeight = FontWeights.SemiBold,
                    Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#0F172A"),
                    Foreground = HexBrush("#7C3AED"),
                    BorderBrush = HexBrush("#DDD6FE"),
                    BorderThickness = new Thickness(1),
                    Cursor = Cursors.Hand
                };
                b.Click += delegate
                {
                    if (txtOnlineModalSearchInput != null)
                    {
                        txtOnlineModalSearchInput.Text = curTag;
                        txtOnlineModalSearchInput.Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC");
                        if (btnOnlineClearSearch != null) btnOnlineClearSearch.Visibility = Visibility.Visible;
                    }
                    RunWingetOnlineSearchModal(curTag);
                };
                quickWrap.Children.Add(b);
            }
            sp.Children.Add(quickWrap);

            emptyCard.Child = sp;
            onlineModalResultsPanel.Children.Add(emptyCard);
        }

        private void RunWingetOnlineSearchModal(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query == "Type software name (e.g. Discord, Blender, Spotify, Zoom, Steam)...")
            {
                if (txtOnlineModalStatus != null) txtOnlineModalStatus.Text = "Please enter an app name (e.g. Discord, Spotify, Zoom, Steam).";
                return;
            }

            if (string.IsNullOrEmpty(wingetCmd))
            {
                if (txtOnlineModalStatus != null) txtOnlineModalStatus.Text = "Windows App Store (Winget) is not available on this system.";
                return;
            }

            if (txtOnlineModalStatus != null)
            {
                txtOnlineModalStatus.Text = string.Format("⏳ Searching Microsoft Winget cloud repository for '{0}'... Please wait.", query);
            }

            if (btnOnlineModalExecuteSearch != null) btnOnlineModalExecuteSearch.IsEnabled = false;
            if (txtOnlineSearchBtnText != null) txtOnlineSearchBtnText.Text = "Searching...";

            modalOnlineSelectedApps.Clear();
            UpdateOnlineModalSelectionUI();

            if (onlineModalResultsPanel != null)
            {
                onlineModalResultsPanel.Children.Clear();
                Border loadingCard = new Border()
                {
                    Background = currentThemeIsLight ? HexBrush("#F8FAFC") : HexBrush("#1E293B"),
                    BorderBrush = currentThemeIsLight ? HexBrush("#E2E8F0") : HexBrush("#334155"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(24, 30, 24, 30),
                    Margin = new Thickness(10, 20, 10, 20),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                StackPanel loadSp = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Center };
                TextBlock loadIcon = new TextBlock()
                {
                    Text = "⏳",
                    FontSize = 28,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 10)
                };
                TextBlock loadText = new TextBlock()
                {
                    Text = string.Format("Querying Microsoft Winget live cloud index for '{0}'...", query),
                    FontSize = 13.5,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC"),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                loadSp.Children.Add(loadIcon);
                loadSp.Children.Add(loadText);
                loadingCard.Child = loadSp;
                onlineModalResultsPanel.Children.Add(loadingCard);
            }

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
                                    description = string.IsNullOrEmpty(ver) ? "Package ID: " + id : "Version: " + ver + " • Package ID: " + id
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
                    if (btnOnlineModalExecuteSearch != null) btnOnlineModalExecuteSearch.IsEnabled = true;
                    if (txtOnlineSearchBtnText != null) txtOnlineSearchBtnText.Text = "Search Online";

                    if (onlineModalResultsPanel != null) onlineModalResultsPanel.Children.Clear();

                    if (errorMsg != null)
                    {
                        if (txtOnlineModalStatus != null) txtOnlineModalStatus.Text = "Search error: " + errorMsg;
                        return;
                    }

                    List<AppModel> matchingCustom = new List<AppModel>();
                    if (config != null && config.apps != null)
                    {
                        string qClean = (query ?? "").Replace(" ", "").ToLowerInvariant();
                        string qLower = (query ?? "").ToLowerInvariant();
                        matchingCustom = config.apps.FindAll(delegate(AppModel a)
                        {
                            if (a == null || !IsRecommendedApp(a)) return false;
                            string name = a.name ?? "";
                            string id = a.id ?? "";
                            string desc = a.description ?? "";
                            return name.IndexOf(qLower, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                   id.IndexOf(qLower, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                   desc.IndexOf(qLower, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                   name.Replace(" ", "").IndexOf(qClean, StringComparison.OrdinalIgnoreCase) >= 0;
                        });
                    }

                    int totalCount = foundApps.Count + matchingCustom.Count;
                    if (totalCount == 0)
                    {
                        if (txtOnlineModalStatus != null) txtOnlineModalStatus.Text = string.Format("No apps found matching '{0}'. Try searching with another name.", query);

                        Border noRes = new Border()
                        {
                            Background = currentThemeIsLight ? HexBrush("#F8FAFC") : HexBrush("#1E293B"),
                            BorderBrush = currentThemeIsLight ? HexBrush("#E2E8F0") : HexBrush("#334155"),
                            BorderThickness = new Thickness(1),
                            CornerRadius = new CornerRadius(10),
                            Padding = new Thickness(24, 30, 24, 30),
                            Margin = new Thickness(10, 20, 10, 20),
                            HorizontalAlignment = HorizontalAlignment.Center
                        };
                        StackPanel spNo = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Center };
                        TextBlock iconNo = new TextBlock() { Text = "🔍", FontSize = 28, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 8) };
                        TextBlock titleNo = new TextBlock() { Text = string.Format("No software packages found for \"{0}\"", query), FontSize = 14, FontWeight = FontWeights.Bold, Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC"), HorizontalAlignment = HorizontalAlignment.Center };
                        TextBlock descNo = new TextBlock() { Text = "Check spelling or search by publisher (e.g. 'Mozilla.Firefox' or 'Microsoft.VisualStudioCode').", FontSize = 12, Foreground = currentThemeIsLight ? HexBrush("#64748B") : HexBrush("#94A3B8"), HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 4, 0, 0) };
                        spNo.Children.Add(iconNo);
                        spNo.Children.Add(titleNo);
                        spNo.Children.Add(descNo);
                        noRes.Child = spNo;
                        if (onlineModalResultsPanel != null) onlineModalResultsPanel.Children.Add(noRes);
                        return;
                    }

                    if (txtOnlineModalStatus != null)
                    {
                        txtOnlineModalStatus.Text = string.Format("✓ Found {0} package(s) matching '{1}' from Microsoft Winget:", totalCount, query);
                    }

                    foreach (AppModel recApp in matchingCustom)
                    {
                        Border card = CreateOnlineModalAppCard(recApp);
                        if (onlineModalResultsPanel != null) onlineModalResultsPanel.Children.Add(card);
                    }

                    foreach (AppModel app in foundApps)
                    {
                        if (matchingCustom.Exists(delegate (AppModel m) { return string.Equals(m.id, app.id, StringComparison.OrdinalIgnoreCase); }))
                            continue;
                        AppModel curApp = app;
                        Border card = CreateOnlineModalAppCard(curApp);
                        if (onlineModalResultsPanel != null) onlineModalResultsPanel.Children.Add(card);
                    }
                }));
            });
        }

        private Border CreateOnlineModalAppCard(AppModel curApp)
        {
            Border card = new Border()
            {
                Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#1E293B"),
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 0, 0, 6),
                Padding = new Thickness(14, 10, 14, 10),
                BorderThickness = new Thickness(1),
                BorderBrush = currentThemeIsLight ? HexBrush("#E2E8F0") : HexBrush("#334155"),
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
                card.Background = isSel
                    ? (currentThemeIsLight ? HexBrush("#EFF6FF") : HexBrush("#1E3A8A"))
                    : (currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#1E293B"));
                card.BorderBrush = isSel
                    ? HexBrush("#3B82F6")
                    : (currentThemeIsLight ? HexBrush("#E2E8F0") : HexBrush("#334155"));
            };

            cb.Checked += delegate
            {
                if (!modalOnlineSelectedApps.Exists(delegate(AppModel a) { return a.id == curApp.id; }))
                {
                    modalOnlineSelectedApps.Add(curApp);
                }
                updateCardVisual();
                UpdateOnlineModalSelectionUI();
            };

            cb.Unchecked += delegate
            {
                modalOnlineSelectedApps.RemoveAll(delegate(AppModel a) { return a.id == curApp.id; });
                updateCardVisual();
                UpdateOnlineModalSelectionUI();
            };

            card.MouseLeftButtonUp += delegate (object sender, MouseButtonEventArgs e)
            {
                if (!e.Handled && !(e.OriginalSource is CheckBox) && !(e.OriginalSource is Button))
                {
                    cb.IsChecked = !(cb.IsChecked == true);
                }
            };

            Grid.SetColumn(cb, 0);
            g.Children.Add(cb);

            // Center Info
            StackPanel infoStack = new StackPanel() { VerticalAlignment = VerticalAlignment.Center };
            StackPanel titleStack = new StackPanel() { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            TextBlock txtName = new TextBlock()
            {
                Text = curApp.name,
                FontSize = 13.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#FFFFFF")
            };
            titleStack.Children.Add(txtName);

            bool isRec = IsRecommendedApp(curApp);
            Border badge = new Border()
            {
                Background = isRec ? (currentThemeIsLight ? HexBrush("#FEF3C7") : HexBrush("#78350F")) : (currentThemeIsLight ? HexBrush("#F3E8FF") : HexBrush("#3B0764")),
                CornerRadius = new CornerRadius(4),
                Margin = new Thickness(8, 0, 0, 0),
                Padding = new Thickness(6, 1, 6, 1),
                VerticalAlignment = VerticalAlignment.Center
            };
            TextBlock txtBadge = new TextBlock()
            {
                Text = isRec ? "TOP CHOICE • CLOUD" : "WINGET",
                FontSize = 9.5,
                FontWeight = FontWeights.Bold,
                Foreground = isRec ? HexBrush("#D97706") : (currentThemeIsLight ? HexBrush("#7C3AED") : HexBrush("#C084FC"))
            };
            badge.Child = txtBadge;
            titleStack.Children.Add(badge);
            infoStack.Children.Add(titleStack);

            TextBlock txtDesc = new TextBlock()
            {
                Text = curApp.description,
                FontSize = 11.5,
                Foreground = currentThemeIsLight ? HexBrush("#64748B") : HexBrush("#94A3B8"),
                Margin = new Thickness(0, 3, 0, 0),
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            infoStack.Children.Add(txtDesc);
            Grid.SetColumn(infoStack, 1);
            g.Children.Add(infoStack);

            // Action Buttons
            StackPanel actionStack = new StackPanel() { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };

            bool alreadyInCatalog = config.apps.Exists(delegate (AppModel a) { return a.id == curApp.id; });
            Button btnAddCatalog = new Button()
            {
                Content = alreadyInCatalog ? "✓ In Catalog" : "➕ Add to Catalog",
                IsEnabled = !alreadyInCatalog,
                Background = alreadyInCatalog ? (currentThemeIsLight ? HexBrush("#F1F5F9") : HexBrush("#334155")) : HexBrush("#0066FF"),
                Foreground = alreadyInCatalog ? (currentThemeIsLight ? HexBrush("#64748B") : HexBrush("#94A3B8")) : HexBrush("#FFFFFF"),
                FontWeight = FontWeights.SemiBold,
                Padding = new Thickness(12, 6, 12, 6),
                Margin = new Thickness(0, 0, 8, 0),
                FontSize = 11.5,
                Cursor = alreadyInCatalog ? Cursors.Arrow : Cursors.Hand,
                BorderThickness = new Thickness(0)
            };
            btnAddCatalog.Click += delegate
            {
                if (!config.apps.Exists(delegate (AppModel a) { return a.id == curApp.id; }))
                {
                    config.apps.Add(curApp);
                    SaveConfigToFile();
                    btnAddCatalog.Content = "✓ In Catalog";
                    btnAddCatalog.IsEnabled = false;
                    btnAddCatalog.Background = HexBrush("#16A34A");
                    btnAddCatalog.Foreground = HexBrush("#FFFFFF");
                    if (txtStatus != null) txtStatus.Text = string.Format("Added '{0}' to your catalog.", curApp.name);
                    BuildCatalog();
                }
            };
            actionStack.Children.Add(btnAddCatalog);

            Button btnInstallNow = new Button()
            {
                Content = "⚡ Install Now",
                Background = HexBrush("#059669"),
                Foreground = HexBrush("#FFFFFF"),
                FontWeight = FontWeights.SemiBold,
                Padding = new Thickness(12, 6, 12, 6),
                FontSize = 11.5,
                Cursor = Cursors.Hand,
                BorderThickness = new Thickness(0)
            };
            btnInstallNow.Click += delegate
            {
                MessageBoxResult res = MessageBox.Show(
                    string.Format("Do you want to install '{0}' now?", curApp.name),
                    "Install Software",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    if (!config.apps.Exists(delegate (AppModel a) { return a.id == curApp.id; }))
                    {
                        config.apps.Add(curApp);
                        SaveConfigToFile();
                        BuildCatalog();
                    }
                    CloseOnlineSearchModal();
                    InstallSingleApp(curApp);
                }
            };
            actionStack.Children.Add(btnInstallNow);

            Grid.SetColumn(actionStack, 2);
            g.Children.Add(actionStack);

            card.Child = g;
            return card;
        }

        private void UpdateOnlineModalSelectionUI()
        {
            int count = modalOnlineSelectedApps != null ? modalOnlineSelectedApps.Count : 0;
            if (txtOnlineModalSelectedCount != null)
            {
                txtOnlineModalSelectedCount.Text = count > 0 ? string.Format("{0} app(s) selected", count) : "0 apps selected";
            }
            if (btnOnlineAddSelected != null)
            {
                btnOnlineAddSelected.Visibility = count > 0 ? Visibility.Visible : Visibility.Collapsed;
                btnOnlineAddSelected.Content = string.Format("➕ Add Selected ({0}) to Catalog", count);
            }
            if (btnOnlineInstallSelected != null)
            {
                btnOnlineInstallSelected.Visibility = count > 0 ? Visibility.Visible : Visibility.Collapsed;
                btnOnlineInstallSelected.Content = string.Format("⚡ Install Selected ({0})", count);
            }
        }

        private void AddSelectedOnlineAppsToCatalog()
        {
            if (modalOnlineSelectedApps == null || modalOnlineSelectedApps.Count == 0) return;
            int addedCount = 0;
            foreach (AppModel a in modalOnlineSelectedApps)
            {
                if (a != null && !config.apps.Exists(delegate(AppModel x) { return x.id == a.id; }))
                {
                    config.apps.Add(a);
                    addedCount++;
                }
            }
            if (addedCount > 0)
            {
                SaveConfigToFile();
                BuildCatalog();
                MessageBox.Show(
                    string.Format("Successfully added {0} application(s) to your catalog!", addedCount),
                    "Apps Added",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("All selected applications are already in your catalog.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            modalOnlineSelectedApps.Clear();
            UpdateOnlineModalSelectionUI();
            if (onlineModalResultsPanel != null)
            {
                foreach (UIElement elem in onlineModalResultsPanel.Children)
                {
                    Border b = elem as Border;
                    if (b != null && b.Child is Grid)
                    {
                        Grid g = b.Child as Grid;
                        if (g != null && g.Children.Count >= 3)
                        {
                            StackPanel act = g.Children[2] as StackPanel;
                            if (act != null && act.Children.Count >= 1)
                            {
                                Button btn = act.Children[0] as Button;
                                if (btn != null)
                                {
                                    btn.Content = "✓ In Catalog";
                                    btn.IsEnabled = false;
                                    btn.Background = HexBrush("#16A34A");
                                    btn.Foreground = HexBrush("#FFFFFF");
                                }
                            }
                        }
                    }
                }
            }
        }

        private void InstallSelectedOnlineApps()
        {
            if (modalOnlineSelectedApps == null || modalOnlineSelectedApps.Count == 0) return;
            MessageBoxResult res = MessageBox.Show(
                string.Format("Ready to install {0} selected application(s) on your computer.\n\nDo you want to start now?", modalOnlineSelectedApps.Count),
                "Install Selected Applications",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (res != MessageBoxResult.Yes) return;

            List<string> idsToInstall = new List<string>();
            foreach (AppModel a in modalOnlineSelectedApps)
            {
                if (a == null) continue;
                if (!config.apps.Exists(delegate(AppModel x) { return x.id == a.id; }))
                {
                    config.apps.Add(a);
                }
                idsToInstall.Add(a.id);
                if (!selectedAppIds.Contains(a.id))
                {
                    selectedAppIds.Add(a.id);
                }
            }
            SaveConfigToFile();
            BuildCatalog();
            CloseOnlineSearchModal();
            StartInstallation(false, idsToInstall, null, "Online Winget Selection");
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
                Background = HexBrush("#0066FF"),
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
                    Background = currentThemeIsLight ? HexBrush("#FFFFFF") : HexBrush("#2A3547"),
                    Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC"),
                    BorderThickness = new Thickness(1),
                    BorderBrush = currentThemeIsLight ? HexBrush("#CBD5E1") : HexBrush("#475569"),
                    Cursor = Cursors.Hand,
                    FontSize = 11,
                    FontWeight = FontWeights.SemiBold
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
                Text = "Type any software name above and press Enter to search the entire cloud app directory.",
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
                                    description = string.IsNullOrEmpty(ver) ? "Cloud package (" + id + ")" : "Version: " + ver + " • Package ID: " + id
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

                    List<AppModel> matchingCustom = new List<AppModel>();
                    if (config != null && config.apps != null)
                    {
                        string qClean = (query ?? "").Replace(" ", "").ToLowerInvariant();
                        string qLower = (query ?? "").ToLowerInvariant();
                        matchingCustom = config.apps.FindAll(delegate(AppModel a)
                        {
                            if (a == null || !IsRecommendedApp(a)) return false;
                            string name = a.name ?? "";
                            string id = a.id ?? "";
                            string desc = a.description ?? "";
                                                        return name.IndexOf(qLower, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                   id.IndexOf(qLower, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                   desc.IndexOf(qLower, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                   name.Replace(" ", "").IndexOf(qClean, StringComparison.OrdinalIgnoreCase) >= 0;
                        });
                    }

                    int totalCount = foundApps.Count + matchingCustom.Count;
                    if (totalCount == 0)
                    {
                        txtOnlineStatus.Text = string.Format("No apps found matching '{0}'. Try searching with another name.", query);
                        return;
                    }

                    txtOnlineStatus.Text = string.Format("Found {0} app(s) matching '{1}':", totalCount, query);

                    foreach (AppModel recApp in matchingCustom)
                    {
                        Border card = CreateAppCard(recApp);
                        onlineResultsPanel.Children.Add(card);
                    }

                    foreach (AppModel app in foundApps)
                    {
                        if (matchingCustom.Exists(delegate (AppModel m) { return string.Equals(m.id, app.id, StringComparison.OrdinalIgnoreCase); }))
                            continue;
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
                        StatusBadge = new Border(),
                        StatusIcon = new TextBlock() { Text = "✔" },
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

            if (IsRecommendedApp(curApp))
            {
                Border recBadge = new Border()
                {
                    Background = currentThemeIsLight ? HexBrush("#FEF3C7") : HexBrush("#451A03"),
                    BorderBrush = currentThemeIsLight ? HexBrush("#FDE68A") : HexBrush("#78350F"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(4),
                    Margin = new Thickness(6, 0, 0, 0),
                    Padding = new Thickness(6, 1, 6, 1)
                };
                TextBlock txtRec = new TextBlock()
                {
                    Text = "⭐ RECOMMENDED",
                    FontSize = 9,
                    FontWeight = FontWeights.Bold,
                    Foreground = currentThemeIsLight ? HexBrush("#B45309") : HexBrush("#FBBF24")
                };
                recBadge.Child = txtRec;
                titleStack.Children.Add(recBadge);
            }


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
                Background = alreadyInCatalog ? (currentThemeIsLight ? HexBrush("#E2E8F0") : HexBrush("#242B38")) : HexBrush("#0066FF"),
                Foreground = HexBrush("#FFFFFF"),
                FontWeight = FontWeights.SemiBold,
                Padding = new Thickness(12, 6, 12, 6),
                Margin = new Thickness(0, 0, 6, 0),
                FontSize = 11.5,
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
                FontWeight = FontWeights.SemiBold,
                Padding = new Thickness(12, 6, 12, 6),
                FontSize = 11.5,
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
                MessageBox.Show("Package Delivery Engine is not available on this system.", "Notice", MessageBoxButton.OK, MessageBoxImage.Information);
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
                        AppendLog("Deployment finished with ExitCode: " + code);
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
                OpenOnlineSearchModal("");
                return;
            }
            localAppsPanel.Visibility = Visibility.Visible;
            if (wingetOnlinePanel != null) wingetOnlinePanel.Visibility = Visibility.Collapsed;
            if (borderCatalogHeaderBar != null) borderCatalogHeaderBar.Visibility = Visibility.Visible;
            ApplyFilters();
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
                        bool recA = IsRecommendedApp(a);
                        bool recB = IsRecommendedApp(b);
                        bool custA = IsCustomApp(a);
                        bool custB = IsCustomApp(b);
                        bool selA = a.id != null && selectedAppIds.Contains(a.id);
                        bool selB = b.id != null && selectedAppIds.Contains(b.id);

                        int rankA = (selA && recA) ? 0 : (selA && custA ? 1 : (selA ? 2 : (recA ? 3 : (custA ? 4 : 5))));
                        int rankB = (selB && recB) ? 0 : (selB && custB ? 1 : (selB ? 2 : (recB ? 3 : (custB ? 4 : 5))));
                        if (rankA != rankB) return rankA.CompareTo(rankB);

                        if (recA && recB)
                        {
                            int recIdxA = (a.id != null && recommendedMasterAppIds != null) ? recommendedMasterAppIds.FindIndex(delegate(string x) { return string.Equals(x, a.id, StringComparison.OrdinalIgnoreCase); }) : int.MaxValue;
                            int recIdxB = (b.id != null && recommendedMasterAppIds != null) ? recommendedMasterAppIds.FindIndex(delegate(string x) { return string.Equals(x, b.id, StringComparison.OrdinalIgnoreCase); }) : int.MaxValue;
                            if (recIdxA < 0) recIdxA = int.MaxValue;
                            if (recIdxB < 0) recIdxB = int.MaxValue;
                            if (recIdxA != recIdxB) return recIdxA.CompareTo(recIdxB);
                        }

                        if (custA && custB)
                        {
                            int custIdxA = (a.id != null && customMasterAppIds != null) ? customMasterAppIds.FindIndex(delegate(string x) { return string.Equals(x, a.id, StringComparison.OrdinalIgnoreCase); }) : int.MaxValue;
                            int custIdxB = (b.id != null && customMasterAppIds != null) ? customMasterAppIds.FindIndex(delegate(string x) { return string.Equals(x, b.id, StringComparison.OrdinalIgnoreCase); }) : int.MaxValue;
                            if (custIdxA < 0) custIdxA = int.MaxValue;
                            if (custIdxB < 0) custIdxB = int.MaxValue;
                            if (custIdxA != custIdxB) return custIdxA.CompareTo(custIdxB);
                        }

                        return string.Compare(a.name ?? "", b.name ?? "", StringComparison.OrdinalIgnoreCase);
                    });
                    break;

                case "alpha_asc":
                    activeFilteredApps.Sort(delegate (AppModel a, AppModel b)
                    {
                        bool recA = IsRecommendedApp(a);
                        bool recB = IsRecommendedApp(b);
                        bool custA = IsCustomApp(a);
                        bool custB = IsCustomApp(b);
                        bool selA = a.id != null && selectedAppIds.Contains(a.id);
                        bool selB = b.id != null && selectedAppIds.Contains(b.id);

                        int rankA = (selA && recA) ? 0 : (selA && custA ? 1 : (selA ? 2 : (recA ? 3 : (custA ? 4 : 5))));
                        int rankB = (selB && recB) ? 0 : (selB && custB ? 1 : (selB ? 2 : (recB ? 3 : (custB ? 4 : 5))));
                        if (rankA != rankB) return rankA.CompareTo(rankB);

                        if (recA && recB)
                        {
                            int recIdxA = (a.id != null && recommendedMasterAppIds != null) ? recommendedMasterAppIds.FindIndex(delegate(string x) { return string.Equals(x, a.id, StringComparison.OrdinalIgnoreCase); }) : int.MaxValue;
                            int recIdxB = (b.id != null && recommendedMasterAppIds != null) ? recommendedMasterAppIds.FindIndex(delegate(string x) { return string.Equals(x, b.id, StringComparison.OrdinalIgnoreCase); }) : int.MaxValue;
                            if (recIdxA < 0) recIdxA = int.MaxValue;
                            if (recIdxB < 0) recIdxB = int.MaxValue;
                            if (recIdxA != recIdxB) return recIdxA.CompareTo(recIdxB);
                        }

                        if (custA && custB)
                        {
                            int custIdxA = (a.id != null && customMasterAppIds != null) ? customMasterAppIds.FindIndex(delegate(string x) { return string.Equals(x, a.id, StringComparison.OrdinalIgnoreCase); }) : int.MaxValue;
                            int custIdxB = (b.id != null && customMasterAppIds != null) ? customMasterAppIds.FindIndex(delegate(string x) { return string.Equals(x, b.id, StringComparison.OrdinalIgnoreCase); }) : int.MaxValue;
                            if (custIdxA < 0) custIdxA = int.MaxValue;
                            if (custIdxB < 0) custIdxB = int.MaxValue;
                            if (custIdxA != custIdxB) return custIdxA.CompareTo(custIdxB);
                        }

                        return string.Compare(a.name ?? "", b.name ?? "", StringComparison.OrdinalIgnoreCase);
                    });
                    break;

                case "alpha_desc":
                    activeFilteredApps.Sort(delegate (AppModel a, AppModel b)
                    {
                        bool recA = IsRecommendedApp(a);
                        bool recB = IsRecommendedApp(b);
                        bool custA = IsCustomApp(a);
                        bool custB = IsCustomApp(b);
                        bool selA = a.id != null && selectedAppIds.Contains(a.id);
                        bool selB = b.id != null && selectedAppIds.Contains(b.id);

                        int rankA = (selA && recA) ? 0 : (selA && custA ? 1 : (selA ? 2 : (recA ? 3 : (custA ? 4 : 5))));
                        int rankB = (selB && recB) ? 0 : (selB && custB ? 1 : (selB ? 2 : (recB ? 3 : (custB ? 4 : 5))));
                        if (rankA != rankB) return rankA.CompareTo(rankB);

                        if (recA && recB)
                        {
                            int recIdxA = (a.id != null && recommendedMasterAppIds != null) ? recommendedMasterAppIds.FindIndex(delegate(string x) { return string.Equals(x, a.id, StringComparison.OrdinalIgnoreCase); }) : int.MaxValue;
                            int recIdxB = (b.id != null && recommendedMasterAppIds != null) ? recommendedMasterAppIds.FindIndex(delegate(string x) { return string.Equals(x, b.id, StringComparison.OrdinalIgnoreCase); }) : int.MaxValue;
                            if (recIdxA < 0) recIdxA = int.MaxValue;
                            if (recIdxB < 0) recIdxB = int.MaxValue;
                            if (recIdxA != recIdxB) return recIdxA.CompareTo(recIdxB);
                        }

                        if (custA && custB)
                        {
                            int custIdxA = (a.id != null && customMasterAppIds != null) ? customMasterAppIds.FindIndex(delegate(string x) { return string.Equals(x, a.id, StringComparison.OrdinalIgnoreCase); }) : int.MaxValue;
                            int custIdxB = (b.id != null && customMasterAppIds != null) ? customMasterAppIds.FindIndex(delegate(string x) { return string.Equals(x, b.id, StringComparison.OrdinalIgnoreCase); }) : int.MaxValue;
                            if (custIdxA < 0) custIdxA = int.MaxValue;
                            if (custIdxB < 0) custIdxB = int.MaxValue;
                            if (custIdxA != custIdxB) return custIdxA.CompareTo(custIdxB);
                        }

                        return string.Compare(b.name ?? "", a.name ?? "", StringComparison.OrdinalIgnoreCase);
                    });
                    break;

                case "category":
                    activeFilteredApps.Sort(delegate (AppModel a, AppModel b)
                    {
                        bool recA = IsRecommendedApp(a);
                        bool recB = IsRecommendedApp(b);
                        bool custA = IsCustomApp(a);
                        bool custB = IsCustomApp(b);
                        bool selA = a.id != null && selectedAppIds.Contains(a.id);
                        bool selB = b.id != null && selectedAppIds.Contains(b.id);

                        int rankA = (selA && recA) ? 0 : (selA && custA ? 1 : (selA ? 2 : (recA ? 3 : (custA ? 4 : 5))));
                        int rankB = (selB && recB) ? 0 : (selB && custB ? 1 : (selB ? 2 : (recB ? 3 : (custB ? 4 : 5))));
                        if (rankA != rankB) return rankA.CompareTo(rankB);

                        if (recA && recB)
                        {
                            int recIdxA = (a.id != null && recommendedMasterAppIds != null) ? recommendedMasterAppIds.FindIndex(delegate(string x) { return string.Equals(x, a.id, StringComparison.OrdinalIgnoreCase); }) : int.MaxValue;
                            int recIdxB = (b.id != null && recommendedMasterAppIds != null) ? recommendedMasterAppIds.FindIndex(delegate(string x) { return string.Equals(x, b.id, StringComparison.OrdinalIgnoreCase); }) : int.MaxValue;
                            if (recIdxA < 0) recIdxA = int.MaxValue;
                            if (recIdxB < 0) recIdxB = int.MaxValue;
                            if (recIdxA != recIdxB) return recIdxA.CompareTo(recIdxB);
                        }

                        if (custA && custB)
                        {
                            int custIdxA = (a.id != null && customMasterAppIds != null) ? customMasterAppIds.FindIndex(delegate(string x) { return string.Equals(x, a.id, StringComparison.OrdinalIgnoreCase); }) : int.MaxValue;
                            int custIdxB = (b.id != null && customMasterAppIds != null) ? customMasterAppIds.FindIndex(delegate(string x) { return string.Equals(x, b.id, StringComparison.OrdinalIgnoreCase); }) : int.MaxValue;
                            if (custIdxA < 0) custIdxA = int.MaxValue;
                            if (custIdxB < 0) custIdxB = int.MaxValue;
                            if (custIdxA != custIdxB) return custIdxA.CompareTo(custIdxB);
                        }

                        int catComp = string.Compare(a.category ?? "", b.category ?? "", StringComparison.OrdinalIgnoreCase);
                        if (catComp != 0) return catComp;
                        return string.Compare(a.name ?? "", b.name ?? "", StringComparison.OrdinalIgnoreCase);
                    });
                    break;

                case "popular":
                default:
                    EnsureAppDefaultIndices();
                    activeFilteredApps.Sort(delegate (AppModel a, AppModel b)
                    {
                        bool recA = IsRecommendedApp(a);
                        bool recB = IsRecommendedApp(b);
                        bool custA = IsCustomApp(a);
                        bool custB = IsCustomApp(b);
                        bool selA = a.id != null && selectedAppIds.Contains(a.id);
                        bool selB = b.id != null && selectedAppIds.Contains(b.id);

                        int rankA = (selA && recA) ? 0 : (selA && custA ? 1 : (selA ? 2 : (recA ? 3 : (custA ? 4 : 5))));
                        int rankB = (selB && recB) ? 0 : (selB && custB ? 1 : (selB ? 2 : (recB ? 3 : (custB ? 4 : 5))));
                        if (rankA != rankB) return rankA.CompareTo(rankB);

                        if (recA && recB)
                        {
                            int recIdxA = (a.id != null && recommendedMasterAppIds != null) ? recommendedMasterAppIds.FindIndex(delegate(string x) { return string.Equals(x, a.id, StringComparison.OrdinalIgnoreCase); }) : int.MaxValue;
                            int recIdxB = (b.id != null && recommendedMasterAppIds != null) ? recommendedMasterAppIds.FindIndex(delegate(string x) { return string.Equals(x, b.id, StringComparison.OrdinalIgnoreCase); }) : int.MaxValue;
                            if (recIdxA < 0) recIdxA = int.MaxValue;
                            if (recIdxB < 0) recIdxB = int.MaxValue;
                            if (recIdxA != recIdxB) return recIdxA.CompareTo(recIdxB);
                        }

                        if (custA && custB)
                        {
                            int custIdxA = (a.id != null && customMasterAppIds != null) ? customMasterAppIds.FindIndex(delegate(string x) { return string.Equals(x, a.id, StringComparison.OrdinalIgnoreCase); }) : int.MaxValue;
                            int custIdxB = (b.id != null && customMasterAppIds != null) ? customMasterAppIds.FindIndex(delegate(string x) { return string.Equals(x, b.id, StringComparison.OrdinalIgnoreCase); }) : int.MaxValue;
                            if (custIdxA < 0) custIdxA = int.MaxValue;
                            if (custIdxB < 0) custIdxB = int.MaxValue;
                            if (custIdxA != custIdxB) return custIdxA.CompareTo(custIdxB);
                        }

                        int idxA = (a.id != null && appDefaultIndices != null && appDefaultIndices.ContainsKey(a.id)) ? appDefaultIndices[a.id] : int.MaxValue;
                        int idxB = (b.id != null && appDefaultIndices != null && appDefaultIndices.ContainsKey(b.id)) ? appDefaultIndices[b.id] : int.MaxValue;
                        if (idxA != idxB) return idxA.CompareTo(idxB);

                        return string.Compare(a.name ?? "", b.name ?? "", StringComparison.OrdinalIgnoreCase);
                    });
                    break;
            }
        }

        private static bool MatchesSearchQuery(AppModel a, string lowerQuery, string cleanQuery)
        {
            if (a == null) return false;
            string aName = a.name ?? "";
            string aId = a.id ?? "";
            string aDesc = a.description ?? "";
            return aName.IndexOf(lowerQuery, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   aId.IndexOf(lowerQuery, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   aDesc.IndexOf(lowerQuery, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   aName.Replace(" ", "").IndexOf(cleanQuery, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   aId.Replace(" ", "").IndexOf(cleanQuery, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void ApplyFilters()
        {
            if (activeCategory == "winget_online") return;

            string rawQuery = txtSearch != null ? txtSearch.Text.Trim() : "";
            if (IsSearchPlaceholder(rawQuery))
            {
                rawQuery = "";
            }

            int searchToken = Interlocked.Increment(ref currentOnlineSearchToken);
            isShowingOnlineResults = false;

            string cleanQuery = rawQuery.Replace(" ", "").ToLowerInvariant();
            string lowerQuery = rawQuery.ToLowerInvariant();

            activeFilteredApps = config.apps.FindAll(delegate (AppModel a)
            {
                if (a == null) return false;
                bool isRec = IsRecommendedApp(a);
                bool isCust = IsCustomApp(a);
                bool matchCat = MatchesCategory(a, activeCategory);

                if (!string.IsNullOrEmpty(lowerQuery))
                {
                    if (!MatchesSearchQuery(a, lowerQuery, cleanQuery)) return false;
                    if (isRec || isCust) return true;
                    return matchCat;
                }
                else
                {
                    return matchCat;
                }
            });

            // If 0 matches in active category, but user typed a search query, search all apps across the database
            if (activeFilteredApps.Count == 0 && !string.IsNullOrEmpty(lowerQuery) && activeCategory != "all" && !string.IsNullOrEmpty(activeCategory))
            {
                activeFilteredApps = config.apps.FindAll(delegate (AppModel a)
                {
                    return MatchesSearchQuery(a, lowerQuery, cleanQuery);
                });
            }

            isStreamingAllApps = false;
            currentRenderedCount = 0;
            appRows.Clear();
            if (localCardsContainer != null) localCardsContainer.Children.Clear();

            // Scenario 1: Local results found in local database!
            if (activeFilteredApps.Count > 0)
            {
                isOnlineSearching = false;
                SortFilteredApps();
                UpdateMasterCheckboxState();
                UpdateHeaderCount();
                RenderNextBatch(250);

                // If user entered a search query (>= 2 chars), automatically query online Winget repository too and display results at the end!
                if (!string.IsNullOrEmpty(rawQuery) && rawQuery.Length >= 2 && !string.IsNullOrEmpty(wingetCmd))
                {
                    ExecuteOnlineWingetSearch(rawQuery, searchToken, true);
                }
                return;
            }

            // Scenario 2: Empty search query (user didn't type anything, viewing empty category)
            if (string.IsNullOrEmpty(lowerQuery))
            {
                isOnlineSearching = false;
                UpdateMasterCheckboxState();
                UpdateHeaderCount();
                if (catalogPaginationPanel != null) catalogPaginationPanel.Visibility = Visibility.Collapsed;
                return;
            }

            // Scenario 3: Query is very short (1 character)
            if (rawQuery.Length < 2)
            {
                isOnlineSearching = false;
                UpdateMasterCheckboxState();
                if (txtCatalogListHeaderCount != null) txtCatalogListHeaderCount.Text = "Type at least 2 characters to search apps...";
                if (catalogPaginationPanel != null) catalogPaginationPanel.Visibility = Visibility.Collapsed;

                Border tipCard = new Border()
                {
                    Background = currentThemeIsLight ? HexBrush("#F8FAFC") : HexBrush("#1E293B"),
                    BorderBrush = currentThemeIsLight ? HexBrush("#E2E8F0") : HexBrush("#334155"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(24, 28, 24, 28),
                    Margin = new Thickness(0, 10, 0, 10),
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                StackPanel sp = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Center };
                TextBlock icon = new TextBlock() { Text = "⌨️", FontSize = 24, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 8) };
                TextBlock txt = new TextBlock() { Text = "Please type at least 2 characters to search applications.", FontSize = 13, FontWeight = FontWeights.SemiBold, Foreground = currentThemeIsLight ? HexBrush("#475569") : HexBrush("#94A3B8"), HorizontalAlignment = HorizontalAlignment.Center };
                sp.Children.Add(icon);
                sp.Children.Add(txt);
                tipCard.Child = sp;
                localCardsContainer.Children.Add(tipCard);
                return;
            }

            // Scenario 4: Query >= 2 characters and not in local database -> Automatically search online on the same table!
            ExecuteOnlineWingetSearch(rawQuery, searchToken, false);
        }

        private void ExecuteOnlineWingetSearch(string queryToExecute, int searchToken, bool hasLocalMatches)
        {
            if (string.IsNullOrEmpty(wingetCmd))
            {
                if (!hasLocalMatches)
                {
                    isOnlineSearching = false;
                    UpdateMasterCheckboxState();
                    if (txtCatalogListHeaderCount != null) txtCatalogListHeaderCount.Text = string.Format("0 apps found for \"{0}\"", queryToExecute);
                    if (catalogPaginationPanel != null) catalogPaginationPanel.Visibility = Visibility.Collapsed;

                    Border noWingetCard = new Border()
                    {
                        Background = currentThemeIsLight ? HexBrush("#F8FAFC") : HexBrush("#1E293B"),
                        BorderBrush = currentThemeIsLight ? HexBrush("#E2E8F0") : HexBrush("#334155"),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(10),
                        Padding = new Thickness(24, 28, 24, 28),
                        Margin = new Thickness(0, 10, 0, 10),
                        HorizontalAlignment = HorizontalAlignment.Stretch
                    };
                    StackPanel sp = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Center };
                    TextBlock icon = new TextBlock() { Text = "🔍", FontSize = 26, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 8) };
                    TextBlock title = new TextBlock() { Text = string.Format("No local applications found for \"{0}\"", queryToExecute), FontSize = 13.5, FontWeight = FontWeights.Bold, Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC"), HorizontalAlignment = HorizontalAlignment.Center };
                    TextBlock sub = new TextBlock() { Text = "Microsoft Winget is not available on this system to search online repository.", FontSize = 12, Foreground = HexBrush("#64748B"), HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 4, 0, 0) };
                    sp.Children.Add(icon);
                    sp.Children.Add(title);
                    sp.Children.Add(sub);
                    noWingetCard.Child = sp;
                    localCardsContainer.Children.Add(noWingetCard);
                }
                return;
            }

            isOnlineSearching = true;

            if (hasLocalMatches)
            {
                Border inlineLoadingCard = new Border()
                {
                    Tag = "online_search_loading_tag",
                    Background = currentThemeIsLight ? HexBrush("#F8FAFC") : HexBrush("#1E293B"),
                    BorderBrush = currentThemeIsLight ? HexBrush("#E2E8F0") : HexBrush("#334155"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(14, 10, 14, 10),
                    Margin = new Thickness(0, 10, 0, 10),
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                StackPanel spLoad = new StackPanel() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
                TextBlock iconLoad = new TextBlock() { Text = "⏳", FontSize = 14, Margin = new Thickness(0, 0, 8, 0), VerticalAlignment = VerticalAlignment.Center };
                TextBlock txtLoad = new TextBlock()
                {
                    Text = string.Format("Querying Microsoft Winget online repository for additional \"{0}\" packages...", queryToExecute),
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = currentThemeIsLight ? HexBrush("#64748B") : HexBrush("#94A3B8"),
                    VerticalAlignment = VerticalAlignment.Center
                };
                spLoad.Children.Add(iconLoad);
                spLoad.Children.Add(txtLoad);
                inlineLoadingCard.Child = spLoad;
                localCardsContainer.Children.Add(inlineLoadingCard);
            }
            else
            {
                UpdateMasterCheckboxState();
                if (txtCatalogListHeaderCount != null)
                {
                    txtCatalogListHeaderCount.Text = string.Format("🔍 Searching online Winget repository for \"{0}\"...", queryToExecute);
                }
                if (catalogPaginationPanel != null) catalogPaginationPanel.Visibility = Visibility.Collapsed;

                localCardsContainer.Children.Clear();
                appRows.Clear();

                Border loadingCard = new Border()
                {
                    Background = currentThemeIsLight ? HexBrush("#F8FAFC") : HexBrush("#1E293B"),
                    BorderBrush = currentThemeIsLight ? HexBrush("#E2E8F0") : HexBrush("#334155"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(24, 28, 24, 28),
                    Margin = new Thickness(0, 10, 0, 10),
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                StackPanel loadSp = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Center };
                TextBlock loadIcon = new TextBlock()
                {
                    Text = "⏳",
                    FontSize = 26,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 8)
                };
                TextBlock loadTitle = new TextBlock()
                {
                    Text = string.Format("Not found in local database. Searching online Winget repository for \"{0}\"...", queryToExecute),
                    FontSize = 13.5,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC"),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                TextBlock loadSub = new TextBlock()
                {
                    Text = "Querying Microsoft Winget live cloud index for matching packages...",
                    FontSize = 12,
                    Foreground = HexBrush("#64748B"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 4, 0, 0)
                };
                loadSp.Children.Add(loadIcon);
                loadSp.Children.Add(loadTitle);
                loadSp.Children.Add(loadSub);
                loadingCard.Child = loadSp;
                localCardsContainer.Children.Add(loadingCard);
            }

            ThreadPool.QueueUserWorkItem(delegate
            {
                List<AppModel> foundApps = new List<AppModel>();
                string errorMsg = null;
                try
                {
                    string args = string.Format("search -q \"{0}\" -n 50 --accept-source-agreements", queryToExecute);
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
                                string appName = string.IsNullOrEmpty(name) ? id : name;
                                foundApps.Add(new AppModel()
                                {
                                    id = id,
                                    name = appName,
                                    category = InferAppCategory(appName, id),
                                    description = string.IsNullOrEmpty(ver) ? "Package ID: " + id : "Version: " + ver + " • Package ID: " + id
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
                    if (searchToken != currentOnlineSearchToken) return;
                    string currentInput = txtSearch != null ? txtSearch.Text.Trim() : "";
                    if (currentInput.Equals("find an app, setting or pack...", StringComparison.OrdinalIgnoreCase) ||
                        currentInput.Equals("search applications...", StringComparison.OrdinalIgnoreCase))
                    {
                        currentInput = "";
                    }
                    if (!string.Equals(currentInput, queryToExecute, StringComparison.OrdinalIgnoreCase)) return;

                    isOnlineSearching = false;

                    if (hasLocalMatches)
                    {
                        for (int i = localCardsContainer.Children.Count - 1; i >= 0; i--)
                        {
                            FrameworkElement fe = localCardsContainer.Children[i] as FrameworkElement;
                            if (fe != null && fe.Tag as string == "online_search_loading_tag")
                            {
                                localCardsContainer.Children.RemoveAt(i);
                            }
                        }

                        List<AppModel> newOnlineApps = new List<AppModel>();
                        foreach (AppModel app in foundApps)
                        {
                            bool alreadyExists = activeFilteredApps.Exists(delegate(AppModel x)
                            {
                                return string.Equals(x.id, app.id, StringComparison.OrdinalIgnoreCase) ||
                                       string.Equals(x.name, app.name, StringComparison.OrdinalIgnoreCase);
                            });
                            if (!alreadyExists)
                            {
                                newOnlineApps.Add(app);
                            }
                        }

                        if (newOnlineApps.Count > 0)
                        {
                            foreach (AppModel app in newOnlineApps)
                            {
                                if (!knownOnlineApps.ContainsKey(app.id)) knownOnlineApps[app.id] = app;
                                if (!config.apps.Exists(delegate(AppModel x) { return string.Equals(x.id, app.id, StringComparison.OrdinalIgnoreCase); }))
                                {
                                    config.apps.Add(app);
                                }
                                activeFilteredApps.Add(app);
                                currentRenderedCount++;
                                Border card = CreateAppCard(app);
                                localCardsContainer.Children.Add(card);
                            }

                            UpdateHeaderCount();
                            UpdateMasterCheckboxState();
                            if (catalogPaginationPanel != null) catalogPaginationPanel.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        localCardsContainer.Children.Clear();
                        appRows.Clear();

                        if (errorMsg != null || foundApps.Count == 0)
                        {
                            Border noRes = new Border()
                            {
                                Background = currentThemeIsLight ? HexBrush("#F8FAFC") : HexBrush("#1E293B"),
                                BorderBrush = currentThemeIsLight ? HexBrush("#E2E8F0") : HexBrush("#334155"),
                                BorderThickness = new Thickness(1),
                                CornerRadius = new CornerRadius(10),
                                Padding = new Thickness(24, 30, 24, 30),
                                Margin = new Thickness(0, 10, 0, 10),
                                HorizontalAlignment = HorizontalAlignment.Stretch
                            };
                            StackPanel spNo = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Center };
                            TextBlock iconNo = new TextBlock() { Text = "🔍", FontSize = 28, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 8) };
                            TextBlock titleNo = new TextBlock() { Text = string.Format("No software packages found for \"{0}\"", queryToExecute), FontSize = 14, FontWeight = FontWeights.Bold, Foreground = currentThemeIsLight ? HexBrush("#0F172A") : HexBrush("#F8FAFC"), HorizontalAlignment = HorizontalAlignment.Center };
                            TextBlock descNo = new TextBlock() { Text = errorMsg != null ? ("Search error: " + errorMsg) : "Not found in local database or online Winget repository. Check spelling or try a publisher name (e.g. 'Mozilla.Firefox').", FontSize = 12, Foreground = currentThemeIsLight ? HexBrush("#64748B") : HexBrush("#94A3B8"), HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 4, 0, 0) };
                            spNo.Children.Add(iconNo);
                            spNo.Children.Add(titleNo);
                            spNo.Children.Add(descNo);
                            noRes.Child = spNo;
                            localCardsContainer.Children.Add(noRes);

                            if (txtCatalogListHeaderCount != null)
                            {
                                txtCatalogListHeaderCount.Text = string.Format("0 apps found for \"{0}\"", queryToExecute);
                            }
                            UpdateMasterCheckboxState();
                            return;
                        }

                        // Register found apps into knownOnlineApps & in-memory config.apps so they are fully recognized
                        foreach (AppModel app in foundApps)
                        {
                            if (!knownOnlineApps.ContainsKey(app.id)) knownOnlineApps[app.id] = app;
                            if (!config.apps.Exists(delegate(AppModel x) { return string.Equals(x.id, app.id, StringComparison.OrdinalIgnoreCase); }))
                            {
                                config.apps.Add(app);
                            }
                        }

                        isShowingOnlineResults = true;
                        activeFilteredApps = foundApps;
                        currentRenderedCount = 0;
                        RenderNextBatch(250);
                        UpdateMasterCheckboxState();
                        UpdateHeaderCount();
                    }
                }));
            });
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
                string rawQ = txtSearch != null ? txtSearch.Text.Trim() : "";
                if (IsSearchPlaceholder(rawQ))
                {
                    rawQ = "";
                }

                if (!string.IsNullOrEmpty(rawQ) && rawQ.Length >= 2)
                {
                    int onlineCount = 0;
                    for (int i = 0; i < activeFilteredApps.Count; i++)
                    {
                        if (activeFilteredApps[i] != null && knownOnlineApps.ContainsKey(activeFilteredApps[i].id))
                        {
                            onlineCount++;
                        }
                    }
                    int localCount = activeFilteredApps.Count - onlineCount;

                    if (onlineCount > 0 && localCount > 0)
                    {
                        if (selectedInFilter > 0)
                            txtCatalogListHeaderCount.Text = Localization.T("showing_local_online_selected", localCount, onlineCount, rawQ, selectedInFilter);
                        else
                            txtCatalogListHeaderCount.Text = Localization.T("showing_local_online", localCount, onlineCount, rawQ);
                        return;
                    }
                    else if (onlineCount > 0 && localCount == 0)
                    {
                        if (selectedInFilter > 0)
                            txtCatalogListHeaderCount.Text = Localization.T("found_online_count_selected", onlineCount, rawQ, selectedInFilter);
                        else
                            txtCatalogListHeaderCount.Text = Localization.T("found_online_count", onlineCount, rawQ);
                        return;
                    }
                    else
                    {
                        if (selectedInFilter > 0)
                            txtCatalogListHeaderCount.Text = Localization.T("showing_query_count_selected", activeFilteredApps.Count, rawQ, selectedInFilter);
                        else
                            txtCatalogListHeaderCount.Text = Localization.T("showing_query_count", activeFilteredApps.Count, rawQ);
                        return;
                    }
                }

                if (selectedInFilter > 0)
                {
                    txtCatalogListHeaderCount.Text = Localization.T("showing_count_selected", currentRenderedCount, activeFilteredApps.Count, selectedInFilter);
                }
                else
                {
                    txtCatalogListHeaderCount.Text = Localization.T("showing_count", currentRenderedCount, activeFilteredApps.Count);
                }
            }
        }

        private void UpdateCount()
        {
            if (txtCount != null) txtCount.Text = string.Format("Selected: {0:N0} of {1:N0} apps", selectedAppIds.Count, config.apps.Count);
            if (btnInstall != null)
            {
                bool isSetupView = (currentView == "setup" || currentView == "roles");
                if (isSetupView)
                {
                    btnInstall.Visibility = Visibility.Collapsed;
                }
                else
                {
                    if (selectedAppIds.Count > 0)
                    {
                        btnInstall.Content = Localization.T("btn_install", selectedAppIds.Count);
                    }
                    else
                    {
                        btnInstall.Content = Localization.T("btn_install_no_count");
                    }
                    btnInstall.Background = HexBrush("#0066FF");
                    btnInstall.Foreground = HexBrush("#FFFFFF");
                }
            }
            if (btnHeroInstall != null)
            {
                if (selectedAppIds.Count > 0)
                {
                    btnHeroInstall.Content = Localization.T("btn_install", selectedAppIds.Count);
                }
                else
                {
                    btnHeroInstall.Content = Localization.T("btn_install_no_count");
                }
                btnHeroInstall.Background = HexBrush("#0066FF");
                btnHeroInstall.Foreground = HexBrush("#FFFFFF");
            }
            if (txtHeroStatusSubtitle != null)
            {
                GroupModel act = config != null && config.groups != null ? config.groups.Find(delegate(GroupModel g) { return g.id == activeRoleId; }) : null;
                string packName = act != null ? act.name : "Custom Selection";
                int packAppCount = (act != null && act.apps != null) ? act.apps.Count : 0;
                txtHeroStatusSubtitle.Text = string.Format("{0} apps selected from \"{1}\". Click Install to proceed.", packAppCount, packName);
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

            // If message represents an error, failure, exception, or stderr, also record into error-log.txt
            if (!string.IsNullOrEmpty(message) &&
                (message.IndexOf("ERROR", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 message.IndexOf("[STDERR]", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 message.IndexOf("Failed", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 message.IndexOf("Timeout", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 message.IndexOf("Exception", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                try
                {
                    if (!string.IsNullOrEmpty(errorLogPath))
                    {
                        File.AppendAllText(errorLogPath, string.Format("[{0}] {1}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), message));
                    }
                }
                catch { }
            }
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
            StartInstallation(false, null, null, null);
        }

        private void StartInstallation(bool isResume)
        {
            StartInstallation(isResume, null, null, null);
        }

        private void StartInstallation(bool isResume, List<string> packAppIds, string overrideRoleId = null, string overrideRoleName = null)
        {
            List<AppModel> selectedApps = new List<AppModel>();
            HashSet<string> targetIdSet = (packAppIds != null) ? new HashSet<string>(packAppIds) : selectedAppIds;
            if (config != null && config.apps != null)
            {
                foreach (AppModel a in config.apps)
                {
                    if (targetIdSet.Contains(a.id)) selectedApps.Add(a);
                }
            }

            if (knownOnlineApps != null)
            {
                foreach (string id in targetIdSet)
                {
                    if (!selectedApps.Exists(delegate(AppModel x) { return string.Equals(x.id, id, StringComparison.OrdinalIgnoreCase); }))
                    {
                        if (knownOnlineApps.ContainsKey(id))
                        {
                            AppModel onlineApp = knownOnlineApps[id];
                            selectedApps.Add(onlineApp);
                            if (config != null && config.apps != null && !config.apps.Exists(delegate(AppModel x) { return string.Equals(x.id, id, StringComparison.OrdinalIgnoreCase); }))
                            {
                                config.apps.Add(onlineApp);
                            }
                        }
                    }
                }
            }

            if (selectedApps.Count == 0)
            {
                MessageBox.Show("Please select an app pack or check at least one app to install.", "No Apps Selected", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!isResume)
            {
                string confirmMsg = !string.IsNullOrEmpty(overrideRoleName)
                    ? string.Format("Ready to install {0} app(s) from \"{1}\" on your computer.\n\nDo you want to start now?", selectedApps.Count, overrideRoleName)
                    : string.Format("Ready to install {0} selected app(s) on your computer.\n\nDo you want to start now?", selectedApps.Count);

                MessageBoxResult res = MessageBox.Show(
                    confirmMsg,
                    "Install Apps",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (res != MessageBoxResult.Yes) return;
            }

            previousViewBeforeInstall = currentView;
            SwitchView("logs");

            isCancelled = false;
            currentProc = null;
            isInstallRunning = true;
            currentProcessingAppTotal = selectedApps.Count;
            currentProcessingAppIndex = 0;
            currentProcessingPercent = 0;
            UpdateToastProgress("Starting Installation", string.Format("0 of {0} apps completed", selectedApps.Count), 0);

            btnInstall.Visibility = Visibility.Collapsed;
            btnStop.Visibility = Visibility.Visible;
            btnStop.IsEnabled = true;
            btnStop.Content = "⏹️ Stop Install";
            btnStop.Background = HexBrush("#DC2626");
            btnStop.Foreground = HexBrush("#FFFFFF");
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
                currentSession.roleId = overrideRoleId ?? (activeRoleId ?? "custom");
                currentSession.roleName = overrideRoleName ?? ((activeRoleId != null && config != null && config.groups != null) ?
                    (config.groups.Find(delegate(GroupModel g) { return g.id == activeRoleId; }) != null ?
                     config.groups.Find(delegate(GroupModel g) { return g.id == activeRoleId; }).name : activeRoleId) : "Custom Workstation");
                currentSession.totalCount = selectedApps.Count;
                foreach (AppModel a in selectedApps)
                {
                    AppDeploymentRecord rec = new AppDeploymentRecord();
                    rec.appId = a.id;
                    rec.appName = a.name;
                    rec.category = a.category;
                    rec.installMethod = !string.IsNullOrEmpty(a.downloadUrl) ? "Direct Cloud Delivery" : "FleetForge Engine";
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
                int downloadCount = 0;

                for (int i = 0; i < selectedApps.Count; i++)
                {
                    if (isCancelled)
                    {
                        AppendLog("[STOPPED] Installation queue stopped by user.");
                        break;
                    }

                    AppModel app = selectedApps[i];
                    currentProcessingAppIndex = i + 1;
                    currentProcessingAppName = app.name;
                    int appStartPercent = (int)(((double)i / selectedApps.Count) * 100);
                    currentProcessingPercent = appStartPercent;
                    currentProcessingAppStatus = "Preparing installer...";
                    UpdateToastProgress(
                        string.Format("[{0}/{1}] Installing {2}", i + 1, selectedApps.Count, app.name),
                        "Preparing installer...",
                        appStartPercent);
                    AppRowView item = appRows.Find(delegate(AppRowView r) { return r != null && r.App != null && r.App.id == app.id; });

                    Dispatcher.Invoke(new Action(delegate
                    {
                        if (item != null)
                        {
                            if (item.StatusBadge != null) item.StatusBadge.Background = HexBrush("#0078D4");
                            item.StatusIcon.Text = "⏳";
                            item.StatusText.Text = "Installing...";
                            item.StatusText.Foreground = HexBrush("#0078D4");
                        }
                        if (txtStatus != null) txtStatus.Text = string.Format("[{0}/{1}] Installing {2}...", i + 1, selectedApps.Count, app.name);
                    }));

                    AppendLog(string.Format("[{0}/{1}] Processing: {2} ({3})", i + 1, selectedApps.Count, app.name, app.id));

                    if (!string.IsNullOrEmpty(app.affiliateWebUrl))
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo(app.affiliateWebUrl) { UseShellExecute = true });
                            AppendLog(string.Format("🌐 Partner discount/activation page opened in browser: {0}", app.affiliateWebUrl));
                        }
                        catch { }
                    }

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
                    if (!isCancelled && rec.status != "Timeout" && !string.IsNullOrEmpty(app.downloadUrl))
                    {
                        AppendLog("Downloading setup package for: " + app.name + "...");
                        UpdateToastProgress(
                            string.Format("[{0}/{1}] Downloading {2}", i + 1, selectedApps.Count, app.name),
                            "Downloading setup package from Cloud Delivery...",
                            appStartPercent);
                        rec.installMethod = "Direct Cloud Delivery";
                        string targetExeName = Regex.Replace(app.name, @"[^\w]", "_") + (app.downloadUrl.EndsWith(".msi", StringComparison.OrdinalIgnoreCase) ? ".msi" : ".exe");

                        string downloadDir = Path.Combine(Path.GetTempPath(), "FleetForgeDownloads");
                        try { if (!Directory.Exists(downloadDir)) Directory.CreateDirectory(downloadDir); } catch { }
                        string downloadTarget = Path.Combine(downloadDir, targetExeName);

                        try
                        {
                            string directUrl = MainWindow.ConvertDirectUrl(app.downloadUrl);
                            using (WebClient wc = new WebClient())
                            {
                                wc.Encoding = Encoding.UTF8;
                                wc.Headers.Add("User-Agent", "FleetForge-Delivery-Engine/2.7.0");
                                wc.DownloadFile(directUrl, downloadTarget);
                            }

                            if (File.Exists(downloadTarget) && new FileInfo(downloadTarget).Length > 0)
                            {
                                downloadCount++;
                                long kb = new FileInfo(downloadTarget).Length / 1024;
                                AppendLog(string.Format("Downloaded {0} ({1:N0} KB). Starting silent background installation...", app.name, kb));
                                UpdateToastProgress(
                                    string.Format("[{0}/{1}] Installing {2}", i + 1, selectedApps.Count, app.name),
                                    "Running silent background installer...",
                                    appStartPercent);

                                ProcessStartInfo psi;
                                bool isMsi = downloadTarget.EndsWith(".msi", StringComparison.OrdinalIgnoreCase);
                                if (isMsi)
                                {
                                    string msiArgs = string.Format("/i \"{0}\" /qn /norestart {1}", downloadTarget, app.silentArgs ?? "").Trim();
                                    psi = new ProcessStartInfo("msiexec.exe", msiArgs);
                                }
                                else
                                {
                                    string exeArgs = !string.IsNullOrEmpty(app.silentArgs) ? app.silentArgs : "/S /silent /quiet /VERYSILENT /NORESTART";
                                    psi = new ProcessStartInfo(downloadTarget, exeArgs);
                                }

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
                                            AppendLog(string.Format("[WATCHDOG TIMEOUT] Setup for {0} exceeded {1} minute limit. Terminated.", app.name, watchdogTimeoutMinutes));
                                            break;
                                        }
                                        Thread.Sleep(500);
                                    }

                                    rec.durationSeconds = (DateTime.Now - startTime).TotalSeconds;

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
                                            AppendLog(app.name + " installed successfully via Direct Cloud Delivery.");
                                        }
                                        else
                                        {
                                            rec.status = "Failed";
                                            rec.errorDetails = "Exit code: " + p.ExitCode;
                                            installed = false;
                                            AppendLog(string.Format("Setup for {0} exited with code {1}.", app.name, p.ExitCode));
                                        }
                                    }
                                }
                            }
                            else
                            {
                                rec.status = "Failed";
                                rec.errorDetails = "Downloaded installer file is empty or missing";
                                AppendLog("ERROR: Downloaded setup file for " + app.name + " is invalid.");
                            }
                        }
                        catch (Exception ex)
                        {
                            rec.status = "Failed";
                            rec.errorDetails = "Direct download/install error: " + ex.Message;
                            AppendLog("ERROR: Direct download failed for " + app.name + ": " + ex.Message);
                        }
                        finally
                        {
                            currentProc = null;
                            try { if (File.Exists(downloadTarget)) File.Delete(downloadTarget); } catch { }
                        }
                    }

                    if (!installed && !isCancelled && rec.status != "Timeout")
                    {
                        if (string.IsNullOrEmpty(wingetCmd))
                        {
                            rec.status = "Failed";
                            rec.errorDetails = "Package engine not available";
                            AppendLog("ERROR: Package Delivery Engine not available.");
                            installed = false;
                        }
                        else
                        {
                            AppendLog("Deploying package: " + app.name + " (" + app.id + ")...");
                            UpdateToastProgress(
                                string.Format("[{0}/{1}] Installing {2}", i + 1, selectedApps.Count, app.name),
                                "Deploying via Package Delivery Engine...",
                                appStartPercent);
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
                                            AppendLog(string.Format("[WATCHDOG TIMEOUT] Installation for {0} exceeded {1} minute limit. Terminated.", app.name, watchdogTimeoutMinutes));
                                            break;
                                        }

                                        string line = proc.StandardOutput.ReadLine();
                                        if (!string.IsNullOrEmpty(line)) AppendLog("  > " + line);
                                    }

                                    rec.durationSeconds = (DateTime.Now - startTime).TotalSeconds;
                                    rec.installMethod = "FleetForge Engine";

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
                                        AppendLog("Deployment finished with ExitCode: " + code);

                                        uint uCode = unchecked((uint)code);
                                        if (code == 0 || code == 3010 || code == -1978335189 || uCode == 2316632107U)
                                        {
                                            rec.status = "Installed";
                                            installed = true;
                                            downloadCount++;
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
                                AppendLog("Deployment exception: " + ex.Message);
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
                                if (item.StatusBadge != null) item.StatusBadge.Background = HexBrush("#16A34A");
                                item.StatusIcon.Text = "✔";
                                item.StatusText.Text = "Installed";
                                item.StatusText.Foreground = HexBrush("#16A34A");
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
                                if (item.StatusBadge != null) item.StatusBadge.Background = HexBrush(isCancelled ? "#F59E0B" : (rec.status == "Timeout" ? "#F59E0B" : "#DC2626"));
                                item.StatusIcon.Text = isCancelled ? "■" : (rec.status == "Timeout" ? "⏱" : "✕");
                                item.StatusText.Text = isCancelled ? "Cancelled" : (rec.status == "Timeout" ? "Timeout" : "Failed");
                                item.StatusText.Foreground = HexBrush(isCancelled ? "#F59E0B" : (rec.status == "Timeout" ? "#F59E0B" : "#DC2626"));
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

                    int appDonePercent = (int)(((double)(i + 1) / selectedApps.Count) * 100);
                    currentProcessingPercent = appDonePercent;
                    UpdateToastProgress(
                        string.Format("[{0}/{1}] {2}", i + 1, selectedApps.Count, app.name),
                        installed ? "Successfully installed." : "Setup finished.",
                        appDonePercent);

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

                isInstallRunning = false;

                string completionStatus = "Complete";
                if (isCancelled)
                {
                    completionStatus = "Interrupted";
                }
                else if (failed > 0 && success > 0)
                {
                    completionStatus = "Partial Failure";
                }
                else if (failed > 0 && success == 0)
                {
                    completionStatus = "Failed";
                }

                string toastTitle = isCancelled ? "⏹️ Setup Interrupted" :
                    (failed == 0 ? "✅ Setup Complete" : "⚠️ Setup Finished with Issues");

                string toastDetails = string.Format(
                    "Selected: {0}  |  Installed: {1}  |  Downloaded: {2}  |  Status: {3}",
                    selectedApps.Count,
                    success,
                    downloadCount,
                    completionStatus);

                AppendLog("==========================================================");
                AppendLog(string.Format("Setup Ended! Status: {0} (Selected: {1}, Installed: {2}, Downloaded: {3}, Failed: {4})",
                    completionStatus, selectedApps.Count, success, downloadCount, failed));
                AppendLog("Log file saved to: " + logFilePath);
                AppendLog("==========================================================");

                Dispatcher.Invoke(new Action(delegate
                {
                    btnStop.Visibility = Visibility.Visible;
                    btnStop.IsEnabled = true;
                    btnStop.Content = "✓ Finish";
                    btnStop.Background = windowsAccentBrush;
                    btnStop.Foreground = HexBrush("#FFFFFF");
                    btnInstall.Visibility = Visibility.Collapsed;
                    btnInstall.IsEnabled = true;
                    UpdateBottomBarVisibility();

                    if (btnViewReport != null)
                    {
                        btnViewReport.Visibility = Visibility.Collapsed;
                    }

                    btnCreateNewGroup.IsEnabled = true;
                    if (btnImportPack != null) btnImportPack.IsEnabled = true;
                    foreach (RoleCardView rc in roleCards)
                    {
                        rc.SelectButton.IsEnabled = true;
                        rc.EditButton.IsEnabled = true;
                        if (rc.ExportButton != null) rc.ExportButton.IsEnabled = true;
                    }

                    // Update bottom bar status
                    if (txtStatus != null)
                    {
                        txtStatus.Text = string.Format("Setup {0} — Selected: {1}, Installed: {2}, Downloaded: {3}, Status: {4}",
                            completionStatus, selectedApps.Count, success, downloadCount, completionStatus);
                    }

                    // Surface the Windows 11 Toaster Notification Popup with complete summary
                    ShowWindows11Toast(
                        toastTitle,
                        toastDetails,
                        isCancelled ? currentProcessingPercent : 100,
                        isFinal: true,
                        statusTag: completionStatus);
                }));
            });
        }
    }
}
