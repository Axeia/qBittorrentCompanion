using Avalonia.Media;
using DynamicData.Aggregation;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Joins;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace qBittorrentCompanion.ViewModels
{
    public class MatchTestRowViewModel : ViewModelBase
    {

        private bool? _matched;
        public bool? Matched
        {
            get => _matched;
            set => this.RaiseAndSetIfChanged(ref _matched, value);
        }

        private string _regexStr = "";
        public string RegexStr
        {
            get => _regexStr;
            set
            {
                if (value != _regexStr)
                {
                    this.RaiseAndSetIfChanged(ref _regexStr, value);
                    RunMatch();
                }
            } 
        }

        private bool _isValidRegex = true;
        public bool IsValidRegex
        {
            get => _isValidRegex;
            set => this.RaiseAndSetIfChanged(ref _isValidRegex, value);
        }

        private string _matchTest = "";
        public string MatchTest
        {
            get => _matchTest;
            set
            {
                if ( value != _matchTest)
                {
                    this.RaiseAndSetIfChanged(ref _matchTest, value);
                    RunMatch();
                }
            }
        }

        public MatchTestRowViewModel(string regex = "") 
        {
            RegexStr = regex;
        }

        private void RunMatch()
        {
            if (RegexStr == "" && _matchTest == "" || _matchTest == "")
            {
                Matched = null;
            }
            else if (RegexStr == string.Empty)
            {
                Matched = false;
            }
            else
                try
                {
                    Matched = Regex.Match(MatchTest, RegexStr).Success;
                }
                // Incomplete regular expressions in the process of being typed will throw this error.
                catch (RegexParseException)
                {
                    IsValidRegex = false;
                    Matched = false;
                }
        }


    }
}
