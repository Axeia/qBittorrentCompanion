using AutoPropertyChangedGenerator;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;

namespace qBittorrentCompanion.ViewModels.LocalSettings
{
    /// <summary>
    /// Dual purpose class (as Avalonia struggles with using different classes in a single view element)<br/>
    /// <br/>
    /// Wraps around an instance of <see cref="MonitoredDirectory"/> to load a Monitored Directory from storage
    /// <br/>or<br/>
    /// Represents a new Monitored Directory
    /// <br/><br/>
    /// The "Selectable" in the name refers to this having <see cref="_isSelected"/> to enable selection which
    /// is used to mark entries for deletion from <see cref="MonitorDirectoriesViewModel.MonitoredDirectories"/> 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="whatToDo"></param>
    public partial class SelectableMonitoredDirectoryViewModel : ReactiveObject, IDisposable
    {
        private readonly MonitoredDirectory _monitoredDirectory;
        [AutoPropertyChanged]
        private bool _existsInConfig = false;

        public MonitoredDirectory MonitoredDirectory => _monitoredDirectory;

        private IDisposable? _isSelectedSubscription;

        public void TrackSelection(Action onChange)
        {
            _isSelectedSubscription?.Dispose();
            _isSelectedSubscription = this
                .WhenAnyValue(md => md.IsSelected)
                .Subscribe(_ => onChange());
        }

        public void StopTrackingSelection()
        {
            _isSelectedSubscription?.Dispose();
            _isSelectedSubscription = null;
        }

        /// <summary>
        /// Only used when adding new entries, not when restoring them from config (and thus nullable)
        /// </summary>
        private readonly DebouncedFileWatcher? _debouncedWatcher;

        public ObservableCollection<SelectableCategory> Categories { get; } = [];
        public ObservableCollection<SelectableTag> Tags { get; } = [];

