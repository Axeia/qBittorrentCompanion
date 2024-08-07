﻿using Avalonia.Media;
using Newtonsoft.Json.Linq;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TorrentState = QBittorrent.Client.TorrentState;

namespace qBittorrentCompanion.ViewModels
{
    // https://github.com/qbittorrent/qBittorrent/wiki/WebUI-API-(qBittorrent-4.1)
    public class TorrentInfoViewModel : INotifyPropertyChanged
    {
        private TorrentPartialInfo _torrentInfo;

        public event PropertyChangedEventHandler? PropertyChanged;
        public static Geometry pausedIcon = Geometry.Parse("");
        public static Geometry downloadingIcon = Geometry.Parse("");
        public static Geometry queuedIcon = Geometry.Parse("");
        public static Geometry uploadingIcon = Geometry.Parse("");
        public static Geometry errorIcon = Geometry.Parse("");
        public static Geometry diskIcon = Geometry.Parse("");
        public static Geometry unknownIcon = Geometry.Parse("");
        public static Geometry checkingIcon = Geometry.Parse("");
        public static Geometry metadataIcon = Geometry.Parse("");

        // Enables filtering
        private bool _isVisible = true;
        private string _hash;
        public string Hash => _hash;

        /// <summary>
        /// Used to show the entries matching the filters
        /// </summary>
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (value != _isVisible)
                {
                    _isVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public TorrentInfoViewModel(TorrentPartialInfo torrentInfo, string hash)
        {
            _torrentInfo = torrentInfo;
            _hash = hash;
        }

        public TorrentPartialInfo TorrentInfo
        {
            get
            {
                return _torrentInfo;
            }
        }

        /// <summary>
        /// The date and time when the torrent was added
        /// </summary>
        public DateTime? AddedOn
        {
            get { return _torrentInfo.AddedOn; }
            set
            {
                if (value != _torrentInfo.AddedOn)
                {
                    _torrentInfo.AddedOn = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(AddedOnHr));
                }
            }
        }

