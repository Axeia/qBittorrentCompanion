using Avalonia.Controls;
using System.Diagnostics;
using qBittorrentCompanion.ViewModels;

namespace qBittorrentCompanion.Views
{
    public partial class LogInWindow : IcoWindow
    {
        private readonly Window? _mainWindow;

        /**
         * Keeps Avalonia axaml preview happy - not used in the program.
         */
        public LogInWindow()
        {
            InitializeComponent();
            DataContext = new LogInWindowViewModel();
        }

        public LogInWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            DataContext = new LogInWindowViewModel();
            _mainWindow = mainWindow;

            if (DataContext is LogInWindowViewModel logInViewModel)
            {
                logInViewModel.AttemptingLogIn += LogInViewModel_AttemptingLogIn;
                logInViewModel.LogInSuccess += LogInViewModel_LogInSuccess;
                logInViewModel.LogInFailure += LogInViewModel_LogInFailure;
            }

            Closed += (_, _) =>
            {
                if (DataContext is LogInWindowViewModel vm)
                {
                    vm.AttemptingLogIn -= LogInViewModel_AttemptingLogIn;
                    vm.LogInSuccess -= LogInViewModel_LogInSuccess;
                    vm.LogInFailure -= LogInViewModel_LogInFailure;
                }
            };
        }

        private void LogInViewModel_AttemptingLogIn()
        {
            //Disable controls to indicate submission has happened
            this.IsEnabled = false;
        }

        private void LogInViewModel_LogInFailure()
        {
            // Looks like the login didn't work, re-enable controls so the user can try again.
            this.IsEnabled = true;
        }
        private void LogInViewModel_LogInSuccess(string username)
        {
            if (_mainWindow?.DataContext is MainWindowViewModel mwvm)
                mwvm.Username = username;

            Close(true);
        }
    }
}