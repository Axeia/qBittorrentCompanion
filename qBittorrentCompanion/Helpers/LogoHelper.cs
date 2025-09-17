using Avalonia.Platform;
using qBittorrentCompanion.Extensions;
using qBittorrentCompanion.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace qBittorrentCompanion.Helpers
{
    public class LogoHelper
    {
        public static LogoColorsRecord ExtractColorsFromXDocument(XDocument xDoc)
        {
            Dictionary<string, string> strokes = ExtractStrokeColorMap(xDoc);
            var stops = xDoc.GetStopColorsFromGradientById("gradient");

            return new LogoColorsRecord(
                Q: strokes["q"],
                B: strokes["b"],
                C: strokes["c"],
                GradientCenter: stops.ElementAt(0)?.Attribute("stop-color")?.Value ?? string.Empty,
                GradientFill: stops.ElementAt(1)?.Attribute("stop-color")?.Value ?? string.Empty,
                GradientRim: stops.ElementAt(2)?.Attribute("stop-color")?.Value ?? string.Empty
            );
        }

        private static Dictionary<string, string> ExtractStrokeColorMap(XDocument doc)
        {

            return doc.Descendants()
                .Where(e => e.Attribute("id") is not null && e.Attribute("stroke") is not null)
                .ToDictionary(
                    e => e.Attribute("id")?.Value ?? string.Empty,
                    e => e.Attribute("stroke")?.Value ?? string.Empty
                );
        }

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
    }
}
