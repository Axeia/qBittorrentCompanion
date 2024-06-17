using Avalonia.Controls;
using Avalonia.Media;
using Avalonia;
using System.Collections.Generic;
using Avalonia.Controls.Shapes;
using QBittorrent.Client;

namespace qBittorrentCompanion.CustomControls
{
    public class PiecesProgressBar : UserControl
    {
        private readonly Canvas _canvas = new();

        public static readonly StyledProperty<IReadOnlyList<TorrentPieceState>> PieceStatesProperty =
            AvaloniaProperty.Register<PiecesProgressBar, IReadOnlyList<TorrentPieceState>>(nameof(PieceStates));

        public new static readonly StyledProperty<IBrush> BorderBrushProperty =
            AvaloniaProperty.Register<PiecesProgressBar, IBrush>(nameof(BorderBrush), defaultValue: Brushes.Black);

        public new static readonly StyledProperty<double> BorderThicknessProperty =
            AvaloniaProperty.Register<PiecesProgressBar, double>(nameof(BorderThickness), defaultValue: 2.0);

        public static readonly StyledProperty<Color> InProgressColorProperty =
            AvaloniaProperty.Register<PiecesProgressBar, Color>("", defaultValue: Colors.Green);

        public static readonly StyledProperty<Color> DoneColorProperty =
            AvaloniaProperty.Register<PiecesProgressBar, Color>("", defaultValue: Colors.Blue);

        public IReadOnlyList<TorrentPieceState> PieceStates
        {
            get { return GetValue(PieceStatesProperty); }
            set { SetValue(PieceStatesProperty, value); }
        }

        public new IBrush BorderBrush
        {
            get { return GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }

        public new double BorderThickness
        {
            get { return GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        public Color InProgressColor
        {
            get { return GetValue(InProgressColorProperty); }
            set { SetValue(InProgressColorProperty, value); }
        }

        public Color DoneColor
        {
            get { return GetValue<Color>(DoneColorProperty); }
            set { SetValue(DoneColorProperty, value); }
        }

        public PiecesProgressBar()
        {
            PieceStatesProperty.Changed.AddClassHandler<PiecesProgressBar>((_, __) => UpdateCanvas());
            this.Content = _canvas;
        }

        private void UpdateCanvas()
        {
            _canvas.Children.Clear();

            if (PieceStates == null)
                return;

            double pieceWidth = _canvas.Bounds.Width / PieceStates.Count;
            for (int i = 0; i < PieceStates.Count; i++)
            {
                var color = PieceStates[i] switch
                {
                    TorrentPieceState.NotDownloaded => Colors.Transparent,
                    TorrentPieceState.Downloading => GetValue(InProgressColorProperty),
                    TorrentPieceState.Downloaded => GetValue(DoneColorProperty),
                    _ => Colors.Transparent,
                };
                var rect = new Rectangle
                {
                    Width = pieceWidth,
                    Height = _canvas.Bounds.Height,
                    Fill = new SolidColorBrush(color),
                };
                Canvas.SetLeft(rect, i * pieceWidth);
                _canvas.Children.Add(rect);
            }
        }
    }
}