
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia.VisualTree;
using qBittorrentCompanion.Models;
using qBittorrentCompanion.ViewModels.LocalSettings;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


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
                            isDark ? LogoDataRecord.DarkModeDefault : LogoDataRecord.LightModeDefault
                        );
                    }
                });

            // In the app it will be assigned by IconCustomizationView
            if (Design.IsDesignMode)
                DataContext = new IconCustomizationViewModel(
                    IsInDarkMode,
                    IsInDarkMode ? LogoDataRecord.DarkModeDefault : LogoDataRecord.LightModeDefault
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
                icvm.AddLogoDataRecordToHistory();
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

        private void ImportButton_Click(object? sender, RoutedEventArgs e)
        {
            if(DataContext is IconCustomizationViewModel icvm)
            {
                _ = ShowImportDialog(DateTime.Now.ToLongDateString(), icvm.SelectedExportAction);
            }
        }

        private async Task ShowImportDialog(string fileNameSuggestion, ExportAction exportAction)
        {
            var options = new FilePickerSaveOptions
            {
                Title = "Save File",
                SuggestedFileName = $"{fileNameSuggestion}.torrent",
                FileTypeChoices =
                [
                    new FilePickerFileType("Supported import files") { Patterns = ["*.svg", ".json"] }
                ]
            };


            var result = await TopLevel.GetTopLevel(this)!.StorageProvider.SaveFilePickerAsync(options);
            if (result != null)
            {
                //var fileBytes = await tivm.SaveDotTorrentAsync();
                //await File.WriteAllBytesAsync(result.Path.LocalPath, fileBytes);
            }
        }

        private void ExportSplitButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is IconCustomizationViewModel icvm)
            {
                _ = ShowExportDialog(DateTime.Now.ToLongDateString(), icvm.SelectedExportAction);
            }
        }

        private async Task ShowExportDialog(string fileNameSuggestion, ExportAction exportAction)
        {
            var options = new FilePickerSaveOptions();

            switch (exportAction)
            {
                case ExportAction.SVG_DARK_LIGHT:
                    options.Title = "Save Dark Light SVG Logo File";
                    break;
                case ExportAction.SVG_DARK:
                    options.Title = "Save Dark SVG Logo File";
                    break;
                case ExportAction.SVG_LIGHT:
                    options.Title = "Save Light SVG Logo File";
                    break;
                case ExportAction.JSON_DARK_LIGHT:
                    options.Title = "Save Dark Light Color Profile File";
                    break;
                case ExportAction.JSON_DARK:
                    options.Title = "Save Dark Color Profile File";
                    break;
                case ExportAction.JSON_LIGHT:
                    options.Title = "Save Light Color Profile File";
                    break;
            }

            switch (exportAction)
            { 
                case ExportAction.SVG_DARK_LIGHT:
                case ExportAction.SVG_DARK:
                case ExportAction.SVG_LIGHT:
                    options.SuggestedFileName = $"{fileNameSuggestion}.svg";
                    options.FileTypeChoices = [new FilePickerFileType("SVG Image Files") { Patterns = ["*.svg"] }];
                    break;
                case ExportAction.JSON_DARK_LIGHT:  
                case ExportAction.JSON_DARK:
                case ExportAction.JSON_LIGHT:
                    options.SuggestedFileName = $"{fileNameSuggestion}.json";
                    options.FileTypeChoices = [new FilePickerFileType("JSON Files") { Patterns = ["*.json"] }];
                    break;
            }


            var result = await TopLevel.GetTopLevel(this)!.StorageProvider.SaveFilePickerAsync(options);
            if (result != null)
            {
                //var fileBytes = await tivm.SaveDotTorrentAsync();
                //await File.WriteAllBytesAsync(result.Path.LocalPath, fileBytes);
            }
        }
    }
}