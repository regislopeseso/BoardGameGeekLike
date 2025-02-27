using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersEditProfileRequest
    {
        public int UserId {get; set;}
        public required string UserNickname { get; set; }
        public required string UserEmail { get; set; }
        public required string UserBirthDate { get; set; }
    }
}