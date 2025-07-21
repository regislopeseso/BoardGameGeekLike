using BoardGameGeekLike.Models.Enums;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersImportUserDataResponse_userDetails
    {
        public string? Name { get; set; }

        public DateOnly? SignUpDate { get; set; }

        public DateOnly? BirthDate { get; set; }

        public Gender? Gender { get; set; }
    }
}
