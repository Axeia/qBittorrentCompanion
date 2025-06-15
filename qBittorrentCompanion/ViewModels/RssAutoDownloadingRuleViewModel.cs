using Avalonia.Collections;
using Avalonia.Controls;
using DynamicData;
using Newtonsoft.Json.Linq;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Services;
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
    public partial class RssAutoDownloadingRuleViewModel : ViewModelBase
    {
        private List<EpisodeFilterToken> _tokens = [];
        public List<EpisodeFilterToken> Tokens
        {
            get => _tokens;
            set => this.RaiseAndSetIfChanged(ref _tokens, value);
        }

        private bool _showRssRuleWarnings = !Design.IsDesignMode && ConfigService.ShowRssRuleWarnings;

        public bool ShowRssRuleWarnings
        {
            get => _showRssRuleWarnings;
            set
            {
                if (_showRssRuleWarnings != value)
                {
                    _showRssRuleWarnings = value;
                    ConfigService.ShowRssRuleWarnings = value;
                    this.RaisePropertyChanged(nameof(ShowRssRuleWarnings));
                }
            }
        }

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
            if (!Design.IsDesignMode)
            {
                var testData = RssRuleTestDataService.GetEntry(Title);
                Rows.Add(testData.Select(t => CreateMatchTestRowViewModel(t)));
            }
            Rows.CollectionChanged += Rows_CollectionChanged;

            FilterRssArticles();

            RssFeeds.CollectionChanged += RssFeeds_CollectionChanged;
            Rows.Add(CreateMatchTestRowViewModel()); 

            CategoryService.Instance.CategoriesUpdated += Instance_CategoriesUpdated;
            Validate();
        }

        private void Rows_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            SaveRows();
        }

        private MatchTestRowViewModel CreateMatchTestRowViewModel(string content = "")
        {
            MatchTestRowViewModel mtrvm = new() { MatchTest = content};
            mtrvm.PropertyChanged += MatchTestRowViewModel_PropertyChanged;

            return mtrvm;
        }

        /// <summary>
        /// Ensures Rows always contains one empty row by either adding an empty row (or removing a secondary empty one)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MatchTestRowViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is string propertyName && propertyName.Equals(nameof(MatchTestRowViewModel.MatchTest)))
            {
                var lastEmptyRow = Rows.LastOrDefault(t => t.MatchTest == string.Empty);

                if (lastEmptyRow == null)
                    Rows.Add(CreateMatchTestRowViewModel(string.Empty));
                else
                    Rows.Remove(
                        Rows.Where(t => t.MatchTest == string.Empty && t != lastEmptyRow).ToList()
                    );

                SaveRows();
            }
        }

        private void SaveRows()
        {
            RssRuleTestDataService.SetValue(Title, Rows.Select(t=>t.MatchTest).ToList());
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

        private (int, int) _mustContainErrorIndexes = (0, 0);
        public (int, int) MustContainErrorIndexes
        {
            get => _mustContainErrorIndexes;
            set => this.RaiseAndSetIfChanged(ref _mustContainErrorIndexes, value);
        }

        private bool _mustContainErrored = false;
        public bool MustContainErrored
        {
            get => _mustContainErrored;
            set => this.RaiseAndSetIfChanged(ref _mustContainErrored, value);
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

        private (int, int) _mustNotContainErrorIndexes = (0, 0);
        public (int, int) MustNotContainErrorIndexes
        {
            get => _mustNotContainErrorIndexes;
            set => this.RaiseAndSetIfChanged(ref _mustNotContainErrorIndexes, value);
        }

        private bool _mustNotContainErrored = false;
        public bool MustNotContainErrored
        {
            get => _mustNotContainErrored;
            set => this.RaiseAndSetIfChanged(ref _mustNotContainErrored, value);
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


        private ObservableCollection<string> _errors = [];
        public ObservableCollection<string> Errors
        {
            get => Design.IsDesignMode ? ["Error error error" ] : _errors;
            private set => this.RaiseAndSetIfChanged(ref _errors, value);
        }

        private ObservableCollection<string> _warnings = [];
        public ObservableCollection<string> Warnings
        {
            get => Design.IsDesignMode ? ["Warning warning warning"] : _warnings;
            private set => this.RaiseAndSetIfChanged(ref _warnings, value);
        }


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
            {
                bool isMatch = false;

                if (Errors.Count == 0 &&
                    (MustContain != string.Empty || MustNotContain != string.Empty || EpisodeFilter != string.Empty))
                {
                    bool episodeFilterMatches = IsEpisodeMatch(article);
                    bool textFilterMatches = 
                        (MustContain == string.Empty && MustNotContain == string.Empty) 
                        || RssRuleIsMatchViewModel.IsTextMatch(article.Title, MustContain, MustNotContain, UseRegex);

                    isMatch = textFilterMatches && episodeFilterMatches;
                }

                article.IsMatch = isMatch;
            }

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

        private bool IsEpisodeMatch(RssArticleViewModel article)
        {
            if (EpisodeFilter == string.Empty) return true;

            if (article.Season != null 
                && article.Season == _season 
                && article.Episode is int articleEp)
            {
                return _episodes.Any(r => articleEp >= r.Item1 && articleEp <= r.Item2);
            }

            return false;
        }

        private void FilterTestData()
        {
            foreach (MatchTestRowViewModel row in Rows)
            {
                row.IsMatch =
                    Errors.Count <= 0
                    && RssRuleIsMatchViewModel.IsTextMatch(
                        row.MatchTest, MustContain, MustNotContain, UseRegex
                    );
                    //&& IsEpisodeMatch(r);
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
            Warnings.Clear();

            if (String.IsNullOrEmpty(MustContain) 
                && String.IsNullOrEmpty(MustNotContain) 
                && String.IsNullOrEmpty(EpisodeFilter))
            {
                Warnings.Add(
                    "A rule should have something to match, ensure at least one of these fields has a value: " +
                    "'Must contain', 'Must not contain' or 'Episode filter'"
                );
            }
            
            if (UseRegex == true)
            {
                // Using actions to assign values to trigger update behavior of properties
                ValidateRegex(MustContain, "Must contain", value => MustContainErrorIndexes = value, value => MustContainErrored = value);
                ValidateRegex(MustNotContain, "Must not contain", value => MustNotContainErrorIndexes = value, value => MustNotContainErrored = value);
            }
            else
            {
                ValidateWildcard(MustContain, nameof(MustContain));
                ValidateWildcard(MustNotContain, nameof(MustNotContain));
            }

            ValidateEpisodeFilter();
        }

        [GeneratedRegex(@"^(?:[0-9]{0,2}[1-9])x(?:(?:[0-9]{1,4}(?:-[0-9]{1,4}|-|);)+)$")]
        public static partial Regex ValidateEpisodeRegex();

        private void ValidateEpisodeFilter()
        {
            if (EpisodeFilter != "")
            {
                var validateEpisodeRegex = ValidateEpisodeRegex();
                var match = validateEpisodeRegex.Match(EpisodeFilter);
                EpisodeFilterErrored = !match.Success;
                if (EpisodeFilterErrored)
                {
                    if (Tokens.FirstOrDefault(epft => !epft.IsValid) is EpisodeFilterToken epft)
                        Errors.Add("Episode filter: " + epft.ErrorMessage!);
                    else
                        Errors.Add("Episode filter isn't valid, perhaps it's incomplete?");
                }
            }
            else
                EpisodeFilterErrored = false;
        }

        private void ValidateRegex(string regexText, string fieldName, Action<(int Start, int End)> setErrorIndexes, Action<bool> setIsErrored)
        {
            try
            {
                Regex regex = new(regexText);
                setErrorIndexes((0, 0)); // Clear error indexes via the Action
                setIsErrored(false); // Set isErrored via the Action
            }
            catch (RegexParseException e)
            {
                setIsErrored(true); // Set isErrored via the Action
                string message = $"{fieldName} {e.Message.Replace($"'{regexText}' ", "")}";
                Errors.Add(message);

                var offsetRegex = _offsetRegex();
                var match = offsetRegex.Match(message);
                if (match.Success)
                {
                    int end = int.Parse(match.Groups["index"].Value);
                    int start = end - 1;
                    setErrorIndexes((start, end)); // Set error indexes via the Action
                }
                else
                {
                    setErrorIndexes((0, 0)); // Clear error indexes via the Action
                }
            }
        }

        private void ValidateMustContainRegex()
        {
            ValidateRegex(MustContain, "Must contain", value => MustContainErrorIndexes = value, value => MustContainErrored = value);
        }

        private void ValidateMustNotContainRegex()
        {
            ValidateRegex(MustNotContain, "Must not contain", value => MustNotContainErrorIndexes = value, value => MustNotContainErrored = value);
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

        private bool _episodeFilterErrored = false;
        public bool EpisodeFilterErrored
        {
            get => _episodeFilterErrored;
            set => this.RaiseAndSetIfChanged(ref _episodeFilterErrored, value);
        }

        /// <inheritdoc cref="RssAutoDownloadingRule.EpisodeFilter"/>
        public string EpisodeFilter
        {
            get => _rule.EpisodeFilter;
            set
            {
                if (value != _rule.EpisodeFilter)
                {
                    _rule.EpisodeFilter = value;
                    this.RaisePropertyChanged(nameof(EpisodeFilter));
                    _tokens = EpisodeFilterTokenizer.Tokenize(value);
                    ReCalculateSeasonAndEpisodes();
                    ValidateAndFilter();
                }
            }
        }

        private int? _season = null;
        private List<(int, int)> _episodes = [];
        private void ReCalculateSeasonAndEpisodes()
        {
            _episodes.Clear();
            for (int i = 0; i < _tokens.Count; i++)
            {
                var token = _tokens[i];
                if (token.Type == EpisodeFilterTokenType.SeasonNumber && token.ErrorMessage == null)
                    _season = int.Parse(token.Value);
                else if (token.Type == EpisodeFilterTokenType.EpisodeNumber && token.ErrorMessage == null)
                {
                    int curInt = int.Parse(token.Value);
                    if (i+1 < _tokens.Count)
                    {
                        EpisodeFilterToken nextToken = _tokens[i+1];
                        EpisodeFilterToken? nextNextToken = i+2 < _tokens.Count ? _tokens[i+2] : null;
                        if (nextToken.Type == EpisodeFilterTokenType.RangeSeparator)
                        {
                            if (nextNextToken != null && nextNextToken.Type == EpisodeFilterTokenType.EpisodeNumber && nextNextToken.IsValid)
                            {
                                i++;
                                _episodes.Add((curInt, int.Parse(nextNextToken.Value)));
                            }
                            else
                                _episodes.Add((curInt, 9999));

                            i++;
                        }
                        else
                            _episodes.Add((curInt, curInt));
                    }
                    else
                        _episodes.Add((curInt, curInt));
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

        [GeneratedRegex("offset (?<index>\\d+)")]
        private static partial Regex _offsetRegex();
    }
}
