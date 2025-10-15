using Avalonia.Media;
using Avalonia.Platform;
using qBittorrentCompanion.Extensions;
using qBittorrentCompanion.Models;
using qBittorrentCompanion.ViewModels.LocalSettings;
using System;
using System.Collections.Generic;
using System.IO;
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

            if (GetModeNode(xDoc) is XComment xComment && _validDarkLightModeOptions.Contains(xComment.Value))
            {
                xComment.Value = iconSaveMode.ModeAsIntString();
            }

            return xDoc;
        }

        private static List<string> _validDarkLightModeOptions 
            => [.. Enum.GetValues(typeof(IconSaveMode))
                .Cast<int>()
                .Select(i => i.ToString())];

        /// <summary>
        /// Retrieves the mode node from the given <see cref="XDocument"/><br/>
        /// (basically just the first <see cref="XComment"/> node)
        /// </summary>
        /// <param name="xDoc"></param>
        /// <returns></returns>
        private static XComment? GetModeNode(XDocument xDoc)
        {
            //Find the comment that contains the dark light mode option
            return xDoc.DescendantNodes()
                .OfType<XComment>()
                .FirstOrDefault();
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

        /// <summary>
        /// Parses the svg at the filepath into a <see cref="LogoPresetRecord">
        /// </summary>
        /// <param name="svgFilePath"></param>
        /// <returns></returns>
        public static LogoPresetRecord CreateLogoPresetRecordFromSvg(string svgFilePath)
        {
            XDocument xDoc = XDocument.Load(svgFilePath);
            var stopColors = xDoc.GetStopColorsFromGradientById("gradient");

            // Set default, then try to get it from the svg xDoc
            IconSaveMode ics = IconSaveMode.DarkAndLight;
            if (GetModeNode(xDoc) is XComment xComment && _validDarkLightModeOptions.Contains(xComment.Value))
                ics = (IconSaveMode)int.Parse(xComment.Value);

            LogoDataRecord ldr = new(
                Q: xDoc.GetSvgStroke("q"),
                B: xDoc.GetSvgStroke("b"),
                C: xDoc.GetSvgStroke("c"),
                GradientCenter: Color.Parse(stopColors.ElementAt(0).Attribute("stop-color")!.Value.ToString()),
                GradientFill: Color.Parse(stopColors.ElementAt(1).Attribute("stop-color")!.Value.ToString()),
                GradientRim: Color.Parse(stopColors.ElementAt(0).Attribute("stop-color")!.Value.ToString())
            );

            return new LogoPresetRecord(
                Path.GetFileNameWithoutExtension(svgFilePath),
                ldr,
                ics
            );
        }
    }
}
