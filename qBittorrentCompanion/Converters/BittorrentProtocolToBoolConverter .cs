using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Converters
{
    public class BittorrentProtocolToBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is BittorrentProtocol protocol && parameter is string radioButtonName)
            {
                return protocol.ToString() == radioButtonName;
            }
            return false;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is bool isChecked && isChecked && parameter is string radioButtonName)
            {
                return Enum.Parse(typeof(BittorrentProtocol), radioButtonName);
            }
            return null;
        }

    }
}
