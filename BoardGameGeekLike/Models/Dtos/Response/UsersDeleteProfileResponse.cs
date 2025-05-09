using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersDeleteProfileResponse
    {
        public int? RemainingPasswordAttempts { get; set; }
    }
}