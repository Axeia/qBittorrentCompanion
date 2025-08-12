using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    public class FixedLengthIntConverter : IValueConverter
    {
        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is int v)
            {
                if (parameter is string whereToPad && whereToPad == "Right")
                {
                    return v > 99
                        ? "99+"
                        : v.ToString().PadRight(3, ' ');
                }
                else
                {
                    return v > 99
                        ? "99+"
                        : v.ToString().PadLeft(3, ' ');
                }
            }

            return string.Empty;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            return BindingOperations.DoNothing;
        }

    }
}
