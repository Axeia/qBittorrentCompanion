using Avalonia.Controls;
using qBittorrentCompanion.ViewModels;

namespace qBittorrentCompanion.Views;

public partial class RssPluginInfoView : UserControl
{
    public RssPluginInfoView()
    {
        InitializeComponent();

        if(Design.IsDesignMode)
        {
            DataContext = new RssPluginSupportBaseViewModel();
        }
    }
}