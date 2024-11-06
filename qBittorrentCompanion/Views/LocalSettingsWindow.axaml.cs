using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using qBittorrentCompanion.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Media;
using System.Linq;
using Avalonia.Media.Immutable;
using Avalonia;
using Avalonia.Media.Imaging;
using qBittorrentCompanion.Helpers;
using System.Diagnostics;

namespace qBittorrentCompanion.Views
{
    /// <summary>
    /// Allow setting of the (likely networked) directory the downloads are saved to
    /// </summary>
    public partial class LocalSettingsWindow : Window
    {
        private static string CustomAxaml =>
            MainWindow.IcoPath.Replace(".ico", ".axaml");

        public LocalSettingsWindow()
        {
            InitializeComponent();
            Closing += DownloadDirectoryWindow_Closing;
            Loaded += DownloadDirectoryWindow_Loaded;

            LoadCanvasContent();
            LoadColorsFromConfig(); // Might undo previous step
            MatchColorPickersToCanvas();
        }

        private void LoadCanvasContent()
        {
            try
            {
                var xamlUri = new Uri("avares://qBittorrentCompanion/Assets/Logo.axaml");
                var logoCanvasContent = (Canvas)AvaloniaXamlLoader.Load(xamlUri);

                LogoCanvas.Children.Clear();
                LogoCanvas.Children.Add(logoCanvasContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading Canvas content: {ex.Message}");
            }
        }

        private void MatchColorPickersToCanvas()
        {
            MatchColorPickerToCanvas(Q_ColorPicker);
            MatchColorPickerToCanvas(B_ColorPicker);
            MatchColorPickerToCanvas(C_ColorPicker);

            MatchColorPickerToCanvas(From_ColorPicker);
            MatchColorPickerToCanvas(To_ColorPicker);
        }
        
        private void LoadColorsFromConfig()
        {
            if (Design.IsDesignMode)
                return; // There is no ConfigService in design mode, prevent it from throwing errors.

            if( ConfigService.IconColors is string[] colors && colors.Length == 5)
            {
                Debug.WriteLine($"Loading colors from config file: {string.Join(",", colors)}");
                if (LogoCanvas.Children.Count > 0 && LogoCanvas.Children[0] is Canvas theRealCanvas && theRealCanvas.Children.Count > 0)
                {
                    var textBlocks = theRealCanvas.Children.OfType<TextBlock>().ToList();//0 = q, 1 = b
                    var linearGradientBrushes = theRealCanvas.Resources.Values.OfType<LinearGradientBrush>().ToList(); // 0 = background, 1 = C-shape

                    textBlocks[0].Foreground = new SolidColorBrush(Color.Parse(colors[0]));
                    textBlocks[1].Foreground = new SolidColorBrush(Color.Parse(colors[1]));
                    linearGradientBrushes[1].GradientStops[0].Color = Color.Parse(colors[2]);

                    linearGradientBrushes[0].GradientStops[0].Color = Color.Parse(colors[3]);
                    linearGradientBrushes[0].GradientStops[1].Color = Color.Parse(colors[4]);
                }
            }
        }

        private void DownloadDirectoryWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            // Must check for designmode or ConfigService access bugs out AXAML preview
            DownloadDirectoryTextBox.Text = Design.IsDesignMode ? "/wherever/whenever" : ConfigService.DownloadDirectory;
            DownloadDirectoryTextBox.IsEnabled = DownloadDirectoryTextBox.Text != "";

            TemporaryDirectoryTextBox.Text = Design.IsDesignMode ? "/wherever/temporary" : ConfigService.TemporaryDirectory;
            TemporaryDirectoryTextBox.IsEnabled = TemporaryDirectoryTextBox.Text != "";
        }

