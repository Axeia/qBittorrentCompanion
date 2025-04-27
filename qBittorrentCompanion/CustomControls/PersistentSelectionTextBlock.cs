using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using qBittorrentCompanion.Helpers;
using System;
using System.Linq;

namespace qBittorrentCompanion.CustomControls
{
    /// <summary>
    /// Basically just SelectableTextBlock but without losing its selection when focus is lost.
    /// But with two utility methods, one to return the Run with the selection and one
    /// that takes a Run and returns its bounds.
    /// 
    /// Also contains a static brush to mark selections with.
    /// </summary>
    public class PersistentSelectionTextBlock : SelectableTextBlock
    {
        public static IBrush markedBrush = new SolidColorBrush(ThemeColors.SystemAccentDark1);
        protected override Type StyleKeyOverride => typeof(SelectableTextBlock);
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            // Don't call base.OnLostFocus to prevent selection clearing
            // base.OnLostFocus(e);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            // Ensure selection works properly on click
            base.OnPointerPressed(e);
            Focus();
        }

        public Run? GetRunWithSelection()
        {
            // Return null if no selection or no runs
            if (SelectionStart == SelectionEnd || Inlines == null)
                return null;

            var start = Math.Min(SelectionStart, SelectionEnd);
            var end = Math.Max(SelectionStart, SelectionEnd);
            int currentPos = 0;

            foreach (var inline in Inlines)
            {
                if (inline is Run run && run.Text is string runText)
                {
                    var runEnd = currentPos + runText.Length;

                    // Check if selection is entirely within this run
                    if (start >= currentPos && end <= runEnd)
                        return run.Background == markedBrush ? null : run;

                    currentPos += runText.Length;
                }
            }

            return null; // Selection spans multiple runs or no matching run found
        }

        public Rect GetBoundsForRun(Run targetRun)
        {
            // Wait for layout if needed
            InvalidateMeasure();
            InvalidateVisual();
            Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background).Wait();

            // Find run position
            int pos = 0;
            foreach (var inline in Inlines!.OfType<Run>())
            {
                if (ReferenceEquals(inline, targetRun))
                {
                    var bounds = TextLayout.HitTestTextRange(pos, inline.Text!.Length);
                    if (bounds.Any())
                    {
                        var rect = bounds.First();
                        return new Rect(
                            rect.X + Padding.Left,
                            rect.Bottom + Padding.Top,  // Use Bottom for vertical positioning
                            rect.Width,
                            TextLayout.Height);
                    }
                    break;
                }
                pos += inline.Text!.Length;
            }
            return new Rect(pos * 8, 0, targetRun.Text!.Length * 8, TextLayout.Height);
        }

    }
}
