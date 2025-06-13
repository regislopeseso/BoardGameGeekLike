namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersQuickStartLifeCounterResponse
    {
        public int? LifeCounterManagerId { get; set; }

        public string? LifeCounterManagerName { get; set; }

        public List<UsersQuickStartLifeCounterResponse_players>? LifeCounterPlayers {get; set;}
    }
}
