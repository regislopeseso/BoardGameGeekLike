using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersEditProfileRequest
    {
        public string? UserId {get; set;}
        public string? UserNickname { get; set; }
        public string? UserEmail { get; set; }
        public string? UserBirthDate { get; set; }
    }
}