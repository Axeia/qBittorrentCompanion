using QBittorrent.Client;
using qBittorrentCompanion.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

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
                    OnPropertyChanged(nameof(TorrentTrackers));
                }
            }
        }

        private TorrentTrackerViewModel? _selectedTorrentTracker;
        public TorrentTrackerViewModel? SelectedTorrentTracker
        {
            get => _selectedTorrentTracker;
            set
            {
                if (value != _selectedTorrentTracker)
                {
                    _selectedTorrentTracker = value;
                    OnPropertyChanged(nameof(SelectedTorrentTracker));
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

        public async Task DeleteTrackerAsync()
        {
            try
            {
                await QBittorrentService.QBittorrentClient.DeleteTrackerAsync(_infoHash, SelectedTorrentTracker!.Url);
                TorrentTrackers.Remove(SelectedTorrentTracker);
            }
            catch (Exception e) { Debug.WriteLine(e.Message); }
        }

        protected override async Task FetchDataAsync()
        {
            TorrentTrackers.Clear();
            var torrentTrackers = await QBittorrentService.QBittorrentClient.GetTorrentTrackersAsync(_infoHash);
            Update(torrentTrackers);

            _refreshTimer.Start();
        }
        protected override async Task UpdateDataAsync(object? sender, EventArgs e)
        {
            await UpdateDataAsyncLogic();
        }

        private async Task UpdateDataAsyncLogic()
        {
            var torrentTrackers = await QBittorrentService.QBittorrentClient.GetTorrentTrackersAsync(_infoHash);
            Update(torrentTrackers);
        }

        public async Task ForceUpdateDataAsync()
        {
            TorrentTrackers.Clear();
            _refreshTimer.Stop();
            await UpdateDataAsyncLogic();
            _refreshTimer.Start();
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
