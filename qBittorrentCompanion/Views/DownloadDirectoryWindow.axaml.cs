using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using qBittorrentCompanion.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Views
{
    /// <summary>
    /// Allow setting of the (likely networked) directory the downloads are saved to
    /// </summary>
    public partial class DownloadDirectoryWindow : Window
    {
        public DownloadDirectoryWindow()
        {
            InitializeComponent();
            Closing += DownloadDirectoryWindow_Closing;
            Loaded += DownloadDirectoryWindow_Loaded;
        }

        private void DownloadDirectoryWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            DownloadDirectoryTextBox.Text = ConfigService.DownloadDirectory;
            DownloadDirectoryTextBox.IsEnabled = DownloadDirectoryTextBox.Text != "";

            TemporaryDirectoryTextBox.Text = ConfigService.TemporaryDirectory;
            TemporaryDirectoryTextBox.IsEnabled = TemporaryDirectoryTextBox.Text != "";
        }

        /// <summary>
        /// Upon closing the window save the Download & Temporary directories.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadDirectoryWindow_Closing(object? sender, WindowClosingEventArgs e)
        {
            Closing -= DownloadDirectoryWindow_Closing;

            if (DownloadDirectoryTextBox.Text != null 
            && DownloadDirectoryTextBox.Text != ConfigService.DownloadDirectory)
            {
                ConfigService.DownloadDirectory = DownloadDirectoryTextBox.Text;
            }

            if (TemporaryDirectoryTextBox.Text != null
            && TemporaryDirectoryTextBox.Text != ConfigService.TemporaryDirectory)
            {
                ConfigService.TemporaryDirectory = TemporaryDirectoryTextBox.Text;
            }
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            this.Close(); 
        }

        private void DownloadDirectoryButton_Click(object? sender, RoutedEventArgs e)
        {
            _= OpenDirectory(DownloadDirectoryTextBox);
        }
        private void TemporaryDirectoryButton_Click(object? sender, RoutedEventArgs e)
        {
            _ = OpenDirectory(TemporaryDirectoryTextBox);
        }
        public async Task OpenDirectory(TextBox tb)
        {
            var storage = StorageProvider;
            if (storage.CanPickFolder)
            {
                IStorageFolder? suggestedStartLocation = null;
                if (!string.IsNullOrEmpty(tb.Text))
                {
                    suggestedStartLocation = await storage.TryGetFolderFromPathAsync(tb.Text);
                }

                IReadOnlyList<IStorageFolder> folders = await StorageProvider.OpenFolderPickerAsync(
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
