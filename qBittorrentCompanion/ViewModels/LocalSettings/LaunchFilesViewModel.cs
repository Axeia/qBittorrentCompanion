using Avalonia.Controls;
using qBittorrentCompanion.Services;
using ReactiveUI;

namespace qBittorrentCompanion.ViewModels.LocalSettings
{
    public class LaunchFilesViewModel : ViewModelBase
    {
        private string _downloadDirectoryPath = Design.IsDesignMode
            ? string.Empty
            : ConfigService.DownloadDirectory;
        public string DownloadDirectoryPath
        {
            get => _downloadDirectoryPath;
            set
            {
                if (value != _downloadDirectoryPath)
                {
                    _downloadDirectoryPath = value;
                    ConfigService.DownloadDirectory = value;
                    this.RaisePropertyChanged(nameof(DownloadDirectoryPath));
                }
            }
        }


        private string _temporaryDirectoryPath = Design.IsDesignMode
            ? string.Empty
            : ConfigService.TemporaryDirectory;
        public string TemporaryDirectoryPath
        {
            get => _temporaryDirectoryPath;
            set
            {
                if (value != _temporaryDirectoryPath)
                {
                    _temporaryDirectoryPath = value;
                    ConfigService.TemporaryDirectory = value;
                    this.RaisePropertyChanged(nameof(TemporaryDirectoryPath));
                }
            }
        }

        private bool _transfersLaunchFileDirectly = Design.IsDesignMode || ConfigService.TorrentContentLaunchFileDirectly;
        public bool TransfersLaunchFileDirectly
        {
            get => _transfersLaunchFileDirectly;
            set
            {
                if (value != _transfersLaunchFileDirectly)
                {
                    _transfersLaunchFileDirectly = value;
                    ConfigService.TransfersLaunchFileDirectly = value;
                    this.RaisePropertyChanged(nameof(TransfersLaunchFileDirectly));
                }
            }
        }

        private bool _torrentContentLaunchFileDirectly = Design.IsDesignMode || ConfigService.TorrentContentLaunchFileDirectly;
        public bool TorrentContentLaunchFileDirectly
        {
            get => _torrentContentLaunchFileDirectly;
            set
            {
                if (value != _torrentContentLaunchFileDirectly)
                {
                    _torrentContentLaunchFileDirectly = value;
                    ConfigService.TorrentContentLaunchFileDirectly = value;
                    this.RaisePropertyChanged(nameof(TorrentContentLaunchFileDirectly));
                }
            }
        }
    }
}