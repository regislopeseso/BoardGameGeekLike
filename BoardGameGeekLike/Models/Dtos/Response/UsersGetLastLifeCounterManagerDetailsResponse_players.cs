namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersGetLastLifeCounterManagerDetailsResponse_players
    {
        public string? PlayerName { get; set; }

        public int? CurrentLifePoints { get; set; }

        public bool? IsDefeated { get; set; }
    }
}
