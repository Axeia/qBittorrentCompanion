using Avalonia.Data;
using Avalonia.Data.Converters;
using qBittorrentCompanion.Helpers;
using System;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Converts a numeric byte value into a human-readable string using binary units (e.g. "MiB", "GiB").
    /// Intended for use in bindings where the input is a <see cref="long"/> representing a byte count.
    /// </summary>
    /// <remarks>
    /// This converter delegates to <see cref="DataConverter.BytesToHumanReadable(long?, DataConverter.ByteUnit?)"/>.
    /// If the input is not a <see cref="long"/>, it falls back to <see cref="object.ToString"/> or an empty string.
    /// </remarks>
    public class BytesToHumanReadableConverter : IValueConverter
    {
        /// <summary>
        /// Converts a byte count into a formatted string like "1.23 GiB".
        /// </summary>
        /// <param name="value">Expected to be a <see cref="long"/> representing the byte count.</param>
        /// <param name="targetType">The target binding type (typically <see cref="string"/>).</param>
        /// <param name="parameter">Unused.</param>
        /// <param name="culture">Culture info for formatting (currently unused).</param>
        /// <returns>A human-readable string if input is valid; otherwise, a fallback string.</returns>
        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            return value switch
            {
                long bytes => DataConverter.BytesToHumanReadable(bytes),
                _ => value?.ToString() ?? string.Empty,
            };
        }

        /// <summary>
        /// Not implemented. This converter is one-way only.
        /// </summary>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            return BindingOperations.DoNothing;
        }
    }
}
