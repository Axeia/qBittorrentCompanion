using AutoPropertyChangedGenerator;
using Avalonia.Controls;
using Newtonsoft.Json.Linq;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using static qBittorrentCompanion.Helpers.DataConverter;

namespace qBittorrentCompanion.ViewModels
{
    public partial class ServerStateViewModel : ViewModelBase
    {
        public static ByteUnit[] SizeOptions => [.. Enum.GetValues<ByteUnit>().Take(3)];

        [AutoProxyPropertyChanged(nameof(GlobalTransferExtendedInfo.AllTimeDownloaded), "AllTimeDl")]
        [AutoProxyPropertyChanged(nameof(GlobalTransferExtendedInfo.AllTimeUploaded), "AllTimeUl")]
        [AutoProxyPropertyChanged(nameof(GlobalTransferExtendedInfo.DhtNodes))]
        [AutoProxyPropertyChanged(nameof(GlobalTransferExtendedInfo.DownloadedData), "DlInfoData")]
        [AutoProxyPropertyChanged(nameof(GlobalTransferExtendedInfo.FreeSpaceOnDisk))]
        [AutoProxyPropertyChanged(nameof(GlobalTransferExtendedInfo.RefreshInterval))]
        [AutoProxyPropertyChanged(nameof(GlobalTransferExtendedInfo.TotalBuffersSize))]
        [AutoProxyPropertyChanged(nameof(GlobalTransferExtendedInfo.TotalPeerConnections))]
        [AutoProxyPropertyChanged(nameof(GlobalTransferExtendedInfo.UploadedData), "UpInfoData")]
        [AutoProxyPropertyChanged(nameof(GlobalTransferExtendedInfo.GlobalAltSpeedLimitsEnabled))]
        private readonly GlobalTransferExtendedInfo _serverState;

        public ReactiveCommand<Unit, Unit> SaveDisplayDlRateLimitCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> SaveDisplayUpRateLimitCommand { get; private set; }

        private readonly long _sessionStartAllTimeDownloaded;
        private readonly long _sessionStartAllTimeUploaded;

        [AutoPropertyChanged]
        private long _sessionDownloaded = 0;
        [AutoPropertyChanged]
        private long _sessionUploaded = 0;

        public ServerStateViewModel(GlobalTransferExtendedInfo serverState)
        {
            _serverState = serverState;
            SaveDisplayDlRateLimitCommand = ReactiveCommand.CreateFromTask(SaveDisplayDlRateLimit);
            SaveDisplayUpRateLimitCommand = ReactiveCommand.CreateFromTask(SaveDisplayUpRateLimit);

            _sessionStartAllTimeDownloaded = serverState.AllTimeDownloaded ?? 0;
            _sessionStartAllTimeUploaded = serverState.AllTimeUploaded ?? 0;
        }

        private async Task SaveDisplayDlRateLimit()
        {
            try
            {
                if (DlRateLimit is long dlrl)
                    await QBittorrentService.SetGlobalDownloadLimitAsync(dlrl);
            }
            catch(Exception e){ Debug.WriteLine(e.Message); }
        }

        private async Task SaveDisplayUpRateLimit()
        {
            try
            {
                if (UpRateLimit is long upL)
                    await QBittorrentService.SetGlobalUploadLimitAsync(upL);
            }
            catch(Exception e){ Debug.WriteLine(e.Message); }
        }

        public void Update(GlobalTransferExtendedInfo serverState)
        {
            if (serverState.AllTimeDownloaded is long allTimeDownloaded)
            {
                SessionDownloaded = allTimeDownloaded - _sessionStartAllTimeDownloaded;
                _serverState.AllTimeDownloaded = allTimeDownloaded;
                this.RaisePropertyChanged(nameof(AllTimeDl));
            }

            if (serverState.AllTimeUploaded is long allTimeUploaded)
            {
                SessionUploaded = allTimeUploaded - _sessionStartAllTimeUploaded;
                _serverState.AllTimeUploaded = allTimeUploaded;
                this.RaisePropertyChanged(nameof(AllTimeUl));
            }

            if (serverState.ConnectionStatus is not null)
                ConnectionStatus = serverState.ConnectionStatus;
            if (serverState.DhtNodes is not null)
                DhtNodes = serverState.DhtNodes;
            if (serverState.DownloadedData is not null)
                DlInfoData = serverState.DownloadedData;
            // Sorts its own value
            DlInfoSpeed = serverState.DownloadSpeed;
            if (serverState.DownloadSpeedLimit is not null)
                DlRateLimit = serverState.DownloadSpeedLimit;
            if (serverState.FreeSpaceOnDisk is not null)
                FreeSpaceOnDisk = serverState.FreeSpaceOnDisk;
            if (serverState.RefreshInterval is not null)
                RefreshInterval = serverState.RefreshInterval;
            if (serverState.TotalBuffersSize is not null)
                TotalBuffersSize = serverState.TotalBuffersSize;
            if (serverState.TotalPeerConnections is not null)
                TotalPeerConnections = serverState.TotalPeerConnections;
            if (serverState.UploadedData is not null)
                UpInfoData = serverState.UploadedData;
            // Sorts its own value
            UpInfoSpeed = serverState.UploadSpeed;
            if (serverState.UploadSpeedLimit is not null)
                UpRateLimit = serverState.UploadSpeedLimit;

            // This does not work as intended because
            // `GlobalTransferExtendedInfo.GlobalAltSpeedLimitsEnabled` is not nullable
            // (Pull request resolving it is pending https://github.com/fedarovich/qbittorrent-net-client/pull/36 )
            //if (serverState.GlobalAltSpeedLimitsEnabled is bool gasle)
            //    GlobalAltSpeedLimitsEnabled = gasle;
        }

