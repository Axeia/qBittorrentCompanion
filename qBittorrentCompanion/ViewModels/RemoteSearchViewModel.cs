using DynamicData;
using QBittorrent.Client;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels
{
    public partial class RemoteSearchViewModel : SearchViewModelBase
    {
        protected RemoteSearchPluginViewModel? _selectedSearchPlugin = null;
        public override RemoteSearchPluginViewModel? SelectedSearchPlugin
        {
            get => _selectedSearchPlugin;
            set
            {
                if (_selectedSearchPlugin != value)
                {
                    //Save selected plugin to config
                    _selectedSearchPlugin = value;
                        ConfigService.LastSelectedRemoteSearchPlugin = _selectedSearchPlugin == null
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

                    ConfigService.LastSelectedRemoteSearchCategory = value == null ? "" : value.Name;
                }
            }
        }

        private void UpdateCategories()
        {
            PluginCategories.Clear();

            if (_selectedSearchPlugin is RemoteSearchPluginViewModel searchPlugin)
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

        public RemoteSearchViewModel()
        {
            SearchPlugins.Add(RemoteSearchPluginService.Instance.SearchPlugins);
            RemoteSearchPluginService.Instance.SearchPlugins.CollectionChanged += SearchPluginService_CollectionChanged;

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

            SearchPlugins.CollectionChanged += SearchPlugins_CollectionChanged;
        }

        private void SearchPluginService_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            SearchPlugins.Clear();
            SearchPlugins.Add(RemoteSearchPluginService.Instance.SearchPlugins); // Will trigger SearchPlugins_CollectionChanged
        }

        private void SearchPlugins_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RestoreLastSelectedSearchPluginOrDefaultToFirst();
            RestoreLastSelectedSearchCategoryOrDefaultToFirst();
            this.RaisePropertyChanged(nameof(SearchPluginCount));
        }

        private void RestoreLastSelectedSearchCategoryOrDefaultToFirst()
        {
            //Attempt to find and restore last selected select search category
            if (ConfigService.LastSelectedRemoteSearchCategory != string.Empty)
            {
                foreach (var category in PluginCategories)
                {
                    if (category.Name.Equals(ConfigService.LastSelectedRemoteSearchCategory))
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
            if(ConfigService.LastSelectedRemoteSearchPlugin != string.Empty)
            {
                foreach(var plugin in SearchPlugins)
                {
                    if(plugin.Name.Equals(ConfigService.LastSelectedRemoteSearchPlugin))
                    {
                        SelectedSearchPlugin = plugin;
                        return;
                    }
                }
            }

            //Couldn't be done, just select the first
            SelectedSearchPlugin = SearchPlugins.FirstOrDefault();
        }

        private int currentSearchJobId = -1;
        public async override Task StartSearchAsync()
        {
            IsSearching = true;
            SearchResults.Clear();

            currentSearchJobId = await QBittorrentService.StartSearchAsync(
                SearchQuery,
                [SelectedSearchPlugin?.Name ?? SearchPlugin.All], // Seems to be support for selecting multiple search plugins?
                SelectedSearchPluginCategory?.Id ?? SearchPlugin.All
            );

            var searchResult = await QBittorrentService.GetSearchResultsAsync(currentSearchJobId);
            if (searchResult != null)
            { 
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
                    searchResult = await QBittorrentService.GetSearchResultsAsync(currentSearchJobId);
                }
                //If the searchjob isn't done yet and the user hasn't cancelled it - keep going.
                while (searchResult!.Status == SearchJobStatus.Running && IsSearching);

                //If the user cancelled it
                if(searchResult.Status == SearchJobStatus.Running)
                {
                    await QBittorrentService.StopSearchAsync(currentSearchJobId);
                }
            }

            IsSearching = false;
        }
    }
}
