using BoardGameGeekLike.Models;
using BoardGameGeekLike.Models.Dtos.Request;
using BoardGameGeekLike.Models.Dtos.Response;
using BoardGameGeekLike.Models.Entities;
using BoardGameGeekLike.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Numerics;
using System.Security.Claims;
using System.Text.RegularExpressions;


namespace BoardGameGeekLike.Services
{
    public class UsersService
    {
        private readonly ApplicationDbContext _daoDbContext;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsersService(ApplicationDbContext daoDbContext, UserManager<User> userManager, SignInManager<User> signInManager, IHttpContextAccessor httpContextAccessor)
        {
            this._daoDbContext = daoDbContext;
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._httpContextAccessor = httpContextAccessor;
        }

        // USER'S PROFILE
        //
        public async Task<(UsersSignUpResponse?, string)> SignUp(UsersSignUpRequest? request, string? userRole)
        {
            var (isValid, message) = SignUp_Validation(request);
            
            if (isValid == false)
            {
                return (null, message);
            }     
           
            var userEmail_exists = await this._daoDbContext
                .Users
                .AsNoTracking()
                .AnyAsync(a => a.Email == request!.Email && a.IsDeleted == false);

            if (userEmail_exists == true)
            {
                return (null, "Error: requested emal is already in use");
            }            

            var parsedDate = DateOnly.ParseExact(request!.UserBirthDate!, "yyyy-MM-dd");

            var user = new User
            {
                Name = request.Name!,
                UserName = request.Email!,
                Email = request.Email!.ToLower(),
                BirthDate = parsedDate,
                Gender = request.Gender
            };

            var signUpAttempt = await _userManager.CreateAsync(user, request.Password!);

            if(signUpAttempt.Succeeded == false)
            {
                var errors = string.Join("; ", signUpAttempt.Errors.Select(e => e.Description));
                return (null, $"Error creating user: {errors}");
            }

            await _userManager.AddToRoleAsync(user, "User");

            return (new UsersSignUpResponse(), "User signed up successfully");
        }
        private static (bool, string) SignUp_Validation(UsersSignUpRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (string.IsNullOrWhiteSpace(request.Name) == true)
            {
                return (false, "Error: Name is null or empty");
            }

            if (string.IsNullOrWhiteSpace(request.Email)== true)
            {
                return (false, "Error: UserEmail is missing");
            }

            string emailPattern = @"(^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}(?:\.[a-zA-Z]{2,10})?$)";

            if (Regex.IsMatch(request.Email, emailPattern) == false)
            {
                return (false, "Error: invalid email format");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return (false, "Error: password is null");
            }

            if (request.Password.Trim().Length < 6)
            {
                return (false, "Error: password must have at leat 6 digits");
            }
            
            if(string.IsNullOrEmpty(request.Password) == true)
            {
                return (false, "Error: UserPassword is missing");
            }
            
            if (string.IsNullOrWhiteSpace(request.UserBirthDate) == true)
            {
                return (false, "Error: UserBirthDate is missing");
            }

            string birthDatePattern = @"^(19|20)\d{2}-(0[1-9]|1[0-2])-(0[1-9]|[12]\d|3[01])$";

            if (Regex.IsMatch(request.UserBirthDate, birthDatePattern) == false)
            {
                return (false, "Error: invalid birth date format. Expected format: yyyy-MM-dd");
            }

            // Convert string to DateOnly
            if (DateOnly.TryParseExact(request.UserBirthDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateOnly parsedDate) == false)
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
                return (false, "Error: invalid birth date");
            }          

            if (Enum.IsDefined(request.Gender) == false)
            {
                var validOption = string.Join(", ", Enum.GetValues(typeof(Gender))
                                       .Cast<Gender>()
                                       .Select(gender => $"{gender} ({(int)gender})"));

                return (false, $"Error: invalid Gender. It must be one of the following: {validOption}");
            }

            return (true, String.Empty);
        }


        private async Task<(int, string)>ValidatePassword(User userDB, string userPassword)
        {
            var isPasswordValid = await _userManager.CheckPasswordAsync(userDB, userPassword);

            var countFailedAttempts = await this._userManager
                .GetAccessFailedCountAsync(userDB);

            var maxAllowedAttempts = this._userManager
                .Options
                .Lockout
                .MaxFailedAccessAttempts;

            var remainingAttempts = maxAllowedAttempts - countFailedAttempts;

            var isUserLocked = await this._userManager.IsLockedOutAsync(userDB);

            if (isUserLocked == true)
            {
                return (-1, "Error: account temporarily locked duo to multiple failed attempts");
            }
         
            if (isPasswordValid == false)
            {
                remainingAttempts--;
                await _userManager.AccessFailedAsync(userDB);
                return (remainingAttempts, $"Invalid Password. You have {remainingAttempts} attempts remaining");
            }

            await this._userManager.ResetAccessFailedCountAsync(userDB);

            return (remainingAttempts, string.Empty);
        }
       

        public async Task<(UsersSignInResponse?, string)> SignIn(UsersSignInRequest? request)
        {
            var (isValid, message) = SignIn_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var userDB = await this._daoDbContext
                .Users
                .FirstOrDefaultAsync(a => a.Email == request!.Email);          

            if(userDB == null)
            {
                return (null, "Requested user does not exist");
            }
            if(userDB.IsDeleted == true)
            {
                return (null, "Requested user has been deleted");
            }

            var result = await this._signInManager
                .PasswordSignInAsync(request!.Email!, request.Password!, false, true);

            if (result.IsLockedOut == true)
            {
                return (null, "Error: account temporarily locked due to multiple failed attempts");
            }

            if (result.IsNotAllowed == true)
            {
                return (null, "Error: account is not allowed to sign in (e.g. email not confirmed)");
            }

            var countFailedAttempts = await this._userManager
                .GetAccessFailedCountAsync(userDB);

            var maxAllowedAttempts = this._userManager
                .Options
                .Lockout
                .MaxFailedAccessAttempts;

            var remainingAttempts = maxAllowedAttempts - countFailedAttempts;
    

            if (result.Succeeded == false)
            {               
                var response = new UsersSignInResponse
                {
                    RemainingSignInAttempts = remainingAttempts
                };

                return (response, $"Error: email or password is incorrect. You have {remainingAttempts} attempts remaining");
            }

            await this._userManager.ResetAccessFailedCountAsync(userDB);

            return (new UsersSignInResponse(), $"User: {userDB.Name} signed in successfully");
        }
        private static (bool, string) SignIn_Validation(UsersSignInRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }       

            if (string.IsNullOrWhiteSpace(request.Email) == true)
            {
                return (false, "Error: Email is missing");
            }

            string emailPattern = @"(^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}(?:\.[a-zA-Z]{2,10})?$)";

            if (Regex.IsMatch(request.Email, emailPattern) == false)
            {
                return (false, "Error: invalid email format");
            }

            if (string.IsNullOrEmpty(request.Password) == true)
            {
                return (false, "Error: UserPassword is missing");
            }

