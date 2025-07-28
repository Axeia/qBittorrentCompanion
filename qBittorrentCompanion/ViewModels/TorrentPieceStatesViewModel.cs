using System;
using System.Threading.Tasks;
using QBittorrent.Client;
using qBittorrentCompanion.Services;
using System.Collections.Generic;
using AutoPropertyChangedGenerator;

namespace qBittorrentCompanion.ViewModels
{
    public partial class TorrentPieceStatesViewModel : AutoUpdateViewModelBase
    {
        [AutoPropertyChanged]
        private IReadOnlyList<TorrentPieceState> _torrentPieceStates = [];

        public TorrentPieceStatesViewModel(TorrentInfoViewModel? torrentInfoViewModel, int interval = 3000)
        {
            // Eventually populates _torrentProperties
            if (torrentInfoViewModel is not null && torrentInfoViewModel.Hash is not null)
            {
                _infoHash = torrentInfoViewModel.Hash.ToString();
                _ = FetchDataAsync();
                _refreshTimer.Interval = TimeSpan.FromMilliseconds(interval);
            }

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