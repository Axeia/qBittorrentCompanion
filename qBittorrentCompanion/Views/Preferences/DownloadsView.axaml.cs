using Avalonia.Controls;
using qBittorrentCompanion.ViewModels;
using System.Diagnostics;

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
