using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using qBittorrentCompanion.ViewModels.LocalSettings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Views.LocalSettings
{
    public partial class MonitorDirectoriesView : SetDirectoryViewBase
    {
        public MonitorDirectoriesView()
        {
            InitializeComponent();
            MonitorDirectoriesViewModel dvm = new();
            DataContext = dvm;
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
                if (DataContext is MonitorDirectoriesViewModel dvm
                    && folder.TryGetLocalPath() is string localPath)
                {
                    dvm.MonitoredDirectories.Add(new SelectableMonitoredDirectoryViewModel(
                        localPath,
                        dvm.AddDirectoryDefaultAction
                    ));
                }
            }
        }
    }
}