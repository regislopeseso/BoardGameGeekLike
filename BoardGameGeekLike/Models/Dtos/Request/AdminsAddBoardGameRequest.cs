using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoardGameGeekLike.Models.Entities;

namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class AdminsAddBoardGameRequest
    {
        public required string BoardGameName { get; set; }

        public string? BoardGameDescription { get; set; }

        public int MinPlayersCount { get; set; }

        public int MaxPlayersCount { get; set; }

        public int MinAge { get; set; }
        
        public int CategoryId { get; set; }

        public List<int> BoardGameMechanicIds { get; set; } = new List<int>();
    }
}