using Avalonia.Threading;
using DynamicData;
using QBittorrent.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Services
{
    // Centralized search plugin service
    public class SearchPluginService : IDisposable
    {
        private static readonly Lazy<SearchPluginService> _instance =
            new Lazy<SearchPluginService>(() => new SearchPluginService());
        public static SearchPluginService Instance => _instance.Value;
        public static List<SearchPluginCategory> DefaultCategories = [new SearchPluginCategory(SearchPlugin.All, "All categories")];

        // Observable collection that all views can bind to
        public ObservableCollection<SearchPlugin> SearchPlugins { get; } = [
            new SearchPlugin() { FullName = "Only enabled", Name = SearchPlugin.Enabled, Categories = DefaultCategories },
            new SearchPlugin() { FullName = "All plugins", Name = SearchPlugin.All, Categories = DefaultCategories }
        ];

        private readonly Timer _updateTimer;

        // Event that classes can subscribe to for notifications
        public event EventHandler SearchPluginsUpdated;

        private SearchPluginService()
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
            try
            {
                // Get the latest plugins from QBittorrent
                var searchPlugins = await QBittorrentService.QBittorrentClient.GetSearchPluginsAsync();

                // Update on UI thread to avoid cross-thread collection exceptions
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    SearchPlugins.Add(searchPlugins);

                    // Notify subscribers
                    SearchPluginsUpdated?.Invoke(this, EventArgs.Empty);
                });
            }
            catch (Exception ex)
            {
                // Log exception
                Debug.WriteLine($"Error updating search plugins: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _updateTimer?.Dispose();
        }
    }
}
