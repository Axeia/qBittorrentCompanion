using Avalonia.Controls;
using qBittorrentCompanion.ViewModels;

namespace qBittorrentCompanion.Views.Preferences
{
    public partial class LanguageAndLogView : UserControl
    {
        public LanguageAndLogView()
        {
            InitializeComponent();
            if (Design.IsDesignMode)
            {
                DataContext = new PreferencesWindowViewModel();
                LanguageComboBox.SelectedIndex = 11;
            }
        }
    }
}
