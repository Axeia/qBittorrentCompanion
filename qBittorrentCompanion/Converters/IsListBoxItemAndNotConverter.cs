using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using qBittorrentCompanion.ViewModels;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace qBittorrentCompanion.Converters
{
    public class IsListBoxAndNotConverter : IValueConverter
    {
        private string[] _categoriesUnremovables = ["All", "Uncategorised"];
        private string[] _tagsUnremovables = ["All", "Untagged"];

        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is null)
                return false;
            else if (value is CategoryCountViewModel ccvm)
                return !_categoriesUnremovables.Contains(ccvm.Name);
            else if (value is TagCountViewModel tagcvm)
                return !_tagsUnremovables.Contains(tagcvm.Tag);
            else
                Debug.WriteLine($"??? {value.GetType()}");

            return true;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            return BindingOperations.DoNothing;
        }

    }
}
