using BoardGameGeekLike.Models.Entities;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class DataBackUp_userObj_Sessions
    {
        public string? BoardGameName { get; set; }
        public DateOnly? Date { get; set; }
        public int PlayersCount { get; set; }
        public int Duration_minutes { get; set; }

        public bool IsDeleted { get; set; }
    }
}
