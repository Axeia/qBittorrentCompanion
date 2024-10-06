using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using qBittorrentCompanion.ViewModels;
using System.Diagnostics;
using System.Linq;

namespace qBittorrentCompanion.Views
{
    public partial class EditTrackersWindow : Window
    {
        private TorrentInfoViewModel? _torrentInfoViewModel;

        public EditTrackersWindow()
        {
            var temp = new EditTrackersWindowViewModel("", "some torrent");
            DataContext = temp;

            InitializeComponent();
        }

        public EditTrackersWindow(TorrentInfoViewModel torrentInfoViewModel)
        {
            this._torrentInfoViewModel = torrentInfoViewModel;
            var temp = new EditTrackersWindowViewModel(torrentInfoViewModel.Hash, torrentInfoViewModel.Name);
            DataContext = temp;

            InitializeComponent();
            this.Title += torrentInfoViewModel.Name;
            TrackersTextBox.AddHandler(KeyDownEvent, TrackersTextBox_KeyDown, RoutingStrategies.Tunnel);
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TrackersTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            Debug.WriteLine("Keydown event triggered");
            if (sender is TextBox tb && tb.Text != null)
            {
                if (e.Key == Key.Return || e.Key == Key.Enter)
                {
                    tb.Text = tb.Text.Replace("\r\n", "\n");

                    int tbLength = tb.Text.Length;
                    int newLinesAroundCaret = 0;
                    int beforeCaret = tb.CaretIndex - 1;
                    int afterCaret = tb.CaretIndex;

                    // Count newlines before the caret
                    while (beforeCaret >= 0 && tb.Text.ElementAt(beforeCaret) == '\n')
                    {
                        newLinesAroundCaret++;
                        beforeCaret--;
                    }
                    Debug.WriteLine($"Before: {beforeCaret}");

                    // Count newlines after the caret
                    while (afterCaret < tbLength && tb.Text.ElementAt(afterCaret) == '\n')
                    {
                        newLinesAroundCaret++;
                        afterCaret++;
                    }
                    Debug.WriteLine($"After: {afterCaret}");

                    // If there are already 3 or more newlines around the caret, prevent adding another
                    if (newLinesAroundCaret >= 3)
                    {
                        Debug.WriteLine($"prevented");
                        e.Handled = true;
                    }
                }
                else
                {
                    e.Handled = false; // Allow backspace to be processed normally
                }
            }
            else
            {
                e.Handled = false; 
            }
        }

    }
}
