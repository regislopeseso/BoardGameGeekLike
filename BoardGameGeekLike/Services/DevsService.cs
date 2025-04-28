using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BoardGameGeekLike.Models;
using BoardGameGeekLike.Models.Dtos.Request;
using BoardGameGeekLike.Models.Dtos.Response;
using BoardGameGeekLike.Models.Entities;
using BoardGameGeekLike.Utility;
using Microsoft.AspNetCore.Identity;
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
            var (isValid, message) = Seed_Validation(request);
            
            if(isValid == false)
            {
                return (null, message);
            }

            var seededCategories = CategorySeeder(request!.CategoriesCount!.Value);

            if(seededCategories == null || seededCategories.Count != request!.CategoriesCount!.Value)
            {
                return (null, "Error: seeding CATEGORIES failed");
            }           
            await this._daoDbContext.Categories.AddRangeAsync(seededCategories);

            var seededMechanics = MechanicSeeder(request.MecanicsCount!.Value);
            if(seededMechanics == null || seededMechanics.Count != request.MecanicsCount!.Value)
            {
                return (null, "Error: seeding MECHANICS failed");
            }
            await this._daoDbContext.Mechanics.AddRangeAsync(seededMechanics);

            var seededBoardGames = BoardGameSeeder(request.BoardGamesCount!.Value, seededCategories, seededMechanics);
            if(seededBoardGames == null || seededBoardGames.Count != request.BoardGamesCount!.Value)
            {
                return (null, "Error: seeding BOARD GAMES failed");
            }
            await this._daoDbContext.BoardGames.AddRangeAsync(seededBoardGames);

            var seededUsers = UserSeeder(request.UsersCount!.Value);
            if(seededUsers == null || seededUsers.Count != request.UsersCount!.Value)
            {
                return (null, "Error: seeding USERS failed");
            }
            await this._daoDbContext.Users.AddRangeAsync(seededUsers);

            await this._daoDbContext.SaveChangesAsync();


            var seededSessions = SessionSeeder(request.SessionsCount!.Value, seededUsers, seededBoardGames);
            if(seededSessions == null || seededSessions.Count == 0)
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

        private static (bool, string) Seed_Validation(DevsSeedRequest? request)
        {
            if(request != null && request.CategoriesCount < 1)
            {
                return (false, "Error: requested CategoriesCount < 1");
            }

            if (request != null && request.MecanicsCount < 1)
            {
                return (false, "Error: requested MecanicsCount < 1");
            }

            if (request != null && request.BoardGamesCount < 1)
            {
                return (false, "Error: requested BoardGamesCount < 1");
            }

            if (request != null && request.UsersCount < 1)
            {
                return (false, "Error: requested UsersCount < 1");
            }

            if (request != null && request.SessionsCount < 1)
            {
                return (false, "Error: requested SessionsCount < 1");
            }          

            return (true, String.Empty);
        }

        private static List<Category>? CategorySeeder(int categoriesCount)
        {
            var newCategories = new List<Category>(){};
            
            for (int catCount = 1; catCount <= categoriesCount; catCount ++)
            {
                newCategories.Add
                (
                    new Category
                    {
                        Name = $"category {catCount}",
                        IsDummy = true
                    }
                );
            }

            return newCategories;
        }

        private static List<Mechanic>? MechanicSeeder(int mecanicsCount)
        {
            var newMechanics = new List<Mechanic>(){};
            
            for (int mecCount = 1; mecCount <= mecanicsCount; mecCount ++)
            {
                newMechanics.Add
                (
                    new Mechanic
                    {
                        Name = $"mechanic {mecCount}",
                        IsDummy = true
                    }
                );
            }

            return newMechanics;
        }

        private static List<BoardGame>? BoardGameSeeder(int boardGamesCount, List<Category> seededCategories, List<Mechanic> seededMechanics)
        {
            var newBoardGames = new List<BoardGame>(){};

            var random = new Random();

            for(int bgCount = 1; bgCount <= boardGamesCount; bgCount++)
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
                        IsDeleted = false,
                        IsDummy = true
                    }
                );
            }

            return newBoardGames;
        } 
    
        private static List<User>? UserSeeder(int usersCount)
        {
            var newUsers = new List<User>(){};

            var random = new Random(); 

            int currentYear = DateTime.Today.Year;           

            for (int usersC = 1; usersC <= usersCount; usersC ++)
            {               
                newUsers.Add
                (
                    new User
                    {
                        Name = $"user #{usersC}",
                        UserName = $"user{usersC}@email.com",
                        Email = $"user{usersC}@email.com",
                        BirthDate = DateGenerator.GetRandomDate(currentYear-12),
                        IsDummy = true
                    }
                );
            }

            return newUsers;
        }

        private static List<Session>? SessionSeeder(int sessionsCount, List<User> seededUsers, List<BoardGame> seededBoardGames)
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
                var randomMaxSessionsCount = random.Next(countSessions, sessionsCount + 1);
                
                while (countSessions < randomMaxSessionsCount)
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

                    randomBoardGame.SessionsCount++;

                    countSessions++;
                }
                countSessions = 0;
            }

            return newSessions;
        }
    
        public async Task<(DevsDeleteSeedResponse?, string)> DeleteSeed(DevsDeleteSeedRequest? request)
        {
            await this._daoDbContext
                .BoardGames
                .Where(a => a.IsDummy == true)
                .ExecuteDeleteAsync();

            await this._daoDbContext
                .Categories
                .Where(a => a.IsDummy == true)
                .ExecuteDeleteAsync();

            await this._daoDbContext
                .Mechanics
                .Where(a => a.IsDummy == true)
                .ExecuteDeleteAsync();


            await this._daoDbContext
                .Users
                .Where(a => a.IsDummy == true)
                .ExecuteDeleteAsync();


            return (null, "Dummies deleted successfully");
        }
    }
}