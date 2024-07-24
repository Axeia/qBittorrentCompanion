using Avalonia.Controls;
using QBittorrent.Client;
using qBittorrentCompanion.Services;
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

        private void SavePreferences_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
        }
    }
}
