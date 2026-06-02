using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using SpaceServer.Models;

namespace SpaceClient.Services
{
    public class IssTrackingService
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        private const string IssApiUrl = "https://api.wheretheiss.at/v1/satellites/25544";

        public async Task<(double lat, double lng)?> GetCurrentIssCoordinatesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(IssApiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonText = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    var data = JsonSerializer.Deserialize<IssTelemetry>(jsonText, options);

                    if (data != null)
                    {
                        return (data.Latitude, data.Longitude);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ISS Service Error: {ex.Message}");
            }

            return null;
        }
    }
}