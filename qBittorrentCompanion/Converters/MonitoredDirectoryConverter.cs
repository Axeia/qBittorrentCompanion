using Avalonia.Data;
using Avalonia.Data.Converters;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.ViewModels.LocalSettings;
using System;
using System.Globalization;
using static qBittorrentCompanion.Helpers.DataConverter;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Converts a <see cref="MonitoredDirectoryAction"/> enum value into a user-friendly description string,
    /// and vice versa. Used for UI bindings where an import or export action is displayed or selected.
    /// </summary>
    /// <remarks>
    /// Descriptions are centralized in <see cref="DataConverter.MonitoredDirectoryActions.Texts"/>.
    /// This converter supports both forward and reverse mapping.
    /// </remarks>
    public class MonitoredDirectoryConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="ImportAction"/> to its description string.
        /// </summary>
        /// <param name="value">Expected to be a <see cref="ImportAction"/>.</param>
        /// <returns>A description string for the search option.</returns>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            return value is MonitoredDirectoryAction mda && MonitoredDirectoryActions.EnumToText.TryGetValue(mda, out var text)
                ? text
                : "";
        }

        /// <summary>
        /// Converts a description string back into a <see cref="MonitoredDirectoryAction"/>.
        /// </summary>
        /// <param name="value">Expected to be a description string.</param>
        /// <returns>The corresponding <see cref="MonitoredDirectoryAction"/>, or throws if unknown.</returns>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            return value is string str && MonitoredDirectoryActions.TextToEnum.TryGetValue(str, out var mda)
                ? mda
                : BindingOperations.DoNothing;
        }
    }
}
