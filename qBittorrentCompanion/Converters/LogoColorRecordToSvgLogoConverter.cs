using Avalonia.Data;
using Avalonia.Data.Converters;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Models;
using System;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    public class LogoColorRecordToSvgLogoConverter : IValueConverter
    {
        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is LogoColorsRecord lcr)
                return LogoHelper.GetLogoAsXDocument(lcr).ToString();

            return string.Empty;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            return BindingOperations.DoNothing;
        }

    }
}
