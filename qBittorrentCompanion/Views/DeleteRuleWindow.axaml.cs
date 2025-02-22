using Avalonia.Interactivity;
using qBittorrentCompanion.ViewModels;
using System.Diagnostics;

namespace qBittorrentCompanion.Views
{

    public partial class DeleteRuleWindow : IcoWindow
    {
        //Needed for previewer
        public DeleteRuleWindow()
        {
            InitializeComponent();
        }

        public void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
