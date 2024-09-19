using Avalonia.Data.Converters;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.ViewModels;
using System;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    public class SearchInOptionsConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is SearchInOption searchInOption)
            {
                return searchInOption switch
                {
                    SearchInOption.NamePlusExtension => DataConverter.SearchInOptionDescriptions.NamePlusExtension,
                    SearchInOption.Name => DataConverter.SearchInOptionDescriptions.Name,
                    SearchInOption.Extension => DataConverter.SearchInOptionDescriptions.Extension,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            return null;
        }
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is string str)
            {
                if (str == DataConverter.SearchInOptionDescriptions.NamePlusExtension)
                    return SearchInOption.NamePlusExtension;
                if (str == DataConverter.SearchInOptionDescriptions.Name)
                    return SearchInOption.Name;
                if (str == DataConverter.SearchInOptionDescriptions.Extension)
                    return SearchInOption.Extension;
                throw new ArgumentOutOfRangeException();
            }
            return null;
        }
    }
}
