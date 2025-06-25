namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersGetLifeCounterStatisticsResponse
    {
        public string? MostUsedLifeCounter { get; set; }
        public int? LifeCountersCreated { get; set; }
        public int? LifeCountersStarted { get; set; }
        public int? UnfinishedLifeCounters { get; set; }
        public int? FavoritePlayersCount { get; set; }
    }
}
