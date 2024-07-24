using Avalonia.Controls;
using System.Diagnostics;

namespace qBittorrentCompanion.Views.Preferences
{
    public partial class WebUiView : UserControl
    {
        public WebUiView()
        {
            InitializeComponent();
            if(this.VisualRoot is Window window)
            {
                Debug.WriteLine("Bingo - window");
                Debug.WriteLine(window);
            }
            else
            {
                Debug.WriteLine("WebUiView.axaml.cs's visual root unexpectedly is:");
                Debug.WriteLine(this.VisualRoot);
                //Debug.WriteLine(this.VisualRoot ?? "null" : VisualRoot!.GetType);
            }
        }
    }
}
