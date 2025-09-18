namespace qBittorrentCompanion.Extensions
{
    public static class ColorExtensions
    {
        /// <summary>
        /// Converts to ARGB hex string (#AARRGGBB), commonly used in HTML/SVG
        /// Has the Alpha value at the start, opposite to .ToString() which adds it to the end.
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string ToHex(this Avalonia.Media.Color color) =>
            $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

        /// <summary>
        /// Converts to RGBA(0,0,0,0.0), commonly used in CSS.
        /// E.g. Color.Red.ToRgba = "RGBA(255,0, 0, 1)"
        /// 
        /// The first three values are in the 0-255 range, the last (Alpha) is in between 0.0 and 1.0
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string ToRgba(this Avalonia.Media.Color color) =>
            $"rgba({color.R},{color.G},{color.B},{color.A / 255.0:F2})";
    }
}
