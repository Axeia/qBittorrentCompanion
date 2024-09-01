using QBittorrent.Client;
using qBittorrentCompanion.Models;
using qBittorrentCompanion.ViewModels;
using System;
using System.IO;


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
                string[] sizes = { "B  ", "KiB", "MiB", "GiB", "TiB" };
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

        public static class DataStorageTypes
        {
            public static string Legacy = "Fastresume files";
            public static string SQLite = "SQLite database (experimental)";
        }

        public static class ProxyTypeDescriptions
        {
            public static string None = "None";
            public static string Http = "HTTP without authentication";
            public static string Socks5 = "SOCKS5 without authentication";
            public static string HttpAuth = "HTTP with authentication";
            public static string Socks5Auth = "SOCKS5 with authentication";
            public static string Socks4 = "SOCKS4 without authentication";
        }

        public static class TorrentContentPriorities // Very confusing naming, but this mirrors what the WebUI does.
        {
            public static string Skip = "Do not download";
            public static string Minimal = "Normal";
            public static string VeryLow = "Very low.."; // Not used 
            public static string Low = "Low..."; // Not used
            public static string Normal = "Normal..."; // Not used
            public static string High = "High..."; // Not used
            public static string VeryHigh = "High"; 
            public static string Maximal = "Maximum";
            public static string Mixed = "Mixed";
        }

        public static class UploadSlotBehaviors
        {
            public static string FixedSlots = "Fixed slots";
            public static string UploadRateBased = "Upload rate based";
        }

        public static class UploadChokingAlgorithms
        {
            public static string RoundRobin = "Round-robin";
            public static string FastestUpload = "Fastest upload";
            public static string AntiLeech = "Anti-leech";
        }

        public static class DnsServices
        {
            public static string None = "None";
            public static string DynDns = "DynDNS";
            public static string NoIp = "NO-IP";
        }

        /// <summary>
        /// Fluent icon that can be displayed which is based on the file extension, 
        /// OpenAI's ChatGPT was used to generate it (although it has been modified)
        /// </summary>
        public static FluentIcons.Common.Symbol FileToFluentIcon(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLower();
            switch (extension)
            {
                // Compressed files
                case ".zip":
                case ".rar":
                case ".7z":
                case ".tar":
                case ".gz":
                case ".bz2":
                case ".xz":
                    return FluentIcons.Common.Symbol.FolderZip;

                // Video files
                case ".mp4":
                case ".mkv":
                case ".avi":
                case ".mov":
                case ".wmv":
                case ".flv":
                case ".webm":
                case ".m4v":
                    return FluentIcons.Common.Symbol.Video;

                // Image files
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".gif":
                case ".bmp":
                case ".tiff":
                case ".svg":
                case ".webp":
                    return FluentIcons.Common.Symbol.Image;

                // Audio files
                case ".mp3":
                case ".wav":
                case ".flac":
                case ".aac":
                case ".ogg":
                case ".m4a":
                case ".wma":
                case ".aiff":
                    return FluentIcons.Common.Symbol.MusicNote1;

                // Document files
                case ".pdf":
                    return FluentIcons.Common.Symbol.DocumentPdf;
                case ".doc":
                case ".docx":
                case ".odt":
                case ".rtf":
                case ".txt":
                    return FluentIcons.Common.Symbol.DocumentText;

                // Spreadsheet files
                case ".xls":
                case ".xlsx":
                case ".ods":
                case ".csv":
                    return FluentIcons.Common.Symbol.LayoutCellFourFocusBottomLeft;

                // Code files
                case ".css":
                    return FluentIcons.Common.Symbol.DocumentCss;
                case ".js":
                    return FluentIcons.Common.Symbol.DocumentJavascript;
                case ".java":
                    return FluentIcons.Common.Symbol.DocumentJava;
                case ".html":
                case ".ts":
                case ".json":
                case ".xml":
                case ".yml":
                case ".yaml":
                case ".cs":
                case ".py":
                case ".cpp":
                case ".c":
                case ".h":
                case ".php":
                case ".rb":
                case ".swift":
                case ".go":
                case ".rs":
                    return FluentIcons.Common.Symbol.Code;

                // Disk image files
                case ".iso":
                case ".img":
                case ".dmg":
                    return FluentIcons.Common.Symbol.DocumentData;

                default:
                    return FluentIcons.Common.Symbol.Document;

            }
        }
    }
}
