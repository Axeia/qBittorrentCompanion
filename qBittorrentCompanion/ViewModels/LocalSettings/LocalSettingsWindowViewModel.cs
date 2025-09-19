using AutoPropertyChangedGenerator;
using Avalonia.Controls;
using FluentIcons.Common;
using qBittorrentCompanion.Services;
using System.Collections.Generic;

namespace qBittorrentCompanion.ViewModels.LocalSettings
{
    public record TabItemRecord(Symbol Symbol, string HeaderText);

    public partial class LocalSettingsWindowViewModel : ViewModelBase
    {
        [AutoPropertyChanged]
        private TabItemRecord? _selectedTab;
        [AutoPropertyChanged]
        private List<TabItemRecord> _tabs = [            
            new TabItemRecord(Symbol.Folder, "Directories"),
            new TabItemRecord(Symbol.Color, "Icon customization"),
            new TabItemRecord(Symbol.Alert, "Notifications"),
            new TabItemRecord(Symbol.WindowNew, "Start up settings")
        ];

        private string _qbDownloadDirectory = Design.IsDesignMode? "/wherever/whenever" : ConfigService.DownloadDirectory;
        public string QbDownloadDirectory
        {
            get => _qbDownloadDirectory;
            set
            {
                if(value != _qbDownloadDirectory)
                {
                    _qbDownloadDirectory = value;
                    ConfigService.DownloadDirectory = value;
                }
            }
        }
    }
}
