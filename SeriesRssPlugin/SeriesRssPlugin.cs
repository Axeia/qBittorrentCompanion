using RssPlugins;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SeriesRssPlugin
{
    public partial class SeriesRssPlugin(string target) : BaseRssPlugin(target)
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


        [GeneratedRegex(@"^(?<Prefix>.*)(?<EpNumber>(?:(?:s|S)(?:[0-9]{1,3})(?:e|E)(?:[0-9]{1,3})|\-(?: |\.)[0-9]{1,3})(?:(?:V|v)[0-9])?)(?<Postfix>.*)$")]
        /// <summary>
        /// Tries to match episode numbering.
        /// What qualifies episode numbering? A dash followed by a space or period followed by:
        /// 1 to 3 numbers (e.g. 001, 05, 3) [optionally followed by version number e.g. 01v2 05V3]<br/>
        /// or the above but instead of the dash preceeded by something like S03E<br/>
        /// (basically the letter S followed by 1 to 3 numbers followed by the letter E (case insensitive)<br/>
        /// </summary>
        private static partial Regex episodeNumbering();


        public override string ConvertToRegex()
        {
            IsSuccess = true;
            Debug.WriteLine("Let's goooo");
            var str = Target;
            string escapedRegex = "^";

            var episodeNumberRegEx = episodeNumbering();
            if (episodeNumberRegEx.Match(str) is Match matchE && matchE.Success)
            {
                escapedRegex += Regex.Escape(matchE.Groups[1].Value);
                var prefix = matchE.Groups[1].Value;
                RuleTitle = matchE.Groups[1].Value;
                var postFix = matchE.Groups[3].Value;

                string epNumber = matchE.Groups[2].Value;
                string epNumberRegex = string.Empty;
                if (epNumber.StartsWith('-'))
                    epNumberRegex = $"\\-\\{epNumber[1]}";
                else
                {
                    epNumberRegex += epNumber[0] == 'S' ? 'S' : 's';
                    epNumberRegex += "(?:[0-9]{1,3})";
                    epNumberRegex += epNumberRegex.Contains('E', StringComparison.Ordinal) ? 'E' : 'e'; 
                }

                epNumberRegex += "(?:[0-9]{1,3})(?:(?:V|v)[0-9])?)";

                // Ensure proper pattern extraction
                escapedRegex += Regex.Escape(prefix) + epNumberRegex + Regex.Escape(postFix);
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
                    Debug.WriteLine($"Unable to find episode number in: {Target}");
                    ErrorText = "Unable to find anything resembling an episode number";
                    IsSuccess = false;
                    return escapedRegex;
                }
            }

            escapedRegex += "$";

            //Final test if it actually works
           /* var testRegex = new Regex(escapedRegex);
            if (testRegex.Match(Target) is Match match)
            {
                Debug.WriteLine(Target);
                Debug.WriteLine(escapedRegex);
                Debug.WriteLine(match.Success);
            }*/

            // Personal preference to just show spaces rather than escaped spaces
            // Much cleaner look in what is presented to the user
            return escapedRegex.Replace("\\ ", " ");
        }
    }
}