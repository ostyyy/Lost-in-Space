using System;
using System.Globalization;     
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;  
using SpaceClient.Services;      

namespace SpaceClient.View
{
    public partial class ISS_Form : Page
    {
        private readonly IssTrackingService _issService = new IssTrackingService();
        private DispatcherTimer _gpsTimer;

        public ISS_Form()
        {
            InitializeComponent();
            InitializeMapAndTimer();
        }

        private async void InitializeMapAndTimer()
        {
            try
            {
                await MyWebView.EnsureCoreWebView2Async();

                string webFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Web");

                MyWebView.CoreWebView2.Profile.PreferredTrackingPreventionLevel = Microsoft.Web.WebView2.Core.CoreWebView2TrackingPreventionLevel.None;

                MyWebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "spaceapp.local",
                    webFolder,
                    Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow);

                MyWebView.CoreWebView2.Navigate("https://spaceapp.local/index.html");

                _gpsTimer = new DispatcherTimer();
                _gpsTimer.Interval = TimeSpan.FromSeconds(3);
                _gpsTimer.Tick += UpdateIssPositionOnMap;
                _gpsTimer.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Помилка ініціалізації карти МКС: {ex.Message}");
            }
        }

        private async void UpdateIssPositionOnMap(object sender, EventArgs e)
        {
            var coordinates = await _issService.GetCurrentIssCoordinatesAsync();

            if (coordinates.HasValue)
            {
                double lat = coordinates.Value.lat;
                double lng = coordinates.Value.lng;

                string jsCommand = string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "updateISS({0}, {1});", lat, lng
                );

                await MyWebView.ExecuteScriptAsync(jsCommand);
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_gpsTimer != null)
            {
                _gpsTimer.Tick -= UpdateIssPositionOnMap;
                _gpsTimer.Stop();
                _gpsTimer = null;
            }
        }
    }
}