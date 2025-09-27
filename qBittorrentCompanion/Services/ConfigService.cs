using Newtonsoft.Json;
using qBittorrentCompanion.Converters;
using qBittorrentCompanion.Models;
using qBittorrentCompanion.ViewModels;
using System.IO;
using System.Runtime.CompilerServices;

namespace qBittorrentCompanion.Services
{
    /// <summary>
    /// Try to keep this nice and simple, avoid using complex types and
    /// stick to common built-in value types, 
    /// e.g. https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/built-in-types
    /// 
    /// If a collection is used stick to arrays, they're immutable which means they can use the same logic in ConfigService
    /// (just reasignment, no collection manipulation)
    /// </summary>
    public class AppConfig
    {
        public bool ByPassDownloadWindow { get; set; } = true;
        public bool DownloadWindowShowAdvanced { get; set; } = true;
        public bool ShowSidebar { get; set; } = true;
        public bool ShowSideBarStatusIcons { get; set; } = true;
        public double SideBarWidth { get; set; } = 11;
        public string DownloadDirectory { get; set; } = string.Empty;
        public string TemporaryDirectory { get; set; } = string.Empty;
        public bool EditTrackersWindowShowExtraInfo { get; set; } = true;
        public int FilterOnStatusIndex = 0;
        public string? FilterOnCategory { get; set; }
        public string? FilterOnTag { get; set; }
        public string? FilterOnTrackerDisplayUrl { get; set; }
        public string ShowUpSizeAs { get; set; } = ServerStateViewModel.SizeOptions[1];
        public string ShowDlSizeAs { get; set; } = ServerStateViewModel.SizeOptions[1];
        public string ShowLineGraphSizeAs { get; set; } = ServerStateViewModel.SizeOptions[1];
        public bool ShowRssExpandedControls { get; set; } = false;
        public bool ShowRssTestData { get; set; } = true;
        public bool RssFeedsExpandRssPlugin { get; set; } = true;

        public string LastSelectedRssPlugin = string.Empty;
        public string LastSelectedLocalSearchPlugin = string.Empty;
        public string LastSelectedLocalSearchCategory = string.Empty;
        public string LastSelectedRemoteSearchPlugin = string.Empty;
        public string LastSelectedRemoteSearchCategory = string.Empty;
        public bool ExpandSearchRssPlugin { get; set; } = true;

        public int LastSelectedTorrentsSubTabIndex = 0;
        public bool ShowTorrentColumnSize { get; set; } = true;
        public bool ShowTorrentColumnTotalSize { get; set; } = false;
        public bool ShowTorrentColumnDone { get; set; } = true;
        public bool ShowTorrentColumnStatus { get; set; } = true;
        public bool ShowTorrentColumnSeeds { get; set; } = true;
        public bool ShowTorrentColumnPeers { get; set; } = true;
        public bool ShowTorrentColumnDownSpeed { get; set; } = true;
        public bool ShowTorrentColumnUpSpeed { get; set; } = true;
        public bool ShowTorrentColumnETA { get; set; } = false;
        public bool ShowTorrentColumnRatio { get; set; } = false;
        public bool ShowTorrentColumnCategory { get; set; } = false;
        public bool ShowTorrentColumnTags { get; set; } = false;
        public bool ShowTorrentColumnAddedOn { get; set; } = true;
        public bool ShowTorrentColumnCompletedOn { get; set; } = false;
        public bool ShowTorrentColumnTracker { get; set; } = false;
        public bool ShowTorrentColumnDownLimit { get; set; } = false;
        public bool ShowTorrentColumnUpLimit { get; set; } = false;
        public bool ShowTorrentColumnDownloaded { get; set; } = true;
        public bool ShowTorrentColumnUploaded { get; set; } = false;
        public bool ShowTorrentColumnDownloadedInSession { get; set; } = false;
        public bool ShowTorrentColumnUploadedInSession { get; set; } = false;
        public bool ShowTorrentColumnIncompletedSize { get; set; } = false;
        public bool ShowTorrentColumnTimeActive { get; set; } = true;
        public bool ShowTorrentColumnSavePath { get; set; } = false;
        public bool ShowTorrentColumnCompletedSize { get; set; } = false;
        public bool ShowTorrentColumnRatioLimit { get; set; } = false;
        public bool ShowTorrentColumnSeenComplete { get; set; } = false;
        public bool ShowTorrentColumnLastActivity { get; set; } = false;
        public bool ExpandRssRuleRssPlugin { get; set; } = true;
        public bool ShowRssRuleWarnings { get; set; } = true;
        public bool ShowRssRuleSmartFilter { get; set; } = false;
        public int RssRuleArticleDetailSelectedTabIndex { get; set; } = 1;
        public bool ShowLogging { get; set; } = false;
        public int LogViewSelectedTabIndex { get; set; } = 0;
        public bool UseRemoteSearch { get; set; } = true;
        public bool ShowGithubSearchPluginWarning { get; set; } = true;
        public bool ShowLocalSearchPluginCopyrightWarning { get; set; } = true;
        public string[] DisabledLocalSearchPlugins { get; set; } = [];
        public bool ShowGitHubSearchPluginDetailLabelText { get; set; } = true;
        public bool ShowGitHubSearchPluginAllDetails { get; set; } = true;

