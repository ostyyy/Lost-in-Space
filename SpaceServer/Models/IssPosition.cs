namespace SpaceServer.Models
{
    public class IssTelemetry
    {
        public string Name { get; set; }
        public long Id { get; set; }

        // Нове API віддає їх одразу як double!
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public double Altitude { get; set; }
        public double Velocity { get; set; }
        public string Visibility { get; set; }
        public long Timestamp { get; set; }
    }
    public class IssPosition
    {
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }
}
