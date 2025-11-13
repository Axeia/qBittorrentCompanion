using Newtonsoft.Json;
using QBittorrent.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace qBittorrentCompanion.Helpers
{
    public class AddTorrentRequestBaseDto
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string DownloadFolder { get; set; } = string.Empty;
        public bool ShouldSerializeDownloadFolder() => !string.IsNullOrEmpty(DownloadFolder);

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Category { get; set; } = string.Empty;
        public bool ShouldSerializeCategory() => !string.IsNullOrEmpty(Category);

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool? SkipHashChecking { get; set; } // Nullable so false won't be stored
        public bool ShouldSerializeSkipHashChecking()
            => SkipHashChecking == true;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool? Paused { get; set; }
        public bool ShouldSerializePaused()
            => Paused == true;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? UploadLimit { get; set; }
        public bool ShouldSerializeUploadLimit()
            => UploadLimit is int upll && upll != -1;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? DownloadLimit { get; set; }
        public bool ShouldSerializeDownloadLimit()
            => DownloadLimit is int dll && dll != -1;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool? SequentialDownload { get; set; }
        public bool ShouldSerializeSequentialDownload()
            => SequentialDownload == true;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool? FirstLastPiecePrioritized { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<string>? Tags { get; set; }
        public bool ShouldSerializeTags() 
            => Tags != null && Tags.Any();

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public TorrentContentLayout ContentLayout { get; set; }
        public bool ShouldSerializeTorrentContentLayout()
            => ContentLayout is not TorrentContentLayout.Original;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double? RatioLimit { get; set; }
        public bool ShouldSerializeRatioLimit()
            => RatioLimit is not null;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public TimeSpan? SeedingTimeLimit { get; set; }
        public bool ShouldSerializeSeedingTimeLimit()
            => SeedingTimeLimit is not null;

        public bool HasAnyNonDefaultValue()
        {
            if (ShouldSerializeDownloadFolder())
                return true;
            if (ShouldSerializeSeedingTimeLimit())
                return true;
            if (ShouldSerializeRatioLimit())
                return true;
            if (ShouldSerializeDownloadLimit())
                return true;
            if (ShouldSerializeUploadLimit())
                return true;
            if (ShouldSerializeCategory())
                return true;
            if (ShouldSerializeSkipHashChecking())
                return true;
            if (ShouldSerializePaused())
                return true;
            if (ShouldSerializeSequentialDownload())
                return true;
            if (FirstLastPiecePrioritized == true)
                return true;
            if (ShouldSerializeTorrentContentLayout())
                return true;
            if (ShouldSerializeTags())
                return true;

            return false;
        }

        /// <summary>
        /// Base object, set fields like <see cref="AddTorrentsRequest.TorrentFiles"/> 
        /// before trying to send it to the qBittorrent API.
        /// </summary>
        /// <returns>A new AddTorrentRequest with the settings</returns>
        public AddTorrentsRequest ToAddTorrentRequest()
        {
            return new()
            {
                DownloadFolder = this.DownloadFolder,
                Category = this.Category,
                SkipHashChecking = this.SkipHashChecking is true,
                Paused = this.Paused is true,
                UploadLimit = this.UploadLimit,
                DownloadLimit = this.DownloadLimit,
                SequentialDownload = this.SequentialDownload is true,
                FirstLastPiecePrioritized = this.FirstLastPiecePrioritized is true,
                Tags = this.Tags?.ToList() ?? [],
                ContentLayout = this.ContentLayout,
                RatioLimit = this.RatioLimit,
                SeedingTimeLimit = this.SeedingTimeLimit
            };
        }
    }
}
