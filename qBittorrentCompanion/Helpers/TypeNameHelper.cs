using System;

namespace qBittorrentCompanion.Helpers
{
    /// <summary>
    /// Returns the full type name including namespace, or the simple name if <c>FullName</c> is null.
    /// Useful for logging, diagnostics, and reflection.
    /// </summary>
    public static class TypeNameHelper
    {
        public static string GetFullTypeName<T>() => GetFullTypeName(typeof(T));
        public static string GetFullTypeName(Type type) => type.FullName ?? type.Name;
    }
}
