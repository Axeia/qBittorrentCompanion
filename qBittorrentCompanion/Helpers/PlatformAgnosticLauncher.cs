using Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Helpers
{
    public static class PlatformAgnosticLauncher
    {
        /// <summary>
        /// Determines what the current platform is and launches the default file/directory explorer
        /// </summary>
        /// <param name="absolutePath">Path to a location accessible on this system</param>
        public static void OpenDirectory(string absolutePath)
        {
            if (OperatingSystem.IsWindows())
            {
                Debug.WriteLine($"Opening explorer with: {absolutePath}");
                if (Directory.Exists(absolutePath))
                {
                    Process.Start("explorer.exe", absolutePath);
                }
                else
                {
                    Debug.WriteLine($"{absolutePath} doesn't exist! Unable to launch");
                }
            }
        }
        public static void OpenFile(string absolutePath)
        {
            if (OperatingSystem.IsWindows())
            {
                Debug.WriteLine($"Opening explorer with file: {absolutePath}");
                if (File.Exists(absolutePath))
                {
                    Process.Start("explorer.exe", absolutePath);
                }
                else
                {
                    Debug.WriteLine($"{absolutePath} doesn't exist! Unable to launch");
                }
            }
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
        public static void OpenDirectoryAndSelectFile(string absolutePath)
        {
            if (OperatingSystem.IsWindows())
            {
                Debug.WriteLine($"Opening explorer with: {absolutePath}");
                if (File.Exists(absolutePath))
                {
                    Process.Start("explorer.exe", "/select, \"" + absolutePath + "\"");
                }
                else if (Directory.Exists(absolutePath))
                {
                    Process.Start("explorer.exe", absolutePath);
                }
                else
                {
                    Debug.WriteLine($"{absolutePath} doesn't exist! Unable to launch");
                }

            }
        }
    }

}
