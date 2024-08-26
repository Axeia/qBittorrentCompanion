using Avalonia.Controls;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Services;
using qBittorrentCompanion.Validators;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reactive;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels
{
    public class LogInWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public event Action LogInSuccess;
        public event Action AttemptingLogIn;
        public event Action LogInFailure;

        private SecureStorage _secureStorage = Design.IsDesignMode ? null! : new SecureStorage();


        private string _savedLoginInfoStatus = "No saved login info found, fields have default values";
        public string SavedLoginInfoStatus
        {
            get => _savedLoginInfoStatus;
            set => this.RaiseAndSetIfChanged(ref _savedLoginInfoStatus, value);
        }

        private string _username = "admin";
        [Required]
        public string Username
        {
            get => _username;
            set
            {
                this.RaiseAndSetIfChanged(ref _username, value);
                this.RaisePropertyChanged(nameof(IsValid));
            }
        }

        private string _password = "";
        [Required]
        public string Password
        {
            get => _password;
            set
            {
                this.RaiseAndSetIfChanged(ref _password, value);
                this.RaisePropertyChanged(nameof(IsValid));
            }
        }

        private string _ip = "192.168.1.100";
        [Required]
        [ValidIp]
        public string Ip
        {
            get => _ip;
            set
            {
                this.RaiseAndSetIfChanged(ref _ip, value);
                this.RaisePropertyChanged(nameof(IsValid));
            }
        }

        private int _port = 8088;
        [Required]
        public int Port
        {
            get => _port;
            set
            {
                this.RaiseAndSetIfChanged(ref _port, value);
                this.RaisePropertyChanged(nameof(IsValid));
            }
        }

        private bool _saveLogInData = false;
        public bool SaveLogInData
        {
            get => _saveLogInData;
            set => this.RaiseAndSetIfChanged(ref _saveLogInData, value);
        }

        public ReactiveCommand<Unit, Unit> LogInCommand { get; }
        public bool IsValid
        {
            get
            {
                var validationContext = new ValidationContext(this, null, null);
                var validationResults = new List<ValidationResult>();
                return Validator.TryValidateObject(this, validationContext, validationResults, true);
            }
        }

        private void LogIn()
        {
            _secureStorage.SaveData(Username, Password, Ip, Port.ToString());
            _ = AttemptLogin();
        }

        public bool IsLoggingIn { get; private set; }

        public async Task AttemptLogin()
        {
            AttemptingLogIn.Invoke();
            try
            {
                await QBittorrentService.Authenticate(
                    Username, Password, Ip, Port.ToString()
                );

                // This point won't be reached unless the login was successfull
                // (the above doesn't error out)
                LogInSuccess.Invoke();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                LogInFailure.Invoke();
            }
        }

        public LogInWindowViewModel()
        {
            LogInCommand = ReactiveCommand.Create(LogIn, this.WhenAnyValue(x => x.IsValid));

            if (_secureStorage.HasSavedData())
                SavedLoginInfoStatus = "Fields populated with previously saved data";

            try // Load stored data for ease of editing.
            {
                if (Design.IsDesignMode)
                {

                }
                else
                {
                    (string username, string password, string ip, string port) = _secureStorage.LoadData();
                    Username = username;
                    Password = password;
                    Ip = ip;
                    Port = int.Parse(port);
                    SaveLogInData = true; // If the data was saved before the user is likely to want to keep doing that.
                }
            }
            catch (NoLoginDataException)
            {
                Debug.WriteLine("No login data present");
                // Expected on first launch - ignore.
            }
        }
    }
}
