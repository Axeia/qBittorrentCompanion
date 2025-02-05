using System;

namespace qBittorrentCompanion.RssPlugins
{
    public interface IRssPlugin
    {
        /// <summary>
        /// Your name, e.g. "John Smith" 
        /// </summary>
        string Author { get; }
        /// <summary>
        /// Link to your website, or Github page. Preferably where this plugin can be downloaded
        /// </summary>
        public Uri AuthorUrl { get; }
        /// <summary>
        /// Long description shown that's shown when no RSS article is selected
        /// A subset of HTML is supported (lists, linebreaks and hyperlinks)
        /// </summary>
        string Description { get; }
        /// <summary>
        /// A short description to show when hovering over the button
        /// </summary>
        string ToolTip { get; }

        /// <summary>
        /// The constructor of your class implementing this should set this, 
        /// it's what to test on.
        /// </summary>
        string ToTestOn { get; }
        /// <summary>
        /// The result (the Regex generated) by the call to ConvertToRegex
        /// </summary>
        string Result { get; }
        /// <summary>
        /// The title, could be "Your rule name" or a part you extract from ToTestOn
        /// </summary>
        string RuleTitle { get; }
        /// <summary>
        /// During the ConvertToRegex() process if something goes wrong
        /// (e.g. the rule isn't the expected format) set this to false.
        /// The UI will display the long description and the author name/url whilst it's false
        /// </summary>
        bool IsSuccess { get; }
        /// <summary>
        /// Abstract method that inheriting classes must implement
        /// It should set IsSuccess and Result and you may wish to set RuleTitle to part of ToTestOn
        /// </summary>
        /// <returns></returns>
        abstract string ConvertToRegex();
    }
}
