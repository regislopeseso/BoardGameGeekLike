using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class ExploreCategoriesRankingResponse_shortestOnes
    {
        public string? CategoryName {get; set;}

        public int? Duration {get; set;}

        public int? SessionsCount { get; set; }
    }
}