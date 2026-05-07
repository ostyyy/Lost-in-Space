namespace SpaceServer.Models
{
    public class ImageOfTheDay
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public string ImageURL { get; set; } = string.Empty;
    }
}
