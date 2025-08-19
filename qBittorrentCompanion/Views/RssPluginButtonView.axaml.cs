using Avalonia.Controls;
using qBittorrentCompanion.ViewModels;

namespace qBittorrentCompanion.Views;

public partial class RssPluginButtonView : UserControl
{
    public RssPluginButtonView()
    {
        InitializeComponent();
        if(Design.IsDesignMode)
        {
            DataContext = new RssPluginSupportBaseViewModel();
        }
    }
}