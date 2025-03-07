using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersCategoriesRankingResponse_mostPlayedOnes
    {
        public string? CategoryName {get; set;}

        public int? SessionsCount {get; set;}
    }
}