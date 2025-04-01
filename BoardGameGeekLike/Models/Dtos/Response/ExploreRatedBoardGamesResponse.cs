namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class ExploreRatedBoardGamesResponse
    {
        public string? BoardGameName { get; set; }
        public decimal? AvgRating { get; set; }
        public int? RatingsCount { get; set; }  
    }
}
