using SpaceClient.ViewModels;
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
using System.Windows.Shapes;
using SpaceClient.ViewModels;

namespace SpaceClient.View
{
    /// <summary>
    /// Логика взаимодействия для Forum.xaml
    /// </summary>
    public partial class Forum : Page
    {
        public ForumViewModel ViewModel { get; set; }
        public Forum()
        {
            InitializeComponent();
            ViewModel = new ForumViewModel();
            this.DataContext = ViewModel;

            _ = ViewModel.LoadPostsFromServer();
        }

        private void CreatePost_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ForumPageCreate(ViewModel));
        }
        private void OpenPost_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
