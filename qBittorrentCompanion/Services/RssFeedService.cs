using Avalonia.Threading;
using QBittorrent.Client;
using qBittorrentCompanion.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace qBittorrentCompanion.Services
{
    // Centralized RSS feed service
    public class RssFeedService : IDisposable
    {
        private static readonly Lazy<RssFeedService> _instance =
            new Lazy<RssFeedService>(() => new RssFeedService());
        public static RssFeedService Instance => _instance.Value;

        // Observable collection that all views can bind to
        public ObservableCollection<RssFeedViewModel> RssFeeds { get; } = [];

        private readonly Timer _updateTimer;

        // Event that classes can subscribe to for notifications
        public event EventHandler FeedsUpdated;

        private RssFeedService()
        {
            // Update feeds every 15 minutes (adjust as needed)
            //_updateTimer = new Timer(async _ => await UpdateFeedsAsync(), null, TimeSpan.Zero, TimeSpan.FromMinutes(15));
        }

        public async Task InitializeAsync()
        {
            await UpdateFeedsAsync();
        }

        public async Task UpdateFeedsAsync()
        {
            try
            {
                // Get the latest feeds from QBittorrent
                var rssItems = await QBittorrentService.QBittorrentClient.GetRssItemsAsync(true);

                // Update on UI thread to avoid cross-thread collection exceptions
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    // Clear existing feeds
                    RssFeeds.Clear();

                    // Add updated feeds
                    foreach (var rssFeed in rssItems.Feeds)
                    {
                        RssFeeds.Add(new RssFeedViewModel(rssFeed));
                    }

                    // Notify subscribers
                    FeedsUpdated?.Invoke(this, EventArgs.Empty);
                });
            }
            catch (Exception ex)
            {
                // Log exception
                Debug.WriteLine($"Error updating RSS feeds: {ex.Message}");
            }
        }

        /*public async Task<RssFeedViewModel> GetFeedAsync(string feedUrl)
        {
            // First check if we have it in our collection
            var feed = RssFeeds.FirstOrDefault(f => f.Url == feedUrl);
            if (feed != null)
                return feed;

            // If not found, try to fetch it individually
            try
            {
                await UpdateFeedsAsync();
                return RssFeeds.FirstOrDefault(f => f.Url == feedUrl);
            }
            catch
            {
                return null;
            }
        }*/

        public void Dispose()
        {
            _updateTimer?.Dispose();
        }
    }
}
