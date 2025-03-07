using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersCategoriesRankingResponse
    {
        public List<UsersCategoriesRankingResponse_mostPlayedOnes>? MostPlayedCategories {get; set;}

        public List<UsersCategoriesRankingResponse_mostPopularOnes>? MostPopularCategories {get; set;}

        public List<UsersCategoriesRankingResponse_bestRatedOnes>? BestRatedCategories {get; set;}

        public List<UsersCategoriesRankingResponse_longestOnes>? LongestCategories {get; set;}

        public List<UsersCategoriesRankingResponse_shortestOnes>? ShortestCategories {get; set;}
    }
}