        /// <summary>
        /// The date and time when the torrent was completed
        /// </summary>
        public DateTime? CompletionOn
        {
            get { return _torrentInfo.CompletionOn; }
            set
            {
                if (value != _torrentInfo.CompletionOn)
                {
                    _torrentInfo.CompletionOn = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The remaining amount to download (bytes)<br/><br/>
        /// <b>Note:</b> in the json it's <c>'amount_left'</c>
        /// </summary>
        public long? IncompletedSize
        {
            get { return _torrentInfo.IncompletedSize; }
            set
            {
                if (value != _torrentInfo.IncompletedSize)
                {
                    _torrentInfo.IncompletedSize = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool? AutoTmm
        {
            get { return _torrentInfo.AutomaticTorrentManagement; }
            set
            {
                if (value != _torrentInfo.AutomaticTorrentManagement)
                {
                    _torrentInfo.AutomaticTorrentManagement = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? Category
        {
            get { return _torrentInfo.Category; }
            set
            {
                if (value != _torrentInfo.Category)
                {
                    _torrentInfo.Category = value;
                    OnPropertyChanged();
                }
            }
        }

        public int? DlLimit
        {
            get { return _torrentInfo.DownloadLimit; }
            set
            {
                if (value != _torrentInfo.DownloadLimit)
                {
                    _torrentInfo.DownloadLimit = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DlLimitHr));
                }
            }
        }

        public long? DlSpeed
        {
            get { return _torrentInfo.DownloadSpeed; }
            set
            {
                if (value != _torrentInfo.DownloadSpeed)
                {
                    _torrentInfo.DownloadSpeed = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? SavePath
        {
            get { return _torrentInfo.SavePath; }
            set
            {
                if (value != _torrentInfo.SavePath)
                {
                    _torrentInfo.SavePath = value;
                    OnPropertyChanged();
                }
            }
        }

        public long? Downloaded
        {
            get { return _torrentInfo.Downloaded; }
            set
            {
                if (value != _torrentInfo.Downloaded)
                {
                    _torrentInfo.Downloaded = value;
                    OnPropertyChanged();
                }
            }
        }

        public long? DownloadedInSession
        {
            get { return _torrentInfo.DownloadedInSession; }
            set
            {
                if (value != _torrentInfo.DownloadedInSession)
                {
                    _torrentInfo.DownloadedInSession = value;
                    OnPropertyChanged();
                }
            }
        }

        public TimeSpan? EstimatedTime
        {
            get { return _torrentInfo.EstimatedTime; }
            set
            {
                if (value != _torrentInfo.EstimatedTime)
                {
                    _torrentInfo.EstimatedTime = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(EtaHr));
                }
            }
        }

        public bool? FirstLastPiecePrioritized
        {
            get { return _torrentInfo.FirstLastPiecePrioritized; }
            set
            {
                if (value != _torrentInfo.FirstLastPiecePrioritized)
                {
                    _torrentInfo.FirstLastPiecePrioritized = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool? ForceStart
        {
            get { return _torrentInfo.ForceStart; }
            set
            {
                if (value != _torrentInfo.ForceStart)
                {
                    _torrentInfo.ForceStart = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? LastActivityTime
        {
            get { return _torrentInfo.LastActivityTime; }
            set
            {
                if (value != _torrentInfo.LastActivityTime)
                {
                    _torrentInfo.LastActivityTime = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(LastActivityHr));
                }
            }
        }

        public string? MagnetUri
        {
            get { return _torrentInfo.MagnetUri; }
            set
            {
                if (value != _torrentInfo.MagnetUri)
                {
                    _torrentInfo.MagnetUri = value;
                    OnPropertyChanged();
                }
            }
        }

        public double? MaxRatio
        {
            get { return _torrentInfo.RatioLimit; }
            set
            {
                if (value != _torrentInfo.RatioLimit)
                {
                    _torrentInfo.RatioLimit = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? Name
        {
            get { return _torrentInfo.Name; }
            set
            {
                if (value != _torrentInfo.Name)
                {
                    _torrentInfo.Name = value;
                    OnPropertyChanged();
                }
            }
        }

        public long? CompletedSize
        {
            get { return _torrentInfo.CompletedSize; }
            set
            {
                if (value != _torrentInfo.CompletedSize)
                {
                    _torrentInfo.CompletedSize = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Number of seeds connected to
        /// </summary>
        public int? ConnectedSeeds
        {
            get { return _torrentInfo.ConnectedSeeds; }
            set
            {
                if (value != _torrentInfo.ConnectedSeeds)
                {
                    _torrentInfo.ConnectedSeeds = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Number of seeds in the swarm
        /// </summary>
        public int? TotalSeeds
        {
            get { return _torrentInfo.TotalSeeds; }
            set
            {
                if (value != _torrentInfo.TotalSeeds)
                {
                    _torrentInfo.TotalSeeds = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Number of leechers connected to
        /// </summary>
        public int? ConnectedLeechers
        {
            get { return _torrentInfo.ConnectedLeechers; }
            set
            {
                if (value != _torrentInfo.ConnectedLeechers)
                {
                    _torrentInfo.ConnectedLeechers = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Number of leechers in the swarm
        /// </summary>
        public int? TotalLeechers
        {
            get { return _torrentInfo.TotalLeechers; }
            set
            {
                if (value != _torrentInfo.TotalLeechers)
                {
                    _torrentInfo.TotalLeechers = value;
                    OnPropertyChanged();
                }
            }
        }

        public int? Priority
        {
            get { return _torrentInfo.Priority; }
            set
            {
                if (value != _torrentInfo.Priority)
                {
                    _torrentInfo.Priority = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 0.00 to 1.00, Multiply by 100 for the percentage.
        /// </summary>
        public double? Progress
        {
            get { return _torrentInfo.Progress; }
            set
            {
                if (value != _torrentInfo.Progress)
                {
                    _torrentInfo.Progress = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Torrent share ratio. Max ratio value is <c>9999</c>. If actual ratio is greater than <c>9999</c>, <c>-1</c> is returned.
        /// </summary>
        public double? Ratio
        {
            get { return _torrentInfo.Ratio; }
            set
            {
                if (value != _torrentInfo.Ratio)
                {
                    _torrentInfo.Ratio = value;
                    OnPropertyChanged();
                }
            }
        }

        public double? RatioLimit
        {
            get { return _torrentInfo.RatioLimit; }
            set
            {
                if (value != _torrentInfo.RatioLimit)
                {
                    _torrentInfo.RatioLimit = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(RatioLimitHr));
                }
            }
        }

        public TimeSpan? SeedingTimeLimit
        {
            get { return _torrentInfo.SeedingTimeLimit; }
            set
            {
                if (value != _torrentInfo.SeedingTimeLimit)
                {
                    _torrentInfo.SeedingTimeLimit = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SeedingTimeHr));
                }
            }
        }

        /*public long? SeedingTimeLimit
        {
            get { return _torrentInfo.SeedingTimeLimit; }
            set
            {
                if (value != _torrentInfo.SeedingTimeLimit)
                {
                    _torrentInfo.SeedingTimeLimit = value;
                    OnPropertyChanged();
                }
            }
        }*/

        public DateTime? SeenComplete
        {
            get { return _torrentInfo.LastSeenComplete; }
            set
            {
                if (value != _torrentInfo.LastSeenComplete)
                {
                    _torrentInfo.LastSeenComplete = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SeenCompleteHr));
                }
            }
        }

        public bool? SequentialDownload
        {
            get { return _torrentInfo.SequentialDownload; }
            set
            {
                if (value != _torrentInfo.SequentialDownload)
                {
                    _torrentInfo.SequentialDownload = value;
                    OnPropertyChanged();
                }
            }
        }

        public long? Size
        {
            get { return _torrentInfo.Size; }
            set
            {
                if (value != _torrentInfo.Size)
                {
                    _torrentInfo.Size = value;
                    OnPropertyChanged();
                }
            }
        }

        public TorrentState? State
        {
            get { return _torrentInfo.State; }
            set
            {
                if (value != _torrentInfo.State)
                {
                    _torrentInfo.State = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StateHr));
                    OnPropertyChanged(nameof(StateIcon));
                    OnPropertyChanged(nameof(TorrentState));
                }
            }
        }

        public bool? SuperSeeding
        {
            get { return _torrentInfo.SuperSeeding; }
            set
            {
                if (value != _torrentInfo.SuperSeeding)
                {
                    _torrentInfo.SuperSeeding = value;
                    OnPropertyChanged();
                }
            }
        }

        public IReadOnlyCollection<string>? Tags
        {
            get { return _torrentInfo.Tags; }
            set
            {
                if (value != _torrentInfo.Tags)
                {
                    _torrentInfo.Tags = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TagsFlattened));
                }
            }
        }

        /// <summary>
        /// Retrieves <see cref="Tags"> but flattened to a comma seperated string.
        /// Or an empty string if there's no tags.
        /// </summary>
        public string TagsFlattened
        {
            get => String.Join(", ", Tags ?? []);
        }

        public TimeSpan? TimeActive
        {
            get { return _torrentInfo.ActiveTime; }
            set
            {
                if (value != _torrentInfo.ActiveTime)
                {
                    _torrentInfo.ActiveTime = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TimeActiveHr));
                    OnPropertyChanged(nameof(SeedingTimeHr)); // Used in concatenated string
                }
            }
        }

        public long? TotalSize
        {
            get { return _torrentInfo.TotalSize; }
            set
            {
                if (value != _torrentInfo.TotalSize)
                {
                    _torrentInfo.TotalSize = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? CurrentTracker
        {
            get { return _torrentInfo.CurrentTracker; }
            set
            {
                if (value != _torrentInfo.CurrentTracker)
                {
                    _torrentInfo.CurrentTracker = value;
                    OnPropertyChanged();
                }
            }
        }

        public int? UpLimit
        {
            get { return _torrentInfo.UploadLimit; }
            set
            {
                if (value != _torrentInfo.UploadLimit)
                {
                    _torrentInfo.UploadLimit = value;
                    OnPropertyChanged();
                }
            }
        }

        public long? Uploaded
        {
            get { return _torrentInfo.Uploaded; }
            set
            {
                if (value != _torrentInfo.Uploaded)
                {
                    _torrentInfo.Uploaded = value;
                    OnPropertyChanged();
                }
            }
        }

        public long? UploadedInSession
        {
            get { return _torrentInfo.UploadedInSession; }
            set
            {
                if (value != _torrentInfo.UploadedInSession)
                {
                    _torrentInfo.UploadedInSession = value;
                    OnPropertyChanged();
                }
            }
        }

        public long? UpSpeed
        {
            get { return _torrentInfo.UploadSpeed; }
            set
            {
                if (value != _torrentInfo.UploadSpeed)
                {
                    _torrentInfo.UploadSpeed = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Takes a new PartialInfo and assigns any new values to this instance.
        /// Since every set property fires its OnPropertyChanged these changes 
        /// should propogate to the UI.
        /// </summary>
        /// <param name="newPartialInfo"></param>
        public void Update(TorrentPartialInfo newPartialInfo)
        {
            AddedOn = newPartialInfo?.AddedOn ?? AddedOn;
            CompletionOn = newPartialInfo?.CompletionOn ?? CompletionOn;
            IncompletedSize = newPartialInfo?.IncompletedSize ?? IncompletedSize;
            AutoTmm = newPartialInfo?.AutomaticTorrentManagement ?? AutoTmm;
            Category = newPartialInfo?.Category ?? Category;
            DlLimit = newPartialInfo?.DownloadLimit ?? DlLimit;
            DlSpeed = newPartialInfo?.DownloadSpeed ?? DlSpeed;
            SavePath = newPartialInfo?.SavePath ?? SavePath;
            Downloaded = newPartialInfo?.Downloaded ?? Downloaded;
            DownloadedInSession = newPartialInfo?.DownloadedInSession ?? DownloadedInSession;
            EstimatedTime = newPartialInfo?.EstimatedTime ?? EstimatedTime;
            FirstLastPiecePrioritized = newPartialInfo?.FirstLastPiecePrioritized ?? FirstLastPiecePrioritized;
            FirstLastPiecePrioritized = newPartialInfo?.ForceStart ?? FirstLastPiecePrioritized;
            LastActivityTime = newPartialInfo?.LastActivityTime ?? LastActivityTime;
            MagnetUri = newPartialInfo?.MagnetUri ?? MagnetUri;
            MaxRatio = newPartialInfo?.RatioLimit ?? MaxRatio;
            Name = newPartialInfo?.Name ?? Name;
            CompletedSize = newPartialInfo?.CompletedSize ?? CompletedSize;
            ConnectedSeeds = newPartialInfo?.ConnectedSeeds ?? ConnectedSeeds;
            TotalSeeds = newPartialInfo?.TotalSeeds ?? TotalSeeds;
            ConnectedLeechers = newPartialInfo?.ConnectedLeechers ?? ConnectedLeechers;
            TotalLeechers = newPartialInfo?.TotalLeechers ?? TotalLeechers;
            Priority = newPartialInfo?.Priority ?? Priority;
            Progress = newPartialInfo?.Progress ?? Progress;
            Ratio = newPartialInfo?.Ratio ?? Ratio;
            RatioLimit = newPartialInfo?.RatioLimit ?? RatioLimit;
            SeedingTimeLimit = newPartialInfo?.SeedingTimeLimit ?? SeedingTimeLimit;
            SeenComplete = newPartialInfo?.LastSeenComplete ?? SeenComplete;
            SequentialDownload = newPartialInfo?.SequentialDownload ?? SequentialDownload;
            Size = newPartialInfo?.Size ?? Size;
            State = newPartialInfo?.State ?? State;
            SuperSeeding = newPartialInfo?.SuperSeeding ?? SuperSeeding;
            Tags = newPartialInfo?.Tags ?? Tags;
            TimeActive = newPartialInfo?.ActiveTime ?? TimeActive;
            TotalSize = newPartialInfo?.TotalSize ?? TotalSize;
            CurrentTracker = newPartialInfo?.CurrentTracker ?? CurrentTracker;
            UpLimit = newPartialInfo?.UploadLimit ?? UpLimit;
            Uploaded = newPartialInfo?.Uploaded ?? Uploaded;
            UploadedInSession = newPartialInfo?.UploadedInSession ?? UploadedInSession;
            UpSpeed = newPartialInfo?.UploadSpeed ?? UpSpeed;
        }

        // Add a property for the human-readable size.
        public string AddedOnHr => AddedOn?.ToString("dd/MM/yyyy HH:mm:ss") ?? "";
        public string CompletedOnHr => CompletionOn?.ToString("dd/MM/yyyy HH:mm:ss") ?? "";
        public string EtaHr => DataConverter.TimeSpanToHumanReadable(EstimatedTime);
        public string SeedingTimeHr => $"{TimeActiveHr} (Seeded for {DataConverter.TimeSpanToHumanReadable(SeedingTimeLimit)})";
        public string UploadedSessionHr => DataConverter.BytesToHumanReadable(UploadedInSession);
        public string TimeActiveHr => DataConverter.TimeSpanToHumanReadable(TimeActive);
        public string SeenCompleteHr => CompletionOn?.ToString("dd/MM/yyyy HH:mm:ss") ?? "";
        public long? DlLimitHr => DlLimit;

        public string LastActivityHr
        {
            get
            {
                if (LastActivityTime is DateTime)
                {
                    TimeSpan timeElapsed = (TimeSpan)(DateTime.Now - LastActivityTime);
                    return string.Format("{0}h {1}m ago", timeElapsed.Hours, timeElapsed.Minutes);
                }
                else
                    return "";

            }
        }
        // https://gist.github.com/pmzqla/7e5733dbecfc50ee4ecd/861099d74a0a979c1ce080593a9590c96217f924#file-web-api-documentation-L81
        public string StateHr
        {
            get
            {
                var prefix = "";
                if (ForceStart is true)
                    prefix = "[F] ";
                switch (State)
                {
                    case TorrentState.Allocating:
                        return "Allocating space";
                    case TorrentState.Error:
                        return $"{prefix}Error";
                    case TorrentState.PausedDownload:
                    case TorrentState.PausedUpload:
                        return $"{prefix}Paused";
                    case TorrentState.QueuedDownload:
                    case TorrentState.QueuedUpload:
                    case TorrentState.QueuedForChecking:
                        return $"Queued";
                    case TorrentState.Uploading:
                        return $"{prefix}Uploading";
                    case TorrentState.StalledUpload:
                    case TorrentState.ForcedUpload:
                        return $"{prefix}Seeding";
                    case TorrentState.CheckingDownload:
                    case TorrentState.CheckingUpload:
                    case TorrentState.CheckingResumeData:
                        return $"{prefix}Checking";
                    case TorrentState.Downloading:
                        return $"{prefix}Downloading";
                    case TorrentState.StalledDownload:
                        return $"{prefix}Stalled";
                    case TorrentState.MissingFiles:
                        return "Missing files";
                    case TorrentState.FetchingMetadata:
                        return "Fetching metadata";
                    case TorrentState.ForcedFetchingMetadata:
                        return "[F] Fetching metadata";
                    case TorrentState.Moving:
                        return "Moving";
                    default:
                        return $"{prefix}Unknown";
                }
            }
        }

        public FluentIcons.Common.Symbol StateIcon
        {
            get
            {
                switch (State)
                {
                    case TorrentState.Allocating:
                    case TorrentState.Moving:
                        return FluentIcons.Common.Symbol.Storage;
                    case TorrentState.ForcedDownload:
                    case TorrentState.Downloading:
                        return FluentIcons.Common.Symbol.ArrowDownload;
                    case TorrentState.StalledUpload:
                    case TorrentState.Uploading:
                    case TorrentState.ForcedUpload:
                        return FluentIcons.Common.Symbol.ArrowUpload;
                    case TorrentState.PausedUpload:
                    case TorrentState.PausedDownload:
                        return FluentIcons.Common.Symbol.Pause;
                    case TorrentState.QueuedUpload:
                    case TorrentState.QueuedDownload:
                        return FluentIcons.Common.Symbol.Clock;
                    case TorrentState.MissingFiles:
                    case TorrentState.Error:
                        return FluentIcons.Common.Symbol.ErrorCircle;
                    case TorrentState.Unknown:
                        return FluentIcons.Common.Symbol.QuestionCircle; 
                    case TorrentState.CheckingUpload:
                    case TorrentState.CheckingDownload:
                    case TorrentState.QueuedForChecking:
                    case TorrentState.CheckingResumeData:
                        return FluentIcons.Common.Symbol.Checkmark;
                    case TorrentState.FetchingMetadata:
                    case TorrentState.ForcedFetchingMetadata:
                        return FluentIcons.Common.Symbol.TagQuestionMark;
                    default:
                        return FluentIcons.Common.Symbol.QuestionCircle;
                }
            }
        }

        public string RatioLimitHr
        {
            get
            {
                switch (RatioLimit)
                {
                    case -2:
                        return "∞";
                    case -1:
                        return "Global";
                    default:
                        return Ratio.ToString() ?? "";
                }
            }
        }

    }
}