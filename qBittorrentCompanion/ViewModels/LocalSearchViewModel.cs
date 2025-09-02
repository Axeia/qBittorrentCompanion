using AutoPropertyChangedGenerator;
using DynamicData;
using QBittorrent.Client;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels
{
    public partial class LocalSearchViewModel : SearchViewModelBase
    {
        private RemoteSearchPluginViewModel? _selectedSearchPlugin = null;

        /// <summary>
        /// Cast to <see cref="LocalSearchPluginViewModel"/> when needed
        /// </summary>
        public override RemoteSearchPluginViewModel? SelectedSearchPlugin
        {
            get => _selectedSearchPlugin;
            set
            {
                if (_selectedSearchPlugin != value)
                {
                    //Save selected plugin to config
                    _selectedSearchPlugin = value;
                        ConfigService.LastSelectedLocalSearchPlugin = _selectedSearchPlugin == null
                        ? ""
                        : _selectedSearchPlugin!.Name;

                    this.RaisePropertyChanged(nameof(SelectedSearchPlugin));
                    UpdateCategories();

                    RestoreLastSelectedSearchCategoryOrDefaultToFirst();
                }
            }
        }

        private SearchPluginCategory? _selectedSearchPluginCategory = null;

        public override SearchPluginCategory? SelectedSearchPluginCategory
        {
            get => _selectedSearchPluginCategory;
            set
            {
                if (value != _selectedSearchPluginCategory)
                {
                    _selectedSearchPluginCategory = value;
                    this.RaisePropertyChanged(nameof(SelectedSearchPluginCategory));

                    ConfigService.LastSelectedLocalSearchCategory = value == null ? "" : value.Name;
                }
            }
        }

        private void UpdateCategories()
        {
            PluginCategories.Clear();

            if (_selectedSearchPlugin is LocalSearchPluginViewModel searchPlugin)
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
                    categories = [.. _selectedSearchPlugin.Categories];
                }
                // Get only unique categories based on the .Name attribute
                PluginCategories.AddRange(categories.GroupBy(c => c.Name).Select(g => g.First()));
            }

            this.RaisePropertyChanged(nameof(PluginCategories));
            this.RaisePropertyChanged(nameof(SelectedSearchPluginCategory));
        }

        public LocalSearchViewModel()
        {
            SearchPlugins.Add(LocalSearchPluginService.Instance.SearchPlugins);
            LocalSearchPluginService.Instance.SearchPlugins.CollectionChanged += SearchPluginService_CollectionChanged;

            SearchResults.CollectionChanged += (e, d) => UpdateFilteredSearchResults();

            // When categories are fetched
            // for every single one this is called. Add a small delay 
            _ = Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                h => SearchPlugins.CollectionChanged += h,
                h => SearchPlugins.CollectionChanged -= h)
                .Throttle(TimeSpan.FromMilliseconds(100)) // Wait for 100ms of inactivity
                .ObserveOn(RxApp.MainThreadScheduler) // Ensure it runs on the UI thread
                .Subscribe(_ =>
                {
                    RestoreLastSelectedSearchPluginOrDefaultToFirst();
                    RestoreLastSelectedSearchCategoryOrDefaultToFirst();
                });
        }

        private void SearchPluginService_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            SearchPlugins.Clear();
            foreach (var searchPlugin in LocalSearchPluginService.Instance.SearchPlugins)
                SearchPlugins.Add(searchPlugin);

            this.RaisePropertyChanged(nameof(SearchPluginCount));
        }

        private void RestoreLastSelectedSearchCategoryOrDefaultToFirst()
        {
            //Attempt to find and restore last selected select search category
            if (ConfigService.LastSelectedLocalSearchCategory != string.Empty)
            {
                foreach (var category in PluginCategories)
                {
                    if (category.Name.Equals(ConfigService.LastSelectedLocalSearchCategory))
                    {
                        SelectedSearchPluginCategory = category;
                        return;
                    }
                }
            }

            SelectedSearchPluginCategory = PluginCategories.FirstOrDefault();
        }

        private void RestoreLastSelectedSearchPluginOrDefaultToFirst()
        {
            //Attempt to find and restore last selected select search plugin
            if(ConfigService.LastSelectedLocalSearchPlugin != string.Empty)
            {
                foreach(var plugin in SearchPlugins)
                {
                    if(plugin.Name.Equals(ConfigService.LastSelectedLocalSearchPlugin))
                    {
                        SelectedSearchPlugin = plugin;
                        return;
                    }
                }
            }

            //Couldn't be done, just select the first
            SelectedSearchPlugin = SearchPlugins.FirstOrDefault();
        }

        [AutoPropertyChanged]
        private ObservableCollection<SearchResult> _filteredSearchResults = [];

        public override async Task StartSearchAsync()
        {
            IsSearching = true;
            SearchResults.Clear();

            PythonSearchBridge psb = new();
            psb.SearchResultProcessed += (result) => { SearchResults.Add(result); };
            await psb.StartSearchAsync(
                GetSearchPlugins(), 
                SearchQuery, 
                SelectedSearchPluginCategory?.Id ?? SearchPlugin.All
            );

            IsSearching = false;
        }

        private IEnumerable<string> GetSearchPlugins()
        {
            if (SelectedSearchPlugin == null)
                return [];

            if (SelectedSearchPlugin.Name == SearchPlugin.All)
            {
                return LocalSearchPluginService.Instance.PluginFilesAll;

            }
            else if (SelectedSearchPlugin.Name == SearchPlugin.Enabled)
            {
                return LocalSearchPluginService.Instance.PluginFilesEnabled;
            }
            else
            {
                return [SelectedSearchPlugin.Name];
            }
        }
    }
}
