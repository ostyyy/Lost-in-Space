namespace SpaceServer.Models
{
    public class Topic
    {
        public int ID { get; set; }
        public string Title { get; set; } = string.Empty;
        public int AuthorID { get; set; }
        public User? Author { get; set; }

        public List<Post> Posts { get; set; } = new();
    }
}
