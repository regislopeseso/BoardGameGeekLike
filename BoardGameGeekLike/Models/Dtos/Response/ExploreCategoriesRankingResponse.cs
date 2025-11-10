using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class ExploreCategoriesRankingResponse
    {
        public List<ExploreCategoriesRankingResponse_mostPlayedOnes>? MostPlayedCategories {get; set;}

        public List<ExploreCategoriesRankingsResponse_mostPopularOnes>? MostPopularCategories {get; set;}

        public List<ExploreCategoriesRankingResponse_bestRatedOnes>? BestRatedCategories {get; set;}

        public List<ExploreCategoriesRankingResponse_longestOnes>? LongestCategories {get; set;}

        public List<ExploreCategoriesRankingResponse_shortestOnes>? ShortestCategories {get; set;}
    }
}