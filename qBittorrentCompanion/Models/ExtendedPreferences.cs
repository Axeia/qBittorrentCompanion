using System.Text.Json;
using Newtonsoft.Json;
using QBittorrent.Client;

namespace qBittorrentCompanion.Models
{
    public class ExtendedPreferences : Preferences
    {
        [JsonProperty("file_log_enabled")]
        public bool? FileLogEnabled { get; set; }

        [JsonProperty("file_log_backup_enabled")]
        public bool? FileLogBackupEnabled { get; set; }

        [JsonProperty("file_log_delete_old")]
        public bool? FileLogDeleteOld { get; set; }

        [JsonProperty("file_log_max_size")]
        public int? FileLogMaxSize { get; set; }


    }
}
