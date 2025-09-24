using BoardGameGeekLike.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BoardGameGeekLike.Models
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}
        
        public DbSet<BoardGame> BoardGames { get; set; }
        
        public DbSet<Category> Categories { get; set; }
        
        public DbSet<Mechanic> Mechanics { get; set; }      
        
        public DbSet<Rating> Ratings  { get; set; }

        public DbSet<Session> Sessions {get; set;}



        public DbSet<LifeCounterTemplate> LifeCounterTemplates { get; set; }

        public DbSet<LifeCounterManager> LifeCounterManagers { get; set; }

        public DbSet<LifeCounterPlayer> LifeCounterPlayers { get; set; }




        public DbSet<MabCampaign> MabCampaigns { get; set; }   
        
        
        public DbSet<MabCard> MabCards { get; set; }

        
        public DbSet<MabNpc> MabNpcs { get; set; }
        
        public DbSet<MabNpcCard> MabNpcCards { get; set; }

        
        public DbSet<MabDefeatedNpc> MabDefeatedNpcs { get; set; }


        public DbSet<MabQuest> MabQuests { get; set; }

        public DbSet<MabFulfilledQuest> MabFulfilledQuests { get; set; }



        public DbSet<MabPlayerCard> MabPlayerCards { get; set; }

        public DbSet<MabAssignedCard> MabAssignedCards { get; set; }

        public DbSet<MabDeck> MabDecks { get; set; }   

        public DbSet<MabBattle> MabBattles { get; set; }

        public DbSet<MabDuel> MabDuels { get; set; }
    }
}
