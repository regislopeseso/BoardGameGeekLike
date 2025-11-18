using BoardGameGeekLike.Models;
using BoardGameGeekLike.Models.Dtos.Request;
using BoardGameGeekLike.Models.Dtos.Response;
using BoardGameGeekLike.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;

namespace BoardGameGeekLike.Services
{
    public class ExploreService
    {
        private readonly ApplicationDbContext _daoDbContext;

        public ExploreService(ApplicationDbContext daoDbContext)
        {
            this._daoDbContext = daoDbContext;
        }

        public async Task<(List<ExploreFindBoardGameResponse>?, string)> FindBoardGame(ExploreFindBoardGameRequest? request)
        {
            var (isValid, message) = FindBoardGame_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var contentQueryable = this._daoDbContext
                                       .BoardGames
                                       .Include(a => a.Category)
                                       .Include(a => a.Mechanics)
                                       .AsNoTracking()
                                       .Where(a => a.IsDeleted == false);

            if (contentQueryable == null)
            {
                return (null, "Error: no board games found");
            }

            if (request == null || string.IsNullOrWhiteSpace(request.BoardGameName) == true)
            {
                var content1 = await contentQueryable
                                .Select(a => new ExploreFindBoardGameResponse
                                {
                                    BoardGameId = a.Id,
                                    BoardGameName = a.Name
                                })
                               .OrderBy(a => a.BoardGameId)
                               .ToListAsync();

                return (content1, message);
            }

            message = "All board games listed successfully";
           
            //Filtering by BoardGameName
            if (String.IsNullOrWhiteSpace(request!.BoardGameName) == false)
            {
                contentQueryable = contentQueryable.Where(a => a.Name.ToLower().Contains(request.BoardGameName.ToLower()));
            }       
                 
            var content = await contentQueryable
                                .Select(a => new ExploreFindBoardGameResponse
                                {
                                    BoardGameId = a.Id,
                                    BoardGameName = a.Name
                                })
                               .OrderBy(a => a.BoardGameName)
                               .ToListAsync();

            if (content == null || content.Count == 0)
            {
                return (null, "Error: nothing found");
            }

            return (content, "Board Games listed found successfully");
        }
        private static (bool, string) FindBoardGame_Validation(ExploreFindBoardGameRequest? request)
        {
            if (request == null)
            {
                return (true, string.Empty);
            }          

            if (string.IsNullOrWhiteSpace(request!.BoardGameName) == false && request.BoardGameName.Length < 3)
            {
                return (false, "Error: filtering board games by name requires at least 3 characters.");
            }           

            return (true, string.Empty);
        }


