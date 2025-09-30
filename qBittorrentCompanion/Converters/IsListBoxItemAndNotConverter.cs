using Avalonia.Data;
using Avalonia.Data.Converters;
using qBittorrentCompanion.ViewModels;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Determines whether a given list item (category or tag) is removable.
    /// Returns <c>false</c> for special items like "All" or "Uncategorised"/"Untagged".
    /// Used to control visibility or enablement of UI actions like delete/remove.
    /// </summary>
    public class IsListBoxAndNotConverter : IValueConverter
    {
        /// <summary>
        /// Category names that should not be removable.
        /// </summary>
        private readonly string[] _categoriesUnremovables = ["All", "Uncategorised"];

        /// <summary>
        /// Tag names that should not be removable.
        /// </summary>
        private readonly string[] _tagsUnremovables = ["All", "Untagged"];

        /// <summary>
        /// Returns <c>true</c> if the item is removable; <c>false</c> if it's a protected category or tag.
        /// </summary>
        /// <param name="value">Expected to be a <see cref="CategoryCountViewModel"/> or <see cref="TagCountViewModel"/>.</param>
        /// <param name="targetType">Irrelevant</param>
        /// <param name="parameter">Irrelevant</param>
        /// <param name="culture">Irrelevant</param>
        /// <returns><c>true</c> if the item is removable; otherwise <c>false</c>.</returns>
        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is null)
                return false;

            if (value is CategoryCountViewModel ccvm)
                return !_categoriesUnremovables.Contains(ccvm.Name);

            if (value is TagCountViewModel tagcvm)
                return !_tagsUnremovables.Contains(tagcvm.Tag);

            Debug.WriteLine($"IsListBoxAndNotConverter: Unexpected type {value.GetType()}");
            return true;
        }

        /// <summary>
        /// Not supported — returns <see cref="BindingOperations.DoNothing"/>.
        /// </summary>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            return BindingOperations.DoNothing;
        }
    }
}
