using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace qBittorrentCompanion.Validators
{
    public class ValidTrackerUrlAttribute : ValidationAttribute
    {
        private static string[] _trackerStarts => ["https://", "http://", "udp://"];

        public static string GetTrackerValidationText(string url)
        {
            var urlLower = url.ToLower();
            if (!url.StartsWith(_trackerStarts[0]) && !url.StartsWith(_trackerStarts[1]) && !url.StartsWith(_trackerStarts[2]))
            {
                return "Tracker URLs have to start with http://, https:// or udp://.";
            }
            else if (_trackerStarts.Contains(urlLower))
            {
                return "Tracker URL needs to point to some location.";
            }
            else if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                return "Tracker could not be recognized as URL, invalid characters?";
            }

            return string.Empty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            string? url = null;

            if (value is string str)
                url = str;
            if (value is Uri uri)
                url = uri.ToString();

            if (url != null)
            {
                string message = GetTrackerValidationText(url);
                if (message == string.Empty)
                    return ValidationResult.Success;
                else
                    return new ValidationResult(message);
            }
            else
            {
                return new ValidationResult("Cannot be null");
            }
        }
    }
}
