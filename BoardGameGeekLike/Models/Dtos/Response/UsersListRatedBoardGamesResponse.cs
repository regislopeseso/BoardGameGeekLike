namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersListRatedBoardGamesResponse
    {
        public required int BoardGameId { get; set; }
        
        public required string BoardGameName { get; set; }
        
        public required int RatingId { get; set; }
        
        public required int Rate { get; set; }
    }
}
