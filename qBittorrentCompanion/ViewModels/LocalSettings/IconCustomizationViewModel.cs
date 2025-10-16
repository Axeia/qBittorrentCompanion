using AutoPropertyChangedGenerator;
using Avalonia.Media;
using DynamicData;
using Newtonsoft.Json;
using qBittorrentCompanion.Extensions;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Models;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Xml.Linq;

namespace qBittorrentCompanion.ViewModels.LocalSettings
{
    public enum ExportAction
    {
        JSON_DARK_LIGHT,
        JSON_DARK,
        JSON_LIGHT,
        SVG_DARK_LIGHT,
        SVG_DARK,
        SVG_LIGHT
    }

    /// <summary>
    /// <b>Important:</b> The order (indexing) matters.<br/><br/>
    /// When exporting this is stored as an int.<br/>
    /// When importing this is parsed as an int and needs to match these values.<br/>
    /// If the order is changed things WILL go awry.<br/>
    /// </summary>
    public enum IconSaveMode
    {
        DarkAndLight,
        Dark,
        Light
    }

    public static class ExportActionExtensions
    {
        public static IconSaveMode ToIconSaveMode(this ExportAction exportAction)
            => exportAction switch
            {
                ExportAction.JSON_DARK_LIGHT or ExportAction.SVG_DARK_LIGHT => IconSaveMode.DarkAndLight,
                ExportAction.JSON_DARK or ExportAction.SVG_DARK => IconSaveMode.Dark,
                ExportAction.JSON_LIGHT or ExportAction.SVG_LIGHT => IconSaveMode.Light,
                _ => throw new ArgumentOutOfRangeException(nameof(exportAction)),
            };

        public static bool AsSvg(this ExportAction exportAction) =>
            exportAction is ExportAction.SVG_DARK_LIGHT or ExportAction.SVG_DARK or ExportAction.SVG_LIGHT;

        public static bool AsJson(this ExportAction exportAction) =>
            exportAction is ExportAction.JSON_DARK_LIGHT or ExportAction.JSON_DARK or ExportAction.JSON_LIGHT;
    }

    public static class IconSaveModeExtensions
    {
        public static string ToDisplayString(this IconSaveMode iconSaveMode)
            => iconSaveMode switch
            {
                IconSaveMode.DarkAndLight => "Dark and light",
                IconSaveMode.Dark => "Dark",
                IconSaveMode.Light => "Light",
                _ => throw new ArgumentOutOfRangeException(nameof(iconSaveMode))
            };
        public static string ModeAsIntString(this IconSaveMode iconSaveMode)
            => ((int)iconSaveMode).ToString();
    }

    public partial class LogoColorsHistoryRecord(LogoDataRecord logoDataRecord) : ReactiveObject
    {
        public LogoDataRecord Ldr => logoDataRecord;
        public string Id => DateTime.Now.ToLongTimeString();

        [AutoPropertyChanged]
        public bool _isForRedo = false;
        [AutoPropertyChanged]
        public bool _isForUndo = false;
    }

    public record LogoPresetRecord(string Name, LogoDataRecord Lcr, IconSaveMode Mode);

    public class LogoPresetCollection(string name, LogoPresetRecord[] logoPresets) : ReactiveObject
    {
        public string Name => name;

        private LogoPresetRecord[] _logoPresets = logoPresets;
        public LogoPresetRecord[] LogoPresets
        {
            get => _logoPresets;
            set => this.RaiseAndSetIfChanged(ref _logoPresets, value);
        }
    }

    public partial class IconCustomizationViewModel : ViewModelBase, IDisposable
    {
        private readonly DebouncedFileWatcher _debouncedWatcher;

        public List<ExportAction> ExportActionOptions 
            => [.. Enum.GetValues(typeof(ExportAction)).Cast<ExportAction>()];

        [AutoPropertyChanged]
        private ExportAction _selectedExportAction = ExportAction.JSON_DARK_LIGHT;

        public List<IconSaveMode> IconSaveModeOptions 
            => [.. Enum.GetValues(typeof(IconSaveMode)).Cast<IconSaveMode>()];

        [AutoPropertyChanged]
        private IconSaveMode _selectedIconSaveMode = IconSaveMode.DarkAndLight;

