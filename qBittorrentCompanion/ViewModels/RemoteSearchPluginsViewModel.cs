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

        private async Task<Unit> ToggleEnabledSearchPluginAsync(bool enable)
        {
            if( SelectedSearchPlugin != null && enable != SelectedSearchPlugin.IsEnabled)
            {
                try
                {
                    SelectedSearchPlugin.IsEnabled = enable;
                    if (enable)
                        await QBittorrentService.EnableSearchPluginAsync(SelectedSearchPlugin.Name);
                    else
                        await QBittorrentService.DisableSearchPluginAsync(SelectedSearchPlugin.Name);
                }
                catch (Exception)
                {
                    AppLoggerService.AddLogMessage(
                        LogLevel.Warn, 
                        GetFullTypeName<RemoteSearchPluginsViewModel>(), 
                        $"Couldn't change enabled state for {SelectedSearchPlugin.Name}"
                    );
                }
            }

            return Unit.Default;
        }

        public RemoteSearchPluginsViewModel() : base() 
        {
            UninstallSearchPluginCommand = 
                ReactiveCommand.CreateFromTask(UninstallSearchPluginAsync);
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