        // When adding new directories, count the amount of files to indicate if it would do anything
        // Nothing will be done with them until apply is used and the files actually start getting monitored
        public SelectableMonitoredDirectoryViewModel(string path, MonitoredDirectoryAction whatToDo)
        {
            PopulateCategories();
            PopulateTags();

            _monitoredDirectory = new MonitoredDirectory(path, whatToDo);
            _storedAction = whatToDo;
            _storedPathToMonitor = path;
            //_storedPathToMoveTo can remain default

            if (Directory.Exists(path))
            {
                // Initialize to overwrite null with an actual value, it's the difference between
                // 'pending' or newly added directories and those that were restored from ConfigService
                _dotTorrentFileCount = GetDotTorrentFilesCount();
                // Keep the count up to date
                _debouncedWatcher = new DebouncedFileWatcher(path, DirectoryMonitorService.DotTorrentFilter);
                _debouncedWatcher.ChangesReady += UpdateDotTorrentCount;
            }
            else
            {
                // Perfectly normal for designmode preview data, not when the app is in actual use
                if (!Design.IsDesignMode)
                    Debug.WriteLine($"Trying to instantiate {nameof(SelectableMonitoredDirectoryViewModel)} with a non-existent path");
            }

            _storedDownloadFolder = string.Empty;
        }
        public SelectableMonitoredDirectoryViewModel(MonitoredDirectory monitoredDirectory)
        {
            PopulateCategories();
            PopulateTags();

            _monitoredDirectory = monitoredDirectory;
            _storedPathToMonitor = _monitoredDirectory.PathToMonitor;
            _storedAction = monitoredDirectory.Action;
            ExistsInConfig = true;

            // Restore optionals and show relevant controls
            if (monitoredDirectory.Optionals is AddTorrentRequestBaseDto dto)
            {
                // Restore download folder and show its control
                if (dto.ShouldSerializeDownloadFolder())
                {
                    _addSaveTo = true;
                    _downloadFolder = dto.DownloadFolder;
                    _storedDownloadFolder = dto.DownloadFolder;
                }

                // Find category matching category string and mark it as selected 
                // (which makes it show up automatically)
                if (dto.ShouldSerializeCategory())
                {
                    var matchingCategory = Categories.FirstOrDefault(c => c.Name == dto.Category);
                    if (matchingCategory != null)
                        matchingCategory.IsSelected = true;
                }

                // Restore download limit and show its control
                if (dto.DownloadLimit.HasValue)
                {
                    _addLimitDownloadSpeed = true;
                    _limitDownloadSpeed = dto.DownloadLimit;
                    _storedLimitDownloadSpeed = dto.DownloadLimit;
                }

                // Restore upload limit and show its control
                if (dto.UploadLimit.HasValue)
                {
                    _addLimitUploadSpeed = true;
                    _limitUploadSpeed = dto.UploadLimit;
                    _storedLimitUploadSpeed = dto.UploadLimit;
                }

                // Restore the content layout (automatically shows its control)
                _storedTorrentContentLayout = TorrentContentLayout.Original;
                if (dto.ContentLayout is TorrentContentLayout tcl)
                {
                    _torrentContentLayout = tcl;
                    _storedTorrentContentLayout = tcl;
                }

                // Mark relevant tags as selected (automatically shows its control)
                if (dto.ShouldSerializeTags() && dto.Tags is IEnumerable<string> tags)
                    foreach (var tagToSelect in Tags.Where(t => tags.Contains(t.Name)))
                        tagToSelect.IsSelected = true;

                // Restore the ratio limit and show its control
                if (dto.RatioLimit.HasValue)
                {
                    _addLimitSharingRatio = true;
                    _limitSharingRatio = dto.RatioLimit;
                    _storedLimitSharingRatio = dto.RatioLimit;
                }

                if (dto.SeedingTimeLimit.HasValue)
                {
                    _addLimitSharingTime = true;
                    _seedingTimeLimit = dto.SeedingTimeLimit.Value.TotalMinutes;
                    _storedSeedingTimeLimit = dto.SeedingTimeLimit.Value.TotalMinutes;
                }

                // Restore skip hash check (automatically shows its control)
                if (dto.SkipHashChecking == true)
                {
                    _skipHashCheck = true;
                    _storedSkipHashCheck = true;
                    Debug.WriteLine("Set SkipHashCheck to true");
                }

                // Restore sequential download (automatically shows its control)
                if (dto.SequentialDownload == true)
                {
                    _downloadOrderSequentially = true;
                    _storedDownloadOrderSequentially = true;
                }

                // Restore first/last piece prioritized (automatically shows its control)
                if (dto.FirstLastPiecePrioritized == true)
                {
                    _downloadOrderPrioritizeFirstLast = true;
                    _storedDownloadOrderPrioritizeFirstLast = true;
                }

                // Restore AddPaused (automatically shows its control)
                if (dto.Paused == true)
                {
                    _addPaused = true;
                    _storedAddPaused = true;
                }
            }
        }

        private void PopulateTags()
        {
            foreach (string tag in TagService.SharedTags)
            {
                SelectableTag selectableTag = new(tag);
                IDisposable disposable = selectableTag
                    .WhenAnyValue(st => st.IsSelected)
                    .Subscribe((b) => {
                        this.RaisePropertyChanged(nameof(ShowAssignedValues));
                        this.RaisePropertyChanged(nameof(AddOptionalInput));
                    });
                _otherDisposables.Add(disposable);
                Tags.Add(selectableTag);
            }
        }

        private readonly List<IDisposable> _otherDisposables = [];
        private void PopulateCategories()
        {
            foreach (Category cat in CategoryService.SharedCategories)
            {
                SelectableCategory selectableCat = new(cat);
                IDisposable disposable = selectableCat
                    .WhenAnyValue(md => md.IsSelected)
                    .Subscribe((b) => {
                        this.RaisePropertyChanged(nameof(Category));
                        this.RaisePropertyChanged(nameof(ShowAssignedValues));
                        this.RaisePropertyChanged(nameof(AddOptionalInput));
                    });
                _otherDisposables.Add(disposable);
                Categories.Add(selectableCat);
            }
        }

