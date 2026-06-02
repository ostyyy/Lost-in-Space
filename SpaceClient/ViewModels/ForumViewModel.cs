using SpaceServer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace SpaceClient.ViewModels
{
    public class ForumViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string _baseUrl;

        private List<Post> _allPosts = new List<Post>();

        private ObservableCollection<Post> _forumPosts = new ObservableCollection<Post>();

        public ObservableCollection<Post> ForumPosts
        {
            get => _forumPosts;
            set
            {
                _forumPosts = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<Topic> _availableTopics = new ObservableCollection<Topic>();
        public ObservableCollection<Topic> AvailableTopics
        {
            get => _availableTopics;
            set
            {
                _availableTopics = value;
                OnPropertyChanged();
            }
        }

        public async Task LoadTopicsFromServer()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/topics");

                if (response.IsSuccessStatusCode)
                {
                    var jsonText = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    var topics = JsonSerializer.Deserialize<List<Topic>>(jsonText, options) ?? new List<Topic>();

                    AvailableTopics.Clear();
                    foreach (var topic in topics)
                    {
                        AvailableTopics.Add(topic);
                    }
                }
                else
                {
                    MessageBox.Show($"Error: {response.StatusCode}", "Erorr");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Database Error");
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
                FilterPosts();
            }
        }

        private DateTime? _startDate;
        public DateTime? StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged();
                FilterPosts();
            }
        }

        private DateTime? _endDate;
        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged();
                FilterPosts();
            }
        }
        public ForumViewModel() 
        {
            _baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:5232";
        }

        public async Task LoadPostsFromServer()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/posts/all");
                if (response.IsSuccessStatusCode)
                {
                    var jsonText = await response.Content.ReadAsStringAsync();

                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    _allPosts = JsonSerializer.Deserialize<List<Post>>(jsonText, options) ?? new List<Post>();

                    FilterPosts();
                }    
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex}");
            }
        }

        public async Task SendNewPost(string title, string content, string topicName)
        {
           
            try
            {
                var requestData = new
                {
                    Title = title,
                    Content = content,
                    TopicName = topicName,
                    AuthorID = App.CurrentUserId
                };

                var jsonText = JsonSerializer.Serialize(requestData);
                var httpContent = new StringContent(jsonText, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/api/posts/create", httpContent);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Successfully added new post!", "Added", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadPostsFromServer();
                }
                else
                {
                    string errorText = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Somthing went wrong! {errorText}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void FilterPosts()
        {
            _forumPosts.Clear();
            foreach (var post in _allPosts)
            {
                _forumPosts.Add(post);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
