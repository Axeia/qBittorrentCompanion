using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Views.LocalSettings
{
    public class SetDirectoryViewBase : UserControl
    {
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

        protected void ChangeControlInTagToSelectedFolder_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button changeFolderButton && changeFolderButton.Tag is Control control)
            {
                _ = OpenDirectory(control);
            }
        }
    }
}
