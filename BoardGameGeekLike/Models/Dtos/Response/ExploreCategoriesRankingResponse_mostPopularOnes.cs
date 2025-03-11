using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class ExploreCategoriesRankingResponse_mostPopularOnes
    {
        public string? CategoryName {get; set;}

        public int? BoardGamesCount {get; set;}
    }
}