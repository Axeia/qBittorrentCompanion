using RaiseChangeGenerator;
using ReactiveUI;

namespace qBittorrentCompanion.Helpers.Preferences
{
    public partial class IpDummy(string ip = "") : ReactiveObject
    {
        [RaiseChange]
        private string _ip = ip;
    }
}
