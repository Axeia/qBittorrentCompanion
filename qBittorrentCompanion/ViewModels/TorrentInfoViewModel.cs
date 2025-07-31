using AutoPropertyChangedGenerator;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using FluentIcons.Common;
using Newtonsoft.Json.Linq;
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
using System.Threading.Tasks;
using TorrentState = QBittorrent.Client.TorrentState;

namespace qBittorrentCompanion.ViewModels
{
    // https://github.com/qbittorrent/qBittorrent/wiki/WebUI-API-(qBittorrent-4.1)
    public partial class TorrentInfoViewModel : BytesBaseViewModel
    {
        [AutoProxyPropertyChanged(nameof(TorrentPartialInfo.CompletionOn))]
        [AutoProxyPropertyChanged(nameof(TorrentPartialInfo.IncompletedSize))]
        [AutoProxyPropertyChanged(nameof(TorrentPartialInfo.AutomaticTorrentManagement), "AutoTmm")]
        [AutoProxyPropertyChanged(nameof(TorrentPartialInfo.SeedingTimeLimit))]
        [AutoProxyPropertyChanged(nameof(TorrentPartialInfo.Ratio))]
        [AutoProxyPropertyChanged(nameof(TorrentPartialInfo.InactiveSeedingTimeLimit))]
        [AutoProxyPropertyChanged(nameof(TorrentPartialInfo.SequentialDownload))]
        [AutoProxyPropertyChanged(nameof(TorrentPartialInfo.Size))]
        [AutoProxyPropertyChanged(nameof(TorrentPartialInfo.SuperSeeding))]
        [AutoProxyPropertyChanged(nameof(TorrentPartialInfo.TotalSize))]
        [AutoProxyPropertyChanged(nameof(TorrentPartialInfo.CurrentTracker))]
        [AutoProxyPropertyChanged(nameof(TorrentPartialInfo.UploadLimit), "UpLimit")]
        [AutoProxyPropertyChanged(nameof(TorrentPartialInfo.Uploaded))]
        [AutoProxyPropertyChanged(nameof(TorrentPartialInfo.UploadSpeed), "UpSpeed")]
        [AutoProxyPropertyChanged(nameof(TorrentPartialInfo.DownloadSpeed), "DlSpeed")]
        [AutoProxyPropertyChanged(nameof(TorrentPartialInfo.SavePath))]
        [AutoProxyPropertyChanged(nameof(TorrentPartialInfo.Downloaded))]
        [AutoProxyPropertyChanged(nameof(TorrentPartialInfo.DownloadedInSession))]
        [AutoProxyPropertyChanged(nameof(TorrentPartialInfo.FirstLastPiecePrioritized))]
        [AutoProxyPropertyChanged(nameof(TorrentPartialInfo.MagnetUri))]
        [AutoProxyPropertyChanged(nameof(TorrentPartialInfo.Name))]
        [AutoProxyPropertyChanged(nameof(TorrentPartialInfo.CompletedSize))]
        [AutoProxyPropertyChanged(nameof(TorrentPartialInfo.ConnectedSeeds))]
        [AutoProxyPropertyChanged(nameof(TorrentPartialInfo.TotalSeeds))]
        [AutoProxyPropertyChanged(nameof(TorrentPartialInfo.ConnectedLeechers))]
        [AutoProxyPropertyChanged(nameof(TorrentPartialInfo.TotalLeechers))]
        [AutoProxyPropertyChanged(nameof(TorrentPartialInfo.Priority))]
        private readonly TorrentPartialInfo _torrentInfo;

        public new static string[] SizeOptions => [.. BytesBaseViewModel.SizeOptions.Take(3)];

        // Enables filtering
        private string _hash;
        public string Hash => _hash;

        /// <summary>
        /// Used to show the entries matching the filters
        /// </summary>
        [AutoPropertyChanged]
        private bool _isVisible = true;

