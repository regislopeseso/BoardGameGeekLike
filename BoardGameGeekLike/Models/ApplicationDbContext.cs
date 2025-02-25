using Microsoft.EntityFrameworkCore;

namespace BoardGameGeekLike.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }


    }
}
