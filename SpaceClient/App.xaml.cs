using System.Configuration;
using System.Data;
using System.Windows;

namespace SpaceClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string CurrentUserLogin { get; set; } = string.Empty;

        public static int CurrentUserId { get; set; }

        public static string CurrentPassword { get; set; } = string.Empty;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DotNetEnv.Env.Load();
        }
    }

}
