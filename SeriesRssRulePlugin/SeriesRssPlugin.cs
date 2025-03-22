using RssPlugins;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SeriesRssPlugin
{
    public partial class SeriesRssPlugin(string target) : RssRulePluginBase(target)
    {
        public override string Name => "Series";
        public override string Version => "v25.02.07.22";
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
        private static partial Regex episodeNumberingRegex();


        public override string ConvertToRegex()
        {
            ResetFieldsPreValidation();
            var str = Target;
            string escapedRegex = string.Empty;

            var episodeNumberRegEx = episodeNumberingRegex();
            if (episodeNumberRegEx.Match(str) is Match matchE && matchE.Success)
            {
                escapedRegex = "^"; // Add match to start of line
                var escapedPrefix = Regex.Escape(matchE.Groups["Prefix"].Value);
                RuleTitle = matchE.Groups[1].Value.TrimEnd('-').TrimEnd(' ');
                var escapedSuffix = Regex.Escape(matchE.Groups["Suffix"].Value);

                string epNumber = matchE.Groups[2].Value;
                string epNumberRegex = string.Empty;
                if (epNumber.StartsWith('-'))
                    epNumberRegex = $"\\-\\{epNumber[1]}";
                else
                {
                    epNumberRegex += epNumber[0] == 'S' ? 'S' : 's';
                    epNumberRegex += "(?:[0-9]{1,3})";
                    epNumberRegex += epNumber.Contains('E', StringComparison.Ordinal) ? 'E' : 'e'; 
                }

                epNumberRegex += "(?:[0-9]{1,3})(?:(?:V|v)[0-9])?";

                // Ensure proper pattern extraction
                escapedRegex += escapedPrefix + epNumberRegex + escapedSuffix;
                escapedRegex = ReplaceLiteralHashCodeWithRegex(escapedRegex);
            }
            else
            {
                if(false) // Search for all numbers and try to determine which one is the most likely episode number
                {
                    WarningText = "Multiple numbers that could be an episode number detected, " +
                        "made an assumption as to which is the right one";
                }
                else
                { 
                    //Debug.WriteLine($"Unable to find episode number in: {Target}");
                    ErrorText = "Unable to find anything resembling an episode number";
                    IsSuccess = false;
                    return escapedRegex;
                }
            }

            escapedRegex += "$"; // Add match to end of line

            // Personal preference to just show spaces rather than escaped spaces
            // Much cleaner look in what is presented to the user
            escapedRegex = escapedRegex.Replace("\\ ", " ");

            try
            {
                var checkRegex = new Regex(escapedRegex);
            }
            catch (Exception ex)
            {
                ErrorText = $"Plugin caused an internal error. Please contact the developer at {AuthorUrl}";
                IsSuccess = false;
                return escapedRegex;
            }

            return escapedRegex;
        }

        [GeneratedRegex(@"(\\\[(?:[a-fA-F0-9]{8})\])(?:\\\.[a-zA-Z0-9]{1,8})?$")]
        private static partial Regex HashCodeRegex();

        private string ReplaceLiteralHashCodeWithRegex(string inpRegex)
        {
            var hashCodeRegex = HashCodeRegex();
            if (hashCodeRegex.Match(inpRegex) is Match match && match.Success)
            {
                InfoText = "Found what seems to be a hashcode and ensured the regex accommodates it";

                //Similiar but not the same as HashCodeRegex (so can't simply .ToString()
                string replacement = @"\[(?:[a-fA-F0-9]{8})\](?:\.[a-zA-Z0-9]{1,8})?";
                string trimmedReplacement = replacement.Substring(0, replacement.Length);
                inpRegex = hashCodeRegex.Replace(inpRegex, trimmedReplacement);

                return inpRegex;
            }

            return inpRegex;
        }
    }
}