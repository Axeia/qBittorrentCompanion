using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Converts a <see cref="Color"/> into a <see cref="SolidColorBrush"/>, and vice versa.
    /// Used for UI bindings where color values are rendered or edited.
    /// </summary>
    public class ColorToBrushConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="Color"/> into a <see cref="SolidColorBrush"/>.
        /// </summary>
        /// <param name="value">Expected to be a <see cref="Color"/>.</param>
        /// <returns>A <see cref="SolidColorBrush"/> with the given color.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is not a <see cref="Color"/>.</exception>
        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is Color color)
                return new SolidColorBrush(color);

            throw new ArgumentOutOfRangeException(nameof(value), value, $"Expected a {nameof(Color)} value.");
        }

        /// <summary>
        /// Converts a <see cref="SolidColorBrush"/> back into its <see cref="Color"/>.
        /// </summary>
        /// <param name="value">Expected to be a <see cref="SolidColorBrush"/>.</param>
        /// <returns>The <see cref="Color"/> of the brush.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is not a <see cref="SolidColorBrush"/>.</exception>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is SolidColorBrush solidBrush)
                return solidBrush.Color;

            throw new ArgumentOutOfRangeException(nameof(value), value, $"Expected a {nameof(SolidColorBrush)} value.");
        }
    }
}
