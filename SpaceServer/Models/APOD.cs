using System.ComponentModel.DataAnnotations.Schema;

namespace SpaceServer.Models
{
    [Table("imageoftheday")]
    public class APOD
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("date")]
        public DateTime Date { get; set; }

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("explanation")]
        public string Explanation { get; set; } = string.Empty;

        [Column("imageurl")]
        public string ImageURL { get; set; } = string.Empty;
    }
}
