using System.ComponentModel.DataAnnotations.Schema;

namespace SpaceServer.Models
{
    [Table("posts")]
    public class Post
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("content")]
        public string Content { get; set; } = string.Empty;

        [Column("authorid")]
        public int AuthorID { get; set; }

        public User? Author { get; set; }

        [Column("topicid")]
        public int TopicID { get; set; }
        public Topic? Topic { get; set; }

        [Column("createddate")]
        public DateTime CreatedDate { get; set; }
    }
}