        public IconCustomizationViewModel(bool isInDarkMode, LogoDataRecord ldr)
        {
            _isInDarkMode = isInDarkMode;
            _gradientRimColor = ldr.GradientRim;
            _gradientFillColor = ldr.GradientFill;
            _gradientCenterColor = ldr.GradientCenter;
            _c_color = ldr.C;
            _b_color = ldr.B;
            _q_color = ldr.Q;
            _svgXDoc = LogoHelper.GetLogoAsXDocument(ldr);
            _logoDataRecord = ldr;
            // Add as first history point so it can be undo(ne) back to
            LogoDataRecordHistory.Add(new LogoColorsHistoryRecord(ldr));

            _logoDataRecordHistory.CollectionChanged += (s, e) => RecheckUndoRedoLogic();

            UndoCommand = ReactiveCommand.Create(Undo, this.WhenAnyValue(x => x.CanUndo));
            RedoCommand = ReactiveCommand.Create(Redo, this.WhenAnyValue(x=>x.CanRedo));


            _debouncedWatcher = new(App.LogoColorsExportDirectory, "*.json");
            Debug.WriteLine(App.LogoColorsExportDirectory);
            _debouncedWatcher.ChangesReady += AddImportFolderToPresetCollections;
            AddImportFolderToPresetCollections();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _debouncedWatcher.ChangesReady -= AddImportFolderToPresetCollections;
            _debouncedWatcher.Dispose();
        }

        private static string _localFilesCollectionName = Path.GetFileName(App.LogoColorsExportDirectory)! + " folder";

        private void AddImportFolderToPresetCollections()
        {
            // Check if import folder was already added, if so delete it to prevent double entries.
            var lastCollection = PresetCollections.Last();

            // Get the file paths for all .json files.
            var jsonFileFilePaths = Directory.GetFiles(App.LogoColorsExportDirectory)
                .Where(f => Path.GetExtension(f).Equals(".json", StringComparison.OrdinalIgnoreCase))
                .Select(f => f);

            List<LogoPresetRecord> historicalLogoPresetRecords = [];

            foreach(var jsonFileFilePath in jsonFileFilePaths)
            {
                string json = File.ReadAllText(jsonFileFilePath);
                LogoPresetRecord? lpr = JsonConvert.DeserializeObject<LogoPresetRecord>(json);
                if (lpr is null)
                {
                    Debug.WriteLine("Deserialized into null");
                    return;
                }

                historicalLogoPresetRecords.Add(lpr);
            }

            lastCollection.LogoPresets = [.. historicalLogoPresetRecords];
            this.RaisePropertyChanged(nameof(PresetCollections));
        }

        private void RecheckUndoRedoLogic()
        {
            for(int i = 0; i < LogoDataRecordHistory.Count; i++)
            {
                var lcrh = LogoDataRecordHistory.ElementAt(i);
                lcrh.IsForUndo = i < HistoryIndex;
                lcrh.IsForRedo = i > HistoryIndex;
            }

            this.RaisePropertyChanged(nameof(CanUndo));
            this.RaisePropertyChanged(nameof(CanRedo));
        }

        /// <summary>
        /// Keeps track of changes as LogoDataRecord is immutable it's always created as a full set of all the relevant data<br/>
        /// <see cref="LogoDataRecordHistory"/> is the property that's actually used for the UI presentation as it skips the 
        /// last (which is the current) result and reverses the list making it suitable for an undo history.
        /// </summary>
        private readonly ObservableCollection<LogoColorsHistoryRecord> _logoDataRecordHistory = [];
        public ObservableCollection<LogoColorsHistoryRecord> LogoDataRecordHistory
            => _logoDataRecordHistory;

        private int _historyIndex = 0;
        public int HistoryIndex
        {
            get => _historyIndex;
            set
            {
                if (_historyIndex != value)
                {
                    if (value < 0 || value >= LogoDataRecordHistory.Count)
                        throw new ArgumentOutOfRangeException(nameof(value), value, "HistoryIndex out of range");

                    _logoDataRecord = LogoDataRecordHistory[value].Ldr;
                    SvgXDoc = LogoHelper.GetLogoAsXDocument(_logoDataRecord);
                    SyncColorsWithLogoRecord();

                    _historyIndex = value;
                    this.RaisePropertyChanged(nameof(HistoryIndex));
                    RecheckUndoRedoLogic();
                }
            }
        }

