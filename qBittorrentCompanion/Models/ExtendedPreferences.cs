using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using QBittorrent.Client;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace qBittorrentCompanion.Models
{
    public class ExtendedPreferences : Preferences
    {
        [JsonProperty("bdecode_depth_limit")]
        public int? BdecodeDepthLimit { get; set; } = 101;
    }
}
