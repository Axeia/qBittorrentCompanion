using Avalonia.Data.Converters;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.ViewModels;
using System;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Converts between internal <see cref="PreferencesWindowViewModel.DataStorageType"/> enum values
    /// and user-facing string labels defined in <see cref="DataConverter.DataStorageTypes"/>.
    /// Used for UI binding and display in the preferences panel.
    /// </summary>
    public class DataStorageTypeConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="PreferencesWindowViewModel.DataStorageType"/> enum value
        /// into a descriptive string label for display.
        /// </summary>
        /// <param name="value">The enum value to convert (expected to be of type <see cref="PreferencesWindowViewModel.DataStorageType"/>)</param>
        /// <param name="targetType">The target binding type (ignored)</param>
        /// <param name="parameter">Optional parameter (ignored)</param>
        /// <param name="culture">Culture info (ignored)</param>
        /// <returns>
        /// A string label from <see cref="DataConverter.DataStorageTypes"/> matching the enum value,
        /// or <c>SQLite</c> label as fallback.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the enum value is not recognized.
        /// </exception>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is PreferencesWindowViewModel.DataStorageType dataStorageType)
            {
                return dataStorageType switch
                {
                    PreferencesWindowViewModel.DataStorageType.Legacy => DataConverter.DataStorageTypes.Legacy,
                    PreferencesWindowViewModel.DataStorageType.SQLite => DataConverter.DataStorageTypes.SQLite,
                    _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unsupported DataStorageType enum value.")
                };
            }

            // Fallback to SQLite label if value is null or of unexpected type
            return DataConverter.DataStorageTypes.SQLite;
        }

        /// <summary>
        /// Converts a string label back into its corresponding <see cref="PreferencesWindowViewModel.DataStorageType"/> enum value.
        /// </summary>
        /// <param name="value">The string label to convert (expected to match one of the values in <see cref="DataConverter.DataStorageTypes"/>)</param>
        /// <param name="targetType">The target binding type (ignored)</param>
        /// <param name="parameter">Optional parameter (ignored)</param>
        /// <param name="culture">Culture info (ignored)</param>
        /// <returns>
        /// A matching <see cref="PreferencesWindowViewModel.DataStorageType"/> enum value,
        /// or <c>SQLite</c> enum as fallback.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the string does not match any known label.
        /// </exception>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is string str)
            {
                if (str == DataConverter.DataStorageTypes.Legacy)
                    return PreferencesWindowViewModel.DataStorageType.Legacy;
                if (str == DataConverter.DataStorageTypes.SQLite)
                    return PreferencesWindowViewModel.DataStorageType.SQLite;

                throw new ArgumentOutOfRangeException(nameof(value), value, "Unsupported data storage type string.");
            }

            // Fallback to SQLite enum if value is null or of unexpected type
            return PreferencesWindowViewModel.DataStorageType.SQLite;
        }
    }
}
