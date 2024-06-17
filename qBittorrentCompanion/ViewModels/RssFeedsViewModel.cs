using Avalonia.Threading;
using QBittorrent.Client;
using qBittorrentCompanion.Models;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace qBittorrentCompanion.ViewModels
{
    public class RssFeedsViewModel : ViewModelBase, INotifyDataErrorInfo
    {
        private Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();

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

        public new event PropertyChangedEventHandler PropertyChanged;
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


        private ObservableCollection<RssFeedViewModel> _rssFeeds = [];
        //private DispatcherTimer _refreshTimer = new DispatcherTimer();
        public ObservableCollection<RssFeedViewModel> RssFeeds
        {
            get => _rssFeeds;
            set => this.RaiseAndSetIfChanged(ref _rssFeeds, value);
        }

        public async void Initialise()
        {
            RssFolder rssItems = await QBittorrentService.QBittorrentClient.GetRssItemsAsync(true);
            
            RssFeeds.Clear();
            foreach(RssFeed feed in rssItems.Feeds)
                RssFeeds.Add(new RssFeedViewModel(feed));
        }

        public RssFeedsViewModel()
        {
            //_refreshTimer.Tick += Update;
        }

        private async void Update(object? sender, EventArgs e)
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

        public void ForceUpdate()
        {
            Update(null, new EventArgs());
        }

        private RssFeedViewModel? _selectedFeed;
        public RssFeedViewModel? SelectedFeed
        {
            get => _selectedFeed;
            set => this.RaiseAndSetIfChanged(ref _selectedFeed, value);
        }

        private RssArticle? _selectedArticle;
        public RssArticle? SelectedArticle
        {
            get => _selectedArticle;
            set => this.RaiseAndSetIfChanged(ref _selectedArticle, value);
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

        private string? _rssRuleMustContain = "";
        public string? RssRuleMustContain
        {
            get => _rssRuleMustContain;
            set => this.RaiseAndSetIfChanged(ref _rssRuleMustContain, value);
        }

    }
}