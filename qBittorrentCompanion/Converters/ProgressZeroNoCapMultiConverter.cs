using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Converts a pair of numeric values (progress and cap) into a formatted string.
    /// If progress is zero, returns <c>"∞"</c> to indicate uncapped or indefinite state.
    /// Otherwise returns a formatted string like <c>"5/10"</c>.
    /// </summary>
    /// <remarks>
    /// Useful for rendering progress indicators where zero implies uncapped or infinite capacity.
    /// </remarks>
    public class ProgressZeroNoCapMultiConverter : IMultiValueConverter
    {
        /// <summary>
        /// Converts two values — progress and cap — into a formatted string.
        /// </summary>
        /// <param name="values">Expected to contain two values: [progress, cap]</param>
        /// <param name="targetType">Irrelevant</param>
        /// <param name="parameter">Irrelevant</param>
        /// <param name="culture">Irrelevant</param>
        /// <returns><c>"∞"</c> if progress is zero; otherwise <c>"progress/cap"</c>.</returns>
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values == null || values.Count != 2)
                return "?";

            long progress = System.Convert.ToInt64(values[0]);
            decimal cap = System.Convert.ToDecimal(values[1]);

            return progress == 0 ? "∞" : $"{progress}/{cap}";
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
