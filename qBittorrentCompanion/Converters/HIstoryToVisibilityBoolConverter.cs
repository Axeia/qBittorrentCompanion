using Avalonia.Data;
using Avalonia.Data.Converters;
using qBittorrentCompanion.ViewModels.LocalSettings;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Determines the visibility of a history item based on if its intended for redo and undo and <br/>
    /// its <see cref="LogoColorsHistoryRecord.IsForRedo"/> &amp; <see cref="LogoColorsHistoryRecord.IsForUndo"/> properties
    /// </summary>
    public class HistoryToVisibilityBoolMultiConverter : IMultiValueConverter
    {
        /// <summary>
        /// Converts a history record and view mode into a visibility boolean.
        /// </summary>
        /// <param name="values">
        /// [0]=&gt;<see cref="LogoColorsHistoryRecord"/> the item being evaluated<br/>
        /// [1]=&gt;<see cref="bool"/> the view's <c>IsForRedo</c> flag
        /// </param>
        /// <param name="targetType">Irrelevant</param>
        /// <param name="parameter">Irrelevant</param>
        /// <param name="culture">Irrelevant</param>
        /// <returns>
        /// <c>true</c> if the item's flag matches the view mode; otherwise <c>false</c>.
        /// </returns>
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count == 2 &&
                values[0] is LogoColorsHistoryRecord lchr &&
                values[1] is bool isForRedo)
            {
                return isForRedo ? lchr.IsForRedo : lchr.IsForUndo;
            }

            return false;
        }

        public object ConvertBack(IList<object?> values, Type[] targetTypes, object? parameter, CultureInfo culture)
            => BindingOperations.DoNothing;
    }
}
