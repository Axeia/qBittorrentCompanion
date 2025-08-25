using Avalonia.Threading;
using DynamicData;
using System;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Services
{
    // Centralized search plugin service
    public class RemoteSearchPluginService : SearchPluginServiceBase
    {
        private static readonly Lazy<RemoteSearchPluginService> _instance =
            new(() => new RemoteSearchPluginService());
        public static RemoteSearchPluginService Instance => _instance.Value;

        // Event that classes can subscribe to for notifications
        public event EventHandler? SearchPluginsUpdated;

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
                    SearchPlugins.Add(searchPlugins);

                    // Notify subscribers
                    SearchPluginsUpdated?.Invoke(this, EventArgs.Empty);
                });
            }
        }
    }
}
