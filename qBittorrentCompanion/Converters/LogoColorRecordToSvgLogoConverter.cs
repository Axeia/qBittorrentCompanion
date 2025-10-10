using Avalonia.Data;
using Avalonia.Data.Converters;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Models;
using System;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Converts a <see cref="LogoDataRecord"/> into an SVG string representation.
    /// Used to render themed logos in the UI based on user-selected or system-defined colors.
    /// </summary>
    /// <remarks>
    /// Delegates SVG generation to <see cref="LogoHelper.GetLogoAsXDocument(LogoDataRecord)"/>.
    /// Returns an empty string if input is null or of unexpected type.
    /// </remarks>
    public class LogoColorRecordToSvgLogoConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="LogoDataRecord"/> into an SVG string.<br/>
        /// <seealso cref="LogoHelper.GetLogoAsXDocument(LogoDataRecord)"/>
        /// </summary>
        /// <param name="value">Expected to be a <see cref="LogoDataRecord"/>.</param>
        /// <param name="targetType">Irrelevant</param>
        /// <param name="parameter">Irrelevant</param>
        /// <param name="culture">Irrelevant</param>
        /// <returns>An SVG string representing the logo, or an empty string if input is invalid.</returns>
        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            return value is LogoDataRecord lcr
                ? LogoHelper.GetLogoAsXDocument(lcr).ToString()
                : string.Empty;
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
