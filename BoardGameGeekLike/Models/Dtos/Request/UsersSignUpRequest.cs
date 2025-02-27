using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class UsersSignUpRequest
    {
        public required string UserNickname { get; set; }
        public required string UserEmail { get; set; }
        public DateOnly UserBirthDate { get; set; }
    }
}