using System;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Net.WebRequestMethods;


namespace SpaceClient.View
{
    /// <summary>
    /// Interaction logic for NavigationWindow.xaml
    /// </summary>
    public partial class NavigationWindow : Window
    {
        private SoundPlayer _backgroundPlayer;
        private bool _isMuted = false;
        public NavigationWindow()
        {
            InitializeComponent();

            StartBackgroundMusic();

            Profile profile = new Profile();
            MainFrame.Navigate(profile);
        }

        private void StartBackgroundMusic()
        {
            try
            {
                var resourceStream = Application.GetResourceStream(new Uri("/music/Space.wav", UriKind.Relative));

                if (resourceStream != null)
                {
                    _backgroundPlayer = new SoundPlayer(resourceStream.Stream);
                    _backgroundPlayer.PlayLooping();
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing background music: {ex.Message}");

            }
        }

        private void Mute_Click(object sender, RoutedEventArgs e)
        {
            if (_backgroundPlayer != null)
            {
                _isMuted = !_isMuted;

                if (_isMuted)
                {
                    _backgroundPlayer.Stop();

                    MuteImage.Source = new BitmapImage(new Uri("/Pictures/unmute.png", UriKind.Relative));
                    BtnMute.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFD71CB");
                }
                else
                {
                    _backgroundPlayer.PlayLooping();

                    MuteImage.Source = new BitmapImage(new Uri("/Pictures/mute.png", UriKind.Relative));
                    BtnMute.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#AECFFF");
                }
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
            if (App.CurrentUserLogin == null)
            {
                MessageBox.Show("You must be authorized first!");
                return;
            }
            Profile profile = new Profile();
            MainFrame.Navigate(profile);
        }

        private void ToForum_Click(object sender, RoutedEventArgs e)
        {
            Forum forumPage = new Forum();
            MainFrame.Navigate(forumPage);
        }

        private void ToImages_Click(object sender, RoutedEventArgs e)
        {
            APODWindow apodPage = new APODWindow();
            MainFrame.Navigate(apodPage);
        }

        private void ToISS_Click(object sender, RoutedEventArgs e)
        {
            ISS_Form issPage = new ISS_Form();
            MainFrame.Navigate(issPage);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (_backgroundPlayer != null)
            {
                _backgroundPlayer.Stop();
                _backgroundPlayer.Dispose();
            }

            LogIn loginWindow = new LogIn();
            loginWindow.Show();

            this.Close();
        }

        private void ToLaunches_Click(object sender, RoutedEventArgs e)
        {
            LaunchesWindow launchPage = new LaunchesWindow();
            MainFrame.Navigate(launchPage);
        }

       
    }
}