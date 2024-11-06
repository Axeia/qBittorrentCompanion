using Newtonsoft.Json;
using System.IO;

namespace qBittorrentCompanion.Services
{
    public class AppConfig
    {
        public bool ByPassDownloadWindow { get; set; } = true;
        public bool DownloadWindowShowAdvanced { get; set; } = true;
        public bool ShowStatusIcons { get; set; } = true;
        public string DownloadDirectory { get; set; } = "";
        public string TemporaryDirectory { get; set; } = "";
        public bool EditTrackersWindowShowExtraInfo {  get; set; } = true;
        public string[] IconColors = [];
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

        public static bool ShowStatusIcons
        {
            get => Config.ShowStatusIcons;
            set
            {
                Config.ShowStatusIcons = value;
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
    }
}