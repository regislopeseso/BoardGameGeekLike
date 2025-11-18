using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class ExploreBoardGamesRankingsResponse
    {
        public List<ExploreBoardGamesRankingsResponse_mostPlayed>? MostPlayedBoardGames {get; set;}

        public List<ExploreBoardGamesRankingsResponse_bestRated>? BestRatedBoardGames {get; set;}

        public List<ExploreBoardGamesRankingsResponse_theShortest>? ShortestBoardGames {get; set;}

        public List<ExploreBoardGamesRankingsResponse_theLongest>? LongestBoardGames {get; set;}

        public List<ExploreBoardGamesRankingsResponse_adultsFavorites>? AdultsFavoriteBoardGames {get; set;}

        public List<ExploreBoardGamesRankingsResponse_teensFavorites>? TeensFavoriteBoardGames {get; set;}
    }
}