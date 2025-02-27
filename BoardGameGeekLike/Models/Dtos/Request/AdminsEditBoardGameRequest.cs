namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class AdminsEditBoardGameRequest
    {
        public int BoardGameId { get; set; }

        public required string BoardGameName { get; set; }

        public required string BoardGameDescription { get; set; }

        public required int MinPlayersCount { get; set; }

        public required int MaxPlayersCount { get; set; }

        public required int MinAge { get; set; }

        public required int CategoryId { get; set; }

        public required List<int> MechanicIds { get; set; }
    }
}