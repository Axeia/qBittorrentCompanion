using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using System.Collections.Generic;
using System.IO;

namespace qBittorrentCompanion.Helpers
{
    public static class IcoHelper
    {
        public static bool ConvertToIcon(RenderTargetBitmap inputBitmap, Stream output)
        {
            if (inputBitmap == null)
                return false;

            int[] sizes = [256, 48, 32, 16];

            // Generate bitmaps for all the sizes and toss them in streams
            List<MemoryStream> imageStreams = [];
            foreach (int size in sizes)
            {
                var resizedBitmap = ResizeImage(inputBitmap, new PixelSize(size, size));
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
                iconWriter.Write((short)sizes.Length);

                offset += 6 + (16 * sizes.Length);

                for (int i = 0; i < sizes.Length; i++)
                {
                    // image entry 1
                    // 0 image width
                    iconWriter.Write((byte)sizes[i]);
                    // 1 image height
                    iconWriter.Write((byte)sizes[i]);

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

                for (int i = 0; i < sizes.Length; i++)
                {
                    // write image data
                    iconWriter.Write(imageStreams[i].ToArray());
                    imageStreams[i].Close();
                }

                iconWriter.Flush();
            }

            return true;
        }

        public static RenderTargetBitmap ResizeImage(RenderTargetBitmap image, PixelSize newSize)
        {
            var control = new Image {
                Source = image
            };

            control.Measure(new Size(newSize.Width, newSize.Height));
            control.Arrange(new Rect(0, 0, newSize.Width, newSize.Height));

            var renderBitmap = new RenderTargetBitmap(newSize, new Vector(96, 96));
            renderBitmap.Render(control);

            return renderBitmap;
        }

        public static void SaveIcon(RenderTargetBitmap bitmap, string filePath)
        {
            using FileStream fs = new(filePath, FileMode.Create);
            ConvertToIcon(bitmap, fs);
        }
    }
}