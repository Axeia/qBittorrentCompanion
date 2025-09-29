using Avalonia.Media;
using System.Collections.Generic;

namespace qBittorrentCompanion.Extensions
{
    public record ColorFormatInfo(
        bool IncludesAlpha,
        bool SupportsShortHex,
        bool IsCssCompatible,
        string Description
    );

    public static class ColorFormatMetadata
    {
        private static readonly Dictionary<ColorFormat, ColorFormatInfo> _info = new()
        {
            [ColorFormat.HEX_RGBA] = new(true, true, false, "Hex with trailing alpha"),
            [ColorFormat.HEX_ARGB] = new(true, true, false, "Hex with leading alpha"),
            [ColorFormat.HEX_RGB] = new(false, true, false, "Hex without alpha"),
            [ColorFormat.ARGB_ALPHA_INT] = new(true, false, true, "CSS rgba with integer alpha first"),
            [ColorFormat.RGBA_ALPHA_INT] = new(true, false, true, "CSS rgba with integer alpha last"),
            [ColorFormat.ARGB_ALPHA_FLOAT] = new(true, false, true, "CSS rgba with float alpha first"),
            [ColorFormat.RGBA_ALPHA_FLOAT] = new(true, false, true, "CSS rgba with float alpha last"),
            [ColorFormat.RGB] = new(false, false, true, "CSS rgb without alpha")
        };

        public static ColorFormatInfo GetInfo(this ColorFormat format) => _info[format];
    }

    /// <summary>
    /// Describes common color formats, to be used with <see cref="ColorExtensions.ToString(Color, ColorFormat, bool, bool, bool)"/>
    /// </summary>
    public enum ColorFormat
    {
        ///<summary>
        ///Hex with alpha at the end, HTML/CSS/SVG (warning: Support for it is lacklustre in a lot of SVG renderers, 
        ///<see cref="ARGB_ALPHA_FLOAT"/> is a much safer bet in most cases)
        ///<list type="bullet">
        ///<item><example><c>Colors.Lime.ToString(ColorFormat.HEX_RGBA); // "#00FF00FF"</c></example></item>
        ///</list>
        ///</summary>
        HEX_RGBA,

        ///<summary>
        ///Hex with alpha at the front, used in some XAML and Win32 contexts
        ///<list type="bullet">
        ///<item><example><c>Colors.Lime.ToString(ColorFormat.HEX_ARGB); // "#FF00FF00"</c></example></item>
        ///</list>
        ///</summary>
        HEX_ARGB,

        ///<summary>
        ///Hex without alpha, suitable for contexts where transparency is ignored
        ///<list type="bullet">
        ///<item><example><c>Colors.Lime.ToString(ColorFormat.HEX_RGB); // "#00FF00"</c></example></item>
        ///</list>
        ///</summary>
        HEX_RGB,

        ///<summary>
        ///CSS-style rgba output with alpha as an integer (0–255), alpha comes first
        ///<list type="bullet">
        ///<item><example><c>Colors.Lime.ToString(ColorFormat.ARGB_ALPHA_INT); // "rgba(255, 0, 255, 0)"</c></example></item>
        ///</list>
        ///</summary>
        ARGB_ALPHA_INT,

        ///<summary>
        ///CSS-style rgba output with alpha as an integer (0–255), alpha comes last
        ///<list type="bullet">
        ///<item><example><c>Colors.Lime.ToString(ColorFormat.RGBA_ALPHA_INT); // "rgba(0, 255, 0, 255)"</c></example></item>
        ///</list>
        ///</summary>
        RGBA_ALPHA_INT,

        ///<summary>
        ///CSS-style rgba output with alpha as a float (0.0–1.0), alpha comes first
        ///<list type="bullet">
        ///<item><example><c>Colors.Lime.ToString(ColorFormat.ARGB_ALPHA_FLOAT); // "rgba(1.00, 0, 255, 0)"</c></example></item>
        ///</list>
        ///</summary>
        ARGB_ALPHA_FLOAT,

        ///<summary>
        ///CSS-style rgba output with alpha as a float (0.0–1.0), alpha comes last
        ///<list type="bullet">
        ///<item><example><c>Colors.Lime.ToString(ColorFormat.ARGB_ALPHA_FLOAT); // "rgba(0, 255, 0, 1.00)"</c></example></item>
        ///</list>
        ///</summary>
        RGBA_ALPHA_FLOAT,

        ///<summary>
        ///CSS-style rgb output without alpha
        ///<list type="bullet">
        ///<item><example><c>Colors.Lime.ToString(ColorFormat.RGB); // "rgb(0, 255, 0)"</c></example></item>
        ///</list>
        ///</summary>
        RGB
    }

    /// <summary>
    /// Streamlines checking what the format is about by adding a few methods that will tell you about
    /// As <see cref="ColorFormat"/> is an enum this is done through this extension
    /// </summary>
    public static class ColorFormatExtensions
    {
        /// <summary>
        /// Checks if this format support the shorthand variant? (spoiler: all HEX_ values do)
        /// e.g. #FFFFFF can be shortened to #FFF but #CEC3CE cannot
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static bool SupportsShortHex(this ColorFormat format) =>
            format == ColorFormat.HEX_RGB ||
            format == ColorFormat.HEX_RGBA ||
            format == ColorFormat.HEX_ARGB;

        /// <summary>
        /// Answers if this is one of the HEX_ values with the Alpha value included
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static bool HexWithAlpha(this ColorFormat format) =>
            format == ColorFormat.HEX_RGBA || format == ColorFormat.HEX_ARGB;
    }

