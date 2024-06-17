using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace qBittorrentCompanion.Models
{
    public class RssTorrentParams
    {
        [JsonProperty("category")]
        public string Category { get; set; } = "";

        [JsonProperty("content_layout")]
        public string? ContentLayout { get; set; }

        [JsonProperty("download_limit")]
        public int DownloadLimit { get; set; } = -1;

        [JsonProperty("download_path")]
        public string DownloadPath { get; set; } = "";

        [JsonProperty("inactive_seeding_time_limit")]
        public int InactiveSeedingTimeLimit { get; set; }

        [JsonProperty("operating_mode")]
        public string OperatingMode{ get; set; } = "";

        [JsonProperty("ratio_limit")]
        public int RatioLimit { get; set; }

        [JsonProperty("save_path")]
        public string SavePath { get; set; } = "";

        [JsonProperty("seeding_time_limit")]
        public int SeedingTimeLimit { get; set; }

        [JsonProperty("skip_checking")]
        public bool SkipChecking { get; set; }

        [JsonProperty("stopped")]
        public bool Stopped { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; } = [];

        [JsonProperty("upload_limit")]
        public int UploadLimit { get; set; }

        public string TagsAsString
        {
            get => string.Join(", ", Tags);
        }

    }
}
