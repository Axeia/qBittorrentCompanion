using Avalonia.Controls;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels
{
    public class ServerStateViewModel : INotifyPropertyChanged
    {
        public static string[] SizeOptions => BytesBaseViewModel.SizeOptions.Take(3).ToArray();

        private GlobalTransferExtendedInfo _serverState;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ReactiveCommand<Unit, Unit> SaveDisplayDlRateLimitCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> SaveDisplayUpRateLimitCommand { get; private set; }

        public ServerStateViewModel(GlobalTransferExtendedInfo serverState)
        {
            _serverState = serverState;
            SaveDisplayDlRateLimitCommand = ReactiveCommand.CreateFromTask(SaveDisplayDlRateLimit);
            SaveDisplayUpRateLimitCommand = ReactiveCommand.CreateFromTask(SaveDisplayUpRateLimit);
        }

        private async Task SaveDisplayDlRateLimit()
        {
            try
            {
                if (DlRateLimit is long dlL)
                    await QBittorrentService.QBittorrentClient.SetGlobalDownloadLimitAsync(dlL);
            }
            catch(Exception e){ Debug.WriteLine(e.Message); }
        }

        private async Task SaveDisplayUpRateLimit()
        {
            try
            {
                if (UpRateLimit is long upL)
                    await QBittorrentService.QBittorrentClient.SetGlobalUploadLimitAsync(upL);
            }
            catch(Exception e){ Debug.WriteLine(e.Message); }
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
                    OnPropertyChanged(nameof(AllTimeDl));
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
                    OnPropertyChanged(nameof(AllTimeUl));
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
                    OnPropertyChanged(nameof(ConnectionStatus));
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
                    OnPropertyChanged(nameof(DhtNodes));
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
                    OnPropertyChanged(nameof(DlInfoData));
                }
            }
        }

        private ObservableCollection<long> _dlInfoSpeedDataY = new ObservableCollection<long>();
        public ObservableCollection<long> DlInfoSpeedDataY
        {
            get => _dlInfoSpeedDataY;
            set
            {
                if (value != _dlInfoSpeedDataY)
                {
                    _dlInfoSpeedDataY = value;
                    OnPropertyChanged(nameof(DlInfoSpeedDataY));
                }
            }
        }

        public long? DlInfoSpeed
        {
            get { return _serverState.DownloadSpeed; }
            set
            {
                // Don't need more than 100 entries
                if (_dlInfoSpeedDataY.Count > 99)
                    _dlInfoSpeedDataY.RemoveAt(0);

                //Debug.WriteLine(string.Join(",", DlInfoSpeedDataY));
                DlInfoSpeedDataY.Add(value is long actuallyLong ? actuallyLong : 0);

                if (value != _serverState.DownloadSpeed)
                {
                    _serverState.DownloadSpeed = value is long lo ? lo : 0;
                    OnPropertyChanged(nameof(DlInfoSpeed));
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
                    OnPropertyChanged(nameof(DlRateLimit));
                    OnPropertyChanged(nameof(DisplayDlRateLimit));
                    OnPropertyChanged(nameof(HighestRateLimit));
                }
            }
        }

        public double DisplayDlRateLimit
        {
            get => Convert.ToDouble(
                DlRateLimit / DataConverter.GetMultiplierForUnit(ShowDlSizeAs)
            );

            set => DlRateLimit = (long)value * DataConverter.GetMultiplierForUnit(ShowDlSizeAs);
        }

        private string _showDlSizeAs = Design.IsDesignMode ? SizeOptions[1] : ConfigService.ShowDlSizeAs;
        public string ShowDlSizeAs
        {
            get => _showDlSizeAs;
            set
            {
                if (value != _showDlSizeAs && SizeOptions.Contains(value))
                {
                    _showDlSizeAs = value;
                    OnPropertyChanged(nameof(ShowDlSizeAs));
                    OnPropertyChanged(nameof(DisplayDlRateLimit));
                    if (!Design.IsDesignMode)
                        ConfigService.ShowDlSizeAs = value;
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
                    OnPropertyChanged(nameof(FreeSpaceOnDisk));
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
                    OnPropertyChanged(nameof(RefreshInterval));
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
                    OnPropertyChanged(nameof(TotalBuffersSize));
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
                    OnPropertyChanged(nameof(TotalPeerConnections));
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
                    OnPropertyChanged(nameof(UpInfoData));
                }
            }
        }

        private ObservableCollection<long> _upInfoSpeedDataY = new ObservableCollection<long>();
        public ObservableCollection<long> UpInfoSpeedDataY
        {
            get => _upInfoSpeedDataY;
            set
            {
                if (value != _upInfoSpeedDataY)
                {
                    _upInfoSpeedDataY = value;
                    OnPropertyChanged(nameof(UpInfoSpeedDataY));
                }
            }
        }

        public long? UpInfoSpeed
        {
            get { return _serverState.UploadSpeed; }
            set
            {
                // Don't need more than 100 entries
                if (_upInfoSpeedDataY.Count > 99)
                    _upInfoSpeedDataY.RemoveAt(0);

                //Debug.WriteLine(string.Join(",", UpInfoSpeedDataY));

                UpInfoSpeedDataY.Add(value is long actuallyLong ? actuallyLong : 0);

                if (value != _serverState.UploadSpeed)
                {
                    _serverState.UploadSpeed = value is long lo ? lo : 0;
                    OnPropertyChanged(nameof(UpInfoSpeed));
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
                    OnPropertyChanged(nameof(UpRateLimit));
                    OnPropertyChanged(nameof(DisplayUpRateLimit));
                    OnPropertyChanged(nameof(HighestRateLimit));
                }
            }
        }

        public long? HighestRateLimit
        {
            get
            {
                if (DlRateLimit.HasValue && UpRateLimit.HasValue)
                    return Math.Max(DlRateLimit.Value, UpRateLimit.Value);
                else if (DlRateLimit.HasValue)
                    return DlRateLimit.Value;
                else if (UpRateLimit.HasValue)
                    return UpRateLimit.Value;

                return null;
            }
        }


        public double DisplayUpRateLimit
        {
            get => Convert.ToDouble(
                UpRateLimit / DataConverter.GetMultiplierForUnit(ShowUpSizeAs)
            );

            set => UpRateLimit = (long)value * DataConverter.GetMultiplierForUnit(ShowUpSizeAs);
        }

        private string _showUpSizeAs = Design.IsDesignMode ? SizeOptions[1] : ConfigService.ShowUpSizeAs;
        public string ShowUpSizeAs
        {
            get => _showUpSizeAs;
            set
            {
                if (value != _showUpSizeAs && SizeOptions.Contains(value))
                {
                    _showUpSizeAs = value;
                    OnPropertyChanged(nameof(ShowUpSizeAs));
                    if (!Design.IsDesignMode)
                        ConfigService.ShowUpSizeAs = value;
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
                    OnPropertyChanged(nameof(UseAltSpeedLimits));
                }
            }
        }

        public FluentIcons.Common.Symbol ConnectionStatusIcon
        {
            get
            {
                return ConnectionStatus switch
                {
                    QBittorrent.Client.ConnectionStatus.Connected => FluentIcons.Common.Symbol.Globe,
                    QBittorrent.Client.ConnectionStatus.Disconnected => FluentIcons.Common.Symbol.GlobeError,
                    QBittorrent.Client.ConnectionStatus.Firewalled => FluentIcons.Common.Symbol.Fireplace,
                    _ => FluentIcons.Common.Symbol.QuestionCircle,
                };
            }
        }

        private string _showLineGraphSizeAs = Design.IsDesignMode ? SizeOptions[1] : ConfigService.ShowLineGraphSizeAs;
        public string ShowLineGraphSizeAs
        {
            get => _showLineGraphSizeAs;
            set
            {
                if (value != _showLineGraphSizeAs && SizeOptions.Contains(value))
                {
                    _showLineGraphSizeAs = value;
                    OnPropertyChanged(nameof(ShowLineGraphSizeAs));
                    if (!Design.IsDesignMode)
                        ConfigService.ShowLineGraphSizeAs = value;
                }
            }
        }
    }
}