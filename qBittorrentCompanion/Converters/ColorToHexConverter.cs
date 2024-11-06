using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    public class ColorToHexConverter : IValueConverter
    {
        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is Color color)
                return ColorToHex(color);

            return AvaloniaProperty.UnsetValue;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is string hex && Color.TryParse(hex, out var color))
                return color;

            return AvaloniaProperty.UnsetValue;
        }

        public static string ColorToHex(Color color) 
            => $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}
