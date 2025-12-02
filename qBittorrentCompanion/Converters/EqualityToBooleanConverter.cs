using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Converts between an enum value and a boolean for use in UI bindings.<br/>
    /// Typically used to bind the <c>.IsChecked</c> state of a control to an enum selection.<br/>
    /// <br/>
    /// However it's not limited to enums and will work on anything .equals, like for example Strings.
    /// <br/>
    /// Need to bind to multiple values? <see cref="EqualityToBooleanMultiConverter"/>
    /// </summary>
    /// <remarks>
    /// Basically the intended use is to be combined with binding <c>.IsChecked</c> on:
    /// <list type="bullet">
    /// <item><seealso cref="Avalonia.Controls.CheckBox"/></item>
    /// <item><seealso cref="Avalonia.Controls.RadioButton"/></item>
    /// <item><seealso cref="Avalonia.Controls.ToggleSplitButton"/></item>
    /// <item><seealso cref="Avalonia.Controls.ToggleSwitch"/></item>
    /// </list>
    /// </remarks>
    public class EqualityToBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Returns <c>true</c> if the enum value equals the provided parameter.
        /// </summary>
        /// <param name="value">The current enum value.</param>
        /// <param name="targetType">Irrelevant</param>
        /// <param name="parameter">The enum value to compare against.</param>
        /// <param name="culture">Irrelevant</param>
        /// <returns><c>true</c> if <paramref name="value"/> equals <paramref name="parameter"/>; otherwise <c>false</c>.</returns>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is not null && parameter is not null && value.Equals(parameter);
        }

        /// <summary>
        /// Returns the value (from <paramref name="parameter"/>) if the bound boolean is <c>true</c>.
        /// </summary>
        /// <param name="value">The boolean value from the UI.</param>
        /// <param name="targetType">Irrelevant</param>
        /// <param name="parameter">The enum value to return if <paramref name="value"/> is <c>true</c>.</param>
        /// <param name="culture">Irrelevant</param>
        /// <returns>The enum value if <paramref name="value"/> is <c>true</c>; otherwise <see cref="BindingOperations.DoNothing"/>.</returns>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value?.Equals(true) == true && parameter is not null
                ? parameter
                : BindingOperations.DoNothing;
        }
    }
}