        private int GetDotTorrentFilesCount()
            => Directory.GetFiles(_monitoredDirectory.PathToMonitor, DirectoryMonitorService.DotTorrentFilter).Length;

        private void UpdateDotTorrentCount()
        {
            DotTorrentFileCount = GetDotTorrentFilesCount();
        }

        public void Dispose()
        {
            _isSelectedSubscription?.Dispose();
            _debouncedWatcher?.Dispose();
            foreach (var disposable in _otherDisposables)
                disposable.Dispose();

            GC.SuppressFinalize(this);
        }

        [AutoPropertyChanged]
        private bool _pathToMonitorIsValid = true;

        private string _storedPathToMonitor = string.Empty;
        private bool _pathToMonitorHasChanged =>
            _storedPathToMonitor != _monitoredDirectory.PathToMonitor;
        public string PathToMonitor
        {
            get => _monitoredDirectory.PathToMonitor;
            set
            {
                if (value != _monitoredDirectory.PathToMonitor)
                {
                    _monitoredDirectory.PathToMonitor = value;
                    this.RaisePropertyChanged(nameof(PathToMonitor));
                    GetDotTorrentFilesCount();
                    this.RaisePropertyChanged(nameof(HasUnsavedChanges));
                }
            }
        }

        private MonitoredDirectoryAction _storedAction = MonitoredDirectoryAction.ChangeExtension;
        private bool _actionHasChanged =>
            _storedAction != _monitoredDirectory.Action;
        public MonitoredDirectoryAction Action
        {
            get => _monitoredDirectory.Action;
            set
            {
                if (value != _monitoredDirectory.Action)
                {
                    _monitoredDirectory.Action = value;
                    this.RaisePropertyChanged(nameof(Action));
                    this.RaisePropertyChanged(nameof(HasUnsavedChanges));
                }
            }
        }

        [AutoPropertyChanged]
        private bool _pathToMoveToIsValid = true;
        private bool _pathToMoveToHasChanged =>
            _storedPathToMoveTo != _monitoredDirectory.PathToMoveTo;
        private string _storedPathToMoveTo = string.Empty;
        public string PathToMoveTo
        {
            get => _monitoredDirectory.PathToMoveTo;
            set
            {
                if (Action is MonitoredDirectoryAction.Move)
                {
                    if (value != _monitoredDirectory.PathToMoveTo)
                    {
                        _monitoredDirectory.PathToMoveTo = value;
                        this.RaisePropertyChanged(nameof(this.PathToMoveTo));
                        this.RaisePropertyChanged(nameof(HasUnsavedChanges));
                    }
                }
                else
                {
                    Debug.WriteLine($"{nameof(PathToMoveTo)} value assignment ignored. " +
                        $"Can only be set if {nameof(Action)} is set to {MonitoredDirectoryAction.Move}");
                }
            }
        }

        [AutoPropertyChanged]
        private bool _isSelected;

        [AutoPropertyChanged]
        private int? _dotTorrentFileCount;


        private string _storedDownloadFolder = string.Empty;
        private bool _addSaveTo = false;
        public bool AddSaveTo
        {
            get => _addSaveTo;
            set
            {
                this.RaiseAndSetIfChanged(ref _addSaveTo, value);
                DownloadFolder = _storedDownloadFolder;
                this.RaisePropertyChanged(nameof(AddOptionalInput));
            }
        }

        private string _downloadFolder = string.Empty;
        private bool _downloadFolderHasChanged = false;

