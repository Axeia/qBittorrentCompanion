using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels
{
    public class PreferencesWindowViewModel : ViewModelBase
    {
        private TabItem? _selectedTabItem;

        public TabItem? SelectedTabItem
        {
            get => _selectedTabItem;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedTabItem, value);
                if (_selectedTabItem != null)
                    SelectedTabText = ((TextBlock)((StackPanel)_selectedTabItem.Header!).Children[1]).Text!;
                else
                    SelectedTabText = "?";
            }
        }

        private string _selectedTabText = "";
        public string SelectedTabText
        {
            get => _selectedTabText;
            set => this.RaiseAndSetIfChanged(ref _selectedTabText, value);
        }
    }
}
