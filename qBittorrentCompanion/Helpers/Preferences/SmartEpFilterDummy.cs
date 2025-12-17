using RaiseChangeGenerator;
using ReactiveUI;

namespace qBittorrentCompanion.Helpers.Preferences
{
    public partial class SmartEpFilterDummy(string smartEpFilter) : ReactiveObject
    {
        [RaiseChange]
        private string _smartEpFilter = smartEpFilter;
    }
}
