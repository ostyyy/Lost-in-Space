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
using System.Windows.Navigation;
using System.Windows.Shapes;
using SpaceClient.ViewModels;
using SpaceServer.Models;


namespace SpaceClient.View
{
    /// <summary>
    /// Interaction logic for PostDetailsPage.xaml
    /// </summary>
    public partial class PostDetailsPage : Page
    {
        public PostDetailsPage(Post selectedPost)
        {
            InitializeComponent();

            TxtPostTitle.Text = selectedPost.Title;
            TxtPostContent.Text = selectedPost.Content;

            if (selectedPost.Topic != null)
            {
                TxtPostTopic.Text = $"Topic: {selectedPost.Topic.Title}";
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService != null && this.NavigationService.CanGoBack)
            {
                this.NavigationService.GoBack();
            }
        }
    }
}
