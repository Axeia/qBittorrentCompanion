using Avalonia.Controls;
using qBittorrentCompanion.ViewModels.LocalSettings;

namespace qBittorrentCompanion.Views.LocalSettings
{
    public partial class LaunchFilesView : SetDirectoryViewBase
    {
        public LaunchFilesView()
        {
            InitializeComponent();
            LaunchFilesViewModel dvm = new();
            DataContext = dvm;
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