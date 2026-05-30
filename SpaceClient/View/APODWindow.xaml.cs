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

        private List<ApodItem> _loadedItems = new List<ApodItem>();
        private int _currentIndex = 0;

        public APODWindow()
        {
            InitializeComponent();

            DpStart.DisplayDateEnd = DateTime.Today;
            DpEnd.DisplayDateEnd = DateTime.Today;
            DpStart.SelectedDate = DateTime.Today;
            DpEnd.SelectedDate = DateTime.Today;

            LoadNasaData($"&date={DateTime.Today.ToString("yyyy-MM-dd")}");
        }


        private async void LoadNasaData(string queryParam)
        {
            TxtLoading.Visibility = Visibility.Visible;
            ImgApod.Source = null;
            _loadedItems.Clear();

            try
            {
                DotNetEnv.Env.Load();
                string apiKey = Environment.GetEnvironmentVariable("NASA_API_KEY");
                string url = $"https://api.nasa.gov/planetary/apod?api_key={apiKey}{queryParam}";

                string response = await client.GetStringAsync(url);

                using (JsonDocument doc = JsonDocument.Parse(response))
                {
                    JsonElement root = doc.RootElement;

                    if (root.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement element in root.EnumerateArray())
                        {
                            _loadedItems.Add(ParseElement(element));
                        }
                    }
                    else 
                    {
                        _loadedItems.Add(ParseElement(root));
                    }
                }

                if (_loadedItems.Count > 0)
                {
                    _currentIndex = 0;
                    DisplayCurrentItem();
                }
            }
            catch (Exception ex)
            {
                TxtTitle.Text = "CONNECTION ERROR";
                TxtLoading.Text = $"Error: {ex.Message}";
            }
        }

        private ApodItem ParseElement(JsonElement element)
        {
            return new ApodItem
            {
                Date = element.TryGetProperty("date", out var d) ? d.GetString() : "",
                Title = element.TryGetProperty("title", out var t) ? t.GetString() : "UNTITLED",
                Explanation = element.TryGetProperty("explanation", out var e) ? e.GetString() : "",
                Url = element.TryGetProperty("url", out var u) ? u.GetString() : "",
                MediaType = element.TryGetProperty("media_type", out var m) ? m.GetString() : "image"
            };
        }

        private void DisplayCurrentItem()
        {
            if (_loadedItems.Count == 0) return;

            var item = _loadedItems[_currentIndex];

            TxtTitle.Text = item.Title.ToUpper();
            TxtDate.Text = $"STARDATE: {item.Date}";
            TxtExplanation.Text = item.Explanation;

         
            if (item.MediaType == "image" && !string.IsNullOrEmpty(item.Url))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(item.Url, UriKind.Absolute);
                bitmap.EndInit();
                ImgApod.Source = bitmap;
                TxtLoading.Visibility = Visibility.Collapsed;
            }
            else
            {
                TxtLoading.Visibility = Visibility.Visible;
                TxtLoading.Text = $"VIDEO CONTENT. URL: {item.Url}";
            }

            TxtPages.Text = $"{_currentIndex + 1}/{_loadedItems.Count}";
            BtnPrev.IsEnabled = _currentIndex > 0;
            BtnNext.IsEnabled = _currentIndex < _loadedItems.Count - 1;
        }

        private void BtnSearchRange_Click(object sender, RoutedEventArgs e)
        {
            if (DpStart.SelectedDate.HasValue && DpEnd.SelectedDate.HasValue && DpStart.SelectedDate != DpEnd.SelectedDate)
            {
                DateTime start = DpStart.SelectedDate.Value;
                DateTime end = DpEnd.SelectedDate.Value;

                if (start > end)
                {
                    MessageBox.Show("The start date cannot be later than the end date!",
                                    "Chronological Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return;
                }

                int daysCount = (end - start).Days + 1;

                if (daysCount > 5)
                {
                    MessageBox.Show($"Maximum telemetry range is limited to 5 days! You selected: {daysCount} days.\nPlease reduce the interval.",
                                    "Telemetry Limit Exceeded",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return;
                }

                string startStr = start.ToString("yyyy-MM-dd");
                string endStr = end.ToString("yyyy-MM-dd");
                LoadNasaData($"&start_date={startStr}&end_date={endStr}");
            }
            else if (DpStart.SelectedDate.HasValue)
            {
                string targetDate = DpStart.SelectedDate.Value.ToString("yyyy-MM-dd");
                LoadNasaData($"&date={targetDate}");
            }
            else
            {
                MessageBox.Show("Please select at least a start date in the 'FROM' field.",
                                "Telemetry Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
        }

        private void BtnRandom_Click(object sender, RoutedEventArgs e)
        {
            LoadNasaData("&count=3"); 
        }

        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
                DisplayCurrentItem();
            }
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (_currentIndex < _loadedItems.Count - 1)
            {
                _currentIndex++;
                DisplayCurrentItem();
            }
        }
        
    }


    public class ApodItem
    {
        public string Date { get; set; }
        public string Title { get; set; }
        public string Explanation { get; set; }
        public string Url { get; set; }
        public string MediaType { get; set; }
    }
}