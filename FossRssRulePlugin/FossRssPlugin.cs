using RssPlugins;
using System.Text.RegularExpressions;

namespace FossRssPlugin
{
    public partial class FossRssPlugin : RssRulePluginBase
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

        public override PluginResult ProcessTarget(string target)
        {
            var typicalVersionNumberedRegex = _typicalVersionNumberedRegex();
            if (typicalVersionNumberedRegex.Match(target) is Match matchTv && matchTv.Success)
            {
                string ruleTitle = target.Replace(matchTv.Groups["Version"].Value, "");
                string returnRegex = CreateRegex(matchTv, @"(?<Version>[0-9]{1,4}\.[0-9]{1,2}(?:\.[0-9]{1,2})?(?<Optional>(?:[\.\-][0-9]{1,2}))?)");
                return PluginResult.Success(returnRegex, ruleTitle);
            }

            var dateVersionNumberedRegex = _dateVersionNumberedRegex();
            if (dateVersionNumberedRegex.Match(target) is Match matchDv && matchDv.Success)
            {
                string ruleTitle = target.Replace(matchDv.Groups["Version"].Value, "");
                string returnRegex = CreateRegex(matchDv, @"(?<Version>[0-9]{8})");
                return PluginResult.Success(returnRegex, ruleTitle);
            }

            var centOsVersionNumberedRegex = _centOsVersionNumberedRegex();
            if(centOsVersionNumberedRegex.Match(target) is Match matchC && matchC.Success)
            {
                string ruleTitle = target.Replace(matchC.Groups["Version"].Value, "");
                string returnRegex = CreateRegex(matchC, @"(?<Version>[0-9]{1,2}\-[0-9]{8}\.0)");
                return PluginResult.Success(returnRegex, ruleTitle);
            }
                
            return PluginResult.Error("Unable to find anything resembling a version number or date");
        }

        private static string CreateRegex(Match match, string versionString)
        {
            return "^"
                + Regex.Escape(match.Groups["Prefix"].Value)
                + versionString
                + Regex.Escape(match.Groups["Suffix"].Value)
                + "$";
        }
    }
}
