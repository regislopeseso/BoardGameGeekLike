namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersGetLastUnfinishedLifeCounterManagerResponse_template
    {
        public int LifeCounterTemplateId { get; set; }
        
        public string? LifeCounterTemplateName { get; set; }

        public int? PlayersStartingLifePoints { get; set; }

        public int? PlayersCount { get; set; }

        public bool? FixedMaxLifePointsMode { get; set; }

        public int? PlayersMaxLifePointsMode { get; set; }

        public bool? AutoDefeatMode { get; set; }

        public bool? AutoEndMode { get; set; }

        public int? LifeCounterManagersCount { get; set; }
    }
}
