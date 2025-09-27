
using Avalonia;
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
    public partial class IconCustomizationPreview : ThemeVariantScope
    {
        public static readonly StyledProperty<bool> IsInDarkModeProperty =
            AvaloniaProperty.Register<IconCustomizationPreview, bool>(nameof(IsInDarkMode));

        public bool IsInDarkMode
        {
            get => GetValue(IsInDarkModeProperty);
            set => SetValue(IsInDarkModeProperty, value);
        }

        public IconCustomizationPreview()
        {
            InitializeComponent();

            this.GetObservable(IsInDarkModeProperty)
                .Subscribe(isDark =>
                {
                    RequestedThemeVariant = isDark ? ThemeVariant.Dark : ThemeVariant.Light;
                    Debug.WriteLine($"IsInDarkMode (resolved): {isDark}");

                    if (Design.IsDesignMode)
                    {
                        DataContext = new IconCustomizationViewModel(
                            isDark,
                            isDark ? LogoColorsRecord.DarkModeDefault : LogoColorsRecord.LightModeDefault
                        );
                    }
                });

            // In the app it will be assigned by IconCustomizationView
            if (Design.IsDesignMode)
                DataContext = new IconCustomizationViewModel(
                    IsInDarkMode,
                    IsInDarkMode ? LogoColorsRecord.DarkModeDefault : LogoColorsRecord.LightModeDefault
                );
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