using Avalonia.Media;
using System.Collections.Generic;

namespace qBittorrentCompanion.Helpers
{
    public static class ThemeColors
    {
        private enum FluentColor
        {
            SystemAltHigh,
            SystemAltLow,
            SystemAltMedium,
            SystemAltMediumHigh,
            SystemAltMediumLow,
            SystemBaseHigh,
            SystemBaseLow,
            SystemBaseMedium,
            SystemBaseMediumHigh,
            SystemBaseMediumLow,
            SystemChromeAltLow,
            SystemChromeBlackHigh,
            SystemChromeBlackLow,
            SystemChromeBlackMedium,
            SystemChromeBlackMediumLow,
            SystemChromeDisabledHigh,
            SystemChromeGray,
            SystemChromeHigh,
            SystemChromeLow,
            SystemChromeMedium,
            SystemChromeMediumLow,
            SystemChromeWhite,
            SystemErrorText,
            SystemListLow,
            SystemListMedium,
            SystemRegion,
            SystemAccent,
            SystemAccent1,
            SystemAccentDark1,
            SystemAccentDark2,
            SystemAccentDark3,
            SystemAccentLight1,
            SystemAccentLight2,
            SystemAccentLight3
        }

        private static readonly Dictionary<FluentColor, string> ResourceMap = new()
        {
            { FluentColor.SystemAltHigh, "SystemAltHighColor" },
            { FluentColor.SystemAltLow, "SystemAltLowColor" },
            { FluentColor.SystemAltMedium, "SystemAltMediumColor" },
            { FluentColor.SystemAltMediumHigh, "SystemAltMediumHighColor" },
            { FluentColor.SystemAltMediumLow, "SystemAltMediumLowColor" },
            { FluentColor.SystemBaseHigh, "SystemBaseHighColor" },
            { FluentColor.SystemBaseLow, "SystemBaseLowColor" },
            { FluentColor.SystemBaseMedium, "SystemBaseMediumColor" },
            { FluentColor.SystemBaseMediumHigh, "SystemBaseMediumHighColor" },
            { FluentColor.SystemBaseMediumLow, "SystemBaseMediumLowColor" },
            { FluentColor.SystemChromeAltLow, "SystemChromeAltLowColor" },
            { FluentColor.SystemChromeBlackHigh, "SystemChromeBlackHighColor" },
            { FluentColor.SystemChromeBlackLow, "SystemChromeBlackLowColor" },
            { FluentColor.SystemChromeBlackMedium, "SystemChromeBlackMediumColor" },
            { FluentColor.SystemChromeBlackMediumLow, "SystemChromeBlackMediumLowColor" },
            { FluentColor.SystemChromeDisabledHigh, "SystemChromeDisabledHighColor" },
            { FluentColor.SystemChromeGray, "SystemChromeGrayColor" },
            { FluentColor.SystemChromeHigh, "SystemChromeHighColor" },
            { FluentColor.SystemChromeLow, "SystemChromeLowColor" },
            { FluentColor.SystemChromeMedium, "SystemChromeMediumColor" },
            { FluentColor.SystemChromeMediumLow, "SystemChromeMediumLowColor" },
            { FluentColor.SystemChromeWhite, "SystemChromeWhiteColor" },
            { FluentColor.SystemErrorText, "SystemErrorTextColor" },
            { FluentColor.SystemListLow, "SystemListLowColor" },
            { FluentColor.SystemListMedium, "SystemListMediumColor" },
            { FluentColor.SystemRegion, "SystemRegionColor" },
            { FluentColor.SystemAccent, "SystemAccentColor" },
            { FluentColor.SystemAccent1, "SystemAccentColor1" },
            { FluentColor.SystemAccentDark1, "SystemAccentColorDark1" },
            { FluentColor.SystemAccentDark2, "SystemAccentColorDark2" },
            { FluentColor.SystemAccentDark3, "SystemAccentColorDark3" },
            { FluentColor.SystemAccentLight1, "SystemAccentColorLight1" },
            { FluentColor.SystemAccentLight2, "SystemAccentColorLight2" },
            { FluentColor.SystemAccentLight3, "SystemAccentColorLight3" }
        };

