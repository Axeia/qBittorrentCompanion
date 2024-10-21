using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using DynamicData;
using QBittorrent.Client;
using qBittorrentCompanion.Services;
using ReactiveUI;

namespace qBittorrentCompanion.ViewModels
{
    public class TorrentsViewModel : ViewModelBase
    {
        public static class TorrentStateGroupings
        {
            public static List<TorrentState> Paused
            {
                get => [TorrentState.PausedDownload, TorrentState.PausedUpload];
            }
            public static List<TorrentState> Seeding
            {
                get => [TorrentState.Uploading, TorrentState.QueuedUpload, TorrentState.StalledUpload, TorrentState.ForcedUpload];
            }
            public static List<TorrentState> Resumed
            {
                get => [
                    TorrentState.Uploading, TorrentState.QueuedUpload, TorrentState.StalledUpload,
                    TorrentState.ForcedUpload, TorrentState.Downloading, TorrentState.Uploading
                ];
            }
            public static List<TorrentState> Download
            {
                get => [TorrentState.Downloading, TorrentState.PausedDownload];
            }
            public static List<TorrentState> Active
            {
                get => [TorrentState.Downloading, TorrentState.Uploading];
            }
            public static List<TorrentState> InActive
            {
                get
                {
                    var activeStates = Active;
                    var allStates = Enum.GetValues(typeof(TorrentState)).Cast<TorrentState>().ToList();
                    return allStates.Except(activeStates).ToList();
                }
            }
            public static List<TorrentState> Stalled
            {
                get => [TorrentState.StalledUpload, TorrentState.StalledDownload];
            }
            public static List<TorrentState> StalledDownload
            {
                get => [TorrentState.StalledDownload];
            }
            public static List<TorrentState> StalledUpload
            {
                get => [TorrentState.StalledUpload];
            }
            public static List<TorrentState> Checking
            {
                get => [TorrentState.CheckingUpload, TorrentState.CheckingDownload];
            }
            public static List<TorrentState> Error
            {
                get => [TorrentState.Error, TorrentState.MissingFiles];
            }
            //Torrents.Count(t => t.State == "checkingDL" || t.State == "CheckingUP");
            //Torrents.Count(t => t.State == "error" || t.State == "missingFiles");
        }

        private bool _showStatusIcons = Design.IsDesignMode || ConfigService.ShowStatusIcons;
        public bool ShowStatusIcons
        {
            get => _showStatusIcons;
            set
            {
                if (value != _showStatusIcons)
                {
                    ConfigService.ShowStatusIcons = value;
                    this.RaiseAndSetIfChanged(ref _showStatusIcons, value);
                }
            }
        }

        private ObservableCollection<TorrentInfoViewModel> _filteredTorrents = [];
        public ObservableCollection<TorrentInfoViewModel> FilteredTorrents
        {
            get => _filteredTorrents;
            set => this.RaiseAndSetIfChanged(ref _filteredTorrents, value);
        }

        private TorrentInfoViewModel? _selectedTorrent;
        public TorrentInfoViewModel? SelectedTorrent
        {
            get => _selectedTorrent;
            set => this.RaiseAndSetIfChanged(ref _selectedTorrent, value);
        }

        private List<TorrentInfoViewModel> _selectedTorrents = [];
        public List<TorrentInfoViewModel> SelectedTorrents
        {
            get => _selectedTorrents;
            set => this.RaiseAndSetIfChanged(ref _selectedTorrents, value);
        }

        private ObservableCollection<TorrentInfoViewModel> _torrents = [];
        public ObservableCollection<TorrentInfoViewModel> Torrents
        {
            get => _torrents;
            set => this.RaiseAndSetIfChanged(ref _torrents, value);
        }

        private string _filterText = "";
        public string FilterText
        {
            get => _filterText;
            set { this.RaiseAndSetIfChanged(ref _filterText, value); }
        }

        private List<TorrentState> _filterStatuses = [];
        public List<TorrentState> FilterStatuses
        {
            get => _filterStatuses;
            set { this.RaiseAndSetIfChanged(ref _filterStatuses, value); }
        }

        private bool _filterCompleted = false;
        public bool FilterCompleted
        {
            get => _filterCompleted;
            set { this.RaiseAndSetIfChanged(ref _filterCompleted, value); }
        }

