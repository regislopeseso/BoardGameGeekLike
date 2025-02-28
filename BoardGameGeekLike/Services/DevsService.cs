using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BoardGameGeekLike.Models;
using BoardGameGeekLike.Models.Dtos.Request;
using BoardGameGeekLike.Models.Dtos.Response;
using BoardGameGeekLike.Models.Entities;
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
                        MinAge = minAges[random.Next(0,minAges.Length)],
                        Category = seededCategories[random.Next(0, seededCategories.Count)],
                        BoardGameMechanics = boardGameMechanics,
                        AverageRating = random.Next(0,5),
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

            for (int usersCount = 1; usersCount <= n; usersCount ++)
            {
                var year = random.Next(1945,2014);

                var month = random.Next(1,13);
                
                var day = month switch
                {
                    2 => random.Next(1, 29), // February (ignoring leap years for simplicity)
                    4 or 6 or 9 or 11 => random.Next(1, 31), // April, June, September, November
                    _ => random.Next(1, 32) // Months with 31 days
                };

                string date_string = $"{day:00}/{month:00}/{year}";

                var parsedDate = DateOnly.ParseExact(date_string,"dd/MM/yyyy");

                newUsers.Add
                (
                    new User
                    {
                        Nickname = $"user #{usersCount}",
                        Email = $"user{usersCount}@email.com",
                        BirthDate = parsedDate,
                        IsDeleted = false                       
                    }
                );
            }

            return newUsers;
        }

    }
}