using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class UsersLogSessionRequest
    {
        public int? UserId {get; set;}
        
        public int? BoardGameId {get; set;}
        
        public string? Date {get; set;}
        public int? PlayersCount {get; set;}
        public int? Duration_minutes {get; set;}
    }
}