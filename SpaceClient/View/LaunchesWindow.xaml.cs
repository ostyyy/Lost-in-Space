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
    /// Логика взаимодействия для LaunchesWindow.xaml
    /// </summary>
    public partial class LaunchesWindow : Window
    {
        public LaunchesViewModel ViewModel { get; set; }
        public LaunchesWindow()
        {
            InitializeComponent();
            ViewModel = new LaunchesViewModel();
            this.DataContext = ViewModel;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadFromAPI();
        }

        private async void ViewArchiveButton_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.saveToDB();
        }
    }
}
