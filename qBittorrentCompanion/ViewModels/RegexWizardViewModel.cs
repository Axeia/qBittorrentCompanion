using Avalonia;
using Avalonia.Controls.Documents;
using ReactiveUI;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace qBittorrentCompanion.ViewModels
{
    public class RegexWizardViewModel(Run run, int id, double leftOffset) : ViewModelBase
    {
        private readonly Thickness _margin = new(leftOffset, 0, 0, 0);
        public Thickness Margin => _margin;

        private Run _associatedRun = run;
        public Run AssociatedRun
        {
            get => _associatedRun;
            set => this.RaiseAndSetIfChanged(ref _associatedRun, value);
        }

        private int _id = id;
        public int Id
        {
            get => _id;
            set => this.RaiseAndSetIfChanged(ref _id, value);
        }

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

        private bool _isValid = true;
        public bool IsValid
        {
            get => _isValid;
            set => this.RaiseAndSetIfChanged(ref _isValid, value);
        }
    }
}
