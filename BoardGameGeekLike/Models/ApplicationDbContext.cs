using BoardGameGeekLike.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BoardGameGeekLike.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}
        
        public DbSet<Category> Categories { get; set; }

        public DbSet<Mechanic> Mechanics { get; set; }
        
        public DbSet<BoardGame> BoardGames { get; set; }

        public DbSet<BoardGameMechanics> BoardGameMechanics { get; set; }

        public DbSet<User> Users { get; set; }
    
        public DbSet<BoardGameRatings> BoardGameRatings  { get; set; }

        public DbSet<PlayLog> PlayLogs {get; set;}
    }
}
