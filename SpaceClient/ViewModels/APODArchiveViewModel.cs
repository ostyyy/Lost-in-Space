using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SpaceClient.ViewModels
{
    public class APODArchiveViewModel : BaseViewModel
    {
        private readonly HttpClient _serverClient = new HttpClient();
        private readonly int _userId;

        // Колекція залишається незмінною як об'єкт, ми змінюємо лише її вміст
        public ObservableCollection<ArchiveItem> ArchivedItems { get; } = new ObservableCollection<ArchiveItem>();

        private ArchiveItem _selectedItem;
        public ArchiveItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDetailsVisible));
            }
        }

        public Visibility IsDetailsVisible => SelectedItem != null ? Visibility.Visible : Visibility.Collapsed;

        public ICommand DeleteItemCommand { get; }

        public APODArchiveViewModel(int userId)
        {
            _userId = userId;
            DeleteItemCommand = new ViewModel.RelayCommand(async () => await ExecuteDeleteAsync());

            ConfigureClient();
            LoadArchiveData();
        }

        private void ConfigureClient()
        {
            DotNetEnv.Env.Load();
            string baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL");

            if (!baseUrl.EndsWith("/"))
            {
                baseUrl += "/";
            }

            _serverClient.BaseAddress = new Uri(baseUrl);
        }

        private async void LoadArchiveData()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var response = await _serverClient.GetAsync($"api/APOD/user/{_userId}");

                if (response.IsSuccessStatusCode)
                {
                    string jsonString = await response.Content.ReadAsStringAsync();
                    var items = JsonSerializer.Deserialize<List<ArchiveItem>>(jsonString, options);

                    if (items != null)
                    {
                        // Важливо: Робимо це в UI-потоці WPF
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ArchivedItems.Clear(); // Очищуємо старі записи
                            foreach (var item in items)
                            {
                                ArchivedItems.Add(item); // Додаємо нові (XAML це миттєво побачить)
                            }
                        });
                    }
                }
                else
                {
                    MessageBox.Show($"Server returned error: {response.StatusCode}", "Transmission Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load archive: {ex.Message}", "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExecuteDeleteAsync()
        {
            if (SelectedItem == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete '{SelectedItem.Title}' from your space log?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                var response = await _serverClient.DeleteAsync($"api/APOD/delete/{SelectedItem.Id}");

                if (response.IsSuccessStatusCode)
                {
                    ArchivedItems.Remove(SelectedItem);
                    SelectedItem = null;
                    MessageBox.Show("Record successfully removed from telemetry logs.", "Archive Updated", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Server refused to delete this record.", "Transmission Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection error during deletion: {ex.Message}", "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    // Клас винесено окремо в простір імен, щоб серіалізатор бачив його структуру без перешкод
    public class ArchiveItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("userId")]
        public int UserId { get; set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("explanation")]
        public string Explanation { get; set; } = string.Empty;

        [JsonPropertyName("imageURL")]
        public string ImageURL { get; set; } = string.Empty;

        public string DateString => Date.ToString("yyyy-MM-dd");
    }
}