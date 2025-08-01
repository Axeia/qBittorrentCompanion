﻿using AutoPropertyChangedGenerator;
using Avalonia.Controls;
using Avalonia.Threading;
using Newtonsoft.Json.Linq;
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
    public partial class RssAutoDownloadingRulesViewModel : RssPluginSupportBaseViewModel
    {
        protected DispatcherTimer _refreshTimer = new();

        private bool _showExpandedControls = !Design.IsDesignMode && ConfigService.ShowRssExpandedControls;

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

        private int _rssRuleArticleDetailSelectedTabIndex = Design.IsDesignMode 
            ? 1 
            : ConfigService.RssRuleArticleDetailSelectedTabIndex;
        public int RssRuleArticleDetailSelectedTabIndex
        {
            get => _rssRuleArticleDetailSelectedTabIndex;
            set
            {
                if (_rssRuleArticleDetailSelectedTabIndex != value)
                {
                    _rssRuleArticleDetailSelectedTabIndex = value;
                    ConfigService.RssRuleArticleDetailSelectedTabIndex = value;
                    this.RaisePropertyChanged(nameof(RssRuleArticleDetailSelectedTabIndex));
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

        [AutoPropertyChanged]
        private ObservableCollection<RssAutoDownloadingRuleViewModel> _rssRules = [];

        private RssAutoDownloadingRuleViewModel? _selectedRssRule;

        public RssAutoDownloadingRuleViewModel? SelectedRssRule
        {
            get => _selectedRssRule;
            set
            {
                if (value != _selectedRssRule)
                {
                    // Stop observing the old value so it can be garbage collected
                    if (_selectedRssRule != null)
                    {
                        ActiveRssRule.Renamed -= RuleRenamed;
                        ActiveRssRule.Saved -= RuleSaved;
                    }

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
                    {
                        ActiveRssRule = GetNewRssRule();
                    }

                    // At this point the ActiveRssRule is either a new entry or a copy of an existing one

                    // Sets the name of the copy on the original as its now saved to reflect the server state.
                    ActiveRssRule.Renamed += RuleRenamed;
                    // Updates the properties of the copy on the original as its now saved to reflect the server
                    // (important saved comes after renamed as it uses the new name to find the entry)
                    ActiveRssRule.Saved += RuleSaved;

                    this.RaisePropertyChanged(nameof(SelectedRssRule));
                }
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="renamedFrom"></param>
        /// <param name="renamedTo"></param>
        private void RuleRenamed(string renamedFrom, string renamedTo)
        {
            RssRules.First(r=>r.Title == renamedFrom).Title = renamedTo;
        }

        private void RuleSaved(RssAutoDownloadingRuleViewModel copy)
        {
            var original = RssRules.First(r => r.Title == copy.Title);
            original.LoadUpdatedRule(copy.Rule);
            original.SelectedContentLayoutItem = copy.SelectedContentLayoutItem;
            original.Tags = copy.Tags;
        }

        [AutoPropertyChanged]
        private List<RssAutoDownloadingRuleViewModel> _selectedRssRules = [];

        private RssAutoDownloadingRuleViewModel _activeRssRule;
        public RssAutoDownloadingRuleViewModel ActiveRssRule
        {
            get => _activeRssRule;
            set
            {
                if (value != _activeRssRule)
                {
                    ActiveRssRule.PropertyChanged -= ActiveRssRule_PropertyChanged;
                    if (ActiveRssRule.DataGridCollectionViewProperty != null)
                        ActiveRssRule.DataGridCollectionViewProperty!.CurrentChanged -= DataGridCollectionView_CurrentChanged;
                    else
                        Debug.WriteLine("Not supposed to happen");

                    _activeRssRule = value;

                    this.RaisePropertyChanged(nameof(ActiveRssRule));
                    ActiveRssRule.PropertyChanged += ActiveRssRule_PropertyChanged;
                    if (ActiveRssRule.DataGridCollectionViewProperty != null)
                        ActiveRssRule.DataGridCollectionViewProperty!.CurrentChanged += DataGridCollectionView_CurrentChanged;
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

            if (e.PropertyName == nameof(RssAutoDownloadingRuleViewModel.DataGridCollectionViewProperty)
                && ActiveRssRule.DataGridCollectionViewProperty != null)
            {
                ActiveRssRule.DataGridCollectionViewProperty.CurrentChanged += DataGridCollectionView_CurrentChanged;
            }
        }

        private void DataGridCollectionView_CurrentChanged(object? sender, EventArgs e)
        {
            //Debug.WriteLine($"RulesViewModel DataGridCollectionView_CurrentChanged called");
            if (ActiveRssRule.DataGridCollectionViewProperty != null 
                && ActiveRssRule.DataGridCollectionViewProperty.CurrentItem is RssArticleViewModel selectedRssArticleViewModel)
            {
                PluginInput = selectedRssArticleViewModel.Title;
            }
        }

        public RssAutoDownloadingRuleViewModel GetNewRssRule(IReadOnlyList<Uri>? affectedFeeds = null)
        {
            affectedFeeds = affectedFeeds ?? [];

            return new RssAutoDownloadingRuleViewModel(
                new RssAutoDownloadingRule() { AffectedFeeds = affectedFeeds, UseRegex = true },
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

        [AutoPropertyChanged]
        private ObservableCollection<RssFeedViewModel> _rssFeeds = [];

        protected async Task FetchDataAsync()
        {
            // Clear existing data
            RssFeeds.Clear();

            // Start calls needs to be executed before calling RefreshRulesAsync
            var rssFolderTask = QBittorrentService.GetRssItemsAsync(true);
            var categoriesTask = CategoryService.Instance.InitializeAsync();
            await Task.WhenAll(rssFolderTask, categoriesTask);

            // Get rules (needs data from RssFeeds and Categories)
            await RefreshRulesAsync();
        }

        public async Task RefreshRulesAsync()
        {
            RssRules.Clear();
            IReadOnlyDictionary<string, RssAutoDownloadingRule>? rules = 
                await QBittorrentService.GetRssAutoDownloadingRulesAsync();

            if (rules != null)
            {
                foreach (KeyValuePair<string, RssAutoDownloadingRule> rule in rules)
                {
                    RssRules.Add(new RssAutoDownloadingRuleViewModel(rule.Value, rule.Key));
                }
            }
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
            await QBittorrentService.SetRssAutoDownloadingRuleAsync(name, newRule ?? new RssAutoDownloadingRule());
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
                        await QBittorrentService.DeleteRssAutoDownloadingRuleAsync(rule.Title);
                    }
                    catch (Exception e) { Debug.WriteLine(e.Message); }
                }

                await RefreshRulesAsync();
            }
        }

        [AutoPropertyChanged]
        private int _articleCount = 0;
    }
}