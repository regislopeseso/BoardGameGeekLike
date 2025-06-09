namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersGetLifeCounterPlayersDetailsResponse_players
    {
        public int? PlayerId { get; set; }
        public string? PlayerName { get; set; }

        public int? PlayerMaxLifePoints { get; set; }

        public int? PlayerCurrentLifePoints { get; set; }

        public bool? IsMaxLifePointsFixed { get; set; }

        public bool? IsDefeated { get; set; }

    }
}
