using AutoPropertyChangedGenerator;
using QBittorrent.Client;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels
{
    public partial class RemoteSearchPluginsViewModel : SearchPluginsViewModelBase
    {
        private async Task<Unit> UninstallSearchPluginAsync(Unit unit)
        {
            await QBittorrentService.UninstallSearchPluginAsync(SelectedSearchPlugin!.Name);
            await Initialise();
            return Unit.Default;
        }

        private async Task<Unit> ToggleEnabledSearchPluginAsync(bool enable)
        {
            if( SelectedSearchPlugin != null )
                SelectedSearchPlugin.IsEnabled = enable;

            await Initialise();
            return Unit.Default;
        }

        public RemoteSearchPluginsViewModel() : base() 
        {
            UninstallSearchPluginCommand = 
                ReactiveCommand.CreateFromTask<Unit, Unit>(UninstallSearchPluginAsync);
            ToggleEnabledSearchPluginCommand = 
                ReactiveCommand.CreateFromTask<bool, Unit>(ToggleEnabledSearchPluginAsync);
        }

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

        [AutoPropertyChanged]
        private string _lastRemoteProcessMessage = string.Empty;
    }
}
