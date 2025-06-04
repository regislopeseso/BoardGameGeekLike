using BoardGameGeekLike.Models.Entities;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersStartLifeCounterManagerResponse
    {
        public string? LifeCounterName { get; set; }
        public int? PlayersCount { get; set; }
        public int? StartingLifePoints { get; set; }

        public int? LifeCounterId { get; set; }
        public int? LifeCounterManagerId { get; set; }
        public List<UsersStartLifeCounterManagerResponse_players>? LifeCounterPlayers { get; set; }

    }
}
