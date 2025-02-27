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

            var parsedDate = DateOnly.ParseExact(request!.UserBirthDate, "dd/MM/yyyy");   

            var user = new User
            {
                Nickname = request!.UserNickname,
                Email = request.UserEmail,
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

            if (string.IsNullOrWhiteSpace(request.UserNickname))
            {
                return (false, "Error: username is null or empty");
            }

            string emailPattern = @"(^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}(?:\.[a-zA-Z]{2,10})?$)";

            if (Regex.IsMatch(request.UserEmail, emailPattern) == false)
            {
                return (false, "Error: invalid email format");
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
                                                .AnyAsync(a => a.Id != request.UserId &&
                                                               a.Nickname == request!.UserNickname &&
                                                               a.IsDeleted == false);

            if(userNickName_exists == true)
            {
                return (null, "Error: requested UserNickName is already in use");
            }

            var userDB = await this._daoDbContext
                                   .Users
                                   .FindAsync(request.UserId);

            if(userDB == null)
            {
                return (null, "Error: user not found");
            }

            if(userDB.IsDeleted == true)
            {
                return (null, "Error: user is deleted");
            }

            var parsedDate = DateOnly.ParseExact(request!.UserBirthDate, "dd/MM/yyyy");           

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

            if (string.IsNullOrWhiteSpace(request.UserNickname))
            {
                return (false, "Error: username is null or empty");
            }

            string emailPattern = @"(^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}(?:\.[a-zA-Z]{2,10})?$)";

            if (Regex.IsMatch(request.UserEmail, emailPattern) == false)
            {
                return (false, "Error: invalid email format");
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

        
    }
}