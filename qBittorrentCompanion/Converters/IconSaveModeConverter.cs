using Avalonia.Data;
using Avalonia.Data.Converters;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.ViewModels;
using qBittorrentCompanion.ViewModels.LocalSettings;
using System;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Converts a <see cref="IconSaveMode"/> enum value into a user-friendly description string,
    /// and vice versa. Used for UI bindings where icon save mode is displayed or selected.
    /// </summary>
    /// <remarks>
    /// Descriptions are centralized in <see cref="DataConverter.SearchInOptionDescriptions"/>.
    /// This converter supports both forward and reverse mapping.
    /// </remarks>
    public class IconSaveModeConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="IconSaveMode"/> to its description string.
        /// </summary>
        /// <param name="value">Expected to be a <see cref="IconSaveMode"/>.</param>
        /// <returns>A description string for the search option.</returns>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            return value is IconSaveMode option
                ? option switch
                {
                    IconSaveMode.DarkAndLight => DataConverter.IconSaveModes.DarkAndLight,
                    IconSaveMode.Dark => DataConverter.IconSaveModes.Dark,
                    IconSaveMode.Light => DataConverter.IconSaveModes.Light,
                    _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown SearchInOption")
                }
                : DataConverter.IconSaveModes.DarkAndLight;
        }

        /// <summary>
        /// Converts a description string back into a <see cref="IconSaveMode"/>.
        /// </summary>
        /// <param name="value">Expected to be a description string.</param>
        /// <returns>The corresponding <see cref="IconSaveMode"/>, or throws if unknown.</returns>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is string str)
            {
                return str switch
                {
                    var s when s == DataConverter.IconSaveModes.DarkAndLight => IconSaveMode.DarkAndLight,
                    var s when s == DataConverter.IconSaveModes.Dark => IconSaveMode.Dark,
                    var s when s == DataConverter.IconSaveModes.Light => IconSaveMode.Light,
                    _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown icon save mode")
                };
            }

            return BindingOperations.DoNothing;
        }
    }
}
