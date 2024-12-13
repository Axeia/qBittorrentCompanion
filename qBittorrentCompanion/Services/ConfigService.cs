using Newtonsoft.Json;
using qBittorrentCompanion.ViewModels;
using System.IO;

namespace qBittorrentCompanion.Services
{
    public class AppConfig
    {
        public bool ByPassDownloadWindow { get; set; } = true;
        public bool DownloadWindowShowAdvanced { get; set; } = true;
        public bool ShowSidebar { get; set; } = true;
        public bool ShowSideBarStatusIcons { get; set; } = true;
        public double SideBarWidth { get; set; } = 11;
        public string DownloadDirectory { get; set; } = "";
        public string TemporaryDirectory { get; set; } = "";
        public bool EditTrackersWindowShowExtraInfo { get; set; } = true;
        public string[] IconColors = [];
        public int FilterOnStatusIndex = 0;
        public string? FilterOnCategory { get; set; }
        public string? FilterOnTag { get; set; }
        public string? FilterOnTrackerDisplayUrl { get; set; }
        public string ShowUpSizeAs { get; set; } = ServerStateViewModel.SizeOptions[1];
        public string ShowDlSizeAs { get; set; } = ServerStateViewModel.SizeOptions[1];
        public string ShowLineGraphSizeAs { get; set; } = ServerStateViewModel.SizeOptions[1];
    }

    public static class ConfigService
    {
        private static string ConfigFilePath = "AppConfig.json";
        //Assigning a value to prevent compiler warnings
        //LoadConfig is called in the constructor and should assign a value
        public static AppConfig Config { get; private set; } = null!;

        static ConfigService()
        {
            LoadConfig();
            SaveConfig();
        }

        public static void LoadConfig()
        {
            if (File.Exists(ConfigFilePath))
            {
                string json = File.ReadAllText(ConfigFilePath);
                Config = JsonConvert.DeserializeObject<AppConfig>(json)!;
            }
            else
            {
                Config = new AppConfig();
            }
        }

        public static void SaveConfig()
        {
            string json = JsonConvert.SerializeObject(Config);
            File.WriteAllText(ConfigFilePath, json);
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

        /// <summary>
        /// 0 = q
        /// 1 = b
        /// 2 = c
        /// 3 = gradient first color
        /// 4 = gradient second color
        /// </summary>
        public static string[] IconColors
        {
            get => Config.IconColors;
            set
            {
                Config.IconColors = value;
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
    }
}