        public async Task<(ExploreShowBoardGameDetailsResponse?, string)> ShowBoardGameDetails(ExploreShowBoardGameDetailsRequest? request)
        {
            var (isValid, message) = ShowBoardGameDetails_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            #region FETCHING THE REQUESTED BOARD GAME
            var boardgameDB = await this._daoDbContext
                                        .BoardGames
                                        .Include(a => a.Mechanics)
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(a => a.Id == request!.BoardGameId);

            if (boardgameDB == null)
            {
                return (null, "Error: request BoardGame not found");
            }

            if (boardgameDB.IsDeleted == true)
            {
                return (null, "Error: request BoardGame is deleted");
            }
            #endregion

            #region FETCHING THE BG CATEGORY NAME
            var categoryDB = await this._daoDbContext
                                       .Categories
                                       .FindAsync(boardgameDB.CategoryId);

            var category = String.Empty;

            if (categoryDB != null && categoryDB.IsDeleted == true)
            {
                category = "Error: the category of the requested board game has been deleted";
            }
            else
            {
                category = categoryDB.Name;
            }
            #endregion

            #region FETCHING THE BG MECHANICS NAMES
            var requestedMechanicIds = boardgameDB.Mechanics!.Select(a => a.Id).ToList();

            var mechanicsDB = await this._daoDbContext
                                        .Mechanics
                                        .Where(a => requestedMechanicIds.Contains(a.Id))
                                        .ToListAsync();

            var mechanics = mechanicsDB.Select(a => a.Name).ToList();
            #endregion

            #region FETCHING THE BG LAST 5 SESSIONS
            var loggedSessionsDB = await this._daoDbContext
                                             .Sessions
                                             .Include(a => a.User)
                                             .Where(a => a.BoardGameId == request!.BoardGameId)
                                             .ToListAsync();        

            var loggedSessionsCount = 0;

            var avgSessionDuration = 0;

            var lastFiveSessions = new List<ExploreShowBoardGameDetailsResponse_sessions>() { };

            if (loggedSessionsDB != null && loggedSessionsDB.Count > 0)
            {
                loggedSessionsCount = loggedSessionsDB.Count;

                avgSessionDuration = (int)Math.Ceiling(loggedSessionsDB.Average(a => a.Duration_minutes));

                var n = 5;

                if (loggedSessionsCount < 5)
                {
                    n = loggedSessionsCount;
                }


                loggedSessionsDB = loggedSessionsDB.OrderByDescending(a => a.Date).ToList();

                for (int i = 0; i < n; i++)
                {
                    lastFiveSessions.Add(new ExploreShowBoardGameDetailsResponse_sessions
                    {
                        SessionId = loggedSessionsDB[i].Id,
                        UserNickName = loggedSessionsDB[i].User!.UserName,
                        Date = loggedSessionsDB[i].Date,
                        PlayersCount = loggedSessionsDB[i].PlayersCount,
                        Duration = loggedSessionsDB[i].Duration_minutes
                    });
                }
            }
            #endregion

            var content = new ExploreShowBoardGameDetailsResponse
            {
                BoardGameName = boardgameDB.Name,
                BoardGameDescription = boardgameDB.Description,
                Category = category,
                Mechanics = mechanics,
                MinPlayersCount = boardgameDB.MinPlayersCount,
                MaxPlayerCount = boardgameDB.MaxPlayersCount,
                MinAge = boardgameDB.MinAge,
                LoggedSessions = loggedSessionsCount,
                AvgSessionDuration = avgSessionDuration,
                AvgRating = boardgameDB.AverageRating,
                LastFiveSessions = lastFiveSessions
            };

            return (content, "Board game details shown sucessfully");
        }
        private static (bool, string) ShowBoardGameDetails_Validation(ExploreShowBoardGameDetailsRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.BoardGameId.HasValue == false)
            {
                return (false, "Error: BoardGameId is missing");
            }

            if (request.BoardGameId < 1)
            {
                return (false, "Error: invalid BoardGameId (requested BoardGameId is zero or a negative number)");
            }

            return (true, string.Empty);
        }


        public async Task<(List<ExploreListBoardGamesResponse>?, string)> ListBoardGames(ExploreListBoardGamesRequest? request)
        {
            var (isValid, message) = ListBoardGames_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var boardGamesDB = await this._daoDbContext
                .BoardGames
                .Select(a => new { a.Name, a.AverageRating, a.RatingsCount, a.MinPlayersCount, a.MaxPlayersCount,a.AvgDuration_minutes, a.SessionsCount })               
                .OrderBy(a => a.Name)
                .ToListAsync();

            if (boardGamesDB == null || boardGamesDB.Count == 0)
            {
                return (null, "Error: no board game found");
            }

            var ratedBoardGames = new List<ExploreListBoardGamesResponse>();

            foreach(var boardGame in boardGamesDB)
            {
                var playersCount = boardGame.MinPlayersCount == boardGame.MaxPlayersCount ?
                    $"{boardGame.MinPlayersCount}" : $"{boardGame.MinPlayersCount} - {boardGame.MaxPlayersCount}";



                ratedBoardGames.Add(
                    new ExploreListBoardGamesResponse
                    {
                        BoardGameName = boardGame.Name,
                        AvgRating = boardGame.AverageRating,
                        RatingsCount = boardGame.RatingsCount,
                        PlayersCount = playersCount,
                        AvgDuration = (int)boardGame.AvgDuration_minutes,
                        SessionsLogged = boardGame.SessionsCount
                    });
            }

            return (ratedBoardGames, "Board games successfully listed by rate");

        }
        private static (bool, string) ListBoardGames_Validation(ExploreListBoardGamesRequest? request)
        {
            if(request != null)
            {
                return (false, "Error: request is not null");
            }

            return (true, string.Empty);
        }


