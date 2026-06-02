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
using SpaceClient.View;

namespace SpaceClient.View
{
    /// <summary>
    /// Логика взаимодействия для Profile.xaml
    /// </summary>
    public partial class Profile : Window
    {
        public Profile()
        {
            InitializeComponent();
        }

        private void ToForumbtn_Click(object sender, RoutedEventArgs e)
        {
            //Forum forumWindow = new Forum();

            //forumWindow.Show();

            //this.Close();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            LogIn loginWindow = new LogIn();

            loginWindow.Show();

            this.Close();
        }

        private void ToISSbtn_Click(object sender, RoutedEventArgs e)
        {
            string htmlFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ISS/ISS_map.html");

            // Даем Windows команду открыть этот файл в браузере по умолчанию (Chrome, Edge и тд)
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(htmlFilePath)
            {
                UseShellExecute = true
            });
        }
    }
}
