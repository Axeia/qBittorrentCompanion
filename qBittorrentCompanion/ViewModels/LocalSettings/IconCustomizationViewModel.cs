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
    public enum IconSaveMode
    {
        DarkAndLight,
        Dark,
        Light
    }

    public partial class LogoColorsHistoryRecord(LogoColorsRecord logoColorsRecord) : ReactiveObject
    {
        public LogoColorsRecord Lcr => logoColorsRecord;
        public string Id => DateTime.Now.ToLongTimeString();

        [AutoPropertyChanged]
        public bool _isForRedo = false;
        [AutoPropertyChanged]
        public bool _isForUndo = false;
    }

    public record LogoPresetRecord(string Name, LogoColorsRecord Lcr, bool IsForDarkMode);
    public record LogoPresetCollectionRecord(string Name, LogoPresetRecord[] LogoPresets);

    public partial class IconCustomizationViewModel : ViewModelBase
    {
        public List<IconSaveMode> IconSaveModeOptions 
            => Enum.GetValues(typeof(IconSaveMode)).Cast<IconSaveMode>().ToList();

        [AutoPropertyChanged]
        private IconSaveMode _selectedIconSaveMode = IconSaveMode.DarkAndLight;

        public IconCustomizationViewModel(bool isInDarkMode, LogoColorsRecord lcr)
        {
            _isInDarkMode = isInDarkMode;
            _gradientRimColor = lcr.GradientRim;
            _gradientFillColor = lcr.GradientFill;
            _gradientCenterColor = lcr.GradientCenter;
            _c_color = lcr.C;
            _b_color = lcr.B;
            _q_color = lcr.Q;
            _svgXDoc = LogoHelper.GetLogoAsXDocument(lcr);
            _logoColorsRecord = lcr;
            // Add as first history point so it can be undo(ne) back to
            LogoColorsRecordHistory.Add(new LogoColorsHistoryRecord(lcr));

            _logoColorsRecordHistory.CollectionChanged += (s, e) => RecheckUndoRedoLogic();

            UndoCommand = ReactiveCommand.Create(Undo, this.WhenAnyValue(x => x.CanUndo));
            RedoCommand = ReactiveCommand.Create(Redo, this.WhenAnyValue(x=>x.CanRedo));
        }

        private void RecheckUndoRedoLogic()
        {
            for(int i = 0; i < LogoColorsRecordHistory.Count; i++)
            {
                var lcrh = LogoColorsRecordHistory.ElementAt(i);
                lcrh.IsForUndo = i < HistoryIndex;
                lcrh.IsForRedo = i > HistoryIndex;
            }

            this.RaisePropertyChanged(nameof(CanUndo));
            this.RaisePropertyChanged(nameof(CanRedo));
        }

        /// <summary>
        /// Keeps track of changes as LogoColorsRecord is immutable it's always created as a full set of all the relevant data<br/>
        /// <see cref="LogoColorsRecordHistory"/> is the property that's actually used for the UI presentation as it skips the 
        /// last (which is the current) result and reverses the list making it suitable for an undo history.
        /// </summary>
        private readonly ObservableCollection<LogoColorsHistoryRecord> _logoColorsRecordHistory = [];
        public ObservableCollection<LogoColorsHistoryRecord> LogoColorsRecordHistory
            => _logoColorsRecordHistory;

        private int _historyIndex = 0;
        public int HistoryIndex
        {
            get => _historyIndex;
            set
            {
                if (_historyIndex != value)
                {
                    if (value < 0 || value >= LogoColorsRecordHistory.Count)
                        throw new ArgumentOutOfRangeException(nameof(value), value, "HistoryIndex out of range");

                    _logoColorsRecord = LogoColorsRecordHistory[value].Lcr;
                    SvgXDoc = LogoHelper.GetLogoAsXDocument(_logoColorsRecord);
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
            => _historyIndex < LogoColorsRecordHistory.Count -1;

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
                    Debug.WriteLine("Assigned and notifying of SelectedLogoPresetRecord");
                    if (value != null)
                    { // Sets color values and reloads SVG with these colors applied.
                        UseLogoColors(value.Lcr);
                    }
                }
            }
        }

        public ReactiveCommand<LogoColorsRecord, Unit> UseLogoColorsCommand =>
            ReactiveCommand.Create<LogoColorsRecord>(UseLogoColors);

        [AutoPropertyChanged]
        private string _customName = string.Empty;

        public ObservableCollection<LogoPresetCollectionRecord> PresetCollections =>
            [
                new LogoPresetCollectionRecord("Defaults",
                [
                    new LogoPresetRecord(
                        "Light mode default",
                        LogoColorsRecord.LightModeDefault,
                        false
                    ),
                    new LogoPresetRecord(
                        "Dark mode default",
                        LogoColorsRecord.DarkModeDefault,
                        true
                    ),
                    new LogoPresetRecord(
                        "Old logo inspired",
                        new LogoColorsRecord(
                            Q: Color.Parse("rgb(137,107,178)"),
                            B: Color.Parse("rgb(137,107,178)"),
                            C: Color.Parse("rgb(137,107,178)"),
                            GradientCenter: Color.Parse("rgb(51,0,128)"),
                            GradientFill: Color.Parse("rgb(137,107,178)"),
                            GradientRim: Color.Parse("rgb(224,201,255)")
                        ),
                        true
                    ),
                    new LogoPresetRecord(
                        "qBittorrent, but different",
                        new LogoColorsRecord(
                            Q: Color.Parse("#97C5E8"),
                            B: Color.Parse("#97C5E8"),
                            C: Color.Parse("#97C5E8"),
                            GradientCenter: Color.Parse("#66a6ea"),
                            GradientFill: Color.Parse("#356ebf"),
                            GradientRim: Color.Parse("#FFF")
                        ),
                        false
                    ),

                ]),
                new LogoPresetCollectionRecord("System accent color",
                [
                    new LogoPresetRecord(
                        "System accent dark",
                        new LogoColorsRecord(
                            Q: ThemeColors.SystemAccentLight1,
                            B: ThemeColors.SystemAccentLight1,
                            C: ThemeColors.SystemAccentLight1,
                            GradientCenter: ThemeColors.SystemAccentDark3,
                            GradientFill: ThemeColors.SystemAccentDark1,
                            GradientRim: ThemeColors.SystemAccentLight3
                        ),
                        true
                    ),
                    new LogoPresetRecord(
                        "System accent light",
                        new LogoColorsRecord(
                            Q: ThemeColors.SystemAccentDark3,
                            B: ThemeColors.SystemAccentDark3,
                            C: ThemeColors.SystemAccentDark3,
                            GradientCenter: ThemeColors.SystemAccentLight1,
                            GradientFill: ThemeColors.SystemAccent,
                            GradientRim: ThemeColors.SystemAccentLight3
                        ),
                        false
                    )
                ])
            ];

        [AutoPropertyChanged]
        private int _selectedPresetCollectionIndex = 0;

        private void UseLogoColors(LogoColorsRecord lcr)
        {
            LoadLogoPresetRecord(lcr);
            SyncColorsWithLogoRecord();
            SvgXDoc = LogoHelper.GetLogoAsXDocument(lcr);
        }

        public void SyncColorsWithLogoRecord()
        {
            if (_q_color != LogoColorsRecord.Q)
            {
                _q_color = LogoColorsRecord.Q;
                this.RaisePropertyChanged(nameof(Q_Color));
            }
            if (_b_color != LogoColorsRecord.B)
            {
                _b_color = LogoColorsRecord.B;
                this.RaisePropertyChanged(nameof(B_Color));
            }
            if (_c_color != LogoColorsRecord.C)
            {
                _c_color = LogoColorsRecord.C;
                this.RaisePropertyChanged(nameof(C_Color));
            }
            if (_gradientCenterColor != LogoColorsRecord.GradientCenter)
            {
                _gradientCenterColor = LogoColorsRecord.GradientCenter;
                this.RaisePropertyChanged(nameof(GradientCenterColor));
            }
            if (_gradientFillColor != LogoColorsRecord.GradientFill)
            {
                _gradientFillColor = LogoColorsRecord.GradientFill;
                this.RaisePropertyChanged(nameof(GradientFillColor));

            }
            if (_gradientRimColor != LogoColorsRecord.GradientRim)
            {
                _gradientRimColor = LogoColorsRecord.GradientRim;
                this.RaisePropertyChanged(nameof(GradientRimColor));
            }
        }

        public ReactiveCommand<bool, Unit> SaveCommand =>
            ReactiveCommand.Create<bool>(Save);

        private void Save(bool saveForDarkMode)
        {
            ExportLogoColorsRecordToDisk(); // Save the export
        }

        private XDocument _svgXDoc;
        /// <summary>
        /// For internal use only. <br/>
        /// You should probably be using <see cref="LogoColorsRecord"/> instead 
        /// which does internal bookkeeping to keep the undo history intact
        /// </summary>
        private LogoColorsRecord _logoColorsRecord;

        /// <summary>
        /// Assigns to _logoColordsRecord and adds it to history if it's actually different
        /// Also sets the previous item as <see cref="LogoColorsRecordHistorySelectedUndoItem"/>
        /// </summary>
        private LogoColorsRecord LogoColorsRecord
        {
            get => _logoColorsRecord;
            set
            {
                Debug.WriteLine("LogoColorsRecord called");
                if (LogoColorsRecordHistory.ElementAt(HistoryIndex).Lcr != value)
                {
                    _logoColorsRecord = value;

                    PurgeRedoHistory();
                    LogoColorsRecordHistory.Add(new LogoColorsHistoryRecord(value));
                    HistoryIndex++;
                }
                else
                    Debug.WriteLine("Unexpected");
            }
        }

        /// <summary>
        /// Removes all entries beyond the current <see cref="HistoryIndex"/>, 
        /// basically call this when adding a new <see cref="LogoColorsRecordHistory"/> entry
        /// </summary>
        private void PurgeRedoHistory()
        {
            int nextIndex = HistoryIndex + 1;
            if (nextIndex < LogoColorsRecordHistory.Count)
                LogoColorsRecordHistory.RemoveMany(LogoColorsRecordHistory.ToList()[nextIndex..]);
        }

        private void LoadLogoPresetRecord(LogoColorsRecord lcr)
        {
            Debug.WriteLine("LoadLogoPresetRecord called");
            LogoColorsRecord = lcr; // Adds to history
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

        public bool AddLogoColorsRecordToHistory()
        {
            if(_logoColorsRecord != LogoColorsRecordHistory.ElementAt(HistoryIndex).Lcr)
            {
                PurgeRedoHistory();
                LogoColorsRecordHistory.Add(new LogoColorsHistoryRecord(_logoColorsRecord));
                HistoryIndex++;
                return true;
            }

            return false;
        }

        private Color _q_color;

        /// <summary>
        /// Does not persist change to history, please use <see cref="AddLogoColorsRecordToHistory"/> to do so.
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
                    _logoColorsRecord = _logoColorsRecord with { Q = value };

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
                    _logoColorsRecord = _logoColorsRecord with { B = value };

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
                    _logoColorsRecord = _logoColorsRecord with { C = value };

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
                    _logoColorsRecord = _logoColorsRecord with { GradientCenter = value };

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
                    _logoColorsRecord = _logoColorsRecord with { GradientFill = value };

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
                    _logoColorsRecord = _logoColorsRecord with { GradientRim = value };

                    this.RaisePropertyChanged(nameof(GradientRimColor));
                    this.RaisePropertyChanged(nameof(PreviewSvg));
                }
            }
        }

        public string ThemeMode => BoolToDarkModeText(IsInDarkMode);
        public string OppositeMode => BoolToDarkModeText(!IsInDarkMode);

        public static string BoolToDarkModeText(bool isInDarkMode) => isInDarkMode ? "dark" : "light";

        public void ExportLogoColorsRecordToDisk()
        {            
            string json = JsonConvert.SerializeObject(_logoColorsRecord, Formatting.Indented);
            DateTime dt = DateTime.Now;
            string fileName = dt.ToString("yyyy-MM-dd_HH-mm-ss") + ".json";
            string path = Path.Combine(App.LogoColorsExportDirectory, fileName);
            
            File.WriteAllText(path, json);
            AppLoggerService.AddLogMessage(
                Splat.LogLevel.Info,
                GetFullTypeName<IconCustomizationViewModel>(),
                $"Exported {fileName}",
                $"Created a new logo color profile file: {path}"
            );
        }

    }
}
