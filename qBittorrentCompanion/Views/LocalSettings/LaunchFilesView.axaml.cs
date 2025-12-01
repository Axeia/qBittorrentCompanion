using qBittorrentCompanion.ViewModels.LocalSettings;

namespace qBittorrentCompanion.Views.LocalSettings
{
    public partial class LaunchFilesView : SetDirectoryViewBase
    {
        public LaunchFilesView()
        {
            InitializeComponent();
            LaunchFilesViewModel dvm = new();
            DataContext = dvm;
        }
    }
}