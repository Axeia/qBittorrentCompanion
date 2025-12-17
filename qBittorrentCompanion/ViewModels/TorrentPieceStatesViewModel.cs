using QBittorrent.Client;
using qBittorrentCompanion.Services;
using RaiseChangeGenerator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels
{
    public partial class TorrentPieceStatesViewModel : AutoUpdateViewModelBase
    {
        [RaiseChange]
        private IReadOnlyList<TorrentPieceState> _torrentPieceStates = [];

        public TorrentPieceStatesViewModel(TorrentInfoViewModel? torrentInfoViewModel, int interval = 3000)
        {
            if (torrentInfoViewModel is not null && torrentInfoViewModel.Hash is not null)
            {
                _infoHash = torrentInfoViewModel.Hash.ToString();
                _ = FetchDataAsync();
                _refreshTimer.Interval = TimeSpan.FromMilliseconds(interval);
            }
            else
                Debug.WriteLine("Creating TorrentPieceStatesViewModel with null or missing hash");
        }

        protected override async Task FetchDataAsync()
        {
            var torrentPieceStates = await QBittorrentService.GetTorrentPiecesStatesAsync(_infoHash);
            if (torrentPieceStates != null)
            {
                TorrentPieceStates = torrentPieceStates;
                _refreshTimer.Start();
            }
        }

        protected override async Task UpdateDataAsync(object? sender, EventArgs e)
        {
            var torrentPieceStates = await QBittorrentService.GetTorrentPiecesStatesAsync(_infoHash);
            if (torrentPieceStates != null)
                TorrentPieceStates = torrentPieceStates;
        }
    }
}