        public bool CanUndo
            => _historyIndex > 0;

        public ReactiveCommand<Unit, Unit> UndoCommand { get; }

        public void Undo()
            => HistoryIndex--; // Triggers colors loading

        public bool CanRedo
            => _historyIndex < LogoDataRecordHistory.Count -1;

        public ReactiveCommand<Unit, Unit> RedoCommand { get; }

        private void Redo()
            => HistoryIndex++; // Triggers colors loading

        private bool _isInDarkMode;
        public bool IsInDarkMode
        {
            get => _isInDarkMode;
            set
            {
                if (_isInDarkMode != value)
                {
                    _isInDarkMode = value;
                    this.RaisePropertyChanged(nameof(IsInDarkMode));
                    this.RaisePropertyChanged(nameof(ThemeMode));
                    this.RaisePropertyChanged(nameof(OppositeMode));
                }
            }
        }

        private LogoPresetRecord? _selectedLogoPresetRecord = null;

        public LogoPresetRecord? SelectedLogoPresetRecord
        {
            get => _selectedLogoPresetRecord;
            set
            {
                if (_selectedLogoPresetRecord != value)
                {
                    _selectedLogoPresetRecord = value;
                    this.RaisePropertyChanged(nameof(SelectedLogoPresetRecord));
                    
                    if (value != null)
                    { // Sets color values and reloads SVG with these colors applied.
                        UseLogoColors(value.Lcr);
                        SelectedIconSaveMode = value.Mode;
                    }
                }
            }
        }

        public ReactiveCommand<LogoDataRecord, Unit> UseLogoColorsCommand =>
            ReactiveCommand.Create<LogoDataRecord>(UseLogoColors);

        [AutoPropertyChanged]
        private string _customName = string.Empty;

        public ObservableCollection<LogoPresetCollection> PresetCollections { get; } = 
            [
                new LogoPresetCollection("Defaults",
                [
                    new LogoPresetRecord(
                        "Light mode default",
                        LogoDataRecord.LightModeDefault,
                        IconSaveMode.Light
                    ),
                    new LogoPresetRecord(
                        "Dark mode default",
                        LogoDataRecord.DarkModeDefault,
                        IconSaveMode.Dark
                    ),
                    new LogoPresetRecord(
                        "Old logo inspired",
                        new LogoDataRecord(
                            Q: Color.Parse("rgb(137,107,178)"),
                            B: Color.Parse("rgb(137,107,178)"),
                            C: Color.Parse("rgb(137,107,178)"),
                            GradientCenter: Color.Parse("rgb(51,0,128)"),
                            GradientFill: Color.Parse("rgb(137,107,178)"),
                            GradientRim: Color.Parse("rgb(224,201,255)")
                        ),
                        IconSaveMode.DarkAndLight
                    ),
                    new LogoPresetRecord(
                        "qBittorrent, but different",
                        new LogoDataRecord(
                            Q: Color.Parse("#97C5E8"),
                            B: Color.Parse("#97C5E8"),
                            C: Color.Parse("#97C5E8"),
                            GradientCenter: Color.Parse("#66a6ea"),
                            GradientFill: Color.Parse("#356ebf"),
                            GradientRim: Color.Parse("#FFF")
                        ),
                        IconSaveMode.DarkAndLight
                    ),

                ]),
                new LogoPresetCollection("System accent color",
                [
                    new LogoPresetRecord(
                        "System accent dark",
                        new LogoDataRecord(
                            Q: ThemeColors.SystemAccentLight1,
                            B: ThemeColors.SystemAccentLight1,
                            C: ThemeColors.SystemAccentLight1,
                            GradientCenter: ThemeColors.SystemAccentDark3,
                            GradientFill: ThemeColors.SystemAccentDark1,
                            GradientRim: ThemeColors.SystemAccentLight3
                        ),
                        IconSaveMode.Dark
                    ),
                    new LogoPresetRecord(
                        "System accent light",
                        new LogoDataRecord(
                            Q: ThemeColors.SystemAccentDark3,
                            B: ThemeColors.SystemAccentDark3,
                            C: ThemeColors.SystemAccentDark3,
                            GradientCenter: ThemeColors.SystemAccentLight1,
                            GradientFill: ThemeColors.SystemAccent,
                            GradientRim: ThemeColors.SystemAccentLight3
                        ),
                        IconSaveMode.Light
                    )
                ]),
                new LogoPresetCollection(_localFilesCollectionName,[])
            ];