        /// <summary>
        /// <see cref="ConfigService.JsonSettings"/>
        /// <seealso cref="Converters.AvaloniaColorJsonConverter"/>
        /// </summary>
        public LogoColorsRecord LogoColorsLight { get; set; } = LogoColorsRecord.LightModeDefault;

        /// <summary>
        /// <see cref="ConfigService.JsonSettings"/>
        /// <seealso cref="Converters.AvaloniaColorJsonConverter"/>
        /// </summary>
        public LogoColorsRecord LogoColorsDark { get; set; } = LogoColorsRecord.DarkModeDefault; 
    }

    public static class ConfigService
    {
        public static string ConfigFilePath { get; private set; } = "AppConfig.json";
        //Assigning a value to prevent compiler warnings
        //LoadConfig is called in the constructor and should assign a value
        public static AppConfig Config { get; private set; } = null!;

        static ConfigService()
        {
            LoadConfig();
            SaveConfig();
        }

        public static readonly JsonSerializerSettings JsonSettings = new()
        {
            Converters = { new AvaloniaColorJsonConverter() },
            Formatting = Formatting.Indented
        };

        public static void LoadConfig()
        {
            if (File.Exists(ConfigFilePath))
            {
                string json = File.ReadAllText(ConfigFilePath);
                Config = JsonConvert.DeserializeObject<AppConfig>(json, JsonSettings)!;
            }
            else
            {
                Config = new AppConfig();
            }
        }

        public static void SaveConfig([CallerMemberName] string? propertyName = null)
        {
            string json = JsonConvert.SerializeObject(Config, Formatting.Indented, JsonSettings);
            File.WriteAllText(ConfigFilePath, json);

            if (propertyName != null)
                AppLoggerService.AddLogMessage(
                    Splat.LogLevel.Info,
                    nameof(ConfigService),
                    $"Saved {propertyName} to " + ConfigFilePath,
                    json,
                    ConfigFilePath
                );
        }

        public static bool BypassDownloadWindow
        {
            get => Config.ByPassDownloadWindow;
            set
            {
                Config.ByPassDownloadWindow = value;
                SaveConfig();
            }
        }

        public static bool ShowSideBar
        {
            get => Config.ShowSidebar;
            set
            {
                Config.ShowSidebar = value;
                SaveConfig();
            }
        }

        public static bool ShowSideBarStatusIcons
        {
            get => Config.ShowSideBarStatusIcons;
            set
            {
                Config.ShowSideBarStatusIcons = value;
                SaveConfig();
            }
        }

        public static double SideBarWidth
        {
            get => Config.SideBarWidth;
            set
            {
                Config.SideBarWidth = value;
                SaveConfig();
            }
        }

        public static bool DownloadWindowShowAdvanced
        {
            get => Config.DownloadWindowShowAdvanced;
            set
            {
                Config.DownloadWindowShowAdvanced = value;
                SaveConfig();
            }
        }

        public static string DownloadDirectory
        {
            get => Config.DownloadDirectory;
            set
            {
                Config.DownloadDirectory = value;
                SaveConfig();
            }
        }

        public static string TemporaryDirectory
        {
            get => Config.TemporaryDirectory;
            set
            {
                Config.TemporaryDirectory = value;
                SaveConfig();
            }
        }

        public static bool EditTrackersWindowShowExtraInfo
        {
            get => Config.EditTrackersWindowShowExtraInfo;
            set
            {
                Config.EditTrackersWindowShowExtraInfo = value;
                SaveConfig();
            }
        }

