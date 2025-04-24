using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class UsersSignUpRequest
    {
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? Password { get; set; }
        public string? UserBirthDate { get; set; }        
    }
}