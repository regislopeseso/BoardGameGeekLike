using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class ExploreCategoriesRankingResponse_bestRatedOnes
    {
        public string? CategoryName {get; set;}

        public decimal? AvgRating {get; set;}

        public int? RatingsCount { get; set; }
    }
}