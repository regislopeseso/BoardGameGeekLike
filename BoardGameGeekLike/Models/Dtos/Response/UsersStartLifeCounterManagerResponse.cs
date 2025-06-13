using BoardGameGeekLike.Models.Entities;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersStartLifeCounterManagerResponse
    {
        public int? LifeCounterTemplateId { get; set; }
        
        public string? LifeCounterTemplateName { get; set; }

        public int? LifeCounterManagerId { get; set; }
        
        public string? LifeCounterManagerName { get; set; }

        public int? PlayersStartingLifePoints { get; set; }
        
        public int? PlayersCount { get; set; }

        public List<UsersStartLifeCounterManagerResponse_players>? LifeCounterPlayers { get; set; }

        public bool? FixedMaxLifePointsMode { get; set; }

        public int? PlayersMaxLifePoints { get; set; }

        public bool? AutoDefeatMode { get; set; }

        public bool? AutoEndMode { get; set; }
    }
}
