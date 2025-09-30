using Avalonia.Data;
using Avalonia.Data.Converters;
using QBittorrent.Client;
using System;
using System.Globalization;
using static qBittorrentCompanion.Helpers.DataConverter;

namespace qBittorrentCompanion.Converters
{
    /// <summary>
    /// Takes in a <see cref="ChokingAlgorithm"/> and returns a string <see cref="UploadSlotBehaviors"/>
    /// (or throws ArgumentOutOfRangeException if presented with something else)
    /// </summary>
    public class UploadSlotBehaviorConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="ChokingAlgorithm"/> enum value into a user-facing string from <see cref="UploadSlotBehaviors"/>.
        /// Intended for UI display and binding.
        /// </summary>
        /// <remarks>
        /// Throws <see cref="ArgumentOutOfRangeException"/> if the input is not a supported <see cref="ChokingAlgorithm"/> value.
        /// See <see cref="UploadSlotBehaviors"/> for the corresponding string constants.
        /// </remarks>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is ChokingAlgorithm chokingAlgorithm)
            {
                return chokingAlgorithm switch
                {
                    ChokingAlgorithm.RateBased => UploadSlotBehaviors.UploadRateBased,
                    ChokingAlgorithm.FixedSlots => UploadSlotBehaviors.FixedSlots,
                    _ => throw new ArgumentOutOfRangeException($"Only {nameof(ChokingAlgorithm)} values are accepted")
                };
            }
            return UploadSlotBehaviors.UploadRateBased;
        }
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is string str)
            {
                if (str == UploadSlotBehaviors.UploadRateBased)
                    return ChokingAlgorithm.RateBased;
                if (str == UploadSlotBehaviors.FixedSlots)
                    return ChokingAlgorithm.FixedSlots;

                throw new ArgumentOutOfRangeException($"Only the strings '{ChokingAlgorithm.RateBased}' and '{ChokingAlgorithm.FixedSlots}' are supported (use: {nameof(ChokingAlgorithm)})");
            }

            return BindingOperations.DoNothing; ;
        }
    }
}
