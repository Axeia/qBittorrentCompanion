using System;

namespace AutoPropertyChangedGenerator
{
    /// <summary>
    /// Generates a public property with ReactiveUI change notification for this field.
    /// The property name will be the field name in PascalCase without underscores.
    /// e.g. private int _myCounter; becomes public int MyCounter
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class AutoPropertyChangedAttribute : Attribute
    {
    }

    /// <summary>
    /// Generates a proxy property that forwards to a property on the annotated field.
    /// <b>HIGHLY Recommended</b> to use nameof() for compile-time safety and easier refractoring
    /// </summary>
    /// <example>
    /// [AutoProxyPropertyChanged(nameof(RssAutoDownloadingRule.SmartFilter))]
    /// [AutoProxyPropertyChanged(nameof(RssAutoDownloadingRule.PreviouslyMatchedEpisodes), "Episodes")]
    /// private RssAutoDownloadingRule _rule;
    /// </example>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class AutoProxyPropertyChangedAttribute(string propertyName, string? customName = null) : Attribute
    {
        public string PropertyName { get; } = propertyName;
        public string? CustomName { get; } = customName;
    }
}