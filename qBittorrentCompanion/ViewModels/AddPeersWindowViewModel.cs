using qBittorrentCompanion.Services;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using System;
using Avalonia.Controls;
using RaiseChangeGenerator;
using qBittorrentCompanion.Helpers;

namespace qBittorrentCompanion.ViewModels
{
    public partial class AddPeersWindowViewModel : ViewModelBase
    {

        [RaiseChange]
        private bool _peersAreValid = false;

        protected string _infoHash;

        [RaiseChange]
        protected ObservableCollection<PeerValidator> _tiers = [];

        protected string _peersText = "";
        public string PeersText
        {
            get => _peersText;
            set
            {
                // Get rid of Windows's \r
                var newValue = value.Replace("\r\n", "\n");
                // Every line is a new tracker (and tier)
                var newLines = newValue.Split('\n');

                // Update the text itself
                this.RaiseAndSetIfChanged(ref _peersText, newValue);

                // Update existing entries and add new ones
                for (int i = 0; i < newLines.Length; i++)
                {
                    string tracker = newLines[i];
                    if (i < Tiers.Count)
                        Tiers[i].Ip = tracker;
                    else
                        Tiers.Add(new PeerValidator(tracker, i));
                }

                // Remove extra entries (in reverse to avoid messing with what's iterated over)
                for (int i = Tiers.Count - 1; i >= newLines.Length; i--)
                    Tiers.RemoveAt(i);

                // All peers are updated, see if there's any problems
                PeersAreValid = Tiers.All(t => t.IsValid);
            }
        }

        public ReactiveCommand<Unit, Unit> AddPeersCommand { get; private set; }
        public event Action? RequestClose;

        public AddPeersWindowViewModel(string hash, string? torrentName)
        {
            _infoHash = hash;

            AddPeersCommand = ReactiveCommand.CreateFromTask(AddPeers);

            AddPeersCommand.IsExecuting
                .Where(isExecuting => !isExecuting)
                .Subscribe(_ => RequestClose?.Invoke());

            if(Design.IsDesignMode)
                PeersText = "192.192.192.192:55\n192.58.58.11\n2001:0000:130F:0000:0000:09C0:876A:130B";
        }

        public async Task AddPeers()
        {
            IEnumerable<Uri> trackersToAdd = Tiers.Where(t => !string.IsNullOrEmpty(t.Ip)).Select(t => new Uri(t.Ip));
            
            var peerAddResult = await QBittorrentService.AddTorrentPeersAsync(_infoHash, Tiers.Select(p => p.Ip));
            RequestClose?.Invoke();
        }
    }
}