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
        }

        private bool _showSideBar = Design.IsDesignMode || ConfigService.ShowSideBar;
        public bool ShowSideBar
        {
            get => _showSideBar;
            set
            {
                if (value != _showSideBar)
                {
                    ConfigService.ShowSideBar = value;
                    _showSideBar = value;
                    this.RaisePropertyChanged(nameof(ShowSideBar));
                }
            }
        }

        private bool _showPeersPausePlay = false;

        private bool _showSideBarStatusIcons = Design.IsDesignMode || ConfigService.ShowSideBarStatusIcons;
        public bool ShowSideBarStatusIcons
        {
            get => _showSideBarStatusIcons;
            set
            {
                if (value != _showSideBarStatusIcons)
                {
                    ConfigService.ShowSideBarStatusIcons = value;
                    this.RaiseAndSetIfChanged(ref _showSideBarStatusIcons, value);
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

        private StatusCountViewModel? _filterStatus = null;
        public StatusCountViewModel? FilterStatus
        {
            get => _filterStatus;
            set 
            {
                if (value != FilterStatus)
                {
                    FilterCompleted = value != null && value.Name == "Completed";
                    this.RaiseAndSetIfChanged(ref _filterStatus, value);
                }
            }
        }

        private bool _isUsingNonTextFilter = false;
        public bool IsUsingNonTextFilter
        {
            get => _isUsingNonTextFilter;
            set { this.RaiseAndSetIfChanged(ref _isUsingNonTextFilter, value); }
        }

        private bool _filterCompleted = false;
        public bool FilterCompleted
        {
            get => _filterCompleted;
            set { this.RaiseAndSetIfChanged(ref _filterCompleted, value); }
        }

        private TagCountViewModel? _filterTag = null;
        public TagCountViewModel? FilterTag
        {
            get => _filterTag;
            set { this.RaiseAndSetIfChanged(ref _filterTag, value); }
        }

        private CategoryCountViewModel? _filterCategory = null;
        public CategoryCountViewModel? FilterCategory
        {
            get => _filterCategory;
            set { this.RaiseAndSetIfChanged(ref _filterCategory, value); }
        }

        private TrackerCountViewModel? _filterTracker;
        public TrackerCountViewModel? FilterTracker
        {
            get => _filterTracker;
            set { this.RaiseAndSetIfChanged(ref _filterTracker, value); }
        }

        private void UpdateFilteredTorrents()
        {
            // Apply most filters 
            var filtered = Torrents.Where(t =>
                (string.IsNullOrWhiteSpace(FilterText) || t.Name!.Contains(FilterText, StringComparison.OrdinalIgnoreCase))
                &&  (FilterStatus == null || FilterStatus.TorrentStates.Count == 0 || FilterStatus.TorrentStates.Contains((TorrentState)t.State!))
                && (!FilterCompleted || (t.Progress == 1))
                && (FilterTag == null || FilterTag.Tag == "All" || (FilterTag.Tag == "Untagged" ? t.Tags?.Count == 0 : t.Tags!.Contains(FilterTag.Tag)))
                 // If filtercategory is not set or "All" return all || If it's uncategorised grab all without a category or grab all belong to category
                && (FilterCategory == null || FilterCategory.Name == "All" || (FilterCategory.Name == "Uncategorised" ? string.IsNullOrEmpty(t.Category) : t.Category == FilterCategory.Name)) 
            );

            // Then apply the FilterTracker filter
            if (FilterTracker != null && FilterTracker.DisplayUrl == "Trackerless")
                filtered = filtered.Where(t => !_trackers.Values.Any(v => v.Contains(t.Hash)));
            else if (FilterTracker != null && !string.IsNullOrWhiteSpace(FilterTracker.DisplayUrl) && FilterTracker.DisplayUrl != "All")
            {
                foreach(var t in _trackers)
                    Debug.WriteLine($"{t.Key == FilterTracker.Url } : {t.Key}");
                filtered = filtered.Where(t => _trackers.ContainsKey(FilterTracker.Url) && _trackers[FilterTracker.Url].Contains(t.Hash));
            }

            //Debug.WriteLine(FilterStatus != null && FilterStatus != StatusCounts[0]);

            IsUsingNonTextFilter = (FilterStatus != null && FilterStatus != StatusCounts[0])
                || (FilterCategory != null && FilterCategory != CategoryCounts[0])
                || (FilterTag != null && FilterTag != TagCounts[0])
                || (FilterTracker != null && FilterTracker != TrackerCounts[0]);

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

            this.WhenAnyValue(x => x.FilterStatus)
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
            
            StatusCounts.Add(new StatusCountViewModel("All", FluentIcons.Common.Symbol.LineHorizontal1, [])); // 0
            StatusCounts.Add(new StatusCountViewModel("Downloading", FluentIcons.Common.Symbol.ArrowDownload, TorrentStateGroupings.Download)); // 1
            StatusCounts.Add(new StatusCountViewModel("Seeding", FluentIcons.Common.Symbol.ArrowUpload, TorrentStateGroupings.Seeding)); // 2
            StatusCounts.Add(new StatusCountViewModel("Completed", FluentIcons.Common.Symbol.CheckmarkCircle, [])); // 3
            StatusCounts.Add(new StatusCountViewModel("Resumed", FluentIcons.Common.Symbol.PlayCircle, TorrentStateGroupings.Resumed)); // 4
            StatusCounts.Add(new StatusCountViewModel("Paused", FluentIcons.Common.Symbol.PauseCircle, TorrentStateGroupings.Paused)); // 5
            StatusCounts.Add(new StatusCountViewModel("Active", FluentIcons.Common.Symbol.ShiftsActivity, TorrentStateGroupings.Active)); // 6
            StatusCounts.Add(new StatusCountViewModel("Inactive", FluentIcons.Common.Symbol.History, TorrentStateGroupings.InActive)); // 7
            StatusCounts.Add(new StatusCountViewModel("Stalled", FluentIcons.Common.Symbol.ArrowSyncDismiss, TorrentStateGroupings.Stalled)); // 8
            StatusCounts.Add(new StatusCountViewModel("Stalled downloading", FluentIcons.Common.Symbol.ArrowCircleDown, TorrentStateGroupings.StalledDownload)); // 9
            StatusCounts.Add(new StatusCountViewModel("Stalled uploading", FluentIcons.Common.Symbol.ArrowCircleUp, TorrentStateGroupings.StalledUpload)); // 10
            StatusCounts.Add(new StatusCountViewModel("Checking", FluentIcons.Common.Symbol.ArrowSyncCircle, TorrentStateGroupings.Checking)); // 11
            StatusCounts.Add(new StatusCountViewModel("Errored", FluentIcons.Common.Symbol.ErrorCircle, TorrentStateGroupings.Error)); // 12
            FilterStatus = StatusCounts[0];

            CategoryCounts.Add(new CategoryCountViewModel(new Category() { Name = "All" }) { IsEditable = false });
            CategoryCounts.Add(new CategoryCountViewModel(new Category() { Name = "Uncategorised" }) { IsEditable = false });
            FilterCategory = CategoryCounts[0];

            TagCounts.Add(new TagCountViewModel("All") { Count = Torrents.Count, IsEditable = false });
            TagCounts.Add(new TagCountViewModel("Untagged") { Count = 0, IsEditable = false });
            FilterTag = TagCounts[0];
            

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

            ResumeTorrentsForTrackerCommand = ReactiveCommand.CreateFromTask(ResumeTorrentsForTrackerAsync);
            PauseTorrentsForTrackerCommand = ReactiveCommand.CreateFromTask(PauseTorrentsForTrackerAsync);
            //DeleteTorrentsForTrackerCommand = ReactiveCommand.CreateFromTask(DeleteTorrentsForTrackerAsync);

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
        public ReactiveCommand<Unit, Unit> ResumeTorrentsForTrackerCommand { get; }
        public ReactiveCommand<Unit, Unit> PauseTorrentsForTrackerCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteTorrentsForTrackerCommand { get; }
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
            StatusCounts[0].Count = Torrents.Count;
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
            var categoryCounts = new Dictionary<string, int>() { { "Uncategorised", 0 } };
            foreach (var torrent in Torrents)
            {
                if (torrent.Category is not null)
                {
                    if (categoryCounts.ContainsKey(torrent.Category))
                        categoryCounts[torrent.Category]++;
                    else if (torrent.Category == string.Empty)
                        categoryCounts["Uncategorised"]++;
                    else
                        categoryCounts[torrent?.Category ?? ""] = 1;
                }
            }

            //Count all
            CategoryCounts[0].Count = Torrents.Count;

            foreach (KeyValuePair<string, int> categoryCountPair in categoryCounts)
            {
                var categoryCount = CategoryCounts.FirstOrDefault(cc => cc.Name == categoryCountPair.Key);
                if (categoryCount is not null)
                    categoryCount.Count = categoryCountPair.Value;
               else
                    Debug.WriteLine($"Somehow category {categoryCountPair.Key} was counted but does not exist.");
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

        private ObservableCollection<StatusCountViewModel> _statusCounts = [];
        public ObservableCollection<StatusCountViewModel> StatusCounts
        {
            get => _statusCounts;
            set => this.RaiseAndSetIfChanged(ref _statusCounts, value);
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
        private void UpdateDownloadingCount()
        {

            StatusCounts[1].Count = Torrents.Count(t => t.State is not null && TorrentStateGroupings.Download.Contains((TorrentState)t.State));
        }

        private void UpdateSeedingCount()
        {
            StatusCounts[2].Count = Torrents.Count(t => t.State is not null && TorrentStateGroupings.Seeding.Contains((TorrentState)t.State));
        }
        private void UpdateCompletedCount()
        {
            StatusCounts[3].Count = Torrents.Count(t => t.Progress == 1);
        }

        private void UpdateResumedCount()
        {
            StatusCounts[4].Count = Torrents.Count(t => t.State is not null && TorrentStateGroupings.Resumed.Contains((TorrentState)t.State));
        }
        private void UpdatePausedCount()
        {
            StatusCounts[5].Count = Torrents.Count(t => t.State is not null && TorrentStateGroupings.Paused.Contains((TorrentState)t.State));
        }

        private void UpdateActiveCount()
        {
            StatusCounts[6].Count = Torrents.Count(t => t.State is not null && TorrentStateGroupings.Active.Contains((TorrentState)t.State));
        }

        private int _inactiveCount;
        public int InactiveCount
        {
            get => _inactiveCount;
            set => this.RaiseAndSetIfChanged(ref _inactiveCount, value);
        }
        private void UpdateInactiveCount()
        {
            StatusCounts[7].Count = Torrents.Count - StatusCounts[6].Count;
        }

        private void UpdateStalledCount()
        {
            StatusCounts[8].Count = Torrents.Count(t => t.State is not null && TorrentStateGroupings.Stalled.Contains((TorrentState)t.State));
        }

        private void UpdateStalledDownloadingCount()
        {
            StatusCounts[9].Count = Torrents.Count(t => t.State is not null && TorrentStateGroupings.StalledDownload.Contains((TorrentState)t.State));
        }

        private void UpdateStalledUploadingCount()
        {
            StatusCounts[10].Count = Torrents.Count(t => t.State is not null && TorrentStateGroupings.StalledUpload.Contains((TorrentState)t.State));
        }

        private void UpdateCheckingCount()
        {
            StatusCounts[11].Count = Torrents.Count(t => t.State is not null && TorrentStateGroupings.Checking.Contains((TorrentState)t.State));
        }

        private void UpdateErrorCount()
        {
            StatusCounts[12].Count = Torrents.Count(t => t.State is not null && TorrentStateGroupings.Error.Contains((TorrentState)t.State));
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
            return FilterCategory == null || FilterCategory.Name == "All"
                ? []  
                : Torrents
                    .Where(t => t.Category == FilterCategory.Name);
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
                .Where(t => t.Tags != null && t.Tags.Contains(FilterTag == null ? "" : FilterTag.Tag));
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

        private IEnumerable<string> TorrentHashesForCurrentTracker()
        {
            return _trackers[FilterTracker!.DisplayUrl];
        }

        public async Task ResumeTorrentsForTrackerAsync()
        {
            try { await QBittorrentService.QBittorrentClient.ResumeAsync(TorrentHashesForCurrentTracker()); }
            catch (Exception e) { Debug.WriteLine(e.Message); }
        }

        public async Task PauseTorrentsForTrackerAsync()
        {
            try { await QBittorrentService.QBittorrentClient.PauseAsync(TorrentHashesForCurrentTracker()); }
            catch (Exception e) { Debug.WriteLine(e.Message); }
        }

        public async Task DeleteTorrentsForTrackerAsync(bool deleteFiles = false)
        {
            var torrentHashesForCurrentTracker = TorrentHashesForCurrentTracker();
            var torrentsForCurrentTracker = Torrents.Where(t => torrentHashesForCurrentTracker.Contains(t.Hash));

            await DeleteTorrentsAsync(torrentsForCurrentTracker, deleteFiles);
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
                await DeleteTorrentsAsync(SelectedTorrents, deleteFiles);
            }
        }

        private async Task DeleteTorrentsAsync(IEnumerable<TorrentInfoViewModel> torrents, bool deleteFiles = false)
        {
            // ToList prevents deffered execution and ensures the RemoveMany later don't make it come up empty.
            var selectedHashes = torrents.Select(t => t.Hash).ToList();

            // If the torrent is the selected torrent - undo the selection
            if(torrents.Contains(SelectedTorrent))
                SelectedTorrent = null;
            SelectedTorrents.RemoveMany(torrents);

            // Remove the to be deleted torrents from Torrents and FilteredTorrent so the UI updates instantly.
            // They should automatically be added back should the actual delete request fail.
            Torrents.RemoveMany(torrents);
            FilteredTorrents.RemoveMany(torrents);

            try{ await QBittorrentService.QBittorrentClient.DeleteAsync(selectedHashes, deleteFiles); }
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