        private static Color GetColor(FluentColor fluentColor)
        {
            App.Current!.TryGetResource(ResourceMap[fluentColor], Avalonia.Styling.ThemeVariant.Default, out var color);

            if (color is Color c)
                return c;
            if (color is ISolidColorBrush b)
                return b.Color;

            return Colors.Aqua;
        }

        // Add properties for each color
        public static Color SystemAltHigh => GetColor(FluentColor.SystemAltHigh);
        public static Color SystemAltLow => GetColor(FluentColor.SystemAltLow);
        public static Color SystemAltMedium => GetColor(FluentColor.SystemAltMedium);
        public static Color SystemAltMediumHigh => GetColor(FluentColor.SystemAltMediumHigh);
        public static Color SystemAltMediumLow => GetColor(FluentColor.SystemAltMediumLow);
        public static Color SystemBaseHigh => GetColor(FluentColor.SystemBaseHigh);
        public static Color SystemBaseLow => GetColor(FluentColor.SystemBaseLow);
        public static Color SystemBaseMedium => GetColor(FluentColor.SystemBaseMedium);
        public static Color SystemBaseMediumHigh => GetColor(FluentColor.SystemBaseMediumHigh);
        public static Color SystemBaseMediumLow => GetColor(FluentColor.SystemBaseMediumLow);
        public static Color SystemChromeAltLow => GetColor(FluentColor.SystemChromeAltLow);
        public static Color SystemChromeBlackHigh => GetColor(FluentColor.SystemChromeBlackHigh);
        public static Color SystemChromeBlackLow => GetColor(FluentColor.SystemChromeBlackLow);
        public static Color SystemChromeBlackMedium => GetColor(FluentColor.SystemChromeBlackMedium);
        public static Color SystemChromeBlackMediumLow => GetColor(FluentColor.SystemChromeBlackMediumLow);
        public static Color SystemChromeDisabledHigh => GetColor(FluentColor.SystemChromeDisabledHigh);
        public static Color SystemChromeGray => GetColor(FluentColor.SystemChromeGray);
        public static Color SystemChromeHigh => GetColor(FluentColor.SystemChromeHigh);
        public static Color SystemChromeLow => GetColor(FluentColor.SystemChromeLow);
        public static Color SystemChromeMedium => GetColor(FluentColor.SystemChromeMedium);
        public static Color SystemChromeMediumLow => GetColor(FluentColor.SystemChromeMediumLow);
        public static Color SystemChromeWhite => GetColor(FluentColor.SystemChromeWhite);
        public static Color SystemErrorText => GetColor(FluentColor.SystemErrorText);
        public static Color SystemListLow => GetColor(FluentColor.SystemListLow);
        public static Color SystemListMedium => GetColor(FluentColor.SystemListMedium);
        public static Color SystemRegion => GetColor(FluentColor.SystemRegion);
        public static Color SystemAccent => GetColor(FluentColor.SystemAccent);
        public static Color SystemAccent1 => GetColor(FluentColor.SystemAccent1);
        public static Color SystemAccentDark1 => GetColor(FluentColor.SystemAccentDark1);
        public static Color SystemAccentDark2 => GetColor(FluentColor.SystemAccentDark2);
        public static Color SystemAccentDark3 => GetColor(FluentColor.SystemAccentDark3);
        public static Color SystemAccentLight1 => GetColor(FluentColor.SystemAccentLight1);
        public static Color SystemAccentLight2 => GetColor(FluentColor.SystemAccentLight2);
        public static Color SystemAccentLight3 => GetColor(FluentColor.SystemAccentLight3);
    }
}
