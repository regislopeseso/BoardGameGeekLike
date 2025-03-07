using System.Text.RegularExpressions;
using BoardGameGeekLike.Models;
using BoardGameGeekLike.Models.Dtos.Request;
using BoardGameGeekLike.Models.Dtos.Response;
using BoardGameGeekLike.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BoardGameGeekLike.Services
{
    public class UsersService
    {
        private readonly ApplicationDbContext _daoDbContext;

        public UsersService(ApplicationDbContext daoDbContext)
        {
            this._daoDbContext = daoDbContext;
        }

        public async Task<(UsersSignUpResponse?, string)> SignUp(UsersSignUpRequest? request)
        {
            var (isValid, message) = SignUp_Validation(request);
            
            if (isValid == false)
            {
                return (null, message);
            }     

            var userNickName_exists = await this._daoDbContext
                                        .Users
                                        .AsNoTracking()
                                        .AnyAsync(a => a.Nickname == request!.UserNickname && a.IsDeleted == false);

            if(userNickName_exists == true)
            {
                return (null, "Error: requested UserNickName is already in use");
            }

            var parsedDate = DateOnly.ParseExact(request!.UserBirthDate!, "dd/MM/yyyy");   

            var user = new User
            {
                Nickname = request.UserNickname!,
                Email = request.UserEmail!,
                BirthDate = parsedDate,
            };

            await this._daoDbContext.Users.AddAsync(user);

            await this._daoDbContext.SaveChangesAsync();

            return (new UsersSignUpResponse(), "User signed up successfully");
        }
        
        private static (bool, string) SignUp_Validation(UsersSignUpRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (string.IsNullOrWhiteSpace(request.UserNickname) == true)
            {
                return (false, "Error: username is null or empty");
            }

            if (string.IsNullOrWhiteSpace(request.UserEmail)== true)
            {
                return (false, "Error: UserEmail is missing");
            }

            string emailPattern = @"(^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}(?:\.[a-zA-Z]{2,10})?$)";

            if (Regex.IsMatch(request.UserEmail, emailPattern) == false)
            {
                return (false, "Error: invalid email format");
            }

            if (string.IsNullOrWhiteSpace(request.UserBirthDate) == true)
            {
                return (false, "Error: UserBirthDate is missing");
            }

            string birthDatePattern = @"^(0[1-9]|[12][0-9]|3[01])/(0[1-9]|1[0-2])/[0-9]{4}$";

            if (Regex.IsMatch(request.UserBirthDate, birthDatePattern) == false)
            {
                return (false, "Error: invalid birth date format. Expected format: DD/MM/YYYY");
            }

            // Convert string to DateOnly
            if (DateOnly.TryParseExact(request.UserBirthDate, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateOnly parsedDate) == false)
            {
                return (false, "Error: invalid birth date");
            }

            int age = DateTime.Now.Year - parsedDate.Year;

            if (parsedDate.Month > DateTime.Now.Month || 
                    (parsedDate.Month == DateTime.Now.Month && parsedDate.Day > DateTime.Now.Day))
            {
                age--;
            }

            if (age < 12)
            {
                return (false, "Error: the minimum age for signing up is 12");
            }

            if (age > 90)
            {
                return (false, "Error: birth date is too old");
            }

            return (true, String.Empty);
        }

        public async Task<(UsersEditProfileResponse?, string)> EditProfile(UsersEditProfileRequest? request)
        {
            var (isValid, message) = EditProfile_Validation(request);
            
            if (isValid == false)
            {
                return (null, message);
            } 

            var userNickName_exists = await this._daoDbContext
                                                .Users
                                                .AsNoTracking()
                                                .AnyAsync(a => a.Id != request!.UserId &&
                                                               a.Nickname == request!.UserNickname &&
                                                               a.IsDeleted == false);

            if(userNickName_exists == true)
            {
                return (null, "Error: requested UserNickName is already in use");
            }

            var userDB = await this._daoDbContext
                                   .Users
                                   .FindAsync(request!.UserId);

            if(userDB == null)
            {
                return (null, "Error: user not found");
            }

            if(userDB.IsDeleted == true)
            {
                return (null, "Error: user is deleted");
            }

            var parsedDate = DateOnly.ParseExact(request.UserBirthDate!, "dd/MM/yyyy");           

            await this._daoDbContext
                      .Users
                      .Where(a => a.Id == request.UserId)
                      .ExecuteUpdateAsync(a => a.SetProperty(b => b.Nickname, request.UserNickname)
                                                .SetProperty(b => b.Email, request.UserEmail)
                                                .SetProperty(b => b.BirthDate, parsedDate));

            return (null, "User's profile edited successfully");
        }

        private static (bool, string) EditProfile_Validation(UsersEditProfileRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if(request.UserId.HasValue == false)
            {
                return (false, "Error: UserId is missing");
            }

            if (string.IsNullOrWhiteSpace(request.UserNickname) == true)
            {
                return (false, "Error: UserNickName is missing");
            }

            if (string.IsNullOrWhiteSpace(request.UserEmail) == true)
            {
                return (false, "Error: UserEmail is missing");
            }

            string emailPattern = @"(^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}(?:\.[a-zA-Z]{2,10})?$)";

            if (Regex.IsMatch(request.UserEmail, emailPattern) == false)
            {
                return (false, "Error: invalid email format");
            }

            if (string.IsNullOrWhiteSpace(request.UserBirthDate) == true)
            {
                return (false, "Error: UserBirthDate is missing");
            }

            string birthDatePattern = @"^(0[1-9]|[12][0-9]|3[01])/(0[1-9]|1[0-2])/[0-9]{4}$";

            if (Regex.IsMatch(request.UserBirthDate, birthDatePattern) == false)
            {
                return (false, "Error: invalid birth date format. Expected format: DD/MM/YYYY");
            }

            // Convert string to DateOnly
            if (DateOnly.TryParseExact(request.UserBirthDate, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateOnly parsedDate) == false)
            {
                return (false, "Error: invalid birth date");
            }

            int age = DateTime.Now.Year - parsedDate.Year;

            if (parsedDate.Month > DateTime.Now.Month || 
                    (parsedDate.Month == DateTime.Now.Month && parsedDate.Day > DateTime.Now.Day))
            {
                age--;
            }

            if (age < 12)
            {
                return (false, "Error: the minimum age for signing up is 12");
            }

            if (age > 90)
            {
                return (false, "Error: birth date is too old");
            }

            return(true, string.Empty);
        }

        public async Task<(UsersDeleteProfileResponse?, string)> DeleteProfile(UsersDeleteProfileRequest? request)
        {
            var (isValid, message) = DeleteProfile_Validation(request);
            
            if (isValid == false)
            {
                return (null, message);
            }

            var userDB = await this._daoDbContext
                                   .Users
                                   .FindAsync(request!.UserId);

            if(userDB == null)
            {
                return (null, "Error: user not found");
            }

            if(userDB.IsDeleted == true)
            {
                return (null, "Error: this user's profile was already deleted");
            }

            await this._daoDbContext
                      .Users
                      .Where(a => a.Id == request.UserId)
                      .ExecuteUpdateAsync(a => a.SetProperty(b => b.IsDeleted, true));            

            return (null, "User's profile deleted successfully");
        }

        private static (bool, string) DeleteProfile_Validation(UsersDeleteProfileRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if(request.UserId.HasValue == false)
            {
                return (false, "Error: UserId is missing");
            }

            if (request.UserId < 1)
            {
                return (false, "Error: invalid CategoryId (is less than 1)");
            }

            return (true, String.Empty);
        }

        public async Task<(UsersRateResponse?, string)> Rate(UsersRateRequest? request)
        {
            var (isValid, message) = Rate_Validation(request);
            
            if (isValid == false)
            {
                return (null, message);
            }

            var user_exists = await this._daoDbContext
                                        .Users
                                        .AsNoTracking()
                                        .AnyAsync(a => a.Id == request!.UserId && a.IsDeleted == false);

            if(user_exists == false)
            {
                return (null, "Error: user not found");
            }

            var boardgameDB = await this._daoDbContext
                                        .BoardGames
                                        .FindAsync(request!.BoardGameId);
            
            if(boardgameDB == null)
            {
                return (null, "Error: board game not found");
            }

            if(boardgameDB.IsDeleted == true)
            {
                return (null, "Error: board game is deleted");
            }

            var rate_exists = await this._daoDbContext
                                        .Ratings
                                        .AsNoTracking()
                                        .AnyAsync(a => a.UserId == request.UserId && a.BoardGameId == request.BoardGameId);

            if(rate_exists == true)
            {
                return (null, "Error: the request board game was already rated by this user");
            }

            var newRate = new Rating
            {
                Rate = request.Rate!.Value,
                UserId = request.UserId!.Value,
                BoardGameId = request.BoardGameId!.Value
            };

            await this._daoDbContext.Ratings.AddAsync(newRate);

            var newAverageRating = (int)Math.Ceiling((double)
            (
                (boardgameDB.AverageRating + newRate.Rate) / (boardgameDB.RatingsCount + 1)
            ));         
            
            await this._daoDbContext
                      .BoardGames
                      .Where(a => a.Id == request.BoardGameId)
                      .ExecuteUpdateAsync(a => a.SetProperty(b => b.AverageRating, newAverageRating)
                                                .SetProperty(b => b.RatingsCount, b => b.RatingsCount + 1));

            await this._daoDbContext.SaveChangesAsync();         

            return (null, $"Board game rated successfully, its new average rating is: {newAverageRating}");
        }

        private static (bool, string) Rate_Validation(UsersRateRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if(request.Rate.HasValue == false)
            {
                 return (false, "Error: Rate is missing");
            }

            if(request.Rate.HasValue == true && (request.Rate < 0 || request.Rate > 5) == true)
            {
                 return (false, "Error: invalid rate. It must be a value between 0 and 5");
            }

            if(request.UserId.HasValue == false)
            {
                return (false, "Error: UserId is missing");
            }
            
            if (request.UserId < 1)
            {
                return (false, "Error: invalid UserId (is less than 1)");
            }

             if(request.BoardGameId.HasValue == false)
            {
                return (false, "Error: BoardGameId is missing");
            }

            if (request.BoardGameId < 1)
            {
                return (false, "Error: invalid BoardGameId (is less than 1)");
            }

            return (true, String.Empty);
        }
   
        public async Task<(UsersLogSessionResponse?, string)> LogSession(UsersLogSessionRequest? request)
        {
            var (isValid, message) = LogSession_Validation(request);
            
            if (isValid == false)
            {
                return (null, message);
            }

            var user_exists = await this._daoDbContext
                                        .Users
                                        .AsNoTracking()
                                        .AnyAsync(a => a.Id == request!.UserId && a.IsDeleted == false);

            if(user_exists == false)
            {
                return (null, "Error: user not found");
            }

            var boardgameDB = await this._daoDbContext
                                             .BoardGames
                                             .FindAsync(request!.BoardGameId);
            
            if(boardgameDB == null)
            {
                return (null, "Error: board game not found");
            }

            if(boardgameDB.IsDeleted == true)
            {
                return (null, "Error: board game is deleted");
            }          

            var newSession = new Session
            {
                UserId = request.UserId!.Value,
                BoardGameId = request.BoardGameId!.Value,
                PlayersCount = request.PlayersCount!.Value,
                Duration_minutes = request.Duration_minutes!.Value
            };

            if(request.Date != null)
            {
                newSession.Date = DateOnly.ParseExact(request.Date!, "dd/MM/yyyy");
            }

            await this._daoDbContext.Sessions.AddAsync(newSession);

            await this._daoDbContext.SaveChangesAsync();

            return (null, "Session logged successfully");
        }

        private static (bool, string) LogSession_Validation(UsersLogSessionRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if(request.UserId.HasValue == false)
            {
                return(false, "Error: UserId is missing");
            }

            if (request.UserId < 1)
            {
                return (false, "Error: invalid CategoryId (is less than 1)");
            }

            if(request.BoardGameId.HasValue == false)
            {
                return(false, "Error: BoardGameId is missing");
            }

            if (request.BoardGameId < 1)
            {
                return (false, "Error: invalid BoarGameId (is less than 1)");
            }

            if(String.IsNullOrWhiteSpace(request.Date) == false)
            {
                string datePattern = @"^(0[1-9]|[12][0-9]|3[01])/(0[1-9]|1[0-2])/[0-9]{4}$";

                if (Regex.IsMatch(request.Date, datePattern) == false)
                {
                    return (false, "Error: invalid date format. Expected format: DD/MM/YYYY");
                }

                // Convert string to DateOnly
                if (DateOnly.TryParseExact(request.Date, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateOnly parsedDate) == false)
                {
                    return (false, "Error: invalid birth date");
                }

                var thisYear = DateTime.Now.Year;
                
                var requestedYear = parsedDate.Year;
                
                if(thisYear - requestedYear < 0)
                {
                    return (false, "Error: invalid date. Requested date is a future date");
                }
            }

            if(request.PlayersCount.HasValue == false)
            {
                return (false, "Error: PlayersCount is missing");
            }

            if(request.PlayersCount < 1)
            {
                return (false, "Error: invalid PlayersCount (is less than 1)");
            }

            if(request.Duration_minutes.HasValue == false)
            {
                return (false, "Error: Daration_minutes is missing");
            }

            if(request.Duration_minutes < 0)
            {
                return (false, "Error: invalid Duration_minutes (is negative)");
            }

            if(request.Duration_minutes > 1440)
            {
                return (false, "Error: invalid Duration_minutes (the maximum duration allowed is 1440 minutes)");
            }

            return (true, String.Empty);           
        }

        public async Task<(UsersEditSessionResponse?, string)> EditSession(UsersEditSessionRequest? request)
        {
            var (isValid, message) = EditSession_Validation(request);
            
            if (isValid == false)
            {
                return (null, message);
            }

            var boardgameDB = await this._daoDbContext
                                             .BoardGames
                                             .FindAsync(request!.BoardGameId);
            
            if(boardgameDB == null)
            {
                return (null, "Error: board game not found");
            }

            if(boardgameDB.IsDeleted == true)
            {
                return (null, "Error: board game is deleted");
            }

            var sessionDB = await this._daoDbContext
                                      .Sessions
                                      .FindAsync(request.SessionId);
            
            if(sessionDB == null)
            {
                return (null, "Error: board game session not found");
            }

            if(sessionDB.IsDeleted == true)
            {
                return (null, "Error: this session is deleted");
            }

            sessionDB.BoardGameId = request.BoardGameId!.Value;
            sessionDB.PlayersCount = request.PlayersCount!.Value;
            sessionDB.Duration_minutes = request.Duration_minutes!.Value;

            if(request.Date != null)
            {
                sessionDB.Date = DateOnly.ParseExact(request.Date!, "dd/MM/yyyy");
            }

            await this._daoDbContext.SaveChangesAsync();

            return (null, "Session edited successfully");
        }

        private static (bool, string) EditSession_Validation(UsersEditSessionRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if(request.SessionId.HasValue == false)
            {
                return (false, "Error: SessionId is missing");
            }

            if(request.SessionId < 1)
            {
                return (false, "Error: invalid SessionId (is less than 1)");
            }

            if(request.BoardGameId.HasValue == false)
            {
                return(false, "Error: BoardGameId is missing");
            }

            if (request.BoardGameId < 1)
            {
                return (false, "Error: invalid BoarGameId (is less than 1)");
            }

            if(String.IsNullOrWhiteSpace(request.Date) == false)
            {
                string datePattern = @"^(0[1-9]|[12][0-9]|3[01])/(0[1-9]|1[0-2])/[0-9]{4}$";

                if (Regex.IsMatch(request.Date, datePattern) == false)
                {
                    return (false, "Error: invalid date format. Expected format: DD/MM/YYYY");
                }

                // Convert string to DateOnly
                if (DateOnly.TryParseExact(request.Date, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateOnly parsedDate) == false)
                {
                    return (false, "Error: invalid birth date");
                }

                var thisYear = DateTime.Now.Year;
                
                var requestedYear = parsedDate.Year;
                
                if(thisYear - requestedYear < 0)
                {
                    return (false, "Error: invalid date. Requested date is a future date");
                }
            }

            if(request.PlayersCount.HasValue == false)
            {
                return (false, "Error: PlayersCount is missing");
            }

            if(request.PlayersCount < 1)
            {
                return (false, "Error: invalid PlayersCount (is less than 1)");
            }

            if(request.Duration_minutes.HasValue == false)
            {
                return (false, "Error: Daration_minutes is missing");
            }

            if(request.Duration_minutes < 0)
            {
                return (false, "Error: invalid Duration_minutes (is negative)");
            }

            if(request.Duration_minutes > 1440)
            {
                return (false, "Error: invalid Duration_minutes (the maximum duration allowed is 1440 minutes)");
            }

            return (true, String.Empty);
        }
    
        public async Task<(UsersDeleteSessionResponse?, string)> DeleteSession(UsersDeleteSessionRequest? request)
        {
            var (isValid, message) = DeleteSession_Validation(request);

            if(isValid == false)
            {
                return (null, message);
            }

            var sessionDB = await this._daoDbContext
                                      .Sessions
                                      .FindAsync(request!.SessionId);
            
            if(sessionDB == null)
            {
                return (null, "Error: session not found");
            }

            if(sessionDB.IsDeleted == true)
            {
                return (null, "Error: session was already deleted");
            }

            await this._daoDbContext
                      .Sessions
                      .Where(a => a.Id == request.SessionId)
                      .ExecuteUpdateAsync(a => a.SetProperty(b => b.IsDeleted, true));

            return (null, "Session deleted successfully");
        }

        private static (bool, string) DeleteSession_Validation(UsersDeleteSessionRequest? request)
        {
            if(request == null)
            {
                return (false, "Error: request is null");
            }

            if(request.SessionId.HasValue == false)
            {
                return (false, "Error: SessionId is missing");   
            }

            if(request.SessionId < 1)
            {
                return (false, "Error: invalid SessionId (is less than 1)");
            }

            return (true, String.Empty);
        }
    
        public async Task<(List<UsersFindBoardGameResponse>?, string)> FindBoardGame(UsersFindBoardGameRequest? request)
        {
            var (isValid, message) = FindBoardGame_Validation(request);

            if(isValid == false)
            {
                return (null, message);
            }
            
            var contentQueriable = this._daoDbContext
                                       .BoardGames
                                       .Include(a => a.Category)
                                       .Include(a => a.Mechanics)
                                       .AsNoTracking()
                                       .Where(a => a.IsDeleted == false);

            if(contentQueriable == null)
            {
                return (null, "Error: no board games found");
            }

            if(request == null ||
               (
                string.IsNullOrWhiteSpace(request.BoardGameName) == true &&
                request.MinPlayersCount.HasValue == false &&
                request.MaxPlayersCount.HasValue == false &&
                request.MinAge.HasValue == false &&
                string.IsNullOrWhiteSpace(request.CategoryName) == true &&
                string.IsNullOrWhiteSpace(request.MechanicName) == true &&
                request.AverageRating.HasValue == false
               ))
            {
                var content1 = await contentQueriable
                                .Select(a => new UsersFindBoardGameResponse
                                {
                                    BoardGameId = a.Id,
                                    BoarGameName = a.Name
                                })
                               .OrderBy(a => a.BoardGameId)
                               .ToListAsync();
                
                return (content1, message);
            }

            message = "All board games listed successfully";

            //Filtering by name
            if(String.IsNullOrWhiteSpace(request!.BoardGameName) == false)
            { 
                contentQueriable = contentQueriable.Where(a => a.Name.ToLower().Contains(request.BoardGameName.ToLower()));
            }

            //Filtering by MinPlayersCount
            if(request.MinPlayersCount.HasValue == true)
            {
                contentQueriable = contentQueriable.Where(a => a.MinPlayersCount == request.MinPlayersCount);
            }

             //Filtering by MaxPlayersCount
            if(request.MaxPlayersCount.HasValue == true)
            {
                contentQueriable = contentQueriable.Where(a => a.MaxPlayersCount == request.MaxPlayersCount);
            }

             //Filtering by MinAge
            if(request.MinAge.HasValue == true)
            {
                contentQueriable = contentQueriable.Where(a => a.MinAge >= request.MinAge);
            }

            //Filtering by Category
            if(String.IsNullOrWhiteSpace(request.CategoryName) == false)
            {
                contentQueriable = contentQueriable.Where(a => a.Category!.Name.ToLower().Contains(request.CategoryName.ToLower()));
            }

            //Filtering by Mechanic
            if(String.IsNullOrWhiteSpace(request.MechanicName) == false)
            {
                contentQueriable = contentQueriable.Where(a => a.Mechanics!
                                                                .Select(a => a.Name.ToLower())
                                                                .Contains(request.MechanicName.ToLower()));
            }

            //Filtering by Rating
            if(request.AverageRating.HasValue == true)
            {
                contentQueriable = contentQueriable.Where(a => a.AverageRating == request.AverageRating);
            }

            var content = await contentQueriable
                                .Select(a => new UsersFindBoardGameResponse
                                {
                                    BoardGameId = a.Id,
                                    BoarGameName = a.Name
                                })
                               .OrderBy(a => a.BoarGameName)
                               .ToListAsync();

            if(content == null || content.Count == 0)
            {
                return (null, "Error: nothing found");
            }             

            return (content, "Board Games listed found successfully");
        }

        private static (bool, string) FindBoardGame_Validation(UsersFindBoardGameRequest? request)
        {
            if(request == null)
            {
                return (true, string.Empty);
            }
            if(string.IsNullOrWhiteSpace(request!.BoardGameName) == false && request.BoardGameName.Length < 3)
            {
                return (false, "Error: filtering board games by name requires at least 3 characters.");
            }

            if(request.MinPlayersCount.HasValue == true && request.MinPlayersCount < 1)
            {
                return (false, "Error: invalid MinPlayersCount (is less than 1).");
            }

            if(request.MaxPlayersCount.HasValue == true && request.MaxPlayersCount < 1)
            {
                return (false, "Error: invalid MaxPlayersCount (is less than 1).");
            }

            if(request.MinAge.HasValue == true && request.MinAge < 1)
            {
                return (false, "Error: invalid MinAge (is less than 1).");
            }

            if( (request.MinPlayersCount.HasValue == true && request.MaxPlayersCount.HasValue == true) && 
                 request.MaxPlayersCount < request.MinPlayersCount)
            {
                return (false, "Error: invalid PlayersCount (MaxPlayersCount greater than MinPlayersCount).");
            }

            if(String.IsNullOrWhiteSpace(request.CategoryName) == false && request.CategoryName.Length < 3)
            {
                return (false, "Error: filtering board games by category requires at least 3 characters.");
            }

            if(String.IsNullOrWhiteSpace(request.MechanicName) == false && request.MechanicName.Length < 3)
            {
                return (false, "Error: filtering board games by mechanic requires at least 3 characters.");
            }

            if(request.AverageRating.HasValue == true && request.AverageRating < 0)
            {
                return (false, "Error: invalid Rate (is less than 0).");
            }

            if(request.AverageRating.HasValue == true && request.AverageRating > 5)
            {
                return (false, "Error: invalid Rate (is greater than 5).");
            }

            return (true, string.Empty);            
        }
    
        public async Task<(UsersShowBoardGameDetailsResponse?, string)> ShowBoardGameDetails(UsersShowBoardGameDetailsRequest? request)
        {
            var (isValid, message) = ShowBoardGameDetails_Validation(request);

            if(isValid == false)
            {
                return (null, message);
            }

            #region FETCHING THE REQUESTED BOARD GAME
            var boardgameDB = await this._daoDbContext
                                        .BoardGames
                                        .Include(a => a.Mechanics)
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(a => a.Id == request!.BoardGameId);

            if(boardgameDB == null)
            {
                return (null, "Error: request BoardGame not found");
            }

            if(boardgameDB.IsDeleted == true)
            {
                return (null, "Error: request BoardGame is deleted");
            }
            #endregion

            #region FETCHING THE BG CATEGORY NAME
            var categoryDB = await this._daoDbContext
                                       .Categories                                       
                                       .FindAsync(boardgameDB.CategoryId);

            if(categoryDB == null)
            {
                return (null, "Error: no category found for the requested board game");
            }
            
            var category = String.Empty;

            if(categoryDB.IsDeleted == true)
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

            if(requestedMechanicIds == null || requestedMechanicIds.Count == 0)
            {
                return (null, "Error: no mechanics found for the requested board game");
            }

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

            if(loggedSessionsDB == null || loggedSessionsDB.Count == 0)
            {
                return (null, "Error: no sessions found for this game board");
            }

            var loggedSessionsCount = 0;

            var avgSessionDuration = 0;
            
            var lastFiveSessions = new List<UsersShowBoardGameDetailsResponse_sessions>(){};

            if(loggedSessionsDB != null && loggedSessionsDB.Count > 0)
            {
                loggedSessionsCount = loggedSessionsDB.Count;
                
                avgSessionDuration = (int)Math.Ceiling(loggedSessionsDB.Average(a => a.Duration_minutes));
                
                var n = 5;

                if(loggedSessionsCount < 5)
                {
                    n = loggedSessionsCount;
                }


                loggedSessionsDB = loggedSessionsDB.OrderByDescending(a => a.Date).ToList();
                
                for(int i = 0; i < n; i ++)
                {
                    lastFiveSessions.Add(new UsersShowBoardGameDetailsResponse_sessions
                    {
                        UserNickName = loggedSessionsDB[i].User!.Nickname,
                        Date = loggedSessionsDB[i].Date,
                        PlayersCount = loggedSessionsDB[i].PlayersCount,
                        Duration = loggedSessionsDB[i].Duration_minutes
                    });
                }
            }
            #endregion

            var content = new UsersShowBoardGameDetailsResponse
            {
                BoardGameName = boardgameDB.Name,
                BoardGameDescription = boardgameDB.Description,
                Category = categoryDB.Name,
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

        private static (bool, string) ShowBoardGameDetails_Validation(UsersShowBoardGameDetailsRequest? request)
        {
            if(request == null)
            {
                return (false, "Error: request is null");
            }

            if(request.BoardGameId.HasValue == false)
            {
                return (false, "Error: BoardGameId is missing");
            }

            if(request.BoardGameId < 1)
            {
                return (false, "Error: invalid BoardGameId (is less than 1)");
            }

            return (true, string.Empty);
        } 
    
        public async Task<(UsersBoardGamesRankingResponse?, string)> BoardGamesRanking(UsersBoardGamesRankingRequest? request)
        {
            var (isValid, message) = BoardGamesRanking_Validation(request);

            if(isValid == false)
            {
                return (null, message);
            }

            #region SESSIONS QUERIES
            var sessionsDB = await this._daoDbContext
                                         .Sessions
                                         .AsNoTracking()
                                         .Include(a => a.BoardGame)
                                         .ToListAsync();
            
            if(sessionsDB == null || sessionsDB.Count < 1)
            {
                return (null, "Error: no board games sessions found for the rankings");
            }

            var theMostPlayed = sessionsDB.GroupBy(a => a.BoardGameId)
                                          .OrderByDescending(a => a.Count())
                                          .Take(3)
                                          .Select(a => a.First().BoardGame!.Name)
                                          .ToList();

            var theShortest = sessionsDB.GroupBy(a => a.BoardGameId)
                                        .OrderBy(a => a.First().Duration_minutes)
                                        .Take(3)
                                        .Select(a => a.First().BoardGame!.Name)
                                        .ToList();

            var theLongest = sessionsDB.GroupBy(a => a.BoardGameId)
                                       .OrderByDescending(a => a.Max(b => b.Duration_minutes))
                                       .ThenByDescending(a => a.Count())
                                       .ThenBy(a => a.Key)
                                       .Take(3)
                                       .Select(a => a.First().BoardGame!.Name)
                                       .ToList();

            var adultsFavorites = sessionsDB.Where(a => a.BoardGame!.MinAge >= 18)
                                            .GroupBy(a => a.BoardGameId)
                                            .OrderByDescending(a => a.Count())
                                            .Take(3)
                                            .Select(a => a.First().BoardGame!.Name)
                                            .ToList();

            var teensFavorites = sessionsDB.Where(a => a.BoardGame!.MinAge >= 18)
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

            if(boardGamesDB == null || boardGamesDB.Count < 1)
            {
                return (null, "Error: no board games found for the ranking");
            }
            
            var theBestRated = boardGamesDB.OrderByDescending(a => a.AverageRating)
                                           .Take(3)
                                           .Select(a => a.Name)
                                           .ToList();         
            #endregion

            var content = new UsersBoardGamesRankingResponse 
            {
                MostPlayedBoardGames = theMostPlayed,
                BestRatedBoardGames = theBestRated,
                ShortestBoardGames = theShortest,
                LongestBoardGames = theLongest,
                AdultsFavoritesBoardGames = adultsFavorites,
                TeensFavoritesBoardGames = teensFavorites
            };

            if(content == null)
            {
                return (null, "Error: ranking failed");
            }

            return (content, "Board games ranked successfully");
        }

        private static (bool, string) BoardGamesRanking_Validation(UsersBoardGamesRankingRequest? request)
        {
            if(request != null)
            {
                return (false, "Error: no parameter should be passed for the ranking of the board games");
            }

            return (true, String.Empty);
        }

        public async Task<(UsersCategoriesRankingResponse?, string)> CategoriesRanking(UsersCategoriesRankingRequest? request)
       {
            var (isValid, message) = CategoriesRanking_Validation(request);

            if(isValid == false)
            {
                return (null, message);
            }

            var boardgamesDB_exist = await this._daoDbContext
                                               .BoardGames
                                               .AsNoTracking()
                                               .AnyAsync(a => a.IsDeleted == false);
            if(boardgamesDB_exist == false)
            {
                return (null, "Error: no board game found for the categories ranking");
            }

            var categorizedBoardgames_exist = await this._daoDbContext
                                             .Categories
                                             .AsNoTracking()                                             
                                             .AnyAsync(a => a.BoardGames!.Any());

            if(categorizedBoardgames_exist == false)
            {
                return (null, "Error: no board game having a category found");
            }

            var sessionsDB_exist = await this._daoDbContext
                                             .Sessions
                                             .AsNoTracking()
                                             .AnyAsync();

            if(sessionsDB_exist == false)
            {
                return (null, "Error: board games session found for the categories ranking");
            }

            var categoriesDB = await this._daoDbContext
                                         .Categories
                                         .Include(a => a.BoardGames!)
                                         .ThenInclude(a => a.Sessions!)
                                         .AsNoTracking()
                                         .ToListAsync();

            if(categoriesDB == null)
            {
                return (null, "Error: no categories found for the ranking");
            }

            var theMostPlayed = categoriesDB.Select(a => new UsersCategoriesRankingResponse_mostPlayedOnes
                                            {
                                                CategoryName = a.Name,
                                                SessionsCount = a.BoardGames!.Sum(b => b.Sessions!.Count)
                                            })
                                            .OrderByDescending(a => a.SessionsCount)
                                            .Take(3)
                                            .ToList();

            var theMostPopular = categoriesDB.Select(a => new UsersCategoriesRankingResponse_mostPopularOnes
                                             {
                                                CategoryName = a.Name,
                                                BoardGamesCount = a.BoardGames!.Count
                                             })
                                             .OrderByDescending(a => a.BoardGamesCount)
                                             .Take(3)
                                             .ToList();
            
            var theBestRated = categoriesDB.Select(a => new UsersCategoriesRankingResponse_bestRatedOnes                                           
                                           {
                                                CategoryName = a.Name,
                                                AvgRating = a.BoardGames!.Count != 0 ? 
                                                    (int)a.BoardGames!.Average(b => b.AverageRating) : 0
                                           })
                                           .OrderByDescending(a => a.AvgRating)
                                           .Take(3)
                                           .ToList();
            
            var theLongest = categoriesDB.Select(a => new UsersCategoriesRankingResponse_longestOnes
                                         {
                                            CategoryName = a.Name,
                                            Duration = a.BoardGames!.SelectMany(b => b.Sessions!).Any() == true ?
                                                (int)a.BoardGames!.SelectMany(b => b.Sessions!).Average(c => c.Duration_minutes) : 0 
                                         })
                                         .OrderByDescending(a => a.Duration)
                                         .Take(3)
                                         .ToList();   

            var theShortest = categoriesDB.Select(a => new UsersCategoriesRankingResponse_shortestOnes
                                          {
                                                CategoryName = a.Name,
                                                Duration = a.BoardGames!.SelectMany(b => b.Sessions!).Any() == true ?
                                                    (int)a.BoardGames!.SelectMany(b => b.Sessions!).Average(c => c.Duration_minutes) : 0                                                
                                          })
                                          .Where(a => a.Duration > 0)
                                          .OrderBy(a => a.Duration)
                                          .Take(3)
                                          .ToList();                             
                                                                             
            var content = new UsersCategoriesRankingResponse
            {
                MostPlayedCategories = theMostPlayed,
                MostPopularCategories = theMostPopular,
                BestRatedCategories = theBestRated,
                LongestCategories = theLongest,
                ShortestCategories = theShortest
            };

            return (content, "Categories ranking successfully");
       }

       private static (bool, string) CategoriesRanking_Validation(UsersCategoriesRankingRequest? request)
       {
            if(request != null)
            {
                return (false, "Error: no parameter should be passed for the ranking of the board games");
            }

            return (true, String.Empty);
       } 
    }
}