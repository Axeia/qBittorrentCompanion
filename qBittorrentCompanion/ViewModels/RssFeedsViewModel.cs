using Avalonia.Controls;
using QBittorrent.Client;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels
{
    public class RssFeedsViewModel : RssPluginSupportBaseViewModel, INotifyDataErrorInfo
    {
        private bool _expandRssArticle = Design.IsDesignMode || ConfigService.ExpandRssArticle;

        public bool ExpandRssArticle
        {
            get => _expandRssArticle;
            set
            {
                if (_expandRssArticle != value)
                {
                    _expandRssArticle = value;
                    ConfigService.ExpandRssArticle = value;
                    this.RaisePropertyChanged(nameof(ExpandRssArticle));
                }
            }
        }

        private bool _expandRssPlugin = Design.IsDesignMode || ConfigService.ExpandRssPlugin;

        public bool ExpandRssPlugin
        {
            get => _expandRssPlugin;
            set
            {
                if (_expandRssPlugin != value)
                {
                    _expandRssPlugin = value;
                    ConfigService.ExpandRssPlugin = value;
                    this.RaisePropertyChanged(nameof(ExpandRssPlugin));
                }
            }
        }

        private Dictionary<string, List<string>> _errors = [];

        private string _rssFeedUrl = "";
        public string RssFeedUrl
        {
            get => _rssFeedUrl;
            set
            {
                _rssFeedUrl = value;
                ValidateRssFeedUrl();
                OnPropertyChanged(nameof(RssFeedUrl));
            }
        }

        private string _rssFeedName = "";
        public string RssFeedName
        {
            get => _rssFeedName;
            set
            {
                RssFeedName = value;
                ValidateRssFeedName();
                OnPropertyChanged(nameof(RssFeedName));
            }
        }

        public bool HasErrors => _errors.Any();

        public new event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public IEnumerable GetErrors(string? propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) || !_errors.ContainsKey(propertyName))
            {
                return Enumerable.Empty<string>();
            }

            return _errors[propertyName];
        }

        private void ValidateRssFeedUrl()
        {
            ClearErrors(nameof(RssFeedUrl));
            if (string.IsNullOrWhiteSpace(_rssFeedUrl))
            {
                AddError(nameof(RssFeedUrl), "URL is required.");
            }
            else if (!Uri.IsWellFormedUriString(_rssFeedUrl, UriKind.Absolute))
            {
                AddError(nameof(RssFeedUrl), "URL is not valid.");
            }
        }

        private void ValidateRssFeedName()
        {
            if (RssFeeds.Any(r => r.Url.ToString() == _rssFeedUrl))
            {
                AddError(nameof(RssFeedUrl), "Another feed already goes by this name");
            }
        }

        private void AddError(string propertyName, string error)
        {
            if (!_errors.ContainsKey(propertyName))
            {
                _errors[propertyName] = new List<string>();
            }

            if (!_errors[propertyName].Contains(error))
            {
                _errors[propertyName].Add(error);
                OnErrorsChanged(propertyName);
            }
        }

        private void ClearErrors(string propertyName)
        {
            if (_errors.ContainsKey(propertyName))
            {
                _errors.Remove(propertyName);
                OnErrorsChanged(propertyName);
            }
        }

        private async Task MarkSelectedFeedAsReadAsync()
        {
            if (SelectedFeed != null)
            {
                try
                {
                    await QBittorrentService.QBittorrentClient.MarkRssItemAsReadAsync(
                        SelectedFeed.Name
                    );
                    // Force update to show changes
                    await ForceUpdateAsync();
                }
                catch (Exception e) { Debug.WriteLine(e.Message); }
            }
        }

        //private DispatcherTimer _refreshTimer = new DispatcherTimer();
        public ObservableCollection<RssFeedViewModel> RssFeeds => 
            RssFeedService.Instance.RssFeeds;

        public async void Initialise()
        {
            _ = RssFeedService.Instance.InitializeAsync();
        }

        public ReactiveCommand<Unit, Unit> DeleteSelectedFeedCommand { get; }
        public ReactiveCommand<Unit, Unit> RefreshAllCommand { get; }
        public ReactiveCommand<Unit, Unit> MarkSelectedFeedAsReadCommand { get; }
        public ReactiveCommand<(string, string?), Unit> AddNewFeedCommand { get; }

        public RssFeedsViewModel()
        {
            //_refreshTimer.Tick += Update;
            DeleteSelectedFeedCommand = ReactiveCommand.CreateFromTask(DeleteSelectedFeedAsync);
            RefreshAllCommand = ReactiveCommand.CreateFromTask(ForceUpdateAsync);
            MarkSelectedFeedAsReadCommand = ReactiveCommand.CreateFromTask(MarkSelectedFeedAsReadAsync);
            AddNewFeedCommand = ReactiveCommand.CreateFromTask<(string, string?)>(CreateNewFeedASync);

            if (Design.IsDesignMode)
            {
                RssFeeds.Add(
                    new RssFeedViewModel(new RssFeed()
                    {
                        Title = "Test feed",
                        Name = "Test feed",
                        Url = new Uri("https://www.tokyotosho.info/rss.php"),
                        Articles = new List<RssArticle>() {
                            new RssArticle() { Author = "Axeia", Title = "The most beautiful title", Date = DateTimeOffset.Now, Description = "Beautiful description", IsRead = false, Id = "1",
                                Link = new Uri("https://github.com/Axeia/qBittorrentCompanion"), TorrentUri = new Uri("https://github.com/Axeia/qBittorrentCompanion") },
                        }
                    })
                );
            }
        }

        private async Task CreateNewFeedASync((string feedUrl, string? feedLabel) parameters)
        {
            var (feedUrl, feedLabel) = parameters;
            if (feedUrl is not null)
            {
                try
                {
                    await QBittorrentService.QBittorrentClient.AddRssFeedAsync(
                        new Uri(feedUrl),
                        feedLabel ?? feedUrl
                    );
                    Initialise(); // Updates (ForceUpdate maybe?)
                }
                catch (Exception e) { Debug.WriteLine(e.Message); }
                finally { Initialise(); }
            }
        }

        private async Task DeleteSelectedFeedAsync()
        {
            if (SelectedFeed is RssFeedViewModel selectedFeed)
            {
                // Delete
                await QBittorrentService.QBittorrentClient.DeleteRssItemAsync(selectedFeed.Name);
                // Re-initialise to refresh
                Initialise();
            }
        }

        private async Task Update(object? sender, EventArgs e)
        {
            try
            {
                RssFolder rssItems = await QBittorrentService.QBittorrentClient.GetRssItemsAsync(true);
                foreach (RssFeed feed in rssItems.Feeds)
                {
                    var existingFeed = RssFeeds.FirstOrDefault(t => t.Name == feed.Name);
                    if (existingFeed is not null)
                    {
                        existingFeed.Update(feed);
                    }
                    else
                    {
                        RssFeeds.Add(new RssFeedViewModel(feed));
                    }
                }
                OnPropertyChanged(nameof(RssFeeds));
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
        }

        public async Task ForceUpdateAsync()
        {
            await Update(null, new EventArgs());
        }

        private RssFeedViewModel? _selectedFeed;
        public RssFeedViewModel? SelectedFeed
        {
            get => _selectedFeed;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedFeed, value);
                /*
                if (_selectedFeed != null)
                    foreach (var f in _selectedFeed.Articles)
                        Debug.WriteLine(f.Title);*/
            }
        }

        private RssArticle? _selectedArticle;
        public RssArticle? SelectedArticle
        {
            get => _selectedArticle;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedArticle, value);

                RssPluginsViewModel.SelectedPlugin.RevalidateOn(
                    _selectedArticle == null ? "" : _selectedArticle.Title
                );
                
                if (_selectedArticle != null)
                {
                    PluginInput = _selectedArticle.Title;
                }
            }
        }

        private ObservableCollection<RssFeedViewModel> _rssFeedsForRule = [];
        public ObservableCollection<RssFeedViewModel> RssFeedsForRule
        {
            get => _rssFeedsForRule;
            set => this.RaiseAndSetIfChanged(ref _rssFeedsForRule, value);
        }
        public void SetRssFeedsForRule(List<RssFeedViewModel> rssFeeds)
        {
            RssFeedsForRule.Clear();
            RssFeedArticlesForRule.Clear();
            //Debug.WriteLine($"Adding {rssFeeds.Count} feeds");
            foreach (var rssFeed in rssFeeds)
            {
                foreach (var article in rssFeed.Articles)
                {
                    RssFeedArticlesForRule.Add(article);
                }
            }
        }
        private ObservableCollection<RssArticle> _rssFeedArticlesForRule = [];
        public ObservableCollection<RssArticle> RssFeedArticlesForRule
        {
            get => _rssFeedArticlesForRule;
            set => this.RaiseAndSetIfChanged(ref _rssFeedArticlesForRule, value);
        }
    }
}