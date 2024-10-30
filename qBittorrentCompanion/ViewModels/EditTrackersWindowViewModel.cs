using QBittorrent.Client;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Avalonia.Controls;
using System;
using System.Diagnostics;

namespace qBittorrentCompanion.ViewModels
{
    public class EditTrackersWindowViewModel : AddTrackersWindowViewModel
    {
        public new bool TrackersAreValid
        {
            get => IsInEditMode && _trackersAreValid;
            set => this.RaiseAndSetIfChanged(ref _trackersAreValid, value);
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

        public EditTrackersWindowViewModel(string hash, string? torrentName) : base (hash, torrentName)
        {
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