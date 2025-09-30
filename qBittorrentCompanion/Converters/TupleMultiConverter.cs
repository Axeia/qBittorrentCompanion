using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Combines two bound values into a tuple of strings.
    /// Useful for composite bindings where paired inputs need to be grouped.
    /// </summary>
    /// <remarks>
    /// This converter assumes exactly two values and returns a <c>(string, string)</c> tuple.
    /// </remarks>
    internal class TupleMultiConverter : IMultiValueConverter
    {
        /// <summary>
        /// Converts two values into a tuple of their string representations.
        /// </summary>
        /// <param name="values">Expected to contain two values.</param>
        /// <returns>A tuple of strings.</returns>
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            return (values[0]?.ToString(), values[1]?.ToString());
        }

        /// <summary>
        /// Not supported — throws <see cref="NotImplementedException"/>.
        /// </summary>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
