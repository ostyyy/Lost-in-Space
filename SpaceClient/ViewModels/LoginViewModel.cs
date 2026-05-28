using SpaceClient.View;
using System;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpaceClient.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private string _login = string.Empty;
        public string Login
        {
            get => _login;
            set
            {
                _login = value;
                OnPropertyChanged(nameof(Login));
            }
        }

        private string _statusMessage = string.Empty;

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new Actions(LoggingIng, CanLogin);
        }

        private bool CanLogin(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Login);
        }

        private async void LoggingIng(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            var password = passwordBox?.Password;

            if (string.IsNullOrWhiteSpace(password))
            {
                StatusMessage = "Enter Password!";
                return;
            }

            StatusMessage = "Connecting...";

            var userCredentials = new
            {
                login = this.Login,
                password = password
            };

            var jsonText = JsonSerializer.Serialize(userCredentials);
            var content = new StringContent(jsonText, Encoding.UTF8, "application/json");

            using var client = new HttpClient();

            try
            {
                string baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL");

                if (string.IsNullOrEmpty(baseUrl))
                {
                    StatusMessage = "Error: API_BASE_URL is missing in .env!";
                    return;
                }

                string apiUrl = $"{baseUrl}/api/users/login";

                var response = await client.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    StatusMessage = "Entering!";

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        NavigationWindow profileWindow = new NavigationWindow();
                        profileWindow.Show();

                        foreach (Window window in Application.Current.Windows)
                        {
                            if (window.DataContext == this)
                            {
                                window.Close();
                                break;
                            }
                        }
                    });
                }
                else
                {
                    StatusMessage = "Incorrect login or password!";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "Server error or offline!";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}