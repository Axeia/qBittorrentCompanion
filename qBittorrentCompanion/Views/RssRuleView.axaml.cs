using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using QBittorrent.Client;
using qBittorrentCompanion.ViewModels;
using System.Diagnostics;
using System.Linq;
using Avalonia.Input;
using System;
using AvaloniaEdit;
using AvaloniaEdit.Editing;
using qBittorrentCompanion.Helpers;
using TextMateSharp.Grammars;
using ReactiveUI;
using Avalonia.Media;
using Avalonia;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.Document;

namespace qBittorrentCompanion.Views
{
    public partial class RssRuleView : UserControl
    {
        public abstract class Marker : TextSegment
        {
            public abstract void Draw(TextView textView, DrawingContext drawingContext);
        }

        /// <summary>
        /// Straight up copied from:
        /// https://github.com/AvaloniaUI/AvaloniaEdit/discussions/411#discussioncomment-9120820
        /// credit to mgarstenauer
        /// </summary>
        public class MarkerRenderer : IBackgroundRenderer
        {
            public TextSegmentCollection<Marker> Markers { get; } = [];
            public KnownLayer Layer => KnownLayer.Background;
            void IBackgroundRenderer.Draw(TextView textView, DrawingContext drawingContext)
            {
                ArgumentNullException.ThrowIfNull(textView);
                ArgumentNullException.ThrowIfNull(drawingContext);
                if (Markers.Count == 0)
                    return;

                var visualLines = textView.VisualLines;
                if (visualLines.Count == 0)
                    return;

                int viewStart = visualLines[0].FirstDocumentLine.Offset;
                int viewEnd = visualLines[^1].LastDocumentLine.EndOffset;

                foreach (var marker in Markers.FindOverlappingSegments(viewStart, viewEnd - viewStart))
                    marker.Draw(textView, drawingContext);

            }
        }

        public class BackgroundHighlightMarker : Marker
        {
            private readonly IBrush _background = new SolidColorBrush(Colors.Red, 0.4);

            public BackgroundHighlightMarker(int startOffset, int length)
            {
                StartOffset = startOffset;
                Length = length;                
            }

            public override void Draw(TextView textView, DrawingContext drawingContext)
            {
                foreach (Rect rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, this))
                {
                    drawingContext.DrawRectangle(_background, null, rect);
                }
            }
        }

        public RssRuleView()
        {
            InitializeComponent();

            if (Design.IsDesignMode)
            {
                DataContext = new RssAutoDownloadingRuleViewModel(
                    new RssAutoDownloadingRule(),
                    ""
                );
            }

            Loaded += RssRuleView_Loaded;
            DataContextChanged += RssRuleView_DataContextChanged;
            // Removed the initial marker here
        }

        private void RssRuleView_DataContextChanged(object? sender, EventArgs e)
        {
            StartObservingIndexErrorChanges();
        }

        private void StartObservingIndexErrorChanges()
        {
            if (DataContext is RssAutoDownloadingRuleViewModel radrvm)
            {
                radrvm
                    .WhenAnyValue(vm => vm.MustContainErrorIndexes)
                    .Subscribe(errorIndexes => UpdateMustContainMarker(errorIndexes));

                radrvm.WhenAnyValue(vm => vm.MustNotContainErrorIndexes)
                    .Subscribe(errorIndexes => UpdateMustNotContainMarker(errorIndexes));
            }
            else
                Debug.WriteLine("Unexpected vm: " + this.DataContext);
        }

        private void UpdateMustNotContainMarker((int, int) errorIndexes)
        {
            //throw new NotImplementedException();
        }

        private void UpdateMustContainMarker((int, int) errorIndexes)
        {
            _mustContainMarkerRenderer.Markers.Clear(); // Clear any existing marker
            (int start, int end) startEnd = errorIndexes;
            Debug.WriteLine($"{startEnd.start} : {startEnd.end}");
            if (startEnd.start < startEnd.end) // Only add a marker if it's a range
            {
                _mustContainMarkerRenderer.Markers.Add(new BackgroundHighlightMarker(startEnd.start, startEnd.end));
            }

            MustContainTextBoxLikeEditor.EditorBase.TextArea.TextView.InvalidateLayer(_mustContainMarkerRenderer.Layer);
        }

        private TextEditor? lastFocussedTextEditor = null;
        MarkerRenderer _mustContainMarkerRenderer = new();

