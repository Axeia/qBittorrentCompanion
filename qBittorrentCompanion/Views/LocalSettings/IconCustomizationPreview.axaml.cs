
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

            Loaded += IconCustomizationPreview_Loaded;
        }

        private void IconCustomizationPreview_Loaded(object? sender, RoutedEventArgs e)
        {
            AddOnFlyoutClosedAddToHistoryHandler(Q_ColorPicker);
            AddOnFlyoutClosedAddToHistoryHandler(B_ColorPicker);
            AddOnFlyoutClosedAddToHistoryHandler(C_ColorPicker);
            AddOnFlyoutClosedAddToHistoryHandler(GradientCenter_ColorPicker);
            AddOnFlyoutClosedAddToHistoryHandler(GradientFill_ColorPicker);
            AddOnFlyoutClosedAddToHistoryHandler(GradientRim_ColorPicker);
        }

        private void AddOnFlyoutClosedAddToHistoryHandler(ColorPicker colorPicker)
        {
            if (colorPicker.GetVisualDescendants()
                .OfType<DropDownButton>()
                .FirstOrDefault() is DropDownButton ddb
                && ddb.Flyout is Flyout flyout)
            {
                flyout.Closed += ColorFlyout_Closed;
            }
            else
                Debug.WriteLine($"Could not locate DropDownButton for {colorPicker}");
        }

        private void ColorFlyout_Closed(object? sender, EventArgs e)
        {
            if (DataContext is IconCustomizationViewModel icvm)
                icvm.AddLogoColorsRecordToHistory();
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