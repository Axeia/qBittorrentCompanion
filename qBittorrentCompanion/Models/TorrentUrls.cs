using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace qBittorrentCompanion.Models
{
    public class TorrentUrls
    {
        [JsonProperty("urls")]
        public string Urls { get; set; }

        [JsonProperty("autoTMM")]
        public string AutoTTM { get; set; }

        [JsonProperty("savepath")]
        public string SavePath { get; set; }

        [JsonProperty("cookie")]
        public string Cookie { get; set; }

        [JsonProperty("rename")]
        public string Rename { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("paused")]
        public bool Paused { get; set; }

        [JsonProperty("stopCondition")]
        public string StopCondition { get; set; }

        [JsonProperty("contentLayout")]
        public string ContentLayout { get; set; }

        [JsonProperty("sequentialDownload")]
        public bool SequentialDownload { get; set; }

        [JsonProperty("firstLastPiecePrio")]
        public bool FirstLastPiecePrio { get; set; }

        [JsonProperty("dlLimit")]
        public string DlLimit { get; set; }

        [JsonProperty("upLimit")]
        public string UpLimit { get; set; }
    }
}
