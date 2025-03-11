using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class ExploreFindBoardGameResponse
    {
        public required int BoardGameId {get; set;}
        public required string BoarGameName {get; set;}
    }
}