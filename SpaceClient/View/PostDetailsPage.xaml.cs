using System;
using System.Windows;
using System.Windows.Controls;
using SpaceClient.ViewModels;
using SpaceServer.Models;

namespace SpaceClient.View
{
    public partial class PostDetailsPage : Page
    {
        private readonly PostDetailsViewModel _viewModel;

        public PostDetailsPage(Post selectedPost)
        {
            InitializeComponent();

            _viewModel = new PostDetailsViewModel(selectedPost.ID);
            this.DataContext = _viewModel;

            TriggerDataLoading();
        }

        private async void TriggerDataLoading()
        {
            try
            {
                await _viewModel.LoadPostFromServer();
                await _viewModel.LoadCommentsFromServer();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error");
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