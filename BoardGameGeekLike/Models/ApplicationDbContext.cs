using BoardGameGeekLike.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BoardGameGeekLike.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}
        
        public DbSet<BoardGame> BoardGames { get; set; }
        
        public DbSet<Category> Categories { get; set; }
        
        public DbSet<Mechanic> Mechanics { get; set; }

        public DbSet<User> Users { get; set; }
        
        public DbSet<Rating> Ratings  { get; set; }

        public DbSet<Session> Sessions {get; set;}
    }
}
