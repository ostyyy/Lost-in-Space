using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SpaceClient.View;

namespace SpaceClient.ViewModel
{
    public class APODViewModel : INotifyPropertyChanged
    {
        private static readonly HttpClient _nasaClient = new HttpClient();

        private readonly HttpClient _serverClient = new HttpClient();

        private List<ApodItemViewModel> _loadedItems = new List<ApodItemViewModel>();
        private int _currentIndex = 0;
        private int _currentUserId;

        // BINDING PROPERTIES
        private string _title = "LOADING TELEMETRY...";
        private string _date = "--.--.----";
        private string _explanation = "...";
        private string _imageUrl;
        private string _loadingStatus = "RECEIVING DATA FROM NASA...";
        private string _pagesText = "0/0";
        private bool _isImgVisible = false;
        private bool _isPrevEnabled = false;
        private bool _isNextEnabled = false;
        private DateTime _dpStart = DateTime.Today;
        private DateTime _dpEnd = DateTime.Today;

        public string Title { get => _title; set { _title = value; OnPropertyChanged(); } }
        public string Date { get => _date; set { _date = value; OnPropertyChanged(); } }
        public string Explanation { get => _explanation; set { _explanation = value; OnPropertyChanged(); } }
        public string ImageURL { get => _imageUrl; set { _imageUrl = value; OnPropertyChanged(); } }
        public string LoadingStatus { get => _loadingStatus; set { _loadingStatus = value; OnPropertyChanged(); } }
        public string PagesText { get => _pagesText; set { _pagesText = value; OnPropertyChanged(); } }
        public bool IsImgVisible { get => _isImgVisible; set { _isImgVisible = value; OnPropertyChanged(); } }
        public bool IsPrevEnabled { get => _isPrevEnabled; set { _isPrevEnabled = value; OnPropertyChanged(); } }
        public bool IsNextEnabled { get => _isNextEnabled; set { _isNextEnabled = value; OnPropertyChanged(); } }
        public DateTime DpStart { get => _dpStart; set { _dpStart = value; OnPropertyChanged(); } }
        public DateTime DpEnd { get => _dpEnd; set { _dpEnd = value; OnPropertyChanged(); } }

        // BUTTONS COMMANDS
        public ICommand SearchRangeCommand { get; }
        public ICommand RandomCommand { get; }
        public ICommand PrevCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand SaveToArchiveCommand { get; }
        public ICommand OpenArchiveCommand { get; }

        public APODViewModel(int userId)
        {
            _currentUserId = userId;

            DotNetEnv.Env.Load();
            string baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL");

            _serverClient.BaseAddress = new Uri(baseUrl);

            SearchRangeCommand = new RelayCommand(ExecuteSearchRange);
            RandomCommand = new RelayCommand(() => LoadNasaData("&count=5"));
            PrevCommand = new RelayCommand(ExecutePrev);
            NextCommand = new RelayCommand(ExecuteNext);
            SaveToArchiveCommand = new RelayCommand(async () => await SaveToArchiveAsync());
            OpenArchiveCommand = new RelayCommand(ExecuteOpenArchive);

            LoadNasaData($"&date={DateTime.Today.ToString("yyyy-MM-dd")}");
        }

        private async void LoadNasaData(string queryParam)
        {
            IsImgVisible = false;
            LoadingStatus = "RECEIVING DATA FROM NASA...";
            _loadedItems.Clear();

            try
            {
                DotNetEnv.Env.Load();
                string apiKey = Environment.GetEnvironmentVariable("NASA_API_KEY");
                string url = $"https://api.nasa.gov/planetary/apod?api_key={apiKey}{queryParam}";

                string response = await _nasaClient.GetStringAsync(url);

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
                Title = "CONNECTION ERROR";
                LoadingStatus = $"Error: {ex.Message}";
            }
        }

        private ApodItemViewModel ParseElement(JsonElement element)
        {
            return new ApodItemViewModel
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

            Title = item.Title.ToUpper();
            Date = $"STARDATE: {item.Date}";
            Explanation = item.Explanation;

            if (item.MediaType == "image" && !string.IsNullOrEmpty(item.Url))
            {
                ImageURL = item.Url;
                IsImgVisible = true;
            }
            else
            {
                IsImgVisible = false;
                LoadingStatus = $"VIDEO CONTENT. URL: {item.Url}";
            }

            PagesText = $"{_currentIndex + 1}/{_loadedItems.Count}";
            IsPrevEnabled = _currentIndex > 0;
            IsNextEnabled = _currentIndex < _loadedItems.Count - 1;
        }

        private void ExecuteSearchRange()
        {
            if (DpStart != DpEnd)
            {
                if (DpStart > DpEnd)
                {
                    MessageBox.Show("The start date cannot be later than the end date!", "Chronological Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int daysCount = (DpEnd - DpStart).Days + 1;

                if (daysCount > 5)
                {
                    MessageBox.Show($"Maximum telemetry range is limited to 5 days! You selected: {daysCount} days.\nPlease reduce the interval.", "Telemetry Limit Exceeded", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string startStr = DpStart.ToString("yyyy-MM-dd");
                string endStr = DpEnd.ToString("yyyy-MM-dd");
                LoadNasaData($"&start_date={startStr}&end_date={endStr}");
            }
            else
            {
                string targetDate = DpStart.ToString("yyyy-MM-dd");
                LoadNasaData($"&date={targetDate}");
            }
        }

        private async Task SaveToArchiveAsync()
        {
            if (_loadedItems.Count == 0) return;
            var currentItem = _loadedItems[_currentIndex];

            try
            {
                var requestData = new
                {
                    UserId = _currentUserId,
                    Date = DateTime.Parse(currentItem.Date),
                    Title = currentItem.Title,
                    Explanation = currentItem.Explanation,
                    ImageURL = currentItem.Url
                };

                var response = await _serverClient.PostAsJsonAsync("api/APOD/add", requestData);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Telemetry successfully saved to your personal space log!", "Archive Secured", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    MessageBox.Show("This image is already saved in your personal archive.", "Archive Duplicate", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show("Server rejected data storage request.", "Transmission Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database connection failure: {ex.Message}", "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteOpenArchive()
        {
            var archiveWin = new APODArchiveWindow(_currentUserId);
            archiveWin.ShowDialog();
        }

        private void ExecutePrev() { if (_currentIndex > 0) { _currentIndex--; DisplayCurrentItem(); } }
        private void ExecuteNext() { if (_currentIndex < _loadedItems.Count - 1) { _currentIndex++; DisplayCurrentItem(); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class ApodItemViewModel
    {
        public string Date { get; set; }
        public string Title { get; set; }
        public string Explanation { get; set; }
        public string Url { get; set; }
        public string MediaType { get; set; }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;

        public RelayCommand(Action execute) => _execute = execute;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => _execute();

        public event EventHandler? CanExecuteChanged;
    }
}