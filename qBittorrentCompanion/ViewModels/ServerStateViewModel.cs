using Avalonia.Media;
using Avalonia.OpenGL;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace qBittorrentCompanion.ViewModels
{
    public class ServerStateViewModel : INotifyPropertyChanged
    {
        private GlobalTransferExtendedInfo _serverState;
        public static Geometry ConnectedIcon = Geometry.Parse("");
        public static Geometry OfflineIcon = Geometry.Parse("");
        public static Geometry FirewalledIcon = Geometry.Parse("");
        public static Geometry StalledIcon = Geometry.Parse("");
        public static Geometry UnknownICon = Geometry.Parse("");

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ServerStateViewModel(GlobalTransferExtendedInfo serverState)
        {
            _serverState = serverState;
        }

        public GlobalTransferExtendedInfo ServerState { get => _serverState; }

        public long? AllTimeDl
        {
            get { return _serverState.AllTimeDownloaded; }
            set
            {
                if (value != _serverState.AllTimeDownloaded)
                {
                    _serverState.AllTimeDownloaded = value;
                    OnPropertyChanged();
                }
            }
        }

        public long? AllTimeUl
        {
            get { return _serverState.AllTimeUploaded; }
            set
            {
                if (value != _serverState.AllTimeUploaded)
                {
                    _serverState.AllTimeUploaded = value;
                    OnPropertyChanged();
                }
            }
        }

        public ConnectionStatus? ConnectionStatus
        {
            get { return _serverState.ConnectionStatus; }
            set
            {
                if (value != _serverState.ConnectionStatus)
                {
                    _serverState.ConnectionStatus = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ConnectionStatusIcon));
                }
            }
        }

        public long? DhtNodes
        {
            get { return _serverState.DhtNodes; }
            set
            {
                if (value != _serverState.DhtNodes)
                {
                    _serverState.DhtNodes = value;
                    OnPropertyChanged();
                }
            }
        }

        public long? DlInfoData
        {
            get { return _serverState.DownloadedData; }
            set
            {
                if (value != _serverState.DownloadedData)
                {
                    _serverState.DownloadedData = value;
                    OnPropertyChanged();
                }
            }
        }

        public long? DlInfoSpeed
        {
            get { return _serverState.DownloadSpeed; }
            set
            {
                if (value != _serverState.DownloadSpeed)
                {
                    _serverState.DownloadSpeed = value;
                    OnPropertyChanged();
                }
            }
        }

        public long? DlRateLimit
        {
            get { return _serverState.DownloadSpeedLimit; }
            set
            {
                if (value != _serverState.DownloadSpeedLimit)
                {
                    _serverState.DownloadSpeedLimit = value;
                    OnPropertyChanged();
                }
            }
        }

        public long? FreeSpaceOnDisk
        {
            get { return _serverState.FreeSpaceOnDisk; }
            set
            {
                if (value != _serverState.FreeSpaceOnDisk)
                {
                    _serverState.FreeSpaceOnDisk = value;
                    OnPropertyChanged();
                }
            }
        }

        public double? GlobalRatio
        {
            get { return _serverState.GlobalRatio; }
        }

        public int? RefreshInterval
        {
            get { return _serverState.RefreshInterval; }
            set
            {
                if (value != _serverState.RefreshInterval)
                {
                    _serverState.RefreshInterval = value;
                    OnPropertyChanged();
                }
            }
        }

        public long? TotalBuffersSize
        {
            get { return _serverState.TotalBuffersSize; }
            set
            {
                if (value != _serverState.TotalBuffersSize)
                {
                    _serverState.TotalBuffersSize = value;
                    OnPropertyChanged();
                }
            }
        }

        public long? TotalPeerConnections
        {
            get { return _serverState.TotalPeerConnections; }
            set
            {
                if (value != _serverState.TotalPeerConnections)
                {
                    _serverState.TotalPeerConnections = value;
                    OnPropertyChanged();
                }
            }
        }

        public long? UpInfoData
        {
            get { return _serverState.UploadedData; }
            set
            {
                if (value != _serverState.UploadedData)
                {
                    _serverState.UploadedData = value;
                    OnPropertyChanged();
                }
            }
        }

        public long? UpInfoSpeed
        {
            get { return _serverState.UploadSpeed; }
            set
            {
                if (value != _serverState.UploadSpeed)
                {
                    _serverState.UploadSpeed = value;
                    OnPropertyChanged();
                }
            }
        }

        public long? UpRateLimit
        {
            get { return _serverState.UploadSpeedLimit; }
            set
            {
                if (value != _serverState.UploadSpeedLimit)
                {
                    _serverState.UploadSpeedLimit = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool UseAltSpeedLimits
        {
            get { return _serverState.GlobalAltSpeedLimitsEnabled; }
            set
            {
                if (value != _serverState.GlobalAltSpeedLimitsEnabled)
                {
                    _serverState.GlobalAltSpeedLimitsEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public Geometry ConnectionStatusIcon{
            get
            {
                return ConnectionStatus switch
                {
                    QBittorrent.Client.ConnectionStatus.Connected => ServerStateViewModel.ConnectedIcon,
                    QBittorrent.Client.ConnectionStatus.Disconnected => ServerStateViewModel.OfflineIcon,
                    QBittorrent.Client.ConnectionStatus.Firewalled => ServerStateViewModel.FirewalledIcon,
                    _ => ServerStateViewModel.UnknownICon,
                };
            }
        }

    }
}