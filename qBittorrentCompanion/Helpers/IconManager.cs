using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using System.Collections.Generic;
using System.IO;

namespace qBittorrentCompanion.Helpers
{
    public enum WindowOverlayType { None, Download, Upload, Both, Preferences, Settings, Add }
    public enum IconLocation { TopLeft, BottomRight, TopRight }

    public static class IconManager
    {
        private static readonly Dictionary<(WindowOverlayType, ThemeVariant), WindowIcon> _cache = [];
        public static void ClearCache() => _cache.Clear();

        // Geometries from https://github.com/microsoft/fluentui-system-icons/blob/main/icons_filled.md 
        // (rightclick icon > open in new tab, view source)
        private static readonly StreamGeometry _downloadArrow = StreamGeometry.Parse("M25.5 6.5C25.5 5.67157 24.8284 5 24 5C23.1716 5 22.5 5.67157 22.5 6.5V31.8787L14.5607 23.9393C13.9749 23.3536 13.0251 23.3536 12.4393 23.9393C11.8536 24.5251 11.8536 25.4749 12.4393 26.0607L22.9393 36.5607C23.5251 37.1464 24.4749 37.1464 25.0607 36.5607L35.5607 26.0607C36.1464 25.4749 36.1464 24.5251 35.5607 23.9393C34.9749 23.3536 34.0251 23.3536 33.4393 23.9393L25.5 31.8787V6.5ZM10.5 40C9.67157 40 9 40.6716 9 41.5C9 42.3284 9.67157 43 10.5 43H37.5C38.3284 43 39 42.3284 39 41.5C39 40.6716 38.3284 40 37.5 40H10.5Z");
        private static readonly StreamGeometry _uploadArrow = StreamGeometry.Parse("M7.25 2C6.55964 2 6 2.55964 6 3.25C6 3.94036 6.55964 4.5 7.25 4.5H24.75C25.4404 4.5 26 3.94036 26 3.25C26 2.55964 25.4404 2 24.75 2H7.25ZM16.8839 6.86612C16.3957 6.37796 15.6043 6.37796 15.1161 6.86612L8.36612 13.6161C7.87796 14.1043 7.87796 14.8957 8.36612 15.3839C8.85427 15.872 9.64573 15.872 10.1339 15.3839L14.75 10.7678V28.75C14.75 29.4404 15.3096 30 16 30C16.6904 30 17.25 29.4404 17.25 28.75V10.7678L21.8661 15.3839C22.3543 15.872 23.1457 15.872 23.6339 15.3839C24.122 14.8957 24.122 14.1043 23.6339 13.6161L16.8839 6.86612Z");
        private static readonly StreamGeometry _preferences = StreamGeometry.Parse("M18 13C18.1678 13 18.3344 13.0083 18.498 13.0244C18.7956 13.0538 19.0252 13.2867 19.0878 13.5791L19.3544 14.8223C19.4237 15.146 19.7582 15.339 20.0732 15.2373L21.2822 14.8457C21.5662 14.754 21.882 14.837 22.0566 15.0791C22.2496 15.3471 22.417 15.6353 22.5546 15.9395C22.6778 16.2119 22.591 16.5271 22.3691 16.7275L21.4257 17.5801C21.1801 17.802 21.1801 18.1883 21.4257 18.4102L22.373 19.2646C22.5948 19.4648 22.6812 19.7804 22.5585 20.0527C22.4217 20.3565 22.2555 20.6443 22.0634 20.9121C21.889 21.1551 21.5717 21.2386 21.2871 21.1465L20.0732 20.7529C19.7582 20.6512 19.4237 20.8442 19.3544 21.168L19.0859 22.4199C19.0233 22.7124 18.7937 22.9457 18.496 22.9756C18.3331 22.9919 18.1672 23 18 23C17.8341 23 17.6695 22.9915 17.5078 22.9756C17.2099 22.946 16.9786 22.7136 16.916 22.4209L16.6494 21.168C16.5801 20.8442 16.2457 20.6511 15.9306 20.7529L14.7119 21.1465C14.4274 21.2382 14.1107 21.155 13.9365 20.9121C13.745 20.6451 13.5789 20.3584 13.4423 20.0557C13.3196 19.7834 13.4062 19.4678 13.6279 19.2676L14.5781 18.4102C14.8238 18.1883 14.8238 17.802 14.5781 17.5801L13.6308 16.7246C13.4088 16.5241 13.3218 16.208 13.4453 15.9355C13.5828 15.6321 13.7505 15.3445 13.9433 15.0771C14.1179 14.8353 14.4339 14.753 14.7177 14.8447L15.9306 15.2373C16.2457 15.3392 16.5801 15.1461 16.6494 14.8223L16.914 13.5791C16.9765 13.2865 17.2072 13.0541 17.5048 13.0244C17.6674 13.0084 17.8333 13 18 13ZM18 16.5C17.1719 16.5004 16.5 17.1718 16.5 18C16.5 18.8282 17.1719 19.4996 18 19.5C18.8284 19.5 19.5 18.8284 19.5 18C19.5 17.1716 18.8284 16.5 18 16.5ZM9.00776 1.49805C9.34737 1.49806 9.68098 1.52151 10.0068 1.56543L10.33 1.61621L10.4394 1.64453C10.6888 1.72927 10.8786 1.94064 10.9326 2.2041L11.2978 3.98633L11.3173 4.06055C11.4334 4.42205 11.8259 4.62709 12.1953 4.50391L13.9228 3.92773L14.0322 3.89941C14.2913 3.8546 14.5589 3.94943 14.7324 4.1543C15.3037 4.82893 15.7593 5.60646 16.0654 6.45605C16.1691 6.74458 16.0867 7.06796 15.8574 7.27148L14.4921 8.48242C14.1819 8.75784 14.1809 9.24216 14.4912 9.51758L15.8554 10.7266C16.0851 10.9302 16.1676 11.2542 16.0634 11.543C15.7567 12.3932 15.3011 13.1705 14.7294 13.8447C14.5313 14.0783 14.2105 14.168 13.9199 14.0713L12.1953 13.4961C11.8259 13.3729 11.4334 13.5779 11.3173 13.9395L11.2978 14.0137L10.9335 15.791C10.8719 16.0918 10.6332 16.3248 10.331 16.3789C9.90421 16.4549 9.46182 16.498 9.00776 16.498C8.54901 16.498 8.10153 16.455 7.6689 16.377C7.36709 16.3225 7.12887 16.0895 7.06733 15.7891L6.70503 14.0137C6.62183 13.6069 6.20142 13.3649 5.80757 13.4961L4.09175 14.0674C3.80095 14.1642 3.48023 14.0737 3.28218 13.8398C2.7102 13.1641 2.25332 12.3856 1.94722 11.5332C1.84354 11.2444 1.92649 10.9212 2.1562 10.7178L3.51167 9.51758C3.8217 9.24218 3.8217 8.75782 3.51167 8.48242L2.15522 7.28027C1.92584 7.07698 1.84288 6.75435 1.94624 6.46582C2.25177 5.61404 2.70754 4.83434 3.27925 4.1582L3.35835 4.07715C3.55642 3.90439 3.83422 3.8458 4.08882 3.93066L5.80757 4.50391C6.20142 4.63511 6.62183 4.39309 6.70503 3.98633L7.06831 2.20605L7.09858 2.09668C7.18953 1.85006 7.4057 1.66578 7.66987 1.61816C8.10313 1.5401 8.55035 1.49805 9.00776 1.49805ZM8.99995 6.75C7.75731 6.75 6.74995 7.75736 6.74995 9C6.74995 10.2426 7.75731 11.25 8.99995 11.25C10.2425 11.2499 11.25 10.2426 11.25 9C11.25 7.75739 10.2425 6.75005 8.99995 6.75Z");
        private static readonly StreamGeometry _settings = StreamGeometry.Parse("M16.5002 2C13.4627 2 11.0002 4.46243 11.0002 7.5C11.0002 7.94322 11.0528 8.37496 11.1523 8.78899L2.84099 17.1003C1.71967 18.2216 1.71967 20.0396 2.84099 21.1609C3.96231 22.2823 5.78033 22.2823 6.90165 21.1609L15.214 12.8486C15.6272 12.9477 16.058 13 16.5002 13C19.5378 13 22.0002 10.5376 22.0002 7.5C22.0002 6.89137 21.9011 6.30437 21.7175 5.75516C21.6351 5.50875 21.4311 5.32252 21.1782 5.26294C20.9253 5.20336 20.6595 5.27892 20.4758 5.46266L18.0317 7.90697C17.7389 8.19989 17.264 8.19991 16.9711 7.907L16.0934 7.0293C15.8005 6.73642 15.8005 6.26156 16.0933 5.96867L18.5375 3.5243C18.7212 3.34057 18.7967 3.07485 18.7371 2.82197C18.6775 2.56908 18.4913 2.36505 18.2449 2.28268C17.6957 2.09912 17.1088 2 16.5002 2Z");
        private static readonly StreamGeometry _add = StreamGeometry.Parse("M44 24C44 35.0457 35.0457 44 24 44C12.9543 44 4 35.0457 4 24C4 12.9543 12.9543 4 24 4C35.0457 4 44 12.9543 44 24ZM24 14C23.3096 14 22.75 14.5596 22.75 15.25V22.75H15.25C14.5596 22.75 14 23.3096 14 24C14 24.6904 14.5596 25.25 15.25 25.25H22.75V32.75C22.75 33.4404 23.3096 34 24 34C24.6904 34 25.25 33.4404 25.25 32.75V25.25H32.75C33.4404 25.25 34 24.6904 34 24C34 23.3096 33.4404 22.75 32.75 22.75H25.25V15.25C25.25 14.5596 24.6904 14 24 14Z");


