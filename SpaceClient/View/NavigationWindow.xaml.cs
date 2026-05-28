using System;
using System.Windows;

namespace SpaceClient.View
{
    /// <summary>
    /// Interaction logic for NavigationWindow.xaml
    /// </summary>
    public partial class NavigationWindow : Window
    {
        public NavigationWindow()
        {
            InitializeComponent();

        }

        private void ToggleSidebar_Click(object sender, RoutedEventArgs e)
        {
            if (SidebarColumn.Width.Value > 0)
            {
                SidebarColumn.Width = new GridLength(0);
                BtnOpenSidebar.Visibility = Visibility.Visible;
            }
            else
            {
                SidebarColumn.Width = new GridLength(250);
                BtnOpenSidebar.Visibility = Visibility.Collapsed;
            }
        }

        private void ToProfile_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ToForum_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ToImages_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ToISS_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}