using Avalonia.Threading;
using DynamicData;
using QBittorrent.Client;
using qBittorrentCompanion.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Services
{
    // Centralized search plugin service
    public class RemoteSearchPluginService
    {
        public readonly static List<SearchPluginCategory> DefaultCategories = [new SearchPluginCategory(SearchPlugin.All, "All categories")];

        private static readonly Lazy<RemoteSearchPluginService> _instance =
            new(() => new RemoteSearchPluginService());
        public static RemoteSearchPluginService Instance => _instance.Value;
        public ObservableCollection<RemoteSearchPluginViewModel> SearchPlugins { get; } = [
            new RemoteSearchPluginViewModel(new SearchPlugin() { FullName = "Only enabled", Name = SearchPlugin.Enabled, Categories = DefaultCategories }),
            new RemoteSearchPluginViewModel(new SearchPlugin() { FullName = "All plugins", Name = SearchPlugin.All, Categories = DefaultCategories })
        ];

        private RemoteSearchPluginService()
        {
            // Update feeds every 15 minutes (adjust as needed)
            //_updateTimer = new Timer(async _ => await UpdateFeedsAsync(), null, TimeSpan.Zero, TimeSpan.FromMinutes(15));
        }

        public async Task InitializeAsync()
        {
            await UpdateSearchPluginsAsync();
        }

        public async Task UpdateSearchPluginsAsync()
        {

            // Get the latest plugins from QBittorrent
            var searchPlugins = await QBittorrentService.GetSearchPluginsAsync();

            if(searchPlugins != null)
            {
                // Update on UI thread to avoid cross-thread collection exceptions
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    foreach (var searchPlugin in searchPlugins)
                    {
                        SearchPlugins.Add(new RemoteSearchPluginViewModel(searchPlugin));
                    }
                });
            }
        }
    }
}
