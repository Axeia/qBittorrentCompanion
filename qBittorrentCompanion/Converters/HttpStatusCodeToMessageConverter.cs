using Avalonia.Data.Converters;
using Avalonia.Data;
using System;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Converts an HTTP status code (as <see cref="int"/>) into a human-readable message.
    /// Adds spacing to PascalCase enum names (e.g., <c>NotFound</c> → <c>Not Found</c>),
    /// and handles special cases like <c>-1</c> or unknown codes.
    /// </summary>
    /// <remarks>
    /// Useful for displaying HTTP diagnostics in tooltips, logs, or UI status panels.
    /// </remarks>
    public partial class HttpStatusCodeToMessageConverter : IValueConverter
    {
        /// <summary>
        /// Adds space between lowercase-uppercase transitions (e.g., <c>NotFound</c> → <c>Not Found</c>).
        /// </summary>
        [GeneratedRegex("([a-z])([A-Z])")]
        private static partial Regex SpacePascalCaseRegex();

        /// <summary>
        /// Matches ALLCAPS enum names (e.g., <c>OK</c>, <c>IMUsed</c>) to avoid spacing.
        /// </summary>
        [GeneratedRegex(@"^[A-Z]+$")]
        private static partial Regex AllCapsRegex();

        /// <summary>
        /// Converts an HTTP status code to a formatted string like <c>404 - Not Found</c>.
        /// </summary>
        /// <param name="value">Expected to be an <see cref="int"/> representing an HTTP status code.</param>
        /// <param name="targetType">Irrelevant</param>
        /// <param name="parameter">Irrelevant</param>
        /// <param name="culture">Irrelevant</param>
        /// <returns>A formatted string describing the status code.</returns>
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
                        statusName = SpacePascalCaseRegex().Replace(statusName, "$1 $2");

                    return $"{code} - {statusName}";
                }

                return $"Unknown ({code})";
            }

            return "Invalid status";
        }

        /// <summary>
        /// Not supported — returns <see cref="BindingOperations.DoNothing"/>.
        /// </summary>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return BindingOperations.DoNothing;
        }
    }
}