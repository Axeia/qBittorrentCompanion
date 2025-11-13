using AutoPropertyChangedGenerator;
using QBittorrent.Client;
using ReactiveUI;

namespace qBittorrentCompanion.Helpers
{
    public partial class SelectableCategory(Category category) : ReactiveObject
    {
        private readonly Category _category = category;

        public string Name => _category.Name;

        [AutoPropertyChanged]
        private bool _isSelected;
    }
}
