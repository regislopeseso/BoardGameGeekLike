namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class UsersIncreaseLifePointsRequest
    {
        public int? LifeCounterPlayerId { get; set; }
        public int? LifePointsToIncrease { get; set; }
    }
}
