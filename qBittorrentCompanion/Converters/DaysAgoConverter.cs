using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Converts a <see cref="DateTimeOffset"/> into a human-readable string like "3 days ago".
    /// Used for UI display of timestamps relative to the current time.
    /// </summary>
    public class DaysAgoConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="DateTimeOffset"/> to a string like "3 days ago", "Today", or "Yesterday".
        /// </summary>
        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is not DateTimeOffset dateTimeOffset)
                return "Unknown";

            var now = DateTimeOffset.Now;
            var days = (int)Math.Floor((now - dateTimeOffset).TotalDays);

            return days switch
            {
                < 0 => "In the future",
                0 => "Today",
                1 => "Yesterday",
                _ => $"{days} days"
            };
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
