namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersListPlayedBoardGamesResponse
    {
        public required int BoardGameId { get; set; }
        public required string BoardGameName { get; set; }
    }
}
