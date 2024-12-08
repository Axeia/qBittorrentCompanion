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
        public static readonly StyledProperty<int> NumberOfTicksProperty =
            AvaloniaProperty.Register<LineGraph, int>(nameof(NumberOfTicks), defaultValue: 5);

        public static readonly StyledProperty<Color> AxesColorProperty =
            AvaloniaProperty.Register<LineGraph, Color>("AxesColor", defaultValue: ThemeColors.SystemAltHigh);
        public Color AxesColor
        {
            get => GetValue(AxesColorProperty);
            set => SetValue(AxesColorProperty, value);
        }

        public static readonly StyledProperty<Color> LineColorProperty =
            AvaloniaProperty.Register<LineGraph, Color>("LineColor", defaultValue: ThemeColors.SystemAccent);
        public Color LineColor
        {
            get => GetValue(LineColorProperty);
            set => SetValue(LineColorProperty, value);
        }

        public static readonly StyledProperty<Color> SecondLineColorProperty =
            AvaloniaProperty.Register<LineGraph, Color>("SecondLineColor", defaultValue: ThemeColors.SystemErrorText);
        public Color SecondLineColor
        {
            get => GetValue(SecondLineColorProperty);
            set => SetValue(SecondLineColorProperty, value);
        }

        public static readonly StyledProperty<long?> TopLimitProperty =
            AvaloniaProperty.Register<LineGraph, long?>("TopLimit", defaultValue: null);
        public long? TopLimit
        {
            get => GetValue(TopLimitProperty);
            set => SetValue(TopLimitProperty, value);
        }

        public static readonly StyledProperty<long?> BottomLimitProperty =
            AvaloniaProperty.Register<LineGraph, long?>("BottomLimit", defaultValue: 0);
        public long? BottomLimit
        {
            get => GetValue(BottomLimitProperty);
            set => SetValue(BottomLimitProperty, value);
        }

        public int labelSpacing = 5;

        public static readonly DirectProperty<LineGraph, string> FormatSizeAsProperty =
            AvaloniaProperty.RegisterDirect<LineGraph, string>(
                nameof(FormatSizeAs),
                o => o.FormatSizeAs,
                (o, v) => o.FormatSizeAs = v);

        private string _formatSizeAs;
        public string FormatSizeAs
        {
            get => _formatSizeAs;
            set
            {
                if (_formatSizeAs != value)
                {
                    _formatSizeAs = value;
                    Dispatcher.UIThread.Post(InvalidateVisual); // Ensure initial render
                }
            }
        }

        public static readonly DirectProperty<LineGraph, ObservableCollection<long>> ValuesProperty =
            AvaloniaProperty.RegisterDirect<LineGraph, ObservableCollection<long>>(
                nameof(Values),
                o => o.Values,
                (o, v) => o.Values = v);

        private ObservableCollection<long> _values;
        public ObservableCollection<long> Values
        {
            get => _values;
            set
            {
                if (_values != null)
                    _values.CollectionChanged -= Values_CollectionChanged;

                _values = value;

                if (_values != null)
                {
                    _values.CollectionChanged += Values_CollectionChanged;
                    Debug.WriteLine("\n\nLineGraph subscribed to new Values collection\n\n");
                    Dispatcher.UIThread.Post(InvalidateVisual); // Ensure initial render
                }
            }
        }

        public static readonly DirectProperty<LineGraph, ObservableCollection<long>> SecondValuesProperty =
            AvaloniaProperty.RegisterDirect<LineGraph, ObservableCollection<long>>(
                nameof(SecondValues),
                o => o.SecondValues,
                (o, v) => o.SecondValues = v);

        private ObservableCollection<long> _secondValues;
        public ObservableCollection<long> SecondValues
        {
            get => _secondValues;
            set
            {
                if (_secondValues != null)
                    _secondValues.CollectionChanged -= Values_CollectionChanged;

                _secondValues = value;

                if (_secondValues != null)
                {
                    _secondValues.CollectionChanged += Values_CollectionChanged;
                    Debug.WriteLine("\n\nLineGraph subscribed to new SecondValues collection\n\n");
                    Dispatcher.UIThread.Post(InvalidateVisual); // Ensure initial render
                }
            }
        }

        public int NumberOfTicks
        {
            get => GetValue(NumberOfTicksProperty);
            set => SetValue(NumberOfTicksProperty, value);
        }

        public LineGraph()
        {
        }

        private void Values_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Ensure it's on the UI thread
            Dispatcher.UIThread.Post(() => { InvalidateVisual(); });
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

            double multiplier = DataConverter.GetMultiplierForUnit(FormatSizeAs);
            double yMax = (TopLimit ?? RoundUpToNearestNiceValue(Math.Max(Values.Max(), SecondValues?.Max() ?? 0))) / (double)multiplier;
            double yMin = (BottomLimit ?? Math.Min(Values.Min(), SecondValues?.Min() ?? long.MaxValue)) / (double)multiplier;
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
                double yValue = yMin + i * yStep;  // Change long to double
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

            DrawLine(context, leftMargin, topMargin, bottomMargin, xStep, yScale, yMin, Values, LineColor);
            if (SecondValues != null)
            {
                double secondXStep = graphWidth / (SecondValues.Count - 1);
                DrawLine(context, leftMargin, topMargin, bottomMargin, secondXStep, yScale, yMin, SecondValues, SecondLineColor);
            }
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
        private void DrawLine(DrawingContext context, double leftMargin, double topMargin, double bottomMargin, double xStep, double yScale, double yMin, ObservableCollection<long> values, Color lineColor)
        {
            var pen = new Pen(new SolidColorBrush(lineColor), 2);
            var geometry = new StreamGeometry();

            double multiplier = DataConverter.GetMultiplierForUnit(FormatSizeAs);

            using (var contextStream = geometry.Open())
            {
                bool isFirstPoint = true;

                for (int i = 0; i < values.Count; i++)
                {
                    double yValue = values[i] / multiplier;
                    var point = new Point(leftMargin + i * xStep, Bounds.Height - bottomMargin - (yValue - yMin) * yScale);

                    // Clamp the Y value to ensure it stays within the graph bounds
                    double clampedY = Math.Max(topMargin, Math.Min(Bounds.Height - bottomMargin, point.Y));

                    if (point.Y != clampedY)
                    {
                        Debug.WriteLine($"Clamping point.Y from {point.Y} to {clampedY} - due to value: {values[i]} (upper limit: {TopLimit})");
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


        private long RoundUpToNearestNiceValue(long value)
        {
            long[] niceValues = { 1, 2, 5, 10, 20, 50, 100, 200, 500, 1000, 2000, 5000, 10000, 20000, 50000 };
            foreach (var niceValue in niceValues)
            {
                if (value <= niceValue)
                    return niceValue;
            }
            return value;
        }
    }
}