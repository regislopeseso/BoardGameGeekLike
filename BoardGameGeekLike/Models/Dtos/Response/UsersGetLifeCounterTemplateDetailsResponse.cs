namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersGetLifeCounterTemplateDetailsResponse
    {
        public string? Name { get; set; }
        public int? DefaultPlayersCount { get; set; }
        public int? PlayersStartingLifePoints { get; set; }
        public int? MaxLifePoints { get; set; }
        public bool? AutoEndMode { get; set; }
    }
}
