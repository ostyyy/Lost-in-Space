using SpaceClient.ViewModel;
using SpaceClient.ViewModels;
using System;
using System.Windows.Controls;

namespace SpaceClient.View
{
    /// <summary>
    /// Interaction logic for APODWindow.xaml
    /// </summary>
    public partial class APODWindow : Page
    {
        public APODWindow()
        {
            InitializeComponent();

            int currentUserId = App.CurrentUserId; 
            this.DataContext = new APODViewModel(currentUserId);
        }
    }
}