    /// <summary>
    /// Adds a new .ToString method accepting a ColorFormat parameter to quickly and easily get common formats
    /// <see cref="ColorFormat"/>
    /// </summary>
    public static class ColorExtensions
    {
        /// <summary>
        ///  Not all 255 color values can be represented by a single digit hex - which goes up to 16 (or rather, F).
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNibbleRepeatable(byte value) => 
            (value >> 4) == (value & 0xF);

        /// <summary>
        /// Checks if the color can have a short hand form (all its colors can be reduced down to a single digit hex value)
        /// </summary>
        /// <param name="color"></param>
        /// <param name="checkAlpha"></param>
        /// <returns></returns>
        public static bool IsHexShortHandCompatible(this Color color, bool checkAlpha = true)
            => IsNibbleRepeatable(color.R) && IsNibbleRepeatable(color.G) && IsNibbleRepeatable(color.B)
                && checkAlpha ? IsNibbleRepeatable(color.A) : true;

        /// <summary>
        /// Call the base .ToString() implementation to see if it figured out a name<br/>
        /// (Can't access <see cref="KnownColors"/> ourselves as it's declared private)
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static bool IsNamedColor(this Color color)
            => color.ToString() is string cstr && !cstr.StartsWith('#');

        /// <summary>
        /// Converts the color to a string according to the format parameter.<br/>
        /// The optional named parameters can be used to deviate from the formatting where and if applicable 
        /// (e.g. get <c>"Lime"</c> instead of <c>"#00FF00"</c>)
        /// 
        /// <list type="bullet"><item><description>
        /// If you need a different format, <see cref="Color.R"/>, <see cref="Color.G"/>, <see cref="Color.B"/> contain the colors with a value between 0 and 255. <br/>
        /// You can format them like so to a float in the <c>0.0-1.0</c> range <example><c>{Colors.Red.R / 255.0:F2}</c></example>, 
        /// and to 2 digit (uppercase) hex like so <example><c>{Colors.Red.R:X2}</c></example> (for single digit use <c>:X1</c>*, use a lowercase x for lowercase <c>:x1</c> or <c>:X2)</c><br/>
        /// *Note: The hexadecimal system goes up to 16 values per digit so not all 255 values can be shortened to 1 digit, you can use <see cref="IsShortHexable(byte)"/> to check if it does
        /// </description></item></list>
        /// </summary>
        /// <param name="color"></param>
        /// <param name="format"></param>
        /// <param name="preferNamedColor"></param>
        /// <param name="preferShortHand"></param>
        /// <param name="preferLowerCaseHex">Only affects hex values
        ///   (<see cref="ColorFormat.HEX_RGBA"/>, <see cref="ColorFormat.HEX_ARGB"/>, <see cref="ColorFormat.HEX_RGB"/>)
        /// </param>
        /// <param name="prefixHexWithHash">Set to false to just get the values without the hash at the start</param>
        /// <returns></returns>
        public static string ToString(this Color color, ColorFormat format, 
            bool preferNamedColor = false, bool preferShortHand = false, bool preferLowerCaseHex = true, bool prefixHexWithHash = true)
        {
            // If named color is preferred, check if it has one
            if (preferNamedColor && color.IsNamedColor())
                return color.ToString();

            string hexPrefix = prefixHexWithHash ? "#" : string.Empty;

            // Check if it should be short handed (requested and supported)
            bool shouldBeShortHand = preferShortHand && format.SupportsShortHex() && IsHexShortHandCompatible(color, format.HexWithAlpha());

            switch (format)
            {
                case ColorFormat.HEX_ARGB:
                    return shouldBeShortHand
                        ? preferLowerCaseHex
                            ? $"{hexPrefix}{color.A:x1}{color.R:x1}{color.G:x1}{color.B:x1}"
                            : $"{hexPrefix}{color.A:X1}{color.R:X1}{color.G:X1}{color.B:X1}"
                        : preferLowerCaseHex
                            ? $"{hexPrefix}{color.A:x2}{color.R:x2}{color.G:x2}{color.B:x2}"
                            : $"{hexPrefix}{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
                case ColorFormat.HEX_RGBA:
                default:
                    return shouldBeShortHand
                        ? preferLowerCaseHex
                            ? $"{hexPrefix}{color.R:x1}{color.G:x1}{color.B:x1}{color.A:x1}"
                            : $"{hexPrefix}{color.R:X1}{color.G:X1}{color.B:X1}{color.A:X1}"
                        : preferLowerCaseHex
                            ? $"{hexPrefix}{color.R:x2}{color.G:x2}{color.B:x2}{color.A:x2}"
                            : $"{hexPrefix}{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";
                case ColorFormat.HEX_RGB:
                    return shouldBeShortHand
                        ? preferLowerCaseHex
                            ? $"{hexPrefix}{color.R:x1}{color.G:x1}{color.B:x1}"
                            : $"{hexPrefix}{color.R:X1}{color.G:X1}{color.B:X1}"
                        : preferLowerCaseHex
                            ? $"{hexPrefix}{color.R:x2}{color.G:x2}{color.B:x2}"
                            : $"{hexPrefix}{color.R:X2}{color.G:X2}{color.B:X2}";
                case ColorFormat.ARGB_ALPHA_FLOAT:
                    return $"rgba({color.A / 255.0:F2}, {color.R}, {color.G}, {color.B})";
                case ColorFormat.RGBA_ALPHA_FLOAT:
                    return $"rgba({color.R}, {color.G}, {color.B}, {color.A / 255.0:F2})";
                case ColorFormat.ARGB_ALPHA_INT:
                    return $"rgba({color.A}, {color.R}, {color.G}, {color.B})";
                case ColorFormat.RGBA_ALPHA_INT:
                    return $"rgba({color.R}, {color.G}, {color.B}, {color.A})";
                case ColorFormat.RGB:
                    return $"rgb({color.R}, {color.G}, {color.B})";
            };
        }
    }
}