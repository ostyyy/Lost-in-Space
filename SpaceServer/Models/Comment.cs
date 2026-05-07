namespace SpaceServer.Models
{
    public class Comment
    {
        public int ID { get; set; }
        public string Content { get; set; } = string.Empty;

        public int? AuthorID { get; set; }
        public User? Author { get; set; }

        public int PostID { get; set; }
        public Post? Post { get; set; }

        public int? ParentCommentID { get; set; }
        public Comment? ParentComment { get; set; }

        public List<Comment> Replies { get; set; } = new();

        public DateTime CreatedDate { get; set; }
    }
}
