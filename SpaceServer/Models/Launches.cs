using System.ComponentModel.DataAnnotations.Schema;

namespace SpaceServer.Models
{
    [Table("launches")] 
    public class Launches
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("launchname")] 
        public string LaunchName { get; set; } = string.Empty;

        [Column("launchdate")]
        public DateTime? LaunchDate { get; set; }

        [Column("launchprovider")]
        public string LaunchProvider { get; set; } = string.Empty;

        [Column("status")]
        public string Status { get; set; } = string.Empty;

        [Column("launchlocation")]
        public string LaunchLocation { get; set; } = string.Empty;

        [Column("userid")]
        public int UserId { get; set; }
    }
}