using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using qBittorrentCompanion.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Views.LocalSettings
{
    public partial class DirectoriesView : UserControl
    {
        public DirectoriesView()
        {
            InitializeComponent();
        }

        private void DownloadDirectoryWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            // Must check for designmode, if it's active ConfigService access needs to be avoided.
            DownloadDirectoryTextBox.Text = Design.IsDesignMode ? "/wherever/whenever" : ConfigService.DownloadDirectory;
            DownloadDirectoryTextBox.IsEnabled = DownloadDirectoryTextBox.Text != "";

            TemporaryDirectoryTextBox.Text = Design.IsDesignMode ? "/wherever/temporary" : ConfigService.TemporaryDirectory;
            TemporaryDirectoryTextBox.IsEnabled = TemporaryDirectoryTextBox.Text != "";
        }

        /// <summary>
        /// Upon closing the window save the Download & Temporary directories.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void DownloadDirectoryWindow_Closing(object? sender, WindowClosingEventArgs e)
        //{
        //    Closing -= DownloadDirectoryWindow_Closing;

        //    // Must check for designmode or ConfigService access bugs out AXAML preview
        //    if (DownloadDirectoryTextBox.Text != null && !Design.IsDesignMode
        //      && DownloadDirectoryTextBox.Text != ConfigService.DownloadDirectory)
        //    {
        //        ConfigService.DownloadDirectory = DownloadDirectoryTextBox.Text;
        //    }

        //    if (TemporaryDirectoryTextBox.Text != null && !Design.IsDesignMode
        //      && TemporaryDirectoryTextBox.Text != ConfigService.TemporaryDirectory)
        //    {
        //        ConfigService.TemporaryDirectory = TemporaryDirectoryTextBox.Text;
        //    }
        //}

        private void DownloadDirectoryButton_Click(object? sender, RoutedEventArgs e)
        {
            _ = OpenDirectory(DownloadDirectoryTextBox);
        }
        private void TemporaryDirectoryButton_Click(object? sender, RoutedEventArgs e)
        {
            _ = OpenDirectory(TemporaryDirectoryTextBox);
        }
        public async Task OpenDirectory(TextBox tb)
        {
            if (TopLevel.GetTopLevel(this)?.StorageProvider is IStorageProvider storageProvider && storageProvider.CanPickFolder)
            {
                IStorageFolder? suggestedStartLocation = null;
                if (!string.IsNullOrEmpty(tb.Text))
                {
                    suggestedStartLocation = await storageProvider.TryGetFolderFromPathAsync(tb.Text);
                }

                IReadOnlyList<IStorageFolder> folders = await storageProvider.OpenFolderPickerAsync(
                    new FolderPickerOpenOptions() { AllowMultiple = false, SuggestedStartLocation = suggestedStartLocation }
                );

                if (folders.Count > 0)
                {
                    tb.Text = folders[0].TryGetLocalPath();
                }
            }
            tb.IsEnabled = true;
        }
    }
}