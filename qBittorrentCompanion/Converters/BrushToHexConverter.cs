using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using qBittorrentCompanion.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Converters
{
    public class BrushToHexConverter : IValueConverter
    {
        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is ISolidColorBrush solidColorBrush)
            {
                var color = solidColorBrush.Color;
                return $"{parameter} \n #{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
            }
            return "#??????";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            return BindingOperations.DoNothing;
        }

    }
}
