using Avalonia.Data;
using Avalonia.Data.Converters;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.ViewModels;
using System;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    public class ReplaceOptionToStringConverter : IValueConverter
    {
        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is RenameOption replaceOption)
            {
                return replaceOption switch
                {
                    RenameOption.Replace => DataConverter.ReplaceOptionDescriptions.Replace,
                    RenameOption.ReplaceOneByOne => DataConverter.ReplaceOptionDescriptions.ReplaceOneByOne,
                    RenameOption.ReplaceAll => DataConverter.ReplaceOptionDescriptions.ReplaceAll,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            return BindingOperations.DoNothing;
        }

    }
}
