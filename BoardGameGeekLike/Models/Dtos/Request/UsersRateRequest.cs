using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class UsersRateRequest
    {
        public decimal? Rate {get; set;}

        public int? UserId {get; set;}

        public int? BoardGameId {get; set;}
    }
}