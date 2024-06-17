using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Diagnostics;

namespace qBittorrentCompanion.Views
{
    public partial class RemoveTorrentWindow : Window
    {
        public RemoveTorrentWindow()
        {
            InitializeComponent();
        }
        public void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void OnRemoveClicked(object sender, RoutedEventArgs e)
        {
            var mainWindow = this.Owner as MainWindow;
            if (mainWindow is not null)
                mainWindow.HttpRemoveTorrents(DeleteFilesCheckbox.IsChecked ?? false);
            else
                Debug.WriteLine("No mainWindow set");

            this.Close();
        }

    }
}
