using Avalonia.Controls;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace qBittorrentCompanion.Views
{
    public class IcoWindow : Window
    {
        protected static string AppPath =>
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        protected static string CustomIcoPath =>
            Path.Combine(AppPath, "qbc-custom-logo.ico");
        public static string IcoPath =>
            File.Exists(CustomIcoPath)
                ? CustomIcoPath
                : Path.Combine(AppPath, "qbc-logo.ico");

        public IcoWindow()
        {   // Set the window icon
            this.Icon = new WindowIcon(IcoPath);
        }
    }
}
