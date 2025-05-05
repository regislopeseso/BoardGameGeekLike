namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersGetProfileDetailsResponse
    {
        public string? Name {get; set;}
        public DateOnly SignUpDate { get; set; }
        public int? SessionsCount { get; set; }
        public int? RatedBgCount { get; set; }
    }
}
