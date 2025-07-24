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
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;


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

            return (true, string.Empty);
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

            return (true, string.Empty);
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

            return (true, string.Empty);
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

            return (true, string.Empty);
        }


        public async Task<(UsersExportUserDataResponse?, string)> ExportUserData(UsersExportUserDataRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = ExportUserData_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var sb = new StringBuilder();

            var errorsReport = new List<string>();         
            

            //-*
            // User details table title
            sb.AppendLine("Errors Report;TABLE #1: USER DETAILS");

            // User details table header
            sb.AppendLine(";Name;Email;BirthDate;Gender;Sign Up Date");

            var userDB = await this._daoDbContext
                .Users
                .Include(a => a.LifeCounterTemplates)
                .Include(a => a.LifeCounterManagers!)
                .ThenInclude(a => a.LifeCounterPlayers)
                .FirstOrDefaultAsync(a => a.Id == userId && a.IsDummy == false && a.IsDeleted == false);

            if (userDB == null)
            {
                return (null, "Error: user not found");
            }
            var userDetails_data = ExportUserData_Validate_ProfileDetails(userDB, errorsReport);

            // User details table row (data)
            sb.AppendLine(userDetails_data);
            //
            //-*

            
            // Blank line between tables
            sb.AppendLine("");

            
            //-*
            // Life Counter Templates table title
            sb.AppendLine(";TABLE #2: LIFE COUNTER TEMPLATES");
            
            // Life Counter Templates table header
            sb.AppendLine(";Life Counter Template Name;Players Starting Life Points;Players Count;Fixed Max Life Points Mode; Players Max Life Points;Auto Defeat Mode;Auto End Mode;Life Counter Managers Count");                

            var lifeCounterTemplatesDB = userDB.LifeCounterTemplates;            
            
            if (lifeCounterTemplatesDB != null && lifeCounterTemplatesDB.Count > 0)
            {
                foreach (var template in lifeCounterTemplatesDB)
                {
                    // Clear the errors report for the next table
                    errorsReport = new List<string>();                 

                    var userLifeCouterTemplate_data = ExportUserData_Validate_LifeCounterTemplates(template, errorsReport);

                    sb.AppendLine(userLifeCouterTemplate_data);
                }
            }
            //
            //*-


            // Blank line between tables
            sb.AppendLine("");


            //-*
            // Life Counter Managers table title
            sb.AppendLine(";TABLE #3: LIFE COUNTER MANAGERS");

            // Life Counter Managers table header
            sb.AppendLine(";Life Counter Template Name; Life Counter Manager Name;Players Starting Life Points;Players Count;First Player Index;Fixed Max Life Points Mode;Players Max Life Points;Auto Defeat Mode;Auto End Mode;Starting Time Mark;Ending Time Mark;Duration (minutes);Is Finished");

            var lifeCounterManagersDB = userDB.LifeCounterManagers;
            
            if (lifeCounterManagersDB != null && lifeCounterManagersDB.Count > 0)
            {          
                foreach (var manager in lifeCounterManagersDB)
                {
                    // Clear the errors report for the next table
                    errorsReport = new List<string>();

                    var templateId = manager.LifeCounterTemplateId;
                    if(templateId == null || templateId < 1)
                    {
                        return (null, "Error: Life Counter Template Id is missing or invalid");
                    }

                    var templateName = lifeCounterTemplatesDB!.Where(a => a.Id == manager.LifeCounterTemplateId).Select(a => a.LifeCounterTemplateName).FirstOrDefault();
                    if(string.IsNullOrWhiteSpace(templateName) == true)
                    {
                        return (null, "Error: Life Counter Template Name is missing or invalid");
                    }
                    
                    var userLifeCouterManager_data = ExportUserData_Validate_LifeCounterManagers(manager, templateName, userDB.SignUpDate, errorsReport);

                    sb.AppendLine(userLifeCouterManager_data);
                }
            }
            //
            //*-


            // Blank line between tables
            sb.AppendLine("");


            //-*
            // Life Counter Players table title
            sb.AppendLine(";TABLE #4: LIFE COUNTER PLAYERS");

            // Life Counter Players table header
            sb.AppendLine(";Life Counter Manager Name;Player Name;Starting Life Points;Current Life Points;Fixed Max Life Points Mode;Max Life Points;Auto Defeat Mode;Is Defeated");
            
            var lifeCounterPlayersDB = userDB.LifeCounterManagers!.SelectMany(a => a.LifeCounterPlayers!).ToList();
            
            if(lifeCounterPlayersDB != null && lifeCounterPlayersDB.Count > 0)
            {
                foreach (var player in lifeCounterPlayersDB)
                {
                    // Clear the errors report for the next table
                    errorsReport = new List<string>();

                    var managerId = player.LifeCounterManagerId;
                    if (managerId == null || managerId < 1)
                    {
                        return (null, "Error: Life Counter Manager Id is missing or invalid");
                    }

                    var managerName = lifeCounterManagersDB!.Where(a => a.Id == managerId).Select(a => a.LifeCounterManagerName).FirstOrDefault();
                    if (string.IsNullOrWhiteSpace(managerName) == true)
                    {
                        return (null, "Error: Life Counter Manager Name is missing or invalid");
                    }

                    var userLifeCouterPlayer_data = ExportUserData_Validate_LifeCounterPlayers(player, managerName, errorsReport);

                    sb.AppendLine(userLifeCouterPlayer_data);
                }
            }
            //
            //*-

            
            // Blank line between tables
            sb.AppendLine("");


            //-*
            // Board Games Sessions table title
            sb.AppendLine(";TABLE #5: BOARD GAME SESSIONS");

            // Logged Board Game Sessions table header
            sb.AppendLine(";Board Game Name;Date;Players Count;Duration (minutes);Is Deleted");
            
            var sessionsDB = await this._daoDbContext
                .Sessions
                .Include(a => a.BoardGame)
                .Where(a => a.UserId == userId)
                .ToListAsync();          
            
            var boardGamesDB = sessionsDB.Select(a => a.BoardGame).ToList();            
            
            if (boardGamesDB != null && (sessionsDB != null && sessionsDB.Count > 0))
            {
                foreach (var session in sessionsDB)
                {
                    // Clear the errors report for the next table
                    errorsReport = new List<string>();

                    var boardGamePlayed = boardGamesDB.FirstOrDefault(a => a.Id == session.BoardGameId);
                    var boardGameName = boardGamePlayed!.Name; 
                    
                    if(string.IsNullOrWhiteSpace(boardGameName) == true)
                    {
                        return (null, "Error: Board Game Name is missing or invalid");
                    }
              
                    var userBoardGameSessions_data = ExportUserData_Validate_BoardGameSessions(session, boardGameName, userDB.SignUpDate, errorsReport);

                    sb.AppendLine(userBoardGameSessions_data);
                }
            }
            //
            //*-

           
            // Blank line between tables            
            sb.AppendLine("");


            //-*
            // Board Games Ratings table title
            sb.AppendLine(";TABLE #6: BOARD GAME RATINGS");

            // Rated Board Games table header
            sb.AppendLine(";Board Game Name;Rate");
           
            var ratingsDB = await this._daoDbContext
                .Ratings
                .Where(a => a.UserId == userId)
                .ToListAsync();

            if (boardGamesDB != null && (ratingsDB != null && ratingsDB.Count > 0))
            {
                foreach (var rating in ratingsDB)
                {
                    var boardGamePlayed = boardGamesDB.FirstOrDefault(a => a.Id == rating.BoardGameId);
                    var boardGameName = boardGamePlayed!.Name;

                    if(string.IsNullOrWhiteSpace(boardGameName) == true)
                    {
                        return (null, "Error: Board Game Name is missing or invalid");
                    }

                    var userBoardGameRatings_data = ExportUserData_Validate_BoardGameRatings(rating, boardGameName, errorsReport);

                    sb.AppendLine(userBoardGameRatings_data);   
                }
            }
            //
            //*-


            //-*
            // Blank line between at the end to follow the logic for importing data
            sb.AppendLine("");
            //
            //*-

            sb = ExportUserData_Validate_ErrorsReportColumn(sb);

            // Encode CSV with UTF-8 BOM to avoid Excel encoding issues
            var csvString = sb.ToString();
            var bom = Encoding.UTF8.GetPreamble();
            var bytes = bom.Concat(Encoding.UTF8.GetBytes(csvString)).ToArray();
            var base64 = Convert.ToBase64String(bytes);

            var response = new UsersExportUserDataResponse
            {
                FileName = "bgg_like_user_data.csv",
                Base64Data = base64,
                ContentType = "text/csv"
            };

            return (response, string.Empty);
        }
        private static (bool, string) ExportUserData_Validation(UsersExportUserDataRequest? request)
        {
           if(request != null)
           {
               return (false, "Request must be null!");
           }

            return (true, string.Empty);
        }
        private static string ExportUserData_Validate_ProfileDetails(User? userDB, List<string> errorsReport) 
        {
            var userName = userDB.Name;
            if (string.IsNullOrWhiteSpace(userName) == true)
            {
                errorsReport.Add("#Error: user name is missing!...");
            }
            else
            {
                userName = userName.Trim().ToUpper();

                if (userName.Length > 30)
                {
                    errorsReport.Add("#Error: user name cannot be longer than 30 characters!...");
                }

                var forbiddenCharacters = @"['"";,]";
                bool isUserNameInvalid = Regex.IsMatch(userName, forbiddenCharacters);

                if (isUserNameInvalid == true)
                {
                    errorsReport.Add("Error: user name may contain " +
                     "neither '(single quotes) nor  \"(double quotes) nor ,(commas) nor ;(semi-colons)...");
                }
            }

            var userEmail = userDB.Email;
            if (string.IsNullOrWhiteSpace(userEmail) == true)
            {
                errorsReport.Add("#Error: user email is missing!...");
            }
            else
            {
                userEmail = userEmail.Trim().ToUpper();

                if (userEmail.Length > 50)
                {
                    errorsReport.Add("#Error: user email cannot be longer than 50 characters!...");
                }

                var invalidEmailFormat = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                bool isUserEmailValid = Regex.IsMatch(userEmail, invalidEmailFormat);

                if (isUserEmailValid == false)
                {
                    errorsReport.Add("#Error: user email format is invalid!...");
                }
            }

            var userBirthDate_DateOnly = userDB.BirthDate;
            if (userBirthDate_DateOnly == null)
            {
                errorsReport.Add("#Error: user birth date is missing!...");
            }
            else
            {
                var today = DateOnly.FromDateTime(DateTime.Today);

                int age = today.Year - userBirthDate_DateOnly.Year;
                if (userBirthDate_DateOnly > today.AddYears(-age)) age--; // adjust if birthday hasn't occurred yet this year

                if (age < 12 || age > 90)
                {
                    errorsReport.Add("#Error: user age must be between 12 and 90 years!...");
                }
            }
            var userBirthDate = userBirthDate_DateOnly == null ? "" : userBirthDate_DateOnly.ToString("yyyy-MM-dd");

            var userGender = userDB.Gender;
            if (userGender == null)
            {
                errorsReport.Add("#Error: user gender is missing!...");
            }
            if (Enum.IsDefined(userGender) == false)
            {
                errorsReport.Add("#Error: user gender must be either male (0) or female (1)!...");
            }

            var userSignUpDate_DateOnly = userDB.SignUpDate;
            if (userSignUpDate_DateOnly == null)
            {
                errorsReport.Add("#Error: user sign up date is missing!...");
            }
            var userSignUpDate = userSignUpDate_DateOnly.ToString("yyyy-MM-dd");

            if (errorsReport == null || errorsReport.Count == 0)
            {
                errorsReport.Add("");
            }

            return $"{string.Join(" | ", errorsReport)};\"{userName}\";{userEmail};{userBirthDate};{userGender};{userSignUpDate}";
        }
        private static string ExportUserData_Validate_LifeCounterTemplates(LifeCounterTemplate? template, List<string> errorsReport)
        {                              
            var templateName = template.LifeCounterTemplateName;
            if (string.IsNullOrWhiteSpace(templateName) == true)
            {
                errorsReport.Add("#Error: Life Counter Template Name is missing!...");
            }
            else
            {
                templateName = templateName.Trim().ToUpper();
                if (templateName.Length > 30)
                {
                    errorsReport.Add("#Error: Life Counter Template Name cannot be longer than 30 characters!...");
                }

                var forbiddenCharacters = @"['"";,]";

                bool isTemplateNameInvalid = Regex.IsMatch(templateName, forbiddenCharacters);

                if (isTemplateNameInvalid == true)
                {
                    errorsReport.Add("Error: Life Counter Template Name may contain " +
                        "neither '(single quotes) nor  \"(double quotes) nor ,(commas) nor ;(semi-colons)...");
                }
            }

            var templatePlayersStartingLifePoints = template.PlayersStartingLifePoints;
            if (templatePlayersStartingLifePoints == null || templatePlayersStartingLifePoints < 1)
            {
                errorsReport.Add("#Error: Players Starting Life Points must be greater than 0!...");
            }

            var templatePlayersCount = template.PlayersCount;
            if (templatePlayersCount == null || templatePlayersCount < 1)
            {
                errorsReport.Add("#Error: Players Count must be greater than 0!...");
            }

            var templateFixedMaxLifePointsMode = template.FixedMaxLifePointsMode;
            if (templateFixedMaxLifePointsMode == null)
            {
                errorsReport.Add("#Error: Fixed Max Life Points Mode is missing!...");
            }

            var templatePlayersMaxLifePoints = template.PlayersMaxLifePoints;
            if (templatePlayersMaxLifePoints != null && templateFixedMaxLifePointsMode == false)
            {
                errorsReport.Add("#Error: If Fixed Max Life mode is FALSE then Max Life Points cell must be empty!...");
            }
            if (templatePlayersMaxLifePoints != null && templateFixedMaxLifePointsMode == true && templatePlayersMaxLifePoints >= templatePlayersStartingLifePoints)
            {
                errorsReport.Add("#Error: Max Life Points may not be greater than nor equal to Players Starting Life Points!...");
            }
            if (templatePlayersMaxLifePoints == null && templateFixedMaxLifePointsMode == true)
            {
                errorsReport.Add("#Error: If Fixed Max Life mode is TRUE then Max Life Points cell must contain a value!...");
            }

            var templateAutoDefeatMode = template.AutoDefeatMode;
            if (templateAutoDefeatMode == null)
            {
                errorsReport.Add("#Error: Auto Defeat Mode is missing!...");
            }

            var templateAutoEndMode = template.AutoEndMode;
            if (templateAutoEndMode == null)
            {
                errorsReport.Add("#Error: Auto End Mode is missing!...");
            }
            if (templateAutoEndMode == true && templateAutoDefeatMode == false)
            {
                errorsReport.Add("#Error: Auto End Mode cannot be TRUE if Auto Defeat Mode is FALSE!...");
            }

            var templateLifeCounterManagersCount = template.LifeCounterManagersCount;
            if (templateLifeCounterManagersCount == null || templateLifeCounterManagersCount < 0)
            {
                errorsReport.Add("#Error: Life Counter Managers Count must be greater than or equal to 0!...");
            }

            if (errorsReport == null || errorsReport.Count == 0)
            {
                errorsReport.Add("");
            }

            return $"{string.Join(" | ", errorsReport)};\"{templateName}\";{templatePlayersStartingLifePoints};{templatePlayersCount};{templateFixedMaxLifePointsMode};{templatePlayersMaxLifePoints};{templateAutoDefeatMode};{templateAutoEndMode}  ;{templateLifeCounterManagersCount}";           
        }
        private static string ExportUserData_Validate_LifeCounterManagers(LifeCounterManager? manager, string templateName, DateOnly signUpDate, List<string> errorsReport)
        {                           
            var managerName = manager.LifeCounterManagerName;
            if (string.IsNullOrWhiteSpace(managerName) == true)
            {
                errorsReport.Add("#Error: Life Counter Manager Name is missing!...");
            }
            else
            {
                managerName = managerName.Trim().ToUpper();
                if (managerName.Length > 30)
                {
                    errorsReport.Add("#Error: Life Counter Manager Name cannot be longer than 30 characters!...");
                }

                var forbiddenCharacters = @"['"";,]";

                bool isManagerNameInvalid = Regex.IsMatch(managerName, forbiddenCharacters);

                if (isManagerNameInvalid == true)
                {
                    errorsReport.Add("Error: Life Counter Manager Name may contain " +
                        "neither '(single quotes) nor  \"(double quotes) nor ,(commas) nor ;(semi-colons)...");
                }
            }

            var managerPlayerStartingLifePoints = manager.PlayersStartingLifePoints;
            if (managerPlayerStartingLifePoints == null || managerPlayerStartingLifePoints < 1)
            {
                errorsReport.Add("#Error: Players Starting Life Points must be greater than 0!...");
            }

            var managerPlayersCount = manager.PlayersCount;
            if (managerPlayersCount == null || managerPlayersCount < 1)
            {
                errorsReport.Add("#Error: Players Count must be greater than 0!...");
            }

            var managerFirstPlayerIndex = manager.FirstPlayerIndex;
            if (managerFirstPlayerIndex == null || managerFirstPlayerIndex < 0 || managerFirstPlayerIndex > 5)
            {
                errorsReport.Add("#Error: First Player Index must be a value between 0 and 5!...");
            }

            var managerFixedMaxLifePointsMode = manager.FixedMaxLifePointsMode;
            if (managerFixedMaxLifePointsMode == null)
            {
                errorsReport.Add("#Error: Fixed Max Life Points Mode is missing!...");
            }

            var managerPlayersMaxLifePoints = manager.PlayersMaxLifePoints;
            if (managerPlayersMaxLifePoints != null && managerFixedMaxLifePointsMode == false)
            {
                errorsReport.Add("#Error: If Fixed Max Life mode is FALSE then Max Life Points cell must be empty!...");
            }
            if (managerPlayersMaxLifePoints != null && managerFixedMaxLifePointsMode == true && managerPlayersMaxLifePoints <= managerPlayerStartingLifePoints)
            {
                errorsReport.Add("#Error: Max Life Points may NOT be greater than NOR equal to Players Starting Life Points!...");
            }
            if (managerPlayersMaxLifePoints == null && managerFixedMaxLifePointsMode == true)
            {
                errorsReport.Add("#Error: If Fixed Max Life mode is TRUE then Max Life Points cell must contain a value!...");
            }

            var managerAutoDefeatMode = manager.AutoDefeatMode;
            if (managerAutoDefeatMode == null)
            {
                errorsReport.Add("#Error: Auto Defeat Mode is missing!...");
            }

            var managerAutoEndMode = manager.AutoEndMode;
            if (managerAutoEndMode == null)
            {
                errorsReport.Add("#Error: Auto End Mode is missing!...");
            }
            if (managerAutoEndMode == true && managerAutoEndMode == false)
            {
                errorsReport.Add("#Error: Auto End Mode cannot be TRUE if Auto Defeat Mode is FALSE!...");
            }

            var managerStartingTime_Ticks = manager.StartingTime;
            DateTime managerStartingTime_DateTime = new DateTime(managerStartingTime_Ticks!.Value);
            DateOnly managerStartingTime_DateOnly = DateOnly.FromDateTime(managerStartingTime_DateTime);
            if (managerStartingTime_Ticks == null || managerStartingTime_DateOnly < signUpDate)
            {
                errorsReport.Add("#Error: Invalid Starting Time mark!...");
            }
            var managerStartingTime = managerStartingTime_DateTime.ToString("yyyy-MM-dd HH:mm:ss");

            var managerEndingTime_Ticks = manager.EndingTime;
            var managerEndingTime = "";
            if (managerEndingTime_Ticks != null && managerEndingTime_Ticks > 0)
            {
                DateTime managerEndingTime_DateTime = new DateTime(managerEndingTime_Ticks!.Value);
                DateTime today_and_now = DateTime.Today;
                DateOnly managerEndingTime_DateOnly = DateOnly.FromDateTime(managerEndingTime_DateTime);

                if (managerStartingTime_Ticks != null && managerEndingTime_Ticks < 0 || managerEndingTime_DateTime > managerStartingTime_DateTime || managerEndingTime_DateTime > today_and_now)
                {
                    errorsReport.Add("#Error: Invalid Ending Time mark!...");
                }
                managerEndingTime = managerEndingTime_Ticks != null && managerEndingTime_Ticks > 0 ? managerEndingTime_DateTime.ToString("yyyy-MM-dd HH:mm:ss") : "";
            }

            var managerDuration_minutes = manager.Duration_minutes;
            if (managerDuration_minutes != null && (managerEndingTime_Ticks == null || managerEndingTime == ""))
            {
                errorsReport.Add("#Error: If Ending Time cell is empty Duration must be empty also!...");
            }
            if (managerDuration_minutes == null && (managerEndingTime_Ticks != null || managerEndingTime_Ticks > 0))
            {
                errorsReport.Add("#Error: Duration must be greater than or equal to 0 minutes!...");
            }

            var managerIsFinished = manager.IsFinished;
            if (managerIsFinished == null)
            {
                errorsReport.Add("#Error: Is Finished data is missing!...");
            }
            if ((managerIsFinished != null && managerIsFinished == true) && (managerEndingTime_Ticks == null || managerDuration_minutes == null))
            {
                errorsReport.Add("#Error: If Ending Time cell is empty and/or Duration cell is also empty then Is Finished cell must be empty as well!...");
            }
            if ((managerIsFinished == null || managerIsFinished == false) && (managerEndingTime_Ticks != null || managerDuration_minutes != null))
            {
                errorsReport.Add("#Error: If neither Ending Time cell is empty NOR Duration cell then Is Finished cell may not be empty nor false!...");
            }

            if (errorsReport == null || errorsReport.Count == 0)
            {
                errorsReport.Add("");
            }

            return $"{string.Join(" | ", errorsReport)};\"{templateName}\";\"{managerName}\";{managerPlayerStartingLifePoints};{managerPlayersCount};{managerFirstPlayerIndex};{managerFixedMaxLifePointsMode};{managerPlayersMaxLifePoints};{managerAutoDefeatMode};{managerAutoEndMode};{managerStartingTime};{managerEndingTime};{managerDuration_minutes};{managerIsFinished}";          
        }
        private static string ExportUserData_Validate_LifeCounterPlayers(LifeCounterPlayer? player, string managerName, List<string> errorsReport)
        {          
            var lifeCounterPlayer_Name = player.PlayerName;
            if (string.IsNullOrWhiteSpace(lifeCounterPlayer_Name) == true)
            {
                errorsReport.Add("#Error: Life Counter Template Name is missing!...");
            }
            else
            {
                lifeCounterPlayer_Name = lifeCounterPlayer_Name.Trim().ToUpper();
                if (lifeCounterPlayer_Name.Length > 30)
                {
                    errorsReport.Add("#Error: Life Counter Template Name cannot be longer than 30 characters!...");
                }

                var forbiddenCharacters = @"['"";,]";

                bool isPlayerNameInvalid = Regex.IsMatch(lifeCounterPlayer_Name, forbiddenCharacters);

                if (isPlayerNameInvalid == true)
                {
                    errorsReport.Add("Error: Life Counter Player Name may contain " +
                        "neither '(single quotes) nor  \"(double quotes) nor ,(commas) nor ;(semi-colons)");
                }
            }

            var lifeCounterPlayer_StartingLifePoints = player.StartingLifePoints;
            if (lifeCounterPlayer_StartingLifePoints == null || lifeCounterPlayer_StartingLifePoints < 1)
            {
                errorsReport.Add("#Error: Player Starting Life Points must be greater than 0!");
            }

            var lifeCounterPlayer_CurrentLifePoints = player.CurrentLifePoints;
            if (lifeCounterPlayer_CurrentLifePoints == null)
            {
                errorsReport.Add("#Error: Player Current Life Points is missing!");
            }

            var lifeCounterPlayer_FixedMaxLifePointsMode = player.FixedMaxLifePointsMode;
            if (lifeCounterPlayer_FixedMaxLifePointsMode == null)
            {
                errorsReport.Add("#Error: Fixed Max Life Points Mode is missing!");
            }

            var lifeCounterPlayer_MaxLifePoints = player.MaxLifePoints;
            if (lifeCounterPlayer_MaxLifePoints != null && lifeCounterPlayer_FixedMaxLifePointsMode == false)
            {
                errorsReport.Add("#Error: If Fixed Max Life mode is FALSE then Max Life Points cell must be empty!");
            }
            if (lifeCounterPlayer_MaxLifePoints != null && lifeCounterPlayer_FixedMaxLifePointsMode == true && lifeCounterPlayer_MaxLifePoints <= lifeCounterPlayer_MaxLifePoints)
            {
                errorsReport.Add("#Error: Max Life Points may NOT be lesser than NOR equal to Starting Life Points!");
            }
            if (lifeCounterPlayer_MaxLifePoints == null && lifeCounterPlayer_FixedMaxLifePointsMode == true)
            {
                errorsReport.Add("#Error: If Fixed Max Life mode is TRUE then Max Life Points cell must contain a value!");
            }

            var lifeCounterPlayer_AutoDefeatMode = player.AutoDefeatMode;
            if (lifeCounterPlayer_AutoDefeatMode == null)
            {
                errorsReport.Add("#Error: Auto Defeat Mode is missing!");
            }
            if (lifeCounterPlayer_AutoDefeatMode != null && lifeCounterPlayer_AutoDefeatMode == true && lifeCounterPlayer_CurrentLifePoints < 0)
            {
                errorsReport.Add("#Error: If Auto Defeat Mode cell value is true then Current Life Points must be zero!");
            }

            var lifeCounterPlayer_IsDefeated = player.IsDefeated;
            if (lifeCounterPlayer_IsDefeated == null)
            {
                errorsReport.Add("#Error: Is Defeated data is missing!");
            }
            if (lifeCounterPlayer_IsDefeated == true && lifeCounterPlayer_AutoDefeatMode == false)
            {
                errorsReport.Add("#Error: If Auto Defeat Mode cell has the value false then Is Defeated cell value MUST be false!");
            }
            if (lifeCounterPlayer_IsDefeated == false && lifeCounterPlayer_AutoDefeatMode == true && lifeCounterPlayer_CurrentLifePoints <= 0)
            {
                errorsReport.Add("#Error: If Auto Defeat Mode cell has the value true and Current Life Points is less or equal to zero then Is Defeated cell value MUST be true!");
            }

            if (errorsReport == null || errorsReport.Count == 0)
            {
                errorsReport.Add("");
            }

            return $"{string.Join(" | ", errorsReport)};\"{managerName}\";\"{lifeCounterPlayer_Name}\";{lifeCounterPlayer_StartingLifePoints};{lifeCounterPlayer_CurrentLifePoints};{lifeCounterPlayer_FixedMaxLifePointsMode};{lifeCounterPlayer_MaxLifePoints};{lifeCounterPlayer_AutoDefeatMode};{lifeCounterPlayer_IsDefeated}";          
        }
        private static string ExportUserData_Validate_BoardGameSessions(Session? session, string boardGameName, DateOnly signUpDate, List<string> errorsReport)
        {

            var date = session.Date;
            var today = DateOnly.FromDateTime(DateTime.Today);
            if (date == null || date < signUpDate || date > today)
            {
                errorsReport.Add("#Error: Date is missing or invalid!...");
            }            
            
            var playersCount = session.PlayersCount;
            if(playersCount == null || playersCount < 1 || playersCount > 6)
            {
                errorsReport.Add("#Error: Players Count must be a value between 1 nad 6!...");
            }         

            var duration = session.Duration_minutes;
            if(duration == null || duration < 0)
            {
                errorsReport.Add("#Error: Duration must be greater than or equal to 0 minutes!...");
            }
            if(duration > 1440)
            {
                errorsReport.Add("#Error: Duration must be less than or equal to 1440 minutes (24 hours)!...");
            }

            var isDeleted = session.IsDeleted;
            if(isDeleted == null)
            {
                errorsReport.Add("#Error: Is Deleted data is missing!...");
            }

            return $"{string.Join(" | ", errorsReport)};\"{boardGameName}\";{date};{playersCount};{duration};{isDeleted}";
        }
        private static string ExportUserData_Validate_BoardGameRatings(Rating? rating, string boardGameName, List<string> errorsReport)
        {
            var rate = rating.Rate;
            if(rate == null || rate < 0 || rate > 5)
            {
                errorsReport.Add("#Error: Rate must be between 0 and 5!...");
            }

            return $"{string.Join(" | ", errorsReport)};\"{boardGameName}\";{rate}";
        }
        private static StringBuilder ExportUserData_Validate_ErrorsReportColumn(StringBuilder sb)
        {
            var original = sb.ToString();

            // Split into lines
            var lines = original.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();

            // Check if all non-header, non-empty lines have an empty first column
            bool removeFirstColumn = lines
                .Skip(1) // Skip header ("Errors Report;TABLE #...")
                .Where(line => !string.IsNullOrWhiteSpace(line)) // Skip empty lines
                .All(line => line.StartsWith(";")); // First column is empty if line starts with ;

            // If we need to remove, rebuild the lines without the first column
            if (removeFirstColumn)
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    var line = lines[i];

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var parts = line.Split(';');
                    if (parts.Length > 1)
                    {
                        lines[i] = string.Join(";", parts.Skip(1));
                    }
                    else
                    {
                        lines[i] = ""; // Line had only one column
                    }
                }
            }

            // Rebuild the new CSV into a StringBuilder
            var cleaned = new StringBuilder();
            foreach (var line in lines)
            {
                cleaned.AppendLine(line);
            }

            return cleaned;
        }


        public async Task<(UsersImportUserDataResponse?, string)> ImportUserData(UsersImportUserDataRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return (null, "Error: User is not authenticated");

            var userDB = await this._daoDbContext
                .Users
                .Include(a => a.LifeCounterTemplates)
                .Include(a => a.LifeCounterManagers!)
                .ThenInclude(a => a.LifeCounterPlayers)
                .FirstOrDefaultAsync(a => a.Id == userId);

            if (userDB == null)
                return (null, "Error: User was not found");

            var (isValid, message) = ImportUserData_Validation(request);
            
            if (!isValid)
                return (null, message);

            byte[] csvBytes;
            try
            {
                csvBytes = Convert.FromBase64String(request!.Base64CsvData);
            }
            catch
            {
                return (null, "Error: Invalid Base64 CSV data.");
            }

            var encoding = Encoding.UTF8;
            if (csvBytes.Length >= 3 && csvBytes[0] == 0xEF && csvBytes[1] == 0xBB && csvBytes[2] == 0xBF)
            {
                csvBytes = csvBytes[3..]; // Remove BOM
            }

            var sections = new Dictionary<int, List<string>>();
            int currentTable = -1;

            using (var reader = new StreamReader(new MemoryStream(csvBytes), encoding))
            {
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (line == null) continue;

                    if (line.StartsWith("TABLE #"))
                    {
                        var numberMatch = Regex.Match(line, @"TABLE\s+#(\d+)");
                        if (numberMatch.Success)
                        {
                            currentTable = int.Parse(numberMatch.Groups[1].Value);
                            sections[currentTable] = new List<string>();
                        }
                        continue;
                    }

                    if (currentTable != -1)
                    {
                        sections[currentTable].Add(line);
                    }
                }
            }

            //
            // Dealing with user'a Details
            if (!sections.TryGetValue(1, out var userDetailsSection) || userDetailsSection.Count < 2)
                return (null, "Error: Missing or invalid user section (Table #1)");            

            var importedUserDetails = ParseUser(userDetailsSection);

            userDB.Name = importedUserDetails.Name;
            userDB.SignUpDate = importedUserDetails.SignUpDate!.Value;
            userDB.BirthDate = importedUserDetails.BirthDate!.Value;
            userDB.Gender = importedUserDetails.Gender!.Value;
            userDB.IsDeleted = false;

            //
            // Dealing with the user'a Life Counter TEMPLATES
            if (!sections.TryGetValue(2, out var userLifeCounterTemplatesSection) || userLifeCounterTemplatesSection.Count < 2)
                return (null, "Error: Missing or invalid user section (Table #2)");

            var importedLifeCounterTemplates = new List<UsersImportUserDataResponse_userLifeCounterTemplate>();

            importedLifeCounterTemplates = ParseLifeCounterTemplates(userLifeCounterTemplatesSection);          
          
            if(importedLifeCounterTemplates != null || importedLifeCounterTemplates.Count > 0)
            {             

                foreach (var template in importedLifeCounterTemplates)
                {
                    if (userDB.LifeCounterTemplates!.Any(a => a.LifeCounterTemplateName!.ToLower().Trim() == template.LifeCounterTemplateName!.ToLower().Trim()))
                    {
                        continue; // Skip if template already exists
                    }

                    userDB.LifeCounterTemplates!.Add(new LifeCounterTemplate
                    {
                        LifeCounterTemplateName = template.LifeCounterTemplateName,
                        PlayersStartingLifePoints = template.PlayersStartingLifePoints,
                        PlayersCount = template.PlayersCount,
                        FixedMaxLifePointsMode =  template.FixedMaxLifePointsMode,
                        PlayersMaxLifePoints = template.PlayersMaxLifePoints,
                        AutoDefeatMode = template.AutoDefeatMode,
                        AutoEndMode = template.AutoEndMode,
                        LifeCounterManagersCount = template.LifeCounterManagersCount,
                        UserId = userId
                    });
                }                 
            }

            //
            // Dealing with the user'a Life Counter MANAGERS
            if (!sections.TryGetValue(3, out var userLifeCounterManagersSection) || userLifeCounterManagersSection.Count < 2)
                return (null, "Error: Missing or invalid user section (Table #3)");

            var importedLifeCounterManagers = new List<UsersImportUserDataResponse_userLifeCounterManager>();

            importedLifeCounterManagers = ParseLifeCounterManagers(userLifeCounterManagersSection);

            if (importedLifeCounterManagers != null || importedLifeCounterManagers!.Count > 0)
            {
                for(int i = 0; i < userDB.LifeCounterTemplates!.Count; i++)
                {
                    userDB.LifeCounterTemplates[i].LifeCounterManagers = new List<LifeCounterManager>();

                    foreach (var manager in importedLifeCounterManagers)
                    {
                        if (userDB.LifeCounterTemplates[i].LifeCounterTemplateName == manager.LifeCounterTemplateName)
                        {
                            userDB.LifeCounterTemplates[i].LifeCounterManagers!.Add(new LifeCounterManager
                            {
                                LifeCounterManagerName = manager.LifeCounterManagerName,
                                PlayersStartingLifePoints = manager.PlayersStartingLifePoints,
                                PlayersCount = manager.PlayersCount,
                                FirstPlayerIndex = manager.FirstPlayerIndex,
                                FixedMaxLifePointsMode = manager.FixedMaxLifePointsMode,
                                PlayersMaxLifePoints = manager.PlayersMaxLifePoints,
                                AutoDefeatMode = manager.AutoDefeatMode,
                                AutoEndMode = manager.AutoEndMode,
                                StartingTime = manager.StartingTime,
                                EndingTime = manager.EndingTime,
                                Duration_minutes = manager.Duration_minutes,
                                IsFinished = manager.IsFinished,
                                UserId = userId
                            });
                        }
                    }                     
                }
            }

            //
            // Dealing with the user'a Life Counter PLAYERS
            if (!sections.TryGetValue(4, out var userLifeCounterPlayersSection) || userLifeCounterPlayersSection.Count < 2)
                return (null, "Error: Missing or invalid user section (Table #4)");

            var importedLifeCounterPlayers = new List<UsersImportUserDataResponse_userLifeCounterPlayer>();

            importedLifeCounterPlayers = ParseLifeCounterPlayers(userLifeCounterPlayersSection);

            if (importedLifeCounterPlayers != null || importedLifeCounterPlayers!.Count > 0)
            {
                for (int i = 0; i < userDB.LifeCounterTemplates!.Count; i++)
                {

                    if (userDB.LifeCounterTemplates[i].LifeCounterManagers != null && userDB.LifeCounterTemplates[i].LifeCounterManagers!.Count > 0)
                    {
                        for (int j = 0; j < userDB.LifeCounterTemplates[i].LifeCounterManagers!.Count; j++)
                        {
                            userDB.LifeCounterTemplates[i].LifeCounterManagers![j].LifeCounterPlayers = new List<LifeCounterPlayer>();

                            foreach (var player in importedLifeCounterPlayers)
                            {
                                if (userDB.LifeCounterTemplates[i].LifeCounterManagers![j].LifeCounterManagerName == player.LifeCounterManagerName)
                                {
                                    userDB.LifeCounterTemplates[i].LifeCounterManagers![j].LifeCounterPlayers!.Add(new LifeCounterPlayer
                                    {
                                        PlayerName = player.PlayerName,
                                        StartingLifePoints = player.StartingLifePoints,
                                        CurrentLifePoints = player.CurrentLifePoints,
                                        FixedMaxLifePointsMode = player.FixedMaxLifePointsMode!.Value,
                                        MaxLifePoints = player.MaxLifePoints,
                                        AutoDefeatMode = player.AutoDefeatMode,
                                        IsDefeated = player.IsDefeated!.Value,
                                    });
                                }
                            }
                        }

                    }
                }
            }            
            
            //
            // Dealing with THE user'a BOARD GAME SESSIONS
            if (!sections.TryGetValue(5, out var userBoardGameSessions) || userBoardGameSessions.Count < 2)
                return (null, "Error: Missing or invalid user section (Table #5)");


            var importedBoarGameSessions = ParseBoardGameSessions(userBoardGameSessions);

            var boardGamesDB = await this._daoDbContext
                .BoardGames
                .AsNoTracking()
                .Include(a => a.Sessions)
                .ToListAsync();

            // Flatten the session list and filter by userId
            var user_sessionsDB = boardGamesDB
                .SelectMany(a => a.Sessions!)
                .Where(s => s.UserId == userId)
                .ToList();

            if (importedBoarGameSessions != null && importedBoarGameSessions.Count > 0)
            {
                foreach (var session in importedBoarGameSessions)
                {
                    var boardGame = boardGamesDB
                        .FirstOrDefault(bg => bg.Name!.ToLower().Trim() == session.BoardGameName!.ToLower().Trim());

                    if (boardGame == null)
                        continue;

                    var alreadyExists = user_sessionsDB.Any(
                        a => a.BoardGame!.Name == session.BoardGameName &&
                             a.Date == session.Date &&
                             a.PlayersCount == session.PlayersCount &&
                             a.Duration_minutes == session.Duration_minutes);

                    if (alreadyExists)
                        continue;

                    this._daoDbContext.Sessions.Add(new Session
                    {
                        UserId = userId,
                        BoardGameId = boardGame.Id,
                        Date = session.Date!.Value,
                        PlayersCount = session.PlayersCount!.Value,
                        Duration_minutes = session.Duration_minutes!.Value,
                        IsDeleted = session.IsDeleted!.Value
                    }); 
                }
            }

            //
            // Dealing with THE user'a BOARD GAME RATINGS
            if (!sections.TryGetValue(6, out var userBoardGameRatings) || userBoardGameRatings.Count < 2)
                return (null, "Error: Missing or invalid user section (Table #6)");

            var importedBoardGameRatings = new List<UsersImportUserDataResponse_userBoardGameRating>();

            importedBoardGameRatings = ParseBoardGameRatings(userBoardGameRatings);

            // Flatten the rating list and filter by userId
            var user_playedBoardGames_Names = importedBoarGameSessions!.Select(a => a.BoardGameName!.ToLower().Trim()).ToList();

            var user_playedBoardGames_Ids = boardGamesDB
                .Where(a => user_playedBoardGames_Names.Contains(a.Name!.ToLower().Trim()))
                .Select(a => a.Id)
                .ToList();

            var user_ratingsDB = await this._daoDbContext
                .Ratings
                .Where(r => r.UserId == userId && user_playedBoardGames_Ids.Contains(r.BoardGameId))
                .ToListAsync();

            if (importedBoardGameRatings != null && importedBoardGameRatings.Count > 0)
            {
                foreach (var rating in importedBoardGameRatings)
                {
                    var boardGame = boardGamesDB
                        .FirstOrDefault(bg => bg.Name!.ToLower().Trim() == rating.BoardGameName!.ToLower().Trim());

                    if (boardGame == null)
                        continue;

                    var alreadyExists = user_ratingsDB.Any(
                        a => a.BoardGame!.Name == rating.BoardGameName);

                    if (alreadyExists)
                        continue;

                    this._daoDbContext.Ratings.Add(new Rating
                    {
                        Rate = rating.Rate!.Value,
                        BoardGameId = boardGame.Id,
                        UserId = userId,                    
                    });
                }
            }

            await _daoDbContext.SaveChangesAsync();

            var response = new UsersImportUserDataResponse();
            return (response, "User data imported successfully."); ;
        }
        private static (bool, string) ImportUserData_Validation(UsersImportUserDataRequest? request)
        {
            if (request == null)
            {
                return (false, "Request is null!");
            }

            return (true, string.Empty);
        }
        private UsersImportUserDataResponse_userDetails ParseUser(List<string> lines)
        {
            if (lines == null || lines.Count < 2)
                throw new Exception("Invalid user section");

            var header = lines[0]; // skip or validate

            var data = lines[1].Split(';');

            if (data.Length < 4)
                throw new Exception("User data row is incomplete.");


            return new UsersImportUserDataResponse_userDetails
            {
                Name = data[0].Trim('"'),
                SignUpDate = DateOnly.Parse(data[1]),
                BirthDate = DateOnly.Parse(data[2]),
                Gender = Enum.Parse<Gender>(data[3].Trim('"'))
            };
        }
        private List<UsersImportUserDataResponse_userLifeCounterTemplate>? ParseLifeCounterTemplates(List<string>? lines)
        {             
            var importedLifeCounterTemplates = new List<UsersImportUserDataResponse_userLifeCounterTemplate>();

            // Skip header line at index 0
            for (int i = 1; i < lines.Count - 1; i++)
            {
                var line = lines[i];
                var data = line.Split(';');               

                importedLifeCounterTemplates.Add(new UsersImportUserDataResponse_userLifeCounterTemplate
                {
                    LifeCounterTemplateName = data[0].Trim('"'),
                    PlayersStartingLifePoints = int.Parse(data[1]),
                    PlayersCount = int.Parse(data[2]),
                    FixedMaxLifePointsMode = bool.Parse(data[3]),
                    PlayersMaxLifePoints = int.TryParse(data[4], out int playersMaxLifePoints) ? playersMaxLifePoints : null,
                    AutoDefeatMode = bool.Parse(data[5]),
                    AutoEndMode = bool.Parse(data[6]),
                    LifeCounterManagersCount = int.Parse(data[7])
                });
            }

            return importedLifeCounterTemplates;
        }
        private List<UsersImportUserDataResponse_userLifeCounterManager>? ParseLifeCounterManagers(List<string>? lines)
        {
            var importedLifeCounterManagers = new List<UsersImportUserDataResponse_userLifeCounterManager>();

            // Skip header line at index 0
            for (int i = 1; i < lines.Count - 1; i++)
            {
                var line = lines[i];
                var data = line.Split(';');

                importedLifeCounterManagers.Add(new UsersImportUserDataResponse_userLifeCounterManager
                {
                    LifeCounterTemplateName = data[0].Trim('"'),
                    LifeCounterManagerName = data[1].Trim('"'),
                    PlayersStartingLifePoints = int.Parse(data[2]),
                    PlayersCount = int.Parse(data[3]),
                    FirstPlayerIndex = int.Parse(data[4]),
                    FixedMaxLifePointsMode = bool.Parse(data[5]),
                    PlayersMaxLifePoints = int.TryParse(data[6], out int playersMaxLifePoints) ? playersMaxLifePoints : null,
                    AutoDefeatMode = bool.Parse(data[7]),
                    AutoEndMode = bool.Parse(data[8]),
                    StartingTime = long.Parse(data[9]),
                    EndingTime = long.TryParse(data[10], out long endingTime) ? endingTime : null,
                    Duration_minutes = double.TryParse(data[11], out double duration) ? duration : null,
                    IsFinished = bool.Parse(data[12]),
                });
            }

            return importedLifeCounterManagers;
        }
        private List<UsersImportUserDataResponse_userLifeCounterPlayer>? ParseLifeCounterPlayers(List<string>? lines)
        {
            var importedLifeCounterPlayers = new List<UsersImportUserDataResponse_userLifeCounterPlayer>();

            // Skip header line at index 0
            for (int i = 1; i < lines.Count - 1; i++)
            {
                var line = lines[i];
                var data = line.Split(';');

                importedLifeCounterPlayers.Add(new UsersImportUserDataResponse_userLifeCounterPlayer
                {                    
                    LifeCounterManagerName = data[0].Trim('"'),
                    PlayerName = data[1].Trim('"'),
                    StartingLifePoints = int.Parse(data[2]),
                    CurrentLifePoints = int.Parse(data[3]),
                    FixedMaxLifePointsMode = bool.Parse(data[4]),
                    MaxLifePoints = int.TryParse(data[5], out int maxLifePoints) ? maxLifePoints : null,
                    AutoDefeatMode = bool.Parse(data[6]),
                    IsDefeated = bool.Parse(data[7]),                 
                });
            }

            return importedLifeCounterPlayers;
        }
        private List<UsersImportUserDataResponse_userBoardGameSession>? ParseBoardGameSessions(List<string>? lines)
        {
            var importedBoarGameSessions = new List<UsersImportUserDataResponse_userBoardGameSession>();

            // Skip header line at index 0
            for (int i = 1; i < lines.Count - 1; i++)
            {
                var line = lines[i];
                var data = line.Split(';');

                importedBoarGameSessions.Add(new UsersImportUserDataResponse_userBoardGameSession
                {
                    BoardGameName = data[0].Trim('"'),
                    Date = DateOnly.TryParse(data[1].Trim('"'), out var date) ? date : null,
                    PlayersCount = int.Parse(data[2]),
                    Duration_minutes = int.Parse(data[3]),
                    IsDeleted = bool.Parse(data[4]),
                });
            }

            return importedBoarGameSessions;
        }
        private List<UsersImportUserDataResponse_userBoardGameRating>? ParseBoardGameRatings(List<string>? lines)
        {
            var importedBoarGameRatings = new List<UsersImportUserDataResponse_userBoardGameRating>();

            // Skip header line at index 0
            for (int i = 1; i < lines.Count - 1; i++)
            {
                var line = lines[i];
                var data = line.Split(';');

                importedBoarGameRatings.Add(new UsersImportUserDataResponse_userBoardGameRating
                {
                    BoardGameName = data[0].Trim('"'),                   
                    Rate = decimal.Parse(data[1])                    
                });
            }

            return importedBoarGameRatings;
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

            if(string.IsNullOrWhiteSpace(request.Date) == false)
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

            return (true, string.Empty);           
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

            if(string.IsNullOrWhiteSpace(request.NewDate) == false)
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

            return (true, string.Empty);
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

            return (true, string.Empty);
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

            return (true, string.Empty);
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

            return (true, string.Empty);
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

            return (true, string.Empty);
        }
        //
        //--* end of BOARD GAMES *--//        



        // LIFE COUNTERS
        //
        // 1º LIFE COUNTER QUICK START      
        public async Task<(UsersSyncLifeCounterDataResponse?, string)> SyncLifeCounterData(UsersSyncLifeCounterDataRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (requestIsValid, message) = SyncLifeCounterData_Validation(request);

            if (requestIsValid == false)
            {
                return (null, message);
            }

            var templates = request!.LifeCounterTemplates;

            foreach (var template in templates!)
            {
                var templateName = template.LifeCounterTemplateName!.Trim();


                var templateDB = await this._daoDbContext
                    .LifeCounterTemplates
                    .Include(a => a.LifeCounterManagers!)
                    .ThenInclude(a => a.LifeCounterPlayers)
                    .FirstOrDefaultAsync(a => a.UserId == userId &&
                                         a.LifeCounterTemplateName!.Trim() == templateName);


                var newManagers = new List<LifeCounterManager>();

                foreach (var manager in template.LifeCounterManagers!)
                {
                    var managerName = manager.LifeCounterManagerName!.Trim();

                    var managerDB = await this._daoDbContext
                    .LifeCounterManagers
                    .FirstOrDefaultAsync(a => a.UserId == userId &&
                                        a.LifeCounterManagerName!.Trim() == managerName);

                    var importedPlayers = manager.LifeCounterPlayers;

                    var newPlayers = new List<LifeCounterPlayer>();

                    foreach (var player in importedPlayers!)
                    {
                        newPlayers.Add(new LifeCounterPlayer
                        {
                            PlayerName = player.PlayerName,
                            CurrentLifePoints = player.CurrentLifePoints,
                            IsDefeated = player.IsDefeated,

                            StartingLifePoints = manager.PlayersStartingLifePoints,
                            FixedMaxLifePointsMode = manager.FixedMaxLifePointsMode!.Value,
                            MaxLifePoints = manager.PlayersMaxLifePoints,
                            AutoDefeatMode = manager.AutoDefeatMode,
                        });
                    }

                    if (managerDB == null)
                    {
                        var newManager = new LifeCounterManager
                        {
                            LifeCounterManagerName = manager.LifeCounterManagerName,
                            PlayersStartingLifePoints = manager.PlayersStartingLifePoints,
                            PlayersCount = manager.PlayersCount,
                            FirstPlayerIndex = manager.FirstPlayerIndex,
                            FixedMaxLifePointsMode = manager.FixedMaxLifePointsMode,
                            PlayersMaxLifePoints = manager.PlayersMaxLifePoints,
                            AutoDefeatMode = manager.AutoDefeatMode,
                            AutoEndMode = manager.AutoEndMode,
                            StartingTime = manager.StartingTime,
                            EndingTime = manager.EndingTime,
                            Duration_minutes = manager.Duration_minutes,
                            IsFinished = manager.IsFinished,

                            LifeCounterPlayers = newPlayers,

                            UserId = userId,
                        };

                        newManagers.Add(newManager);
                    }
                    else
                    {
                        if (managerDB.LifeCounterPlayers == null || managerDB.LifeCounterPlayers.Count < 1)
                        {
                            managerDB.LifeCounterPlayers = newPlayers;
                            managerDB.PlayersCount = newPlayers.Count;
                        }
                        else
                        {
                            foreach (var newPlayer in newPlayers)
                            {
                                var newPlayerName = newPlayer.PlayerName!.Trim();

                                bool alreadyExists = managerDB.LifeCounterPlayers
                                    .Any(p => p.PlayerName!.Trim() == newPlayerName);

                                if (alreadyExists == false)
                                {
                                    managerDB.LifeCounterPlayers.Add(newPlayer);

                                    if (managerDB.LifeCounterPlayers.Count == 6)
                                    {
                                        break;
                                    }
                                }
                            }
                            
                            managerDB.PlayersCount = managerDB.LifeCounterPlayers.Count;
                        }
                    }

                    if (templateDB == null)
                    {
                        var newTemplate = new LifeCounterTemplate
                        {
                            LifeCounterTemplateName = template.LifeCounterTemplateName,
                            PlayersStartingLifePoints = template.PlayersStartingLifePoints,
                            PlayersCount = template.PlayersCount,
                            FixedMaxLifePointsMode = template.FixedMaxLifePointsMode,
                            PlayersMaxLifePoints = template.PlayersMaxLifePoints,
                            AutoDefeatMode = template.AutoDefeatMode,
                            AutoEndMode = template.AutoEndMode,
                            LifeCounterManagersCount = template.LifeCounterManagersCount,

                            LifeCounterManagers = newManagers,

                            UserId = userId
                        };

                        this._daoDbContext.LifeCounterTemplates.Add(newTemplate);
                    }
                    else
                    {
                        if (templateDB.LifeCounterManagers == null || templateDB.LifeCounterManagers.Count < 1)
                        {
                            templateDB.LifeCounterManagers = newManagers;
                        }
                        else
                        {
                            for (int i = 0; i < newManagers.Count; i++)
                            {
                                var newMangerName = newManagers[i].LifeCounterManagerName!.Trim();

                                var managersToBeAdded = new List<LifeCounterManager>() { };

                                foreach (var existingManager in templateDB.LifeCounterManagers!)
                                {
                                    var existingManagerName = existingManager.LifeCounterManagerName!.Trim();

                                    if (newMangerName != existingManagerName)
                                    {
                                        managersToBeAdded.Add(newManagers[i]);
                                    }
                                }
                                templateDB.LifeCounterManagers.AddRange(managersToBeAdded);
                            }

                        }
                    }
                }
            }

            await this._daoDbContext.SaveChangesAsync();

            return (new UsersSyncLifeCounterDataResponse(), "Life Counter Data sucessfully synced");
        }
        private static (bool, string) SyncLifeCounterData_Validation(UsersSyncLifeCounterDataRequest? request)
        {
            if (request == null)
            {
                return (false, $"Error: request is null.");
            }

            if(request.LifeCounterTemplates == null || request.LifeCounterTemplates.Count == 0)
            {
                return (false, $"Error: requested life counter templates are null.");
            }

            return (true, string.Empty);
        }


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

            //  => Search for any existing life counter MANAGER
            var doesAnyLifeCounterManagerExists = await this._daoDbContext
                .LifeCounterManagers
                .AnyAsync(a => a.UserId == userId);

            var text = "";


            int? lifeCounterTemplateId = 0;
            string? lifeCounterTemplateName = "";
            int? lifeCounterManagerId = 0;          

            if (doesAnyLifeCounterManagerExists == false)
            {
                // No life counter MANAGER was found, THEN:
                // => Search for ANY existing life counter TEMPLATE
                var doesAnyLifeCounterTemplateExists = await this._daoDbContext
                .LifeCounterTemplates
                .AnyAsync(a => a.UserId == userId);

                

                if (doesAnyLifeCounterTemplateExists == false)
                {
                    //  No life counter player exists AND no life counter TEMPLATE exists, THEN:
                    //  => Call Create LifeCounter Template and Create Life Counter Manager

                    // Creating a new default Life Counter TEMPLATE:
                    var (createTemplate_reponse_content, createTemplate_response_message) = await this.CreateLifeCounterTemplate();

                    if (createTemplate_reponse_content == null)
                    {
                        return (null, $"Error: request to create a new Default Life Counter TEMPLATE failed: {createTemplate_response_message}");
                    }

                    // Default Life Counter Template Id:
                    lifeCounterTemplateId = createTemplate_reponse_content.LifeCounterTemplateId;
                    lifeCounterTemplateName = createTemplate_reponse_content.LifeCounterTemplateName;

                    text = "New Default life counter TEMPLATE and new Default life counter MANAGER were created successfully";
                }
                else
                {
                    //  No life counter player exists BUT at least one life counter TEMPLATE exists, THEN:
                    //  => Call Create Life Counter Manager and create one for the newest created template

                    // Fechting the id of the most recently life counter TEMPLATE created:
                    var (getLastTemplate_reponse_content, getLastTemplateId_response_message) = await this.GetLastLifeCounterTemplate();

                    if (getLastTemplate_reponse_content == null)
                    {
                        return (null,
                            $"Error: failed to fetch the ID of the most recently created Life Counter TEMPLATE: {getLastTemplateId_response_message}");
                    }

                    // Most recently created Life Counter Template Id:
                    lifeCounterTemplateId = getLastTemplate_reponse_content.LastLifeCounterTemplateId;
                    lifeCounterTemplateName = getLastTemplate_reponse_content.LastLifeCounterTemplateName;

                    text = "New Default life counter MANAGER started successfully, belonging to the last created life counter Template";
                }

                // Starting a new Life Counter MANAGER
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
                .Include(a => a.LifeCounterTemplate)
                .Include(a => a.LifeCounterPlayers)
                .OrderByDescending(a => a.Id)
                .FirstOrDefaultAsync(a => a.UserId == userId);

                if (lifeCounterManagerDB == null)
                {
                    return (null, "Error: attempt to fetch the last life counter MANAGER failed, returning null");
                }

                lifeCounterTemplateId = lifeCounterManagerDB.LifeCounterTemplateId;
                lifeCounterTemplateName = lifeCounterManagerDB.LifeCounterTemplate!.LifeCounterTemplateName;

                var playersDB = lifeCounterManagerDB.LifeCounterPlayers;

                if (playersDB == null || playersDB.Count < 1)
                {
                    return (null, "Error: the fetched most recently started life counter MANAGER faled to return its players: null or less than 1 player");
                }

                if (lifeCounterManagerDB.IsFinished == false)
                {
                    // The most recently started life counter MANAGER found is NOT yet finished, THEN:
                    // => reload the life counter MANAGER found 
                    lifeCounterManagerId = lifeCounterManagerDB.Id;                

                    text = "Most recently not yet finished Life Counter Manager loaded successfully";
                }
                else
                {
                    // No life counter MANAGER that was not yet finished was found to be reloaded, THEN:
                    //
                    // 1st => start a new life counter MANAGER of the template 
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

                    // 2nd => overwrite via EDIT endpoint the data of started life counter MANAGER with the data of the most recently finished life counter MANAGER                    
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
                LifeCounterTemplateId = lifeCounterTemplateId,
                LifeCounterTemplateName = lifeCounterTemplateName,
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
            
            
            var lastTemplateId = await this._daoDbContext
            .LifeCounterTemplates        
            .OrderByDescending(a => a.Id)
            .Select(a => a.Id)
            .FirstOrDefaultAsync();
                
            if(lastTemplateId + 1 < 10)
            {
                lifeCounterTemplateName = $"Life Counter Template #0{lastTemplateId + 1}";          
            } else
            {
                lifeCounterTemplateName = $"Life Counter Template #{lastTemplateId + 1}";
            }

                var newLifeCounterTemplate = new LifeCounterTemplate
                {
                    LifeCounterTemplateName = lifeCounterTemplateName,
                    PlayersStartingLifePoints = 10,
                    PlayersCount = 1,
                    FixedMaxLifePointsMode = false,
                    PlayersMaxLifePoints = null,
                    AutoDefeatMode = false,
                    AutoEndMode = false,

                    UserId = userId,
                };

            this._daoDbContext.Add(newLifeCounterTemplate);

            await this._daoDbContext.SaveChangesAsync();

            return (new UsersCreateLifeCounterTemplateResponse { 
                LifeCounterTemplateId = newLifeCounterTemplate.Id, 
                LifeCounterTemplateName = newLifeCounterTemplate.LifeCounterTemplateName
                }, "New LifeCounterTemplate created successfully");
        }
        public static (bool, string) CreateLifeCounterTemplate_Validation(UsersCreateLifeCounterTemplateRequest? request)
        {
            if(request != null)
            {
                return (false, "Error: request is not null however it MUST be null");
            }          

            return (true, string.Empty);
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


        public async Task<(UsersGetLastLifeCounterTemplateResponse?, string)> GetLastLifeCounterTemplate(UsersGetLastLifeCounterTemplateRequest? request = null)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = GetLastLifeCounterTemplate_Validation(request);

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

            return (new UsersGetLastLifeCounterTemplateResponse
            {
                LastLifeCounterTemplateId = lifeCounterTemplateDB.Id,
                LastLifeCounterTemplateName = lifeCounterTemplateDB.LifeCounterTemplateName
            }, "Last LifeCounterTemplateId fetched successfully");
        }
        private static (bool, string) GetLastLifeCounterTemplate_Validation(UsersGetLastLifeCounterTemplateRequest? request)
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
                .OrderBy(a => a.LifeCounterTemplateName)
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
                return (false, $"Error: request is NOT null but it MUST be null!");
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
                LifeCounterManagersCount = lifeCounterDB.LifeCounterManagersCount
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


        public async Task<(UsersEditLifeCounterTemplateResponse?, string)> EditLifeCounterTemplate(UsersEditLifeCounterTemplateRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = EditLifeCounterTemplate_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var lifeCounterTemplateDB = await this._daoDbContext
                .LifeCounterTemplates
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Id == request!.LifeCounterTemplateId);

            if (lifeCounterTemplateDB == null)
            {
                return (null, $"Error: life counter template request failed: {lifeCounterTemplateDB}");
            }

            var nameAlreadyInUse = await this._daoDbContext
                .LifeCounterTemplates
                .AnyAsync(a => a.UserId == userId && 
                               a.LifeCounterTemplateName!.ToLower().Trim() == request!.NewLifeCounterTemplateName!.ToLower().Trim() && 
                               a.Id != request.LifeCounterTemplateId);

            if (nameAlreadyInUse == true)
            {
                return (null, $"Error: requested Life Counter Template NAME is already in use, please choose a different name: {request!.NewLifeCounterTemplateName}");
            }

            lifeCounterTemplateDB.LifeCounterTemplateName = request!.NewLifeCounterTemplateName;
            lifeCounterTemplateDB.PlayersStartingLifePoints = request.NewPlayersStartingLifePoints;
            lifeCounterTemplateDB.PlayersCount = request.NewPlayersCount;
            lifeCounterTemplateDB.FixedMaxLifePointsMode = request.FixedMaxLifePointsMode;
            lifeCounterTemplateDB.PlayersMaxLifePoints = request.NewPlayersMaxLifePoints;
            lifeCounterTemplateDB.AutoDefeatMode = request.AutoDefeatMode;
            lifeCounterTemplateDB.AutoEndMode = request.AutoEndMode;

            await this._daoDbContext.SaveChangesAsync();    

            return (new UsersEditLifeCounterTemplateResponse
            {
                LifeCounterTemplateId = lifeCounterTemplateDB.Id,
            }, "LifeCounterTemplate edited successfully");
        }
        private static (bool, string) EditLifeCounterTemplate_Validation(UsersEditLifeCounterTemplateRequest? request)
        {
            if (request == null)
            {
                return (false, $"Error: request failed: {request}");
            }

            if (request.LifeCounterTemplateId == null || request.LifeCounterTemplateId.HasValue == false || request.LifeCounterTemplateId < 1)
            {
                return (false, $"Error: requested LifeCounterTemplateId failed: {request.LifeCounterTemplateId}");
            }

            if (string.IsNullOrWhiteSpace(request!.NewLifeCounterTemplateName) == true)
            {
                return (false,
                   "Error: LifeCounterTemplateName is null or empty! " +
                   $"request.LifeCounterTemplateName: {request.NewLifeCounterTemplateName}");
            }

            if (request.NewPlayersStartingLifePoints.HasValue == false || request.NewPlayersStartingLifePoints == null)
            {
                return (false,
                   "Error: StartingLifePoints is null! " +
                   $"request.PlayersStartingLifePoints: {request.NewPlayersStartingLifePoints}");
            }

            if (request.NewPlayersStartingLifePoints < 0)
            {
                return (false,
                    "Error: StartingLifePoints cannot be less than 0! " +
                    $"request.PlayersStartingLifePoints: {request.NewPlayersStartingLifePoints}");
            }

            if (request.NewPlayersCount.HasValue == false || request.NewPlayersCount == null)
            {
                return (false,
                   "Error: PlayersCount is null! " +
                   $"request.PlayersCount: {request.NewPlayersCount}");
            }

            if (request.NewPlayersCount < 1 || request.NewPlayersCount > 6)
            {
                return (false,
                    "Error: PlayersCount cannot be less than 1 or more than 6! " +
                    $"request.PlayersCount: {request.NewPlayersCount}");
            }

            if (request.FixedMaxLifePointsMode.HasValue == false)
            {
                return (false,
                   "Error: FixedMaxLifePointsMode is null! " +
                   $"request.FixedMaxLifePointsMode.HasValue == false: {request.FixedMaxLifePointsMode.HasValue == false}");
            }

            if (request.FixedMaxLifePointsMode == true && (request.NewPlayersStartingLifePoints.HasValue == false || request.NewPlayersStartingLifePoints == null))
            {
                return (false,
                   "Error: PlayersMaxLifePoints is null! " +
                   $"request.PlayersMaxLifePoints: {request.NewPlayersMaxLifePoints}");
            }

            if (request.NewPlayersMaxLifePoints < 1 || request.NewPlayersMaxLifePoints > 999)
            {
                return (false,
                    "Error: PlayersMaxLifePoints cannot be less than 1 or more than 999! " +
                    $"request.PlayersMaxLifePoints: {request.NewPlayersMaxLifePoints}");
            }

            if (request.AutoDefeatMode.HasValue == false || request.AutoDefeatMode == null)
            {
                return (false,
                   "Error: AutoDefeatMode is null! " +
                   $"request.AutoDefeatMode: {request.AutoDefeatMode}");
            }

            if (request.AutoEndMode.HasValue == false || request.AutoEndMode == null)
            {
                return (false,
                   "Error: AutoEndMatch is null! " +
                   $"request.AutoEndMatch: {request.AutoEndMode}");
            }


            return (true, string.Empty);
        }


        public async Task<(UsersDeleteLifeCounterTemplateResponse?, string)> DeleteLifeCounterTemplate(UsersDeleteLifeCounterTemplateRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = DeleteLifeCounterTemplate_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var attemptToDeleteLifeCounterManagerDB = await this._daoDbContext
                .LifeCounterManagers
                .Where(a =>
                    a.UserId == userId &
                    a.LifeCounterTemplateId == request!.LifeCounterTemplateId)
                 .ExecuteDeleteAsync();

            if (attemptToDeleteLifeCounterManagerDB <= 0)
            {
                return (null, "Error: request to delete LIFE COUNTER MANAGER failed");
            }

            var attemptToDeleteLifeCounterTemplateDB = await this._daoDbContext
                .LifeCounterTemplates
                .Where(a =>
                    a.UserId == userId &
                    a.Id == request!.LifeCounterTemplateId)
                 .ExecuteDeleteAsync();

            if (attemptToDeleteLifeCounterTemplateDB <= 0)
            {
                return (null, "Error: request to delete LIFE COUNTER MANAGER failed");
            }


            return (new UsersDeleteLifeCounterTemplateResponse
            {

            }, $"Life Counter Template deleted successfully,");

        }
        private static (bool, string) DeleteLifeCounterTemplate_Validation(UsersDeleteLifeCounterTemplateRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.LifeCounterTemplateId == null || request.LifeCounterTemplateId < 1)
            {
                return (false, $"Error: LifeCounterManagerId request failed: {request.LifeCounterTemplateId}");
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
            var random = new Random();
            int firstPlayerIndex = random.Next(0, lifeCounterTemplateDB.PlayersCount!.Value);

            var lifeCounterManagerName = $"{lifeCounterTemplateDB.LifeCounterTemplateName}-M{lifeCounterTemplateDB.LifeCounterManagersCount + 1}";

            var newLifeCounterManager = new LifeCounterManager
            {
                LifeCounterTemplateId = request!.LifeCounterTemplateId,
                
                LifeCounterManagerName = lifeCounterManagerName,
                LifeCounterPlayers = newPlayers,
                PlayersCount = lifeCounterTemplateDB.PlayersCount,
                FirstPlayerIndex = firstPlayerIndex,
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
                LifeCounterManagerName = newLifeCounterManager.LifeCounterManagerName,

                PlayersStartingLifePoints = newLifeCounterManager.PlayersStartingLifePoints,
                PlayersCount = newLifeCounterManager.PlayersCount,
                FirstPlayerIndex = newLifeCounterManager.FirstPlayerIndex,
                LifeCounterPlayers = [],

                FixedMaxLifePointsMode = newLifeCounterManager.FixedMaxLifePointsMode,
                PlayersMaxLifePoints = newLifeCounterManager.PlayersMaxLifePoints,

                AutoDefeatMode = newLifeCounterManager.AutoDefeatMode,
                AutoEndMode = newLifeCounterManager.AutoEndMode,
                StartingTime = startMark

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
                return (false, $"Error: requested LifeCounterManagerId failed:" +
                    $" request.LifeCounterTemplateId == null -> {request.LifeCounterTemplateId == null} or  " +
                    $"request.LifeCounterTemplateId.HasValue == false -> {request.LifeCounterTemplateId.HasValue == false}");
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

            lifeCounterManagerDB.LifeCounterManagerName = request!.NewLifeCounterManagerName;
            lifeCounterManagerDB.PlayersStartingLifePoints = request!.NewPlayersStartingLifePoints;

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
            } else if(request.NewPlayersCount < lifeCounterManagerDB.PlayersCount)
            {
                for (var i = lifeCounterManagerDB.PlayersCount.Value - 1; i >= request.NewPlayersCount; i--)
                {
                    playersDB.RemoveAt(i);
                }

            }

            lifeCounterManagerDB.PlayersCount = request.NewPlayersCount;

            lifeCounterManagerDB.FirstPlayerIndex = request.FirstPlayerIndex;

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

            var response = await this._daoDbContext
                .LifeCounterManagers
                .AsNoTracking()
                .Where(a => a.UserId == userId && a.Id == request!.LifeCounterManagerId)
                .Select(a => new UsersGetLifeCounterManagerDetailsResponse
                {
                    LifeCounterTemplateId = a.LifeCounterTemplate!.Id,
                    LifeCounterTemplateName = a.LifeCounterTemplate!.LifeCounterTemplateName,

                    LifeCounterManagerName = a.LifeCounterManagerName,
                    PlayersCount = a.PlayersCount,
                    FirstPlayerIndex = a.FirstPlayerIndex,
                    LifeCounterPlayers = a.LifeCounterPlayers.Select(b => new UsersGetLifeCounterManagerDetailsResponse_players
                    {
                        PlayerId = b.Id,
                        PlayerName = b.PlayerName,
                        CurrentLifePoints = b.CurrentLifePoints,
                        IsDefeated = b.IsDefeated,
                    }).ToList(),
                    PlayersStartingLifePoints = a.PlayersStartingLifePoints,
                    FixedMaxLifePointsMode = a.FixedMaxLifePointsMode,
                    PlayersMaxLifePoints = a.PlayersMaxLifePoints,
                    AutoDefeatMode = a.AutoDefeatMode,
                    AutoEndMode = a.AutoEndMode,
                    StartingTime = a.StartingTime,
                    EndingTime = a.EndingTime,
                    Duration_minutes = a.Duration_minutes,
                    IsFinished = a.IsFinished,
                })
                .FirstOrDefaultAsync();

            if (response == null)
            {
                return (null, "Error: requested LifeCounterManagerDB failed");
            }     

            if (response.LifeCounterTemplateId == null || string.IsNullOrWhiteSpace(response.LifeCounterTemplateName) == true)
            {
                return (null, "Error: requested LifeCounterTemplateDB failed");
            }         
      

            if (response.LifeCounterPlayers == null || response.LifeCounterPlayers.Count < 1)
            {
                return (null, "Error: requested LifeCounterPlayersDB failed");
            }

            return (response, "Life counter details fetched successfully");
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



        public async Task<(UsersGetLastLifeCounterManagerResponse?, string)> GetLastLifeCounterManager(UsersGetLastLifeCounterManagerRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = GetLastLifeCounterManager_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var lifeCounterManagerDB = await this._daoDbContext
                .LifeCounterManagers              
                .Include(a => a.LifeCounterPlayers)             
                .OrderByDescending(a => a.Id)
                .FirstOrDefaultAsync(a => a.UserId == userId && a.LifeCounterTemplateId == request!.LifeCounterTemplateId && a.IsFinished == false);

            var response = new UsersGetLastLifeCounterManagerResponse();

         

            if (lifeCounterManagerDB == null)
            {
                var startLifeCounterManager_request = new UsersStartLifeCounterManagerRequest
                {
                    LifeCounterTemplateId = request!.LifeCounterTemplateId
                };

                var (createManager_response_content, createManager_response_message) = await this.StartLifeCounterManager(startLifeCounterManager_request);

                if (createManager_response_content == null)
                {
                    return (null, $"Error: request to create a new Default Life Counter MANAGER failed: {createManager_response_message}");
                }
          
                response.LifeCounterManagerId = createManager_response_content.LifeCounterManagerId;
                response.LifeCounterManagerName = createManager_response_content.LifeCounterManagerName;
                response.PlayersCount = createManager_response_content.PlayersCount;
                response.FirstPlayerIndex = createManager_response_content.FirstPlayerIndex;
                
                if(createManager_response_content.LifeCounterPlayers == null || createManager_response_content.LifeCounterPlayers.Count == 0)
                {
                    return (null, "Error, failed to fetch life counter PLAYERS while creating new life counter manager");
                }

                response.LifeCounterPlayers = [];

                foreach (var player in createManager_response_content.LifeCounterPlayers)
                {
                    response.LifeCounterPlayers.Add(new UsersGetLastLifeCounterManagerResponse_players
                    {
                        LifeCounterPlayerId = player.LifeCounterPlayerId,
                        LifeCounterPlayerName = player.PlayerName,
                        CurrentLifePoints = player.PlayerStartingLifePoints,
                        IsDefeated = false
                    });
                }

                response.PlayersStartingLifePoints = createManager_response_content.PlayersStartingLifePoints;
                response.AutoDefeatMode = createManager_response_content.AutoDefeatMode;
                response.AutoEndMode = createManager_response_content.AutoEndMode;
                response.StartingTime = createManager_response_content.StartingTime;

                return (response, "New life counter manager started successfully for the seleced life counter template.");
            }
      
            response.LifeCounterManagerId = lifeCounterManagerDB.Id;
            response.LifeCounterManagerName = lifeCounterManagerDB.LifeCounterManagerName;
            response.PlayersCount = lifeCounterManagerDB.PlayersCount;
            response.FirstPlayerIndex = lifeCounterManagerDB.FirstPlayerIndex;

            if (lifeCounterManagerDB.LifeCounterPlayers == null || lifeCounterManagerDB.LifeCounterPlayers.Count == 0)
            {
                return (null, "Error, failed to fetch life counter PLAYERS of the unfinished life counter manager");
                }

            response.LifeCounterPlayers = [];

            foreach (var player in lifeCounterManagerDB.LifeCounterPlayers)
            {
                response.LifeCounterPlayers.Add(new UsersGetLastLifeCounterManagerResponse_players
                {
                    LifeCounterPlayerId = player.Id,
                    LifeCounterPlayerName = player.PlayerName,
                    CurrentLifePoints = player.CurrentLifePoints,
                    IsDefeated = false
                });
            }

            response.PlayersStartingLifePoints = lifeCounterManagerDB.PlayersStartingLifePoints;
            response.AutoDefeatMode = lifeCounterManagerDB.AutoDefeatMode;
            response.AutoEndMode = lifeCounterManagerDB.AutoEndMode;
            response.StartingTime = lifeCounterManagerDB.StartingTime;

            return (response, "Unfinished life counter manager reloaded successfully for the seleced life counter template.");
        }
        private static (bool, string) GetLastLifeCounterManager_Validation(UsersGetLastLifeCounterManagerRequest? request)
        {
            if (request == null)
            {
                return (false, $"Error: request failed: {request}");
            }

            if (request.LifeCounterTemplateId.HasValue == false || request.LifeCounterTemplateId == null || request.LifeCounterTemplateId < 1)
            {
                return (false, $"Error: requested LifeCounterManagerId failed: {request.LifeCounterTemplateId}");
            }

            return (true, string.Empty);
        }



        public async Task<(List<UsersListUnfinishedLifeCounterManagersResponse>?, string)> ListUnfinishedLifeCounterManagers(UsersListUnfinishedLifeCounterManagersRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = ListUnfinishedLifeCounterManagers_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var unfinishedLifeCounterManagersDB = await this._daoDbContext
                .LifeCounterManagers
                .Where(a => a.UserId == userId && a.LifeCounterTemplateId == request.LifeCounterTemplateId && a.IsFinished == false)
                .OrderByDescending(a => a.Id)
                .Select(a => new
                {
                    a.Id,
                    a.LifeCounterManagerName,
                    a.StartingTime 
                })
                .ToListAsync();

            if (unfinishedLifeCounterManagersDB == null || unfinishedLifeCounterManagersDB.Count < 1)
            {
                return (null, "Error: requested list of unfinished Life Counter Managers returned null or with 0 elements");
            }

            var content = unfinishedLifeCounterManagersDB
                .Select(a => new UsersListUnfinishedLifeCounterManagersResponse
                {
                    LifeCounterManagerId = a.Id,
                    LifeCounterManagerName = a.LifeCounterManagerName,
                    StartingDate = DateOnly
                        .FromDateTime(new DateTime(a.StartingTime!.Value))
                        .ToString("dd/MM/yyyy")
                })
                .ToList();         

            return (content, "Unfinished Life Counter Managers listed successfully");
        }
        private static (bool, string) ListUnfinishedLifeCounterManagers_Validation(UsersListUnfinishedLifeCounterManagersRequest? request)
        {
            if (request == null)
            {
                return (false, $"Error: request is null!");
            }

            if(request.LifeCounterTemplateId == null || request.LifeCounterTemplateId < 0)
            {
                return (false, "Requested LifeCounterTemplateId failed");
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

            var random = new Random();
            lifeCounterManagerDB.FirstPlayerIndex = random.Next(0, lifeCounterPlayers.Count);

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

            if (!requestIsValid)
            {
                return (null, message);
            }

            var lifeCounterManagerDB = await this._daoDbContext
                .LifeCounterManagers
                .Include(a => a.LifeCounterPlayers)
                .FirstOrDefaultAsync(a =>
                    a.UserId == userId &&
                    a.Id == request!.LifeCounterManagerId &&
                    a.AutoEndMode == true);

            if (lifeCounterManagerDB == null)
            {
                return (null, "Error: LifeCounterManager request failed");
            }

            if (!lifeCounterManagerDB.StartingTime.HasValue)
            {
                return (null, "Error: StartingTime is not set");
            }

            long rawDurationTicks;
            double roundedDuration;

            if (lifeCounterManagerDB.IsFinished == true)
            {
                rawDurationTicks = lifeCounterManagerDB.EndingTime!.Value - lifeCounterManagerDB.StartingTime.Value;
                roundedDuration = Math.Round((double)(rawDurationTicks) / 600_000_000, 2);

                return (new UsersCheckForLifeCounterManagerEndResponse
                {
                    IsFinished = true,
                    Duration_minutes = roundedDuration,
                }, "This life counter manager is finished");
            }

            var lifeCounterPlayersDB = lifeCounterManagerDB.LifeCounterPlayers;

            if (lifeCounterPlayersDB == null || lifeCounterPlayersDB.Count <= 0)
            {
                return (null, "Error: lifeCounterPlayersDB request failed");
            }

            if (!lifeCounterManagerDB.PlayersCount.HasValue)
            {
                return (null, "Error: PlayersCount is not set");
            }

            var playersCount = lifeCounterManagerDB.PlayersCount.Value;
            var defeatedPlayers = lifeCounterPlayersDB.Count(a => a.IsDefeated == true);

            var isLifeCounterManagerEnded = (playersCount - defeatedPlayers) <= 1;

            if (!isLifeCounterManagerEnded)
            {
                return (new UsersCheckForLifeCounterManagerEndResponse(), "This life counter manager is NOT finished");
            }

            var currentTimeMark = DateTime.UtcNow.ToLocalTime().Ticks;

            lifeCounterManagerDB.IsFinished = true;
            lifeCounterManagerDB.EndingTime = currentTimeMark;

            rawDurationTicks = currentTimeMark - lifeCounterManagerDB.StartingTime.Value;
            roundedDuration = Math.Round((double)rawDurationTicks / 600_000_000, 2);
            lifeCounterManagerDB.Duration_minutes = roundedDuration;

            foreach (var player in lifeCounterPlayersDB)
            {
                if (player.CurrentLifePoints <= 0)
                {
                    player.IsDefeated = true;
                }
            }

            await this._daoDbContext.SaveChangesAsync();

            return (new UsersCheckForLifeCounterManagerEndResponse
            {
                IsFinished = true,
                Duration_minutes = roundedDuration,
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


        public async Task<(UsersDeleteLifeCounterManagerResponse?, string)> DeleteLifeCounterManager(UsersDeleteLifeCounterManagerRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = DeleteLifeCounterManager_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var attemptToDeleteLifeCounterManagerDB = await this._daoDbContext
                .LifeCounterManagers
                .Where(a =>
                    a.UserId == userId &
                    a.Id == request!.LifeCounterManagerId)
                 .ExecuteDeleteAsync();

            if(attemptToDeleteLifeCounterManagerDB <= 0)
            {
                return (null, "Error: request to delete LIFE COUNTER MANAGER failed");
            }         

            return (new UsersDeleteLifeCounterManagerResponse
            {

            }, $"Life Counter Template deleted successfully,");
                
        }
        private static (bool, string) DeleteLifeCounterManager_Validation(UsersDeleteLifeCounterManagerRequest? request)
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

            if (request.LifePointsToIncrease.HasValue == false )
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

            if (request.LifePointsToDecrease.HasValue == false)
            {
                return (false, $"Error: invalid decrease amount request: {request.LifePointsToDecrease}");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersRestoreLifeCounterPlayerResponse?, string)> RestoreLifeCounterPlayer(UsersRestoreLifeCounterPlayerRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (requestIsValid, message) = RestoreLifeCounterPlayer_Validation(request);

            if (requestIsValid == false)
            {
                return (null, message);
            }

            var lifeCounterPlayerDB = await this._daoDbContext
                .LifeCounterPlayers
                .Include(a => a.LifeCounterManager)
                .Where(a => a.Id == request!.LifeCounterPlayerId &&
                    a.LifeCounterManager!.UserId == userId)
                .AnyAsync();

            if (lifeCounterPlayerDB == false)
            {
                return (null, $"Error, invalid requested LifeCounterPlayer, returning: {lifeCounterPlayerDB}");
            }


            await this._daoDbContext
                .LifeCounterPlayers
                .Where(a => a.Id == request!.LifeCounterPlayerId &&
                            a.LifeCounterManager!.UserId == userId)
                .ExecuteUpdateAsync(a => a
                    .SetProperty(b => b.CurrentLifePoints, 1)
                    .SetProperty(b => b.IsDefeated, false));

            return (new UsersRestoreLifeCounterPlayerResponse {  }, "Player restored successfully");
        }
        private static (bool, string) RestoreLifeCounterPlayer_Validation(UsersRestoreLifeCounterPlayerRequest? request)
        {
            if (request == null)
            {
                return (false, $"Error: request failed: {request}");
            }

            if (request.LifeCounterPlayerId.HasValue == false || request.LifeCounterPlayerId < 1)
            {
                return (false, $"Error: requested LifeCounterPlayerId failed: {request.LifeCounterPlayerId}");
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

            if (string.IsNullOrWhiteSpace(request.PlayerNewName) == true)
            {
                return (false, $"Error: requested PlayerNewName failed: String.IsNullOrWhiteSpace(request.PlayerNewName) == true? {string.IsNullOrWhiteSpace(request.PlayerNewName) == true}");
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
        // 5º LIFE COUNTER STATISTICS
        public async Task<(UsersGetLifeCounterStatisticsResponse?, string)> GetLifeCounterStatistics(UsersGetLifeCounterStatisticsRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = GetLifeCounterStatistics_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var lifeCounterTemplatesDB = await this._daoDbContext
                .LifeCounterTemplates
                .Include(a => a.LifeCounterManagers)
                .Where(a => a.UserId == userId)            
                .ToListAsync();

            if (lifeCounterTemplatesDB == null || lifeCounterTemplatesDB.Count < 0)
            {
                return (null, "Error: requested list of Life Counter returned null or with 0 elements");
            }

            var mustUsedLifeCounter = "";
            var lifeCountersCreated = 0;
            var lifeCountersStarted = 0;
            var unfinishedLifeCounters = 0;
            var favoritePlayersCount = 0;

            if (lifeCounterTemplatesDB.Count == 0)
            {
                return (new UsersGetLifeCounterStatisticsResponse
                {
                    MostUsedLifeCounter = "No Life Counter created yet",
                    LifeCountersCreated = lifeCountersCreated,
                    LifeCountersStarted = lifeCountersStarted,
                    UnfinishedLifeCounters = unfinishedLifeCounters,
                    FavoritePlayersCount = favoritePlayersCount
                }, "Statistics fetched successfully");

            }

            mustUsedLifeCounter = lifeCounterTemplatesDB
                .OrderByDescending(a => a.LifeCounterManagers!.Count)
                .FirstOrDefault()?.LifeCounterTemplateName;

            lifeCountersCreated = lifeCounterTemplatesDB.Count;

            var lifeCounterManagersDB = lifeCounterTemplatesDB
                .SelectMany(a => a.LifeCounterManagers!)
                .Where(a => a.UserId == userId)
                .ToList();

            if (lifeCounterManagersDB == null || lifeCounterManagersDB.Count <= 0)
            {
                var avgTemplatePlayersCount =  (lifeCounterManagersDB!.Select(a => a.PlayersCount!).DefaultIfEmpty(0).Average() ?? 0);

                return (new UsersGetLifeCounterStatisticsResponse
                {
                    MostUsedLifeCounter = mustUsedLifeCounter,
                    LifeCountersCreated = lifeCountersCreated,
                    LifeCountersStarted = 0,
                    UnfinishedLifeCounters = 0,
                    FavoritePlayersCount = (int?)Math.Ceiling(avgTemplatePlayersCount!)
                }, "Statistics fetched successfully");

            }

            lifeCountersStarted = lifeCounterTemplatesDB
                .SelectMany(a => a.LifeCounterManagers!)
                .Count();

            unfinishedLifeCounters = lifeCounterTemplatesDB
                .SelectMany(a => a.LifeCounterManagers!)
                .Where(a => a.IsFinished == false)
                .Count();

            var avgPlayersCount = lifeCounterTemplatesDB
                .SelectMany(a => a.LifeCounterManagers!)
                .Select(a => a.PlayersCount!)
                .Average();

            var content = new UsersGetLifeCounterStatisticsResponse
            {
                MostUsedLifeCounter = mustUsedLifeCounter,
                LifeCountersCreated = lifeCountersCreated,
                LifeCountersStarted = lifeCountersStarted,
                UnfinishedLifeCounters = unfinishedLifeCounters,
                FavoritePlayersCount = (int?)Math.Ceiling(avgPlayersCount!.Value)
            };

            return (content, "Statistics fetched successfully");
        }
        private static (bool, string) GetLifeCounterStatistics_Validation(UsersGetLifeCounterStatisticsRequest? request)
        {
            if (request != null)
            {
                return (false, $"Error: request is NOT null but it MUST be null!");
            }

            return (true, string.Empty);
        }



        //
        //--* end of LIFE COUNTERS *--//
    }
}