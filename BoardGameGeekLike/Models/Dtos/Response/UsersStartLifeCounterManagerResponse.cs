using BoardGameGeekLike.Models.Entities;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersStartLifeCounterManagerResponse
    {
        public string? LifeCounterTemplateName { get; set; }

        public string? LifeCounterManagerName { get; set; }

        public int? PlayersCount { get; set; }

        public int? PlayersStartingLifePoints { get; set; }

        public int? LifeCounterTemplateId { get; set; }

        public int? LifeCounterManagerId { get; set; }

        public List<UsersStartLifeCounterManagerResponse_players>? LifeCounterPlayers { get; set; }

    }
}
