
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.VisualTree;
using qBittorrentCompanion.Models;
using qBittorrentCompanion.ViewModels.LocalSettings;
using System;
using System.Linq;


namespace qBittorrentCompanion.Views.LocalSettings
{
    public partial class IconCustomizationView : ThemeVariantScope
    {
        public IconCustomizationView()
        {
            InitializeComponent();
            bool isCurrentlyInDarkMode = IsCurrentlyInDarkMode();
            PreviewSwitcher.PageTransition = new PageSlide(TimeSpan.FromSeconds(0.5), PageSlide.SlideAxis.Horizontal);

            DataContext = new IconCustomizationViewModel(
                isCurrentlyInDarkMode,
                isCurrentlyInDarkMode ? LogoColorsRecord.DarkModeDefault : LogoColorsRecord.LightModeDefault
            );

            if (Resources["DarkIconCustomizationPreview"] is IconCustomizationPreview dicp
            && Resources["LightIconCustomizationPreview"] is IconCustomizationPreview licp)
            {
                PreviewSwitcher.Content = dicp;

                dicp.SwitchToDarkModeButton.Click += SwitchPreviewModeButton_Click;
                dicp.SwitchToLightModeButton.Click += SwitchPreviewModeButton_Click;
                licp.SwitchToDarkModeButton.Click += SwitchPreviewModeButton_Click;
                licp.SwitchToLightModeButton.Click += SwitchPreviewModeButton_Click;
            }
        }

        private void SwitchPreviewModeButton_Click(object? sender, RoutedEventArgs e)
        {
            if (Resources["DarkIconCustomizationPreview"] is IconCustomizationPreview dicp
            && Resources["LightIconCustomizationPreview"] is IconCustomizationPreview licp)
            {
                if (sender == dicp.SwitchToDarkModeButton || sender == licp.SwitchToDarkModeButton)
                    PreviewSwitcher.Content = dicp;
                else
                    PreviewSwitcher.Content = licp;
            }
        }

        public bool IsCurrentlyInDarkMode()
        {
            var resolvedTheme = this
                .GetSelfAndVisualAncestors()
                .OfType<Control>()
                .Select(e => e.ActualThemeVariant)
                .FirstOrDefault(tv => tv != ThemeVariant.Default);

            // Fallback to application theme if none found
            resolvedTheme ??= Application.Current?.ActualThemeVariant;

            return resolvedTheme == ThemeVariant.Dark;
        }
    }
}