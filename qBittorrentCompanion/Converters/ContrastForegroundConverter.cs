using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using System;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Simple but effective, give it a brush and it will return a white or black brush, whichever has the highest contrast
    /// </summary>
    public class ContrastForegroundConverter : IValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">Should be <see cref="IBrush"/></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns><see cref="Brushes.White"/> or <see cref="Brushes.Black"/></returns>
        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is IBrush brush)
            {
                Color color;

                if (brush is ImmutableSolidColorBrush immutableBrush)
                {
                    color = immutableBrush.Color;
                }
                else if (brush is SolidColorBrush solidBrush)
                {
                    color = solidBrush.Color;
                }
                else
                {
                    return Brushes.Black;
                }

                var brightness = (color.R * 299 + color.G * 587 + color.B * 114) / 1000;
                return brightness > 128 ? Brushes.Black : Brushes.White;
            }
            return Brushes.Black;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            return BindingOperations.DoNothing;
        }

    }
}
