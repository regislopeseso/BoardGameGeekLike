using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersBoardGamesRankingResponse
    {
        public List<string>? TheMostPlayed {get; set;}

        public List<string>? TheBestRated {get; set;}

        public List<string>? TheShortest {get; set;}

        public List<string>? TheLongest {get; set;}

        public List<string>? AdultsFavorites {get; set;}

        public List<string>? TeensFavorites {get; set;}
    }
}