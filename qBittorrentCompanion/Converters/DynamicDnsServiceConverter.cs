using Avalonia.Data.Converters;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using System;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    public class DynamicDnsServiceConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is DynamicDnsService dnsService)
            {
                return dnsService switch
                {
                    DynamicDnsService.None => DataConverter.DnsServices.None,
                    DynamicDnsService.DynDNS => DataConverter.DnsServices.DynDns,
                    DynamicDnsService.NoIP => DataConverter.DnsServices.NoIp,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            return null;
        }
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

                throw new ArgumentOutOfRangeException();
            }
            return null;
        }
    }
}
