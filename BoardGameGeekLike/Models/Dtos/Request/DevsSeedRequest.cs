using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class DevsSeedRequest
    {
        public int? CategoriesCount { get; set; } = 10;

        public int? MecanicsCount { get; set; } = 10;
        
        public int? BoardGamesCount { get; set; } = 10;

        public int? UsersCount { get; set; } = 200;

        public int? SessionsCount { get; set; } = 30;
    }
}