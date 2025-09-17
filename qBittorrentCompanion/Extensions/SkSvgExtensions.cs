using qBittorrentCompanion.Helpers;
using Svg.Skia;
using System.IO;

namespace qBittorrentCompanion.Extensions
{
    public static class SKSvgExtensions
    { 
        public static bool SaveAsIco(this SKSvg skSvg, Stream output)
        {
            return IcoHelper.ConvertToIcon(skSvg, output);
        }

        public static bool SaveAsIco(this SKSvg skSvg, string filePath)
        {
            using var fs = File.Create(filePath);
            return IcoHelper.ConvertToIcon(skSvg, fs);
        }
    }
}