        private string _filterTag = "";
        public string FilterTag
        {
            get => _filterTag;
            set { this.RaiseAndSetIfChanged(ref _filterTag, value); }
        }

        private string _filterCategory = "";
        public string FilterCategory
        {
            get => _filterCategory;
            set { this.RaiseAndSetIfChanged(ref _filterCategory, value); }
        }

        private string _filterTracker = "";
        public string FilterTracker
        {
            get => _filterTracker;
            set { this.RaiseAndSetIfChanged(ref _filterTracker, value); }
        }

        private void UpdateFilteredTorrents()
        {
            // Apply most filters 
            var filtered = Torrents.Where(t =>
                (string.IsNullOrWhiteSpace(FilterText) || t.Name!.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) &&
                (FilterStatuses == null || FilterStatuses.Count == 0 || FilterStatuses.Contains((TorrentState)t.State!)) &&
                (!FilterCompleted || (t.Progress == 1)) &&
                (string.IsNullOrWhiteSpace(FilterTag) || (FilterTag == "Untagged" ? t.Tags?.Count == 0 : t.Tags!.Contains(FilterTag))) &&
                (string.IsNullOrWhiteSpace(FilterCategory) || (FilterCategory == "Uncategorised" ? string.IsNullOrEmpty(t.Category) : t.Category == FilterCategory))  // Check if FilterCategory is "Uncategorised" and Category is empty or if Category matches FilterCategory
            );

            // Then apply the FilterTracker filter
            if (FilterTracker == "Trackerless")
                filtered = filtered.Where(t => !_trackers.Values.Any(v => v.Contains(t.Hash)));
            else if (!string.IsNullOrWhiteSpace(FilterTracker) && FilterTracker != "All")
                filtered = filtered.Where(t => _trackers.ContainsKey(FilterTracker) && _trackers[FilterTracker].Contains(t.Hash));

            //filtered.ToList
            FilteredTorrents = new ObservableCollection<TorrentInfoViewModel>(filtered.ToList());
        }

        public TorrentsViewModel()
        {
            // Add name search filtering
            this.WhenAnyValue(x => x.FilterText)
                .Throttle(TimeSpan.FromMilliseconds(200))
                .Select(term => term?.Trim())
                .DistinctUntilChanged()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(term => {
                    UpdateFilteredTorrents();
                });

            this.WhenAnyValue(x => x.FilterStatuses)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(statuses => {
                    UpdateFilteredTorrents();
                });

            this.WhenAnyValue(x => x.FilterCompleted)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(completed => {
                    UpdateFilteredTorrents();
                });

            this.WhenAnyValue(x => x.FilterTag)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(tag => {
                    UpdateFilteredTorrents();
                });

            this.WhenAnyValue(x => x.FilterCategory)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(category => {
                    UpdateFilteredTorrents();
                });

            this.WhenAnyValue(x => x.FilterTracker)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(tracker => {
                    UpdateFilteredTorrents();
                });

            Torrents.CollectionChanged += Torrents_CollectionChanged;
            
            CategoryCounts.Add(new CategoryCountViewModel(new Category() { Name = "All" }) { IsEditable = false });
            CategoryCounts.Add(new CategoryCountViewModel(new Category() { Name = "Uncategorised" }) { IsEditable = false });

            TagCounts.Add(new TagCountViewModel("All") { Count = Torrents.Count, IsEditable = false });
            TagCounts.Add(new TagCountViewModel("Untagged") { Count = 0, IsEditable = false });

            TrackerCounts.Add(new TrackerCountViewModel("All", Torrents.Count));
            TrackerCounts.Add(new TrackerCountViewModel("Trackerless", Torrents.Count));

            if (Design.IsDesignMode)
            {
                CategoryCounts.Add(new CategoryCountViewModel(new Category() { Name = "Test Category", SavePath = "/somewhere/good" }));
                TagCounts.Add(new TagCountViewModel("Test tag"));
            }

            PauseCommand = ReactiveCommand.CreateFromTask(PauseSelectedTorrentsAsync);
            ResumeCommand = ReactiveCommand.CreateFromTask(ResumeSelectedTorrentsAsync);
            SetPriorityCommand = ReactiveCommand.CreateFromTask<TorrentPriorityChange>(SetPriorityForSelectedTorrentsAsync);

            RemoveUnusedCategoriesCommand = ReactiveCommand.CreateFromTask(RemoveUnusedCategoriesAsync);
            ResumeTorrentsForCategoryCommand = ReactiveCommand.CreateFromTask(ResumeTorrentsForCategoryAsync);
            PauseTorrentsForCategoryCommand = ReactiveCommand.CreateFromTask(PauseTorrentsForCategoryAsync);

            RemovedUnusedTagsCommand = ReactiveCommand.CreateFromTask(RemoveUnusedTagsAsync);
            ResumeTorrentsForTagCommand = ReactiveCommand.CreateFromTask(ResumeTorrentsForTagAsync);
            PauseTorrentsForTagCommand = ReactiveCommand.CreateFromTask(PauseTorrentsForTagAsync);

            PropertyChanged += TorrentsViewModel_PropertyChanged;

            if(Design.IsDesignMode)
            {
                Torrents.Add(new TorrentInfoViewModel(new TorrentPartialInfo() {
                    Name = "Ubuntu LTS",
                    State = TorrentState.Downloading,
                    Downloaded = 1000,
                    ConnectedSeeds = 5,
                    TotalSeeds = 100,
                    ConnectedLeechers = 8,
                    TotalLeechers = 54
                }, "533ASDAFAFDA232", new ObservableCollection<string>()));
            }
        }

