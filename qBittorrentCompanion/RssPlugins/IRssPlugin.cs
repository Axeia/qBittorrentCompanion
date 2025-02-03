namespace qBittorrentCompanion.RssPlugins
{
    public interface IRssPlugin
    {
        string Result { get; }
        string RuleTitle { get; }

        string ToTestOn { get; }
        bool IsSuccess { get; }

        /// <summary>
        /// Description should preferably be relatively short
        /// </summary>
        string Description { get; }
        /// <summary>
        /// Your name, "John Smith" or something along those lines
        /// </summary>
        string Author { get; }

        // Abstract method that inheriting classes must implement
        string ConvertToRegex();
    }
}
