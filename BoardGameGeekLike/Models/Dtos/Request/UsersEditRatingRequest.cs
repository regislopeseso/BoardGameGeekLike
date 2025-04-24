namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class UsersEditRatingRequest
    {
        public decimal? Rate { get; set; }
        
        public string? UserId { get; set; }

        public int? BoardGameId { get; set; }
    }
}
