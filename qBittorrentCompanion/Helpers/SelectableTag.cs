using RaiseChangeGenerator;
using ReactiveUI;

namespace qBittorrentCompanion.Helpers
{
    public partial class SelectableTag(string tag) : ReactiveObject
    {
        [RaiseChange]
        private string _name = tag;

        [RaiseChange]
        private bool _isSelected;
    }
}
