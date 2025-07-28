using AutoPropertyChangedGenerator;
using Avalonia.Threading;
using QBittorrent.Client;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels
{
    public partial class SearchPluginsViewModel : AutoUpdateViewModelBase
    {
        public ReactiveCommand<Unit, Unit> UninstallSearchPluginCommand { get; }
        public bool IsPopulating { get; set; } = false;

        private async Task<Unit> UninstallSearchPluginAsync(Unit unit)
        {
            await QBittorrentService.UninstallSearchPluginAsync(SelectedSearchPlugin!.Name);
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

        [AutoPropertyChanged]
        private ObservableCollection<SearchPluginViewModel> _searchPlugins = [];
        [AutoPropertyChanged]
        private SearchPluginViewModel? _selectedSearchPlugin = null;

        protected override async Task FetchDataAsync()
        {
            IsPopulating = true;
            SearchPlugins.Clear();

            IReadOnlyList<SearchPlugin>? plugins = await QBittorrentService.GetSearchPluginsAsync();
            if (plugins != null)
            {
                foreach (SearchPlugin plugin in plugins)
                {
                    SearchPlugins.Add(new SearchPluginViewModel(plugin));
                    //plugin.Categories
                }
                SelectedSearchPlugin = SearchPlugins.First();
            }

            IsPopulating = false;

            //Update(httpSources);
            //_refreshTimer.Start();
        }
        public Task Initialise()
        {
            return FetchDataAsync();
        }


        protected override async Task UpdateDataAsync(object? sender, EventArgs e)
        {
            //Debug.WriteLine($"Updating HttpSources for {_infoHash}");
            IReadOnlyList<Uri>? httpSources = await QBittorrentService.GetTorrentWebSeedsAsync(_infoHash).ConfigureAwait(false);
            if (httpSources != null)
            {
                Update(httpSources);
            }
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
