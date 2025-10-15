using Avalonia.Media;
using Avalonia.Platform;
using qBittorrentCompanion.Extensions;
using qBittorrentCompanion.Models;
using qBittorrentCompanion.ViewModels.LocalSettings;
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

            // Set SvgStroke colors of letters through extension method
            xDoc
                .SetSvgStroke("q", logoDataRecord.Q.ToString(ColorFormat.RGBA_ALPHA_FLOAT))
                .SetSvgStroke("b", logoDataRecord.B.ToString(ColorFormat.RGBA_ALPHA_FLOAT))
                .SetSvgStroke("c", logoDataRecord.C.ToString(ColorFormat.RGBA_ALPHA_FLOAT));

            // Create array to match the gradient found in the svg (will be index matched)
            string[] gradientColors = [
                logoDataRecord.GradientCenter.ToString(ColorFormat.RGBA_ALPHA_FLOAT), 
                logoDataRecord.GradientFill.ToString(ColorFormat.RGBA_ALPHA_FLOAT), 
                logoDataRecord.GradientRim.ToString(ColorFormat.RGBA_ALPHA_FLOAT)
            ];

            // Get gradient colors and set them to the values of the above gradientColors by matching index
            var stopColors = xDoc.GetStopColorsFromGradientById("gradient");
            int count = Math.Min(stopColors.Count(), gradientColors.Length);
            for(int i = 0; i < count; i++)
                if (stopColors.ElementAt(i).Attribute("stop-color") is XAttribute xAttribute)
                    xAttribute.Value = gradientColors[i];

            return xDoc;
        }

        /// <summary>
        /// Overloads <see cref="GetLogoAsXDocument()"/> and applies the colors from the logoDataRecord
        /// as wel as setting the dark light mode comment to the given value.
        /// </summary>
        /// <param name="logoDataRecord"></param>
        /// <param name="iconSaveMode"></param>
        /// <returns></returns>
        public static XDocument GetLogoAsXDocument(LogoDataRecord logoDataRecord, IconSaveMode iconSaveMode)
        {
            XDocument xDoc = GetLogoAsXDocument(logoDataRecord);

            // Should be ["0", "1", "2"] representing [Dark+Light, Dark, Light]
            var validDarkLightModeOptions = Enum.GetValues(typeof(IconSaveMode))
                .Cast<int>()
                .Select(i => i.ToString())
                .ToList();

            //Find the comment that contains the dark light mode option
            var firstComment = xDoc.DescendantNodes()
                .OfType<XComment>()
                .FirstOrDefault();
            if (firstComment is XComment xComment && validDarkLightModeOptions.Contains(xComment.Value))
            {
                xComment.Value = iconSaveMode.ModeAsIntString();
            }

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
