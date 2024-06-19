using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using qBittorrentCompanion.Helpers;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using qBittorrentCompanion.Services;
using qBittorrentCompanion.ViewModels; // Add this

namespace qBittorrentCompanion.Views
{
    public partial class LogInWindow : Window
    {
        private MainWindow _mainWindow;

        /**
         * Keeps Avalonia axaml preview happy - not used in the program.
         */
        public LogInWindow()
        {
            InitializeComponent();

            DataContext = new LogInWindowViewModel();
            _mainWindow = new MainWindow();
            AddHelpHtml();
        }


        public LogInWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            DataContext = new LogInWindowViewModel();
            _mainWindow = mainWindow; // Set the client field to the passed client
            AddHelpHtml();

            if(DataContext is LogInWindowViewModel LogInViewModel)
            {
                LogInViewModel.AttemptingLogIn += LogInViewModel_AttemptingLogIn;
                LogInViewModel.LogInSuccess += LogInViewModel_LogInSuccess;
                LogInViewModel.LogInFailure += LogInViewModel_LogInFailure;
            }

            //If the login window is manually closed, close the entire app.
            //this.Closed += (_, __) => mainWindow.Close();
        }

        private void AddHelpHtml()
        {
            string html = @"<ol>
<li>Ensure qBittorrent is running on your 'server' Windows users can download it from <a href='https://qbittorrent.org'>qbittorrent.org</a> - Linux users check your distro's repositories.</li>
<li>Ensure the server qBittorrent instance has the WebUI enabled - you can find this under Options » Tools » WebUI</li>
<li>Test the UI is working by visiting http://127.0.0.1:8080 (8080 is the port, if you changed it, change it for the URL as well)</li>
<li>After having confirmed the WebUI works on the server, figure out what the ip is to reach the server (see <a href='https://ipecho.net/localip.html'>ipecho.net/localip.html</a> on how to do so)</a></li>
<li>From this device (the one running qBittorrent Companion) confirm the server is reachable by visiting the servers IP on the port you figured out earlier - for example <a href='http://192.168.1.100:8080'>http://192.168.1.100:8080</a> or something along those lines</a></li>
<li>If that all works, please enter the same IP/port under the Login Tab in this window</li>
</ol>
            ";
            htmlBlock.Text = html;
        }

        private void LogInViewModel_AttemptingLogIn()
        {
            //Disable controls to indicate submission has happened
            this.IsEnabled = false;
            //Show the progressbar to indicate the login process is in progress
            //SubmittingProgressBar.IsVisible = true;
            //SubmittingProgressBar.IsEnabled = true;
        }

        private void LogInViewModel_LogInFailure()
        {
            // Looks like the login didn't work, re-enable controls so the user can try again.
            // Also, hide the progressbar.
            this.IsEnabled = true;
            //SubmittingProgressBar.IsVisible = false;
            //SubmittingProgressBar.IsEnabled = false;
        }

        private void LogInViewModel_LogInSuccess()
        {
            if (DataContext is LogInWindowViewModel LogInViewModel)
            {
                // Ensure proper clean up
                LogInViewModel.AttemptingLogIn += LogInViewModel_AttemptingLogIn;
                LogInViewModel.LogInSuccess += LogInViewModel_LogInSuccess;
                LogInViewModel.LogInFailure += LogInViewModel_LogInFailure;

                //And done - logged in and the garbage collector should be able to tidy up the ViewModel
                this.Close();
            }
        }

        private void ViewModel_LoggedIn()
        {
            if (DataContext is LogInWindowViewModel viewModel)
            {

                this.Close();
            }
        }

    }
}
