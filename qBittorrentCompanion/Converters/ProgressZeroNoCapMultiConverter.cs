using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    public class ProgressZeroNoCapMultiConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values == null || values.Count != 2)
                return "?";

            long val0 = System.Convert.ToInt64(values[0]);
            decimal val1 = System.Convert.ToDecimal(values[1]);

            return val0 == 0 ? "∞" : $"{val0}/{val1}";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
