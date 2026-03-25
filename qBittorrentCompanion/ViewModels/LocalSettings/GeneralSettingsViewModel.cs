using Avalonia.Controls;
using qBittorrentCompanion.Services;
using ReactiveUI;

namespace qBittorrentCompanion.ViewModels.LocalSettings
{
    public class GeneralSettingsViewModel : ViewModelBase
    {
        private bool _bypasssDownloadWindow = Design.IsDesignMode || ConfigService.BypassDownloadWindow;
        public bool BypassDownloadWindow
        {
            get => _bypasssDownloadWindow;
            set
            {
                if (value != _bypasssDownloadWindow)
                {
                    ConfigService.BypassDownloadWindow = value;
                    this.RaisePropertyChanged(nameof(BypassDownloadWindow));
                }
            }
        }

        private bool _showUploadDownloadStatusOnIcon = Design.IsDesignMode || ConfigService.ShowUploadDownloadStatusOnIcon;
        public bool ShowUploadDownloadStatusOnIcon
        {
            get => _showUploadDownloadStatusOnIcon;
            set
            {
                if (value != _showUploadDownloadStatusOnIcon)
                {
                    ConfigService.ShowUploadDownloadStatusOnIcon = value;
                    _showUploadDownloadStatusOnIcon = value;
                    if (App.Current is App app)
                    {
                        app.ShowUploadDownloadStatusOnIcon = value;
                    }
                }
            }
        }
    }
}