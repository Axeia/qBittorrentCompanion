using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Converts an <see cref="int"/> into a fixed-width string for UI display.
    /// Always a fixed length of 3 characters (hence the name). Either 99+ or the value padded with spaces<br/>
    /// Padding can be assigned through parameter ("Right" for right-padding, otherwise defaults to left-padding).
    /// </summary>
    /// <remarks>
    /// Useful for aligning numeric counters in list views, badges, or grid cells.
    /// Padding direction is controlled via the <paramref name="parameter"/>:
    /// <c>"Right"</c> for right-padding, anything else for left-padding.
    /// </remarks>
    public class FixedLengthIntConverter : IValueConverter
    {
        /// <summary>
        /// Converts an integer to a padded string, or <c>"99+"</c> if the value exceeds 99.
        /// </summary>
        /// <param name="value">The integer value to format.</param>
        /// <param name="targetType">Irrelevant</param>
        /// <param name="parameter">"Right" for right-padding, otherwise defaults to left-padding.</param>
        /// <param name="culture">Irrelevant</param>
        /// <returns>A padded string representation of the integer.</returns>
        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is int v)
            {
                if (v > 99)
                    return "99+";

                return parameter is string pad && pad == "Right"
                    ? v.ToString().PadRight(3, ' ')
                    : v.ToString().PadLeft(3, ' ');
            }

            return string.Empty;
        }

        /// <summary>
        /// Not supported — returns <see cref="BindingOperations.DoNothing"/>.
        /// </summary>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            return BindingOperations.DoNothing;
        }
    }
}