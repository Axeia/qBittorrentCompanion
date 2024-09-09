using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Converters
{
    public class MinutesToTimeSpanConverter : IValueConverter
    {
        public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is TimeSpan timeSpan)
            {
                return timeSpan.TotalMinutes;
            }

            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is int || value is decimal || value is long || value is double)
            {
                var timeInMinutes = System.Convert.ToInt64(value);
                return TimeSpan.FromMinutes(timeInMinutes);
            }
            else
            {
                return BindingOperations.DoNothing;
            }
        }
    }
}
