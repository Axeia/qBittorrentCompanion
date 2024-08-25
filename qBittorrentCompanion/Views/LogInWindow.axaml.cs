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
        }


        public LogInWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            DataContext = new LogInWindowViewModel();
            _mainWindow = mainWindow; // Set the client field to the passed client

            if(DataContext is LogInWindowViewModel LogInViewModel)
            {
                LogInViewModel.AttemptingLogIn += LogInViewModel_AttemptingLogIn;
                LogInViewModel.LogInSuccess += LogInViewModel_LogInSuccess;
                LogInViewModel.LogInFailure += LogInViewModel_LogInFailure;
            }

            //If the login window is manually closed, close the entire app.
            //this.Closed += (_, __) => mainWindow.Close();
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
