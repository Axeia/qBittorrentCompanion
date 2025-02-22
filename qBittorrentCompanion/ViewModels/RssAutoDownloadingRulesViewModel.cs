using Avalonia.Controls;
using Avalonia.Threading;
using QBittorrent.Client;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels
{
    public class RssAutoDownloadingRulesViewModel : AutoUpdateViewModelBase
    {
        private bool _showExpandedControls = Design.IsDesignMode
            ? false
            : ConfigService.ShowRssExpandedControls;

        public bool ShowExpandedControls
        {
            get => _showExpandedControls;
            set
            {
                if (_showExpandedControls != value)
                {
                    _showExpandedControls = value;
                    ConfigService.ShowRssExpandedControls = value;
                    OnPropertyChanged(nameof(ShowExpandedControls));
                }
            }
        }

        private bool _showTestData = Design.IsDesignMode || ConfigService.ShowRssTestData;
        public bool ShowTestData
        {
            get => _showTestData;
            set
            {
                if (_showTestData != value)
                {
                    _showTestData = value;
                    ConfigService.ShowRssTestData = value;
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
                    if (_selectedRssRule != null)
                    {
                        _activeRssRule = _selectedRssRule.GetCopy();
                        _activeRssRule.Filter();
                        // Store the title separately as it needs to be handled through a rename operation
                        _activeRssRule.OldTitle = _selectedRssRule.Title;
                        OnPropertyChanged(nameof(ActiveRssRule));
                    }
                    else
                        _activeRssRule = GetNewRssRule();

                    OnPropertyChanged(nameof(SelectedRssRule));
                }
            }
        }

        private List<RssAutoDownloadingRuleViewModel> _selectedRssRules = [];
        public List<RssAutoDownloadingRuleViewModel> SelectedRssRules
        {
            get => _selectedRssRules;
            set
            {
                if (value != _selectedRssRules)
                {
                    _selectedRssRules = value;
                    OnPropertyChanged(nameof(SelectedRssRules));
                }
            }
        }

        private RssAutoDownloadingRuleViewModel _activeRssRule;
        public RssAutoDownloadingRuleViewModel ActiveRssRule
        {
            get => _activeRssRule;
            set
            {
                if (value != _activeRssRule)
                {
                    ActiveRssRule.PropertyChanged -= ActiveRssRule_PropertyChanged;
                    _activeRssRule = value;
                    OnPropertyChanged(nameof(ActiveRssRule));
                    ActiveRssRule.PropertyChanged += ActiveRssRule_PropertyChanged;
                }
            }
        }

        private void ActiveRssRule_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RssAutoDownloadingRuleViewModel.IsNew))
            {
                RssRules.Add(ActiveRssRule);
                SelectedRssRule = ActiveRssRule;
            }
        }

        public RssAutoDownloadingRuleViewModel GetNewRssRule()
        {
            return new(
                new RssAutoDownloadingRule(),
                "",
                RssFeeds,
                new List<string>().AsReadOnly()
            )
            { IsNew = true };
        }

        private void TimerTick(object? sender, EventArgs e)
        {
            _ = FetchDataAsync();
            _refreshTimer.Stop();
        }

        public ReactiveCommand<Unit, Unit> DeleteSelectedRulesCommand { get; }
        public ReactiveCommand<Unit, Unit> RefreshRulesCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearSelectedCommand { get; }

        public RssAutoDownloadingRulesViewModel(int intervalInMs = 1500)
        {
            _activeRssRule = GetNewRssRule();
            _rssFeeds.CollectionChanged += SyncRssFeedsToRules;
            _refreshTimer.Interval = TimeSpan.FromMilliseconds(intervalInMs);
            _refreshTimer.Tick += TimerTick;
            ActiveRssRule.PropertyChanged += ActiveRssRule_PropertyChanged;

            DeleteSelectedRulesCommand = ReactiveCommand.CreateFromTask(DeleteSelectedRulesAsync);
            RefreshRulesCommand = ReactiveCommand.CreateFromTask(RefreshRulesAsync);
            ClearSelectedCommand = ReactiveCommand.Create(ClearSelected);
            Debug.WriteLine("\nInstantiated RssAutoDownloadingRulesViewModel\n");
        }

        private void ClearSelected()
        {
            SelectedRssRule = GetNewRssRule();
        }

        private void DataRow_PropertyChanged(object? sender, PropertyChangedEventArgs? e)
        {
            if (e!.PropertyName == nameof(MatchTestRowViewModel.MatchTest) && sender is MatchTestRowViewModel row)
            {
                if (!string.IsNullOrEmpty(row.MatchTest))
                {
                    if (SelectedRssRule?.Rows.Last() == row)
                    {
                        AddNewRow();
                    }
                }
            }
        }

        public void AddNewRow()
        {
            SelectedRssRule?.Rows.Add(new MatchTestRowViewModel());
        }

        private ObservableCollection<RssFeedViewModel> _rssFeeds = [];
        public ObservableCollection<RssFeedViewModel> RssFeeds
        {
            get => _rssFeeds;
            set
            {
                if (value != _rssFeeds)
                {
                    _rssFeeds.CollectionChanged -= SyncRssFeedsToRules;
                    _rssFeeds = value;
                    _rssFeeds.CollectionChanged += SyncRssFeedsToRules;

                    // Create a new NotifyCollectionChangedEventArgs instance
                    var eventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                    SyncRssFeedsToRules(null, eventArgs);
                }
            }
        }

        private void SyncRssFeedsToRules(object? sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var rule in RssRules)
                rule.RssFeeds = RssFeeds;

            Debug.WriteLine($"ActiveRssRule should have {RssFeeds.Count} feeds");
            ActiveRssRule.RssFeeds = RssFeeds;
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
            // First get feeds
            RssFeeds.Clear();
            var rssFolder = await QBittorrentService.QBittorrentClient.GetRssItemsAsync(true);
            foreach (var rssFeed in rssFolder.Feeds)
                RssFeeds.Add(new RssFeedViewModel(rssFeed));
            ActiveRssRule.RssFeeds = RssFeeds;

            // Then get categories.
            Categories.Clear();
            Categories.Add("");
            IReadOnlyDictionary<string, Category> categories = await QBittorrentService.QBittorrentClient.GetCategoriesAsync();
            foreach (var categoryKvp in categories)
                Categories.Add(categoryKvp.Key);

            // Then get rules (which get populated with data from feeds and categories)
            await RefreshRulesAsync();
        }

        public async Task RefreshRulesAsync()
        {
            RssRules.Clear();

            try
            {
                IReadOnlyDictionary<string, RssAutoDownloadingRule> rules = await QBittorrentService.QBittorrentClient.GetRssAutoDownloadingRulesAsync();

                foreach (KeyValuePair<string, RssAutoDownloadingRule> rule in rules)
                {
                    RssRules.Add(
                        new RssAutoDownloadingRuleViewModel(
                            rule.Value, 
                            rule.Key, 
                            RssFeeds,
                            new ReadOnlyCollection<string>(Categories.ToList()) 
                        )
                    );
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected override async Task UpdateDataAsync(object? sender, EventArgs e)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {

        }

        public void Initialise()
        {
            Debug.WriteLine("\nInitialising RssAutoDownloadingRulesViewModel\n");
            _ = FetchDataAsync();
        }

        public async Task AddRule(string name, RssAutoDownloadingRule? newRule = null)
        {
            await QBittorrentService.QBittorrentClient.SetRssAutoDownloadingRuleAsync(name, newRule ?? new RssAutoDownloadingRule());
            await RefreshRulesAsync();

            // Set selection(s) to the new rule
            SelectedRssRule = RssRules.FirstOrDefault(r => r.Title == name);
            SelectedRssRules.Clear();
            if (SelectedRssRule != null)
                SelectedRssRules.Add(SelectedRssRule);
        }

        public async Task DeleteSelectedRulesAsync()
        {
            await DeleteRulesAsync(SelectedRssRules);
            SelectedRssRule = null;
            ActiveRssRule = GetNewRssRule();
        }

        /// <summary>
        /// Deletes the rules one by one and then refreshes <see cref="RssRules"/>
        /// </summary>
        /// <param name="rules"></param>
        public async Task DeleteRulesAsync(IEnumerable<RssAutoDownloadingRuleViewModel> rules)
        {
            Debug.WriteLine($"Delete {rules.Count()} rules");
            if (rules.Any())
            {
                foreach (var rule in rules)
                {
                    try
                    {
                        Debug.WriteLine($"Sending WebRequest for deleting rule `{rule.Title}`");
                        await QBittorrentService.QBittorrentClient.DeleteRssAutoDownloadingRuleAsync(rule.Title);
                    }
                    catch (Exception e) { Debug.WriteLine(e.Message); }
                }

                await RefreshRulesAsync();
            }
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
    }
}