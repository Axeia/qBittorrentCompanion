using qBittorrentCompanion.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Services
{
    public class RssFeedsService : INotifyPropertyChanged
    {
        private static readonly Lazy<RssFeedsService> lazy =
            new Lazy<RssFeedsService>(() => new RssFeedsService());

        public static RssFeedsService Instance { get { return lazy.Value; } }

        private ObservableCollection<RssFeedViewModel> _rssFeeds = new();

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<RssFeedViewModel> RssFeeds => _rssFeeds;

        private RssFeedsService()
        {
        }

        public async Task InitiateAsync()
        {
            var rssFeeds = new ObservableCollection<RssFeedViewModel>();
            var rssItems = await QBittorrentService.QBittorrentClient.GetRssItemsAsync(true);

            foreach (var rssFeed in rssItems.Feeds)
            {
                rssFeeds.Add(new RssFeedViewModel(rssFeed));
            }

            _rssFeeds = rssFeeds;
            _rssFeeds.CollectionChanged += _rssFeeds_CollectionChanged;
            Debug.WriteLine($"RssFeeds: {_rssFeeds.Count}");
            OnPropertyChanged(nameof(RssFeeds));
        }

        private void _rssFeeds_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Debug.WriteLine("RSS Feeds collection changed");
        }
    }
}
