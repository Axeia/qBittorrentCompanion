using Avalonia.Controls;
using Avalonia.Threading;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using qBittorrentCompanion.Logging;
using Splat;
using RaiseChangeGenerator;

namespace qBittorrentCompanion.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly ObservableCollection<HttpData> _httpData = [];
        public ObservableCollection<HttpData> HttpData => _httpData;

        private HttpData? _selectedHttpData = null;
        public HttpData? SelectedHttpData
        {
            get => _selectedHttpData;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedHttpData, value);
                this.RaisePropertyChanged(nameof(SelectedHttpDataHasLinkDocInfo));
            }
        }

        public bool SelectedHttpDataHasLinkDocInfo =>
            SelectedHttpData?.HasLinkDocInfo ?? false;

        private readonly ObservableCollection<HttpDataUrl> _httpDataUrls = [];
        public ObservableCollection<HttpDataUrl> HttpDataUrls => _httpDataUrls;

        private readonly ObservableCollection<LogMessage> _logData = [];
        public ObservableCollection<LogMessage> LogData => _logData;

        [RaiseChange]
        private LogMessage? _selectedLogMessage = null;

        private readonly ObservableCollection<LogMessage> _logMessages = [];
        public ObservableCollection<LogMessage> LogMessages => _logMessages;
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

        [RaiseChange]
        private bool _canUncheckHttpDataUrl = true;

        [RaiseChange]
        private bool _isLoggedIn = false;

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

        private int _logViewSelectedTabIndex = Design.IsDesignMode ? 0 : ConfigService.LogViewSelectedTabIndex;
        public int LogViewSelectedTabIndex
        {
            get => _logViewSelectedTabIndex;
            set
            {
                if (value != _logViewSelectedTabIndex)
                {
                    ConfigService.LogViewSelectedTabIndex = value;
                    this.RaiseAndSetIfChanged(ref _logViewSelectedTabIndex, value);
                }
            }
        }

        private bool _useRemoteSearch = Design.IsDesignMode || ConfigService.UseRemoteSearch;
        public bool UseRemoteSearch
        {
            get => _useRemoteSearch;
            set
            {
                if (value != _useRemoteSearch)
                {
                    ConfigService.UseRemoteSearch = value;
                    _useRemoteSearch = value;
                    this.RaisePropertyChanged(nameof(UseRemoteSearch));
                }
            }
        }

        public async Task<bool> LogIn()
        {
            _ = new SecureStorage();
            if (!SecureStorage.HasSavedData())
                return false;

            IsLoggedIn = await QBittorrentService.AutoAthenticate();
            if (IsLoggedIn)
                Username = SecureStorage.LoadData().username;

            return IsLoggedIn;
        }

        [RaiseChange]
        private ServerStateViewModel? _serverStateVm;

        private readonly DispatcherTimer _refreshTimer = new();
        public TorrentsViewModel TorrentsViewModel { get; } = new();

        [RaiseChange]
        private int _torrentsCount = 0;
        [RaiseChange]
        private int _filteredTorrentsCount = 0;

        public MainWindowViewModel()
        {
            long refreshInterval = 1500;
            _refreshTimer.Interval = TimeSpan.FromMilliseconds(refreshInterval);
            _refreshTimer.Tick += RefreshTimer_Elapsed;

            ToggleLogNetworkRequestsCommand = ReactiveCommand.Create(ToggleLogNetworkRequests);
            UncheckAllHttpDataUrlsCommand = ReactiveCommand.Create(() => HttpDataUrls.ToList().ForEach(h=>h.IsChecked=false));

            QBittorrentService.NetworkRequestSent += QBittorrentService_NetworkRequestSent;
            AppLoggerService.LogMessageAdded += LogMessageService_LogMessageAdded;

            this.RaisePropertyChanged(nameof(SelectedHttpData));

            TorrentsViewModel
                .WhenAnyValue(t=>t.TorrentsCount)
                .Subscribe(t => TorrentsCount = t);
            TorrentsViewModel
                .WhenAnyValue(t => t.FilteredTorrentsCount)
                .Subscribe(t => FilteredTorrentsCount = t);

            QBittorrentService.MaxRetryAttemptsReached += QBittorrentService_MaxRetryAttemptsReached;
        }

        private void QBittorrentService_MaxRetryAttemptsReached()
        {
            IsLoggedIn = false;
            _refreshTimer.Stop();

            NotificationService.Instance.NotifyDisconnected();
        }

        private void LogMessageService_LogMessageAdded(LogMessage message)
        {
            LogMessages.Add(message);
        }

        private void ShowHideHttpData()
        {
            // Get paths as a HashSet (for fast lookups)
            var enabledPaths = HttpDataUrls
                .Where(x => x.IsChecked)
                .Select(x => x.Url)
                .ToHashSet();

            foreach (var httpData in HttpData)
                httpData.IsVisible = enabledPaths.Contains(httpData.Url.AbsolutePath);
        }

        private void ToggleLogNetworkRequests()
        {
            LogNetworkRequests = !LogNetworkRequests;
        }

        [RaiseChange]
        private bool _logNetworkRequests = true;

        private void QBittorrentService_NetworkRequestSent(HttpData obj)
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
                        urlEntry = new HttpDataUrl(path, obj.LinkDocInfo);
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
            bool areAllUrlsChecked = HttpDataUrls.All(h => h.IsChecked);
            if (_checkAllHttpDataUrls  != areAllUrlsChecked)
            {
                _checkAllHttpDataUrls = areAllUrlsChecked;
                this.RaisePropertyChanged(nameof(CheckAllHttpDataUrls));
            }
            CanUncheckHttpDataUrl = HttpDataUrls.Any(h => h.IsChecked);
        }

        private int _rid = 0;

        [RaiseChange]
        private string _username = string.Empty;

        /// <summary>
        /// Uses <c>QBittorrentClient.GetPartialDataAsync</c> rather than <c>QBittorrentClient.GetTorrentsList</c>
        /// so it pulls in -all- information, not just torrents.
        /// </summary>
        /// <param name="torrentsViewModel"></param>
        public async void PopulateAndUpdate(TorrentsViewModel torrentsViewModel)
        {
            PartialData? mainData = await QBittorrentService.GetPartialDataAsync(_rid);
            if (mainData != null)
            {
                AppLoggerService.AddLogMessage(
                    LogLevel.Info, 
                    GetFullTypeName<MainWindowViewModel>(), 
                    $"Populating generic torrent info (rid {_rid})", 
                    mainData, 
                    GetFullTypeName<PartialData>()
                );
                //Use TagsChanged not CategoriesAdded, the latter is for older versions of the API
                if (mainData.TagsAdded != null)
                    TagService.Instance.AddTags(mainData.TagsAdded);
                //Use CategoriesChanged not CategoriesAdded, the latter is for older versions of the API
                if (mainData.CategoriesChanged != null)
                    CategoryService.Instance.AddCategories(mainData.CategoriesChanged.Values);

                // The order of operations matters, Torrents have to be added AFTER Tags/Categories are added
                // or they won't be counted.
                if (mainData.TorrentsChanged != null)
                    foreach (var kvp in mainData.TorrentsChanged)
                        torrentsViewModel.AddTorrent(kvp.Value, kvp.Key);

                //Trackers are part of additionaldata rather than getting their own property.
                if (mainData.AdditionalData is not null)
                    if (mainData.AdditionalData.TryGetValue("trackers", out Newtonsoft.Json.Linq.JToken? value))
                        torrentsViewModel.UpdateTrackers(value);

                ServerStateVm = new ServerStateViewModel(mainData.ServerState);
                this.RaisePropertyChanged(nameof(ServerStateVm));
                // Once serverstate is set it should be safe to enable the menu item
                if (App.Current?.GetAltSpeedNativeMenuItem() is NativeMenuItem nvi)
                    nvi.IsEnabled = true;

                _rid = mainData.ResponseId;

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
            var partialData = await QBittorrentService.GetPartialDataAsync(_rid);
            if(partialData != null)
                PopulateOrUpdateTorrents(partialData);
        }

        public void PopulateOrUpdateTorrents(PartialData partialData)
        {
            if (TorrentsViewModel is null)
                return;

            //AppLoggerService.AddLogMessage(
            //    LogLevel.Info, 
            //    GetFullTypeName<MainWindowViewModel>(), 
            //    $"Updating generic torrent info (rid {_rid})", 
            //    partialData, 
            //    GetFullTypeName<PartialData>()
            //);

            if (partialData.TorrentsChanged is not null)
            {
                foreach (var kvp in partialData.TorrentsChanged)
                {
                    //Debug.WriteLine(item.Key);
                    TorrentInfoViewModel? oldEntry = TorrentsViewModel.Torrents.FirstOrDefault(t => t.Hash == kvp.Key);
                    if (oldEntry is not null)
                        oldEntry.Update(kvp.Value);
                    else // New but pulled in from an update rather than the initial response
                    {
                        TorrentsViewModel.AddTorrent(kvp.Value, kvp.Key);
                        NotificationService.Instance.NotifyTorrentAdded(kvp.Value);
                    }
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
            _serverStateVm?.Update(partialData.ServerState);
            this.RaisePropertyChanged(nameof(ServerStateVm));

            _rid = partialData.ResponseId;
        }

        public void Pause() => _refreshTimer.Stop();
    }
}