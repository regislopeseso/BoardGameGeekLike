namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class UsersStartLifeCounterManagerRequest
    {
        public int? LifeCounterId { get; set; }
        public int? PlayersCount {  get; set; }
        public int? PlayersStartingLifePoints { get; set; }
    }
}
