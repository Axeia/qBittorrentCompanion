using AutoPropertyChangedGenerator;
using Avalonia.Media;
using Newtonsoft.Json;
using qBittorrentCompanion.Extensions;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Models;
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
    public record LogoColorsHistoryRecord(LogoColorsRecord Lcr, string Id)
    {
        public LogoColorsHistoryRecord(LogoColorsRecord lcr)
            : this(lcr, DateTime.Now.ToLongTimeString()) { }
    }

    public record LogoPresetRecord(string Name, LogoColorsRecord Lcr, bool IsForDarkMode);
    public record LogoPresetCollectionRecord(string Name, LogoPresetRecord[] LogoPresets);

    public partial class IconCustomizationViewModel : ViewModelBase
    {
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
            _logoColorsRecordHistory.Add(new LogoColorsHistoryRecord(lcr));

            _logoColorsRecordHistory.CollectionChanged += (s, e) => RecheckUndoRedoLogic();
        }

        private void RecheckUndoRedoLogic()
        {
            this.RaisePropertyChanged(nameof(LogoColorsRecordUndoHistory));
            this.RaisePropertyChanged(nameof(CanUndo));
            this.RaisePropertyChanged(nameof(LogoColorsRecordRedoHistory));
            this.RaisePropertyChanged(nameof(CanRedo));
        }

        /// <summary>
        /// Keeps track of changes as LogoColorsRecord is immutable it's always created as a full set of all the relevant data<br/>
        /// <see cref="LogoColorsRecordHistory"/> is the property that's actually used for the UI presentation as it skips the 
        /// last (which is the current) result and reverses the list making it suitable for an undo history.
        /// </summary>
        private ObservableCollection<LogoColorsHistoryRecord> _logoColorsRecordHistory = [];
        /// <summary>
        /// Strips out the current entry and reverses the list for UI display 
        /// A subselection of <see cref="_logoColorsRecordHistory"/>, it contains 
        /// its entries ending at LogoColorsRecordHistrySelectedItem.
        /// </summary>
        public IEnumerable<LogoColorsHistoryRecord> LogoColorsRecordUndoHistory
            => _logoColorsRecordHistorySelectedUndoItem == null
                ? _logoColorsRecordHistory.SkipLast(1).Reverse()
                : _logoColorsRecordHistory.Take(_logoColorsRecordHistory.IndexOf(_logoColorsRecordHistorySelectedUndoItem));

        public IEnumerable<LogoColorsHistoryRecord> LogoColorsRecordRedoHistory
            => _logoColorsRecordHistorySelectedUndoItem == null
                ? []
                : _logoColorsRecordHistory.SkipWhile(r => r != LogoColorsRecordHistorySelectedRedoItem).Skip(1);

        public bool CanUndo
            => LogoColorsRecordHistorySelectedUndoItem is not null;

        public bool CanRedo
            => LogoColorsRecordHistorySelectedRedoItem is not null;

        private LogoColorsHistoryRecord? _logoColorsRecordHistorySelectedUndoItem = null;

        public LogoColorsHistoryRecord? LogoColorsRecordHistorySelectedUndoItem
        {
            get => _logoColorsRecordHistorySelectedUndoItem;
            set
            {
                Debug.WriteLine("Setting LogoColorsRecordHistorySelectedUndoItem");
                if (_logoColorsRecordHistorySelectedUndoItem != value)
                {
                    _logoColorsRecordHistorySelectedUndoItem = value;
                    this.RaisePropertyChanged(nameof(LogoColorsRecordHistorySelectedUndoItem));
                    this.RaisePropertyChanged(nameof(CanUndo));
                }
                else
                {
                    Debug.WriteLine("Did not assign LogoColorsRecordHistorySelectedUndoItem - same as previous");
                }
            }
        }

        private LogoColorsHistoryRecord? _logoColorsRecordHistorySelectedRedoItem = null;

        public LogoColorsHistoryRecord? LogoColorsRecordHistorySelectedRedoItem
        {
            get => _logoColorsRecordHistorySelectedRedoItem;
            set
            {
                if (_logoColorsRecordHistorySelectedRedoItem != value)
                {
                    _logoColorsRecordHistorySelectedRedoItem = value;
                    this.RaisePropertyChanged(nameof(LogoColorsRecordHistorySelectedRedoItem));
                }
            }
        }

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
            Debug.WriteLine("UseLogoColors called");
            LoadLogoPresetRecord(lcr);
            SvgXDoc = LogoHelper.GetLogoAsXDocument(lcr);
        }

        public ReactiveCommand<Unit, Unit> UndoCommand 
            => ReactiveCommand.Create(Undo);

        public void Undo()
        {
            Debug.WriteLine("Undoing");
            if(LogoColorsRecordHistorySelectedUndoItem is not null)
            {
                _logoColorsRecord = LogoColorsRecordHistorySelectedUndoItem.Lcr;

                var index = _logoColorsRecordHistory.IndexOf(LogoColorsRecordHistorySelectedUndoItem);
                _logoColorsRecordHistorySelectedUndoItem = (index-1) >= 0
                    ? _logoColorsRecordHistory[index-1]
                    : null;
                _logoColorsRecordHistorySelectedRedoItem = _logoColorsRecordHistory[index];

                SvgXDoc = LogoHelper.GetLogoAsXDocument(_logoColorsRecord);
                RecheckUndoRedoLogic();
            }
        }

        public ReactiveCommand<Unit, Unit> RedoCommand
            => ReactiveCommand.Create(Redo);

        private void Redo()
        {
            if (LogoColorsRecordHistorySelectedRedoItem is not null)
            {
                _logoColorsRecord = LogoColorsRecordHistorySelectedRedoItem.Lcr;

                var index = _logoColorsRecordHistory.IndexOf(LogoColorsRecordHistorySelectedRedoItem);

                SvgXDoc = LogoHelper.GetLogoAsXDocument(_logoColorsRecord);
                RecheckUndoRedoLogic();
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
            get => LogoColorsRecord;
            set
            {
                Debug.WriteLine("LogoColorsRecord called");
                if (_logoColorsRecordHistory.Last()?.Lcr != value)
                {
                    LogoColorsRecordHistorySelectedUndoItem = _logoColorsRecordHistory.Last();
                    _logoColorsRecord = value;
                    _logoColorsRecordHistory.Add(new LogoColorsHistoryRecord(value));
                }
                else
                    Debug.WriteLine("Unexpected");
            }
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


        private Color _q_color;

        /// <summary>
        /// In sync with <see cref="Q_HexColor"/>, its backing field <see cref="_q_HexColor"/> will get updated
        /// and`RaisePropertyChanged` will be run for <see cref="Q_HexColor"/>
        /// 
        /// As to avoid a loop condition it does not edit <see cref="Q_HexColor"/> directly
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
            // TODO set light / dark mode
            string path = Path.Combine(App.LogoColorsExportDirectory, fileName);
            Debug.WriteLine($"Saving colors to {path}");

            File.WriteAllText(path, json);
        }

    }
}
