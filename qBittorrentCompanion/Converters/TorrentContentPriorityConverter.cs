using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using DynamicData.Aggregation;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static qBittorrentCompanion.Helpers.DataConverter;

namespace qBittorrentCompanion.Converters
{
    public class TorrentContentPriorityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is TorrentContentPriority torrentContentPriority)
            {
                return torrentContentPriority switch
                {
                    TorrentContentPriority.Skip => TorrentContentPriorities.Skip,
                    TorrentContentPriority.Minimal => TorrentContentPriorities.Minimal,
                    TorrentContentPriority.VeryLow => TorrentContentPriorities.VeryLow,
                    TorrentContentPriority.Low => TorrentContentPriorities.Low,
                    TorrentContentPriority.Normal => TorrentContentPriorities.Normal,
                    TorrentContentPriority.High => TorrentContentPriorities.High,
                    TorrentContentPriority.VeryHigh => TorrentContentPriorities.VeryHigh,
                    TorrentContentPriority.Maximal => TorrentContentPriorities.Maximal,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is string str)
            {
                if (str == TorrentContentPriorities.Skip)
                    return TorrentContentPriority.Skip;
                if (str == TorrentContentPriorities.Minimal)
                    return TorrentContentPriority.Minimal;
                if (str == TorrentContentPriorities.VeryLow)
                    return TorrentContentPriority.VeryLow;
                if (str == TorrentContentPriorities.Low)
                    return TorrentContentPriority.Low;
                if (str == TorrentContentPriorities.Normal)
                    return TorrentContentPriority.Normal;
                if (str == TorrentContentPriorities.High)
                    return TorrentContentPriority.High;
                if (str == TorrentContentPriorities.VeryHigh)
                    return TorrentContentPriority.VeryHigh;
                if (str == TorrentContentPriorities.Maximal)
                    return TorrentContentPriority.Maximal;
                if (str == TorrentContentPriorities.Mixed)
                    return null;

                throw new ArgumentOutOfRangeException();
            }
            return null;
        }
    }
}
