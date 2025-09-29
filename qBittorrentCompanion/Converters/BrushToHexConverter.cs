using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Diagnostics;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Takes a <see cref="ISolidColorBrush"/>, extracts its <seealso cref="Color"/> and converts to a 9 character string<br/>.
    /// Or the other way around, parses a string to a color by usings its <seealso cref="Color.Parse(string)"/> 
    /// like #FF00FF00 for a fully opaque 
    /// </summary>
    public class BrushToHexConverter : IValueConverter
    {
        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is ISolidColorBrush solidColorBrush)
            {
                var color = solidColorBrush.Color;

                // In this project `return color.ToString(ColorFormat.HEX_ARGB);` would work as well,
                // but this reduces dependencies
                return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
            }

            return "#??????";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            /// Untested, just added for completeness sake
            try
            {
                return value is string str ? Color.Parse(str) : Colors.Fuchsia;
            }
            catch(Exception e)
            {
                Debug.WriteLine(e);
                return Colors.Aqua;
            }
        }
    }
}