        public TorrentInfoViewModel(TorrentPartialInfo torrentInfo, string hash, ObservableCollection<string> tags)
        {
            _torrentInfo = torrentInfo;
            SetSeedingTime(torrentInfo);
            _hash = hash;
            AllTags = tags;

            PauseCommand = ReactiveCommand.CreateFromTask(PauseAsync);
            ResumeCommand = ReactiveCommand.CreateFromTask(ResumeAsync);
            ForceResumeCommand = ReactiveCommand.CreateFromTask(ForceResumeAsync);
            SetPriorityCommand = ReactiveCommand.CreateFromTask<TorrentPriorityChange>(SetPriorityAsync);
            SetCategoryCommand = ReactiveCommand.CreateFromTask<string>(SetCategoryAsync);
            AddTagCommand = ReactiveCommand.CreateFromTask<string>(AddTagAsync);
            ToggleAutoTmmCommand = ReactiveCommand.CreateFromTask(ToggleAutoTmmAsync);
            ToggleFirstLastPiecePrioritizedCommand = ReactiveCommand.CreateFromTask(ToggleFirstLastPiecePrioritizedAsync);
            ToggleSequentialDownloadCommand = ReactiveCommand.CreateFromTask(ToggleSequentialDownloadAsync);
            ToggleSuperSeedingCommand = ReactiveCommand.CreateFromTask(ToggleSuperSeedingAsync);
            SaveDownloadLimitCommand = ReactiveCommand.CreateFromTask(SaveDownloadLimitAsync);
            SaveUploadLimitCommand = ReactiveCommand.CreateFromTask(SaveUploadLimitAsync);
            SaveShareLimitsCommand = ReactiveCommand.CreateFromTask(SaveShareLimitsAsync);
            RecheckCommand = ReactiveCommand.CreateFromTask(RecheckAsync);
            ReannounceCommand = ReactiveCommand.CreateFromTask(ReannounceAsync);
            OpenDestinationDirectoryCommand = ReactiveCommand.CreateFromTask(OpenDestinationDirectoryAsync);
            RemoveAllTagsCommand = ReactiveCommand.CreateFromTask(RemoveAllTagsAsync);
        }

        public ObservableCollection<Category> Categories => CategoryService.Instance.Categories;

        private ObservableCollection<string> _allTags = ["default tag"];
        public ObservableCollection<string> AllTags
        {
            get => _allTags;
            set
            {
                if (value != _allTags)
                {
                    _allTags = value;
                    this.RaisePropertyChanged(nameof(AllTags));
                    this.RaisePropertyChanged(nameof(TagMenuItems));
                }
            }
        }

        public ObservableCollection<TemplatedControl> CategoryMenuItems
        {
            get
            {
                var menuItems = new ObservableCollection<TemplatedControl>();

                // Add category menu items
                foreach (var category in Categories)
                {
                    menuItems.Add(new MenuItem()
                    {
                        Icon = new CheckBox() { IsChecked = Category == category.Name, Command = SetCategoryCommand, CommandParameter = category.Name },
                        Header = category.Name,
                        Command = SetCategoryCommand,
                        CommandParameter = category.Name
                    });
                }

                return menuItems;
            }
        }

        public ObservableCollection<TemplatedControl> TagMenuItems
        {
            get
            {
                var menuItems = new ObservableCollection<TemplatedControl>();

                // Add tag menu items
                foreach (var tag in AllTags)
                {
                    menuItems.Add(new MenuItem()
                    {
                        Icon = new CheckBox() { IsChecked = Tags!.ToList<string>().Contains(tag), Command = AddTagCommand, CommandParameter = tag },
                        Header = tag,
                        Command = AddTagCommand,
                        CommandParameter = tag
                    });
                }

                return menuItems;
            }
        }

        public ReactiveCommand<Unit, Unit> PauseCommand { get; }
        private async Task PauseAsync()
        {
            await QBittorrentService.PauseAsync(Hash);
        }

        public ReactiveCommand<Unit, Unit> ResumeCommand { get; }
        private async Task ResumeAsync()
        {
            await QBittorrentService.ResumeAsync(Hash);
        }

        public ReactiveCommand<Unit, Unit> ForceResumeCommand { get; }
        private async Task ForceResumeAsync()
        {
            await QBittorrentService.SetForceStartAsync(Hash, true);
        }

        public ReactiveCommand<TorrentPriorityChange, Unit> SetPriorityCommand { get; }
        private async Task SetPriorityAsync(TorrentPriorityChange newPriority)
        {
            await QBittorrentService.ChangeTorrentPriorityAsync(Hash, newPriority);
        }

        public ReactiveCommand<string, Unit> SetCategoryCommand { get; }
        private async Task SetCategoryAsync(string cat)
        {
            if (cat != null && cat == Category) // Empty string removes the category.
            {
                await QBittorrentService.SetTorrentCategoryAsync(Hash, "");
                Category = null;
            }
            else
            {
                await QBittorrentService.SetTorrentCategoryAsync(Hash, cat!);
                Category = cat;
            }
        }

