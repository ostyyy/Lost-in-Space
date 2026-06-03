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

        public async Task<(double lat, double lng, double alt, double vel)?> GetCurrentIssCoordinatesAsync()
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
                        return (data.Latitude, data.Longitude, data.Altitude, data.Velocity);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ISS Service Error: {ex.Message}");
            }

            return null;
        }


        public class AstrosResponse
        {
            public int Number { get; set; }
            public List<Astro> People { get; set; }
        }

        public class Astro
        {
            public string Name { get; set; }
            public string Craft { get; set; }
        }

        public async Task<List<string>> GetISSCrew()
        {
            try 
            {
                var response = await _httpClient.GetAsync("http://api.open-notify.org/astros.json");
                if (response.IsSuccessStatusCode)
                {
                    var jsonText = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var data = JsonSerializer.Deserialize<AstrosResponse>(jsonText, options);

                    if (data != null && data.People != null)
                    {
                        var issCrew = data.People
                            .Where(p => p.Craft == "ISS")
                            .Select(p => p.Name)
                            .ToList();

                        return issCrew;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Astronauts API Error: {ex.Message}");
            }
            return new List<string>();
        }
    }
}