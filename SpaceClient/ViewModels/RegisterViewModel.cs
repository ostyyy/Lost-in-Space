using SpaceClient.View;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DotNetEnv;

namespace SpaceClient.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private string _login = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _statusMessage = string.Empty;
        private Brush _statusColor = Brushes.Transparent;

        // Binding properties for the registration form and status display
        public string Login
        {
            get => _login;
            set
            {
                _login = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public Brush StatusColor
        {
            get => _statusColor;
            set
            {
                _statusColor = value;
                OnPropertyChanged();
            }
        }

        public ICommand NavigateToLoginCommand { get; }
        public ICommand RegisterCommand { get; }

        public RegisterViewModel()
        {
            NavigateToLoginCommand = new Actions(ExecuteNavigateToLogin);
            RegisterCommand = new Actions(ExecuteRegister, CanExecuteRegister);

            StatusColor = Brushes.Transparent;
            StatusMessage = string.Empty;
        }

        private bool CanExecuteRegister(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Login) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !string.IsNullOrWhiteSpace(ConfirmPassword);
        }

        private async void ExecuteRegister(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                StatusColor = Brushes.Crimson;
                StatusMessage = "ERROR: ALL SYSTEM FIELDS MUST BE INITIALIZED!";
                return;
            }

            if (Password != ConfirmPassword)
            {
                StatusColor = Brushes.Crimson;
                StatusMessage = "ERROR: PASSWORDS DO NOT MATCH!";
                return;
            }

            try
            {
                StatusColor = Brushes.Cyan;
                StatusMessage = "CONNECTING...";

                DotNetEnv.Env.Load();
                string baseUrl = DotNetEnv.Env.GetString("API_BASE_URL");
                if (string.IsNullOrEmpty(baseUrl)) baseUrl = "http://localhost:5000";

                string url = $"{baseUrl.TrimEnd('/')}/api/users/register";

                var registerData = new { Login = Login.Trim(), Password = Password };
                string jsonPayload = JsonSerializer.Serialize(registerData);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.PostAsync(url, content);
                    string responseText = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        StatusColor = Brushes.LimeGreen;
                        StatusMessage = "ACCOUNT CREATED!";

                        await System.Threading.Tasks.Task.Delay(1200);

                        ExecuteNavigateToLogin(null);
                    }
                    else
                    {
                        StatusColor = Brushes.Crimson;
                        try
                        {
                            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                            var errorObj = JsonSerializer.Deserialize<Dictionary<string, string>>(responseText, options);

                            if (errorObj != null && errorObj.TryGetValue("message", out string? serverMessage))
                            {
                                StatusMessage = $"SERVER ERROR: {serverMessage.ToUpper()}";
                            }
                            else
                            {
                                StatusMessage = $"SERVER ERROR: {responseText.ToUpper()}";
                            }
                        }
                        catch
                        {
                            StatusMessage = $"SERVER ERROR: {response.ReasonPhrase?.ToUpper()}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StatusColor = Brushes.Crimson;
                StatusMessage = $"ERROR: {ex.InnerException?.Message ?? ex.Message}";
            }
        }

        private void ExecuteNavigateToLogin(object parameter)
        {
            LogIn loginWindow = new LogIn();
            loginWindow.Show();

            foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
            {
                if (window is SpaceClient.View.RegisterWindow)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}