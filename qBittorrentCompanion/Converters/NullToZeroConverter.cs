using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Converts <c>null</c> to <c>0</c> for numeric bindings.
    /// Used to ensure UI controls like <c>ProgressBar</c> receive a valid numeric value
    /// even when the bound source is <c>null</c>.
    /// </summary>
    /// <remarks>
    /// This is especially useful when binding to nullable properties like <c>double?</c>,
    /// where <c>FallbackValue</c> would not apply because the binding itself succeeds.
    /// </remarks>
    public class NullToZeroConverter : IValueConverter
    {
        /// <summary>
        /// Converts <c>null</c> to <c>0</c>; otherwise returns the original value.
        /// </summary>
        /// <param name="value">The bound value, possibly <c>null</c>.</param>
        /// <param name="targetType">Irrelevant</param>
        /// <param name="parameter">Irrelevant</param>
        /// <param name="culture">Irrelevant</param>
        /// <returns><c>0</c> if <paramref name="value"/> is <c>null</c>; otherwise <paramref name="value"/>.</returns>
        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            return value ?? 0;
        }

        /// <summary>
        /// Not supported — returns <see cref="BindingOperations.DoNothing"/>.
        /// </summary>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            return BindingOperations.DoNothing;
        }
    }
}
