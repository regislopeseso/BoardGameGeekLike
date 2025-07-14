namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersGetLastLifeCounterManagerResponse_players
    {
        public int? LifeCounterPlayerId { get; set; }

        public string? LifeCounterPlayerName { get; set; }

        public int? CurrentLifePoints { get; set; }

        public bool? IsDefeated { get; set; }
    }
}
