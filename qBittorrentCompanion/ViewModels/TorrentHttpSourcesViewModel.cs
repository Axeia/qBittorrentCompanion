using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using DynamicData;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Models;
using qBittorrentCompanion.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels
{
    public class TorrentHttpSourcesViewModel : AutoUpdateViewModelBase
    {
        private string? _selectedHttpSource = null;
        public string? SelectedHttpSource
        {
            get => _selectedHttpSource;
            set
            {
                if(value != _selectedHttpSource)
                {
                    _selectedHttpSource = value;
                    OnPropertyChanged(SelectedHttpSource);
                }
            }
        }

        private ObservableCollection<string> _httpSources = [];
        public ObservableCollection<string> HttpSources
        {
            get => _httpSources;
            set
            {
                if(value != _httpSources)
                {
                    _httpSources = value;
                    OnPropertyChanged(nameof(HttpSources));
                }
            }
        }

        public TorrentHttpSourcesViewModel(TorrentInfoViewModel? torrentInfoViewModel, int interval = 1500*7)
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
            IReadOnlyList<Uri>? httpSources = await QBittorrentService.GetTorrentWebSeedsAsync(_infoHash).ConfigureAwait(false);
            if(httpSources != null)
                Update(httpSources);
        }
        protected override async Task UpdateDataAsync(object? sender, EventArgs e)
        {
            await FetchDataAsync();
        }

        public async void Update(IReadOnlyList<Uri> httpSources)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                foreach (Uri httpSource in httpSources)
                {
                    if (!HttpSources.Contains(httpSource.AbsoluteUri))
                        HttpSources.Add(httpSource.AbsoluteUri);
                }
            });
        }
    }
}
