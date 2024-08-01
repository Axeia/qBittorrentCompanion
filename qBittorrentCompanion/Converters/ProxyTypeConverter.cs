using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Converters
{
    public class ProxyTypeConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is ProxyType proxyType)
            {
                return proxyType switch
                {
                    ProxyType.None => DataConverter.ProxyTypeDescriptions.None,
                    ProxyType.Http => DataConverter.ProxyTypeDescriptions.Http,
                    ProxyType.Socks5 => DataConverter.ProxyTypeDescriptions.Socks5,
                    ProxyType.HttpAuth => DataConverter.ProxyTypeDescriptions.HttpAuth,
                    ProxyType.Socks5Auth => DataConverter.ProxyTypeDescriptions.Socks5Auth,
                    ProxyType.Socks4 => DataConverter.ProxyTypeDescriptions.Socks4,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            return null;
        }
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is string str)
            {
                if (str == DataConverter.ProxyTypeDescriptions.None)
                    return ProxyType.None;
                if (str == DataConverter.ProxyTypeDescriptions.Http)
                    return ProxyType.Http;
                if (str == DataConverter.ProxyTypeDescriptions.Socks5)
                    return ProxyType.Socks5;
                if (str == DataConverter.ProxyTypeDescriptions.HttpAuth)
                    return ProxyType.HttpAuth;
                if (str == DataConverter.ProxyTypeDescriptions.Socks5Auth)
                    return ProxyType.Socks5Auth;
                if (str == DataConverter.ProxyTypeDescriptions.Socks4)
                    return ProxyType.Socks4;

                throw new ArgumentOutOfRangeException();
            }
            return null;
        }
    }
}
