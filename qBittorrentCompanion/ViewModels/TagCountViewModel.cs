using AutoPropertyChangedGenerator;
using ReactiveUI;

namespace qBittorrentCompanion.ViewModels
{
    public partial class TagCountViewModel : ViewModelBase
    {
        [AutoPropertyChanged]
        private string _tag = string.Empty;
        [AutoPropertyChanged]
        private int _count = 0;

        public TagCountViewModel(string tag)
        {
            Tag = tag;
        }

        public bool IsEditable { get; set; } = true;
    }
}
