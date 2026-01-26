using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Services;
using qBittorrentCompanion.Validators;
using qBittorrentCompanion.Views;
using RaiseChangeGenerator;
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

    public partial class LogInWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public event Action<string>? LogInSuccess;
        public event Action? AttemptingLogIn;
        public event Action? LogInFailure;

        private static bool _isLoggedIn = false;
        public static bool IsLoggedIn => _isLoggedIn;

        [RaiseChange]
        private string _savedLoginInfoStatus = "No saved login info found, fields have default values";

        private bool _useHttps = false;
        public bool UseHttps
        {
            get => _useHttps;
            set
            {
                if (value != _useHttps)
                {
                    _useHttps = value;
                    this.RaisePropertyChanged(nameof(UseHttps));
                    this.RaisePropertyChanged(nameof(QBittorrentUrl));
                    this.RaisePropertyChanged(nameof(IsValid));
                }
            }
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

        private string _address = "192.168.1.100";
        [Required]
        [ValidIpOrAddress]
        public string Address
        {
            get => _address;
            set
            {
                this.RaiseAndSetIfChanged(ref _address, value);
                this.RaisePropertyChanged(nameof(QBittorrentUrl));
                this.RaisePropertyChanged(nameof(IsValid));
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
                this.RaisePropertyChanged(nameof(QBittorrentUrl));
                this.RaisePropertyChanged(nameof(IsValid));
            }
        }

        public string QBittorrentUrl => 
            $"{(UseHttps ? "https" : "http")}://{Address}:{Port}";

        [RaiseChange]
        private bool _saveLogInData = false;

        public ReactiveCommand<Unit, Unit> LogInCommand =>
            ReactiveCommand.Create(LogIn, this.WhenAnyValue(x => x.IsValid));

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
            SecureStorage.SaveData(Username, Password, Address, Port.ToString(), UseHttps);
            _ = AttemptLogin();
        }

        public bool IsLoggingIn { get; private set; }

        public async Task AttemptLogin()
        {
            AttemptingLogIn!.Invoke();
            try
            {
                await QBittorrentService.Authenticate(
                    Username, Password, Address, Port.ToString(), UseHttps
                );

                // This point won't be reached unless the login was successfull
                // (the above hasn't errored out)
                _isLoggedIn = true;
                LogInSuccess!.Invoke(Username);

                if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                    && desktop.MainWindow is MainWindow mw
                    && mw.DataContext is MainWindowViewModel mwvm)
                {
                    //mw.StoreLoginData;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                LogInFailure!.Invoke();
            }
        }

        public LogInWindowViewModel()
        {
            try // Present the user with previously entered (and saved) data
            {
                if (Design.IsDesignMode)
                {

                }
                else
                {
                    // Can throw NoLoginDataException
                    (string username, string password, string ip, string port, bool useHttps) = SecureStorage.LoadData();

                    Username = username;
                    Password = password;
                    Address = ip;
                    Port = int.Parse(port);
                    UseHttps = useHttps;
                    SaveLogInData = true; // If the data was saved before the user is likely to want to keep doing that.

                    SavedLoginInfoStatus = "Fields populated with previously saved data";
                }
            }
            catch (NoLoginDataException)
            {
                Debug.WriteLine("No login data present");
                // Expected on first launch - ignore.
            }

            this.WhenAnyValue(x => x.Address)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(ip =>
                {
                    ValidateQBittorrentUri();
                });

            this.WhenAnyValue(x => x.Port)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(port =>
                {
                    ValidateQBittorrentUri();
                });

            this.WhenAnyValue(x => x.UseHttps)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(useHttps =>
                {
                    ValidateQBittorrentUri();
                });
        }

        [RaiseChange]
        private bool _isCheckingQBittorrentUri = false;

        [RaiseChange]
        private bool _isValidQBittorrentUri = true;

        private void ValidateQBittorrentUri()
        {
            IsValidQBittorrentUri = Uri.IsWellFormedUriString(QBittorrentUrl, UriKind.Absolute);
            if (IsValidQBittorrentUri)
            {
                _ = CheckForQbittorrentAsync(QBittorrentUrl);
            }
        }

        private async Task CheckForQbittorrentAsync(string uri)
        {
            IsCheckingQBittorrentUri = true;

            if (Design.IsDesignMode)
                return;

            try
            {
                using HttpClient client = new();
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
