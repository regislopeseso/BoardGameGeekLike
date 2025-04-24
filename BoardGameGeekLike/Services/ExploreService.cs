using BoardGameGeekLike.Models;
using BoardGameGeekLike.Models.Dtos.Request;
using BoardGameGeekLike.Models.Dtos.Response;
using Microsoft.EntityFrameworkCore;

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

            if (request == null ||
               (
               request.BoardGameId.HasValue == false &&
                string.IsNullOrWhiteSpace(request.BoardGameName) == true &&
                request.MinPlayersCount.HasValue == false &&
                request.MaxPlayersCount.HasValue == false &&
                request.MinAge.HasValue == false &&
                string.IsNullOrWhiteSpace(request.CategoryName) == true &&
                string.IsNullOrWhiteSpace(request.MechanicName) == true &&
                request.AverageRating.HasValue == false
               ))
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

            //Filtering by BoardGameId
            if (request!.BoardGameId.HasValue == true)
            {
                contentQueryable = contentQueryable.Where(a => a.Id == request.BoardGameId);
            }

            //Filtering by BoardGameName
            if (String.IsNullOrWhiteSpace(request!.BoardGameName) == false)
            {
                contentQueryable = contentQueryable.Where(a => a.Name.ToLower().Contains(request.BoardGameName.ToLower()));
            }

            //Filtering by MinPlayersCount
            if (request.MinPlayersCount.HasValue == true)
            {
                contentQueryable = contentQueryable.Where(a => a.MinPlayersCount == request.MinPlayersCount);
            }

            //Filtering by MaxPlayersCount
            if (request.MaxPlayersCount.HasValue == true)
            {
                contentQueryable = contentQueryable.Where(a => a.MaxPlayersCount == request.MaxPlayersCount);
            }

            //Filtering by MinAge
            if (request.MinAge.HasValue == true)
            {
                contentQueryable = contentQueryable.Where(a => a.MinAge >= request.MinAge);
            }

            //Filtering by Category
            if (String.IsNullOrWhiteSpace(request.CategoryName) == false)
            {
                contentQueryable = contentQueryable.Where(a => a.Category!.Name.ToLower().Contains(request.CategoryName.ToLower()));
            }

            //Filtering by Mechanic
            if (String.IsNullOrWhiteSpace(request.MechanicName) == false)
            {
                contentQueryable = contentQueryable.Where(a => a.Mechanics!
                                                                .Select(a => a.Name.ToLower())
                                                                .Contains(request.MechanicName.ToLower()));
            }

            //Filtering by Rating
            if (request.AverageRating.HasValue == true)
            {
                contentQueryable = contentQueryable.Where(a => a.AverageRating == request.AverageRating);
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

            if(request.BoardGameId.HasValue == true && request.BoardGameId < 1)
            {
                return (false, "Error: invalid BoardGameId (requested Id is a negative number)");
            }

            if (string.IsNullOrWhiteSpace(request!.BoardGameName) == false && request.BoardGameName.Length < 3)
            {
                return (false, "Error: filtering board games by name requires at least 3 characters.");
            }

            if (request.MinPlayersCount.HasValue == true && request.MinPlayersCount < 1)
            {
                return (false, "Error: invalid MinPlayersCount (requested MinPlayersCount is zero or a negative number).");
            }

            if (request.MaxPlayersCount.HasValue == true && request.MaxPlayersCount < 1)
            {
                return (false, "Error: invalid MaxPlayersCount (requested MaxPlayersCount is zero or a negative number).");
            }

            if (request.MinAge.HasValue == true && request.MinAge < 1)
            {
                return (false, "Error: invalid MinAge (requested Age is zero or a negative number).");
            }

            if ((request.MinPlayersCount.HasValue == true && request.MaxPlayersCount.HasValue == true) &&
                 request.MaxPlayersCount < request.MinPlayersCount)
            {
                return (false, "Error: invalid PlayersCount (MaxPlayersCount greater than MinPlayersCount).");
            }

            if (String.IsNullOrWhiteSpace(request.CategoryName) == false && request.CategoryName.Length < 3)
            {
                return (false, "Error: filtering board games by category requires at least 3 characters.");
            }

            if (String.IsNullOrWhiteSpace(request.MechanicName) == false && request.MechanicName.Length < 3)
            {
                return (false, "Error: filtering board games by mechanic requires at least 3 characters.");
            }

            if (request.AverageRating.HasValue == true && request.AverageRating < 0)
            {
                return (false, "Error: invalid Rate (requested Rate is a negative number).");
            }

            if (request.AverageRating.HasValue == true && request.AverageRating > 5)
            {
                return (false, "Error: invalid Rate (is greater than 5).");
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
                .Select(a => new { a.Name, a.AverageRating, a.RatingsCount, a.MinPlayersCount, a.MaxPlayersCount, a.SessionsCount })               
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
                        AvgDuration = 0,
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

            #region SESSIONS QUERIES
            var sessionsDB = await this._daoDbContext
                .Sessions
                .AsNoTracking()
                .Include(a => a.BoardGame)
                .ToListAsync();

            if (sessionsDB == null || sessionsDB.Count < 1)
            {
                return (null, "Error: no board games sessions found for the rankings");
            }

            var theMostPlayed = sessionsDB.GroupBy(a => a.BoardGameId)
                .OrderByDescending(a => a.Count())
                .Take(3)
                .Select(a => a.First().BoardGame!.Name)
                .ToList();         

            var theShortest = sessionsDB.GroupBy(a => a.Duration_minutes)
                .OrderBy(a => a.Key)
                .SelectMany(a => a.GroupBy(b => b.BoardGameId).OrderByDescending(b => b.Count()).ToList())                
                .Take(3)
                .Select(a => a.First().BoardGame!.Name)
                .ToList();
        

            var theLongest = sessionsDB.GroupBy(a => a.Duration_minutes)
                .OrderByDescending(a => a.Key)
                .SelectMany(a => a.GroupBy(b => b.BoardGameId).OrderByDescending(b => b.Count()).ToList())
                .Take(3)
                .Select(a => a.First().BoardGame!.Name)
                .ToList();

            var adultsFavorites = sessionsDB.Where(a => a.BoardGame!.MinAge >= 18)
                .GroupBy(a => a.BoardGameId)
                .OrderByDescending(a => a.Count())
                .Take(3)
                .Select(a => a.First().BoardGame!.Name)
                .ToList();

            var teensFavorites = sessionsDB.Where(a => a.BoardGame!.MinAge < 18)
                .GroupBy(a => a.BoardGameId)
                .OrderByDescending(a => a.Count())
                .Take(3)
                .Select(a => a.First().BoardGame!.Name)
                .ToList();
            #endregion

            #region BOARD GAMES QUERIES
            var boardGamesDB = await this._daoDbContext
                                         .BoardGames
                                         .AsNoTracking()
                                         .ToListAsync();

            if (boardGamesDB == null || boardGamesDB.Count < 1)
            {
                return (null, "Error: no board games found for the ranking");
            }

            var theBestRated = boardGamesDB.OrderByDescending(a => a.AverageRating)
                                           .Take(3)
                                           .Select(a => a.Name)
                                           .ToList();
            #endregion

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

        public async Task<(ExploreCategoriesRankingResponse?, string)> CategoriesRanking(ExploreCategoriesRankingRequest? request)
        {
            var (isValid, message) = CategoriesRanking_Validation(request);

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

            var theMostPopular = categoriesDB.Select(a => new ExploreCategoriesRankingResponse_mostPopularOnes
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

        private static (bool, string) CategoriesRanking_Validation(ExploreCategoriesRankingRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: no parameter should be passed for the ranking of the board games");
            }

            return (true, String.Empty);
        }
    }
}
