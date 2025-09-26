using AutoPropertyChangedGenerator;
using Avalonia.Media;
using Avalonia.Styling;
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
using System.Reactive;
using System.Xml.Linq;

namespace qBittorrentCompanion.ViewModels.LocalSettings
{
    public record LogoPreset(string Name, String Svg, bool IsForDarkMode);
    public record LogoPresetCollection(string Name, LogoPreset[] LogoPresets);

    public partial class IconCustomizationViewModel(bool isInDarkMode, LogoColorsRecord lcr) : ViewModelBase
    {
        public ReactiveCommand<LogoColorsRecord, Unit> UseLogoColorsCommand =>
            ReactiveCommand.Create<LogoColorsRecord>(UseLogoColors);

        [AutoPropertyChanged]
        private string _customName = string.Empty;

        public ObservableCollection<LogoPresetCollection> PresetCollections =>
            [
                new LogoPresetCollection("Defaults",
                [
                    new LogoPreset(
                        "Light mode default",
                        LogoHelper.GetLogoAsXDocument(LogoColorsRecord.LightModeDefault).ToString(),
                        false
                    ),
                    new LogoPreset(
                        "Dark mode default",
                        LogoHelper.GetLogoAsXDocument(LogoColorsRecord.DarkModeDefault).ToString(),
                        true
                    ),
                    new LogoPreset(
                        "Old logo inspired",
                        LogoHelper.GetLogoAsXDocument(new LogoColorsRecord(
                            Q: "rgb(137,107,178)",
                            B: "rgb(137,107,178)",
                            C: "rgb(137,107,178)",
                            GradientCenter: "rgb(51,0,128)",
                            GradientFill: "rgb(137,107,178)",
                            GradientRim: "rgb(224,201,255)"
                        )).ToString(),
                        true
                    ),
                    new LogoPreset(
                        "qBittorrent, but different",
                        LogoHelper.GetLogoAsXDocument(new LogoColorsRecord(
                            Q: "#97C5E8",
                            B: "#97C5E8",
                            C: "#97C5E8",
                            GradientCenter: "#66a6ea",
                            GradientFill: "#356ebf",
                            GradientRim: "#FFF"
                        )).ToString(),
                        false
                    ),

                ]),
                new LogoPresetCollection("System accent color",
                [
                    new LogoPreset(
                        "System accent dark",
                        LogoHelper.GetLogoAsXDocument(new LogoColorsRecord(
                            Q: ThemeColors.SystemAccentLight1.ToString(ColorFormat.RGBA_ALPHA_FLOAT),
                            B: ThemeColors.SystemAccentLight1.ToString(ColorFormat.RGBA_ALPHA_FLOAT),
                            C: ThemeColors.SystemAccentLight1.ToString(ColorFormat.RGBA_ALPHA_FLOAT),
                            GradientCenter: ThemeColors.SystemAccentDark3.ToString(ColorFormat.RGBA_ALPHA_FLOAT),
                            GradientFill: ThemeColors.SystemAccentDark1.ToString(ColorFormat.RGBA_ALPHA_FLOAT),
                            GradientRim: ThemeColors.SystemAccentLight3.ToString(ColorFormat.RGBA_ALPHA_FLOAT)
                        )).ToString(),
                        true
                    ),
                    new LogoPreset(
                        "System accent light",
                        LogoHelper.GetLogoAsXDocument(new LogoColorsRecord(
                            Q: ThemeColors.SystemAccentDark3.ToString(ColorFormat.RGBA_ALPHA_FLOAT),
                            B: ThemeColors.SystemAccentDark3.ToString(ColorFormat.RGBA_ALPHA_FLOAT),
                            C: ThemeColors.SystemAccentDark3.ToString(ColorFormat.RGBA_ALPHA_FLOAT),
                            GradientCenter: ThemeColors.SystemAccentLight1.ToString(ColorFormat.RGBA_ALPHA_FLOAT),
                            GradientFill: ThemeColors.SystemAccent.ToString(ColorFormat.RGBA_ALPHA_FLOAT),
                            GradientRim: ThemeColors.SystemAccentLight3.ToString(ColorFormat.RGBA_ALPHA_FLOAT)
                        )).ToString(),
                        false
                    )
                ])
            ];

        [AutoPropertyChanged]
        private int _selectedPresetCollectionIndex = 1;

