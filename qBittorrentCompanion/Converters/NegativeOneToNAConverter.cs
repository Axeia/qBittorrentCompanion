using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Converters
{
    public class NegativeOneToNAConverter : IValueConverter
    {
        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is long && (long)value == -1)
                return "N/A";
            else
                return value?.ToString() ?? "";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is string stringValue && stringValue == "N/A")
            {
                return -1;
            }
            else if (value is string numberString && long.TryParse(numberString, out long numberValue))
            {
                return numberValue;
            }
            else
            {
                return BindingOperations.DoNothing;
            }
        }

    }
}
