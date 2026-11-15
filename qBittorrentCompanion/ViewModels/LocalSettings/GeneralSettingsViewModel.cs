using Avalonia.Controls;
using qbittorrentCompanion.Helpers;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels.LocalSettings
{
    public class GeneralSettingsViewModel : ViewModelBase
    {
        public ReactiveCommand<Unit, Unit> SetAsDefaultMagnetHandlerCommand { get; private set; }
        public GeneralSettingsViewModel()
        {
            SetAsDefaultMagnetHandlerCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if (OperatingSystem.IsLinux())
                    await Task.Run(() => LinuxIntegration.Register(setAsDefault: true));
                else if (OperatingSystem.IsWindows())
                {
                    await Task.Run(WindowsIntegration.Register);
                    WindowsIntegration.OpenDefaultAppsSettings();
                }
            });
        }

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