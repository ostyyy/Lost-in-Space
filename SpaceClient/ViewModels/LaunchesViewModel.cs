using SpaceClient.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using SpaceServer.Models;
using Microsoft.AspNetCore.Identity;



namespace SpaceClient.ViewModels
{
    public class LaunchesViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient = new HttpClient();

        private readonly string _baseUrl;

        public LaunchesViewModel()
        {
            _baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:5232";
        }

        private ObservableCollection<Launches> _availableLaunches = new();
        public ObservableCollection<Launches> AvailableLaunches
        {
            get => _availableLaunches;
            set
            {
                _availableLaunches = value;
                OnPropertyChanged();
            }
        }

        private Launches _selectedLaunch;
        public Launches SelectedLaunch
        {
            get => _selectedLaunch;
            set
            {
                _selectedLaunch = value;
                OnPropertyChanged();
            }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }

        public async Task DeleteFromFavorites()
        {
            if (SelectedLaunch == null)
            {
                return;
            }

            if (SelectedLaunch.Id == 0)
            {
                MessageBox.Show("Error! Not in Favorites!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string apiUrl = $"{_baseUrl}/api/launches/{SelectedLaunch.Id}";

                var response = await _httpClient.DeleteAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    AvailableLaunches.Remove(SelectedLaunch);
                }
                else
                {
                    MessageBox.Show("Something went wrong!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error! {ex}");
            }
        }
        public async Task LoadFromAPI()
        {
            try
            {
                string API_URL = "https://ll.thespacedevs.com/2.3.0/launches/?format=json";

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    API_URL += $"&search={SearchText}";
                }

                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("SpaceClient/1.0");

                var response = await _httpClient.GetAsync(API_URL);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Error {response.StatusCode}");
                    return;
                }


                var jsonString = await response.Content.ReadAsStringAsync();

                var rootNode = JsonNode.Parse(jsonString);
                var resultsArray = rootNode["results"]?.AsArray();

                if (resultsArray == null)
                {
                    return;
                }

                AvailableLaunches.Clear();
                foreach (var result in resultsArray)
                {
                    DateTime? parsedDateTime = null;
                    if (DateTime.TryParse(result["net"]?.ToString(), out DateTime date))
                    {
                        parsedDateTime = date;
                    }

                    var launches = new Launches
                    {
                        LaunchName = result["name"]?.ToString() ?? "Unknown",
                        LaunchDate = parsedDateTime,
                        LaunchLocation = result["pad"]?["location"]?["name"]?.ToString() ?? "Unknown",
                        Status = result["status"]?["name"]?.ToString() ?? "Unknown",
                        LaunchProvider = result["launch_service_provider"]?["name"]?.ToString() ?? "Unknown"
                        
                    };
                    AvailableLaunches.Add(launches);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error! {ex}");
            }
        }

        public async Task LoadFavorites()
        {
            try
            {
                string apiUrl = $"{_baseUrl}/api/launches/my/{App.CurrentUserLogin}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();

                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var mySavedLaunches = JsonSerializer.Deserialize<List<Launches>>(jsonString, options);

                    AvailableLaunches.Clear();

                    if (mySavedLaunches != null)
                    {
                        foreach (var launch in mySavedLaunches)
                        {
                            AvailableLaunches.Add(launch);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Failed to load favorites from server.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error {ex}");
            }
        }
        public async Task saveToDB()
        {
            if (SelectedLaunch == null)
            {
                return;
            }

            var requestData = new
            {
                UserLogin = App.CurrentUserLogin,
                LaunchData = SelectedLaunch
            };

            var jsonText = JsonSerializer.Serialize(requestData);
            var content = new StringContent(jsonText, Encoding.UTF8, "application/json");

            try
            {
                string apiUrl = $"{_baseUrl}/api/launches/favorites";

                var response = await _httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Launch {SelectedLaunch.LaunchName} successfully added to favorites!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    string errorText = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Server Error: {errorText}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error! {ex}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
