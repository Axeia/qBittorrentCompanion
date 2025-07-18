using Avalonia.Data.Converters;
using Avalonia.Data;
using System;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace qBittorrentCompanion.Converters
{
    public partial class HttpStatusCodeToMessageConverter : IValueConverter
    {
        // Adds space between lowercase-uppercase transitions (e.g., NotFound → Not Found)
        [GeneratedRegex("([a-z])([A-Z])")]
        private static partial Regex SpacePascalCaseRegex();

        [GeneratedRegex(@"^[A-Z]+$")]
        private static partial Regex AllCapsRegex();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int code)
            {
                if (code == -1)
                    return $"{code} - Could not send request";

                if (Enum.IsDefined(typeof(HttpStatusCode), code))
                {
                    var statusName = ((HttpStatusCode)code).ToString();

                    // If not ALL CAPS, apply spacing (e.g., NotFound → Not Found)
                    if (!AllCapsRegex().IsMatch(statusName))
                    {
                        statusName = SpacePascalCaseRegex().Replace(statusName, "$1 $2");
                    }

                    return $"{code} - {statusName}";
                }

                return $"Unknown ({code})";
            }

            return "Invalid status";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return BindingOperations.DoNothing;
        }
    }
}
