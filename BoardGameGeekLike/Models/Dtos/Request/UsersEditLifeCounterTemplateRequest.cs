namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class UsersEditLifeCounterTemplateRequest
    {
        public int? LifeCounterTemplateId { get; set; }

        public string? NewLifeCounterTemplateName { get; set; }

        public int? NewPlayersStartingLifePoints { get; set; }

        public int? NewPlayersCount { get; set; } 

        public bool? FixedMaxLifePointsMode { get; set; }

        public int? NewPlayersMaxLifePoints { get; set; }

        public bool? AutoDefeatMode { get; set; }

        public bool? AutoEndMode { get; set; }

    }
}
