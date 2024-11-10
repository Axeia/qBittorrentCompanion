using System;

public static class StringExtensions
{
    public static string ReplaceFirstOccurrence(this string source, string oldSubstring, string newSubstring)
    {
        return ReplaceOccurrence(source, oldSubstring, newSubstring, false);
    }

    /// <summary>
    /// Replace the last occurence of oldSubstring with newSubstring
    /// If there's no occurence of oldSubstring it simply returns the original string.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="oldSubstring"></param>
    /// <param name="newSubstring"></param>
    /// <returns></returns>
    public static string ReplaceLastOccurrence(this string source, string oldSubstring, string newSubstring)
    {
        return ReplaceOccurrence(source, oldSubstring, newSubstring, true);
    }

    /// <summary>
    /// Replace the last occurence of oldSubstring with newSubstring
    /// If there's no occurence of oldSubstring it simply returns the original string.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="oldSubstring"></param>
    /// <param name="newSubstring"></param>
    /// <returns></returns>
    private static string ReplaceOccurrence(string source, string oldSubstring, string newSubstring, bool replaceLast)
    {
        int place = replaceLast ? source.LastIndexOf(oldSubstring) : source.IndexOf(oldSubstring);

        return place == -1
            ? source
            : source.Remove(place, oldSubstring.Length).Insert(place, newSubstring);
    }
}