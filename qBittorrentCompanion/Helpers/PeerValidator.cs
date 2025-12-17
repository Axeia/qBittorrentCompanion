using RaiseChangeGenerator;
using ReactiveUI;
using System.Net;
using System.Text.RegularExpressions;
using static qBittorrentCompanion.ViewModels.TorrentTrackerViewModel;

namespace qBittorrentCompanion.Helpers
{


    public partial class PeerValidator : ReactiveObject
    {
        [RaiseChange]
        private bool _isValid = true;
        [RaiseChange]
        private string _errorMessage = string.Empty;
        [RaiseChange]
        private int _tier;

        public static bool IsValidIpWithPort(string input)
        {
            // Regular expression to check if the format is correct (IPv4:port or [IPv6]:port)
            var regex = ValidIpRegex();

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

        [GeneratedRegex(@"^(\[(?<ip6>.+)]|(?<ip4>.+)):(?<port>\d+)$")]
        private static partial Regex ValidIpRegex();
    }
}
