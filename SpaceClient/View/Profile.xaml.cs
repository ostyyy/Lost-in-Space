using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SpaceClient.View
{
    public partial class Profile : Page
    {
        public string Login { get; set; }
        private readonly string _baseUrl;

        public class UserPost
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
            public TopicDto? Topic { get; set; }
        }

        public class TopicDto
        {
            public string Title { get; set; } = string.Empty;
        }

        public Profile()
        {
            InitializeComponent();
            Login = App.CurrentUserLogin ?? "Space Traveler";
            this.DataContext = this;

            _baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:5232";

            LoadUserPosts();
        }

        private async void LoadUserPosts()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string url = $"{_baseUrl.TrimEnd('/')}/api/posts/user/{Login}";
                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonText = await response.Content.ReadAsStringAsync();
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var posts = JsonSerializer.Deserialize<List<UserPost>>(jsonText, options);

                        PostsContainer.Children.Clear();

                        if (posts == null || posts.Count == 0)
                        {
                            PostsContainer.Children.Add(new TextBlock
                            {
                                Text = "YOU HAVE NOT LAUNCHED ANY POSTS YET...",
                                Foreground = Brushes.Gray,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                Margin = new Thickness(0, 20, 0, 0)
                            });
                            return;
                        }

                        foreach (var post in posts)
                        {
                            PostsContainer.Children.Add(CreatePostUiElement(post));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading posts: {ex.Message}");
            }
        }

        private UIElement CreatePostUiElement(UserPost post)
        {
            Border postBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(40, 25, 35, 60)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(40, 50, 80)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 0, 10)
            };

            Grid postGrid = new Grid();
            postGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            postGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel textContainer = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

            string topicTitle = post.Topic != null ? post.Topic.Title : "Unknown Topic";
            TextBlock topicTxt = new TextBlock
            {
                Text = $"TOPIC: {topicTitle.ToUpper()}",
                Foreground = new SolidColorBrush(Color.FromRgb(100, 150, 240)),
                FontWeight = FontWeights.Bold,
                FontSize = 11,
                Margin = new Thickness(0, 0, 0, 2)
            };
            textContainer.Children.Add(topicTxt);

            TextBlock titleTxt = new TextBlock
            {
                Text = post.Title,
                Foreground = Brushes.White,
                FontWeight = FontWeights.SemiBold,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 4)
            };
            textContainer.Children.Add(titleTxt);

            TextBlock contentTxt = new TextBlock
            {
                Text = post.Content,
                Foreground = new SolidColorBrush(Color.FromRgb(200, 220, 255)),
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap
            };
            textContainer.Children.Add(contentTxt);

            Grid.SetColumn(textContainer, 0);
            postGrid.Children.Add(textContainer);

            Button deleteBtn = new Button
            {
                Content = "Delete",
                Background = Brushes.DarkRed,
                Foreground = Brushes.White,
                BorderBrush = Brushes.Red,
                Width = 65,
                Height = 26,
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Tag = post.Id
            };
            deleteBtn.Click += DeletePostBtn_Click;

            Grid.SetColumn(deleteBtn, 1);
            postGrid.Children.Add(deleteBtn);

            postBorder.Child = postGrid;
            return postBorder;
        }

        private async void DeletePostBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int postId)
            {
                var confirm = MessageBox.Show("Are you sure you want to delete this post?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirm != MessageBoxResult.Yes) return;

                try
                {
                    using (var client = new HttpClient())
                    {
                        string url = $"{_baseUrl.TrimEnd('/')}/api/posts/delete/{postId}";
                        var response = await client.DeleteAsync(url);

                        if (response.IsSuccessStatusCode)
                        {
                            LoadUserPosts(); 
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete the post.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting post: {ex.Message}");
                }
            }
        }

        private async void DeleteAccountBtn_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure?", "Critical", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                using (var client = new HttpClient())
                {
                    var requestData = new { Login = App.CurrentUserLogin, Password = App.CurrentPassword };
                    var jsonText = JsonSerializer.Serialize(requestData);

                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Delete,
                        RequestUri = new Uri($"{_baseUrl}/api/users/delete"),
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