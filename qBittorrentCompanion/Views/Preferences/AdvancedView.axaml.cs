
using Avalonia.Controls;
using qBittorrentCompanion.ViewModels;

namespace qBittorrentCompanion.Views.Preferences
{
    public partial class AdvancedView : UserControl
    {
        public AdvancedView()
        {
            InitializeComponent();
            if(Design.IsDesignMode)
            {
                DataContext = new PreferencesWindowViewModel();
            }
        }
    }
}
