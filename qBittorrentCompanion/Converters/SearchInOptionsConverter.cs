using Avalonia.Data;
using Avalonia.Data.Converters;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.ViewModels;
using System;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Converts a <see cref="SearchInOption"/> enum value into a user-friendly description string,
    /// and vice versa. Used for UI bindings where search scope is displayed or selected.
    /// </summary>
    /// <remarks>
    /// Descriptions are centralized in <see cref="DataConverter.SearchInOptionDescriptions"/>.
    /// This converter supports both forward and reverse mapping.
    /// </remarks>
    public class SearchInOptionsConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="SearchInOption"/> to its description string.
        /// </summary>
        /// <param name="value">Expected to be a <see cref="SearchInOption"/>.</param>
        /// <returns>A description string for the search option.</returns>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            return value is SearchInOption option
                ? option switch
                {
                    SearchInOption.NamePlusExtension => DataConverter.SearchInOptionDescriptions.NamePlusExtension,
                    SearchInOption.Name => DataConverter.SearchInOptionDescriptions.Name,
                    SearchInOption.Extension => DataConverter.SearchInOptionDescriptions.Extension,
                    _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown SearchInOption")
                }
                : DataConverter.SearchInOptionDescriptions.NamePlusExtension;
        }

        /// <summary>
        /// Converts a description string back into a <see cref="SearchInOption"/>.
        /// </summary>
        /// <param name="value">Expected to be a description string.</param>
        /// <returns>The corresponding <see cref="SearchInOption"/>, or throws if unknown.</returns>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is string str)
            {
                return str switch
                {
                    var s when s == DataConverter.SearchInOptionDescriptions.NamePlusExtension => SearchInOption.NamePlusExtension,
                    var s when s == DataConverter.SearchInOptionDescriptions.Name => SearchInOption.Name,
                    var s when s == DataConverter.SearchInOptionDescriptions.Extension => SearchInOption.Extension,
                    _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown search option description")
                };
            }

            return BindingOperations.DoNothing;
        }
    }
}
