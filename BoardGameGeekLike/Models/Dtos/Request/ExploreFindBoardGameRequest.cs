using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class ExploreFindBoardGameRequest
    {
        public int? BoardGameId { get; set; }
        public string? BoardGameName {get; set;}
        public int? MinPlayersCount {get; set;}

        public int? MaxPlayersCount {get; set;}

        public int? MinAge {get; set;}

        public string? CategoryName {get; set;}

        public string? MechanicName {get; set;}

        public int? AverageRating {get; set;}
    }
}