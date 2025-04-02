namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class ExploreListBoardGamesResponse
    {
        public string? BoardGameName { get; set; }
        public decimal? AvgRating { get; set; }
        public int? RatingsCount { get; set; }
        public string? PlayersCount { get; set; }
        public int? AvgDuration { get; set; }
        public int? SessionsLogged { get; set; }       
    }
}
