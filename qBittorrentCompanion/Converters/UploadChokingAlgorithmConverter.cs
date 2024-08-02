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
    public class UploadChokingAlgorithmConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is SeedChokingAlgorithm seedChokingAlgorithm)
            {
                return seedChokingAlgorithm switch
                {
                    SeedChokingAlgorithm.RoundRobin => DataConverter.UploadChokingAlgorithms.RoundRobin,
                    SeedChokingAlgorithm.FastestUpload => DataConverter.UploadChokingAlgorithms.FastestUpload,
                    SeedChokingAlgorithm.AntiLeech => DataConverter.UploadChokingAlgorithms.AntiLeech,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            return null;
        }
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is string str)
            {
                if (str == DataConverter.UploadChokingAlgorithms.RoundRobin)
                    return SeedChokingAlgorithm.RoundRobin;
                if (str == DataConverter.UploadChokingAlgorithms.FastestUpload)
                    return SeedChokingAlgorithm.FastestUpload;
                if (str == DataConverter.UploadChokingAlgorithms.AntiLeech)
                    return SeedChokingAlgorithm.AntiLeech;

                throw new ArgumentOutOfRangeException();
            }
            return null;
        }
    }
}
