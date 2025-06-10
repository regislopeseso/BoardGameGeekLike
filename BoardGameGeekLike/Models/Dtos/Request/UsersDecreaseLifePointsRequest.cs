namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class UsersDecreaseLifePointsRequest
    {
        public int? LifeCounterPlayerId { get; set; }
        public int? LifePointsToDecrease { get; set; }
    }
}
