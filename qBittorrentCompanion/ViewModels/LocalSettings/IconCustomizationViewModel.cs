using AutoPropertyChangedGenerator;
using Avalonia.Media;
using Newtonsoft.Json;
using qBittorrentCompanion.Extensions;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Models;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Xml.Linq;

namespace qBittorrentCompanion.ViewModels.LocalSettings
{
    public record LogoPresetRecord(string Name, LogoColorsRecord Lcr, bool IsForDarkMode);
    public record LogoPresetCollectionRecord(string Name, LogoPresetRecord[] LogoPresets);

    public partial class IconCustomizationViewModel(bool isInDarkMode, LogoColorsRecord lcr) : ViewModelBase
    {
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
        private int _selectedPresetCollectionIndex = 1;

        private void UseLogoColors(LogoColorsRecord lcr)
        {
            LoadLogoPresetRecord(lcr);
            SvgXDoc = LogoHelper.GetLogoAsXDocument(lcr);
        }
        public ReactiveCommand<bool, Unit> SaveCommand =>
            ReactiveCommand.Create<bool>(Save);

        private void Save(bool saveForDarkMode)
        {
            ExportLogoColorsRecordToDisk(); // Save the export
        }

        private XDocument _svgXDoc = LogoHelper.GetLogoAsXDocument(lcr);
        private LogoColorsRecord _logoColorsRecord = lcr;

        private void LoadLogoPresetRecord(LogoColorsRecord lcr)
        {
            _logoColorsRecord = lcr;
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


        private Color _q_color = lcr.Q;
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

                    _q_HexColor = value.ToString(ColorFormat.HEX_ARGB);
                    this.RaisePropertyChanged(nameof(Q_Color));
                    this.RaisePropertyChanged(nameof(Q_HexColor));
                    this.RaisePropertyChanged(nameof(PreviewSvg));
                }
            }
        }

        private string _q_HexColor = lcr.Q.ToString(ColorFormat.HEX_ARGB);
        /// <summary>
        /// In sync with <see cref="Q_Color"/>, its backing field <see cref="_q_color"/> will get updated
        /// and`RaisePropertyChanged` will be run for <see cref="Q_Color"/>
        /// 
        /// As to avoid a loop condition it does not edit <see cref="Q_Color"/> directly
        /// </summary>
        public string Q_HexColor
        {
            get => _q_HexColor;
            set
            {
                if (Color.TryParse(value, out Color color))
                {
                    var colorAsHex = color.ToString(ColorFormat.HEX_ARGB);
                    if (colorAsHex != _q_HexColor)
                    {
                        _q_HexColor = colorAsHex;
                        _q_color = color;
                        this.RaisePropertyChanged(nameof(Q_HexColor));
                        this.RaisePropertyChanged(nameof(Q_Color));
                        this.RaisePropertyChanged(nameof(PreviewSvg));
                    }
                }
            }
        }

        private Color _b_color = lcr.B;
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

        private Color _c_color = lcr.C;
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

        private Color _gradientCenterColor = lcr.GradientCenter;
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

        private Color _gradientFillColor = lcr.GradientFill;
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

        private Color _gradientRimColor = lcr.GradientRim;
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

        private bool _isInDarkMode = isInDarkMode;
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
