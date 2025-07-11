﻿using System;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels
{
    using QBittorrent.Client;
    using qBittorrentCompanion.Helpers;
    using qBittorrentCompanion.Models;
    using qBittorrentCompanion.Services;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Timers;

    public class TorrentPieceStatesViewModel : AutoUpdateViewModelBase
    {
        private IReadOnlyList<TorrentPieceState> _torrentPieceStates = [];
        public IReadOnlyList<TorrentPieceState> TorrentPieceStates
        {
            get => _torrentPieceStates;
            set
            {
                if (value != _torrentPieceStates)
                {
                    _torrentPieceStates = value;
                    OnPropertyChanged(nameof(TorrentPieceStates));
                }
            }
        }

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