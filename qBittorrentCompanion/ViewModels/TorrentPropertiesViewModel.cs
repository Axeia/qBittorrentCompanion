using System;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels
{
    using QBittorrent.Client;
    using qBittorrentCompanion.Helpers;
    using qBittorrentCompanion.Models;
    using qBittorrentCompanion.Services;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Timers;

    public class TorrentPropertiesViewModel : INotifyPropertyChanged
    {
        private TorrentProperties? _torrentProperties;
        private string _infoHash = "";
        private System.Timers.Timer _refreshTimer = new();

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public TorrentPropertiesViewModel(TorrentInfoViewModel? torrentInfoViewModel, long interval = 1500)
        {
            // Eventually populates _torrentProperties
            if (torrentInfoViewModel is not null && torrentInfoViewModel.Hash is not null)
            {
                _infoHash = torrentInfoViewModel.Hash.ToString();
                _ = FetchDataAsync();

                _refreshTimer.Interval = interval;
                _refreshTimer.Elapsed += UpdateDataAsync;
            }

        }

        private async void UpdateDataAsync(object? sender, ElapsedEventArgs e)
        {
            TorrentProperties torrentProperties;
            torrentProperties = await QBittorrentService.QBittorrentClient.GetTorrentPropertiesAsync(_infoHash);


            // Torrent must have gotten deleted, no need to update anymore.
            if (torrentProperties is null)
            {
                Debug.WriteLine($"{_infoHash} Must have gotten deleted. Stopping updates");
                _refreshTimer.Stop();
                return;
            }

            if (_torrentProperties is not null)
            {
                _torrentProperties.AdditionDate = torrentProperties.AdditionDate;
                _torrentProperties.Comment = torrentProperties.Comment;
                _torrentProperties.CompletionDate = torrentProperties.CompletionDate;
                _torrentProperties.CreatedBy = torrentProperties.CreatedBy;
                _torrentProperties.CreationDate = torrentProperties.CreationDate;
                _torrentProperties.DownloadLimit = torrentProperties.DownloadLimit;
                _torrentProperties.DownloadSpeed = torrentProperties.DownloadSpeed;
                _torrentProperties.AverageDownloadSpeed = torrentProperties.AverageDownloadSpeed;
                _torrentProperties.EstimatedTime = torrentProperties.EstimatedTime;
                //_torrentProperties.Hash = _infoHash;
                _torrentProperties.LastSeen = torrentProperties.LastSeen;
                _torrentProperties.ConnectionCount = torrentProperties.ConnectionCount;
                _torrentProperties.ConnectionLimit = torrentProperties.ConnectionLimit;
                _torrentProperties.Peers = torrentProperties.Peers;
                _torrentProperties.TotalPeers = torrentProperties.TotalPeers;
                _torrentProperties.PieceSize = torrentProperties.PieceSize;
                _torrentProperties.OwnedPieces = torrentProperties.OwnedPieces;
                _torrentProperties.TotalPieces = torrentProperties.TotalPieces;
                _torrentProperties.Reannounce = torrentProperties.Reannounce;
                _torrentProperties.SavePath = torrentProperties.SavePath;
                _torrentProperties.SeedingTime = torrentProperties.SeedingTime;
                _torrentProperties.Seeds = torrentProperties.Seeds;
                _torrentProperties.TotalSeeds = torrentProperties.TotalSeeds;
                _torrentProperties.ShareRatio = torrentProperties.ShareRatio;
                _torrentProperties.TimeElapsed = torrentProperties.TimeElapsed;
                _torrentProperties.TotalDownloaded = torrentProperties.TotalDownloaded;
                _torrentProperties.TotalDownloadedInSession = torrentProperties.TotalDownloadedInSession;
                _torrentProperties.Size = torrentProperties.Size;
                _torrentProperties.TotalUploaded = torrentProperties.TotalUploaded;
                _torrentProperties.TotalUploadedInSession = torrentProperties.TotalUploadedInSession;
                _torrentProperties.TotalWasted = torrentProperties.TotalWasted;
                _torrentProperties.UploadLimit = torrentProperties.UploadLimit;
                _torrentProperties.UploadSpeed = torrentProperties.UploadSpeed;
            }
        }

        public async Task FetchDataAsync()
        {
            _torrentProperties = await QBittorrentService.QBittorrentClient.GetTorrentPropertiesAsync(_infoHash);
            OnPropertyChanged(nameof(AdditionDate));
            OnPropertyChanged(nameof(Comment));
            OnPropertyChanged(nameof(CompletionDate));
            OnPropertyChanged(nameof(CreatedBy));
            OnPropertyChanged(nameof(CreationDate));
            OnPropertyChanged(nameof(DownloadLimit));
            OnPropertyChanged(nameof(DownloadSpeed));
            OnPropertyChanged(nameof(Eta));
            OnPropertyChanged(nameof(LastSeen));
            OnPropertyChanged(nameof(NumberOfConnections));
            OnPropertyChanged(nameof(NumberOfConnectionsLimit));
            OnPropertyChanged(nameof(Peers));
            OnPropertyChanged(nameof(TotalPeers));
            OnPropertyChanged(nameof(PieceSize));
            OnPropertyChanged(nameof(PiecesHave));
            OnPropertyChanged(nameof(PiecesNumber));
            OnPropertyChanged(nameof(Reannounce));
            OnPropertyChanged(nameof(SavePath));
            OnPropertyChanged(nameof(SeedingTime));
            OnPropertyChanged(nameof(Seeds));
            OnPropertyChanged(nameof(TotalSeeds));
            OnPropertyChanged(nameof(ShareRatio));
            OnPropertyChanged(nameof(TimeElapsed));
            OnPropertyChanged(nameof(TotalDownloaded));
            OnPropertyChanged(nameof(TotalDownloadedSession));
            OnPropertyChanged(nameof(Size));
            OnPropertyChanged(nameof(TotalUploaded));
            OnPropertyChanged(nameof(TotalUploadedSession));
            OnPropertyChanged(nameof(TotalWasted));
            OnPropertyChanged(nameof(UploadLimit));
            OnPropertyChanged(nameof(UploadSpeed));

            _refreshTimer.Start();
        }

        public void Pause()
        {
            _refreshTimer.Stop();
        }

        public DateTime? AdditionDate
        {
            get { return _torrentProperties?.AdditionDate; }
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.AdditionDate)
                {
                    _torrentProperties.AdditionDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? Comment
        {
            get { return _torrentProperties?.Comment; }
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.Comment)
                {
                    _torrentProperties.Comment = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? CompletionDate
        {
            get { return _torrentProperties?.CompletionDate; }
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.CompletionDate)
                {
                    _torrentProperties.CompletionDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? CreatedBy
        {
            get { return _torrentProperties?.CreatedBy; }
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.CreatedBy)
                {
                    _torrentProperties.CreatedBy = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? CreationDate
        {
            get { return _torrentProperties?.CreationDate; }
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.CreationDate)
                {
                    _torrentProperties.CreationDate = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CreationDateHr));
                }
            }
        }

        public long? DownloadLimit
        {
            get { return _torrentProperties?.DownloadLimit; }
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.DownloadLimit)
                {
                    _torrentProperties.DownloadLimit = value;
                    OnPropertyChanged();
                }
            }
        }

        public long? DownloadSpeed
        {
            get { return _torrentProperties?.DownloadSpeed; }
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.DownloadSpeed)
                {
                    _torrentProperties.DownloadSpeed = value;
                    OnPropertyChanged();
                }
            }
        }

        public TimeSpan? Eta
        {
            get { return _torrentProperties?.EstimatedTime; }
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.EstimatedTime)
                {
                    _torrentProperties.EstimatedTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? LastSeen
        {
            get { return _torrentProperties?.LastSeen; }
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.LastSeen)
                {
                    _torrentProperties.LastSeen = value;
                    OnPropertyChanged();
                }
            }
        }

        public int? NumberOfConnections
        {
            get { return _torrentProperties?.ConnectionCount; }
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.ConnectionCount)
                {
                    _torrentProperties.ConnectionCount = value;
                    OnPropertyChanged();
                }
            }
        }

        public int? NumberOfConnectionsLimit
        {
            get { return _torrentProperties?.ConnectionLimit; }
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.ConnectionLimit)
                {
                    _torrentProperties.ConnectionLimit = value;
                    OnPropertyChanged();
                }
            }
        }

        public int? Peers
        {
            get { return _torrentProperties?.Peers; }
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.Peers)
                {
                    _torrentProperties.Peers = value;
                    OnPropertyChanged();
                }
            }
        }

        public int? TotalPeers
        {
            get { return _torrentProperties?.TotalPeers; }
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.TotalPeers)
                {
                    _torrentProperties.TotalPeers = value;
                    OnPropertyChanged();
                }
            }
        }

        public long? PieceSize
        {
            get { return _torrentProperties?.PieceSize; }
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.PieceSize)
                {
                    _torrentProperties.PieceSize = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PieceSizeHr));
                }
            }
        }

        public int? PiecesHave
        {
            get { return _torrentProperties?.OwnedPieces; }
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.OwnedPieces)
                {
                    _torrentProperties.OwnedPieces = value;
                    OnPropertyChanged();
                }
            }
        }

        public long? PiecesNumber
        {
            get { return _torrentProperties?.PieceSize; }
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.PieceSize)
                {
                    _torrentProperties.PieceSize = value;
                    OnPropertyChanged();
                }
            }
        }

        public TimeSpan? Reannounce
        {
            get { return _torrentProperties?.Reannounce; }
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.Reannounce)
                {
                    _torrentProperties.Reannounce = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ReannounceHr));
                }
            }
        }

        public string? SavePath
        {
            get { return _torrentProperties?.SavePath; }
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.SavePath)
                {
                    _torrentProperties.SavePath = value;
                    OnPropertyChanged();
                }
            }
        }

        public TimeSpan? SeedingTime
        {
            get { return _torrentProperties?.SeedingTime; }
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.SeedingTime)
                {
                    _torrentProperties.SeedingTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public int? Seeds
        {
            get { return _torrentProperties?.Seeds; }
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.Seeds)
                {
                    _torrentProperties.Seeds = value;
                    OnPropertyChanged();
                }
            }
        }

        public int? TotalSeeds
        {
            get { return _torrentProperties?.TotalSeeds; }
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.TotalSeeds)
                {
                    _torrentProperties.TotalSeeds = value;
                    OnPropertyChanged();
                }
            }
        }

        public double ShareRatio
        {
            get { return _torrentProperties?.ShareRatio ?? 0; }
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.ShareRatio)
                {
                    _torrentProperties.ShareRatio = value;
                    OnPropertyChanged();
                }
            }
        }

        public TimeSpan? TimeElapsed
        {
            get { return _torrentProperties?.TimeElapsed; }
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.TimeElapsed)
                {
                    _torrentProperties.TimeElapsed = value;
                    OnPropertyChanged();
                }
            }
        }

        public long? TotalDownloaded
        {
            get { return _torrentProperties?.TotalDownloaded; }
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.TotalDownloaded)
                {
                    _torrentProperties.TotalDownloaded = value;
                    OnPropertyChanged();
                }
            }
        }

        public long? TotalDownloadedSession
        {
            get { return _torrentProperties?.TotalDownloadedInSession; }
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.TotalDownloadedInSession)
                {
                    _torrentProperties.TotalDownloadedInSession = value;
                    OnPropertyChanged();
                }
            }
        }

        public long? Size
        {
            get { return _torrentProperties?.Size; }
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.Size)
                {
                    _torrentProperties.Size = value;
                    OnPropertyChanged();
                }
            }
        }

        public long? TotalUploaded
        {
            get { return _torrentProperties?.TotalUploaded; }
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.TotalUploaded)
                {
                    _torrentProperties.TotalUploaded = value;
                    OnPropertyChanged();
                }
            }
        }

        public long? TotalUploadedSession
        {
            get { return _torrentProperties?.TotalUploadedInSession; }
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.TotalUploadedInSession)
                {
                    _torrentProperties.TotalUploadedInSession = value;
                    OnPropertyChanged();
                }
            }
        }

        public long? TotalWasted
        {
            get { return _torrentProperties?.TotalWasted; }
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.TotalWasted)
                {
                    _torrentProperties.TotalWasted = value;
                    OnPropertyChanged();
                }
            }
        }

        public long? UploadLimit
        {
            get { return _torrentProperties?.UploadLimit; }
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.UploadLimit)
                {
                    _torrentProperties.UploadLimit = value;
                    OnPropertyChanged();
                }
            }
        }

        public long? UploadSpeed
        {
            get { return _torrentProperties?.UploadSpeed; }
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.UploadSpeed)
                {
                    _torrentProperties.UploadSpeed = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? PieceSizeHr => PieceSize is null ? null : DataConverter.BytesToHumanReadable(PieceSize);
        public string? ReannounceHr => Reannounce is null ? null : DataConverter.TimeSpanToHumanReadable(Reannounce);
        public string? CreationDateHr => CreationDate?.ToString("dd/MM/yyyy HH:mm:ss");
    }
}