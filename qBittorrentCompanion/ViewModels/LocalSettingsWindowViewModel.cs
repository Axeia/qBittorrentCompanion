using Avalonia.Controls;
using Avalonia.Media;
using FluentIcons.Common;
using qBittorrentCompanion.Extensions;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Models;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using System.Xml.Linq;

namespace qBittorrentCompanion.ViewModels
{

    public enum IconSize
    {// Content = 28px
        Small, //16px,Small, +List, Details
        SmallPlus, // 24px, Taskbar, search results, start all apps list
        SmallPlusPlus, // 32px, Start pins
        Medium, //48px, Medium +Tile
        Large, // 96px, Extra large
        ExtraLarge, //256px, Extra Large
    }

    public record IconDefinition
    {
        public required int IconSize { get; init; }
        public required string[] UsageLocations { get; init; } 
    }


    public record IconSizeCollection
    {
        public required string Os { get; init; }
        public required string OsVersion { get; init; }
    }


    public partial class LocalSettingsWindowViewModel(bool isInDarkMode, LogoColorsRecord lcr) : ViewModelBase
    {
        public ReactiveCommand<LogoColorsRecord, Unit> UseLogoColorsCommand => 
            ReactiveCommand.Create<LogoColorsRecord>(UseLogoColors);

        private void UseLogoColors(LogoColorsRecord lcr)
        {
            SetLogoColorsRecord(lcr);
            SvgXDoc = LogoHelper.GetLogoAsXDocument(lcr);
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
                    SvgXDoc = SvgXDoc.SetSvgStroke("q", value.ToRgba());
                    _logoColorsRecord = _logoColorsRecord with { Q = value.ToHex() };

                    _q_HexColor = value.ToHex();
                    this.RaisePropertyChanged(nameof(Q_Color));
                    this.RaisePropertyChanged(nameof(Q_HexColor));
                    this.RaisePropertyChanged(nameof(PreviewSvg));
                }
            }
        }

        private string _q_HexColor = Color.Parse(lcr.Q).ToHex();
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
                if(Color.TryParse(value, out Color color))
                {
                    var colorAsHex = color.ToHex();
                    if(colorAsHex != _q_HexColor)
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
                    SvgXDoc = SvgXDoc.SetSvgStroke("b", value.ToRgba());
                    _logoColorsRecord = _logoColorsRecord with { B = value.ToHex() };

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
                    SvgXDoc = SvgXDoc.SetSvgStroke("c", value.ToRgba());
                    _logoColorsRecord = _logoColorsRecord with { C = value.ToHex() };

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
                    SvgXDoc = SvgXDoc.SetSvgGradientStop("gradient", 0, value.ToRgba());
                    _logoColorsRecord = _logoColorsRecord with { GradientCenter = value.ToHex() };

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
                    SvgXDoc = SvgXDoc.SetSvgGradientStop("gradient", 1, value.ToRgba());
                    _logoColorsRecord = _logoColorsRecord with { GradientFill = value.ToHex() };

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
                    SvgXDoc = SvgXDoc.SetSvgGradientStop("gradient", 2, value.ToRgba());
                    _logoColorsRecord = _logoColorsRecord with { GradientRim = value.ToHex() };

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

        // Might as well just pack pretty much every size in. Storage isn't really a concern.W
        private int[] _iconSizes = [16, 20, 24, 30, 32, 36, 40, 48, 60, 64, 72, 80, 96, 256];

        // Based off https://learn.microsoft.com/en-us/windows/apps/design/style/iconography/app-icon-construction
        public IReadOnlyDictionary<IconSize, string[]> WindowsUsageLocation => new Dictionary<IconSize, string[]>() {
            { IconSize.Small, ["Task mananager", "Context menu", "System tray", "Title bar"] }, // QBC uses a custom title bar, title bar might thus not be relevant to mention
            { IconSize.SmallPlus, ["Start all apps list", "Search results", "Task bar"] },
            { IconSize.SmallPlusPlus, ["Start pins"] },
            { IconSize.Medium, ["Common file manager size"] },
        };

        // The common sizes at 100% scaling. Use the scale as a multiplier if scaling is used.
        public Dictionary<IconSize, int> WindowBaseSizes = new() {
            { IconSize.Small, 16 },
            { IconSize.SmallPlus, 24 },
            { IconSize.SmallPlusPlus, 32 }
        };


        // Disable warning for static, need them as regular variables so they can be access by Avalonia's Bindings
        // Ironically the surpression leads to another thing needing to be supressed
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1822 // Mark members as static
        public string[] TabTitles => ["Directories", "Icon customization", "Notifications", "Start up settings"];
        public Symbol[] TabSymbols => [Symbol.Folder, Symbol.Color, Symbol.Alert, Symbol.WindowNew];
#pragma warning restore CA1822 // Mark members as static
#pragma warning restore IDE0079 // Remove unnecessary suppression

        private int _selectedTabIndex = 1;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                if (value != _selectedTabIndex)
                {
                    _selectedTabIndex = value;
                    this.RaisePropertyChanged(nameof(SelectedTabIndex));
                    this.RaisePropertyChanged(nameof(SelectedTabTitle));
                    this.RaisePropertyChanged(nameof(SelectedTabSymbol));
                }
            }
        }

        public string SelectedTabTitle => TabTitles[SelectedTabIndex];
        public Symbol SelectedTabSymbol => TabSymbols[SelectedTabIndex];

        private string _qbDownloadDirectory = Design.IsDesignMode? "/wherever/whenever" : ConfigService.DownloadDirectory;
        public string QbDownloadDirectory
        {
            get => _qbDownloadDirectory;
            set
            {
                if(value != _qbDownloadDirectory)
                {
                    _qbDownloadDirectory = value;
                    ConfigService.DownloadDirectory = value;
                }
            }
        }
    }
}
