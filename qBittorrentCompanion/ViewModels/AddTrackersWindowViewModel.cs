using qBittorrentCompanion.Services;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using System;
using qBittorrentCompanion.Validators;
using RaiseChangeGenerator;
using qBittorrentCompanion.Helpers;

namespace qBittorrentCompanion.ViewModels
{
    public partial class AddTrackersWindowViewModel : ViewModelBase
    {
        [RaiseChange]
        protected bool _trackersAreValid = false;

        protected string _infoHash;

        [RaiseChange]
        protected ObservableCollection<TrackerValidator> _tiers = [];

        protected string _trackersText = "";
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

        public ReactiveCommand<Unit, Unit> AddTrackersCommand { get; private set; }
        public event Action? RequestClose;

        public AddTrackersWindowViewModel(string hash, string? torrentName)
        {
            _infoHash = hash;

            AddTrackersCommand = ReactiveCommand.CreateFromTask(AddTrackers);

            AddTrackersCommand.IsExecuting
                .Where(isExecuting => !isExecuting)
                .Subscribe(_ => RequestClose?.Invoke());
        }

        public async Task AddTrackers()
        {
            IEnumerable<Uri> trackersToAdd = Tiers.Where(t => !string.IsNullOrEmpty(t.Url)).Select(t => new Uri(t.Url));

            await QBittorrentService.AddTrackersAsync(_infoHash, trackersToAdd);
            RequestClose?.Invoke();
        }
    }
}