using Avalonia.Media.Immutable;
using Avalonia.Media;
using Avalonia;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using System;

namespace qBittorrentCompanion.Helpers
{
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

    public abstract class Marker : TextSegment
    {
        public abstract void Draw(TextView textView, DrawingContext drawingContext);
    }

    public class ZigzagMarker : Marker
    {
        private static readonly ImmutablePen s_defaultPen = new(Brushes.Red, 0.75);

        public IPen? Pen { get; set; }

        public override void Draw(TextView textView, DrawingContext drawingContext)
        {
            foreach (Rect rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, this))
            {
                if (rect.Width <= 1 || rect.Height <= 1)
                {
                    // Current segment is inside a fold.
                    continue;
                }

                var pen = Pen ?? s_defaultPen;
                var start = rect.BottomLeft;
                var end = rect.BottomRight;
                var geometry = new StreamGeometry();
                using (var context = geometry.Open())
                {
                    context.BeginFigure(start, false);

                    const double zigLength = 3;
                    const double zigHeight = 3;
                    int numberOfZigs = (int)double.Round((end.X - start.X) / zigLength);
                    if (numberOfZigs < 2)
                        numberOfZigs = 2;

                    for (int i = 0; i < numberOfZigs; i++)
                    {
                        var p = new Point(
                            start.X + (i + 1) * zigLength,
                            start.Y - (i % 2) * zigHeight + 1);

                        context.LineTo(p);
                    }
                }

                drawingContext.DrawGeometry(null, pen, geometry);
            }
        }
    }
}
