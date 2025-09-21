using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace qBittorrentCompanion.Extensions
{
    public static class ColorExtensions
    {
        /// <summary>
        /// Overload to support a wealth of formatting options
        /// 
        /// The most popular options are (optional between brackets: #HHH[A], #HHHHHH[AA], #AAHHHHH, RGB[A]<br/>
        /// Upper & Lowercase `H` result in upper and lower case hex values respectively. `A` adds Alpha. Invididual values in the 
        /// 0.255 range can also be requests through `R`, `G`, `B` and `A`
        /// </summary>
        /// <param name="color"></param>
        /// <param name="format"></param>
        /// <param name="useNamedColor"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public static string ToString(
            this Avalonia.Media.Color color, 
            [StringSyntax("AvaloniaColorFormat")]string format, 
            bool useNamedColor = false)
        {
            var builder = new StringBuilder();
            int i = 0;

            if (useNamedColor)
            {
                string name = color.ToString();
                if (!name.StartsWith('#'))
                    return name;
            }

            static bool IsShortHexable(byte value) => (value >> 4) == (value & 0xF);

            while (i < format.Length)
            {
                // Try to match known tokens (when there's a match 'overlap' always match the longest first)

                if (format[i..].StartsWith("#hhhhhhaa", StringComparison.Ordinal))
                {
                    builder.Append($"#{color.R:x2}{color.G:x2}{color.B:x2}{color.A:x2}");
                    i += 9;
                }
                else if (format[i..].StartsWith("#HHHHHHAA", StringComparison.Ordinal))
                {
                    builder.Append($"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}");
                    i += 9;
                }
                else if (format[i..].StartsWith("#aahhhhhh", StringComparison.Ordinal))
                {
                    builder.Append($"#{color.A:x2}{color.R:x2}{color.G:x2}{color.B:x2}");
                    i += 9;
                }
                else if (format[i..].StartsWith("#AAHHHHHH", StringComparison.Ordinal))
                {
                    builder.Append($"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}");
                    i += 9;
                }
                else if (format[i..].StartsWith("#hhhhhh", StringComparison.Ordinal))
                {
                    builder.Append($"#{color.A:x2}{color.R:x2}{color.G:x2}{color.B:x2}");
                    i += 7;
                }
                else if (format[i..].StartsWith("#HHHHHH", StringComparison.Ordinal))
                {
                    builder.Append($"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}");
                    i += 7;
                }
                // Matches '#hhha' as well, and validates short hex compatibility
                else if (format[i..].StartsWith("#hhha", StringComparison.Ordinal))
                {
                    if (!IsShortHexable(color.R) || !IsShortHexable(color.G) || !IsShortHexable(color.B))
                        throw new FormatException("Color channels are not compatible with short hex format.");

                    builder.Append($"#{color.R:h2}{color.G:h2}{color.B:h2}");
                    i += 4;
                    if (i < format.Length && format[i] == 'a')
                    {
                        if (!IsShortHexable(color.A))
                            throw new FormatException("Alpha channel is not compatible with short hex format.");
                        
                        builder.Append($"{color.A:x2}");
                        i++;
                    }
                }
                // Matches `#HHHA` as well, and validates short hex compatibility
                else if (format[i..].StartsWith("#HHH", StringComparison.Ordinal))
                {
                    if (!IsShortHexable(color.R) || !IsShortHexable(color.G) || !IsShortHexable(color.B))
                        throw new FormatException("Color channels are not compatible with short hex format.");

                    builder.Append($"#{color.R:X2}{color.G:X2}{color.B:X2}");
                    i += 4;

                    if(i < format.Length && format[i] == 'A')
                    {
                        if (!IsShortHexable(color.A))
                            throw new FormatException("Alpha channel is not compatible with short hex format.");

                        builder.Append($"{color.A:X2}");
                        i++;
                    }
                }
                else if (format[i..].StartsWith("RGBa"))
                {
                    builder.Append($"rgba({color.R}, {color.G}, {color.B}, {color.A / 255.0:F2})");
                    i += 4;
                }
                else if (format[i..].StartsWith("RGB"))
                {
                    builder.Append($"rgb({color.R}, {color.G}, {color.B})");
                    i += 3;
                }
                else
                {
                    char c = format[i];
                    switch (c)
                    {
                        case 'R': builder.Append(color.R); break;
                        case 'r': builder.Append((color.R / 255.0).ToString("F2")); break;
                        case 'G': builder.Append(color.G); break;
                        case 'g': builder.Append((color.G / 255.0).ToString("F2")); break;
                        case 'B': builder.Append(color.B); break;
                        case 'b': builder.Append((color.B / 255.0).ToString("F2")); break;
                        case 'A': builder.Append(color.A); break;
                        case 'a': builder.Append((color.A / 255.0).ToString("F2")); break;
                        default: builder.Append(c); break;
                    }
                    i++;
                }
            }

            return builder.ToString();
        }

    }


}
