using Avalonia.Data;
using Avalonia.Data.Converters;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.ViewModels;
using System;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Converts a <see cref="RenameOption"/> enum value into a user-friendly description string.
    /// Used for UI bindings where rename behavior is displayed or selected.
    /// </summary>
    /// <remarks>
    /// Descriptions are centralized in <see cref="DataConverter.ReplaceOptionDescriptions"/>.
    /// This converter supports one-way conversion only.
    /// </remarks>
    public class ReplaceOptionToStringConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="RenameOption"/> to its description string.
        /// </summary>
        /// <param name="value">Expected to be a <see cref="RenameOption"/>.</param>
        /// <param name="targetType">Irrelevant</param>
        /// <param name="parameter">Irrelevant</param>
        /// <param name="culture">Irrelevant</param>
        /// <returns>A description string for the rename option.</returns>
        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            return value is RenameOption option
                ? option switch
                {
                    RenameOption.Replace => DataConverter.ReplaceOptionDescriptions.Replace,
                    RenameOption.ReplaceOneByOne => DataConverter.ReplaceOptionDescriptions.ReplaceOneByOne,
                    RenameOption.ReplaceAll => DataConverter.ReplaceOptionDescriptions.ReplaceAll,
                    _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown RenameOption")
                }
                : DataConverter.ReplaceOptionDescriptions.Replace;
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