            return (true, String.Empty);
        }


        public (UsersValidateStatusResponse?, string) ValidateStatus()
        {         
            if (this._httpContextAccessor.HttpContext?.User.Identity != null &&
        this._httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true)
            {
                return (new UsersValidateStatusResponse {IsUserLoggedIn = true  } , "User is authenticated.");
            }
            else
            {
                return (new UsersValidateStatusResponse { IsUserLoggedIn = false },  "User is not authenticated.");
            }
        }


        public async Task<(UsersGetRoleResponse?, string)> GetRole()
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext!.User);
            if (user == null)
            {
                return (null, "Error: User not found");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var userRole = roles.FirstOrDefault(); // assuming 1 role per user

            if (string.IsNullOrEmpty(userRole))
            {
                return (null, "Error: User has no role assigned");
            }

            return (new UsersGetRoleResponse
            {
                Role = userRole
            }, $"User role: {userRole}" );
        }


        public async Task<(UsersSignOutResponse?, string)> SignOut()
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext!.User);
            if (user == null)
            {
                return (null, "Error: User not found");
            }

            if (user.IsDeleted == true)
            {
                return (null, "Error: User has been deleted");
            }   

            try
            {
                await _signInManager.SignOutAsync();

                return (new UsersSignOutResponse { IsUserSignOut = true }, "User signed out successfully");
            }
            catch (Exception ex)
            {               
                return (new UsersSignOutResponse { IsUserSignOut = false }, $"Error: Failed to sign out user. {ex.Message}");
            }
        }


        public async Task<(UsersEditProfileResponse?, string)> EditProfile(UsersEditProfileRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = EditProfile_Validation(request);
            
            if (isValid == false)
            {
                return (null, message);
            } 

            var userEmail_exists = await this._daoDbContext
                                                .Users
                                                .AsNoTracking()
                                                .AnyAsync(a => a.Id != userId &&
                                                               a.Email == request!.NewEmail &&
                                                               a.IsDeleted == false);

            if(userEmail_exists == true)
            {
                return (null, "Error: requested Email is already in use");
            }

            var userDB = await this._daoDbContext
                                   .Users
                                   .FindAsync(userId);

            if(userDB == null)
            {
                return (null, "Error: user not found");
            }

            if(userDB.IsDeleted == true)
            {
                return (null, "Error: user is deleted");
            }
            
            var parsedDate = DateOnly.ParseExact(request.NewBirthDate!, "yyyy-MM-dd");

           

            userDB.Name = request.NewName;
            userDB.Email = request.NewEmail!.ToLower();
            userDB.UserName = request.NewEmail;
            userDB.BirthDate = parsedDate;
            
            var updateResult = await _userManager.UpdateAsync(userDB);

            if (!updateResult.Succeeded)
            {
                return (null, "Error: failed to update user profile");
            }

            return (null, "User's profile edited successfully");
        }
        private static (bool, string) EditProfile_Validation(UsersEditProfileRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (string.IsNullOrWhiteSpace(request.NewName) == true)
            {
                return (false, "Error: Name is missing");
            }

            if (string.IsNullOrWhiteSpace(request.NewEmail) == true)
            {
                return (false, "Error: UserEmail is missing");
            }

            string emailPattern = @"(^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}(?:\.[a-zA-Z]{2,10})?$)";

            if (Regex.IsMatch(request.NewEmail, emailPattern) == false)
            {
                return (false, "Error: invalid email format");
            }

            if (string.IsNullOrWhiteSpace(request.NewBirthDate) == true)
            {
                return (false, "Error: UserBirthDate is missing");
            }

            string birthDatePattern = @"^(19|20)\d{2}-(0[1-9]|1[0-2])-(0[1-9]|[12]\d|3[01])$";

            if (Regex.IsMatch(request.NewBirthDate, birthDatePattern) == false)
            {
                return (false, "Error: invalid birth date format. Expected format: yyyy-MM-dd");
            }

            // Convert string to DateOnly
            if (DateOnly.TryParseExact(request.NewBirthDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateOnly parsedDate) == false)
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


        public async Task<(UsersChangePasswordResponse?, string)> ChangePassword(UsersChangePasswordRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = ChangePassword_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var userDB = await this._daoDbContext
                                   .Users
                                   .FindAsync(userId);

            if (userDB == null)
            {
                return (null, "Error: user not found");
            }

            if (userDB.IsDeleted == true)
            {
                return (null, "Error: user is deleted");
            }

            var (remainingAttempts, text) = await this.ValidatePassword(userDB, request!.CurrentPassword!);
            
            if (remainingAttempts < 3)
            {
                return (new UsersChangePasswordResponse
                {
                    RemainingPasswordAttempts = remainingAttempts
                }, text);
            }
            
            // Remove current password
            var removeResult = await _userManager.RemovePasswordAsync(userDB);
            if (removeResult.Succeeded == false)
            {
                return (null, "Error: failed to remove old password");
            }

            // Sets new password
            var addPasswordResult = await _userManager.AddPasswordAsync(userDB, request!.NewPassword!);
            if (addPasswordResult.Succeeded == false)
            {
                return (null, "Error: failed to set new password");
            }

            return (new UsersChangePasswordResponse(), "Password changed successfully");
        }
        public static (bool, string) ChangePassword_Validation(UsersChangePasswordRequest? request)
        {
            if(request == null)
            {
                return (false, "Error: null request");
            }

            if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            {
                return (false, "Error: password is null");
            }

            if (request.CurrentPassword.Trim().Length < 6)
            {
                return (false, "Error: password must have at leat 6 digits");
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return (false, "Error: password is null");
            }

            if (request.NewPassword.Trim().Length < 6)
            {
                return (false, "Error: password must have at leat 6 digits");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersDeleteProfileResponse?, string)> DeleteProfile(UsersDeleteProfileRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }                        

            var (isValid, message) = DeleteProfile_Validation(request);
            
            if (isValid == false)
            {
                return (null, message);
            }

            var userDB = await this._daoDbContext
                                   .Users
                                   .FindAsync(userId);

            if(userDB == null)
            {
                return (null, "Error: user not found");
            }

            if(userDB.IsDeleted == true)
            {
                return (null, "Error: this user's profile was already deleted");
            }

            var (remainingAttempts, text) = await this.ValidatePassword(userDB, request!.Password!);

            if (remainingAttempts <= 3)
            {
                return (new UsersDeleteProfileResponse
                {
                    RemainingPasswordAttempts = remainingAttempts
                }, text);
            }             
  
            await this._daoDbContext
                      .Users
                      .Where(a => a.Id == userId)
                      .ExecuteUpdateAsync(a => a.SetProperty(b => b.IsDeleted, true));

            await this.SignOut();

            return (new UsersDeleteProfileResponse(), "User's profile deleted successfully");
        }
        private static (bool, string) DeleteProfile_Validation(UsersDeleteProfileRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }
            
            if(string.IsNullOrWhiteSpace(request.Password) == true)
            {
                return (false, "Error: requested password is empty");
            }

            return (true, String.Empty);
        }


        public async Task<(UsersGetProfileDetailsResponse?, string)> GetProfileDetails(UsersGetProfileDetailsRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = GetProfileDetails_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var userDB = await this._daoDbContext
                .Users
                .FindAsync(userId);

            if(userDB == null)
            {
                return (null, "Error: User not found");
            }
            
            if (userDB.IsDeleted == true)
            {
                return (null, "Error: User has been deleted");
            }

            var countSessionsDB = await this._daoDbContext
                .Sessions
                .AsNoTracking()
                .Where(a => a.UserId == userId && a.IsDeleted == false)
                .CountAsync();

            var countRatedBgDB = await this._daoDbContext
                .Ratings
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .CountAsync();

            var treatmentTitle = userDB.Gender == 0 ? "Mr." : "Mrs.";

            return (new UsersGetProfileDetailsResponse
            {
                TreatmentTitle = treatmentTitle,
                Name = userDB.Name,
                Email = userDB.Email,
                BirthDate = userDB.BirthDate,
                SignUpDate = userDB.SignUpDate,
                SessionsCount = countSessionsDB,
                RatedBgCount = countRatedBgDB
            }, "User details loaded successfully");

        }
        private static (bool, string) GetProfileDetails_Validation(UsersGetProfileDetailsRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: request is NOT null. It must be null!");
            }         

            return (true, String.Empty);
        }
        //
        //--* end of USER'S PROFILE *--//
        //



        //
        // BOARD GAMES
        //
        public async Task<(UsersLogSessionResponse?, string)> LogSession(UsersLogSessionRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = LogSession_Validation(request);
            
            if (isValid == false)
            {
                return (null, message);
            }

            var user_exists = await this._daoDbContext
                                        .Users
                                        .AsNoTracking()
                                        .AnyAsync(a => a.Id == userId && a.IsDeleted == false);

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
                UserId = userId!,
                BoardGameId = request.BoardGameId!.Value,
                PlayersCount = request.PlayersCount!.Value,
                Duration_minutes = request.Duration_minutes!.Value
            };

            boardgameDB.SessionsCount++;

            if(request.Date != null)
            {
                newSession.Date = DateOnly.ParseExact(request.Date!, "yyyy-MM-dd");
            }

            await this._daoDbContext.Sessions.AddAsync(newSession);

            var newAvgDuration = (boardgameDB.AvgDuration_minutes * boardgameDB.SessionsCount + request.Duration_minutes!.Value) / (boardgameDB.SessionsCount + 1);

            await this._daoDbContext
                .BoardGames
                .Where(a => a.Id == boardgameDB.Id)
                .ExecuteUpdateAsync(a => a.SetProperty(b => b.AvgDuration_minutes, newAvgDuration));    

            await this._daoDbContext.SaveChangesAsync();

            return (null, $"Session logged successfully, new Avg.Duration is: {newAvgDuration} minutes");
        }
        private static (bool, string) LogSession_Validation(UsersLogSessionRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
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
                string datePattern = @"^(19|20)\d{2}-(0[1-9]|1[0-2])-(0[1-9]|[12]\d|3[01])$";

                if (Regex.IsMatch(request.Date, datePattern) == false)
                {
                    return (false, "Error: invalid date format. Expected format: yyyy-MM-dd");
                }

                // Convert string to DateOnly
                if (DateOnly.TryParseExact(request.Date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateOnly parsedDate) == false)
                {
                    return (false, "Error: invalid date");
                }

                var thisYear = DateTime.Now.Year;
                
                var requestedYear = parsedDate.Year;
                
                if(thisYear - requestedYear < 0)
                {
                    return (false, "Error: invalid date. Requested date is a future date");
                }

                if (thisYear - requestedYear > 100)
                {
                    return (false, "Error: invalid date. Minimum date allowed is 100 years ago");
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
                return (false, "Error: invalid Duration_minutes (the maximum duration allowed is 1440 minutes = 1 day)");
            }

            return (true, String.Empty);           
        }
        

        public async Task<(List<UsersListPlayedBoardGamesResponse>?, string)> ListPlayedBoardGames(UsersListPlayedBoardGamesRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = ListPlayedBoardGames_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var content = await this._daoDbContext
                .BoardGames
                .Where(a => a.IsDeleted == false && a.Sessions!.Any(b => b.UserId == userId && b.IsDeleted == false))
                .Select(a => new UsersListPlayedBoardGamesResponse
                {
                    BoardGameId = a.Id,
                    BoardGameName = a.Name
                })
                .ToListAsync();
           

            if (content == null || content.Count == 0)
            {
                return (null, "No sessions logged by user yet");
            }      
        
            return (content, "Board Games played by user listed successfully");
        }
        private static (bool, string) ListPlayedBoardGames_Validation(UsersListPlayedBoardGamesRequest? request)
        {
            if (request == null)
            {
                return (true, string.Empty);
            }
            
            return (true, string.Empty);
        }


        public async Task<(UsersGetSessionsResponse?, string)> GetSessions(UsersGetSessionsRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = GetSessions_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var bgExists = await this._daoDbContext
                .BoardGames
                .AsNoTracking()
                .AnyAsync(a => a.Id == request!.BoardGameId );

            if(bgExists == false)
            {
                return (null, "Error: requested board game was not found");
            }
            

            var sessionsDB = await this._daoDbContext
                .Sessions
                .Where(a => a.BoardGameId == request!.BoardGameId && a.UserId == userId && a.IsDeleted == false)
                .ToListAsync();

            if(sessionsDB == null)
            {
                return (null, "Error: no sessions found for the requested board game");
            }
            
            return (new UsersGetSessionsResponse { Sessions = sessionsDB},"Seessions loaded successfully");
        }
        private static (bool, string) GetSessions_Validation(UsersGetSessionsRequest? request)
        {
            if (request == null)
            {
                return (false, $"Error, request failed, request is null == {request == null}");
            }
            
            if(request.BoardGameId.HasValue == false)
            {
                return (false, $"Error, BoardGameId Id request failed, request.BoardGameId.HasValue == {request.BoardGameId.HasValue}");
            }

            if (request.BoardGameId.HasValue == true & request.BoardGameId.Value <= 0)
            {
                return (false, $"Error, BoardGameId Id request failed, request.BoardGameId.Value == {request.BoardGameId.Value}");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersEditSessionResponse?, string)> EditSession(UsersEditSessionRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

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
                .FirstOrDefaultAsync(a => a.Id == request.SessionId && a.UserId == userId);
            
            if(sessionDB == null)
            {
                return (null, "Error: board game session not found");
            }

            if(sessionDB.IsDeleted == true)
            {
                return (null, "Error: this session is deleted");
            }

            if(boardgameDB.Id != request.BoardGameId)
            {
                await this._daoDbContext
                    .BoardGames
                    .Where(a => a.Id == boardgameDB.Id)
                    .ExecuteUpdateAsync(a => a.SetProperty(b => b.SessionsCount, b => b.SessionsCount - 1));

                await this._daoDbContext
                    .BoardGames
                    .Where(a => a.Id == request.BoardGameId)
                    .ExecuteUpdateAsync(a => a.SetProperty(b => b.SessionsCount, b => b.SessionsCount + 1));
            }

            sessionDB.BoardGameId = request.BoardGameId!.Value;
            sessionDB.PlayersCount = request.NewPlayersCount!.Value;
            sessionDB.Duration_minutes = request.NewDuration_minutes!.Value;

            if(request.NewDate != null)
            {
                sessionDB.Date = DateOnly.ParseExact(request.NewDate!, "yyyy-MM-dd");
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

            if(String.IsNullOrWhiteSpace(request.NewDate) == false)
            {
                string datePattern = @"^\d{4}-(0[1-9]|1[0-2])-(0[1-9]|[12][0-9]|3[01])$";

                if (Regex.IsMatch(request.NewDate, datePattern) == false)
                {
                    return (false, "Error: invalid date format. Expected format: yyyy-MM-dd");
                }

                // Convert string to DateOnly
                if (DateOnly.TryParseExact(request.NewDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateOnly parsedDate) == false)
                {
                    return (false, "Error: invalid date");
                }

                var thisYear = DateTime.Now.Year;
                
                var requestedYear = parsedDate.Year;
                
                if(thisYear - requestedYear < 0)
                {
                    return (false, "Error: invalid date. Requested date is a future date");
                }
            }

            if(request.NewPlayersCount.HasValue == false)
            {
                return (false, "Error: PlayersCount is missing");
            }

            if(request.NewPlayersCount < 1)
            {
                return (false, "Error: invalid PlayersCount (is less than 1)");
            }

            if(request.NewDuration_minutes.HasValue == false)
            {
                return (false, "Error: Daration_minutes is missing");
            }

            if(request.NewDuration_minutes < 0)
            {
                return (false, "Error: invalid Duration_minutes (is negative)");
            }

            if(request.NewDuration_minutes > 1440)
            {
                return (false, "Error: invalid Duration_minutes (the maximum duration allowed is 1440 minutes)");
            }

            return (true, String.Empty);
        }
    

        public async Task<(UsersDeleteSessionResponse?, string)> DeleteSession(UsersDeleteSessionRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = DeleteSession_Validation(request);

            if(isValid == false)
            {
                return (null, message);
            }

            var sessionDB = await this._daoDbContext
                .Sessions
                .FirstOrDefaultAsync(a => a.Id == request!.SessionId && a.UserId == userId);

            if (sessionDB == null || sessionDB.UserId != userId)
            {
                return (null, "Error: session not found");
            }

            if(sessionDB.IsDeleted == true)
            {
                return (null, "Error: session was already deleted");
            }

            var boardGameId = sessionDB.BoardGameId;

            await this._daoDbContext
                .BoardGames
                .Where(a => a.Id == boardGameId)
                .ExecuteUpdateAsync(a => a.SetProperty(b => b.SessionsCount, b => b.SessionsCount - 1));

            await this._daoDbContext
                .Sessions
                .Where(a => a.Id == request!.SessionId)
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


        public async Task<(UsersRateResponse?, string)> Rate(UsersRateRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = Rate_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }           

            var boardgameDB = await this._daoDbContext
                .BoardGames
                .FindAsync(request!.BoardGameId);

            if (boardgameDB == null)
            {
                return (null, "Error: board game not found");
            }

            if (boardgameDB.IsDeleted == true)
            {
                return (null, "Error: board game is deleted");
            }

            var rate_exists = await this._daoDbContext
                .Ratings
                .AsNoTracking()
                .AnyAsync(a => a.UserId == userId && a.BoardGameId == request.BoardGameId);

            if (rate_exists == true)
            {
                return (null, "Error: the request board game was already rated by this user");
            }

            var newRate = new Rating
            {
                Rate = request.Rate!.Value,
                UserId = userId,
                BoardGameId = request.BoardGameId!.Value
            };

            await this._daoDbContext.Ratings.AddAsync(newRate);

            var newAverageRating =
            (
                ((boardgameDB.AverageRating * boardgameDB.RatingsCount) + newRate.Rate) / (boardgameDB.RatingsCount + 1)
            );

            boardgameDB.AverageRating = newAverageRating;
            boardgameDB.RatingsCount++;



            await this._daoDbContext.SaveChangesAsync();

            return (new UsersRateResponse(), $"Board game rated successfully, its new average rating is: {newAverageRating:F1}");
        }
        private static (bool, string) Rate_Validation(UsersRateRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.Rate.HasValue == false)
            {
                return (false, "Error: Rate is missing");
            }

            if (request.Rate.HasValue == true && (request.Rate < 0 || request.Rate > 5) == true)
            {
                return (false, "Error: invalid rate. It must be a value between 0 and 5");
            }       

            if (request.BoardGameId.HasValue == false)
            {
                return (false, "Error: BoardGameId is missing");
            }

            if (request.BoardGameId < 1)
            {
                return (false, "Error: invalid BoardGameId (is less than 1)");
            }

            return (true, String.Empty);
        }
     

        public async Task<(List<UsersListRatedBoardGamesResponse>?, string)> ListRatedBoardGames(UsersListRatedBoardGamesRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = ListRatedBoardGames_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var content = new List<UsersListRatedBoardGamesResponse>();

            if (request!.BoardGameName! == null || string.IsNullOrWhiteSpace(request.BoardGameName) == true)
            {
                var ratedBoardgamesDB = await this._daoDbContext
                    .Ratings
                    .AsNoTracking()
                    .Where(a => a.UserId == userId)
                    .Select(a => new UsersListRatedBoardGamesResponse 
                    { 
                        BoardGameId = a.BoardGameId, 
                        BoardGameName = a.BoardGame!.Name,
                        RatingId = a.Id,
                        Rate = (int)Math.Ceiling(a.Rate)
                    })
                    .ToListAsync();

                content = ratedBoardgamesDB;
            }
            else
            {
                var ratedBoardgamesDB = await this._daoDbContext
                    .Ratings
                    .AsNoTracking()
                    .Where(a => a.UserId == userId && a!.BoardGame!.Name.ToLower().Contains(request!.BoardGameName!.ToLower()))
                    .Select(a => new UsersListRatedBoardGamesResponse
                    {
                        BoardGameId = a.BoardGameId,
                        BoardGameName = a.BoardGame!.Name,
                        RatingId = a.Id,
                        Rate = (int)Math.Ceiling(a.Rate)
                    })
                    .ToListAsync();

                content = ratedBoardgamesDB;
            }

            if (content == null)
            {
                return (null, "Error: no board games found");
            }
             
            return (content, "Board Games listed found successfully");
        }
        private static (bool, string) ListRatedBoardGames_Validation(UsersListRatedBoardGamesRequest? request)
        {
            if (request == null)
            {
                return (false, "Error request is null");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersEditRatingResponse?, string)> EditRating(UsersEditRatingRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = EditRating_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }            

            var ratingDB = await this._daoDbContext
                .Ratings
                .Include(a => a.BoardGame)
                .FirstOrDefaultAsync(a => a.Id == request!.RatingId && a.UserId == userId);

            if (ratingDB == null)
            {
                return (null, "Error: requested rating not found");
            }
                     
            var oldRate = ratingDB.Rate;

            ratingDB.Rate = request!.Rate!.Value;

            var boardgameDB = ratingDB.BoardGame;

            if(boardgameDB == null)
            {
                return (null, "Error: requested rated board game not found");
            }

            var newAverageRating =
            (
                (boardgameDB.AverageRating * boardgameDB.RatingsCount - oldRate + request.Rate!.Value) / (boardgameDB.RatingsCount)
            );

            boardgameDB.AverageRating = newAverageRating;

            await this._daoDbContext.SaveChangesAsync();

            return (null, $"Rating edited successfully, its new average rating is: {newAverageRating:F1}");
        }
        private static (bool, string) EditRating_Validation(UsersEditRatingRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.Rate.HasValue == false)
            {
                return (false, "Error: Rate is missing");
            }

            if (request.Rate.HasValue == true && (request.Rate < 0 || request.Rate > 5) == true)
            {
                return (false, "Error: invalid rate. It must be a value between 0 and 5");
            }       

            if (request.RatingId.HasValue == false)
            {
                return (false, "Error: BoardGameId is missing");
            }

            if (request.RatingId < 1)
            {
                return (false, "Error: invalid BoardGameId (is less than 1)");
            }

            return (true, String.Empty);
        }
        

        public async Task<(UsersDeleteRatingResponse?, string)> DeleteRating(UsersDeleteRatingRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = DeleteRating_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var ratingDB = await this._daoDbContext
                .Ratings
                .Include(a => a.BoardGame)
                .FirstOrDefaultAsync(a => a.Id == request!.RateId && a.UserId == userId);

            if (ratingDB == null || ratingDB.UserId != userId)
            {
                return (null, "Error: user rating not found");
            }

            var boardgameDB = ratingDB.BoardGame;

            if (boardgameDB == null || boardgameDB.IsDeleted == true)
            {
                return (null, "Error: rated board game not found");
            }

            var ratingsCount = boardgameDB.RatingsCount;
            var oldAvgRating = boardgameDB.AverageRating;

            
            if (ratingsCount < 2)
            {
                boardgameDB.AverageRating = 0;
                
                boardgameDB.RatingsCount = 0;

                this._daoDbContext.Ratings.Remove(ratingDB);

                await this._daoDbContext.SaveChangesAsync();

                return (null, $"Board game rating deleted successfully");
            }
                 
            var newAverageRating = (oldAvgRating * ratingsCount - ratingDB.Rate) / (ratingsCount - 1); 

            boardgameDB.AverageRating = newAverageRating;
            boardgameDB.RatingsCount = ratingsCount - 1;    

            this._daoDbContext.Ratings.Remove(ratingDB);
            
            await this._daoDbContext.SaveChangesAsync();

            return (null, $"Board game rating deleted successfully, its new average rating is: {newAverageRating:F1}");
        }
        private static (bool, string) DeleteRating_Validation(UsersDeleteRatingRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.RateId.HasValue == false)
            {
                return (false, "Error: RateId is missing");
            }

            if (request.RateId < 1)
            {
                return (false, "Error: invalid RateId (is less than 1)");
            }

            return (true, String.Empty);
        }
        //
        //--* end of BOARD GAMES *--//        



        // LIFE COUNTERS
        //
        // 1º LIFE COUNTER QUICK START      
        public async Task<(UsersQuickStartLifeCounterResponse?, string)> QuickStartLifeCounter(UsersQuickStartLifeCounterRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (requestIsValid, message) = QuickStartLifeCounter_Validation(request);

            if (requestIsValid == false)
            {
                return (null, message);
            }

            //  => Search for any existing life counter MANAGER...
            var doesAnyLifeCounterManagerExists = await this._daoDbContext
                .LifeCounterManagers
                .AnyAsync(a => a.UserId == userId);

            var text = "";

            int? lifeCounterManagerId = 0;          

            if (doesAnyLifeCounterManagerExists == false)
            {
                // No life counter MANAGER was found, THEN:
                // => Search for ANY existing life counter TEMPLATE...
                var doesAnyLifeCounterTemplateExists = await this._daoDbContext
                .LifeCounterTemplates
                .AnyAsync(a => a.UserId == userId);

                int? lifeCounterTemplateId;

                if (doesAnyLifeCounterTemplateExists == false)
                {
                    //  No life counter manager exists AND no life counter TEMPLATE exists, THEN:
                    //  => Call Create LifeCounter Template and Create Life Counter Manager...

                    // Creating a new default Life Counter TEMPLATE:
                    var (createTemplate_reponse_content, createTemplate_response_message) = await this.CreateLifeCounterTemplate();

                    if (createTemplate_reponse_content == null)
                    {
                        return (null, $"Error: request to create a new Default Life Counter TEMPLATE failed: {createTemplate_response_message}");
                    }

                    // Default Life Counter Template Id:
                    lifeCounterTemplateId = createTemplate_reponse_content.LifeCounterTemplateId;

                    text = "New Default life counter TEMPLATE and new Default life counter MANAGER were created successfully";
                }
                else
                {
                    //  No life counter manager exists BUT at least one life counter TEMPLATE exists, THEN:
                    //  => Call Create Life Counter Manager and create one for the newest created template...

                    // Fechting the id of the most recently life counter TEMPLATE created:
                    var (getLastTemplateId_reponse_content, getLastTemplateId_response_message) = await this.GetLastLifeCounterTemplateId();

                    if (getLastTemplateId_reponse_content == null)
                    {
                        return (null,
                            $"Error: failed to fetch the ID of the most recently created Life Counter TEMPLATE: {getLastTemplateId_response_message}");
                    }

                    // Most recently created Life Counter Template Id:
                    lifeCounterTemplateId = getLastTemplateId_reponse_content.LastLifeCounterTemplateId;

                    text = "New Default life counter MANAGER started successfully, belonging to the last created life counter Template";

                }

                // Starting a new Life Counter MANAGER...
                var startLifeCounterManager_request = new UsersStartLifeCounterManagerRequest
                {
                    LifeCounterTemplateId = lifeCounterTemplateId
                };

                var (createManager_response_content, createManager_response_message) = await this.StartLifeCounterManager(startLifeCounterManager_request);

                if (createManager_response_content == null)
                {
                    return (null, $"Error: request to create a new Default Life Counter MANAGER failed: {createManager_response_message}");
                }                

                lifeCounterManagerId = createManager_response_content.LifeCounterManagerId;               
            }
            else
            {
                // At least one life counter MANAGER was found, THEN:               
                // => fetch the most recently started life counter MANAGER..
                var lifeCounterManagerDB = await this._daoDbContext
                .LifeCounterManagers
                .Include(a => a.LifeCounterPlayers)
                .OrderByDescending(a => a.Id)
                .FirstOrDefaultAsync(a => a.UserId == userId);

                if (lifeCounterManagerDB == null)
                {
                    return (null, "Error: attempt to fetch the last life counter MANAGER failed, returning null");
                }

                var playersDB = lifeCounterManagerDB.LifeCounterPlayers;

                if (playersDB == null || playersDB.Count < 1)
                {
                    return (null, "Error: the fetched most recently started life counter MANAGER faled to return its players: null or less than 1 player");
                }

                if (lifeCounterManagerDB.IsFinished == false)
                {
                    // The most recently started life counter MANAGER found is NOT yet finished, THEN:
                    // => reload the life counter MANAGER found... 
                    lifeCounterManagerId = lifeCounterManagerDB.Id;                

                    text = "Most recently not yet finished Life Counter Manager loaded successfully";
                }
                else
                {
                    // No life counter MANAGER that was not yet finished was found to be reloaded, THEN:
                    //
                    // 1st => start a new life counter MANAGER of the template... 
                    var startNewManagerCopy_request = new UsersStartLifeCounterManagerRequest
                    {
                        LifeCounterTemplateId = lifeCounterManagerDB.LifeCounterTemplateId,
                    };

                    var (startNewManagerCopy_response_content, startNewManagerCopy_response_message) = await this.StartLifeCounterManager(startNewManagerCopy_request);

                    if (startNewManagerCopy_response_content == null)
                    {
                        return (null,
                            $"Error trying to start a life counter MANAGER relative to the most recently finished one: {startNewManagerCopy_response_message}");
                    }
                    ;

                    var newLifeCounterManagerId = startNewManagerCopy_response_content.LifeCounterManagerId;

                    // 2nd => overwrite via EDIT endpoint the data of started life counter MANAGER with the data of the most recently finished life counter MANAGER...                    
                    var editManager_request = new UsersEditLifeCounterManagerRequest
                    {
                        LifeCounterManagerId = newLifeCounterManagerId,
                        NewLifeCounterManagerName = lifeCounterManagerDB.LifeCounterManagerName,
                        NewPlayersCount = lifeCounterManagerDB.PlayersCount,
                        NewPlayersStartingLifePoints = lifeCounterManagerDB.PlayersStartingLifePoints,
                        FixedMaxLifePointsMode = lifeCounterManagerDB.FixedMaxLifePointsMode,
                        NewPlayersMaxLifePoints = lifeCounterManagerDB.PlayersMaxLifePoints,
                        AutoDefeatMode = lifeCounterManagerDB.AutoDefeatMode,
                        AutoEndMode = lifeCounterManagerDB.AutoEndMode,
                    };

                    var (editManager_response_content, editManager_response_message) = await this.EditLifeCounterManager(editManager_request);

                    if (editManager_response_content == null)
                    {
                        return (null,
                            $"Error trying to edit the new a life counter MANAGER started " +
                            $"that mirrors the most recently finished one: {editManager_response_message}");
                    }
                    ;

                    lifeCounterManagerId = editManager_response_content!.LifeCounterManagerId;

                    // 3rd => fetch the players belonging to the new life counter MANAGER started in order to edit its players...
                    var newPlayers = await this._daoDbContext
                        .LifeCounterPlayers
                        .Where(a => a.LifeCounterManagerId == newLifeCounterManagerId)
                        .ToListAsync();

                    // 4th => overwite the new players data of the started life counter MANAGER with the data of the most recently finished life counter MANAGER (except for the PlayersCurrentLifePoints and isDefeated fields)...
                    for (int i = 0; i < playersDB.Count; i++)
                    {
                        newPlayers[i].PlayerName = playersDB[i].PlayerName;
                        newPlayers[i].CurrentLifePoints = lifeCounterManagerDB.PlayersStartingLifePoints;
                        newPlayers[i].AutoDefeatMode = lifeCounterManagerDB.AutoDefeatMode;
                        newPlayers[i].IsDefeated = false;
                    }

                    text = "Most recently FINISHED Life Counter Manager has been copied and started successfully";
                }
            }

            var content = new UsersQuickStartLifeCounterResponse
            {
                LifeCounterManagerId = lifeCounterManagerId,            
            };

            await this._daoDbContext.SaveChangesAsync();

            return (content, text);
        }
        private static (bool, string) QuickStartLifeCounter_Validation(UsersQuickStartLifeCounterRequest? request)
        {
            if (request != null)
            {
                return (false, $"Error: request is not null however it must be null");
            }

            return (true, string.Empty);
        }


        //
        // 2º LIFE COUNTER TEMPLATES
        public async Task<(UsersCreateLifeCounterTemplateResponse?, string)> CreateLifeCounterTemplate(UsersCreateLifeCounterTemplateRequest? request = null)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var lifeCounterTemplateName = "";
            
            if (request == null)
            {
                var countLifeCounterTemplatesDB = await this._daoDbContext
                .LifeCounterTemplates
                .Where(a => a.UserId == userId)
                .CountAsync();
                
                lifeCounterTemplateName = $"Life Counter Template #{countLifeCounterTemplatesDB}";

                request = new UsersCreateLifeCounterTemplateRequest
                {
                    LifeCounterTemplateName = lifeCounterTemplateName,
                    PlayersStartingLifePoints = 10,
                    PlayersCount = 1,
                    FixedMaxLifePointsMode = false,
                    PlayersMaxLifePoints = null,
                    AutoDefeatMode = false,
                    AutoEndMatch = false,
                };
            }
            else
            {
                var (isValid, message) = CreateLifeCounterTemplate_Validation(request);

                if (isValid == false)
                {
                    return (null, message);
                }

                lifeCounterTemplateName = request!.LifeCounterTemplateName;

                var exists = await this._daoDbContext
                                       .LifeCounterTemplates
                                       .AnyAsync(a => a.UserId == userId && a.LifeCounterTemplateName == request!.LifeCounterTemplateName);

                if (exists == true)
                {
                    return (null, $"Error: {request!.LifeCounterTemplateName} already exists");
                }
            }
                
            var newLifeCounterTemplate = new LifeCounterTemplate
            {
                LifeCounterTemplateName = lifeCounterTemplateName,
                PlayersStartingLifePoints = request.PlayersStartingLifePoints,
                PlayersCount = request!.PlayersCount,
                FixedMaxLifePointsMode = request.FixedMaxLifePointsMode,
                PlayersMaxLifePoints = request.PlayersMaxLifePoints,
                AutoDefeatMode = request.AutoDefeatMode,
                AutoEndMode = request.AutoEndMatch,

                UserId = userId,
            };

            this._daoDbContext.Add(newLifeCounterTemplate);

            await this._daoDbContext.SaveChangesAsync();

            return (new UsersCreateLifeCounterTemplateResponse { LifeCounterTemplateId = newLifeCounterTemplate.Id }, "New LifeCounterTemplate created successfully");
        }
        public static (bool, string) CreateLifeCounterTemplate_Validation(UsersCreateLifeCounterTemplateRequest? request)
        {
            if(String.IsNullOrWhiteSpace(request!.LifeCounterTemplateName) == true)
            {
                return (false,
                   "Error: LifeCounterTemplateName is null or empty! " +
                   $"request.LifeCounterTemplateName: {request.LifeCounterTemplateName}");
            }

            if(request.PlayersStartingLifePoints.HasValue == false || request.PlayersStartingLifePoints == null)
            {
                return (false,
                   "Error: StartingLifePoints is null! " +
                   $"request.PlayersStartingLifePoints: {request.PlayersStartingLifePoints}");
            }

            if (request.PlayersStartingLifePoints < 0)
            {
                return (false, 
                    "Error: StartingLifePoints cannot be less than 0! " +
                    $"request.PlayersStartingLifePoints: {request.PlayersStartingLifePoints}");
            }

            if (request.PlayersCount.HasValue == false || request.PlayersCount == null)
            {
                return (false,
                   "Error: PlayersCount is null! " +
                   $"request.PlayersCount: {request.PlayersCount}");
            }

            if (request.PlayersCount < 1 || request.PlayersCount > 6)
            {
                return (false, 
                    "Error: PlayersCount cannot be less than 1 or more than 6! " +
                    $"request.PlayersCount: {request.PlayersCount}");
            }

            if (request.FixedMaxLifePointsMode.HasValue == false || request.PlayersMaxLifePoints == null)
            {
                return (false,
                   "Error: FixedMaxLifePointsMode is null! " +
                   $"request.FixedMaxLifePointsMode: {request.FixedMaxLifePointsMode}");
            }

            if (request.PlayersMaxLifePoints.HasValue == false || request.PlayersMaxLifePoints == null)
            {
                return (false,
                   "Error: PlayersMaxLifePoints is null! " +
                   $"request.PlayersMaxLifePoints: {request.PlayersMaxLifePoints}");
            }

            if (request.PlayersMaxLifePoints < 1 || request.PlayersMaxLifePoints > 999)
            {
                return (false, 
                    "Error: PlayersMaxLifePoints cannot be less than 1 or more than 999! " +
                    $"request.PlayersMaxLifePoints: {request.PlayersMaxLifePoints}");
            }

            if (request.AutoDefeatMode.HasValue == false || request.AutoDefeatMode == null)
            {
                return (false,
                   "Error: AutoDefeatMode is null! " +
                   $"request.AutoDefeatMode: {request.AutoDefeatMode}");
            }

            if (request.AutoEndMatch.HasValue == false || request.AutoEndMatch == null)
            {
                return (false,
                   "Error: AutoEndMatch is null! " +
                   $"request.AutoEndMatch: {request.AutoEndMatch}");
            }

            return (true, String.Empty);
        }


        public async Task<(UsersCountLifeCounterTemplatesResponse?, string)> CountLifeCountersTemplates(UsersCountLifeCounterTemplatesRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = CountLifeCounterTemplates_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var lifecountersCountDB = await this._daoDbContext
                .LifeCounterTemplates
                .Where(a => a.UserId == userId)
                .CountAsync();

            return (new UsersCountLifeCounterTemplatesResponse { LifeCounterTemplatesCount = lifecountersCountDB }, "User's LifeCounterTemplates counted successfully");
        }
        private static (bool, string) CountLifeCounterTemplates_Validation(UsersCountLifeCounterTemplatesRequest? request)
        {
            if (request != null)
            {
                return (false, $"Error: request is not null: {request}");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersGetLastLifeCounterTemplateIdResponse?, string)> GetLastLifeCounterTemplateId(UsersGetLastLifeCounterTemplateIdRequest? request = null)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = GetLastLifeCounterTemplateId_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var lifeCounterTemplateDB = await this._daoDbContext
                .LifeCounterTemplates
                .OrderByDescending(a => a.Id)
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (lifeCounterTemplateDB == null)
            {
                return (null, $"Error: life counter template request failed: {lifeCounterTemplateDB}");
            }           

            return (new UsersGetLastLifeCounterTemplateIdResponse
            {
                LastLifeCounterTemplateId = lifeCounterTemplateDB.Id,
            }, "LifeCounterTemplate details fetched successfully");
        }
        private static (bool, string) GetLastLifeCounterTemplateId_Validation(UsersGetLastLifeCounterTemplateIdRequest? request)
        {
            if (request != null)
            {
                return (false, $"Error: request is not null, it must be null!");
            }            

            return (true, string.Empty);
        }


        public async Task<(List<UsersListLifeCounterTemplatesResponse>?, string)> ListLifeCounterTemplates(UsersListLifeCounterTemplatesRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = ListLifeCounterTemplates_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var content = await this._daoDbContext
                .LifeCounterTemplates
                .Where(a => a.UserId == userId)
                .Select(a => new UsersListLifeCounterTemplatesResponse
                {
                    LifeCounterTemplateId = a.Id,
                    LifeCounterTemplateName = a.LifeCounterTemplateName!
                })
                .ToListAsync();


            if (content == null || content.Count == 0)
            {
                return (null, "No life counter templates were created by this user yet");
            }

            return (content, "User's LifeCounterTemplates listed successfully");
        }
        private static (bool, string) ListLifeCounterTemplates_Validation(UsersListLifeCounterTemplatesRequest? request)
        {
            if (request != null)
            {
                return (false, $"Error: request is not null: {request}");
            }

            return (true, string.Empty);
        }

        
        public async Task<(UsersGetLifeCounterTemplateDetailsResponse?, string)> GetLifeCounterTemplateDetails(UsersGetLifeCounterTemplateDetailsRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = GetLifeCounterTemplateDetails_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var lifeCounterDB = await this._daoDbContext
                .LifeCounterTemplates
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Id == request.LifeCounterTemplateId);

            if (lifeCounterDB == null)
            {
                return (null, $"Error: life counter template request failed: {lifeCounterDB}");
            }

            var response = new UsersGetLifeCounterTemplateDetailsResponse
            {
                LifeCounterTemplateName = lifeCounterDB.LifeCounterTemplateName,
                PlayersStartingLifePoints = lifeCounterDB.PlayersStartingLifePoints,
                PlayersCount = lifeCounterDB.PlayersCount,
                FixedMaxLifePointsMode = lifeCounterDB.FixedMaxLifePointsMode,
                PlayersMaxLifePoints = lifeCounterDB.PlayersMaxLifePoints,
                AutoDefeatMode = lifeCounterDB.AutoDefeatMode,
                AutoEndMode = lifeCounterDB.AutoEndMode,
            };

            return (response, "LifeCounterTemplate details fetched successfully");
        }
        private static (bool, string) GetLifeCounterTemplateDetails_Validation(UsersGetLifeCounterTemplateDetailsRequest request)
        {
            if (request == null)
            {
                return (false, $"Error: request failed: {request}");
            }

            if (request.LifeCounterTemplateId == null || request.LifeCounterTemplateId.HasValue == false)
            {
                return (false, $"Error: requested LifeCounterTemplateId failed: {request.LifeCounterTemplateId}");
            }

            return (true, string.Empty);
        }

        //
        // 3º LIFE COUNTER MANAGERS
        public async Task<(UsersStartLifeCounterManagerResponse?, string)> StartLifeCounterManager(UsersStartLifeCounterManagerRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (requestIsValid, message) = StartLifeCounterManager_Validation(request);

            if (requestIsValid == false)
            {
                return (null, message);
            }

            var lifeCounterTemplateDB = await this._daoDbContext
                .LifeCounterTemplates
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Id == request!.LifeCounterTemplateId);

            if (lifeCounterTemplateDB == null)
            {
                return (null, $"Error, invalid requested Life Counter, returning: {lifeCounterTemplateDB}");
            }          

            var newPlayers = new List<LifeCounterPlayer>();

            for (int playersCount = 1; playersCount <= lifeCounterTemplateDB.PlayersCount; playersCount++)
            {
                var name = $"Player #{playersCount}";

                newPlayers.Add
                (
                    new LifeCounterPlayer
                    {
                        PlayerName = name,
                        StartingLifePoints = lifeCounterTemplateDB.PlayersStartingLifePoints,
                        CurrentLifePoints = lifeCounterTemplateDB.PlayersStartingLifePoints,
                        FixedMaxLifePointsMode = lifeCounterTemplateDB.FixedMaxLifePointsMode!.Value,
                        MaxLifePoints = lifeCounterTemplateDB.PlayersMaxLifePoints,
                        AutoDefeatMode = lifeCounterTemplateDB.AutoDefeatMode!.Value,
                    }
                );
            }

            var startMark = DateTime.UtcNow.ToLocalTime().Ticks;

            var newLifeCounterManager = new LifeCounterManager
            {
                LifeCounterTemplateId = request!.LifeCounterTemplateId,
                
                LifeCounterManagerName = lifeCounterTemplateDB.LifeCounterTemplateName,
                LifeCounterPlayers = newPlayers,
                PlayersCount = lifeCounterTemplateDB.PlayersCount,
                PlayersStartingLifePoints = lifeCounterTemplateDB.PlayersStartingLifePoints,
                FixedMaxLifePointsMode = lifeCounterTemplateDB.FixedMaxLifePointsMode,
                PlayersMaxLifePoints = lifeCounterTemplateDB.PlayersMaxLifePoints,
                AutoDefeatMode = lifeCounterTemplateDB.AutoDefeatMode!.Value,
                AutoEndMode = lifeCounterTemplateDB.AutoEndMode!.Value,
                
                StartingTime = startMark,
                
                UserId = userId,
            };

            lifeCounterTemplateDB.LifeCounterManagers ??= [];

            lifeCounterTemplateDB.LifeCounterManagers.Add(newLifeCounterManager);

            lifeCounterTemplateDB.LifeCounterManagersCount++;

            await this._daoDbContext.SaveChangesAsync();

            var content = new UsersStartLifeCounterManagerResponse
            {
                LifeCounterTemplateId = lifeCounterTemplateDB.Id,
                LifeCounterTemplateName = lifeCounterTemplateDB.LifeCounterTemplateName,

                LifeCounterManagerId = newLifeCounterManager.Id,
                LifeCounterManagerName = lifeCounterTemplateDB.LifeCounterTemplateName,

                PlayersStartingLifePoints = lifeCounterTemplateDB.PlayersStartingLifePoints,
                PlayersCount = lifeCounterTemplateDB.PlayersCount,
                LifeCounterPlayers = [],

                FixedMaxLifePointsMode = lifeCounterTemplateDB.FixedMaxLifePointsMode,
                PlayersMaxLifePoints = lifeCounterTemplateDB.PlayersMaxLifePoints,
                
                AutoDefeatMode = lifeCounterTemplateDB.AutoDefeatMode,
                AutoEndMode = lifeCounterTemplateDB.AutoEndMode
            };

            foreach (var newPlayer in newPlayers)
            {
                content.LifeCounterPlayers.Add(new UsersStartLifeCounterManagerResponse_players
                {
                    LifeCounterPlayerId = newPlayer.Id,
                    PlayerName = newPlayer.PlayerName,
                    PlayerStartingLifePoints = newPlayer.CurrentLifePoints,
                });
            }

            var teste = await this._daoDbContext.LifeCounterTemplates
                .Where(a => a.Id == lifeCounterTemplateDB.Id)
                .ExecuteUpdateAsync(a => 
                    a.SetProperty(b => b.LifeCounterManagersCount, 
                    b => b.LifeCounterManagersCount + 1));     

            if(teste <= 0)
            {
                return (null, "failed!");
            }

            return (content, $"New {lifeCounterTemplateDB.LifeCounterTemplateName} instance started with {newPlayers.Count} players");
        }
        private static (bool, string) StartLifeCounterManager_Validation(UsersStartLifeCounterManagerRequest? request)
        {
            if (request == null)
            {
                return (false, $"Error: request failed: {request}");
            }

            if (request.LifeCounterTemplateId == null || request.LifeCounterTemplateId.HasValue == false)
            {
                return (false, $"Error: requested LifeCounterId failed: {request.LifeCounterTemplateId}");
            }          

            return (true, string.Empty);
        }


        public async Task<(UsersEditLifeCounterManagerResponse?, string)> EditLifeCounterManager(UsersEditLifeCounterManagerRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = EditLifeCounterManager_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var lifeCounterManagerDB = await this._daoDbContext
                .LifeCounterManagers
                .Include(a => a.LifeCounterPlayers)
                .FirstOrDefaultAsync(a =>
                    a.UserId == userId &
                    a.Id == request!.LifeCounterManagerId);

            if (lifeCounterManagerDB == null)
            {
                return (null, "Error: LifeCounterManager request failed");
            }

            if (request!.NewPlayersCount < lifeCounterManagerDB.PlayersCount)
            {
                return (null, "Error, new players count request failed");
            }

            lifeCounterManagerDB.LifeCounterManagerName = request!.NewLifeCounterManagerName;

            var playersDB = lifeCounterManagerDB.LifeCounterPlayers;

            if (playersDB == null)
            {
                return (null, "Error: Life counter manager players request failed");
            }

            foreach (var player in playersDB)
            {
                player.StartingLifePoints = request.NewPlayersStartingLifePoints;
                player.FixedMaxLifePointsMode = request.FixedMaxLifePointsMode!.Value;
                player.MaxLifePoints = request.NewPlayersMaxLifePoints;
                player.AutoDefeatMode = request.AutoDefeatMode!.Value;
            }

            if (request.NewPlayersCount > lifeCounterManagerDB.PlayersCount)
            {
                for (var i = lifeCounterManagerDB.PlayersCount; i < request.NewPlayersCount; i++)
                {
                    var newPlayerName = $"Player #{i+1}";
                    playersDB.Add(new LifeCounterPlayer
                    {
                        PlayerName = newPlayerName,
                        StartingLifePoints = request.NewPlayersStartingLifePoints,
                        CurrentLifePoints = request.NewPlayersStartingLifePoints,
                        FixedMaxLifePointsMode = request.FixedMaxLifePointsMode!.Value,
                        MaxLifePoints = request.NewPlayersMaxLifePoints,
                        AutoDefeatMode = request.AutoDefeatMode!.Value, 
                    });
                }
            }           

            lifeCounterManagerDB.PlayersCount = request.NewPlayersCount;

            lifeCounterManagerDB.FixedMaxLifePointsMode = request.FixedMaxLifePointsMode;
            
            if(request.FixedMaxLifePointsMode == true)
            {
                lifeCounterManagerDB.PlayersMaxLifePoints = request.NewPlayersMaxLifePoints;
            }

            lifeCounterManagerDB.AutoDefeatMode = request.AutoDefeatMode;

            foreach (var player in playersDB)
            {
                if(request.AutoDefeatMode == false) player.IsDefeated = false;
                if(request.AutoDefeatMode == true && player.CurrentLifePoints == 0) player.IsDefeated = true;
            }         

            lifeCounterManagerDB.AutoEndMode = request.AutoEndMode!.Value;

            if(lifeCounterManagerDB.AutoDefeatMode == false || lifeCounterManagerDB.AutoEndMode == false)
            {
                lifeCounterManagerDB.Duration_minutes = 0;
                lifeCounterManagerDB.EndingTime = null;
                lifeCounterManagerDB.IsFinished = false;
            }

            await this._daoDbContext.SaveChangesAsync();

            return (new UsersEditLifeCounterManagerResponse
            {
                LifeCounterManagerId = lifeCounterManagerDB.Id,
            }, $"Life Counter Manager {lifeCounterManagerDB.LifeCounterManagerName} edited successfully");
        }
        private static (bool, string) EditLifeCounterManager_Validation(UsersEditLifeCounterManagerRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.LifeCounterManagerId == null || request.LifeCounterManagerId < 1)
            {
                return (false, $"Error: LifeCounterManagerId request failed: {request.LifeCounterManagerId}");
            }

            if (string.IsNullOrWhiteSpace(request.NewLifeCounterManagerName) == true)
            {
                return (false, $"Error: NewLifeCounterName request failed: {request.NewLifeCounterManagerName}");
            }

            if (request.NewPlayersCount == null || request.NewPlayersCount < 1)
            {
                return (false, $"Error: NewPlayersCount request failed: {request.NewPlayersCount}");
            }

            if (request.NewPlayersStartingLifePoints == null || request.NewPlayersStartingLifePoints < 1)
            {
                return (false, $"Error: NewPlayersStartingLifePoints request failed: {request.NewPlayersStartingLifePoints}");
            }

            if (request.FixedMaxLifePointsMode == null)
            {
                return (false, $"Error: FixedMaxLifePointsMode request failed: {request.FixedMaxLifePointsMode}");
            }

            if (request.FixedMaxLifePointsMode == true && 
                (request.NewPlayersMaxLifePoints == null || request.NewPlayersMaxLifePoints < 1))
            {
                return (false, $"Error: NewPlayersMaxLifePoints request failed: {request.NewPlayersMaxLifePoints}");
            }

            if (request.AutoDefeatMode == null)
            {
                return (false, $"Error: AutoDefeatMode request failed: {request.AutoDefeatMode}");
            }

            if (request.AutoEndMode == null)
            {
                return (false, $"Error: AutoEndMode request failed: {request.AutoEndMode}");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersRefreshLifeCounterManagerResponse?, string)> RefreshLifeCounterManager(UsersRefreshLifeCounterManagerRequest request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (requestIsValid, message) = RefreshLifeCounterManager_Validation(request);

            if (requestIsValid == false)
            {
                return (null, message);
            }

            var lifeCounterManagerDB = await this._daoDbContext
                .LifeCounterManagers
                .Include(a => a.LifeCounterPlayers)
                .Where(a => a.Id == request.LifeCounterManagerId &&
                    a.UserId == userId)
                .FirstOrDefaultAsync();

            if (lifeCounterManagerDB == null)
            {
                return (null, $"Error, invalid requested LifeCounterManager, returning: {lifeCounterManagerDB}");
            }    

            lifeCounterManagerDB.StartingTime = DateTime.UtcNow.ToLocalTime().Ticks;

            lifeCounterManagerDB.Duration_minutes = 0;

            lifeCounterManagerDB.EndingTime = null;

            lifeCounterManagerDB.IsFinished = false;

            var lifeCounterPlayers = lifeCounterManagerDB.LifeCounterPlayers;

            if (lifeCounterPlayers == null || lifeCounterPlayers.Count <= 0)
            {
                return (null, $"Error: life counter players request failed: {lifeCounterPlayers}");
            }

            foreach (var player in lifeCounterPlayers)
            {
                player.CurrentLifePoints = player.StartingLifePoints;
                player.IsDefeated = false;
            }

            await this._daoDbContext.SaveChangesAsync();

            return (new UsersRefreshLifeCounterManagerResponse
            {
                IsLifeCounterAlreadyFinished = false,
            }, "Life Counter Manager refreshed successfully!");
        }
        private static (bool, string) RefreshLifeCounterManager_Validation(UsersRefreshLifeCounterManagerRequest request)
        {
            if (request == null)
            {
                return (false, $"Error: request failed: {request}");
            }

            if (request.LifeCounterManagerId.HasValue == false || request.LifeCounterManagerId < 1)
            {
                return (false, $"Error: requested LifeCounterPlayerId failed: {request.LifeCounterManagerId}");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersCheckForLifeCounterManagerEndResponse?, string)> CheckForLifeCounterManagerEnd(UsersCheckForLifeCounterManagerEndRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (requestIsValid, message) = CheckForLifeCounterManagerEnd_Validation(request);

            if (requestIsValid == false)
            {
                return (null, message);
            }
            var lifeCounterManagerDB = await this._daoDbContext
                .LifeCounterManagers
                .Include(a => a.LifeCounterPlayers)
                .FirstOrDefaultAsync(a => a.UserId == userId & a.Id == request!.LifeCounterManagerId & a.AutoEndMode == true);

            if (lifeCounterManagerDB == null)
            {
                return (null, "Error: LifeCounterManager request failed");
            }

            var lifeCounterPlayersDB = lifeCounterManagerDB.LifeCounterPlayers;

            if (lifeCounterPlayersDB == null || lifeCounterPlayersDB.Count <= 0)
            {
                return (null, $"Error: lifeCounterPlayersDB request failed: {lifeCounterPlayersDB}");
            }

            var playersCount = lifeCounterManagerDB!.PlayersCount;

            var defeatePlayers = lifeCounterPlayersDB.Where(a => a.IsDefeated == true).Count();

            var isLifeCounterManagerEnded = (playersCount - defeatePlayers <= 1) == true;

            if (isLifeCounterManagerEnded == false)
            {
                return (new UsersCheckForLifeCounterManagerEndResponse()
                , "This life counter manager is NOT finished");
            }

            var currentTimeMark = DateTime.UtcNow.ToLocalTime().Ticks;

            lifeCounterManagerDB.IsFinished = true;
            lifeCounterManagerDB.EndingTime = currentTimeMark;
            lifeCounterManagerDB.Duration_minutes = (int)Math.Ceiling((double)((currentTimeMark - lifeCounterManagerDB.StartingTime!.Value) / 600_000_000));

            foreach(var player in lifeCounterPlayersDB)
            {
                if(player.CurrentLifePoints <= 0)
                {
                    player.IsDefeated = true;
                }
            }

            await this._daoDbContext.SaveChangesAsync();

            return (new UsersCheckForLifeCounterManagerEndResponse
            {
                IsFinished = true,
                Duration_minutes = lifeCounterManagerDB.Duration_minutes,
            }, "This life counter manager is finished");
        }
        private static (bool, string) CheckForLifeCounterManagerEnd_Validation(UsersCheckForLifeCounterManagerEndRequest? request)
        {
            if (request == null)
            {
                return (false, $"Error: request failed: {request}");
            }

            if (request.LifeCounterManagerId.HasValue == false || request.LifeCounterManagerId < 1)
            {
                return (false, $"Error: requested LifeCounterPlayerId failed: {request.LifeCounterManagerId}");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersFinishLifeCounterManagerResponse?, string)> FinishLifeCounterManager(UsersFinishLifeCounterManagerRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = FinishLifeCounterManager_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var lifeCounterManagerDB = await this._daoDbContext
                .LifeCounterManagers
                .Include(a => a.LifeCounterPlayers)
                .FirstOrDefaultAsync(a =>
                    a.UserId == userId &
                    a.Id == request!.LifeCounterManagerId &
                    a.IsFinished == false);

            if (lifeCounterManagerDB == null)
            {
                return (null, "Error: LifeCounterManager request failed");
            }

            var playersDB = lifeCounterManagerDB.LifeCounterPlayers;

            if (playersDB == null)
            {
                return (null, "Error: LifeCounterPlayes of requested LifeCounterManager to-be-edited failed");
            }

            foreach(var player in lifeCounterManagerDB.LifeCounterPlayers!)
            {
                player.CurrentLifePoints = 0;
                player.IsDefeated = true;
            }

            var currentTimeMark = DateTime.UtcNow.ToLocalTime().Ticks;

            var duration = (int)Math.Ceiling((double)((currentTimeMark - lifeCounterManagerDB.StartingTime!.Value) / 600_000_000));

            lifeCounterManagerDB.IsFinished = true;
            lifeCounterManagerDB.EndingTime = currentTimeMark;
            lifeCounterManagerDB.Duration_minutes = duration;

            await this._daoDbContext.SaveChangesAsync();

            return (new UsersFinishLifeCounterManagerResponse
            {
                LifeCounterManagerName = lifeCounterManagerDB.LifeCounterManagerName,
                LifeCounterManager_Duration_minutes = lifeCounterManagerDB.Duration_minutes,
            }, $"Life Counter Manager {lifeCounterManagerDB.LifeCounterManagerName} finished successfully," +
                $" it's duration was: {lifeCounterManagerDB.Duration_minutes}");
        }
        private static (bool, string) FinishLifeCounterManager_Validation(UsersFinishLifeCounterManagerRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.LifeCounterManagerId == null || request.LifeCounterManagerId < 1)
            {
                return (false, $"Error: LifeCounterManagerId request failed: {request.LifeCounterManagerId}");
            }           

            return (true, string.Empty);
        }

       
        public async Task<(UsersGetLifeCounterManagerDetailsResponse?, string)> GetLifeCounterManagerDetails(UsersGetLifeCounterManagerDetailsRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = GetLifeCounterManagerDetails_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var lifeCounterManagerDB = await this._daoDbContext
                .LifeCounterManagers
                .Include(a => a.LifeCounterPlayers)
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Id == request!.LifeCounterManagerId);

            if (lifeCounterManagerDB == null)
            {
                return (null, "Error: LifeCounterManagerDB request failed");
            }

            var lifeCounterPlayersDB = lifeCounterManagerDB.LifeCounterPlayers;

            if (lifeCounterPlayersDB == null || lifeCounterPlayersDB.Count < 1)
            {
                return (null, "Error: LifeCounterPlayersDB request failed");
            }

            var players = new List<UsersGetLifeCounterManagerDetailsResponse_players>();

            foreach (var player in lifeCounterPlayersDB)
            {
                players.Add(new UsersGetLifeCounterManagerDetailsResponse_players
                {
                    PlayerId = player.Id,
                    PlayerName = player.PlayerName,
                    CurrentLifePoints = player.CurrentLifePoints,
                    IsDefeated = player.IsDefeated,
                });
            }

            return (new UsersGetLifeCounterManagerDetailsResponse
            {
                LifeCounterManagerName = lifeCounterManagerDB.LifeCounterManagerName,
                PlayersCount = players.Count,
                LifeCounterPlayers = players,
                PlayersStartingLifePoints = lifeCounterManagerDB.PlayersStartingLifePoints,
                FixedMaxLifePointsMode = lifeCounterManagerDB.FixedMaxLifePointsMode,
                PlayersMaxLifePoints = lifeCounterManagerDB.PlayersMaxLifePoints,
                AutoDefeatMode = lifeCounterManagerDB.AutoDefeatMode,
                AutoEndMode = lifeCounterManagerDB.AutoEndMode,
                IsFinished = lifeCounterManagerDB.IsFinished,

            }, "Life counter details fetched successfully");
        }
        private static (bool, string) GetLifeCounterManagerDetails_Validation(UsersGetLifeCounterManagerDetailsRequest? request)
        {
            if (request == null)
            {
                return (false, $"Error: request failed: {request}");
            }

            if (request.LifeCounterManagerId.HasValue == false || request.LifeCounterManagerId == null || request.LifeCounterManagerId < 1)
            {
                return (false, $"Error: requested LifeCounterManagerId failed: {request.LifeCounterManagerId}");
            }

            return (true, string.Empty);
        }

        //
        // 4º LIFE COUNTER PLAYERS    
        public async Task<(UsersGetLifeCounterPlayersDetailsResponse?, string)> GetLifeCounterPlayersDetails(UsersGetLifeCounterPlayersDetailsRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = GetLifeCounterPlayersDetails_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var lifeCounterPlayersDB = await this._daoDbContext
                .LifeCounterPlayers
                .Where(a => a.LifeCounterManagerId == request!.LifeCounterManagerId && 
                            a.LifeCounterManager!.UserId == userId && 
                            a.LifeCounterManager.IsFinished == false)
                .Select(a => new 
                { 
                    a.Id, 
                    a.PlayerName,
                    a.CurrentLifePoints,
                    a.MaxLifePoints,
                    a.FixedMaxLifePointsMode,
                    a.IsDefeated
                })
                .OrderBy(a => a.Id)
                .ToListAsync();

            if (lifeCounterPlayersDB == null)
            {
                return (null, $"Error: life counter manager request failed: {lifeCounterPlayersDB}");
            }

            var players = new List<UsersGetLifeCounterPlayersDetailsResponse_players>();
           
            foreach (var player in lifeCounterPlayersDB) {
                players.Add(new UsersGetLifeCounterPlayersDetailsResponse_players
                {
                    PlayerId = player.Id,
                    PlayerName = player.PlayerName,
                    PlayerMaxLifePoints = player.MaxLifePoints,
                    PlayerCurrentLifePoints = player.CurrentLifePoints,
                    IsMaxLifePointsFixed = player.FixedMaxLifePointsMode,
                    IsDefeated = player.IsDefeated
                });
             }

            return (new UsersGetLifeCounterPlayersDetailsResponse
            {
                LifeCounterPlayers = players
            }, "Life counter players details loaded successfully");
        }
        private static (bool, string) GetLifeCounterPlayersDetails_Validation(UsersGetLifeCounterPlayersDetailsRequest? request)
        {
            if (request == null)
            {
                return (false, $"Error: request is not null: {request}");
            }

            if(request.LifeCounterManagerId.HasValue == false || request.LifeCounterManagerId < 1)
            {
                return (false, $"Error: LifeCounterManagerId request failed: {request.LifeCounterManagerId}");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersGetLifeCounterPlayerDetailsResponse?, string)> GetLifeCounterPlayerDetails(UsersGetLifeCounterPlayerDetailsRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = GetLifeCounterPlayerDetails_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var lifeCounterPlayerDB = await this._daoDbContext
                .LifeCounterPlayers
                .FirstOrDefaultAsync(a => a.Id == request!.LifeCounterPlayerId &&
                            a.LifeCounterManager!.UserId == userId);              

            if (lifeCounterPlayerDB == null)
            {
                return (null, $"Error: lifeCounterPlayerDB request failed, lifeCounterPlayerDB == null: {lifeCounterPlayerDB}");
            }

 
            return (new UsersGetLifeCounterPlayerDetailsResponse
            {
                LifeCounterManagerId = lifeCounterPlayerDB.LifeCounterManagerId,
                LifeCounterPlayerName = lifeCounterPlayerDB.PlayerName,
                PlayerStartingLifePoints = lifeCounterPlayerDB.StartingLifePoints,
                PlayerCurrentLifePoints = lifeCounterPlayerDB.CurrentLifePoints,
                FixedMaxLifePointsMode = lifeCounterPlayerDB.FixedMaxLifePointsMode,
                PlayerMaxLifePoints = lifeCounterPlayerDB.MaxLifePoints,
                AutoDefeatMode = lifeCounterPlayerDB.AutoDefeatMode,
            }, "Life counter players details loaded successfully");
        }
        private static (bool, string) GetLifeCounterPlayerDetails_Validation(UsersGetLifeCounterPlayerDetailsRequest? request)
        {
            if (request == null)
            {
                return (false, $"Error: request is not null: {request}");
            }

            if (request.LifeCounterPlayerId.HasValue == false || request.LifeCounterPlayerId < 1)
            {
                return (false, $"Error: LifeCounterPlayerId request failed: {request.LifeCounterPlayerId}");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersIncreaseLifePointsResponse?, string)> IncreaseLifePoints(UsersIncreaseLifePointsRequest request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (requestIsValid, message) = IncreaseLifePoints_Validation(request);

            if (requestIsValid == false)
            {
                return (null, message);
            }

            var lifeCounterPlayerDB = await this._daoDbContext
                .LifeCounterPlayers
                .Include(a => a.LifeCounterManager)
                .Where(a => a.Id == request.LifeCounterPlayerId && 
                    a.LifeCounterManager!.UserId == userId)                   
                .FirstOrDefaultAsync();

            if (lifeCounterPlayerDB == null)
            {
                return (null, $"Error, invalid requested LifeCounterManager, returning: {lifeCounterPlayerDB}");
            }

            if (lifeCounterPlayerDB.LifeCounterManager!.IsFinished == true)
            {
                return (null, "Error: this life counter manager was already finished");
            }

            if (lifeCounterPlayerDB.IsDefeated == true) 
            {
                return (null, $"Error: {lifeCounterPlayerDB.PlayerName} has been already defeated");
            }
            
            int? updatedLifePoints = 0;
            var text = "";
          
            if (lifeCounterPlayerDB.FixedMaxLifePointsMode == true &&
                (lifeCounterPlayerDB.CurrentLifePoints == lifeCounterPlayerDB.MaxLifePoints ||
                lifeCounterPlayerDB.CurrentLifePoints + request.LifePointsToIncrease > lifeCounterPlayerDB.MaxLifePoints))
            {
                updatedLifePoints = lifeCounterPlayerDB.MaxLifePoints;

                text = $"{lifeCounterPlayerDB.PlayerName} life points updated successfully. Max life points reached: {updatedLifePoints}";
            }
            else
            {
                updatedLifePoints = lifeCounterPlayerDB.CurrentLifePoints + request.LifePointsToIncrease;
                text = $"{lifeCounterPlayerDB.PlayerName} life points updated successfully. Current life points: {updatedLifePoints}";
            }

            lifeCounterPlayerDB.CurrentLifePoints = updatedLifePoints;

            await this._daoDbContext.SaveChangesAsync();

            return (new UsersIncreaseLifePointsResponse { UpdatedLifePoints = updatedLifePoints }, text);
        }
        private static (bool, string) IncreaseLifePoints_Validation(UsersIncreaseLifePointsRequest request)
        {
            if (request == null)
            {
                return (false, $"Error: request failed: {request}");
            }
            
            if(request.LifeCounterPlayerId.HasValue == false || request.LifeCounterPlayerId < 1)
            {
                return (false, $"Error: requested LifeCounterPlayerId failed: {request.LifeCounterPlayerId}");
            }

            if (request.LifePointsToIncrease.HasValue == false || (request.LifePointsToIncrease != 1 && request.LifePointsToIncrease != 10))
            {
                return (false, $"Error: invalid increase amount request: {request.LifePointsToIncrease}");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersDecreaseLifePointsResponse?, string)> DecreaseLifePoints(UsersDecreaseLifePointsRequest request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (requestIsValid, message) = DecreaseLifePoints_Validation(request);

            if (requestIsValid == false)
            {
                return (null, message);
            }

            var lifeCounterPlayerDB = await this._daoDbContext
                .LifeCounterPlayers
                .Include(a => a.LifeCounterManager)
                .Where(a => a.Id == request.LifeCounterPlayerId &&
                    a.LifeCounterManager!.UserId == userId)
                .FirstOrDefaultAsync();

            if (lifeCounterPlayerDB == null)
            {
                return (null, $"Error, invalid requested LifeCounterManager, returning: {lifeCounterPlayerDB}");
            }

            if (lifeCounterPlayerDB.LifeCounterManager!.IsFinished == true)
            {
                return (null, "Error: this life counter manager was already finished");
            }

            if (lifeCounterPlayerDB.AutoDefeatMode == true && lifeCounterPlayerDB.IsDefeated == true)
            {
                return (null, $"Error: {lifeCounterPlayerDB.PlayerName} has been already defeated");
            }            

            var updatedLifePoints = lifeCounterPlayerDB.CurrentLifePoints - request.LifePointsToDecrease;

            lifeCounterPlayerDB.CurrentLifePoints = updatedLifePoints;

            var text = $"{lifeCounterPlayerDB.PlayerName} life points updated successfully. Current life points: {updatedLifePoints}";

            if (lifeCounterPlayerDB.CurrentLifePoints <= 0 && lifeCounterPlayerDB.AutoDefeatMode == true)
            {
                lifeCounterPlayerDB.CurrentLifePoints = 0; 
                lifeCounterPlayerDB.IsDefeated = true;

                text = $"{lifeCounterPlayerDB.PlayerName} has been defeated!";
            }

            await this._daoDbContext.SaveChangesAsync();
            
            return (new UsersDecreaseLifePointsResponse { UpdatedLifePoints = updatedLifePoints }, text);
        }
        private static (bool, string) DecreaseLifePoints_Validation(UsersDecreaseLifePointsRequest request)
        {
            if (request == null)
            {
                return (false, $"Error: request failed: {request}");
            }

            if (request.LifeCounterPlayerId.HasValue == false || request.LifeCounterPlayerId < 1)
            {
                return (false, $"Error: requested LifeCounterPlayerId failed: {request.LifeCounterPlayerId}");
            }

            if (request.LifePointsToDecrease.HasValue == false || (request.LifePointsToDecrease != 1 && request.LifePointsToDecrease != 10))
            {
                return (false, $"Error: invalid decrease amount request: {request.LifePointsToDecrease}");
            }

            return (true, string.Empty);
        }

        public async Task<(UsersGetPlayersCountResponse?, string)> GetPlayersCount(UsersGetPlayersCountRequest request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (requestIsValid, message) = GetPlayersCount_Validation(request);

            if (requestIsValid == false)
            {
                return (null, message);
            }

            var playersCountDB = await this._daoDbContext
                .LifeCounterManagers               
                .Where(a => a.UserId == userId &&
                           a.Id == request.LifeCounterManagerId)
                .Select(a => a.PlayersCount)
                .FirstOrDefaultAsync();

            if (playersCountDB == null || playersCountDB < 0)
            {
                return (null, $"Error, PlayersCount request failed, returning: {playersCountDB}");
            }
    
            return (new UsersGetPlayersCountResponse { PlayersCount = playersCountDB }, "Players count fetched successfully)");
        }
        private static (bool, string) GetPlayersCount_Validation(UsersGetPlayersCountRequest request)
        {
            if (request == null)
            {
                return (false, $"Error: request failed: {request}");
            }

            if (request.LifeCounterManagerId.HasValue == false || request.LifeCounterManagerId < 1)
            {
                return (false, $"Error: requested LifeCounterPlayerId failed: {request.LifeCounterManagerId}");
            }        

            return (true, string.Empty);
        }



        public async Task<(UsersChangePlayerNameResponse?, string)> ChangePlayerName(UsersChangePlayerNameRequest request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (requestIsValid, message) = ChangePlayerName_Validation(request);

            if (requestIsValid == false)
            {
                return (null, message);
            }

            var lifeCounterPlayerDB_isValid = await this._daoDbContext
                .LifeCounterPlayers
                .AnyAsync(a => a.LifeCounterManager!.UserId == userId && a.Id == request.LifeCounterPlayerId);                         

            if (lifeCounterPlayerDB_isValid == false)
            {
                return (null, $"Error, invalid requested LifeCounterManager, returning: {lifeCounterPlayerDB_isValid}");
            }

            var isNewNameValid = await this._daoDbContext
                .LifeCounterPlayers
                .AnyAsync(a =>
                     a.PlayerName!.ToLower().Trim() == request.PlayerNewName!.ToLower().Trim());

            if(isNewNameValid == true)
            {
                return (null, "Error: requested name is already in use!");
            }

            await this._daoDbContext
                .LifeCounterPlayers
                .Where(a => a.LifeCounterManager!.UserId == userId && a.Id == request.LifeCounterPlayerId)
                .ExecuteUpdateAsync(a => a.SetProperty(b => b.PlayerName, request.PlayerNewName));

            return (new UsersChangePlayerNameResponse { }, "Life counter player name changed successfully.");
        }
        private static (bool, string) ChangePlayerName_Validation(UsersChangePlayerNameRequest request)
        {
            if (request == null)
            {
                return (false, $"Error: request failed: {request}");
            }

            if (request.LifeCounterPlayerId.HasValue == false ||
                (request.LifeCounterPlayerId.HasValue == true && request.LifeCounterPlayerId.Value < 1))
            {
                return (false, $"Error: requested LifeCounterPlayerId failed: request.LifeCounterPlayerId == {request.LifeCounterPlayerId}");

            }

            if (String.IsNullOrWhiteSpace(request.PlayerNewName) == true)
            {
                return (false, $"Error: requested PlayerNewName failed: String.IsNullOrWhiteSpace(request.PlayerNewName) == true? {String.IsNullOrWhiteSpace(request.PlayerNewName) == true}");
            }

            return (true, string.Empty);
        }

        public async Task<(UsersDeleteLifeCounterPlayerResponse?, string)> DeleteLifeCounterPlayer(UsersDeleteLifeCounterPlayerRequest request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (requestIsValid, message) = DeleteLifeCounterPlayer_Validation(request);

            if (requestIsValid == false)
            {
                return (null, message);
            }

            var lifeCounterPlayerDB = await this._daoDbContext
                .LifeCounterPlayers
                .Include(a => a.LifeCounterManager)
                .FirstOrDefaultAsync(a => a.LifeCounterManager!.UserId == userId && a.Id == request.LifeCounterPlayerId);

            if (lifeCounterPlayerDB == null)
            {
                return (null, $"Error, invalid requested LifeCounterManager, returning: {lifeCounterPlayerDB}");
            }

            
            var lifeCounterManagerDB = lifeCounterPlayerDB.LifeCounterManager;

            if (lifeCounterManagerDB == null)
            {
                return (null, $"Error, LifeCounterManager request failed, returning: {lifeCounterManagerDB}");
            }
           
            if( lifeCounterManagerDB.PlayersCount <= 1)
            {
                return (null, "Error: a life counter manager must host at least one player");
            }
            
            this._daoDbContext.LifeCounterPlayers.Remove(lifeCounterPlayerDB);

            lifeCounterManagerDB.PlayersCount--;

            await this._daoDbContext.SaveChangesAsync();

            return (new UsersDeleteLifeCounterPlayerResponse { }, "Life counter player name changed successfully.");
        }
        private static (bool, string) DeleteLifeCounterPlayer_Validation(UsersDeleteLifeCounterPlayerRequest request)
        {
            if (request == null)
            {
                return (false, $"Error: request failed: {request}");
            }

            if (request.LifeCounterPlayerId.HasValue == false ||
                (request.LifeCounterPlayerId.HasValue == true && request.LifeCounterPlayerId.Value < 1))
            {
                return (false, $"Error: requested LifeCounterPlayerId failed: request.LifeCounterPlayerId == {request.LifeCounterPlayerId}");

            }           

            return (true, string.Empty);
        }


        //
        //--* end of LIFE COUNTERS *--//
    }
}