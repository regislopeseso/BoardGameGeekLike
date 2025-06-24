namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class UsersEditLifeCounterTemplateRequest
    {
        public int? LifeCounterTemplateId { get; set; }

        public string? LifeCounterTemplateName { get; set; }

        public int? PlayersStartingLifePoints { get; set; }

        public int? PlayersCount { get; set; } 

        public bool? FixedMaxLifePointsMode { get; set; }

        public int? PlayersMaxLifePoints { get; set; }

        public bool? AutoDefeatMode { get; set; }

        public bool? AutoEndMode { get; set; }

    }
}
