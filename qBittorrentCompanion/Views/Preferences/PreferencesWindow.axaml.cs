using Avalonia.Controls;
using qBittorrentCompanion.ViewModels;

namespace qBittorrentCompanion.Views.Preferences
{
    public partial class PreferencesWindow : Window
    {
        public PreferencesWindow()
        {
            InitializeComponent();
            var prefVm = new PreferencesWindowViewModel();
            prefVm.SelectedTabItem = (TabItem)PreferencesTabControl.SelectedItem!;
            DataContext = prefVm;
        }

    }
}