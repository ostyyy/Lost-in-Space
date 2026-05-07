namespace SpaceServer.Models
{
    public class Post
    {
        public int ID { get; set; }
        public string Content { get; set; } = string.Empty;

        public int AuthorID { get; set; }
        public User? Author { get; set; }

        public int TopicID { get; set; }
        public Topic? Topic { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
