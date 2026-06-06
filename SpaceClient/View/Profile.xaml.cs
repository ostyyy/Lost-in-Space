using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SpaceClient.View;
using SpaceServer.Controllers;

namespace SpaceClient.View
{
    /// <summary>
    /// Логика взаимодействия для Profile.xaml
    /// </summary>
    public partial class Profile : Page
    {
        public string Login { get; set; }
        public Profile()
        {
            InitializeComponent();
            Login = App.CurrentUserLogin;
            this.DataContext = this;

        }

        private async void DeleteAccountBtn_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure?", "Critical", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }
            try
            {
                using (var client = new HttpClient())
                {
                    var baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:5232";

                    var requestData = new
                    {
                        Login = App.CurrentUserLogin,
                        Password = App.CurrentPassword
                    };

                    var jsonText = JsonSerializer.Serialize(requestData);

                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Delete,
                        RequestUri = new Uri($"{baseUrl}/api/users/delete"),
                        Content = new StringContent(jsonText, Encoding.UTF8, "application/json")
                    };

                    var response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Successfully deleted account!");
                        App.CurrentUserId = 0;
                        App.CurrentUserLogin = null;
                        LogIn loginWindow = new LogIn();
                        loginWindow.Show();
                        Window.GetWindow(this)?.Close();
                    }
                    else 
                    {
                        string errorText = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Error deleting account! {errorText}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error trying deleting account: {ex}");
            }
        }
    }
}
