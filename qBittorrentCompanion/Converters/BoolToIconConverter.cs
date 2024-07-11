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
        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is bool booled)
                if (booled)
                    return FluentIcons.Common.Symbol.Checkmark;
                else
                    return FluentIcons.Common.Symbol.DismissCircle;

            return FluentIcons.Common.Symbol.QuestionCircle;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            return BindingOperations.DoNothing;
        }

    }
}
