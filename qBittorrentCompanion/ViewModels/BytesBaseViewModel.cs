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

        protected override Task UpdateDataAsync(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override Task FetchDataAsync()
        {
            throw new NotImplementedException();
        }
    }
}
