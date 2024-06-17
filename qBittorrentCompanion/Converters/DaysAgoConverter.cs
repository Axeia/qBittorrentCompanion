using Avalonia.Data;
using Avalonia.Data.Converters;
using qBittorrentCompanion.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            return value ?? "";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            return BindingOperations.DoNothing;
        }

    }
}
