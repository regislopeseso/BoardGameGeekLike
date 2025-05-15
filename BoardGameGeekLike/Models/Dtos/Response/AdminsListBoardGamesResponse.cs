using BoardGameGeekLike.Models.Entities;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class AdminsListBoardGamesResponse
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? PlayersCount { get; set; }        
        public int? MinAge { get; set; }
        public Category? Category { get; set; }
        public List<Mechanic>? Mechanics { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
