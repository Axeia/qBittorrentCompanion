using System.Text.RegularExpressions;

namespace FossRssPlugin
{
    public partial class FossRssPlugin(string target) : RssPlugins.RssRulePluginBase(target)
    {
        public override string Name => "FOSS";
        public override string Version => "v25.03.04.01";
        public override string Author => "Axeia";
        public override Uri AuthorUrl => new("https://github.com/Axeia/qBittorrentCompanion");
        public override string ToolTip => "Tries to create rules for names like using dot seperated version numbers or dates";
        public override string Description => "To be used for the RSS feed of <a href='https://fosstorrents.com/feed/torrents.xml'>fosstorrents.com</a> mostly but it might work elsewhere.<br/>"
            + "It attempts to find a version number and make that part dynamic in the rule it creates.";
        
        // Group 0 is the entire match (all the text)
        // Group "Prefix" is the name of the software, e.g. LibreOffice
        // Group "Version" is the version number - basically numbers seperated by dots
        // (up to a total of 3 dots doing this and the last dot could be a dash instead)
        // - Although uncommon the date can be used dot seperated as the version number making the first number 4 digits long (full year number), e.g "Melawy Linux 2025.03.02 - Base System (x86_64)"
        // Group "Suffix" is all the extra information, release platform, architecture that kind of stuff
        [GeneratedRegex(@"^(?<Prefix>.+ )(?<Version>[0-9]{1,4}\.[0-9]{1,2}(?:\.[0-9]{1,2})?(?<Optional>(?:[\.\-][0-9]{1,2}))?)(?<Suffix>.+)$")]
        private static partial Regex _typicalVersionNumberedRegex();

        // Group 0 is the entire match (all the text)
        // Group "Prefix" is the name of the software, e.g. LibreOffice
        // Group "Version" is the version number - a date is just 8 digits
        // Group "Suffix" is all the extra information, release platform, architecture that kind of stuff
        [GeneratedRegex(@"^(?<Prefix>.+ )(?<Version>[0-9]{8})(?<Suffix>.+)$")]
        private static partial Regex _dateVersionNumberedRegex();

        // CentOS is atypical with names like: "CentOS 10-20250226.0 - DVD (x86_64) (x86_64)" 
        [GeneratedRegex(@"^(?<Prefix>.+ )(?<Version>[0-9]{1,2}\-[0-9]{8}\.0)(?<Suffix>.+)$")]
        private static partial Regex _centOsVersionNumberedRegex();

        public override string ConvertToRegex()
        {
            ResetFieldsPreValidation();
            string escapedRegex = string.Empty;

            var typicalVersionNumberedRegex = _typicalVersionNumberedRegex();
            if (typicalVersionNumberedRegex.Match(Target) is Match matchTv && matchTv.Success)
            {
                RuleTitle = Target.Replace(matchTv.Groups["Version"].Value, "");
                return CreateRegex(matchTv, @"(?<Version>[0-9]{1,4}\.[0-9]{1,2}(?:\.[0-9]{1,2})?(?<Optional>(?:[\.\-][0-9]{1,2}))?)");
            }

            var dateVersionNumberedRegex = _dateVersionNumberedRegex();
            if (dateVersionNumberedRegex.Match(Target) is Match matchDv && matchDv.Success)
            {
                RuleTitle = Target.Replace(matchDv.Groups["Version"].Value, "");
                return CreateRegex(matchDv, @"(?<Version>[0-9]{8})");
            }

            var centOsVersionNumberedRegex = _centOsVersionNumberedRegex();
            if(centOsVersionNumberedRegex.Match(Target) is Match matchC && matchC.Success)
            {
                RuleTitle = Target.Replace(matchC.Groups["Version"].Value, "");
                return CreateRegex(matchC, @"(?<Version>[0-9]{1,2}\-[0-9]{8}\.0)");
            }
            else
            {
                // Debug.WriteLine($"Unable to find version number in: {Target}");
                ErrorText = "Unable to find anything resembling a version number or date";
                IsSuccess = false;
                return escapedRegex;
            }
        }

        private string CreateRegex(Match match, string versionString)
        {
            return "^"
                + Regex.Escape(match.Groups["Prefix"].Value)
                + versionString
                + Regex.Escape(match.Groups["Suffix"].Value)
                + "$";
        }
    }
}
