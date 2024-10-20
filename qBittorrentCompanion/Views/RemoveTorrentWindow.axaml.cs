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

        private string _category = "";
        public RemoveTorrentWindow(string category)
        {
            InitializeComponent();
            _category = category;
        }

        public void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void OnRemoveClicked(object sender, RoutedEventArgs e)
        {
            var mainWindow = this.Owner as MainWindow;
            if (mainWindow is not null)
            {
                // Removes torrents belonging to category
                if(_category != string.Empty)
                {
                    mainWindow.HttpRemoveTorrentsForCategoryClicked(DeleteFilesCheckBox.IsChecked ?? false);
                }
                else // Removes selected torrents
                    mainWindow.HttpRemoveTorrentsClicked(DeleteFilesCheckbox.IsChecked ?? false);
            }
            else
                Debug.WriteLine("No mainWindow set");

            this.Close();
        }

    }
}
