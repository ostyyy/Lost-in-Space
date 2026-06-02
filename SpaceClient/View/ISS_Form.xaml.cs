using System;
using System.Globalization;     // Для правильного форматування крапки в координатах
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;  // Для DispatcherTimer
using SpaceClient.Services;      // Підключаємо наш сервіс трекінгу МКС

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
                // 1. Чекаємо ініціалізації WebView2 движка
                await MyWebView.EnsureCoreWebView2Async();

                // 2. Вказуємо шлях до нашого index.html у папці Web
                string htmlPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Web", "index.html");
                MyWebView.CoreWebView2.Navigate(htmlPath);

                // 3. Запускаємо таймер оновлення (запит кожні 3 секунди)
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
            // Отримуємо координати з нашого сервісу
            var coordinates = await _issService.GetCurrentIssCoordinatesAsync();

            if (coordinates.HasValue)
            {
                double lat = coordinates.Value.lat;
                double lng = coordinates.Value.lng;

                // ЗМІНЕНО ТУТ: тепер назва чітко збігається з твоїм JS (updateISS)
                string jsCommand = string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "updateISS({0}, {1});", lat, lng
                );

                // Викликаємо функцію всередині index.js
                await MyWebView.ExecuteScriptAsync(jsCommand);
            }
        }

        // Подія спрацьовує, коли ми йдемо зі сторінки трекера в інше меню
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