using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    public class CountryCodeToFlagConverter : IValueConverter
    {
        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            string countryCode = "blank";
            if (value is string str && str != "")
            {
                countryCode = str;
            }

            var uri = new Uri($"avares://qBittorrentCompanion/Assets/Flags/{countryCode}.png");
            var bitmap = new Bitmap(AssetLoader.Open(uri));

            return bitmap;
        }

        public object ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            return BindingOperations.DoNothing;
        }
    }
}
