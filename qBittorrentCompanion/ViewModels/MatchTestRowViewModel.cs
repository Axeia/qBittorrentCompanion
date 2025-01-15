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

        private bool _isMatch = false;
        public bool IsMatch
        {
            get { Debug.WriteLine($">{_isMatch}"); return _isMatch; }
            set => this.RaiseAndSetIfChanged(ref _isMatch, value);
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
                }
            }
        }

        public MatchTestRowViewModel(string text = "") 
        {
            //RegexStr = regex;
        }



    }
}
