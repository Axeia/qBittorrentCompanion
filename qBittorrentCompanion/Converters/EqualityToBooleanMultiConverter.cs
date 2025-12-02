using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Compares two bound values and returns <c>true</c> if they are equal.
    /// Typically used to bind the <c>IsChecked</c> state of a control to an enum selection
    /// when both the current value and the target value are bound separately.
    /// </summary>
    /// <remarks>
    /// This is the multi-binding equivalent of <see cref="EqualityToBooleanConverter"/>.
    /// Useful in scenarios where the selected enum and the candidate enum are both bound as separate sources.
    /// </remarks>
    public class EqualityToBooleanMultiConverter : IMultiValueConverter
    {
        /// <summary>
        /// Compares two bound values and returns <c>true</c> if they are equal.
        /// </summary>
        /// <param name="values">A list of two values: [current enum value, candidate enum value]</param>
        /// <param name="targetType">Irrelevant</param>
        /// <param name="parameter">Optional parameter (ignored)</param>
        /// <param name="culture">Irrelevant</param>
        /// <returns><c>true</c> if both values are non-null and equal; otherwise <c>false</c>.</returns>
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count != 2 || values[0] is null || values[1] is null)
                return false;

            return values[0]!.Equals(values[1]);
        }

        /// <summary>
        /// Not supported — throws <see cref="NotImplementedException"/>.
        /// </summary>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("EqualityToBooleanMultiConverter does not support ConvertBack.");
        }
    }
}