        public string DownloadFolder
        {
            get => _downloadFolder;
            set
            {
                this.RaiseAndSetIfChanged(ref _downloadFolder, value);
                _downloadFolderHasChanged = DownloadFolder != _storedDownloadFolder;
                this.RaisePropertyChanged(nameof(HasUnsavedChanges));
            }
        }

        // Sharing ratio
        private double? _storedLimitSharingRatio = null;
        private bool _addLimitSharingRatio = false;
        public bool AddLimitSharingRatio
        {
            get => _addLimitSharingRatio;
            set
            {
                this.RaiseAndSetIfChanged(ref _addLimitSharingRatio, value);
                LimitSharingRatio = _storedLimitSharingRatio;
                this.RaisePropertyChanged(nameof(AddOptionalInput));
            }
        }
        private double? _limitSharingRatio;
        private bool _limitSharingRatioHasChanged = false;
        public double? LimitSharingRatio
        {
            get => _limitSharingRatio;
            set
            {
                this.RaiseAndSetIfChanged(ref _limitSharingRatio, value);
                Debug.WriteLine($"Setting Limit sharing ratio stored {_storedLimitSharingRatio} - now {_limitSharingRatio}");
                _limitSharingRatioHasChanged = _limitSharingRatio != _storedLimitSharingRatio;
                this.RaisePropertyChanged(nameof(HasUnsavedChanges));
            }
        }

        // Download speed
        private int? _storedLimitDownloadSpeed = null;
        private bool _addLimitDownloadSpeed = false;
        public bool AddLimitDownloadSpeed
        {
            get => _addLimitDownloadSpeed;
            set
            {
                this.RaiseAndSetIfChanged(ref _addLimitDownloadSpeed, value);
                LimitDownloadSpeed = _storedLimitDownloadSpeed;
                this.RaisePropertyChanged(nameof(AddOptionalInput));
            }
        }
        private int? _limitDownloadSpeed;
        private bool _limitDownloadSpeedHasChanged = false;
        public int? LimitDownloadSpeed
        {
            get => _limitDownloadSpeed;
            set
            {
                this.RaiseAndSetIfChanged(ref _limitDownloadSpeed, value);
                _limitDownloadSpeedHasChanged = _limitDownloadSpeed != _storedLimitDownloadSpeed;
                this.RaisePropertyChanged(nameof(HasUnsavedChanges));
            }
        }

        // Upload speed
        private int? _storedLimitUploadSpeed = null;
        private bool _addLimitUploadSpeed = false;
        public bool AddLimitUploadSpeed
        {
            get => _addLimitUploadSpeed;
            set
            {
                this.RaiseAndSetIfChanged(ref _addLimitUploadSpeed, value);
                LimitUploadSpeed = _storedLimitUploadSpeed;
                this.RaisePropertyChanged(nameof(AddOptionalInput));
            }
        }
        private int? _limitUploadSpeed;
        private bool _limitUploadSpeedHasChanged = false;
        public int? LimitUploadSpeed
        {
            get => _limitUploadSpeed;
            set
            {
                this.RaiseAndSetIfChanged(ref _limitUploadSpeed, value);
                _limitUploadSpeedHasChanged = _limitUploadSpeed != _storedLimitUploadSpeed;
                this.RaisePropertyChanged(nameof(HasUnsavedChanges));
            }
        }


        // From here on, it's values that are simply on or off (no input)

        private bool? _storedSkipHashCheck;
        [AutoPropertyChanged]
        [AlsoNotify(nameof(ShowAssignedValues))]
        [AlsoNotify(nameof(AddOptionalInput))]
        [AlsoNotify(nameof(HasUnsavedChanges))]
        public bool? _skipHashCheck;
        private bool _skipHashCheckHasChanged => 
            _skipHashCheck != _storedSkipHashCheck;

        private bool? _storedAddPaused;
        [AutoPropertyChanged]
        [AlsoNotify(nameof(AddOptionalInput))]
        [AlsoNotify(nameof(ShowAssignedValues))]
        [AlsoNotify(nameof(HasUnsavedChanges))]
        private bool? _addPaused;
        private bool _skipAddPausedHasChanged =>
            _addPaused != _storedAddPaused;