        public async Task<(ExploreBoardGamesRankingsResponse?, string)> BoardGamesRankings(ExploreBoardGamesRankingsRequest? request)
        {
            var (isValid, message) = BoardGamesRankings_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }
           
            var boardGamesDB = await this._daoDbContext
                .BoardGames
                .Select(a => new
                {                
                    a.Name,
                    AvgRating = a.Ratings != null? (decimal)a.Ratings.Average(b => b.Rate) : 0,
                    RatingsCount = a.Ratings != null ? (int)a.Ratings.Count: 0,
                    SessionsCount = a.Sessions != null ? (int)a.Sessions.Count : 0,
                    AvgDuration = a.Sessions != null ? (int)a.Sessions.Average(b => b.Duration_minutes) : 0,
                    a.MinAge,
                })              
                .AsNoTracking()
                .AsSplitQuery()
                .ToListAsync();

            if (boardGamesDB == null || boardGamesDB.Count < 1)
            {
                return (null, "Error: no board games found for the ranking");
            }

            var theMostPlayed = boardGamesDB
                .Where(a => a.SessionsCount > 0)
                .OrderByDescending(a => a.SessionsCount)
                .ThenByDescending(a => a.AvgRating)
                .ThenByDescending(a => a.RatingsCount)
                .Select(a => new ExploreBoardGamesRankingsResponse_mostPlayed
                {
                    BoardGame_Name = a.Name,
                    BoardGame_AvgRating = Math.Round(a.AvgRating, 1),
                    BoardGame_RatingsCount = a.RatingsCount,
                    BoardGame_SessionsCount = a.SessionsCount,
                    BoardGame_AvgDuration = a.AvgDuration,
                    BoarGame_MinAge = a.MinAge,
                })
                .Take(3)
                .ToList();

            var theBestRated = boardGamesDB
                .Where(a => a.SessionsCount > 0)
                .OrderByDescending(a => a.AvgRating)
                .ThenByDescending(a => a.SessionsCount)
                .Select(a => new ExploreBoardGamesRankingsResponse_bestRated
                {
                    BoardGame_Name = a.Name,
                    BoardGame_AvgRating = Math.Round(a.AvgRating, 1),
                    BoardGame_RatingsCount = a.RatingsCount,
                    BoardGame_SessionsCount = a.SessionsCount,
                    BoardGame_AvgDuration = a.AvgDuration,
                    BoarGame_MinAge = a.MinAge,
                })
                .Take(3)
                .ToList();

            var adultsFavorites = boardGamesDB
                .Where(a => a.SessionsCount > 0 && a.MinAge >= 18)
                .OrderByDescending(a => a.AvgRating)
                .ThenByDescending(a => a.RatingsCount)
                .ThenByDescending(a => a.SessionsCount)
                .Select(a => new ExploreBoardGamesRankingsResponse_adultsFavorites
                {
                    BoardGame_Name = a.Name,
                    BoardGame_AvgRating = Math.Round(a.AvgRating, 1),
                    BoardGame_RatingsCount = a.RatingsCount,
                    BoardGame_SessionsCount = a.SessionsCount,
                    BoardGame_AvgDuration = a.AvgDuration,
                    BoarGame_MinAge = a.MinAge,
                })
                .Take(3)
                .ToList();

            var teensFavorites = boardGamesDB
                .Where(a => a.SessionsCount > 0 && a.MinAge < 18)
                .OrderByDescending(a => a.AvgRating)           
                .ThenByDescending(a => a.SessionsCount)
                .Select(a => new ExploreBoardGamesRankingsResponse_teensFavorites
                {
                    BoardGame_Name = a.Name,
                    BoardGame_AvgRating = Math.Round(a.AvgRating, 1),
                    BoardGame_RatingsCount = a.RatingsCount,
                    BoardGame_SessionsCount = a.SessionsCount,
                    BoardGame_AvgDuration = a.AvgDuration,
                    BoarGame_MinAge = a.MinAge,
                })
                .Take(3)
                .ToList();


