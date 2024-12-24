using Avalonia.Controls;
using DynamicData;
using QBittorrent.Client;
using qBittorrentCompanion.Models;
using qBittorrentCompanion.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels
{
    public class RssAutoDownloadingRulesViewModel : AutoUpdateViewModelBase
    {
        private ObservableCollection<MatchTestRowViewModel> _rows = [];
        public ObservableCollection<MatchTestRowViewModel> Rows
        {
            get => _rows;
            set
            {
                _rows = value;
                OnPropertyChanged(nameof(Rows));
            }
        }

        private bool _showTestData = Design.IsDesignMode ? true : ConfigService.ShowRssTestData;
        public bool ShowTestData
        {
            get => _showTestData;
            set
            {
                if (_showTestData != value)
                {
                    _showTestData = value;
                    OnPropertyChanged(nameof(ShowTestData));
                }
            }
        }

        private ObservableCollection<RssAutoDownloadingRuleViewModel> _rssRules = [];
        public ObservableCollection<RssAutoDownloadingRuleViewModel> RssRules
        {
            get => _rssRules;
            set
            {
                if (value != _rssRules)
                {
                    _rssRules = value;
                    OnPropertyChanged(nameof(RssRules));
                }
            }
        }

        private RssAutoDownloadingRuleViewModel? _selectedRssRule;
        public RssAutoDownloadingRuleViewModel? SelectedRssRule
        {
            get => _selectedRssRule;
            set
            {
                if (value != _selectedRssRule)
                {
                    _selectedRssRule = value;
                    if (_selectedRssRule is not null)
                    {
                        _selectedRssRule.Categories = new ReadOnlyCollection<string>(Categories.ToList());
                        // Tried using a Dictonary but the ViewModel was displaying the entire KeyValuePair regardless of if
                        // key or value was used. So SimplifiedRssFeed it is.
                        _selectedRssRule.RssFeeds = new ReadOnlyCollection<SimplifiedRssFeed>(RssFeeds.Select(f => new SimplifiedRssFeed(f.Url, f.Title)).ToList());
                        _selectedRssRule.RssArticles.Add<RssArticle>(GetArticlesFromRssFeeds(GetRssFeedsForRule(_selectedRssRule)));
                        _selectedRssRule.Filter();

                        // Set the testData
                        Rows.Clear();
                        var testData = RssRuleTestDataService.GetEntry(_selectedRssRule.Title);
                        if (testData != null)
                        {
                            foreach (var testCase in testData)
                            {
                                // Setting MatchTest will trigger MatchTestRowViewModel.RunMatch()
                                Rows.Add(new MatchTestRowViewModel(_selectedRssRule.MustContain) { MatchTest = testCase });
                            }
                        }
                        if (Rows.Count > 0 && Rows.Last().MatchTest != string.Empty)
                            Rows.Add(new MatchTestRowViewModel(_selectedRssRule.MustContain));
                    }
                    else
                    {
                        Rows.Clear();
                    }
                    OnPropertyChanged(nameof(SelectedRssRule));
                }
            }
        }

        private List<RssFeedViewModel> GetRssFeedsForRule(RssAutoDownloadingRuleViewModel rule)
        {
            List<RssFeedViewModel> rssFeeds = [];
            foreach(var rssFeed in RssFeeds)
            {
                foreach (var simpFeed in rule.RssFeeds)
                {
                    if (rssFeed.Url == simpFeed.Url)
                    {
                        rssFeeds.Add(rssFeed);
                    }
                }
            }
            return rssFeeds;
        }

        private List<RssArticle> GetArticlesFromRssFeeds(List<RssFeedViewModel> rssFeeds)
        {
            List<RssArticle> articles = new List<RssArticle>();
            return rssFeeds.SelectMany(f => f.Articles).ToList();
        }

        public RssAutoDownloadingRulesViewModel(int intervalInMs = 1500)
        {
            _refreshTimer.Interval = TimeSpan.FromMilliseconds(intervalInMs);
            Rows.CollectionChanged += Rows_CollectionChanged;
            AddNewRow();
        }
        private void Rows_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs? e)
        {
            foreach (var item in Rows)
            {
                item.PropertyChanged -= DataRow_PropertyChanged;
                item.PropertyChanged += DataRow_PropertyChanged;
            }
        }

        private void DataRow_PropertyChanged(object? sender, PropertyChangedEventArgs? e)
        {
            if (e!.PropertyName == nameof(MatchTestRowViewModel.MatchTest) && sender is MatchTestRowViewModel row)
            {
                if (!string.IsNullOrEmpty(row.MatchTest))
                {
                    if (Rows.Last() == row)
                    {
                        AddNewRow();
                    }
                }
            }
        }

        public void AddNewRow(string regex = "")
        {
            Rows.Add(new MatchTestRowViewModel(regex));
        }

        private ObservableCollection<RssFeedViewModel> _rssFeeds = [];
        public ObservableCollection<RssFeedViewModel> RssFeeds
        {
            get => _rssFeeds;
            set
            {
                if (value != _rssFeeds)
                {
                    _rssFeeds = value;
                    OnPropertyChanged(nameof(RssFeeds));
                }
            }
        }

        private ObservableCollection<string> _categories = [];
        public ObservableCollection<string> Categories
        {
            get => _categories;
            set
            {
                if (value != _categories)
                {
                    _categories = value;
                    OnPropertyChanged(nameof(Categories));
                }
            }
        }

        protected async override Task FetchDataAsync()
        {
            await RefreshRules();

            RssFeeds.Clear();
            var rssFolder = await QBittorrentService.QBittorrentClient.GetRssItemsAsync(true);
            foreach (var rssFeed in rssFolder.Feeds)
                RssFeeds.Add(new RssFeedViewModel(rssFeed));

            Categories.Clear();
            Categories.Add("");
            IReadOnlyDictionary<string, Category> categories = await QBittorrentService.QBittorrentClient.GetCategoriesAsync();
            foreach (var categoryKvp in categories)
                Categories.Add(categoryKvp.Key);
        }

        public async Task RefreshRules()
        {
            RssRules.Clear();
            IReadOnlyDictionary<string, RssAutoDownloadingRule> rules = await QBittorrentService.QBittorrentClient.GetRssAutoDownloadingRulesAsync();
            foreach (var rule in rules)
                RssRules.Add(new RssAutoDownloadingRuleViewModel(rule.Value, rule.Key));
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected override async Task UpdateDataAsync(object? sender, EventArgs e)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {

        }

        public void Initialise()
        {
            _ = FetchDataAsync();
        }

        public async void AddRule(string name)
        {
            await QBittorrentService.QBittorrentClient.SetRssAutoDownloadingRuleAsync(name, new RssAutoDownloadingRule());
            await RefreshRules();
            foreach (var rule in RssRules)
            {
                if (rule.Title == name)
                {
                    SelectedRssRule = rule;
                }
            }
        }

        /// <summary>
        /// Deletes the rules one by one and then refreshes <see cref="RssRules"/>
        /// </summary>
        /// <param name="rules"></param>
        public async void DeleteRules(IEnumerable<RssAutoDownloadingRuleViewModel> rules)
        {
            foreach (var rule in rules)
            {
                try
                {
                    await QBittorrentService.QBittorrentClient.DeleteRssAutoDownloadingRuleAsync(rule.Title);
                }
                catch(Exception e) { Debug.WriteLine(e.Message); }
            }
            await RefreshRules();
        }

        private int _articleCount = 0;
        public int ArticleCount
        {
            get => _articleCount;
            set
            {
                if (value != _articleCount)
                {
                    _articleCount = value;
                    OnPropertyChanged(nameof(ArticleCount));
                }
            }
        }

        private int _filteredArticleCount = 0;
        public int FilteredArticleCount
        {
            get => _filteredArticleCount;
            set
            {
                if (value != _filteredArticleCount)
                {
                    _filteredArticleCount = value;
                    OnPropertyChanged(nameof(FilteredArticleCount));
                }
            }
        }

        /*
        private void UpdateFiltered()
        {
            UpdateTestMatches();
            IEnumerable<RssArticle> filteredArticles = RssFeedArticlesForRule;

            // No need to apply any other filter. Invalid `FilterMustContain` = no matches, the end.
            if (!IsMustContainValid)
            {
                Debug.WriteLine($"Invalid FilterMustContain {FilterMustContain}");
                FilteredRssFeedArticlesForRule = new ObservableCollection<RssArticle>();
                return;
            }

            // FilterMustContain should be applied.
            if (!string.IsNullOrEmpty(FilterMustContain))
            {
                if (FilterUseRegex)
                {
                    Debug.WriteLine("Filtering FilterMustContain on regex");
                    filteredArticles = RssFeedArticlesForRule.Where(a => _regexMustContain.IsMatch(a.Title));
                    Debug.WriteLine($"After FilterMustContain, {filteredArticles.Count()} articles remain");
                }
            }

            // FilterMustNotContain should be applied
            if (!string.IsNullOrEmpty(FilterMustNotContain) && IsMustNotContainValid)
            {
                if (FilterUseRegex)
                {
                    Debug.WriteLine("Filtering FilterMustNotContain on regex");
                    var articlesToRemove = filteredArticles.Where(a => _regexMustNotContain.IsMatch(a.Title)).ToList();
                    Debug.WriteLine($"Regex: {FilterMustNotContain} removed {articlesToRemove.Count} out of {filteredArticles.Count()} articles");

                    foreach (var article in articlesToRemove)
                    {
                        Debug.WriteLine($"Regex: {FilterMustNotContain} Removing article with title: {article.Title}");
                    }

                    filteredArticles = filteredArticles.Except(articlesToRemove);
                    Debug.WriteLine($"After FilterMustNotContain, {filteredArticles.Count()} articles remain");
                }
            }

            FilteredRssFeedArticlesForRule = new ObservableCollection<RssArticle>(filteredArticles.ToList());
            FilteredArticleCount = FilteredRssFeedArticlesForRule.Count;
        }

        public void UpdateTestMatches()
        {
            foreach (var row in Rows)
                row.RegexStr = FilterMustContain;
        }*/

    }
}