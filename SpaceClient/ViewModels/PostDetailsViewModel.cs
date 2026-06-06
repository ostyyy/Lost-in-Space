using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Text.Json.Serialization;

namespace SpaceClient.ViewModels
{
    public class ServerPostResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("topic")]
        public ServerTopicResponse? Topic { get; set; }
    }

    public class ServerTopicResponse
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
    }

    public class ServerCommentResponse
    {
        [JsonPropertyName("id")]
        public int id { get; set; }

        [JsonPropertyName("content")]
        public string content { get; set; } = string.Empty;

        [JsonPropertyName("postId")]
        public int postID { get; set; }

        [JsonPropertyName("parentCommentId")]
        public int? parentCommentID { get; set; }

        [JsonPropertyName("authorId")]
        public int? authorID { get; set; }

        [JsonPropertyName("authorName")]
        public string authorName { get; set; } = string.Empty;

        [JsonPropertyName("createdDate")]
        public DateTime createdDate { get; set; }
    }
    public class PostDetailsViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string _baseUrl;
        private readonly int _postId;

        private string _postTitle = string.Empty;
        public string PostTitle
        {
            get => _postTitle;
            set { _postTitle = value; OnPropertyChanged(); }
        }

        private string _postContent = string.Empty;
        public string PostContent
        {
            get => _postContent;
            set { _postContent = value; OnPropertyChanged(); }
        }

        private string _postTopic = string.Empty;
        public string PostTopic
        {
            get => _postTopic;
            set { _postTopic = value; OnPropertyChanged(); }
        }

        private string _commentInputText = string.Empty;
        public string CommentInputText
        {
            get => _commentInputText;
            set { _commentInputText = value; OnPropertyChanged(); }
        }

        private int? _selectedParentCommentId = null;

        private Visibility _replyIndicatorVisibility = Visibility.Collapsed;
        public Visibility ReplyIndicatorVisibility
        {
            get => _replyIndicatorVisibility;
            set { _replyIndicatorVisibility = value; OnPropertyChanged(); }
        }

        private ObservableCollection<CommentDisplayWrapper> _comments = new ObservableCollection<CommentDisplayWrapper>();
        public ObservableCollection<CommentDisplayWrapper> Comments
        {
            get => _comments;
            set { _comments = value; OnPropertyChanged(); }
        }

        public ICommand SendCommentCommand { get; }
        public ICommand ReplyCommand { get; }
        public ICommand CancelReplyCommand { get; }

        public PostDetailsViewModel(int postId)
        {
            _postId = postId;

            _baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL");
            if (string.IsNullOrEmpty(_baseUrl))
            {
                _baseUrl = "http://localhost:5000";
            }

            SendCommentCommand = new RelayCommand(async (obj) => await SendNewComment());
            ReplyCommand = new RelayCommand((param) => ExecuteReply(param));
            CancelReplyCommand = new RelayCommand((obj) => ExecuteCancelReply());
        }

        public async Task LoadPostFromServer()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/posts/{_postId}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonText = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var post = JsonSerializer.Deserialize<ServerPostResponse>(jsonText, options);

                    if (post != null)
                    {
                        PostTitle = post.Title;
                        PostContent = post.Content;
                        PostTopic = $"Topic: {post.Topic?.Title ?? "General"}";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading post: {ex.Message}", "Error");
            }
        }

        public async Task LoadCommentsFromServer()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/comments/post/{_postId}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonText = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var allComments = JsonSerializer.Deserialize<List<ServerCommentResponse>>(jsonText, options) ?? new List<ServerCommentResponse>();

                    Comments.Clear();

                    var rootComments = allComments.Where(c => c.parentCommentID == null).OrderBy(c => c.createdDate);

                    foreach (var root in rootComments)
                    {
                        AddCommentWithReplies(root, allComments, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading comments: {ex.Message}", "Database Error");
            }
        }

        private void AddCommentWithReplies(ServerCommentResponse currentComment, List<ServerCommentResponse> allComments, double currentIndent)
        {
            Comments.Add(new CommentDisplayWrapper(currentComment, currentIndent));

            var directReplies = allComments.Where(c => c.parentCommentID == currentComment.id).OrderBy(c => c.createdDate);

            foreach (var reply in directReplies)
            {
                AddCommentWithReplies(reply, allComments, currentIndent + 35);
            }
        }

        public async Task SendNewComment()
        {
            string text = CommentInputText?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Please enter a comment text.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var requestData = new
                {
                    Content = text,
                    PostID = _postId,
                    ParentCommentID = _selectedParentCommentId,
                    AuthorID = App.CurrentUserId
                };

                var jsonText = JsonSerializer.Serialize(requestData);
                var httpContent = new StringContent(jsonText, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/api/comments/create", httpContent);

                if (response.IsSuccessStatusCode)
                {
                    CommentInputText = string.Empty;
                    ExecuteCancelReply();
                    await LoadCommentsFromServer(); 
                }
                else
                {
                    string errorText = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Server error: {errorText}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending comment: {ex.Message}", "Transmission Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteReply(object parameter)
        {
            if (parameter is int commentId)
            {
                _selectedParentCommentId = commentId;
                ReplyIndicatorVisibility = Visibility.Visible;
            }
        }

        private void ExecuteCancelReply()
        {
            _selectedParentCommentId = null;
            ReplyIndicatorVisibility = Visibility.Collapsed;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class CommentDisplayWrapper
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string CreatedDate { get; set; } = string.Empty;
        public Thickness LeftMargin { get; set; }

        public CommentDisplayWrapper(ServerCommentResponse comment, double indent)
        {
            Id = comment.id;
            Content = comment.content;
            AuthorName = comment.authorName;
            CreatedDate = comment.createdDate.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
            LeftMargin = new Thickness(indent, 6, 0, 6);
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool>? _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter!);
        public void Execute(object? parameter) => _execute(parameter!);
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}