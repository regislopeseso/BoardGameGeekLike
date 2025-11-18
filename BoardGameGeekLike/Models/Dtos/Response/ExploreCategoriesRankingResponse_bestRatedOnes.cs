using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class ExploreCategoriesRankingResponse_bestRatedOnes
    {
        public string? Category_Name { get; set; }

        public decimal? Category_AvgRating { get; set; }

        public int? Category_RatingsCount { get; set; }

        public int? Category_SessionsCount { get; set; }

        public int? Category_BoardGamesCount { get; set; }

        public int? Category_AvgDuration { get; set; }
    }
}