            var theShortest = boardGamesDB
              .Where(a => a.SessionsCount > 0)
              .OrderBy(a => a.AvgDuration)
              .ThenByDescending(a => a.AvgRating)
              .ThenByDescending(a => a.SessionsCount)
              .Select(a => new ExploreBoardGamesRankingsResponse_theShortest
              {
                  BoardGame_Name = a.Name,
                  BoardGame_AvgRating = Math.Round(a.AvgRating, 1),
                  BoardGame_RatingsCount = a.RatingsCount,
                  BoardGame_SessionsCount = a.SessionsCount,
                  BoardGame_AvgDuration = a.AvgDuration,
                  BoarGame_MinAge = a.MinAge,
              })              
              .Take(3)
              .ToList();

            var theLongest = boardGamesDB
                .Where(a => a.SessionsCount > 0)
                .OrderByDescending(a => a.AvgDuration)
                .ThenByDescending(a => a.AvgRating)
                .ThenByDescending(a => a.SessionsCount)
                .Select(a => new ExploreBoardGamesRankingsResponse_theLongest
                {
                    BoardGame_Name = a.Name,
                    BoardGame_AvgRating = Math.Round(a.AvgRating, 1),
                    BoardGame_RatingsCount = a.RatingsCount,
                    BoardGame_SessionsCount = a.SessionsCount,
                    BoardGame_AvgDuration = a.AvgDuration,
                    BoarGame_MinAge = a.MinAge,
                })
                .Take(3)
                .ToList();   

            var content = new ExploreBoardGamesRankingsResponse
            {
                MostPlayedBoardGames = theMostPlayed,
                BestRatedBoardGames = theBestRated,
                ShortestBoardGames = theShortest,
                LongestBoardGames = theLongest,
                AdultsFavoriteBoardGames = adultsFavorites,
                TeensFavoriteBoardGames = teensFavorites
            };

            if (content == null)
            {
                return (null, "Error: ranking failed");
            }

