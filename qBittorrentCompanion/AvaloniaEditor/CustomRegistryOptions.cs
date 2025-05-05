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
    public class CustomRegistryOptions : IRegistryOptions
    {
        private readonly Dictionary<string, IRawGrammar> _customGrammars = [];

        public CustomRegistryOptions(string fileName, string grammarHandle)
        {
            LoadCustomGrammar(fileName, grammarHandle);
        }

        private void LoadCustomGrammar(string fileName, string grammarHandle)
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, "Resources", fileName);
            using var stream = File.OpenRead(filePath);
            using var reader = new StreamReader(stream, System.Text.Encoding.UTF8);
            var rawGrammar = GrammarReader.ReadGrammarSync(reader);

            _customGrammars.Add(grammarHandle, rawGrammar);
        }

        public IRawGrammar GetGrammar(string scopeName)
        {
            if (_customGrammars.TryGetValue(scopeName, out IRawGrammar? value))
            {
                return value;
            }

            return new RegistryOptions(ThemeName.DarkPlus).GetGrammar(scopeName);
        }

        public IRawTheme GetDefaultTheme()
            => new RegistryOptions(ThemeName.DarkPlus).GetDefaultTheme();

        public IRawTheme GetTheme(string scopeName)
            => new RegistryOptions(ThemeName.DarkPlus).GetTheme(scopeName);

        public IEnumerable<GrammarDefinition> GetAvailableGrammarDefinitions() 
            => new RegistryOptions(ThemeName.DarkPlus).GetAvailableGrammarDefinitions();

        public ICollection<string> GetInjections(string scopeName)
            => new RegistryOptions(ThemeName.DarkPlus).GetInjections(scopeName);
    }
}