        private TorrentContentLayout _storedTorrentContentLayout = TorrentContentLayout.Original;
        [AutoPropertyChanged]
        [AlsoNotify(nameof(AddOptionalInput))]
        [AlsoNotify(nameof(ShowAssignedValues))]
        [AlsoNotify(nameof(HasUnsavedChanges))]
        private TorrentContentLayout _torrentContentLayout = TorrentContentLayout.Original;
        private bool _torrentContentLayoutHasChanged =>
            _torrentContentLayout != _storedTorrentContentLayout;

        public ReactiveCommand<Unit, Unit> RestoreContentLayoutOriginalCommand
            => ReactiveCommand.Create(() => { TorrentContentLayout = QBittorrent.Client.TorrentContentLayout.Original; });

        // Download order
        public bool? _storedDownloadOrderSequentially;
        [AutoPropertyChanged]
        [AlsoNotify(nameof(AddOptionalInput))]
        [AlsoNotify(nameof(ShowAssignedValues))]
        [AlsoNotify(nameof(HasUnsavedChanges))]
        private bool? _downloadOrderSequentially;
        private bool _downloadOrderSequentiallyHasChanged =>
            _downloadOrderSequentially != _storedDownloadOrderSequentially;

        public bool? _storedDownloadOrderPrioritizeFirstLast;
        [AutoPropertyChanged]
        [AlsoNotify(nameof(AddOptionalInput))]
        [AlsoNotify(nameof(ShowAssignedValues))]
        [AlsoNotify(nameof(HasUnsavedChanges))]
        private bool? _downloadOrderPrioritizeFirstLast;
        private bool _downloadOrderPrioritizeFirstLastHasChanged =>
            _downloadOrderPrioritizeFirstLast != _storedDownloadOrderPrioritizeFirstLast;

        public ReactiveCommand<Unit, Unit> ToggleDownloadOrderSequentiallyCommand
            => ReactiveCommand.Create(() => { DownloadOrderSequentially = DownloadOrderSequentially == true ? null : true; });
        public ReactiveCommand<Unit, Unit> ToggleDownloadOrderPrioritizeFirstLastCommand
            => ReactiveCommand.Create(() => { DownloadOrderPrioritizeFirstLast = DownloadOrderPrioritizeFirstLast == true ? null : true; });

        // Should the 'Optional values (remote actions)' section be displayed?
        public bool AddOptionalInput 
            => AddSaveTo
            || AddLimitSharingTime 
            || AddLimitSharingRatio
            || AddLimitDownloadSpeed 
            || AddLimitUploadSpeed
            || ShowAssignedValues;

        /// <summary>
        /// Should the 'Assigned values' section be displayed?
        /// </summary>
        public bool ShowAssignedValues
            => Category is not null
            || AddPaused is true 
            || SkipHashCheck is true
            || DownloadOrderSequentially is true 
            || DownloadOrderPrioritizeFirstLast is true
            || TorrentContentLayout is TorrentContentLayout.NoSubfolder or TorrentContentLayout.Subfolder
            || Tags.Any(t => t.IsSelected);

        /// <summary>
        /// Returns selected category or null if none is selected
        /// </summary>
        public SelectableCategory? Category => Categories
            .Where(sc => sc.IsSelected)
            .FirstOrDefault();

        // Never needs to be false, just true or null.
        public ReactiveCommand<Unit, Unit> ToggleHashCheckCommand
            => ReactiveCommand.Create(() => { SkipHashCheck = SkipHashCheck is null ? true : null; });
        public ReactiveCommand<Unit, Unit> ToggleAddPausedCommand
            => ReactiveCommand.Create(() => { AddPaused = AddPaused is null ? true : null; });

