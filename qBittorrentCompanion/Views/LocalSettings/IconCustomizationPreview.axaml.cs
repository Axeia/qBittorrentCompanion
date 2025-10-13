
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
using System.IO;
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
            _ = ShowImportDialog();
        }

        private async Task ShowImportDialog()
        {
            var options = new FilePickerOpenOptions
            {
                Title = "Open logo file",
                FileTypeFilter =
                [
                    new FilePickerFileType("Supported import files") { Patterns = ["*.svg", "*.json"] }
                ]
            };


            var result = await TopLevel.GetTopLevel(this)!.StorageProvider.OpenFilePickerAsync(options);
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
                _ = ShowExportDialog(IconCustomizationViewModel.ExportNameDateTimeString, icvm.SelectedExportAction);
            }
        }

        private async Task ShowExportDialog(string fileNameSuggestion, ExportAction exportAction)
        {
            if (DataContext is not IconCustomizationViewModel icvm)
                return;
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is null)
                return;

            var options = new FilePickerSaveOptions
            {
                Title = exportAction switch
                {
                    _ when exportAction.AsSvg() => $"Save {exportAction.ToIconSaveMode().ToDisplayString().ToLower()} SVG Logo File",
                    _ when exportAction.AsJson() => $"Save {exportAction.ToIconSaveMode().ToDisplayString().ToLower()} Color Profile File",
                    _ => throw new ArgumentOutOfRangeException(nameof(exportAction))
                },
                SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(App.LogoColorsExportDirectory)
            };

            if (exportAction.AsJson())
            {
                options.SuggestedFileName = $"{fileNameSuggestion}.json";
                options.FileTypeChoices = [new FilePickerFileType("JSON Files") { Patterns = ["*.json"] }];
            }
            if (exportAction.AsSvg())
            {
                options.SuggestedFileName = $"{fileNameSuggestion}.svg";
                options.FileTypeChoices = [new FilePickerFileType("SVG Image Files") { Patterns = ["*.svg"] }];
            }

            var result = await TopLevel.GetTopLevel(this)!.StorageProvider.SaveFilePickerAsync(options);
            if (result is IStorageFile storageFile)
            {
                string extension = Path.GetExtension(storageFile.Path.ToString());
                bool exportAsJson = extension.Equals(".json", StringComparison.OrdinalIgnoreCase);
                bool exportAsSvg = extension.Equals(".svg", StringComparison.OrdinalIgnoreCase);


                if (exportAction.AsSvg() && exportAsJson)
                    Debug.WriteLine("Selected SVG but then opted to export .json");
                if (exportAction.AsJson() && exportAsSvg)
                    Debug.WriteLine("Selected JSON but then opted to export .svg");

                if (exportAsJson)
                {
                    icvm.ExportLogoPresetRecordToDisk(
                        storageFile.Path.LocalPath,
                        icvm.LogoDataRecord,
                        exportAction.ToIconSaveMode()
                    );
                }

                //if(exportAsSvg)
                //{
                //    icvm.ExportSvgToDisk(
                //        fileNameSuggestion,
                //        icvm.LogoDataRecord,
                //        exportAction.ToIconSaveMode()
                //    );
                //}
                //var fileBytes = await tivm.SaveDotTorrentAsync();
                //await File.WriteAllBytesAsync(result.Path.LocalPath, fileBytes);
            }
        }
    }
}