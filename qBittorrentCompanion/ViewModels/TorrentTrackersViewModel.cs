using Avalonia;
using Avalonia.Media;
using DynamicData;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Models;
using qBittorrentCompanion.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;

namespace qBittorrentCompanion.ViewModels
{
    public class TorrentTrackersViewModel : AutoUpdateViewModelBase
    {
        private ObservableCollection<TorrentTrackerViewModel> _torrentTrackers = [];
        public ObservableCollection<TorrentTrackerViewModel> TorrentTrackers
        {
            get => _torrentTrackers;
            set
            {
                if(value != _torrentTrackers)
                {
                    _torrentTrackers = value;
                    OnPropertyChanged();
                }
            }
        }

        public TorrentTrackersViewModel(TorrentInfoViewModel? torrentInfoViewModel, int interval = 1500*7)
        {
            if (torrentInfoViewModel is not null && torrentInfoViewModel.Hash is not null)
            {
                _infoHash = torrentInfoViewModel.Hash;
                _ = FetchDataAsync();
                _refreshTimer.Interval = TimeSpan.FromMilliseconds(interval);
            }
        }

        protected override async Task FetchDataAsync()
        {
            TorrentTrackers.Clear();
            var torrentTrackers = await QBittorrentService.QBittorrentClient.GetTorrentTrackersAsync(_infoHash);
            Update(torrentTrackers);

            _refreshTimer.Start();
        }
        protected override async Task UpdateDataAsync(object? sender, ElapsedEventArgs e)
        {
            var torrentTrackers = await QBittorrentService.QBittorrentClient.GetTorrentTrackersAsync(_infoHash);
            Update(torrentTrackers);
        }

        public void Update(IReadOnlyList<TorrentTracker> newTorrentTrackers)
        {
            foreach (TorrentTracker torrentTracker in newTorrentTrackers)
            {
                var existingTorrentTrackerVm = TorrentTrackers.FirstOrDefault(vm => vm.Url == torrentTracker.Url);

                if (existingTorrentTrackerVm != null)
                    existingTorrentTrackerVm.Update(torrentTracker);
                else
                    TorrentTrackers.Add(new TorrentTrackerViewModel(torrentTracker));
            }
        }
    }
}
