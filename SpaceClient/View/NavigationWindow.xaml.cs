using System;
using System.Media;
using System.IO;
using System.Windows;
using System.Windows.Media;


namespace SpaceClient.View
{
    /// <summary>
    /// Interaction logic for NavigationWindow.xaml
    /// </summary>
    public partial class NavigationWindow : Window
    {
        private SoundPlayer _backgroundPlayer;
        public NavigationWindow()
        {
            InitializeComponent();

            //StartBackgroundMusic();
        }

        private void StartBackgroundMusic()
        {
            try
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

                string musicPath = Path.Combine(baseDirectory, "music", "Space.wav");

                if (File.Exists(musicPath))
                {
                    _backgroundPlayer = new SoundPlayer(musicPath);

                    _backgroundPlayer.PlayLooping();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[Music Error] Файл не знайдено за шляхом: {musicPath}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing background music: {ex.Message}");
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
            _backgroundPlayer.Stop();
            _backgroundPlayer?.Dispose();

            this.Close();
        }

        private void ToLaunches_Click(object sender, RoutedEventArgs e)
        {
            LaunchesWindow launchPage = new LaunchesWindow();
            MainFrame.Navigate(launchPage);
        }
    }
}