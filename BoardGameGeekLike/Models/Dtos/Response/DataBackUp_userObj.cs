using BoardGameGeekLike.Models.Enums;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class DataBackUp_userObj
    {
        public string? Name { get; set; }

        public string? Email { get; set; }

        public DateOnly? BirthDate { get; set;  }

        public Gender? Gender { get; set; }

        public DateOnly? SignUpDate { get; set; }

        public List<int>? LifeCounterTemplateIds { get; set; }
    }
}
