using Avalonia.Controls;
using Avalonia.Threading;
using DynamicData;
using QBittorrent.Client;
using qBittorrentCompanion.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace qBittorrentCompanion.ViewModels
{
    public class BytesBaseViewModel : AutoUpdateViewModelBase
    {
        public static string[] SizeOptions => ["B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB"];

        public long GetMultiplierForUnit(string sizeUnit)
        {
            switch (sizeUnit)
            {
                case "B":
                default:
                    return 1L;
                case "KiB":
                    return 1024L; // 1 KiB = 1024 B
                case "MiB":
                    return 1024L * 1024L; // 1 MiB = 1024 KiB
                case "GiB":
                    return 1024L * 1024L * 1024L; // 1 GiB = 1024 MiB
                case "TiB":
                    return 1024L * 1024L * 1024L * 1024L; // 1 TiB = 1024 GiB
                case "PiB":
                    return 1024L * 1024L * 1024L * 1024L * 1024L; // 1 PiB = 1024 TiB
                case "EiB":
                    return 1024L * 1024L * 1024L * 1024L * 1024L * 1024L; // 1 EiB = 1024 PiB
            }
        }

        protected override Task UpdateDataAsync(object? sender, ElapsedEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override Task FetchDataAsync()
        {
            throw new NotImplementedException();
        }
    }
}
