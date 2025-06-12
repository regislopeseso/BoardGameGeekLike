namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersGetLastLifeCounterManagerDetailsResponse
    {
        public int? LifeCounterManagerId { get; set; }
        public string? LifeCounterManagerName { get; set; }
        public int? PlayersCount { get; set; }
        public List<UsersGetLastLifeCounterManagerDetailsResponse_players>? LifeCounterManagerPlayers { get; set; }

        public bool? FixedMaxLifePointsMode { get; set; }
        public int? PlayersMaxLifePoints { get; set; }

        public bool? AutoEndMode { get; set; }
    }
}
