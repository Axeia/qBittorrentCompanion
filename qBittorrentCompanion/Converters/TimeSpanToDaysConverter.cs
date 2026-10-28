using Avalonia.Data;
using Avalonia.Data.Converters;
using qBittorrentCompanion.Helpers;
using System;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Converts a <see cref="TimeSpan"/> to a formatted days string using <see cref="DataConverter.TimeSpanToDays"/>.
    /// </summary>
    public class TimeSpanToDaysConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="TimeSpan"/> into a trimmed string representation of days.
        /// </summary>
        /// <param name="value">Expected to be a <see cref="TimeSpan"/>.</param>
        /// <param name="targetType">Irrelevant</param>
        /// <param name="parameter">Optional boolean parameter to pass to TimeSpanToDays (defaults to true if omitted).</param>
        /// <param name="culture">Irrelevant</param>
        /// <returns>A formatted and trimmed string, or <c>null</c> if input is invalid.</returns>
        public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is TimeSpan timeSpan)
            {
                bool flag = parameter is bool b ? b : true;
                return DataConverter.TimeSpanToDays(timeSpan, flag).Trim(' ');
            }

            return null;
        }

        /// <summary>
        /// ConvertBack is not supported for this display-only conversion.
        /// </summary>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            return BindingOperations.DoNothing;
        }
    }
}