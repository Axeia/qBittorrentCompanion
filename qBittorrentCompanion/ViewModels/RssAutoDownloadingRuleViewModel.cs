using Avalonia.Collections;
using Newtonsoft.Json.Linq;
using QBittorrent.Client;
using qBittorrentCompanion.Services;
using qBittorrentCompanion.Validators;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace qBittorrentCompanion.ViewModels
{
    public class RssAutoDownloadingRuleViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private bool _isSaving = false;
        public bool IsSaving
        {
            get => _isSaving;
            set
            {
                if (value != _isSaving)
                {
                    _isSaving = value;
                    OnPropertyChanged(nameof(IsSaving));
                }
            }
        }

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

        private DataGridCollectionView? _dataGridCollectionView;
        public DataGridCollectionView? DataGridCollectionView
        {
            get => _dataGridCollectionView;
            set 
            {
                if (value != _dataGridCollectionView)
                {
                    _dataGridCollectionView = value;
                    OnPropertyChanged(nameof(DataGridCollectionView));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Gets populated externally (once). 
        /// Not actually a property for the model, only used to display the options
        /// </summary>
        private ReadOnlyCollection<string> _categories;
        public ReadOnlyCollection<string> Categories
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

        /// <summary>
        /// Gets populated externally (once)<br/>
        /// Not actually a property for the model, only used to display the options<br/>
        /// <br/>
        /// <br/>
        /// <see cref="UpdateSelectedFeeds"/> is called so that SelectedFeeds contains 
        /// <see cref="AffectedFeeds"/>.
        /// </summary>
        private ObservableCollection<RssFeedViewModel> _rssFeeds;
        public ObservableCollection<RssFeedViewModel> RssFeeds
        {
            get => _rssFeeds;
            set
            {
                if (value != _rssFeeds)
                {
                    _rssFeeds = value;
                    OnPropertyChanged(nameof(RssFeeds));
                    UpdateSelectedFeeds();
                }
            }
        }

        /// <summary>
        /// Adds all <see cref="AffectedFeeds"/> to <see cref="SelectedFeeds"/>. <br/>
        /// Because of the type difference (<c>string</c> vs <c>SimplifiedRssFeed</c>) 
        /// <see cref="RssFeed"/> is used as a lookup table, <br/> ultimately the selection is on these RssFeeds anyhow.
        /// </summary>
        private void UpdateSelectedFeeds()
        {
            foreach (var affectedRssFeedUri in AffectedFeeds)
                foreach (var rssFeed in RssFeeds)
                    if (rssFeed.Url == affectedRssFeedUri)
                        SelectedFeeds.Add(rssFeed);

            OnPropertyChanged(nameof(SelectedFeeds));
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
                    OnPropertyChanged(nameof(RssArticles));
                    Filter();
                }
            }
        }

        private RssAutoDownloadingRule _rule;

        private string _title = "";
        public string Title
        {
            get => _title;
            set
            {
                if (value != _title)
                {
                    _title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        public ReactiveCommand<string, Unit> RenameCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearDownloadedEpisodesCommand { get; }

        public RssAutoDownloadingRuleViewModel(RssAutoDownloadingRule rule, string title, ObservableCollection<RssFeedViewModel> rssFeeds, IReadOnlyList<Uri> affectedFeeds)
        {
            _title = title;
            _rule = rule;

            RssFeeds = rssFeeds;
            SelectedFeeds = new ObservableCollection<RssFeedViewModel>(RssFeeds.Where(r => affectedFeeds.Contains(r.Url)));
            RenameCommand = ReactiveCommand.CreateFromTask<string>(RenameAsync);
            ClearDownloadedEpisodesCommand = ReactiveCommand.CreateFromTask(ClearDownloadedEpisodesAsync);
            RssArticles =  RssFeeds.SelectMany(f => f.Articles)
                .Select(article => new RssArticleViewModel(article))
                .ToList();


            // Set testdata
            var testData = RssRuleTestDataService.GetEntry(Title);
            if (testData != null)
            {
                foreach (var testCase in testData)
                {
                    Rows.Add(new MatchTestRowViewModel() { MatchTest = testCase });
                }
            }
            if (Rows.Count > 0 && Rows.Last().MatchTest != string.Empty)
                Rows.Add(new MatchTestRowViewModel());

            FilterRssArticles();
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
                OnPropertyChanged(Title);

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
                    OnPropertyChanged(nameof(Enabled));
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
                    OnPropertyChanged(nameof(MustContain));
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
                    OnPropertyChanged(nameof(MustNotContain));
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
                    OnPropertyChanged(nameof(UseRegex));
                    ValidateAndFilter();
                }
            }
        }

        private Dictionary<string, string> _errors = [];

        public bool HasErrors => _errors.Any();

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public IEnumerable GetErrors(string? propertyName)
        {
            if (propertyName is not null && _errors.ContainsKey(propertyName))
                yield return _errors[propertyName];
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
            Validate(); //Validates (and sets _mustContainRegex/_mustNotContainRegex)
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
                article.IsMatch = RssRuleIsMatchViewModel.IsTextMatch(
                    article.Title, MustContain, MustNotContain, EpisodeFilter, UseRegex
                );


            DataGridCollectionView = new DataGridCollectionView(RssArticles);
            var dgsdIsMatch = DataGridSortDescription.FromPath(nameof(RssArticleViewModel.IsMatch), ListSortDirection.Descending);
            DataGridCollectionView.SortDescriptions.Add(dgsdIsMatch);
            var dgsdDate = DataGridSortDescription.FromPath(nameof(RssArticleViewModel.Date), ListSortDirection.Ascending);
            DataGridCollectionView.SortDescriptions.Add(dgsdDate);

            FilteredArticleCount = RssArticles.Count(a => a.IsMatch);
        }
        
        private void FilterTestData()
        {
            // Temporarily keep count
            int c = 0;

            // Filter test data
            foreach (MatchTestRowViewModel row in Rows)
            {
                row.IsMatch = RssRuleIsMatchViewModel.IsTextMatch(
                    row.MatchTest, MustContain, MustNotContain, EpisodeFilter, UseRegex
                );

                if (row.IsMatch)
                    c++;
            }

            // Assign count to property - by doing it once instead of the loop the UI will 
            // only be updated when it's needed.
            FilteredTestDataCount = c;
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
            _errors.Clear();

            if (String.IsNullOrEmpty(MustContain) 
                && String.IsNullOrEmpty(MustNotContain) 
                && String.IsNullOrEmpty(EpisodeFilter))
            {
                string errorMessage = "At least 1 of the marked fields has to be filled in.";
                _errors[nameof(MustContain)] = errorMessage;
                _errors[nameof(MustNotContain)] = errorMessage;
                _errors[nameof(EpisodeFilter)] = errorMessage;
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

            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(null));
        }

        private void ValidateRegex(string value, string propertyName)
        {
            Regex? regex = null;
            try
            {
                regex = new Regex(value);
            }
            catch (RegexParseException)
            {
                _errors[propertyName] = "Not a valid regular expression";
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
                _errors[propertyName] = "Not a valid wildcard (or the live preview just can't be trusted)";
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
                    OnPropertyChanged(nameof(EpisodeFilter));
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
                    OnPropertyChanged(nameof(SmartFilter));
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
                    OnPropertyChanged(nameof(PreviouslyMatchedEpisodes));
                }
            }
        }

        /// <inheritdoc cref="RssAutoDownloadingRule.AffectedFeeds"/>
        public IReadOnlyList<Uri> AffectedFeeds
        {
            get => _rule.AffectedFeeds;
            set
            {
                if (value != _rule.AffectedFeeds)
                {
                    _rule.AffectedFeeds = value;
                    OnPropertyChanged(nameof(AffectedFeeds));
                }
            }
        }

        private ObservableCollection<RssFeedViewModel> _selectedFeeds = [];
        public ObservableCollection<RssFeedViewModel> SelectedFeeds
        {
            get => _selectedFeeds;
            set
            {
                if(value != _selectedFeeds)
                {
                    _selectedFeeds = value;
                    OnPropertyChanged(nameof(SelectedFeeds));
                    foreach (RssFeedViewModel feed in value)
                        Debug.WriteLine($"selected: {feed.Title}");
                }
            }
        }

        /// <inheritdoc cref="RssAutoDownloadingRule.IgnoreDays"/>
        public int IgnoreDays
        {
            get => _rule.IgnoreDays;
            set
            {
                if (value != _rule.IgnoreDays)
                {
                    _rule.IgnoreDays = value;
                    OnPropertyChanged(nameof(IgnoreDays));
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
                    OnPropertyChanged(nameof(LastMatch));
                }
            }
        }

        /// <inheritdoc cref="RssAutoDownloadingRule.AddPaused"/>
        public bool? AddPaused
        {
            get => _rule.AddPaused;
            set
            {
                if (value != _rule.AddPaused)
                {
                    _rule.AddPaused = value;
                    OnPropertyChanged(nameof(AddPaused));
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
                    OnPropertyChanged(nameof(AssignedCategory));
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
                    OnPropertyChanged(nameof(SavePath));
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
                    OnPropertyChanged(nameof(AdditionalData));
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

        private int _filteredTestDataCount = 0;
        public int FilteredTestDataCount
        {
            get => _filteredTestDataCount;
            set
            {
                if (value != _filteredTestDataCount)
                {
                    _filteredTestDataCount = value;
                    OnPropertyChanged(nameof(FilteredTestDataCount));
                }
            }
        }

        private async Task ClearDownloadedEpisodesAsync()
        {
            try
            {
                await QBittorrentService.QBittorrentClient.SetRssAutoDownloadingRuleAsync(Title, _rule);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
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
                SavePath = this.SavePath
            },
            Title,
            RssFeeds,
            AffectedFeeds);
        }
    }
}
