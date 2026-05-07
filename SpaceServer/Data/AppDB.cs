using Microsoft.EntityFrameworkCore;
using SpaceServer.Models;

namespace SpaceServer.Data
{
    public class AppDB : DbContext
    {
        public AppDB(DbContextOptions<AppDB> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Launches> Launches { get; set; }
        public DbSet<ImageOfTheDay> ImagesOfTheDay { get; set; }
    }
}
