using Avalonia.Media;
using Avalonia.Platform;
using qBittorrentCompanion.Extensions;
using qBittorrentCompanion.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace qBittorrentCompanion.Helpers
{
    public static class LogoHelper
    {

        /// <remarks>
        /// As it states, gets the logo (<a href="/qBittorrentCompanion/Assets/qbc-logo.svg">qbc-logo.svg</a>)
        /// as an XDocument
        /// </remarks>
        /// <returns></returns>
        public static XDocument GetLogoAsXDocument()
        {
            var logoUri = new Uri("avares://qBittorrentCompanion/Assets/qbc-logo.svg");
            return XDocument.Load(AssetLoader.Open(logoUri));
        }

        /// <summary>
        /// Overloads <see cref="GetLogoAsXDocument()"/> and applies the colors from the given parameter
        /// </summary>
        /// <param name="logoDataRecord"></param>
        /// <returns></returns>
        public static XDocument GetLogoAsXDocument(LogoDataRecord logoDataRecord)
        {
            XDocument xDoc = GetLogoAsXDocument();
            xDoc
                .SetSvgStroke("q", logoDataRecord.Q.ToString(ColorFormat.RGBA_ALPHA_FLOAT))
                .SetSvgStroke("b", logoDataRecord.B.ToString(ColorFormat.RGBA_ALPHA_FLOAT))
                .SetSvgStroke("c", logoDataRecord.C.ToString(ColorFormat.RGBA_ALPHA_FLOAT));

            var stopColors = xDoc.GetStopColorsFromGradientById("gradient");
            string[] gradientColors = [
                logoDataRecord.GradientCenter.ToString(ColorFormat.RGBA_ALPHA_FLOAT), 
                logoDataRecord.GradientFill.ToString(ColorFormat.RGBA_ALPHA_FLOAT), 
                logoDataRecord.GradientRim.ToString(ColorFormat.RGBA_ALPHA_FLOAT)
            ];

            int count = Math.Min(stopColors.Count(), gradientColors.Length);
            for(int i = 0; i < count; i++)
                if (stopColors.ElementAt(i).Attribute("stop-color") is XAttribute xAttribute)
                    xAttribute.Value = gradientColors[i];

            return xDoc;
        }

        public static string PaleSystemAccentOutlinedLogoAsString
        {
            get
            {
                var tc = ThemeColors.SystemAccent;
                Color color = new(20, tc.R, tc.G, tc.B);
                //Debug.WriteLine(color.ToString(HEX_ARGB));
                return GetLogoAsXDocument(
                    new LogoDataRecord(
                        Q: color,
                        B: color,
                        C: color,
                        GradientCenter: Colors.Transparent,
                        GradientFill: Colors.Transparent,
                        GradientRim: Colors.Transparent
                    )
                ).ToString();
            }
        }
    }
}
