using BoardGameGeekLike.Models;
using BoardGameGeekLike.Models.Dtos.Request;
using BoardGameGeekLike.Models.Dtos.Response;
using BoardGameGeekLike.Models.Entities;
using BoardGameGeekLike.Models.Enums;
using BoardGameGeekLike.Utilities;
using BoardGameGeekLike.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Services
{
    public class DevsService
    {
        private readonly ApplicationDbContext _daoDbContext;    
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DevsService(ApplicationDbContext daoDbContext, IHttpContextAccessor httpContextAccessor)
        {
            this._daoDbContext = daoDbContext;
            this._httpContextAccessor = httpContextAccessor;
        }

        #region Board Games

        public async Task<(DevsBoardGamesSeedResponse?, string)> BoardGamesSeed(DevsBoardGamesSeedRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

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

            foreach (var boardGame in seededBoardGames)
            {
                var avgDuration = seededSessions
                    .Where(a => a.BoardGame == boardGame)
                    .Select(a => a.Duration_minutes)
                    .DefaultIfEmpty(5)
                    .Average();

                boardGame.AvgDuration_minutes = (int)Math.Ceiling(avgDuration);
            }       

            await this._daoDbContext.SaveChangesAsync();

            return(new DevsBoardGamesSeedResponse(), "Seeding was successful");
        }

        private static (bool, string) Seed_Validation(DevsBoardGamesSeedRequest? request)
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
                        AvgDuration_minutes = 0,
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
                        BirthDate = DateGenerator.GetRandomDate(currentYear - 12),
                        Gender = (Models.Enums.Gender)random.Next(0, 2),
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
    
        public async Task<(DevsBoardGamesDeleteSeedResponse?, string)> BoardGamesDeleteSeed(DevsBoardGamesDeleteSeedRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

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

        #endregion

        #region Medieval Auto Battler

        public async Task<(DevsMabSeedResponse?, string)> MabSeed(DevsMabSeedRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (cardsSeedingResult, msg1) = this.SeedCards();

            var (npcsSeedingResult, msg2) =  this.SeedNpcs(cardsSeedingResult!);

            var (questsSeedingResult, msg3) = this.SeedQuests(npcsSeedingResult!);

            await this._daoDbContext.SaveChangesAsync();

            return (null, "Seeding was successful. " + msg1 + ". " + msg2 + ". " + msg3);
        }

        private (List<MabCard>?, string) SeedCards()
        {
            var cardsSeed = new List<MabCard>();
            var cardsCount = 1;

            foreach (var cardType in new[] { MabCardType.Neutral, MabCardType.Ranged, MabCardType.Cavalry, MabCardType.Infantry })
            {
                for (int power = Constants.MinCardPower; power <= Constants.MaxCardPower; power++)
                {
                    for (int upperHand = Constants.MinCardUpperHand; upperHand <= Constants.MaxCardUpperHand; upperHand++)
                    {
                        var cardCode = cardsCount < 10 ? 
                            "MAB00" + cardsCount: 
                            (cardsCount >= 10 && cardsCount < 100) ? 
                            "MAB0" + cardsCount: 
                            "MAB" + cardsCount;

                        var cardName = power switch
                        {
                            1 => $"{MabRawMaterialType.Brass} ",
                            2 => $"{MabRawMaterialType.Copper} ",
                            3 => $"{MabRawMaterialType.Iron} ",
                            4 => $"{MabRawMaterialType.Steel} ",
                            5 => $"{MabRawMaterialType.Titanium} ",
                            6 => $"{MabRawMaterialType.Silver} ",
                            7 => $"{MabRawMaterialType.Gold} ",
                            8 => $"{MabRawMaterialType.Diamond} ",
                            9 => $"{MabRawMaterialType.Adamantium} ",
                            _ => ""
                        };


                        cardName += cardType.ToString()[0] == 'R' ?
                            " Bow" : cardType.ToString()[0] == 'C' ?
                            " Spear" : cardType.ToString()[0] == 'I' ?
                            " Sword" : "Fist";

                        var cardLevel = Helper.MabGetCardLevel(power, upperHand);

                        if(power == 0 && upperHand == 0 && cardType == MabCardType.Neutral)
                        {
                            cardName = "Truce";
                        }

                        if (power == 0 && upperHand == 0 && cardType == MabCardType.Infantry)
                        {
                            cardName = "Pickeaxe";
                        }

                        var newCard = new MabCard
                        {
                            Mab_CardCode = cardCode,
                            Mab_CardName = cardName,
                            Mab_CardPower = power,
                            Mab_CardUpperHand = upperHand,
                            Mab_CardLevel = cardLevel,
                            Mab_CardType = cardType,
                            Mab_IsCardDeleted = false,
                            Mab_IsCardDummy = true
                        };
                        cardsSeed.Add(newCard);

                        cardsCount++;
                    }
                }
            }

            this._daoDbContext.AddRange(cardsSeed);            

            return (cardsSeed, $"{cardsSeed.Count} new cards haven been successfully seeded");
        }
        private (List<MabNpc>?, string) SeedNpcs(List<MabCard> cardsSeedingResult)
        {
            var cardsDB = cardsSeedingResult;               

            //Se não existir pelo menos uma carta de cada level não é possível fazer o seed dos NPCs.
            var countCardsLvl = cardsDB.GroupBy(a => a.Mab_CardLevel).Count();
            if (countCardsLvl < Constants.MaxCardLevel - Constants.MinCardLevel)
            {
                return (null, "Error: not enough card variety for seeding NPCs. The existance of at least one card of each level is mandatory for seeding NPCs");
            }

            var npcsSeed = new List<MabNpc>();

            var neutralCards = cardsDB.Where(a => a.Mab_CardType == MabCardType.Neutral).ToList();
            var martialArtsNpcs = MabGetGuildNpcs(neutralCards, MabCardType.Neutral);
            npcsSeed.AddRange(martialArtsNpcs);

            var swordsmanNpcs = MabGetGuildNpcs(cardsDB, MabCardType.Infantry);
            npcsSeed.AddRange(swordsmanNpcs);

            var archerNpcs = MabGetGuildNpcs(cardsDB, MabCardType.Ranged);
            npcsSeed.AddRange(archerNpcs);

            var cavalryNpcs = MabGetGuildNpcs(cardsDB, MabCardType.Cavalry);
            npcsSeed.AddRange(cavalryNpcs);

            for (int level = Constants.MinCardLevel; level <= Constants.MaxCardLevel; level++)
            {
                npcsSeed.AddRange(GenerateRandomNpcs(level, cardsDB));
            }

            if (npcsSeed == null || npcsSeed.Count == 0)
            {
                return (null, "Error: seeding NPCs failed");
            }

            var barbarianKing = MabGetBarbarianKingNpc(cardsDB);

            npcsSeed.Add(barbarianKing);

            this._daoDbContext.MabNpcs.AddRange(npcsSeed);

            return (npcsSeed, "NPCs have been successfully seeded");
        }
        private static List<MabNpc> MabGetGuildNpcs(List<MabCard> mabCardsDB, MabCardType guildType)
        {
            var guildNpcs = new List<MabNpc>();

            var guild = "";

            switch (guildType)
            {
                case MabCardType.Neutral:
                    guild = "Martial Arts";
                    break;
                case MabCardType.Infantry:
                    guild = "Swordsmanship";
                    break;
                case MabCardType.Ranged:
                    guild = "Markmanship";
                    break;
                case MabCardType.Cavalry:
                    guild = "Horsemanship in arms";
                    break;
                default:
                    break;
            }

            var minPower = 0;
            var maxPower = 3;

            var fellowApprentice = new MabNpc
            {
                Mab_NpcName = $"Fellow Apprentice of {guild}",
                Mab_NpcDescription = "( 0, 0, 0, 0, 0 )",
                Mab_NpcLevel = 0,
                Mab_IsNpcDeleted = false,
                Mab_IsNpcDummy = true,
                Mab_NpcCards = new List<MabNpcCard>()
                {
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 0 && card!.Mab_CardUpperHand! == 0) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 0 && card!.Mab_CardUpperHand! == 0) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 0 && card!.Mab_CardUpperHand! == 0) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 1 && card!.Mab_CardUpperHand! == 0) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 0 && card!.Mab_CardUpperHand! == 1) },
                }
            };
            guildNpcs.Add(fellowApprentice);

            var firstTeacher = new MabNpc
            {
                Mab_NpcName = $"{guild} First Teacher",
                Mab_NpcDescription = "( 0, 0, 0, 0, 0 )",               
                Mab_IsNpcDeleted = false,
                Mab_IsNpcDummy = true,
                Mab_NpcCards = new List<MabNpcCard>()
                {
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 0 && card!.Mab_CardUpperHand! == 0) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 1 && card!.Mab_CardUpperHand! == 0) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 0 && card!.Mab_CardUpperHand! == 1) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 1 && card!.Mab_CardUpperHand! == 1) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 0 && card!.Mab_CardUpperHand! == 0) },
                }
            };
            firstTeacher.Mab_NpcLevel = 
                Helper.MabGetNpcLevel(firstTeacher.Mab_NpcCards.Select(a => a.Mab_Card.Mab_CardLevel).ToList());
            guildNpcs.Add(firstTeacher);

            var secondtTeacher = new MabNpc
            {
                Mab_NpcName = $"{guild} Second Teacher",
                Mab_NpcDescription = "( 0, 0, 0, 0, 0 )",
                Mab_IsNpcDeleted = false,
                Mab_IsNpcDummy = true,
                Mab_NpcCards = new List<MabNpcCard>()
                {
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 0 && card!.Mab_CardUpperHand! == 0) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 1 && card!.Mab_CardUpperHand! == 1) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 1 && card!.Mab_CardUpperHand! == 1) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 1 && card!.Mab_CardUpperHand! == 1) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 0 && card!.Mab_CardUpperHand! == 0) },
                }
            };
            secondtTeacher.Mab_NpcLevel =
                Helper.MabGetNpcLevel(secondtTeacher.Mab_NpcCards.Select(a => a.Mab_Card.Mab_CardLevel).ToList());
            guildNpcs.Add(secondtTeacher);

            var thirdTeacher = new MabNpc
            {
                Mab_NpcName = $"{guild} Third Teacher",
                Mab_NpcDescription = "( 0, 0, 0, 0, 0 )",
                Mab_IsNpcDeleted = false,
                Mab_IsNpcDummy = true,
                Mab_NpcCards = new List<MabNpcCard>()
                {
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 0 && card!.Mab_CardUpperHand! == 0) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 1 && card!.Mab_CardUpperHand! == 1) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 1 && card!.Mab_CardUpperHand! == 1) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 1 && card!.Mab_CardUpperHand! == 1) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 1 && card!.Mab_CardUpperHand! == 1) },
                }
            };
            thirdTeacher.Mab_NpcLevel =
                Helper.MabGetNpcLevel(thirdTeacher.Mab_NpcCards.Select(a => a.Mab_Card.Mab_CardLevel).ToList());
            guildNpcs.Add(thirdTeacher);

            var fourthTeacher = new MabNpc
            {
                Mab_NpcName = $"{guild} Fourth Teacher",
                Mab_NpcDescription = "( 0, 0, 0, 0, 0 )",
                Mab_IsNpcDeleted = false,
                Mab_IsNpcDummy = true,
                Mab_NpcCards = new List<MabNpcCard>()
                {
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 1 && card!.Mab_CardUpperHand! == 1) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 1 && card!.Mab_CardUpperHand! == 1) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 1 && card!.Mab_CardUpperHand! == 1) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 1 && card!.Mab_CardUpperHand! == 1) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 1 && card!.Mab_CardUpperHand! == 1) },
                }
            };
            fourthTeacher.Mab_NpcLevel =
                Helper.MabGetNpcLevel(fourthTeacher.Mab_NpcCards.Select(a => a.Mab_Card.Mab_CardLevel).ToList());
            guildNpcs.Add(fourthTeacher);

            var fifthTeacher = new MabNpc
            {
                Mab_NpcName = $"{guild} Fifth Teacher",
                Mab_NpcDescription = "( 0, 0, 0, 0, 0 )",
                Mab_IsNpcDeleted = false,
                Mab_IsNpcDummy = true,
                Mab_NpcCards = new List<MabNpcCard>()
                {
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 0 && card!.Mab_CardUpperHand! == 0) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 1 && card!.Mab_CardUpperHand! == 1) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 2 && card!.Mab_CardUpperHand! == 0) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 0 && card!.Mab_CardUpperHand! == 2) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 2 && card!.Mab_CardUpperHand! == 1) },
                }
            };
            fifthTeacher.Mab_NpcLevel =
                Helper.MabGetNpcLevel(fifthTeacher.Mab_NpcCards.Select(a => a.Mab_Card.Mab_CardLevel).ToList());
            guildNpcs.Add(fifthTeacher);

            var sixthTeacher = new MabNpc
            {
                Mab_NpcName = $"{guild} Sixth Teacher",
                Mab_NpcDescription = "( 0, 0, 0, 0, 0 )",
                Mab_IsNpcDeleted = false,
                Mab_IsNpcDummy = true,
                Mab_NpcCards = new List<MabNpcCard>()
                {
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 0 && card!.Mab_CardUpperHand! == 0) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 1 && card!.Mab_CardUpperHand! == 1) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 1 && card!.Mab_CardUpperHand! == 2) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 2 && card!.Mab_CardUpperHand! == 1) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 2 && card!.Mab_CardUpperHand! == 2) },
                }
            };
            sixthTeacher.Mab_NpcLevel =
                Helper.MabGetNpcLevel(sixthTeacher.Mab_NpcCards.Select(a => a.Mab_Card.Mab_CardLevel).ToList());
            guildNpcs.Add(sixthTeacher);

            var master = new MabNpc
            {
                Mab_NpcName = $"Master of {guild}",
                Mab_NpcDescription = "( 0, 0, 0, 0, 0 )",
                Mab_IsNpcDeleted = false,
                Mab_IsNpcDummy = true,
                Mab_NpcCards = new List<MabNpcCard>()
                {
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 0 && card!.Mab_CardUpperHand! == 0) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 1 && card!.Mab_CardUpperHand! == 3) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 2 && card!.Mab_CardUpperHand! == 1) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 3 && card!.Mab_CardUpperHand! == 3) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == guildType && card!.Mab_CardPower! == 3 && card!.Mab_CardUpperHand! == 3) },
                }
            };
            master.Mab_NpcLevel =
                Helper.MabGetNpcLevel(master.Mab_NpcCards.Select(a => a.Mab_Card.Mab_CardLevel).ToList());
            guildNpcs.Add(master);


            return guildNpcs;
        }
        private static MabNpc MabGetBarbarianKingNpc(List<MabCard> mabCardsDB)
        {              

            var barbarianKing = new MabNpc
            {
                Mab_NpcName = $"The Barbarian King",
                Mab_NpcDescription = "( 0, 0, 0, 0, 0 )",
                Mab_NpcLevel = 10,
                Mab_IsNpcDeleted = false,
                Mab_IsNpcDummy = true,
                Mab_NpcCards = new List<MabNpcCard>()
                {
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == MabCardType.Neutral && card!.Mab_CardPower! == 9 && card!.Mab_CardUpperHand! == 9) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == MabCardType.Neutral && card!.Mab_CardPower! == 0 && card!.Mab_CardUpperHand! == 0) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == MabCardType.Ranged && card!.Mab_CardPower! == 9 && card!.Mab_CardUpperHand! == 9) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == MabCardType.Infantry && card!.Mab_CardPower! == 9 && card!.Mab_CardUpperHand! == 9) },
                    new MabNpcCard { Mab_Card = mabCardsDB.FirstOrDefault(card => card!.Mab_CardType! == MabCardType.Infantry && card!.Mab_CardPower! == 9 && card!.Mab_CardUpperHand! == 9) },
                }
            };       

            return barbarianKing;
        }


        private static List<MabNpc> GenerateRandomNpcs(int level, List<MabCard> cardsDB)

        {
            var random = new Random();

            var npcCount = "";

            if (level == Constants.MinCardLevel || level == Constants.MaxCardLevel)
            {
                var countBotsLvlZero = 0;
                var countBotsLvlNine = 0;

                // Creating a list that will contain 10 NPCs of cardLvl 0 or cardLvl 9:
                var npcs = new List<MabNpc>();

                while (npcs.Count < 10)
                {
                    // Filtering all cards having cardLvl iguals to 0 or 9 (currently contains 4 cards npcLvl 0 or 10 npcLvl 9):
                    var cardsFiltered = cardsDB.Where(a => a.Mab_CardLevel == level).ToList();

                    // Creating a new list of NpcDeckentries
                    var validNpcDeckEntries = new List<MabNpcCard>();
                    for (int countCards = 0; countCards < 5; countCards++)
                    {
                        // Obtaining one random request out of the list of filtered cards
                        var card = cardsFiltered.OrderBy(a => random.Next()).FirstOrDefault();

                        // "Converting" the random request into a new NPC DECK ENTRY and adding it to a new list of valid deck entries:
                        validNpcDeckEntries.Add
                        (
                            new MabNpcCard
                            {
                                Mab_Card = card
                            }
                        );
                    }

                    // Creating a new npc with 5 cards (npcLvl 0 or npcLvl 9) and adding it to a list of new NPCs:          
                    var npcName = "";                    
                    var npcDescription = "";
                    var npcLvl = Helper.MabGetNpcLevel(validNpcDeckEntries.Select(a => a.Mab_Card.Mab_CardLevel).ToList());
                    if (level == 0)
                    {
                        countBotsLvlZero++;
                        
                        npcCount = countBotsLvlZero < 10 ? $"0{countBotsLvlZero}" : $"{countBotsLvlZero}";
                       
                        npcName = "NPC-LVL" + npcLvl + "-" + npcCount;

                        npcDescription = "(0, 0, 0, 0, 0)";
                    }
                    if (level == 9)
                    {
                        countBotsLvlNine++;
                        
                        npcCount = countBotsLvlNine < 10 ? $"0{countBotsLvlNine}" : $"{countBotsLvlNine}";

                        npcName = "NPC-LVL" + npcLvl + "-" + npcCount;

                        npcDescription = "(9, 9, 9, 9, 9)";
                    }
                    npcs.Add(new MabNpc
                    {
                        Mab_NpcName = npcName,
                        Mab_NpcDescription = npcDescription,
                        Mab_NpcCards = validNpcDeckEntries,
                        Mab_NpcLevel = npcLvl,
                        Mab_IsNpcDeleted = false,
                        Mab_IsNpcDummy = true
                    });

                    //This while loop will stop when the list of NPCs has 10 NPCs
                }

                return npcs;
            }
            else if (level == 1)
            {
                var npcs = new List<MabNpc>();
                var countBotsLvlOne = 0;
                var count = 1;                

                while (count <= 2)
                {
                    // Obtaining the lists of all unique sequences
                    // (for cardLvl 1 there are 5 unique possible combinations):
                    for (int i = 8; i <= 12; i++)
                    {
                        // Obtaining the list of all 5 unique sequence for cardLvl 1)
                        var levelSequence = Helper.MabGetPowerSequence(level, i);

                        // Criating a new list of valid NPC deck entries:
                        var validNpcDeckEntries = new List<MabNpcCard>();

                        // Obtaing a random request of cardLvl corresponding to its position in the sequence:
                        foreach (var cardLvl in levelSequence)
                        {
                            // Filtering all cards whose cardLvl is 1:
                            var cardsFiltered = cardsDB.Where(a => a.Mab_CardLevel == cardLvl).ToList();

                            // Obtaining one random request out of the list of filtered cards
                            var card = cardsFiltered.OrderBy(a => random.Next()).Take(1).FirstOrDefault();

                            // "Converting" the random request into a new NPC DECK ENTRY and adding it to a new list of valid deck entries:                               
                            validNpcDeckEntries.Add
                            (
                                new MabNpcCard
                                {
                                    Mab_Card = card
                                }
                            );
                        }

                        // Creating a new npc with 5 cards npcLvl 1 and adding it to a list of new NPCs:
                        countBotsLvlOne++;

                        npcCount = countBotsLvlOne < 10 ? $"0{countBotsLvlOne}" : $"{countBotsLvlOne}";

                        var npcLvl = Helper.MabGetNpcLevel(validNpcDeckEntries.Select(a => a.Mab_Card.Mab_CardLevel).ToList());
                        
                        var npcName = "NPC-LVL" + npcLvl + "-" + npcCount;
                        
                        npcs.Add(new MabNpc
                        {
                            Mab_NpcName = npcName,
                            Mab_NpcDescription = "( " + string.Join(", ", levelSequence) + " )",
                            Mab_NpcCards = validNpcDeckEntries,
                            Mab_NpcLevel = npcLvl,
                            Mab_IsNpcDeleted = false,
                            Mab_IsNpcDummy = true
                        });

                    }
                    //This while loop will stop when the count is 2.                       
                    count++;
                }

                return npcs;
            }
            else if (level == 8)
            {
                var npcs = new List<MabNpc>();
                var countBotsLvlEight = 0;
                var count = 1;


                while (count <= 4)
                {
                    // Obtaining the lists of all unique sequences
                    // (for cardLvl 8 there are 3 unique possible combinations):
                    for (int i = 5; i <= 12; i++)
                    {
                        if (i == 5 || i == 7 || i == 12)
                        {
                            // Obtaining the list of all 3 unique sequence for cardLvl 8)
                            var levelSequence = Helper.MabGetPowerSequence(level, i);

                            // Criating a new list of valid NPC deck entries:
                            var validNpcDeckEntries = new List<MabNpcCard>();

                            // Obtaing a random cards of cardLvl corresponding to its position in the sequence:
                            foreach (var cardLvl in levelSequence)
                            {
                                // Filtering all cards whose cardLvl is 8:
                                var cardsFiltered = cardsDB.Where(a => a.Mab_CardLevel == cardLvl).ToList();

                                // Obtaining one random request out of the list of filtered cards
                                var card = cardsFiltered.OrderBy(a => random.Next()).FirstOrDefault();

                                // "Converting" the random request into a new NPC DECK ENTRY and adding it to a new list of valid deck entries:                               
                                validNpcDeckEntries.Add
                                (
                                    new MabNpcCard
                                    {
                                        Mab_Card = card
                                    }
                                );
                            }

                            // Creating a new npc with 5 cards npcLvl 1 and adding it to a list of new NPCs:                        
                            countBotsLvlEight++;

                            npcCount = countBotsLvlEight < 10 ? $"0{countBotsLvlEight}" : $"{countBotsLvlEight}";

                            var npcLvl = Helper.MabGetNpcLevel(validNpcDeckEntries.Select(a => a.Mab_Card.Mab_CardLevel).ToList());

                            var npcName = "NPC-LVL" + npcLvl + "-" + npcCount;

                            npcs.Add(new MabNpc
                            {
                                Mab_NpcName = npcName,
                                Mab_NpcDescription = "( " + string.Join(", ", levelSequence) + " )",
                                Mab_NpcCards = validNpcDeckEntries,
                                Mab_NpcLevel = npcLvl,
                                Mab_IsNpcDeleted = false,
                                Mab_IsNpcDummy = true
                            });
                        }
                    }

                    count++;
                    //This while loop will stop when count is 4, since there 3 possible sequences looping 4 times will result in 12 npcs. 
                }

                //Returns only the first 10 npcs
                return npcs.Take(10).ToList();
            }
            else //(cardLvl != 0 && cardLvl != 1 && cardLvl != 8 && cardLvl != 9)
            {
                var countBotsLvlTwo = 0;
                var countBotsLvlThree = 0;
                var countBotsLvlFour = 0;
                var countBotsLvlFive = 0;
                var countBotsLvlSix = 0;
                var countBotsLvlSeven = 0;

                var validDecks = new List<List<MabNpcCard>>();
                var npcs = new List<MabNpc>();

                // Obtaining the lists of all unique sequences
                // (for cardLvl 2 up to 7 there are 12 unique possible combinations):
                for (int i = 1; i <= 12; i++)
                {
                    // Criating a new list of valid NPC deck entries:
                    var validNpcDeckEntries = new List<MabNpcCard>();

                    //ex.: cardLvl == 6 and  i == 1 => (4, 4, 6, 8, 8 )
                    var levelSequence = Helper.MabGetPowerSequence(level, i);

                    // Obtaing a random request of cardLvl corresponding to its position in the sequence:
                    foreach (var cardLvl in levelSequence)
                    {
                        // Filtering all cards by cardLvl (2, 3, 4, 5, 6 or 7):
                        var cardsFiltered = cardsDB.Where(a => a.Mab_CardLevel == cardLvl).ToList();

                        // Obtaing a random request of cardLvl corresponding to its position in the sequence:
                        var card = cardsFiltered.OrderBy(a => random.Next()).FirstOrDefault();

                        // "Converting" the random request into a new NPC DECK ENTRY and adding it to a new list of valid deck entries:
                        validNpcDeckEntries.Add
                        (
                            new MabNpcCard
                            {
                                Mab_Card = card
                            }
                        );
                    }

                    // Creating a new npc with 5 cards and adding it to a list of new NPCs:   
                    var npcName = "";
                    var npcLvl = Helper.MabGetNpcLevel(validNpcDeckEntries.Select(a => a.Mab_Card.Mab_CardLevel).ToList());

                    switch (level)
                    {
                        case 2:
                            countBotsLvlTwo++;

                            npcCount = countBotsLvlTwo < 10 ? $"0{countBotsLvlTwo}" : $"{countBotsLvlTwo}";

                            npcName = "NPC-LVL" + npcLvl + "-" + npcCount;

                            break;
                        case 3:
                            countBotsLvlThree++;

                            npcCount = countBotsLvlThree < 10 ? $"0{countBotsLvlThree}" : $"{countBotsLvlThree}";

                            npcName = "NPC-LVL" + npcLvl + "-" + npcCount;

                            break;
                        case 4:
                            countBotsLvlFour++;

                            npcCount = countBotsLvlFour < 10 ? $"0{countBotsLvlFour}" : $"{countBotsLvlFour}";

                            npcName = "NPC-LVL" + npcLvl + "-" + npcCount;

                            break;
                        case 5:
                            countBotsLvlFive++;

                            npcCount = countBotsLvlFive < 10 ? $"0{countBotsLvlFive}" : $"{countBotsLvlFive}";

                            npcName = "NPC-LVL" + npcLvl + "-" + npcCount;

                            break;
                        case 6:
                            countBotsLvlSix++;

                            npcCount = countBotsLvlSix < 10 ? $"0{countBotsLvlSix}" : $"{countBotsLvlSix}";

                            npcName = "NPC-LVL" + npcLvl + "-" + npcCount;

                            break;
                        case 7:
                            countBotsLvlSeven++;

                            npcCount = countBotsLvlSeven < 10 ? $"0{countBotsLvlSeven}" : $"{countBotsLvlSeven}";

                            npcName = "NPC-LVL" + npcLvl + "-" + npcCount;

                            break;
                    }

                    npcs.Add(new MabNpc
                    {
                        Mab_NpcName = npcName,
                        Mab_NpcDescription = "( " + string.Join(", ", levelSequence) + " )",
                        Mab_NpcCards = validNpcDeckEntries,
                        Mab_NpcLevel = npcLvl,
                        Mab_IsNpcDeleted = false,
                        Mab_IsNpcDummy = true
                    });

                    // By the end of this for loop, there will be 12 npcs                  
                }

                // Returns 10 random npcs out of the list of 12: 
                return npcs.OrderBy(a => random.Next()).Take(10).ToList(); ;
            }
        }
        
        private (DevsMabSeedResponse?, string) SeedQuests(List<MabNpc> npcsSeedingResult)
        {
            var npcsDB = npcsSeedingResult;

            var questsTitles = GetQuestsTitles();

            var questsDescriptions = GetQuestsDescription();

            var questsNpcsLists = GetQuestsNpcs(npcsDB);

            var mabQuests = new List<MabQuest>();

            for ( var i = 0; i < questsTitles.Count; i++)
            {
                mabQuests.Add(new MabQuest
                {
                    Mab_QuestTitle = questsTitles[i],
                    Mab_QuestDescription = questsDescriptions[i],
                    Mab_QuestLevel = i,
                    Mab_GoldBounty = (i + 1) * 10,
                    Mab_XpReward = (i + 1) * 20,
                    Mab_IsDummy = true,

                    Mab_Npcs = questsNpcsLists[i],                    
                });
            }
                  
            this._daoDbContext.AddRange(mabQuests);

            return (new DevsMabSeedResponse(), "Seeding Quests was successful!");
        }
        private static List<string> GetQuestsTitles()
        {
            var titleOne = "First Quest";
            var titleTwo = "Second Quest";
            var titleThree = "Third Quest";
            var titleFour = "Fourth Quest";
            var titleFive = "Fifth Quest";
            var titleSix = "Sixth Quest";
            var titleSeven = "Seventh Quest";
            var titleEight = "Eighth Quest";
            var titleNine = "Ninth Quest";
      

            var titles = new List<string>()
            {
                titleOne, titleTwo, titleThree, titleFour, titleFive,
                titleSix, titleSeven, titleEight, titleNine
            };

            return titles;
        }
        private static List<string> GetQuestsDescription()
        {           
            var descriptionOne = "Enroll in the Guild of Martial Arts"; // Duel avarage Fighters of level 2 and level 3
            var descriptionTwo = "Enroll in the Guild of Swordsmanship"; // Duel avarage Swordsman from level 0 up to level 3
            var descriptionThree = "Enroll in the Guild of Archers"; // Duel avarage Archers from level 0 up to level 3
            var descriptionFour = "Enroll in the Guild of Knights"; // Duel avarage Knights from level 0 up to level 3

            var descriptionFive = "Fight the Barbarian Bandits in the nearby Village"; // Duel avarage Fighters and Swordsman, and Archers of level 4 and level 5.

            var descriptionSix = "War at the Barbarrian's Land!"; // Fight Knights, from level 6 and up to level 7
            var descriptionSeven = "Attack the Barbarrian Town's Gate!"; // Duel Archers, from level 6 up to level 7
            var descriptionEight = "Invade the Barbarrian's Citadal"; // Duels Fighters, Swordsman, Knights, and Archers from level 6 and level 7
            var descriptionNine = "Break in the Barbarian's Castel"; // Duels Fighters, Swordsman, Knights, and Archers of level 8
            var descriptionTen = "Defeat the Barbarian King!"; // Duel the most Fighters, Swordsman, Knights, and Archers of level 9 and the Barbarian King (npcLvl 10)

            var descriptions = new List<string>()
            {
                descriptionOne, descriptionTwo, descriptionThree, descriptionFour,
                descriptionSix, descriptionSeven, descriptionEight, descriptionNine, descriptionTen
            };

            return descriptions;
        }
        private static List<List<MabNpc>> GetQuestsNpcs(List<MabNpc> Mab_Npcs)
        {
            var random = new Random();

            var mabNpcs = Mab_Npcs;

            // #1: Enroll in the Guild of Martial Arts
            // Fighters of level 0 uo to and level 3
            var npcsQuestOne = mabNpcs
                .Where(mabNpc =>                    
                    mabNpc.Mab_NpcLevel <= 3 &&
                    mabNpc.Mab_NpcCards
                    .Any(card => 
                        card.Mab_Card.Mab_CardType != MabCardType.Neutral) == false)
                .OrderBy(mabNpc => mabNpc.Mab_NpcLevel)
                .ToList();

            // #2: Enroll in the Guild of Swordsmanship
            // Swordsman of level 0 up to level 3
            var npcsQuestTwo = mabNpcs
                .Where(mabNpc =>                
                    mabNpc.Mab_NpcLevel <= 3 &&
                    mabNpc.Mab_NpcCards
                    .Any(card =>
                        card.Mab_Card.Mab_CardType != MabCardType.Infantry) == false)
                .OrderBy(mabNpc => mabNpc.Mab_NpcLevel)
                .ToList();

            // #3: Enroll in the Guild of Archers
            // Archers of level 0 up to level 3
            var npcsQuestThree = mabNpcs
                .Where(mabNpc =>
                    mabNpc.Mab_NpcLevel <= 3 &&
                    mabNpc.Mab_NpcCards
                    .Any(card =>
                        card.Mab_Card.Mab_CardType != MabCardType.Ranged) == false)
                .OrderBy(mabNpc => mabNpc.Mab_NpcLevel)
                .ToList();

            // #4: Enroll in the Guild of Knights
            // Knights of level 0 up to level 3
            var npcsQuestFour = mabNpcs
                .Where(mabNpc =>
                    mabNpc.Mab_NpcLevel <= 3 &&
                    mabNpc.Mab_NpcCards
                    .Any(card =>
                        card.Mab_Card.Mab_CardType != MabCardType.Cavalry) == false)
                .OrderBy(mabNpc => mabNpc.Mab_NpcLevel)
                .ToList();

            // #5: War at the Barbarrian's Land!
            // Fighters, Swordsman, Knights and Archers of level 4 and level 5
            var npcsQuestFive = mabNpcs
                .Where(mabNpc =>
                    mabNpc.Mab_NpcLevel >= 4 &&
                    mabNpc.Mab_NpcLevel <= 5)             
                .OrderBy(a => random.Next()) // Randomizing the list of npcs
                .Skip(random.Next(1,9))
                .Take(10)
                .OrderBy(mabNpc => mabNpc.Mab_NpcLevel)
                .ToList();

            // #6: Attack the Barbarrian Town's Gate!
            // Fighters, Swordsman, Knights and Archers from level 5 up to level 6.
            var npcsQuestSix = mabNpcs
                .Where(mabNpc =>
                    mabNpc.Mab_NpcLevel >= 5 &&
                    mabNpc.Mab_NpcLevel <= 6)
                .OrderBy(a => random.Next()) // Randomizing the list of npcs
                .Skip(random.Next(1, 9))
                .Take(10)
                .OrderBy(mabNpc => mabNpc.Mab_NpcLevel)
                .ToList();


            // #7: Invade the Barbarrian's Citadel
            // Fighters, Swordsman, Knights and Archers of level 7 and 8
            var npcsQuestSeven = mabNpcs
                .Where(mabNpc =>
                    mabNpc.Mab_NpcLevel >= 7 &&
                    mabNpc.Mab_NpcLevel <= 8)
                .OrderBy(a => random.Next()) // Randomizing the list of npcs
                .Skip(random.Next(1, 9))
                .Take(10)
                .OrderBy(mabNpc => mabNpc.Mab_NpcLevel)
                .ToList();



            // #8: Break in the Barbarian's Castel
            // Fighters, Swordsman, Knights, and Archers from level 8 and level 9
            var npcsQuestEight = mabNpcs
                .Where(mabNpc =>
                    mabNpc.Mab_NpcLevel >= 8 &&
                    mabNpc.Mab_NpcLevel <= 9)
                .OrderBy(mabNpc => mabNpc.Mab_NpcLevel)
                .ToList();         

            // #9: Defeat the Barbarian King!
            // Swordsman level 10
            var npcsQuestNine = mabNpcs.Where(a => a.Mab_NpcLevel == 10).ToList();

            var mabNpcsLists = new List<List<MabNpc>>()
            { 
                npcsQuestOne, 
                npcsQuestTwo, 
                npcsQuestThree, 
                npcsQuestFour, 
                npcsQuestFive, 
                npcsQuestSix, 
                npcsQuestSeven, 
                npcsQuestEight,
                npcsQuestNine             
            };

            return mabNpcsLists;
        }
        
        public async Task<(DevsMabDeleteSeedResponse?, string)> MabDeleteSeed(DevsMabDeleteSeedRequest? request = null)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }           

            var cardIds = await this._daoDbContext
                .MabCards
                .Where(a => a.Mab_IsCardDummy == true)
                .Select(a => a.Id)
                .ToListAsync();

            List<int> playerCardIds = await this._daoDbContext
                .MabPlayerCards
                .Where(playerCard => cardIds.Any(cardId => cardId == playerCard.Mab_CardId))
                .Select(playerCard => playerCard.Id)
                .ToListAsync();

            // Fetching Duels having cards which derive from DUMMY CARDS                   
            var duelsDB = await this._daoDbContext
                .MabDuels             
                .ToListAsync();

            // Fetching Duels having cards which derive from DUMMY CARDS                   
            var battlesDB = await this._daoDbContext
                .MabBattles
                .ToListAsync();          

            // Fetching ASSIGNED CARDS deriving from DUMMY CARDS
            var assignedCardsDB = await this._daoDbContext
                .MabAssignedCards
                .Where(a => a.Mab_PlayerCard!.Mab_Card!.Mab_IsCardDummy == true)
                .ToListAsync();

            // Fetching PLAYER CARDS deriving from DUMMY CARDS
            var playerCardsDB = await this._daoDbContext
                .MabPlayerCards
                .Where(playerCard => playerCardIds.Any(playerCardId => playerCardId == playerCard.Id))
                .ToListAsync();

            // Fetching DECKS having cards which derive from DUMMY CARDS            
            var playerDecksDB = await this._daoDbContext
               .MabDecks
              .ToListAsync();           
                        
            // Fetching DUMMY QUESTS
            var questsDB = await this._daoDbContext
                .MabQuests
                .Where(a => a.Mab_IsDummy == true)
                .ToListAsync();


            // Fetching NPC CARDS related to DUMMY CARDS
            var npcCardsDB = await this._daoDbContext
                .MabNpcCards
                .Where(card => card.Mab_Card.Mab_IsCardDummy == true)
                .ToListAsync();

            // Fetching DUMMY NPCs
            var npcsDB = await this._daoDbContext
                .MabNpcs
                .Where(a => a.Mab_IsNpcDummy == true)
                .ToListAsync();

            // Fetching DUMMY CARDS
            var cardsDB =  await this._daoDbContext
                .MabCards
                .Where(a => a.Mab_IsCardDummy == true)
                .ToListAsync();

            // Deleting...
            this._daoDbContext
                .MabDuels
                .RemoveRange(duelsDB);

            this._daoDbContext
                .MabBattles
                .RemoveRange(battlesDB);

            this._daoDbContext
                .MabAssignedCards
                .RemoveRange(assignedCardsDB);

            this._daoDbContext
                .MabPlayerCards
                .RemoveRange(playerCardsDB);

            this._daoDbContext
                  .MabDecks
                  .RemoveRange(playerDecksDB);           

            this._daoDbContext
                .MabQuests
                .RemoveRange(questsDB);

            this._daoDbContext
                .MabNpcCards
                .RemoveRange(npcCardsDB);

            this._daoDbContext
                .MabNpcs
                .RemoveRange(npcsDB);

            this._daoDbContext
                .MabCards
                .RemoveRange(cardsDB);

            await this._daoDbContext.SaveChangesAsync();

            return (null, "Dummies deleted successfully");
        }

        #endregion
    }
}