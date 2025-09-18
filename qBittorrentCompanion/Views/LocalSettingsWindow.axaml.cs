using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using ExCSS;
using Newtonsoft.Json;
using qBittorrentCompanion.Extensions;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Models;
using qBittorrentCompanion.Services;
using qBittorrentCompanion.ViewModels;
using Svg.Skia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;

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
            Closing += DownloadDirectoryWindow_Closing;
            Loaded += DownloadDirectoryWindow_Loaded;

            LoadColorsFromConfig(); // Might undo previous step
            MatchColorPickersToCanvas();
            _localSettingsWindowViewModel = new LocalSettingsWindowViewModel(
                ActualThemeVariant == ThemeVariant.Dark,
                ActualThemeVariant == ThemeVariant.Dark
                ? LogoColorsRecord.DarkModeDefault
                : LogoColorsRecord.LightModeDefault
            );
            this.DataContext = _localSettingsWindowViewModel;
            PreviewBgColorPicker.Color = Avalonia.Media.Colors.Transparent;
        }

        private void LoadImportedColors(LogoColorsRecord logoColorsRecord)
        {
            //XDocument xDoc = LogoHelper.GetLogoAsXDocument(logoColorsRecord);
            //PreviewXDoc(xDoc);
        }

        private void MatchColorPickersToCanvas()
        {
            MatchColorPickerToCanvas(Q_ColorPicker);
            //MatchColorPickerToCanvas(B_ColorPicker);
            //MatchColorPickerToCanvas(C_ColorPicker);

            //MatchColorPickerToCanvas(From_ColorPicker);
            //MatchColorPickerToCanvas(To_ColorPicker);
        }
        
        private void LoadColorsFromConfig()
        {
            if (Design.IsDesignMode)
                return; // There is no ConfigService in design mode, prevent it from throwing errors.
        }

        private void DownloadDirectoryWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            // Must check for designmode, if it's active ConfigService access needs to be avoided.
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

            //if (LogoViewbox.Child is Canvas theRealCanvas && theRealCanvas.Children.Count > 0)
            //{
            //    var textBlocks = theRealCanvas.Children.OfType<TextBlock>().ToList();//0 = q, 1 = b
            //    var linearGradientBrushes = theRealCanvas.Resources.Values.OfType<LinearGradientBrush>().ToList(); // 0 = background, 1 = C-shape

            //    //if(colorPicker == Q_ColorPicker && textBlocks[0].Foreground is ImmutableSolidColorBrush scbq)
            //    //    Q_ColorPicker.Color = scbq.Color;
            //    //if (colorPicker == B_ColorPicker && textBlocks[1].Foreground is ImmutableSolidColorBrush scbb)
            //    //    B_ColorPicker.Color = scbb.Color;
            //    //if (colorPicker == C_ColorPicker)
            //    //    C_ColorPicker.Color = linearGradientBrushes[1].GradientStops[0].Color;

            //    //if (colorPicker == From_ColorPicker)
            //    //    From_ColorPicker.Color = linearGradientBrushes[0].GradientStops[0].Color;
            //    //if (colorPicker == To_ColorPicker)
            //    //    To_ColorPicker.Color = linearGradientBrushes[0].GradientStops[1].Color;

            //}
        }

        private void RestoreDefaultIconButton_Click(object? sender, RoutedEventArgs e)
        {
            //LoadCanvasContent();
            MatchColorPickersToCanvas();
        }

        private void SaveAndApplyIconSplitButton_Click(object? sender, RoutedEventArgs e)
        {
            if (Owner is MainWindow mw)
            {
                var lcr = SelectedColorsToLogoColorsRecord();
                ExportLogoColorsRecord(lcr);
            //    IcoHelper.SaveIcon(LogoViewbox, IcoPath);

            //    Icon = new WindowIcon(IcoPath);
            //    mw.Icon = new WindowIcon(IcoPath);

            //    SaveCanvasColorsToConfig();
            }
        }

        private static void ExportLogoColorsRecord(LogoColorsRecord lcr)
        {
            string json = JsonConvert.SerializeObject(lcr, Formatting.Indented);
            DateTime dt = DateTime.Now;
            string fileName = dt.ToString("yyyy-MM-dd_HH-mm-ss") + ".json";
            // TODO set light / dark mode
            string path = Path.Combine(App.LogoColorsExportDirectory, fileName);
            Debug.WriteLine($"Saving colors to {path}");

            File.WriteAllText(path, json);
        }

        private LogoColorsRecord SelectedColorsToLogoColorsRecord()
        {
            LogoColorsRecord lcr = new(
                Q: Converters.ColorToHexConverter.ColorToHex(Q_ColorPicker.Color),
                B: Converters.ColorToHexConverter.ColorToHex(Q_ColorPicker.Color),
                C: Converters.ColorToHexConverter.ColorToHex(Q_ColorPicker.Color),
                GradientCenter: Converters.ColorToHexConverter.ColorToHex(Q_ColorPicker.Color),
                GradientFill: Converters.ColorToHexConverter.ColorToHex(Q_ColorPicker.Color),
                GradientRim: Converters.ColorToHexConverter.ColorToHex(Q_ColorPicker.Color)
            );

            return lcr;
        }


        private void PreviewBgColorPicker_ColorChanged(object? sender, ColorChangedEventArgs e)
        {
            if (PreviewBgColorPicker.Color.A == 0)
            {
                App.Current!.TryGetResource("ColorControlCheckeredBackgroundBrush", ActualThemeVariant, out var brush);
                if (brush is VisualBrush vb)
                    SetPreviewBgBrush(vb);
            }
            else
            {
                SetPreviewBgBrush(new SolidColorBrush(PreviewBgColorPicker.Color));
            }
        }

        private void SetPreviewBgBrush(IBrush brush)
        {
            LargePreviewBorder.Background = brush;
            Preview16Panel.Background = brush;
            Preview24Panel.Background = brush;
            Preview32Panel.Background = brush;
        }

        private void ForceOppositeMode(object? sender, RoutedEventArgs e)
        {
            IconCustomizationThemeVariantScope.RequestedThemeVariant = ActualThemeVariant == ThemeVariant.Dark
                ? ThemeVariant.Light
                : ThemeVariant.Dark;
        }

        private void RestoreDefaultMode(object? sender, RoutedEventArgs e)
        {
            IconCustomizationThemeVariantScope.RequestedThemeVariant = ActualThemeVariant == ThemeVariant.Dark
                ? ThemeVariant.Dark
                : ThemeVariant.Light;
        }
    }
}
