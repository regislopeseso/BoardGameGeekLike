namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class UsersEditRatingRequest
    {
        public int? RatingId { get; set; }

        public decimal? Rate { get; set; }
    }
}