        private void TorrentsViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(Tags))
            {
                foreach(var torrent in Torrents)
                {
                    torrent.Tags = Tags;
                }
            }
        }

        public ReactiveCommand<Unit, Unit> PauseCommand { get; }
        public ReactiveCommand<Unit, Unit> ResumeCommand { get; }
        public ReactiveCommand<TorrentPriorityChange, Unit> SetPriorityCommand { get; }
        public ReactiveCommand<Unit, Unit> RemoveUnusedCategoriesCommand { get; }
        public ReactiveCommand<Unit, Unit> ResumeTorrentsForCategoryCommand { get; }
        public ReactiveCommand<Unit, Unit> PauseTorrentsForCategoryCommand { get; }
        public ReactiveCommand<Unit, Unit> RemovedUnusedTagsCommand { get; }
        public ReactiveCommand<Unit, Unit> ResumeTorrentsForTagCommand { get; }
        public ReactiveCommand<Unit, Unit> PauseTorrentsForTagCommand { get; }

        public bool SelectedTorrentIsPaused
        {
            get
            {
                return SelectedTorrent?.IsPaused ?? false;
            }
        }

        private void Torrents_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            bool updateTagCounts = false, updateCategoryCounts = false;

            if (e.NewItems is not null)
            {
                foreach (TorrentInfoViewModel item in e.NewItems)
                {
                    item.WhenAnyValue(x => x.Progress).Subscribe(_ => UpdateCompletedCount());
                    item.WhenAnyValue(x => x.State).Subscribe(_ => UpdatePausedCount());
                    item.WhenAnyValue(x => x.State).Subscribe(_ => UpdateSeedingCount());
                    item.WhenAnyValue(x => x.State).Subscribe(_ => UpdateResumedCount());
                    item.WhenAnyValue(x => x.State).Subscribe(_ => UpdateDownloadingCount());
                    item.WhenAnyValue(x => x.State).Subscribe(_ => UpdateActiveCount());
                    item.WhenAnyValue(x => x.State).Subscribe(_ => UpdateInactiveCount());
                    item.WhenAnyValue(x => x.State).Subscribe(_ => UpdateStalledCount());
                    item.WhenAnyValue(x => x.State).Subscribe(_ => UpdateStalledUploadingCount());
                    item.WhenAnyValue(x => x.State).Subscribe(_ => UpdateStalledDownloadingCount());
                    item.WhenAnyValue(x => x.State).Subscribe(_ => UpdateCheckingCount());
                    item.WhenAnyValue(x => x.State).Subscribe(_ => UpdateErrorCount());
                    item.WhenAnyValue(x => x.Tags).Subscribe(_ => updateTagCounts = true);
                    item.WhenAnyValue(x => x.Category).Subscribe(_ => updateCategoryCounts = true);
                }
            }
            if (e.OldItems is not null)
            {
                foreach (TorrentInfoViewModel item in e.OldItems)
                {
                    // If memory leaks occur this would be the place to unsubscribe things
                }
            }
            UpdateCompletedCount();
            UpdatePausedCount();
            UpdateSeedingCount();
            UpdateResumedCount();
            UpdateDownloadingCount();
            UpdateActiveCount();
            UpdateInactiveCount();
            UpdateStalledCount();
            UpdateStalledUploadingCount();
            UpdateStalledDownloadingCount();
            UpdateCheckingCount();
            UpdateErrorCount();
            UpdateTagCountsAllTag();
            if (updateTagCounts)
                UpdateTagCounts();
            if (updateCategoryCounts)
                UpdateCategoryCounts();

            UpdateFilteredTorrents(); //If Torrents changed, so should FilteredTorrents
        }

        private void UpdateTagCounts()
        {
            Dictionary<string, int> tagCounts = [];

            foreach (var torrent in Torrents)
            {
                if (torrent.Tags is not null)
                    foreach (var tag in torrent.Tags)
                    {
                        if (tagCounts.ContainsKey(tag))
                            tagCounts[tag]++;
                        else
                            tagCounts[tag] = 1;
                    }
            }

            foreach (KeyValuePair<string, int> tagCountPair in tagCounts)
            {
                //Debug.WriteLine($"{tagCountPair.Key} : {tagCountPair.Value}");
                var tagCount = TagCounts.FirstOrDefault<TagCountViewModel>(tc => tc.Tag == tagCountPair.Key);
                if (tagCount is not null)
                {
                    tagCount.Count = tagCountPair.Value;
                }
            }

            var untaggedTagCount = TagCounts.FirstOrDefault<TagCountViewModel>(tc => tc.Tag == "Untagged");
            if (untaggedTagCount is not null)
                untaggedTagCount.Count = Torrents.Count(t => t.Tags is null || t.Tags.Count == 0);

            this.RaisePropertyChanged(nameof(TagCounts));
        }

        private void UpdateCategoryCounts()
        {
            Dictionary<string, int> categoryCounts = [];
            foreach (var torrent in Torrents)
            {
                if (torrent.Category is not null && categoryCounts.ContainsKey(torrent.Category))
                    categoryCounts[torrent.Category]++;
                else
                    categoryCounts[torrent?.Category ?? ""] = 1;
            }

            // Rename "" to Uncategorised
            /*categoryCounts["Uncategorised"] = categoryCounts[""];
            categoryCounts.Remove("");*/

            //Count all
            categoryCounts["All"] = Torrents.Count;

            foreach (KeyValuePair<string, int> categoryCountPair in categoryCounts)
            {
                var categoryCount = CategoryCounts.FirstOrDefault(cc => cc.Name == categoryCountPair.Key);
                if (categoryCount is not null)
                    categoryCount.Count = categoryCountPair.Value;
                /*else
                    Debug.WriteLine($"Somehow category {categoryCountPair.Key} was counted but does not exist.");*/
            }

            this.RaisePropertyChanged(nameof(categoryCounts));
        }


        /** 
        * Updates the "All" tags count by counting all the torrents.
        * (To be called if the torrent collection changes)
        */
        private void UpdateTagCountsAllTag()
        {
            var tagCount = TagCounts.FirstOrDefault(t => t.Tag == "All");
            if (tagCount != null)
                tagCount.Count = Torrents.Count;
        }

        private void UpdateCompletedCount()
        {
            CompletedCount = Torrents.Count(t => t.Progress == 1);
        }

        private TorrentPropertiesViewModel? _propertiesForSelectedTorrent;
        public TorrentPropertiesViewModel? PropertiesForSelectedTorrent
        {
            get => _propertiesForSelectedTorrent;
            set => this.RaiseAndSetIfChanged(ref _propertiesForSelectedTorrent, value);
        }

        private TorrentPieceStatesViewModel? _torrentPieceStatesViewModel;
        public TorrentPieceStatesViewModel? TorrentPieceStatesViewModel
        {
            get => _torrentPieceStatesViewModel;
            set => this.RaiseAndSetIfChanged(ref _torrentPieceStatesViewModel, value);
        }

        private ObservableCollection<string> _tags = [];
        public ObservableCollection<string> Tags
        {
            get => _tags;
            set => this.RaiseAndSetIfChanged(ref _tags, value);
        }

        private ObservableCollection<TagCountViewModel> _tagCounts = [];
        public ObservableCollection<TagCountViewModel> TagCounts
        {
            get => _tagCounts;
            set => this.RaiseAndSetIfChanged(ref _tagCounts, value);
        }

        /// <summary>
        /// Note: Updates <see cref="TagCounts"/> and <see cref="Tags"/>
        /// </summary>
        /// <param name="tags"></param>
        public void UpdateTags(IReadOnlyList<string>? tags)
        {
            if (tags is null)
                return;

            var newTagCounts = tags.Where(tag => !TagCounts.Any(t => t.Tag == tag))
                .Select(tag => new TagCountViewModel(tag))
                .ToList();

            if (newTagCounts.Any())
            {
                TagCounts.AddRange(newTagCounts);
                this.RaisePropertyChanged(nameof(TagCounts));
            }

            var newTags = tags.Where(t => !Tags.Contains(t));
            if (newTags.Any())
            {
                Tags.AddRange(newTags);
                this.RaisePropertyChanged(nameof(Tags));
            }
        }

        public void RemoveTags(IReadOnlyList<string>? tagsRemoved)
        {
            if (tagsRemoved is null)
                return;

            //.ToList() gets around lazy evaluation / InvalidOperationException
            var toRemoveItems = TagCounts.Where(t => tagsRemoved.Contains(t.Tag)).ToList();
            TagCounts.Remove(toRemoveItems);

            if (toRemoveItems.Count() > 0)
                this.RaisePropertyChanged(nameof(TagCounts));
        }

        private ObservableCollection<Category> _categories = [];
        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set
            {
                this.RaiseAndSetIfChanged(ref _categories, value);
                SyncTorrentInfoViewModelCategories();
            }
        }

        private ObservableCollection<CategoryCountViewModel> _categoryCounts = [];
        public ObservableCollection<CategoryCountViewModel> CategoryCounts
        {
            get => _categoryCounts;
            set => this.RaiseAndSetIfChanged(ref _categoryCounts, value);
        }

        /// <summary>
        /// Note: This updates both <see cref="CategoryCounts"/> and the categories of <see cref="Torrents"/> items
        /// </summary>
        /// <param name="categories"></param>
        public void UpdateCategories(IReadOnlyDictionary<string, Category> categories)
        {
            if (categories is null)
                return;

            this.RaisePropertyChanging(nameof(CategoryCounts));
            foreach (var category in categories)
            {
                var existingCc = CategoryCounts.FirstOrDefault(c => c.Name == category.Key);
                if (!CategoryCounts.Any(c => c.Name == category.Key))
                    CategoryCounts.Add(new CategoryCountViewModel(category.Value));

                if (!Categories.Any(c => c.Name == category.Key))
                    Categories.Add(category.Value);

                //else
                //    CategoryCounts.Update();
            }
            SyncTorrentInfoViewModelCategories();

            this.RaisePropertyChanged(nameof(CategoryCounts));
        }

        public void SyncTorrentInfoViewModelCategories()
        {
            foreach (var torrent in Torrents)
            {
                torrent.Categories = Categories;
            }
        }

        public void RemoveCategories(IReadOnlyList<string>? categoriesRemoved)
        {
            if (categoriesRemoved is null)
                return;

            //.ToList() gets around lazy evaluation / InvalidOperationException 
            var toRemoveItems = CategoryCounts.Where(c => categoriesRemoved.Contains(c.Name)).ToList();
            CategoryCounts.Remove(toRemoveItems);

            if (toRemoveItems.Count() > 0)
                this.RaisePropertyChanged(nameof(CategoryCounts));
        }

        private Dictionary<string, string[]> _trackers = [];

        private ObservableCollection<TrackerCountViewModel> _trackerCounts = [];
        public ObservableCollection<TrackerCountViewModel> TrackerCounts
        {
            get => _trackerCounts;
            set => this.RaiseAndSetIfChanged(ref _trackerCounts, value);
        }
        public void UpdateTrackers(Newtonsoft.Json.Linq.JToken trackers)
        {
            this.RaisePropertyChanged(nameof(TrackerCounts));

            foreach (var tracker in trackers)
            {
                var property = tracker as Newtonsoft.Json.Linq.JProperty;
                var array = property!.Value as Newtonsoft.Json.Linq.JArray;

                TrackerCounts.Add(new TrackerCountViewModel(property.Name, array!.Count));
            }

            var all = TrackerCounts.FirstOrDefault<TrackerCountViewModel>(t => t.Url == "All");
            if (all != null)
                all.Count = Torrents.Count;

            foreach (var tracker in trackers)
            {
                var property = tracker as Newtonsoft.Json.Linq.JProperty;
                var array = property!.Value as Newtonsoft.Json.Linq.JArray;

                if (_trackers.ContainsKey(property.Name))
                {
                    if (!array!.ToObject<string[]>()!.SequenceEqual(_trackers[property.Name]))
                    { // Updates to new values
                        _trackers[property.Name] = array.ToObject<string[]>()!;
                    }
                }
                else
                { // Initialises. 
                    _trackers[property.Name] = array!.ToObject<string[]>()!;
                }

                UpdateTrackerless();
            }

            this.RaisePropertyChanged(nameof(TrackerCounts));
        }
        private void UpdateTrackerless()
        {

            var trackerless = TrackerCounts.FirstOrDefault<TrackerCountViewModel>(t => t.Url == "Trackerless");
            if (trackerless != null)
                trackerless.Count = 0;
        }

        private int _downloadingCount;
        public int DownloadingCount
        {
            get => _downloadingCount;
            set => this.RaiseAndSetIfChanged(ref _downloadingCount, value);
        }

        private int _completedCount;
        public int CompletedCount
        {
            get => _completedCount;
            set => this.RaiseAndSetIfChanged(ref _completedCount, value);
        }

        private int _pausedCount;
        public int PausedCount
        {
            get => _pausedCount;
            set => this.RaiseAndSetIfChanged(ref _pausedCount, value);
        }
        private void UpdatePausedCount()
        {
            PausedCount = Torrents.Count(t => t.State is not null && TorrentStateGroupings.Paused.Contains((TorrentState)t.State));
        }

        private int _seedingCount;
        public int SeedingCount
        {
            get => _seedingCount;
            set => this.RaiseAndSetIfChanged(ref _seedingCount, value);
        }
        private void UpdateSeedingCount()
        {
            SeedingCount = Torrents.Count(t => t.State is not null && TorrentStateGroupings.Seeding.Contains((TorrentState)t.State));
        }

        private int _resumedCount;
        public int ResumedCount
        {
            get => _resumedCount;
            set => this.RaiseAndSetIfChanged(ref _resumedCount, value);
        }
        private void UpdateResumedCount()
        {
            ResumedCount = Torrents.Count(t => t.State is not null && TorrentStateGroupings.Resumed.Contains((TorrentState)t.State));
        }

        private void UpdateDownloadingCount()
        {

            DownloadingCount = Torrents.Count(t => t.State is not null && TorrentStateGroupings.Download.Contains((TorrentState)t.State));
        }

        private int _activeCount;
        public int ActiveCount
        {
            get => _activeCount;
            set => this.RaiseAndSetIfChanged(ref _activeCount, value);
        }
        private void UpdateActiveCount()
        {
            ActiveCount = Torrents.Count(t => t.State is not null && TorrentStateGroupings.Active.Contains((TorrentState)t.State));
        }

        private int _inactiveCount;
        public int InactiveCount
        {
            get => _inactiveCount;
            set => this.RaiseAndSetIfChanged(ref _inactiveCount, value);
        }
        private void UpdateInactiveCount()
        {
            InactiveCount = Torrents.Count - ActiveCount;
        }

        private int _stalledCount;
        public int StalledCount
        {
            get => _stalledCount;
            set => this.RaiseAndSetIfChanged(ref _stalledCount, value);
        }
        private void UpdateStalledCount()
        {
            StalledCount = Torrents.Count(t => t.State is not null && TorrentStateGroupings.Stalled.Contains((TorrentState)t.State));
        }

        private int _stalledUploadingCount;
        public int StalledUploadingCount
        {
            get => _stalledUploadingCount;
            set => this.RaiseAndSetIfChanged(ref _stalledUploadingCount, value);
        }
        private void UpdateStalledUploadingCount()
        {
            StalledUploadingCount = Torrents.Count(t => t.State is not null && TorrentStateGroupings.StalledUpload.Contains((TorrentState)t.State));
        }

        private int _stalledDownloadingCount;
        public int StalledDownloadingCount
        {
            get => _stalledDownloadingCount;
            set => this.RaiseAndSetIfChanged(ref _stalledDownloadingCount, value);
        }
        private void UpdateStalledDownloadingCount()
        {
            StalledDownloadingCount = Torrents.Count(t => t.State is not null && TorrentStateGroupings.StalledDownload.Contains((TorrentState)t.State));
        }

        private int _checkingCount;
        public int CheckingCount
        {
            get => _checkingCount;
            set => this.RaiseAndSetIfChanged(ref _checkingCount, value);
        }
        private void UpdateCheckingCount()
        {
            CheckingCount = Torrents.Count(t => t.State is not null && TorrentStateGroupings.Checking.Contains((TorrentState)t.State));
        }

        private int _errorCount;
        public int ErrorCount
        {
            get => _errorCount;
            set => this.RaiseAndSetIfChanged(ref _errorCount, value);
        }

        private TorrentTrackersViewModel? _torrentTrackersViewModel;
        public TorrentTrackersViewModel? TorrentTrackersViewModel 
        {
            get => _torrentTrackersViewModel;
            set => this.RaiseAndSetIfChanged(ref _torrentTrackersViewModel, value);
        }

        public void PauseTrackers()
        {
            _torrentTrackersViewModel?.Pause();
        }

        private TorrentPeersViewModel? _torrentPeersViewModel;
        public TorrentPeersViewModel? TorrentPeersViewModel
        {
            get => _torrentPeersViewModel;
            set => this.RaiseAndSetIfChanged(ref _torrentPeersViewModel, value);
        }

        public void PausePeers()
        {
            _torrentPeersViewModel?.Pause();
        }

        private TorrentHttpSourcesViewModel? _httpSourcesViewModel;
        public TorrentHttpSourcesViewModel? TorrentHttpSourcesViewModel
        {
            get => _httpSourcesViewModel;
            set => this.RaiseAndSetIfChanged(ref _httpSourcesViewModel, value);
        }

        public void PauseHttpSources()
        {
            _httpSourcesViewModel?.Pause();
        }

        private TorrentContentsViewModel? _torrentContentsViewModel;
        public TorrentContentsViewModel? TorrentContentsViewModel
        {
            get => _torrentContentsViewModel;
            set => this.RaiseAndSetIfChanged(ref _torrentContentsViewModel, value);
        }

        public void PauseTorrentContents()
        {
            _torrentContentsViewModel?.Pause();
        }

        private void UpdateErrorCount()
        {
            ErrorCount = Torrents.Count(t => t.State is not null && TorrentStateGroupings.Error.Contains((TorrentState)t.State));
        }

        private IEnumerable<string> SelectedHashes => SelectedTorrents.Select(st => st.Hash!);
        private bool TorrentsSelected => SelectedTorrents is not null && SelectedTorrents.Count > 0;

        public async Task PauseSelectedTorrentsAsync()
        {
            if (TorrentsSelected)
                await QBittorrentService.QBittorrentClient.PauseAsync(SelectedHashes);
        }

        public async Task ResumeSelectedTorrentsAsync()
        {
            if (TorrentsSelected)
                await QBittorrentService.QBittorrentClient.ResumeAsync(SelectedHashes);
        }

        public async Task SetPriorityForSelectedTorrentsAsync(TorrentPriorityChange newPriority)
        {
            if (TorrentsSelected)
                await QBittorrentService.QBittorrentClient.ChangeTorrentPriorityAsync(SelectedHashes, newPriority);
        }

        public async Task RemoveUnusedCategoriesAsync()
        {
            var categoriesInUse = Torrents.Select(t => t.Category);
            var unusedCategories = Categories
                .Where(c => !categoriesInUse.Contains(c.Name))
                .Select(c => c.Name);

            try
            {
                await QBittorrentService.QBittorrentClient.DeleteCategoriesAsync(unusedCategories);
            }
            catch(Exception ex) 
            {
                Debug.WriteLine(ex.Message);
            }
        }
        public async Task RemoveUnusedTagsAsync()
        {
            var tagsInUse = Torrents.SelectMany(t => t.Tags!).ToList();
            var unusedTags = Tags.Where(t => !tagsInUse.Contains(t));

            try
            {
                await QBittorrentService.QBittorrentClient.DeleteTagsAsync(unusedTags);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private IEnumerable<TorrentInfoViewModel> TorrentsForCurrentCategory()
        {
            return Torrents
                .Where(t => t.Category == FilterCategory && FilterCategory != string.Empty);
        }

        
        public async Task ResumeTorrentsForCategoryAsync()
        {
            try
            {
                await QBittorrentService.QBittorrentClient.ResumeAsync(
                    TorrentsForCurrentCategory().Select(t => t.Hash)
                );
            }
            catch(Exception e) { Debug.WriteLine(e.Message); }
        }

        public async Task PauseTorrentsForCategoryAsync()
        {
            try
            {
                await QBittorrentService.QBittorrentClient.PauseAsync(
                    TorrentsForCurrentCategory().Select(t => t.Hash)
                );
            }
            catch (Exception e) { Debug.WriteLine(e.Message); }
        }

        public async Task DeleteTorrentsForCategoryAsync(bool deleteFiles = false)
        {
            await DeleteTorrentsAsync(TorrentsForCurrentCategory(), deleteFiles);
        }

        private IEnumerable<TorrentInfoViewModel> TorrentsForCurrentTag()
        {
            return Torrents
                .Where(t => t.Tags != null && t.Tags.Contains(FilterTag));
        }
        public async Task ResumeTorrentsForTagAsync()
        {
            try
            {
                await QBittorrentService.QBittorrentClient.ResumeAsync(
                    TorrentsForCurrentTag().Select(t => t.Hash)
                );
            }
            catch (Exception e) { Debug.WriteLine(e.Message); }
        }

        public async Task PauseTorrentsForTagAsync()
        {
            try
            {
                await QBittorrentService.QBittorrentClient.PauseAsync(
                    TorrentsForCurrentTag().Select(t => t.Hash)
                );
            }
            catch (Exception e) { Debug.WriteLine(e.Message); }
        }

        public async Task DeleteTorrentsForTagAsync(bool deleteFiles = false)
        {
            await DeleteTorrentsAsync(TorrentsForCurrentTag(), deleteFiles);
        }


        /// <summary>
        /// <list type="number">
        ///     <item>To prevent updates from going awry on non existing items and making the GUI snappier
        ///         <list type="bullet">
        ///             <item>Sets <see cref="SelectedTorrent">SelectedTorrent</see> to <c>null</c> </item>
        ///             <item>Removes the torrents to delete from <see cref="SelectedTorrents">SelectedTorrents</see></item>
        ///         </list>
        ///     </item>
        /// </list>
        /// 
        /// </summary>
        /// <param name="deleteFiles"></param>
        /// <returns></returns>
        public async Task DeleteSelectedTorrentsAsync(bool deleteFiles = false)
        {
            if (TorrentsSelected)
            {
                await DeleteTorrentsAsync(SelectedTorrents);
            }
        }

        private async Task DeleteTorrentsAsync(IEnumerable<TorrentInfoViewModel> torrents, bool deleteFiles = false)
        {
            var selectedHashes = torrents.Select(t => t.Hash);
            if(torrents.Contains(SelectedTorrent))
                SelectedTorrent = null;
            SelectedTorrents.RemoveMany(torrents);
            Torrents.RemoveMany(torrents);

            try
            {
                await QBittorrentService.QBittorrentClient.DeleteAsync(selectedHashes, deleteFiles);
            }
            catch (Exception e) { Debug.WriteLine(e.Message); } 
        }

        public void RemoveTorrents(IReadOnlyList<String>? infoHashes)
        {
            if (infoHashes is null)
                return;

            if (SelectedTorrent is not null && infoHashes.Contains(SelectedTorrent.Hash))
                SelectedTorrent = null;

            // Iterate in reverse to avoid indice issues 
            for (int i = Torrents.Count - 1; i >= 0; i--)
            {
                TorrentInfoViewModel torrent = Torrents[i];
                if (infoHashes.Contains(torrent.Hash))
                {
                    SelectedTorrents.Remove(torrent);
                    Torrents.RemoveAt(i);
                }
            }
        }

        public void AddTorrent(TorrentPartialInfo torrentInfo, string key)
        {
            Torrents.Add(new TorrentInfoViewModel(torrentInfo, key, Tags));
        }
    }
}