using SpaceServer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
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
                _allPosts = new List<Post>
                {
                    new Post { Title = "Test post", Content = "Testim.", CreatedDate = DateTime.Now }
                };

                FilterPosts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex}");
            }
        }
        private void FilterPosts()
        {
            ForumPosts.Clear();
            foreach (var post in _allPosts)
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