        [AutoPropertyChanged]
        private int _selectedPresetCollectionIndex = 0;

        private void UseLogoColors(LogoDataRecord lcr)
        {
            LoadLogoPresetRecord(lcr);
            SyncColorsWithLogoRecord();
            SvgXDoc = LogoHelper.GetLogoAsXDocument(lcr);
        }

        public void SyncColorsWithLogoRecord()
        {
            if (_q_color != LogoDataRecord.Q)
            {
                _q_color = LogoDataRecord.Q;
                this.RaisePropertyChanged(nameof(Q_Color));
            }
            if (_b_color != LogoDataRecord.B)
            {
                _b_color = LogoDataRecord.B;
                this.RaisePropertyChanged(nameof(B_Color));
            }
            if (_c_color != LogoDataRecord.C)
            {
                _c_color = LogoDataRecord.C;
                this.RaisePropertyChanged(nameof(C_Color));
            }
            if (_gradientCenterColor != LogoDataRecord.GradientCenter)
            {
                _gradientCenterColor = LogoDataRecord.GradientCenter;
                this.RaisePropertyChanged(nameof(GradientCenterColor));
            }
            if (_gradientFillColor != LogoDataRecord.GradientFill)
            {
                _gradientFillColor = LogoDataRecord.GradientFill;
                this.RaisePropertyChanged(nameof(GradientFillColor));

            }
            if (_gradientRimColor != LogoDataRecord.GradientRim)
            {
                _gradientRimColor = LogoDataRecord.GradientRim;
                this.RaisePropertyChanged(nameof(GradientRimColor));
            }
        }

        public ReactiveCommand<IconSaveMode, Unit> SaveCommand =>
            ReactiveCommand.Create<IconSaveMode>(Save);

        private void Save(IconSaveMode iconSaveMode = IconSaveMode.DarkAndLight)
        {
            // Export JSON
            ExportLogoPresetRecordToDisk(
                Path.Combine(App.LogoColorsExportDirectory, ExportNameDateTimeString+".json"),
                _logoDataRecord, 
                iconSaveMode
            ); 
        }

        private XDocument _svgXDoc;
        /// <summary>
        /// For internal use only. <br/>
        /// You should probably be using <see cref="LogoDataRecord"/> instead 
        /// which does internal bookkeeping to keep the undo history intact
        /// </summary>
        private LogoDataRecord _logoDataRecord;

        /// <summary>
        /// Assigns to _logoColordsRecord and adds it to history if it's actually different
        /// Also sets the previous item as <see cref="LogoDataRecordHistorySelectedUndoItem"/>
        /// </summary>
        public LogoDataRecord LogoDataRecord
        {
            get => _logoDataRecord;
            private set
            {
                if (LogoDataRecordHistory.ElementAt(HistoryIndex).Ldr != value)
                {
                    _logoDataRecord = value;

                    PurgeRedoHistory();
                    LogoDataRecordHistory.Add(new LogoColorsHistoryRecord(value));
                    HistoryIndex++;
                }
                else
                    Debug.WriteLine("Skip - trying to add history entry with the same value as the current one");
            }
        }

        /// <summary>
        /// Removes all entries beyond the current <see cref="HistoryIndex"/>, 
        /// basically call this when adding a new <see cref="LogoDataRecordHistory"/> entry
        /// </summary>
        private void PurgeRedoHistory()
        {
            int nextIndex = HistoryIndex + 1;
            if (nextIndex < LogoDataRecordHistory.Count)
                LogoDataRecordHistory.RemoveMany(LogoDataRecordHistory.ToList()[nextIndex..]);
        }

        private void LoadLogoPresetRecord(LogoDataRecord lcr)
        {
            Debug.WriteLine("LoadLogoPresetRecord called");
            LogoDataRecord = lcr; // Adds to history
            Q_Color = lcr.Q;
            B_Color = lcr.B;
            C_Color = lcr.C;
            GradientCenterColor = lcr.GradientCenter;
            GradientFillColor = lcr.GradientFill;
            GradientRimColor = lcr.GradientRim;
        }

