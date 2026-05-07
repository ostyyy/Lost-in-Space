namespace SpaceServer.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public List<Topic> Topics { get; set; } = new List<Topic>();
        public List<Post> Posts { get; set; } = new List<Post>();
    }
}
