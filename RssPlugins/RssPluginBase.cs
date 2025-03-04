namespace RssPlugins
{
    public abstract class RssPluginBase
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
        /// <summary>
        /// The constructor of your class implementing this should set this, 
        /// it's what to test on. \n is supported to create a new line
        /// </summary>
        public string Target { get; protected set; } = "";
        /// <summary>
        /// The result (the Regex generated) by the call to ConvertToRegex
        /// </summary>
        public string Result { get; protected set; } = "";
        /// <summary>
        /// The title, could be "Your rule name" or a part you extract from ToTestOn
        /// </summary>
        public string RuleTitle { get; protected set; } = "";
        /// <summary>
        /// During the ConvertToRegex() process if something goes wrong
        /// (e.g. the rule isn't the expected format) set this to false.
        /// The UI will display the long description and the author name/url whilst it's false
        /// </summary>
        public bool IsSuccess { get; protected set; } = true;
        /// <summary>
        /// Most severe message to display to the user, likely in a different color to stand out
        /// Text to display if the user provides a target that cannot be parsed.
        /// Could be as simple as "Could not process input" or more specific like 
        /// "Found x and y but not z"
        /// </summary>
        public string ErrorText { get; protected set; } = "Could not process input";
        /// <summary>
        /// A heads up kind of message to the user to double check the output is as expected.
        /// e.g. "Could not find an episode number reliably, double check output"
        /// </summary>
        public string WarningText { get; protected set; } = "";
        /// <summary>
        /// The least severe kind of message
        /// </summary>
        public string InfoText { get; protected set; } = "";

        /// <summary>
        /// Use <see cref="ConvertToRegex"/> to set <see cref="Result"> and <see cref="RuleTitle"/>
        /// </summary>
        /// <param name="target"></param>
        public RssPluginBase(string target)
        {
            RevalidateOn(target);
        }

        /// <summary>
        /// Can be used to change <see cref="Target"/> after initialisation 
        /// and will rerun <see cref="ConvertToRegex"/>
        /// </summary>
        /// <param name="target"></param>
        public void RevalidateOn(string target)
        {
            Target = target;
            Result = ConvertToRegex();
        }

        /// <summary>
        /// Distills <see cref="Target"/> into a regular expression and stores it in <see cref="Result"/>
        /// if it fails to do so set <see cref="IsSuccess"/> to false and provide a description of why
        /// it didn't succeed in <see cref="ErrorText"/>
        /// </summary>
        /// <returns></returns>
        public abstract string ConvertToRegex();

        /// <summary>
        /// Resets <see cref="ErrorText"/> and <see cref="IsSuccess"/>
        /// </summary>
        public void ResetFieldsPreValidation()
        {
            ErrorText = string.Empty;
            IsSuccess = true;
        }
    }
}
