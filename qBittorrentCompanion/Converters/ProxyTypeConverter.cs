using Avalonia.Data;
using Avalonia.Data.Converters;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using System;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Converts a <see cref="ProxyType"/> enum value into a user-friendly description string,
    /// and vice versa. Used for UI bindings where proxy types are displayed or selected.
    /// </summary>
    /// <remarks>
    /// Descriptions are centralized in <see cref="DataConverter.ProxyTypeDescriptions"/>.
    /// This converter supports both forward and reverse mapping.
    /// </remarks>
    public class ProxyTypeConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="ProxyType"/> to its description string.
        /// </summary>
        /// <param name="value">Expected to be a <see cref="ProxyType"/>.</param>
        /// <param name="targetType">Irrelevant</param>
        /// <param name="parameter">Irrelevant</param>
        /// <param name="culture">Irrelevant</param>
        /// <returns>A description string for the proxy type.</returns>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            return value is ProxyType proxyType
                ? proxyType switch
                {
                    ProxyType.None => DataConverter.ProxyTypeDescriptions.None,
                    ProxyType.Http => DataConverter.ProxyTypeDescriptions.Http,
                    ProxyType.Socks5 => DataConverter.ProxyTypeDescriptions.Socks5,
                    ProxyType.HttpAuth => DataConverter.ProxyTypeDescriptions.HttpAuth,
                    ProxyType.Socks5Auth => DataConverter.ProxyTypeDescriptions.Socks5Auth,
                    ProxyType.Socks4 => DataConverter.ProxyTypeDescriptions.Socks4,
                    _ => throw new ArgumentOutOfRangeException(nameof(value), proxyType, "Unknown proxy type")
                }
                : DataConverter.ProxyTypeDescriptions.None;
        }

        /// <summary>
        /// Converts a description string back into a <see cref="ProxyType"/>.
        /// </summary>
        /// <param name="value">Expected to be a description string.</param>
        /// <param name="targetType">Irrelevant</param>
        /// <param name="parameter">Irrelevant</param>
        /// <param name="culture">Irrelevant</param>
        /// <returns>The corresponding <see cref="ProxyType"/>, or throws if unknown.</returns>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is string str)
            {
                return str switch
                {
                    var s when s == DataConverter.ProxyTypeDescriptions.None => ProxyType.None,
                    var s when s == DataConverter.ProxyTypeDescriptions.Http => ProxyType.Http,
                    var s when s == DataConverter.ProxyTypeDescriptions.Socks5 => ProxyType.Socks5,
                    var s when s == DataConverter.ProxyTypeDescriptions.HttpAuth => ProxyType.HttpAuth,
                    var s when s == DataConverter.ProxyTypeDescriptions.Socks5Auth => ProxyType.Socks5Auth,
                    var s when s == DataConverter.ProxyTypeDescriptions.Socks4 => ProxyType.Socks4,
                    _ => throw new ArgumentOutOfRangeException(nameof(value), str, "Unknown proxy description")
                };
            }

            return BindingOperations.DoNothing;
        }
    }
}
