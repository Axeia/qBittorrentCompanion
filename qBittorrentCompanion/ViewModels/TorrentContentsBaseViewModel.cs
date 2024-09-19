using Avalonia.Controls;
using QBittorrent.Client;
using qBittorrentCompanion.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace qBittorrentCompanion.ViewModels
{
    public abstract class TorrentContentsBaseViewModel : AutoUpdateViewModelBase
    {

        protected ObservableCollection<TorrentContentViewModel> _torrentContents = [];

        public ObservableCollection<TorrentContentViewModel> TorrentContents
        {
            get => _torrentContents;
            set
            {
                if (value != _torrentContents)
                {
                    _torrentContents = value;
                    OnPropertyChanged(nameof(TorrentContents));
                }
            }
        }

        protected override async Task FetchDataAsync()
        {
            IReadOnlyList<TorrentContent> torrentContent = await QBittorrentService.QBittorrentClient.GetTorrentContentsAsync(_infoHash);
            Initialise(torrentContent);
        }

        public abstract void Initialise(IReadOnlyList<TorrentContent> torrentContents);

        protected override async Task UpdateDataAsync(object? sender, ElapsedEventArgs e)
        {
            //Debug.WriteLine($"Updating contents for {_infoHash}");
            IReadOnlyList<TorrentContent> torrentContent = await QBittorrentService.QBittorrentClient.GetTorrentContentsAsync(_infoHash);
            Initialise(torrentContent);
        }

        public new event PropertyChangedEventHandler? PropertyChanged;
    }
}