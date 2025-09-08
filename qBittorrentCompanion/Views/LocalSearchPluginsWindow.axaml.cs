using qBittorrentCompanion.ViewModels;

namespace qBittorrentCompanion.Views
{
    public partial class LocalSearchPluginsWindow : EscIcoWindow
    {
        public LocalSearchPluginsWindow()
        {
            InitializeComponent();
            this.DataContext = new LocalSearchPluginsViewModel();
        }
    }
}
