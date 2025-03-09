using System;
using System.Collections.Generic;
using System.IO;
using TextMateSharp.Grammars;
using TextMateSharp.Internal.Grammars.Reader;
using TextMateSharp.Internal.Types;
using TextMateSharp.Registry;
using TextMateSharp.Themes;

namespace qBittorrentCompanion.AvaloniaEditor
{
    public class RegexRegistryOptions : IRegistryOptions
    {
        private readonly Dictionary<string, IRawGrammar> _customGrammars = [];

        public RegexRegistryOptions()
        {
            LoadCustomGrammar();
        }

        private void LoadCustomGrammar()
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, "Resources", "regex.tmLanguage.json");
            using var stream = File.OpenRead(filePath);
            using var reader = new StreamReader(stream);
            var rawGrammar = GrammarReader.ReadGrammarSync(reader);
            _customGrammars.Add("source.regexp", rawGrammar);
        }

        public IRawGrammar GetGrammar(string scopeName)
        {
            if (_customGrammars.ContainsKey(scopeName))
            {
                return _customGrammars[scopeName];
            }

            return new RegistryOptions(ThemeName.DarkPlus).GetGrammar(scopeName);
        }

        public IRawTheme GetDefaultTheme()
        {
            return new RegistryOptions(ThemeName.DarkPlus).GetDefaultTheme();
        }
        public IRawTheme GetTheme(string scopeName)
        {
            return new RegistryOptions(ThemeName.DarkPlus).GetTheme(scopeName);
        }
        public IEnumerable<GrammarDefinition> GetAvailableGrammarDefinitions()
        {
            return new RegistryOptions(ThemeName.DarkPlus).GetAvailableGrammarDefinitions();
        }
        public ICollection<string> GetInjections(string scopeName)
        {
            return new RegistryOptions(ThemeName.DarkPlus).GetInjections(scopeName);
        }
    }
}