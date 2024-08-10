using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
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

        public void RandomizePortButton_Click(object sender, RoutedEventArgs args)
        {
            Random random = new Random();
            ListenPortNumericUpDown.Value = random.Next(1024, 65535);
        }
    }
}
