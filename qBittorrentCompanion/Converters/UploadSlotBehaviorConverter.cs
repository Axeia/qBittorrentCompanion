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
using static qBittorrentCompanion.Helpers.DataConverter;

namespace qBittorrentCompanion.Converters
{
    public class UploadSlotBehaviorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is ChokingAlgorithm chokingAlgorithm)
            {
                return chokingAlgorithm switch
                {
                    ChokingAlgorithm.RateBased => DataConverter.UploadSlotBehaviors.UploadRateBased,
                    ChokingAlgorithm.FixedSlots => DataConverter.UploadSlotBehaviors.FixedSlots,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            return null;
        }
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is string str)
            {
                if (str == DataConverter.UploadSlotBehaviors.UploadRateBased)
                    return ChokingAlgorithm.RateBased;
                if (str == DataConverter.UploadSlotBehaviors.FixedSlots)
                    return ChokingAlgorithm.FixedSlots;

                throw new ArgumentOutOfRangeException();
            }
            return null;
        }
    }
}