        public XDocument SvgXDoc
        {
            get => _svgXDoc;
            set
            {
                if (value != _svgXDoc)
                {
                    _svgXDoc = value;
                    this.RaisePropertyChanged(nameof(PreviewSvg));
                }
            }
        }

        public string PreviewSvg
            => _svgXDoc.ToString();

        public bool AddLogoDataRecordToHistory()
        {
            if(_logoDataRecord != LogoDataRecordHistory.ElementAt(HistoryIndex).Ldr)
            {
                PurgeRedoHistory();
                LogoDataRecordHistory.Add(new LogoColorsHistoryRecord(_logoDataRecord));
                HistoryIndex++;
                return true;
            }

            return false;
        }

        private Color _q_color;

        /// <summary>
        /// Does not persist change to history, please use <see cref="AddLogoDataRecordToHistory"/> to do so.
        /// This is so that the UI can call this when the color dialog is closed rather than every time an adjustment is made.
        /// </summary>
        public Color Q_Color
        {
            get => _q_color;
            set
            {
                if (_q_color != value)
                {
                    _q_color = value;
                    SvgXDoc = SvgXDoc.SetSvgStroke("q", value.ToString(ColorFormat.RGBA_ALPHA_FLOAT));
                    _logoDataRecord = _logoDataRecord with { Q = value };

                    this.RaisePropertyChanged(nameof(Q_Color));
                    this.RaisePropertyChanged(nameof(PreviewSvg));
                }
            }
        }

        private Color _b_color;

        /// <summary><inheritdoc cref="Q_Color"/></summary>
        public Color B_Color
        {
            get => _b_color;
            set
            {
                if (_b_color != value)
                {
                    _b_color = value;
                    SvgXDoc = SvgXDoc.SetSvgStroke("b", value.ToString(ColorFormat.RGBA_ALPHA_FLOAT));
                    _logoDataRecord = _logoDataRecord with { B = value };

                    this.RaisePropertyChanged(nameof(B_Color));
                    this.RaisePropertyChanged(nameof(PreviewSvg));
                }
            }
        }

        private Color _c_color;
        /// <summary><inheritdoc cref="Q_Color"/></summary>
        public Color C_Color
        {
            get => _c_color;
            set
            {
                if (_c_color != value)
                {
                    _c_color = value;
                    SvgXDoc = SvgXDoc.SetSvgStroke("c", value.ToString(ColorFormat.RGBA_ALPHA_FLOAT));
                    _logoDataRecord = _logoDataRecord with { C = value };

                    this.RaisePropertyChanged(nameof(C_Color));
                    this.RaisePropertyChanged(nameof(PreviewSvg));
                }
            }
        }

        private Color _gradientCenterColor;
        /// <summary><inheritdoc cref="Q_Color"/></summary>
        public Color GradientCenterColor
        {
            get => _gradientCenterColor;
            set
            {
                if (_gradientCenterColor != value)
                {
                    _gradientCenterColor = value;
                    SvgXDoc = SvgXDoc.SetSvgGradientStop("gradient", 0, value.ToString(ColorFormat.RGBA_ALPHA_FLOAT));
                    _logoDataRecord = _logoDataRecord with { GradientCenter = value };

                    this.RaisePropertyChanged(nameof(GradientCenterColor));
                    this.RaisePropertyChanged(nameof(PreviewSvg));
                }
            }
        }

        private Color _gradientFillColor;
        /// <summary><inheritdoc cref="Q_Color"/></summary>
        public Color GradientFillColor
        {
            get => _gradientFillColor;
            set
            {
                if (_gradientFillColor != value)
                {
                    _gradientFillColor = value;
                    SvgXDoc = SvgXDoc.SetSvgGradientStop("gradient", 1, value.ToString(ColorFormat.RGBA_ALPHA_FLOAT));
                    _logoDataRecord = _logoDataRecord with { GradientFill = value };

                    this.RaisePropertyChanged(nameof(GradientFillColor));
                    this.RaisePropertyChanged(nameof(PreviewSvg));
                }
            }
        }

