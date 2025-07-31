using AutoPropertyChangedGenerator;
using QBittorrent.Client;
using ReactiveUI;

namespace qBittorrentCompanion.ViewModels
{
    public partial class CategoryCountViewModel(Category category) : ReactiveObject
    {
        [AutoProxyPropertyChanged(nameof(Category.Name))]
        [AutoProxyPropertyChanged(nameof(Category.SavePath))]
        private readonly Category _category = category;

        public bool HasPath => _category != null && !string.IsNullOrEmpty(_category.SavePath);

        public override string ToString()
        {
            return SavePath ?? string.Empty;
        }

        [AutoPropertyChanged]
        private int _count = 0;

        public bool IsEditable { get; set; } = true;
    }
}
