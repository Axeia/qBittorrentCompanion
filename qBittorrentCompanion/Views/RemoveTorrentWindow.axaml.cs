using Avalonia.Controls;
using Avalonia.Interactivity;
using QBittorrent.Client;
using qBittorrentCompanion.ViewModels;
using System.Diagnostics;

namespace qBittorrentCompanion.Views
{
    public enum DeleteBy
    {
        Selected,
        Category,
        Tag,
        Tracker
    }

    public partial class RemoveTorrentWindow : IcoWindow
    {
        private DeleteBy _deleteBy;

        //Needed for previewer
        public RemoveTorrentWindow()
        {
            InitializeComponent();
        }

        public RemoveTorrentWindow(DeleteBy deleteBy)
        {
            _deleteBy = deleteBy;
            InitializeComponent();

            switch(deleteBy)
            {
                case DeleteBy.Category:
                    TitleTextBlock.Text += " for category";
                    break;
                case DeleteBy.Tag:
                    TitleTextBlock.Text += " for tag";
                    break;
                case DeleteBy.Tracker:
                    TitleTextBlock.Text += " for tracker";
                    break;
            };
        }

        public void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void OnRemoveClicked(object sender, RoutedEventArgs e)
        {
            if(Owner is MainWindow mainWindow 
                && mainWindow.DataContext is MainWindowViewModel mwvm 
                && mwvm.TorrentsViewModel is TorrentsViewModel tvm)
            {
                _ = _deleteBy switch
                {
                    DeleteBy.Category   => tvm.DeleteTorrentsForCategoryAsync(DeleteFilesCheckBox.IsChecked ?? false),
                    DeleteBy.Tag        => tvm.DeleteTorrentsForTagAsync(DeleteFilesCheckBox.IsChecked ?? false),
                    DeleteBy.Tracker    => tvm.DeleteTorrentsForTrackerAsync(DeleteFilesCheckBox.IsChecked ?? false),
                    DeleteBy.Selected   => tvm.DeleteSelectedTorrentsAsync(DeleteFilesCheckBox.IsChecked ?? false),
                    _                   => tvm.DeleteSelectedTorrentsAsync(DeleteFilesCheckBox.IsChecked ?? false)
                };
            }
            else
                Debug.WriteLine("No mainWindow set");

            this.Close();
        }

        private void CheckBox_Loaded(object? sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
                checkBox.Focus();
        }
    }
}