        public static int FilterOnStatusIndex
        {
            get => Config.FilterOnStatusIndex;
            set
            {
                Config.FilterOnStatusIndex = value;
                SaveConfig();
            }
        }

        public static string? FilterOnCategory
        {
            get => Config.FilterOnCategory;
            set
            {
                Config.FilterOnCategory = value;
                SaveConfig();
            }
        }

        public static string? FilterOnTag
        {
            get => Config.FilterOnTag;
            set
            {
                Config.FilterOnTag = value;
                SaveConfig();
            }
        }

        public static string? FilterOnTrackerDisplayUrl
        {
            get => Config.FilterOnTrackerDisplayUrl;
            set
            {
                Config.FilterOnTrackerDisplayUrl = value;
                SaveConfig();
            }
        }

        public static string ShowDlSizeAs
        {
            get => Config.ShowDlSizeAs;
            set
            {
                Config.ShowDlSizeAs = value;
                SaveConfig();
            }
        }

        public static string ShowUpSizeAs
        {
            get => Config.ShowUpSizeAs;
            set
            {
                Config.ShowUpSizeAs = value;
                SaveConfig();
            }
        }

        public static string ShowLineGraphSizeAs
        {
            get => Config.ShowLineGraphSizeAs;
            set
            {
                Config.ShowLineGraphSizeAs = value;
                SaveConfig();
            }
        }

        public static bool ShowRssExpandedControls
        {
            get => Config.ShowRssExpandedControls;
            set
            {
                Config.ShowRssExpandedControls = value;
                SaveConfig();
            }
        }

        public static bool ShowRssTestData
        {
            get => Config.ShowRssTestData;
            set
            {
                Config.ShowRssTestData = value;
                SaveConfig();
            }
        }

        public static bool RssFeedsExpandRssPlugin
        {
            get => Config.RssFeedsExpandRssPlugin;
            set
            {
                Config.RssFeedsExpandRssPlugin = value;
                SaveConfig();
            }
        }

        public static string LastSelectedRssPlugin
        {
            get => Config.LastSelectedRssPlugin;
            set
            {
                Config.LastSelectedRssPlugin = value;
                SaveConfig();
            }
        }

        public static string LastSelectedLocalSearchPlugin
        {
            get => Config.LastSelectedLocalSearchPlugin;
            set
            {
                Config.LastSelectedLocalSearchPlugin = value;
                SaveConfig();
            }
        }
        public static string LastSelectedLocalSearchCategory
        {
            get => Config.LastSelectedLocalSearchCategory;
            set
            {
                Config.LastSelectedLocalSearchCategory = value;
                SaveConfig();
            }
        }

        public static string LastSelectedRemoteSearchPlugin
        {
            get => Config.LastSelectedRemoteSearchPlugin;
            set
            {
                Config.LastSelectedRemoteSearchPlugin = value;
                SaveConfig();
            }
        }
        public static string LastSelectedRemoteSearchCategory
        {
            get => Config.LastSelectedRemoteSearchCategory;
            set
            {
                Config.LastSelectedRemoteSearchCategory = value;
                SaveConfig();
            }
        }

        public static bool ExpandSearchRssPlugin
        {
            get => Config.ExpandSearchRssPlugin;
            set
            {
                Config.ExpandSearchRssPlugin = value;
                SaveConfig();
            }
        }

        public static int LastSelectedTorrentsSubTabIndex
        {
            get => Config.LastSelectedTorrentsSubTabIndex;
            set
            {
                Config.LastSelectedTorrentsSubTabIndex = value;
                SaveConfig();
            }
        }

        public static bool ShowTorrentColumnSize
        {
            get => Config.ShowTorrentColumnSize;
            set
            {
                Config.ShowTorrentColumnSize = value;
                SaveConfig();
            }
        }

        public static bool ShowTorrentColumnTotalSize
        {
            get => Config.ShowTorrentColumnTotalSize;
            set
            {
                Config.ShowTorrentColumnTotalSize = value;
                SaveConfig();
            }
        }

        public static bool ShowTorrentColumnDone
        {
            get => Config.ShowTorrentColumnDone;
            set
            {
                Config.ShowTorrentColumnDone = value;
                SaveConfig();
            }
        }