        public ReactiveCommand<string, Unit> AddTagCommand { get; }
        private async Task AddTagAsync(string tag)
        {
            try
            {
                if (Tags != null)
                {
                    var tempTags = Tags.ToList();
                    if (Tags.Contains(tag))
                    {
                        await QBittorrentService.DeleteTorrentTagAsync(Hash, tag);
                        tempTags.Add(tag);
                    }
                    else
                    {
                        await QBittorrentService.AddTorrentTagAsync(Hash, tag);
                        tempTags.Remove(tag);
                    }
                    Tags = tempTags;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        public ReactiveCommand<Unit, Unit> RemoveAllTagsCommand { get; } 
        private async Task RemoveAllTagsAsync()
        {
            await QBittorrentService.DeleteTorrentTagsAsync(Hash, Tags);
        }

        public ReactiveCommand<Unit, Unit> ToggleAutoTmmCommand { get; }
        private async Task ToggleAutoTmmAsync()
        {
            await QBittorrentService.SetAutomaticTorrentManagementAsync(Hash, !(AutoTmm == true));
            AutoTmm = !(AutoTmm == true);
        }

        public ReactiveCommand<Unit, Unit> ToggleFirstLastPiecePrioritizedCommand { get; }
        private async Task ToggleFirstLastPiecePrioritizedAsync()
        {
            await QBittorrentService.ToggleFirstLastPiecePrioritizedAsync();
            FirstLastPiecePrioritized = !FirstLastPiecePrioritized;
        }

        public ReactiveCommand<Unit, Unit> ToggleSequentialDownloadCommand { get; }
        private async Task ToggleSequentialDownloadAsync()
        {
            await QBittorrentService.ToggleSequentialDownloadAsync();
            SequentialDownload = !SequentialDownload;
        }

        public ReactiveCommand<Unit, Unit> ToggleSuperSeedingCommand { get; }
        private async Task ToggleSuperSeedingAsync()
        {
            await QBittorrentService.SetSuperSeedingAsync(!(SuperSeeding == true));
            SuperSeeding = !SuperSeeding;
        }

        public ReactiveCommand<Unit, Unit> SaveDownloadLimitCommand { get; }
        private async Task SaveDownloadLimitAsync()
        {
            DlLimitIsSaving = true;

            if (DlLimit != null)
                await QBittorrentService.SetTorrentDownloadLimitAsync(Hash, Convert.ToInt64(DlLimit));
            else
                Debug.WriteLine($"{nameof(TorrentInfoViewModel)}.{nameof(SaveDownloadLimitAsync)} was somehow called whilst {nameof(DlLimit)} is null");

            DlLimitIsSaving = false;
        }

        public ReactiveCommand<Unit, Unit> SaveUploadLimitCommand { get; }
        private async Task SaveUploadLimitAsync()
        {
            UpLimitIsSaving = true;

            if (UpLimit != null)
                await QBittorrentService.SetTorrentUploadLimitAsync(Hash, Convert.ToInt64(UpLimit));
            else
                Debug.WriteLine($"{nameof(TorrentInfoViewModel)}.{nameof(SaveUploadLimitAsync)} was somehow called whilst {nameof(UpLimit)} is null");

            UpLimitIsSaving = false;
        }

        public ReactiveCommand<Unit, Unit> OpenDestinationDirectoryCommand { get; }

        [AutoPropertyChanged]
        private bool _isLocatingDirectory = false;

        private async Task OpenDestinationDirectoryAsync()
        {
            // If the file isn't complete it will be in the temporary directory.
            string baseDirectoryPath = Progress == 1.00
                ? ConfigService.DownloadDirectory
                : ConfigService.TemporaryDirectory;

            // This model does not actually hold any direct link where it's saved exactly.
            // Fetch the contents and figure it out based off that:
            IsLocatingDirectory = true;
            IReadOnlyList<TorrentContent> result = await QBittorrentService.QBittorrentClient.GetTorrentContentsAsync(Hash);
            await Task.Delay(200);
            IsLocatingDirectory = false;

            string relevativeDirectoryPath = result.First().Name;
            string combined = Path.Combine(baseDirectoryPath, relevativeDirectoryPath);

            if (combined is string directoryPath)
            {
                while (!Directory.Exists(directoryPath) && !string.IsNullOrEmpty(directoryPath))
                {
                    directoryPath = Path.GetDirectoryName(directoryPath)!;
                }

                if (string.IsNullOrEmpty(directoryPath))
                {
                    // If no directory is found, handle the error
                    throw new DirectoryNotFoundException($"No valid directory found for the path '{combined}'.");
                }

                PlatformAgnosticLauncher.OpenDirectory(directoryPath);
            }
        }

        [AutoPropertyChanged]
        private bool _shareLimitsIsSaving = false;

        public ReactiveCommand<Unit, Unit> SaveShareLimitsCommand { get; }
        public async Task SaveShareLimitsAsync()
        {
            if (MaxRatio is double doubleMaxRatio 
                && SeedingTimeLimit is TimeSpan tsSeedingTimeLimit 
                && InactiveSeedingTimeLimit is TimeSpan tsInactiveSeedingTimeLimit
            )
            {
                ShareLimitsIsSaving = true;

                await QBittorrentService.SetShareLimitsAsync(
                    Hash, doubleMaxRatio, tsSeedingTimeLimit, tsInactiveSeedingTimeLimit
                );

                ShareLimitsIsSaving = false;
            }
            else
            {
                if(MaxRatio is null)
                    Debug.WriteLine($"{nameof(MaxRatio)} was null");
                else if (SeedingTimeLimit is null)
                    Debug.WriteLine($"{nameof(SeedingTimeLimit)} was null");
                else if(InactiveSeedingTimeLimit is null)
                    Debug.WriteLine($"{nameof(InactiveSeedingTimeLimit)} was null");
            }
        }

        public ReactiveCommand<Unit, Unit> RecheckCommand { get; }
        public async Task RecheckAsync()
        {
            await QBittorrentService.RecheckAsync(Hash);
        }

        public ReactiveCommand<Unit, Unit> ReannounceCommand { get; }
        public async Task ReannounceAsync()
        {
            await QBittorrentService.ReannounceAsync(Hash);
        }

        public TorrentPartialInfo TorrentInfo
        {
            get => _torrentInfo;
        }

        /// <summary>
        /// <inheritdoc cref="TorrentPartialInfo.AddedOn"/>
        /// </summary>
        public DateTime? AddedOn
        {
            get { return _torrentInfo.AddedOn; }
            set
            {
                if (value != _torrentInfo.AddedOn)
                {
                    _torrentInfo.AddedOn = value;
                    this.RaisePropertyChanged(nameof(AddedOn));
                    this.RaisePropertyChanged(nameof(AddedOnHr));
                }
            }
        }


        /// <summary>
        /// <inheritdoc cref="TorrentPartialInfo.Category"/>
        /// </summary>
        public string? Category
        {
            get { return _torrentInfo.Category; }
            set
            {
                if (value != _torrentInfo.Category)
                {
                    _torrentInfo.Category = value;
                    this.RaisePropertyChanged(nameof(Category));
                    this.RaisePropertyChanged(nameof(CategoryMenuItems));
                }
            }
        }

        /// <summary>
        /// <inheritdoc cref="TorrentPartialInfo.DownloadLimit"/>
        /// </summary>
        public int? DlLimit
        {
            get { return _torrentInfo.DownloadLimit; }
            set
            {
                if (value != _torrentInfo.DownloadLimit)
                {
                    _torrentInfo.DownloadLimit = value;
                    this.RaisePropertyChanged(nameof(DlLimit));
                    this.RaisePropertyChanged(nameof(DlLimitHr));
                }
            }
        }
        public long DlLimit_DisplayValue
        {
            get
            {
                var returnValue = (DlLimit ?? 0) / DataConverter.GetMultiplierForUnit(DlLimitSize);
                Debug.Write($"{DlLimit} / {DataConverter.GetMultiplierForUnit(DlLimitSize)} = {returnValue}");

                return returnValue;
            }
            set
            {
                DlLimit = (int?)(value * DataConverter.GetMultiplierForUnit(DlLimitSize));
            }
        }

        /// <summary>
        /// Special case - just displayed in the UI to determine the multiplier needd to get
        /// to the bytes value needed for <see cref="UpLimit"/>
        /// </summary>
        [AutoPropertyChanged]
        private string _dlLimitSize = SizeOptions[1]; // Default to KiB

        /// <summary>
        /// Special case, determines whether the button in the UI is enabled or not
        /// </summary>
        [AutoPropertyChanged]
        private bool _dlLimitIsSaving = false;

        public async Task<bool> SetSavePathAsync(string newLocation)
        {
            await QBittorrentService.SetLocationAsync(newLocation);
            SavePath = newLocation;

            return true;
        }

        /// <summary>
        /// <inheritdoc cref="TorrentPartialInfo.EstimatedTime"/>
        /// </summary>
        public TimeSpan? EstimatedTime
        {
            get { return _torrentInfo.EstimatedTime; }
            set
            {
                if (value != _torrentInfo.EstimatedTime)
                {
                    _torrentInfo.EstimatedTime = value;
                    this.RaisePropertyChanged(nameof(EstimatedTime));
                    this.RaisePropertyChanged(nameof(EtaHr));
                }
            }
        }

        /// <summary>
        /// <inheritdoc cref="TorrentPartialInfo.ForceStart"/>
        /// </summary>
        public bool? ForceStart
        {
            get { return _torrentInfo.ForceStart; }
            set
            {
                if (value != _torrentInfo.ForceStart)
                {
                    _torrentInfo.ForceStart = value;
                    this.RaisePropertyChanged(nameof(ForceStart));
                    this.RaisePropertyChanged(nameof(ShowResume));
                }
            }
        }

        public bool ShowResume => IsPaused || ForceStart == true;

        /// <summary>
        /// <inheritdoc cref="TorrentPartialInfo.LastActivityTime"/>
        /// </summary>
        public DateTime? LastActivityTime
        {
            get { return _torrentInfo.LastActivityTime; }
            set
            {
                if (value != _torrentInfo.LastActivityTime)
                {
                    _torrentInfo.LastActivityTime = value;
                    this.RaisePropertyChanged(nameof(LastActivityTime));
                    this.RaisePropertyChanged(nameof(LastActivityHr));
                }
            }
        }

        /// <summary>
        /// <inheritdoc cref="TorrentPartialInfo.RatioLimit"/>
        /// The maximum seeding ratio for the torrent. 
        /// <list type="bullet">
        /// <item><c>-2</c> means the global limit should be used</item>
        /// <item><c>-1</c> means no limit.</item>
        /// <item>Positive values function as the limit</item>
        /// </list>
        /// </summary>
        public double? MaxRatio
        {
            get { return _torrentInfo.RatioLimit; }
            set
            {
                if (value != _torrentInfo.RatioLimit)
                {
                    _torrentInfo.RatioLimit = value;
                    this.RaisePropertyChanged(nameof(MaxRatio));
                    this.RaisePropertyChanged(nameof(MaxRatioIsGlobalControlled));
                    this.RaisePropertyChanged(nameof(MaxRatioIsUnlimited));
                }
            }
        }

        public bool MaxRatioIsGlobalControlled
        {
            set => MaxRatio = -2.00;
            get => MaxRatio == -2.00;
        }
        public bool MaxRatioIsUnlimited
        {
            set => MaxRatio = -1.00;
            get => MaxRatio == -1.00;
        }

        public async Task<bool> SetNameAsync(string newName)
        {
            await QBittorrentService.RenameAsync(Hash, newName);
            Name = newName;

            return true;
        }

        /// <summary>
        /// <inheritdoc cref="TorrentPartialInfo.UploadedInSession"/>
        /// </summary>
        public long? UploadedInSession
        {
            get { return _torrentInfo.UploadedInSession; }
            set
            {
                if (value != _torrentInfo.UploadedInSession)
                {
                    _torrentInfo.UploadedInSession = value;
                    this.RaisePropertyChanged(nameof(UploadedInSession));
                    this.RaisePropertyChanged(nameof(UploadedSessionHr));
                }
            }
        }

        /// <summary>
        /// <inheritdoc cref="TorrentPartialInfo.Progress"/>
        /// </summary>
        public double? Progress
        {
            get { return _torrentInfo.Progress; }
            set
            {
                if (value != _torrentInfo.Progress)
                {
                    _torrentInfo.Progress = value;
                    this.RaisePropertyChanged(nameof(Progress));
                    this.RaisePropertyChanged(nameof(IsCompleted));
                }
            }
        }

        public bool IsCompleted
        {
            get => Progress == 1;
        }

        /// <summary>
        /// <inheritdoc cref="TorrentPartialInfo.RatioLimit"/>
        /// </summary>
        public double? RatioLimit
        {
            get { return _torrentInfo.RatioLimit; }
            set
            {
                if (value != _torrentInfo.RatioLimit)
                {
                    _torrentInfo.RatioLimit = value;
                    this.RaisePropertyChanged(nameof(RatioLimit));
                    this.RaisePropertyChanged(nameof(RatioLimitHr));
                    this.RaisePropertyChanged(nameof(IsRatioLimited));
                }
            }
        }

        public bool IsRatioLimited
        {
            get => RatioLimit != -2.00 && RatioLimit != 1.00;
            set => RatioLimit = 0.00;
        }

        private TimeSpan _seedingTime = TimeSpan.Zero;
        /// <summary>
        /// <inheritdoc cref="TorrentPartialInfo"/>
        /// </summary>
        public TimeSpan SeedingTime
        {
            get { return _seedingTime; }
            set
            {
                if (value != _seedingTime)
                {
                    _seedingTime = value;
                    this.RaisePropertyChanged(nameof(SeedingTime));
                    this.RaisePropertyChanged(nameof(SeedingTimeHr));
                }
            }
        }

        private TimeSpan _minus2Minutes = TimeSpan.FromMinutes(-2);

        public bool IsSeedingTimeEnabled
        {
            get => !SeedingTimeLimit.Equals(_minus2Minutes);
            set
            {
                if (value)
                {
                    SeedingTimeLimit = TimeSpan.FromMinutes(60);
                }
                else
                {
                    SeedingTimeLimit = _minus2Minutes;
                }
            }
        }

        public bool IsInactiveSeedingTimeEnabled
        {
            get => !InactiveSeedingTimeLimit.Equals(_minus2Minutes);
            set
            {
                InactiveSeedingTimeLimit = value 
                    ? TimeSpan.FromMinutes(0)
                    : _minus2Minutes;
            }
        }

        /// <summary>
        /// <inheritdoc cref="TorrentPartialInfo.LastSeenComplete"/>
        /// </summary>
        public DateTime? SeenComplete
        {
            get { return _torrentInfo.LastSeenComplete; }
            set
            {
                if (value != _torrentInfo.LastSeenComplete)
                {
                    _torrentInfo.LastSeenComplete = value;
                    this.RaisePropertyChanged(nameof(SeenComplete));
                    this.RaisePropertyChanged(nameof(SeenCompleteHr));
                }
            }
        }

        /// <summary>
        /// <inheritdoc cref="TorrentPartialInfo.State"/>
        /// </summary>
        public TorrentState? State
        {
            get { return _torrentInfo.State; }
            set
            {
                if (value != _torrentInfo.State)
                {
                    _torrentInfo.State = value;
                    this.RaisePropertyChanged(nameof(State));
                    this.RaisePropertyChanged(nameof(StateHr));
                    this.RaisePropertyChanged(nameof(StateIcon));
                    this.RaisePropertyChanged(nameof(TorrentState));
                    this.RaisePropertyChanged(nameof(IsPaused));
                    this.RaisePropertyChanged(nameof(ShowResume));
                }
            }
        }

        /// <summary>
        /// Convenience method, filters through the TorrentStates to see if it's paused.
        /// </summary>
        public bool IsPaused
        {
            get
            {
                return _torrentInfo != null
                    && _torrentInfo.State != null
                    && TorrentsViewModel.TorrentStateGroupings.Paused.Contains((TorrentState)_torrentInfo.State);
            }
        }

        /// <summary>
        /// <inheritdoc cref="TorrentPartialInfo.Tags"/>
        /// </summary>
        public IReadOnlyCollection<string>? Tags
        {
            get { return _torrentInfo.Tags; }
            set
            {
                if (value != _torrentInfo.Tags)
                {
                    _torrentInfo.Tags = value;
                    this.RaisePropertyChanged(nameof(Tags));
                    this.RaisePropertyChanged(nameof(TagsFlattened));
                    this.RaisePropertyChanged(nameof(TagMenuItems));
                }
            }
        }

        /// <summary>
        /// Retrieves <see cref="Tags"> but flattened to a comma seperated string.
        /// Or an empty string if there's no tags.
        /// </summary>
        public string TagsFlattened
        {
            get => String.Join(", ", Tags ?? []);
        }

        /// <summary>
        /// <inheritdoc cref="TorrentPartialInfo.ActiveTime"/>
        /// </summary>
        public TimeSpan? TimeActive
        {
            get { return _torrentInfo.ActiveTime; }
            set
            {
                if (value != _torrentInfo.ActiveTime)
                {
                    _torrentInfo.ActiveTime = value;
                    this.RaisePropertyChanged(nameof(TimeActive));
                    this.RaisePropertyChanged(nameof(TimeActiveHr));
                    this.RaisePropertyChanged(nameof(SeedingTimeHr)); // Used in concatenated string
                }
            }
        }

        public long UpLimit_DisplayValue
        {
            get
            {
                var returnValue = (UpLimit ?? 0) / DataConverter.GetMultiplierForUnit(UpLimitSize);
                Debug.Write($"{UpLimit} / {DataConverter.GetMultiplierForUnit(UpLimitSize)} = {returnValue}");

                return returnValue;
            }
            set
            {
                UpLimit = (int?)(value * DataConverter.GetMultiplierForUnit(UpLimitSize));
            }
        }

        /// <summary>
        /// Special case - just displayed in the UI to determine the multiplier needd to get
        /// to the bytes value needed for <see cref="UpLimit"/>
        /// </summary>
        [AutoPropertyChanged]
        private string _upLimitSize = SizeOptions[1]; // Default to KiB

        /// <summary>
        /// Special case, determines whether the button in the UI is enabled or not
        /// </summary>
        [AutoPropertyChanged]
        private bool _upLimitIsSaving = false;

        /// <summary>
        /// Takes a new PartialInfo and assigns any new values to this instance.
        /// Since every set property fires its this.RaisePropertyChanged these changes 
        /// should propogate to the UI.
        /// </summary>
        /// <param name="newPartialInfo"></param>
        public void Update(TorrentPartialInfo newPartialInfo)
        {
            AddedOn = newPartialInfo?.AddedOn ?? AddedOn;
            CompletionOn = newPartialInfo?.CompletionOn ?? CompletionOn;
            IncompletedSize = newPartialInfo?.IncompletedSize ?? IncompletedSize;
            AutoTmm = newPartialInfo?.AutomaticTorrentManagement ?? AutoTmm;
            Category = newPartialInfo?.Category ?? Category;
            DlLimit = newPartialInfo?.DownloadLimit ?? DlLimit;
            DlSpeed = newPartialInfo?.DownloadSpeed ?? DlSpeed;
            SavePath = newPartialInfo?.SavePath ?? SavePath;
            Downloaded = newPartialInfo?.Downloaded ?? Downloaded;
            DownloadedInSession = newPartialInfo?.DownloadedInSession ?? DownloadedInSession;
            EstimatedTime = newPartialInfo?.EstimatedTime ?? EstimatedTime;
            FirstLastPiecePrioritized = newPartialInfo?.FirstLastPiecePrioritized ?? FirstLastPiecePrioritized;
            FirstLastPiecePrioritized = newPartialInfo?.ForceStart ?? FirstLastPiecePrioritized;
            LastActivityTime = newPartialInfo?.LastActivityTime ?? LastActivityTime;
            MagnetUri = newPartialInfo?.MagnetUri ?? MagnetUri;
            MaxRatio = newPartialInfo?.RatioLimit ?? MaxRatio;
            Name = newPartialInfo?.Name ?? Name;
            CompletedSize = newPartialInfo?.CompletedSize ?? CompletedSize;
            ConnectedSeeds = newPartialInfo?.ConnectedSeeds ?? ConnectedSeeds;
            TotalSeeds = newPartialInfo?.TotalSeeds ?? TotalSeeds;
            ConnectedLeechers = newPartialInfo?.ConnectedLeechers ?? ConnectedLeechers;
            TotalLeechers = newPartialInfo?.TotalLeechers ?? TotalLeechers;
            Priority = newPartialInfo?.Priority ?? Priority;
            Progress = newPartialInfo?.Progress ?? Progress;
            Ratio = newPartialInfo?.Ratio ?? Ratio;
            RatioLimit = newPartialInfo?.RatioLimit ?? RatioLimit;
            SeedingTimeLimit = newPartialInfo?.SeedingTimeLimit ?? SeedingTimeLimit;
            SeenComplete = newPartialInfo?.LastSeenComplete ?? SeenComplete;
            SequentialDownload = newPartialInfo?.SequentialDownload ?? SequentialDownload;
            Size = newPartialInfo?.Size ?? Size;
            State = newPartialInfo?.State ?? State;
            SuperSeeding = newPartialInfo?.SuperSeeding ?? SuperSeeding;
            Tags = newPartialInfo?.Tags ?? Tags;
            TimeActive = newPartialInfo?.ActiveTime ?? TimeActive;
            TotalSize = newPartialInfo?.TotalSize ?? TotalSize;
            CurrentTracker = newPartialInfo?.CurrentTracker ?? CurrentTracker;
            UpLimit = newPartialInfo?.UploadLimit ?? UpLimit;
            Uploaded = newPartialInfo?.Uploaded ?? Uploaded;
            UploadedInSession = newPartialInfo?.UploadedInSession ?? UploadedInSession;
            UpSpeed = newPartialInfo?.UploadSpeed ?? UpSpeed;
        
            if(newPartialInfo != null)
                SetSeedingTime(newPartialInfo);
        }

        private void SetSeedingTime(TorrentPartialInfo partialInfo)
        {
            // No idea why this isn't included in PartialInfo properly
            if (partialInfo == null)
            {
                return;
            }

            if (partialInfo.AdditionalData == null)
            {
                return;
            }

            if (partialInfo.AdditionalData.TryGetValue("seeding_time", out JToken? seedingTime))
            {
                SeedingTime = seedingTime == null
                    ? TimeSpan.Zero
                    : SeedingTime = TimeSpan.FromSeconds(seedingTime.ToObject<long>());
            }

        }

        // Add a property for the human-readable size.
        public string AddedOnHr => AddedOn?.ToString("dd/MM/yyyy HH:mm:ss") ?? "";
        public string CompletedOnHr => CompletionOn?.ToString("dd/MM/yyyy HH:mm:ss") ?? "";
        public string EtaHr => DataConverter.TimeSpanToHumanReadable(EstimatedTime);
        public string SeedingTimeHr => SeedingTime == TimeSpan.Zero 
            ? $"{TimeActiveHr}"
            : $"{TimeActiveHr}(seeded for {DataConverter.TimeSpanToDays(SeedingTime, true).Trim(' ')})";
        public string UploadedSessionHr => DataConverter.BytesToHumanReadable(UploadedInSession);
        public string TimeActiveHr => DataConverter.TimeSpanToDays(TimeActive, true);
        public string SeenCompleteHr => CompletionOn?.ToString("dd/MM/yyyy HH:mm:ss") ?? "";
        public long? DlLimitHr => DlLimit;

        public string LastActivityHr
        {
            get
            {
                if (LastActivityTime is DateTime)
                {
                    TimeSpan timeElapsed = (TimeSpan)(DateTime.Now - LastActivityTime);
                    return string.Format("{0}h {1}m ago", timeElapsed.Hours, timeElapsed.Minutes);
                }
                else
                    return "";

            }
        }
        // https://gist.github.com/pmzqla/7e5733dbecfc50ee4ecd/861099d74a0a979c1ce080593a9590c96217f924#file-web-api-documentation-L81
        public string StateHr
        {
            get
            {
                var prefix = "";
                if (ForceStart is true)
                    prefix = "[F] ";
                switch (State)
                {
                    case TorrentState.Allocating:
                        return "Allocating space";
                    case TorrentState.Error:
                        return $"{prefix}Error";
                    case TorrentState.PausedDownload:
                    case TorrentState.PausedUpload:
                        return $"{prefix}Paused";
                    case TorrentState.QueuedDownload:
                    case TorrentState.QueuedUpload:
                    case TorrentState.QueuedForChecking:
                        return $"Queued";
                    case TorrentState.Uploading:
                        return $"{prefix}Uploading";
                    case TorrentState.StalledUpload:
                    case TorrentState.ForcedUpload:
                        return $"{prefix}Seeding";
                    case TorrentState.CheckingDownload:
                    case TorrentState.CheckingUpload:
                    case TorrentState.CheckingResumeData:
                        return $"{prefix}Checking";
                    case TorrentState.Downloading:
                        return $"{prefix}Downloading";
                    case TorrentState.StalledDownload:
                        return $"{prefix}Stalled";
                    case TorrentState.MissingFiles:
                        return "Missing files";
                    case TorrentState.FetchingMetadata:
                        return "Fetching metadata";
                    case TorrentState.ForcedFetchingMetadata:
                        return "[F] Fetching metadata";
                    case TorrentState.Moving:
                        return "Moving";
                    default:
                        return $"{prefix}Unknown";
                }
            }
        }

        public Symbol StateIcon
        {
            get
            {
                switch (State)
                {
                    case TorrentState.Allocating:
                    case TorrentState.Moving:
                        return Symbol.Storage;
                    case TorrentState.ForcedDownload:
                    case TorrentState.Downloading:
                        return Symbol.ArrowDownload;
                    case TorrentState.StalledUpload:
                    case TorrentState.Uploading:
                    case TorrentState.ForcedUpload:
                        return Symbol.ArrowUpload;
                    case TorrentState.PausedUpload:
                    case TorrentState.PausedDownload:
                        return Symbol.Pause;
                    case TorrentState.QueuedUpload:
                    case TorrentState.QueuedDownload:
                        return Symbol.Clock;
                    case TorrentState.MissingFiles:
                    case TorrentState.Error:
                        return Symbol.ErrorCircle;
                    case TorrentState.Unknown:
                        return Symbol.QuestionCircle; 
                    case TorrentState.CheckingUpload:
                    case TorrentState.CheckingDownload:
                    case TorrentState.QueuedForChecking:
                    case TorrentState.CheckingResumeData:
                        return Symbol.Checkmark;
                    case TorrentState.FetchingMetadata:
                    case TorrentState.ForcedFetchingMetadata:
                        return Symbol.TagQuestionMark;
                    default:
                        return Symbol.QuestionCircle;
                }
            }
        }

        public string RatioLimitHr
        {
            get
            {
                switch (RatioLimit)
                {
                    case -2:
                        return "∞";
                    case -1:
                        return "Global";
                    default:
                        return Ratio.ToString() ?? "";
                }
            }
        }

        public async Task<byte[]> SaveDotTorrentAsync()
        {
            var httpClient = QBittorrentService.GetHttpClient();
            var baseUrl = QBittorrentService.GetUrl();
            var requestUri = new Uri(baseUrl, $"api/v2/torrents/export?hash={Hash}");

            try
            {
                return await httpClient.GetByteArrayAsync(requestUri.ToString());
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            return [];
        }
    }
}