using RssPlugins;
using System;
using System.Diagnostics;
namespace qBittorrentCompanion.Helpers
{
    public class RssRuleWizard : RssRulePluginBase
    {
        public override string Name => "Wizard";

        public override string Version => "v25.08.03.30";

        public override string Author => "Axeia";

        public override Uri AuthorUrl => new ("https://github.com/Axeia/qBittorrentCompanion");
        public override string ToolTip => "Customizable regex creation made easy";
        public override string Description => "The wizard helps you write a regex the easy way<br/>" +
            "<ol>" +
            "<li>Select a torrent or enter text in the input field</li>" +
            "<li>Select the part you would like to make dynamic</li>" +
            "<li>Hit the 'Regexify' button</li>" +
            "<li>Write your own regular expression or select one of the premade options</li>"+
            "</ol>";

        /// <summary>
        /// Not actually to be used directly, use SetTargetData instead
        /// Just allows the wizard to slot into the existing plugin system
        /// </summary>
        /// <param name="regexPattern"></param>
        /// <returns></returns>
        public override PluginResult ProcessTarget(string regexPattern)
        {
            return PluginResult.Success(_regexPattern, _title);
        }

        private string _title = string.Empty;
        private string _regexPattern = string.Empty;
        public void SetTargetData(string regexPattern, string title)
        {
            _regexPattern = regexPattern;
            _title = title;
        }
    }
}
