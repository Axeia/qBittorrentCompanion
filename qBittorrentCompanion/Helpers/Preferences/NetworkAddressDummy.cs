using RaiseChangeGenerator;
using ReactiveUI;

namespace qBittorrentCompanion.Helpers.Preferences
{
    public partial class NetworkAddressDummy(string networkAddress) : ReactiveObject
    {
        [RaiseChange]
        private string _networkAddress = networkAddress;
    }
}