            return (content, "Board games ranked successfully");
        }
        private static (bool, string) BoardGamesRankings_Validation(ExploreBoardGamesRankingsRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: no parameter should be passed for the ranking of the board games");
            }

            return (true, String.Empty);
        }


        public async Task<(ExploreCategoriesRankingResponse?, string)> CategoriesRankings(ExploreCategoriesRankingsRequest? request)
        {
            var (isValid, message) = CategoriesRankings_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var boardgamesDB_exist = await this._daoDbContext
                                               .BoardGames
                                               .AsNoTracking()
                                               .AnyAsync(a => a.IsDeleted == false);
            if (boardgamesDB_exist == false)
            {
                return (null, "Error: no board game found for the categories ranking");
            }

            var categorizedBoardgames_exist = await this._daoDbContext
                                             .Categories
                                             .AsNoTracking()
                                             .AnyAsync(a => a.BoardGames!.Any());

            if (categorizedBoardgames_exist == false)
            {
                return (null, "Error: no board game having a category found");
            }

            var sessionsDB_exist = await this._daoDbContext
                                             .Sessions
                                             .AsNoTracking()
                                             .AnyAsync();

            if (sessionsDB_exist == false)
            {
                return (null, "Error: board games session found for the categories ranking");
            }

            var categoriesDB = await this._daoDbContext
                .Categories
                .Include(a => a.BoardGames!)
                .ThenInclude(a => a.Sessions!)
                .AsNoTracking()
                .ToListAsync();

            if (categoriesDB == null)
            {
                return (null, "Error: no categories found for the ranking");
            }

            var theMostPlayed = categoriesDB.Select(a => new ExploreCategoriesRankingResponse_mostPlayedOnes
                {
                    CategoryName = a.Name,
                    SessionsCount = a.BoardGames!.Sum(b => b.Sessions!.Count)
                })
                .OrderByDescending(a => a.SessionsCount)
                .Take(3)
                .ToList();

            var theMostPopular = categoriesDB.Select(a => new ExploreCategoriesRankingsResponse_mostPopularOnes
                {
                    CategoryName = a.Name,
                    BoardGamesCount = a.BoardGames!.Count,
                    SessionsCount = a.BoardGames.Select(b => b.SessionsCount).Sum()
                })
                .OrderByDescending(a => a.BoardGamesCount)
                .ThenByDescending(a => a.SessionsCount)
                .Take(3)
                .ToList();

            var theBestRated = categoriesDB.Select(a => new ExploreCategoriesRankingResponse_bestRatedOnes
            {
                CategoryName = a.Name,
                AvgRating = a.BoardGames!.Count != 0 ?
                    Math.Round(a.BoardGames!.Average(b => b.AverageRating),2) : 0,
                RatingsCount = a.BoardGames.Select(b => b.RatingsCount).Sum()
            })
                .OrderByDescending(a => a.AvgRating)
                .ThenByDescending(a => a.RatingsCount)
                .Take(3)
                .ToList();

            var theLongest = categoriesDB.Select(a => new ExploreCategoriesRankingResponse_longestOnes
                {
                    CategoryName = a.Name,
                    Duration = a.BoardGames!.SelectMany(b => b.Sessions!).Any() == true ?
                        (int)a.BoardGames!.SelectMany(b => b.Sessions!).Max(c => c.Duration_minutes) : 0,
                    SessionsCount = a.BoardGames!.Select(a => a.SessionsCount).Sum()
                })
                .OrderByDescending(a => a.Duration)
                .ThenByDescending(a => a.SessionsCount)
                .Take(3)
                .ToList();

            var theShortest = categoriesDB.Select(a => new ExploreCategoriesRankingResponse_shortestOnes
            {
                CategoryName = a.Name,
                Duration = a.BoardGames!.SelectMany(b => b.Sessions!).Any() == true ?
                        (int)a.BoardGames!.SelectMany(b => b.Sessions!).Min(c => c.Duration_minutes) : 0,
                SessionsCount = a.BoardGames!.Select(a => a.SessionsCount).Sum()
            })
                .Where(a => a.Duration > 0)
                .OrderBy(a => a.Duration)
                .ThenByDescending(a => a.SessionsCount)
                .Take(3)
                .ToList();

            var content = new ExploreCategoriesRankingResponse
            {
                MostPlayedCategories = theMostPlayed,
                MostPopularCategories = theMostPopular,
                BestRatedCategories = theBestRated,
                LongestCategories = theLongest,
                ShortestCategories = theShortest
            };

            return (content, "Categories ranking successfully");
        }
        private static (bool, string) CategoriesRankings_Validation(ExploreCategoriesRankingsRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: no parameter should be passed for the ranking of the board games");
            }

            return (true, String.Empty);
        }


        public async Task<(ExploreQuickStartLifeCounterResponse?, string)> QuickStartLifeCounter(ExploreQuickStartLifeCounterRequest? request)
        {
            var (isValid, message) = QuickStartLifeCounter_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var expandableLifeCounterManager = await this._daoDbContext
                .LifeCounterManagers
                .Where(a => a.LifeCounterTemplate == null && a.UserId == null && a.Duration_minutes >= 2880)               
                .ToListAsync();

            var newExpandableLifeCounterManager = new LifeCounterManager
            {
                LifeCounterManagerName = "Life Counter",
                PlayersCount = 1,
                PlayersStartingLifePoints = 10,
                FixedMaxLifePointsMode = false,
                PlayersMaxLifePoints = null,
                AutoDefeatMode = false,
                AutoEndMode = false,
                StartingTime = null,
                EndingTime = null,
                Duration_minutes = null,
                LifeCounterPlayers = new List<LifeCounterPlayer>(),
                UserId = null,
                LifeCounterTemplateId = null
            };

            var players = new List<LifeCounterPlayer>()
            {
                new LifeCounterPlayer
                {


                    PlayerName = "Player 1",
                    StartingLifePoints = 10,
                    CurrentLifePoints = 10,
                    FixedMaxLifePointsMode = false,
                    MaxLifePoints = null,
                    LifeCounterManagerId = 1,
                    AutoDefeatMode = false,
                    IsDefeated  = false
                }
            };

            newExpandableLifeCounterManager.LifeCounterPlayers = players;

            this._daoDbContext.LifeCounterManagers.RemoveRange(expandableLifeCounterManager);
            this._daoDbContext.LifeCounterManagers.Add(newExpandableLifeCounterManager);

            await this._daoDbContext.SaveChangesAsync();

            return (null, "");
        }
        private static (bool, string) QuickStartLifeCounter_Validation(ExploreQuickStartLifeCounterRequest? request)
        {
            if (request != null)
            {
                return (false, $"Error: request is not null however it must be null");
            }

            return (true, string.Empty);
        }
    }
}
