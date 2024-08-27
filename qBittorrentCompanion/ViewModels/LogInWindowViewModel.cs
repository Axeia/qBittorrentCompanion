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
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
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
                this.RaisePropertyChanged(nameof(QBittorrentUrl));
            }
        }

        private int _port = 8080;
        [Required]
        public int Port
        {
            get => _port;
            set
            {
                this.RaiseAndSetIfChanged(ref _port, value);
                this.RaisePropertyChanged(nameof(IsValid));
                this.RaisePropertyChanged(nameof(QBittorrentUrl));
            }
        }

        public string QBittorrentUrl
        {
            get => $"http://{Ip}:{Port}";
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


            this.WhenAnyValue(x => x.Ip)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(ip =>
                {
                    _ValidateQBittorrentUri();
                });

            this.WhenAnyValue(x => x.Port)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(port =>
                {
                    _ValidateQBittorrentUri();
                });
        }

        private bool _isCheckingQBittorrentUri = false;
        public bool IsCheckingQBittorrentUri
        {
            get => _isCheckingQBittorrentUri;
            set => this.RaiseAndSetIfChanged(ref _isCheckingQBittorrentUri, value);
        }

        private string _qbittorrentUriInvalidString = " is not a valid url";

        private bool _isValidQBittorrentUri = true;
        public bool IsValidQBittorrentUri
        {
            get => _isValidQBittorrentUri;
            set => this.RaiseAndSetIfChanged(ref _isValidQBittorrentUri, value);
        }

        private void _ValidateQBittorrentUri()
        {
            IsValidQBittorrentUri = Uri.IsWellFormedUriString(QBittorrentUrl, UriKind.Absolute);
            if (IsValidQBittorrentUri)
            {
                _ = _CheckForQbittorrentAsync(QBittorrentUrl);
            }
        }

        private async Task _CheckForQbittorrentAsync(string uri)
        {
            IsCheckingQBittorrentUri = true;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(uri);
                    response.EnsureSuccessStatusCode();
                    string content = await response.Content.ReadAsStringAsync();

                    if (content.Contains("qbittorrent"))
                    {
                        IsValidQBittorrentUri = true;
                    }
                    else
                    {
                        IsValidQBittorrentUri = false;
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP Request error: {httpEx.Message}");
                IsValidQBittorrentUri = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                IsValidQBittorrentUri = false;
            }
            finally
            {
                IsCheckingQBittorrentUri = false;
            }
        }
    }
}
