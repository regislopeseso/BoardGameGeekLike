namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class UsersEditLifeCounterManagerRequest
    {
        public int? LifeCounterManagerId { get; set; }
        public string? NewLifeCounterManagerName { get; set; }
        public int? NewPlayersCount { get; set; }
        public int? FirstPlayerIndex { get; set; }
        public int? NewPlayersStartingLifePoints { get; set; }
        public bool? FixedMaxLifePointsMode { get; set; }
        public int? NewPlayersMaxLifePoints { get; set; }
        public bool? AutoDefeatMode { get; set; }
        public bool? AutoEndMode { get; set; }
    }
}
