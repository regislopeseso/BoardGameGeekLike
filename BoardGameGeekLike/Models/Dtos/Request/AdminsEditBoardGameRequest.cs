using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class AdminsEditBoardGameRequest
    {
        public int BoardGameId { get; set; }

        public string? BoardGameName { get; set; }

        public string? BoardGameDescription { get; set; }

        public int? MinPlayersCount { get; set; }

        public int? MaxPlayersCount { get; set; }

        public int? MinAge { get; set; }

        public int? CategoryId { get; set; }

        public List<int>? MechanicsIds { get; set; }
    }
}