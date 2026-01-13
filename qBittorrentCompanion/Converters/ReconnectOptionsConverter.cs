using Avalonia.Data;
using Avalonia.Data.Converters;
using qBittorrentCompanion.Helpers;
using System;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Converts a <see cref="ViewModels.ReconnectOptions"/> enum value into a user-friendly description string,
    /// and vice versa. Used for UI bindings where upload choking behavior is displayed or selected.
    /// </summary>
    /// <remarks>
    /// Descriptions are centralized in <see cref="DataConverter.ReconnectOptions"/>.
    /// This converter supports both forward and reverse mapping.
    /// </remarks>
    public class ReconnectOptionsConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="ViewModels.ReconnectOptions"/> to its description string.
        /// </summary>
        /// <param name="value">Expected to be a <see cref="DataConverter.ReconnectOptions"/>.</param>
        /// <returns>A description for the reconnect option, or default if input is invalid.</returns>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            return value is ViewModels.ReconnectOptions algorithm
                ? algorithm switch
                {
                    ViewModels.ReconnectOptions.FOREVER => DataConverter.ReconnectOptions.FOREVER,
                    ViewModels.ReconnectOptions.GIVE_UP_AFTER => DataConverter.ReconnectOptions.GIVE_UP_AFTER,
                    _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown SeedChokingAlgorithm")
                }
                : ViewModels.ReconnectOptions.FOREVER;
        }

        /// <summary>
        /// Converts a description string back into a <see cref="ViewModels.ReconnectOptions"/>.
        /// </summary>
        /// <param name="value">Expected to be a description string.</param>
        /// <returns>The corresponding <see cref="ViewModels.ReconnectOptions"/>, or throws if unknown.</returns>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is string str)
            {
                return str switch
                {
                    var s when s == DataConverter.ReconnectOptions.FOREVER => ViewModels.ReconnectOptions.FOREVER,
                    var s when s == DataConverter.ReconnectOptions.GIVE_UP_AFTER => ViewModels.ReconnectOptions.GIVE_UP_AFTER,
                    _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown reconnect option")
                };
            }

            return BindingOperations.DoNothing;
        }
    }
}