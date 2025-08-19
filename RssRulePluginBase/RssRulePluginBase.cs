namespace RssPlugins
{
    public class PluginResult
    {
        public bool IsSuccess { get; init; }
        public string RegexPattern { get; init; } = string.Empty;
        public string RuleTitle { get; init; } = string.Empty;
        public string ErrorMessage { get; init; } = string.Empty;
        public string WarningMessage { get; init; } = string.Empty;
        public string InfoMessage { get; init; } = string.Empty;

        public static PluginResult Success(string regexPattern, string ruleTitle, string? info = null)
            => new() { IsSuccess = true, RegexPattern = regexPattern, RuleTitle = ruleTitle, InfoMessage = info ?? string.Empty };

        public static PluginResult Error(string errorMessage)
            => new() { IsSuccess = false, ErrorMessage = errorMessage };

        public static PluginResult Warning(string regexPattern, string ruleTitle, string warningMessage)
            => new() { IsSuccess = true, RegexPattern = regexPattern, RuleTitle = ruleTitle, WarningMessage = warningMessage };
    }

    public abstract class RssRulePluginBase
    {
        /// <summary>
        /// Displayed as the 'tag' to be selected in a dropdown and as part of the generate button
        /// </summary>
        public abstract string Name { get; }
        /// <summary>
        /// Version number, any format is allowed - it isn't used for anything other than indicating to the 
        /// user what the version of this plugin is
        /// </summary>
        public abstract string Version { get; }
        /// <summary>
        /// Your name, nickname, username, whatever you want to use. e.g. "John Smith" 
        /// </summary>
        public abstract string Author { get; }
        /// <summary>
        /// Link to your website, or Github page. Preferably where this plugin can be downloaded
        /// </summary>
        public abstract Uri AuthorUrl { get; }
        /// <summary>
        /// Long description you can use to describe how this plugin is supposed to work
        /// A subset of HTML is supported (lists, linebreaks and hyperlinks)
        /// </summary>
        public abstract string Description { get; }
        /// <summary>
        /// A short description to show when hovering over the button
        /// </summary>
        public abstract string ToolTip { get; }

        public abstract PluginResult ProcessTarget(string target);
    }
}
