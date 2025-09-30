using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using System;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Converts a <see cref="Uri"/> into a <see cref="SymbolIcon"/> based on its scheme or file extension.
    /// Used to visually represent download types or link behaviors in the UI.
    /// </summary>
    /// <remarks>
    /// - Magnet links and .torrent files map to <c>ArrowDownload</c>
    /// - .html files map to <c>Open</c>
    /// - All other URIs map to <c>Hourglass</c> as a generic fallback
    /// </remarks>
    public class UrlToIconConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="Uri"/> into a corresponding <see cref="SymbolIcon"/>.
        /// </summary>
        /// <param name="value">Expected to be a <see cref="Uri"/>.</param>
        /// <returns>A <see cref="SymbolIcon"/> based on the URI type, or <see cref="AvaloniaProperty.UnsetValue"/> if input is invalid.</returns>
        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is Uri url)
            {
                string fileUrl = url.ToString();

                if (fileUrl.StartsWith("magnet:", StringComparison.OrdinalIgnoreCase) ||
                    fileUrl.EndsWith(".torrent", StringComparison.OrdinalIgnoreCase))
                {
                    return new SymbolIcon { Symbol = Symbol.ArrowDownload };
                }

                if (fileUrl.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                {
                    return new SymbolIcon { Symbol = Symbol.Open };
                }

                return new SymbolIcon { Symbol = Symbol.Hourglass };
            }

            return AvaloniaProperty.UnsetValue;
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
