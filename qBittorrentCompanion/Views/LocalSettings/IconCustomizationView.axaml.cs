
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Newtonsoft.Json;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Models;
using qBittorrentCompanion.ViewModels.LocalSettings;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;


namespace qBittorrentCompanion.Views.LocalSettings
{
    public partial class IconCustomizationView : UserControl
    {
        public IconCustomizationView()
        {
            InitializeComponent();

            DataContext = new IconCustomizationViewModel(
                ActualThemeVariant == ThemeVariant.Dark,
                ActualThemeVariant == ThemeVariant.Dark
                ? LogoColorsRecord.DarkModeDefault
                : LogoColorsRecord.LightModeDefault
            );
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