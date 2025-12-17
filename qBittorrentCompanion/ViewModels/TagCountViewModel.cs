using RaiseChangeGenerator;

namespace qBittorrentCompanion.ViewModels
{
    public partial class TagCountViewModel : ViewModelBase
    {
        [RaiseChange]
        private string _tag = string.Empty;
        [RaiseChange]
        private int _count = 0;

        public TagCountViewModel(string tag)
        {
            Tag = tag;
        }

        public bool IsEditable { get; set; } = true;
    }
}
