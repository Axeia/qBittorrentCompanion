using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Converters
{
    public class DataStorageTypeConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is PreferencesWindowViewModel.DataStorageType dataStorageType)
            {
                return dataStorageType switch
                {
                    PreferencesWindowViewModel.DataStorageType.Legacy => DataConverter.DataStorageTypes.Legacy,
                    PreferencesWindowViewModel.DataStorageType.SQLite => DataConverter.DataStorageTypes.SQLite,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            return null;
        }
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is string str)
            {
                if (str == DataConverter.DataStorageTypes.Legacy)
                    return PreferencesWindowViewModel.DataStorageType.Legacy;
                if (str == DataConverter.DataStorageTypes.SQLite)
                    return PreferencesWindowViewModel.DataStorageType.SQLite;

                throw new ArgumentOutOfRangeException();
            }
            return null;
        }
    }
}
