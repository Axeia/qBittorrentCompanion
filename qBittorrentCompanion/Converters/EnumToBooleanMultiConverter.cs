using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    public class EnumToBooleanMultiConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count != 2 || values[0] == null || values[1] == null)
                return false;

            Debug.WriteLine($"value: {values[0]} == {values[1]}");
            return values[0]!.Equals(values[1]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
