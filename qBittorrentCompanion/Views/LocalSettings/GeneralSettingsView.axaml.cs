using Avalonia.Controls;
using qBittorrentCompanion.ViewModels;
using qBittorrentCompanion.ViewModels.LocalSettings;

namespace qBittorrentCompanion.Views.LocalSettings
{
    public partial class GeneralSettingsView : UserControl
    {
        public GeneralSettingsView()
        {
            InitializeComponent();
            GeneralSettingsViewModel gsvm = new();
            DataContext = gsvm;
        }

        private void ClearFieldButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (sender is Button changeFolderButton && changeFolderButton.Tag is TextBox textBox)
            {
                textBox.Clear();
            }
        }
    }
}