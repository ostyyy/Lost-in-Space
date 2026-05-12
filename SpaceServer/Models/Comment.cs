using System.ComponentModel.DataAnnotations.Schema;

namespace SpaceServer.Models
{
    [Table("comments")] 
    public class Comment
    {
        [Column("id")] 
        public int ID { get; set; }

        [Column("content")]
        public string Content { get; set; } = string.Empty;

        [Column("authorid")]
        public int? AuthorID { get; set; }
        public User? Author { get; set; }

        [Column("postid")]
        public int PostID { get; set; }
        public Post? Post { get; set; }

        [Column("parentcommentid")] 
        public int? ParentCommentID { get; set; }
        public Comment? ParentComment { get; set; }
      
        public List<Comment> Replies { get; set; } = new();

        [Column("createddate")] 
        public DateTime CreatedDate { get; set; }
    }
}