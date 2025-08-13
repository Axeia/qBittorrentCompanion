using QBittorrent.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace qBittorrentCompanion.Helpers
{
    public static class TorrentStateGroupings
    {
        public static List<TorrentState> Paused =>
            [TorrentState.PausedDownload, TorrentState.PausedUpload];

        public static List<TorrentState> Seeding =>
            [TorrentState.Uploading, TorrentState.QueuedUpload, TorrentState.StalledUpload, TorrentState.ForcedUpload];
        public static List<TorrentState> Resumed => [
                TorrentState.Uploading, TorrentState.QueuedUpload, TorrentState.StalledUpload,
                    TorrentState.ForcedUpload, TorrentState.Downloading, TorrentState.Uploading
            ];
        public static List<TorrentState> Download =>
            [TorrentState.Downloading, TorrentState.PausedDownload];
        public static List<TorrentState> Active =>
            [TorrentState.Downloading, TorrentState.Uploading];

        public static List<TorrentState> InActive
        {
            get
            {
                var activeStates = Active;
                var allStates = Enum.GetValues(typeof(TorrentState)).Cast<TorrentState>().ToList();
                return allStates.Except(activeStates).ToList();
            }
        }
        public static List<TorrentState> Stalled =>
            [TorrentState.StalledUpload, TorrentState.StalledDownload];
        public static List<TorrentState> StalledDownload =>
            [TorrentState.StalledDownload];
        public static List<TorrentState> StalledUpload =>
            [TorrentState.StalledUpload];
        public static List<TorrentState> Checking =>
            [TorrentState.CheckingUpload, TorrentState.CheckingDownload];
        public static List<TorrentState> Error => [
            TorrentState.Error, TorrentState.MissingFiles];
    }
}