        public GlobalTransferExtendedInfo ServerState { get => _serverState; }

        public ConnectionStatus? ConnectionStatus
        {
            get { return _serverState.ConnectionStatus; }
            set
            {
                if (value != _serverState.ConnectionStatus)
                {
                    _serverState.ConnectionStatus = value;
                    this.RaisePropertyChanged(nameof(ConnectionStatus));
                    this.RaisePropertyChanged(nameof(ConnectionStatusIcon));
                }
            }
        }


        [AutoPropertyChanged]
        private ObservableCollection<long> _dlInfoSpeedDataY = [];

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
                    this.RaisePropertyChanged(nameof(DlInfoSpeed));
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
                    this.RaisePropertyChanged(nameof(DlRateLimit));
                    this.RaisePropertyChanged(nameof(DisplayDlRateLimit));
                    this.RaisePropertyChanged(nameof(HighestRateLimit));
                }
            }
        }

        public double DisplayDlRateLimit
        {
            get => Convert.ToDouble(
                DlRateLimit / DataConverter.Multipliers[ShowDlSizeAs]
            );

            set => DlRateLimit = (long)value * DataConverter.Multipliers[ShowDlSizeAs];
        }

        private ByteUnit _showDlSizeAs = Design.IsDesignMode ? SizeOptions[1] : ConfigService.ShowDlSizeAs;
        public ByteUnit ShowDlSizeAs
        {
            get => _showDlSizeAs;
            set
            {
                if (value != _showDlSizeAs && SizeOptions.Contains(value))
                {
                    _showDlSizeAs = value;
                    this.RaisePropertyChanged(nameof(ShowDlSizeAs));
                    this.RaisePropertyChanged(nameof(DisplayDlRateLimit));
                    if (!Design.IsDesignMode)
                        ConfigService.ShowDlSizeAs = value;
                }
            }
        }

        public double? GlobalRatio => _serverState.GlobalRatio;

        [AutoPropertyChanged]
        private ObservableCollection<long> _upInfoSpeedDataY = [];

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
                    this.RaisePropertyChanged(nameof(UpInfoSpeed));
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
                    this.RaisePropertyChanged(nameof(UpRateLimit));
                    this.RaisePropertyChanged(nameof(DisplayUpRateLimit));
                    this.RaisePropertyChanged(nameof(HighestRateLimit));
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
                UpRateLimit / DataConverter.Multipliers[ShowUpSizeAs]
            );

            set => UpRateLimit = (long)value * DataConverter.Multipliers[ShowUpSizeAs];
        }

        private ByteUnit _showUpSizeAs = Design.IsDesignMode ? SizeOptions[1] : ConfigService.ShowUpSizeAs;
        public ByteUnit ShowUpSizeAs
        {
            get => _showUpSizeAs;
            set
            {
                if (value != _showUpSizeAs && SizeOptions.Contains(value))
                {
                    _showUpSizeAs = value;
                    this.RaisePropertyChanged(nameof(ShowUpSizeAs));
                    if (!Design.IsDesignMode)
                        ConfigService.ShowUpSizeAs = value;
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

        private ByteUnit _showLineGraphSizeAs = Design.IsDesignMode ? DataConverter.ByteUnit.KiB : ConfigService.ShowLineGraphSizeAs;
        public ByteUnit ShowLineGraphSizeAs
        {
            get => _showLineGraphSizeAs;
            set
            {
                if (value != _showLineGraphSizeAs && SizeOptions.Contains(value))
                {
                    _showLineGraphSizeAs = value;
                    this.RaisePropertyChanged(nameof(ShowLineGraphSizeAs));
                    if (!Design.IsDesignMode)
                        ConfigService.ShowLineGraphSizeAs = value;
                }
            }
        }
    }
}