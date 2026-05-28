using SpaceClient.View;
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

        

        private string _login;
        private string _password;
        private string _confirmPassword;
        private string _statusMessage;
        private Brush _statusColor;


        // Binding properties for the registration form and status display
        public string Login
        {
            get => _login;
            set
            {
                _login = value;
                OnPropertyChanged(); 
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set { _confirmPassword = value; OnPropertyChanged(); }
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

        // Commands for navigation and registration actions
        public ICommand NavigateToLoginCommand { get; }
        public ICommand RegisterCommand { get; }


        public RegisterViewModel() {
            NavigateToLoginCommand = new Actions(ExecuteNavigateToLogin);

            RegisterCommand = new Actions(ExecuteRegister, CanExecuteRegister);

            StatusColor = Brushes.Transparent;
            StatusMessage = string.Empty;
        }

        // Registration logic with validation and status updates
        private bool CanExecuteRegister(object parameter)
        {
            return true;
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
                DotNetEnv.Env.Load();

                string baseUrl = DotNetEnv.Env.GetString("API_BASE_URL");

                string url = $"{baseUrl.TrimEnd('/')}/api/users/register";

                var registerData = new { Login = Login, Password = Password };
                string jsonPayload = JsonSerializer.Serialize(registerData);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.PostAsync(url, content);
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
