namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class AdminsShowBoardGameDetailsResponse
    {
        public int? BoardGameId { get; set; }
        public string? BoardGameName { get; set; }

        public string? BoardGameDescription { get; set; }

        public int? MinPlayersCount { get; set; }
        
        public int? MaxPlayerCount { get; set; }
        
        public int? MinAge { get; set; }
        
        public int? Category { get; set; }

        public List<int>? Mechanics { get; set; }

        public bool IsDeleted { get; set; }
              
    }
}
