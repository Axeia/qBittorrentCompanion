using QBittorrent.Client;
using qBittorrentCompanion.Models;
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
    public class TorrentPeersViewModel : AutoUpdateViewModelBase
    {
        private ObservableCollection<TorrentPeerViewModel> _torrentPeers = [];

        private int _rid = -1;
        protected int Rid
        {
            get
            {
                _rid++;
                return _rid;
            }
        }
        public ObservableCollection<TorrentPeerViewModel> TorrentPeers
        {
            get => _torrentPeers;
            set
            {
                if (value != _torrentPeers)
                {
                    _torrentPeers = value;
                    OnPropertyChanged();
                }
            }
        }

        public TorrentPeersViewModel(TorrentInfoViewModel? torrentInfoViewModel, int interval = 1500)
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
            TorrentPeers.Clear();

            var torrentPeers = await QBittorrentService.QBittorrentClient.GetPeerPartialDataAsync(_infoHash, Rid);
            Update(torrentPeers.PeersChanged, torrentPeers.PeersRemoved);

            _refreshTimer.Start();
        }

        protected override async Task UpdateDataAsync(object? sender, ElapsedEventArgs e)
        {
            //Debug.WriteLine($"DateTime.Now.ToString(\"HH:ss\")}} Updating peers for {_infoHash}");
            var torrentPeers = await QBittorrentService.QBittorrentClient.GetPeerPartialDataAsync(_infoHash, Rid);
            Update(torrentPeers.PeersChanged, torrentPeers.PeersRemoved);
        }

        public void Update(IReadOnlyDictionary<string, PeerPartialInfo>? newTorrentPeers, IReadOnlyList<string>? peersRemoved)
        {
            if (newTorrentPeers is not null)
                foreach (KeyValuePair<string, PeerPartialInfo> kvp in newTorrentPeers)
                {
                    var existingPeerVm = TorrentPeers.FirstOrDefault(vm => vm.Id == kvp.Key);

                    if (existingPeerVm != null)
                    {
                        existingPeerVm.Update(kvp.Value);
                    }
                    else
                    {
                        TorrentPeers.Add(new TorrentPeerViewModel(kvp.Value, kvp.Key));
                    }
                }

            if (peersRemoved is not null)
                foreach (var id in peersRemoved)
                {
                    var item = TorrentPeers.FirstOrDefault(p => p.Id == id);
                    if (item is not null)
                        TorrentPeers.Remove(item);
                }
        }

    }
}
