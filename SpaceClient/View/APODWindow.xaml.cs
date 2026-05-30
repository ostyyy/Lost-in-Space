using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net.Http;
using System.Text.Json;
using DotNetEnv;

namespace SpaceClient.View
{
    /// <summary>
    /// Interaction logic for APODWindow.xaml
    /// </summary>
    public partial class APODWindow : Page
    {
        private static readonly HttpClient client = new HttpClient();
        public APODWindow()
        {
            InitializeComponent();
            LoadNasaPhoto();
        }

        private async void LoadNasaPhoto()
        {
            try
            {
                DotNetEnv.Env.Load();

                string apiKey = Environment.GetEnvironmentVariable("NASA_API_KEY");

                if (string.IsNullOrEmpty(apiKey))
                {
                    MessageBox.Show("API key not found in .env file.");
                    return;
                }

                string apiUrl = $"https://api.nasa.gov/planetary/apod?api_key={apiKey}";

                string response = await client.GetStringAsync(apiUrl);

                using(JsonDocument doc = JsonDocument.Parse(response))
                {
                    JsonElement root = doc.RootElement;
                    string title = root.GetProperty("title").GetString() ?? "No Title";
                    string explanation = root.GetProperty("explanation").GetString() ?? "No Explanation";
                    string imageUrl = root.GetProperty("url").GetString() ?? "";
                    string mediaType = root.GetProperty("media_type").GetString() ?? "image";

                    TxtTitle.Text = title.ToUpper();
                    TxtExplanation.Text = explanation;

                    if (mediaType == "image" && !string.IsNullOrEmpty(imageUrl))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(imageUrl, UriKind.Absolute);
                        bitmap.EndInit();

                        ImgApod.Source = bitmap;
                        TxtLoading.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        MessageBox.Show("Today's APOD is not an image.");
                    }
                }

            }
            catch (Exception ex)
            {
                TxtTitle.Text = "CONNECTION ERROR";
                TxtLoading.Text = "FAILED TO LINK WITH NASA SERVERS.";
                MessageBox.Show($"ERROR: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }
    }
}