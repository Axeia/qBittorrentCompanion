using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace qBittorrentCompanion.RssPlugins
{
    public partial class AnimeEpisodeRssPlugin : IRssPlugin
    {
        public bool IsSuccess { get; private set; } = true;
        public string ToTestOn { get; private set; } = "";
        public string Result { get; private set; } = "";
        public string RuleTitle { get; private set; } = "";
        public string ToolTip => "For typical anime naming schemes like [SubGroup] Anime name - 05 (1080p)[AE5AF6].mkv<br>";
        public string Description => "Attempts to match typical anime naming schemes by creating a rule that tries to match: <br><ul>" +
            "<li>Everything before the episode numbering dash</li>" +
            "<li>An episode number either in the format ' - S01E05' or ' - 05' depending on which is applicable</li>" +
            "<li>Resolution (e.g. 480p, 720p, 1080p) - if it's in brackets and includes other information for example like [1080p x264] it will try to match that as well</li>" +
            "<li>extension (e.g. .mp4 or .mkv) - but only if 2-4 characters long</li>" +
            "</ul>";
        public string Author => "Axeia";
        public Uri AuthorUrl => new("https://github.com/Axeia/qBittorrentCompanion");

        /// <summary>
        /// Tries to match the title by matching `.+` any character on more more times, followed by:
        /// a dash surrounded by spaces ` \- ` (Dashes have a special meaning and thus need to be escaped)
        /// followed by
        /// Any sequence of numbers of one to three numbers long (001, 1, 103 etc) 
        /// OPTIONALLY followed by a version number (capital v or big V followed by a single number)
        /// </summary>
        [GeneratedRegex(@"(.+) \- (?:[0-9]{1,3})(?:(?:v|V)2)? ")]
        private static partial Regex _titleWithEpisodeNumber();

        /// <summary>
        /// Similar to <see cref="_titleWithEpisodeNumber"/> but allows for a "S05" type of season
        /// prefix before it
        /// </summary>
        /// <returns></returns>
        [GeneratedRegex(@"(.+) \- (?:s|S)(?:[0-9]{1,})(?:e|E)(?:[0-9]{1,2})(?:(?:V|v)[0-9])? ")]
        private static partial Regex _titleWithSeasonAndEpisodeNumber();

        public AnimeEpisodeRssPlugin(string toTestOn)
        {
            ToTestOn = toTestOn;
            Result = ConvertToRegex();
        }

        public string ConvertToRegex()
        {
            var str = ToTestOn;
            string escapedRegex = "^";

            var titleWithEpisodeNumber = _titleWithEpisodeNumber();
            var titleWithSeasonAndEpisodeNumber = _titleWithSeasonAndEpisodeNumber();
            // Match up to the episode separator dash searching for something like ' - 02' (or - 02v1)
            if (titleWithEpisodeNumber.Match(str) is Match matchE && matchE.Success)
            {
                //Debug.WriteLine($"Found title {matchE.Groups[1].Value}");
                escapedRegex += Regex.Escape(matchE.Groups[1].Value);
                RuleTitle = matchE.Groups[1].Value;
                // Ensure proper pattern extraction
                escapedRegex += titleWithEpisodeNumber.ToString()[4..];
            }
            // Try matching up to something like ' - S05E01' (or S05E02)
            else if (titleWithSeasonAndEpisodeNumber.Match(str) is Match matchSe && matchSe.Success)
            {
                //Debug.WriteLine($"Found title {matchSe.Groups[1].Value}");
                escapedRegex += Regex.Escape(matchSe.Groups[1].Value);
                RuleTitle = matchSe.Groups[1].Value;
                // Ensure proper pattern extraction
                escapedRegex += titleWithSeasonAndEpisodeNumber.ToString()[4..];
            }
            else
            {
                Debug.WriteLine("Unable to find episode number");
                IsSuccess = false;
                return escapedRegex;
            }

            // Confirmed that it has both a title and episode number
            // Now cut them off so the rest can be processed
            Regex re = new(escapedRegex);
            if (re.Match(ToTestOn) is Match matchRe && matchRe.Success)
            {
                str = ToTestOn.Substring(matchRe.Value.Length);
            }
            else
            {
                Debug.WriteLine("Code somehow reached a match but then was unable to match");
            }

            // Try and find a resolution string
            string[] resolutions = ["480p", "720p", "1080p", "1440p", "2160p"];
            foreach (var resolution in resolutions)
            {
                // They don't always have a direction closing 
                string roundBracketed = $"({resolution}"; 
                string squareBracketed = $"[{resolution}";

                if(str.IndexOf(squareBracketed, System.StringComparison.OrdinalIgnoreCase) is int sResult && sResult > -1)
                {
                    string matchedResolution = str.Substring(sResult, roundBracketed.Length);
                    var regexAllBracketInfo = new Regex(Regex.Escape(matchedResolution) + @"[^\]]*\]");
                    if (regexAllBracketInfo.Match(str) is Match strRegexBracketInfo)
                    {
                        escapedRegex += ".*" + Regex.Escape(strRegexBracketInfo.ToString());
                    }
                }
                else if (str.IndexOf(roundBracketed, System.StringComparison.OrdinalIgnoreCase) is int rResult && rResult > -1)
                {
                    string matchedResolution = str.Substring(rResult, roundBracketed.Length);
                    var regexAllBracketInfo = new Regex(Regex.Escape(matchedResolution) + @"[^\)]*\)");
                    if (regexAllBracketInfo.Match(str) is Match strRegexBracketInfo)
                    {
                        escapedRegex += ".*" + Regex.Escape(strRegexBracketInfo.ToString());
                    }
                }
                else if (str.IndexOf(resolution, System.StringComparison.OrdinalIgnoreCase) is int baseResult && baseResult > -1)
                {
                    string rightCaseResolution = str.Substring(baseResult, resolution.Length);
                    escapedRegex += ".*" + Regex.Escape(rightCaseResolution); 
                }
            }

            if (GetFileExtension(str) is string ext)
                escapedRegex += Regex.Escape($".{ext}");

            //Final test if it actually works
            var testRegex = new Regex(escapedRegex);
            if (testRegex.Match(ToTestOn) is Match match)
            {
                Debug.WriteLine(ToTestOn);
                Debug.WriteLine(escapedRegex);
                Debug.WriteLine(match.Success);
            }

            // Personal preference to just show spaces rather than escaped spaces
            // Much cleaner look in what is presented to the user
            return escapedRegex.Replace("\\ ", " ");
        }

        /// <summary>
        /// This method isn't perfect.
        /// It basically looks if there's a dot since all extensions have at least that in common
        /// and then checks if everything behind that dot has the characteristics of a typical file extension
        /// It has to be 2-4 characters long and contain alpha-numeric characters exclusively.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string? GetFileExtension(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            int lastDotIndex = fileName.LastIndexOf('.');
            if (lastDotIndex == -1 || lastDotIndex == fileName.Length - 1)
                return null; // No dot found or dot is at the end

            string potentialExtension = fileName.Substring(lastDotIndex + 1);

            // Check if the potential extension has a typical length and contains only alphanumeric characters
            if (potentialExtension.Length >= 2 && potentialExtension.Length <= 4 && potentialExtension.All(char.IsLetterOrDigit))
                return potentialExtension;

            return null; 
        }

        public static void Test()
        {
            string[] testCases = [
                "[Erai-raws] Girumasu - 04 [720p CR WEBRip HEVC EAC3][MultiSub][290D6FAB].mkv",
                "[Judas] Class no Daikirai na Joshi to Kekkon suru Koto ni Natta (I'm Getting Married to a Girl I Hate in My Class) - S01E05 [1080p][HEVC x265 10bit][Multi-Subs]",
                "[Erai-raws] Girumasu - S5E01V3 [CR WEBRip HEVC EAC3][MultiSub][290D6FAB].mkv",
                "[Yameii] ZENSHU - S01E05 [English Dub] [CR WEB-DL 720p] [1550F7C0] (Zenshuu.)"
            ];

            foreach(var testCase in testCases)
            {
                var tmp = new AnimeEpisodeRssPlugin(testCase);
                tmp.ConvertToRegex();
            }
        }
    }
}
