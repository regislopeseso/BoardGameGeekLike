namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersGetLifeCounterDetailsResponse
    {
        public string? Name { get; set; }
        public int? DefaultPlayersCount { get; set; }
        public int? PlayersStartingLifePoints { get; set; }
        public int? MaxLifePoints { get; set; }
        public bool? AutoEndMode { get; set; }
    }
}
