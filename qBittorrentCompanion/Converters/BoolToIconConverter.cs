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
    public class BoolToIconConverter : IValueConverter
    {
        public static Geometry trueIcon = Geometry.Parse("");
        public static Geometry falseIcon = Geometry.Parse("");
        public static Geometry unclearIcon = Geometry.Parse("");

        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is bool booled)
                if (booled)
                    return trueIcon;
                else
                    return falseIcon;

            return unclearIcon;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            return BindingOperations.DoNothing;
        }

    }
}
