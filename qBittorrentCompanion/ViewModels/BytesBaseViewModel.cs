using System;
using System.Threading.Tasks;

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
