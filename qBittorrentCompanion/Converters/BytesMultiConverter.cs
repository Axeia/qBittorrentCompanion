using Avalonia.Data;
using Avalonia.Data.Converters;
using qBittorrentCompanion.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using static qBittorrentCompanion.Helpers.DataConverter;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Converts a numeric byte value and <see cref="DataConverter.ByteUnit"/> into a scaled double value.
    /// </summary>
    public class BytesMultiConverter : IMultiValueConverter
    {
        /// <summary>
        /// Converts a numeric value and a unit string into a scaled double value.
        /// Expects <paramref name="values"/> to contain:
        /// <list type="bullet">
        /// <item><description>Index 0: an <see cref="int"/>, <see cref="long"/>, or <see cref="Int64"/> representing the byte count</description></item>
        /// <item><description>Index 1: a <see cref="string"/> or <see cref="ByteUnit"/> representing the unit (e.g. "MiB", "GiB"). Must match a value in <see cref="ByteUnit"/>.</description></item>
        /// </list>
        /// Returns the byte count divided by the multiplier for the unit, as a <see cref="double"/>.
        /// </summary>
        /// <param name="values">[0] => numeric (int, long, Int64). [1] => <see cref="DataConverter.ByteUnit"/></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            long? valToConvert = null;

            if (values is null)
                return null;

            if (values[0] is IConvertible convertible)
                valToConvert = convertible.ToInt64(culture);

            if (valToConvert != null && values[1] is ByteUnit sizeUnit)
            {
                return System.Convert.ToDouble(valToConvert / DataConverter.Multipliers[sizeUnit]);
            }

            return BindingOperations.DoNothing;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
