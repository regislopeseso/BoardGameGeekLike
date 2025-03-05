using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
                Durantion_minutes = request.Duration_minutes!.Value
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
            sessionDB.Durantion_minutes = request.Duration_minutes!.Value;

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
    }
}