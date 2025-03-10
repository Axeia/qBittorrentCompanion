using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using qBittorrentCompanion.ViewModels;
using System.Linq;

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