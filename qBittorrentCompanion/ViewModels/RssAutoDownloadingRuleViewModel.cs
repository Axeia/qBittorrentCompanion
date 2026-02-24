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
using RaiseChangeGenerator;

namespace qBittorrentCompanion.ViewModels
{
    public partial class RuleTag(string tag, bool isRegularTag = true, bool isSelected = false) : ReactiveObject
    {
        [RaiseChange]
        private string _tag = tag;
        [RaiseChange]
        private bool _isRegularTag = isRegularTag;
        [RaiseChange]
        private bool _isSelected = isSelected;
    }

    /// <summary>
    /// When adding properties to this class pay attention to the <see href="GetCopy"> method, it should copy the value over
    /// 
    /// The copy is used to display to the user so they can edit it without directly altering the original. 
    /// Doing it this way changes don't persist when changing the focus between rules (unless saved)
    /// </summary>
    public partial class RssAutoDownloadingRuleViewModel : ViewModelBase
    {
        public event Action<RssAutoDownloadingRuleViewModel>? Saved;
        public event Action<string, string>? Renamed;
        public static IReadOnlyList<KeyValuePair<string, string?>> TorrentContentLayoutOptions { get; } =
        [
            new(Resources.Resources.TorrentContentLayout_Global, null),
            new(Resources.Resources.TorrentContentLayout_Original, "Original"),
            new(Resources.Resources.TorrentContentLayout_CreateSubFolder, "Subfolder"),
            new(Resources.Resources.TorrentContentLayout_DontCreateSubFolder, "NoSubfolder")
        ];

        public void LoadUpdatedRule(RssAutoDownloadingRule rule)
        {
            _rule = rule;
            _preselectedTags.Clear();
            _preselectedTags.Add(LoadTorrentParams());
        }
        public RssAutoDownloadingRule Rule => _rule;

        [RaiseChange]
        private KeyValuePair<string, string?> _selectedContentLayoutItem =
                TorrentContentLayoutOptions[0];

        [RaiseChange]
        private List<EpisodeFilterToken> _tokens = [];

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

        [RaiseChange]
        public ObservableCollection<RuleTag> _tags = [];

        public ObservableCollection<RssFeedViewModel> RssFeeds =>
            RssFeedService.Instance.RssFeeds;

        [RaiseChange]
        private string _pendingTag = "";
        [RaiseChange]
        private string _regularTag = "";
        [RaiseChange]
        private string _warning = "";
        /// <summary>
        /// Is a new rule rather than an existing one
        /// </summary>
        [RaiseChange]
        private bool _isNew = false;
        [RaiseChange]
        private bool _isSaving = false;
        [RaiseChange]
        private ObservableCollection<MatchTestRowViewModel> _rows = [];
        [RaiseChange]
        private DataGridCollectionView? _dataGridCollectionViewProperty;

        public string OldTitle = "";

        /// <summary>
        /// Adds an empty option in addition to <see cref="CategoryService.Instance.Categories"/> 
        /// allowing for an empty selection to be made.
        /// </summary>
        private Collection<Category> _compositeCategories = [];
        public IEnumerable<Category> CompositeCategories => _compositeCategories;

        private void UpdateCompositeCategories()
        {
            _compositeCategories = [new() { Name = "" }];
            _compositeCategories.Add(CategoryService.Instance.Categories);
        }

        private Category? _selectedCategory = null;
        /// <summary>
        /// A proxy for the UI that should either be null or hold one of the values from <see cref="CompositeCategories"/>, 
        /// a proxy because <see cref="AssignedCategory"/> is the actual value for qBittorrent
        /// </summary>
        public Category? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedCategory, value);
                this.AssignedCategory = value == null ? "" : value.Name;
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

        [RaiseChangeProxy(nameof(RssAutoDownloadingRule.SmartFilter))]
        [RaiseChangeProxy(nameof(RssAutoDownloadingRule.PreviouslyMatchedEpisodes))]
        [RaiseChangeProxy(nameof(RssAutoDownloadingRule.Enabled))]
        [RaiseChangeProxy(nameof(RssAutoDownloadingRule.AffectedFeeds))]
        /// Only useful for initially setting SelectedFeeds and when saving the value
        [RaiseChangeProxy(nameof(RssAutoDownloadingRule.IgnoreDays))]
        [RaiseChangeProxy(nameof(RssAutoDownloadingRule.LastMatch))]
        [RaiseChangeProxy(nameof(RssAutoDownloadingRule.AssignedCategory))]
        [RaiseChangeProxy(nameof(RssAutoDownloadingRule.SavePath))]
        [RaiseChangeProxy(nameof(RssAutoDownloadingRule.AdditionalData))]
        private RssAutoDownloadingRule _rule;

        [RaiseChange]
        private string _title = "";

        public ReactiveCommand<string, Unit> RenameCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearDownloadedEpisodesCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        private readonly List<string> _preselectedTags = [];
        public ReactiveCommand<Unit, Unit> AddPendingTagCommand { get; }
        public ReactiveCommand<Unit, Unit> AddRegularTagCommand { get; }
        public ReactiveCommand<RuleTag, Unit> DeleteRegularTagCommand { get; }

