using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentIcons.Common;
using Splat;
using System;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Converts a <see cref="LogLevel"/> into a corresponding <see cref="Symbol"/> icon.
    /// Used to visually represent log severity in the UI.
    /// </summary>
    /// <remarks>
    /// Maps log levels to FluentIcons:
    /// <list type="bullet">
    /// <item><c>Debug</c> → <see cref="Symbol.Bug"/></item>
    /// <item><c>Info</c> → <see cref="Symbol.Info"/></item>
    /// <item><c>Warn</c> → <see cref="Symbol.Warning"/></item>
    /// <item><c>Error</c> → <see cref="Symbol.ErrorCircle"/></item>
    /// <item><c>Fatal</c> → <see cref="Symbol.ShieldError"/></item>
    /// </list>
    /// Falls back to <see cref="Symbol.QuestionCircle"/> for unknown or null values.
    /// </remarks>
    public class LogLevelToIconConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="LogLevel"/> to a <see cref="Symbol"/> icon.<br/>
        /// Falls back to <see cref="Symbol.QuestionCircle"/> for unknown values.
        /// </summary>
        /// <param name="value">Expected to be a <see cref="LogLevel"/>.</param>
        /// <param name="targetType">Irrelevant</param>
        /// <param name="parameter">Irrelevant</param>
        /// <param name="culture">Irrelevant</param>
        /// <returns>A <see cref="Symbol"/> representing the log level.</returns>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is LogLevel logLevel
                ? logLevel switch
                {
                    LogLevel.Debug => Symbol.Bug,
                    LogLevel.Info => Symbol.Info,
                    LogLevel.Warn => Symbol.Warning,
                    LogLevel.Error => Symbol.ErrorCircle,
                    LogLevel.Fatal => Symbol.ShieldError,
                    _ => Symbol.QuestionCircle
                }
                : Symbol.QuestionCircle;
        }

        /// <summary>
        /// Not supported — returns <see cref="BindingOperations.DoNothing"/>.
        /// </summary>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => BindingOperations.DoNothing;
    }
}
