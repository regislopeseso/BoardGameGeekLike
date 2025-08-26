namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersStartMabBattleResponse
    {
        public string? MabPlayerNickName { get; set; }

        public string? MabNpcName { get; set; }

        public bool? DoesPlayerGoFirst { get; set; }
    }
}
