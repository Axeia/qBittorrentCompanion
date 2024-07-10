using QBittorrent.Client;
using qBittorrentCompanion.Models;
using qBittorrentCompanion.ViewModels;
using System;


namespace qBittorrentCompanion.Helpers
{
    public static class DataConverter
    {
        public static string BytesToHumanReadable(long? bytesInp)
        {
            // Should never occur, null values should only come from subsequent responses 
            // and those should not get updated to be the fields value.
            if (bytesInp == null)
                return "";
            else
            {
                double bytes = (double)bytesInp;
                string[] sizes = { "  B", "KiB", "MiB", "GiB", "TiB" };
                int order = 0;
                while (bytes >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    bytes = bytes / 1024;
                }
                return string.Format("{0:0.00} {1}", bytes, sizes[order]);
            }
        }

        public static string StateToHumanReadable(TorrentState state)
        {
            switch(state)
            {
                case TorrentState.Error:
                    return "Error";
                case TorrentState.MissingFiles:
                    return "Missing files";
                case TorrentState.Uploading:
                    return "Uploading";
                case TorrentState.PausedUpload:
                    return "Paused upload";
                case TorrentState.QueuedUpload:
                    return "Queued upload";
                case TorrentState.StalledUpload:
                    return "Stalled upload";
                case TorrentState.CheckingUpload:
                    return "Checking upload";
                case TorrentState.ForcedUpload:
                    return "Allocating";
                case TorrentState.Allocating:
                    return "Allocating";
                case TorrentState.Downloading:
                    return "Downloading";
                case TorrentState.FetchingMetadata:
                case TorrentState.ForcedFetchingMetadata:
                    return "Metadata downloading";
                case TorrentState.PausedDownload:
                    return "Paused download";
                case TorrentState.QueuedDownload:
                    return "Queued download";
                case TorrentState.StalledDownload:
                    return "Stalled upload";
                case TorrentState.CheckingDownload:
                    return "Checking download";
                case TorrentState.ForcedDownload:
                    return "[F] Download";
                case TorrentState.CheckingResumeData:
                    return "Checking resume data";
                case TorrentState.Moving:
                    return "";
                case TorrentState.Unknown:
                    return "";
                default:
                    return "???";
            }
        }

        public static string DownloadPriorityToHumanReadable(TorrentContentPriority dlPriority) =>
            dlPriority switch
            {
                TorrentContentPriority.Skip => "Do not download",
                TorrentContentPriority.Normal => "Normal",
                TorrentContentPriority.High => "High",
                TorrentContentPriority.Maximal => "Maximum",
                _ => "Mixed",
            };


        public static string TorrentTrackerStatusToHumanReadable(TorrentTrackerStatus status) =>
            status switch
            {
                TorrentTrackerStatus.Disabled => "Disabled",
                TorrentTrackerStatus.NotContacted => "Not contacted yet",
                TorrentTrackerStatus.Working => "Working",
                TorrentTrackerStatus.Updating => "Updating",
                TorrentTrackerStatus.NotWorking => "Not working",
                _ => "???"
            };

        public static string AvailabilityToPercentageString(double? availability) =>
            availability switch
            {
                _ when !availability.HasValue || availability == -1 || availability == 0 => "0.0%",
                _ => (availability.Value * 100).ToString("0.0%")
            };

        public static string TimeSpanToHumanReadable(TimeSpan? timeSpan)
        {
            // Should never occur, null values should only come from subsequent responses 
            // and those should not get updated to be the fields value.
            if (timeSpan == null)
                return "???";
                //throw new ArgumentNullException(nameof(timeSpan));

            if (timeSpan == TimeSpan.FromDays(100))
                return "∞";

            TimeSpan time = timeSpan.Value;
            string formattedTime = string.Empty;

            if (time.TotalDays > 365)
            {
                int years = (int)(time.TotalDays / 365);
                formattedTime += $"{years}y";
                time = time.Subtract(TimeSpan.FromDays(years * 365));
            }

            if (time.TotalDays > 30)
            {
                int months = (int)(time.TotalDays / 30);
                formattedTime += $"{months}mo";
                time = time.Subtract(TimeSpan.FromDays(months * 30));
            }

            if (time.TotalDays >= 1)
            {
                formattedTime += $"{time.Days:D2}d";
                time = time.Subtract(TimeSpan.FromDays(time.Days));
            }

            if (time.TotalHours >= 1)
            {
                formattedTime += $"{time.Hours:D2}h";
                time = time.Subtract(TimeSpan.FromHours(time.Hours));
            }

            if (time.TotalMinutes >= 1)
            {
                formattedTime += $"{time.Minutes:D2}m";
            }
            else
            {
                formattedTime += "<1m";
            }

            return formattedTime.TrimStart('0');
        }

    }
}
