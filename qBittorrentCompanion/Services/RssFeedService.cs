using Avalonia.Controls;
using Avalonia.Threading;
using QBittorrent.Client;
using qBittorrentCompanion.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Services
{
    // Centralized RSS feed service
    public class RssFeedService
    {
        private static readonly Lazy<RssFeedService> _instance = new(() => new());
        public static RssFeedService Instance => _instance.Value;

        // Observable collection that all views can bind to
        public ObservableCollection<RssFeedViewModel> RssFeeds { get; } = Design.IsDesignMode
            ? [new RssFeedViewModel(new RssFeed() { Title = "RSS preview feed", Name = "RSS preview feed" })]
            : [];

        // Event that classes can subscribe to for notifications
        public event EventHandler? FeedsUpdated;

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
            // Get the latest feeds from QBittorrent
            var rssItems = await QBittorrentService.GetRssItemsAsync(true);

            if(rssItems != null)
            {
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
        }
    }
}
