﻿using BoardGameGeekLike.Models.Entities;
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
    }
}
