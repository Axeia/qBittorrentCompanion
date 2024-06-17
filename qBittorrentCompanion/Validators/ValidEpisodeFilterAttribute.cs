using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace qBittorrentCompanion.Validators
{
    public class ValidEpisodeFilterAttribute : ValidationAttribute
    {
        /// <summary>
        /// <list type="table">
        /// <listheader>
        /// <term>Regex Part</term>
        /// <header>Description</header>
        /// </listheader>
        /// <item>
        /// <term><code>[0-9]{0,2}[1-9]</code></term>
        /// <description>Match a positive non-zero number up to 3 digits (this is the season number)</description>
        /// </item>
        /// <item>
        /// <term><code>x</code></term>
        /// <description>Matches the letter 'x' - Seperates the season from the episode numbering.</description>
        /// </item>
        /// <item>
        /// <term><code>(([0-9]{1,4}(-[0-9]{1,4}|-|);)+)</code> </term>
        /// <description>
        /// Matches one or more occurrences of a positive non-zero number up to 4 digits (this is an episode number).
        /// <br/><b> <u>Optionally</u> followed by:</b>
        /// <list type="bullet">
        /// <item>a dash and another positive non-zero number up to 4 digits</item>
        /// <item>a dash alone.</item>
        /// </list>
        /// The <c>;</c> is <b>always</b> required. The <c>+</c> makes it optionally repeatable.
        /// </description>
        /// </item>
        /// </list>
        /// </summary>
        public static Regex EpisodeFilterRegex
        {
            get => new Regex(@"([0-9]{0,2}[1-9])x(([0-9]{1,4}(-[0-9]{1,4}|-|);)+)");
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string str)
            {
                if (!String.IsNullOrEmpty(str))
                {
                    if (!EpisodeFilterRegex.Match(str).Success)
                    {
                        return new ValidationResult("Not a valid episode filter. The field has a tooltip describing the format.");
                    }
                }
            }

            return ValidationResult.Success;
        }
    }
}
