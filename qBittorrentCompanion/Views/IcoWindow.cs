using Avalonia.Controls;
using Avalonia.Platform;
using System;
using System.Diagnostics;
using System.IO;

namespace qBittorrentCompanion.Views
{
    public class IcoWindow : Window
    {
        protected static string AppPath => System.AppContext.BaseDirectory;
        protected static string CustomIcoPath =>
            Path.Combine(AppPath, "qbc-custom-logo.ico");
        public static string IcoPath =>
            File.Exists(CustomIcoPath)
                ? CustomIcoPath
                : Path.Combine(AppPath, "qbc-logo.ico");

        public IcoWindow()
        {
            try
            {
                var uri = new Uri("avares://qBittorrentCompanion/Assets/qbc-logo.ico");
                using var stream = AssetLoader.Open(uri);
                this.Icon = new WindowIcon(stream);
            }
            catch (FileNotFoundException ex)
            {
                Debug.WriteLine($"Could not find qbc-logo.ico : {ex.Message}");
            }
        }
    }
}
