using Avalonia.Controls;
using Avalonia.Threading;
using QBittorrent.Client;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels
{
    public class RssAutoDownloadingRulesViewModel : RssPluginSupportBaseViewModel
    {
        protected DispatcherTimer _refreshTimer = new();

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
                    this.RaisePropertyChanged(nameof(ShowExpandedControls));
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
                    this.RaisePropertyChanged(nameof(ShowTestData));
                }
            }
        }

        private bool _expandRssRuleRssPlugin = Design.IsDesignMode || ConfigService.ExpandSearchRssPlugin;
        public bool ExpandRssRuleRssPlugin
        {
            get => ConfigService.ExpandRssRuleRssPlugin;
            set
            {
                ConfigService.ExpandRssRuleRssPlugin = value;
                this.RaiseAndSetIfChanged(ref _expandRssRuleRssPlugin, value);
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
                    this.RaisePropertyChanged(nameof(RssRules));
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
                        ActiveRssRule = _selectedRssRule.GetCopy();

                        ActiveRssRule.Filter();
                        // Store the title separately as it needs to be handled through a rename operation
                        ActiveRssRule.OldTitle = _selectedRssRule.Title;
                        this.RaisePropertyChanged(nameof(ActiveRssRule));
                    }
                    else
                        ActiveRssRule = GetNewRssRule();

                    this.RaisePropertyChanged(nameof(SelectedRssRule));
                }
            }
        }

        private void SetDataGridCollectionViewObservers()
        {
            throw new NotImplementedException();
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
                    this.RaisePropertyChanged(nameof(SelectedRssRules));
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
                    if (ActiveRssRule.DataGridCollectionView != null)
                        ActiveRssRule.DataGridCollectionView!.CurrentChanged -= DataGridCollectionView_CurrentChanged;
                    else
                        Debug.WriteLine("Not supposed to happen");

                    _activeRssRule = value;

                    this.RaisePropertyChanged(nameof(ActiveRssRule));
                    ActiveRssRule.PropertyChanged += ActiveRssRule_PropertyChanged;
                    if (ActiveRssRule.DataGridCollectionView != null)
                        ActiveRssRule.DataGridCollectionView!.CurrentChanged += DataGridCollectionView_CurrentChanged;
                    else
                        Debug.WriteLine("Not supposed to happen");
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

            if (e.PropertyName == nameof(RssAutoDownloadingRuleViewModel.DataGridCollectionView)
                && ActiveRssRule.DataGridCollectionView != null)
            {
                ActiveRssRule.DataGridCollectionView.CurrentChanged += DataGridCollectionView_CurrentChanged;
            }
        }

        private void DataGridCollectionView_CurrentChanged(object? sender, EventArgs e)
        {
            //Debug.WriteLine($"RulesViewModel DataGridCollectionView_CurrentChanged called");
            if (ActiveRssRule.DataGridCollectionView != null 
                && ActiveRssRule.DataGridCollectionView.CurrentItem is RssArticleViewModel selectedRssArticleViewModel)
            {
                PluginInput = selectedRssArticleViewModel.Title;
            }
        }

        public RssAutoDownloadingRuleViewModel GetNewRssRule(IReadOnlyList<Uri>? affectedFeeds = null)
        {
            affectedFeeds = affectedFeeds ?? [];

            return new RssAutoDownloadingRuleViewModel(
                new RssAutoDownloadingRule() { AffectedFeeds = affectedFeeds },
                ""
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
        public ReactiveCommand<Unit, Unit> UsePluginToPopulateFieldsCommand { get; }

        public RssAutoDownloadingRulesViewModel(int intervalInMs = 1500)
        {
            _activeRssRule = GetNewRssRule();
            ActiveRssRule.PropertyChanged += ActiveRssRule_PropertyChanged;

            _refreshTimer.Interval = TimeSpan.FromMilliseconds(intervalInMs);
            _refreshTimer.Tick += TimerTick;

            DeleteSelectedRulesCommand = ReactiveCommand.CreateFromTask(DeleteSelectedRulesAsync);
            RefreshRulesCommand = ReactiveCommand.CreateFromTask(RefreshRulesAsync);
            ClearSelectedCommand = ReactiveCommand.Create(ClearSelected);
            UsePluginToPopulateFieldsCommand = ReactiveCommand.Create(UsePluginToPopulateFields);
        }

        private void UsePluginToPopulateFields()
        {
            if (PluginIsSuccess)
            {
                ActiveRssRule.Title = PluginRuleTitle;
                ActiveRssRule.MustContain = PluginResult;
            }
            else
                Debug.WriteLine("Plugin could not process the input");
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
                    _rssFeeds = value;
                }
            }
        }

        protected async Task FetchDataAsync()
        {
            // Clear existing data
            RssFeeds.Clear();

            // Start calls needs to be executed before calling RefreshRulesAsync
            var rssFolderTask = QBittorrentService.QBittorrentClient.GetRssItemsAsync(true);
            var categoriesTask = CategoryService.Instance.InitializeAsync();
            await Task.WhenAll(rssFolderTask, categoriesTask);

            // Get rules (needs data from RssFeeds and Categories)
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
                            rule.Key
                        )
                    );
                }
            }
            catch (Exception e) { Debug.WriteLine(e.Message); }
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected async Task UpdateDataAsync(object? sender, EventArgs e)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {

        }

        public void Initialize()
        {
            _ = FetchDataAsync();
            _ = RssFeedService.Instance.InitializeAsync();
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
            if (rules.Any())
            {
                foreach (var rule in rules)
                {
                    try
                    {
                        //Debug.WriteLine($"Sending WebRequest for deleting rule `{rule.Title}`");
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
            set => this.RaiseAndSetIfChanged(ref _articleCount, value);
        }
    }
}