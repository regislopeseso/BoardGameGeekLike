namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class AdminsAddBoardGameRequest
    {
        public string? BoardGameName { get; set; }

        public string? BoardGameDescription { get; set; }

        public int? MinPlayersCount { get; set; }

        public int? MaxPlayersCount { get; set; }

        public int? MinAge { get; set; }
        
        public int? CategoryId { get; set; }

        public List<int>? MechanicIds { get; set; } = new List<int>();
    }
}