﻿using Avalonia.Controls;
using Avalonia.Threading;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
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

        public async Task<bool> LogIn()
        {
            SecureStorage ss = new();
            if (!ss.HasSavedData())
                return false;


            IsLoggedIn = await QBittorrentService.AutoAthenticate();
            if (IsLoggedIn)
            {
                (string username, string password, string url, string port) = ss.LoadData();
                Username = username;
            }

            return IsLoggedIn;
        }


        private ServerStateViewModel? _serverStateViewModel;
        public ServerStateViewModel? ServerStateViewModel
        {
            get => _serverStateViewModel;
            set => this.RaiseAndSetIfChanged(ref _serverStateViewModel, value);
        }

        private DispatcherTimer _refreshTimer = new();
        public TorrentsViewModel TorrentsViewModel { get; set; } = new();

        public MainWindowViewModel()
        {
            long refreshInterval = 1500;
            _refreshTimer.Interval = TimeSpan.FromMilliseconds(refreshInterval);
            _refreshTimer.Tick += RefreshTimer_Elapsed;
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
            try
            {
                PartialData mainData = await QBittorrentService.QBittorrentClient.GetPartialDataAsync(RidIncrement);

                //Use TagsChanged not CategoriesAdded, the latter is for older versions of the API
                TagService.Instance.AddTags(mainData.TagsAdded);
                //Use CategoriesChanged not CategoriesAdded, the latter is for older versions of the API
                CategoryService.Instance.AddCategories(mainData.CategoriesChanged.Values);

                // The order of operations matters, Torrents have to be added AFTER Tags/Categories are added
                // or they won't be counted.
                if (mainData.TorrentsChanged != null)
                    foreach (var kvp in mainData.TorrentsChanged)
                        torrentsViewModel.AddTorrent(kvp.Value, kvp.Key);
                //TODO? Delete torrents

                ServerStateViewModel = new ServerStateViewModel(mainData.ServerState);

                //Trackers are part of additionaldata rather than getting their own property.
                if (mainData.AdditionalData is not null)
                    if (mainData.AdditionalData.ContainsKey("trackers"))
                        torrentsViewModel.UpdateTrackers(mainData.AdditionalData["trackers"]);

                //Keep everything up to date with this timer
                _refreshTimer.Start();
            }
            catch (QBittorrentClientRequestException e)
            {
                if(e.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    IsLoggedIn = false;
                }
            }
        }

        private async void RefreshTimer_Elapsed(object? sender, EventArgs e)
        {
            PopulateOrUpdateTorrents(await QBittorrentService.QBittorrentClient.GetPartialDataAsync(RidIncrement));
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
