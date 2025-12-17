using QBittorrent.Client;
using RaiseChangeGenerator;
using ReactiveUI;

namespace qBittorrentCompanion.ViewModels
{
    public partial class CategoryCountViewModel(Category category) : ReactiveObject
    {
        [RaiseChangeProxy(nameof(Category.Name))]
        [RaiseChangeProxy(nameof(Category.SavePath))]
        private readonly Category _category = category;

        public bool HasPath => _category != null && !string.IsNullOrEmpty(_category.SavePath);

        public override string ToString()
        {
            return SavePath ?? string.Empty;
        }

        [RaiseChange]
        private int _count = 0;

        public bool IsEditable { get; set; } = true;
    }
}
