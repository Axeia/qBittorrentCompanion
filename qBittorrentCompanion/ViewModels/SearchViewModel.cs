using Avalonia.Controls;
using Avalonia.Threading;
using DynamicData;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace qBittorrentCompanion.ViewModels
{
    public class SearchViewModel : BytesBaseViewModel
    {
        private ObservableCollection<SearchPlugin> _searchPlugins = [];

        public ObservableCollection<SearchPlugin> SearchPlugins
        {
            get => _searchPlugins;
            set
            {
                if (_searchPlugins != value)
                {
                    _searchPlugins = value;
                    OnPropertyChanged(nameof(SearchPlugins));
                }
            }
        }

        private SearchPlugin? _selectedSearchPlugin = null;
        public SearchPlugin? SelectedSearchPlugin
        {
            get => _selectedSearchPlugin;
            set
            {
                if (_selectedSearchPlugin != value)
                {
                    // Save current category
                    var tempSelectedCategoryId = PluginCategories.FirstOrDefault(pc =>
                        pc.Id == (SelectedSearchPluginCategory?.Id ?? SearchPlugin.All),
                        _defaultCategories.First()
                    ).Id;

                    _selectedSearchPlugin = value;
                    OnPropertyChanged(nameof(SelectedSearchPlugin));
                    UpdateCategories();

                    // Restore category if possible, if not - default it to `all`
                    SelectedSearchPluginCategory = PluginCategories.FirstOrDefault(pc =>
                        pc.Id == tempSelectedCategoryId, PluginCategories.FirstOrDefault() ??
                        _defaultCategories.First()
                    );
                }
            }
        }

        private SearchPluginCategory? _selectedSearchPluginCategory = null;

        public SearchPluginCategory? SelectedSearchPluginCategory
        {
            get => _selectedSearchPluginCategory;
            set
            {
                if (value != _selectedSearchPluginCategory)
                {
                    _selectedSearchPluginCategory = value;
                    OnPropertyChanged(nameof(SelectedSearchPluginCategory));
                }
            }
        }

        private void UpdateCategories()
        {
            PluginCategories.Clear();

            if (_selectedSearchPlugin is SearchPlugin searchPlugin)
            {
                List<SearchPluginCategory> categories = [];

                if (searchPlugin.Name == SearchPlugin.Enabled)
                {
                    categories = SearchPlugins.Where(p => p.IsEnabled).SelectMany(p => p.Categories).ToList();
                }
                else if (searchPlugin.Name == SearchPlugin.All)
                {
                    categories = SearchPlugins.SelectMany(p => p.Categories).ToList();
                }
                else
                {
                    categories = _selectedSearchPlugin.Categories.ToList();
                }
                // Get only unique categories based on the .Name attribute
                PluginCategories.AddRange(categories.GroupBy(c => c.Name).Select(g => g.First()));
            }

            OnPropertyChanged(nameof(PluginCategories));
            OnPropertyChanged(nameof(SelectedSearchPluginCategory));
        }

        private ObservableCollection<SearchPluginCategory> _pluginCategories = [];

        public ObservableCollection<SearchPluginCategory> PluginCategories
        {
            get => _pluginCategories;
            set
            {
                if (_pluginCategories != value)
                {
                    _pluginCategories = value;
                    OnPropertyChanged(nameof(PluginCategories));
                }
            }
        }

        private List<SearchPluginCategory> _defaultCategories = [new SearchPluginCategory(SearchPlugin.All, "All categories")];
        public SearchViewModel()
        {
            SearchPlugins.Add(new SearchPlugin() { FullName = "Only enabled", Name = SearchPlugin.Enabled, Categories = _defaultCategories });
            SearchPlugins.Add(new SearchPlugin() { FullName = "All plugins", Name = SearchPlugin.All, Categories = _defaultCategories });
            //_ = FetchDataAsync();
        }

        protected override async Task FetchDataAsync()
        {
            IReadOnlyList<SearchPlugin> plugins = await QBittorrentService.QBittorrentClient.GetSearchPluginsAsync();
            foreach (SearchPlugin plugin in plugins)
            {
                SearchPlugins.Add(plugin);
                //plugin.Categories
            }
            SelectedSearchPlugin = SearchPlugins.First();
            SelectedSearchPluginCategory = PluginCategories.First();

            //Update(httpSources);

            //_refreshTimer.Start();
        }
        public void Initialise()
        {
            _ = FetchDataAsync();
        }


        protected override async Task UpdateDataAsync(object? sender, EventArgs e)
        {
            //Debug.WriteLine($"Updating HttpSources for {_infoHash}");
            IReadOnlyList<Uri> httpSources = await QBittorrentService.QBittorrentClient.GetTorrentWebSeedsAsync(_infoHash).ConfigureAwait(false);
            Update(httpSources);
        }

        public async void Update(IReadOnlyList<Uri> httpSources)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                /*foreach (Uri httpSource in httpSources)
                {
                    if (!HttpSources.Contains(httpSource.AbsoluteUri))
                        HttpSources.Add(httpSource.AbsoluteUri);
                }*/
            });
        }

        private string _searchQuery = "";
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (value != _searchQuery)
                {
                    _searchQuery = value;
                    OnPropertyChanged(nameof(SearchQuery));
                }
            }
        }

        private ObservableCollection<SearchResult> _searchResults = [];
        public ObservableCollection<SearchResult> SearchResults 
        {
            get => _searchResults;
            set
            {
                _searchResults = value;
                OnPropertyChanged(nameof(SearchResults));
                UpdateFilteredSearchResults();
            }
        }

        private ObservableCollection<SearchResult> _filteredSearchResults = [];
        public ObservableCollection<SearchResult> FilteredSearchResults
        {
            get => _filteredSearchResults;
            set
            {
                if (_filteredSearchResults != value)
                {
                    _filteredSearchResults = value;
                    OnPropertyChanged(nameof(FilteredSearchResults));
                }
            }
        }

        private SearchResult? _selectedSearchResult = null;
        public SearchResult? SelectedSearchResult
        {
            get => _selectedSearchResult;
            set
            {
                if (value != _selectedSearchResult)
                {
                    //if (value is not null)
                    //    Debug.WriteLine(value.FileSize);
                    _selectedSearchResult = value;
                    OnPropertyChanged(nameof(SelectedSearchResult));
                }
            }
        }

        private int currentSearchJobId = -1;
        public async void StartSearch()
        {
            IsSearching = true;
            SearchResults.Clear();

            currentSearchJobId = await QBittorrentService.QBittorrentClient.StartSearchAsync(
                SearchQuery,
                [SelectedSearchPlugin?.Name ?? SearchPlugin.All], // Seems to be support for selecting multiple search plugins?
                SelectedSearchPluginCategory?.Id ?? SearchPlugin.All
            );

            var searchResult = await QBittorrentService.QBittorrentClient.GetSearchResultsAsync(currentSearchJobId);
            do
            {
                foreach (var result in searchResult.Results)
                {
                    // Filter out erroneous results 
                    if(result.FileSize > -1)
                        SearchResults.Add(result);
                }
                UpdateFilteredSearchResults();
                await Task.Delay(2000);
                searchResult = await QBittorrentService.QBittorrentClient.GetSearchResultsAsync(currentSearchJobId);
            }
            //If the searchjob isn't done yet and the user hasn't cancelled it - keep going.
            while (searchResult.Status == SearchJobStatus.Running && IsSearching);

            //If the user cancelled it
            if(searchResult.Status == SearchJobStatus.Running)
            {
                await QBittorrentService.QBittorrentClient.StopSearchAsync(currentSearchJobId);
            }

            IsSearching = false;
        }

        internal void EndSearch()
        {
            IsSearching = false;
        }

        public bool _isSearching = false;
        public bool IsSearching
        {
            get => _isSearching;
            set
            {
                if (value != _isSearching)
                {
                    _isSearching = value;
                    OnPropertyChanged(nameof(IsSearching));
                }
            }
        }

        /// <summary>
        /// Search results gets filtered on this text, the target 
        /// is determined by FilterOn
        /// </summary>
        private string _filterText = string.Empty;
        public string FilterText
        {
            get => _filterText;
            set
            {
                if (_filterText != value)
                {
                    _filterText = value;
                    OnPropertyChanged(nameof(FilterText));
                    UpdateFilteredSearchResults();
                }
            }
        }

        public static string[] FilterOnOptions => ["Name"];

        private string _filterOn = FilterOnOptions[0];
        public string FilterOn
        {
            get => _filterOn;
            set
            {
                if (_filterOn != value)
                {
                    _filterOn = value;
                    OnPropertyChanged(nameof(FilterOn));
                    UpdateFilteredSearchResults();
                }
            }
        }

        private int _filterSeeds = 0;
        public int? FilterSeeds
        {
            get => _filterSeeds;
            set
            {
                if (_filterSeeds != value)
                {
                    _filterSeeds = value ?? 0;
                    OnPropertyChanged(nameof(FilterSeeds));
                    UpdateFilteredSearchResults();
                }
            }
        }

        private int _filterSeedsTo = 0;
        public int? FilterSeedsTo
        {
            get => _filterSeedsTo;
            set
            {
                if (_filterSeedsTo != value)
                {
                    _filterSeedsTo = value ?? 0;
                    OnPropertyChanged(nameof(FilterSeedsTo));
                    UpdateFilteredSearchResults();
                }
            }
        }

        private double _filterSize = 0.0;
        public double? FilterSize
        {
            get => _filterSize;
            set
            {
                if (_filterSize != value)
                {
                    _filterSize = value ?? 0.0;
                    OnPropertyChanged(nameof(FilterSize));
                    UpdateFilteredSearchResults();
                }
            }
        }

        private double _filterSizeTo = 0.0;
        public double? FilterSizeTo
        {
            get => _filterSizeTo;
            set
            {
                if (_filterSizeTo != value)
                {
                    _filterSizeTo = value ?? 0;
                    OnPropertyChanged(nameof(FilterSizeTo));
                    UpdateFilteredSearchResults();
                }
            }
        }

        public static string[] SizeOptions => ["B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB"];

        private string _filterSizeUnit = SizeOptions[2];
        public string FilterSizeUnit
        {
            get => _filterSizeUnit;
            set
            {
                if (_filterSizeUnit != value)
                {
                    _filterSizeUnit = value;
                    OnPropertyChanged(nameof(FilterSizeUnit));
                    UpdateFilteredSearchResults();
                }
            }
        }

        private string _filterSizeToUnit = SizeOptions[2];
        public string FilterSizeToUnit
        {
            get => _filterSizeToUnit;
            set
            {
                if (_filterSizeToUnit != value)
                {
                    _filterSizeToUnit = value;
                    OnPropertyChanged(nameof(FilterSizeToUnit));
                    UpdateFilteredSearchResults();
                }
            }
        }

        private void UpdateFilteredSearchResults()
        {
            var filteredSearchResults = SearchResults.Where(s =>
                (string.IsNullOrWhiteSpace(FilterText) || s.FileName.Contains(FilterText, StringComparison.OrdinalIgnoreCase))
            );

            if (FilterSeeds > 0)
                filteredSearchResults = filteredSearchResults.Where(s => s.Seeds >= FilterSeeds);
            if (FilterSeedsTo > 0)
                filteredSearchResults = filteredSearchResults.Where(s => s.Seeds <= FilterSeedsTo);

            if (FilterSize > 0)
            {
                filteredSearchResults = filteredSearchResults.Where(
                    s => s.FileSize >= (FilterSize * DataConverter.GetMultiplierForUnit(FilterSizeUnit))
                );
            }

            if (FilterSizeTo > 0)
            {
                filteredSearchResults = filteredSearchResults.Where(
                    s => s.FileSize <= (FilterSizeTo * DataConverter.GetMultiplierForUnit(FilterSizeToUnit))
                );
            }

            FilteredSearchResults = new ObservableCollection<SearchResult>(filteredSearchResults);
        }
    }
}
