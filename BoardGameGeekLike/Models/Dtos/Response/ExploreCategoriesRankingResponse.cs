using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class ExploreCategoriesRankingResponse
    {
        public List<ExploreCategoriesRankingsResponse_statistics>? MostPlayedCategories {get; set;}

        public List<ExploreCategoriesRankingsResponse_statistics>? MostPopularCategories {get; set;}

        public List<ExploreCategoriesRankingsResponse_statistics>? BestRatedCategories {get; set;}

        public List<ExploreCategoriesRankingsResponse_statistics>? LongestCategories {get; set;}

        public List<ExploreCategoriesRankingsResponse_statistics>? ShortestCategories {get; set;}
    }
}