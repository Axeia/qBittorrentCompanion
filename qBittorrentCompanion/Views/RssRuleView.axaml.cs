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
using ReactiveUI;
using Avalonia.Media;
using Avalonia;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.Document;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace qBittorrentCompanion.Views
{
    public class EpisodeFilterPart : ViewModelBase
    {
        private string _content = string.Empty;
        public string Content
        {
            get => _content;
            set => _content = value;
        }

        private bool _isSeason = false;
        public bool IsSeason
        {
            get => _isSeason;
            set => _isSeason = value;
        }
    }

    public partial class RssRuleView : UserControl
    {
        private Flyout _beforeCaretFlyout;
        private Flyout _afterCaretFlyout;
        private TextEditor? lastFocussedTextEditor = null;
        private static readonly MarkerRenderer markerRenderer = new();
        MarkerRenderer _mustContainMarkerRenderer = markerRenderer;
        MarkerRenderer _mustNotContainMarkerRenderer = new();
        MarkerRenderer _episodeFilterMarkerRenderer = new();

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
            private readonly IBrush _background;

            public BackgroundHighlightMarker(int startOffset, int length, IBrush? bgBrush = null)
            {
                StartOffset = startOffset;
                Length = length;
                _background = bgBrush ?? new SolidColorBrush(Colors.Red, 0.4);
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
            _beforeCaretFlyout = (Flyout)Resources["BeforeCaretFlyout"]!;
            _afterCaretFlyout = (Flyout)Resources["AfterCaretFlyout"]!;
            // Removed the initial marker here
        }

        private void RssRuleView_DataContextChanged(object? sender, EventArgs e)
        {
            StartObservingIndexErrorChanges();
            UpdateEpisodeFilterParts();
        }

        private ObservableCollection<EpisodeFilterPart> _episodeFilterParts = [];

        private void UpdateEpisodeFilterParts()
        {
            _episodeFilterParts.Clear();

            if(DataContext is RssAutoDownloadingRuleViewModel radvm)
            { 
                for(int i = 0; i < radvm.Tokens.Count; i++)
                {
                    EpisodeFilterToken token = radvm.Tokens[i];

                    if (token.Type == EpisodeFilterTokenType.SeasonNumber)
                    {
                        _episodeFilterParts.Add(new EpisodeFilterPart() { Content = "Season: " + token.Value, IsSeason = true });
                    }
                    else if (token.Type == EpisodeFilterTokenType.EpisodeNumber)
                    {
                        _episodeFilterParts.Add(new EpisodeFilterPart() { Content = 'E' + token.Value });
                        if (i+1 < radvm.Tokens.Count)
                        {
                            EpisodeFilterToken nextToken = radvm.Tokens[i+1];
                            EpisodeFilterToken? nextNextToken = i+2 < radvm.Tokens.Count ? radvm.Tokens[i+2] : null;
                            if(nextToken.Type == EpisodeFilterTokenType.RangeSeparator)
                            {
                                if(nextNextToken != null && nextNextToken.Type == EpisodeFilterTokenType.EpisodeNumber && nextNextToken.IsValid)
                                {
                                    i++;
                                    _episodeFilterParts.Last().Content += '-'+nextNextToken.Value;
                                }
                                else
                                    _episodeFilterParts.Last().Content += '+';

                                i++;
                            }
                        }
                    }
                }
            }

            EpisodeFilterPartsItemsControl.ItemsSource = _episodeFilterParts;
        }

        private void StartObservingIndexErrorChanges()
        {
            if (DataContext is RssAutoDownloadingRuleViewModel radrvm)
            {
                radrvm
                    .WhenAnyValue(vm => vm.MustContainErrorIndexes)
                    .Subscribe(errorIndexes => 
                        UpdateTextEditorMarker(errorIndexes, _mustContainMarkerRenderer, MustContainTextBoxLikeEditor));

                radrvm.WhenAnyValue(vm => vm.MustNotContainErrorIndexes)
                    .Subscribe(errorIndexes =>
                        UpdateTextEditorMarker(errorIndexes, _mustNotContainMarkerRenderer, MustNotContainTextBoxLikeEditor));
            }
            else
                Debug.WriteLine("Unexpected vm: " + this.DataContext);
        }

        private void UpdateTextEditorMarker(
            (int Start, int Length) errorIndexes,
            MarkerRenderer markerRenderer,
            TextBoxLikeEditor textBoxLikeEditor,
            IBrush? highlightBrush = null,
            bool clear = false)
        {
            if (clear)
                markerRenderer.Markers.Clear(); // Clear any existing marker

            if (errorIndexes.Length > 0) // Only add a marker if there's something to mark
            {
                markerRenderer.Markers.Add(new BackgroundHighlightMarker(errorIndexes.Start, errorIndexes.Length, highlightBrush));
            }

            textBoxLikeEditor.EditorBase.TextArea.TextView.InvalidateLayer(markerRenderer.Layer);
        }

        private void RssRuleView_Loaded(object? sender, RoutedEventArgs e)
        {
            MustContainTextBoxLikeEditor.EditorBase.TextArea.Caret.PositionChanged += Caret_PositionChanged;
            MustContainTextBoxLikeEditor.EditorBase.TextArea.GotFocus += TextArea_GotFocus;
            MustContainTextBoxLikeEditor.EditorBase.LostFocus += BindableRegexEditor_LostFocus;
            MustContainTextBoxLikeEditor.EditorBase.TextArea.TextView.BackgroundRenderers.Add(_mustContainMarkerRenderer);

            MustNotContainTextBoxLikeEditor.EditorBase.TextArea.Caret.PositionChanged += Caret_PositionChanged;
            MustNotContainTextBoxLikeEditor.EditorBase.TextArea.GotFocus += TextArea_GotFocus;
            MustNotContainTextBoxLikeEditor.EditorBase.LostFocus += BindableRegexEditor_LostFocus;
            MustContainTextBoxLikeEditor.EditorBase.TextArea.TextView.BackgroundRenderers.Add(_mustNotContainMarkerRenderer);

            StartObservingIndexErrorChanges();

            EpisodeFilterTextBoxLikeEditor.EditorBase.TextArea.TextView.BackgroundRenderers.Add(_episodeFilterMarkerRenderer);
            EpisodeFilterTextBoxLikeEditor.EditorBase.TextArea.GotFocus += EpisodeFilterTextBoxLikeEditor_GotFocus;
            EpisodeFilterTextBoxLikeEditor.EditorBase.TextChanged += EpisodeFilterEditorBase_TextChanged;
            EpisodeFilterTextBoxLikeEditor.EditorBase.TextArea.Caret.PositionChanged += EpFilterCaret_PositionChanged;
            EpisodeFilterTextBoxLikeEditor.EditorBase.LostFocus += EditorBase_LostFocus;
        }

        private void EpisodeFilterTextBoxLikeEditor_GotFocus(object? sender, GotFocusEventArgs e)
        {
            HintWhatShouldBeEntered(EpisodeFilterTextBoxLikeEditor.EditorBase.TextArea.Caret);
        }

        private void EditorBase_LostFocus(object? sender, RoutedEventArgs e)
        {
            _afterCaretFlyout.Hide();
            _beforeCaretFlyout.Hide();
        }

        private void EpisodeFilterEditorBase_TextChanged(object? sender, EventArgs e)
        {
            string currentText = EpisodeFilterTextBoxLikeEditor.EditorBase.Text;
            UpdateEpisodeFilterParts();
        }

        private void EpFilterCaret_PositionChanged(object? sender, EventArgs e)
        {
            //ShowCaretPosition(sender);
            var editorBase = EpisodeFilterTextBoxLikeEditor.EditorBase;
            var textArea = editorBase.TextArea;
            var caret = textArea.Caret!;

            HintWhatShouldBeEntered(caret);
        }

        private void HintWhatShouldBeEntered(Caret caret)
        {
            if (DataContext is RssAutoDownloadingRuleViewModel radvm)
            { 
                string hint = $"Char: {caret.Position.VisualColumn} | ";

                _episodeFilterMarkerRenderer.Markers.Clear();

                TextBlock beforeCaretTextBox = ((TextBlock)_beforeCaretFlyout.Content!);
                TextBlock afterCaretTextBox = ((TextBlock)_afterCaretFlyout.Content!);
                bool showBeforeFlyout = false;
                bool showAfterFlyout = false;

                // Token *immediately to the left* of the caret (or containing the caret)
                EpisodeFilterToken? tokenLeft = radvm.Tokens
                    .FirstOrDefault(t => caret.Position.VisualColumn > t.StartIndex && caret.Position.VisualColumn <= t.EndIndex);

                // Token *immediately to the right* of the caret (or containing the caret)
                EpisodeFilterToken? tokenRight = radvm.Tokens
                    .FirstOrDefault(t => caret.Position.VisualColumn >= t.StartIndex && caret.Position.VisualColumn < t.EndIndex);

                // Handle special case for caret at position 0:
                // If the filter string starts with a token, that token is "right".
                if (caret.Position.VisualColumn == 0)
                {
                    hint += "Enter season number [0-9] (up to 4 digits)";

                    if (radvm.Tokens.Count > 0)
                        tokenRight = radvm.Tokens.First();
                }

                // Highlight and show info for the token to the LEFT (or containing caret if it's the *only* one there)
                if (tokenLeft is not null)
                {
                    UpdateTextEditorMarker(tokenLeft.StartLengthTuple, _episodeFilterMarkerRenderer, EpisodeFilterTextBoxLikeEditor, new SolidColorBrush(Colors.LightBlue, 0.4), false);
                    beforeCaretTextBox.Text = GetTextForEpisodeFilterToken(tokenLeft);
                    showBeforeFlyout = true;

                    if (tokenLeft.Type == EpisodeFilterTokenType.SeasonNumber)
                    {
                        hint += "Continue season number with [0-9] or enter episode denoter 'x'";
                    }

                    //Next should be episode 
                    if(tokenLeft.Type is EpisodeFilterTokenType.EpisodeIndicator_x or EpisodeFilterTokenType.SegmentSeparator)
                    {
                        hint += "Episode or episode range [0-9]";
                    }

                    if (tokenLeft.Type == EpisodeFilterTokenType.EpisodeNumber)
                    {
                        hint += "Continue episode number with [0-9]:, end it with ';' or signify a range with '-'";
                    }

                    if (tokenLeft.Type == EpisodeFilterTokenType.RangeSeparator)
                    {
                        hint += "Limit range with episode number [0-9] or end it with ';'";
                    }
                }

                // Highlight and show info for the token to the RIGHT
                // Only highlight if it's a *different* token from the left one,
                // or if there is no tokenLeft (meaning tokenRight is the only one to consider).
                if (tokenRight is not null && (tokenLeft == null || tokenRight.StartIndex != tokenLeft.StartIndex))
                {
                    UpdateTextEditorMarker(tokenRight.StartLengthTuple, _episodeFilterMarkerRenderer, EpisodeFilterTextBoxLikeEditor, new SolidColorBrush(Colors.Yellow, 0.4), false);
                    afterCaretTextBox.Text = GetTextForEpisodeFilterToken(tokenRight);
                    showAfterFlyout = true;
                }

                // --- Display Flyouts ---
                if (showBeforeFlyout) _beforeCaretFlyout.ShowAt(EpisodeFilterTextBoxLikeEditor);
                else _beforeCaretFlyout.Hide();

                if (showAfterFlyout) _afterCaretFlyout.ShowAt(EpisodeFilterTextBoxLikeEditor);
                else _afterCaretFlyout.Hide();

                // Force redraw of the editor to ensure marker changes are visible
                EpisodeFilterTextBoxLikeEditor.EditorBase.TextArea.TextView.InvalidateVisual();
                ShowStatusBarMessage(hint);
            }
        }

        private static string GetTextForEpisodeFilterToken(EpisodeFilterToken eft)
        {
            switch (eft.Type)
            {
                case EpisodeFilterTokenType.SeasonNumber:
                    return "Season number: Positive non-zero number up to 4 digits long (followed by 'x')";
                case EpisodeFilterTokenType.EpisodeIndicator_x:
                    return "Season/Episode separator (x)";
                case EpisodeFilterTokenType.EpisodeNumber:
                    return "Episode number: Positive number up to 4 characters long";
                case EpisodeFilterTokenType.SegmentSeparator:
                    return "Segment separator";
                case EpisodeFilterTokenType.End:
                    return "Ending 'separator'";
                case EpisodeFilterTokenType.RangeSeparator:
                    return "Range indicator, use after or in between episode number(s) (e.g. 4-9; or 4-;)";
                case EpisodeFilterTokenType.MissingEndSegmentSeparator:
                    return "Reached end but no ending seperator (;) was found";
                case EpisodeFilterTokenType.Unknown:
                    return "Invalid input";
                default:
                    return "default";
            }
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
                    .FirstOrDefault(item => item.ToString()!.Equals(acb.Text, StringComparison.OrdinalIgnoreCase));

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
            if (sender is Caret caret)
            {
                var text = $"Char: {(caret.Offset).ToString()}";

                if (lastFocussedTextEditor is TextEditor te)
                    text += " | Length: " + te.Text.Length;

                ShowStatusBarMessage(text);
            }
        }

        private void ShowStatusBarMessage(string message)
        {

            var mainWindow = this.GetVisualAncestors().OfType<MainWindow>().First();
            mainWindow.PermanentMessageTextBlock.Opacity = 1;
            mainWindow.PermanentMessageTextBlock.Text = message;
        }

        private void BindableRegexEditor_LostFocus(object? sender, RoutedEventArgs e)
        {
            var mainWindow = this.GetVisualAncestors().OfType<MainWindow>().First();
            mainWindow.PermanentMessageTextBlock.Opacity = 0;
        }
    }
}