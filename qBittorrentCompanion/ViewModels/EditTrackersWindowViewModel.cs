using QBittorrent.Client;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Timers;
using System.Reactive.Linq;
using System.Text.RegularExpressions;

namespace qBittorrentCompanion.ViewModels
{
    public class EditTrackersWindowViewModel : ViewModelBase
    {
        private List<Uri> trackersToDelete = [];
        private List<Uri> trackersToAdd = [];
        private List<TorrentTracker> _newTrackers = [];
        private List<TorrentTracker> _toDelete = [];
        private List<TorrentTracker> _toAdd = [];
        private string _infoHash;


        private int _deletedCount = 0;
        public int DeletedCount
        {
            get => _deletedCount;
            set => this.RaiseAndSetIfChanged(ref _deletedCount, value);
        }
        private int _addedCount = 0;
        public int AddedCount
        {
            get => _addedCount;
            set => this.RaiseAndSetIfChanged(ref _addedCount, value);
        }

        private int _dupedCount = 0;
        public int DupedCount
        {
            get => _dupedCount;
            set
            {
                this.RaiseAndSetIfChanged(ref _dupedCount, value);
                this.RaisePropertyChanged(nameof(HasDupes));
            }
        }

        public bool HasDupes { get => _dupedCount > 0; }

        private IEnumerable<TorrentTracker> _oldTrackers;
        /// <summary>
        /// Useful for enabling/disabling UI elements whilst an update is taking place
        /// </summary>
        private string _trackers = string.Empty;
        public string Trackers
        {
            get => _trackers;
            set
            {
                var updateCounts = value != _trackers;
                this.RaiseAndSetIfChanged(ref _trackers, value);
                if (updateCounts)
                    UpdateCounts();
            }
        }

        /// <summary>
        /// Useful for enabling/disabling UI elements whilst an update is taking place
        /// </summary>
        private string _tiers = " 1 \n 2 \n 3 \n 4 \n 5";
        public string Tiers
        {
            get => _tiers;
            set => this.RaiseAndSetIfChanged(ref _tiers, value);
        }

        private void UpdateCounts()
        {
            // Windows uses \r\n for a new line rather than \r\n
            // That messes up the logic below so normalize to \n to prevent this.
            string normalizedTrackers = Trackers.Replace("\r\n", "\n");

            string pattern = @"((?:http:\/\/|udp:\/\/)[^\n]+)(\n{0,3})";
            Regex regex = new Regex(pattern);
            MatchCollection matches = regex.Matches(normalizedTrackers);

            _newTrackers = [];
            int tier = 0;

            foreach (Match match in matches)
            {
                Uri trackerUri = new Uri(match.Groups[1].ToString());
                _newTrackers.Add(new TorrentTracker { Tier = tier, Url = trackerUri });

                int enterCount = match.Groups[2].ToString().Length;
                //if (enterCount == 1) do nothing
                if (enterCount == 2)
                    tier++;
                else if (enterCount == 3)
                    tier+= 2;
            }

            RedoTiersText();
            UpdateDeleted();
            UpdateAdded();
            UpdateDuped();
        }

        private void RedoTiersText()
        {
            Tiers = "0";
            int prevTier = 0;
            foreach (var tt in _newTrackers)
            {
                if (tt.Tier != null)
                {
                    int tierDiff = (tt.Tier ?? default) - prevTier;
                    if (tierDiff == 0 && prevTier != 0)
                        Tiers += "\n";
                    if (tierDiff == 1)
                        Tiers += "\n\n" + tt.Tier;
                    else if (tierDiff == 2)
                        Tiers += "\n\n\n" + tt.Tier;

                    prevTier = tt.Tier ?? default;
                }
            }
        }

        private void UpdateDeleted()
        {
            var newTrackersDict = _newTrackers.ToDictionary(t => t.Url);

            var changedOrDeletedTrackers = _oldTrackers
                .Where(oldTracker =>
                    !newTrackersDict.TryGetValue(oldTracker.Url, out var newTracker) ||
                    oldTracker.Tier != newTracker.Tier)
                .ToList();

            _toDelete = changedOrDeletedTrackers;
            DeletedCount = changedOrDeletedTrackers.Count();
        }

        private void UpdateAdded()
        {
            var oldTrackersDict = _oldTrackers.ToDictionary(t => t.Url);

            var trackersToAdd = _newTrackers
                .Where(newTracker =>
                    !oldTrackersDict.TryGetValue(newTracker.Url, out var oldTracker) ||
                    newTracker.Tier != oldTracker.Tier)
                .ToList();

            AddedCount = trackersToAdd.Count();
        }

        private void UpdateDuped()
        {
            DupedCount = _newTrackers.GroupBy(t => t.Url.ToString())
                .Where(x => x.Count() > 1)
                .Sum(c => c.Count() - 1);
        }

        public EditTrackersWindowViewModel(string hash, string? torrentName)
        {
            _infoHash = hash;
            _ = FetchDataAsync();

            // Add name search filtering
            this.WhenAnyValue(x => x.Trackers)
                .Throttle(TimeSpan.FromMilliseconds(200))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => CleanTrackers());
        }

        private void CleanTrackers()
        {
            _trackers = _trackers.Replace("\r\n", "\n");
            while (_trackers.Contains("\n\n\n\n"))
                _trackers = _trackers.Replace("\n\n\n\n", "\n\n\n");

            this.RaisePropertyChanged(nameof(Trackers));
        }

        public ReactiveCommand<Unit, Unit> SaveTrackersCommand { get; }

        public async Task SaveTrackers()
        {
            List<TorrentTracker> edittedTrackers = [];
            /*
            if (_toDelete.Count > 0)
                await QBittorrentService.QBittorrentClient.DeleteTrackersAsync(_infoHash, _toDelete);
            if (_toAdd.Count > 0)
                await QBittorrentService.QBittorrentClient.AddTrackersAsync(_infoHash, _toAdd);*/
        }


        protected async Task FetchDataAsync()
        {
            _oldTrackers = await QBittorrentService.QBittorrentClient.GetTorrentTrackersAsync(_infoHash);
            _oldTrackers = _oldTrackers.Where(t => t.Tier > -1);
            Intialise();
        }

        private void Intialise()
        {
            string temp = string.Empty;

            int prevTier = 0;
            foreach (var tracker in _oldTrackers)
            {

                if ((tracker.Tier - prevTier) > 1)
                    temp += "\n";

                temp += tracker.Url.ToString() + "\n\n";

                prevTier = tracker.Tier ?? default;
            }

            Trackers = temp;
        }
    }
}