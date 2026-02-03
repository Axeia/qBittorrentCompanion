using Avalonia.Controls;
using DynamicData;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Services;
using RaiseChangeGenerator;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels
{
    public partial class TorrentsViewModel : RssPluginSupportBaseViewModel
    {
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

        private bool _showTorrentColumnTotalSize = !Design.IsDesignMode && ConfigService.ShowTorrentColumnTotalSize;
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

        private bool _showTorrentColumnCompletedOn = !Design.IsDesignMode && ConfigService.ShowTorrentColumnCompletedOn;
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

        private bool _showTorrentColumnSavePath = !Design.IsDesignMode && ConfigService.ShowTorrentColumnSavePath;
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

        [RaiseChange]
        private ObservableCollection<TorrentInfoViewModel> _filteredTorrents = [];
        [RaiseChange]
        private int _filteredTorrentsCount = 0;

        private TorrentInfoViewModel? _selectedTorrent;
        public TorrentInfoViewModel? SelectedTorrent
        {
            get => _selectedTorrent;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedTorrent, value);

                RxApp.MainThreadScheduler.Schedule(() =>
                {
                    PluginInput = _selectedTorrent?.Name ?? string.Empty;
                });

                if (_selectedTorrent is null)
                {
                    AppLoggerService.AddLogMessage(
                        LogLevel.Info,
                        GetFullTypeName<TorrentsViewModel>(),
                        Resources.Resources.TorrentsViewModel_UnsetSelectedTorrentTitle,
                        Resources.Resources.TorrentsViewModel_UnsetSelectedTorrentMessage
                    );
                }
                else
                {
                    AppLoggerService.AddLogMessage(
                        LogLevel.Info,
                        GetFullTypeName<TorrentsViewModel>(),
                        Resources.Resources.TorrentsViewModel_SelectedTorrentTitle,
                        Resources.Resources.TorrentsViewModel_SelectedTorrentMessage
                    );
                }
            }
        }

        [RaiseChange]
        private List<TorrentInfoViewModel> _selectedTorrents = [];
        [RaiseChange]
        private ObservableCollection<TorrentInfoViewModel> _torrents = [];
        [RaiseChange]
        private int _torrentsCount = 0;
        [RaiseChange]
        private string _filterText = "";
        [RaiseChange]
        private bool _isUsingNonTextFilter = false;
        [RaiseChange]
        private bool _filterCompleted = false;

        private StatusCountViewModel? _filterStatus = null;
        public StatusCountViewModel? FilterStatus
        {
            get => _filterStatus;
            set
            {
                if (value != FilterStatus)
                {
                    FilterCompleted = value != null && value.Name == Resources.Resources.TorrentsView_Completed;
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
                && (FilterTag == null || FilterTag.Tag == Resources.Resources.Global_All || (FilterTag.Tag == Resources.Resources.TorrentsViewModel_Tagless ? t.Tags?.Count == 0 : t.Tags!.Contains(FilterTag.Tag)))
                 // If filtercategory is not set or "All" return all || If it's uncategorised grab all without a category or grab all belong to category
                && (FilterCategory == null || FilterCategory.Name == Resources.Resources.Global_All || (FilterCategory.Name == Resources.Resources.TorrentsViewModel_Uncategorized ? string.IsNullOrEmpty(t.Category) : t.Category == FilterCategory.Name)) 
            );

            // Then apply the FilterTracker filter
            if (FilterTracker != null && FilterTracker.DisplayUrl == Resources.Resources.TorrentsViewModel_Trackerless)
                filtered = filtered.Where(t => !_trackers.Values.Any(v => v.Contains(t.Hash)));
            else if (FilterTracker != null && !string.IsNullOrWhiteSpace(FilterTracker.DisplayUrl) && FilterTracker.DisplayUrl != Resources.Resources.Global_All)
            {
                filtered = filtered.Where(t => _trackers.ContainsKey(FilterTracker.Url) && _trackers[FilterTracker.Url].Contains(t.Hash));
            }

            IsUsingNonTextFilter = (FilterStatus != null && FilterStatus != StatusCounts[0])
                || (FilterCategory != null && FilterCategory != CategoryCounts[0])
                || (FilterTag != null && FilterTag != TagCounts[0])
                || (FilterTracker != null && FilterTracker != TrackerCounts[0]);

            //filtered.ToList
            FilteredTorrents = new ObservableCollection<TorrentInfoViewModel>(filtered.ToList());
            FilteredTorrentsCount = FilteredTorrents.Count;
        }

        public TorrentsViewModel() : base()
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
            
            StatusCounts.Add(new StatusCountViewModel(Resources.Resources.Global_All, FluentIcons.Common.Symbol.LineHorizontal1, [])); // 0
            StatusCounts.Add(new StatusCountViewModel(Resources.Resources.TorrentsViewModel_Downloading, FluentIcons.Common.Symbol.ArrowDownload, TorrentStateGroupings.Download)); // 1
            StatusCounts.Add(new StatusCountViewModel(Resources.Resources.TorrentsViewModel_Uploading, FluentIcons.Common.Symbol.ArrowUpload, TorrentStateGroupings.Seeding)); // 2
            StatusCounts.Add(new StatusCountViewModel(Resources.Resources.TorrentsView_Completed, FluentIcons.Common.Symbol.CheckmarkCircle, [])); // 3
            StatusCounts.Add(new StatusCountViewModel(Resources.Resources.TorrentsViewModel_Resumed, FluentIcons.Common.Symbol.PlayCircle, TorrentStateGroupings.Resumed)); // 4
            StatusCounts.Add(new StatusCountViewModel(Resources.Resources.TorrentsViewModel_Paused, FluentIcons.Common.Symbol.PauseCircle, TorrentStateGroupings.Paused)); // 5
            StatusCounts.Add(new StatusCountViewModel(Resources.Resources.TorrentsViewModel_Active, FluentIcons.Common.Symbol.ShiftsActivity, TorrentStateGroupings.Active)); // 6
            StatusCounts.Add(new StatusCountViewModel(Resources.Resources.TorrentsViewModel_Inactive, FluentIcons.Common.Symbol.History, TorrentStateGroupings.InActive)); // 7
            StatusCounts.Add(new StatusCountViewModel(Resources.Resources.TorrentsViewModel_Stalled, FluentIcons.Common.Symbol.ArrowSyncDismiss, TorrentStateGroupings.Stalled)); // 8
            StatusCounts.Add(new StatusCountViewModel(Resources.Resources.TorrentsViewModel_StalledDownloading, FluentIcons.Common.Symbol.ArrowCircleDown, TorrentStateGroupings.StalledDownload)); // 9
            StatusCounts.Add(new StatusCountViewModel(Resources.Resources.TorrentsViewModel_StalledUploading, FluentIcons.Common.Symbol.ArrowCircleUp, TorrentStateGroupings.StalledUpload)); // 10
            StatusCounts.Add(new StatusCountViewModel(Resources.Resources.TorrentsViewModel_Checking, FluentIcons.Common.Symbol.ArrowSyncCircle, TorrentStateGroupings.Checking)); // 11
            StatusCounts.Add(new StatusCountViewModel(Resources.Resources.TorrentsViewModel_Errored, FluentIcons.Common.Symbol.ErrorCircle, TorrentStateGroupings.Error)); // 12
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

            TrackerCounts.Add(new TrackerCountViewModel(Resources.Resources.Global_All, Torrents.Count));
            TrackerCounts.Add(new TrackerCountViewModel(Resources.Resources.TorrentsViewModel_Trackerless, Torrents.Count));
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
            ResumeTorrentsForCategoryCommand = ReactiveCommand.CreateFromTask(() =>
                QBittorrentService.ResumeAsync(TorrentHashesForCurrentCategory));
            PauseTorrentsForCategoryCommand = ReactiveCommand.CreateFromTask(() =>
                QBittorrentService.PauseAsync(TorrentHashesForCurrentCategory));

            RemovedUnusedTagsCommand = ReactiveCommand.CreateFromTask(RemoveUnusedTagsAsync);
            ResumeTorrentsForTagCommand = ReactiveCommand.CreateFromTask(() =>
                QBittorrentService.ResumeAsync(TorrentHashesForCurrentTag));
            PauseTorrentsForTagCommand = ReactiveCommand.CreateFromTask(() =>
                QBittorrentService.PauseAsync(TorrentHashesForCurrentTag));

            ResumeTorrentsForTrackerCommand = ReactiveCommand.CreateFromTask(() =>
                QBittorrentService.ResumeAsync(TorrentHashesForCurrentTracker));
            PauseTorrentsForTrackerCommand = ReactiveCommand.CreateFromTask(() =>
                QBittorrentService.PauseAsync(TorrentHashesForCurrentTracker));

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
                }, "533ASDAFAFDA232", []));
            }
        }

        private IEnumerable<string> TorrentHashesForCurrentCategory => 
            TorrentsForCurrentCategory().Select(t => t.Hash);

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
                    item.WhenAnyValue(x => x.State).Subscribe(_ => this.RaisePropertyChanged(nameof(CanBeMassPaused)));
                    item.WhenAnyValue(x => x.State).Subscribe(_ => this.RaisePropertyChanged(nameof(CanBeMassUnpaused)));
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

            UpdateAllTorrentStatusCounts();

            if (updateTagCounts)
                UpdateTagCounts();
            if (updateCategoryCounts)
                UpdateCategoryCounts();

            UpdateFilteredTorrents(); //If Torrents changed, so should FilteredTorrents
            UpdateTagCounts();
            UpdateCategoryCounts();
            UpdateTrackerAllAndTrackerlessCount();
            TorrentsCount = Torrents.Count;
        }

        private void UpdateAllTorrentStatusCounts()
        {
            StatusCounts[0].Count = Torrents.Count;
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
        }

        private void UpdateTagCounts()
        {
            AddOrUpdateTagCount(Resources.Resources.Global_All, Torrents.Count);
            AddOrUpdateTagCount(Resources.Resources.TorrentsViewModel_Tagless, Torrents.Count(t => t.Tags == null || t.Tags.Count == 0));

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

        public static void UpdateCategories(IReadOnlyDictionary<string, Category> changedCategories)
        {
            if (changedCategories is null)
                return;

            foreach (var kvp in changedCategories)
                CategoryService.Instance.ChangeCategory(kvp.Key, kvp.Value);
        }

        private void UpdateCategoryCounts()
        {
            // Default categories - should always show up
            AddOrUpdateCategoryCount(new Category() { Name = Resources.Resources.Global_All }, Torrents.Count);
            AddOrUpdateCategoryCount(new Category() { Name = Resources.Resources.TorrentsViewModel_Uncategorized }, Torrents.Count(t => string.IsNullOrEmpty(t.Category)));

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
            var tagCount = TagCounts.FirstOrDefault(t => t.Tag == Resources.Resources.Global_All);
            if (tagCount != null)
                tagCount.Count = Torrents.Count;
        }

        [RaiseChange]
        private TorrentPropertiesViewModel? _propertiesForSelectedTorrent;
        [RaiseChange]
        private TorrentPieceStatesViewModel? _torrentPieceStatesViewModel;
        [RaiseChange]
        private ObservableCollection<string> _tags = [];
        [RaiseChange]
        private ObservableCollection<StatusCountViewModel> _statusCounts = [];
        [RaiseChange]
        private ObservableCollection<TagCountViewModel> _tagCounts = [];

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

            if (newTagCounts.Count != 0)
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

            if (toRemoveItems.Count > 0)
                this.RaisePropertyChanged(nameof(TagCounts));
        }

        [RaiseChange]
        private ObservableCollection<Category> _categories = [];
        [RaiseChange]
        private ObservableCollection<CategoryCountViewModel> _categoryCounts = [];

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

        private readonly Dictionary<string, string[]> _trackers = [];

        [RaiseChange]
        private ObservableCollection<TrackerCountViewModel> _trackerCounts = [];

        public void UpdateTrackers(Newtonsoft.Json.Linq.JToken trackers)
        {
            this.RaisePropertyChanged(nameof(TrackerCounts));

            foreach (var tracker in trackers)
            {
                var property = tracker as Newtonsoft.Json.Linq.JProperty;
                var array = property!.Value as Newtonsoft.Json.Linq.JArray;

                TrackerCounts.Add(new TrackerCountViewModel(property.Name, array!.Count));
            }

            foreach (var tracker in trackers)
            {
                var property = tracker as Newtonsoft.Json.Linq.JProperty;
                var array = property!.Value as Newtonsoft.Json.Linq.JArray;

                if (_trackers.TryGetValue(property.Name, out string[]? value))
                {
                    if (!array!.ToObject<string[]>()!.SequenceEqual(value))
                    { // Updates to new values
                        _trackers[property.Name] = array.ToObject<string[]>()!;
                    }
                }
                else
                { // Initialises. 
                    _trackers[property.Name] = array!.ToObject<string[]>()!;
                }

                UpdateTrackerAllAndTrackerlessCount();
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

        /// <summary>
        /// Updates the .Count property of the TrackerCountViewModel for "All" and "Trackerless" 
        /// </summary>
        private void UpdateTrackerAllAndTrackerlessCount()
        {
            TrackerCounts.First<TrackerCountViewModel>(t => t.Url == Resources.Resources.Global_All).Count = Torrents.Count;

            var trackerless = TrackerCounts.FirstOrDefault<TrackerCountViewModel>(t => t.Url == Resources.Resources.TorrentsViewModel_Trackerless);
            if (trackerless != null)
                trackerless.Count = 0;
        }

        public bool CanBeMassPaused
            => Torrents.Any(t => t.State is TorrentState ts && TorrentStateGroupings.Active.Contains(ts));
        public bool CanBeMassUnpaused
            => Torrents.Any(t => t.State is TorrentState ts && TorrentStateGroupings.Paused.Contains(ts));


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

        [RaiseChange]
        private int _inactiveCount;
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

        [RaiseChange]
        private TorrentTrackersViewModel? _torrentTrackersViewModel;
        [RaiseChange]
        private TorrentPeersViewModel? _torrentPeersViewModel;
        [RaiseChange]
        private TorrentHttpSourcesViewModel? _httpSourcesViewModel;
        [RaiseChange]
        private TorrentContentsViewModel? _torrentContentsViewModel;

        public void PauseTorrentContents()
        {
            _torrentContentsViewModel?.Pause();
        }

        private IEnumerable<string> SelectedHashes => SelectedTorrents.Select(st => st.Hash!);
        private bool TorrentsSelected => SelectedTorrents is not null && SelectedTorrents.Count > 0;

        public async Task PauseSelectedTorrentsAsync()
        {
            if (TorrentsSelected)
                await QBittorrentService.PauseAsync(SelectedHashes);
        }

        public async Task PauseAll()
        {
            var activeTorrentHashes = Torrents
                .Where(t => t.State is TorrentState ts && TorrentStateGroupings.Active.Contains(ts))
                .Select(t=>t.Hash);

            await QBittorrentService.PauseAsync(activeTorrentHashes);
        }

        public async Task UnpauseAll()
        {
            var activeTorrentHashes = Torrents
                .Where(t => t.State is TorrentState ts && TorrentStateGroupings.Paused.Contains(ts))
                .Select(t => t.Hash);

            await QBittorrentService.ResumeAsync(activeTorrentHashes);
        }

        public async Task ResumeSelectedTorrentsAsync()
        {
            if (TorrentsSelected)
                await QBittorrentService.ResumeAsync(SelectedHashes);
        }

        public async Task SetPriorityForSelectedTorrentsAsync(TorrentPriorityChange newPriority)
        {
            if (TorrentsSelected)
                await QBittorrentService.ChangeTorrentPriorityAsync(SelectedHashes, newPriority);
        }

        public async Task RemoveUnusedCategoriesAsync()
        {
            var categoriesInUse = Torrents.Select(t => t.Category);
            var unusedCategories = Categories
                .Where(c => !categoriesInUse.Contains(c.Name))
                .Select(c => c.Name);

            await QBittorrentService.DeleteCategoriesAsync(unusedCategories);
        }

        public async Task RemoveUnusedTagsAsync()
        {
            var tagsInUse = Torrents.SelectMany(t => t.Tags!).ToList();
            var unusedTags = Tags.Where(t => !tagsInUse.Contains(t));

            await QBittorrentService.DeleteTagsAsync(unusedTags);
        }

        private IEnumerable<TorrentInfoViewModel> TorrentsForCurrentCategory()
        {
            return FilterCategory == null || FilterCategory.Name == Resources.Resources.Global_All
                ? []  
                : Torrents.Where(t => t.Category == FilterCategory.Name);
        }

        
        public async Task ResumeTorrentsForCategoryAsync()
        {
            await QBittorrentService.ResumeAsync(TorrentsForCurrentCategory().Select(t => t.Hash));
        }

        public async Task DeleteTorrentsForCategoryAsync(bool deleteFiles = false)
        {
            await DeleteTorrentsAsync(TorrentsForCurrentCategory(), deleteFiles);
        }

        private IEnumerable<TorrentInfoViewModel> TorrentsForCurrentTag =>
            Torrents
                .Where(t => t.Tags != null 
                && t.Tags.Contains(FilterTag == null ? "" : FilterTag.Tag));

        private IEnumerable<string> TorrentHashesForCurrentTag => 
            TorrentsForCurrentTag.Select(t => t.Hash);

        public async Task DeleteTorrentsForTagAsync(bool deleteFiles = false)
        {
            await DeleteTorrentsAsync(TorrentsForCurrentTag, deleteFiles);
        }

        private string[] TorrentHashesForCurrentTracker =>
            _trackers[FilterTracker!.DisplayUrl];

        public async Task DeleteTorrentsForTrackerAsync(bool deleteFiles = false)
        {
            var torrentHashesForCurrentTracker = TorrentHashesForCurrentTracker;
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

            await QBittorrentService.DeleteAsync(selectedHashes, deleteFiles);
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