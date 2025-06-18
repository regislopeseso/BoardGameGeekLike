namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersGetLifeCounterPlayerDetailsResponse
    {
        public int? LifeCounterManagerId { get; set; }
        
        public string? LifeCounterPlayerName { get; set; }

        public int? PlayerStartingLifePoints { get; set; }
        
        public int? PlayerCurrentLifePoints { get; set; }
        
        public bool? FixedMaxLifePointsMode { get; set; }
        
        public int? PlayerMaxLifePoints { get; set; }

        public bool? AutoDefeatMode { get; set; }
    }
}
