using Avalonia.Data;
using Avalonia.Data.Converters;
using QBittorrent.Client;
using System;
using System.Globalization;
using static qBittorrentCompanion.Helpers.DataConverter;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Converts a <see cref="SeedChokingAlgorithm"/> enum value into a user-friendly description string,
    /// and vice versa. Used for UI bindings where upload choking behavior is displayed or selected.
    /// </summary>
    /// <remarks>
    /// Descriptions are centralized in <see cref="UploadChokingAlgorithms"/>.
    /// This converter supports both forward and reverse mapping.
    /// </remarks>
    public class UploadChokingAlgorithmConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="SeedChokingAlgorithm"/> to its description string.
        /// </summary>
        /// <param name="value">Expected to be a <see cref="SeedChokingAlgorithm"/>.</param>
        /// <returns>A description string for the algorithm, or default if input is invalid.</returns>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            return value is SeedChokingAlgorithm algorithm
                ? algorithm switch
                {
                    SeedChokingAlgorithm.RoundRobin => UploadChokingAlgorithms.RoundRobin,
                    SeedChokingAlgorithm.FastestUpload => UploadChokingAlgorithms.FastestUpload,
                    SeedChokingAlgorithm.AntiLeech => UploadChokingAlgorithms.AntiLeech,
                    _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown SeedChokingAlgorithm")
                }
                : UploadChokingAlgorithms.RoundRobin;
        }

        /// <summary>
        /// Converts a description string back into a <see cref="SeedChokingAlgorithm"/>.
        /// </summary>
        /// <param name="value">Expected to be a description string.</param>
        /// <returns>The corresponding <see cref="SeedChokingAlgorithm"/>, or throws if unknown.</returns>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is string str)
            {
                return str switch
                {
                    var s when s == UploadChokingAlgorithms.RoundRobin => SeedChokingAlgorithm.RoundRobin,
                    var s when s == UploadChokingAlgorithms.FastestUpload => SeedChokingAlgorithm.FastestUpload,
                    var s when s == UploadChokingAlgorithms.AntiLeech => SeedChokingAlgorithm.AntiLeech,
                    _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown upload choking description")
                };
            }

            return BindingOperations.DoNothing;
        }
    }
}