namespace qBittorrentCompanion.Helpers
{
    public static class TypeNameHelper
    {
        public static string GetFullTypeName<T>() => typeof(T).FullName ?? typeof(T).Name;
    }
}
