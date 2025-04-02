using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class ExploreBoardGamesRankingsResponse
    {
        public List<string>? MostPlayedBoardGames {get; set;}

        public List<string>? BestRatedBoardGames {get; set;}

        public List<string>? ShortestBoardGames {get; set;}

        public List<string>? LongestBoardGames {get; set;}

        public List<string>? AdultsFavoriteBoardGames {get; set;}

        public List<string>? TeensFavoriteBoardGames {get; set;}
    }
}