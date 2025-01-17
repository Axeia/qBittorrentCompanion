using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    public class DaysAgoConverter : IValueConverter
    {
        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is DateTimeOffset dateTimeOffset)
            {
                var timespan = DateTimeOffset.Now - dateTimeOffset;
                return $"{timespan.TotalDays:0} days";
            }

            return value ?? "no match";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            return BindingOperations.DoNothing;
        }

    }
}
