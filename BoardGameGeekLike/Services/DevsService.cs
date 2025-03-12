using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BoardGameGeekLike.Models;
using BoardGameGeekLike.Models.Dtos.Request;
using BoardGameGeekLike.Models.Dtos.Response;
using BoardGameGeekLike.Models.Entities;
using BoardGameGeekLike.Utility;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace BoardGameGeekLike.Services
{
    public class DevsService
    {
        private readonly ApplicationDbContext _daoDbContext;

        public DevsService(ApplicationDbContext daoDbContext)
        {
            this._daoDbContext = daoDbContext;
        }

        public async Task<(DevsSeedResponse?, string)> Seed(DevsSeedRequest? request)
        {
            var seededCategories = CategorySeeder(10);
            if(seededCategories == null || seededCategories.Count != 10)
            {
                return (null, "Error: seeding CATEGORIES failed");
            }           
            await this._daoDbContext.Categories.AddRangeAsync(seededCategories);

            var seededMechanics = MechanicSeeder(10);
            if(seededMechanics == null || seededMechanics.Count != 10)
            {
                return (null, "Error: seeding MECHANICS failed");
            }
            await this._daoDbContext.Mechanics.AddRangeAsync(seededMechanics);

            var seededBoardGames = BoardGameSeeder(10, seededCategories, seededMechanics);
            if(seededBoardGames == null || seededBoardGames.Count != 10)
            {
                return (null, "Error: seeding BOARD GAMES failed");
            }
            await this._daoDbContext.BoardGames.AddRangeAsync(seededBoardGames);

            var seededUsers = UserSeeder(200);
            if(seededUsers == null || seededUsers.Count != 200)
            {
                return (null, "Error: seeding USERS failed");
            }
            await this._daoDbContext.Users.AddRangeAsync(seededUsers);

            await this._daoDbContext.SaveChangesAsync();


            var seededSessions = SessionSeeder(30, seededUsers, seededBoardGames);
            if(seededSessions == null || seededSessions.Count !=  6000)
            {
                return (null, "Error: seeding SESSIONS failed");
            }
            await this._daoDbContext.Sessions.AddRangeAsync(seededSessions);
  
            var random = new Random();

            var ratings = new List<Rating>(){};

            foreach(var boardGame in seededBoardGames)
            {
                var users = seededSessions.Where(a => a.BoardGame == boardGame)
                    .Select(a => a.User)
                    .Distinct()
                    .ToList(); 

                var boardGameRatings = new List<decimal>(){};               

                foreach(var user in users)
                {                                  
                    decimal baseRating = random.Next(2, 5); // Give each board game a base quality rating
                    decimal ratingValue = Math.Clamp(baseRating + random.Next(-1, 2), 0, 5);

                    boardGameRatings.Add(ratingValue);

                    ratings.Add(new Rating
                    {
                        Rate = ratingValue,
                        BoardGame = boardGame,
                        User = user

                    });

                    boardGame.RatingsCount++;
                }
                
                boardGame.AverageRating = boardGameRatings.Any() ? 
                    boardGameRatings.Average() : 0;               
            }

            await this._daoDbContext.Ratings.AddRangeAsync(ratings);

            await this._daoDbContext.SaveChangesAsync();

            return(null, "Seeding was successful");
        }

        private static List<Category>? CategorySeeder(int n)
        {
            var newCategories = new List<Category>(){};
            
            for (int catCount = 1; catCount <= n; catCount ++)
            {
                newCategories.Add
                (
                    new Category
                    {
                        Name = $"category {catCount}"
                    }
                );
            }

            return newCategories;
        }

        private static List<Mechanic>? MechanicSeeder(int n)
        {
            var newMechanics = new List<Mechanic>(){};
            
            for (int mecCount = 1; mecCount <= 10; mecCount ++)
            {
                newMechanics.Add
                (
                    new Mechanic
                    {
                        Name = $"mechanic {mecCount}"
                    }
                );
            }

            return newMechanics;
        }

        private static List<BoardGame>? BoardGameSeeder(int n, List<Category> seededCategories, List<Mechanic> seededMechanics)
        {
            var newBoardGames = new List<BoardGame>(){};

            var random = new Random();

            for(int bgCount = 1; bgCount <= 10; bgCount++)
            {
                var minPlayersCount = random.Next(1,3);
                
                var maxPlayersCount = minPlayersCount == 1 ? random.Next(1,5) : random.Next(2,6);
                
                var minAges = new int[] {5, 8, 12, 18};

                var mechanicsCount = random.Next(1,5);
                
                var boardGameMechanics = new List<Mechanic>(){};
                
                for(int i = 0; i < mechanicsCount; i++)
                {
                    boardGameMechanics.Add(seededMechanics[random.Next(0,seededMechanics.Count)]);
                }

                newBoardGames.Add
                (
                    new BoardGame
                    {
                        Name = $"board game {bgCount}",
                        Description = $"this is the board game number {bgCount}",
                        MinPlayersCount = minPlayersCount,
                        MaxPlayersCount = maxPlayersCount,
                        MinAge = minAges[random.Next(0, minAges.Length)],
                        Category = seededCategories[random.Next(0, seededCategories.Count)],
                        Mechanics = boardGameMechanics,
                        AverageRating = 0,
                        IsDeleted = false                                         
                    }
                );
            }

            return newBoardGames;
        } 
    
        private static List<User>? UserSeeder(int n)
        {
            var newUsers = new List<User>(){};

            var random = new Random(); 

            int currentYear = DateTime.Today.Year;           

            for (int usersCount = 1; usersCount <= n; usersCount ++)
            {               
                newUsers.Add
                (
                    new User
                    {
                        Nickname = $"user #{usersCount}",
                        Email = $"user{usersCount}@email.com",
                        BirthDate = DateGenerator.GetRandomDate(currentYear-12),
                        IsDeleted = false                       
                    }
                );
            }

            return newUsers;
        }

        private static List<Session>? SessionSeeder(int n, List<User> seededUsers, List<BoardGame> seededBoardGames)
        {
            var newSessions = new List<Session>(){};

            var random = new Random();

            var countSessions = 0;

            var randomDuration = new int[] {15, 30, 45, 60, 
                                            75, 90, 105, 120, 
                                            135, 150, 165, 180, 
                                            195, 210, 225, 240,
                                            255, 270, 285, 300};
            
            int currentYear = DateTime.UtcNow.Year; 
            int currentMonth = DateTime.UtcNow.Month;
            int currentDay = DateTime.UtcNow.Day;

            foreach(var user in seededUsers)
            {
                while (countSessions < n)
                {
                    var randomBoardGame = seededBoardGames.OrderBy(a => random.Next()).First();                  
                    
                    var randomDate = DateGenerator.GetRandomDate(currentYear);

                    if (randomDate.Year == currentYear)
                    {
                        if (randomDate.Month > currentMonth)
                        {
                            randomDate = randomDate.AddMonths(currentMonth - randomDate.Month);
                        }

                        if (randomDate.Month == currentMonth && randomDate.Day > currentDay)
                        {
                            randomDate = randomDate.AddDays(currentDay - randomDate.Day);
                        }
                    }
                   
                    newSessions.Add
                    (
                        new Session
                        {
                            UserId = user.Id,
                            BoardGame = randomBoardGame,
                            Date = randomDate,
                            PlayersCount = random.Next(1,6),
                            Duration_minutes = randomDuration[random.Next(0, randomDuration.Length)],
                            IsDeleted = false
                        }
                    );
                    countSessions++;
                }
                countSessions = 0;
            }

            return newSessions;
        }
    }
}