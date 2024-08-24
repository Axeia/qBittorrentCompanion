using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Converters
{
    public class ContrastForegroundConverter : IValueConverter
    {
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
