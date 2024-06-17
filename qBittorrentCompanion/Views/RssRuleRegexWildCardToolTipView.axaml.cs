using Avalonia;
using Avalonia.Controls;

namespace qBittorrentCompanion.Views
{
    public partial class RssRuleRegexWildCardToolTipView : UserControl
    {
        public static readonly DirectProperty<RssRuleRegexWildCardToolTipView, bool> IsRegexUsedProperty =
            AvaloniaProperty.RegisterDirect<RssRuleRegexWildCardToolTipView, bool>(
                nameof(IsRegexUsed),
                o => o.IsRegexUsed,
                (o, v) => o.IsRegexUsed = v);

        private bool _isRegexUsed;

        public bool IsRegexUsed
        {
            get { return _isRegexUsed; }
            set { SetAndRaise(IsRegexUsedProperty, ref _isRegexUsed, value); }
        }

        public RssRuleRegexWildCardToolTipView()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
