namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class AdminsShowBoardGameDetailsResponse
    {
        public string? BoardGameName { get; set; }

        public string? BoardGameDescription { get; set; }

        public int? MinPlayersCount { get; set; }
        
        public int? MaxPlayerCount { get; set; }
        
        public int? MinAge { get; set; }
        
        public string? Category { get; set; }

        public List<string>? Mechanics { get; set; }

        public bool IsDeleted { get; set; }
              
    }
}
