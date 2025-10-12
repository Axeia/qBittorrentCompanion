using Avalonia.Data;
using Avalonia.Data.Converters;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.ViewModels.LocalSettings;
using System;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Converts a <see cref="ImportAction"/> enum value into a user-friendly description string,
    /// and vice versa. Used for UI bindings where an import or export action is displayed or selected.
    /// </summary>
    /// <remarks>
    /// Descriptions are centralized in <see cref="DataConverter.ExportActions"/>.
    /// This converter supports both forward and reverse mapping.
    /// </remarks>
    public class ExportActionConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="ImportAction"/> to its description string.
        /// </summary>
        /// <param name="value">Expected to be a <see cref="ImportAction"/>.</param>
        /// <returns>A description string for the search option.</returns>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            return value is ExportAction option
                ? option switch
                {
                    ExportAction.JSON_DARK_LIGHT => DataConverter.ExportActions.JSON_DARK_LIGHT,
                    ExportAction.JSON_DARK => DataConverter.ExportActions.JSON_DARK,
                    ExportAction.JSON_LIGHT => DataConverter.ExportActions.JSON_LIGHT,
                    ExportAction.SVG_DARK_LIGHT => DataConverter.ExportActions.SVG_DARK_LIGHT,
                    ExportAction.SVG_DARK => DataConverter.ExportActions.SVG_DARK,
                    ExportAction.SVG_LIGHT => DataConverter.ExportActions.SVG_LIGHT,
                    _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown SearchInOption")
                }
                : DataConverter.ExportActions.JSON_DARK_LIGHT;
        }

        /// <summary>
        /// Converts a description string back into a <see cref="ImportAction"/>.
        /// </summary>
        /// <param name="value">Expected to be a description string.</param>
        /// <returns>The corresponding <see cref="ImportAction"/>, or throws if unknown.</returns>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is string str)
            {
                return str switch
                {
                    var s when s == DataConverter.ExportActions.JSON_DARK_LIGHT => ExportAction.JSON_DARK_LIGHT,
                    var s when s == DataConverter.ExportActions.JSON_DARK => ExportAction.JSON_DARK,
                    var s when s == DataConverter.ExportActions.JSON_LIGHT => ExportAction.JSON_LIGHT,
                    var s when s == DataConverter.ExportActions.SVG_DARK_LIGHT => ExportAction.SVG_DARK_LIGHT,
                    var s when s == DataConverter.ExportActions.SVG_DARK => ExportAction.SVG_DARK,
                    var s when s == DataConverter.ExportActions.SVG_LIGHT => ExportAction.SVG_LIGHT,
                    _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown export action value")
                };
            }

            return BindingOperations.DoNothing;
        }
    }
}
