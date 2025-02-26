using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class AdminsDeleteBoardGameRequest
    {
        public required int BoardGameId {get; set;}
    }
}