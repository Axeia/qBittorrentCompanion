using QBittorrent.Client;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using System;
using System.Diagnostics;

namespace qBittorrentCompanion.ViewModels
{
    public class EditTrackersWindowViewModel : ViewModelBase
    {

        private bool _trackersAreValid = true;
        public bool TrackersAreValid
        {
            get => IsInEditMode && _trackersAreValid;
            set => this.RaiseAndSetIfChanged(ref _trackersAreValid, value);
        }

        public class TrackerValidator : ReactiveObject
        {
            private string[] _trackerStarts => ["https://", "http://", "udp://"];

            public TrackerValidator(string url, int tier)
            {
                Url = url;
                _tier = tier;
            }

            private int _tier;
            public int Tier
            {
                get => _tier;
                set => this.RaiseAndSetIfChanged(ref _tier, value);
            }

            private string _url = string.Empty;
            public string Url
            {
                get => _url;
                set
                {
                    if (value != _url)
                    {
                        var valueLower = value.ToLower();
                        if (!value.StartsWith(_trackerStarts[0]) && !value.StartsWith(_trackerStarts[1]) && !value.StartsWith(_trackerStarts[2]))
                        {
                            ErrorMessage = "Tracker URLs have to start with http://, https:// or udp://.";
                            IsValid = false;
                        }
                        else if(_trackerStarts.Contains(valueLower))
                        {
                            ErrorMessage = "Tracker URL needs to point to some location.";
                            IsValid = false;
                        }
                        else if (!Uri.TryCreate(value, UriKind.Absolute, out _))
                        {
                            ErrorMessage = "Tracker could not be recognized as URL, invalid characters?";
                            IsValid = false;
                        }
                        else
                        {
                            ErrorMessage = "Tracker URL has the right format.";
                            IsValid = true;
                        }

                        _url = value;
                        this.RaisePropertyChanged(nameof(Url));
                    }
                }
            }

            private bool _isValid = true;
            public bool IsValid
            {
                get => _isValid;
                set => this.RaiseAndSetIfChanged(ref _isValid, value);
            }

            private string _errorMessage = string.Empty;
            public string ErrorMessage
            {
                get => _errorMessage;
                set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
            }


            private bool _isSharedTier = false;
            public bool IsSharedTier
            {
                get => _isSharedTier;
                set => this.RaiseAndSetIfChanged(ref _isSharedTier, value);
            }

            private bool _isTierJump = false;
            public bool IsTierJump
            {
                get => _isTierJump;
                set => this.RaiseAndSetIfChanged(ref _isTierJump, value);
            }
        }

        private string _infoHash;

        private ObservableCollection<TrackerValidator> _tiers = [];
        public ObservableCollection<TrackerValidator> Tiers
        {
            get => _tiers;
            set => this.RaiseAndSetIfChanged(ref _tiers, value);
        }

        private string _trackersText = "";
        public string TrackersText
        {
            get => _trackersText;
            set
            {
                // Get rid of Windows's \r
                var newValue = value.Replace("\r\n", "\n");
                // Every line is a new tracker (and tier)
                var newLines = newValue.Split('\n');

                // Update the text itself
                this.RaiseAndSetIfChanged(ref _trackersText, newValue);

                // Update existing entries and add new ones
                for (int i = 0; i < newLines.Length; i++)
                {
                    string tracker = newLines[i];
                    if (i < Tiers.Count)
                        Tiers[i].Url = tracker;
                    else
                        Tiers.Add(new TrackerValidator(tracker, 0));
                }

                // Remove extra entries (in reverse to avoid messing with what's iterated over)
                for (int i = Tiers.Count - 1; i >= newLines.Length; i--)
                    Tiers.RemoveAt(i);

                // All trackers are updated, see if there's any problems
                TrackersAreValid = Tiers.All(t => t.IsValid);
            }
        }

