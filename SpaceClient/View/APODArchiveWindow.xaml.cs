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
    /// Interaction logic for APODArchiveWindow.xaml
    /// </summary>
    public partial class APODArchiveWindow : Window
    {
        private int _userId;
        public APODArchiveWindow(int userId)
        {
            InitializeComponent();

            this.DataContext = new APODArchiveViewModel(userId);
        }
    }
}
