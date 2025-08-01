﻿using AutoPropertyChangedGenerator;
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
    public partial class LogInWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public event Action<string>? LogInSuccess;
        public event Action? AttemptingLogIn;
        public event Action? LogInFailure;

        private SecureStorage _secureStorage = Design.IsDesignMode ? null! : new SecureStorage();
        private static bool _isLoggedIn = false;
        public static bool IsLoggedIn => IsLoggedIn;

        [AutoPropertyChanged]
        private string _savedLoginInfoStatus = "No saved login info found, fields have default values";

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

        [AutoPropertyChanged]
        private bool _saveLogInData = false;

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
            AttemptingLogIn!.Invoke();
            try
            {
                await QBittorrentService.Authenticate(
                    Username, Password, Ip, Port.ToString()
                );

                // This point won't be reached unless the login was successfull
                // (the above doesn't error out)
                _isLoggedIn = true;
                LogInSuccess!.Invoke(Username);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                LogInFailure!.Invoke();
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

        [AutoPropertyChanged]
        private bool _isCheckingQBittorrentUri = false;

        [AutoPropertyChanged]
        private bool _isValidQBittorrentUri = true;

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
