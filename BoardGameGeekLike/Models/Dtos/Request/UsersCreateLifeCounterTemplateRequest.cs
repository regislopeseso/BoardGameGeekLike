namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class UsersCreateLifeCounterTemplateRequest
    {
        public string? LifeCounterTemplateName { get; set; }
        public int? PlayersCount { get; set; }
        public int? PlayersStartingLifePoints { get; set; }
        public int? MaxLifePoints { get; set; }
        public bool? FixedMaxLife { get; set; }
        public bool? AutoEndMatch { get; set; }
    }
}
