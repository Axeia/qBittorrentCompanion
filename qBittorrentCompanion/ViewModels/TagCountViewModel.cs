using ReactiveUI;

namespace qBittorrentCompanion.ViewModels
{
    public class TagCountViewModel : ViewModelBase
    {
        private string _tag = string.Empty;
        public string Tag
        {
            get => _tag;
            set => this.RaiseAndSetIfChanged(ref _tag, value);
        }

        private int _count = 0;
        public int Count
        {
            get => _count;
            set => this.RaiseAndSetIfChanged(ref _count, value);
        }

        public TagCountViewModel(string tag)
        {
            Tag = tag;
        }
    }

}
