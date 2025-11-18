namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class ExploreBoardGamesRankingsResponse_bestRated
    {
        public string? BoardGame_Name { get; set; }

        public decimal? BoardGame_AvgRating { get; set; }

        public int? BoardGame_RatingsCount { get; set; }

        public int? BoardGame_SessionsCount { get; set; }

        public int? BoardGame_AvgDuration { get; set; }

        public int? BoarGame_MinAge { get; set; }
    }
}
