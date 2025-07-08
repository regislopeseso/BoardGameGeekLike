namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersGetLifeCounterManagerDetailsResponse
    {
        public UsersGetLifeCounterManagerDetailsResponse_template? LifeCounterTemplate { get; set; }

        public string? LifeCounterManagerName { get; set; }

        public int? PlayersCount { get; set; }

        public List<UsersGetLifeCounterManagerDetailsResponse_players>? LifeCounterPlayers { get; set; }

        public int? PlayersStartingLifePoints { get; set; }
        
        public bool? FixedMaxLifePointsMode { get; set; }

        public int? PlayersMaxLifePoints { get; set; }

        public bool? AutoDefeatMode {  get; set; }

        public bool? AutoEndMode { get; set; }

        public long? StartingTime { get; set; }

        public long? EndingTime { get; set; }

        public double? Duration_minutes { get; set; }

        public bool? IsFinished { get; set; }
    }
}
