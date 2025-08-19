using RssPlugins;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SeriesRssPlugin
{
    public partial class SeriesRssPlugin : RssRulePluginBase
    {
        public override string Name => "Series";
        public override string Version => "v25.08.03.15";
        public override string Author => "Axeia";
        public override Uri AuthorUrl => new("https://github.com/Axeia/qBittorrentCompanion");
        public override string ToolTip => "For typical episode naming schemes like \n '[SubGroup] Anime name - 05 (1080p)[AE5AF6].mkv'";
        public override string Description => "Finds an episode number (expected format listed below) and will match episodes matching the same pattern" +
            "<br/> <ul>" +
            "<li>'S05E08', 'S1E105'</li>" +
            "<li>'- 08', '- 09v2'</li>" +
            "</ul><br/>" +
            "<i>Note: File naming has to be very consistent and not deviate in spacing or capitilization</i>";

        [GeneratedRegex(@"^(?<Prefix>.*)(?<EpNumber>(?:(?:s|S)(?:[0-9]{1,3})(?:e|E)(?:[0-9]{1,3})|\-(?: |\.)[0-9]{1,3})(?:(?:V|v)[0-9])?)(?<Suffix>.*)$")]
        /// <summary>
        /// Tries to match episode numbering.
        /// What qualifies episode numbering? A dash followed by a space or period followed by:
        /// 1 to 3 numbers (e.g. 001, 05, 3) [optionally followed by version number e.g. 01v2 05V3]<br/>
        /// or the above but instead of the dash preceeded by something like S03E<br/>
        /// (basically the letter S followed by 1 to 3 numbers followed by the letter E (case insensitive)<br/>
        /// </summary>
        private static partial Regex EpisodeNumberingRegex();

        [GeneratedRegex(@"(\\\[(?:[a-fA-F0-9]{8})\])(?:\\\.[a-zA-Z0-9]{1,8})?$")]
        private static partial Regex HashCodeRegex();

        public override PluginResult ProcessTarget(string target)
        {
            if (string.IsNullOrEmpty(target))
                return PluginResult.Error("Target cannot be empty");

            var match = EpisodeNumberingRegex().Match(target);
            if (!match.Success)
                return PluginResult.Error("Unable to find anything resembling an episode number");

            try
            {
                var regexPattern = BuildRegexPattern(match);
                var ruleTitle = ExtractRuleTitle(match);
                var info = GetInfoMessage(regexPattern);

                // Test that the generated regex is valid
                _ = new Regex(regexPattern);

                return string.IsNullOrEmpty(info)
                    ? PluginResult.Success(regexPattern, ruleTitle)
                    : PluginResult.Success(regexPattern, ruleTitle, info);
            }
            catch (Exception)
            {
                return PluginResult.Error($"Plugin caused an internal error. Please contact the developer at {AuthorUrl}");
            }
        }

        private static string BuildRegexPattern(Match match)
        {
            var escapedPrefix = Regex.Escape(match.Groups["Prefix"].Value);
            var escapedSuffix = Regex.Escape(match.Groups["Suffix"].Value);

            string epNumber = match.Groups["EpNumber"].Value;
            string epNumberRegex = string.Empty;

            if (epNumber.StartsWith('-'))
            {
                // Handle dash-style episode numbering like "- 08" or "- 09v2"
                epNumberRegex = $"\\-\\{epNumber[1]}";
            }
            else
            {
                // Handle season/episode style like "S05E08" or "s1e105"
                epNumberRegex += epNumber[0] == 'S' ? 'S' : 's';
                epNumberRegex += "(?:[0-9]{1,3})";
                epNumberRegex += epNumber.Contains('E', StringComparison.Ordinal) ? 'E' : 'e';
            }

            // Add the episode number pattern and optional version suffix
            epNumberRegex += "(?:[0-9]{1,3})(?:(?:V|v)[0-9])?";

            // Combine all parts
            var fullRegex = "^" + escapedPrefix + epNumberRegex + escapedSuffix;

            // Handle hashcodes in the pattern
            fullRegex = ReplaceLiteralHashCodeWithRegex(fullRegex);

            // Add end anchor
            fullRegex += "$";

            // Personal preference to just show spaces rather than escaped spaces
            // Much cleaner look in what is presented to the user
            fullRegex = fullRegex.Replace("\\ ", " ");

            return fullRegex;
        }

        private static string ExtractRuleTitle(Match match)
        {
            return match.Groups["Prefix"].Value.TrimEnd('-').TrimEnd(' ');
        }

        private static string ReplaceLiteralHashCodeWithRegex(string inputRegex)
        {
            var hashCodeRegex = HashCodeRegex();
            var match = hashCodeRegex.Match(inputRegex);

            if (match.Success)
            {
                // Similar but not the same as HashCodeRegex (so can't simply .ToString())
                string replacement = @"\[(?:[a-fA-F0-9]{8})\](?:\.[a-zA-Z0-9]{1,8})?";
                inputRegex = hashCodeRegex.Replace(inputRegex, replacement);
            }

            return inputRegex;
        }

        private static string GetInfoMessage(string regexPattern)
        {
            // Check if we found and handled a hashcode
            if (regexPattern.Contains(@"\[(?:[a-fA-F0-9]{8})\]"))
                return "Found what seems to be a hashcode and ensured the regex accommodates it";

            return string.Empty;
        }
    }
}