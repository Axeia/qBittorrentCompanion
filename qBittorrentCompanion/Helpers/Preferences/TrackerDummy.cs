using RaiseChangeGenerator;
using ReactiveUI;

namespace qBittorrentCompanion.Helpers.Preferences
{
    public partial class TrackerDummy(string tracker) : ReactiveObject
    {
        [RaiseChange]
        private string _tracker = tracker;
    }
}
