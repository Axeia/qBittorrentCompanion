using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using QBittorrent.Client;
using qBittorrentCompanion.Services;
using ReactiveUI;
using RaiseChangeGenerator;

namespace qBittorrentCompanion.ViewModels
{
    public partial class TorrentPeersViewModel : AutoUpdateViewModelBase
    {
        [RaiseChange]
        private ObservableCollection<TorrentPeerViewModel> _torrentPeers = [];

        [RaiseChange]
        private TorrentPeerViewModel? _selectedPeer;

        private int _rid = -1;
        protected int Rid
        {
            get
            {
                _rid++;
                return _rid;
            }
        }

        [RaiseChange]
        private bool _isPaused = true;

        public ReactiveCommand<Unit, Unit> ToggleTimerCommand { get; }
        public ReactiveCommand<Unit, Unit> CopyIpAndPortCommand { get; }

        public TorrentPeersViewModel(TorrentInfoViewModel? torrentInfoViewModel, int interval = 1500)
        {
            if (torrentInfoViewModel is not null && torrentInfoViewModel.Hash is not null)
            {
                _infoHash = torrentInfoViewModel.Hash;
                _ = FetchDataAsync();
                _refreshTimer.Interval = TimeSpan.FromMilliseconds(interval);
            }

            ToggleTimerCommand = ReactiveCommand.Create(ToggleTimer);
            CopyIpAndPortCommand = ReactiveCommand.Create(CopyIpAndPort);
        }

        private void ToggleTimer()
        {
            if (_refreshTimer.IsEnabled)
            {
                _refreshTimer.Stop();
                IsPaused = true;
            }
            else
            {
                _ = FetchDataAsync();
            }

            this.RaisePropertyChanged(nameof(IsPaused));
        }

        private void CopyIpAndPort()
        {

        }

        protected override async Task FetchDataAsync()
        {
            TorrentPeers.Clear();
            IsPaused = false;

            var torrentPeers = await QBittorrentService.GetPeerPartialDataAsync(_infoHash, Rid);
            if (torrentPeers != null)
            {
                Update(torrentPeers.PeersChanged, torrentPeers.PeersRemoved);

                _refreshTimer.Start();
            }
        }

        protected override async Task UpdateDataAsync(object? sender, EventArgs e)
        {
            //Debug.WriteLine($"{DateTime.Now:HH:ss} Updating peers for {_infoHash}");
            var torrentPeers = await QBittorrentService.GetPeerPartialDataAsync(_infoHash, Rid);
            if (torrentPeers != null)
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
