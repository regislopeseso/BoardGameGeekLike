namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersQuickStartLifeCounterResponse_players
    {
        public string? LifeCounterPlayerName { get; set; }

        public int? CurrentLifePoints { get; set; }

        public bool? IsDefeated { get; set; }
    }
}
