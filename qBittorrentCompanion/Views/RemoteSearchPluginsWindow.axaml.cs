using qBittorrentCompanion.ViewModels;

namespace qBittorrentCompanion.Views
{
    public partial class RemoteSearchPluginsWindow : EscIcoWindow
    {
        public RemoteSearchPluginsWindow()
        {
            InitializeComponent();
            this.DataContext = new RemoteSearchPluginsViewModel();
        }
    }
}
