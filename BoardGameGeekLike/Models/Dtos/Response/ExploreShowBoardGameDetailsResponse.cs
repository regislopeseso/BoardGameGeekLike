using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoardGameGeekLike.Models.Entities;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class ExploreShowBoardGameDetailsResponse
    {
        public string? BoardGameName {get; set;}

        public string? BoardGameDescription {get; set;}

        public string? Category {get; set;}

        public List<string>? Mechanics {get; set;}

        public int? MinPlayersCount {get; set;}
        
        public int? MaxPlayerCount {get; set;}

        public int? MinAge {get; set;}

        public int? LoggedSessions {get; set;}

        public int? AvgSessionDuration {get; set;}

        public int? AvgRating {get; set;}

        public List<ExploreShowBoardGameDetailsResponse_sessions>? LastFiveSessions {get; set;}
    }
}