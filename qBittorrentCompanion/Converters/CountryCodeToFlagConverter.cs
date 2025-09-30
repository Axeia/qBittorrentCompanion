using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using qBittorrentCompanion.Services;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Converts a short ISO 3166-1 alpha-2 country code string into a flag <see cref="Bitmap"/>.
    /// Assumes the flag image is located at "Assets/Flags/{code}.png" within the application resources.
    /// Falls back to "blank.png" if input is null or empty.
    /// </summary>
    public class CountryCodeToFlagConverter : IValueConverter
    {
        /// <summary>
        /// Results cache
        /// </summary>
        private static readonly Dictionary<string, Bitmap> _cache = [];

        /// <summary>
        /// The value is expected to be a 2 character ISO 3166-1 country code. 
        /// If a matching flag can be found it's returned, 
        /// if not a placeholder flag with questionmarks is returned as a fallback value
        /// </summary>
        /// <param name="value">2 character country code as per the ISO-3166-1 standard, <br/> 
        /// For example: <c>us</c>, <c>gb</c> and <c>nl</c><br/><br/><b>Note:</b> Not case sensistive </param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns><see cref="Bitmap"/> representing the country flag</returns>
        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            string countryCode = "blank";

            if (value is string str && !string.IsNullOrWhiteSpace(str) && str.Length == 2)
                countryCode = str.ToLowerInvariant();

            // Try cache and return it from there if possible
            if (_cache.TryGetValue(countryCode, out var cached))
                return cached;

            string avaResFlagAddress = $"avares://qBittorrentCompanion/Assets/Flags/{countryCode}.png";

            // else, try to create and add to cache 
            try
            {
                var uri = new Uri(avaResFlagAddress);
                Bitmap bitmap = new(AssetLoader.Open(uri));
                _cache[countryCode] = bitmap;

                return bitmap;
            }
            catch
            {
                AppLoggerService.AddLogMessage(
                    Splat.LogLevel.Warn,
                    GetFullTypeName<CountryCodeToFlagConverter>(),
                    $"Unable to locate {countryCode}",
                    $"Attempted to load resource: {avaResFlagAddress}"

                );
                var fallbackUri = new Uri("avares://qBittorrentCompanion/Assets/Flags/blank.png");
                Bitmap fallBackBitmap = new(AssetLoader.Open(fallbackUri));

                return fallBackBitmap;
            }
        }

        public object ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            return BindingOperations.DoNothing;
        }
    }
}
