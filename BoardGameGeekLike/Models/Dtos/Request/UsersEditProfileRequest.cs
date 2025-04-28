using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersEditProfileRequest
    {
        public string? NewName { get; set; }
        public string? NewEmail { get; set; }
        public string? NewBirthDate { get; set; }
        public string? NewPassword { get; set; }
    }
}