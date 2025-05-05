using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class UsersRateRequest
    {
        public int? BoardGameId {get; set;}
        
        public decimal? Rate {get; set;}
    }
}