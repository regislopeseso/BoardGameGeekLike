namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class UsersChangePlayerNameRequest
    {
        public int? LifeCounterPlayerId { get; set; }
        public string? PlayerNewName { get; set; }
    }
}
