using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Styling;
using Avalonia.Threading;
using qBittorrentCompanion.ViewModels.LocalSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            Loaded += LocalSettingsWindow_Loaded;
            //PreviewBgColorPicker.Color = Avalonia.Media.Colors.Transparent;
        }

        private void LocalSettingsWindow_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var canvas = TabStripBackgroundCanvas;
            var svgs = canvas.Children.OfType<Avalonia.Svg.Skia.Svg>();
            foreach (var svg in svgs)
            {
                if (svg.Tag is string tagValue && int.TryParse(tagValue, out int delaySeconds))
                {
                    // Start each path's animation after its delay
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(delaySeconds * 1000); // Convert to milliseconds

                        // Add the class on UI thread to trigger animation
                        _ = Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            svg.Classes.Add("SpinLogo");
                        });
                    });
                }
            }
        }
    }
}