        public static bool ShowTorrentColumnStatus
        {
            get => Config.ShowTorrentColumnStatus;
            set
            {
                Config.ShowTorrentColumnStatus = value;
                SaveConfig();
            }
        }

        public static bool ShowTorrentColumnSeeds
        {
            get => Config.ShowTorrentColumnSeeds;
            set
            {
                Config.ShowTorrentColumnSeeds = value;
                SaveConfig();
            }
        }

        public static bool ShowTorrentColumnPeers
        {
            get => Config.ShowTorrentColumnPeers;
            set
            {
                Config.ShowTorrentColumnPeers = value;
                SaveConfig();
            }
        }

        public static bool ShowTorrentColumnDownSpeed
        {
            get => Config.ShowTorrentColumnDownSpeed;
            set
            {
                Config.ShowTorrentColumnDownSpeed = value;
                SaveConfig();
            }
        }

        public static bool ShowTorrentColumnUpSpeed
        {
            get => Config.ShowTorrentColumnUpSpeed;
            set
            {
                Config.ShowTorrentColumnUpSpeed = value;
                SaveConfig();
            }
        }

        public static bool ShowTorrentColumnETA
        {
            get => Config.ShowTorrentColumnETA;
            set
            {
                Config.ShowTorrentColumnETA = value;
                SaveConfig();
            }
        }

        public static bool ShowTorrentColumnRatio
        {
            get => Config.ShowTorrentColumnRatio;
            set
            {
                Config.ShowTorrentColumnRatio = value;
                SaveConfig();
            }
        }

        public static bool ShowTorrentColumnCategory
        {
            get => Config.ShowTorrentColumnCategory;
            set
            {
                Config.ShowTorrentColumnCategory = value;
                SaveConfig();
            }
        }

        public static bool ShowTorrentColumnAddedOn
        {
            get => Config.ShowTorrentColumnAddedOn;
            set
            {
                Config.ShowTorrentColumnAddedOn = value;
                SaveConfig();
            }
        }

        public static bool ShowTorrentColumnTags
        {
            get => Config.ShowTorrentColumnTags;
            set
            {
                Config.ShowTorrentColumnTags = value;
                SaveConfig();
            }
        }

        public static bool ShowTorrentColumnCompletedOn
        {
            get => Config.ShowTorrentColumnCompletedOn;
            set
            {
                Config.ShowTorrentColumnCompletedOn = value;
                SaveConfig();
            }
        }

        public static bool ShowTorrentColumnTracker
        {
            get => Config.ShowTorrentColumnTracker;
            set
            {
                Config.ShowTorrentColumnTracker = value;
                SaveConfig();
            }
        }

        public static bool ShowTorrentColumnDownLimit
        {
            get => Config.ShowTorrentColumnDownLimit;
            set
            {
                Config.ShowTorrentColumnDownLimit = value;
                SaveConfig();
            }
        }

        public static bool ShowTorrentColumnUpLimit
        {
            get => Config.ShowTorrentColumnUpLimit;
            set
            {
                Config.ShowTorrentColumnUpLimit = value;
                SaveConfig();
            }
        }

        public static bool ShowTorrentColumnDownloaded
        {
            get => Config.ShowTorrentColumnDownloaded;
            set
            {
                Config.ShowTorrentColumnDownloaded = value;
                SaveConfig();
            }
        }

        public static bool ShowTorrentColumnUploaded
        {
            get => Config.ShowTorrentColumnUploaded;
            set
            {
                Config.ShowTorrentColumnUploaded = value;
                SaveConfig();
            }
        }

        public static bool ShowTorrentColumnDownloadedInSession
        {
            get => Config.ShowTorrentColumnDownloadedInSession;
            set
            {
                Config.ShowTorrentColumnDownloadedInSession = value;
                SaveConfig();
            }
        }

        public static bool ShowTorrentColumnUploadedInSession
        {
            get => Config.ShowTorrentColumnUploadedInSession;
            set
            {
                Config.ShowTorrentColumnUploadedInSession = value;
                SaveConfig();
            }
        }

        public static bool ShowTorrentColumnIncompletedSize
        {
            get => Config.ShowTorrentColumnIncompletedSize;
            set
            {
                Config.ShowTorrentColumnIncompletedSize = value;
                SaveConfig();
            }
        }

        public static bool ShowTorrentColumnTimeActive
        {
            get => Config.ShowTorrentColumnTimeActive;
            set
            {
                Config.ShowTorrentColumnTimeActive = value;
                SaveConfig();
            }
        }

