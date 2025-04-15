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
    public class TorrentsViewModel : RssPluginSupportBaseViewModel
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

        private int _lastSelectedTorrentsSubTabIndex = Design.IsDesignMode ? 0 : ConfigService.LastSelectedTorrentsSubTabIndex;
        public int LastSelectedTorrentsSubTabIndex
        {
            get => _lastSelectedTorrentsSubTabIndex;
            set
            {
                if (value != _lastSelectedTorrentsSubTabIndex)
                {
                    ConfigService.LastSelectedTorrentsSubTabIndex = value;
                    this.RaiseAndSetIfChanged(ref _lastSelectedTorrentsSubTabIndex, value);
                }
            }
        }

        private bool _showTorrentColumnSize = Design.IsDesignMode || ConfigService.ShowTorrentColumnSize;
        public bool ShowTorrentColumnSize
        {
            get => _showTorrentColumnSize;
            set
            {
                if (value != _showTorrentColumnSize)
                {
                    ConfigService.ShowTorrentColumnSize = value;
                    this.RaiseAndSetIfChanged(ref _showTorrentColumnSize, value);
                }
            }
        }

        private bool _showTorrentColumnTotalSize = Design.IsDesignMode ? false : ConfigService.ShowTorrentColumnTotalSize;
        public bool ShowTorrentColumnTotalSize
        {
            get => _showTorrentColumnTotalSize;
            set
            {
                if (value != _showTorrentColumnTotalSize)
                {
                    ConfigService.ShowTorrentColumnTotalSize = value;
                    this.RaiseAndSetIfChanged(ref _showTorrentColumnTotalSize, value);
                }
            }
        }

        private bool _showTorrentColumnDone = Design.IsDesignMode || ConfigService.ShowTorrentColumnDone;
        public bool ShowTorrentColumnDone
        {
            get => _showTorrentColumnDone;
            set
            {
                if (value != _showTorrentColumnDone)
                {
                    ConfigService.ShowTorrentColumnDone = value;
                    this.RaiseAndSetIfChanged(ref _showTorrentColumnDone, value);
                }
            }
        }

        private bool _showTorrentColumnStatus = Design.IsDesignMode || ConfigService.ShowTorrentColumnStatus;
        public bool ShowTorrentColumnStatus
        {
            get => _showTorrentColumnStatus;
            set
            {
                if (value != _showTorrentColumnStatus)
                {
                    ConfigService.ShowTorrentColumnStatus = value;
                    this.RaiseAndSetIfChanged(ref _showTorrentColumnStatus, value);
                }
            }
        }

        private bool _showTorrentColumnSeeds = Design.IsDesignMode || ConfigService.ShowTorrentColumnSeeds;
        public bool ShowTorrentColumnSeeds
        {
            get => _showTorrentColumnSeeds;
            set
            {
                if (value != _showTorrentColumnSeeds)
                {
                    ConfigService.ShowTorrentColumnSeeds = value;
                    this.RaiseAndSetIfChanged(ref _showTorrentColumnSeeds, value);
                }
            }
        }

        private bool _showTorrentColumnPeers = Design.IsDesignMode || ConfigService.ShowTorrentColumnPeers;
        public bool ShowTorrentColumnPeers
        {
            get => _showTorrentColumnPeers;
            set
            {
                if (value != _showTorrentColumnPeers)
                {
                    ConfigService.ShowTorrentColumnPeers = value;
                    this.RaiseAndSetIfChanged(ref _showTorrentColumnPeers, value);
                }
            }
        }

        private bool _showTorrentColumnDownSpeed = Design.IsDesignMode || ConfigService.ShowTorrentColumnDownSpeed;
        public bool ShowTorrentColumnDownSpeed
        {
            get => _showTorrentColumnDownSpeed;
            set
            {
                if (value != _showTorrentColumnDownSpeed)
                {
                    ConfigService.ShowTorrentColumnDownSpeed = value;
                    this.RaiseAndSetIfChanged(ref _showTorrentColumnDownSpeed, value);
                }
            }
        }

        private bool _showTorrentColumnUpSpeed = Design.IsDesignMode || ConfigService.ShowTorrentColumnUpSpeed;
        public bool ShowTorrentColumnUpSpeed
        {
            get => _showTorrentColumnUpSpeed;
            set
            {
                if (value != _showTorrentColumnUpSpeed)
                {
                    ConfigService.ShowTorrentColumnUpSpeed = value;
                    this.RaiseAndSetIfChanged(ref _showTorrentColumnUpSpeed, value);
                }
            }
        }

        private bool _showTorrentColumnETA = !Design.IsDesignMode && ConfigService.ShowTorrentColumnETA;
        public bool ShowTorrentColumnETA
        {
            get => _showTorrentColumnETA;
            set
            {
                if (value != _showTorrentColumnETA)
                {
                    ConfigService.ShowTorrentColumnETA = value;
                    this.RaiseAndSetIfChanged(ref _showTorrentColumnETA, value);
                }
            }
        }

        private bool _showTorrentColumnRatio = !Design.IsDesignMode && ConfigService.ShowTorrentColumnETA;
        public bool ShowTorrentColumnRatio
        {
            get => _showTorrentColumnRatio;
            set
            {
                if (value != _showTorrentColumnRatio)
                {
                    ConfigService.ShowTorrentColumnRatio = value;
                    this.RaiseAndSetIfChanged(ref _showTorrentColumnRatio, value);
                }
            }
        }

        private bool _showTorrentColumnCategory = !Design.IsDesignMode && ConfigService.ShowTorrentColumnCategory;
        public bool ShowTorrentColumnCategory
        {
            get => _showTorrentColumnCategory;
            set
            {
                if (value != _showTorrentColumnCategory)
                {
                    ConfigService.ShowTorrentColumnCategory = value;
                    this.RaiseAndSetIfChanged(ref _showTorrentColumnCategory, value);
                }
            }
        }

        private bool _showTorrentColumnTags = !Design.IsDesignMode && ConfigService.ShowTorrentColumnTags;
        public bool ShowTorrentColumnTags
        {
            get => _showTorrentColumnTags;
            set
            {
                if (value != _showTorrentColumnTags)
                {
                    ConfigService.ShowTorrentColumnTags = value;
                    this.RaiseAndSetIfChanged(ref _showTorrentColumnTags, value);
                }
            }
        }

        private bool _showTorrentColumnAddedOn = Design.IsDesignMode || ConfigService.ShowTorrentColumnAddedOn;
        public bool ShowTorrentColumnAddedOn
        {
            get => _showTorrentColumnAddedOn;
            set
            {
                if (value != _showTorrentColumnAddedOn)
                {
                    ConfigService.ShowTorrentColumnAddedOn = value;
                    this.RaiseAndSetIfChanged(ref _showTorrentColumnAddedOn, value);
                }
            }
        }

        private bool _showTorrentColumnCompletedOn = Design.IsDesignMode ? false : ConfigService.ShowTorrentColumnCompletedOn;
        public bool ShowTorrentColumnCompletedOn
        {
            get => _showTorrentColumnCompletedOn;
            set
            {
                if (value != _showTorrentColumnCompletedOn)
                {
                    ConfigService.ShowTorrentColumnCompletedOn = value;
                    this.RaiseAndSetIfChanged(ref _showTorrentColumnCompletedOn, value);
                }
            }
        }

        private bool _showTorrentColumnTracker = !Design.IsDesignMode && ConfigService.ShowTorrentColumnTags;
        public bool ShowTorrentColumnTracker
        {
            get => _showTorrentColumnTracker;
            set
            {
                if (value != _showTorrentColumnTracker)
                {
                    ConfigService.ShowTorrentColumnTags = value;
                    this.RaiseAndSetIfChanged(ref _showTorrentColumnTracker, value);
                }
            }
        }

        private bool _showTorrentColumnDownLimit= !Design.IsDesignMode && ConfigService.ShowTorrentColumnDownLimit;
        public bool ShowTorrentColumnDownLimit
        {
            get => _showTorrentColumnDownLimit;
            set
            {
                if (value != _showTorrentColumnDownLimit)
                {
                    ConfigService.ShowTorrentColumnDownLimit = value;
                    this.RaiseAndSetIfChanged(ref _showTorrentColumnDownLimit, value);
                }
            }
        }

        private bool _showTorrentColumnUpLimit = !Design.IsDesignMode && ConfigService.ShowTorrentColumnUpLimit;
        public bool ShowTorrentColumnUpLimit
        {
            get => _showTorrentColumnUpLimit;
            set
            {
                if (value != _showTorrentColumnUpLimit)
                {
                    ConfigService.ShowTorrentColumnUpLimit = value;
                    this.RaiseAndSetIfChanged(ref _showTorrentColumnUpLimit, value);
                }
            }
        }

        private bool _showTorrentColumnDownloaded = Design.IsDesignMode || ConfigService.ShowTorrentColumnDownloaded;
        public bool ShowTorrentColumnDownloaded
        {
            get => _showTorrentColumnDownloaded;
            set
            {
                if (value != _showTorrentColumnDownloaded)
                {
                    ConfigService.ShowTorrentColumnDownloaded = value;
                    this.RaiseAndSetIfChanged(ref _showTorrentColumnDownloaded, value);
                }
            }
        }

        private bool _showTorrentColumnUploaded = !Design.IsDesignMode && ConfigService.ShowTorrentColumnUploaded;
        public bool ShowTorrentColumnUploaded
        {
            get => _showTorrentColumnUploaded;
            set
            {
                if (value != _showTorrentColumnUploaded)
                {
                    ConfigService.ShowTorrentColumnUploaded = value;
                    this.RaiseAndSetIfChanged(ref _showTorrentColumnUploaded, value);
                }
            }
        }

        private bool _showTorrentColumnDownloadedInSession = !Design.IsDesignMode && ConfigService.ShowTorrentColumnDownloadedInSession;
        public bool ShowTorrentColumnDownloadedInSession
        {
            get => _showTorrentColumnDownloadedInSession;
            set
            {
                if (value != _showTorrentColumnDownloadedInSession)
                {
                    ConfigService.ShowTorrentColumnDownloadedInSession = value;
                    this.RaiseAndSetIfChanged(ref _showTorrentColumnDownloadedInSession, value);
                }
            }
        }

        private bool _showTorrentColumnUploadedInSession = !Design.IsDesignMode && ConfigService.ShowTorrentColumnUploadedInSession;
        public bool ShowTorrentColumnUploadedInSession
        {
            get => _showTorrentColumnUploadedInSession;
            set
            {
                if (value != _showTorrentColumnUploadedInSession)
                {
                    ConfigService.ShowTorrentColumnUploadedInSession = value;
                    this.RaiseAndSetIfChanged(ref _showTorrentColumnUploadedInSession, value);
                }
            }
        }

        private bool _showTorrentColumnIncompletedSize = !Design.IsDesignMode && ConfigService.ShowTorrentColumnIncompletedSize;
        public bool ShowTorrentColumnIncompletedSize
        {
            get => _showTorrentColumnIncompletedSize;
            set
            {
                if (value != _showTorrentColumnIncompletedSize)
                {
                    ConfigService.ShowTorrentColumnIncompletedSize = value;
                    this.RaiseAndSetIfChanged(ref _showTorrentColumnIncompletedSize, value);
                }
            }
        }

        private bool _showTorrentColumnTimeActive = Design.IsDesignMode || ConfigService.ShowTorrentColumnTimeActive;
        public bool ShowTorrentColumnTimeActive
        {
            get => _showTorrentColumnTimeActive;
            set
            {
                if (value != _showTorrentColumnTimeActive)
                {
                    ConfigService.ShowTorrentColumnTimeActive = value;
                    this.RaiseAndSetIfChanged(ref _showTorrentColumnTimeActive, value);
                }
            }
        }

        private bool _showTorrentColumnSavePath = Design.IsDesignMode ? false : ConfigService.ShowTorrentColumnSavePath;
        public bool ShowTorrentColumnSavePath
        {
            get => _showTorrentColumnSavePath;
            set
            {
                if (value != _showTorrentColumnSavePath)
                {
                    ConfigService.ShowTorrentColumnSavePath = value;
                    this.RaiseAndSetIfChanged(ref _showTorrentColumnSavePath, value);
                }
            }
        }

        private bool _showTorrentColumnCompletedSize = !Design.IsDesignMode && ConfigService.ShowTorrentColumnCompletedSize;
        public bool ShowTorrentColumnCompletedSize
        {
            get => _showTorrentColumnCompletedSize;
            set
            {
                if (value != _showTorrentColumnCompletedSize)
                {
                    ConfigService.ShowTorrentColumnCompletedSize = value;
                    this.RaiseAndSetIfChanged(ref _showTorrentColumnCompletedSize, value);
                }
            }
        }

        private bool _showTorrentColumnRatioLimit = !Design.IsDesignMode && ConfigService.ShowTorrentColumnRatioLimit;
        public bool ShowTorrentColumnRatioLimit
        {
            get => _showTorrentColumnRatioLimit;
            set
            {
                if (value != _showTorrentColumnRatioLimit)
                {
                    ConfigService.ShowTorrentColumnRatioLimit = value;
                    this.RaiseAndSetIfChanged(ref _showTorrentColumnRatioLimit, value);
                }
            }
        }

        private bool _showTorrentColumnSeenComplete = !Design.IsDesignMode && ConfigService.ShowTorrentColumnSeenComplete;
        public bool ShowTorrentColumnSeenComplete
        {
            get => _showTorrentColumnSeenComplete;
            set
            {
                if (value != _showTorrentColumnSeenComplete)
                {
                    ConfigService.ShowTorrentColumnSeenComplete = value;
                    this.RaiseAndSetIfChanged(ref _showTorrentColumnSeenComplete, value);
                }
            }
        }

        private bool _showTorrentColumnLastActivity = !Design.IsDesignMode && ConfigService.ShowTorrentColumnLastActivity;
        public bool ShowTorrentColumnLastActivity
        {
            get => _showTorrentColumnLastActivity;
            set
            {
                if (value != _showTorrentColumnLastActivity)
                {
                    ConfigService.ShowTorrentColumnIncompletedSize = value;
                    this.RaiseAndSetIfChanged(ref _showTorrentColumnLastActivity, value);
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
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedTorrent, value);
                PluginInput = _selectedTorrent == null ? "" : _selectedTorrent.Name!;
            }
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
                    if (!Design.IsDesignMode && value != null)
                    {
                        ConfigService.FilterOnStatusIndex = StatusCounts.IndexOf(value);
                    }
                }
            }
        }

        private TagCountViewModel? _filterTag = null;
        public TagCountViewModel? FilterTag
        {
            get => _filterTag;
            set 
            {
                if (value != _filterTag)
                {
                    this.RaiseAndSetIfChanged(ref _filterTag, value);
                    if(!Design.IsDesignMode && value != null)
                    {
                        ConfigService.FilterOnTag = value.Tag;
                    }
                }
            }
        }

        private CategoryCountViewModel? _filterCategory = null;
        public CategoryCountViewModel? FilterCategory
        {
            get => _filterCategory;
            set 
            {
                if (value != _filterCategory)
                {
                    this.RaiseAndSetIfChanged(ref _filterCategory, value);
                    if (!Design.IsDesignMode && value != null)
                    {
                        ConfigService.FilterOnCategory = value.Name;
                    }
                }
            }
        }

        /// <summary>
        /// <see cref="UpdateTrackers(Newtonsoft.Json.Linq.JToken)"/> uses and sets it,
        /// FilterTracker uses to distinguish if it should save the Tracker to the config or not
        /// </summary>
        private bool _firstTrackerInit = true;
        private TrackerCountViewModel? _filterTracker;
        public TrackerCountViewModel? FilterTracker
        {
            get => _filterTracker;
            set
            {
                if (value != _filterTracker)
                {
                    this.RaiseAndSetIfChanged(ref _filterTracker, value);
                    if(!Design.IsDesignMode && value != null && !_firstTrackerInit)
                    {
                        ConfigService.FilterOnTrackerDisplayUrl = value.DisplayUrl;
                    }
                }
            }
        }

        /// <summary>
        /// <inheritdoc cref="ClearNonTextFilters"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> ClearNonTextFiltersCommand { get; }

        /// <summary>
        /// Sets these filters back to their default: <list type="bullet">
        /// <item><see cref="FilterStatus"/></item>
        /// <item><see cref="FilterCategory"/></item>
        /// <item><see cref="FilterStatus"/></item>
        /// <item><see cref="FilterTracker"/></item>
        /// </list>
        /// </summary>
        public void ClearNonTextFilters()
        {
            FilterStatus = StatusCounts[0];
            FilterCategory = CategoryCounts[0];
            FilterTag = TagCounts[0];
            FilterTracker = TrackerCounts[0];
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
                filtered = filtered.Where(t => _trackers.ContainsKey(FilterTracker.Url) && _trackers[FilterTracker.Url].Contains(t.Hash));
            }

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
            FilterStatus = Design.IsDesignMode 
                ? StatusCounts[0]
                : StatusCounts[ConfigService.FilterOnStatusIndex];


            CategoryService.Instance.CategoriesUpdated += Instance_CategoriesUpdated;
            CategoryService.Instance.Categories.CollectionChanged += Categories_CollectionChanged;

            TagService.Instance.TagsUpdated += Instance_TagsUpdated;
            TagService.Instance.Tags.CollectionChanged += Tags_CollectionChanged;
            
            //if (Design.IsDesignMode)
            //    FilterTag = TagCounts[0];
            //else // Attempt to retrieve from ConfigService, if unsuccessful default to first one
            //    FilterTag = TagCounts.FirstOrDefault(tc => tc.Tag == ConfigService.FilterOnTag) 
            //        ?? TagCounts[0];            

            TrackerCounts.Add(new TrackerCountViewModel("All", Torrents.Count));
            TrackerCounts.Add(new TrackerCountViewModel("Trackerless", Torrents.Count));
            if (Design.IsDesignMode)
                FilterTracker = TrackerCounts[0];
            // else : Handled in UpdateTrackers()

            // Add some data to preview
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

            ClearNonTextFiltersCommand = ReactiveCommand.Create(ClearNonTextFilters);

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

        private void Tags_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateTagCounts();
        }

        private void Instance_TagsUpdated(object? sender, EventArgs e)
        {
            UpdateTagCounts();
            if (Design.IsDesignMode)
                FilterTag = TagCounts[0];
            else
                FilterTag = TagCounts.FirstOrDefault(tc => tc.Tag == ConfigService.FilterOnTag) ?? TagCounts[0];
        }

        private void Categories_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateCategoryCounts();
        }

        private void Instance_CategoriesUpdated(object? sender, EventArgs e)
        {
            UpdateCategoryCounts();
            if (Design.IsDesignMode)
                FilterCategory = CategoryCounts[0];
            else // Attempt to retrieve from ConfigService, if unsuccessful default to first one
                FilterCategory = CategoryCounts.FirstOrDefault(cc => cc.Name == ConfigService.FilterOnCategory)
                    ?? CategoryCounts[0];
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
            AddOrUpdateTagCount("Untagged", Torrents.Count(t => t.Tags == null || t.Tags.Count == 0));

            foreach (var tag in TagService.Instance.Tags)
                AddOrUpdateTagCount(tag, Torrents.Count(t=>t.Tags != null && t.Tags.Contains(tag)));
        }

        private void AddOrUpdateTagCount(string tag, int count)
        {
            if (TagCounts.FirstOrDefault(t => t.Tag == tag) is TagCountViewModel tcvm)
                tcvm.Count = count;
            else
                TagCounts.Add(new TagCountViewModel(tag) { Count = count });
        }

        public void UpdateCategories(IReadOnlyDictionary<string, Category> changedCategories)
        {
            if (changedCategories is null)
                return;

            foreach (var kvp in changedCategories)
                CategoryService.Instance.ChangeCategory(kvp.Key, kvp.Value);
        }

        private void UpdateCategoryCounts()
        {
            // Default categories - should always show up
            AddOrUpdateCategoryCount(new Category() { Name = "All" }, Torrents.Count);
            AddOrUpdateCategoryCount(new Category() { Name = "Uncategorised" }, Torrents.Count(t => string.IsNullOrEmpty(t.Category)));

            foreach (var category in CategoryService.Instance.Categories)
                AddOrUpdateCategoryCount(category, Torrents.Count(t => t.Category == category.Name));
        }

        /// <summary>
        /// Utility method, creates an entry if there isn't one yet - otherwise accesses the existing one
        /// </summary>
        /// <param name="cat"></param>
        /// <param name="count"></param>
        private void AddOrUpdateCategoryCount(Category cat, int count)
        {
            if (CategoryCounts.FirstOrDefault(cc => cc.Name == cat.Name) is CategoryCountViewModel ccvm)
                ccvm.Count = count;
            else
                CategoryCounts.Add(new CategoryCountViewModel(cat){ Count = count });
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
            set => this.RaiseAndSetIfChanged(ref _categories, value);
        }

        private ObservableCollection<CategoryCountViewModel> _categoryCounts = [];
        public ObservableCollection<CategoryCountViewModel> CategoryCounts
        {
            get => _categoryCounts;
            set => this.RaiseAndSetIfChanged(ref _categoryCounts, value);
        }

        public void RemoveCategories(IReadOnlyList<string>? categoriesRemoved)
        {
            if (categoriesRemoved is null)
                return;

            var itemsToRemove = CategoryService.Instance.Categories
                .Where(c => categoriesRemoved.Contains(c.Name))
                .ToList(); // Materialize the query to avoid modifying the collection during enumeration

            if (itemsToRemove.Count > 0)
            {
                foreach (var item in itemsToRemove)
                    CategoryService.Instance.Categories.Remove(item);

                UpdateCategoryCounts();
            }
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


            if (_firstTrackerInit == true && !Design.IsDesignMode)
            {
                // Attempt to retrieve from ConfigService, if unsuccessful default to first one
                FilterTracker = TrackerCounts.FirstOrDefault(tc => tc.DisplayUrl == ConfigService.FilterOnTrackerDisplayUrl)
                    ?? TrackerCounts[0];


                _firstTrackerInit = false;
            }
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