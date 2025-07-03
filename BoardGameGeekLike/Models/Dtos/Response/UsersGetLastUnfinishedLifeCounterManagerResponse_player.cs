namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersGetLastUnfinishedLifeCounterManagerResponse_player
    {
        public int? LifeCounterPlayerId { get; set; }
        public string? PlayerName { get; set; }
        public int? CurrentLifePoints { get; set; }
        public bool? IsDefeated = false;
    }
}
