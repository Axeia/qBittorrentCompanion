using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using qBittorrentCompanion.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace qBittorrentCompanion.CustomControls
{
    public class LineGraph : Control
    {
        public static readonly StyledProperty<ObservableCollection<long>> ValuesProperty =
            AvaloniaProperty.Register<LineGraph, ObservableCollection<long>>(nameof(Values));

        public static readonly StyledProperty<int> NumberOfTicksProperty =
            AvaloniaProperty.Register<LineGraph, int>(nameof(NumberOfTicks), defaultValue: 5);

        public static readonly StyledProperty<Color> AxesColorProperty =
            AvaloniaProperty.Register<LineGraph, Color>("", defaultValue: ThemeColors.SystemAltHigh);
        public Color AxesColor
        {
            get => GetValue<Color>(AxesColorProperty);
            set => SetValue(AxesColorProperty, value);
        }

        public static readonly StyledProperty<Color> LineColorProperty =
            AvaloniaProperty.Register<LineGraph, Color>("", defaultValue: ThemeColors.SystemAccent);
        public Color LineColor
        {
            get => GetValue<Color>(LineColorProperty);
            set => SetValue(LineColorProperty, value);
        }

        public static readonly StyledProperty<long?> TopLimitProperty =
            AvaloniaProperty.Register<LineGraph, long?>("", defaultValue: null);
        public long? TopLimit
        {
            get => GetValue<long?>(TopLimitProperty);
            set => SetValue(TopLimitProperty, value);
        }

        public static readonly StyledProperty<long?> BottomLimitProperty =
            AvaloniaProperty.Register<LineGraph, long?>("", defaultValue: 0);
        public long? BottomLimit
        {
            get => GetValue<long?>(BottomLimitProperty);
            set => SetValue(BottomLimitProperty, value);
        }

        public int labelSpacing = 5;

        public ObservableCollection<long> Values
        {
            get => GetValue(ValuesProperty);
            set
            {
                var oldValue = GetValue(ValuesProperty);
                if (oldValue != null)
                    oldValue.CollectionChanged -= Values_CollectionChanged;

                SetValue(ValuesProperty, value);

                if (value != null)
                    value.CollectionChanged += Values_CollectionChanged;
            }
        }

        public int NumberOfTicks
        {
            get => GetValue(NumberOfTicksProperty);
            set => SetValue(NumberOfTicksProperty, value);
        }


        public LineGraph()
        {
            Width = 200;
            Height = 150;

        }

        private void Values_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Debug.WriteLine("Change is afoot");
            Dispatcher.UIThread.Post(InvalidateVisual);
        }

        private FormattedText CreateFormattedText(double value, IBrush brush)
        {
            return new FormattedText(
                value.ToString("0.##"),
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                12,
                brush);
        }

        private (double minValue, double maxValue, double step) CalculateAxisValues()
        {
            if (Values == null || Values.Count == 0)
                return (0, 0, 0);

            double yMax = TopLimit ?? RoundUpToNearestNiceValue(Values.Max());
            double yMin = BottomLimit ?? Values.Min();
            double yStep = (yMax - yMin) / (NumberOfTicks - 1);

            return (yMin, yMax, yStep);
        }

        private (double labelHeight, double maxLabelWidth) CalculateLabelDimensions()
        {
            var tickBrush = new SolidColorBrush(AxesColor);
            var (yMin, yMax, yStep) = CalculateAxisValues();

            double maxLabelWidth = 0;
            double labelHeight = 0;

            for (int i = 0; i < NumberOfTicks; i++)
            {
                double yValue = yMin + i * yStep;
                var formattedText = CreateFormattedText(yValue, tickBrush);
                maxLabelWidth = Math.Max(maxLabelWidth, formattedText.Width);
                labelHeight = formattedText.Height; // All labels should have the same height
            }

            return (labelHeight, maxLabelWidth);
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            if (Values == null || Values.Count == 0)
                return;

            var (labelHeight, maxLabelWidth) = CalculateLabelDimensions();
            maxLabelWidth += labelSpacing;
            var (yMin, yMax, yStep) = CalculateAxisValues();

            // Calculate margins
            double topMargin = labelHeight / 2;
            double bottomMargin = labelHeight / 2;
            double leftMargin = maxLabelWidth;

            // Calculate available space for the graph
            double graphHeight = Bounds.Height - topMargin - bottomMargin;
            double graphWidth = Bounds.Width - leftMargin;

            // Calculate scales
            double yScale = graphHeight / (yMax - yMin);
            double xStep = graphWidth / (Values.Count - 1);

            DrawLine(context, leftMargin, topMargin, bottomMargin, xStep, yScale, yMin);
            DrawAxes(context, leftMargin, topMargin, bottomMargin, graphWidth);
            DrawTicks(context, leftMargin, topMargin, bottomMargin, yScale, yMin, yMax, yStep, graphWidth);
        }

        private void DrawAxes(DrawingContext context, double leftMargin, double topMargin,
                            double bottomMargin, double graphWidth)
        {
            //Lower opacity so that if the graph is 0 all the way through it's at least somewhat visible
            var axisPen = new Pen(new SolidColorBrush(AxesColor, 0.8), 1);

            // Y-axis
            var yStart = new Point(leftMargin, Bounds.Height - bottomMargin);
            var yEnd = new Point(leftMargin, topMargin);
            context.DrawLine(axisPen, yStart, yEnd);

            // X-axis
            var xEnd = new Point(leftMargin + graphWidth, Bounds.Height - bottomMargin);
            context.DrawLine(axisPen, yStart, xEnd);
        }

        private void DrawTicks(DrawingContext context, double leftMargin, double topMargin,
                             double bottomMargin, double yScale, double yMin, double yMax,
                             double yStep, double graphWidth)
        {
            var tickBrush = new SolidColorBrush(AxesColor);
            var tickPen = new Pen(tickBrush, 1);

            for (int i = 0; i < NumberOfTicks; i++)
            {
                double yValue = yMin + i * yStep;
                var formattedText = CreateFormattedText(yValue, tickBrush);

                double yPosition = Bounds.Height - bottomMargin - (yValue - yMin) * yScale;

                // Draw tick mark
                context.DrawLine(tickPen,
                    new Point(leftMargin - 3, yPosition),
                    new Point(leftMargin, yPosition));

                // Draw horizontal grid line
                context.DrawLine(tickPen,
                    new Point(leftMargin, yPosition),
                    new Point(leftMargin + graphWidth, yPosition));

                // Draw label
                context.DrawText(
                    formattedText,
                    new Point(leftMargin - formattedText.Width - labelSpacing, yPosition - formattedText.Height / 2));
            }
        }

        private void DrawLine(DrawingContext context, double leftMargin, double topMargin, double bottomMargin, double xStep, double yScale, double yMin)
        {
            var pen = new Pen(new SolidColorBrush(LineColor), 2);
            var geometry = new StreamGeometry();

            using (var contextStream = geometry.Open())
            {
                bool isFirstPoint = true;

                for (int i = 0; i < Values.Count; i++)
                {
                    var point = new Point(leftMargin + i * xStep, Bounds.Height - bottomMargin - (Values[i] - yMin) * yScale);

                    // Clamp the Y value to ensure it stays within the graph bounds
                    double clampedY = Math.Max(topMargin, Math.Min(Bounds.Height - bottomMargin, point.Y));

                    if (point.Y != clampedY)
                    {
                        Debug.WriteLine($"Clamping point.Y from {point.Y} to {clampedY} - due to value: {Values[i]} (upper limit: {TopLimit})");
                        point = new Point(point.X, clampedY);
                    }

                    if (isFirstPoint)
                    {
                        contextStream.BeginFigure(point, false);
                        isFirstPoint = false;
                    }
                    else
                    {
                        contextStream.LineTo(point);
                    }
                }

                contextStream.EndFigure(false);
            }

            context.DrawGeometry(null, pen, geometry);
        }



        private double RoundUpToNearestNiceValue(double value)
        {
            double[] niceValues = { 1, 2, 5, 10, 20, 50, 100, 200, 500, 1000, 2000, 5000, 10000, 20000, 50000 };
            foreach (var niceValue in niceValues)
            {
                if (value <= niceValue)
                    return niceValue;
            }
            return value;
        }
    }
}