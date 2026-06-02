using SpaceClient.ViewModels;
using SpaceClient.ViewModels;
using SpaceServer.Models;
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

namespace SpaceClient.View
{
    /// <summary>
    /// Логика взаимодействия для ForumPageCreate.xaml
    /// </summary>
    public partial class ForumPageCreate : Page
    {
        private ForumViewModel _viewModel;
        public ForumPageCreate(ForumViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            this.DataContext = _viewModel;

            _ = _viewModel.LoadTopicsFromServer();
        }
        private void CmbTopic_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbTopic.SelectedItem is Topic selectedTopic)
            {
                TxtTopicInput.Text = selectedTopic.Title;
            }
        }
        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            string topicName = TxtTopicInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(TxtTitle.Text) ||
                string.IsNullOrWhiteSpace(TxtContent.Text) ||
                string.IsNullOrWhiteSpace(topicName))
            {
                MessageBox.Show("Do not leave fields blank!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await _viewModel.SendNewPost(TxtTitle.Text, TxtContent.Text, topicName);
            NavigationService.GoBack();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
