using Avalonia.Data.Converters;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using System;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Converts between <see cref="DynamicDnsService"/> enum values and user-facing string labels
    /// defined in <see cref="DataConverter.DnsServices"/>. Used for UI display and binding.
    /// </summary>
    public class DynamicDnsServiceConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="DynamicDnsService"/> enum value into a descriptive string label.
        /// </summary>
        /// <param name="value">The enum value to convert (expected to be of type <see cref="DynamicDnsService"/>)</param>
        /// <param name="targetType">The target binding type (ignored)</param>
        /// <param name="parameter">Optional parameter (ignored)</param>
        /// <param name="culture">Culture info (ignored)</param>
        /// <returns>
        /// A string label from <see cref="DataConverter.DnsServices"/> matching the enum value,
        /// or <c>"None"</c> as fallback.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the enum value is not recognized.
        /// </exception>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is DynamicDnsService dnsService)
            {
                return dnsService switch
                {
                    DynamicDnsService.None => DataConverter.DnsServices.None,
                    DynamicDnsService.DynDNS => DataConverter.DnsServices.DynDns,
                    DynamicDnsService.NoIP => DataConverter.DnsServices.NoIp,
                    _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unsupported DynamicDnsService enum value.")
                };
            }

            return DataConverter.DnsServices.None;
        }

        /// <summary>
        /// Converts a string label back into its corresponding <see cref="DynamicDnsService"/> enum value.
        /// </summary>
        /// <param name="value">The string label to convert (expected to match one of the values in <see cref="DataConverter.DnsServices"/>)</param>
        /// <param name="targetType">The target binding type (ignored)</param>
        /// <param name="parameter">Optional parameter (ignored)</param>
        /// <param name="culture">Culture info (ignored)</param>
        /// <returns>
        /// A matching <see cref="DynamicDnsService"/> enum value,
        /// or <c>DynamicDnsService.None</c> as fallback.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the string does not match any known label.
        /// </exception>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is string str)
            {
                if (str == DataConverter.DnsServices.None)
                    return DynamicDnsService.None;
                if (str == DataConverter.DnsServices.DynDns)
                    return DynamicDnsService.DynDNS;
                if (str == DataConverter.DnsServices.NoIp)
                    return DynamicDnsService.NoIP;

                throw new ArgumentOutOfRangeException(nameof(value), value, "Unsupported DNS service string.");
            }

            return DynamicDnsService.None;
        }
    }
}
