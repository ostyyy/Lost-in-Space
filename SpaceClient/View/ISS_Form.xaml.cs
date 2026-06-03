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

                var crew = await _issService.GetISSCrew();
                if (crew.Count > 0)
                {
                    TxtCrewCount.Text = $"{crew.Count} PEOPLE ONBOARD";
                    TxtCrewNames.Text = string.Join(", ", crew);
                }
                else
                {
                    TxtCrewCount.Text = "NO DATA";
                    TxtCrewNames.Text = "Cannot retrieve crew list.";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading ISS: {ex.Message}");
            }
        }

        private async void UpdateIssPositionOnMap(object sender, EventArgs e)
        {
            var coordinates = await _issService.GetCurrentIssCoordinatesAsync();

            if (coordinates.HasValue)
            {
                double lat = coordinates.Value.lat;
                double lng = coordinates.Value.lng;
                double alt = coordinates.Value.alt;
                double vel = coordinates.Value.vel;

                string jsCommand = string.Format(System.Globalization.CultureInfo.InvariantCulture, "updateISS({0}, {1});", lat, lng);
                await MyWebView.ExecuteScriptAsync(jsCommand);

                TxtLat.Text = $"{lat.ToString("F4", System.Globalization.CultureInfo.InvariantCulture)}°";
                TxtLng.Text = $"{lng.ToString("F4", System.Globalization.CultureInfo.InvariantCulture)}°";

                TxtAlt.Text = alt.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
                TxtVel.Text = vel.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

                TxtStatus.Text = "STABLE";
                TxtStatus.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF10B981"));
            }
            else
            {
                TxtStatus.Text = "LOST SIGNAL";
                TxtStatus.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFF4C4C"));
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