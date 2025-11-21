using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class ExploreBoardGamesRankingsResponse
    {
        public List<ExploreBoardGamesRankingsResponse_statistics>? MostPlayedBoardGames {get; set;}

        public List<ExploreBoardGamesRankingsResponse_statistics>? BestRatedBoardGames {get; set;}

        public List<ExploreBoardGamesRankingsResponse_statistics>? ShortestBoardGames {get; set;}

        public List<ExploreBoardGamesRankingsResponse_statistics>? LongestBoardGames {get; set;}

        public List<ExploreBoardGamesRankingsResponse_statistics>? AdultsFavoriteBoardGames {get; set;}

        public List<ExploreBoardGamesRankingsResponse_statistics>? TeensFavoriteBoardGames {get; set;}
    }
}