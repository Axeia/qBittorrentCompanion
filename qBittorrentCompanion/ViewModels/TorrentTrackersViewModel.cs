using QBittorrent.Client;
using qBittorrentCompanion.Services;
using RaiseChangeGenerator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels
{
    public partial class TorrentTrackersViewModel : AutoUpdateViewModelBase
    {
        [RaiseChange]
        private ObservableCollection<TorrentTrackerViewModel> _torrentTrackers = [];

        [RaiseChange]
        private TorrentTrackerViewModel? _selectedTorrentTracker;

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
            await QBittorrentService.DeleteTrackerAsync(_infoHash, SelectedTorrentTracker!.Url);
            TorrentTrackers.Remove(SelectedTorrentTracker);
        }

        protected override async Task FetchDataAsync()
        {
            TorrentTrackers.Clear();
            await UpdateDataAsyncLogic();

            _refreshTimer.Start();
        }
        protected override async Task UpdateDataAsync(object? sender, EventArgs e)
        {
            await UpdateDataAsyncLogic();
        }

        private async Task UpdateDataAsyncLogic()
        {
            var torrentTrackers = await QBittorrentService.GetTorrentTrackersAsync(_infoHash);
            if (torrentTrackers != null)
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
