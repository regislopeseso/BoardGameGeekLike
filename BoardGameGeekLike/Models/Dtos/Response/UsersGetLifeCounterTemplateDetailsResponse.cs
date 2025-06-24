namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersGetLifeCounterTemplateDetailsResponse
    {
        public string? LifeCounterTemplateName { get; set; }


        public int? PlayersStartingLifePoints { get; set; }
        public int? PlayersCount { get; set; }

        public bool? FixedMaxLifePointsMode { get; set; }

        public int? PlayersMaxLifePoints { get; set; }

        public bool? AutoDefeatMode { get; set; }

        public bool? AutoEndMode { get; set; }
    }
}