        public static bool ShowTorrentColumnSavePath
        {
            get => Config.ShowTorrentColumnSavePath;
            set
            {
                Config.ShowTorrentColumnSavePath = value;
                SaveConfig();
            }
        }

        public static bool ShowTorrentColumnCompletedSize
        {
            get => Config.ShowTorrentColumnCompletedSize;
            set
            {
                Config.ShowTorrentColumnCompletedSize = value;
                SaveConfig();
            }
        }

        public static bool ShowTorrentColumnRatioLimit
        {
            get => Config.ShowTorrentColumnRatioLimit;
            set
            {
                Config.ShowTorrentColumnRatioLimit = value;
                SaveConfig();
            }
        }

        public static bool ShowTorrentColumnSeenComplete
        {
            get => Config.ShowTorrentColumnSeenComplete;
            set
            {
                Config.ShowTorrentColumnSeenComplete = value;
                SaveConfig();
            }
        }

        public static bool ShowTorrentColumnLastActivity
        {
            get => Config.ShowTorrentColumnLastActivity;
            set
            {
                Config.ShowTorrentColumnLastActivity = value;
                SaveConfig();
            }
        }

        public static bool ExpandRssRuleRssPlugin
        {
            get => Config.ExpandRssRuleRssPlugin;
            set
            {
                Config.ExpandRssRuleRssPlugin = value;
                SaveConfig();
            }
        }

        public static int RssRuleArticleDetailSelectedTabIndex
        {
            get => Config.RssRuleArticleDetailSelectedTabIndex;
            set
            {
                Config.RssRuleArticleDetailSelectedTabIndex = value;
                SaveConfig();
            }
        }

        public static bool ShowRssRuleWarnings
        {
            get => Config.ShowRssRuleWarnings;
            set
            {
                Config.ShowRssRuleWarnings = value;
                SaveConfig();
            }
        }

        public static bool ShowRssRuleSmartFilter
        {
            get => Config.ShowRssRuleSmartFilter;
            set
            {
                Config.ShowRssRuleSmartFilter = value;
                SaveConfig();
            }
        }

        public static bool ShowLogging
        {
            get => Config.ShowLogging;
            set
            {
                Config.ShowLogging = value;
                SaveConfig();
            }
        }

        public static int LogViewSelectedTabIndex
        {
            get => Config.LogViewSelectedTabIndex;
            set
            {
                Config.LogViewSelectedTabIndex = value;
                SaveConfig();
            }
        }

        public static bool UseRemoteSearch
        {
            get => Config.UseRemoteSearch;
            set
            {
                Config.UseRemoteSearch = value;
                SaveConfig();
            }
        }

        public static bool ShowGithubSearchPluginWarning
        {
            get => Config.ShowGithubSearchPluginWarning;
            set
            {
                Config.ShowGithubSearchPluginWarning = value;
                SaveConfig();
            }
        }

        public static bool ShowSearchPluginCopyrightWarning
        {
            get => Config.ShowLocalSearchPluginCopyrightWarning;
            set
            {
                Config.ShowLocalSearchPluginCopyrightWarning = value;
                SaveConfig();
            }
        }

        public static string[] DisabledLocalSearchPlugins
        {
            get => Config.DisabledLocalSearchPlugins;
            set
            {
                Config.DisabledLocalSearchPlugins = value;
                SaveConfig();
            }
        }

        public static bool ShowGitHubSearchPluginDetailLabelText
        {
            get => Config.ShowGitHubSearchPluginDetailLabelText;
            set
            {
                Config.ShowGitHubSearchPluginDetailLabelText = value;
                SaveConfig();
            }
        }

        public static bool ShowGitHubSearchPluginAllDetails
        {
            get => Config.ShowGitHubSearchPluginAllDetails;
            set
            {
                Config.ShowGitHubSearchPluginAllDetails = value;
                SaveConfig();
            }
        }

        public static LogoColorsRecord LogoColorsLight
        {
            get => Config.LogoColorsLight;
            set
            {
                Config.LogoColorsLight = value;
                SaveConfig();
            }
        }

        public static LogoColorsRecord LogoColorsDark
        {
            get => Config.LogoColorsDark;
            set
            {
                Config.LogoColorsDark = value;
                SaveConfig();
            }
        }

    }
}