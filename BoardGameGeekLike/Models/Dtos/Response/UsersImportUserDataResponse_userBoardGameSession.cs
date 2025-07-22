namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersImportUserDataResponse_userBoardGameSession
    {
        public string? BoardGameName { get; set; }

        public DateOnly? Date { get; set; }

        public int? PlayersCount { get; set; }

        public int? Duration_minutes { get; set; }

        public bool? IsDeleted { get; set; }
    }
}
