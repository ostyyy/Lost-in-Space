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

                    AvailableTopics.Add(new Topic { Title = "ALL TOPICS" });
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

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); FilterPosts(); }
        }

        private DateTime? _startDate;
        public DateTime? StartDate
        {
            get => _startDate;
            set { _startDate = value; OnPropertyChanged(); FilterPosts(); }
        }

        private DateTime? _endDate;
        public DateTime? EndDate
        {
            get => _endDate;
            set { _endDate = value; OnPropertyChanged(); FilterPosts(); }
        }

        private Topic _selectedFilterTopic;
        public Topic SelectedFilterTopic
        {
            get => _selectedFilterTopic;
            set { _selectedFilterTopic = value; OnPropertyChanged(); FilterPosts(); } 
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
            if (_allPosts == null)
            { 
                return; 
            }

            var filtered = _allPosts.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var lowerSearch = SearchText.ToLower();
                filtered = filtered.Where(p =>
                    (p.Title != null && p.Title.ToLower().Contains(lowerSearch)) ||
                    (p.Content != null && p.Content.ToLower().Contains(lowerSearch))
                );
            }

            if(SelectedFilterTopic != null && SelectedFilterTopic.Title != "ALL TOPICS")
            {
                filtered = filtered.Where(p => p.Topic != null && p.Topic.Title == SelectedFilterTopic.Title);
            }

            if (StartDate.HasValue)
            {
                filtered = filtered.Where(p => p.CreatedDate >= StartDate.Value);
            }

            if (EndDate.HasValue)
            {
                var endOfEnd = EndDate.Value.AddDays(1);
                filtered = filtered.Where(p => p.CreatedDate < endOfEnd);
            }

            ForumPosts.Clear();
            foreach (var post in filtered.ToList())
            {
                ForumPosts.Add(post);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
