using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Converts a sentinel value of <c>-1</c> (as <see cref="long"/>) into the string <c>"N/A"</c> for display.
    /// Converts <c>"N/A"</c> back into <c>-1</c>, and parses other numeric strings into <see cref="long"/>.
    /// </summary>
    /// <remarks>
    /// Useful for rendering optional or unavailable numeric values in a user-friendly way.
    /// </remarks>
    public class NegativeOneToNAConverter : IValueConverter
    {
        /// <summary>
        /// Converts <c>-1</c> to <c>"N/A"</c>, or returns the string representation of the value.
        /// </summary>
        /// <param name="value">Expected to be a <see cref="long"/>.</param>
        /// <param name="targetType">Irrelevant</param>
        /// <param name="parameter">Irrelevant</param>
        /// <param name="culture">Irrelevant</param>
        /// <returns><c>"N/A"</c> if value is <c>-1</c>, otherwise <c>value.ToString()</c>.</returns>
        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            return value is long l && l == -1
                ? "N/A"
                : value?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Converts <c>"N/A"</c> to <c>-1</c>, or parses numeric strings into <see cref="long"/>.
        /// </summary>
        /// <param name="value">Expected to be a <see cref="string"/>.</param>
        /// <param name="targetType">Irrelevant</param>
        /// <param name="parameter">Irrelevant</param>
        /// <param name="culture">Irrelevant</param>
        /// <returns><c>-1</c> for <c>"N/A"</c>, parsed <see cref="long"/> for numeric strings, or <see cref="BindingOperations.DoNothing"/> otherwise.</returns>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is string str)
            {
                if (str == "N/A")
                    return -1;

                if (long.TryParse(str, out var number))
                    return number;
            }

            return BindingOperations.DoNothing;
        }
    }
}