        private void UseLogoColors(LogoColorsRecord lcr)
        {
            SetLogoColorsRecord(lcr);
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
        private void SetLogoColorsRecord(LogoColorsRecord lcr)
        {
            _logoColorsRecord = lcr;
            Q_Color = Color.Parse(lcr.Q);
            B_Color = Color.Parse(lcr.B);
            C_Color = Color.Parse(lcr.C);
            GradientCenterColor = Color.Parse(lcr.GradientCenter);
            GradientFillColor = Color.Parse(lcr.GradientFill);
            GradientRimColor = Color.Parse(lcr.GradientRim);
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


        private Color _q_color = Color.Parse(lcr.Q);
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
                    _logoColorsRecord = _logoColorsRecord with { Q = value.ToString(ColorFormat.HEX_ARGB) };

                    _q_HexColor = value.ToString(ColorFormat.HEX_ARGB);
                    this.RaisePropertyChanged(nameof(Q_Color));
                    this.RaisePropertyChanged(nameof(Q_HexColor));
                    this.RaisePropertyChanged(nameof(PreviewSvg));
                }
            }
        }

        private string _q_HexColor = Color.Parse(lcr.Q).ToString(ColorFormat.HEX_ARGB);
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

        private Color _b_color = Color.Parse(lcr.B);
        public Color B_Color
        {
            get => _b_color;
            set
            {
                if (_b_color != value)
                {
                    _b_color = value;
                    SvgXDoc = SvgXDoc.SetSvgStroke("b", value.ToString(ColorFormat.RGBA_ALPHA_FLOAT));
                    _logoColorsRecord = _logoColorsRecord with { B = value.ToString(ColorFormat.HEX_ARGB) };

                    this.RaisePropertyChanged(nameof(PreviewSvg));
                }
            }
        }

        private Color _c_color = Color.Parse(lcr.C);
        public Color C_Color
        {
            get => _c_color;
            set
            {
                if (_c_color != value)
                {
                    _c_color = value;
                    SvgXDoc = SvgXDoc.SetSvgStroke("c", value.ToString(ColorFormat.RGBA_ALPHA_FLOAT));
                    _logoColorsRecord = _logoColorsRecord with { C = value.ToString(ColorFormat.HEX_ARGB) };

                    this.RaisePropertyChanged(nameof(PreviewSvg));
                }
            }
        }

        private Color _gradientCenterColor = Color.Parse(lcr.GradientCenter);
        public Color GradientCenterColor
        {
            get => _gradientCenterColor;
            set
            {
                if (_gradientCenterColor != value)
                {
                    _gradientCenterColor = value;
                    SvgXDoc = SvgXDoc.SetSvgGradientStop("gradient", 0, value.ToString(ColorFormat.RGBA_ALPHA_FLOAT));
                    _logoColorsRecord = _logoColorsRecord with { GradientCenter = value.ToString(ColorFormat.HEX_ARGB) };

                    this.RaisePropertyChanged(nameof(PreviewSvg));
                }
            }
        }

        private Color _gradientFillColor = Color.Parse(lcr.GradientFill);
        public Color GradientFillColor
        {
            get => _gradientFillColor;
            set
            {
                if (_gradientFillColor != value)
                {
                    _gradientFillColor = value;
                    SvgXDoc = SvgXDoc.SetSvgGradientStop("gradient", 1, value.ToString(ColorFormat.RGBA_ALPHA_FLOAT));
                    _logoColorsRecord = _logoColorsRecord with { GradientFill = value.ToString(ColorFormat.HEX_ARGB) };

                    this.RaisePropertyChanged(nameof(PreviewSvg));

                    Debug.WriteLine(SvgXDoc.ToString());
                }
            }
        }

        private Color _gradientRimColor = Color.Parse(lcr.GradientRim);
        public Color GradientRimColor
        {
            get => _gradientRimColor;
            set
            {
                if (_gradientRimColor != value)
                {
                    _gradientRimColor = value;
                    SvgXDoc = SvgXDoc.SetSvgGradientStop("gradient", 2, value.ToString(ColorFormat.RGBA_ALPHA_FLOAT));
                    _logoColorsRecord = _logoColorsRecord with { GradientRim = value.ToString(ColorFormat.HEX_ARGB) };

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