        public static WindowIcon GetIcon(WindowOverlayType type, ThemeVariant theme, Bitmap? baseBmp)
        {
            if (baseBmp == null) return null!;
            
            var key = (type, theme);

            if (_cache.TryGetValue(key, out var cached)) return cached;

            // If not cached yet, create and cache
            var newIcon = CreateIconWithOverlay(baseBmp, type);
            _cache[key] = newIcon;
            return newIcon;
        }

        private static WindowIcon CreateIconWithOverlay(Bitmap baseBmp, WindowOverlayType type)
        {
            var renderSize = new PixelSize(32, 32);
            using var rtbm = new RenderTargetBitmap(renderSize, new Vector(96, 96));

            using var msScaled = new MemoryStream();
            baseBmp.Save(msScaled);
            msScaled.Position = 0;
            using var smallBitmap = Bitmap.DecodeToWidth(msScaled, 32, BitmapInterpolationMode.HighQuality);

            using (var context = rtbm.CreateDrawingContext())
            {
                context.DrawImage(smallBitmap, new Rect(0, 0, 32, 32));

                // Speed arrows: remain at 55% to avoid overcrowding the center
                if (type == WindowOverlayType.Download || type == WindowOverlayType.Both)
                    DrawSymbol(context, _downloadArrow, IconLocation.BottomRight, 48.0, 0.35);

                if (type == WindowOverlayType.Upload || type == WindowOverlayType.Both)
                    DrawSymbol(context, _uploadArrow, IconLocation.TopLeft, 24.0, 0.35);

                // Action icons: larger at 70%
                if (type == WindowOverlayType.Preferences)
                    DrawSymbol(context, _preferences, IconLocation.TopRight, 28.0);

                if (type == WindowOverlayType.Settings)
                    DrawSymbol(context, _settings, IconLocation.TopRight, 28.0);

                if (type == WindowOverlayType.Add)
                    DrawSymbol(context, _add, IconLocation.TopRight, 28.0);
            }

            using var ms = new MemoryStream();
            rtbm.Save(ms, PngBitmapEncoderOptions.Default);
            ms.Position = 0;
            return new WindowIcon(ms);

        }

