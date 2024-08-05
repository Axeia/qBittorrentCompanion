using Avalonia.Controls;
using qBittorrentCompanion.ViewModels;

namespace qBittorrentCompanion.Views.Preferences
{
    public partial class BitTorrentView : UserControl
    {
        public BitTorrentView()
        {
            InitializeComponent();
            if (Design.IsDesignMode)
            {
                DataContext = new PreferencesWindowViewModel();
            }
        }
    }
}
