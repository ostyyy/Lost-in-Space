using System.ComponentModel.DataAnnotations.Schema;

namespace SpaceServer.Models
{
    [Table("topics")] 
    public class Topic
    {
        [Column("id")] 
        public int ID { get; set; }

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("authorid")]
        public int AuthorID { get; set; }

        public User? Author { get; set; }

        public List<Post> Posts { get; set; } = new();
    }
}