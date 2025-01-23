using qBittorrentCompanion.ViewModels;

namespace qBittorrentCompanion.Views
{
    public partial class RenameTorrentFilesWindow : EscIcoWindow
    {
        private TorrentInfoViewModel? _torrentInfoViewModel;

        public RenameTorrentFilesWindow()
        {
            InitializeComponent();
        }

        public RenameTorrentFilesWindow(TorrentInfoViewModel torrentInfoViewModel)
        {
            this._torrentInfoViewModel = torrentInfoViewModel;
            var temp = new RenameTorrentFilesWindowViewModel(torrentInfoViewModel.Hash);
            DataContext = temp;
            InitializeComponent();
        }
    }
}