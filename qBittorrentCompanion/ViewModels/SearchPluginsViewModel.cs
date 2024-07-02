using Avalonia.Controls;
using Avalonia.Threading;
using DynamicData;
using QBittorrent.Client;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Timers;

namespace qBittorrentCompanion.ViewModels
{
    public class SearchPluginsViewModel : AutoUpdateViewModelBase
    {
        public ReactiveCommand<Unit, Unit> UninstallSearchPluginCommand { get; }
        public bool IsPopulating { get; set; } = false;

        private async Task<Unit> UninstallSearchPluginAsync(Unit unit)
        {
            await QBittorrentService.QBittorrentClient.UninstallSearchPluginAsync(SelectedSearchPlugin!.Name);
            await Initialise();
            return Unit.Default;
        }

        public ReactiveCommand<bool, Unit> ToggleEnabledSearchPluginCommand { get; }
        private async Task<Unit> ToggleEnabledSearchPluginAsync(bool enable)
        {
            if( SelectedSearchPlugin != null )
                SelectedSearchPlugin.IsEnabled = enable;

            await Initialise();
            return Unit.Default;
        }

        public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

        public SearchPluginsViewModel()
        {
            UninstallSearchPluginCommand = ReactiveCommand.CreateFromTask<Unit, Unit>(UninstallSearchPluginAsync);
            ToggleEnabledSearchPluginCommand = ReactiveCommand.CreateFromTask<bool, Unit>(ToggleEnabledSearchPluginAsync);
            RefreshCommand = ReactiveCommand.CreateFromTask(Initialise);
        }

        private ObservableCollection<SearchPluginViewModel> _searchPlugins = [];

        public ObservableCollection<SearchPluginViewModel> SearchPlugins
        {
            get => _searchPlugins;
            set
            {
                if (_searchPlugins != value)
                {
                    _searchPlugins = value;
                    OnPropertyChanged(nameof(SearchPlugins));
                }
            }
        }

        private SearchPluginViewModel? _selectedSearchPlugin = null;
        public SearchPluginViewModel? SelectedSearchPlugin
        {
            get => _selectedSearchPlugin;
            set
            {
                if (_selectedSearchPlugin != value)
                {
                    _selectedSearchPlugin = value;
                    OnPropertyChanged(nameof(SelectedSearchPlugin));
                }
            }
        }

        protected override async Task FetchDataAsync()
        {
            IsPopulating = true;
            SearchPlugins.Clear();

            IReadOnlyList<SearchPlugin> plugins = await QBittorrentService.QBittorrentClient.GetSearchPluginsAsync();
            foreach (SearchPlugin plugin in plugins)
            {
                SearchPlugins.Add(new SearchPluginViewModel(plugin));
                //plugin.Categories
            }
            SelectedSearchPlugin = SearchPlugins.First();

            IsPopulating = false;

            //Update(httpSources);
            //_refreshTimer.Start();
        }
        public Task Initialise()
        {
            return FetchDataAsync();
        }


        protected override async Task UpdateDataAsync(object? sender, ElapsedEventArgs e)
        {
            //Debug.WriteLine($"Updating HttpSources for {_infoHash}");
            IReadOnlyList<Uri> httpSources = await QBittorrentService.QBittorrentClient.GetTorrentWebSeedsAsync(_infoHash).ConfigureAwait(false);
            Update(httpSources);
        }

        public async void Update(IReadOnlyList<Uri> httpSources)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                //
            });
        }
    }
}
