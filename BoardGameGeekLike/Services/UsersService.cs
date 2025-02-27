using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BoardGameGeekLike.Models;
using BoardGameGeekLike.Models.Dtos.Request;
using BoardGameGeekLike.Models.Dtos.Response;
using BoardGameGeekLike.Models.Entities;

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

            var user = new User
            {
                Nickname = request!.UserNickname,
                Email = request.UserEmail,
                BirthDate = request.UserBirthDate,
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

            string birthDatePattern = @"(^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}(?:\.[a-zA-Z]{2,10})?$)";

            if (Regex.IsMatch(request.UserEmail, birthDatePattern) == false)
            {
                return (false, "Error: invalid email format");
            }

            if(request.UserBirthDate.Year > DateTime.Now.Year)
            {
                return (false, "Error: birth date is in the future");
            }

            if(DateTime.Now.Year - request.UserBirthDate.Year > 90)
            {
                return (false, "Error: birth date is too old");
            }

            if(DateTime.Now.Year - request.UserBirthDate.Year < 12)
            {
                return (false, "Error: the minimum age for signing up is 12");
            }

            return (true, "User signed up successfully");
        }

    }
}