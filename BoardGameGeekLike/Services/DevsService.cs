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
using System.Security.Claims;
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

        public async Task<(DevsMedievalAutoBattlerSeedResponse?, string)> MedievalAutoBattlerSeed(DevsMedievalAutoBattlerSeedRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (cardsSeedingResult, msg1) = await this.SeedCards();
            var (npcsSeedingResult, msg2) = await this.SeedNpcs();

            return (null, "Seeding was successful. " + msg1 + ". " + msg2);
        }

        private async Task<(DevsMedievalAutoBattlerSeedResponse?, string)> SeedCards()
        {
            var cardsSeed = new List<Card>();

            foreach (var cardType in new[] { CardType.Archer, CardType.Cavalry, CardType.Spearman })
            {
                for (int power = Constants.MinCardPower; power <= Constants.MaxCardPower; power++)
                {
                    for (int upperHand = Constants.MinCardUpperHand; upperHand <= Constants.MaxCardUpperHand; upperHand++)
                    {
                        var newCard = new Card
                        {
                            Name = cardType.ToString() + " *" + power + "|" + upperHand + "*",
                            Power = power,
                            UpperHand = upperHand,
                            Level = Helper.GetCardLevel(power, upperHand),
                            Type = cardType,
                            IsDeleted = false,
                            IsDummy = true
                        };
                        cardsSeed.Add(newCard);
                    }
                }
            }

            this._daoDbContext.AddRange(cardsSeed);

            await this._daoDbContext.SaveChangesAsync();

            return (null, $"{cardsSeed.Count} new cards haven been successfully seeded");
        }

        private async Task<(DevsMedievalAutoBattlerSeedResponse?, string)> SeedNpcs()
        {
            var cardsDB = await _daoDbContext
                                    .Cards
                                    .Where(a => a.IsDeleted == false)
                                    .ToListAsync();

            if (cardsDB == null || cardsDB.Count == 0)
            {
                return (null, "Error: cards not found");
            }

            //Se não existir pelo menos uma carta de cada level não é possível fazer o seed dos NPCs.
            var countCardsLvl = cardsDB.GroupBy(a => a.Level).Count();
            if (countCardsLvl < Constants.MaxCardLevel - Constants.MinCardLevel)
            {
                return (null, "Error: not enough card variety for seeding NPCs. The existance of at least one card of each level is mandatory for seeding NPCs");
            }

            var npcsSeed = new List<Npc>();

            for (int level = Constants.MinCardLevel; level <= Constants.MaxCardLevel; level++)
            {
                npcsSeed.AddRange(GenerateRandomNpcs(level, cardsDB));
            }

            if (npcsSeed == null || npcsSeed.Count == 0)
            {
                return (null, "Error: seeding NPCs failed");
            }

            _daoDbContext.AddRange(npcsSeed);

            await _daoDbContext.SaveChangesAsync();

            return (null, "NPCs have been successfully seeded");
        }

        private static List<Npc> GenerateRandomNpcs(int level, List<Card> cardsDB)
        {
            var random = new Random();

            if (level == Constants.MinCardLevel || level == Constants.MaxCardLevel)
            {
                var countBotsLvlZero = 0;
                var countBotsLvlNine = 0;

                // Creating a list that will contain 10 NPCs of cardLvl 0 or cardLvl 9:
                var npcs = new List<Npc>();

                while (npcs.Count < 10)
                {
                    // Filtering all cards having cardLvl iguals to 0 or 9 (currently contains 4 cards npcLvl 0 or 10 npcLvl 9):
                    var cardsFiltered = cardsDB.Where(a => a.Level == level).ToList();

                    // Creating a new list of NpcDeckentries
                    var validNpcDeckEntries = new List<NpcDeckEntry>();
                    for (int countCards = 0; countCards < 5; countCards++)
                    {
                        // Obtaining one random request out of the list of filtered cards
                        var card = cardsFiltered.OrderBy(a => random.Next()).FirstOrDefault();

                        // "Converting" the random request into a new NPC DECK ENTRY and adding it to a new list of valid deck entries:
                        validNpcDeckEntries.Add
                        (
                            new NpcDeckEntry
                            {
                                Card = card
                            }
                        );
                    }

                    // Creating a new npc with 5 cards (npcLvl 0 or npcLvl 9) and adding it to a list of new NPCs:          
                    var npcName = "";
                    var npcDescription = "";
                    var npcLvl = Helper.GetNpcLevel(validNpcDeckEntries.Select(a => a.Card.Level).ToList());
                    if (level == 0)
                    {
                        countBotsLvlZero++;
                        npcName = "NPC-LVL" + npcLvl + "-" + countBotsLvlZero;
                        npcDescription = "(0, 0, 0, 0, 0)";
                    }
                    if (level == 9)
                    {
                        countBotsLvlNine++;
                        npcName = "NPC-LVL" + npcLvl + "-" + countBotsLvlNine;
                        npcDescription = "(9, 9, 9, 9, 9)";
                    }
                    npcs.Add(new Npc
                    {
                        Name = npcName,
                        Description = npcDescription,
                        Deck = validNpcDeckEntries,
                        Level = npcLvl,
                        IsDeleted = false,
                        IsDummy = true
                    });

                    //This while loop will stop when the list of NPCs has 10 NPCs
                }

                return npcs;
            }
            else if (level == 1)
            {
                var npcs = new List<Npc>();
                var countBotsLvlOne = 0;
                var count = 1;

                while (count <= 2)
                {
                    // Obtaining the lists of all unique sequences
                    // (for cardLvl 1 there are 5 unique possible combinations):
                    for (int i = 8; i <= 12; i++)
                    {
                        // Obtaining the list of all 5 unique sequence for cardLvl 1)
                        var levelSequence = Helper.GetPowerSequence(level, i);

                        // Criating a new list of valid NPC deck entries:
                        var validNpcDeckEntries = new List<NpcDeckEntry>();

                        // Obtaing a random request of cardLvl corresponding to its position in the sequence:
                        foreach (var cardLvl in levelSequence)
                        {
                            // Filtering all cards whose cardLvl is 1:
                            var cardsFiltered = cardsDB.Where(a => a.Level == cardLvl).ToList();

                            // Obtaining one random request out of the list of filtered cards
                            var card = cardsFiltered.OrderBy(a => random.Next()).Take(1).FirstOrDefault();

                            // "Converting" the random request into a new NPC DECK ENTRY and adding it to a new list of valid deck entries:                               
                            validNpcDeckEntries.Add
                            (
                                new NpcDeckEntry
                                {
                                    Card = card
                                }
                            );
                        }

                        // Creating a new npc with 5 cards npcLvl 1 and adding it to a list of new NPCs:
                        countBotsLvlOne++;
                        var npcLvl = Helper.GetNpcLevel(validNpcDeckEntries.Select(a => a.Card.Level).ToList());
                        var npcName = "NPC-LVL" + npcLvl + "-" + countBotsLvlOne;
                        npcs.Add(new Npc
                        {
                            Name = npcName,
                            Description = "( " + string.Join(", ", levelSequence) + " )",
                            Deck = validNpcDeckEntries,
                            Level = npcLvl,
                            IsDeleted = false,
                            IsDummy = true
                        });

                    }
                    //This while loop will stop when the count is 2.                       
                    count++;
                }

                return npcs;
            }
            else if (level == 8)
            {
                var npcs = new List<Npc>();
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
                            var levelSequence = Helper.GetPowerSequence(level, i);

                            // Criating a new list of valid NPC deck entries:
                            var validNpcDeckEntries = new List<NpcDeckEntry>();

                            // Obtaing a random cards of cardLvl corresponding to its position in the sequence:
                            foreach (var cardLvl in levelSequence)
                            {
                                // Filtering all cards whose cardLvl is 8:
                                var cardsFiltered = cardsDB.Where(a => a.Level == cardLvl).ToList();

                                // Obtaining one random request out of the list of filtered cards
                                var card = cardsFiltered.OrderBy(a => random.Next()).FirstOrDefault();

                                // "Converting" the random request into a new NPC DECK ENTRY and adding it to a new list of valid deck entries:                               
                                validNpcDeckEntries.Add
                                (
                                    new NpcDeckEntry
                                    {
                                        Card = card
                                    }
                                );
                            }

                            // Creating a new npc with 5 cards npcLvl 1 and adding it to a list of new NPCs:                        
                            countBotsLvlEight++;
                            var npcLvl = Helper.GetNpcLevel(validNpcDeckEntries.Select(a => a.Card.Level).ToList());
                            var npcName = "NPC-LVL" + npcLvl + "-" + countBotsLvlEight;
                            npcs.Add(new Npc
                            {
                                Name = npcName,
                                Description = "( " + string.Join(", ", levelSequence) + " )",
                                Deck = validNpcDeckEntries,
                                Level = npcLvl,
                                IsDeleted = false,
                                IsDummy = true
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

                var validDecks = new List<List<NpcDeckEntry>>();
                var npcs = new List<Npc>();

                // Obtaining the lists of all unique sequences
                // (for cardLvl 2 up to 7 there are 12 unique possible combinations):
                for (int i = 1; i <= 12; i++)
                {
                    // Criating a new list of valid NPC deck entries:
                    var validNpcDeckEntries = new List<NpcDeckEntry>();

                    //ex.: cardLvl == 6 and  i == 1 => (4, 4, 6, 8, 8 )
                    var levelSequence = Helper.GetPowerSequence(level, i);

                    // Obtaing a random request of cardLvl corresponding to its position in the sequence:
                    foreach (var cardLvl in levelSequence)
                    {
                        // Filtering all cards by cardLvl (2, 3, 4, 5, 6 or 7):
                        var cardsFiltered = cardsDB.Where(a => a.Level == cardLvl).ToList();

                        // Obtaing a random request of cardLvl corresponding to its position in the sequence:
                        var card = cardsFiltered.OrderBy(a => random.Next()).FirstOrDefault();

                        // "Converting" the random request into a new NPC DECK ENTRY and adding it to a new list of valid deck entries:
                        validNpcDeckEntries.Add
                        (
                            new NpcDeckEntry
                            {
                                Card = card
                            }
                        );
                    }

                    // Creating a new npc with 5 cards and adding it to a list of new NPCs:   
                    var npcName = "";
                    var npcLvl = Helper.GetNpcLevel(validNpcDeckEntries.Select(a => a.Card.Level).ToList());

                    switch (level)
                    {
                        case 2:
                            countBotsLvlTwo++;
                            npcName = "NPC-LVL" + npcLvl + "-" + countBotsLvlTwo;
                            break;
                        case 3:
                            countBotsLvlThree++;
                            npcName = "NPC-LVL" + npcLvl + "-" + countBotsLvlThree;
                            break;
                        case 4:
                            countBotsLvlFour++;
                            npcName = "NPC-LVL" + npcLvl + "-" + countBotsLvlFour;
                            break;
                        case 5:
                            countBotsLvlFive++;
                            npcName = "NPC-LVL" + npcLvl + "-" + countBotsLvlFive;
                            break;
                        case 6:
                            countBotsLvlSix++;
                            npcName = "NPC-LVL" + npcLvl + "-" + countBotsLvlSix;
                            break;
                        case 7:
                            countBotsLvlSeven++;
                            npcName = "NPC-LVL" + npcLvl + "-" + countBotsLvlSeven;
                            break;
                    }

                    npcs.Add(new Npc
                    {
                        Name = npcName,
                        Description = "( " + string.Join(", ", levelSequence) + " )",
                        Deck = validNpcDeckEntries,
                        Level = npcLvl,
                        IsDeleted = false,
                        IsDummy = true
                    });

                    // By the end of this for loop, there will be 12 npcs                  
                }

                // Returns 10 random npcs out of the list of 12: 
                return npcs.OrderBy(a => random.Next()).Take(10).ToList(); ;
            }
        }

        public async Task<(DevsMedievalAutoBattlerDeleteSeedResponse?, string)> MedievalAutoBattlerDeleteSeed(DevsMedievalAutoBattlerDeleteSeedRequest request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            await this._daoDbContext
                            .Npcs
                            .Where(a => a.IsDummy == true)
                            .ExecuteDeleteAsync();

            await this._daoDbContext
                .Cards
                .Where(a => a.IsDummy == true)
                .ExecuteDeleteAsync();

            return (null, "Dummies deleted successfully");
        }

        #endregion
    }
}