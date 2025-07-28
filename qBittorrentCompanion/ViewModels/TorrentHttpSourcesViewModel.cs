using AutoPropertyChangedGenerator;
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
    public partial class TorrentHttpSourcesViewModel : AutoUpdateViewModelBase
    {
        [AutoPropertyChanged]
        private string? _selectedHttpSource = null;
        [AutoPropertyChanged]
        private ObservableCollection<string> _httpSources = [];

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