        private bool _isInEditMode = false;
        public bool IsInEditMode
        {
            get => _isInEditMode;
            set
            {
                this.RaiseAndSetIfChanged(ref _isInEditMode, value);
                if (_isInEditMode)
                    DisplayEditMode();
                else
                    DisplayOriginalMode();

                this.RaisePropertyChanged(nameof(TrackersAreValid));
            }
        }

        private ObservableCollection<TorrentTracker> _trackers = [];

        /// <summary>
        /// Do **NOT** use .add Directly on Trackers! Use <see cref="AddTracker(TorrentTracker, bool)">
        /// or things like <see cref="Tiers"/> will go awry!
        /// </summary>
        public ObservableCollection<TorrentTracker> Trackers 
            => _trackers;

        public EditTrackersWindowViewModel(string hash, string? torrentName)
        {
            _infoHash = hash;
            _ = FetchDataAsync();

            if(Design.IsDesignMode)
            {
                Initialise(new List<TorrentTracker> {
                    new TorrentTracker{ Url = new Uri("https://tracker1.com/announce"), Tier = 0 },
                    new TorrentTracker{ Url = new Uri("https://tracker2.com/announce"), Tier = 0 },
                    new TorrentTracker{ Url = new Uri("https://tracker3.com/announce"), Tier = 1 },
                    new TorrentTracker{ Url = new Uri("https://tracker4.com/announce"), Tier = 2 },
                    new TorrentTracker{ Url = new Uri("https://tracker5.com/announce"), Tier = 4 },
                });
            }
            
            SaveTrackersCommand = ReactiveCommand.CreateFromTask(SaveTrackers);
        }

        public ReactiveCommand<Unit, Unit> SaveTrackersCommand { get; }

        public async Task SaveTrackers()
        {
            IEnumerable<Uri> trackersToDelete = _originalTrackerList.Select(t => t.Url);
            try
            {
                await QBittorrentService.QBittorrentClient.DeleteTrackersAsync(_infoHash, trackersToDelete);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            IEnumerable<Uri> trackersToAdd = Tiers.Where(t => t.Url != string.Empty).Select(t => new Uri(t.Url));
            try
            {
                await QBittorrentService.QBittorrentClient.AddTrackersAsync(_infoHash, trackersToAdd);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            IsInEditMode = false;
            await FetchDataAsync();
        }

        protected async Task FetchDataAsync()
        {
            var rawTrackers = await QBittorrentService.QBittorrentClient.GetTorrentTrackersAsync(_infoHash);
            var trackers = rawTrackers.Where(t => t.Tier > -1).ToList();
            Initialise(trackers);
        }

        private List<TorrentTracker> _originalTrackerList = [];

        private void Initialise(List<TorrentTracker> trackers)
        {
            _originalTrackerList = trackers;

            DisplayOriginalMode();
        } 

        private void DisplayOriginalMode()
        {
            Tiers.Clear();
            _trackersText = string.Empty;

            foreach (var tracker in _originalTrackerList)
            {
                Tiers.Add(new TrackerValidator(tracker.Url.ToString(), tracker.Tier ?? default));
                _trackersText += tracker.Url.ToString() + "\n";
            }

            _trackersText.TrimEnd('\n');
            this.RaisePropertyChanged(nameof(TrackersText));

            // Mark the more advanced tiering as such
            Tiers
                .GroupBy(t => t.Tier)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g)
                .ToList()
                .ForEach(t => t.IsSharedTier = true);

            for (int i = 1; i < Tiers.Count; i++)
                Tiers[i].IsTierJump = Tiers[i].Tier - Tiers[i - 1].Tier > 1;
        }

        private void DisplayEditMode()
        {
            Tiers.Clear();
            TrackersText = string.Empty;

            string tmpText = string.Empty;
            foreach (var tracker in _originalTrackerList)
                tmpText += tracker.Url.ToString() + "\n";

            TrackersText = tmpText.TrimEnd('\n');
        }
    }
}