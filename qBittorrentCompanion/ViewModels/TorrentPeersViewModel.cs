using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Threading;
using QBittorrent.Client;
using qBittorrentCompanion.Services;
using ReactiveUI;

namespace qBittorrentCompanion.ViewModels
{
    public class TorrentPeersViewModel : AutoUpdateViewModelBase
    {
        private ObservableCollection<TorrentPeerViewModel> _torrentPeers = new();

        private TorrentPeerViewModel? _selectedPeer;
        public TorrentPeerViewModel? SelectedPeer
        {
            get => _selectedPeer;
            set
            {
                if (_selectedPeer != value)
                {
                    _selectedPeer = value;
                    OnPropertyChanged(nameof(SelectedPeer));
                }
            }
        }

        private int _rid = -1;
        protected int Rid
        {
            get
            {
                _rid++;
                return _rid;
            }
        }

        private bool _isPaused = true;
        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                if (_isPaused != value)
                {
                    _isPaused = value;
                    OnPropertyChanged(nameof(IsPaused));
                }
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
                    OnPropertyChanged(nameof(TorrentPeers));
                }
            }
        }

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

            OnPropertyChanged(nameof(IsPaused));
        }

        private void CopyIpAndPort()
        {

        }

        protected override async Task FetchDataAsync()
        {
            TorrentPeers.Clear();
            IsPaused = false;

            var torrentPeers = await QBittorrentService.QBittorrentClient.GetPeerPartialDataAsync(_infoHash, Rid);
            Update(torrentPeers.PeersChanged, torrentPeers.PeersRemoved);

            _refreshTimer.Start();
        }

        protected override async Task UpdateDataAsync(object? sender, EventArgs e)
        {
            //Debug.WriteLine($"{DateTime.Now:HH:ss} Updating peers for {_infoHash}");
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
