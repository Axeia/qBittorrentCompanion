using DynamicData;
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
using System.Xml.Linq;

namespace qBittorrentCompanion.ViewModels
{
    public class TorrentContentsViewModel : AutoUpdateViewModelBase
    {
        private ObservableCollection<TorrentContentViewModel> _torrentContents = [];
        public ObservableCollection<TorrentContentViewModel> TorrentContents
        {
            get => _torrentContents;
            set
            {
                if(value != _torrentContents)
                {
                    _torrentContents = value;
                    OnPropertyChanged();
                }
            }
        }

        public TorrentContentsViewModel(TorrentInfoViewModel? torrentInfoViewModel, int interval = 1500*7)
        {
            if (torrentInfoViewModel is not null && torrentInfoViewModel.Hash is string hash)
            {
                _infoHash = hash;
                _ = FetchDataAsync();
                _refreshTimer.Interval = TimeSpan.FromMilliseconds(interval);
            }
        }

        protected override async Task FetchDataAsync()
        {
            IReadOnlyList<TorrentContent> torrentContent = await QBittorrentService.QBittorrentClient.GetTorrentContentsAsync(_infoHash);
            Initialise(torrentContent);

            /*var httpSources = await QBittorrentService.GetTorrentContentsAsync(_infoHash);
            if (httpSources is not null)
                Update(httpSources, false);

            _refreshTimer.Start();*/
        }
        protected override async Task UpdateDataAsync(object? sender, ElapsedEventArgs e)
        {
            //Debug.WriteLine($"Updating contents for {_infoHash}");
            IReadOnlyList<TorrentContent> torrentContent = await QBittorrentService.QBittorrentClient.GetTorrentContentsAsync(_infoHash);
            Initialise(torrentContent);
        }

        public void Initialise(IReadOnlyList<TorrentContent> torrentContents)
        {
            var folders = new Dictionary<string, TorrentContentViewModel>();
            HashSet<string> fullFolderPaths = [];
            string reassembledPath = "";

            foreach (var torrentContent in torrentContents)
            {
                foreach (string pathPart in GetPathParts(torrentContent.Name))
                {
                    reassembledPath += pathPart + "/";
                    if (!fullFolderPaths.Contains(reassembledPath))
                    {
                        TorrentContents.Add(new TorrentContentViewModel(reassembledPath, pathPart));
                        fullFolderPaths.Add(reassembledPath);
                    }
                }
                TorrentContents.Add(new TorrentContentViewModel(torrentContent));

                reassembledPath = ""; // Reset
            }
        }

        public void Update(IReadOnlyList<TorrentContent> torrentContents)
        {
            foreach (var torrentContent in torrentContents)
            {
                var existingFile = TorrentContents
                    .OfType<TorrentContentViewModel>()
                    .FirstOrDefault(file => file.Name == torrentContent.Name);

                existingFile?.Update(torrentContent);
            }
        }

        private string[] GetPathParts(string name)
        {
            return name.Contains("/") ? name.Split('/').Take(name.Split('/').Length - 1).ToArray() : Array.Empty<string>();
        }

        private string FileName(string name)
        {
            return name.Split("/").Last();
        }
    }
}
