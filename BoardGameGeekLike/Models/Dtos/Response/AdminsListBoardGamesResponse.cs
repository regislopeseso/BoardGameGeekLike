using BoardGameGeekLike.Models.Entities;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class AdminsListBoardGamesResponse
    {
        public int? BoardGameId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? PlayersCount { get; set; }        
        public int? MinAge { get; set; }
        public string? Category { get; set; }
        public List<string>? Mechanics { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