        /// <summary>
        /// Upon closing the window save the Download & Temporary directories.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadDirectoryWindow_Closing(object? sender, WindowClosingEventArgs e)
        {
            Closing -= DownloadDirectoryWindow_Closing;

            // Must check for designmode or ConfigService access bugs out AXAML preview
            if (DownloadDirectoryTextBox.Text != null && !Design.IsDesignMode
            && DownloadDirectoryTextBox.Text != ConfigService.DownloadDirectory)
            {
                ConfigService.DownloadDirectory = DownloadDirectoryTextBox.Text;
            }

            if (TemporaryDirectoryTextBox.Text != null && !Design.IsDesignMode
            && TemporaryDirectoryTextBox.Text != ConfigService.TemporaryDirectory)
            {
                ConfigService.TemporaryDirectory = TemporaryDirectoryTextBox.Text;
            }
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DownloadDirectoryButton_Click(object? sender, RoutedEventArgs e)
        {
            _ = OpenDirectory(DownloadDirectoryTextBox);
        }
        private void TemporaryDirectoryButton_Click(object? sender, RoutedEventArgs e)
        {
            _ = OpenDirectory(TemporaryDirectoryTextBox);
        }
        public async Task OpenDirectory(TextBox tb)
        {
            var storage = StorageProvider;
            if (storage.CanPickFolder)
            {
                IStorageFolder? suggestedStartLocation = null;
                if (!string.IsNullOrEmpty(tb.Text))
                {
                    suggestedStartLocation = await storage.TryGetFolderFromPathAsync(tb.Text);
                }

                IReadOnlyList<IStorageFolder> folders = await StorageProvider.OpenFolderPickerAsync(
                    new FolderPickerOpenOptions() { AllowMultiple = false, SuggestedStartLocation = suggestedStartLocation }
                );

                if (folders.Count > 0)
                {
                    tb.Text = folders[0].TryGetLocalPath();
                }
            }
            tb.IsEnabled = true;
        }

        private void MatchColorPickerToCanvas(ColorPicker colorPicker)
        {
            if (LogoCanvas.Children.Count > 0 && LogoCanvas.Children[0] is Canvas theRealCanvas && theRealCanvas.Children.Count > 0)
            {
                var textBlocks = theRealCanvas.Children.OfType<TextBlock>().ToList();//0 = q, 1 = b
                var linearGradientBrushes = theRealCanvas.Resources.Values.OfType<LinearGradientBrush>().ToList(); // 0 = background, 1 = C-shape

                if(colorPicker == Q_ColorPicker && textBlocks[0].Foreground is ImmutableSolidColorBrush scbq)
                    Q_ColorPicker.Color = scbq.Color;
                if (colorPicker == B_ColorPicker && textBlocks[1].Foreground is ImmutableSolidColorBrush scbb)
                    B_ColorPicker.Color = scbb.Color;
                if (colorPicker == C_ColorPicker)
                    C_ColorPicker.Color = linearGradientBrushes[1].GradientStops[0].Color;

                if (colorPicker == From_ColorPicker)
                    From_ColorPicker.Color = linearGradientBrushes[0].GradientStops[0].Color;
                if (colorPicker == To_ColorPicker)
                    To_ColorPicker.Color = linearGradientBrushes[0].GradientStops[1].Color;

            }
        }

        private void ColorPicker_ColorChanged(object? sender, ColorChangedEventArgs e)
        {
            if (LogoCanvas.Children.Count > 0 && LogoCanvas.Children[0] is Canvas theRealCanvas && theRealCanvas.Children.Count > 0)
            {
                var textBlocks = theRealCanvas.Children.OfType<TextBlock>().ToList();//0 = q, 1 = b
                var linearGradientBrushes = theRealCanvas.Resources.Values.OfType<LinearGradientBrush>().ToList(); // 0 = background, 1 = C-shape

                if (sender == Q_ColorPicker)
                    textBlocks[0].Foreground = new SolidColorBrush(Q_ColorPicker.Color);
                if (sender == B_ColorPicker)
                    textBlocks[1].Foreground = new SolidColorBrush(B_ColorPicker.Color);
                if (sender == C_ColorPicker)
                    linearGradientBrushes[1].GradientStops[0].Color = C_ColorPicker.Color;

                if (sender == From_ColorPicker)
                    linearGradientBrushes[0].GradientStops[0].Color = From_ColorPicker.Color;
                if (sender == To_ColorPicker)
                    linearGradientBrushes[0].GradientStops[1].Color = To_ColorPicker.Color;
            }
        }

        private void RestoreDefaultIconButton_Click(object? sender, RoutedEventArgs e)
        {
            LoadCanvasContent();
            MatchColorPickersToCanvas();
        }

        private void SaveIconButton_Click(object? sender, RoutedEventArgs e)
        {
            if (Owner is MainWindow mw)
            {
                var theRealCanvas = LogoCanvas.Children[0];
                var size = new Size(theRealCanvas.Width, theRealCanvas.Height);
                var renderSize = new PixelSize((int)theRealCanvas.Width, (int)theRealCanvas.Height);
                var renderBitmap = new RenderTargetBitmap(renderSize, new Vector(96, 96));

                theRealCanvas.Measure(size);
                theRealCanvas.Arrange(new Rect(size));
                renderBitmap.Render(theRealCanvas);

                IcoHelper.SaveIcon(renderBitmap, MainWindow.IcoPath);
                Icon = new WindowIcon(MainWindow.IcoPath);
                mw.Icon = new WindowIcon(MainWindow.IcoPath);

                SaveCanvasColorsToConfig();
            }
        }

        private void SaveCanvasColorsToConfig()
        {
            ConfigService.IconColors = [
                Converters.ColorToHexConverter.ColorToHex(Q_ColorPicker.Color),
                Converters.ColorToHexConverter.ColorToHex(B_ColorPicker.Color), 
                Converters.ColorToHexConverter.ColorToHex(C_ColorPicker.Color),
                Converters.ColorToHexConverter.ColorToHex(From_ColorPicker.Color),
                Converters.ColorToHexConverter.ColorToHex(To_ColorPicker.Color),
            ];
        }
    }
}
