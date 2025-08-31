using AutoPropertyChangedGenerator;
using QBittorrent.Client;
using qBittorrentCompanion.Services;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace qBittorrentCompanion.ViewModels
{
    public partial class RemoteSearchPluginsViewModel : SearchPluginsViewModelBase
    {
        private async Task UninstallSearchPluginAsync()
        {
            await QBittorrentService.UninstallSearchPluginAsync(SelectedSearchPlugin!.Name);
            await Initialise();
        }

        public RemoteSearchPluginsViewModel() : base() 
        {
            UninstallSearchPluginCommand = 
                ReactiveCommand.CreateFromTask(UninstallSearchPluginAsync);
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
                    SearchPlugins.Add(new RemoteSearchPluginViewModel(plugin));
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
