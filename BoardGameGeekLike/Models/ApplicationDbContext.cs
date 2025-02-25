using BoardGameGeekLike.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BoardGameGeekLike.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}

        public DbSet<BoardGame> BoardGames { get; set; }


    }
}
