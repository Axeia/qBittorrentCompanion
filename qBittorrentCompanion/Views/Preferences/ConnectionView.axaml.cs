using Avalonia.Controls;
using System.Diagnostics;

namespace qBittorrentCompanion.Views.Preferences
{
    public partial class ConnectionView : UserControl
    {
        public ConnectionView()
        {
            InitializeComponent();
        }

        private void DataGrid_BeginningEdit(object? sender, Avalonia.Controls.DataGridBeginningEditEventArgs e)
        {
            Debug.WriteLine("EDIT");
        }
    }
}
