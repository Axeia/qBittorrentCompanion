using Avalonia.Controls;
using Avalonia.Threading;
using DynamicData;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using static qBittorrentCompanion.ViewModels.MainWindowViewModel;

namespace qBittorrentCompanion.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ObservableCollection<QBittorrentService.HttpData> _httpData = [];
        public ObservableCollection<QBittorrentService.HttpData> HttpData => _httpData;

        private QBittorrentService.HttpData? _selectedHttpData = null;
        public QBittorrentService.HttpData? SelectedHttpData
        {
            get => _selectedHttpData;
            set => this.RaiseAndSetIfChanged(ref _selectedHttpData, value);
        }

        private readonly ObservableCollection<HttpDataUrl> _httpDataUrls = [];
        public ObservableCollection<HttpDataUrl> HttpDataUrls
        {
            get => _httpDataUrls;
        }

        public class HttpDataUrl : ReactiveObject
        {
            private bool _isChecked = true;
            public bool IsChecked
            {
                get => _isChecked;
                set => this.RaiseAndSetIfChanged(ref this._isChecked, value);
            }

            private readonly string _url = string.Empty;
            public string Url => _url;

            private readonly string? _shortDescription = null;
            public string? ShortDescription => _shortDescription;

            public readonly Uri? _documentationUrl = null;
            public Uri? DocumentationUrl => _documentationUrl;

            public HttpDataUrl(string url)
            {
                _url = url;

                switch (url)
                {
                    case "/api/v2/auth/login":
                        {
                            _shortDescription = "Authentication";
                            _documentationUrl = new Uri("https://github.com/qbittorrent/qBittorrent/wiki/WebUI-API-(qBittorrent-5.0)#login");
                            break;
                        }
                    default:
                        {
                            _shortDescription = "";
                            break;
                        }
                }
            }

            private int _count = 1;
            public int Count => _count;
            public void IncreaseCount()
            {
                _count++;
                this.RaisePropertyChanged(nameof(Count));
            }
        }

        public ReactiveCommand<Unit, Unit> ToggleLogNetworkRequestsCommand { get; }

        private bool _checkAllHttpDataUrls = true;

        /// <summary>
        /// Represents state for CheckBox - Setting it to true with Check all HttpDataUrls
        /// If everything is checked it should auto default to true (and false if not)
        /// </summary>
        public bool CheckAllHttpDataUrls 
        {
            get => _checkAllHttpDataUrls;
            set
            {
                if (_checkAllHttpDataUrls != value)
                {
                    HttpDataUrls.ToList().ForEach(t => t.IsChecked = value); 
                    _checkAllHttpDataUrls = value;
                    this.RaisePropertyChanged(nameof(CheckAllHttpDataUrls));
                }
            }
        }

        public ReactiveCommand<Unit, Unit> UncheckAllHttpDataUrlsCommand { get; }

        private bool _canUncheckHttpDataUrl = true;
        public bool CanUncheckHttpDataUrl
        { 
            get => _canUncheckHttpDataUrl;
            set => this.RaiseAndSetIfChanged(ref _canUncheckHttpDataUrl, value);
        }

        private bool _isLoggedIn = false;
        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => this.RaiseAndSetIfChanged(ref _isLoggedIn, value);
        }

        private bool _bypasssDownloadWindow = Design.IsDesignMode || ConfigService.ShowSideBarStatusIcons;
        public bool BypassDownloadWindow
        {
            get => _bypasssDownloadWindow;
            set
            {
                if (value != _bypasssDownloadWindow)
                {
                    ConfigService.BypassDownloadWindow = value;
                    this.RaiseAndSetIfChanged(ref _bypasssDownloadWindow, value);
                }
            }
        }

        private bool _showRssRuleSmartFilter = Design.IsDesignMode || ConfigService.ShowRssRuleSmartFilter;
        public bool ShowRssRuleSmartFilter
        {
            get => _showRssRuleSmartFilter;
            set
            {
                if (value != _showRssRuleSmartFilter)
                {
                    ConfigService.ShowRssRuleSmartFilter = value;
                    this.RaiseAndSetIfChanged(ref _showRssRuleSmartFilter, value);
                }
            }
        }

        private bool _showLogging = Design.IsDesignMode || ConfigService.ShowLogging;
        public bool ShowLogging
        {
            get => _showLogging;
            set
            {
                if (value != _showLogging)
                {
                    ConfigService.ShowLogging = value;
                    this.RaiseAndSetIfChanged(ref _showLogging, value);
                }
            }
        }

        public async Task<bool> LogIn()
        {
            SecureStorage ss = new();
            if (!ss.HasSavedData())
                return false;

            IsLoggedIn = await QBittorrentService.AutoAthenticate();
            if (IsLoggedIn)
                Username = ss.LoadData().username;

            return IsLoggedIn;
        }

        private ServerStateViewModel? _serverStateViewModel;
        public ServerStateViewModel? ServerStateViewModel
        {
            get => _serverStateViewModel;
            set => this.RaiseAndSetIfChanged(ref _serverStateViewModel, value);
        }

        private readonly DispatcherTimer _refreshTimer = new();
        public TorrentsViewModel TorrentsViewModel { get; set; } = new();

        public MainWindowViewModel()
        {
            long refreshInterval = 1500;
            _refreshTimer.Interval = TimeSpan.FromMilliseconds(refreshInterval);
            _refreshTimer.Tick += RefreshTimer_Elapsed;

            ToggleLogNetworkRequestsCommand = ReactiveCommand.Create(ToggleLogNetworkRequests);
            UncheckAllHttpDataUrlsCommand = ReactiveCommand.Create(() => HttpDataUrls.ToList().ForEach(h=>h.IsChecked=false));

            QBittorrentService.NetworkRequestSent += QBittorrentService_NetworkRequestSent;
        }

        private void ShowHideHttpData()
        {
            // Get paths as a HashSet (for fast lookups)
            var enabledPaths = HttpDataUrls
                .Where(x => x.IsChecked)
                .Select(x => x.Url)
                .ToHashSet();

            foreach (var httpData in HttpData)
            {
                httpData.IsVisible = enabledPaths.Contains(httpData.Url.AbsolutePath);
            }
        }

        private void ToggleLogNetworkRequests()
        {
            LogNetworkRequests = !LogNetworkRequests;
        }

        private bool _logNetworkRequests = true;
        public bool LogNetworkRequests
        {
            get => _logNetworkRequests;
            set => this.RaiseAndSetIfChanged(ref _logNetworkRequests, value);
        }

        private void QBittorrentService_NetworkRequestSent(QBittorrentService.HttpData obj)
        {
            if (LogNetworkRequests)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    if (HttpData.Count == 100)
                        HttpData.RemoveAt(0);

                    var path = obj.Url.AbsolutePath;
                    var urlEntry = HttpDataUrls.FirstOrDefault(h => h.Url == path);
                    if (urlEntry == null)
                    {
                        urlEntry = new HttpDataUrl(path);
                        urlEntry.WhenAnyValue(x => x.IsChecked)
                            .Subscribe(_ =>
                                {
                                    ShowHideHttpData();
                                    DetermineHttpDataUrlsCheckedAndUnchecked();
                                }
                            );
                        HttpDataUrls.Add(urlEntry);
                    }
                    else
                        urlEntry.IncreaseCount();

                    obj.IsVisible = urlEntry.IsChecked;
                    HttpData.Add(obj);
                });
            }
        }

        private void DetermineHttpDataUrlsCheckedAndUnchecked()
        {
            CheckAllHttpDataUrls = HttpDataUrls.All(h=>h.IsChecked);
            CanUncheckHttpDataUrl = HttpDataUrls.Any(h => h.IsChecked);
        }

        private int _rid = -1;
        protected int RidIncrement
        {
            get
            {
                _rid++;
                return _rid;
            }
        }

        private string _username = string.Empty;
        public string Username 
        {
            get => _username;
            set => this.RaiseAndSetIfChanged(ref _username, value);
        }


        /// <summary>
        /// Uses <c>QBittorrentClient.GetPartialDataAsync</c> rather than <c>QBittorrentClient.GetTorrentsList</c>
        /// so it pulls in -all- information, not just torrents.
        /// </summary>
        /// <param name="torrentsViewModel"></param>
        public async void PopulateAndUpdate(TorrentsViewModel torrentsViewModel)
        {
            PartialData? mainData = await QBittorrentService.GetPartialDataAsync(RidIncrement);
            if (mainData != null)
            {
                //Use TagsChanged not CategoriesAdded, the latter is for older versions of the API
                TagService.Instance.AddTags(mainData.TagsAdded);
                //Use CategoriesChanged not CategoriesAdded, the latter is for older versions of the API
                CategoryService.Instance.AddCategories(mainData.CategoriesChanged.Values);

                // The order of operations matters, Torrents have to be added AFTER Tags/Categories are added
                // or they won't be counted.
                if (mainData.TorrentsChanged != null)
                    foreach (var kvp in mainData.TorrentsChanged)
                        torrentsViewModel.AddTorrent(kvp.Value, kvp.Key);

                ServerStateViewModel = new ServerStateViewModel(mainData.ServerState);

                //Trackers are part of additionaldata rather than getting their own property.
                if (mainData.AdditionalData is not null)
                    if (mainData.AdditionalData.TryGetValue("trackers", out Newtonsoft.Json.Linq.JToken? value))
                        torrentsViewModel.UpdateTrackers(value);

                //Keep everything up to date with this timer
                _refreshTimer.Start();
            }
            else
            {
                IsLoggedIn = false;
            }
        }

        private async void RefreshTimer_Elapsed(object? sender, EventArgs e)
        {
            var partialData = await QBittorrentService.GetPartialDataAsync(RidIncrement);
            if(partialData != null)
                PopulateOrUpdateTorrents(partialData);
        }

        public void PopulateOrUpdateTorrents(PartialData partialData)
        {
            if (TorrentsViewModel is null)
                return;

            if (partialData.TorrentsChanged is not null)
            {
                foreach (var kvp in partialData.TorrentsChanged)
                {
                    //Debug.WriteLine(item.Key);
                    TorrentInfoViewModel? oldEntry = TorrentsViewModel.Torrents.FirstOrDefault(t => t.Hash == kvp.Key);
                    if (oldEntry is not null)
                        oldEntry.Update(kvp.Value);
                    else // New but pulled in from an update rather than the initial response
                        TorrentsViewModel.AddTorrent(kvp.Value, kvp.Key);
                }
            }
            //If any torrents were removed, remove them from the ViewModel
            TorrentsViewModel.RemoveTorrents(partialData.TorrentsRemoved);
            //Note: .CategoriesChanged may contain new categories, CategoriesAdded is deprecated. 
            TorrentsViewModel.UpdateCategories(partialData.CategoriesChanged);
            TorrentsViewModel.RemoveCategories(partialData.CategoriesRemoved);

            TorrentsViewModel.UpdateTags(partialData.TagsAdded);
            TorrentsViewModel.RemoveTags(partialData.TagsRemoved);

            //Updates all the bottom status bar data (diskspace, dht nodes etc)
            UpdateServerState(partialData.ServerState);
        }

        public void UpdateServerState(GlobalTransferExtendedInfo serverState)
        {
            if (ServerStateViewModel is not null && serverState is not null)
            {
                if (serverState.AllTimeDownloaded is not null)
                    ServerStateViewModel.AllTimeDl = serverState.AllTimeDownloaded;
                if (serverState.AllTimeUploaded is not null)
                    ServerStateViewModel.AllTimeUl = serverState.AllTimeUploaded;
                if (serverState.ConnectionStatus is not null)
                    ServerStateViewModel.ConnectionStatus = serverState.ConnectionStatus;
                if (serverState.DhtNodes is not null)
                    ServerStateViewModel.DhtNodes = serverState.DhtNodes;
                if (serverState.DownloadedData is not null)
                    ServerStateViewModel.DlInfoData = serverState.DownloadedData;
                // Sorts its own value
                ServerStateViewModel.DlInfoSpeed = serverState.DownloadSpeed;
                if (serverState.DownloadSpeedLimit is not null)
                    ServerStateViewModel.DlRateLimit = serverState.DownloadSpeedLimit;
                if (serverState.FreeSpaceOnDisk is not null)
                    ServerStateViewModel.FreeSpaceOnDisk = serverState.FreeSpaceOnDisk;
                if (serverState.RefreshInterval is not null)
                    ServerStateViewModel.RefreshInterval = serverState.RefreshInterval;
                if (serverState.TotalBuffersSize is not null)
                    ServerStateViewModel.TotalBuffersSize = serverState.TotalBuffersSize;
                if (serverState.TotalPeerConnections is not null)
                    ServerStateViewModel.TotalPeerConnections = serverState.TotalPeerConnections;
                if (serverState.UploadedData is not null)
                    ServerStateViewModel.UpInfoData = serverState.UploadedData;
                // Sorts its own value
                ServerStateViewModel.UpInfoSpeed = serverState.UploadSpeed;
                if (serverState.UploadSpeedLimit is not null)
                    ServerStateViewModel.UpRateLimit = serverState.UploadSpeedLimit;
                
                ServerStateViewModel.UseAltSpeedLimits = serverState.GlobalAltSpeedLimitsEnabled;
            }
        }

        public void Pause()
        {
            _refreshTimer.Stop();
        }
    }
}