﻿using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Converters
{
    public class DoubleMatchToBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is double doubleValue && parameter is string radioButtonValue)
            {
                return doubleValue == double.Parse(radioButtonValue);
            }
            return false;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is bool isChecked && isChecked && parameter is string radioButtonValue)
            {
                return double.Parse(radioButtonValue);
            }
            return null;
        }
    }
}
