namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class AdminsAddBoardGameRequest
    {
        public required string BoardGameName { get; set; }

        public required string BoardGameDescription { get; set; }

        public required int MinPlayersCount { get; set; }

        public required int MaxPlayersCount { get; set; }

        public required int MinAge { get; set; }
        
        public int? CategoryId { get; set; }

        public List<int>? MechanicIds { get; set; } = new List<int>();
    }
}