        public RssAutoDownloadingRuleViewModel(RssAutoDownloadingRule rule, string title)
        {
            _rule = rule;
            _title = title;

            List<string> tags = LoadTorrentParams();
            //Debug.WriteLine($"Rule: {rule.Key}, Tags: {string.Join(", ", tags)}");

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

            _preselectedTags.Add(tags);
            UpdateTags();
            Tags.CollectionChanged += Tags_CollectionChanged;
            TagService.Instance.Tags.CollectionChanged += Tags_CollectionChanged;

            AddPendingTagCommand = ReactiveCommand.Create(AddPendingTag);
            AddRegularTagCommand = ReactiveCommand.Create(AddRegularTag);
            DeleteRegularTagCommand = ReactiveCommand.CreateFromTask<RuleTag, Unit>(DeleteRegularTag);

            UpdateCompositeCategories();
            SelectedCategory = CompositeCategories.FirstOrDefault(c => c.Name.Equals(AssignedCategory));
        }

        private List<string> LoadTorrentParams()
        {
            List<string> tags = [];

            if (_rule.AdditionalData is IDictionary<string, JToken> dic
                && dic.TryGetValue("torrentParams", out var torrentParamsToken)
                && torrentParamsToken is JObject torrentParams)
            {
                if (torrentParams.TryGetValue("tags", out var tagsToken))
                    tags = tagsToken.ToObject<List<string>>()!;

                if (torrentParams.TryGetValue("content_layout", out var contentLayoutToken))
                {
                    var contentLayoutValue = contentLayoutToken.Type == JTokenType.Null
                        ? null
                        : contentLayoutToken.ToString();

                    Debug.WriteLine(contentLayoutValue);

                    SelectedContentLayoutItem = TorrentContentLayoutOptions
                        .First(kv => kv.Value == contentLayoutValue);
                }
            }

            return tags;
        }

        private async Task<Unit> DeleteRegularTag(RuleTag tag)
        {
            try
            {
                await QBittorrentService.DeleteTagAsync(tag.Tag);
                await TagService.Instance.UpdateTagsAsync();
                Tags.Remove(tag);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return Unit.Default;
        }

        private async void AddRegularTag()
        {
            if (Tags.FirstOrDefault(t => t.IsRegularTag == true && t.Tag == RegularTag) is RuleTag ruleTag)
            {
                ruleTag.IsSelected = true;
            }
            else
            {
                await QBittorrentService.CreateTagAsync(RegularTag);
                await TagService.Instance.UpdateTagsAsync();
            }
            RegularTag = "";
        }

        private void AddPendingTag()
        {
            if (Tags.FirstOrDefault(t => t.IsRegularTag == false && t.Tag == PendingTag) is RuleTag ruleTag)
            {
                ruleTag.IsSelected = true;
            }
            else
            {
                Tags.Add(new RuleTag(PendingTag, false, true));
            }
            PendingTag = "";
        }

        private void Tags_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateTags();
        }

        private void UpdateTags()
        {
            // Add known tags
            Tags.AddRange(
                TagService.Instance.Tags
                    .Where(tag => !Tags.Any(t => t.Tag == tag))
                    .Select(tag => new RuleTag(tag, true, _preselectedTags.Contains(tag)))
                    .ToList()
            );

            // Add tags unique to this torrent
            Tags.AddRange(
                _preselectedTags
                    .Where(tag => !Tags.Any(t => t.Tag == tag))
                    .Select(tag => new RuleTag(tag, false, true))
                    .ToList()
            );

            //Debug.WriteLine($"Total tags: {Tags.Count}, {Tags.Count(t=>t.IsSelected)} selected");
        }

