using System.ComponentModel.DataAnnotations.Schema;

namespace SpaceServer.Models
{
    [Table("users")]
    public class User
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("login")]
        public string Login { get; set; } = string.Empty;

        [Column("password")]
        public string Password { get; set; } = string.Empty;

        public List<Topic> Topics { get; set; } = new List<Topic>();
        public List<Post> Posts { get; set; } = new List<Post>();
        public List<APOD> SavedApods { get; set; } = new List<APOD>();
    }
}
