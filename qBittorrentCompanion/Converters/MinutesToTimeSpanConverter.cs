using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Converts between a <see cref="TimeSpan"/> and its total minutes as a numeric value.
    /// Used for UI bindings where durations are edited or displayed in minutes.
    /// </summary>
    /// <remarks>
    /// Supports <c>int</c>, <c>long</c>, <c>double</c>, and <c>decimal</c> inputs for conversion.
    /// Returns <see cref="BindingOperations.DoNothing"/> for unsupported types.
    /// </remarks>
    public class MinutesToTimeSpanConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="TimeSpan"/> to its total minutes as a <c>double</c>.
        /// </summary>
        /// <param name="value">Expected to be a <see cref="TimeSpan"/>.</param>
        /// <param name="targetType">Irrelevant</param>
        /// <param name="parameter">Irrelevant</param>
        /// <param name="culture">Irrelevant</param>
        /// <returns>Total minutes as <c>double</c>, or <c>null</c> if input is invalid.</returns>
        public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            return value is TimeSpan timeSpan
                ? timeSpan.TotalMinutes
                : null;
        }

        /// <summary>
        /// Converts a numeric value (minutes) into a <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="value">Expected to be a numeric type: <c>int</c>, <c>long</c>, <c>double</c>, or <c>decimal</c>.</param>
        /// <param name="targetType">Irrelevant</param>
        /// <param name="parameter">Irrelevant</param>
        /// <param name="culture">Irrelevant</param>
        /// <returns>A <see cref="TimeSpan"/> representing the given number of minutes, or <see cref="BindingOperations.DoNothing"/> if input is invalid.</returns>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            return value is int or long or double or decimal
                ? TimeSpan.FromMinutes(System.Convert.ToDouble(value))
                : BindingOperations.DoNothing;
        }
    }
}
