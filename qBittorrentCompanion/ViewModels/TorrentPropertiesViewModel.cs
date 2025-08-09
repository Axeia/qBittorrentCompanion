using System;
using System.Threading.Tasks;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Services;
using System.Diagnostics;
using System.Timers;
using AutoPropertyChangedGenerator;
using ReactiveUI;

namespace qBittorrentCompanion.ViewModels
{
    public partial class TorrentPropertiesViewModel : AutoUpdateViewModelBase
    {
        [AutoProxyPropertyChanged(nameof(TorrentProperties.AdditionDate))]
        [AutoProxyPropertyChanged(nameof(TorrentProperties.Comment))]
        [AutoProxyPropertyChanged(nameof(TorrentProperties.CompletionDate))]
        [AutoProxyPropertyChanged(nameof(TorrentProperties.CreatedBy))]
        [AutoProxyPropertyChanged(nameof(TorrentProperties.DownloadLimit))]
        [AutoProxyPropertyChanged(nameof(TorrentProperties.DownloadSpeed))]
        [AutoProxyPropertyChanged(nameof(TorrentProperties.EstimatedTime), "Eta")]
        [AutoProxyPropertyChanged(nameof(TorrentProperties.LastSeen))]
        [AutoProxyPropertyChanged(nameof(TorrentProperties.CreationDate))]
        [AutoProxyPropertyChanged(nameof(TorrentProperties.ConnectionCount), "NumberOfConnections")]
        [AutoProxyPropertyChanged(nameof(TorrentProperties.ConnectionLimit), "NumberOfConnectionsLimit")]
        [AutoProxyPropertyChanged(nameof(TorrentProperties.Peers))]
        [AutoProxyPropertyChanged(nameof(TorrentProperties.TotalPeers))]
        [AutoProxyPropertyChanged(nameof(TorrentProperties.OwnedPieces), "PiecesHave")]
        [AutoProxyPropertyChanged(nameof(TorrentProperties.PieceSize), "PiecesNumber")]
        [AutoProxyPropertyChanged(nameof(TorrentProperties.SavePath))]
        [AutoProxyPropertyChanged(nameof(TorrentProperties.SeedingTime))]
        [AutoProxyPropertyChanged(nameof(TorrentProperties.Seeds))]
        [AutoProxyPropertyChanged(nameof(TorrentProperties.TotalSeeds))]
        [AutoProxyPropertyChanged(nameof(TorrentProperties.TimeElapsed))]
        [AutoProxyPropertyChanged(nameof(TorrentProperties.TotalDownloaded))]
        [AutoProxyPropertyChanged(nameof(TorrentProperties.TotalDownloadedInSession))]
        [AutoProxyPropertyChanged(nameof(TorrentProperties.Size))]
        [AutoProxyPropertyChanged(nameof(TorrentProperties.TotalUploaded))]
        [AutoProxyPropertyChanged(nameof(TorrentProperties.TotalUploadedInSession))]
        [AutoProxyPropertyChanged(nameof(TorrentProperties.TotalWasted))]
        [AutoProxyPropertyChanged(nameof(TorrentProperties.UploadLimit))]
        [AutoProxyPropertyChanged(nameof(TorrentProperties.UploadSpeed))]
        private TorrentProperties _torrentProperties = new();

        public TorrentPropertiesViewModel(TorrentInfoViewModel torrentInfoViewModel, long interval = 1500)
        {
            // Eventually populates _torrentProperties
            if (torrentInfoViewModel is not null && torrentInfoViewModel.Hash is not null)
            {
                _infoHash = torrentInfoViewModel.Hash.ToString();
                _refreshTimer.Interval = TimeSpan.FromMilliseconds(interval);
                _ = FetchDataAsync();
            }
        }

        protected override async Task UpdateDataAsync(object? sender, EventArgs e)
        {
            TorrentProperties? torrentProperties;
            torrentProperties = await QBittorrentService.GetTorrentPropertiesAsync(_infoHash);


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

        protected override async Task FetchDataAsync()
        {
            if(await QBittorrentService.GetTorrentPropertiesAsync(_infoHash) is TorrentProperties torrentProperties)
            {
                _torrentProperties = torrentProperties;
                this.RaisePropertyChanged(nameof(AdditionDate));
                this.RaisePropertyChanged(nameof(Comment));
                this.RaisePropertyChanged(nameof(CompletionDate));
                this.RaisePropertyChanged(nameof(CreatedBy));
                this.RaisePropertyChanged(nameof(CreationDate));
                this.RaisePropertyChanged(nameof(DownloadLimit));
                this.RaisePropertyChanged(nameof(DownloadSpeed));
                this.RaisePropertyChanged(nameof(Eta));
                this.RaisePropertyChanged(nameof(LastSeen));
                this.RaisePropertyChanged(nameof(NumberOfConnections));
                this.RaisePropertyChanged(nameof(NumberOfConnectionsLimit));
                this.RaisePropertyChanged(nameof(Peers));
                this.RaisePropertyChanged(nameof(TotalPeers));
                this.RaisePropertyChanged(nameof(PieceSize));
                this.RaisePropertyChanged(nameof(PiecesHave));
                this.RaisePropertyChanged(nameof(PiecesNumber));
                this.RaisePropertyChanged(nameof(Reannounce));
                this.RaisePropertyChanged(nameof(SavePath));
                this.RaisePropertyChanged(nameof(SeedingTime));
                this.RaisePropertyChanged(nameof(Seeds));
                this.RaisePropertyChanged(nameof(TotalSeeds));
                this.RaisePropertyChanged(nameof(ShareRatio));
                this.RaisePropertyChanged(nameof(TimeElapsed));
                this.RaisePropertyChanged(nameof(TotalDownloaded));
                this.RaisePropertyChanged(nameof(TotalDownloadedInSession));
                this.RaisePropertyChanged(nameof(Size));
                this.RaisePropertyChanged(nameof(TotalUploaded));
                this.RaisePropertyChanged(nameof(TotalUploadedInSession));
                this.RaisePropertyChanged(nameof(TotalWasted));
                this.RaisePropertyChanged(nameof(UploadLimit));
                this.RaisePropertyChanged(nameof(UploadSpeed));
            }
            else
            {
                _torrentProperties = new TorrentProperties();
            }

            _refreshTimer.Start();
        }

        public long? PieceSize
        {
            get => _torrentProperties?.PieceSize;
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.PieceSize)
                {
                    _torrentProperties.PieceSize = value;
                    this.RaisePropertyChanged(nameof(PieceSize));
                    this.RaisePropertyChanged(nameof(PieceSizeHr));
                }
            }
        }

        public TimeSpan? Reannounce
        {
            get => _torrentProperties?.Reannounce;
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.Reannounce)
                {
                    _torrentProperties.Reannounce = value;
                    this.RaisePropertyChanged(nameof(Reannounce));
                    this.RaisePropertyChanged(nameof(ReannounceHr));
                }
            }
        }

        public double ShareRatio
        {
            get => _torrentProperties?.ShareRatio ?? 0;
            set
            {
                if (_torrentProperties is not null && value != _torrentProperties.ShareRatio)
                {
                    _torrentProperties.ShareRatio = value;
                    this.RaisePropertyChanged(nameof(ShareRatio));
                }
            }
        }

        public string? PieceSizeHr => PieceSize is null ? null : DataConverter.BytesToHumanReadable(PieceSize);
        public string? ReannounceHr => Reannounce is null ? null : DataConverter.TimeSpanToHumanReadable(Reannounce);
        public string? CreationDateHr => CreationDate?.ToString("dd/MM/yyyy HH:mm:ss");
    }
}