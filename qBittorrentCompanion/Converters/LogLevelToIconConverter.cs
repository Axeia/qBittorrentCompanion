using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentIcons.Common;
using Splat;
using System;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    public class LogLevelToIconConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is LogLevel logLevel)
            {
                return logLevel switch
                {
                    LogLevel.Debug => Symbol.Bug,
                    LogLevel.Info => Symbol.Info,
                    LogLevel.Warn => Symbol.Warning,
                    LogLevel.Error => Symbol.ErrorCircle,
                    LogLevel.Fatal => Symbol.ShieldError,
                    _ => Symbol.QuestionCircle
                };
            }
            else
                return Symbol.QuestionCircle;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => BindingOperations.DoNothing;
    }
}
