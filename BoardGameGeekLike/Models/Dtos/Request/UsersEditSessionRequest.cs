using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class UsersEditSessionRequest
    {
        public int? BoardGameId {get; set;}        
        public int? SessionId {get; set;}        
        public string? NewDate {get; set;}
        public int? NewPlayersCount {get; set;}
        public int? NewDuration_minutes {get; set;}
    }
}