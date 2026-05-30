using System;
using System.Windows;
using System.Windows.Media;


namespace SpaceClient.View
{
    /// <summary>
    /// Interaction logic for NavigationWindow.xaml
    /// </summary>
    public partial class NavigationWindow : Window
    {

        private MediaPlayer _backgroundPlayer = new MediaPlayer();
        public NavigationWindow()
        {
            InitializeComponent();

            StartBackgroundMusic();

        }


        private void StartBackgroundMusic()
        {
            try
            {
                string musicPath = @"D:\uni_projects\SpaceApp\SpaceApp\SpaceClient\music\Space.mp3";

                _backgroundPlayer.Open(new Uri(musicPath, UriKind.Absolute));

                _backgroundPlayer.Volume = 0.3;

                _backgroundPlayer.MediaEnded += (sender, e) =>
                {
                    _backgroundPlayer.Position = TimeSpan.Zero; 
                    _backgroundPlayer.Play();
                };

                _backgroundPlayer.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error playing background music: {ex.Message}");
            }
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
            APODWindow apodPage = new APODWindow();
            MainFrame.Navigate(apodPage);
        }

        private void ToISS_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            _backgroundPlayer.Stop();
            _backgroundPlayer.Close();

            this.Close();
        }
    }
}