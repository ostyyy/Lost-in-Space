namespace SpaceServer.Models
{
    public class Launches
    {
        public int Id { get; set; }
        public string LaunchName { get; set; } = string.Empty;
        public DateTime LaunchDate { get; set; }
        public string LaunchProvider { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string LaunchLocation { get; set; } = string.Empty;
    }
}
