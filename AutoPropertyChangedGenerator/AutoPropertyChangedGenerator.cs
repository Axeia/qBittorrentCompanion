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


    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class AutoProxyPropertyChangedAttribute(string? property, string? name) : Attribute
    {
        public string? Property { get; set; } = property;
        public string? Name { get; set; } = name ?? property;
    }
}
