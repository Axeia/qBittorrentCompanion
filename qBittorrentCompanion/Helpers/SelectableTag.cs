using AutoPropertyChangedGenerator;
using ReactiveUI;

namespace qBittorrentCompanion.Helpers
{
    public partial class SelectableTag(string tag) : ReactiveObject
    {
        [AutoPropertyChanged]
        private string _name = tag;

        [AutoPropertyChanged]
        private bool _isSelected;
    }
}
