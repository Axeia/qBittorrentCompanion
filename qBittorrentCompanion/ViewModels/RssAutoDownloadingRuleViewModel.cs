using Avalonia.Collections;
using Newtonsoft.Json.Linq;
using QBittorrent.Client;
using qBittorrentCompanion.Services;
using qBittorrentCompanion.Validators;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace qBittorrentCompanion.ViewModels
{
    public class RssAutoDownloadingRuleViewModel : ViewModelBase
    {
        public ObservableCollection<string> Tags => 
            TagService.Instance.Tags;

        private ObservableCollection<string> _selectedTags = [];
        public ObservableCollection<string> SelectedTags
        {
            get => _selectedTags;
            set => this.RaiseAndSetIfChanged(ref _selectedTags, value);
        }

        public ObservableCollection<RssFeedViewModel> RssFeeds =>
            RssFeedService.Instance.RssFeeds;

        private string _warning = "";
        public string Warning
        {
            get => _warning;
            set => this.RaiseAndSetIfChanged(ref _warning, value);
        }

        /// <summary>
        /// Is a new rule rather than an existing one
        /// </summary>
        private bool _isNew = false;
        public bool IsNew
        {
            get => _isNew;
            set => this.RaiseAndSetIfChanged(ref _isNew, value);
        }

        public string OldTitle = "";

        private bool _isSaving = false;
        public bool IsSaving
        {
            get => _isSaving;
            set => this.RaiseAndSetIfChanged(ref _isSaving, value);
        }

        private ObservableCollection<MatchTestRowViewModel> _rows = [];
        public ObservableCollection<MatchTestRowViewModel> Rows
        {
            get => _rows;
            set => this.RaiseAndSetIfChanged(ref _rows, value);
        }

        private DataGridCollectionView? _dataGridCollectionView;
        public DataGridCollectionView? DataGridCollectionView
        {
            get => _dataGridCollectionView;
            set => this.RaiseAndSetIfChanged(ref _dataGridCollectionView, value);
        }

        /// <summary>
        /// Adds an empty option in addition to <see cref="CategoryService.Instance.Categories"/> 
        /// allowing for an empty selection to be made.
        /// </summary>
        public IEnumerable<Category> CompositeCategories
        {
            get
            {
                yield return new Category { Name = "" };
                foreach(var category in CategoryService.Instance.Categories)
                    yield return category;
            }
        }

        /// <summary>
        /// Adds all <see cref="AffectedFeeds"/> to <see cref="SelectedFeeds"/>. <br/>
        /// Because of the type difference (<c>string</c> vs <c>SimplifiedRssFeed</c>) 
        /// <see cref="RssFeed"/> is used as a lookup table, <br/> ultimately the selection is on these RssFeeds anyhow.
        /// </summary>
        public void UpdateSelectedFeeds()
        {
            var selectedFeeds = new ObservableCollection<RssFeedViewModel>(
                RssFeedService.Instance.RssFeeds.Where(r => _rule.AffectedFeeds.Contains(r.Url)).ToList()
            );

            // Use Clear and Add instead of reassigning to maintain references
            SelectedFeeds.Clear();
            foreach (var feed in selectedFeeds)
                SelectedFeeds.Add(feed);

            this.RaisePropertyChanged(nameof(SelectedFeeds));
            UpdateSelectedFeedsRelatedProperties();

            FilterRssArticles();
        }

        private List<RssArticleViewModel> _rssArticles = [];
        public List<RssArticleViewModel> RssArticles
        {
            get => _rssArticles;
            set
            {
                if (value != _rssArticles)
                {
                    _rssArticles = value;
                    this.RaisePropertyChanged(nameof(RssArticles));
                    FilterRssArticles();
                }
            }
        }

        private RssAutoDownloadingRule _rule;

        private string _title = "";
        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        public ReactiveCommand<string, Unit> RenameCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearDownloadedEpisodesCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }

        public RssAutoDownloadingRuleViewModel(RssAutoDownloadingRule rule, string title)
        {
            _rule = rule;
            _title = title;
            _selectedFeeds.CollectionChanged += SelectedFeeds_CollectionChanged;
            UpdateSelectedFeeds();

            RenameCommand = ReactiveCommand.CreateFromTask<string>(RenameAsync);
            ClearDownloadedEpisodesCommand = ReactiveCommand.CreateFromTask(ClearDownloadedEpisodesAsync);
            SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);

            // Set testdata
            var testData = RssRuleTestDataService.GetEntry(Title);
            if (testData != null)
            {
                foreach (var testCase in testData)
                {
                    Rows.Add(new MatchTestRowViewModel() { MatchTest = testCase });
                }
            }
            //if (Rows.Count > 0 && Rows.Last().MatchTest != string.Empty)
            Rows.Add(new MatchTestRowViewModel());

            FilterRssArticles();

            RssFeeds.CollectionChanged += RssFeeds_CollectionChanged;
            CategoryService.Instance.CategoriesUpdated += Instance_CategoriesUpdated;
        }

        private void Instance_CategoriesUpdated(object? sender, EventArgs e)
        {
            this.RaisePropertyChanged(nameof(CompositeCategories));
        }

        //Reselect
        private void RssFeeds_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateSelectedFeeds();
        }

        private async Task SaveAsync()
        {
            // Got to rename first to prevent duplicating the entry
            if (OldTitle != Title)
                await RenameAsync(OldTitle);

            try
            {
                IsSaving = true;
                // Ensure AffectedFeeds are set correctly 
                AffectedFeeds = SelectedFeeds.Select(f => f.Url).ToList().AsReadOnly();
                await QBittorrentService.QBittorrentClient.SetRssAutoDownloadingRuleAsync(Title, _rule);
                IsSaving = false;
                // Will trigger RssAutoDownloadingRulesViewModel to update its Rules collection
                IsNew = false;
                Warning = "";
            }
            catch (Exception e) { Debug.WriteLine(e.Message); }
            finally { IsSaving = false; }
        }

        public async Task RenameAsync(string oldTitle)
        {
            try
            {
                IsSaving = true;
                await QBittorrentService.QBittorrentClient.RenameRssAutoDownloadingRuleAsync(oldTitle, Title);
            }
            catch (Exception e)
            {
                _title = oldTitle;
                this.RaisePropertyChanged(nameof(Title));

                Debug.WriteLine($"Rename of {this.GetType} failed, restored Title: {oldTitle}");
                Debug.WriteLine(e.Message);
            }
            finally
            {
                IsSaving = false;
            }
        }


        /// <inheritdoc cref="RssAutoDownloadingRule.Enabled"/>
        public bool Enabled
        {
            get => _rule.Enabled;
            set 
            {
                if (value != _rule.Enabled)
                {
                    _rule.Enabled = value;
                    this.RaisePropertyChanged(nameof(Enabled));
                }
            }
        }

        /// <inheritdoc cref="RssAutoDownloadingRule.MustContain"/>
        public string MustContain
        {
            get => _rule.MustContain;
            set
            {
                if (value != _rule.MustContain)
                {
                    _rule.MustContain = value;
                    this.RaisePropertyChanged(nameof(MustContain));
                    ValidateAndFilter();
                }
            }
        }

        /// <inheritdoc cref="RssAutoDownloadingRule.MustNotContain"/>
        public string MustNotContain
        {
            get => _rule.MustNotContain;
            set
            {
                if (value != _rule.MustNotContain)
                {
                    _rule.MustNotContain = value;
                    this.RaisePropertyChanged(nameof(MustNotContain));
                    ValidateAndFilter();
                }
            }
        }

        /// <inheritdoc cref="RssAutoDownloadingRule.UseRegex"/>
        public bool UseRegex
        {
            get => _rule.UseRegex;
            set
            {
                if (value != _rule.UseRegex)
                {
                    _rule.UseRegex = value;
                    this.RaisePropertyChanged(nameof(UseRegex));
                    ValidateAndFilter();
                }
            }
        }

        public ObservableCollection<string> Errors => [];

        public bool HasErrors => Errors.Any();

        private Regex? _mustContainRegex = null;
        private Regex? _mustNotContainRegex = null;

        /// <summary>
        /// A very simple method that ensures Validate is called before Filter.
        /// This is because validate will set _mustContainRegex and _mustNotContainRegex.
        /// If they're not valid it's set to null, if it's valid it's a regex to be used as a filter.
        /// </summary>
        private void ValidateAndFilter()
        {
            Validate();
            Filter();
        }

        public void Filter()
        {
            FilterRssArticles();
            FilterTestData();
        }

        private void FilterRssArticles()
        {
            // Filter RssArticles
            foreach (var article in RssArticles)
                article.IsMatch = Errors.Count <= 0 && RssRuleIsMatchViewModel.IsTextMatch(
                    article.Title, MustContain, MustNotContain, EpisodeFilter, UseRegex
                );

            DataGridCollectionView = new DataGridCollectionView(RssArticles);
            var dgsdIsMatch = DataGridSortDescription.FromPath(nameof(RssArticleViewModel.IsMatch), ListSortDirection.Descending);
            var dgsdDate = DataGridSortDescription.FromPath(nameof(RssArticleViewModel.Date), ListSortDirection.Ascending);

            // Clearing and re-adding sort descriptions to notify the data grid
            DataGridCollectionView.SortDescriptions.Clear();
            DataGridCollectionView.SortDescriptions.Add(dgsdIsMatch);
            DataGridCollectionView.SortDescriptions.Add(dgsdDate);
            DataGridCollectionView.MoveCurrentToFirst();

            this.RaisePropertyChanged(nameof(DataGridCollectionView));
            FilteredArticleCount = RssArticles.Count(a => a.IsMatch);
        }

        

        private void FilterTestData()
        {
            foreach (MatchTestRowViewModel row in Rows)
            {
                row.IsMatch = Errors.Count <= 0 && RssRuleIsMatchViewModel.IsTextMatch(
                    row.MatchTest, MustContain, MustNotContain, EpisodeFilter, UseRegex
                );
            }

            FilteredTestDataCount = Rows.Count(r=>r.IsMatch);
        }

        /// <summary>
        /// Validates the interdependent fields where at least one of the following must be set:
        /// <list type="bullet">
        /// <item><see cref="MustContain"/></item>
        /// <item><see cref="MustNotContain"/></item>
        /// <item><see cref="EpisodeFilter"/></item>
        /// </list>
        /// Additionally, the validation logic for <see cref="MustContain"/> and <see cref="MustNotContain"/>
        /// depends on whether <see cref="UseRegex"/> is true, invoking either <see cref="ValidateRegex"/>
        /// or <see cref="ValidateWildcard"/> accordingly.
        /// </summary>
        private void Validate()
        {
            Errors.Clear();

            if (String.IsNullOrEmpty(MustContain) 
                && String.IsNullOrEmpty(MustNotContain) 
                && String.IsNullOrEmpty(EpisodeFilter))
            {
                Errors.Add("A rule should have something to match, ensure at least one of these fields has a value: 'Must contain', 'Must not contain' or 'Episode filter'");
            }
            
            if (UseRegex == true)
            {
                ValidateRegex(MustContain, nameof(MustContain));
                ValidateRegex(MustNotContain, nameof(MustNotContain));
            }
            else
            {
                ValidateWildcard(MustContain, nameof(MustContain));
                ValidateWildcard(MustNotContain, nameof(MustNotContain));
            }

            Debug.WriteLine(Errors.Count());
        }

        private void ValidateRegex(string value, string propertyName)
        {
            Regex? regex = null;
            try
            {
                regex = new Regex(value);
            }
            catch (RegexParseException e)
            {
                Errors.Add(e.Message.Replace($"'{value}' ", ""));
            }
            finally
            {
                SetRegex(regex, propertyName);
            }
        }

        private void ValidateWildcard(string value, string propertyName)
        {
            Regex? regex = null;
            try
            {
                regex = new Regex(RssRuleIsMatchViewModel.WildCardToRegular(value));
            }
            catch (RegexParseException)
            {
                Errors.Add("Not a valid wildcard (or the live preview just can't be trusted)");
            }
            finally
            {
                SetRegex(regex, propertyName);
            }
        }

        /// <summary>
        /// Sets the _mustContain/_mustNotContain regex so it can be used to filter the articles.
        /// </summary>
        /// <param name="regex"></param>
        /// <param name="propertyName"></param>
        private void SetRegex(Regex? regex, string propertyName)
        {
            if (propertyName == nameof(MustContain))
            {
                _mustContainRegex = regex;
            }
            else if (propertyName == nameof(MustNotContain))
            { 
                _mustNotContainRegex = regex;
            }
        }

        /// <inheritdoc cref="RssAutoDownloadingRule.EpisodeFilter"/>
        [ValidEpisodeFilter]
        public string EpisodeFilter
        {
            get => _rule.EpisodeFilter;
            set
            {
                if (value != _rule.EpisodeFilter)
                {
                    _rule.EpisodeFilter = value;
                    this.RaisePropertyChanged(nameof(EpisodeFilter));
                    ValidateAndFilter();
                }
            }
        }

        /// <inheritdoc cref="RssAutoDownloadingRule.SmartFilter"/>
        public bool SmartFilter
        {
            get => _rule.SmartFilter;
            set
            {
                if (value != _rule.SmartFilter)
                {
                    _rule.SmartFilter = value;
                    this.RaisePropertyChanged(nameof(SmartFilter));
                }
            }
        }

        /// <inheritdoc cref="RssAutoDownloadingRule.PreviouslyMatchedEpisodes"/>
        public IReadOnlyList<string> PreviouslyMatchedEpisodes
        {
            get => _rule.PreviouslyMatchedEpisodes;
            set
            {
                if (value != _rule.PreviouslyMatchedEpisodes)
                {
                    _rule.PreviouslyMatchedEpisodes = value;
                    this.RaisePropertyChanged(nameof(PreviouslyMatchedEpisodes));
                }
            }
        }

        /// <inheritdoc cref="RssAutoDownloadingRule.AffectedFeeds"/>
        /// Only useful for initially setting SelectedFeeds and when saving the value
        public IReadOnlyList<Uri> AffectedFeeds
        {
            get => _rule.AffectedFeeds;
            set
            {
                if (value != _rule.AffectedFeeds)
                {
                    _rule.AffectedFeeds = value;
                    this.RaisePropertyChanged(nameof(AffectedFeeds));
                }
            }
        }

        private ObservableCollection<RssFeedViewModel> _selectedFeeds = [];
        public ObservableCollection<RssFeedViewModel> SelectedFeeds
        {
            get => _selectedFeeds;
            set
            {
                _selectedFeeds.CollectionChanged -= SelectedFeeds_CollectionChanged;

                if(value != _selectedFeeds)
                {
                    _selectedFeeds = value;
                    this.RaisePropertyChanged(nameof(SelectedFeeds));

                    if(value != null)
                    {
                        UpdateSelectedFeedsRelatedProperties();
                        _selectedFeeds.CollectionChanged += SelectedFeeds_CollectionChanged;
                    }


                    // (Re?)apply filter
                    Filter();
                }
            }
        }

        private void SelectedFeeds_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateSelectedFeedsRelatedProperties();
        }

        private void UpdateSelectedFeedsRelatedProperties()
        {
            // Change affected
            AffectedFeeds = SelectedFeeds
                .Select(s => s.Url)
                .ToList().AsReadOnly();

            // Change articles (will trigger update to FilterRssArticles)
            RssArticles = _selectedFeeds.SelectMany(f => f.Articles)
                .Select(article => new RssArticleViewModel(article))
                .ToList();

            this.RaisePropertyChanged(nameof(HasSelectedFeeds));
            this.RaisePropertyChanged(nameof(ArticleCount));
        }

        public bool HasSelectedFeeds =>
            SelectedFeeds.Count > 0;
        public int ArticleCount =>
            RssArticles.Count;

        /// <inheritdoc cref="RssAutoDownloadingRule.IgnoreDays"/>
        public int IgnoreDays
        {
            get => _rule.IgnoreDays;
            set
            {
                if (value != _rule.IgnoreDays)
                {
                    _rule.IgnoreDays = value;
                    this.RaisePropertyChanged(nameof(IgnoreDays));
                }
            }
        }

        /// <inheritdoc cref="RssAutoDownloadingRule.LastMatch"/>
        public DateTimeOffset? LastMatch
        {
            get => _rule.LastMatch;
            set
            {
                if (value != _rule.LastMatch)
                {
                    _rule.LastMatch = value;
                    this.RaisePropertyChanged(nameof(LastMatch));
                }
            }
        }

        /// <inheritdoc cref="RssAutoDownloadingRule.AddPaused"/>
        public bool AddPaused
        {
            get => _rule.AddPaused ?? false;
            set
            {
                if (value != _rule.AddPaused)
                {
                    _rule.AddPaused = value;
                    this.RaisePropertyChanged(nameof(AddPaused));
                }
            }
        }

        /// <inheritdoc cref="RssAutoDownloadingRule.AssignedCategory"/>
        public string AssignedCategory
        {
            get => _rule.AssignedCategory;
            set
            {
                if (value != _rule.AssignedCategory)
                {
                    _rule.AssignedCategory = value;
                    this.RaisePropertyChanged(nameof(AssignedCategory));
                }
            }
        }

        /// <inheritdoc cref="RssAutoDownloadingRule.SavePath"/>
        public string SavePath
        {
            get => _rule.SavePath;
            set
            {
                if (value != _rule.SavePath)
                {
                    _rule.SavePath = value;
                    this.RaisePropertyChanged(nameof(SavePath));
                }
            }
        }

        /// <inheritdoc cref="RssAutoDownloadingRule.AdditionalData"/>
        public IDictionary<string, JToken> AdditionalData
        {
            get => _rule.AdditionalData;
            set
            {
                if (value != _rule.AdditionalData)
                {
                    _rule.AdditionalData = value;
                    this.RaisePropertyChanged(nameof(AdditionalData));
                }
            }
        }

        private int _filteredArticleCount = 0;
        public int FilteredArticleCount
        {
            get => _filteredArticleCount;
            set => this.RaiseAndSetIfChanged(ref _filteredArticleCount, value);
        }

        private int _filteredTestDataCount = 0;
        public int FilteredTestDataCount
        {
            get => _filteredTestDataCount;
            set => this.RaiseAndSetIfChanged(ref _filteredTestDataCount, value);
        }

        private async Task ClearDownloadedEpisodesAsync()
        {
            try
            {
                await QBittorrentService.QBittorrentClient.SetRssAutoDownloadingRuleAsync(Title, _rule);
            }
            catch (Exception e) { Debug.WriteLine(e.Message); }
        }

        public RssAutoDownloadingRuleViewModel GetCopy()
        {
            return new RssAutoDownloadingRuleViewModel(new RssAutoDownloadingRule{
                    Enabled = this.Enabled,
                    MustContain = this.MustContain,
                    MustNotContain = this.MustNotContain,
                    UseRegex = this.UseRegex,
                    EpisodeFilter = this.EpisodeFilter,
                    SmartFilter = this.SmartFilter,
                    PreviouslyMatchedEpisodes = this.PreviouslyMatchedEpisodes,
                    AffectedFeeds = this.AffectedFeeds,
                    IgnoreDays = this.IgnoreDays,
                    LastMatch = this.LastMatch,
                    AddPaused = this.AddPaused,
                    AssignedCategory = this.AssignedCategory,
                    SavePath = this.SavePath,
                },
                Title
            )
            { IsNew = this.IsNew, Warning = this.Warning };
        }
    }
}