        private void Rows_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            SaveRows();
        }

        private MatchTestRowViewModel CreateMatchTestRowViewModel(string content = "")
        {
            MatchTestRowViewModel mtrvm = new() { MatchTest = content };
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
            if (!Design.IsDesignMode)
                RssRuleTestDataService.SetValue(Title, Rows.Select(t => t.MatchTest).ToList());
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
            try
            {
                IsSaving = true;

                // Got to rename first to prevent duplicating the entry
                if (OldTitle != Title)
                {
                    var tmpOldTitle = OldTitle;
                    await RenameAsync(OldTitle);
                    Renamed?.Invoke(OldTitle, Title);
                }

                // Update tags in rule itself:
                var selectedTags = Tags
                    .Where(t => t.IsSelected)
                    .Select(t => t.Tag)
                    .ToList();

                if (_rule.AdditionalData is not IDictionary<string, JToken> dic)
                    _rule.AdditionalData = dic = new Dictionary<string, JToken>();

                if (!dic.TryGetValue("torrentParams", out var torrentParamsToken) || torrentParamsToken is not JObject torrentParams)
                {
                    torrentParams = [];
                    dic["torrentParams"] = torrentParams;
                }

                // Ensure AffectedFeeds are set correctly 
                AffectedFeeds = SelectedFeeds.Select(f => f.Url).ToList().AsReadOnly();

                //
                // qBittorrent-net-client lacks properties for the majority of options that can be set.
                // Instead these will have to be set on the torrentParms property which is done below.
                // Confusingly some of these have a similar property which isn't used - so their value is
                // assigned to torrentparms as well.
                //
                torrentParams["tags"] = JToken.FromObject(selectedTags);
                torrentParams["download_path"] = JToken.FromObject(SavePath);
                torrentParams["category"] = JToken.FromObject(AssignedCategory);
                torrentParams["content_layout"] = SelectedContentLayoutItem.Value is null
                    ? JValue.CreateNull()
                    : JToken.FromObject(SelectedContentLayoutItem.Value);

                Debug.WriteLine("»" + torrentParams["content_layout"]);

                // Attempt to save
                await QBittorrentService.SetRssAutoDownloadingRuleAsync(Title, _rule);
                IsSaving = false;

                // Will trigger RssAutoDownloadingRulesViewModel to update its Rules collection
                IsNew = false;
                Warning = "";
                Saved?.Invoke(this);
            }
            catch (Exception e) { Debug.WriteLine(e.Message); }
            finally { IsSaving = false; }
        }

        public async Task RenameAsync(string oldTitle)
        {
            try
            {
                IsSaving = true;
                await QBittorrentService.RenameRssAutoDownloadingRuleAsync(oldTitle, Title);
                oldTitle = "";
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

        [RaiseChange]
        private (int, int) _mustContainErrorIndexes = (0, 0);
        [RaiseChange]
        private bool _mustContainErrored = false;
        [RaiseChange]
        private (int, int) _mustNotContainErrorIndexes = (0, 0);
        [RaiseChange]
        private bool _mustNotContainErrored = false;

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

        [RaiseChange]
        private ObservableCollection<string> _errors = Design.IsDesignMode ? ["Error error error"] : [];
        [RaiseChange]
        private ObservableCollection<string> _warnings = Design.IsDesignMode ? ["Warning warning warning"] : [];

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

            DataGridCollectionViewProperty = new DataGridCollectionView(RssArticles);
            var dgsdIsMatch = DataGridSortDescription.FromPath(nameof(RssArticleViewModel.IsMatch), ListSortDirection.Descending);
            var dgsdDate = DataGridSortDescription.FromPath(nameof(RssArticleViewModel.Date), ListSortDirection.Ascending);

            // Clearing and re-adding sort descriptions to notify the data grid
            DataGridCollectionViewProperty.SortDescriptions.Clear();
            DataGridCollectionViewProperty.SortDescriptions.Add(dgsdIsMatch);
            DataGridCollectionViewProperty.SortDescriptions.Add(dgsdDate);
            DataGridCollectionViewProperty.MoveCurrentToFirst();

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

            FilteredTestDataCount = Rows.Count(r => r.IsMatch);
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
                Warnings.Add(Resources.Resources.RssRuleView_Warning);
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
                        Errors.Add(Resources.Resources.RssRuleView_EpisodeFilter + ": " + epft.ErrorMessage!);
                    else
                        Errors.Add(Resources.Resources.RssRuleView_EpisodeFilterInvalid);
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
                Errors.Add(Resources.Resources.RssRuleView_NotValidWildcard);
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
        [RaiseChange]
        private bool _episodeFilterErrored = false;

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
                    if (i + 1 < _tokens.Count)
                    {
                        EpisodeFilterToken nextToken = _tokens[i + 1];
                        EpisodeFilterToken? nextNextToken = i + 2 < _tokens.Count ? _tokens[i + 2] : null;
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

        private ObservableCollection<RssFeedViewModel> _selectedFeeds = [];
        public ObservableCollection<RssFeedViewModel> SelectedFeeds
        {
            get => _selectedFeeds;
            set
            {
                _selectedFeeds.CollectionChanged -= SelectedFeeds_CollectionChanged;

                if (value != _selectedFeeds)
                {
                    _selectedFeeds = value;
                    this.RaisePropertyChanged(nameof(SelectedFeeds));

                    if (value != null)
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

        [RaiseChange]
        private int _filteredArticleCount = 0;
        [RaiseChange]
        private int _filteredTestDataCount = 0;

        private async Task ClearDownloadedEpisodesAsync()
        {
            try
            {
                await QBittorrentService.SetRssAutoDownloadingRuleAsync(Title, _rule);
            }
            catch (Exception e) { Debug.WriteLine(e.Message); }
        }

        public RssAutoDownloadingRuleViewModel GetCopy()
        {
            return new RssAutoDownloadingRuleViewModel(new RssAutoDownloadingRule
            {
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
            {
                SelectedContentLayoutItem = this.SelectedContentLayoutItem,
                Tags = this.Tags,
                IsNew = this.IsNew,
                Warning = this.Warning
            };
        }

        [GeneratedRegex("offset (?<index>\\d+)")]
        private static partial Regex _offsetRegex();
    }
}