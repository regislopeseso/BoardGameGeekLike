using System.Security.Claims;
using System.Text.RegularExpressions;
using BoardGameGeekLike.Models;
using BoardGameGeekLike.Models.Dtos.Request;
using BoardGameGeekLike.Models.Dtos.Response;
using BoardGameGeekLike.Models.Entities;
using BoardGameGeekLike.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;


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

                return (null, $"Board game rate deleted successfully");
            }
                 
            var newAverageRating = (oldAvgRating * ratingsCount - ratingDB.Rate) / (ratingsCount - 1); 

            boardgameDB.AverageRating = newAverageRating;
            boardgameDB.RatingsCount = ratingsCount - 1;    

            this._daoDbContext.Ratings.Remove(ratingDB);
            
            await this._daoDbContext.SaveChangesAsync();

            return (null, $"Board game rate edited successfully, its new average rating is: {newAverageRating:F1}");
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

    }
}