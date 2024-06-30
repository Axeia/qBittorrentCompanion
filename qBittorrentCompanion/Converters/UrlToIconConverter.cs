using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using qBittorrentCompanion.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Converters
{
    public class UrlToIconConverter : IValueConverter
    {
        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is Uri url)
            {
                string fileUrl = url.ToString();
                if (fileUrl.StartsWith("magnet:") || fileUrl.EndsWith(".torrent"))
                {
                    return new SymbolIcon { Symbol = Symbol.ArrowDownload };
                }
                else if (fileUrl.EndsWith(".html"))
                {
                    return new SymbolIcon { Symbol = Symbol.Open };
                }
                else
                {
                    return new SymbolIcon { Symbol = Symbol.Hourglass };
                }
            }
            return AvaloniaProperty.UnsetValue;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            return BindingOperations.DoNothing;
        }

    }
}
