using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using qBittorrentCompanion.Services;
using qBittorrentCompanion.ViewModels.LocalSettings;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Views.LocalSettings
{
    public partial class DirectoriesView : UserControl
    {
        public DirectoriesView()
        {
            InitializeComponent();
            DirectoriesViewModel dvm = new();
            DataContext = dvm;
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

        public async Task OpenDirectory(Control control)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is null || !topLevel.StorageProvider.CanPickFolder)
                return;

            IStorageProvider storageProvider = topLevel.StorageProvider;
            IStorageFolder? suggestedStartLocation = null;

            string? currentText = control switch
            {
                TextBox tb => tb.Text,
                TextBlock tb => tb.Text,
                _ => null
            };

            if (!string.IsNullOrWhiteSpace(currentText))
            {
                suggestedStartLocation = await storageProvider.TryGetFolderFromPathAsync(currentText);
            }

            IReadOnlyList<IStorageFolder> folders = await storageProvider.OpenFolderPickerAsync(
                new FolderPickerOpenOptions
                {
                    AllowMultiple = false,
                    SuggestedStartLocation = suggestedStartLocation
                });

            if (folders.Count > 0)
            {
                string? newPath = folders[0].TryGetLocalPath();
                switch (control)
                {
                    case TextBox tb:
                        tb.Text = newPath;
                        break;
                    case TextBlock tb:
                        tb.Text = newPath;
                        break;
                }
            }

            control.IsEnabled = true;
        }

        /// <summary>
        /// Main button click to add a new entry (a monitored directory)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddFolderToMonitorButton_Click(object? sender, RoutedEventArgs e)
        {
            _ = AddFoldersToMonitorAsync();
        }

        private async Task AddFoldersToMonitorAsync()
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is null)
                return;

            FolderPickerOpenOptions folderPickerOptions = new()
            {
                Title = "Add folder(s) to monitor for .torrent files",
                AllowMultiple = true
            };

            var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(folderPickerOptions);
            foreach (var folder in folders)
            {
                if (DataContext is DirectoriesViewModel dvm
                    && folder.TryGetLocalPath() is string localPath)
                {
                    dvm.MonitoredDirectories.Add(new SelectableMonitoredDirectoryViewModel(
                        localPath,
                        dvm.AddDirectoryDefaultAction
                    ));
                }
            }
        }

        private void ChangeControlInTagToSelectedFolder_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button changeFolderButton && changeFolderButton.Tag is Control control)
            {
                _ = OpenDirectory(control);
            }
        }
    }
}