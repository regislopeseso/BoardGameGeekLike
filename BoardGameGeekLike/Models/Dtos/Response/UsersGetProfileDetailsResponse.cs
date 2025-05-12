namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersGetProfileDetailsResponse
    {
        public string? TreatmentTitle { get; set; }
        public string? Name {get; set;}
        public string? Email { get; set; }
        public DateOnly BirthDate { get; set; }
        public DateOnly SignUpDate { get; set; }
        public int? SessionsCount { get; set; }
        public int? RatedBgCount { get; set; }
    }
}
