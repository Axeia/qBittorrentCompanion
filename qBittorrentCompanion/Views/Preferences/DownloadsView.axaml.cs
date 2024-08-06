using Avalonia.Controls;
using qBittorrentCompanion.ViewModels;

namespace qBittorrentCompanion.Views.Preferences
{
    public partial class DownloadsView : UserControl
    {
        public DownloadsView()
        {
            InitializeComponent();
            if(Design.IsDesignMode)
            {
                DataContext = new PreferencesWindowViewModel();
            }
        }
    }
}
