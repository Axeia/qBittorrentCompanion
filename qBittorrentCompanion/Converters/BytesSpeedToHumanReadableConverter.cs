using Avalonia.Data;
using Avalonia.Data.Converters;
using qBittorrentCompanion.Helpers;
using System;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Converts a numeric byte-per-second value into a human-readable string with binary units (e.g. "1.23 MiB/s").
    /// Intended for use in bindings where the input is a <see cref="long"/> representing a speed in bytes per second.
    /// </summary>
    /// <remarks>
    /// This converter delegates to <see cref="DataConverter.BytesToHumanReadable(long?, DataConverter.ByteUnit?)"/> 
    /// and appends "/s" to the result. See <see cref="BytesToHumanReadableConverter"/> for a version without the "/s" suffix.
    /// </remarks>
    public class BytesSpeedToHumanReadableConverter : IValueConverter
    {
        /// <summary>
        /// Converts a byte-per-second value into a formatted string like "1.23 MiB/s".
        /// </summary>
        /// <param name="value">Expected to be a <see cref="long"/> representing bytes per second.</param>
        /// <param name="targetType">The target binding type (typically <see cref="string"/>).</param>
        /// <param name="parameter">Unused.</param>
        /// <param name="culture">Culture info for formatting (currently unused).</param>
        /// <returns>A human-readable string if input is valid; otherwise, a fallback string.</returns>
        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            return value switch
            {
                long bytesPerSecond => DataConverter.BytesToHumanReadable(bytesPerSecond) + "/s",
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