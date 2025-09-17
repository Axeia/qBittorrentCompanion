using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using SkiaSharp;
using Svg;
using Svg.Skia;
using System;
using System.Collections.Generic;
using System.IO;

namespace qBittorrentCompanion.Helpers
{
    public static class IcoHelper
    {

        /// <summary>
        /// Based on <see href="https://learn.microsoft.com/en-us/windows/apps/design/style/iconography/app-icon-construction#icon-scaling">
        /// Microsoft's recommendation for Icon sizes</see> to have the best possible looking icon on all possible windows scaling settings
        /// </summary>
        public static int[] WindowsIconSizes =>
            [16, 20, 24, 30, 32, 36, 40, 48, 60, 64, 72, 80, 96, 256];

        /// <summary>
        /// Converts <see cref="SKSvg"/> to the .ico format and outputs it to the given output stream
        /// Heavily based on code from DarkFall ( https://gist.github.com/darkfall/1656050 ) 
        /// <see cref="SkSvgExtensions.SaveAsIco"/>
        /// </summary>
        /// <param name="skSvg"></param>
        /// <param name="output"></param>
        /// 
        /// <returns>True on success. False on fail</returns>
        public static bool ConvertToIcon(SKSvg skSvg, Stream output)
        {
            // Generate .png's for all WindowsIconSizes and add them to pngStreams array
            List<MemoryStream> pngStreams = [];
            foreach (int size in WindowsIconSizes)
            {
                var info = new SKImageInfo(size, size);
                var bitmap = new SKBitmap(info);
                using var canvas = new SKCanvas(bitmap);

                canvas.Clear(SKColors.Transparent);

                var bounds = skSvg.Picture!.CullRect;
                var scale = Math.Min(size / bounds.Width, size / bounds.Height);

                var xOffset = (size - bounds.Width * scale) / 2;
                var yOffset = (size - bounds.Height * scale) / 2;

                canvas.Translate(xOffset, yOffset);
                canvas.Scale(scale);
                canvas.DrawPicture(skSvg.Picture);

                using var image = SKImage.FromBitmap(bitmap);
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                pngStreams.Add(new MemoryStream(data.ToArray()));

                bitmap.Dispose();
            }

            // Format details on:
            //https://en.wikipedia.org/wiki/ICO_(file_format)
            using BinaryWriter iconWriter = new(output);
            int offset = 0;

            // 0-1 reserved, 0
            iconWriter.Write((byte)0);
            iconWriter.Write((byte)0);

            // 2-3 image type, 1 = icon, 2 = cursor
            iconWriter.Write((short)1);

            // 4-5 number of images
            iconWriter.Write((short)WindowsIconSizes.Length);

            offset += 6 + (16 * WindowsIconSizes.Length);

            for (int i = 0; i < WindowsIconSizes.Length; i++)
            {
                // image entry 1
                // 0 image width
                iconWriter.Write((byte)WindowsIconSizes[i]);
                // 1 image height
                iconWriter.Write((byte)WindowsIconSizes[i]);

                // 2 number of colors
                iconWriter.Write((byte)0);

                // 3 reserved
                iconWriter.Write((byte)0);

                // 4-5 color planes
                iconWriter.Write((short)0);

                // 6-7 bits per pixel
                iconWriter.Write((short)32);

                // 8-11 size of image data
                iconWriter.Write((int)pngStreams[i].Length);

                // 12-15 offset of image data
                iconWriter.Write((int)offset);

                offset += (int)pngStreams[i].Length;
            }

            for (int i = 0; i < WindowsIconSizes.Length; i++)
            {
                // write image data
                iconWriter.Write(pngStreams[i].ToArray());
                pngStreams[i].Close();
            }

            iconWriter.Flush();

            return true;
        }

        public static void SaveIcon(SKSvg skSvg, string filePath)
        {
            using FileStream fs = new(filePath, FileMode.Create);
            ConvertToIcon(skSvg, fs);
        }
    }
}