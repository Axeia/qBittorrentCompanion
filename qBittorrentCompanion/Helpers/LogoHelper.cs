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
        /// <param name="logoColorsRecord"></param>
        /// <returns></returns>
        public static XDocument GetLogoAsXDocument(LogoColorsRecord logoColorsRecord)
        {
            XDocument xDoc = GetLogoAsXDocument();
            xDoc
                .SetSvgStroke("q", logoColorsRecord.Q)
                .SetSvgStroke("b", logoColorsRecord.B)
                .SetSvgStroke("c", logoColorsRecord.C);

            var stopColors = xDoc.GetStopColorsFromGradientById("gradient");
            string[] gradientColors =
                [logoColorsRecord.GradientCenter, logoColorsRecord.GradientFill, logoColorsRecord.GradientRim];

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
                Debug.WriteLine(color.ToString("RGBa"));
                return GetLogoAsXDocument(
                    new LogoColorsRecord(
                        Q: color.ToString("RGBa"),
                        B: color.ToString("RGBa"),
                        C: color.ToString("RGBa"),
                        GradientCenter: "transparent",
                        GradientFill: "transparent",
                        GradientRim: "transparent"
                    )
                ).ToString();
            }
        }
    }
}
