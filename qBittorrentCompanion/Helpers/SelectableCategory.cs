using QBittorrent.Client;
using RaiseChangeGenerator;
using ReactiveUI;

namespace qBittorrentCompanion.Helpers
{
    public partial class SelectableCategory(Category category) : ReactiveObject
    {
        private readonly Category _category = category;

        public string Name => _category.Name;

        [RaiseChange]
        private bool _isSelected;
    }
}
