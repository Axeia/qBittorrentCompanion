using AutoPropertyChangedGenerator;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static qBittorrentCompanion.Helpers.DataConverter;

namespace qBittorrentCompanion.ViewModels
{
    /// <summary>
    /// Serves as the base for <see cref="LocalSearchViewModel"/> and <see cref="RemoteSearchViewModel"/> 
    /// Keep in mind that those classes populate <see cref="SearchPlugins"/> with 
    /// <see cref="LocalSearchPluginViewModel"/> and <see cref="RemoteSearchPluginViewModel"/> respectively.
    /// 
    /// So when the difference between local and remote search plugins is needed, cast the entry to the appropriate type.
    /// </summary>
    public abstract partial class SearchViewModelBase : RssPluginSupportBaseViewModel
    {
        public abstract RemoteSearchPluginViewModel? SelectedSearchPlugin { get; set; }

        [AutoPropertyChanged]
        private string _searchQuery = "";

        public bool UseRemoteSearch
        {
            get
            {
                if (Design.IsDesignMode)
                    return true;

                return ConfigService.UseRemoteSearch;
            }
            set
            {
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                    && desktop.MainWindow?.DataContext is MainWindowViewModel mwvm)
                {
                    mwvm.UseRemoteSearch = value;
                    this.RaisePropertyChanged(nameof(UseRemoteSearch));
                }
                else
                    Debug.WriteLine($"{GetFullTypeName<SearchViewModelBase>()}.UseRemoteSearch setter called without there being a MainWindowViewModel");
            }
        }

        public ObservableCollection<RemoteSearchPluginViewModel> SearchPlugins { get; } = [];
        public int SearchPluginCount => SearchPlugins.Count - 2; // Subtract the "Only enabled" and "All plugins" entries

        private ObservableCollection<SearchResult> _searchResults = [];
        public ObservableCollection<SearchResult> SearchResults
        {
            get => _searchResults;
            set
            {
                _searchResults = value;
                this.RaisePropertyChanged(nameof(SearchResults));
                UpdateFilteredSearchResults();
            }
        }

        [AutoPropertyChanged]
        private ObservableCollection<SearchResult> _filteredSearchResults = [];

        private SearchResult? _selectedSearchResult = null;
        public SearchResult? SelectedSearchResult
        {
            get => _selectedSearchResult;
            set
            {
                if (value != _selectedSearchResult)
                {
                    _selectedSearchResult = value;
                    if (_selectedSearchResult != null)
                        PluginInput = _selectedSearchResult.FileName;
                    this.RaisePropertyChanged(nameof(SelectedSearchResult));
                }
            }
        }

        [AutoPropertyChanged]
        private ObservableCollection<SearchPluginCategory> _pluginCategories = [];

        /// <summary>
        /// Should contain "Only enabled" and "All" at the very least
        /// </summary>
        public abstract SearchPluginCategory? SelectedSearchPluginCategory { get; set; }

        [AutoPropertyChanged]
        public bool _isSearching = false;

        private bool _expandSearchRssPlugin = Design.IsDesignMode || ConfigService.ExpandSearchRssPlugin;
        public bool ExpandSearchRssPlugin
        {
            get => ConfigService.ExpandSearchRssPlugin;
            set
            {
                ConfigService.ExpandSearchRssPlugin = value;
                this.RaiseAndSetIfChanged(ref _expandSearchRssPlugin, value);
            }
        }

        /// <summary>
        /// Search results gets filtered on this text, the target 
        /// is determined by <see cref="FilterOn"/>
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
                    this.RaisePropertyChanged(nameof(FilterText));
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
                    this.RaisePropertyChanged(nameof(FilterOn));
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
                    this.RaisePropertyChanged(nameof(FilterSeeds));
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
                    this.RaisePropertyChanged(nameof(FilterSeedsTo));
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
                    this.RaisePropertyChanged(nameof(FilterSize));
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
                    this.RaisePropertyChanged(nameof(FilterSizeTo));
                    UpdateFilteredSearchResults();
                }
            }
        }

        public static ByteUnit[] SizeOptions => (ByteUnit[])Enum.GetValues(typeof(ByteUnit));


        private ByteUnit _filterSizeUnit = ByteUnit.MiB;
        public ByteUnit FilterSizeUnit
        {
            get => _filterSizeUnit;
            set
            {
                if (_filterSizeUnit != value)
                {
                    _filterSizeUnit = value;
                    this.RaisePropertyChanged(nameof(FilterSizeUnit));
                    UpdateFilteredSearchResults();
                }
            }
        }

        private ByteUnit _filterSizeToUnit = ByteUnit.MiB;
        public ByteUnit FilterSizeToUnit
        {
            get => _filterSizeToUnit;
            set
            {
                if (_filterSizeToUnit != value)
                {
                    _filterSizeToUnit = value;
                    this.RaisePropertyChanged(nameof(FilterSizeToUnit));
                    UpdateFilteredSearchResults();
                }
            }
        }

        /// <summary>
        /// Abstract method that derived classes must implement to define their search logic
        /// </summary>
        public abstract Task StartSearchAsync();

        /// <summary>
        /// Virtual method that can be overridden by derived classes to define custom end search logic
        /// </summary>
        public virtual void EndSearch()
        {
            IsSearching = false;
        }

        /// <summary>
        /// Convenience method that wraps StartSearchAsync for compatibility with existing async void calls
        /// </summary>
        public async void StartSearch()
        {
            await StartSearchAsync();
        }

        protected virtual void UpdateFilteredSearchResults()
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
                    s => s.FileSize >= (FilterSize * DataConverter.Multipliers[FilterSizeUnit])
                );
            }

            if (FilterSizeTo > 0)
            {
                filteredSearchResults = filteredSearchResults.Where(
                    s => s.FileSize <= (FilterSizeTo * DataConverter.Multipliers[FilterSizeToUnit])
                );
            }

            FilteredSearchResults = new ObservableCollection<SearchResult>(filteredSearchResults);
        }
    }
}