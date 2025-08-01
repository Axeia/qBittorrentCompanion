﻿using AutoPropertyChangedGenerator;
using ReactiveUI;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace qBittorrentCompanion.ViewModels
{
    public partial class RssRuleIsMatchViewModel : ReactiveObject
    {
        [AutoPropertyChanged]
        private bool _isMatch = false;

        public static string WildCardToRegular(string value)
        {
            // Split the input string by white spaces and "|"
            string[] parts = value.Split([' ', '\t', '\n', '\r', '|'], StringSplitOptions.RemoveEmptyEntries);

            // Convert each part into a regex pattern
            var patterns = parts.Select(part => Regex.Escape(part).Replace("\\?", ".").Replace("\\*", ".*"));

            // Join the patterns with ".*", which functions as an AND operator in regex
            var regexPattern = string.Join(".*", patterns);

            return regexPattern;
        }

        public static bool IsTextMatch(string toMatch, string mustContain, string mustNotContain, bool isRegex)
        {
            if (mustContain == string.Empty && mustNotContain == string.Empty)
                return false;

            try
            { 
                if (mustContain != string.Empty)
                {
                    Regex mustContainRegex = isRegex
                        ? new Regex(mustContain)
                        : new Regex(WildCardToRegular(mustContain));

                    if (!mustContainRegex.Match(toMatch).Success)
                        return false;
                }

                if (mustNotContain != string.Empty)
                {
                    Regex mustNotContainRegex = isRegex
                        ? new Regex(mustNotContain)
                        : new Regex(WildCardToRegular(mustNotContain));

                    return !mustNotContainRegex.Match(toMatch).Success;
                }
            }
            catch (RegexParseException)
            {
                return false;
            }

            return true;
        }
    }
}
