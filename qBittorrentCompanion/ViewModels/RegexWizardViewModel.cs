using Avalonia;
using Avalonia.Controls.Documents;
using RaiseChangeGenerator;
using ReactiveUI;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace qBittorrentCompanion.ViewModels
{
    public partial class RegexWizardViewModel(Run run, int id, double leftOffset) : ViewModelBase
    {
        private readonly Thickness _margin = new(leftOffset, 0, 0, 0);
        public Thickness Margin => _margin;

        [RaiseChange]
        private Run _associatedRun = run;
        [RaiseChange]
        private int _id = id;

        public string Original
        {
            get => AssociatedRun.Text!;
        }

        private string _replaceWith = ".+";
        public string ReplaceWith
        {
            get => _replaceWith;
            set
            {
                this.RaiseAndSetIfChanged(ref _replaceWith, value);
                try
                {
                    var r = new Regex(value);
                    IsValid = true;
                }
                catch (RegexParseException e)
                {
                    IsValid = false;
                    Debug.WriteLine(e.Message);
                }
            }
        }

        [RaiseChange]
        private bool _isValid = true;
    }
}