        private Color _gradientRimColor;
        /// <summary><inheritdoc cref="Q_Color"/></summary>
        public Color GradientRimColor
        {
            get => _gradientRimColor;
            set
            {
                if (_gradientRimColor != value)
                {
                    _gradientRimColor = value;
                    SvgXDoc = SvgXDoc.SetSvgGradientStop("gradient", 2, value.ToString(ColorFormat.RGBA_ALPHA_FLOAT));
                    _logoDataRecord = _logoDataRecord with { GradientRim = value };

                    this.RaisePropertyChanged(nameof(GradientRimColor));
                    this.RaisePropertyChanged(nameof(PreviewSvg));
                }
            }
        }

        public string ThemeMode => BoolToDarkModeText(IsInDarkMode);
        public string OppositeMode => BoolToDarkModeText(!IsInDarkMode);

        public static string BoolToDarkModeText(bool isInDarkMode) => isInDarkMode ? "dark" : "light";

        public static void ExportLogoPresetRecordToDisk(string path, LogoDataRecord ldr, IconSaveMode iconSaveMode = IconSaveMode.DarkAndLight)
        {
            string fileName = Path.GetFileName(path);
            LogoPresetRecord lpr = new(Name: fileName, Lcr: ldr, Mode: iconSaveMode);
            string json = JsonConvert.SerializeObject(lpr, Formatting.Indented);
            try
            {
                File.WriteAllText(path, json);
                AppLoggerService.AddLogMessage(
                    Splat.LogLevel.Info,
                    GetFullTypeName<IconCustomizationViewModel>(),
                    $"Exported {fileName}",
                    $"Created a new logo color profile file: {path}"
                );
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        public void ImportLogoFromJson(string path)
        {
            string fileName = Path.GetFileName(path);
            if(File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    LogoPresetRecord? lpr = JsonConvert.DeserializeObject<LogoPresetRecord>(json);
                    if (lpr is null)
                    {
                        Debug.WriteLine($"Deserialized JSON into null but was expecting {nameof(LogoPresetRecord)}");
                        return;
                    }

                    // Load and add to history
                    UseLogoColors(lpr.Lcr);
                    SelectedIconSaveMode = lpr.Mode;
                }
                catch (Exception e)
                {
                    AppLoggerService.AddLogMessage(
                        Splat.LogLevel.Error,
                        GetFullTypeName<IconCustomizationViewModel>(),
                        $"Error deserializing LogoPresetRecord",
                        e.Message,
                        "File not found"
                    );
                }
            }
            else
            {
                AppLoggerService.AddLogMessage(
                    Splat.LogLevel.Error,
                    GetFullTypeName<IconCustomizationViewModel>(),
                    $"File {fileName} not found",
                    path + "does not exist",
                    "File not found"
                );
            }
        }

        public static void ExportSvgToDisk(string path, LogoDataRecord ldr, IconSaveMode iconSaveMode = IconSaveMode.DarkAndLight)
        {
            string fileName = Path.GetFileName(path);
            string svgAsString = LogoHelper.GetLogoAsXDocument(ldr, iconSaveMode).ToString();

            try
            {
                File.WriteAllText(path, svgAsString);
                AppLoggerService.AddLogMessage(
                    Splat.LogLevel.Info,
                    GetFullTypeName<IconCustomizationViewModel>(),
                    $"Exported {fileName}",
                    $"Created a new logo svg file: {path}"
                );
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        public void ImportLogoFromSvg(string path)
        {
            string fileName = Path.GetFileName(path);
            if (File.Exists(path))
            {
                try
                {
                    LogoPresetRecord? lpr = LogoHelper.CreateLogoPresetRecordFromSvg(path);
                    if (lpr is null)
                    {
                        Debug.WriteLine("Deserialized into null");
                        return;
                    }

                    // Load and add to history
                    UseLogoColors(lpr.Lcr);
                    // Match the selected icon save mode
                    SelectedIconSaveMode = lpr.Mode;
                }
                catch (Exception e)
                {
                    AppLoggerService.AddLogMessage(
                        Splat.LogLevel.Error,
                        GetFullTypeName<IconCustomizationViewModel>(),
                        $"Error deserializing LogoPresetRecord",
                        e.Message,
                        "File not found"
                    );
                }
            }
            else
            {
                AppLoggerService.AddLogMessage(
                    Splat.LogLevel.Error,
                    GetFullTypeName<IconCustomizationViewModel>(),
                    $"File {fileName} not found",
                    path + "does not exist",
                    "File not found"
                );
            }
        }

        public static string ExportNameDateTimeString
            => DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
    }
}
