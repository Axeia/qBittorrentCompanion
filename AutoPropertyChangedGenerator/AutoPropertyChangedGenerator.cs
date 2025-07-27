namespace AutoPropertyChangedGenerator
{
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
