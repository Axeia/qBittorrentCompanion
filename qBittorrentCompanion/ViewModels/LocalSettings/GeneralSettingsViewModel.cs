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
    }
}