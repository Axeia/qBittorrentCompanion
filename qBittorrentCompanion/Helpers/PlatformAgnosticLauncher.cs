using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;


namespace qBittorrentCompanion.Helpers
{
    public static class PlatformAgnosticLauncher
    {
        public static async Task<bool> LaunchFileAsync(Visual? visual, string filePath)
        {
            if (TopLevel.GetTopLevel(visual) is { } topLevel)
            {
                // Check if it's a file
                if (File.Exists(filePath))
                {
                    var file = await topLevel.StorageProvider.TryGetFileFromPathAsync(new Uri(filePath));

                    if (file is null)
                    {
                        Debug.WriteLine($"file supposedly exists but could not get it through storageprovider: {filePath}");
                        return false;
                    }
                    else
                        return await topLevel.Launcher.LaunchFileAsync(file);
                }
            }
            else
            {
                Debug.WriteLine("LaunchFileAsync didn't receive a valid visual");
            }

            return false;
        }

        public static async Task<bool> LaunchDirectoryAsync(Visual? visual, string directoryPath)
        {
            if (visual is null)
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime icdsal)
                    visual = icdsal.MainWindow;

            if (TopLevel.GetTopLevel(visual) is { } topLevel)
            {
                if (Directory.Exists(directoryPath))
                {
                    var directory = new DirectoryInfo(directoryPath);
                    return await topLevel.Launcher.LaunchDirectoryInfoAsync(directory);
                }
            }
            else
            {
                Debug.WriteLine("LaunchDirectoryAsync didn't receive a valid visual");
            }

            return false;
        }

        /// <summary>
        /// <list type="bullet">
        /// <item>
        /// <term>The given path leads to a file</term>
        /// <description>open the containing directory in the explorer with the file pre-selected.</description></item>
        /// <item>
        /// <term>The given path is a directory</term>
        /// <description>open it in the explorer</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="absolutePath">Path to a location accessible on this system</param>
        public static bool LaunchDirectoryAndSelectFile(string absolutePath)
        {
            if (OperatingSystem.IsWindows())
            {
                Debug.WriteLine($"Opening explorer with: {absolutePath}");
                if (File.Exists(absolutePath))
                {
                    Process.Start("explorer.exe", "/select, \"" + absolutePath + "\"");
                    return true;
                }
                else if (Directory.Exists(absolutePath))
                {
                    Process.Start("explorer.exe", absolutePath);
                    return true;
                }
                else
                {
                    Debug.WriteLine($"{absolutePath} doesn't exist! Unable to launch");
                    return false;
                }
            }
            else if (OperatingSystem.IsLinux())
            {
                if (File.Exists(absolutePath))
                {
                    Process.Start("xdg-open", Path.GetDirectoryName(absolutePath)!);
                    return true;
                }
                else if (Directory.Exists(absolutePath))
                {
                    Process.Start("xdg-open", absolutePath);
                    return true;
                }
                else
                {
                    Debug.WriteLine($"{absolutePath} doesn't exist! Unable to launch");
                    return false;
                }
            }
            else if (OperatingSystem.IsMacOS())
            {
                Debug.WriteLine($"Opening Finder with: {absolutePath}");
                if (File.Exists(absolutePath)  || Directory.Exists(absolutePath))
                {
                    Process.Start("open", absolutePath);
                    return true;
                }
                else
                {
                    Debug.WriteLine($"{absolutePath} doesn't exist! Unable to launch");
                    return false;
                }
            }

            return false;
        }
    }

}
