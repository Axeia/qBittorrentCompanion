using qBittorrentCompanion.ViewModels.LocalSettings;

namespace qBittorrentCompanion.Views
{
    /// <summary>
    /// Allow setting of the (likely networked) directory the downloads are saved to
    /// </summary>
    public partial class LocalSettingsWindow : IcoWindow
    {
        private LocalSettingsWindowViewModel _localSettingsWindowViewModel;

        public LocalSettingsWindow()
        {
            InitializeComponent();
            //Closing += DownloadDirectoryWindow_Closing;
            //Loaded += DownloadDirectoryWindow_Loaded;

            //LoadColorsFromConfig(); // Might undo previous step
            //MatchColorPickersToCanvas();
            _localSettingsWindowViewModel = new LocalSettingsWindowViewModel();
            this.DataContext = _localSettingsWindowViewModel;
            //PreviewBgColorPicker.Color = Avalonia.Media.Colors.Transparent;
        }
    }
}
