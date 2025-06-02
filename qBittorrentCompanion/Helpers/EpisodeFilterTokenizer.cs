using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace qBittorrentCompanion.Helpers
{
    public enum EpisodeFilterTokenType
    {
        Unknown, // Catches anything not explicitly recognized
        SeasonNumber, // e.g., "1", "10"
        EpisodeIndicator_x, // The 'x' character
        EpisodeNumber, // e.g., "1", "5", "10" (individual episode numbers)
        RangeSeparator, // The '-' character
        SegmentSeparator, // The ';' character
        End,
        MissingEndSegmentSeparator
    }

    public class EpisodeFilterToken(EpisodeFilterTokenType type, string value, int startIndex, string? errorMessage = null)
    {
        public EpisodeFilterTokenType Type { get; } = type;
        public string Value { get; } = value;
        public int StartIndex { get; } = startIndex;
        public int Length { get; } = value.Length;
        public (int, int) StartLengthTuple => (StartIndex, Length);
        public int EndIndex => StartIndex + Length; // Exclusive end index
        public string? ErrorMessage { get; } = errorMessage;

        public override string ToString()
        {
            // For debugging: "Type: Value (StartIndex, Length)"
            return $"{Type}: '{Value}' ({StartIndex}, {Length},{ErrorMessage == null}, {ErrorMessage})";
        }

        public bool IsValid => ErrorMessage == null;
        public bool InRange(int caretPos)
        {
            return caretPos >= StartIndex && caretPos < EndIndex;
        }
    }

    public class EpisodeFilterTokenizer
    {
        public static List<EpisodeFilterToken> Tokenize(string episodeFilter)
        {
            List<EpisodeFilterToken> tokens = [];
            if (string.IsNullOrEmpty(episodeFilter))
            {
                return tokens;
            }

            int currentIndex = 0;
            int lastIndex = episodeFilter.Length - 1;
            bool seasonXEncountered = false; // To ensure the "seasonX" prefix is handled

            while (currentIndex < episodeFilter.Length)
            {
                char currentChar = episodeFilter[currentIndex];
                int tokenStart = currentIndex;

                // Should start with Season Number (first token, must be a number)
                if (currentIndex == 0) // Only check for season number at the very beginning
                {
                    if (IsAsciiDigit(currentChar))
                    {
                        int endOfNumber = FindEndOfNumber(episodeFilter, currentIndex);
                        int length = endOfNumber - currentIndex;

                        // Basic validation for season number length (e.g., max 4 digits)
                        if (length > 4)
                        {
                            tokens.Add(
                                new EpisodeFilterToken(EpisodeFilterTokenType.SeasonNumber, episodeFilter.Substring(tokenStart, length), tokenStart,
                                "Season number is too long")
                            );
                        }
                        else
                        {
                            tokens.Add(new EpisodeFilterToken(EpisodeFilterTokenType.SeasonNumber, episodeFilter.Substring(tokenStart, length), tokenStart));
                        }

                        currentIndex = endOfNumber; // Move index past the number
                        continue; // Process next character
                    }
                    else
                    {
                        tokens.Add(
                            new EpisodeFilterToken(EpisodeFilterTokenType.SeasonNumber, episodeFilter, 0,
                            "Should start with season number (positive number) followed by a 'x'")
                        );
                        return tokens; // Stop parsing if initial format is wrong
                    }
                }

                if (!seasonXEncountered && currentChar == 'x')
                {
                    // Check if the previous token was a SeasonNumber
                    if (tokens.Count > 0 && tokens.Last().Type == EpisodeFilterTokenType.SeasonNumber)
                    {
                        tokens.Add(new EpisodeFilterToken(EpisodeFilterTokenType.EpisodeIndicator_x, "x", tokenStart));
                        seasonXEncountered = true;
                        currentIndex++;
                        continue;
                    }
                    else
                    {
                        // 'x' in an unexpected position
                        tokens.Add(
                            new EpisodeFilterToken(EpisodeFilterTokenType.EpisodeIndicator_x, "x", tokenStart,
                            "Episode indicator should be at the start right after the season number (e.g. 5x1)")
                        );
                        currentIndex++;
                        continue;
                    }
                }
                // Ensure 'x' has been encountered before parsing episode numbers/ranges
                else if (!seasonXEncountered)
                {
                    tokens.Add(new EpisodeFilterToken(EpisodeFilterTokenType.Unknown, episodeFilter[currentIndex..], currentIndex));
                    return tokens;
                }

                // 4. Handle other tokens (Episode Numbers, Separators) after 'x'
                if (IsAsciiDigit(currentChar))
                {
                    int endOfNumber = FindEndOfNumber(episodeFilter, currentIndex);
                    int length = endOfNumber - currentIndex;
                    tokens.Add(new EpisodeFilterToken(EpisodeFilterTokenType.EpisodeNumber, episodeFilter.Substring(tokenStart, length), tokenStart));
                    currentIndex = endOfNumber;
                }
                else if (currentChar == '-')
                {
                    tokens.Add(new EpisodeFilterToken(EpisodeFilterTokenType.RangeSeparator, "-", tokenStart));
                    currentIndex++;
                }
                else if (currentChar == ';')
                {
                    tokens.Add(new EpisodeFilterToken(
                        currentIndex == lastIndex 
                        ? EpisodeFilterTokenType.End
                        : EpisodeFilterTokenType.SegmentSeparator, 
                        ";", 
                        tokenStart));
                    currentIndex++;
                }
                else
                {
                    // Unrecognized character after the initial "seasonX" part
                    tokens.Add(new EpisodeFilterToken(EpisodeFilterTokenType.Unknown, episodeFilter.Substring(tokenStart, 1), tokenStart,
                        "Season number if followed up should be by an episode number (or range)"));
                    currentIndex++;
                }
            }

            if (episodeFilter.Length > 0 && episodeFilter.Last() != ';')
            {
                tokens.Add(new EpisodeFilterToken(EpisodeFilterTokenType.MissingEndSegmentSeparator, ";", episodeFilter.Length + 1));
            }

            return tokens;
        }

        /// <summary>
        /// Is a little bit faster than simply doing .IsDigit() but only
        /// works for ASCII 0-9 (which is perfect for the use case here)
        /// </summary>
        /// <param name="currentChar"></param>
        /// <returns></returns>
        private static bool IsAsciiDigit(char currentChar)
        {
            return currentChar >= '0' && currentChar <= '9';
        }

        private static int FindEndOfNumber(string text, int startIndex)
        {
            int i = startIndex;
            while (i < text.Length && char.IsDigit(text[i]))
            {
                i++;
            }
            return i;
        }
    }
}