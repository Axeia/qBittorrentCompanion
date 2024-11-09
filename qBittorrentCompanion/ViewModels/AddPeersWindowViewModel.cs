using qBittorrentCompanion.Services;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using System;
using System.Diagnostics;
using QBittorrent.Client;
using System.Text.RegularExpressions;
using System.Net;
using Avalonia.Controls;

namespace qBittorrentCompanion.ViewModels
{
    public class AddPeersWindowViewModel : ViewModelBase
    {
        protected bool _peersAreValid = false;
        public bool PeersAreValid
        {
            get => _peersAreValid;
            set => this.RaiseAndSetIfChanged(ref _peersAreValid, value);
        }

        public class PeerValidator : ReactiveObject
        {
            public static bool IsValidIpWithPort(string input)
            {
                // Regular expression to check if the format is correct (IPv4:port or [IPv6]:port)
                var regex = new Regex(@"^(\[(?<ip6>.+)]|(?<ip4>.+)):(?<port>\d+)$");

                var match = regex.Match(input);

                if (!match.Success)
                    return false;

                // Extract the IP address and port
                var ipString = match.Groups["ip4"].Success ? match.Groups["ip4"].Value : match.Groups["ip6"].Value;
                var portString = match.Groups["port"].Value;

                // Validate IP address
                if (!IPAddress.TryParse(ipString, out _))
                    return false;

                // Validate port
                if (!int.TryParse(portString, out int port) || port < 1 || port > 65535)
                    return false;

                return true;
            }

            public PeerValidator(string url, int tier)
            {
                Ip = url;
                _tier = tier;
            }

            private int _tier;
            public int Tier
            {
                get => _tier;
                set => this.RaiseAndSetIfChanged(ref _tier, value);
            }

            private string _ip = string.Empty;
            public string Ip
            {
                get => _ip;
                set
                {
                    if (value != _ip)
                    {
                        IsValid = IsValidIpWithPort(value);
                        if (!IsValid)
                        {
                            ErrorMessage = "This does not appear to be a valid IP address with a port";
                        }
                        _ip = value;
                        this.RaisePropertyChanged(nameof(Ip));
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
        }

        protected string _infoHash;

        protected ObservableCollection<PeerValidator> _tiers = [];
        public ObservableCollection<PeerValidator> Tiers
        {
            get => _tiers;
            set => this.RaiseAndSetIfChanged(ref _tiers, value);
        }

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
        public event Action RequestClose;

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
            try
            {
                await QBittorrentService.QBittorrentClient.AddTorrentPeersAsync(_infoHash, Tiers.Select(p => p.Ip));
                RequestClose?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}