using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using qBittorrentCompanion.Converters;
using qBittorrentCompanion.ViewModels;
using qBittorrentCompanion.Views;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using System.Threading;

namespace qBittorrentCompanion
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            TorrentInfoViewModel.downloadingIcon = FindGeometry("arrow_download_regular");
            TorrentInfoViewModel.pausedIcon = FindGeometry("pause_regular");
            TorrentInfoViewModel.queuedIcon = FindGeometry("clock_regular");
            TorrentInfoViewModel.uploadingIcon = FindGeometry("arrow_upload_regular");
            TorrentInfoViewModel.errorIcon = FindGeometry("error_circle_regular");
            TorrentInfoViewModel.diskIcon = FindGeometry("storage_regular");
            TorrentInfoViewModel.unknownIcon = FindGeometry("question_circular_regular");
            TorrentInfoViewModel.checkingIcon = FindGeometry("checkmark_regular");
            TorrentInfoViewModel.metadataIcon = FindGeometry("missing_metadata_regular");

            ServerStateViewModel.ConnectedIcon = FindGeometry("globe_regular");
            ServerStateViewModel.OfflineIcon = FindGeometry("cloud_offline_regular");
            ServerStateViewModel.FirewalledIcon = FindGeometry("lock_shield_regular");
            ServerStateViewModel.StalledIcon = FindGeometry("clock_regular");
            ServerStateViewModel.UnknownICon = FindGeometry("question_circle_regular");

            TorrentContentViewModel.fileIcon = FindGeometry("document_regular");
            TorrentContentViewModel.folderIcon = FindGeometry("folder_regular");

            BoolToIconConverter.trueIcon = FindGeometry("checkmark_regular");
            BoolToIconConverter.falseIcon = FindGeometry("dismiss_circle_regular");
            BoolToIconConverter.unclearIcon = FindGeometry("question_circle_regular");

            //TorrentContentViewModel.fileIcon = FindGeometry("document_regular");
            //TorrentContentViewModel.folderIcon = FindGeometry("folder_regular");

        }

        private Geometry FindGeometry(string name)
        {
            if (Application.Current is not null)
            {
                var resource = Application.Current.FindResource(name);
                if (resource is Geometry geometry)
                    return geometry;
            }
            return Geometry.Parse("");
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };

                var args = desktop.Args ?? [];
                foreach(var arg in args)
                    ((MainWindow)desktop.MainWindow).AddToFileQueue(arg);

            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}