        //Seeding time
        private double? _storedSeedingTimeLimit = null;
        private bool _addLimitSharingTime = false;
        public bool AddLimitSharingTime
        {
            get => _addLimitSharingTime;
            set
            {
                this.RaiseAndSetIfChanged(ref _addLimitSharingTime, value);
                SeedingTimeLimit = _storedSeedingTimeLimit;
            }
        }
        private double? _seedingTimeLimit;
        private bool _seedingTimeLimitHasChanged = false;
        public double? SeedingTimeLimit
        {
            get => _seedingTimeLimit;
            set
            {
                this.RaiseAndSetIfChanged(ref _seedingTimeLimit, value);
                _seedingTimeLimitHasChanged = _seedingTimeLimit != _storedSeedingTimeLimit;
                this.RaisePropertyChanged(nameof(HasUnsavedChanges));
            }
        }

        public AddTorrentRequestBaseDto? ToAddTorrentRequestBaseDto()
        {
            var dto = new AddTorrentRequestBaseDto()
            {
                DownloadFolder = AddSaveTo
                    ? this.DownloadFolder
                    : string.Empty,
                SeedingTimeLimit = this.SeedingTimeLimit is double stl
                    ? TimeSpan.FromMinutes(stl)
                    : null,
                RatioLimit = AddLimitSharingRatio
                    ? this.LimitSharingRatio is double rl
                        ? rl
                        : null
                    : null,
                DownloadLimit = AddLimitDownloadSpeed
                    ? this.LimitDownloadSpeed
                    : null,
                UploadLimit = AddLimitUploadSpeed
                    ? this.LimitUploadSpeed
                    : null,
                Category = this.Category?.Name ?? string.Empty,
                SkipHashChecking = this.SkipHashCheck,
                Paused = this.AddPaused,
                SequentialDownload = this.DownloadOrderSequentially,
                FirstLastPiecePrioritized = this.DownloadOrderPrioritizeFirstLast,
                ContentLayout = this.TorrentContentLayout,
                Tags = [.. this.Tags
                    .Where(t => t.IsSelected)
                    .Select(t => t.Name)]
            };

            return dto.HasAnyNonDefaultValue() ? dto : null;
        }

        public void MarkAsSaved()
        {
            // Values got stored so set them as the 'stored' values.
            _storedAddPaused = AddPaused;
            _storedDownloadFolder = DownloadFolder;
            _storedDownloadOrderPrioritizeFirstLast = DownloadOrderPrioritizeFirstLast;
            _storedDownloadOrderSequentially = DownloadOrderSequentially;
            _storedLimitDownloadSpeed = LimitDownloadSpeed;
            _storedLimitSharingRatio = LimitSharingRatio;
            _storedLimitUploadSpeed = LimitUploadSpeed;
            _storedSeedingTimeLimit = SeedingTimeLimit;
            _storedSkipHashCheck = SkipHashCheck;
            _storedTorrentContentLayout = TorrentContentLayout;

            // Now exists on config
            ExistsInConfig = true;

            // Re-evaluate if there's changes (should be false)
            this.RaisePropertyChanged(nameof(HasUnsavedChanges));
        }

        public bool HasUnsavedChanges
        {
            get
            {
                return _downloadFolderHasChanged
                    || _seedingTimeLimitHasChanged
                    || _limitSharingRatioHasChanged
                    || _limitDownloadSpeedHasChanged
                    || _limitUploadSpeedHasChanged
                    || _skipHashCheckHasChanged
                    || _skipAddPausedHasChanged
                    || _torrentContentLayoutHasChanged
                    || _downloadOrderPrioritizeFirstLastHasChanged
                    || _downloadOrderSequentiallyHasChanged
                    || _pathToMonitorHasChanged
                    || _actionHasChanged
                    || (_pathToMoveToHasChanged && Action == MonitoredDirectoryAction.Move)
                    || !ExistsInConfig;
            }
        }
    }
}
