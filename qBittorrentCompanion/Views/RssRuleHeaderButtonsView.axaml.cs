using Avalonia;
using Avalonia.Controls;

namespace qBittorrentCompanion.Views
{
    public partial class RssRuleHeaderButtonsView : UserControl
    {
        public static readonly DirectProperty<RssRuleHeaderButtonsView, bool> IsDeletableProperty =
            AvaloniaProperty.RegisterDirect<RssRuleHeaderButtonsView, bool>(
                nameof(IsDeletable),
                o => o.IsDeletable,
                (o, v) => o.IsDeletable = v);

        private bool _isDeletable;

        public bool IsDeletable
        {
            get { return _isDeletable; }
            set { SetAndRaise(IsDeletableProperty, ref _isDeletable, value); }
        }

        public RssRuleHeaderButtonsView()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