        private static void DrawSymbol(DrawingContext context, Geometry geo, IconLocation alignment, double internalDesignSize, double sizeRatio = 0.70)
        {
            double targetSize = 32 * sizeRatio;
            double scale = targetSize / internalDesignSize;

            double offsetX = 0;
            double offsetY = 0;

            switch (alignment)
            {
                case IconLocation.BottomRight:
                    offsetX = 32 - targetSize;
                    offsetY = 32 - targetSize;
                    break;
                case IconLocation.TopRight:
                    offsetX = 32 - targetSize;
                    offsetY = 0;
                    break;
                case IconLocation.TopLeft:
                    offsetX = 0;
                    offsetY = 0;
                    break;
            }

            using (context.PushTransform(Matrix.CreateScale(scale, scale) * Matrix.CreateTranslation(offsetX, offsetY)))
            {
                var borderBrush = new SolidColorBrush(ThemeColors.SystemAccentDark3);
                var iconBrush = new SolidColorBrush(ThemeColors.SystemAccentLight1);

                var outlinePen = new Pen(borderBrush, 3 / scale, lineJoin: PenLineJoin.Miter);
                context.DrawGeometry(borderBrush, outlinePen, geo);

                var innerPen = new Pen(iconBrush, 1 / scale, lineJoin: PenLineJoin.Miter);
                context.DrawGeometry(iconBrush, innerPen, geo);
            }
        }
    }
}