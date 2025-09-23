
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.VisualTree;
using qBittorrentCompanion.Models;
using qBittorrentCompanion.ViewModels.LocalSettings;
using System;
using System.Diagnostics;
using System.Linq;


namespace qBittorrentCompanion.Views.LocalSettings
{
    public partial class IconCustomizationView : ThemeVariantScope
    {
        public IconCustomizationView()
        {
            InitializeComponent();
            bool isCurrentlyInDarkMode = IsCurrentlyInDarkMode();
            Debug.WriteLine($"IsInDarkMode {isCurrentlyInDarkMode}");

            DataContext = new IconCustomizationViewModel(
                isCurrentlyInDarkMode,
                isCurrentlyInDarkMode ? LogoColorsRecord.DarkModeDefault : LogoColorsRecord.LightModeDefault
            );

            PreviewSwitcher.PageTransition = new PageSlide(TimeSpan.FromSeconds(0.5), PageSlide.SlideAxis.Horizontal);
            SaveButtonSwitcher.PageTransition = new PageSlide(TimeSpan.FromSeconds(0.5), PageSlide.SlideAxis.Horizontal);

            // Initialize button slider
            if (Resources["DarkToLightModeButton"] is Button darkToLightModeButton 
            && Resources["LightToDarkModeButton"] is Button lightToDarkModeButton)
            {
                PreviewSwitcher.Content = isCurrentlyInDarkMode ? darkToLightModeButton : lightToDarkModeButton;
            }

            if (Resources["SaveDarkModeButton"] is Button SaveDarkModeButton
            && Resources["SaveLightModeButton"] is Button SaveLightModeButton)
            {
                SaveButtonSwitcher.Content = isCurrentlyInDarkMode ? SaveDarkModeButton : SaveLightModeButton;
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

        private void DarkToLightModeButton_Click(object? sender, RoutedEventArgs e)
        {
            if (Resources["LightToDarkModeButton"] is Button lightToDarkModeButton
            && Resources["SaveLightModeButton"] is Button saveLightModeButton)
            {
                PreviewSwitcher.Content = lightToDarkModeButton;
                SaveButtonSwitcher.Content = saveLightModeButton;
            }
            if (DataContext is IconCustomizationViewModel icvm)
                icvm.IsInDarkMode = false;

            PreviewThemeVariantScope.RequestedThemeVariant = ThemeVariant.Light;
        }

        private void LightToDarkModeButton_Click(object? sender, RoutedEventArgs e)
        {
            if (Resources["DarkToLightModeButton"] is Button darkToLightModeButton
            && Resources["SaveDarkModeButton"] is Button saveDarkModeButton)
            {
                PreviewSwitcher.Content = darkToLightModeButton;
                SaveButtonSwitcher.Content = saveDarkModeButton;
            }
            if (DataContext is IconCustomizationViewModel icvm)
                icvm.IsInDarkMode = true;

            PreviewThemeVariantScope.RequestedThemeVariant = ThemeVariant.Dark;
        }
    }
}