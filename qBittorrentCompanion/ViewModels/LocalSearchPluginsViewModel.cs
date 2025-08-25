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
    public partial class LocalSearchPluginsViewModel : SearchPluginsViewModelBase
    {
        protected override async Task<Unit> UninstallSearchPluginAsync(Unit unit)
        {
            //await QBittorrentService.UninstallSearchPluginAsync(SelectedSearchPlugin!.Name);
            //await Initialise();
            return Unit.Default;
        }

        protected override async Task<Unit> ToggleEnabledSearchPluginAsync(bool enable)
        {
            //if( SelectedSearchPlugin != null )
            //    SelectedSearchPlugin.IsEnabled = enable;

            //await Initialise();
            return Unit.Default;
        }

        public LocalSearchPluginsViewModel() : base() {}

        protected override async Task FetchDataAsync()
        {
            /*IsPopulating = true;
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
            */

            //Update(httpSources);
            //_refreshTimer.Start();
        }
    }
}
