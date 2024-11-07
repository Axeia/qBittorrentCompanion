using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System.Collections.Generic;
using System.IO;

namespace qBittorrentCompanion.Helpers
{
    public static class IcoHelper
    {
        public static int[] Sizes =>
            [16, 32, 48, 256];

        public static bool ConvertToIcon(Viewbox viewbox, Stream output)
        {
            if (viewbox == null)
                return false;

            RenderOptions.SetBitmapInterpolationMode(viewbox, BitmapInterpolationMode.LowQuality);

            var originalSize = viewbox.Width;

            // Generate bitmaps for all the sizes and toss them in streams
            List<MemoryStream> imageStreams = new List<MemoryStream>();
            foreach (int size in Sizes)
            {
                var resizedBitmap = RenderControlWithViewbox(viewbox, new PixelSize(size, size));
                if (resizedBitmap == null)
                    return false;
                var memoryStream = new MemoryStream();
                resizedBitmap.Save(memoryStream);
                imageStreams.Add(memoryStream);
            }

            using (BinaryWriter iconWriter = new BinaryWriter(output))
            {
                if (output == null || iconWriter == null)
                    return false;

                int offset = 0;

                // 0-1 reserved, 0
                iconWriter.Write((byte)0);
                iconWriter.Write((byte)0);

                // 2-3 image type, 1 = icon, 2 = cursor
                iconWriter.Write((short)1);

                // 4-5 number of images
                iconWriter.Write((short)Sizes.Length);

                offset += 6 + (16 * Sizes.Length);

                for (int i = 0; i < Sizes.Length; i++)
                {
                    // image entry 1
                    // 0 image width
                    iconWriter.Write((byte)Sizes[i]);
                    // 1 image height
                    iconWriter.Write((byte)Sizes[i]);

                    // 2 number of colors
                    iconWriter.Write((byte)0);

                    // 3 reserved
                    iconWriter.Write((byte)0);

                    // 4-5 color planes
                    iconWriter.Write((short)0);

                    // 6-7 bits per pixel
                    iconWriter.Write((short)32);

                    // 8-11 size of image data
                    iconWriter.Write((int)imageStreams[i].Length);

                    // 12-15 offset of image data
                    iconWriter.Write((int)offset);

                    offset += (int)imageStreams[i].Length;
                }

                for (int i = 0; i < Sizes.Length; i++)
                {
                    // write image data
                    iconWriter.Write(imageStreams[i].ToArray());
                    imageStreams[i].Close();
                }

                iconWriter.Flush();
            }

            //viewbox.Width = originalSize;
            //viewbox.Height = originalSize;

            return true;
        }

        public static RenderTargetBitmap RenderControlWithViewbox(Viewbox viewbox, PixelSize size)
        {
            viewbox.Width = size.Width;
            viewbox.Height = size.Height;

            viewbox.Measure(new Size(size.Width, size.Height));
            viewbox.Arrange(new Rect(0, 0, size.Width, size.Height));

            var renderBitmap = new RenderTargetBitmap(size, new Vector(96, 96));
            renderBitmap.Render(viewbox);

            return renderBitmap;
        }

        public static void SaveIcon(Viewbox viewbox, string filePath)
        {
            using FileStream fs = new(filePath, FileMode.Create);
            ConvertToIcon(viewbox, fs);
        }
    }
}