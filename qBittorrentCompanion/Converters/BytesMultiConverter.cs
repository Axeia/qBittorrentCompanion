using Avalonia.Data.Converters;
using qBittorrentCompanion.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    public class BytesMultiConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            long? valToConvert = null;

            if (values is null)
                return null;

            if (values[0] is int intVal)
                valToConvert = intVal;
            if (values[0] is long longVal)
                valToConvert = longVal;
            if (values[0] is Int64 int64Val)
                valToConvert = int64Val;

            if (valToConvert != null && values[1] is string sizeUnit)
            {
                //Debug.WriteLine($"(using: {sizeUnit}) {valToConvert} / {DataConverter.GetMultiplierForUnit(sizeUnit)} = {valToConvert / DataConverter.GetMultiplierForUnit(sizeUnit)}");
                return System.Convert.ToDecimal(valToConvert / DataConverter.GetMultiplierForUnit(sizeUnit));
            }
            /*else
            {
                Debug.WriteLine($"BytesConverter does not deal with values of type {(values[0] == null ? "null" : values[0].GetType())} or parameters of type {(parameter == null ? "null" : parameter.GetType())}");
            }*/

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
