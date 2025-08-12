using System;

namespace qBittorrentCompanion.Helpers
{
    public static class TypeNameHelper
    {
        public static string GetFullTypeName<T>() => GetFullTypeName(typeof(T));
        public static string GetFullTypeName(Type type) => type.FullName ?? type.Name;
    }
}