        private void RssRuleView_Loaded(object? sender, RoutedEventArgs e)
        {
            MustContainTextBoxLikeEditor.EditorBase.TextArea.Caret.PositionChanged += Caret_PositionChanged;
            MustNotContainTextBoxLikeEditor.EditorBase.TextArea.Caret.PositionChanged += Caret_PositionChanged;

            MustContainTextBoxLikeEditor.EditorBase.TextArea.GotFocus += TextArea_GotFocus;
            MustNotContainTextBoxLikeEditor.EditorBase.TextArea.GotFocus += TextArea_GotFocus;

            MustContainTextBoxLikeEditor.EditorBase.LostFocus += BindableRegexEditor_LostFocus;
            MustNotContainTextBoxLikeEditor.EditorBase.LostFocus += BindableRegexEditor_LostFocus;

            MustContainTextBoxLikeEditor.EditorBase.TextArea.TextView.BackgroundRenderers.Add(_mustContainMarkerRenderer);
            StartObservingIndexErrorChanges();
        }

        private void TextArea_GotFocus(object? sender, GotFocusEventArgs e)
        {
            if (sender is TextArea ta)
            {
                lastFocussedTextEditor = ta.FindAncestorOfType<TextEditor>();
                Caret_PositionChanged(ta.Caret, new EventArgs());
            }
        }

        private void RemoveFeedButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is RssAutoDownloadingRuleViewModel radRuleVm 
                && sender is Button button 
                && button.DataContext is RssFeedViewModel rfvm)
            {
                radRuleVm.SelectedFeeds.Remove(rfvm);
            }
        }

        private void RemoveTagButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is RssAutoDownloadingRuleViewModel radRuleVm
                && sender is Button button
                && button.DataContext is string tag)
            {
                radRuleVm.SelectedTags.Remove(tag);
            }
        }

        private void SwitchToFeedsButton_Click(object? sender, RoutedEventArgs e)
        {
            var mainWindow = this.GetVisualAncestors().OfType<MainWindow>().First();
            mainWindow.MainTabStrip.SelectedIndex = 2;
        }

        private void PluginSourceTextBox_PastingFromClipboard(object? sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Text != null)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    textBox.Text = textBox.Text.Trim();
                });
            }
        }

        private void AutoCompleteBox_GotFocus(object? sender, GotFocusEventArgs e)
        {
            if (sender is AutoCompleteBox acb)
            {
                acb.IsDropDownOpen = true;
            }
        }

        /// <summary>
        /// Adds to Selected list when the dropdown is closed (happens when an items is selected or enter is pressed)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoCompleteBox_DropDownClosed(object? sender, EventArgs e)
        {
            if (sender is AutoCompleteBox acb
                && DataContext is RssAutoDownloadingRuleViewModel radrvm)
            {
                var listType = radrvm.RssFeeds.GetType().GetGenericArguments().FirstOrDefault();
                if (listType == null)
                {
                    Debug.WriteLine("Unable to determine list type.");
                    return;
                }

                // Use reflection to find the matching item and cast it
                var matchingItem = radrvm.RssFeeds
                    .FirstOrDefault(item => item.ToString().Equals(acb.Text, StringComparison.OrdinalIgnoreCase));

                if (matchingItem != null && listType.IsInstanceOfType(matchingItem))
                {
                    if (!radrvm.SelectedFeeds.Contains(matchingItem))
                    {
                        radrvm.SelectedFeeds.Add(matchingItem);
                        acb.Text = ""; 
                        acb.IsDropDownOpen = true;
                    }
                }
            }
        }

        private void CategoryComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox cb && cb.SelectedItem is Category q)
            {
                SavePathTextBox.Watermark = q is not null && !string.IsNullOrEmpty(q.SavePath)
                    ? q.SavePath
                    : "path/to/save/to";
            }
        }

        private void Caret_PositionChanged(object? sender, EventArgs e)
        {
            ShowCaretPosition(sender);
        }

        private void ShowCaretPosition(object? sender)
        {
            var mainWindow = this.GetVisualAncestors().OfType<MainWindow>().First();
            mainWindow.PermanentMessageTextBlock.Opacity = 1;

            if (sender is Caret caret)
            {
                var text = $"Char: {(caret.Offset).ToString()}";

                if (lastFocussedTextEditor is TextEditor te)
                    text += " | Length: " + te.Text.Length;
                mainWindow.PermanentMessageTextBlock.Text = text;
            }
        }

        private void BindableRegexEditor_LostFocus(object? sender, RoutedEventArgs e)
        {
            var mainWindow = this.GetVisualAncestors().OfType<MainWindow>().First();
            mainWindow.PermanentMessageTextBlock.Opacity = 0;
        }
    }
}